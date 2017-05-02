#include "UnityCG.cginc"
#include "GBuffer.cginc"

// Global lighting data (setup from C# code once per frame).
CBUFFER_START(GlobalLightData)
    // The variables are very similar to built-in unity_LightColor, unity_LightPosition,
    // unity_LightAtten, unity_SpotDirection as used by the VertexLit shaders, except here
    // we use world space positions instead of view space.
half4 globalLightColor[8];
float4 globalLightPos[8];
float4 globalLightSpotDir[8];
float4 globalLightAtten[8];
int4 globalLightCount;
    // Global ambient/SH probe, similar to unity_SH* built-in variables.
float4 globalSH[7];
CBUFFER_END

float4x4 _LightView;
// sampler2D _ShadowmapTexture;
Texture2D<float4> _ShadowmapTexture;
SamplerState sampler_ShadowmapTexture;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float depth = SampleLinearDepth(i.uv);
    if (depth < 1e-3)
        return 0;
    float3 wsPosition = CalculateWorldPosition(i.uv, depth);
    float3 normal = Normal(SampleGBuffer2(i.uv));
    float3 lightPosition = globalLightPos[0].xyz;
    float isPointLight = globalLightPos[0].w;
    float3 directionToLight = normalize(lightPosition - wsPosition * isPointLight);
    if (dot(normal, directionToLight) < 0)
        return 0;

    float3 lsPosition = mul(_LightView, float4(wsPosition, 1.0)).xyz;
    lsPosition.z = -lsPosition.z;
    float distanceToLight = length(lsPosition);
    lsPosition = lsPosition / distanceToLight;

    lsPosition.z = lsPosition.z + 1;
    lsPosition.x = lsPosition.x / (lsPosition.z);
    lsPosition.y = lsPosition.y / (lsPosition.z);

    float near = 0.2;
    float far = 20;

    lsPosition.z = (distanceToLight - near) / (far - near);

#if defined(UNITY_REVERSED_Z)
    lsPosition.z = min(1 - lsPosition.z, UNITY_NEAR_CLIP_VALUE);
#else
    lsPosition.z = max(lsPosition.z, UNITY_NEAR_CLIP_VALUE);
#endif

    // float sceneDepth = (distanceToLight - near) / (far - near);
    float4 occluderDepths = _ShadowmapTexture.Gather(sampler_ShadowmapTexture, float2(lsPosition.x * 0.5 + 0.5,  lsPosition.y * 0.5 + 0.5));
    float4 occlusions = saturate(sign(lsPosition.zzzz - occluderDepths + 0.01));
    float occlusion = occlusions.r + occlusions.g + occlusions.b + occlusions.a;
    occlusion *= 0.25;

    // float3 normal = Normal(SampleGBuffer2(i.uv));
    // float3 lightPosition = globalLightPos[0].xyz;
    // float isPointLight = globalLightPos[0].w;
    // float3 directionToLight = normalize(lightPosition - worldPosition * isPointLight);


    // return occluderDepth;

    return occlusion;
}
