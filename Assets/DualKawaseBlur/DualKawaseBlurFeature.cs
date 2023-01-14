namespace DualKawaseBlur
{
	using UnityEngine;
	using UnityEngine.Rendering;
	using UnityEngine.Rendering.Universal;
	
	public class DualKawaseBlurFeature : ScriptableRendererFeature
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
		[Range(0, 5)]
		public float blurRadius = 1.0f;
		[Range(1, 5)]
		public int steps = 1;
		
		private Shader shader;
		private Material material;
		private DualKawaseBlurPass pass;
		
		public override void Create()
		{
			if (shader == null) {
				shader = Shader.Find("Baedrick/DualKawaseBlur");
			}
			if (material == null) {
				material = new Material(shader);
			}
			pass = new DualKawaseBlurPass(renderPassEvent, material);
		}
		
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			pass.ConfigureBlur(blurRadius, steps);
			renderer.EnqueuePass(pass);
		}
		
		protected override void Dispose(bool disposing)
		{
			CoreUtils.Destroy(material);
		}
	}
}