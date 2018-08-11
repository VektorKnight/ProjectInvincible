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

        GrabPass { }

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            // Downsampled grab pass and ratio
            sampler2D _GrabTexture;
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

            // Simple Gaussian Blur
            fixed4 GaussianBlur(sampler2D tex, float2 uv, float2 dim, float blurSize) {
                fixed4 s = tex2D(tex, uv) * 0.77548;

                // Horizontal Blur
				s += tex2D(tex, uv + float2(blurSize * 2, 0) / dim.x) * 0.06136;
				s += tex2D(tex, uv + float2(blurSize, 0) / dim.x) * 0.24477;
				s += tex2D(tex, uv + float2(blurSize * -1, 0) / dim.x) * 0.24477;
				s += tex2D(tex, uv + float2(blurSize * -2, 0) / dim.x) * 0.06136;

                // Vertical Blur
                s += tex2D(tex, uv + float2(0, blurSize * 2) / dim.y) * 0.06136;
				s += tex2D(tex, uv + float2(0, blurSize) / dim.y) * 0.24477;			
				s += tex2D(tex, uv + float2(0, blurSize * -1) / dim.y) * 0.24477;
				s += tex2D(tex, uv + float2(0, blurSize * -2) / dim.y) * 0.06136;

                return s/2;
            }

            // Alex-inspired function thing for blur stuff
            fixed4 TrigBlur(sampler2D tex, float2 uv, float2 pxD, float radius, int increments) {
                // Initialize blur result with source sample
                fixed4 sum = tex2D(tex, uv);

                // Initialize blur coef for division
                float coef = 1;

                // Loop from 0-360
                for (float i = 0.0; i < 360; i += (360.0 / increments)) {
                    for (float n = 0.0; n < radius; n++) {
                        // Calculate offsets
                        float offsetX = (cos(i)*n) / pxD.x;
                        float offsetY = (sin(i)*n) / pxD.y;

                        // Sample the screen texture at the given UV + Offset
                        sum += tex2D(tex, uv + float2(offsetX, offsetY));

                        // Increment coef
                        coef++;
                    }
                }

                // Return the sample / coef
                return sum / coef;
            }

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
            fixed4 frag(VertexOutput i) : COLOR {
                // Grab the scene UVs
                float2 screenPos = (i.projPos.xy / i.projPos.w);
                screenPos.y = 1 - screenPos.y;
                
                // Grab sample from main texture
                fixed4 mainTexSample = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));

                // Mask the blurred pass sample by the main texture alpha
                fixed4 blurSample = TrigBlur(_GrabTexture, screenPos, _ScreenParams.xy, 64, 11);

                if (mainTexSample.a > 0.8)
                    blurSample = saturate(blurSample * mainTexSample.a);
                else
                    blurSample *= 0.0;

                // Combine main texture sample color, tint color, and vertex color
                fixed4 combinedColor = mainTexSample * _Color * i.vertexColor;

                // Lerp between the combined screen sample and combined color sample by the combined alpha
                fixed3 finalColor = lerp(blurSample, combinedColor, combinedColor.a);

                // Return the final sample
                return fixed4(finalColor.rgb, mainTexSample.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
