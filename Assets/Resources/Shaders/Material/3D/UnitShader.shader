Shader "InvincibleEngine/UnitShader" {
	Properties {
		// Albedo and Tint
		_TintColor ("Albedo Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		
		// Team Mask and Tint
		_TeamColor ("Team Color", Color) = (1,1,1,1)
		_TeamTex ("Team Map", 2D) = "black" {}
		
		// Emission Mask and Color
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_EmissionTex ("Emission Map", 2D) = "black" {}
		_EmissionPower ("Emission Power", Range(0, 100)) = 1.0

		// Surface Properties
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		// Construction Properties
		_UnbuiltColor("Unbuilt Color", Color) = (0,1,0.25,1)
		_UnbuiltTexture("Unbuilt Texture", 2D) = "white" {}
		_UnbuiltEmission("Unbuilt Emission Power", Range(0, 10)) = 1
		_AlphaMask ("Alpha Mask Texture", 2D) = "white" {}
		_ClipValue ("Alpha Clip Value", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Albedo Texture and Tint
		sampler2D _MainTex;

		// Team Mask and Tint
		sampler2D _TeamTex;

		// Emission Mask and Color
		sampler2D _EmissionTex;

		// Surface Properties
		half _Glossiness;
		half _Metallic;

		// Construction Properties
		sampler2D _UnbuiltTexture;
		sampler2D _AlphaMask;

		// Input Struct
		struct Input {
			float2 uv_MainTex;
			float2 uv_UnbuiltTexture;
			float2 uv_AlphaMask;
			float4 screenPos;
		};

		// Set up instanced properties
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _TeamColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
			UNITY_DEFINE_INSTANCED_PROP(half, _EmissionPower)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _UnbuiltColor)
			UNITY_DEFINE_INSTANCED_PROP(half, _UnbuiltEmission)
			UNITY_DEFINE_INSTANCED_PROP(half, _ClipValue)
		UNITY_INSTANCING_BUFFER_END(Props)

		// Surface program
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Sample and tint main texture (albedo)
			fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor);

			// Sample and tint team texture
			fixed4 team = tex2D(_TeamTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _TeamColor);

			// Sample and tint emission texture (can be set immediately)
			fixed4 emissionColor = UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionColor) * UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionPower);
			fixed4 emission = tex2D(_EmissionTex, IN.uv_MainTex) * emissionColor;

			// Combine albedo and team
			fixed4 finalAlbedo = (albedo + team) / 2;

			// Sample the build mask and unbuilt color property
			fixed4 maskSample = tex2D(_AlphaMask, IN.uv_AlphaMask);
			fixed4 unbuiltSample = tex2D(_UnbuiltTexture, IN.uv_UnbuiltTexture) * UNITY_ACCESS_INSTANCED_PROP(Props, _UnbuiltColor);
			half unbuiltEmission = UNITY_ACCESS_INSTANCED_PROP(Props, _UnbuiltEmission);

			// Set output albedo and emission or unbuilt color based on clip value
			if (maskSample.a <= UNITY_ACCESS_INSTANCED_PROP(Props, _ClipValue)) {
				o.Albedo = finalAlbedo.rgb;
				o.Emission = emission;
			}
			else {
				o.Albedo = unbuiltSample;
				o.Emission = unbuiltSample * unbuiltEmission;
			}

			// Set surface properties and final alpha
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = albedo.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
