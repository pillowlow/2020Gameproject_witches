/*
*	Copyright (c) 2017-2021. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	[Serializable]
	public class apTransform_MeshGroup
	{
		// Members
		//--------------------------------------------
		[SerializeField]
		public int _meshGroupUniqueID = -1;

		[SerializeField]
		public int _transformUniqueID = -1;

		[SerializeField]
		public string _nickName = "";

		[NonSerialized]
		public apMeshGroup _meshGroup = null;

		[SerializeField]
		public apMatrix _matrix = new apMatrix();//이건 기본 Static Matrix

		[SerializeField]
		public Color _meshColor2X_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible_Default = true;

		[SerializeField]
		public int _depth = 0;

		[SerializeField]
		public int _level = 0;//Parent부터 내려오는 Level


		//추가 : Socket
		//Bake할때 소켓을 생성한다.
		[SerializeField]
		public bool _isSocket = false;


		//[SerializeField]
		//public Color _color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_ToParent = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_LocalModified = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_World = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_WorldWithoutMod = new apMatrix();

		//계산용 변수
		///// <summary>Parent로부터 누적된 WorldMatrix. 자기 자신의 Matrix는 포함되지 않는다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Parent = apMatrix3x3.identity;

		////추가
		///// <summary>누적되지 않은 기본 Pivot Transform + Modifier 결과만 가지고 있는 값이다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Local = apMatrix3x3.identity;


		//private apMatrix _calculateTmpMatrix = new apMatrix();
		//public apMatrix CalculatedTmpMatrix {  get { return _calculateTmpMatrix; } }

		//private apMatrix _calculateTmpMatrix_Local = new apMatrix();


		[NonSerialized]
		public apRenderUnit _linkedRenderUnit = null;

		//[NonSerialized]
		//public bool _isVisible_TmpWork = true;//<<이값이 false이면 아예 렌더링이 안된다. 작업용. 허용되는 경우 외에는 항상 True

		// Init
		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서 호출하지 말자
		/// </summary>
		public apTransform_MeshGroup()
		{
			
		}
		public apTransform_MeshGroup(int uniqueID)
		{
			_transformUniqueID = uniqueID;
		}

		public void RegistIDToPortrait(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Transform, _transformUniqueID);
		}

		// Functions
		//--------------------------------------------
		public void ReadyToCalculate()
		{
			_matrix.MakeMatrix();

			//_matrix_TF_Cal_Parent = apMatrix3x3.identity;
			//_matrix_TF_Cal_Local = _matrix.MtrxToSpace;

			//if(_calculateTmpMatrix == null)
			//{
			//	_calculateTmpMatrix = new apMatrix();
			//}
			//_calculateTmpMatrix.SetIdentity();
			//_calculateTmpMatrix.SetMatrix(_matrix);

			//if(_calculateTmpMatrix_Local == null)
			//{
			//	_calculateTmpMatrix_Local = new apMatrix();
			//}
			//_calculateTmpMatrix_Local.SetIdentity();
			//_calculateTmpMatrix_Local.SetMatrix(_matrix);

			//변경
			//[Parent World x To Parent x Local TF] 조합으로 변경

			if (_matrix_TF_ParentWorld == null)				{ _matrix_TF_ParentWorld = new apMatrix(); }
			if (_matrix_TF_ToParent == null)				{ _matrix_TF_ToParent = new apMatrix(); }
			if (_matrix_TF_LocalModified == null)			{ _matrix_TF_LocalModified = new apMatrix(); }
			if (_matrix_TFResult_World == null)				{ _matrix_TFResult_World = new apMatrix(); }
			if (_matrix_TFResult_WorldWithoutMod == null)	{ _matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			_matrix_TF_ParentWorld.SetIdentity();
			_matrix_TF_ToParent.SetIdentity();
			_matrix_TF_LocalModified.SetIdentity();

			//ToParent는 Pivot이므로 고정
			_matrix_TF_ToParent.SetMatrix(_matrix, true);

			_matrix_TFResult_World.SetIdentity();
			_matrix_TFResult_WorldWithoutMod.SetIdentity();
		}

		public void SetModifiedTransform(apMatrix matrix_modified)
		{
			////_calculateTmpMatrix_Local.SRMultiply(matrix_modified, true);//Parent
			//_calculateTmpMatrix_Local.SRMultiply(matrix_modified, false);//Child

			//_matrix_TF_Cal_Local = _calculateTmpMatrix_Local.MtrxToSpace;

			_matrix_TF_LocalModified.SetMatrix(matrix_modified, false);
		}


		/// <summary>
		/// Parent의 Matrix를 추가한다. (Parent x This)
		/// </summary>
		/// <param name="matrix_parentTransform"></param>
		//public void AddWorldMatrix_Parent(apMatrix3x3 matrix_parentTransform)
		public void AddWorldMatrix_Parent(apMatrix matrix_parentTransform)
		{
			//_matrix_TF_Cal_Parent = matrix_parentTransform.MtrxToSpace * _matrix_TF_Cal_Parent;
			////_calculateTmpMatrix.SRMultiply(matrix_parentTransform, true);
			////_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

			_matrix_TF_ParentWorld.SetMatrix(matrix_parentTransform, false);
		}

		public void MakeTransformMatrix()
		{

			//중요! 계산에 적용되는 Matrix를 여기서 Make하자
			//_matrix_TF_ToParent.MakeMatrix();
			_matrix_TF_LocalModified.MakeMatrix();
			_matrix_TF_ParentWorld.MakeMatrix();

			//1) SR Multiply로 만드는 경우
			//[SR]
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_LocalModified, true);
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_ToParent, true);
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_ParentWorld, true);

			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ToParent, true);
			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ParentWorld, true);

			//[R]
			//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//위치 이동 20.10.28
			//_matrix_TFResult_World.OnBeforeRMultiply();
			//_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();

			//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent, false);//이전
			_matrix_TFResult_World.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			_matrix_TFResult_World.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)
			

			_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified, false);
			_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld, true);

			//_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ToParent, false);//이전
			_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)


			_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld, true);
		}

		

		// Get / Set
		//--------------------------------------------
	}
}