#ifndef _GBUFFER_INCLUDED
#define _GBUFFER_INCLUDED

float4x4 _InverseView;

sampler2D _CameraGBufferTexture0;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture3;
sampler2D_float _CameraDepthTexture;

float4 SampleGBuffer0(float2 uv)
{
    return tex2D(_CameraGBufferTexture0, uv);
}

float4 SampleGBuffer1(float2 uv)
{
    return tex2D(_CameraGBufferTexture1, uv);
}

float4 SampleGBuffer2(float2 uv)
{
    return tex2D(_CameraGBufferTexture2, uv);
}

float4 SampleGBuffer3(float2 uv)
{
    return tex2D(_CameraGBufferTexture3, uv);
}

float SampleDepth(float2 uv)
{
    return tex2D(_CameraDepthTexture, uv).x;
}

float SampleLinearDepth(float2 uv)
{
    return LinearEyeDepth(SampleDepth(uv));
}

float3 CalculateWorldPosition(float2 uv, float linearDepth)
{
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * linearDepth;
    return mul(_InverseView, float4(vpos, 1));
}

void SampleLinearDepthAndWorldPosition(float2 uv, out float linearDepth, out float3 worldPosition)
{
    linearDepth = SampleLinearDepth(uv);
    worldPosition = CalculateWorldPosition(uv, linearDepth);
}

float3 Normal(float4 gbuffer2)
{
    return normalize(gbuffer2.rgb * 2 - 1);
}

float Visibility(float4 gbuffer3)
{
    return gbuffer3.r;
}

#endif
