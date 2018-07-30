// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:1,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33783,y:32497,varname:node_3138,prsc:2|emission-9018-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32146,y:32827,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:4784,x:32146,y:32651,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_4784,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:3233,x:32444,y:32706,varname:node_3233,prsc:2|A-4784-RGB,B-7241-RGB,C-3603-OUT,D-997-RGB;n:type:ShaderForge.SFN_Multiply,id:349,x:32444,y:32838,varname:node_349,prsc:2|A-4784-A,B-7241-A,C-997-A;n:type:ShaderForge.SFN_VertexColor,id:997,x:32146,y:33055,varname:node_997,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:3603,x:32146,y:32995,ptovrint:False,ptlb:Multiplier,ptin:_Multiplier,varname:node_3603,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ScreenPos,id:186,x:32136,y:33312,varname:node_186,prsc:2,sctp:2;n:type:ShaderForge.SFN_Set,id:1417,x:32320,y:33312,varname:_screenPos,prsc:2|IN-186-UVOUT;n:type:ShaderForge.SFN_Set,id:2855,x:32320,y:33358,varname:_screenPosU,prsc:2|IN-186-U;n:type:ShaderForge.SFN_Set,id:2017,x:32488,y:33459,varname:_screenPosVInv,prsc:2|IN-7672-OUT;n:type:ShaderForge.SFN_Slider,id:1474,x:32137,y:33625,ptovrint:False,ptlb:Blur Iterations,ptin:_BlurIterations,varname:node_1474,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:16;n:type:ShaderForge.SFN_OneMinus,id:7672,x:32324,y:33459,varname:node_7672,prsc:2|IN-186-V;n:type:ShaderForge.SFN_Set,id:7343,x:33813,y:33572,varname:_blurredBackground,prsc:2|IN-8513-OUT;n:type:ShaderForge.SFN_Lerp,id:9018,x:32926,y:32555,varname:node_9018,prsc:2|A-8668-OUT,B-3233-OUT,T-349-OUT;n:type:ShaderForge.SFN_Set,id:4352,x:32444,y:32657,varname:_mainTexAlpha,prsc:2|IN-4784-A;n:type:ShaderForge.SFN_SceneColor,id:8186,x:32476,y:33187,varname:node_8186,prsc:2|UVIN-5211-OUT;n:type:ShaderForge.SFN_Append,id:5211,x:32316,y:33187,varname:node_5211,prsc:2|A-6983-OUT,B-2387-OUT;n:type:ShaderForge.SFN_Get,id:6983,x:32146,y:33187,varname:node_6983,prsc:2|IN-2855-OUT;n:type:ShaderForge.SFN_Get,id:2387,x:32146,y:33237,varname:node_2387,prsc:2|IN-2017-OUT;n:type:ShaderForge.SFN_Set,id:1321,x:32320,y:33410,varname:_screenPosV,prsc:2|IN-186-V;n:type:ShaderForge.SFN_Get,id:7119,x:33363,y:33673,varname:node_7119,prsc:2|IN-4352-OUT;n:type:ShaderForge.SFN_Blend,id:8513,x:33656,y:33663,varname:node_8513,prsc:2,blmd:1,clmp:True|SRC-7843-OUT,DST-7119-OUT;n:type:ShaderForge.SFN_Set,id:6108,x:32678,y:33187,varname:_sceneColor,prsc:2|IN-8186-RGB;n:type:ShaderForge.SFN_Blend,id:791,x:32444,y:32448,varname:node_791,prsc:2,blmd:1,clmp:True|SRC-2993-OUT,DST-4019-OUT;n:type:ShaderForge.SFN_Get,id:4019,x:32260,y:32576,varname:node_4019,prsc:2|IN-6108-OUT;n:type:ShaderForge.SFN_OneMinus,id:2993,x:32281,y:32448,varname:node_2993,prsc:2|IN-8346-OUT;n:type:ShaderForge.SFN_Get,id:8346,x:32111,y:32448,varname:node_8346,prsc:2|IN-4352-OUT;n:type:ShaderForge.SFN_Get,id:1016,x:32444,y:32603,varname:node_1016,prsc:2|IN-7343-OUT;n:type:ShaderForge.SFN_Add,id:8668,x:32632,y:32519,varname:node_8668,prsc:2|A-791-OUT,B-1016-OUT;n:type:ShaderForge.SFN_Code,id:7843,x:32464,y:33583,varname:node_7843,prsc:2,code:LwAvACAAQwBvAG0AcABlAG4AcwBhAHQAZQAgAGYAbwByACAAZgBsAGkAcABwAGUAZAAgAHYAZQByAHQAaQBjAGEAbAAgAFUAVgANAAoAIAAgACAAIAAgACAAIAAgACAAIAAgACAACQBVAFYALgB5ACAAPQAgADEAIAAtACAAVQBWAC4AeQA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAC8ALwAgAEQAZQBmAGkAbgBlACAAZwBhAHUAcwBzAGkAYQBuACAAbwBmAGYAcwBlAHQAcwAgAGEAbgBkACAAdwBlAGkAZwBoAHQAcwANAAoAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIABmAGwAbwBhAHQAIABvAGYAZgBzAGUAdABbAF0AIAA9ACAAewAgADAALAAgADEALAAgADIALAAgADMALAAgADQALAAgADUALAAgADYAfQA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAGYAbABvAGEAdAAgAHcAZQBpAGcAaAB0AFsAXQAgAD0AIAB7ACAAMAAuADAAMAA1ADkAOAAsACAAMAAuADAANgAwADYAMgA2ACwAIAAwAC4AMgA0ADEAOAA0ADMALAAgADAALgAzADgAMwAxADAAMwAsACAAMAAuADIANAAxADgANAAzACwAIAAwAC4AMAA2ADAANgAyADYALAAgADAALgAwADAANQA5ADgAIAB9ADsADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAALwAvACAASQBuAGkAdABpAGEAbABpAHoAZQAgAHAAaQB4AGUAbAAgAHMAYQBtAHAAbABlAA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAGYAbABvAGEAdAAzACAAYgBsAHUAcgBDAG8AbABvAHIAIAA9ACAAdABlAHgAMgBEACgAXwBHAHIAYQBiAFQAZQB4AHQAdQByAGUALAAgAFUAVgApAC4AcgBnAGIAIAAqACAAMAAuADAAMAA1ADkAOAA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAC8ALwAgAFIAdQBuACAAZgBpAGwAdABlAHIAIABhAHMAIABtAGEAbgB5ACAAdABpAG0AZQBzACAAYQBzACAAbgBlAGUAZABlAGQADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAZgBvAHIAIAAoAGkAbgB0ACAAYQAgAD0AIAAwADsAIABhACAAPAAgAGkAdABlAHIAYQB0AGkAbwBuAHMAOwAgAGEAKwArACkAIAB7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAALwAvACAARwBhAHUAcwBzAGkAYQBuACAANwAtAHQAYQBwACAAZgBpAGwAdABlAHIADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIABmAG8AcgAgACgAaQBuAHQAIABpACAAPQAgADAAOwAgAGkAIAA8ACAANwA7ACAAaQArACsAKQAgAHsADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAGIAbAB1AHIAQwBvAGwAbwByACAAKwA9ACAAdABlAHgAMgBEACgAXwBHAHIAYQBiAFQAZQB4AHQAdQByAGUALAAgAFUAVgAgACsAIABmAGwAbwBhAHQAMgAoADAALgAwACwAIABvAGYAZgBzAGUAdABbAGkAXQAgACsAIABhACkAIAAvACAAcAB4AEgAKQAuAHIAZwBiACAAKgAgAHcAZQBpAGcAaAB0AFsAaQBdADsADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAGIAbAB1AHIAQwBvAGwAbwByACAAKwA9ACAAdABlAHgAMgBEACgAXwBHAHIAYQBiAFQAZQB4AHQAdQByAGUALAAgAFUAVgAgAC0AIABmAGwAbwBhAHQAMgAoADAALgAwACwAIABvAGYAZgBzAGUAdABbAGkAXQAgACsAIABhACkAIAAvACAAcAB4AEgAKQAuAHIAZwBiACAAKgAgAHcAZQBpAGcAaAB0AFsAaQBdADsADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIABiAGwAdQByAEMAbwBsAG8AcgAgACsAPQAgAHQAZQB4ADIARAAoAF8ARwByAGEAYgBUAGUAeAB0AHUAcgBlACwAIABVAFYAIAArACAAZgBsAG8AYQB0ADIAKABvAGYAZgBzAGUAdABbAGkAXQAgACsAIABhACwAIAAwAC4AMAApACAALwAgAHAAeABXACkALgByAGcAYgAgACoAIAB3AGUAaQBnAGgAdABbAGkAXQA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIABiAGwAdQByAEMAbwBsAG8AcgAgACsAPQAgAHQAZQB4ADIARAAoAF8ARwByAGEAYgBUAGUAeAB0AHUAcgBlACwAIABVAFYAIAAtACAAZgBsAG8AYQB0ADIAKABvAGYAZgBzAGUAdABbAGkAXQAgACsAIABhACwAIAAwAC4AMAApACAALwAgAHAAeABXACkALgByAGcAYgAgACoAIAB3AGUAaQBnAGgAdABbAGkAXQA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAANAAoAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAALwAvACAARABpAHYAaQBkAGUAIAB0AG8AIAByAGUAdABhAGkAbgAgAGIAcgBpAGcAaAB0AG4AZQBzAHMADQAKACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAGIAbAB1AHIAQwBvAGwAbwByACAALwA9ACAAMQAuADQAMQA0ADIAMQA7AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAfQANAAoAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAB9AA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAA0ACgAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgAC8ALwAgAFIAZQB0AHUAcgBuACAAdABoAGUAIABzAGEAbQBwAGwAZQANAAoAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAAgACAAIAByAGUAdAB1AHIAbgAgAGIAbAB1AHIAQwBvAGwAbwByADsA,output:2,fname:GaussianVertical,width:1015,height:601,input:8,input:1,input:0,input:0,input:0,input_1_label:iterations,input_2_label:UV,input_3_label:pxW,input_4_label:pxH,input_5_label:radius|A-1474-OUT,B-826-OUT,C-9519-PXW,D-9519-PXH,E-5678-OUT;n:type:ShaderForge.SFN_Get,id:826,x:32273,y:33694,varname:node_826,prsc:2|IN-1417-OUT;n:type:ShaderForge.SFN_ScreenParameters,id:9519,x:32294,y:33744,varname:node_9519,prsc:2;n:type:ShaderForge.SFN_Slider,id:5678,x:32156,y:33907,ptovrint:False,ptlb:Blur Radius,ptin:_BlurRadius,varname:node_5678,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:16;proporder:7241-4784-3603-1474-5678;pass:END;sub:END;*/

Shader "InvincibleEngine/Blur Behind UI" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _Multiplier ("Multiplier", Float ) = 1
        _BlurIterations ("Blur Iterations", Range(1, 16)) = 1
        _BlurRadius ("Blur Radius", Range(1, 16)) = 1
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
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Multiplier;
            uniform float _BlurIterations;
            float3 GaussianVertical( fixed iterations , float2 UV , float pxW , float pxH , float radius ){
            // Compensate for flipped vertical UV
                        	UV.y = 1 - UV.y;
                                        
                            // Define gaussian offsets and weights
                            float offset[] = { 0, 1, 2, 3, 4, 5, 6};
                            float weight[] = { 0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598 };
                                        
                            // Initialize pixel sample
                            float3 blurColor = tex2D(_GrabTexture, UV).rgb * 0.00598;
                                        
                            // Run filter as many times as needed
                            for (int a = 0; a < iterations; a++) {
                                // Gaussian 7-tap filter
                                for (int i = 0; i < 7; i++) {
                                    blurColor += tex2D(_GrabTexture, UV + float2(0.0, offset[i] + a) / pxH).rgb * weight[i];
                                    blurColor += tex2D(_GrabTexture, UV - float2(0.0, offset[i] + a) / pxH).rgb * weight[i];
                                    
                                    blurColor += tex2D(_GrabTexture, UV + float2(offset[i] + a, 0.0) / pxW).rgb * weight[i];
                                    blurColor += tex2D(_GrabTexture, UV - float2(offset[i] + a, 0.0) / pxW).rgb * weight[i];
                                                
                                    // Divide to retain brightness
                                    blurColor /= 1.41421;
                                }
                            }
                                        
                            // Return the sample
                            return blurColor;
            }
            
            uniform float _BlurRadius;
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
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 sceneUVs = (i.projPos.xy / i.projPos.w);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float _mainTexAlpha = _MainTex_var.a;
                float _screenPosU = sceneUVs.r;
                float _screenPosVInv = (1.0 - sceneUVs.g);
                float3 _sceneColor = tex2D( _GrabTexture, float2(_screenPosU,_screenPosVInv)).rgb;
                float2 _screenPos = sceneUVs.rg;
                float3 _blurredBackground = saturate((GaussianVertical( _BlurIterations , _screenPos , _ScreenParams.r , _ScreenParams.g , _BlurRadius )*_mainTexAlpha));
                float3 emissive = lerp((saturate(((1.0 - _mainTexAlpha)*_sceneColor))+_blurredBackground),(_MainTex_var.rgb*_Color.rgb*_Multiplier*i.vertexColor.rgb),(_MainTex_var.a*_Color.a*i.vertexColor.a));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
