// Background blur for UI elements with texture mask
// Author: VektorKnight
Shader "InvincibleEngine/UI Background Blur" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(1, 32)) = 1
        _BlurIterations ("Blur Iterations", Range(2, 32)) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        GrabPass{ }
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

            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _MainTex; 
            uniform float4 _MainTex_ST;
            uniform float _BlurRadius;
            uniform float _BlurIterations;
            
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

            // Alex-inspired function thing for blur stuff
            float3 TrigBlur(sampler2D tex, float2 uv, float2 pxD, float radius, int increments) {
                // Initialize blur result with source sample
                float4 sum = tex2D(tex, uv);

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
                return sum.rgb / coef;
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
            float4 frag(VertexOutput i) : COLOR {
                // Grab the scene UVs and compensate for flipped vertical axis on UI elements
                float2 screenPos = (i.projPos.xy / i.projPos.w);
                screenPos.y = 1 - screenPos.y;

                // Grab rgb sample from screen
                float3 screenSample = tex2D(_GrabTexture, screenPos).rgb;
                
                // Grab sample from main texture
                float4 mainTexSample = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));

                // Apply blur to the screen sample and mask it by the main texture alpha
                float3 blurSample = TrigBlur(_GrabTexture, screenPos, _ScreenParams.xy, _BlurRadius, _BlurIterations);
                blurSample = saturate(blurSample * mainTexSample.a);

                // Mask the screen sample by the inverse of the main texture alpha and combine with blur sample
                screenSample = saturate((1.0 - mainTexSample.a) * screenSample) + blurSample;

                // Combine main texture sample color, tint color, and vertex color
                float4 combinedColor = mainTexSample * _Color * i.vertexColor;

                // Lerp between the combined screen sample and combined color sample by the combined alpha
                float3 finalColor = lerp(screenSample, combinedColor, combinedColor.a);

                // Return the final color at 100% alpha
                return fixed4(finalColor.rgb,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
