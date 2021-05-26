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
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{

	//GizmoController -> Modifier [Vertex를 선택하는 타입]에 대한 내용이 담겨있다.
	public partial class apGizmoController
	{
		// 작성해야하는 함수
		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
		// Rotate : void - (float deltaAngleW)
		// Scale : void - (Vector2 deltaScaleW)

		//	TODO : 현재 Transform이 가능한지도 알아야 할 것 같다.
		// Transform Position : void - (Vector2 pos, int depth)
		// Transform Rotation : void - (float angle)
		// Transform Scale : void - (Vector2 scale)
		// Transform Color : void - (Color color)

		// Pivot Return : apGizmos.TransformParam - ()

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		// Vertex 전용 툴
		// SoftSelection() : bool
		// PressBlur(Vector2 pos, float tDelta) : bool



		//----------------------------------------------------------------
		// Gizmo - MeshGroup : Modifier / Morph계열 및 Vertex를 선택하는 Weight 계열의 모디파이어
		//----------------------------------------------------------------
		/// <summary>
		/// Modifier [Morph]에 대한 Gizmo Event의 Set이다.
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Morph()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Modifier_Vertex,
														Unselect__Modifier_Vertex, 
														Move__Modifier_VertexPos, 
														Rotate__Modifier_VertexPos, 
														Scale__Modifier_VertexPos, 
														PivotReturn__Modifier_Vertex);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__Modifier_VertexPos,
																null,
																null,
																null,
																TransformChanged_Color__Modifier_Vertex,
																TransformChanged_Extra__Modifier_Vertex,
																apGizmos.TRANSFORM_UI.Position2D 
																	| apGizmos.TRANSFORM_UI.Vertex_Transform 
																	| apGizmos.TRANSFORM_UI.Color
																	| apGizmos.TRANSFORM_UI.Extra
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__Modifier_Vertex, 
														FFDTransform__Modifier_VertexPos, 
														StartFFDTransform__Modifier_VertexPos, 
														SoftSelection__Modifier_VertexPos, 
														SyncBlurStatus__Modifier_VertexPos, 
														PressBlur__Modifier_VertexPos);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Modifier_Vertex, 
																AddHotKeys__Modifier_Vertex, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Move, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Rotate, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(
			//	Select__Modifier_Vertex,
			//	Unselect__Modifier_Vertex,
			//	Move__Modifier_VertexPos,
			//	Rotate__Modifier_VertexPos,
			//	Scale__Modifier_VertexPos,
			//	TransformChanged_Position__Modifier_VertexPos,
			//	null,
			//	null,
			//	null,
			//	TransformChanged_Color__Modifier_Vertex,
			//	TransformChanged_Extra__Modifier_Vertex,
			//	PivotReturn__Modifier_Vertex,
			//	MultipleSelect__Modifier_Vertex,
			//	FFDTransform__Modifier_VertexPos,
			//	StartFFDTransform__Modifier_VertexPos,
			//	SoftSelection__Modifier_VertexPos,
			//	SyncBlurStatus__Modifier_VertexPos,
			//	PressBlur__Modifier_VertexPos,
			//	apGizmos.TRANSFORM_UI.Position2D 
			//	| apGizmos.TRANSFORM_UI.Vertex_Transform 
			//	| apGizmos.TRANSFORM_UI.Color
			//	| apGizmos.TRANSFORM_UI.Extra,
			//	FirstLink__Modifier_Vertex,
			//	AddHotKeys__Modifier_Vertex);
		}




		public apGizmos.SelectResult FirstLink__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVerts_All != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}
			return null;
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Vertex(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			//(Editing 상태일 때)
			//1. Vertex 선택
			//2. (Lock 걸리지 않았다면) 다른 Transform을 선택

			//(Editing 상태가 아닐 때)
			//(Lock 걸리지 않았다면) Transform을 선택한다.
			// Child 선택이 가능하면 MeshTransform을 선택. 그렇지 않으면 MeshGroupTransform을 선택해준다.

			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}

			int prevSelectedCount = Editor.Select.ModRenderVerts_All.Count;

			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				//return prevSelectedCount;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			//추가 20.5.27 : 다중 선택
			//Vertex Mod는 다중 선택이 가능하다.
			apSelection.MULTI_SELECT multiSelect = (selectType == apGizmos.SELECT_TYPE.Add) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main;



			bool isTransformSelectable = false;


			#region [미사용 코드] 단일 선택과 MRV 구버전 처리

			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			//{
			//	//(Editing 상태일 때)
			//	//1. Vertex 선택
			//	//2. (Lock 걸리지 않았다면) 다른 Transform을 선택
			//	bool selectVertex = false;
			//	//if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			//	if (Editor.Select.ModMesh_Main != null && Editor.Select.MeshGroup != null)//변경 20.6.18
			//	{
			//		//일단 선택한 Vertex가 클릭 가능한지 체크
			//		if (Editor.Select.ModRenderVertOfMod != null)
			//		{
			//			if (Editor.Select.ModRenderVertListOfMod.Count == 1)
			//			{
			//				if (Editor.Controller.IsVertexClickable(apGL.World2GL(Editor.Select.ModRenderVertOfMod._renderVert._pos_World), mousePosGL))
			//				{
			//					if (selectType == apGizmos.SELECT_TYPE.Subtract)
			//					{
			//						//삭제인 경우
			//						Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVertOfMod._modVert, null, null, Editor.Select.ModRenderVertOfMod._renderVert);

			//						//return apGizmos.SELECT_RESULT.None;
			//						if(Editor.Select.ModRenderVertListOfMod.Count > 0)
			//						{
			//							selectVertex = true;
			//						}
			//					}
			//					else
			//					{
			//						//그 외에는 => 그대로 갑시다.
			//						selectVertex = true;
			//						//return apGizmos.SELECT_RESULT.SameSelected;
			//					}
			//					//return Editor.Select.ModRenderVertListOfMod.Count;
			//					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			//				}
			//			}
			//			else
			//			{
			//				//여러개라고 하네요.
			//				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
			//				for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
			//				{
			//					apSelection.ModRenderVert modRenderVert = modRenderVerts[iModRenderVert];

			//					if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRenderVert._renderVert._pos_World), mousePosGL))
			//					{
			//						if (selectType == apGizmos.SELECT_TYPE.Subtract)
			//						{
			//							//삭제인 경우
			//							//하나 지우고 끝
			//							//결과는 List의 개수
			//							Editor.Select.RemoveModVertexOfModifier(modRenderVert._modVert, null, null, modRenderVert._renderVert);
			//							//return apGizmos.SELECT_RESULT.None;
			//							//return Editor.Select.ModRenderVertOfModList.Count;
			//							//if(Editor.Select.ModRenderVertOfModList.Count == 0)
			//							//{
			//							//	return apGizmos.SELECT_RESULT.None;
			//							//}
			//							//else
			//							//{
			//							//	return apGizmos.SELECT_RESULT.NewSelected;
			//							//}

			//							if(Editor.Select.ModRenderVertListOfMod.Count > 0)
			//							{
			//								selectVertex = true;
			//							}
			//						}
			//						else if (selectType == apGizmos.SELECT_TYPE.Add)
			//						{
			//							//Add 상태에서 원래 선택된걸 누른다면
			//							//추가인 경우 => 그대로
			//							selectVertex = true;
			//							//return apGizmos.SELECT_RESULT.SameSelected;
			//							//return Editor.Select.ModRenderVertOfModList.Count;
			//						}
			//						else
			//						{
			//							//만약... new 라면?
			//							//다른건 초기화하고
			//							//얘만 선택해야함
			//							apRenderVertex selectedRenderVert = modRenderVert._renderVert;
			//							apModifiedVertex selectedModVert = modRenderVert._modVert;
			//							Editor.Select.SetModVertexOfModifier(null, null, null, null);
			//							Editor.Select.SetModVertexOfModifier(selectedModVert, null, null, selectedRenderVert);
			//							//return apGizmos.SELECT_RESULT.NewSelected;
			//							//return Editor.Select.ModRenderVertOfModList.Count;
			//						}

			//						//return Editor.Select.ModRenderVertListOfMod.Count;
			//						return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			//					}
			//				}
			//			}

			//		}

			//		if (selectType == apGizmos.SELECT_TYPE.New)
			//		{
			//			//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
			//			Editor.Select.SetModVertexOfModifier(null, null, null, null);
			//		}

			//		if (selectType != apGizmos.SELECT_TYPE.Subtract)
			//		{
			//			//if (Editor.Select.ExKey_ModMesh._transform_Mesh != null && Editor.Select.ExKey_ModMesh._vertices != null)
			//			if (Editor.Select.ModMesh_Main._transform_Mesh != null && Editor.Select.ModMesh_Main._vertices != null)//변경 20.6.18
			//			{
			//				//선택된 RenderUnit을 고르자
			//				//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);//이전

			//				//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//변경 1 (20.6.18)
			//				apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main; //변경 2 (20.6.18) 1과 2 중에서 맞는걸 사용하자

			//				if (targetRenderUnit != null)
			//				{
			//					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//					{
			//						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
			//						bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(renderVert._pos_World), mousePosGL);
			//						if (isClick)
			//						{
			//							//apModifiedVertex selectedModVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
			//							apModifiedVertex selectedModVert = Editor.Select.ModMesh_Main._vertices.Find(delegate (apModifiedVertex a)
			//							{
			//								return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//							});

			//							if (selectedModVert != null)
			//							{
			//								if (selectType == apGizmos.SELECT_TYPE.New)
			//								{
			//									Editor.Select.SetModVertexOfModifier(selectedModVert, null, null, renderVert);
			//								}
			//								else if (selectType == apGizmos.SELECT_TYPE.Add)
			//								{
			//									Editor.Select.AddModVertexOfModifier(selectedModVert, null, null, renderVert);
			//								}

			//								selectVertex = true;
			//								//result = apGizmos.SELECT_RESULT.NewSelected;
			//								break;
			//							}

			//						}
			//					}
			//				}
			//			}
			//		}
			//	}

			//	//Vertex를 선택한게 없다면
			//	//+ Lock 상태가 아니라면
			//	if (!selectVertex && !Editor.Select.IsSelectionLock)
			//	{
			//		//Transform을 선택
			//		isTransformSelectable = true;
			//	}
			//}
			//else
			//{
			//	//(Editing 상태가 아닐때)
			//	isTransformSelectable = true;

			//	if (
			//		//Editor.Select.ExKey_ModMesh != null 
			//		Editor.Select.ModMesh_Main != null //변경 20.6.18
			//		&& Editor.Select.IsSelectionLock)
			//	{
			//		//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
			//		isTransformSelectable = false;
			//	}
			//} 
			#endregion


			//변경 20.6.25 : MRV 신버전 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			{
				//(Editing 상태일 때)
				//1. Vertex 선택
				//2. (Lock 걸리지 않았다면) 다른 Transform을 선택
				bool selectVertex = false;
				
				
				
				if (Editor.Select.ModMesh_Main != null && 
					Editor.Select.ModMeshes_All != null &&//<<다중 선택된 ModMesh도 체크한다.
					Editor.Select.MeshGroup != null)
				{
					//일단 선택한 Vertex가 클릭 가능한지 체크
					if (Editor.Select.ModRenderVert_Main != null)
					{
						if (Editor.Select.ModRenderVerts_All.Count == 1)
						{
							if (Editor.Controller.IsVertexClickable(apGL.World2GL(Editor.Select.ModRenderVert_Main._renderVert._pos_World), mousePosGL))
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									//삭제인 경우
									//이전
									//Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVertOfMod._modVert, null, null, Editor.Select.ModRenderVertOfMod._renderVert);

									//변경 20.6.25
									Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVert_Main);


									
									//return apGizmos.SELECT_RESULT.None;
									if(Editor.Select.ModRenderVerts_All.Count > 0)
									{
										selectVertex = true;
									}
								}
								else
								{
									//그 외에는 => 그대로 갑시다.
									selectVertex = true;
								}
								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
							}
						}
						else
						{
							//여러개라고 하네요.
							List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
							apSelection.ModRenderVert curMRV = null;
							for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
							{
								curMRV = modRenderVerts[iModRenderVert];

								if (Editor.Controller.IsVertexClickable(apGL.World2GL(curMRV._renderVert._pos_World), mousePosGL))
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우
										//하나 지우고 끝
										//결과는 List의 개수
										
										//이전
										//Editor.Select.RemoveModVertexOfModifier(modRenderVert._modVert, null, null, modRenderVert._renderVert);
										//변경 20.6.25
										Editor.Select.RemoveModVertexOfModifier(curMRV);

										
										if(Editor.Select.ModRenderVerts_All.Count > 0)
										{
											selectVertex = true;
										}
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Add 상태에서 원래 선택된걸 누른다면
										//추가인 경우 => 그대로
										selectVertex = true;
									}
									else
									{
										//만약... new 라면?
										//다른건 초기화하고
										//얘만 선택해야함
										//이전
										//apRenderVertex selectedRenderVert = modRenderVert._renderVert;
										//apModifiedVertex selectedModVert = modRenderVert._modVert;
										//Editor.Select.SetModVertexOfModifier(null, null, null, null);
										//Editor.Select.SetModVertexOfModifier(selectedModVert, null, null, selectedRenderVert);
										
										//변경 20.6.25
										Editor.Select.SetModVertexOfModifier(curMRV);
									}

									//return Editor.Select.ModRenderVertListOfMod.Count;
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
								}
							}
						}

					}


					//선택된 거를 다시 선택한건 아닌 것 같다.
					//새로 선택하자.

					if (selectType == apGizmos.SELECT_TYPE.New)
					{
						//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
						//이전
						//Editor.Select.SetModVertexOfModifier(null, null, null, null);

						//변경 20.6.25
						Editor.Select.SetModVertexOfModifier(null);
					}

					if (selectType != apGizmos.SELECT_TYPE.Subtract)
					{
						//변경 사항 20.6.25 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
						//이제는 아예 MRV가 생성되어있는 상태이다.
						//이 부분이 가장 크게 바뀌었다.
						List<apSelection.ModRenderVert> selectableModRenderVerts = Editor.Select.ModData.ModRenderVert_All;
						int nMRVs = selectableModRenderVerts != null ? selectableModRenderVerts.Count : 0;
						if(nMRVs > 0)
						{
							apSelection.ModRenderVert curMRV = null;
							for (int iMRV = 0; iMRV < nMRVs; iMRV++)
							{
								curMRV = selectableModRenderVerts[iMRV];
								
								bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(curMRV._renderVert._pos_World), mousePosGL);
								if (isClick)
								{
									if (selectType == apGizmos.SELECT_TYPE.New)
									{
										Editor.Select.SetModVertexOfModifier(curMRV);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										Editor.Select.AddModVertexOfModifier(curMRV);
									}

									selectVertex = true;
									//result = apGizmos.SELECT_RESULT.NewSelected;
									break;
								}
							}
						}
					}
				}

				//Vertex를 선택한게 없다면
				//+ Lock 상태가 아니라면
				if (!selectVertex && !Editor.Select.IsSelectionLock)
				{
					//Transform을 선택
					isTransformSelectable = true;
				}
			}
			else
			{
				//(Editing 상태가 아닐때)
				isTransformSelectable = true;

				if (Editor.Select.ModMesh_Main != null //변경 20.6.18
					&& Editor.Select.IsSelectionLock)
				{
					//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
					isTransformSelectable = false;
				}
			}


			// 여기서부터는 동일하다 (20.6.25 변경 사항으로부터)
			//---------------------------------------------------

			//메시 추가 선택 가능
			if (isTransformSelectable
				 //&& selectType == apGizmos.SELECT_TYPE.New//<<이거 삭제해야 제대로 다른 메시들이 선택된다.
				 )
			{
				//(Editing 상태가 아닐 때)
				//Transform을 선택한다.

				//추가 21.2.17 : 편집 중이 아닌 오브젝트를 선택하는 건 "편집 모드가 아니거나" / "선택 제한 옵션이 꺼진 경우"이다.
				bool isNotEditObjSelectable = Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || !Editor._exModObjOption_NotSelectable;


				apTransform_Mesh selectedMeshTransform = null;

				//정렬된 Render Unit
				//List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<이전 : RenderUnits_All 이용
				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 이용

				//이전
				//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)

				//변경 20.5.27 : 피킹 순서를 "앞에서"부터 하자
				if (renderUnits.Count > 0)
				{
					apRenderUnit renderUnit = null;
					for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)
					{
						renderUnit = renderUnits[iUnit];
						if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
						{
							//추가 21.2.17
							if (!isNotEditObjSelectable &&
								(renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_NotEdit || renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_ExRun))
							{
								//모디파이어에 등록되지 않은 메시는 선택 불가이다.
								//Debug.LogError("[" + renderUnit.Name + "] 옵션이 꺼져서 선택 불가");
								continue;
							}

							if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//Alpha 옵션 추가
							{
								//Debug.LogError("TODO : Mouse Picking 바꿀것");
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
									mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
									//찾았어도 계속 찾는다. 뒤의 아이템이 "앞쪽"에 있는 것이기 때문
									//>> 피킹 순서가 "앞에서"부터 찾는거라 바로 break;하면 됨
									break;
								}
							}
						}
					}
				}

				if (selectedMeshTransform != null)
				{
					//만약 ChildMeshGroup에 속한 거라면,
					//Mesh Group 자체를 선택해야 한다. <- 추가 : Child Mesh Transform이 허용되는 경우 그럴 필요가 없다.
					apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);
					if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
					{
						Editor.Select.SetSubMeshInGroup(selectedMeshTransform, multiSelect);
					}
					else
					{
						apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
						if (childMeshGroupTransform != null)
						{
							Editor.Select.SetSubMeshGroupInGroup(childMeshGroupTransform, multiSelect);
						}
						else
						{
							Editor.Select.SetSubMeshInGroup(selectedMeshTransform, multiSelect);
						}
					}
				}
				else
				{
					//선택 없음 + 다중 선택이 아닌 경우
					if(multiSelect == apSelection.MULTI_SELECT.Main)
					{
						Editor.Select.SetSubMeshInGroup(null, apSelection.MULTI_SELECT.Main);
					}
					
				}

				//Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//개수에 따라 한번더 결과 보정
			if (Editor.Select.ModRenderVerts_All != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}
			return null;
		}



		public void Unselect__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if(Editor.Gizmos.IsFFDMode)
			{
				//Debug.Log("IsFFD Mode");
				//추가 : FFD 모드에서는 버텍스 취소가 안된다.
				return;
			}

			//Editor.Select.SetModVertexOfModifier(null, null, null, null);//이전
			Editor.Select.SetModVertexOfModifier(null);//변경 20.6.25

			if (!Editor.Select.IsSelectionLock)
			{
				//SubMesh 해제를 위해서는 Lock이 풀려있어야함
				Editor.Select.SetSubMeshInGroup(null, apSelection.MULTI_SELECT.Main);
			}

			//Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}

		//-------------------------------------------------------------------------------------
		// 단축키 등록
		//-------------------------------------------------------------------------------------
		public void AddHotKeys__Modifier_Vertex(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			//Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Vertex__Ctrl_A, apHotKey.LabelText.SelectAllVertices, KeyCode.A, false, false, true, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Vertex__Ctrl_A, apHotKeyMapping.KEY_TYPE.SelectAllVertices_EditMod, null);//변경 20.12.3
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__Modifier_Vertex__Ctrl_A(object paramObject)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}
			
			bool isAnyChanged = false;


			//------------------------------
			#region [미사용 코드] 단일 처리
			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
			//	//&& Editor.Select.ExKey_ModMesh != null //이전
			//	&& Editor.Select.ModMesh_Main != null //변경 20.6.18
			//	&& Editor.Select.MeshGroup != null)
			//{
			//	//선택된 RenderUnit을 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

			//	//변경 20.6.18 : 변경 사항 2개 중 하나를 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<1>

			//	if (targetRenderUnit != null)
			//	{
			//		for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//		{
			//			apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
			//			//apModifiedVertex selectedModVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
			//			apModifiedVertex selectedModVert = Editor.Select.ModMesh_Main._vertices.Find(delegate (apModifiedVertex a)
			//			{
			//				return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//			});

			//			if (selectedModVert != null)
			//			{
			//				Editor.Select.AddModVertexOfModifier(selectedModVert, null, null, renderVert);

			//				isAnyChanged = true;
			//			}
			//		}

			//		Editor.RefreshControllerAndHierarchy(false);
			//		//Editor.Repaint();
			//		Editor.SetRepaint();
			//	}
			//} 
			#endregion

			//변경 20.6.25 : MRV 신버전 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
				&& Editor.Select.ModMesh_Main != null //변경 20.6.18
				&& Editor.Select.ModMeshes_All != null//<<다중 선택된 ModMesh도 체크한다.
				&& Editor.Select.MeshGroup != null)
			{
				//"이미 생성된" MRV 들을 한번에 선택하자
				Editor.Select.AddModRenderVerts(Editor.Select.ModData.ModRenderVert_All);
				isAnyChanged = true;
			}
			//------------------------------- > 변경 범위

			if (isAnyChanged)
			{
				Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);

				//Editor.Select.AutoSelectModMeshOrModBone();

				Editor.SetRepaint();//<<추가

			}
			return apHotKey.HotKeyResult.MakeResult();
		}

		//추가 20.1.27 : 키보드로 버텍스 이동
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__Modifier_VertexPos 함수의 코드를 이용함

			//이걸로 선택된 버텍스를 이동하는 로직을 만들어보자
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return false;
			}

			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				
				//|| Editor.Select.ExKey_ModMesh == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18
				
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVert_Main == null)
			{
				return false;
			}

			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, 
												Editor, 
												Editor.Select.Modifier, 
												null, 
												false);
			}

			if (Editor.Select.ModRenderVerts_All.Count == 1)
			{
				//1. 단일 선택일 때
				apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
				renderVert.Calculate(0.0f);

				Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;
				
				apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
				
				//이전
				//Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform * martrixMorph * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//변경 21.4.5 : 리깅 적용 (다중 모드 위함)
				Vector2 prevWorldPos2 = (	renderVert._matrix_Cal_VertWorld 
											* renderVert._matrix_MeshTransform 
											* renderVert._matrix_Rigging
											* martrixMorph 
											* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);


				Vector2 nextWorldPos = new Vector2(prevWorldPos2.x, prevWorldPos2.y) + deltaMoveW;
				
				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				//기존 계산 : Matrix를 구해서 일일이 계산한다.
				Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				
				//이전
				//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

				//변경 21.4.5 : 리깅 적용
				Vector2 nextMorphedPosM = ((	renderVert._matrix_Cal_VertWorld 
												* renderVert._matrix_MeshTransform
												* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

				Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
			}
			else
			{
				//2. 복수개 선택일 때
				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];
					apRenderVertex renderVert = modRenderVert._renderVert;
					Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;
					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}


		//추가 20.1.27 : 키보드로 버텍스 회전
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__Modifier_VertexPos 함수의 코드를 이용함

			if (Editor.Select.MeshGroup == null 
				|| Editor.Select.Modifier == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None

				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}


			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateVertex, 
													Editor, Editor.Select.Modifier, 
													null, false);
			}

			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}


			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;


		}

		//추가 20.1.27 : 키보드로 버텍스 크기 설정
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__Modifier_VertexPos 함수의 코드를 이용
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				
				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleVertex, 
													Editor, Editor.Select.Modifier, 
													null, false);
			}



			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}
		
		//-------------------------------------------------------------------------------------
		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [복수 선택]
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Vertex(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}


			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}
			// 이건 다중 버텍스 선택밖에 없다.
			//Transform 선택은 없음

			//if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			//{
			//	return apGizmos.SELECT_RESULT.None;
			//}

			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			bool isAnyChanged = false;

			//----------------------------------------------
			#region [미사용 코드] 단일 처리
			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 

			//	//&& Editor.Select.ExKey_ModMesh != null //이전
			//	&& Editor.Select.ModMesh_Main != null //변경

			//	&& Editor.Select.MeshGroup != null)
			//{
			//	//선택된 RenderUnit을 고르자
			//	//이전
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);
			//	//변경 20.6.18 : 다음 두가지 중 하나를 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<2>

			//	if (targetRenderUnit != null)
			//	{
			//		for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//		{
			//			apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
			//			bool isSelectable = (mousePosW_Min.x < renderVert._pos_World.x && renderVert._pos_World.x < mousePosW_Max.x)
			//						&& (mousePosW_Min.y < renderVert._pos_World.y && renderVert._pos_World.y < mousePosW_Max.y);
			//			if (isSelectable)
			//			{
			//				//apModifiedVertex selectedModVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
			//				apModifiedVertex selectedModVert = Editor.Select.ModMesh_Main._vertices.Find(delegate (apModifiedVertex a)
			//				{
			//					return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//				});

			//				if (selectedModVert != null)
			//				{
			//					if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
			//						areaSelectType == apGizmos.SELECT_TYPE.New)
			//					{
			//						Editor.Select.AddModVertexOfModifier(selectedModVert, null, null, renderVert);
			//					}
			//					else
			//					{
			//						Editor.Select.RemoveModVertexOfModifier(selectedModVert, null, null, renderVert);
			//					}

			//					isAnyChanged = true;
			//					//result = apGizmos.SELECT_RESULT.NewSelected;
			//					//break;
			//				}

			//			}
			//		}

			//		Editor.RefreshControllerAndHierarchy(false);
			//		//Editor.Repaint();
			//		Editor.SetRepaint();
			//	}


			//} 
			#endregion

			//변경 20.6.25 : MRV 새로운 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
				&& Editor.Select.ModMesh_Main != null //변경
				&& Editor.Select.ModMeshes_All != null //다중 선택된 ModMesh도 체크한다.
				&& Editor.Select.MeshGroup != null)
			{
				//변경 사항 20.6.25 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
				//이제는 아예 MRV가 생성되어있는 상태이다.
				//이 부분이 가장 크게 바뀌었다.

				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModData.ModRenderVert_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;
				if (nMRVs > 0)
				{
					apSelection.ModRenderVert curMRV = null;
					for (int iMRV = 0; iMRV < nMRVs; iMRV++)
					{
						curMRV = modRenderVerts[iMRV];

						bool isSelectable = (mousePosW_Min.x < curMRV._renderVert._pos_World.x && curMRV._renderVert._pos_World.x < mousePosW_Max.x)
												&& (mousePosW_Min.y < curMRV._renderVert._pos_World.y && curMRV._renderVert._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							if (areaSelectType == apGizmos.SELECT_TYPE.Add || areaSelectType == apGizmos.SELECT_TYPE.New)
							{
								Editor.Select.AddModVertexOfModifier(curMRV);
							}
							else
							{
								Editor.Select.RemoveModVertexOfModifier(curMRV);
							}

							isAnyChanged = true;
						}
					}
				}
			}
			//----------------------------------------------

			if (isAnyChanged)
			{
				Editor.Select.AutoSelectModMeshOrModBone();
				Editor.SetRepaint();//<<추가
			}

			//return Editor.Select.ModRenderVertListOfMod.Count;
			return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Move]
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Modifier_VertexPos(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			//(Editing 상태일 때)
			//1. 선택된 Vertex가 있다면
			//2. 없다면 -> 패스

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				
				//|| Editor.Select.ExKey_ModMesh == null //이전
				|| Editor.Select.ModMesh_Main == null //변경 20.6.18
				
				|| Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			if (Editor.Select.ModRenderVert_Main == null)
			{
				return;
			}

			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, 
													Editor, Editor.Select.Modifier, 
													null, false);
			}



			if (Editor.Select.ModRenderVerts_All.Count == 1)
			{
				//1. 단일 선택일 때
				apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
				renderVert.Calculate(0.0f);

				Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;

				apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
				
				//이전
				//Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld 
				//							* renderVert._matrix_MeshTransform 
				//							* martrixMorph 
				//							* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
				Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld 
											* renderVert._matrix_MeshTransform 
											* renderVert._matrix_Rigging
											* martrixMorph 
											* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				Vector2 nextWorldPos = new Vector2(prevWorldPos2.x, prevWorldPos2.y) + deltaMoveW;

				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				//기존 계산 : Matrix를 구해서 일일이 계산한다.
				Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
				
				//기존
				//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);
				
				//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
				Vector2 nextMorphedPosM = (	(	renderVert._matrix_Cal_VertWorld 
												* renderVert._matrix_MeshTransform 
												* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

				Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
			}
			else
			{
				//2. 복수개 선택일 때
				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

					apRenderVertex renderVert = modRenderVert._renderVert;

					Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
					//apMatrix3x3 matToAfterVertLocal = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse;
					//Vector3 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint3x4(new Vector3(nextWorldPos.x, nextWorldPos.y, 0));
					//Vector3 beforeLocalMorphedPos = (renderVert._matrix_Cal_VertLocal * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);


					//modRenderVert._modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
					//modRenderVert._modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
			//Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Rotate]
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_VertexPos(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModMesh == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18
				)
			{
				return;
			}

			if (Editor.Select.ModRenderVerts_All == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			if (deltaAngleW > 180.0f) { deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f) { deltaAngleW += 360.0f; }

			//Quaternion quat = Quaternion.Euler(0.0f, 0.0f, deltaAngleW);

			apMatrix3x3 matrix_Rotate =		apMatrix3x3.TRS(centerPos, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
											* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateVertex, 
													Editor, Editor.Select.Modifier, 
													null, false);
			}

			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}


			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Scale]
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_VertexPos(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && isFirstScale)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModMesh == null//이전
				|| Editor.Select.ModMesh_Main == null//변경
				)
			{
				return;
			}

			if (Editor.Select.ModRenderVerts_All == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			//bool isMultipleVerts = true;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleVertex, 
													Editor, 
													Editor.Select.Modifier, 
													null, false);
			}



			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();

			//apMatrix targetMatrix;
			//Vector2 scale2 = new Vector2(targetMatrix._scale.x, targetMatrix._scale.y);
			//targetMatrix.SetScale(deltaScaleW + scale2);
			//targetMatrix.MakeMatrix();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Vertex의 위치값 [Position]
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Modifier_VertexPos(Vector2 pos)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.ExEditingMode == apSelection.EX_EDIT.None ||
				
				Editor.Select.ModRenderVert_Main == null ||
				
				//이전
				//Editor.Select.ExKey_ModMesh == null ||
				//Editor.Select.RenderUnitOfMod == null ||
				
				//변경 20.6.18
				Editor.Select.ModMesh_Main == null ||
				Editor.Select.RenderUnitOfMod_Main == null ||

				Editor.Select.ModRenderVerts_All.Count == 0)
			{
				//편집 가능한 상태가 아니면 패스
				return;
			}

			//if (Editor.Select.SubMeshTransform == null && Editor.Select.SubMeshGroupTransform == null)
			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)
			{
				return;
			}

			//Vector2 deltaPosW = Vector2.zero;
			Vector2 deltaPosChanged = Vector2.zero;

			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, 
												Editor, 
												Editor.Select.Modifier, 
												null, false);


			//Depth는 신경쓰지 말자
			if (Editor.Select.ModRenderVerts_All.Count == 1)
			{
				//수정 : 직접 대입한다.
				deltaPosChanged = pos - Editor.Select.ModRenderVert_Main._modVert._deltaPos;

				Editor.Select.ModRenderVert_Main._modVert._deltaPos = pos;


				Editor.Select.MeshGroup.RefreshForce();
			}
			else
			{
				//복수 선택시
				//수정 : 
				//AvgCenterDeltaPos의 변화값을 대입한다.
				Vector2 avgDeltaPos = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					avgDeltaPos += Editor.Select.ModRenderVerts_All[i]._modVert._deltaPos;
				}
				avgDeltaPos /= Editor.Select.ModRenderVerts_All.Count;

				Vector2 deltaPos2Next = pos - avgDeltaPos;
				deltaPosChanged = deltaPos2Next;

				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					Editor.Select.ModRenderVerts_All[i]._modVert._deltaPos += deltaPos2Next;
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					//변경 : DeltaPos의 변경 값으로만 계산한다.
					modRenderVert._modVert._deltaPos = ((modRenderVert._modVert._deltaPos + deltaPosChanged) * weight) + (modRenderVert._modVert._deltaPos * (1.0f - weight));
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
		}



		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_Vertex(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//이전
				//|| Editor.Select.ExKey_ModMesh == null 
				//|| Editor.Select.ExKey_ModParamSet == null
				//변경 20.6.18
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ModMesh_Gizmo_Main == null
				|| Editor.Select.ParamSetOfMod == null
				)
			{
				return;
			}

			//기존 방식
			////Undo
			////apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color, Editor, Editor.Select.Modifier, Editor.Select.ExKey_ModMesh, false);
			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color,
			//									Editor,
			//									Editor.Select.Modifier,
			//									Editor.Select.ModMesh_Main, false);//변경 20.6.18

			////Editor.Select.ExKey_ModMesh._meshColor = color;
			////Editor.Select.ExKey_ModMesh._isVisible = isVisible;

			////qusrud 20.6.18
			//Editor.Select.ModMesh_Main._meshColor = color;
			//Editor.Select.ModMesh_Main._isVisible = isVisible;

			////강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.RefreshForce();

			//다중 처리 + 기즈모 메인 방식
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;

			if (nModMeshes == 0)
			{
				//선택된게 없다면
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color,
											Editor, Editor.Select.Modifier,
											//Editor.Select.ModMesh_Main, 
											Editor.Select.ModMesh_Gizmo_Main,//<< [GizmoMain]
											false);//변경 20.6.18

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;


				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{ continue; }

					curModMesh._meshColor = color;
					curModMesh._isVisible = isVisible;
				}
			}

			//강제로 업데이트할 객체를 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}



		public void TransformChanged_Extra__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//이전
				//|| Editor.Select.ExKey_ModMesh == null 
				//|| Editor.Select.ExKey_ModParamSet == null
				//|| Editor.Select.RenderUnitOfMod == null
				//변경 20.6.18
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ParamSetOfMod == null
				|| Editor.Select.RenderUnitOfMod_Main == null
				)
			{
				return;
			}

			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			apModifierBase modifier = Editor.Select.Modifier;

			//apModifiedMesh modMesh = Editor.Select.ExKey_ModMesh;
			//apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod;

			//변경 20.6.18
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod_Main;

			if(!modifier._isExtraPropertyEnabled)
			{
				return;
			}

			//Extra Option을 제어하는 Dialog를 호출하자
			apDialog_ExtraOption.ShowDialog(Editor, Editor._portrait, meshGroup, modifier, modMesh, renderUnit, false, null, null);
		}

		public apGizmos.TransformParam PivotReturn__Modifier_Vertex()
		{

			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main== null)
			{
				return null;
			}



			if (Editor.Select.RenderUnitOfMod_Main == null)
			{
				return null;
			}



			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18
				)
			{
				return null;
			}

			apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.None;
			if (Editor.Select.Modifier._isColorPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
			}
			if (Editor.Select.Modifier._isExtraPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Extra;//추가 : Extra 옵션
			}

			if (Editor.Select.ModRenderVert_Main == null)
			{
				//Vert 선택한게 없다면
				//Color 옵션만이라도 설정하게 하자
				if (Editor.Select.Modifier._isColorPropertyEnabled
					|| Editor.Select.Modifier._isExtraPropertyEnabled)
				{
					return apGizmos.TransformParam.Make(apEditorUtil.InfVector2, 0.0f, Vector2.one, 0,
													//이전
													//Editor.Select.ExKey_ModMesh._meshColor,
													//Editor.Select.ExKey_ModMesh._isVisible,
													//변경
													Editor.Select.ModMesh_Main._meshColor,
													Editor.Select.ModMesh_Main._isVisible,

													apMatrix3x3.identity,
													false,
													paramType,
													Vector2.zero, 0.0f, Vector2.one
													);
				}
				return null;
			}

			//TODO : 여러개의 Vert를 수정할 수 있도록 한다.
			paramType |= apGizmos.TRANSFORM_UI.Position2D;

			if (Editor.Select.ModRenderVerts_All.Count > 1)
			{
				paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

				Vector2 avgDeltaPos = Vector2.zero;
				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					avgDeltaPos += Editor.Select.ModRenderVerts_All[i]._modVert._deltaPos;
				}
				avgDeltaPos /= Editor.Select.ModRenderVerts_All.Count;

				
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertsCenterPos,
													0.0f,
													Vector2.one,
													//이전
													//Editor.Select.RenderUnitOfMod.GetDepth(),
													//Editor.Select.ExKey_ModMesh._meshColor,
													//Editor.Select.ExKey_ModMesh._isVisible,
													
													//변경 20.6.18
													Editor.Select.RenderUnitOfMod_Main.GetDepth(),
													Editor.Select.ModMesh_Main._meshColor,
													Editor.Select.ModMesh_Main._isVisible,

													apMatrix3x3.identity,
													true,
													paramType,
													avgDeltaPos,
													0.0f,
													Vector2.one
													);
			}
			else
			{
				return apGizmos.TransformParam.Make(Editor.Select.ModRenderVert_Main._renderVert._pos_World,
													0.0f,
													Vector2.one,
													//이전
													//Editor.Select.RenderUnitOfMod.GetDepth(),
													//Editor.Select.ExKey_ModMesh._meshColor,
													//Editor.Select.ExKey_ModMesh._isVisible,
													//변경 20.6.18 
													Editor.Select.RenderUnitOfMod_Main.GetDepth(),
													Editor.Select.ModMesh_Main._meshColor,
													Editor.Select.ModMesh_Main._isVisible,

													Editor.Select.ModRenderVert_Main._renderVert._matrix_ToWorld,
													false,
													paramType,
													Editor.Select.ModRenderVert_Main._modVert._deltaPos,
													0.0f,
													Vector2.one
													);
			}
		}

		public bool FFDTransform__Modifier_VertexPos(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			if (!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.
				if (Editor.Select.ModRenderVerts_All == null)
				{
					return false;
				}

				if (Editor.Select.ModRenderVerts_All.Count <= 1)
				{
					return false;
				}
			}

			//Undo
			if (isRecord)//Undo 요청이 있을 때만 저장
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_FFDVertex,
													Editor, Editor.Select.Modifier,
													null, false);
			}


			apSelection.ModRenderVert modRenderVert = null;
			if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
			{
				//World Pos를 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
					if (modRenderVert == null)
					{
						continue;
					}

					modRenderVert.SetWorldPosToModifier_VertLocal(posData[i]);
				}
			}
			else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
			{
				//저장된 데이터를 직접 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
					if (modRenderVert == null)
					{
						continue;
					}

					modRenderVert._modVert._deltaPos = posData[i];
				}
			}
			

			//강제로 업데이트할 객체 선택하고 Refresh
			//if (Editor.Select.ExKey_ModMesh != null)
			//{
			//	Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			//}
			Editor.Select.MeshGroup.RefreshForce();
			return true;
		}

		public bool StartFFDTransform__Modifier_VertexPos()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}
			if (Editor.Select.ModRenderVerts_All == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			List<Vector2> modDataList = new List<Vector2>();

			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];
				srcObjectList.Add(modRenderVert);
				worldPosList.Add(modRenderVert._renderVert._pos_World);
				modDataList.Add(modRenderVert._modVert._deltaPos);//<<추가 20.7.22
			}

			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, modDataList);//<<True로 리턴할거면 이 함수를 호출해주자

			return true;
		}

		public bool SoftSelection__Modifier_VertexPos()
		{
			Editor.Select.ModRenderVerts_Weighted.Clear();

			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0)
			{
				return false;
			}

			float radius = (float)Editor.Gizmos.SoftSelectionRadius;
			if (radius <= 0.0f)
			{
				return false;
			}

			bool isConvex = Editor.Gizmos.SoftSelectionCurveRatio >= 0;
			float curveRatio = Mathf.Clamp01(Mathf.Abs((float)Editor.Gizmos.SoftSelectionCurveRatio / 100.0f));//0이면 직선, 1이면 커브(볼록/오목)

			//선택되지 않은 Vertex 중에서
			//"기본 위치 값"을 기준으로 영역을 선택해주자
			float minDist = 0.0f;
			float dist = 0.0f;
			apSelection.ModRenderVert minRV = null;

			#region [미사용 코드] 단일 선택에 대해서만 동작하는 코드
			////if (Editor.Select.ExKey_ModMesh._transform_Mesh != null && Editor.Select.ExKey_ModMesh._vertices != null)
			//if (Editor.Select.ModMesh_Main._transform_Mesh != null && Editor.Select.ModMesh_Main._vertices != null)//변경 20.6.18 
			//{
			//	//선택된 RenderUnit을 고르자
			//	//이전
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

			//	//변경 20.6.18
			//	//다음 중 하나를 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<2>

			//	if (targetRenderUnit != null)
			//	{
			//		for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//		{
			//			apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];

			//			//선택된 RenderVert는 제외한다.
			//			if (Editor.Select.ModRenderVertListOfMod.Exists(delegate (apSelection.ModRenderVert a)
			//			 {
			//				 return a._renderVert == renderVert;
			//			 }))
			//			{
			//				continue;
			//			}


			//			//가장 가까운 RenderVert를 찾는다.
			//			minDist = 0.0f;
			//			dist = 0.0f;
			//			minRV = null;

			//			for (int iSelectedRV = 0; iSelectedRV < Editor.Select.ModRenderVertListOfMod.Count; iSelectedRV++)
			//			{
			//				apSelection.ModRenderVert selectedModRV = Editor.Select.ModRenderVertListOfMod[iSelectedRV];
			//				//현재 World위치로 선택해보자.

			//				dist = Vector2.Distance(selectedModRV._renderVert._pos_World, renderVert._pos_World);
			//				if (dist < minDist || minRV == null)
			//				{
			//					minRV = selectedModRV;
			//					minDist = dist;
			//				}
			//			}

			//			if (minRV != null && minDist <= radius)
			//			{
			//				//apModifiedVertex modVert = Editor.Select.ExKey_ModMesh._vertices.Find(delegate (apModifiedVertex a)
			//				apModifiedVertex modVert = Editor.Select.ModMesh_Main._vertices.Find(delegate (apModifiedVertex a)
			//				{
			//					return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//				});

			//				if (modVert != null)
			//				{

			//					//radius에 들어가는 Vert 발견.
			//					//Weight는 CurveRatio에 맞게 (minDist가 0에 가까울수록 Weight는 1이 된다.)
			//					float itp_Linear = minDist / radius;
			//					float itp_Curve = 0.0f;
			//					if (isConvex)
			//					{
			//						//Weight가 더 1에 가까워진다. => minDist가 0이 되는 곳에 Control Point를 넣자
			//						itp_Curve = (1.0f * (itp_Linear * itp_Linear))
			//							+ (2.0f * 0.0f * itp_Linear * (1.0f - itp_Linear))
			//							+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
			//					}
			//					else
			//					{
			//						//Weight가 더 0에 가까워진다. => minDist가 radius가 되는 곳에 Control Point를 넣자
			//						itp_Curve = (1.0f * (itp_Linear * itp_Linear))
			//							+ (2.0f * 1.0f * itp_Linear * (1.0f - itp_Linear))
			//							+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
			//					}
			//					float itp = itp_Linear * (1.0f - curveRatio) + itp_Curve * curveRatio;
			//					float weight = 0.0f * itp + 1.0f * (1.0f - itp);

			//					apSelection.ModRenderVert newModRenderVert = new apSelection.ModRenderVert(modVert, renderVert);
			//					//Weight를 추가로 넣어주고 리스트에 넣자
			//					newModRenderVert._vertWeightByTool = weight;

			//					Editor.Select.ModRenderVertListOfMod_Weighted.Add(newModRenderVert);
			//				}
			//			}
			//		}
			//	}
			//} 
			#endregion


			//전체 MRV를 돌면서 "선택되지 않은 MRV"를 찾자
			List<apSelection.ModRenderVert> allMRVs = Editor.Select.ModData.ModRenderVert_All;
			List<apSelection.ModRenderVert> selectedMRVs = Editor.Select.ModRenderVerts_All;

			int nMRVs = allMRVs != null ? allMRVs.Count : 0;
			if (nMRVs > 0)
			{
				apSelection.ModRenderVert curMRV = null;
				for (int iMRV = 0; iMRV < nMRVs; iMRV++)
				{
					curMRV = allMRVs[iMRV];

					//선택된 RenderVert는 제외한다.
					if (selectedMRVs.Contains(curMRV))
					{
						continue;
					}

					//가장 가까운 선택된 RenderVert를 찾는다.
					minDist = 0.0f;
					dist = 0.0f;
					minRV = null;

					for (int iSelectedRV = 0; iSelectedRV < selectedMRVs.Count; iSelectedRV++)
					{
						apSelection.ModRenderVert selectedModRV = selectedMRVs[iSelectedRV];
						//현재 World위치로 선택해보자.

						dist = Vector2.Distance(selectedModRV._renderVert._pos_World, curMRV._renderVert._pos_World);
						if (dist < minDist || minRV == null)
						{
							minRV = selectedModRV;
							minDist = dist;
						}
					}

					if (minRV != null && minDist <= radius)
					{
						//radius에 들어가는 Vert 발견.
						//Weight는 CurveRatio에 맞게 (minDist가 0에 가까울수록 Weight는 1이 된다.)
						float itp_Linear = minDist / radius;
						float itp_Curve = 0.0f;
						if (isConvex)
						{
							//Weight가 더 1에 가까워진다. => minDist가 0이 되는 곳에 Control Point를 넣자
							itp_Curve = (1.0f * (itp_Linear * itp_Linear))
								+ (2.0f * 0.0f * itp_Linear * (1.0f - itp_Linear))
								+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
						}
						else
						{
							//Weight가 더 0에 가까워진다. => minDist가 radius가 되는 곳에 Control Point를 넣자
							itp_Curve = (1.0f * (itp_Linear * itp_Linear))
								+ (2.0f * 1.0f * itp_Linear * (1.0f - itp_Linear))
								+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
						}
						float itp = itp_Linear * (1.0f - curveRatio) + itp_Curve * curveRatio;
						float weight = 0.0f * itp + 1.0f * (1.0f - itp);

						//Weight를 추가로 넣어주고 리스트에 넣자
						curMRV._vertWeightByTool = weight;

						Editor.Select.ModRenderVerts_Weighted.Add(curMRV);
					}
				}
			}

			

			return true;
		}

		private List<apModifiedVertex> _tmpBlurVertices = new List<apModifiedVertex>();
		private List<float> _tmpBlurVertexWeights = new List<float>();

		public apGizmos.BrushInfo SyncBlurStatus__Modifier_VertexPos(bool isEnded)
		{
			if (isEnded)
			{
				Editor._blurEnabled = false;
			}
			apGizmos.BRUSH_COLOR_MODE colorMode = apGizmos.BRUSH_COLOR_MODE.Default;
			if(Editor._blurIntensity == 0)		{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
			else if(Editor._blurIntensity < 33)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
			else if(Editor._blurIntensity < 66)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
			else								{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

			return apGizmos.BrushInfo.MakeInfo(Editor._blurRadius, Editor._blurIntensity, colorMode, null);
		}

		public bool PressBlur__Modifier_VertexPos(Vector2 pos, float tDelta, bool isFirstBlur)
		{
			if (
				//Editor.Select.ExKey_ModMesh._transform_Mesh == null
				//|| Editor.Select.ExKey_ModMesh._vertices == null
				
				//변경 20.6.18
				Editor.Select.ModMesh_Main._transform_Mesh == null
				|| Editor.Select.ModMesh_Main._vertices == null

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}

			float radius = Editor.Gizmos.BrushRadiusGL;
			float intensity = Mathf.Clamp01((float)Editor.Gizmos.BrushIntensity / 100.0f);

			if (radius <= 0.0f || intensity <= 0.0f)
			{
				return false;
			}

			_tmpBlurVertices.Clear();
			_tmpBlurVertexWeights.Clear();


			Vector2 totalModValue = Vector2.zero;
			float totalWeight = 0.0f;


			//1. 영역 안의 Vertex를 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
			//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자

			//영역 체크는 GL값
			float dist = 0.0f;
			float weight = 0.0f;
			if(isFirstBlur)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_BlurVertex, 
												Editor, 
												Editor.Select.Modifier, 
												null, false);
			}

			//선택된 Vert에 한해서만 처리하자
			apSelection.ModRenderVert curMRV = null;
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				curMRV = Editor.Select.ModRenderVerts_All[i];
				dist = Vector2.Distance(curMRV._renderVert._pos_GL, pos);
				if (dist > radius)
				{
					continue;
				}

				weight = (radius - dist) / radius;
				totalModValue += curMRV._modVert._deltaPos * weight;
				totalWeight += weight;

				_tmpBlurVertices.Add(curMRV._modVert);
				_tmpBlurVertexWeights.Add(weight);
			}

			if (_tmpBlurVertices.Count > 0 && totalWeight > 0.0f)
			{
				//Debug.Log("Blur : " + _tmpBlurVertices.Count + "s Verts / " + totalWeight);

				totalModValue /= totalWeight;

				for (int i = 0; i < _tmpBlurVertices.Count; i++)
				{
					//0이면 유지, 1이면 변경
					float itp = Mathf.Clamp01(_tmpBlurVertexWeights[i] * tDelta * intensity * 5.0f);

					_tmpBlurVertices[i]._deltaPos =
						_tmpBlurVertices[i]._deltaPos * (1.0f - itp) +
						totalModValue * itp;
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}
	}

}