using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Plugins.DualKawaseBlur.Feature
{
	public class DualKawaseBlurPass : ScriptableRenderPass
	{
		public enum Quality
		{
			Low,
			Medium,
			High,
		}

		private enum ShaderPass
		{
			Copy = 0,
			DownSample = 1,
			UpSample = 2,
		}

		private const string PROFILER_TAG = "DualKawaseBlur";
		
		// Shader Properties
		private static readonly int BLUR_OFFSET_P = Shader.PropertyToID("_BlurOffset");
		private static readonly int SOURCE_TEX_P = Shader.PropertyToID("_SourceTex");
		
		// Buffers and Textures
		private static readonly int BUFFER_0_TEX_P = Shader.PropertyToID("_BufferRT0");
		private static readonly int BUFFER_1_TEX_P = Shader.PropertyToID("_BufferRT1");
		private static readonly int BUFFER_2_TEX_P = Shader.PropertyToID("_BufferRT2");
		private static readonly int BLUR_TEX_P = Shader.PropertyToID("_BlurTex");
		private readonly RenderTargetIdentifier buffer0 = new(BUFFER_0_TEX_P, 0, CubemapFace.Unknown, -1);
		private readonly RenderTargetIdentifier buffer1 = new(BUFFER_1_TEX_P, 0, CubemapFace.Unknown, -1);
		private readonly RenderTargetIdentifier buffer2 = new(BUFFER_2_TEX_P, 0, CubemapFace.Unknown, -1);
		private readonly RenderTargetIdentifier blurBuffer = new(BLUR_TEX_P, 0, CubemapFace.Unknown, -1);
		private RenderTextureDescriptor fullDesc, halfDesc, quarterDesc, eightsDesc, sixteenthsDesc;

		// Configuration
		private readonly Material material;
		private float blurAmount;
		private Quality iterations;

		public DualKawaseBlurPass(RenderPassEvent renderPassEvent, Material material)
		{
			this.renderPassEvent = renderPassEvent;
			this.material = material;
		}

		public void ConfigureBlur(float blurRadius, Quality quality)
		{
			blurAmount = blurRadius;
			iterations = quality;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			fullDesc = renderingData.cameraData.cameraTargetDescriptor;
			fullDesc.depthBufferBits = 0;
			halfDesc = quarterDesc = eightsDesc = sixteenthsDesc = fullDesc;
			halfDesc.width /= 2;
			halfDesc.height /= 2;
			quarterDesc.width /= 4;
			quarterDesc.height /= 4;
			eightsDesc.width /= 8;
			eightsDesc.height /= 8;
			sixteenthsDesc.width /= 16;
			sixteenthsDesc.height /= 16;
			
			cmd.GetTemporaryRT(BLUR_TEX_P, fullDesc, FilterMode.Bilinear);
		}
		
		private static void DrawFullScreenTriangle(CommandBuffer cmd, RenderTargetIdentifier from, RenderTargetIdentifier to, Material blitMaterial, int pass)
		{
			cmd.SetGlobalTexture(SOURCE_TEX_P, from);
			cmd.SetRenderTarget(to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, pass, MeshTopology.Triangles, 3);
		}
		
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (Mathf.Approximately(blurAmount, 0.0f)) {
				return;
			}
			
			var cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG))) {
				material.SetFloat(BLUR_OFFSET_P, blurAmount);
				var cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
				switch (iterations) {
					case Quality.Low:
						cmd.GetTemporaryRT(BUFFER_0_TEX_P, halfDesc, FilterMode.Bilinear);
						DrawFullScreenTriangle(cmd, cameraColor, buffer0, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer0, blurBuffer, material, (int)ShaderPass.UpSample);
						break;
					case Quality.Medium:
						cmd.GetTemporaryRT(BUFFER_0_TEX_P, halfDesc, FilterMode.Bilinear);
						cmd.GetTemporaryRT(BUFFER_1_TEX_P, quarterDesc, FilterMode.Bilinear);
						DrawFullScreenTriangle(cmd, cameraColor, buffer0, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer0, buffer1, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer1, buffer0, material, (int)ShaderPass.UpSample);
						DrawFullScreenTriangle(cmd, buffer0, blurBuffer, material, (int)ShaderPass.UpSample);
						break;
					case Quality.High:
						cmd.GetTemporaryRT(BUFFER_0_TEX_P, halfDesc, FilterMode.Bilinear);
						cmd.GetTemporaryRT(BUFFER_1_TEX_P, quarterDesc, FilterMode.Bilinear);
						cmd.GetTemporaryRT(BUFFER_2_TEX_P, eightsDesc, FilterMode.Bilinear);
						DrawFullScreenTriangle(cmd, cameraColor, buffer0, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer0, buffer1, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer1, buffer2, material, (int)ShaderPass.DownSample);
						DrawFullScreenTriangle(cmd, buffer2, buffer1, material, (int)ShaderPass.UpSample);
						DrawFullScreenTriangle(cmd, buffer1, buffer0, material, (int)ShaderPass.UpSample);
						DrawFullScreenTriangle(cmd, buffer0, blurBuffer, material, (int)ShaderPass.UpSample);
						break;
					default:
						throw new System.ArgumentOutOfRangeException();
				}
				DrawFullScreenTriangle(cmd, blurBuffer, cameraColor, material, (int)ShaderPass.Copy);
			}
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}
		
		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			if (cmd == null) {
				throw new System.ArgumentNullException(nameof(cmd));
			}
			cmd.ReleaseTemporaryRT(BUFFER_0_TEX_P);
			cmd.ReleaseTemporaryRT(BUFFER_1_TEX_P);
			cmd.ReleaseTemporaryRT(BUFFER_2_TEX_P);
			cmd.ReleaseTemporaryRT(BLUR_TEX_P);
		}
	}
}
