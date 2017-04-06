#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma shader_feature _METALLICGLOSSMAP
#include "UnityCG.cginc"
#include "UnityStandardBRDF.cginc"
#include "UnityStandardUtils.cginc"


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


// Surface inputs for evaluating Standard BRDF
struct SurfaceInputData
{
    half3 diffColor, specColor;
    half oneMinusReflectivity, smoothness;
};


// Compute attenuation & illumination from one light
half3 EvaluateOneLight(int idx, float3 positionWS, half3 normalWS, half3 eyeVec, SurfaceInputData s)
{
    // direction to light
    float3 dirToLight = globalLightPos[idx].xyz;
    dirToLight -= positionWS * globalLightPos[idx].w;
    // distance attenuation
    float att = 1.0;
    float distSqr = dot(dirToLight, dirToLight);
    att /= (1.0 + globalLightAtten[idx].z * distSqr);
    if (globalLightPos[idx].w != 0 && distSqr > globalLightAtten[idx].w)
        att = 0.0; // set to 0 if outside of range
    distSqr = max(distSqr, 0.000001); // don't produce NaNs if some vertex position overlaps with the light
    dirToLight *= rsqrt(distSqr);
    // spotlight angular attenuation
    float rho = max(dot(dirToLight, globalLightSpotDir[idx].xyz), 0.0);
    float spotAtt = (rho - globalLightAtten[idx].x) * globalLightAtten[idx].y;
    att *= saturate(spotAtt);

    // Super simple diffuse lighting instead of PBR would be this:
    //half ndotl = max(dot(normalWS, dirToLight), 0.0);
    //half3 color = ndotl * s.diffColor * globalLightColor[idx].rgb;
    //return color * att;

    // Fill in light & indirect structures, and evaluate Standard BRDF
    UnityLight light;
    light.color = globalLightColor[idx].rgb * att;
    light.dir = dirToLight;
    UnityIndirect indirect;
    indirect.diffuse = 0;
    indirect.specular = 0;
    half4 c = BRDF1_Unity_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, normalWS, -eyeVec, light, indirect);
    return c.rgb;
}


// Evaluate 2nd order spherical harmonics, given normalized world space direction.
// Similar to ShadeSH9 in UnityCG.cginc
half3 EvaluateSH(half3 n)
{
    half3 res;
    half4 normal = half4(n, 1);
    // Linear (L1) + constant (L0) polynomial terms
    res.r = dot(globalSH[0], normal);
    res.g = dot(globalSH[1], normal);
    res.b = dot(globalSH[2], normal);
    // 4 of the quadratic (L2) polynomials
    half4 vB = normal.xyzz * normal.yzzx;
    res.r += dot(globalSH[3], vB);
    res.g += dot(globalSH[4], vB);
    res.b += dot(globalSH[5], vB);
    // Final (5th) quadratic (L2) polynomial
    half vC = normal.x * normal.x - normal.y * normal.y;
    res += globalSH[6].rgb * vC;
    return res;
}


// Vertex shader
struct v2f
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 hpos : SV_POSITION;
};

float4 _MainTex_ST;

v2f vert(appdata_base v)
{
    v2f o;
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.hpos = UnityObjectToClipPos(v.vertex);
    o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.normalWS = UnityObjectToWorldNormal(v.normal);
    return o;
}



sampler2D _MainTex;
sampler2D _MetallicGlossMap;
float4 _Color;
float _Metallic;
float _Glossiness;


// Fragment shader
void frag(v2f i, out float4 gbuffer0 : SV_Target0, out float4 gbuffer1 : SV_Target1)
{
    i.normalWS = normalize(i.normalWS);
    half3 eyeVec = normalize(i.positionWS - _WorldSpaceCameraPos);

    // Sample textures
    half4 diffuseAlbedo = tex2D(_MainTex, i.uv) * _Color;
    half2 metalSmooth;
#ifdef _METALLICGLOSSMAP
    metalSmooth = tex2D(_MetallicGlossMap, i.uv).ra;
#else
    metalSmooth.r = _Metallic;
    metalSmooth.g = _Glossiness;
#endif

    // Fill in surface input structure
    SurfaceInputData s;
    s.diffColor = DiffuseAndSpecularFromMetallic(diffuseAlbedo.rgb, metalSmooth.x, s.specColor, s.oneMinusReflectivity);
    s.smoothness = metalSmooth.y;

    // Ambient lighting
    half4 color = half4(0, 0, 0, diffuseAlbedo.a);
    UnityLight light;
    light.color = 0;
    light.dir = 0;
    UnityIndirect indirect;
    indirect.diffuse = EvaluateSH(i.normalWS);
    indirect.specular = 0;
    color.rgb += BRDF1_Unity_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, i.normalWS, -eyeVec, light, indirect);

    // Add illumination from all lights
    for (int il = 0; il < globalLightCount.x; ++il)
    {
        color.rgb += EvaluateOneLight(il, i.positionWS, i.normalWS, eyeVec, s);
    }
    gbuffer0 = color;
    gbuffer1 = float4(1, 0, 0, 1);
}
