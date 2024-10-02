// Anaglyph depth kit

uniform Texture2DArray<float> dk_DepthTexture;
uniform Texture2DArray<float4> dk_EdgeDepthTexture;
uniform SamplerState bilinearClampSampler;

uniform float4x4 dk_Proj[2];
uniform float4x4 dk_InvProj[2];

uniform float4x4 dk_View[2];
uniform float4x4 dk_InvView[2];

uniform float4 dk_ZBufferParams;


float SampleDepthNDC(float2 uv, int eye = 0)
{	
    return dk_DepthTexture.SampleLevel(bilinearClampSampler, float3(uv.xy, eye), 0).r;
}

float4 SampleEdgeDepthNDC(float2 uv, int eye = 0)
{
    return dk_EdgeDepthTexture.SampleLevel(bilinearClampSampler, float3(uv.xy, eye), 0);
}

float DepthNDCToLinear(float depthNDC)
{
    return (1.0f / (depthNDC + dk_ZBufferParams.y)) * dk_ZBufferParams.x;
}

float4 WorldtoHCS(float3 worldPos, int eye = 0)
{
    return mul(dk_Proj[eye], mul(dk_View[eye], float4(worldPos, 1)));
}

float4 HCStoWorldH(float4 hcs, int eye = 0)
{
    return mul(dk_InvView[eye], mul(dk_InvProj[eye], hcs));
}

float3 HCStoNDC(float4 hcs)
{
	return (hcs.xyz / hcs.w) * 0.5 + 0.5;
}

float4 NDCtoHCS(float3 ndc)
{
	return float4(ndc * 2.0 - 1.0, 1);
}

float3 WorldtoNDC(float3 worldPos, int eye = 0)
{
    float4 hcs = WorldtoHCS(worldPos, eye);
	return HCStoNDC(hcs);
}

float3 NDCtoWorld(float3 ndc, int eye = 0)
{
    float4 hcs = NDCtoHCS(ndc);
    float4 worldH = HCStoWorldH(hcs, eye);
    return worldH.xyz / worldH.w;
}

bool WorldWithinDepthFrame(float3 world, float crop = 0)
{
    float3 ndc = WorldtoNDC(world);
    return (ndc.x > crop) * (ndc.x < 1 - crop)
		* (ndc.y > crop) * (ndc.y < 1 - crop);
}