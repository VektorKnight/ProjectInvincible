// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33492,y:32517,varname:node_3138,prsc:2|custl-437-OUT;n:type:ShaderForge.SFN_Tex2d,id:8938,x:32517,y:32479,ptovrint:False,ptlb:Map Texture,ptin:_MapTexture,varname:node_8938,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:1,isnm:False;n:type:ShaderForge.SFN_TexCoord,id:4049,x:32517,y:32739,varname:node_4049,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_If,id:8838,x:33108,y:32605,varname:node_8838,prsc:2|A-4049-U,B-4049-V,GT-433-RGB,EQ-8077-RGB,LT-433-RGB;n:type:ShaderForge.SFN_Color,id:8077,x:32685,y:32879,ptovrint:False,ptlb:node_8077,ptin:_node_8077,varname:node_8077,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Color,id:433,x:32846,y:32954,ptovrint:False,ptlb:node_8077_copy,ptin:_node_8077_copy,varname:_node_8077_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Blend,id:437,x:33278,y:32459,varname:node_437,prsc:2,blmd:10,clmp:True|SRC-8938-RGB,DST-8838-OUT;proporder:8077-433-8938;pass:END;sub:END;*/

Shader "Shader Forge/InvincibleMinimap" {
    Properties {
        _node_8077 ("node_8077", Color) = (1,0,0,1)
        _node_8077_copy ("node_8077_copy", Color) = (0,0,0,1)
        _MapTexture ("Map Texture", 2D) = "gray" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MapTexture; uniform float4 _MapTexture_ST;
            uniform float4 _node_8077;
            uniform float4 _node_8077_copy;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
                float4 _MapTexture_var = tex2D(_MapTexture,TRANSFORM_TEX(i.uv0, _MapTexture));
                float node_8838_if_leA = step(i.uv0.r,i.uv0.g);
                float node_8838_if_leB = step(i.uv0.g,i.uv0.r);
                float3 finalColor = saturate(( lerp((node_8838_if_leA*_node_8077_copy.rgb)+(node_8838_if_leB*_node_8077_copy.rgb),_node_8077.rgb,node_8838_if_leA*node_8838_if_leB) > 0.5 ? (1.0-(1.0-2.0*(lerp((node_8838_if_leA*_node_8077_copy.rgb)+(node_8838_if_leB*_node_8077_copy.rgb),_node_8077.rgb,node_8838_if_leA*node_8838_if_leB)-0.5))*(1.0-_MapTexture_var.rgb)) : (2.0*lerp((node_8838_if_leA*_node_8077_copy.rgb)+(node_8838_if_leB*_node_8077_copy.rgb),_node_8077.rgb,node_8838_if_leA*node_8838_if_leB)*_MapTexture_var.rgb) ));
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
