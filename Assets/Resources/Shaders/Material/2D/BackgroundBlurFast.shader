// Background blur for UI elements with texture mask
// Author: VektorKnight
Shader "InvincibleEngine/UI/BackgroundBlurFast" {
    Properties {
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            // Downsampled and blurred pass
            uniform sampler2D _DSGrabTex;

            // Tint color and main texture
            uniform float4 _TintColor;
            uniform sampler2D _MainTex; 
            uniform float4 _MainTex_ST;
            
            // Vertex input struct
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            
            // Vertex output struct
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
            fixed4 frag(VertexOutput i) : COLOR {
                // Grab the screen UVs
                float2 screenPos = (i.projPos.xy / i.projPos.w);
                
                // Grab sample from main texture
                fixed4 mainTexSample = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));

                // Grab the blurred pass sample and mask it by the main texture alpha
                fixed4 blurSample = tex2D(_DSGrabTex, screenPos);

                if (mainTexSample.a > 0.8)
                    blurSample = saturate(blurSample * mainTexSample.a);
                else
                    blurSample *= 0.0;

                // Combine main texture sample color, tint color, and vertex color
                fixed4 combinedColor = mainTexSample * _TintColor * i.vertexColor;

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
