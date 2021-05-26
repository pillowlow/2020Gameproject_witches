/*
*	Copyright (c) 2017-2021. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apPSDBakeData
	{
		// Members
		//-----------------------------------------------------------
		public int _atlasIndex = -1;
		public bool _isLeftover = false;//<<PSD에서 가져온 데이터면 False, PSD에 없지만 Reimport과정에서 Bake된 정보라면 True
		public int _width = 0;
		public int _height = 0;
		public List<apPSDLayerData> _bakedLayerData = new List<apPSDLayerData>();
		public Color[] _colorData = null;
		public string _textureAssetPath = "";
		public Texture2D _bakedImage = null;

		private int _nPixels = 0;


		public string Name
		{
			get
			{
				if(_isLeftover)
				{
					return "Atlas " + _atlasIndex + " (LO)";
				}
				else
				{
					return "Atlas " + _atlasIndex;
				}
			}
		}

		// Init
		//-----------------------------------------------------------
		public apPSDBakeData(int atlasIndex, int width, int height, bool isLeftover)
		{
			_atlasIndex = atlasIndex;
			_width = width;
			_height = height;

			_nPixels = _width * _height;

			_colorData = new Color[_width * _height];//W x H x RGBA;
			_bakedImage = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

			_isLeftover = isLeftover;
		}

		public void ReadyToBake()
		{
			for (int i = 0; i < _nPixels; i++)
			{
				_colorData[i] = Color.clear;
			}


		}

		public bool AddImage(apPSDLayerData layerData, int pixelX, int pixelY, float resizeRatio, int resizedWidth, int resizedHeight, int padding)
		{
			int iLastX = -1;
			int iLastY = -1;
			int iLastIndex = 0;
			try
			{
				Color curColor = Color.black;
				for (int iX = 0; iX < resizedWidth; iX++)
				{
					iLastX = iX;
					for (int iY = 0; iY < resizedHeight; iY++)
					{
						iLastY = iY;
						curColor = layerData._image.GetPixelBilinear((float)iX / (float)resizedWidth, (float)iY / (float)resizedHeight);

						iLastIndex = ((iY + pixelY + padding) * _width) + (iX + pixelX + padding);
						_colorData[iLastIndex] = curColor;
					}
				}

				_bakedLayerData.Add(layerData);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Error");
				Debug.LogError("X, Y Index : " + iLastX + ", " + iLastY + " (" + resizedWidth + ", " + resizedHeight + ")");
				Debug.LogError("Width / Height : " + _width + " / " + _height);
				Debug.LogError("Data Length : " + _colorData.Length);
				Debug.LogError("Last Pixel X : " + (iLastX + pixelX));
				Debug.LogError("Last Pixel Y : " + (iLastY + pixelY));
				Debug.LogError("Last Data Pixel Index : " + (((iLastY + pixelY) * _width) + (iLastX + pixelX)));
				Debug.LogError("Last Index : " + iLastIndex);
				Debug.LogError("AddImage Exception : " + ex);
				return false;
			}

		}

		//TODO : Gaussian Blur를 활용한 이미지 마감을 추가해야한다.
		public void EndToBake(bool isBlur, int padding)
		{
			if (isBlur)
			{
				//RGB 채널 블러를 수행한다.
				//RGB만 이용하려 블러를 수행하고, 원래의 A 값을 이용하여 AlphaBlending을 하자
				Color[] blurColorData = new Color[_colorData.Length];

				int iColor = 0;
				int checkSize = padding / 2;

				if (checkSize < 2)
				{ checkSize = 2; }
				else if (checkSize > 6)
				{ checkSize = 6; }

				for (int iX = 0; iX < _width; iX++)
				{
					for (int iY = 0; iY < _height; iY++)
					{
						iColor = (iY * _width) + iX;
						blurColorData[iColor] = GetBlurredColor(iX, iY, checkSize);
					}
				}
				//이제 다시 돌면서
				//Alpha Blend를 이용해서 값을 섞자
				int dataLength = _colorData.Length;
				for (int i = 0; i < dataLength; i++)
				{
					_colorData[i] = GetAlphaBlendedColor(_colorData[i], blurColorData[i], _colorData[i].a);
				}
			}

			_bakedImage.SetPixels(_colorData);
			_bakedImage.Apply();
		}

		// Functions
		//-----------------------------------------------------------
		private Color GetBlurredColor(int iX, int iY, int checkSize)
		{
			Color srcColor = Color.black;

			float colorR = 0;
			float colorG = 0;
			float colorB = 0;

			float totalWeight = 0.0f;
			int iSubColor = 0;
			float weight = 0.0f;

			for (int iSubX = iX - checkSize; iSubX <= iX + checkSize; iSubX++)
			{
				if (iSubX < 0 || iSubX >= _width)
				{ continue; }

				for (int iSubY = iY - checkSize; iSubY <= iY + checkSize; iSubY++)
				{
					if (iSubY < 0 || iSubY >= _height)
					{ continue; }

					iSubColor = (iSubY * _width) + iSubX;

					srcColor = _colorData[iSubColor];
					if (srcColor.a < 0.01f)
					{
						continue;
					}

					weight = 10.0f / ((float)Mathf.Abs(iSubX - iX) + (float)Mathf.Abs(iSubY - iY) + 0.5f);

					//srcColor = _colorData[iSubColor];

					//if(srcColor.a < 0.01f)
					//{
					//	weight = 0.0f;
					//}
					//else
					//{
					//	//weight *= srcColor.a;	
					//	weight *= 1.0f;
					//}


					totalWeight += weight;

					colorR += srcColor.r * weight;
					colorG += srcColor.g * weight;
					colorB += srcColor.b * weight;
				}
			}
			if (totalWeight > 0.0f)
			{
				colorR /= totalWeight;
				colorG /= totalWeight;
				colorB /= totalWeight;
			}

			return new Color(colorR, colorG, colorB, 1.0f);
		}
		private const float BLUR_ALPHA_BIAS = 0.3f;
		private Color GetAlphaBlendedColor(Color srcColor, Color blurColor, float alpha)
		{
			//return blurColor;
			//if (alpha < BLUR_ALPHA_BIAS)
			//{
			//	return new Color(	((srcColor.r * alpha) + (blurColor.r * (BLUR_ALPHA_BIAS - alpha))) / BLUR_ALPHA_BIAS,
			//						((srcColor.g * alpha) + (blurColor.g * (BLUR_ALPHA_BIAS - alpha))) / BLUR_ALPHA_BIAS,
			//						((srcColor.b * alpha) + (blurColor.b * (BLUR_ALPHA_BIAS - alpha))) / BLUR_ALPHA_BIAS,
			//						alpha);
			//}

			return new Color((srcColor.r * alpha) + (blurColor.r * (1.0f - alpha)),
								(srcColor.g * alpha) + (blurColor.g * (1.0f - alpha)),
								(srcColor.b * alpha) + (blurColor.b * (1.0f - alpha)),
								alpha);

			//return srcColor;
		}
	}

}