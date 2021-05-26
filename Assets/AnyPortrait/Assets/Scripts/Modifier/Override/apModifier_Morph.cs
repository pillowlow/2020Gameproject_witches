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
	public class apModifier_Morph : apModifierBase
	{
		// Members
		//----------------------------------------------
		//[NonSerialized]
		//private MODIFIER_TYPE[] _generalExEditableModType = new MODIFIER_TYPE[] {   MODIFIER_TYPE.Morph,
		//																		MODIFIER_TYPE.Rigging,
		//																		MODIFIER_TYPE.TF };

		// Init
		//----------------------------------------------
		//public apModifier_Morph() : base()
		//{
		//}

		//public override void Init()
		//{
		//	base.Init();
		//}


		public override void SetInitSetting(int uniqueID, int layer, int meshGroupID, apMeshGroup meshGroup)
		{
			base.SetInitSetting(uniqueID, layer, meshGroupID, meshGroup);
		}



		//public override void RefreshParamSet()
		//{
		//	base.RefreshParamSet();

		//	//ModifiedMesh를 ParamSet에 추가해준다면 CalculatedSet에 자동으로 추가된다.

		//	////테스트로 쓸 MeshTransform 하나를 가져오자
		//	//if (_meshGroup._childMeshTransforms.Count > 0)
		//	//{
		//	//	apTransform_Mesh testMeshTransform = _meshGroup._childMeshTransforms[0];
		//	//	if (testMeshTransform._mesh != null)
		//	//	{
		//	//		bool isNewAddedModMesh = false;

		//	//		// 테스트 코드
		//	//		//파라미터 셋을 돌며, ModMesh가 없는 경우 하나씩 추가해주자
		//	//		for (int i = 0; i < _paramSetList.Count; i++)
		//	//		{
		//	//			if (_paramSetList[i]._meshData.Count == 0)
		//	//			{
		//	//				apModifiedMesh modMesh = new apModifiedMesh();
		//	//				modMesh.Init_VertexMorph(
		//	//					_meshGroup._uniqueID,
		//	//					testMeshTransform._transformUniqueID,
		//	//					testMeshTransform._mesh._uniqueID);

		//	//				modMesh.Link_VertexMorph(_meshGroup, testMeshTransform, _meshGroup.GetRenderUnit(testMeshTransform));
		//	//				_paramSetList[i]._meshData.Add(modMesh);

		//	//				isNewAddedModMesh = true;
		//	//			}
		//	//		}

		//	//		//Calculated 리스트를 갱신해주자
		//	//		if (isNewAddedModMesh)
		//	//		{
		//	//			_meshGroup.RefreshModifierLink();
		//	//		}
		//	//	}
		//	//}
		//}

		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Morph; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.Controller; }
		}

		private const string NAME_MORPH_LONG = "Morph (Controller)";
		private const string NAME_MORPH_SHORT = "Morph (Ctrl)";

		public override string DisplayName
		{
			//get { return "Morph (Controller)"; }
			get { return NAME_MORPH_LONG; }
		}

		public override string DisplayNameShort
		{
			//get { return "Morph (Ctrl)"; }
			get { return NAME_MORPH_SHORT; }
		}
		/// <summary>
		/// Calculate 계산시 어느 단계에서 적용되는가
		/// </summary>
		public override apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get
			{
				return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos |
				   apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color;
			}
		}

		public override apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Object; }
		}

		public override apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get
			{
				return apModifiedMesh.MOD_VALUE_TYPE.VertexPosList |
						apModifiedMesh.MOD_VALUE_TYPE.Color;
			}
		}


		// MeshTransform만 적용
		public override bool IsTarget_MeshTransform { get { return true; } }
		public override bool IsTarget_MeshGroupTransform { get { return false; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return true; } }

		public override bool IsUseParamSetWeight { get { return true; } }//ParamSet 자체의 OverlapWeight를 사용한다.

		//추가
		public override bool IsPhysics { get { return false; } }
		public override bool IsVolume { get { return false; } }

		//[NonSerialized]
		//private int _prevOutputParams = -1;



		///// <summary>
		///// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		///// </summary>
		///// <returns></returns>
		//public override MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		//{
		//	return _generalExEditableModType;
		//}


		//[NonSerialized]
		//private bool _isPrevSelectedMatched = false;

		// Functions
		//----------------------------------------------
		public override void InitCalculate(float tDelta)
		{
			base.InitCalculate(tDelta);

			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			apCalculatedResultParam calParam = null;
			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];
				calParam.InitCalculate();
				calParam._isAvailable = false;

				//calParam._isAvailable = true;

				//List<Vector2> posList = calParam._result_Positions;
				//int nParamKeys = calParam._paramKeyValues.Count;
				//for (int iPos = 0; iPos < posList.Count; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;


				//}
			}

		}

		//private float _lastDebug = 0.0f;

		public override void Calculate(float tDelta)
		{
			base.Calculate(tDelta);

			CalculatePattern_Morph(tDelta);

			#region [미사용 코드] -> 공통 함수로 바꾸었다.
			//if (_calculatedResultParams.Count == 0)
			//{			
			//	return;
			//}


			//apCalculatedResultParam calParam = null;

			////bool isRenderOutput = false;
			//if (_prevOutputParams != _calculatedResultParams.Count)
			//{
			//	_prevOutputParams = _calculatedResultParams.Count;

			//}

			////bool isDebuggable = false;
			////_lastDebug += tDelta;
			////if(_lastDebug > 2.0f)
			////{
			////	_lastDebug = 0.0f;
			////	isDebuggable = true;
			////}

			////if(isDebuggable)
			////{
			////	Debug.Log("Morph : Params " + _calculatedResultParams.Count);
			////}

			//for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			//{
			//	calParam = _calculatedResultParams[iCalParam];

			//	//Sub List를 돌면서 Weight 체크

			//	// 중요!
			//	//-------------------------------------------------------
			//	//1. Param Weight Calculate
			//	calParam.Calculate();
			//	//-------------------------------------------------------

			//	//if (isDebuggable)
			//	//{
			//	//	if (calParam._targetRenderUnit != null)
			//	//	{
			//	//		Debug.Log("[" + iCalParam + "] Target Render Unit : " + calParam._targetRenderUnit._tmpName);
			//	//	}
			//	//}

			//	List<Vector2> posList = calParam._result_Positions;
			//	List<Vector2> tmpPosList = calParam._tmp_Positions;
			//	List<apCalculatedResultParamSubList> subParamGroupList = calParam._subParamKeyValueList;
			//	List<apCalculatedResultParam.ParamKeyValueSet> subParamKeyValueList = null;
			//	float layerWeight = 0.0f;
			//	apModifierParamSetGroup keyParamSetGroup = null;

			//	apModifierParamSetGroupVertWeight weigetedVertData = calParam._weightedVertexData;

			//	//일단 초기화
			//	for (int iPos = 0; iPos < posList.Count; iPos++)
			//	{
			//		posList[iPos] = Vector2.zero;
			//	}

			//	//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
			//	//레이어간 병합 과정에 신경 쓸것
			//	for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
			//	{
			//		apCalculatedResultParamSubList curSubList = subParamGroupList[iSubList];

			//		//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
			//		int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
			//		subParamKeyValueList = curSubList._subParamKeyValues;

			//		apCalculatedResultParam.ParamKeyValueSet paramKeyValue = null;

			//		keyParamSetGroup = curSubList._keyParamSetGroup;


			//		//Vector2 calculatedValue = Vector2.zero;

			//		bool isFirstParam = true;

			//		//레이어 내부의 임시 데이터를 먼저 초기화
			//		for (int iPos = 0; iPos < posList.Count; iPos++)
			//		{
			//			tmpPosList[iPos] = Vector2.zero;
			//		}


			//		//if (isDebuggable)
			//		//{
			//		//	Debug.Log("--------------------------------------------");
			//		//	Debug.Log("[" + iCalParam + "] Params : " + nParamKeys);
			//		//	Debug.Log("--------------------------------------------");
			//		//}

			//		//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
			//		//Dist에 따른 ParamWeight를 가중치로 적용한다.
			//		for (int iPV = 0; iPV < nParamKeys; iPV++)
			//		{
			//			//paramKeyValue = calParam._paramKeyValues[iPV];
			//			paramKeyValue = subParamKeyValueList[iPV];
			//			//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

			//			if (!paramKeyValue._isCalculated) { continue; }


			//			//---------------------------- Pos List
			//			for (int iPos = 0; iPos < posList.Count; iPos++)
			//			{
			//				//posList[iPos] = Vector2.zero;
			//				//calculatedValue = paramKeyValue._modfiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;

			//				#region [미사용 코드] 잘못된 계산법
			//				//if (!isFirstParam)
			//				//{
			//				//	//맨 처음 Layer가 아닐때
			//				//	//Vertex별 부분 Weight도 적용한다.
			//				//	layerWeight *= paramKeyValue._modfiedValue._vertices[iPos]._overlapWeight;
			//				//}

			//				////계산 방식에 따라 처리된다.
			//				//switch (paramKeyValue._keyParamSetGroup._blendMethod)
			//				//{
			//				//	case apModifierParamSetGroup.BLEND_METHOD.Additive:
			//				//		{
			//				//			//Layer Weight에 맞게 더한다.
			//				//			posList[iPos] += calculatedValue * layerWeight;
			//				//		}
			//				//		break;

			//				//	case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
			//				//		{
			//				//			//Layer Weight에 맞게 보간한다.
			//				//			posList[iPos] = (posList[iPos] * (1.0f - layerWeight))
			//				//							+ (calculatedValue * layerWeight);
			//				//		}
			//				//		break;
			//				//}

			//				////이전 코드 
			//				#endregion
			//				//posList[iPos] += paramKeyValue._modfiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;//기존의 Add Only

			//				tmpPosList[iPos] += paramKeyValue._modifiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;
			//			}
			//			//---------------------------- Pos List

			//			if (isFirstParam)
			//			{
			//				isFirstParam = false;
			//			}

			//		}//--- Params

			//		//이제 tmp값을 Result에 넘겨주자
			//		//처음 Layer라면 -> 100% 적용
			//		//그렇지 않다면 Blend를 해주자
			//		if (keyParamSetGroup._layerIndex == 0)
			//		{
			//			for (int iPos = 0; iPos < posList.Count; iPos++)
			//			{
			//				posList[iPos] = tmpPosList[iPos];
			//			}
			//		}
			//		else
			//		{
			//			layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);

			//			switch (keyParamSetGroup._blendMethod)
			//			{
			//				case apModifierParamSetGroup.BLEND_METHOD.Additive:
			//					{
			//						if (weigetedVertData != null)
			//						{
			//							//Vertex 가중치가 추가되었다.
			//							float vertWeight = 0.0f;
			//							for (int iPos = 0; iPos < posList.Count; iPos++)
			//							{
			//								vertWeight = layerWeight * weigetedVertData._weightedVerts[iPos]._adaptWeight;

			//								posList[iPos] += tmpPosList[iPos] * vertWeight;
			//							}
			//						}
			//						else
			//						{
			//							for (int iPos = 0; iPos < posList.Count; iPos++)
			//							{
			//								posList[iPos] += tmpPosList[iPos] * layerWeight;
			//							}
			//						}
			//					}
			//					break;

			//				case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
			//					{
			//						if (weigetedVertData != null)
			//						{
			//							//Vertex 가중치가 추가되었다.
			//							float vertWeight = 0.0f;
			//							for (int iPos = 0; iPos < posList.Count; iPos++)
			//							{
			//								vertWeight = layerWeight * weigetedVertData._weightedVerts[iPos]._adaptWeight;

			//								posList[iPos] = (posList[iPos] * (1.0f - vertWeight)) +
			//												(tmpPosList[iPos] * vertWeight);
			//							}
			//						}
			//						else
			//						{
			//							for (int iPos = 0; iPos < posList.Count; iPos++)
			//							{
			//								posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
			//												(tmpPosList[iPos] * layerWeight);
			//							}
			//						}
			//					}
			//					break;

			//				default:
			//					Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
			//					break;
			//			}
			//		}

			//		//if (isDebuggable)
			//		//{
			//		//	Debug.Log("--------------------------------------------");
			//		//}

			//	}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
			//	calParam._isAvailable = true;

			//	#region [미사용 코드]
			//	//테스트 코드

			//	//_tMove += tDelta;
			//	//if(_tMove > _tMoveLength)
			//	//{
			//	//	_tMove -= _tMoveLength;
			//	//}

			//	//float pos1 = Mathf.Sin((_tMove / _tMoveLength) * Mathf.PI * 2.0f) * 10.0f;
			//	//float pos2 = Mathf.Cos((_tMove / _tMoveLength) * Mathf.PI * 2.0f) * 5.0f;

			//	//apCalculatedResultParam calParam = null;
			//	//for (int i = 0; i < _calculatedResultParams.Count; i++)
			//	//{
			//	//	calParam = _calculatedResultParams[i];

			//	//	calParam._isAvailable = true;
			//	//	List<Vector2> posList = calParam._result_Positions;
			//	//	for (int iPos = 0; iPos < posList.Count; iPos++)
			//	//	{
			//	//		if(iPos % 2 == 0)
			//	//		{
			//	//			posList[iPos] = new Vector2(pos1, pos2);
			//	//		}
			//	//		else
			//	//		{
			//	//			posList[iPos] = new Vector2(pos2, pos1);
			//	//		}
			//	//	}
			//	//}
			//	//TODO
			//	//ParamSet을 계산한 후
			//	//Dictionay에 [Vertex / WorldMatrix] 를 만들어 넣는다.
			//	//
			//	//_calculateResult.ReadyToCalculate();
			//	//..오버라이드! 
			//	#endregion
			//} 
			#endregion


		}
	}

}