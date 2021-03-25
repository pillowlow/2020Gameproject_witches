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
using UnityEditor;

using AnyPortrait;

namespace AnyPortrait
{
	[CustomEditor(typeof(apOptMultiCameraController))]
	public class apInspector_MultiCamController : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if(!Application.isPlaying)
			{
				return;
			}

			apOptMultiCameraController targetMCC = target as apOptMultiCameraController;

			if(targetMCC == null)
			{
				return;
			}
			
			GUILayout.Space(10);
			Dictionary<apOptMesh, apOptMultiCameraController.FUNC_MESH_PRE_RENDERED> preRenderedEvents = targetMCC.GetPreRenderedEvents();
			if(preRenderedEvents != null && preRenderedEvents.Count > 0)
			{
				int index = 0;
				foreach (KeyValuePair<apOptMesh, apOptMultiCameraController.FUNC_MESH_PRE_RENDERED> pair in preRenderedEvents)
				{
					EditorGUILayout.LabelField("[" + index + "] : " + pair.Key.name);
					index++;
				}
			}
		}
	}
}