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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Ntreev.Library.Psd;

using AnyPortrait;

namespace AnyPortrait
{
	public class apPSDLayerBakeParam
	{
		public apPSDLayerData _targetLayer = null;
		public int _atlasIndex = 0;
		public int _posOffset_X = 0;
		public int _posOffset_Y = 0;
		public bool _isLeftover = false;//PSD로 부터 있는 이미지라면 false, PSD에 없는 이미지라면 True

		public apPSDLayerBakeParam(apPSDLayerData targetLayer,
								int atlasIndex,
								int posOffset_X,
								int posOffset_Y,
								bool isLeftover
								)
		{
			_targetLayer = targetLayer;
			_atlasIndex = atlasIndex;
			_posOffset_X = posOffset_X;
			_posOffset_Y = posOffset_Y;
			_isLeftover = isLeftover;
		}
	}
}
