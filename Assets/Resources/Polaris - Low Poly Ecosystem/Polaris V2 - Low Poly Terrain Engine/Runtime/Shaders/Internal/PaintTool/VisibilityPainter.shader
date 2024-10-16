﻿Shader "Hidden/Griffin/VisibilityPainter"
{
    Properties
    {
		_MainTex ("MainTex", 2D) = "black" {}
		_Mask ("Mask", 2D) = "white" {}
		_Opacity ("Opacity", Float) = 1
    }

	CGINCLUDE
    #include "UnityCG.cginc"
	#include "Assets/Polaris - Low Poly Ecosystem/Polaris V2 - Low Poly Terrain Engine/Runtime/Shaders/CGIncludes/GriffinCG.cginc"

	struct appdata
    {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
    };

    struct v2f
    {
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 localPos : TEXCOORD1;
    };

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _Mask;
	float _Opacity;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
    }

	fixed4 fragAdd (v2f i) : SV_Target
    {
		float4 maskColor = tex2D(_Mask, i.uv);
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 desColor = currentColor + maskColor.rrrr*_Opacity;
		float value = desColor.a;

		return saturate(float4(currentColor.r, currentColor.g, currentColor.b, value));
	}

	
	fixed4 fragSub (v2f i) : SV_Target
	{
		float4 maskColor = tex2D(_Mask, i.uv);
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 desColor = currentColor - maskColor.rrrr*_Opacity;
		float value = desColor.a;

		return saturate(float4(currentColor.r, currentColor.g, currentColor.b, value));
	}
	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		
        Pass
        {
			//normal painting mode, alpha below 0.5 is visible, otherwise invisible, so subtract pass should have index of 0
			Name "Sub"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragSub
            ENDCG
        }
		
		Pass
        {
			Name "Add"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragAdd
            ENDCG
        }
    }
}
