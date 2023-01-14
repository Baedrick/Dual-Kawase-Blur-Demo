namespace DualKawaseBlur
{
	using UnityEngine;
	using UnityEngine.Rendering;
	using UnityEngine.Rendering.Universal;
	
	public class DualKawaseBlurPass : ScriptableRenderPass
	{
		private enum DownSampleRT
		{
			Full = 1,
			Half = 2,
			Quarter = 4,
			Eights = 8,
			Sixteenths = 16
		}
		
		private enum ShaderPass
		{
			Copy = 0,
			DownSample = 1,
			UpSample = 2,
		}

		private const string PROFILER_TAG = "DualKawaseBlur";
		private static readonly int BLUR_OFFSET_P = Shader.PropertyToID("_BlurOffset");
		private static readonly int SOURCE_TEX_P = Shader.PropertyToID("_SourceTex");

		private static readonly int MIP_DOWN_TEX_P = Shader.PropertyToID("_BlurMipDownTex");
		private static readonly int MIP_UP_TEX_P = Shader.PropertyToID("_BlurMipUpTex");
		

		private readonly RenderTargetIdentifier mipDownBuffer = new(MIP_DOWN_TEX_P, 0, CubemapFace.Unknown, -1);
		private readonly RenderTargetIdentifier mipUpBuffer = new(MIP_UP_TEX_P, 0, CubemapFace.Unknown, -1);

		private RenderTextureDescriptor fullDesc;
		
		private readonly Material material;
		private float blurAmount;
		private int iterations;

		public DualKawaseBlurPass(RenderPassEvent renderPassEvent, Material material)
		{
			this.renderPassEvent = renderPassEvent;
			this.material = material;
		}

		public void ConfigureBlur(float blurRadius, int steps)
		{
			blurAmount = blurRadius;
			iterations = steps;
		}
		
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			fullDesc = renderingData.cameraData.cameraTargetDescriptor;
			fullDesc.depthBufferBits = 0;

			cmd.GetTemporaryRT(MIP_DOWN_TEX_P, fullDesc, FilterMode.Bilinear);
			cmd.GetTemporaryRT(MIP_UP_TEX_P, fullDesc, FilterMode.Bilinear);
		}
		
		private static void DrawFullScreenTriangle(CommandBuffer cmd, RenderTargetIdentifier from, RenderTargetIdentifier to, Material blitMaterial, int pass)
		{
			cmd.SetGlobalTexture(SOURCE_TEX_P, from);
			cmd.SetRenderTarget(to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, pass, MeshTopology.Triangles, 3);
		}
		
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get();
			
			using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG))) {
				material.SetFloat(BLUR_OFFSET_P, blurAmount);
				for (var i = 0; i < iterations; ++i) {

				}
				
				for (var i = 0; i < iterations; ++i) {

				}
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
			cmd.ReleaseTemporaryRT(MIP_DOWN_TEX_P);
			cmd.ReleaseTemporaryRT(MIP_UP_TEX_P);
		}
	}
}
