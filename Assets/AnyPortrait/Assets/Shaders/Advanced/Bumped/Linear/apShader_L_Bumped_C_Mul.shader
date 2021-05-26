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

Shader "AnyPortrait/Advanced/Bumped/Linear/Multiplicative Clipped"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}			// Main Texture controlled by AnyPortrait
		_MaskTex("Mask Texture (A)", 2D) = "white" {}				// Mask Texture for clipping Rendering (controlled by AnyPortrait)
		_MaskScreenSpaceOffset("Mask Screen Space Offset (XY_Scale)", Vector) = (0, 0, 0, 1)	// Mask Texture's Transform Offset (controlled by AnyPortrait)
		_BumpMap("Bump Texture (Normalmap)", 2D) = "bump" {}		// Bump(Normal) Texture controlled by AnyPortrait
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend DstColor SrcColor//2X Multiply <

		LOD 200

		CGPROGRAM
		//#pragma surface surf SimpleColor alpha
		
		//AlphaBlend가 아닌경우
		#pragma surface surf SimpleColor finalcolor:alphaCorrection noforwardadd

		#pragma target 3.0

		half4 LightingSimpleColor(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			half nl = max(0, dot(s.Normal, lightDir));
			c.rgb = s.Albedo * _LightColor0.rgb * (nl * atten);
			c.a = s.Alpha;
			return c;
		}

		half4 _Color;
		sampler2D _MainTex;

		//Clipped
		sampler2D _MaskTex;
		float4 _MaskScreenSpaceOffset;

		sampler2D _BumpMap;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 screenPos;//Clipped
			float4 color : COLOR;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			//c.rgb *= _Color.rgb * 2.0f;
			c.rgb *= _Color.rgb * 4.595f;//Linear : pow(2, 2.2) = 4.595

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
			
			//Multiply 식
			//o.Albedo = c.rgb;
			//o.Albedo = pow(c.rgb, 2.2f) * (o.Alpha) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - o.Alpha);//Linear
			o.Albedo = pow(c.rgb, 2.2f);//Linear

			//Normal Map
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}

		void alphaCorrection(Input IN, SurfaceOutput o, inout half4 color)
		{
			color.rgb = color.rgb * (color.a) + float4(0.5f, 0.5f, 0.5f, 1.0f) * (1.0f - color.a);
			color.a = 1.0f;
		}


		ENDCG
	}
		FallBack "Diffuse"
}
