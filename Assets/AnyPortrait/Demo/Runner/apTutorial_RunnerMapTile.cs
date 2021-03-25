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

using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_RunnerMapTile : MonoBehaviour
	{
		// Members
		//--------------------------------------------
		public MeshRenderer _renderer;
		public apTutorial_RunnerGame.MapTileType _tileType = apTutorial_RunnerGame.MapTileType.Ground;

		//private bool _isLive = false;


		// Initialize
		//--------------------------------------------
		void Start()
		{
			this.enabled = false;
			Hide();
		}

		public void Show()
		{
			_renderer.enabled = true;
			//_isLive = true;
		}

		public void Hide()
		{
			_renderer.enabled = false;
			//_isLive = false;
		}

		public void SetWorldPos(Vector3 posWorld)
		{
			transform.position = posWorld;
		}

		public void SetWorldPos(float posX, float posY)
		{
			transform.position = new Vector3(posX, posY, transform.position.z);
		}

		public void SetLocalPos(Vector3 posLocal)
		{
			transform.localPosition = posLocal;
		}
	}
}