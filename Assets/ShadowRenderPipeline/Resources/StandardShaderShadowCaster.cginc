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

float _ShadowBias;

float4 _WorldLightDirAndBias;

#include "UnityCG.cginc"

struct v2f
{
    float4 vertex : SV_POSITION;
    float clipDepth : TEXCOORD1;
    // float depth : TEXCOORD2;
};

// Similar to UnityClipSpaceShadowCasterPos but using LDPipeline lightdir and bias and applying near plane clamp
float4 ClipSpaceShadowCasterPos(float4 vertex, float3 normal)
{
    float4 wPos = mul(unity_ObjectToWorld, vertex);

    if (false && _WorldLightDirAndBias.w > 0.0)
    {
        float3 wNormal = UnityObjectToWorldNormal(normal);

        // apply normal offset bias (inset position along the normal)
        // bias needs to be scaled by sine between normal and light direction
        // (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
        //
        // _WorldLightDirAndBias.w shadow bias defined in LRRenderPipeline asset

        float shadowCos = dot(wNormal, _WorldLightDirAndBias.xyz);
        float shadowSine = sqrt(1 - shadowCos*shadowCos);
        float normalBias = _WorldLightDirAndBias.w * shadowSine;

        wPos.xyz -= wNormal * normalBias;
    }

    float4 clipPos = mul(UNITY_MATRIX_VP, wPos);
#if defined(UNITY_REVERSED_Z)
    clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
#else
    clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
#endif
    return clipPos;
}

float4 ParaboloidProjection(float4 osPosition, float3 osNormal)
{
    float3 pos = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, osPosition)).xyz;

    float3 wsNormal = (UnityObjectToWorldNormal(normalize(osNormal)));
    float3 lightPosition = globalLightPos[0].xyz;
    float isPointLight = globalLightPos[0].w;
    float3 directionToLight = normalize(lightPosition - pos * isPointLight);

    pos.z = -pos.z;
    pos.y = -pos.y;

    float shadowCos = saturate(dot(wsNormal, directionToLight));
    float shadowSine = sqrt(1 - shadowCos*shadowCos);
    pos -= wsNormal * shadowSine * _ShadowBias;

    float L = length(pos.xyz);
    pos = pos / L;
    // o.clipDepth = pos.z;

    pos.z = pos.z + 1;
    pos.x = pos.x / (pos.z);
    pos.y = pos.y / (pos.z);

    float near = 0.2;
    float far = 20;

    pos.z = (L - near) / (far - near);

#if defined(UNITY_REVERSED_Z)
    pos.z = min(1 - pos.z, UNITY_NEAR_CLIP_VALUE);
#else
    pos.z = max(pos.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return float4(pos, 1.0);
}

float4 vert(appdata_base i) : SV_POSITION
{
    #ifdef _PARABOLOID_MAPPING
        return ParaboloidProjection(i.vertex, i.normal);
    #else
       return ClipSpaceShadowCasterPos(i.vertex, i.normal);
    #endif
}

half4 frag() : SV_TARGET
{
    return 0;
}
