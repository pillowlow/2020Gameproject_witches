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

	public class apTransform_Vertex
	{
		// Members
		//--------------------------------------------
		[SerializeField]
		public int _meshUniqueID = -1;

		[SerializeField]
		public int _vertexID = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[SerializeField]
		public apMatrix _matrix = new apMatrix();


		// Init
		//--------------------------------------------
		public apTransform_Vertex()
		{

		}


		// Functions
		//--------------------------------------------



		// Get / Set
		//--------------------------------------------
	}
}