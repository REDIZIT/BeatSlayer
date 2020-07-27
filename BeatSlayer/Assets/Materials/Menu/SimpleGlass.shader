// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SemiTransparent"
{
    Properties
    {
        _Color("Color",Color) = (0,0,1,0.1)
    }
    SubShader
    {
Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

        LOD 100

        Pass
        {
            Stencil {
                Ref 0
                Comp Equal
                Pass IncrSat 
                Fail IncrSat 
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screen : TEXCOORD1;
                float4 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_base  v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen = ComputeScreenPos(o.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //_Color.r
                float brightness = i.screen.y;

                fixed4 col = fixed4(_Color.rgb, 0.4);

                //fixed4 col = fixed4(_Color.rgb,i.texcoord.y);

                return col;
            }
            ENDCG
        }
    }
}