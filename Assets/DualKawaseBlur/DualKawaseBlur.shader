Shader "Baedrick/DualKawaseBlur"
{
	Properties
	{
		
	}
	
	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "DualKawaseBlur.hlsl"
	ENDHLSL
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
		LOD 100
		ZTest Always
		ZWrite Off
		Cull Off
		Fog {Mode Off}
		
		Pass
		{ // 0
			Name "Copy"
			
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			ENDHLSL
		}
		
		Pass
		{ // 1
			Name "DownSample"
			
			HLSLPROGRAM
			#pragma vertex Vert_DownSample
			#pragma fragment Frag_DownSample
			ENDHLSL
		}
		
		Pass
		{ // 2
			Name "UpSample"
			
			HLSLPROGRAM
			#pragma vertex Vert_UpSample
			#pragma fragment Frag_UpSample
			ENDHLSL
		}
	}
}
