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
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature _METALLICGLOSSMAP
            #include "StandardShaderOpaque.cginc"

			ENDCG
		}

		Pass
		{
			Name "SP_SHADOW_CASTER"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			// Cull Back

			CGPROGRAM

			#pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _PARABOLOID_MAPPING
            #include "StandardShaderShadowCaster.cginc"

			ENDCG
		}
	}
	CustomEditor "StandardShaderGUI"
}
