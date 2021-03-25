/*
*	Copyright (c) 2017-2020. RainyRizzle. All rights reserved
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 기존의 optModifiedMesh에서 Color 부분만 가져온 클래스
	/// 최적화를 위해서 분리가 되었다.
	/// 데이터가 훨씬 더 최적화되었다.
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh_Color
	{
		// Members
		//--------------------------------------------
		//[NonSerialized]
		//private apOptModifiedMeshSet _parentModMeshSet = null;

		[SerializeField]
		public Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible = true;

		// Init
		//--------------------------------------------
		public apOptModifiedMesh_Color()
		{

		}

		public void Link(apOptModifiedMeshSet parentModMeshSet)
		{
			//_parentModMeshSet = parentModMeshSet;
		}

		// Init - Bake
		//--------------------------------------------
		public void Bake(Color meshColor, bool isVisible)
		{
			_meshColor = meshColor;
			_isVisible = isVisible;
		}
	}
}