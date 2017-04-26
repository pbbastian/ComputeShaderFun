#include "UnityCG.cginc"
#include "GBuffer.cginc"

sampler2D _TempShadows;

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
		color.bg = 1;
	}
	else
	{
		if (shadowmapVisibility > one_epsilon && rayTracedVisiblity > one_epsilon)
			color.g = 0.5;
		else if (shadowmapVisibility < epsilon && rayTracedVisiblity < epsilon)
			color.b = 0.5;
		else
			color.r = 1;
	}

    return color;
}
