Shader "InvincibleEngine/Standard/EnergySheild"
{
	Properties
	{
		_SheildTexture ("SheildTexture", 2D) = "white" {}
		_SheildColor("SheildColor", float4)
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100


		Pass
		{
			CGPROGRAM

			//Define functions
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			//Grab properties
			float3 worldPositon:_WorldSpaceCameraPos;


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float4 _SheildColor
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				Change


				return col;
			}
			ENDCG
		}
	}
}
