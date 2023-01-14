#ifndef DUALKAWASEBLUR_HLSL
#define DUALKAWASEBLUR_HLSL

TEXTURE2D_X(_SourceTex);
SAMPLER(sampler_linear_clamp);

struct MeshData
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F_DownSample
{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

struct V2F_UpSample
{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

V2F_DownSample Vert_DownSample(MeshData input, const uint vertexID : SV_VertexID)
{
	V2F_DownSample output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
	output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
	output.uv = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
	#if UNITY_UV_STARTS_AT_TOP
	output.uv.y = 1 - output.uv.y;
	#endif
				
	return output;
}

V2F_UpSample Vert_UpSample(MeshData input, const uint vertexID : SV_VertexID)
{
	V2F_UpSample output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
	output.positionCS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
	output.uv = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
	#if UNITY_UV_STARTS_AT_TOP
	output.uv.y = 1 - output.uv.y;
	#endif
				
	return output;
}

half4 Frag_DownSample(V2F_DownSample input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	return half4(input.uv, 0.0, 1.0);
}

half4 Frag_UpSample(V2F_UpSample input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	half4 col = SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv, 0);
	return half4(col.x, 1.0, col.z, 1.0);
}
#endif