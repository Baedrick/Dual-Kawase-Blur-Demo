Shader "Baedrick/DualKawaseBlur"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
	}
	
	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	TEXTURE2D_X(_SourceTex);
	SAMPLER(sampler_linear_clamp);
	ENDHLSL
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
		LOD 100
		ZWrite Off
		Cull Off
		
		Pass
		{
			Name "Pass"
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct MeshData
			{
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertToFrag
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertToFrag vert(uint vertexID : SV_VertexID)
			{
				VertToFrag o;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(input);
				
				o.positionCS = float4(
					vertexID <= 1 ? -1.0 : 3.0,
					vertexID == 1 ? 3.0 : -1.0,
					0.0, 1.0
				);
				o.uv = float2(
					vertexID <= 1 ? 0.0 : 2.0,
					vertexID == 1 ? 2.0 : 0.0
				);
				#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = 1 - o.uv.y;
				#endif
				
				return o;
			}

			half4 frag(VertToFrag input) : SV_TARGET
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				half4 col = SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv, 0);
				return half4(col.x, 1.0, col.z, 1.0);
			}
			ENDHLSL
		}
	}
}
