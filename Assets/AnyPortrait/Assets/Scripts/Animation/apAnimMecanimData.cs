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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AnyPortrait;

namespace AnyPortrait
{
	//[Serializable]
	//public class apAnimMecanimData_AssetPair
	//{
	//	[SerializeField]
	//	public int _animClipUniqueID = -1;

	//	[SerializeField, NonBackupField]
	//	public AnimationClip _animationClip = null;


	//	public apAnimMecanimData_AssetPair(int uniqueID, AnimationClip animationClip)
	//	{
	//		_animClipUniqueID = uniqueID;
	//		_animationClip = animationClip;
	//	}
	//}


	[Serializable]
	public class apAnimMecanimData_Layer
	{
		[SerializeField]
		public int _layerIndex = 0;

		[SerializeField]
		public string _layerName = "";

		public enum MecanimLayerBlendType
		{
			Unknown,
			Override,
			Additive
		}

		[SerializeField]
		public MecanimLayerBlendType _blendType = MecanimLayerBlendType.Unknown;

		public apAnimMecanimData_Layer()
		{

		}

		public apAnimMecanimData_Layer(apAnimMecanimData_Layer srcLayer)
		{
			_layerIndex = srcLayer._layerIndex;
			_layerName = srcLayer._layerName;
			_blendType = srcLayer._blendType;
		}
	}
}