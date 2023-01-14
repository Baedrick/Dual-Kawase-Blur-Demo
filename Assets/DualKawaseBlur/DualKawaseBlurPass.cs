namespace DualKawaseBlur
{
	using UnityEngine;
	using UnityEngine.Rendering;
	using UnityEngine.Rendering.Universal;
	
	public class DualKawaseBlurPass : ScriptableRenderPass
	{
		private const string PROFILER_TAG = "DualKawaseBlur";
		private static readonly int SOURCE_TEX_P = Shader.PropertyToID("_SourceTex");
		private static readonly int TEMP_TEX_P = Shader.PropertyToID("_TempTex");
		
		private RenderTargetIdentifier tempBuffer = new(TEMP_TEX_P, 0, CubemapFace.Unknown, -1);
		
		private readonly Material material;
		
		public DualKawaseBlurPass(RenderPassEvent renderPassEvent, Material material)
		{
			this.renderPassEvent = renderPassEvent;
			this.material = material;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var fullDesc = renderingData.cameraData.cameraTargetDescriptor;
			fullDesc.depthBufferBits = 0;
			
			cmd.GetTemporaryRT(TEMP_TEX_P, fullDesc, FilterMode.Bilinear);
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
				DrawFullScreenTriangle(cmd, renderingData.cameraData.renderer.cameraColorTarget, tempBuffer, material, 0);
				DrawFullScreenTriangle(cmd, tempBuffer, renderingData.cameraData.renderer.cameraColorTarget, material, 0);
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
			cmd.ReleaseTemporaryRT(TEMP_TEX_P);
		}
	}
}
