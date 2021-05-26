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

	[Serializable]
	public class apTextureData
	{
		// Members
		//-------------------------------------------
		public int _uniqueID = -1;

		public string _name = "";

		[SerializeField]
		public Texture2D _image = null;
		
		
		public int _width = 0;

		public int _height = 0;

		public string _assetFullPath = "";

		//public bool _isPSDFile = false;//<<이거 삭제


		// Init
		//-------------------------------------------
		/// <summary>
		/// 백업 로드시에만 사용되는 생성자
		/// </summary>
		public apTextureData()
		{
			
		}
		public apTextureData(int index)
		{
			_uniqueID = index;
		}

		public void ReadyToEdit(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Texture, _uniqueID);
		}

		// Get / Set
		//-------------------------------------------
		public void SetImage(Texture2D image, int width, int height)
		{
			_image = image;
			_name = image.name;

			_width = width;
			_height = height;
		}

		
	}

}