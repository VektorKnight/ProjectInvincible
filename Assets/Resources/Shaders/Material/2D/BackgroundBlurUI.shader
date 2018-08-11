// Background blur for UI elements with texture mask
// Author: VektorKnight
Shader "InvincibleEngine/UI/BackgroundBlur" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            // Downsampled grab pass and ratio
            sampler2D _DSGrabTex;
            int _DSGrabRatio;
            uniform float4 _Color;
            uniform sampler2D _MainTex; 
            uniform float4 _MainTex_ST;
            
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
            };

            // Vertex shader
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            
            // Fragment shader
            float4 frag(VertexOutput i) : COLOR {
                // Grab the scene UVs
                float2 screenPos = (i.projPos.xy / i.projPos.w);
                
                // Grab sample from main texture
                float4 mainTexSample = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));

                // Mask the blurred pass sample by the main texture alpha
                float3 blurSample = tex2D(_DSGrabTex, screenPos).rgb;
                if (mainTexSample.a > 0.8)
                    blurSample = saturate(blurSample * mainTexSample.a);
                else
                    blurSample *= 0.0;

                // Combine main texture sample color, tint color, and vertex color
                float4 combinedColor = mainTexSample * _Color * i.vertexColor;

                // Lerp between the combined screen sample and combined color sample by the combined alpha
                float3 finalColor = lerp(blurSample, combinedColor, combinedColor.a);

                // Return the final sample
                return fixed4(finalColor.rgb, mainTexSample.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
