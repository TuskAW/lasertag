uniform Texture2D<float2> agdk_EnvHeightMap; // signed float

uniform float agdk_EnvSize;
uniform SamplerState agdk_pointClampSampler;

float2 agdk_WorldToEnvUV(float3 world)
{
    return float2(world.xz / agdk_EnvSize + 0.5);
}

float agdk_SampleEnvWorld(float3 world)
{
    float2 uv = agdk_WorldToEnvUV(world);
    return agdk_EnvHeightMap.SampleLevel(agdk_pointClampSampler, uv, 0).r;
}