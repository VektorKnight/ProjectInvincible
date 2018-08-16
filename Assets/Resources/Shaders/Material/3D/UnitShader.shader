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
		_BuildColor("Build Effect Color", Color) = (0,1,0.25,1)
		_BuildMask ("Build Mask Texture", 2D) = "white" {}
		_BuildScale("Build Mask Scale", Range(0.01, 4)) = 1
		_BuildEmission("Build Emission Power", Range(0, 20)) = 1
		_BuildEdge ("Build Edge Clip", Range(0, 0.5)) = 0.025
		_ClipValue ("Build Clip Value", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0

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
		sampler2D _BuildTexture;
		sampler2D _BuildMask;

		// Input Struct
		struct Input {
			// UV coords
			float2 uv_MainTex;
			float2 uv_BuildMask;

			// Triplanar mapping
			float3 localCoord;
            float3 localNormal;
		};

		// Set up instanced properties
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _TeamColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
			UNITY_DEFINE_INSTANCED_PROP(half, _EmissionPower)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _BuildColor)
			UNITY_DEFINE_INSTANCED_PROP(half, _BuildScale)
			UNITY_DEFINE_INSTANCED_PROP(half, _BuildEmission)
			UNITY_DEFINE_INSTANCED_PROP(half, _BuildEdge)
			UNITY_DEFINE_INSTANCED_PROP(half, _ClipValue)
		UNITY_INSTANCING_BUFFER_END(Props)

		// Vertex program
		void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.localCoord = v.vertex.xyz;
            data.localNormal = v.normal.xyz;
        }

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

			half clipValue = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipValue);

			// Branch for construction effects
			if (clipValue < 1) {
				// Access instanced copnstruction properties
				fixed4 buildColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BuildColor);
				fixed4 buildEmission = UNITY_ACCESS_INSTANCED_PROP(Props, _BuildEmission) * buildColor;
				half buildEdge = UNITY_ACCESS_INSTANCED_PROP(Props, _BuildEdge);
				half buildScale = UNITY_ACCESS_INSTANCED_PROP(Props, _BuildScale);

				// Blending factor of triplanar mapping
            	float3 bf = normalize(abs(IN.localNormal));
            	bf /= dot(bf, (float3)1);

            	// Triplanar mapping
            	float2 tx = IN.localCoord.yz * buildScale;
            	float2 ty = IN.localCoord.zx * buildScale;
            	float2 tz = IN.localCoord.xy * buildScale;

            	// Build mask sample
            	half4 mx = tex2D(_BuildMask, tx) * bf.x;
            	half4 my = tex2D(_BuildMask, ty) * bf.y;
            	half4 mz = tex2D(_BuildMask, tz) * bf.z;
            	half4 maskSample = (mx + my + mz);

				// Add build color to mask sample
            	half4 buildSample = maskSample * buildColor;

				// Set output albedo and emission or unbuilt color based on clip value
				if (maskSample.a <= clipValue) {
					// Set edge-masked build effects to output
					if (abs(maskSample.a - clipValue) < buildEdge) {
						o.Albedo = buildSample;
						o.Emission = buildEmission;
					}
					else {
						// Set albedo and emission output where build effect is not
						o.Albedo = finalAlbedo.rgb;
						o.Emission = emission;
						o.Metallic = _Metallic;
						o.Smoothness = _Glossiness;
					}
				}
				else {
					// Set unbuilt texture effect to output
					//o.Albedo = fixed4(0.1, 0.1, 0.1, 1) * maskSample;
					//o.Metallic = _Metallic;
					//o.Smoothness = 0;
					discard;
				}
				return;
			}

			// Set albedo and emission output
			o.Albedo = finalAlbedo.rgb;
			o.Emission = emission;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			// Set surface properties
			o.Alpha = albedo.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
