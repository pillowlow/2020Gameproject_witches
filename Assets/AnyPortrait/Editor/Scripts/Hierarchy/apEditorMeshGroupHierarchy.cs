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

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apEditorMeshGroupHierarchy
	{
		// Members
		//---------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }



		public enum CATEGORY
		{
			MainName,
			//Transform,
			//Mesh_Name,
			Mesh_Item,
			//Mesh_Load,
			//MeshGroup_Name,
			MeshGroup_Item,
			//MeshGroup_Load,

			MainName_Bone,
			SubName_Bone,
			Bone_Item
		}

		//루트들만 따로 적용
		//private apEditorHierarchyUnit _rootUnit_Mesh = null;
		//private apEditorHierarchyUnit _rootUnit_MeshGroup = null;

		//Mesh / Bone으로 나눔
		private apEditorHierarchyUnit _rootUnit_Meshes = null;
		private apEditorHierarchyUnit _rootUnit_Bones_Main = null;
		private List<apEditorHierarchyUnit> _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>();

		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Meshes = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Bones = new List<apEditorHierarchyUnit>();

		//추가 20.7.4 : 다중 선택 위해서 마지막으로 선택된 Unit을 기록하자
		private apEditorHierarchyUnit _lastClickedUnit = null;//마지막으로 클릭한 유닛 (클릭한 경우만 유효)
		private List<apEditorHierarchyUnit> _shiftClickedUnits = new List<apEditorHierarchyUnit>();//Shift로 클릭했을때 결과로 나올 추가 선택 유닛들



		//개선 20.3.28 : Pool을 이용하자
		private apEditorHierarchyUnitPool _unitPool = new apEditorHierarchyUnitPool();

		public enum HIERARCHY_TYPE
		{
			Meshes, Bones
		}


		//Visible Icon에 대한 GUIContent
		private apGUIContentWrapper _guiContent_Visible_Current = null;
		private apGUIContentWrapper _guiContent_NonVisible_Current = null;

		private apGUIContentWrapper _guiContent_Visible_TmpWork = null;
		private apGUIContentWrapper _guiContent_NonVisible_TmpWork = null;

		private apGUIContentWrapper _guiContent_Visible_Rule = null;
		private apGUIContentWrapper _guiContent_NonVisible_Rule = null;

		private apGUIContentWrapper _guiContent_Visible_Default = null;
		private apGUIContentWrapper _guiContent_NonVisible_Default = null;

		private apGUIContentWrapper _guiContent_Visible_ModKey = null;
		private apGUIContentWrapper _guiContent_NonVisible_ModKey = null;

		private apGUIContentWrapper _guiContent_NoKey = null;

		//추가 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = null;
		private apGUIContentWrapper _guiContent_FoldRight = null;
		private apGUIContentWrapper _guiContent_ModRegisted = null;
		
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;
		private int _curUnitPosY = 0;

		// Init
		//------------------------------------------------------------------------
		public apEditorMeshGroupHierarchy(apEditor editor)
		{
			_editor = editor;

			if(_unitPool == null)
			{
				_unitPool = new apEditorHierarchyUnitPool();
			}
		}


		private void ReloadGUIContent()
		{
			if (_editor == null)
			{
				return;
			}
			if (_guiContent_Visible_Current == null)		{ _guiContent_Visible_Current =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current)); }
			if (_guiContent_NonVisible_Current == null)		{ _guiContent_NonVisible_Current =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current)); }
			if (_guiContent_Visible_TmpWork == null)		{ _guiContent_Visible_TmpWork =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_TmpWork)); }
			if (_guiContent_NonVisible_TmpWork == null)		{ _guiContent_NonVisible_TmpWork =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_TmpWork)); }
			if (_guiContent_Visible_Rule == null)			{ _guiContent_Visible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Rule)); }
			if (_guiContent_NonVisible_Rule == null)		{ _guiContent_NonVisible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Rule)); }
			if (_guiContent_Visible_Default == null)		{ _guiContent_Visible_Default =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Default)); }
			if (_guiContent_NonVisible_Default == null)		{ _guiContent_NonVisible_Default =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Default)); }
			if (_guiContent_Visible_ModKey == null)			{ _guiContent_Visible_ModKey =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_ModKey)); }
			if (_guiContent_NonVisible_ModKey == null)		{ _guiContent_NonVisible_ModKey =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_ModKey)); }
			if (_guiContent_NoKey == null)					{ _guiContent_NoKey =				apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKey)); }

			//GUIContent 추가
			if (_guiContent_FoldDown == null)			{ _guiContent_FoldDown =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)); }
			if (_guiContent_FoldRight == null)			{ _guiContent_FoldRight =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight)); }
			if (_guiContent_ModRegisted == null)		{ _guiContent_ModRegisted =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered)); }

			if (_guiContent_RestoreTmpWorkVisible_ON == null)		{ _guiContent_RestoreTmpWorkVisible_ON =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON)); }
			if (_guiContent_RestoreTmpWorkVisible_OFF == null)		{ _guiContent_RestoreTmpWorkVisible_OFF =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF)); }
		}


		public void ResetSubUnits()
		{
			_units_All.Clear();
			_units_Root_Meshes.Clear();
			_units_Root_Bones.Clear();


			_rootUnit_Meshes = null;
			_rootUnit_Bones_Main = null;
			_rootUnit_Bones_Sub.Clear();

			//추가 20.7.4 : 다중 선택용 변수
			_lastClickedUnit = null;

			

			//_rootUnit_Mesh = AddUnit_Label(null, "Child Meshes", CATEGORY.Mesh_Name, null, true, null);
			//_rootUnit_MeshGroup = AddUnit_Label(null, "Child Mesh Groups", CATEGORY.MeshGroup_Name, null, true, null);

			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			ReloadGUIContent();//<<추가

			//Pool 초기화
			if(_unitPool.IsInitialized)
			{
				_unitPool.PushAll();
			}
			else
			{
				_unitPool.Init();
			}

			string meshGroupName = Editor.Select.MeshGroup._name;
			if(meshGroupName.Length > 16)
			{
				meshGroupName = meshGroupName.Substring(0, 14) + "..";
			}
			_rootUnit_Meshes = AddUnit_Label(null, meshGroupName, CATEGORY.MainName, null, true, null, HIERARCHY_TYPE.Meshes);
			_rootUnit_Bones_Main = AddUnit_Label(null, meshGroupName, CATEGORY.MainName_Bone, null, true, null, HIERARCHY_TYPE.Bones);

			//추가 19.6.29 : RestoreTmpWorkVisible 버튼을 추가하자.
			//_rootUnit_Meshes.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
			//											Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
			//											OnUnitClickRestoreTmpWork_Mesh);

			_rootUnit_Meshes.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
														_guiContent_RestoreTmpWorkVisible_OFF,
														OnUnitClickRestoreTmpWork_Mesh);

			_rootUnit_Meshes.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);

			//_rootUnit_Bones_Main.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
			//												Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
			//												OnUnitClickRestoreTmpWork_Bone);

			_rootUnit_Bones_Main.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
															_guiContent_RestoreTmpWorkVisible_OFF,
															OnUnitClickRestoreTmpWork_Bone);


			_rootUnit_Bones_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);



			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			

			//수정
			if(meshGroup._boneListSets.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
				{
					boneSet = meshGroup._boneListSets[iSet];
					if(boneSet._isRootMeshGroup)
					{
						//Root MeshGroup의 Bone이면 패스
						continue;
					}

					//Bone을 가지고 있는 Child MeshGroup Transform을 Sub 루트로 삼는다.
					//나중에 구분하기 위해 meshGroupTransform을 SavedObj에 넣는다.
					_rootUnit_Bones_Sub.Add(
						AddUnit_Label(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
										boneSet._meshGroupTransform._nickName,
										CATEGORY.SubName_Bone,
										boneSet._meshGroupTransform, //<Saved Obj
										true, null,
										HIERARCHY_TYPE.Bones));
				}
				
			}

			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//구버전 코드
			#region [미사용 코드]
			////> 재귀적인 Hierarchy를 허용하지 않는다.
			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}

			//	AddUnit_ToggleButton_Visible(	iconImage, 
			//									meshTransform._nickName, 
			//									CATEGORY.Mesh_Item, 
			//									meshTransform, 
			//									false, 
			//									//_rootUnit_Mesh,
			//									_rootUnit,
			//									meshTransform._isVisible_Default);
			//}

			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	AddUnit_ToggleButton_Visible(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//									meshGroupTransform._nickName, 
			//									CATEGORY.MeshGroup_Item, 
			//									meshGroupTransform, 
			//									false, 
			//									//_rootUnit_MeshGroup,
			//									_rootUnit,
			//									meshGroupTransform._isVisible_Default);
			//} 
			#endregion

			//변경된 코드
			//재귀적인 구조를 만들고 싶다면 여길 수정하자
			AddTransformOfMeshGroup(Editor.Select.MeshGroup, _rootUnit_Meshes);

			//Bone도 만들어주자
			//Main과 Sub 모두
			AddBoneUnitsOfMeshGroup(meshGroup, _rootUnit_Bones_Main);

			if (_rootUnit_Bones_Sub.Count > 0)
			{
				for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
				{
					apTransform_MeshGroup mgTranform = _rootUnit_Bones_Sub[i]._savedObj as apTransform_MeshGroup;
					if (mgTranform != null && mgTranform._meshGroup != null)
					{
						AddBoneUnitsOfMeshGroup(mgTranform._meshGroup, _rootUnit_Bones_Sub[i]);
					}
				}
			}


			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
		}

		// Functions
		//------------------------------------------------------------------------

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon,
														string text,
														CATEGORY savedKey,
														object savedObj,
														bool isRoot,
														apEditorHierarchyUnit parent,
														HIERARCHY_TYPE hierarchyType)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);
			
			newUnit.SetEvent(OnUnitClick);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(false);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private apEditorHierarchyUnit AddUnit_ToggleButton_Visible(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot,
																		apEditorHierarchyUnit parent,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
																		bool isModRegisted,
																		HIERARCHY_TYPE hierarchyType,
																		bool isRefreshVisiblePrefixWhenRender)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));


			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			apEditorHierarchyUnit.FUNC_REFRESH_VISIBLE_PREFIX funcRefreshVislbePrefix = null;
			if(isRefreshVisiblePrefixWhenRender)
			{
				funcRefreshVislbePrefix = OnRefreshVisiblePrefix;
			}


			newUnit.SetVisibleIconImage(
				_guiContent_Visible_Current, _guiContent_NonVisible_Current,
				_guiContent_Visible_TmpWork, _guiContent_NonVisible_TmpWork,
				_guiContent_Visible_Default, _guiContent_NonVisible_Default,
				_guiContent_Visible_ModKey, _guiContent_NonVisible_ModKey,
				_guiContent_Visible_Rule, _guiContent_NonVisible_Rule,
				_guiContent_NoKey,
				funcRefreshVislbePrefix
				);

			newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick);
			newUnit.SetToggleButton_Visible(icon, text, (int)savedKey, savedObj, true, visibleType_Prefix, visibleType_Postfix);

			newUnit.SetModRegistered(isModRegisted);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}

			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private apEditorHierarchyUnit AddUnit_ToggleButton(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot, bool isModRegisted,
																		apEditorHierarchyUnit parent,
																		HIERARCHY_TYPE hierarchyType)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			
			newUnit.SetEvent(OnUnitClick);
			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj, true);

			newUnit.SetModRegistered(isModRegisted);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}

			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// 추가 : Transform 리스트를 재귀적으로 만든다.
		//-----------------------------------------------------------------------------------------
		private void AddTransformOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegisted = false;
			apModifierBase modifier = Editor.Select.Modifier;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];
				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
				}

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshTransform);
				}

				//apEditorHierarchyUnit.VISIBLE_ICON_TYPE visibleType = (meshTransform._isVisible_Default) ? apEditorHierarchyUnit.VISIBLE_ICON_TYPE.Visible : apEditorHierarchyUnit.VISIBLE_ICON_TYPE.NonVisible;



				AddUnit_ToggleButton_Visible(	iconImage,
												meshTransform._nickName,
												CATEGORY.Mesh_Item,
												meshTransform,
												false,
												//_rootUnit_Mesh,
												parentUnit,
												GetVisibleIconType(meshTransform, isModRegisted, true),
												GetVisibleIconType(meshTransform, isModRegisted, false),
												isModRegisted,
												HIERARCHY_TYPE.Meshes,
												true
												);


			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit newUnit = AddUnit_ToggleButton_Visible(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform._nickName,
													CATEGORY.MeshGroup_Item,
													meshGroupTransform,
													false,
													//_rootUnit_MeshGroup,
													parentUnit,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(meshGroupTransform, isModRegisted, true),
													GetVisibleIconType(meshGroupTransform, isModRegisted, false),
													isModRegisted,
													HIERARCHY_TYPE.Meshes,
													true);


				if (meshGroupTransform._meshGroup != null)
				{
					AddTransformOfMeshGroup(meshGroupTransform._meshGroup, newUnit);
				}
			}
		}


		// 본 리스트를 만든다.
		// 본은 Child MeshGroup의 본에 상위의 메시가 리깅되는 경우가 없다.
		// 다만, Transform계열 Modifier (Anim 포함)에서 Child MeshGroup의 Bone을 제어할 수 있다.
		// Root Node를 여러개두자.
		//------------------------------------------------------------------------------------

		private void AddBoneUnitsOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			if (targetMeshGroup._boneList_Root == null || targetMeshGroup._boneList_Root.Count == 0)
			{
				return;
			}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			//Root 부터 재귀적으로 호출한다.
			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			{
				AddBoneUnit(targetMeshGroup._boneList_Root[i], parentUnit,
					iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}

		}

		private void AddBoneUnit(apBone bone, apEditorHierarchyUnit parentUnit,
								Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			Texture2D icon = iconNormal;

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);
			
			//이전
			//apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			
			//if(bone.IsGUIVisible)
			//{
			//	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//}

			//변경 21.1.28
			//- 본의 보이기 타입이 많아졌다.
			apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;

			switch (bone.VisibleIconType)
			{
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Default:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Tmp:		visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Tmp:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Rule:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible; break;
			}


			apEditorHierarchyUnit addedUnit = AddUnit_ToggleButton_Visible(icon,
														bone._name,
														CATEGORY.Bone_Item,
														bone,
														false, 
														//_rootUnit_Mesh,
														parentUnit,
														visibleType,
														apEditorHierarchyUnit.VISIBLE_TYPE.None,
														isModRegisted,
														HIERARCHY_TYPE.Bones,
														false
														);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnit(bone._childBones[i], addedUnit, iconNormal, iconIKHead, iconIKChained, iconIKSingle);
			}
		}

		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			ReloadGUIContent();//<<추가

			//Pool 초기화
			if(!_unitPool.IsInitialized)
			{
				_unitPool.Init();
			}

			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			//AddUnit_ToggleButton(null, "Select Overall", CATEGORY.Overall_item, null, false, _rootUnit_Overall);


			#region [미사용 코드] 재귀적인 구조가 없는 기존 코드
			//1. 메시 들을 검색하자
			//구버전 : 단일 레벨의 Child Transform에 대해서 Refresh
			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}
			//	RefreshUnit(	CATEGORY.Mesh_Item, 
			//					iconImage, 
			//					meshTransform, 
			//					meshTransform._nickName,
			//					Editor.Select.SubMeshInGroup, 
			//					meshTransform._isVisible_Default,
			//					//_rootUnit_Mesh
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);

			////2. Mesh Group들을 검색하자
			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	RefreshUnit(	CATEGORY.MeshGroup_Item, 
			//					Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//					meshGroupTransform, 
			//					meshGroupTransform._nickName,
			//					Editor.Select.SubMeshGroupInGroup, 
			//					meshGroupTransform._isVisible_Default,
			//					//_rootUnit_MeshGroup
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms); 
			#endregion


			//신버전 : 재귀적으로 탐색 및 갱신을 한다.
			List<apTransform_Mesh> childMeshTransforms = new List<apTransform_Mesh>();
			List<apTransform_MeshGroup> childMeshGroupTransforms = new List<apTransform_MeshGroup>();

			SearchMeshGroupTransforms(Editor.Select.MeshGroup, _rootUnit_Meshes, childMeshTransforms, childMeshGroupTransforms);

			CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);
			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms);

			//본도 Refresh한다.
			//본의 경우 Child MeshGroup Transform에 속한 Bone도 있으므로, 이 ChildMeshGroup이 현재도 유효한지 체크하는 것이 중요하다

			List<apBone> resultBones = new List<apBone>();
			List<apTransform_MeshGroup> resultMeshGroupTransformWithBones = new List<apTransform_MeshGroup>();

			
			//일단 메인부터
			SearchBones(Editor.Select.MeshGroup, _rootUnit_Bones_Main, resultBones);

			//<BONE_EDIT>
			////서브는 Child MeshGroup Transform 부터 체크한다.
			//List<apTransform_MeshGroup> childMeshGroupTransformWithBones = Editor.Select.MeshGroup._childMeshGroupTransformsWithBones;

			//>> Bone Set으로 변경
			List<apMeshGroup.BoneListSet> subBoneSetList = Editor.Select.MeshGroup._boneListSets;
			List<apTransform_MeshGroup> subMeshGroups = new List<apTransform_MeshGroup>();
			if(subBoneSetList != null && subBoneSetList.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < subBoneSetList.Count; iSet++)
				{
					boneSet = subBoneSetList[iSet];
					if(boneSet._isRootMeshGroup)
					{
						continue;
					}
					subMeshGroups.Add(boneSet._meshGroupTransform);
				}
			}

			for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
			{
				apEditorHierarchyUnit subBoneRootUnit = _rootUnit_Bones_Sub[i];
				apTransform_MeshGroup mgTranform = subBoneRootUnit._savedObj as apTransform_MeshGroup;
				if (mgTranform != null && mgTranform._meshGroup != null 
					//&& childMeshGroupTransformWithBones.Contains(mgTranform)//<BONE_EDIT>
					&& subMeshGroups.Contains(mgTranform)//Bone Set에서 추출
					)
				{
					//유효하게 포함되어있는 mgTransform이네염
					resultMeshGroupTransformWithBones.Add(mgTranform);
					SearchBones(mgTranform._meshGroup, subBoneRootUnit, resultBones);
				}
			}

			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.SubName_Bone, resultMeshGroupTransformWithBones);
			CheckRemovableUnits<apBone>(deletedUnits, CATEGORY.Bone_Item, resultBones);


			for (int i = 0; i < deletedUnits.Count; i++)
			{
				//1. 먼저 All에서 없앤다.
				//2. Parent가 있는경우,  Parent에서 없애달라고 한다.
				apEditorHierarchyUnit dUnit = deletedUnits[i];
				if (dUnit._parentUnit != null)
				{
					dUnit._parentUnit._childUnits.Remove(dUnit);
				}

				_units_All.Remove(dUnit);

				//추가 20.3.18 : 그냥 삭제하면 안되고, Pool에 반납해야 한다.
				_unitPool.PushUnit(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root_Meshes.Count; i++)
			{
				SortUnit_Recv(_units_Root_Meshes[i]);
			}

			//추가 21.2.9 : Sub List는 MeshGroupTF의 순서대로 표시해야한다.
			_units_Root_Bones.Sort(delegate(apEditorHierarchyUnit a, apEditorHierarchyUnit b)
			{
				if ((CATEGORY)(a._savedKey) == CATEGORY.MainName_Bone)
				{
					return -1;
				}
				if ((CATEGORY)(b._savedKey) == CATEGORY.MainName_Bone)
				{
					return 1;
				}
				if ((CATEGORY)(a._savedKey) == CATEGORY.SubName_Bone && (CATEGORY)(b._savedKey) == CATEGORY.SubName_Bone)
				{
					//둘다 서브일 경우에 (추가 21.2.9)
					apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
					apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
					if(meshGroup_a != null && meshGroup_b != null)
					{
						return meshGroup_b._depth - meshGroup_a._depth;
					}
				}
				return 0;
			});

			for (int i = 0; i < _units_Root_Bones.Count; i++)
			{
				SortUnit_Recv_Bones(_units_Root_Bones[i]);
			}

			//추가 19.6.29 : TmpWorkVisible 확인
			_rootUnit_Meshes.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);
			_rootUnit_Bones_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);

			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
		}

		private apEditorHierarchyUnit RefreshUnit(CATEGORY category,
													Texture2D iconImage,
													object obj, string objName, 
													object selectedObj, 
													apSelection.FUNC_IS_SUB_SELECTED funcIsSubSelected,//추가 20.5.28 : 다중 선택도 체크한다.
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
													bool isModRegistered,
													bool isAvailable,
													apEditorHierarchyUnit parentUnit,
													HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit unit = _units_All.Find(delegate (apEditorHierarchyUnit a)
			{
				if (obj != null)
				{
					return (CATEGORY)a._savedKey == category && a._savedObj == obj;
				}
				else
				{
					return (CATEGORY)a._savedKey == category;
				}
			});

			if (objName == null)
			{
				objName = "";
			}

			if (unit != null)
			{
				if (selectedObj != null && unit._savedObj == selectedObj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true, true);
				}
				else
				{
					//unit._isSelected = false;

					//추가 20.5.28 : 다중 선택도 허용한다.
					if(funcIsSubSelected != null)
					{
						apSelection.SUB_SELECTED_RESULT selectedResult = funcIsSubSelected(unit._savedObj);
						if(selectedResult == apSelection.SUB_SELECTED_RESULT.Main)
						{
							unit.SetSelected(true, true);
						}
						else if(selectedResult == apSelection.SUB_SELECTED_RESULT.Added)
						{
							unit.SetSelected(true, false);
						}
						else
						{
							unit.SetSelected(false, true);
						}
					}
					else
					{
						unit.SetSelected(false, true);
					}
					
				}

				unit.SetModRegistered(isModRegistered);
				unit.SetAvailable(isAvailable);

				unit._visibleType_Prefix = visibleType_Prefix;
				unit._visibleType_Postfix = visibleType_Postfix;

				//수정 1.1 : 버그
				//if(unit._text == null)
				//{
				//	unit._text = "";
				//}

				int nextLevel = (parentUnit == null) ? 0 : (parentUnit._level + 1);

				if (!unit._text.Equals(objName) || unit._level != nextLevel)
				{
					unit._level = nextLevel;
					unit.ChangeText(objName);
				}

				if (unit._icon != iconImage)
				{
					unit.ChangeIcon(iconImage);
				}
			}
			else
			{

				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Meshes, true);
				}
				else
				{
					//unit = AddUnit_ToggleButton(iconImage, objName, category, obj, false, isModRegistered, parentUnit, HIERARCHY_TYPE.Bones);
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Bones, false);
					
				}
				if (selectedObj == obj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true, true);
				}
				unit.SetAvailable(isAvailable);
			}
			return unit;
		}


		private void CheckRemovableUnits<T>(List<apEditorHierarchyUnit> deletedUnits, CATEGORY category, List<T> objList)
		{
			List<apEditorHierarchyUnit> deletedUnits_Sub = _units_All.FindAll(delegate (apEditorHierarchyUnit a)
			{
				if ((CATEGORY)a._savedKey == category)
				{
					if (a._savedObj == null || !(a._savedObj is T))
					{
						return true;
					}

					T savedData = (T)a._savedObj;
					if (!objList.Contains(savedData))
					{
					//리스트에 없는 경우 (무효한 경우)
					return true;
					}
				}
				return false;
			});
			for (int i = 0; i < deletedUnits_Sub.Count; i++)
			{
				deletedUnits.Add(deletedUnits_Sub[i]);
			}
		}

		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Transform인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentUnit"></param>
		/// <param name="resultMeshTransforms"></param>
		/// <param name="resultMeshGroupTransforms"></param>
		private void SearchMeshGroupTransforms(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apTransform_Mesh> resultMeshTransforms, List<apTransform_MeshGroup> resultMeshGroupTransforms)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegistered = false;
			apModifierBase modifier = Editor.Select.Modifier;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];

				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
				}

				resultMeshTransforms.Add(meshTransform);

				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshTransform);
				}


				RefreshUnit(CATEGORY.Mesh_Item,
								iconImage,
								meshTransform,
								meshTransform._nickName,
								//Editor.Select.SubMeshInGroup, 
								Editor.Select.MeshTF_Main,
								Editor.Select.IsSubSelected,
								//meshTransform._isVisible_Default,
								GetVisibleIconType(meshTransform, isModRegistered, true),
								GetVisibleIconType(meshTransform, isModRegistered, false),
								isModRegistered,
								true,
								//_rootUnit_Mesh
								parentUnit,
								HIERARCHY_TYPE.Meshes
								);
			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				resultMeshGroupTransforms.Add(meshGroupTransform);


				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit existUnit = RefreshUnit(CATEGORY.MeshGroup_Item,
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
													meshGroupTransform,
													meshGroupTransform._nickName,
													//Editor.Select.SubMeshGroupInGroup, 
													Editor.Select.MeshGroupTF_Main,
													Editor.Select.IsSubSelected,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(meshGroupTransform, isModRegistered, false),
													isModRegistered,
													true,
													//_rootUnit_MeshGroup
													parentUnit,
													HIERARCHY_TYPE.Meshes

													);

				if (meshGroupTransform._meshGroup != null)
				{
					SearchMeshGroupTransforms(meshGroupTransform._meshGroup, existUnit, resultMeshTransforms, resultMeshGroupTransforms);
				}
			}
		}







		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Bone인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		private void SearchBones(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apBone> resultBones)
		{


			List<apBone> rootBones = targetMeshGroup._boneList_Root;

			if (rootBones.Count == 0)
			{
				return;
			}

			//if(Editor.Select.Bone == null)
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [ Not Selected ]");
			//}
			//else
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [" + Editor.Select.Bone._name + "]");
			//}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			for (int i = 0; i < rootBones.Count; i++)
			{
				apBone rootBone = rootBones[i];

				SearchAndRefreshBone(rootBone, parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}
		}

		private void SearchAndRefreshBone(apBone bone, apEditorHierarchyUnit parentUnit, List<apBone> resultBones,
			Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			resultBones.Add(bone);

			Texture2D icon = iconNormal;
			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);
			bool isAvailable = IsBoneAvailable(bone);

			//이전
			//apEditorHierarchyUnit.VISIBLE_TYPE boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			//if(bone.IsGUIVisible)
			//{
			//	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//}

			//변경 21.1.28
			//- 본의 보이기 타입이 많아졌다.
			apEditorHierarchyUnit.VISIBLE_TYPE boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;

			switch (bone.VisibleIconType)
			{
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Default:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Tmp:		boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Tmp:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Rule:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible; break;
			}

			apEditorHierarchyUnit curUnit = RefreshUnit(CATEGORY.Bone_Item,
															icon,
															bone,
															bone._name,
															//Editor.Select.SubMeshInGroup, 
															Editor.Select.Bone,
															Editor.Select.IsSubSelected,
															//true,
															boneVisible,
															apEditorHierarchyUnit.VISIBLE_TYPE.None,
															isModRegisted,
															isAvailable,
															//_rootUnit_Mesh
															parentUnit,
															HIERARCHY_TYPE.Bones
															);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				SearchAndRefreshBone(bone._childBones[i], curUnit, resultBones, iconNormal,
					iconIKHead, iconIKChained, iconIKSingle);
			}
		}









		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
					{
						return -1;
					}

					int depthA = -1;
					int depthB = -1;
					int indexPerParentA = a._indexPerParent;
					int indexPerParentB = b._indexPerParent;

					if (a._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
						depthA = meshGroup_a._depth;
					}
					else if (a._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
						depthA = mesh_a._depth;
					}
					

					if (b._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
						depthB = meshGroup_b._depth;
					}
					else if (b._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;
						depthB = mesh_b._depth;
					}
					

					if (depthA == depthB)
					{
						return indexPerParentA - indexPerParentB;
					}

					return depthB - depthA;

				#region [미사용 코드]

				//if(a._savedKey == b._savedKey)
				//{	
				//	if(a._savedObj is apTransform_MeshGroup && b._savedObj is apTransform_MeshGroup)
				//	{
				//		apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
				//		apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;

				//		if(Mathf.Abs(meshGroup_b._depth - meshGroup_a._depth) < 0.0001f)
				//		{
				//			return a._indexPerParent - b._indexPerParent;
				//		}
				//		return (int)((meshGroup_b._depth - meshGroup_a._depth) * 1000.0f);
				//	}
				//	else if(a._savedObj is apTransform_Mesh && b._savedObj is apTransform_Mesh)
				//	{
				//		apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
				//		apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;

				//		//Clip인 경우
				//		//서로 같은 Parent를 가지는 Child인 경우 -> Index의 역순
				//		//하나가 Parent인 경우 -> Parent가 아래쪽으로
				//		//그 외에는 Depth 비교
				//		if (mesh_a._isClipping_Child && mesh_b._isClipping_Child &&
				//			mesh_a._clipParentMeshTransform == mesh_b._clipParentMeshTransform)
				//		{
				//			return (mesh_b._clipIndexFromParent - mesh_a._clipIndexFromParent);
				//		}
				//		else if (	mesh_a._isClipping_Child && mesh_b._isClipping_Parent &&
				//					mesh_a._clipParentMeshTransform == mesh_b)
				//		{
				//			//b가 Parent -> b가 뒤로 가야함
				//			return -1;
				//		}
				//		else if (	mesh_b._isClipping_Child && mesh_a._isClipping_Parent &&
				//					mesh_b._clipParentMeshTransform == mesh_a)
				//		{
				//			//a가 Parent -> a가 뒤로 가야함
				//			return 1;
				//		}
				//		else
				//		{
				//			if (Mathf.Abs(mesh_b._depth - mesh_a._depth) < 0.0001f)
				//			{
				//				return a._indexPerParent - b._indexPerParent;
				//			}
				//			return (int)((mesh_b._depth - mesh_a._depth) * 1000.0f);
				//		}
				//	}
				//	return a._indexPerParent - b._indexPerParent;
				//}
				//return a._savedKey - b._savedKey; 
				#endregion
			});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv(unit._childUnits[i]);
				}
			}
		}


		private void SortUnit_Recv_Bones(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName_Bone)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName_Bone)
					{
						return -1;
					}

					if ((CATEGORY)(a._savedKey) == CATEGORY.SubName_Bone && (CATEGORY)(b._savedKey) == CATEGORY.SubName_Bone)
					{
						//둘다 서브일 경우에 (추가 21.2.9)
						apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
						apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
						if(meshGroup_a != null && meshGroup_b != null)
						{
							return meshGroup_b._depth - meshGroup_a._depth;
						}
					}


					int depthA = -1;
					int depthB = -1;

					if(a._savedObj is apBone)
					{
						apBone bone_a = a._savedObj as apBone;
						depthA = bone_a._depth;
					}

					if(b._savedObj is apBone)
					{
						apBone bone_b = b._savedObj as apBone;
						depthB = bone_b._depth;
					}

					if (depthA == depthB)
					{
						//그 외에는 그냥 문자열 순서로 매기자
						int compare = string.Compare(a._text.ToString(), b._text.ToString());
						if (compare == 0)
						{
							return a._indexPerParent - b._indexPerParent;
						}
						return compare;
					}

					return depthB - depthA;
				});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv_Bones(unit._childUnits[i]);
				}
			}
		}



		// 갱신된 유닛들을 "선형"으로 정리한다. (Shift로 여러개 선택할 수 있게)
		//-----------------------------------------------------------------------------------------
		//추가 20.7.4 : GUI 출력 순서에 맞게 "재귀 Index"와 다른 "선형 Index"를 설정한다.
		//GUI 출력 함수를 참고하자
		public void RefreshLinearIndices()
		{
			//Mesh 따로, Bone 따로
			int curIndex = 0;
			for (int i = 0; i < _units_Root_Meshes.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Meshes[i], curIndex);
			}

			curIndex = 0;
			for (int i = 0; i < _units_Root_Bones.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Bones[i], curIndex);
			}
		}

		private int SetLinearIndexRecursive(apEditorHierarchyUnit unit, int curIndex)
		{	
			unit.SetLinearIndex(curIndex);
			curIndex++;//적용 후 1 증가

			if(unit._childUnits == null || unit._childUnits.Count == 0)
			{
				return curIndex;
			}

			for (int i = 0; i < unit._childUnits.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(unit._childUnits[i], curIndex);
			}

			return curIndex;
		}

		//추가 20.7.4 : Shift키를 눌렀을 때, 이전 유닛 ~ 현재 클릭한 유닛 사이의 유닛들을 클릭할 수 있게 만들자
		private bool GetShiftClickedUnits(apEditorHierarchyUnit clickedUnit, CATEGORY category)
		{
			if(_shiftClickedUnits == null)
			{
				_shiftClickedUnits = new List<apEditorHierarchyUnit>();
			}
			_shiftClickedUnits.Clear();
			
			//이전에 클릭한게 없거나 카테고리가 다르면
			//현재 Main으로 선택된게 있나 찾는다.
			bool isNeedToFindSelectedMainUnit = false;
			if(_lastClickedUnit == null || !_units_All.Contains(_lastClickedUnit))
			{
				//클릭한게 없다.
				isNeedToFindSelectedMainUnit = true;
			}
			else if(category != (CATEGORY)_lastClickedUnit._savedKey)
			{
				//카테고리가 다르다.
				isNeedToFindSelectedMainUnit = true;
			}
			//if(!_lastClickedUnit.IsSelected_Main && !_lastClickedUnit.IsSelected_Sub)
			//{
			//	//클릭한게 선택된게 아니다.
			//	isNeedToFindSelectedMainUnit = true;
			//}

			apEditorHierarchyUnit startUnit = null;

			if(isNeedToFindSelectedMainUnit)
			{
				//클릭한게 없으므로 선택된 유닛을 찾자. 못찾을수도 있다.
				List<apEditorHierarchyUnit> prevSelectedUnits = null;
				
				if(category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
				{
					prevSelectedUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
					{
						return (a.IsSelected_Main || a.IsSelected_Sub)
							&& ((CATEGORY)a._savedKey == CATEGORY.Mesh_Item || (CATEGORY)a._savedKey == CATEGORY.MeshGroup_Item);
					});
				}
				else if(category == CATEGORY.Bone_Item)
				{
					prevSelectedUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
					{
						return (a.IsSelected_Main || a.IsSelected_Sub)
							&& (CATEGORY)a._savedKey == CATEGORY.Bone_Item;
					});
				}

				if(prevSelectedUnits != null && prevSelectedUnits.Count == 1)
				{
					//선택된 유닛이 한개만 있을때
					startUnit = prevSelectedUnits[0];
				}
			}
			else
			{
				//이전에 클릭했던게 영역 시작이다.
				startUnit = _lastClickedUnit;
			}

			if(startUnit == null)
			{
				//실패..
				return false;
			}

			if(clickedUnit == startUnit)
			{
				//같은걸 선택했다면
				return false;
			}

			int prevIndex = startUnit.LinearIndex;
			int nextIndex = clickedUnit.LinearIndex;

			//그 사이에 있는 걸 찾자
			//이미 선택된 것과 클릭한건 따로 처리될 것이므로 제외
			//선택 여부는 클릭한 유닛의 반대 값이어야 한다.
			bool isGetUnselected = !clickedUnit.IsSelected_Main && !clickedUnit.IsSelected_Sub; //선택되지 않은걸 목표로 클릭했으면 다같이 선택하는 것으로만)


			int minIndex = Mathf.Min(prevIndex, nextIndex);
			int maxIndex = Mathf.Max(prevIndex, nextIndex);

			List<apEditorHierarchyUnit> selectableUnits = null;
			if(category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				selectableUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
				{
					return (a.LinearIndex > minIndex)
						&& (a.LinearIndex < maxIndex)
						&& (
							(isGetUnselected && (!a.IsSelected_Main && !a.IsSelected_Sub))//선택되지 않은 것만 선택하거나
							|| (!isGetUnselected && (a.IsSelected_Main || a.IsSelected_Sub))//선택된 것만 선택해서 해제하자
							)
						&& ((CATEGORY)a._savedKey == CATEGORY.Mesh_Item || (CATEGORY)a._savedKey == CATEGORY.MeshGroup_Item);
				});
			}
			else if(category == CATEGORY.Bone_Item)
			{
				selectableUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
				{
					return (a.LinearIndex > minIndex)
						&& (a.LinearIndex < maxIndex)
						&& (
							(isGetUnselected && (!a.IsSelected_Main && !a.IsSelected_Sub))//선택되지 않은 것만 선택하거나
							|| (!isGetUnselected && (a.IsSelected_Main || a.IsSelected_Sub))//선택된 것만 선택해서 해제하자
							)
						&& (CATEGORY)a._savedKey == CATEGORY.Bone_Item;
				});
			}

			if(selectableUnits == null || selectableUnits.Count == 0)
			{
				//선택될게 없다면
				return false;
			}

			//결과 복사
			//일단, startUnit의 선택 여부가 타겟의 선택 여부와 다르다면 포함시키자
			if((isGetUnselected && (!startUnit.IsSelected_Main && !startUnit.IsSelected_Sub))
				|| (!isGetUnselected && (startUnit.IsSelected_Main || startUnit.IsSelected_Sub))
				)
			{
				_shiftClickedUnits.Add(startUnit);
			}
			for (int i = 0; i < selectableUnits.Count; i++)
			{
				_shiftClickedUnits.Add(selectableUnits[i]);
			}

			return _shiftClickedUnits.Count > 0;
		}


		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift)
		{
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//이전 : 선택한것만 찾아서 나머지 비활성하면 되는 것이었다.
			//apEditorHierarchyUnit selectedUnit = null;

			//변경 20.5.28 : 변경된 내역이 있다면 모두 다시 검토해봐야 한다. (다중 선택 때문에)
			bool isAnyChanged = false;

			//현재 모디파이어가 선택되어 있는가
			//> 리깅 모디파이어가 선택된 상태가 아니라면, Mesh/MeshGroup Transform 선택시 Bone을 null로, 또는 그 반대로 설정해야한다.
			//(리깅 모디파이어는 둘다 선택된 상태에서 작업하기 때문)
			//물리 모디파이어는 다중 선택이 안된다.
			bool isRiggingModifier = false;
			bool isPhysicModifier = false;

			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					isRiggingModifier = true;
				}
				else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					isPhysicModifier = true;
				}
			}

			
			CATEGORY category = (CATEGORY)savedKey;
			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						apTransform_Mesh meshTransform = savedObj as apTransform_Mesh;
						if (meshTransform != null)
						{
							if(isRiggingModifier)
							{
								//리깅 모디파이어인 경우
								//- 다중 선택 불가.
								//- 메시 1개, 본 1개 따로 선택 가능
								Editor.Select.SetSubMeshInGroup(meshTransform, apSelection.MULTI_SELECT.Main);
							}
							else if(isPhysicModifier)
							{
								//물리 모디파이어인 경우
								//- 다중 선택 불가
								//- TF/Bone 중에서 하나만 선택 가능
								Editor.Select.SetSubObjectInGroup(meshTransform, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							else
							{
								//그외
								//- 다중 선택 가능
								//- TF/Bone 중에서 하나만 선택 가능
								
								//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
								if(isShift)
								{
									if(GetShiftClickedUnits(eventUnit, category))
									{
										apEditorHierarchyUnit shiftUnit = null;
										apTransform_Mesh shiftMeshTF = null;
										apTransform_MeshGroup shiftMeshGroupTF = null;

										for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
										{
											//사이의 것을 추가한다.
											shiftUnit = _shiftClickedUnits[iShiftUnit];
											
											if((CATEGORY)shiftUnit._savedKey == CATEGORY.Mesh_Item)
											{
												shiftMeshTF = shiftUnit._savedObj as apTransform_Mesh;
												if(shiftMeshTF != null)
												{
													Editor.Select.SetSubObjectInGroup(	shiftMeshTF, null, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
											else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
											{
												shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
												if(shiftMeshGroupTF != null)
												{
													Editor.Select.SetSubObjectInGroup(	null, shiftMeshGroupTF, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
										}

										_shiftClickedUnits.Clear();

									}	
								}	

								//클릭한걸 추가하자
								Editor.Select.SetSubObjectInGroup(meshTransform, null, null, 
																	((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																	apSelection.TF_BONE_SELECT.Exclusive);

								
							}
							
							//이전 : 선택된게 1개이다.
							//if (Editor.Select.SubMeshInGroup == meshTransform)
							//{
							//	selectedUnit = eventUnit;
							//	isAnyChanged = true;
							//}

							//변경 : 무조건 전체 리셋
							isAnyChanged = true;
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apTransform_MeshGroup meshGroupTransform = savedObj as apTransform_MeshGroup;
						if (meshGroupTransform != null)
						{
							if(isRiggingModifier)
							{
								//리깅 모디파이어인 경우
								//- 다중 선택 불가.
								//- 메시 1개, 본 1개 따로 선택 가능
								Editor.Select.SetSubMeshGroupInGroup(meshGroupTransform, apSelection.MULTI_SELECT.Main);
							}
							else if(isPhysicModifier)
							{
								//물리 모디파이어인 경우
								//- 다중 선택 불가
								//- TF/Bone 중에서 하나만 선택 가능
								Editor.Select.SetSubObjectInGroup(null, meshGroupTransform, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							else
							{
								//그외
								//- 다중 선택 가능
								//- TF/Bone 중에서 하나만 선택 가능

								//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
								if(isShift)
								{
									if(GetShiftClickedUnits(eventUnit, category))
									{
										apEditorHierarchyUnit shiftUnit = null;
										apTransform_Mesh shiftMeshTF = null;
										apTransform_MeshGroup shiftMeshGroupTF = null;

										for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
										{
											//사이의 것을 추가한다.
											shiftUnit = _shiftClickedUnits[iShiftUnit];
											
											if((CATEGORY)shiftUnit._savedKey == CATEGORY.Mesh_Item)
											{
												shiftMeshTF = shiftUnit._savedObj as apTransform_Mesh;
												if(shiftMeshTF != null)
												{
													Editor.Select.SetSubObjectInGroup(	shiftMeshTF, null, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
											else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
											{
												shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
												if(shiftMeshGroupTF != null)
												{
													Editor.Select.SetSubObjectInGroup(	null, shiftMeshGroupTF, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
										}

										_shiftClickedUnits.Clear();
									}	
								}	


								Editor.Select.SetSubObjectInGroup(null, meshGroupTransform, null, 
																	((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																	apSelection.TF_BONE_SELECT.Exclusive);
							}

							
							//이전
							//if (Editor.Select.SubMeshGroupInGroup == meshGroupTransform)
							//{
							//	selectedUnit = eventUnit;
							//	isAnyChanged = true;
							//}

							//변경
							isAnyChanged = true;
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						if (eventUnit.IsAvailable)//추가 : Bone의 경우 선택이 불가능한 경우가 있다.
						{
							apBone bone = savedObj as apBone;
							if (bone != null)
							{
								if(isRiggingModifier)
								{
									//리깅 모디파이어인 경우
									//- 다중 선택 불가.
									//- 메시 1개, 본 1개 따로 선택 가능
									Editor.Select.SetBone(bone, apSelection.MULTI_SELECT.Main);
								}
								else if(isPhysicModifier)
								{
									//물리 모디파이어인 경우
									//- 다중 선택 불가
									//- TF/Bone 중에서 하나만 선택 가능
									Editor.Select.SetSubObjectInGroup(null, null, bone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
								}
								else
								{
									//그외
									//- 다중 선택 가능
									//- TF/Bone 중에서 하나만 선택 가능

									//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
									if (isShift)
									{
										if (GetShiftClickedUnits(eventUnit, category))
										{
											apEditorHierarchyUnit shiftUnit = null;
											apBone shiftBone = null;

											for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
											{
												//사이의 것을 추가한다.
												shiftUnit = _shiftClickedUnits[iShiftUnit];

												if ((CATEGORY)shiftUnit._savedKey == CATEGORY.Bone_Item)
												{
													shiftBone = shiftUnit._savedObj as apBone;
													if (shiftBone != null)
													{
														Editor.Select.SetSubObjectInGroup(null, null, shiftBone,
																							apSelection.MULTI_SELECT.AddOrSubtract,
																							apSelection.TF_BONE_SELECT.Exclusive);
													}
												}
											}
											_shiftClickedUnits.Clear();
										}
									}	

									Editor.Select.SetSubObjectInGroup(null, null, bone, 
																		((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																		apSelection.TF_BONE_SELECT.Exclusive);
								}
								
								//이전
								//if (Editor.Select.Bone == bone)
								//{
								//	selectedUnit = eventUnit;
								//	isAnyChanged = true;
								//}

								//변경
								isAnyChanged = true;
							}
						}
					}
					break;
			}

			if (isAnyChanged)
			{
				//이전
				//if (selectedUnit != null)
				//{
				//	for (int i = 0; i < _units_All.Count; i++)
				//	{
				//		if (_units_All[i] == selectedUnit)
				//		{
				//			//_units_All[i]._isSelected = true;
				//			_units_All[i].SetSelected(true, true);
				//		}
				//		else
				//		{
				//			//_units_All[i]._isSelected = false;
				//			_units_All[i].SetSelected(false, true);
				//		}
				//	}
				//}
				//else
				//{
				//	for (int i = 0; i < _units_All.Count; i++)
				//	{
				//		//_units_All[i]._isSelected = false;
				//		_units_All[i].SetSelected(false, true);
				//	}
				//}


				//변경 20.5.28 : 전체 갱신
				apEditorHierarchyUnit curUnit = null;
				apSelection.SUB_SELECTED_RESULT selectResult = apSelection.SUB_SELECTED_RESULT.None;

				for (int i = 0; i < _units_All.Count; i++)
				{
					curUnit = _units_All[i];
					
					selectResult = apSelection.SUB_SELECTED_RESULT.None;
					
					switch ((CATEGORY)curUnit._savedKey)
					{
						case CATEGORY.Mesh_Item:
						case CATEGORY.MeshGroup_Item:
						case CATEGORY.Bone_Item:
							{
								selectResult = Editor.Select.IsSubSelected(curUnit._savedObj);
							}
							break;
					}

					if(selectResult == apSelection.SUB_SELECTED_RESULT.Main)
					{
						curUnit.SetSelected(true, true);
					}
					else if(selectResult == apSelection.SUB_SELECTED_RESULT.Added)
					{
						curUnit.SetSelected(true, false);
					}
					else
					{
						curUnit.SetSelected(false, true);
					}
				}

				//추가 20.7.4 : 클릭한 유닛은 여기서 체크한다.
				_lastClickedUnit = eventUnit;

				//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
				Editor.Gizmos.OnSelectedObjectsChanged();
			}
		}


		public void OnUnitVisibleClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPrefixButton)
		{
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}


			CATEGORY category = (CATEGORY)savedKey;

			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;

			bool isMeshTransform = false;

			bool isVisibleDefault = false;

			bool isCtrl = false;
			if(Event.current != null)
			{
#if UNITY_EDITOR_OSX
				isCtrl = Event.current.command;
#else
				isCtrl = Event.current.control;
#endif		
			}
			
			
			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						meshTransform = savedObj as apTransform_Mesh;
						isVisibleDefault = meshTransform._isVisible_Default;
						isMeshTransform = true;
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						meshGroupTransform = savedObj as apTransform_MeshGroup;
						isVisibleDefault = meshGroupTransform._isVisible_Default;
						isMeshTransform = false;
					}
					break;

				case CATEGORY.Bone_Item:
					{
						bone = savedObj as apBone;
						isVisibleDefault = bone.IsVisibleInGUI;
					}
					break;
				default:
					return;
			}
			if (meshTransform == null && meshGroupTransform == null && bone == null)
			{
				//?? 뭘 선택했나염..
				return;
			}


			if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				if (isPrefixButton)
				{
					//Prefix : TmpWorkVisible을 토글한다.
					apRenderUnit linkedRenderUnit = null;

					if (meshTransform != null)
					{
						linkedRenderUnit = meshTransform._linkedRenderUnit;
					}
					else if (meshGroupTransform != null)
					{
						linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
					}

					if (linkedRenderUnit != null)
					{
						bool isTmpWorkToShow = false;
						//Editor.Select.MeshGroup.
						if (linkedRenderUnit._isVisible_WithoutParent == linkedRenderUnit._isVisibleCalculated)
						{

							//TmpWork가 꺼져있다. (실제 Visible과 값이 같다)
							if (linkedRenderUnit._isVisible_WithoutParent)
							{
								//Debug.Log("Visible Type 1");

								//Show -> Hide
								//이전
								//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = true;
								//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

								//변경 21.1.28 : Show(None) -> Hide
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;

								isTmpWorkToShow = false;
							}
							else
							{
								//Debug.Log("Visible Type 2");

								//Hide -> Show
								//이전
								//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
								//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = true;

								//변경 21.1.28 : Hide(None) -> Show
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;

								isTmpWorkToShow = true;
							}
						}
						else
						{
							//TmpWork가 켜져있다. 꺼야한다. (같은 값으로 바꾼다)
							//이전
							//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
							//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

							//Debug.Log("Visible Type 3 : " + linkedRenderUnit._workVisible_Rule);

							//변경 21.1.28 : Show/Hide -> None
							if(linkedRenderUnit._workVisible_Rule == apRenderUnit.WORK_VISIBLE_TYPE.None)
							{
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.None;
							}
							else
							{
								//만약 겉으로 보기엔 다르다 > 같게 만들어야 하는데, Rule이 None이 아니다.
								//계산값과 같게 만들자
								if(linkedRenderUnit._isVisibleCalculated)
								{
									linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
								}
								else
								{
									linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
								}
							}
							

							isTmpWorkToShow = !linkedRenderUnit._isVisible_WithoutParent;
						}

						if (isCtrl)
						{
							//Ctrl을 눌렀으면 반대로 행동
							//Debug.Log("TmpWork를 다른 RenderUnit에 반대로 적용 : Show : " + !isTmpWorkToShow);
							Editor.Controller.SetMeshGroupTmpWorkVisibleAll(Editor.Select.MeshGroup, !isTmpWorkToShow, linkedRenderUnit);
						}
						else
						{
							//Render Unit의 Visibility를 저장하자
							Editor.VisiblityController.Save_RenderUnit(Editor.Select.MeshGroup, linkedRenderUnit);
						}
					}

					Editor.Controller.CheckTmpWorkVisible(Editor.Select.MeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출

					//그냥 Refresh
					Editor.Select.MeshGroup.RefreshForce();

					Editor.RefreshControllerAndHierarchy(false);
				}
				else
				{
					//Postfix :
					//Setting : isVisibleDefault를 토글한다.
					//Modifier 선택중 + ParamSetGroup을 선택하고 있다. : ModMesh의 isVisible을 변경한다.
					bool isModVisibleSetting = Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
													&& Editor.Select.Modifier != null
													&& Editor.Select.SubEditedParamSetGroup != null
													&& Editor.Select.ParamSetOfMod != null;


					if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
					{
						//Setting 탭에서는 Default Visible을 토글한다.
						apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, Editor.Select.MeshGroup, savedObj, false, true);

						if (meshTransform != null)
						{
							meshTransform._isVisible_Default = !meshTransform._isVisible_Default;
						}
						else if (meshGroupTransform != null)
						{
							meshGroupTransform._isVisible_Default = !meshGroupTransform._isVisible_Default;
						}

						Editor.Select.MeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy(false);
					}
					else if (isModVisibleSetting)
					{

						//Mod Visible을 조절한다.
						//가능한지 여부 체크
						if ((int)(Editor.Select.Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0 &&
								Editor.Select.Modifier._isColorPropertyEnabled)
						{

							//apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor.Select.MeshGroup, savedObj, false, Editor);

							//MeshGroup이 아닌 Modifier를 저장할것
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor, Editor.Select.Modifier, null, false);

							//색상을 지원한다면
							//현재 객체를 선택하고,
							//ModMesh가 있는지 확인
							//없으면 -> 추가 후 받아온다.
							//ModMesh의 isVisible 값을 지정한다.

							apModifiedMesh targetModMesh = null;


							if (meshTransform != null)
							{
								Editor.Select.SetSubMeshInGroup(meshTransform, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								Editor.Select.SetSubMeshGroupInGroup(meshGroupTransform, apSelection.MULTI_SELECT.Main);
							}

							

							Editor.Select.AutoSelectModMeshOrModBone();

							targetModMesh = Editor.Select.ModMesh_Main;

							if (targetModMesh == null)
							{
								//ModMesh가 등록이 안되어있다면
								//추가를 시도한다.

								Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

								//여기서 주의 : "현재 ParamSet"의 ModMesh가 아니라 "모든 ParamSet"의 ModMesh에 대해서 처리를 해야한다.
								List<apModifierParamSet> paramSets = Editor.Select.SubEditedParamSetGroup._paramSetList;
								for (int i = 0; i < paramSets.Count; i++)
								{
									apModifierParamSet paramSet = paramSets[i];
									apModifiedMesh addedMesh = paramSet._meshData.Find(delegate (apModifiedMesh a)
									{
										if (isMeshTransform)
										{
											return a._transform_Mesh == meshTransform;
										}
										else
										{
											return a._transform_MeshGroup == meshGroupTransform;
										}
									});
									if (addedMesh != null)
									{
										addedMesh._isVisible = !isVisibleDefault;
									}
								}

								targetModMesh = Editor.Select.ModMesh_Main;
								if (targetModMesh == null)
								{
									return;
								}
								targetModMesh._isVisible = !isVisibleDefault;

							}
							else
							{
								//Visible을 변경한다.
								targetModMesh._isVisible = !targetModMesh._isVisible;
							}

							Editor.Select.MeshGroup.RefreshForce();
							Editor.RefreshControllerAndHierarchy(false);


						}
					}
				}
			}
			else if(category == CATEGORY.Bone_Item)
			{
				if(isPrefixButton)
				{
					//Bone의 GUI Visible을 토글한다.
					if(isCtrl)
					{
						Editor.Select.MeshGroup.SetBoneGUIVisibleAll(isVisibleDefault, bone);

						//이 함수가 추가되면 일괄적으로 저장을 해야한다.
						Editor.VisiblityController.Save_AllBones(Editor.Select.MeshGroup);
					}
					else
					{
						//bone.SetGUIVisible(!isVisibleDefault);//이전
						bone.SetGUIVisible_Tmp_ByCheckRule(!isVisibleDefault);

						//Visible Controller에도 반영해야 한다. (20.4.13)
						Editor.VisiblityController.Save_Bone(Editor.Select.MeshGroup, bone);
					}
					
					Editor.Controller.CheckTmpWorkVisible(Editor.Select.MeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출
					Editor.RefreshControllerAndHierarchy(false);
				}
			}
		}


		//추가 19.6.29 : RestoreTmpWork 이벤트
		public void OnUnitClickRestoreTmpWork_Mesh()
		{
			if(Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(Editor.Select.MeshGroup, true, true, false);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	Editor.Select.MeshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.RenderUnits);
			Editor.RefreshControllerAndHierarchy(true);
		}

		public void OnUnitClickRestoreTmpWork_Bone()
		{
			if(Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(Editor.Select.MeshGroup, true, false, true);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	Editor.Select.MeshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.Bones);
			Editor.RefreshControllerAndHierarchy(true);
		}

		//------------------------------------------------------------------------------------------------------------
		// Modifier에 등록되었는지 체크
		//------------------------------------------------------------------------------------------------------------
		private bool IsModRegistered(apTransform_Mesh meshTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshTransformContain(meshTransform);
				}
			}

			return false;
		}



		private bool IsModRegistered(apTransform_MeshGroup meshGroupTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshGroupTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshGroupTransformContain(meshGroupTransform);
				}
			}

			return false;
		}


		private bool IsModRegistered(apBone bone)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(bone);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsBoneContain(bone);
				}
			}

			return false;
		}

		/// <summary>
		/// Hierarchy에 포함될 Bone이 선택 가능한가.
		/// MeshGroup 메뉴 -> Bone 탭에서는 Sub MeshGroup의 Bone은 선택될 수 없다.
		/// </summary>
		/// <param name="bone"></param>
		/// <returns></returns>
		private bool IsBoneAvailable(apBone bone)
		{
			if(bone == null)
			{
				return false;
			}

			if(Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if(Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone)
				{
					if(bone._meshGroup != Editor.Select.MeshGroup)
					{
						//조건 충족. 선택 불가이다.
						return false;
					}
				}
			}
			return true;
		}


		private apEditorHierarchyUnit.VISIBLE_TYPE GetVisibleIconType(object targetObject, bool isModRegistered, bool isPrefix)
		{

			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
			}
			else
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}


			if (linkedRenderUnit == null)
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}


			if (isPrefix)
			{
				//Prefix는
				//1) RenderUnit의 현재 렌더링 상태 -> Current
				//2) MeshTransform/MeshGroupTransform의 
				bool isVisible = linkedRenderUnit._isVisible_WithoutParent;//Visible이 아닌 VisibleParent를 출력한다.
																		   //TmpWork에 의해서 Visible 값이 바뀌는가)
																		   //Calculate != WOParent 인 경우 (TmpWork의 영향을 받았다)
				if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
				{
					//이전
					//if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
					//else				{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }

					//변경 21.1.29 : Tmp와 Rule이 따로 있다.
					if(linkedRenderUnit._workVisible_Tmp != linkedRenderUnit._workVisible_Rule)
					{
						//Tmp와 다르다면, Tmp의 값을 따른다. 단, None인 경우 빼고
						switch (linkedRenderUnit._workVisible_Tmp)
						{
							case apRenderUnit.WORK_VISIBLE_TYPE.None: return isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
							case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
							case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
						}
					}
					else
					{
						//Tmp와 같다면, Rule을 따른다.
						return isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
					}
				}
				else
				{
					//TmpWork의 영향을 받지 않았다.
					if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
					else			{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
				}
			}
			else
			{
				//PostFix는
				//1) MeshGroup Setting에서는 Default를 표시하고,
				//2) Modifier/AnimClip 상태에서 ModMesh가 발견 되었을때 ModKey 상태를 출력한다.
				//아무것도 아닐때는 None 리턴
				if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
				{
					if (meshTransform != null)
					{
						if (meshTransform._isVisible_Default)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else									{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
					else if (meshGroupTransform != null)
					{
						if (meshGroupTransform._isVisible_Default)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else										{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
				}
				else if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
				{
					if (_editor.Select.Modifier != null && _editor.Select.SubEditedParamSetGroup != null)
					{
						//Modifier가 Color를 지원하는 경우
						if (_editor.Select.Modifier._isColorPropertyEnabled
							&& (int)(_editor.Select.Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0
							&& _editor.Select.SubEditedParamSetGroup._isColorPropertyEnabled)
						{
							if (_editor.Select.ParamSetOfMod != null)
							{
								if (isModRegistered)
								{
									apModifiedMesh modMesh = null;
									if (meshTransform != null)
									{
										modMesh = _editor.Select.ParamSetOfMod._meshData.Find(delegate (apModifiedMesh a)
										{
											return a._transform_Mesh == meshTransform;
										});
									}
									else if(meshGroupTransform != null)
									{
										modMesh = _editor.Select.ParamSetOfMod._meshData.Find(delegate (apModifiedMesh a)
										{
											return a._transform_MeshGroup == meshGroupTransform;
										});
									}
									
									//if (linkedRenderUnit._isVisible_WithoutParent != meshTransform._isVisible_Default)
									if (modMesh != null)
									{
										//Mod 아이콘
										if (modMesh._isVisible)		{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_Visible; }
										else						{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_NonVisible; }
									}
									else
									{
										//ModMesh가 없다면 => NoKey를 리턴한다.
										return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;//<<키가 등록 안되어 있네염
									}
								}
								else
								{
									//Mod에 등록이 안되어도 NoKey 출력
									return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
								}
							}
						}
					}
				}

			}

			return apEditorHierarchyUnit.VISIBLE_TYPE.None;
		}





		private void OnRefreshVisiblePrefix(apEditorHierarchyUnit unit)
		{
			object targetObject = unit._savedObj;
			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
			}
			else
			{
				return;
			}


			if (linkedRenderUnit == null)
			{
				return;
			}


			//Prefix는
			//1) RenderUnit의 현재 렌더링 상태 -> Current
			//2) MeshTransform/MeshGroupTransform의 
			//Visible이 아닌 VisibleParent를 출력한다.
			//TmpWork에 의해서 Visible 값이 바뀌는가)
			//Calculate != WOParent 인 경우 (TmpWork의 영향을 받았다)
			bool isVisible = linkedRenderUnit._isVisible_WithoutParent;
			//bool isVisible = linkedRenderUnit._isVisible;


			//이전
			//if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
			//{
			//	if (isVisible)
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
			//	}
			//	else
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			//	}
			//}
			//else
			//{
			//	//TmpWork의 영향을 받지 않았다.
			//	if (isVisible)
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//	}
			//	else
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible;
			//	}

			//	//PostFix는 뭐 알아서 잘 하겠징
			//}

			//변경 21.1.31
			if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
			{
				//이전
				//if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
				//else				{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }

				//변경 21.1.29 : Tmp와 Rule이 따로 있다.
				if(linkedRenderUnit._workVisible_Tmp != linkedRenderUnit._workVisible_Rule)
				{
					//Tmp와 다르다면, Tmp의 값을 따른다. 단, None인 경우 빼고
					switch (linkedRenderUnit._workVisible_Tmp)
					{
						case apRenderUnit.WORK_VISIBLE_TYPE.None:
							unit._visibleType_Prefix = isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
							break;
						case apRenderUnit.WORK_VISIBLE_TYPE.ToShow:
							unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
							break;
						case apRenderUnit.WORK_VISIBLE_TYPE.ToHide:
							unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
							break;
					}
				}
				else
				{
					//Tmp와 같다면, Rule을 따른다.
					unit._visibleType_Prefix = isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
				}
			}
			else
			{
				//TmpWork의 영향을 받지 않았다.
				if (isVisible)	{ unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
				else			{ unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
			}
		}





		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public void GUI_RenderHierarchy(int width, bool isMeshHierarchy, Vector2 scroll, int scrollLayoutHeight)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			if (isMeshHierarchy)
			{
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Meshes.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Meshes[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
					GUILayout.Space(10);
				}
			}
			else
			{
				//Bone 루트를 출력합시다.
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Bones.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Bones[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
					GUILayout.Space(10);
				}
			}

		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private void GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent)
		{
			//unit.GUI_Render(_curUnitPosY, level * 10, width, scroll, scrollLayoutHeight, isGUIEvent, level);//이전
			unit.GUI_Render(_curUnitPosY, width, scroll, scrollLayoutHeight, isGUIEvent, level);//변경
			
			//_curUnitPosY += 20;
			_curUnitPosY += apEditorHierarchyUnit.HEIGHT;

			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						GUI_RenderUnit(unit._childUnits[i], level + 1, width, scroll, scrollLayoutHeight, isGUIEvent);
					}

					//추가 > 자식 Unit을 호출한 이후에는 약간 갭을 두자
					//GUILayout.Space(2);
				}

			}
		}
	}

}