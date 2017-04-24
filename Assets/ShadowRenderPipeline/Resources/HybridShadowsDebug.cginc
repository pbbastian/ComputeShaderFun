#include "UnityCG.cginc"

sampler2D _MainTex;

fixed4 frag (v2f_img i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    // just invert the colors
    col = 1 - col;
    return col;
}
