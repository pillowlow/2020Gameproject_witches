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
using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_SDController : MonoBehaviour
	{
		// Target AnyPortrait
		public apPortrait portrait;

		void Start()
		{

		}

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (portrait.IsPlaying("Idle"))
				{
					portrait.StopAll(0.3f);
				}
				else
				{
					portrait.CrossFade("Idle", 0.3f);
				}
			}
		}
	}
}