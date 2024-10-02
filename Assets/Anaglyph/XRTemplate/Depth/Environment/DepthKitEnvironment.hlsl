uniform Texture2D<float4> agdk_EnvHeightMap;

uniform float agdk_EnvSize;
uniform float agdk_EnvMaxHeight;
uniform SamplerState agdk_pointClampSampler;

float2 agdk_WorldToEnvHeightUV(float3 world)
{
    return float2(world.xz / agdk_EnvSize + 0.5);
}

float4 agdk_SampleEnvHeightWorld(float3 world)
{
    float2 uv = agdk_WorldToEnvHeightUV(world);
    return agdk_EnvHeightMap.SampleLevel(agdk_pointClampSampler, uv, 0);
}