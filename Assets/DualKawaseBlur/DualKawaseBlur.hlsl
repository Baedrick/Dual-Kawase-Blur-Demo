#ifndef DUALKAWASEBLUR_HLSL
#define DUALKAWASEBLUR_HLSL

TEXTURE2D_X(_SourceTex);
SAMPLER(sampler_linear_clamp);
float4 _SourceTex_TexelSize;
half1 _BlurOffset;

struct MeshData
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct V2F
{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

struct V2F_DownSample
{
	float4 positionCS : SV_POSITION;
	float2 uv0 : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
	UNITY_VERTEX_OUTPUT_STEREO
};

struct V2F_UpSample
{
	float4 positionCS : SV_POSITION;
	float2 uv0 : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
	float4 uv3 : TEXCOORD3;
	float4 uv4 : TEXCOORD4;
	UNITY_VERTEX_OUTPUT_STEREO
};

V2F Vert(MeshData input, const uint vertexID : SV_VertexID)
{
	V2F output;
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
	output.uv.y = 1.0 - output.uv.y;
	#endif
	
	return output;
}

half4 Frag(const V2F input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	return SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv, 0);
}

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
	output.uv0 = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
	#if UNITY_UV_STARTS_AT_TOP
	output.uv0.y = 1.0 - output.uv0.y;
	#endif

	const float2 uv = output.uv0;
	const float2 halfPixel = _SourceTex_TexelSize * 0.5;
	const float2 offset = float2(1.0 + _BlurOffset, 1.0 + _BlurOffset);
	
	output.uv1.xy = uv - halfPixel * offset; 
	output.uv1.zw = uv + halfPixel * offset;
	
	output.uv2.xy = uv - float2(halfPixel.x, -halfPixel.y) * offset;
	output.uv2.zw = uv + float2(halfPixel.x, -halfPixel.y) * offset;
	return output;
}

half4 Frag_DownSample(const V2F_DownSample input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	half4 sum = SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv0, 0) * 4.0;
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv1.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv1.zw, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv2.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv2.zw, 0);
	return sum * 0.125;
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
	output.uv0 = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);
	#if UNITY_UV_STARTS_AT_TOP
	output.uv0.y = 1.0 - output.uv0.y;
	#endif

	const float2 uv = output.uv0;
	const float2 halfPixel = _SourceTex_TexelSize * 0.5;
	const float2 offset = float2(1.0 + _BlurOffset, 1.0 + _BlurOffset);
	
	output.uv1.xy = uv + float2(-halfPixel.x * 2.0, 0.0) * offset;
	output.uv1.zw = uv + float2(-halfPixel.x, halfPixel.y) * offset;
	
	output.uv2.xy = uv + float2(0.0, halfPixel.y * 2.0) * offset;
	output.uv2.zw = uv + halfPixel * offset;

	output.uv3.xy = uv + float2(halfPixel.x * 2.0, 0.0) * offset;
	output.uv3.zw = uv + float2(halfPixel.x, -halfPixel.y) * offset;
	
	output.uv4.xy = uv + float2(0.0, -halfPixel.y * 2.0) * offset;
	output.uv4.zw = uv - halfPixel * offset;
	
	return output;
}

half4 Frag_UpSample(const V2F_UpSample input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	half4 sum = SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv1.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv1.zw, 0) * 2.0;
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv2.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv2.zw, 0) * 2.0;
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv3.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv3.zw, 0) * 2.0;
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv4.xy, 0);
	sum += SAMPLE_TEXTURE2D_X_LOD(_SourceTex, sampler_linear_clamp, input.uv4.zw, 0) * 2.0;
	return sum * 0.0833;
}
#endif