/*
*	Copyright (c) 2017-2019. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee].
*
*	Unless this file is downloaded from the Unity Asset Store or RainyRizzle homepage,
*	this file and its users are illegal.
*	In that case, the act may be subject to legal penalties.
*/

Shader "AnyPortrait/Advanced/Bumped Ramp/Additive Clipped"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}			// Main Texture controlled by AnyPortrait
		_MaskTex("Mask Texture (A)", 2D) = "white" {}				// Mask Texture for clipping Rendering (controlled by AnyPortrait)
		_MaskScreenSpaceOffset("Mask Screen Space Offset (XY_Scale)", Vector) = (0, 0, 0, 1)	// Mask Texture's Transform Offset (controlled by AnyPortrait)
		_BumpMap("Bump Texture (Normalmap)", 2D) = "bump" {}		// Bump(Normal) Texture controlled by AnyPortrait
		_RampMap("Ramp Gradient Map (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend One One//Add <

		LOD 200

		CGPROGRAM
		//#pragma surface surf SimpleColor alpha
		#pragma surface surf SimpleColor finalcolor:alphaCorrection//AlphaBlend가 아닌 경우 <

		#pragma target 3.0

		half4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;

		//Clipped
		sampler2D _MaskTex;
		float4 _MaskScreenSpaceOffset;

		//Ramp
		sampler2D _RampMap;
		
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 screenPos;//Clipped
			float4 color : COLOR;
		};

		half4 LightingSimpleColor(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			half nl = max(0, dot(s.Normal, lightDir));

			//Ramped Diffuse
			half halfDiff = (nl * 0.5f) + 0.5f;
			half3 rampColor = tex2D(_RampMap, float2(halfDiff, 0.5f)).rgb;

			c.rgb = saturate(s.Albedo * _LightColor0.rgb * (rampColor * atten));

			c.a = s.Alpha;

			return c;
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			c.rgb *= _Color.rgb * 2.0f;

			//-------------------------------------------
			// Clipped
			float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w, 0.0001f);

			screenUV -= float2(0.5f, 0.5f);

			screenUV.x *= _MaskScreenSpaceOffset.z;
			screenUV.y *= _MaskScreenSpaceOffset.w;
			screenUV.x += _MaskScreenSpaceOffset.x * _MaskScreenSpaceOffset.z;
			screenUV.y += _MaskScreenSpaceOffset.y * _MaskScreenSpaceOffset.w;

			screenUV += float2(0.5f, 0.5f);

			c.a *= tex2D(_MaskTex, screenUV).r;
			//-------------------------------------------

			o.Alpha = c.a * _Color.a;
			o.Albedo = c.rgb;

			//Normal Map
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}

		//Additive 계산
		void alphaCorrection(Input IN, SurfaceOutput o, inout half4 color)
		{
			color.rgb *= color.a;
			color.a = 1.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
