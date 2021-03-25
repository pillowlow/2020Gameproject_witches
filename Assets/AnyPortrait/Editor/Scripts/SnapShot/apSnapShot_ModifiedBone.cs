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
	/// ModBone 복사를 위한 스냅샷 객체
	/// Pose를 복사하는 기능은 따로 만들자
	/// (Pose 복사를 위해서는 Bone의 다중 선택이 필요하다)
	/// </summary>
	public class apSnapShot_ModifiedBone : apSnapShotBase
	{
		// Members
		//-------------------------------------------------
		// 키 + 데이터
		private apMeshGroup _key_MeshGroupOfMod = null;
		private apMeshGroup _key_MeshGroupOfBone = null;
		private apBone _key_Bone = null;

		//데이터
		public apMatrix _transformMatrix = new apMatrix();


		// Init
		//--------------------------------------------
		public apSnapShot_ModifiedBone() : base()
		{

		}


		// Functions
		//--------------------------------------------
		public override bool IsKeySyncable(object target)
		{
			if (!(target is apModifiedBone))
			{
				return false;
			}

			apModifiedBone targetModBone = target as apModifiedBone;
			if (targetModBone == null)
			{
				return false;
			}

			//Key 체크를 하자
			if (targetModBone._meshGroup_Modifier != _key_MeshGroupOfMod ||
				targetModBone._meshGroup_Bone != _key_MeshGroupOfBone ||
				targetModBone._bone != _key_Bone)
			{
				return false;
			}

			return true;
		}

		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			apModifiedBone modBone = target as apModifiedBone;
			if (modBone == null)
			{
				return false;
			}

			_key_MeshGroupOfMod = modBone._meshGroup_Modifier;
			_key_MeshGroupOfBone = modBone._meshGroup_Bone;
			_key_Bone = modBone._bone;

			_transformMatrix.SetMatrix(modBone._transformMatrix, true);
			return true;
		}

		public override bool Load(object targetObj)
		{
			apModifiedBone modBone = targetObj as apModifiedBone;
			if (modBone == null)
			{
				return false;
			}

			modBone._transformMatrix.SetMatrix(_transformMatrix, true);
			modBone._transformMatrix.MakeMatrix();

			return true;
		}
	}

}