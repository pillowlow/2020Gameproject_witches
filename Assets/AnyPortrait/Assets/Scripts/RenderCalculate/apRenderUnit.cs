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
//using System.Runtime.InteropServices;
//using UnityEngine.Profiling;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// 렌더링시 Depth Sort위한 파라미터 객체
	/// 저장되는 값은 아니다. (Realtime용 파라미터)
	/// 저장은 계층적으로 만들고 (Modifier/Matrix 계산 위함)
	/// 렌더링은 선형적으로
	/// </summary>
	public class apRenderUnit
	{
		// Members
		//---------------------------------------
		public enum UNIT_TYPE
		{
			GroupNode = 0,
			Mesh = 1
		}
		public UNIT_TYPE _unitType = UNIT_TYPE.Mesh;

		public apPortrait _portrait = null;
		public apMeshGroup _meshGroup = null;
		public apTransform_MeshGroup _meshGroupTransform = null;//<<이게 Null이면 Root이다.

		public apTransform_Mesh _meshTransform = null;

		public int _level = 0;//Parent부터 내려오는 Level
							  //public int _depth = 0;//<<이거 계산에 문제가 있다.
		private int _depthForSort = 0;//변경 : Sorting을 위한 값으로만 사용

		public int _guiIndex = -1;

		public apRenderUnit _parentRenderUnit = null;
		public List<apRenderUnit> _childRenderUnits = new List<apRenderUnit>();

		//Matrix - 현재 / 누적
		//public apMatrix3x3 _matrixToParent = apMatrix3x3.identity;//현재 유닛의 Matrix
		//public apMatrix3x3 _matrixToWorld = apMatrix3x3.identity;//바로 World 가기 위한 누적 Matrix
		//public apMatrix3x3 _matrixVertToLocal = apMatrix3x3.identity;
		//public apMatrix3x3 _matrixWorldToVert = apMatrix3x3.identity;

		//public apMatrix3x3 _curMatrix_Pivot = apMatrix3x3.identity;//현재 유닛의 Matrix
		//public apMatrix3x3 _curMatrixToWorld_Pivot = apMatrix3x3.identity;//바로 World 가기 위한 누적 Matrix


		//에디터용
		public bool _isSelectedInEditor = false;

		//계산된 VertexList (Mesh 타입인 경우)
		[NonSerialized]
		public List<apRenderVertex> _renderVerts = new List<apRenderVertex>();




		//Modifier의 값을 전달 받는 Stack
		[NonSerialized]
		public apCalculatedResultStack _calculatedStack = null;

		[NonSerialized]
		private bool _isRenderVertsInit = false;

		[NonSerialized]
		public Color _meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1);

		[NonSerialized]
		public bool _isVisible = true;//Color값에 의해 렌더링 여부를 결정한다.

		[NonSerialized]
		public bool _isVisible_WithoutParent = true;//계층에 상관없는 IsVislble

		[NonSerialized]
		public bool _isVisibleCalculated = true;//Color값에 의해 렌더링 여부를 결정한다.


		//이전 : TmpWork만 있다.
		//[NonSerialized]
		//public bool _isVisibleWorkToggle_Hide2Show = false;//Hide 상태일때 작업을 위해 Show로 만든다.

		//[NonSerialized]
		//public bool _isVisibleWorkToggle_Show2Hide = false;//Show 상태일때 작업을 위해 Hide로 만든다.

		//변경 21.1.28 : Tmp Visibilie을 두개의 Bool이 아닌 하나의 enum으로 변경한다
		//추가로 Visibility Preset Rule에 의해서도 제어받을 수 있게 변수를 추가한다.
		public enum WORK_VISIBLE_TYPE { None, ToShow, ToHide }

		[NonSerialized]
		public WORK_VISIBLE_TYPE _workVisible_Tmp = WORK_VISIBLE_TYPE.None;

		[NonSerialized]
		public WORK_VISIBLE_TYPE _workVisible_Rule = WORK_VISIBLE_TYPE.None;//규칙에 의한 것




		private const float VISIBLE_ALPHA = 0.01f;

		[NonSerialized]
		public int _debugID = -1;


		//Mod Lock 실행 여부를 저장한다.
		public enum EX_CALCULATE
		{
			//이전
			///// <summary>기본 상태</summary>
			//Normal,
			///// <summary>Ex Edit 상태 중 "선택된 Modifier"에 포함된 상태</summary>
			//ExAdded,
			///// <summary>Ex Edit 상태 중, "선택된 Modifier"에 포함되지 않은 상태</summary>
			//ExNotAdded

			//변경 21.2.14
			/// <summary>활성화되어 있다. (편집 모드 아닐때)</summary>
			Enabled_Run,
			/// <summary>편집 모드에서 활성 상태이다.</summary>
			Enabled_Edit,
			/// <summary>편집 모드에서 비활성 상태</summary>
			Disabled_NotEdit,
			/// <summary>
			/// 추가된 상태 : 편집 모드에서 선택되지 않았지만, 옵션에 의해 다른 모디파이어가 적용된다.
			/// </summary>
			Disabled_ExRun,

		}

		[NonSerialized]
		public EX_CALCULATE _exCalculateMode = EX_CALCULATE.Enabled_Run;


		//추가 11.30 : Extra 옵션
		[NonSerialized]
		public bool _isExtraDepthChanged = false;
		[NonSerialized]
		public int _extraDeltaDepth = 0;
		[NonSerialized]
		public bool _isExtraTextureChanged = false;
		[NonSerialized]
		public apTextureData _extraTextureData = null;

		public delegate void FUNC_EXTRA_DEPTH_CHANGED(apRenderUnit renderUnit, int deltaDepth);
		[NonSerialized]
		private FUNC_EXTRA_DEPTH_CHANGED _func_ExtraDepthChanged = null;



		public apMatrix3x3 WorldMatrix
		{
			get
			{
				if (_unitType == UNIT_TYPE.Mesh)
				{
					//return _meshTransform._matrix_TF_Cal_ToWorld;
					//return _meshTransform._matrix_TF_Cal_Parent * _meshTransform._matrix_TF_Cal_Local;
					return _meshTransform._matrix_TFResult_World.MtrxToSpace;
				}
				else
				{
					if (_meshGroupTransform == null)
					{ return apMatrix3x3.identity; }
					else
					{
						//return _meshGroupTransform._matrix_TF_Cal_Parent * _meshGroupTransform._matrix_TF_Cal_Local;
						return _meshGroupTransform._matrix_TFResult_World.MtrxToSpace;
					}
				}
			}
		}



		public apMatrix WorldMatrixWrap
		{
			get
			{
				if (_unitType == UNIT_TYPE.Mesh)
				{
					return _meshTransform._matrix_TFResult_World;
				}
				else
				{
					if (_meshGroupTransform == null)
					{ return null; }
					else
					{
						return _meshGroupTransform._matrix_TFResult_World;
					}
				}
			}
		}

		public apMatrix WorldMatrixWrapWithoutModified
		{
			get
			{
				if (_unitType == UNIT_TYPE.Mesh)
				{
					return _meshTransform._matrix_TFResult_WorldWithoutMod;
				}
				else
				{
					if (_meshGroupTransform == null)
					{ return null; }
					else
					{
						return _meshGroupTransform._matrix_TFResult_WorldWithoutMod;
					}
				}
			}
		}

		public string _tmpName = "";

		public string Name
		{
			get
			{
				if (_unitType == UNIT_TYPE.Mesh)
				{
					if (_meshTransform == null)
					{
						return "";
					}
					return _meshTransform._nickName;
				}
				else
				{
					if (_meshGroupTransform == null)
					{ return ""; }
					else
					{
						return _meshGroupTransform._nickName;
					}
				}

			}
		}

		public apPortrait.SHADER_TYPE ShaderType
		{
			get
			{
				if (_meshTransform != null)
				{
					return _meshTransform._shaderType;
				}
				return apPortrait.SHADER_TYPE.AlphaBlend;
			}
		}

		public bool IsClippedChildOrClippingParent
		{
			get
			{
				if (_meshTransform != null)
				{
					return _meshTransform._isClipping_Parent || _meshTransform._isClipping_Child;
				}
				return false;
			}
		}

		public bool IsClippedChild
		{
			get
			{
				if (_meshTransform != null)
				{
					return _meshTransform._isClipping_Child;
				}
				return false;
			}
		}

		public bool IsClippingParent
		{
			get
			{
				if (_meshTransform != null)
				{
					return _meshTransform._isClipping_Parent;
				}
				return false;
			}
		}

		// Init
		//---------------------------------------
		public apRenderUnit(apPortrait portrait, string nameKeyword)
		{
			_isSelectedInEditor = false;
			_exCalculateMode = EX_CALCULATE.Enabled_Run;
			_portrait = portrait;

			_tmpName = "RenderUnit_" + nameKeyword + "_" + UnityEngine.Random.Range(0, 1000);
			//Debug.LogError("New RenderUnit [" + _tmpName + "]");

			//CalculatedStack을 새로 만들자
			_calculatedStack = new apCalculatedResultStack(this);

			_debugID = UnityEngine.Random.Range(0, 1000);
		}


		


		public void SetGroup(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, apRenderUnit parentRenderUnit)
		{
			_unitType = UNIT_TYPE.GroupNode;

			_meshGroup = meshGroup;

			_meshGroupTransform = meshGroupTransform;
			_meshTransform = null;

			//역으로 Link를 하자
			_meshGroupTransform._linkedRenderUnit = this;


			//이전 코드 : 변수 이름만 바뀌었다.
			//_depth = 0;
			//if (_meshTransform != null)
			//{
			//	_depth += _meshTransform._depth;
			//}
			//if (parentRenderUnit != null)
			//{
			//	_depth += parentRenderUnit._depth;
			//}
			//if (meshGroupTransform != null)
			//{
			//	//루트가 아니라면 Mesh Group Transform도 있다.
			//	_depth += _meshGroupTransform._depth;
			//}

			_depthForSort = 0;
			if (_meshTransform != null)
			{
				_depthForSort += _meshTransform._depth;
			}
			if (parentRenderUnit != null)
			{
				_depthForSort += parentRenderUnit._depthForSort;
			}
			if (meshGroupTransform != null)
			{
				//루트가 아니라면 Mesh Group Transform도 있다.
				_depthForSort += _meshGroupTransform._depth;
			}

			if (parentRenderUnit != null)
			{
				parentRenderUnit._childRenderUnits.Add(this);
				_parentRenderUnit = parentRenderUnit;
			}
		}

		public void RefreshDepth()
		{
			//_depth = 0;
			//if (_meshTransform != null)
			//{
			//	_depth += _meshTransform._depth;
			//}
			//if (_parentRenderUnit != null)
			//{
			//	_depth += _parentRenderUnit._depth;
			//}
			//if (_meshGroupTransform != null)
			//{
			//	//루트가 아니라면 Mesh Group Transform도 있다.
			//	_depth += _meshGroupTransform._depth;
			//}

			_depthForSort = 0;
			if (_meshTransform != null)
			{
				_depthForSort += _meshTransform._depth;
			}
			if (_parentRenderUnit != null)
			{
				_depthForSort += _parentRenderUnit._depthForSort;
			}
			if (_meshGroupTransform != null)
			{
				//루트가 아니라면 Mesh Group Transform도 있다.
				_depthForSort += _meshGroupTransform._depth;
			}
		}

		public void SetMesh(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apRenderUnit parentRenderUnit)
		{
			_unitType = UNIT_TYPE.Mesh;

			_meshGroup = meshGroup;

			_meshGroupTransform = null;
			_meshTransform = meshTransform;

			_meshTransform._linkedRenderUnit = this;

			//_depth = 0;
			//if (parentRenderUnit != null)
			//{
			//	_depth += parentRenderUnit._depth;
			//}
			//if (_meshGroupTransform != null)
			//{
			//	_depth += _meshGroupTransform._depth;
			//}
			//_depth += meshTransform._depth;

			_depthForSort = 0;
			if (parentRenderUnit != null)
			{
				_depthForSort += parentRenderUnit._depthForSort;
			}
			if (_meshGroupTransform != null)
			{
				_depthForSort += _meshGroupTransform._depth;
			}
			_depthForSort += meshTransform._depth;


			if (parentRenderUnit != null)
			{
				parentRenderUnit._childRenderUnits.Add(this);
				_parentRenderUnit = parentRenderUnit;
			}
		}


		/// <summary>
		/// 추가 21.3.20 : 새로 생성하지 않고 재활용해서 RenderUnit을 구성하는 경우
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="meshGroupTransform"></param>
		/// <param name="parentRenderUnit"></param>
		public void ResetReuse()
		{
			_isSelectedInEditor = false;
			_exCalculateMode = EX_CALCULATE.Enabled_Run;
			
			//CalculatedStack을 새로 만들자 또는 리셋
			if(_calculatedStack == null)
			{
				_calculatedStack = new apCalculatedResultStack(this);
			}
			else
			{
				_calculatedStack.ClearResultParams();
				_calculatedStack.ResetRenderVerts();
			}

			_parentRenderUnit = null;
			if(_childRenderUnits == null)
			{
				_childRenderUnits = new List<apRenderUnit>();
			}
			else
			{
				_childRenderUnits.Clear();
			}
		}




		/// <summary>
		/// Mesh의 Vertex가 바뀌면 이 함수를 호출하자. Vertex Buffer를 다시 리셋할 수 있게 만든다.
		/// </summary>
		public void ResetVertexIndex()
		{
			_isRenderVertsInit = false;
		}

		/// <summary>
		/// 작업용 TmpWork Visible 변수를 초기화한다.
		/// </summary>
		public void ResetTmpWorkVisible(bool isResetWithRules)
		{
			//이전
			//_isVisibleWorkToggle_Hide2Show = false;//Hide 상태일때 작업을 위해 Show로 만든다.	
			//_isVisibleWorkToggle_Show2Hide = false;//Show 상태일때 작업을 위해 Hide로 만든다.

			//변경 21.1.28
			_workVisible_Tmp = WORK_VISIBLE_TYPE.None;

			if(isResetWithRules)
			{
				//Debug.LogError("RenderUnit : ResetTmpWorkVisible > Rule");
				_workVisible_Rule = WORK_VISIBLE_TYPE.None;
			}
		}


		// Calculate
		//---------------------------------------------------------------------
		public void ReadyToUpdate()
		{
			//if(_unitType != UNIT_TYPE.Mesh)
			//{
			//	return;
			//}

			_meshColor2X = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isVisible = true;
			_isVisibleCalculated = true;
			_isVisible_WithoutParent = true;

			//추가 11.30 : Extra Option
			_isExtraDepthChanged = false;
			_extraDeltaDepth = 0;
			_isExtraTextureChanged = false;
			_extraTextureData = null;


			if (_unitType == UNIT_TYPE.Mesh)
			{
				_meshTransform.ReadyToCalculate();

				if (_isRenderVertsInit && _renderVerts.Count > 0)
				{
					//return;
				}
				else
				{

					_renderVerts.Clear();

					if (_meshTransform != null)
					{

						if (_meshTransform._mesh != null)
						{
							List<apVertex> verts = _meshTransform._mesh._vertexData;
							for (int i = 0; i < verts.Count; i++)
							{
								_renderVerts.Add(new apRenderVertex(this, _meshGroup, _meshTransform._mesh, verts[i]));
							}

						}

						_isRenderVertsInit = true;
					}
				}
			}
			else
			{
				if (_meshGroupTransform != null)
				{
					_meshGroupTransform.ReadyToCalculate();
				}
			}

			if (_calculatedStack != null)
			{
				_calculatedStack.ReadyToCalculate();
			}


			if (_childRenderUnits.Count == 0)
			{
				return;
			}

			apRenderUnit childRenderUnit = null;
			for (int i = 0; i < _childRenderUnits.Count; i++)
			{
				childRenderUnit = _childRenderUnits[i];
				childRenderUnit.ReadyToUpdate();
			}
		}

		/// <summary>
		/// 현재 상태에 대한 Matrix 들을 계산하고 렌더링 준비를 한다.
		/// 이건 실시간으로 호출 될 수 있으므로 성능이 매우 중요하다.
		/// </summary>
		/// <param name="isMatrixCalculateForce">Matrix를 강제로 넣어야 한다. 초기화에 가까운 Force 옵션일때만 true 값을 넣자</param>
		//public void Update_Pre(float tDelta, bool isMatrixCalculateForce, apMeshGroup.FUNC_IS_FORCE_UPDATE funcForceUpdate)
		public void Update_Pre(float tDelta)
		{

			if (_calculatedStack != null)
			{
				//강제로 CalculateStack의 Hash를 업데이트해야하는지 결정한다.
				//bool isForceUpdateHash = isMatrixCalculateForce;
				//if(!isForceUpdateHash && funcForceUpdate != null)
				//{
				//	isForceUpdateHash = funcForceUpdate(this);
				//}

				//_calculatedStack.Calculate_Pre(tDelta, isForceUpdateHash);
				_calculatedStack.Calculate_Pre(tDelta);
			}


			apRenderUnit childRenderUnit = null;


			//_curMatrixToWorld를 처리한다.
			//1-1 Group Node일때
			if (_unitType == UNIT_TYPE.GroupNode)
			{
				if (_meshGroupTransform != null)
				{
					//_meshGroupTransform._matrix.MakeMatrix();

					//_matrixToParent = _meshGroupTransform._matrix.MtrxToSpace;

					//------------------------------------------------
					//[TODO] : Calculate Mesh Modifier를 넣어주자
					if (_calculatedStack.MeshWorldMatrixWrap != null)
					{
						_meshGroupTransform.SetModifiedTransform(_calculatedStack.MeshWorldMatrixWrap);
					}
					//------------------------------------------------


					//기존
					//Parent의 계산된 Matrix를 중첩해주자
					if (_parentRenderUnit != null)
					{
						if (_parentRenderUnit.WorldMatrixWrap != null)
						{
							//_meshGroupTransform.AddWorldMatrix_Parent(_parentRenderUnit.WorldMatrixOfNodeWrap);
							_meshGroupTransform.AddWorldMatrix_Parent(_parentRenderUnit.WorldMatrixWrap);
						}
					}

					#region [미사용 코드]
					//신규
					//Parent가 아닌 Child를 중첩해주자
					//if (_childRenderUnits.Count > 0)
					//{
					//	//apRenderUnit childRenderUnit = null;
					//	for (int i = 0; i < _childRenderUnits.Count; i++)
					//	{
					//		childRenderUnit = _childRenderUnits[i];
					//		if(childRenderUnit._unitType == UNIT_TYPE.GroupNode)
					//		{
					//			if(childRenderUnit._meshGroupTransform != null)
					//			{
					//				childRenderUnit._meshGroupTransform.AddWorldMatrix_Parent(WorldMatrixOfNodeWrap);
					//			}
					//		}
					//		else if(childRenderUnit._unitType == UNIT_TYPE.Mesh)
					//		{
					//			if(childRenderUnit._meshTransform != null)
					//			{
					//				childRenderUnit._meshTransform.AddWorldMatrix_Parent(WorldMatrixOfNodeWrap);
					//			}
					//		}
					//	}
					//} 
					#endregion


					_meshGroupTransform.MakeTransformMatrix();
				}
				else
				{
					//Root인 경우는 Mesh Group Transform이 없다.
					//Root의 Local이 곧 World 이므로
					//_matrixToParent = apMatrix3x3.identity;
					//Root는 움직임이 없다. 외부에서 컨트롤하시길
					////>>>> 아니다 이놈아!
					//if (_calculatedStack.MeshWorldMatrixWrap != null)
					//{
					//	_meshGroupTransform.SetModifiedTransform(_calculatedStack.MeshWorldMatrixWrap);
					//}

					//_meshGroupTransform.MakeTransformMatrix();

				}
			}
			//1-2 Mesh Node 일때
			else//if (_unitType == UNIT_TYPE.Mesh)
			{
				//--------------------------------------------------
				if (_calculatedStack.MeshWorldMatrixWrap != null)
				{
					//_meshTransform.SetModifiedTransform(_calculatedStack.MeshWorldMatrixWrap, _calculatedStack.CalculateLog_3_MeshTransform);
					_meshTransform.SetModifiedTransform(_calculatedStack.MeshWorldMatrixWrap);
				}


				//추가 20.8.10
				//리깅이 추가된 렌더 유닛의 MeshTF는 매트릭스 계산시 다른 계산을 해야한다.
				//부모의 "Modifer가 적용 안된 WorldMatrix"이 계산에 사용된다.
				//Opt에서는 Bake에 넣어서 상시 활용되도록 한다.
				if(_calculatedStack.IsRigging)
				{
					_meshTransform.SetRiggingApplied();
				}
				//--------------------------------------------------

				//기존
				//Parent의 계산된 Matrix를 중첩해주자
				if (_parentRenderUnit != null)
				{
					if (_parentRenderUnit.WorldMatrixWrap != null)
					{
						_meshTransform.AddWorldMatrix_Parent(_parentRenderUnit.WorldMatrixWrap, _parentRenderUnit.WorldMatrixWrapWithoutModified);
					}
				}

				_meshTransform.MakeTransformMatrix();
				//신규
				//부모 MeshGroupTransform의 RenderUnit이 알아서 해줄 것이다.
			}


			//색상도 만들어주자
			if (_calculatedStack.IsAnyColorCalculated)
			{
				_meshColor2X = _calculatedStack.MeshColor;
				_isVisible = _calculatedStack.IsMeshVisible;

			}
			else
			{
				//만약 계산된게 없다면 Default 값을 사용한다.
				if (_unitType == UNIT_TYPE.GroupNode)
				{
					if (_meshGroupTransform != null)
					{
						_meshColor2X = _meshGroupTransform._meshColor2X_Default;
						_isVisible = _meshGroupTransform._isVisible_Default;
					}
					else
					{
						_meshColor2X = _calculatedStack.MeshColor;
						_isVisible = _calculatedStack.IsMeshVisible;
					}
				}
				else
				{
					if (_meshTransform != null)
					{
						_meshColor2X = _meshTransform._meshColor2X_Default;
						_isVisible = _meshTransform._isVisible_Default;
					}
					else
					{
						_meshColor2X = _calculatedStack.MeshColor;
						_isVisible = _calculatedStack.IsMeshVisible;
					}
				}
			}

			//추가 11.30 : Extra Option
			if (_calculatedStack.IsExtraDepthChanged)
			{
				_isExtraDepthChanged = true;
				_extraDeltaDepth = _calculatedStack.ExtraDeltaDepth;

				//Debug.Log("Depth Event Occurred : " + Name);
				if(_func_ExtraDepthChanged != null)
				{
					_func_ExtraDepthChanged(this, _extraDeltaDepth);
				}
				//else
				//{
				//	//Debug.LogError("No Event");
				//}
			}
			if (_calculatedStack.IsExtraTextureChanged)
			{
				_isExtraTextureChanged = true;
				_extraTextureData = _calculatedStack.ExtraTextureData;
			}

			//추가 19.11.4 : Alpha값으로 일단 먼저 isVisible을 설정하자
			if (_meshColor2X.a < VISIBLE_ALPHA)
			{
				_isVisible = false;
				_meshColor2X.a = 0.0f;
			}


			//에디터 전용
			//작업용 임시 Visible을 적용하자
			_isVisibleCalculated = _isVisible;

			

			//이전 : Modifier > Tmp
			//if (_isVisibleCalculated && _isVisibleWorkToggle_Show2Hide)
			//{
			//	_isVisible = false;//<<강제로 Hide로 만든다.
			//	_meshColor2X.a = 0.0f;
			//}
			//else if (!_isVisibleCalculated && _isVisibleWorkToggle_Hide2Show)
			//{
			//	_isVisible = true;//<<강제로 Show로 만든다.
			//	_meshColor2X.a = 1.0f;//강제로 Alpha도 1로 만든다.
			//}

			//변경 21.1.28 : Modifier > Rule > Tmp (실제론 우선순위가 Tmp이므로, 거꾸로 조건을 체크해야한다.)
			if (_isVisibleCalculated)
			{
				//Hide로 강제할지 체크
				if(_workVisible_Tmp == WORK_VISIBLE_TYPE.ToHide
					|| (_workVisible_Tmp == WORK_VISIBLE_TYPE.None && _workVisible_Rule == WORK_VISIBLE_TYPE.ToHide))
				{
					//Tmp가 Hide이거나
					//Rule이 Hide이고, Tmp에서 처리하지 않을 때 강제로 Hide로 만든다.
					_isVisible = false;
					_meshColor2X.a = 0.0f;
				}
				
			}
			else if (!_isVisibleCalculated)
			{
				//Show로 강제할지 체크
				if(_workVisible_Tmp == WORK_VISIBLE_TYPE.ToShow
					|| (_workVisible_Tmp == WORK_VISIBLE_TYPE.None && _workVisible_Rule == WORK_VISIBLE_TYPE.ToShow))
				{
					//Tmp가 Show이거나
					//Rule이 Show이고, Tmp에서 처리하지 않을 때 강제로 Hide로 만든다.
					_isVisible = true;//<<강제로 Show로 만든다.
					_meshColor2X.a = 1.0f;//강제로 Alpha도 1로 만든다.
				}
			}





			//아직 Parent RenderUnit의 값을 
			_isVisible_WithoutParent = _isVisible;

			if (!_isVisible)
			{
				_meshColor2X.a = 0.0f;
			}

			if (_parentRenderUnit != null)
			{
				//2X 방식의 Add
				_meshColor2X.r = Mathf.Clamp01(((float)(_meshColor2X.r) - 0.5f) + ((float)(_parentRenderUnit._meshColor2X.r) - 0.5f) + 0.5f);
				_meshColor2X.g = Mathf.Clamp01(((float)(_meshColor2X.g) - 0.5f) + ((float)(_parentRenderUnit._meshColor2X.g) - 0.5f) + 0.5f);
				_meshColor2X.b = Mathf.Clamp01(((float)(_meshColor2X.b) - 0.5f) + ((float)(_parentRenderUnit._meshColor2X.b) - 0.5f) + 0.5f);
				_meshColor2X.a *= _parentRenderUnit._meshColor2X.a;
			}

			//Alpha가 너무 작다면 => 아예 렌더링을 하지 않도록 제어 / 또는 Visible이 아닐때
			if (_meshColor2X.a < VISIBLE_ALPHA
				//|| !_calculatedStack.IsMeshVisible
				)
			{
				_isVisible = false;
				_meshColor2X.a = 0.0f;
			}

			
			for (int i = 0; i < _childRenderUnits.Count; i++)
			{
				childRenderUnit = _childRenderUnits[i];

				//childRenderUnit.Update_Pre(tDelta, isMatrixCalculateForce, funcForceUpdate);
				childRenderUnit.Update_Pre(tDelta);
			}
		}


		/// <summary>
		/// 현재 상태에 대한 Matrix 들을 계산하고 렌더링 준비를 한다.
		/// 1차 업데이트 이후에 처리하는 Post 업데이트이다.
		/// Rigging, VertWorld 타입만 처리된다.
		/// </summary>
		/// <param name="isMatrixCalculateForce">Matrix를 강제로 넣어야 한다. 초기화에 가까운 Force 옵션일때만 true 값을 넣자</param>
		//public void Update_Post(float tDelta, bool isMatrixCalculateForce, apMeshGroup.FUNC_IS_FORCE_UPDATE funcForceUpdate)
		public void Update_Post(float tDelta)
		{

			if (_calculatedStack != null)
			{
				//강제로 CalculateStack의 Hash를 업데이트해야하는지 결정한다.
				//bool isForceUpdateHash = isMatrixCalculateForce;
				//if(!isForceUpdateHash && funcForceUpdate != null)
				//{
				//	isForceUpdateHash = funcForceUpdate(this);
				//}

				//_calculatedStack.Calculate_Post(tDelta, isForceUpdateHash);
				_calculatedStack.Calculate_Post(tDelta);
			}


			apRenderUnit childRenderUnit = null;


			for (int i = 0; i < _childRenderUnits.Count; i++)
			{
				childRenderUnit = _childRenderUnits[i];

				//childRenderUnit.Update_Post(tDelta, isMatrixCalculateForce, funcForceUpdate);
				childRenderUnit.Update_Post(tDelta);
			}
		}

		/// <summary>
		/// Update를 끝내고 호출해야하는 함수
		/// 갱신된 정보를 RenderVertex 정보로 넣어준다.
		/// Child RenderUnit에 자동으로 호출한다.
		/// </summary>
		/// <param name="isUpdateAlways">Vertex 작업을 하는 상태에서는 True를 넣는다. 재생 전용이고 Gizmo가 안뜨면 False를 넣어주자</param>
		/// <param name="isMatrixCalculateForce">Matrix를 강제로 넣어야 한다. Force 옵션 또는 tDelta > bias일때 true 값을 넣자</param>
		//public void UpdateToRenderVert(float tDelta, bool isUpdateAlways, bool isMatrixCalculateForce, apMeshGroup.FUNC_IS_FORCE_UPDATE funcForceUpdate)
		//public void UpdateToRenderVert(float tDelta, bool isUpdateAlways, bool isMatrixCalculateForce, apMeshGroup.FUNC_IS_FORCE_UPDATE funcForceUpdate)
		public void UpdateToRenderVert(float tDelta, bool isUpdateAlways)
		{
			//강제로 업데이트해야하는지 결정한다.

			//Profiler.BeginSample("1. Func Check");
			//if(!isMatrixCalculateForce && funcForceUpdate != null)
			//{
			//	isMatrixCalculateForce = funcForceUpdate(this);
			//}
			//Profiler.EndSample();

			//Child까지 계산한 이후 Vertex를 계산해줘야 한다.
			if (_unitType == UNIT_TYPE.Mesh && (isUpdateAlways || _isVisible))
			{
				bool isRigging = _calculatedStack.IsRigging;
				bool isVertexLocal = _calculatedStack.IsVertexLocal;
				bool isVertexWorld = _calculatedStack.IsVertexWorld;

				apRenderVertex rVert = null;


				for (int i = 0; i < _renderVerts.Count; i++)
				{
					//행렬 디버깅
					//if(i == 0 && _meshTransform != null && _meshTransform._nickName.Contains("Reversed"))
					//{
					//	Debug.LogWarning("-------- Reversed Mesh 디버깅 --------");
					//	if(isRigging)
					//	{
					//		Debug.Log("> 리깅 DeltaPos : " + _calculatedStack.GetVertexRigging(i).ToString());
					//	}
					//	Debug.Log("> Mesh World Matrix : \n" + WorldMatrix.ToString());
						
					//}


					rVert = _renderVerts[i];

					rVert.ReadyToCalculate();

					//단계별로 처리하자

					//1) Pivot 위치 적용
					rVert.SetMatrix_1_Static_Vert2Mesh(_meshTransform._mesh.Matrix_VertToLocal);

					if (isRigging)
					{
						rVert.SetRigging_0_LocalPosWeight(_calculatedStack.GetVertexRigging(i), _calculatedStack.GetRiggingWeight(), _calculatedStack.GetMatrixRigging(i));
					}

					if (isVertexLocal)
					{
						//Calculate - Vertex Local Morph (Vec2)
						rVert.SetMatrix_2_Calculate_VertLocal(_calculatedStack.GetVertexLocalPos(i));
					}

					rVert.SetMatrix_3_Transform_Mesh(WorldMatrix);

					if (isVertexWorld)
					{
						//Calculate - Vertex World Morph (Vec2)
						rVert.SetMatrix_4_Calculate_VertWorld(_calculatedStack.GetVertexWorldPos(i));
					}

					rVert.Calculate(tDelta);
				}
			}

			if (_childRenderUnits.Count > 0)
			{
				apRenderUnit childRenderUnit = null;
				for (int i = 0; i < _childRenderUnits.Count; i++)
				{
					childRenderUnit = _childRenderUnits[i];

					//childRenderUnit.UpdateToRenderVert(tDelta, isUpdateAlways, isMatrixCalculateForce, funcForceUpdate);
					childRenderUnit.UpdateToRenderVert(tDelta, isUpdateAlways);
				}
			}
		}



		//추가
		/// <summary>
		/// Modifier가 적용되지 않은 Render Vert의 World Position이 필요할때 이 함수를 호출하자.
		/// 결과값은 각 RenderVert의 _pos_World_NoMod에 저장된다.
		/// </summary>
		public void CalculateWorldPositionWithoutModifier()
		{
			//강제로 업데이트해야하는지 결정한다.
			//Child까지 계산한 이후 Vertex를 계산해줘야 한다.
			if (_unitType == UNIT_TYPE.Mesh)
			{
				apRenderVertex rVert = null;
				for (int i = 0; i < _renderVerts.Count; i++)
				{
					rVert = _renderVerts[i];

					rVert.CalculateNotModified(_meshTransform._mesh.Matrix_VertToLocal, WorldMatrixWrapWithoutModified.MtrxToSpace);
				}
			}

			//이건 필요한 RenderUnit만 따로 호출하므로 굳이 Child도 호출할 필요가 없다.
			//if (_childRenderUnits.Count > 0)
			//{
			//	apRenderUnit childRenderUnit = null;
			//	for (int i = 0; i < _childRenderUnits.Count; i++)
			//	{
			//		childRenderUnit = _childRenderUnits[i];

			//		childRenderUnit.CalculateWorldPositionWithoutModifier();
			//	}
			//}
		}



		public int GetDepth()
		{
			if (_unitType == UNIT_TYPE.Mesh)
			{
				if (_meshTransform != null)
				{
					return _meshTransform._depth;
				}
			}
			else
			{
				if (_meshGroupTransform != null)
				{
					return _meshGroupTransform._depth;
				}
			}
			return 0;
		}

		/// <summary>
		/// Sort 용도로만 사용되는 Get 함수.
		/// 일단 Sort 이후에는 이 함수 대신 GetDepth를 사용하자
		/// </summary>
		/// <returns></returns>
		public int DepthForOnlySort
		{
			get { return _depthForSort; }
		}

		public int SetDepth(int depth)
		{
			if (_unitType == UNIT_TYPE.Mesh)
			{
				if (_meshTransform != null)
				{
					_meshTransform._depth = depth;
				}
			}
			else
			{
				if (_meshGroupTransform != null)
				{
					_meshGroupTransform._depth = depth;
				}
			}
			//if(_childRenderUnits != null && _childRenderUnits.Count > 0)
			//{
			//	//int curDepth = depth + 1;
			//	int curDepth = 0;//Child는 Offset 형식으로 저장된다.

			//	_childRenderUnits.Sort(delegate (apRenderUnit a, apRenderUnit b)
			//	{
			//		return a._depth - b._depth;
			//	});

			//	for (int i = 0; i < _childRenderUnits.Count; i++)
			//	{
			//		curDepth++;
			//		apRenderUnit childRenderUnit = _childRenderUnits[i];
			//		curDepth = childRenderUnit.SetDepth(curDepth);
			//	}

			//	return curDepth + depth;
			//}
			//else
			//{
			//	return depth;
			//}
			return depth;
		}

		public void SetDepthForSort(int depthForSort)
		{
			_depthForSort = depthForSort;
		}

		public int GetLastDepth()
		{
			//int curDepth = _depth;
			int curDepth = GetDepth();
			if (_childRenderUnits != null && _childRenderUnits.Count > 0)
			{
				int maxDepth = 0;
				for (int i = 0; i < _childRenderUnits.Count; i++)
				{
					//if (maxDepth < _childRenderUnits[i]._depth)
					//{
					//	maxDepth = _childRenderUnits[i]._depth;
					//}

					//Sort 이후에는 GetDepth로 변경
					if (maxDepth < _childRenderUnits[i].GetDepth())
					{
						maxDepth = _childRenderUnits[i].GetDepth();
					}
				}

				curDepth += maxDepth;
			}

			return curDepth;
		}


		//추가 12.2 : Extra Option에 의해 Depth가 바뀐 경우 예외적으로 해당 MeshGroup에 호출해야한다.
		//이 MeshGroup은 멤버와 다른 선택된 MeshGroup을 대상으로 한다.
		public void SetExtraDepthChangedEvent(FUNC_EXTRA_DEPTH_CHANGED func_ExtraDepthChanged)
		{
			_func_ExtraDepthChanged = func_ExtraDepthChanged;
		}


		//추가 12.4 : 변경된 텍스쳐에 대한 정보
		public bool IsExtraTextureChanged
		{
			get
			{
				return _isExtraTextureChanged && _extraTextureData != null;
			}
		}
		public apTextureData ChangedExtraTextureData
		{
			get
			{
				return _extraTextureData;
			}
		}
		
				
	}

}