﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AlphaGradient" {
 Properties{
         _TopColor("Color1", Color) = (1,1,1,1)
         _BottomColor("Color2", Color) = (1,1,1,1)
         _TextureTop("Texture up uv", float) = 0.8
         _TextureBottom("Texture bottom uv", float) = 0.3
         _MainTex ("Main Texture", 2D) = "white" {}
     }
         SubShader
     {
         Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
         Pass
         {
             ZWrite Off
             Blend SrcAlpha OneMinusSrcAlpha
 
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
 
             #include "UnityCG.cginc"
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
 
             fixed4 _TopColor;
             fixed4 _BottomColor;
             half _Value;
 
             struct v2f {
                 float4 position : SV_POSITION;
                 fixed4 color : COLOR;
                 float2 uv : TEXCOORD0;
             };
 
             v2f vert (appdata_full v)
             {
                 v2f o;
                 o.position = UnityObjectToClipPos (v.vertex);
                 o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                 //o.color = lerp (_TopColor,_BottomColor, v.texcoord.y);
                 return o;
             }
 
             fixed4 frag(v2f i) : SV_Target
             {
                 float4 color;
                 color = tex2D(_MainTex, i.uv.xy);
                 float bottom = 0.3;
                 float top = 0.55;
                 
                 float ay = i.uv.y - bottom;
                 float atop = top - bottom;
                 color.a = saturate(lerp(color,_BottomColor, ay / atop).a);
                 
                 return color;
             }
             ENDCG
         }
     }
 }