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
	/// apModifiedBone의 Opt 버전
	/// </summary>
	[Serializable]
	public class apOptModifiedBone
	{
		// Members
		//--------------------------------------------
		public int _meshGroupID_Bone = -1;
		public int _transformUniqueID = -1;//_meshGropuUniqueID_Bone의 MeshGroupTransform이다.

		public apOptTransform _meshGroup_Bone = null;
		public apOptBone _bone = null;
		public int _boneID = -1;

		public apPortrait _portrait = null;

		/// <summary>
		/// Bone 제어 정보.
		/// </summary>
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();
		
		////추가 : 5.18
		////본 + IK Controller인 경우
		////Position Controller와 LookAt Controller의 Mix Weight를 설정할 수 있다.
		//[SerializeField]
		//public float _boneIKController_MixWeight = 0.0f;

		// Init
		//--------------------------------------------
		public apOptModifiedBone()
		{

		}


		public bool Bake(apModifiedBone srcModBone, apPortrait portrait)
		{
			_portrait = portrait;
			_meshGroupID_Bone = srcModBone._meshGropuUniqueID_Bone;
			apOptTransform meshGroupTransform = portrait.GetOptTransformAsMeshGroup(_meshGroupID_Bone);
			if (meshGroupTransform == null)
			{
				Debug.LogError("[ModBone] Bake 실패 : 찾을 수 없는 OptTransform [" + _meshGroupID_Bone + "]");
				return false;
			}

			_transformUniqueID = meshGroupTransform._transformID;
			_meshGroup_Bone = meshGroupTransform;

			_boneID = srcModBone._boneID;

			_bone = meshGroupTransform.GetBone(_boneID);
			if (_bone == null)
			{
				Debug.LogError("[ModBone] Bake 실패 : 찾을 수 없는 Bone [" + _boneID + "]");
				return false;
			}
			_transformMatrix = new apMatrix(srcModBone._transformMatrix);

			
			//_boneIKController_MixWeight = srcModBone._boneIKController_MixWeight;
			return true;
		}

		// Functions
		//--------------------------------------------


		// Get / Set
		//--------------------------------------------
	}
}