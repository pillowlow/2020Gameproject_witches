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

	public class apSnapShotStackUnit
	{
		// Members
		//-------------------------------------------
		public string _unitName = "";
		public bool _isCurrentSnapShot = false;

		public apSnapShotBase _snapShot = null;


		// Init
		//-------------------------------------------
		public apSnapShotStackUnit(string unitName)
		{
			_unitName = unitName;
			_isCurrentSnapShot = false;
		}



		// Set Snapshot
		//---------------------------------------------------------------------------
		public bool SetSnapShot_Mesh(apMesh mesh, string strParam)
		{
			_snapShot = new apSnapShot_Mesh();
			return _snapShot.Save(mesh, strParam);
		}

		public bool SetSnapShot_MeshGroup(apMeshGroup meshGroup, string strParam)
		{
			_snapShot = new apSnapShot_MeshGroup();
			return _snapShot.Save(meshGroup, strParam);
		}

		public bool SetSnapShot_Portrait(apPortrait portrait, string strParam)
		{
			_snapShot = new apSnapShot_Portrait();
			return _snapShot.Save(portrait, strParam);
		}

		public bool SetSnapShot_ModMesh(apModifiedMesh modMesh, string strParam)
		{
			_snapShot = new apSnapShot_ModifiedMesh();
			return _snapShot.Save(modMesh, strParam);
		}

		public bool SetSnapShot_Keyframe(apAnimKeyframe keyframe, string strParam)
		{
			_snapShot = new apSnapShot_Keyframe();
			return _snapShot.Save(keyframe, strParam);
		}

		public bool SetSnapShot_VertRig(apModifiedVertexRig vertRig, string strParam)
		{
			_snapShot = new apSnapShot_VertRig();
			return _snapShot.Save(vertRig, strParam);
		}

		public bool SetSnapShot_ModBone(apModifiedBone modBone, string strParam)
		{
			_snapShot = new apSnapShot_ModifiedBone();
			return _snapShot.Save(modBone, strParam);
		}

		// Functions
		//-------------------------------------------
		/// <summary>
		/// Load / Paste가 가능한 "동기화 가능한" 객체인가
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsKeySyncable(object target)
		{
			if (_snapShot != null)
			{
				return _snapShot.IsKeySyncable(target);
			}
			return false;
		}
		public bool Load(object targetObj)
		{
			if (_snapShot != null)
			{
				return _snapShot.Load(targetObj);
			}
			return false;
		}

		public void Unload()
		{
			_isCurrentSnapShot = false;
		}



		// Get / Set
		//-------------------------------------------
	}

}