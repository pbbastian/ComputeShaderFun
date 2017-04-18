// Example shader for a scriptable render loop that calculates multiple lights
// in a single forward-rendered shading pass. Uses same PBR shading model as the
// Standard shader.
//
// The parameters and inspector of the shader are the same as Standard shader,
// for easier experimentation.
Shader "ShadowRenderPipeline/Standard"
{
	// Properties is just a copy of Standard.shader. Our example shader does not use all of them,
	// but the inspector UI expects all these to exist.
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		[Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0
		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
		_ParallaxMap("Height Map", 2D) = "black" {}
		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}
		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_DetailMask("Detail Mask", 2D) = "white" {}
		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}
		[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
		LOD 300

		// Include forward (base + additive) pass from regular Standard shader.
		// They are not used by the scriptable render loop; only here so that
		// if we turn off our example loop, then regular forward rendering kicks in
		// and objects look just like with a Standard shader.
		// UsePass "Standard/FORWARD"
		// UsePass "Standard/FORWARD_DELTA"


		// Multiple lights at once pass, for our example Basic render loop.
		Pass
		{
			Tags{ "LightMode" = "BasicPass" }

			// Use same blending / depth states as Standard shader
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma shader_feature _METALLICGLOSSMAP
#include "StandardShader.cginc"

			ENDCG
		}

		Pass
		{
			Name "SP_SHADOW_CASTER"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			Cull Back

			CGPROGRAM

			#pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _PARABOLOID_MAPPING

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


			#ifndef _PARABOLOID_MAPPING

 float4x4 inverse(float4x4 input)
 {
     #define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
     //determinant(float3x3(input._22_23_23, input._32_33_34, input._42_43_44))

     float4x4 cofactors = float4x4(
          minor(_22_23_24, _32_33_34, _42_43_44),
         -minor(_21_23_24, _31_33_34, _41_43_44),
          minor(_21_22_24, _31_32_34, _41_42_44),
         -minor(_21_22_23, _31_32_33, _41_42_43),

         -minor(_12_13_14, _32_33_34, _42_43_44),
          minor(_11_13_14, _31_33_34, _41_43_44),
         -minor(_11_12_14, _31_32_34, _41_42_44),
          minor(_11_12_13, _31_32_33, _41_42_43),

          minor(_12_13_14, _22_23_24, _42_43_44),
         -minor(_11_13_14, _21_23_24, _41_43_44),
          minor(_11_12_14, _21_22_24, _41_42_44),
         -minor(_11_12_13, _21_22_23, _41_42_43),

         -minor(_12_13_14, _22_23_24, _32_33_34),
          minor(_11_13_14, _21_23_24, _31_33_34),
         -minor(_11_12_14, _21_22_24, _31_32_34),
          minor(_11_12_13, _21_22_23, _31_32_33)
     );
     #undef minor
     return transpose(cofactors) / determinant(input);
 }

            v2f vert(appdata_base i)
            {
            	v2f o;

        		// float3 pos = UnityObjectToViewPos(i.vertex);
            	// float3 pos = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(i.vertex.xyz, 1.0))).xyz;
            	// float4x4 scale = {
            	// 	1, 0, 0, 0,
            	// 	0, 1, 0, 0,
            	// 	0, 0, -1, 0,
            	// 	0, 0, 0, 1
            	// };
            	// float4x4 view = mul(inverse(scale), UNITY_MATRIX_V);
            	float3 pos = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, i.vertex)).xyz;
            	pos.z = -pos.z;
            	pos.y = -pos.y;
            	// pos.x = -pos.x;
            	// pos = pos / pos.w;

            	float L = length(pos.xyz);
            	pos = pos / L;
            	o.clipDepth = pos.z;

            	pos.z = pos.z + 1;
            	pos.x = pos.x / (pos.z);
            	pos.y = pos.y / (pos.z);

            	float near = 0.2; // _ProjectionParams.y;
            	float far = 20; //_ProjectionParams.z;
       //      	half near = _ProjectionParams.y;
    			// half far = _ProjectionParams.z;
            	pos.z = 1 - (L - near) / (far - near);

// #if defined(UNITY_REVERSED_Z)
//                 pos.z = min(pos.z, UNITY_NEAR_CLIP_VALUE);
// #else
//                 pos.z = max(pos.z, UNITY_NEAR_CLIP_VALUE);
// #endif

            	// pos.z = - pos.z;
                o.vertex = float4(pos, 1.0);

                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
            	// clip(i.clipDepth);
                return 0;
            }

            #else

            float4 vert(appdata_base i) : SV_POSITION
            {
            	return ClipSpaceShadowCasterPos(i.vertex, i.normal);
            }

            half4 frag() : SV_TARGET
            {
                return 0;
            }

            #endif

			ENDCG
		}
	}
	CustomEditor "StandardShaderGUI"
}
