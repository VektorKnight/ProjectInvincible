Shader "InvincibleEngine/EnergyShield"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Multiplier ("Color Multiplier", Range(0,50)) = 1.0
		_EdgeBrightness ("Edge Brightness", Range(0, 1)) = 0.25
	}

	SubShader
	{
		// Energy Shield Pass
		Tags { 
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

		Pass
		{
			Blend OneMinusDstColor One
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// Vertex Input
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			// Vertex Output
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenUV : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 objectPos : TEXCOORD3;
				float4 vertex : SV_POSITION;
				float depth : DEPTH;
				float3 normal : NORMAL;
			};

			fixed4 _TintColor;
			sampler2D _MainTex;
			half _Multiplier;
			half _EdgeBrightness;
			float4 _MainTex_ST;
			sampler2D _CameraDepthNormalsTexture;
			
			// Vertex Program
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.screenUV = ((o.vertex.xy / o.vertex.w) + 1)/2;
				o.screenUV.y = 1 - o.screenUV.y;

				o.uv += _Time;

				o.objectPos = v.vertex.xyz;
				o.depth = -UnityObjectToViewPos(v.vertex).z *_ProjectionParams.w;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{	
				// Decode depth normals texture to range 0-1
				float screenDepth = DecodeFloatRG(tex2D(_CameraDepthNormalsTexture, i.screenUV).zw);
				float diff = screenDepth - i.depth;
				float intersect = 0;
				
				// Calculate intersections
				if (diff >= 0)
					intersect = 1 - smoothstep(0, _ProjectionParams.w * 2, diff);

				// Calculate glowing rim combine with intersections
				float rim = 1 - abs(dot(i.normal, normalize(i.viewDir))) * 2;
				float northPole = (i.objectPos.y - 0.45) * 20;
				float glow = max(max(intersect, rim), northPole);
				fixed3 edgeColor = lerp(_TintColor.rgb, fixed3(1,1,1), _EdgeBrightness);
				fixed4 glowColor = fixed4(lerp(_TintColor.rgb, edgeColor, pow(glow, 4)), 1);

				// Calculate combined tint and main texture color
				fixed4 combinedColor = _TintColor * tex2D(_MainTex, i.uv) * _Multiplier;

				// Calculate final color from combined color and glow
				fixed4 finalColor = combinedColor * combinedColor.a + glowColor * glow;

				return finalColor;
			}
			ENDCG
		}
	}
}
