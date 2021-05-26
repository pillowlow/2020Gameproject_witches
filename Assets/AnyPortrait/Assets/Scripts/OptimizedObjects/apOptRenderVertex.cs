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
	public class apOptRenderVertex
	{
		// Members
		//----------------------------------------------
		//Parent MonoBehaviour
		public apOptTransform _parentTransform = null;
		public apOptMesh _parentMesh = null;


		//Vertex의 값에 해당하는 apVertex가 없으므로 바로 Index 접근을 한다.
		//기본 데이터
		public int _uniqueID = -1;
		public int _index;

		public Vector2 _pos_Local = Vector2.zero;
		//public Vector3 _pos3_Local = Vector3.zero;

		public Vector2 _uv = Vector2.zero;

		public float _zDepth = 0.0f;

		//업데이트 데이터
		public Vector3 _vertPos3_LocalUpdated = Vector3.zero;

		public Vector2 _vertPos_World = Vector2.zero;
		//public Vector3 _vertPos3_World = Vector3.zero;

		// Transform 데이터들
		//0. Rigging
		//리깅의 경우는 Additive없이 Weight, Pos로만 값을 가져온다.
		//레이어의 영향을 전혀 받지 않는다.
		//구버전 코드 : 
		//public Vector2 _pos_Rigging = Vector2.zero;
		//public float _weight_Rigging = 0.0f;//0이면 Vertex Pos를 사용, 1이면 posRigging을 사용한다. 기본값은 0

		//수정된 코드 : Rigging Matrix로 수정
		public apMatrix3x3 _matrix_Rigging = apMatrix3x3.identity;


		//1. [Static] Vert -> Mesh (Pivot)
		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh = apMatrix3x3.identity;

		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;


		//2. [Cal] Vert Local - Blended
		public apMatrix3x3 _matrix_Cal_VertLocal = apMatrix3x3.identity;

		//3. [TF+Cal] 중첩된 Mesh/MeshGroup Transform
		public apMatrix3x3 _matrix_MeshTransform = apMatrix3x3.identity;
		

		//4. [Cal] Vert World - Blended
		public apMatrix3x3 _matrix_Cal_VertWorld = apMatrix3x3.identity;

		//private Vector2 _cal_VertWorld = Vector2.zero;
		
		//5. [TF] Mesh의 Perspective -> Ortho를 위한 변환 매트릭스
		[NonSerialized]
		public apMatrix3x3 _matrix_MeshOrthoCorrection = apMatrix3x3.identity;

		[NonSerialized]
		public bool _isMeshOrthoCorrection = false;

		//추가 2.25
		//6. [Flip] Flip Multiply 값
		[NonSerialized]
		private float _flipWeight_X = 1.0f;
		[NonSerialized]
		private float _flipWeight_Y = 1.0f;

		// 계산 완료
		public apMatrix3x3 _matrix_ToWorld = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_ToVert = apMatrix3x3.identity;


		//계산 관련 변수
		[NonSerialized]
		private bool _isCalculated = false;

		[NonSerialized]
		private Vector2 _cal_posLocalUpdated2 = Vector2.zero;

		//TODO : 물리 관련 지연 변수 추가 필요




		// Init
		//----------------------------------------------
		public apOptRenderVertex(apOptTransform parentTransform, apOptMesh parentMesh,
									int vertUniqueID, int vertIndex, Vector2 vertPosLocal,
									Vector2 vertUV, float zDepth)
		{
			_parentTransform = parentTransform;
			_parentMesh = parentMesh;
			_uniqueID = vertUniqueID;
			_index = vertIndex;
			_pos_Local = vertPosLocal;
			_uv = vertUV;
			_zDepth = zDepth;


			//_pos3_Local = new Vector3(_pos_Local.x, _pos_Local.y, 0);
			//_pos3_Local.x = _pos_Local.x;
			//_pos3_Local.y = _pos_Local.y;
			//_pos3_Local.z = 0;

			_vertPos3_LocalUpdated.x = _pos_Local.x;
			_vertPos3_LocalUpdated.y = _pos_Local.y;
			_vertPos3_LocalUpdated.z = 0;

			_isCalculated = false;

			//_pos_Rigging = Vector2.zero;
			//_weight_Rigging = 0.0f;
			_matrix_Rigging = apMatrix3x3.identity;

			//추가 : 2.25 
			_flipWeight_X = 1.0f;
			_flipWeight_Y = 1.0f;

			_matrix_MeshOrthoCorrection = apMatrix3x3.identity;
			_isMeshOrthoCorrection = false;
		}

		// Functions
		//----------------------------------------------
		// 준비 + Matrix/Delta Pos 입력
		//---------------------------------------------------------
		public void ReadyToCalculate()
		{
			_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			_matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;

			_matrix_Cal_VertLocal = apMatrix3x3.identity;
			_matrix_MeshTransform = apMatrix3x3.identity;

			_matrix_Cal_VertWorld = apMatrix3x3.identity;
			_matrix_ToWorld = apMatrix3x3.identity;
			//_matrix_ToVert = apMatrix3x3.identity;
			_vertPos_World = Vector2.zero;

			//_cal_VertWorld = Vector2.zero;

			_vertPos3_LocalUpdated.x = _pos_Local.x;
			_vertPos3_LocalUpdated.y = _pos_Local.y;
			_vertPos3_LocalUpdated.z = 0;

			//추가 : 2.25 
			_flipWeight_X = 1.0f;
			_flipWeight_Y = 1.0f;

			//_pos_Rigging = Vector2.zero;
			//_weight_Rigging = 0.0f;

			_matrix_Rigging = apMatrix3x3.identity;
			_isMeshOrthoCorrection = false;//<<추가
		}

		//public void SetRigging_0_LocalPosWeight(Vector2 posRiggingResult, float weight)
		public void SetRigging_0_LocalPosWeight(apMatrix3x3 matrix_Rigging, float weight)
		{
			//_pos_Rigging = posRiggingResult;
			//_weight_Rigging = weight;

			_matrix_Rigging.SetMatrixWithWeight(matrix_Rigging, weight);
		}

		public void SetMatrix_1_Static_Vert2Mesh(apMatrix3x3 matrix_Vert2Local)
		{
			_matrix_Static_Vert2Mesh = matrix_Vert2Local;
			_matrix_Static_Vert2Mesh_Inverse = _matrix_Static_Vert2Mesh.inverse;
		}

		public void SetMatrix_2_Calculate_VertLocal(Vector2 deltaPos)
		{
			_matrix_Cal_VertLocal = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
		}

		public void SetMatrix_3_Transform_Mesh(apMatrix3x3 matrix_meshTransform)
		{
			_matrix_MeshTransform = matrix_meshTransform;
		}

		

		public void SetMatrix_4_Calculate_VertWorld(Vector2 deltaPos)
		{
			_matrix_Cal_VertWorld = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
			//_cal_VertWorld = deltaPos;
		}

		//추가
		public void SetMatrix_5_OrthoCorrection(apMatrix3x3 matrix_orthoCorrection)
		{
			_matrix_MeshOrthoCorrection = matrix_orthoCorrection;
			_isMeshOrthoCorrection = true;
		}

		public void SetMatrix_6_FlipWeight(float flipWeightX, float flipWeightY)
		{
			//추가 : 2.25 
			_flipWeight_X = flipWeightX;
			_flipWeight_Y = flipWeightY;
		}

		// Calculate
		//---------------------------------------------------------
		public void Calculate()
		{
			//역순으로 World Matrix를 계산하자
			#region [미사용 코드 : 리깅을 Vertex Pos로 받던 방식]
			//기본 코드
			//_matrix_ToWorld = _matrix_Cal_VertWorld
			//				//* _matrix_TF_Cal_Parent 
			//				//* _matrix_Cal_Mesh 
			//				//* _matrix_TF_Mesh 
			//				* _matrix_MeshTransform
			//				* _matrix_Cal_VertLocal
			//				* _matrix_Static_Vert2Mesh;

			//마개조 코드/.... 예전 코드
			//_matrix_ToWorld._m00 = _matrix_MeshTransform._m00;
			//_matrix_ToWorld._m01 = _matrix_MeshTransform._m01;
			//_matrix_ToWorld._m02 = _matrix_MeshTransform._m00 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
			//						+ _matrix_MeshTransform._m01 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
			//						+ _matrix_MeshTransform._m02
			//						+ _matrix_Cal_VertWorld._m02;

			//_matrix_ToWorld._m10 = _matrix_MeshTransform._m10;
			//_matrix_ToWorld._m11 = _matrix_MeshTransform._m11;
			//_matrix_ToWorld._m12 = _matrix_MeshTransform._m10 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
			//						+ _matrix_MeshTransform._m11 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
			//						+ _matrix_MeshTransform._m12
			//						+ _matrix_Cal_VertWorld._m12;

			//_matrix_ToWorld._m20 = 0;
			//_matrix_ToWorld._m21 = 0;
			//_matrix_ToWorld._m22 = 1; 
			#endregion

			//Rigging이 포함된 코드
			//_matrix_ToWorld = _matrix_Cal_VertWorld // T
			//				* _matrix_MeshTransform // TRS
			//				* _matrix_Rigging//<<추가 // TRS
			//				* _matrix_Cal_VertLocal // T
			//				* _matrix_Static_Vert2Mesh; // T


			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (_matrix_MeshTransform._m00 * _matrix_Rigging._m00) + (_matrix_MeshTransform._m01 * _matrix_Rigging._m10);
			_matrix_ToWorld._m01 = (_matrix_MeshTransform._m00 * _matrix_Rigging._m01) + (_matrix_MeshTransform._m01 * _matrix_Rigging._m11);
			_matrix_ToWorld._m10 = (_matrix_MeshTransform._m10 * _matrix_Rigging._m00) + (_matrix_MeshTransform._m11 * _matrix_Rigging._m10);
			_matrix_ToWorld._m11 = (_matrix_MeshTransform._m10 * _matrix_Rigging._m01) + (_matrix_MeshTransform._m11 * _matrix_Rigging._m11);

			////추가 2.25 : Flip
			//_matrix_ToWorld._m00 *= _flipWeight_X;
			//_matrix_ToWorld._m11 *= _flipWeight_Y;
			

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
								+ _matrix_MeshTransform._m00 * _matrix_Rigging._m02
								+ _matrix_MeshTransform._m01 * _matrix_Rigging._m12
								+ _matrix_Cal_VertWorld._m02
								+ _matrix_MeshTransform._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
								+ _matrix_MeshTransform._m10 * _matrix_Rigging._m02
								+ _matrix_MeshTransform._m11 * _matrix_Rigging._m12
								+ _matrix_Cal_VertWorld._m12
								+ _matrix_MeshTransform._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			

			//_matrix_ToVert = _matrix_ToWorld.inverse;

			//이전 식
			//_vertPos3_World = _matrix_ToWorld.MultiplyPoint3x4(_pos3_Local);

			//리깅 포함한 식으로 변경

			//리깅 변경 이전 코드
			//_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local * (1.0f - _weight_Rigging) + _pos_Rigging * _weight_Rigging);

			//리깅 변경 후 코드
			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= _flipWeight_X;
			_vertPos_World.y *= _flipWeight_Y;

			//_vertPos_World.x = _vertPos3_World.x;
			//_vertPos_World.y = _vertPos3_World.y;

			if(_isMeshOrthoCorrection)
			{
				//추가 : Pers -> Ortho Correction을 적용한다.
				_cal_posLocalUpdated2 = (_matrix_MeshOrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			}
			else
			{
				_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			}
			
			_vertPos3_LocalUpdated.x = _cal_posLocalUpdated2.x;
			_vertPos3_LocalUpdated.y = _cal_posLocalUpdated2.y;
			_vertPos3_LocalUpdated.z = _zDepth * 0.01f;

			_isCalculated = true;
		}


		
		// Get / Set
		//----------------------------------------------
		public bool IsCalculated { get { return _isCalculated; } }
	}
}