#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"

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

sampler2D _MainTex;
sampler2D _GBufferTexture1;
sampler2D _GBufferTexture2;

fixed4 frag (v2f i) : SV_Target
{
	UnityStandardData data = UnityStandardDataFromGbuffer(tex2D(_MainTex, i.uv), tex2D(_GBufferTexture1, i.uv), tex2D(_GBufferTexture2, i.uv));
	fixed4 col = fixed4(data.diffuseColor, 1.0);
	// just invert the colors
	col = col;
	return col;
}