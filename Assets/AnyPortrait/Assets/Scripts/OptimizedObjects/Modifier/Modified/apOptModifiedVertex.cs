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

	[Serializable]
	public class apOptModifiedVertex
	{
		// Members
		//--------------------------------------------
		//버텍스 비교 없이 바로 Index 접근을 한다. (Bake 된 상태이므로 불필요한 체크 필요 없음)
		public int _vertUniqueID = -1;
		public int _vertIndex = -1;

		[SerializeField]
		public apOptMesh _mesh = null;

		public Vector2 _deltaPos = Vector2.zero;



		// Init
		//--------------------------------------------
		public apOptModifiedVertex()
		{

		}

		//public void Init(int vertUniqueID, int vertIndex, apOptMesh mesh, Vector2 deltaPos)
		//{
		//	_vertUniqueID = vertUniqueID;
		//	_vertIndex = vertIndex;
		//	_mesh = mesh;
		//	_deltaPos = deltaPos;
		//}

		public void Bake(apModifiedVertex srcModVert, apOptMesh mesh)
		{
			_vertUniqueID = srcModVert._vertexUniqueID;
			_vertIndex = srcModVert._vertIndex;
			_mesh = mesh;
			_deltaPos = srcModVert._deltaPos;
		}

		// Functions
		//--------------------------------------------


		// Get / Set
		//--------------------------------------------
	}
}