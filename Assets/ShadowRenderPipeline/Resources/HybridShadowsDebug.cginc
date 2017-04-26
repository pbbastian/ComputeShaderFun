#include "UnityCG.cginc"
#include "GBuffer.cginc"

sampler2D _TempShadows;

float4 rgb(float r, float g, float b)
{
	 return float4(r/255.0, g/255.0, b/255.0, 1);
}

fixed4 frag (v2f_img i) : SV_Target
{
	float shadowmapVisibility = Visibility(SampleGBuffer3(i.uv));
	float rayTracedVisiblity = tex2D(_TempShadows, i.uv).r;

	float4 color = 0;

	// epsl
	float epsilon = 1e-3;
	float one_epsilon = 1 - epsilon;
	
	if (shadowmapVisibility > epsilon && shadowmapVisibility < one_epsilon)
	{
		color = rgb(0,200,83);
	}
	else
	{
		if (shadowmapVisibility > one_epsilon && rayTracedVisiblity > one_epsilon)
			color.rgb = rgb(238,238,238);
		else if (shadowmapVisibility < epsilon && rayTracedVisiblity < epsilon)
			color.rgb = rgb(158,158,158);
		else
			color = rgb(213,0,0);
	}

    return color;
}
