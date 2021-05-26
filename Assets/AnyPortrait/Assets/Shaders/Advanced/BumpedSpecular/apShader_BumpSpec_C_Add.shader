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

Shader "AnyPortrait/Advanced/Bumped Specular/Additive Clipped"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}			// Main Texture controlled by AnyPortrait
		_MaskTex("Mask Texture (A)", 2D) = "white" {}				// Mask Texture for clipping Rendering (controlled by AnyPortrait)
		_MaskScreenSpaceOffset("Mask Screen Space Offset (XY_Scale)", Vector) = (0, 0, 0, 1)	// Mask Texture's Transform Offset (controlled by AnyPortrait)
		_BumpMap("Bump Texture (Normalmap)", 2D) = "bump" {}		// Bump(Normal) Texture controlled by AnyPortrait
		_SpecularPower("Specular Power", Float) = 5.0
		_SpecularMap("Specular (Color(RGB), Power(Alpha))", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend One One//Add <

		LOD 200

		CGPROGRAM
		//#pragma surface surf SimpleColor alpha
		#pragma surface surf SimpleColor finalcolor:alphaCorrection//AlphaBlend가 아닌 경우 <

		#pragma target 3.0

		half4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _SpecularPower;
		sampler2D _SpecularMap;

		//Clipped
		sampler2D _MaskTex;
		float4 _MaskScreenSpaceOffset;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_SpecularMap;
			float4 screenPos;//Clipped
			float4 color : COLOR;
		};

		struct CustomSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Alpha;
			half4 Specular;
		};

		half4 LightingSimpleColor(CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half4 c;
			half nl = max(0, dot(s.Normal, lightDir));
			
			half3 h = normalize(lightDir + viewDir);
			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, _SpecularPower * s.Specular.a);
			

			c.rgb = s.Albedo * _LightColor0.rgb * (nl * atten);
			c.rgb += s.Specular.rgb * _LightColor0.rgb * max(0, (spec * atten));

			c.rgb = saturate(c.rgb);

			c.a = s.Alpha;

			return c;
		}


		void surf(Input IN, inout CustomSurfaceOutput o)
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

			//Specular
			o.Specular = tex2D(_SpecularMap, IN.uv_SpecularMap);
		}

		//Additive 계산
		void alphaCorrection(Input IN, CustomSurfaceOutput o, inout half4 color)
		{
			color.rgb *= color.a;
			color.a = 1.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
