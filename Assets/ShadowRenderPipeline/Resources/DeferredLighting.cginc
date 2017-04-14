#include "UnityCG.cginc"
#include "UnityStandardBRDF.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"

float4x4 _InverseView;

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

sampler2D _MainTex;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture3;
sampler2D_float _CameraDepthTexture;

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

half4 frag (v2f_img i) : SV_Target
{
    float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).x);
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float2 uv = i.uv;
    float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * depth;
    float4 wpos = mul(_InverseView, float4(vpos, 1));

    float3 eyeVec = normalize(wpos - _WorldSpaceCameraPos);

    UnityStandardData data = UnityStandardDataFromGbuffer(tex2D(_MainTex, i.uv), tex2D(_CameraGBufferTexture1, i.uv), tex2D(_CameraGBufferTexture2, i.uv));
    SurfaceInputData s;
    s.diffColor = data.diffuseColor;
    s.specColor = data.specularColor;
    s.oneMinusReflectivity = 1 - SpecularStrength(data.specularColor.rgb);
    s.smoothness = data.smoothness;

    half4 color = half4(0.0, 0.0, 0.0, 1.0);

    // Ambient lighting
    UnityLight light;
    light.color = 0;
    light.dir = 0;
    UnityIndirect indirect;
    indirect.diffuse = EvaluateSH(data.normalWorld);
    indirect.specular = 0;
    color.rgb += BRDF1_Unity_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, data.normalWorld, -eyeVec, light, indirect);

    float visibility = tex2D(_CameraGBufferTexture3, i.uv).r;

    // Add illumination from all lights
    for (int il = 0; il < globalLightCount.x; ++il)
    {
        color.rgb += EvaluateOneLight(il, wpos, data.normalWorld, eyeVec, s) * visibility;
    }

    return color; 
}