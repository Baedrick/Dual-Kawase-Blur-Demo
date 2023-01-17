using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Plugins.DualKawaseBlur.Feature
{
	public class DualKawaseBlurFeature : ScriptableRendererFeature
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
		public DualKawaseBlurPass.Quality quality = DualKawaseBlurPass.Quality.Medium;
		[Range(0.0f, 10.0f)] public float blurRadius = 0.0f;
		
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
			pass.ConfigureBlur(blurRadius, quality);
			renderer.EnqueuePass(pass);
		}
		
		protected override void Dispose(bool disposing)
		{
			CoreUtils.Destroy(material);
		}
	}
}