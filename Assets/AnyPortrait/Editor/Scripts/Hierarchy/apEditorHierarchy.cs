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

	public class apEditorHierarchy
	{
		// Members
		//---------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root = new List<apEditorHierarchyUnit>();

		//개선 20.3.28 : Pool을 이용하자
		private apEditorHierarchyUnitPool _unitPool = new apEditorHierarchyUnitPool();

		//추가 21.6.12 : 우클릭에 대한 메뉴 멤버 추가
		private apEditorHierarchyMenu _rightMenu = null;
		private object _loadKey_Rename = null;//이름 변경 다이얼로그의 유효성 테스트를 위한 로드키 
		private object _loadKey_Search = null;

		public enum CATEGORY
		{
			Overall_Name,
			Overall_Item,
			Images_Name,
			Images_Item,
			Images_Add,
			Images_AddPSD,
			Mesh_Name,
			Mesh_Item,
			Mesh_Add,
			MeshGroup_Name,
			MeshGroup_Item,
			MeshGroup_Add,
			//Face_Name,
			//Face_Item,
			//Face_Add,
			Animation_Name,
			Animation_Item,
			Animation_Add,
			Param_Name,
			Param_Item,
			Param_Add,
		}

		//루트들만 따로 적용
		private apEditorHierarchyUnit _rootUnit_Overall = null;
		private apEditorHierarchyUnit _rootUnit_Image = null;
		private apEditorHierarchyUnit _rootUnit_Mesh = null;
		private apEditorHierarchyUnit _rootUnit_MeshGroup = null;
		//private apEditorHierarchyUnit _rootUnit_Face = null;
		private apEditorHierarchyUnit _rootUnit_Animation = null;
		private apEditorHierarchyUnit _rootUnit_Param = null;

		//public Texture2D _icon_Image = null;
		//public Texture2D _icon_Mesh = null;
		//public Texture2D _icon_MeshGroup = null;
		//public Texture2D _icon_Face = null;
		//public Texture2D _icon_Animation = null;
		//public Texture2D _icon_Add = null;

		//public Texture2D _icon_FoldDown = null;
		//public Texture2D _icon_FoldRight = null;

		//추가 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = null;
		private apGUIContentWrapper _guiContent_FoldRight = null;
		private apGUIContentWrapper _guiContent_ModRegisted = null;
		
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;

		private apGUIContentWrapper _guiContent_OrderUp = null;
		private apGUIContentWrapper _guiContent_OrderDown = null;

		private bool _isNeedReset = false;
		private int _curUnitPosY = 0;//<<추가

		public void SetNeedReset()
		{
			_isNeedReset = true;
		}

		// Init
		//---------------------------------------------
		public apEditorHierarchy(apEditor editor)
		{
			_editor = editor;

			_rightMenu = new apEditorHierarchyMenu(_editor, OnSelectRightMenu);

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
			//GUIContent 추가
			if (_guiContent_FoldDown == null)			{ _guiContent_FoldDown =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)); }
			if (_guiContent_FoldRight == null)			{ _guiContent_FoldRight =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight)); }
			if (_guiContent_ModRegisted == null)		{ _guiContent_ModRegisted =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered)); }

			if (_guiContent_RestoreTmpWorkVisible_ON == null)		{ _guiContent_RestoreTmpWorkVisible_ON =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON)); }
			if (_guiContent_RestoreTmpWorkVisible_OFF == null)		{ _guiContent_RestoreTmpWorkVisible_OFF =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF)); }

			if (_guiContent_OrderUp == null)		{ _guiContent_OrderUp =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp)); }
			if (_guiContent_OrderDown == null)		{ _guiContent_OrderDown =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown)); }
		}
		
		// Functions
		//---------------------------------------------
		public void ResetAllUnits()
		{
			_isNeedReset = false;
			_units_All.Clear();
			_units_Root.Clear();

			

			ReloadGUIContent();

			//Pool 초기화
			if(_unitPool.IsInitialized)
			{
				_unitPool.PushAll();
			}
			else
			{
				_unitPool.Init();
			}

			//메인 루트들을 만들어주자
			//_rootUnit_Overall =		AddUnit_OnlyButton(null, "Portrait", CATEGORY.Overall_Name, null, true, null);
			_rootUnit_Overall = AddUnit_Label(null, Editor.GetUIWord(UIWORD.RootUnits), CATEGORY.Overall_Name, null, true, null);
			_rootUnit_Image = AddUnit_Label(null, Editor.GetUIWord(UIWORD.Images), CATEGORY.Images_Name, null, true, null);
			_rootUnit_Mesh = AddUnit_Label(null, Editor.GetUIWord(UIWORD.Meshes), CATEGORY.Mesh_Name, null, true, null);
			_rootUnit_MeshGroup = AddUnit_Label(null, Editor.GetUIWord(UIWORD.MeshGroups), CATEGORY.MeshGroup_Name, null, true, null);
			//_rootUnit_Face =		AddUnit_Label(null, "Faces", CATEGORY.Face_Name, null, true, null);
			_rootUnit_Animation = AddUnit_Label(null, Editor.GetUIWord(UIWORD.AnimationClips), CATEGORY.Animation_Name, null, true, null);
			_rootUnit_Param = AddUnit_Label(null, Editor.GetUIWord(UIWORD.ControlParameters), CATEGORY.Param_Name, null, true, null);

			if (Editor == null || Editor._portrait == null)
			{
				return;
			}



			//추가 20.7.6 : 선택된 객체를 생성할때 바로 확인해야한다.
			object selectedObject = null;
			switch (Editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Overall:	if(Editor.Select.RootUnit != null)		{ selectedObject = Editor.Select.RootUnit; } break;
				case apSelection.SELECTION_TYPE.ImageRes:	if(Editor.Select.TextureData != null)	{ selectedObject = Editor.Select.TextureData; } break;
				case apSelection.SELECTION_TYPE.Mesh:		if(Editor.Select.Mesh != null)			{ selectedObject = Editor.Select.Mesh; } break;
				case apSelection.SELECTION_TYPE.MeshGroup:	if(Editor.Select.MeshGroup != null)		{ selectedObject = Editor.Select.MeshGroup; } break;
				case apSelection.SELECTION_TYPE.Animation:	if(Editor.Select.AnimClip != null)		{ selectedObject = Editor.Select.AnimClip; } break;
				case apSelection.SELECTION_TYPE.Param:		if(Editor.Select.Param != null)			{ selectedObject = Editor.Select.Param; } break;
			}
			

			//0. 루트 유닛
			//기존 : 리스트 그대로
			//List<apRootUnit> rootUnits = Editor._portrait._rootUnits;
			//for (int i = 0; i < rootUnits.Count; i++)
			//{
			//	apRootUnit rootUnit = rootUnits[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), "Root Unit " + i, CATEGORY.Overall_Item, rootUnit, false, _rootUnit_Overall);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> rootUnitSets = Editor._portrait._objectOrders.RootUnits;
			for (int i = 0; i < rootUnitSets.Count; i++)
			{
				apObjectOrders.OrderSet rootUnitSet = rootUnitSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), 
										"Root Unit " + i + " (" + rootUnitSet._linked_RootUnit.Name + ")",//<<변경
										CATEGORY.Overall_Item, rootUnitSet._linked_RootUnit, selectedObject,
										false, _rootUnit_Overall, RIGHTCLICK_MENU.Enabled);
			}


			//1. 이미지 파일들을 검색하자
			//기존 : 리스트 그대로
			//List<apTextureData> textures = Editor._portrait._textureData;
			//for (int i = 0; i < textures.Count; i++)
			//{
			//	apTextureData textureData = textures[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), textureData._name, CATEGORY.Images_Item, textureData, false, _rootUnit_Image);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> textureSets = Editor._portrait._objectOrders.Images;
			for (int i = 0; i < textureSets.Count; i++)
			{
				apObjectOrders.OrderSet textureDataSet = textureSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), 
										textureDataSet._linked_Image._name, 
										CATEGORY.Images_Item, 
										textureDataSet._linked_Image, 
										selectedObject,
										false, _rootUnit_Image,
										RIGHTCLICK_MENU.Enabled);
			}

			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddImage), CATEGORY.Images_Add, null, false, _rootUnit_Image);
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_AddPSD), Editor.GetUIWord(UIWORD.ImportPSDFile), CATEGORY.Images_AddPSD, null, false, _rootUnit_Image);

			//2. 메시 들을 검색하자
			//기존 : 리스트 그대로
			//List<apMesh> meshes = Editor._portrait._meshes;
			//for (int i = 0; i < meshes.Count; i++)
			//{
			//	apMesh mesh = meshes[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh), mesh._name, CATEGORY.Mesh_Item, mesh, false, _rootUnit_Mesh);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> mesheSets = Editor._portrait._objectOrders.Meshes;
			for (int i = 0; i < mesheSets.Count; i++)
			{
				apObjectOrders.OrderSet meshSet = mesheSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh), 
										meshSet._linked_Mesh._name, 
										CATEGORY.Mesh_Item, 
										meshSet._linked_Mesh, 
										selectedObject,
										false, _rootUnit_Mesh,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddMesh), CATEGORY.Mesh_Add, null, false, _rootUnit_Mesh);

			//3. 메시 그룹들을 검색하자
			//메시 그룹들은 하위에 또다른 Mesh Group을 가지고 있다.
			//기존 : 리스트 그대로
			//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> meshGroupSets = Editor._portrait._objectOrders.MeshGroups;

			for (int i = 0; i < meshGroupSets.Count; i++)
			{
				//기존
				//apMeshGroup meshGroup = meshGroupSets[i];

				//변경
				apObjectOrders.OrderSet meshGrouSet = meshGroupSets[i];
				apMeshGroup meshGroup = meshGrouSet._linked_MeshGroup;

				if (meshGroup._parentMeshGroup == null || meshGroup._parentMeshGroupID < 0)
				{
					//Debug.Log("Reset H : MeshGroup(" + meshGroup._name + ") - Root");
					apEditorHierarchyUnit addedHierarchyUnit = AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
																						meshGroup._name, 
																						CATEGORY.MeshGroup_Item, 
																						meshGroup, 
																						selectedObject,
																						false, _rootUnit_MeshGroup,
																						RIGHTCLICK_MENU.Enabled);
					if (meshGroup._childMeshGroupTransforms.Count > 0)
					{
						AddUnit_SubMeshGroup(meshGroup, addedHierarchyUnit, selectedObject);
					}
				}
				//else
				//{
				//	//Debug.Log("Reset H : MeshGroup(" + meshGroup._name + ") - Child");
				//}
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddMeshGroup), CATEGORY.MeshGroup_Add, null, false, _rootUnit_MeshGroup);


			//7. 파라미터들을 검색하자
			//기존 : 리스트 그대로
			//List<apControlParam> cParams = Editor.ParamControl._controlParams;
			//for (int i = 0; i < cParams.Count; i++)
			//{
			//	apControlParam cParam = cParams[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param), cParam._keyName, CATEGORY.Param_Item, cParam, false, _rootUnit_Param);
			//}

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> cParamSets = Editor._portrait._objectOrders.ControlParams;
			for (int i = 0; i < cParamSets.Count; i++)
			{
				apObjectOrders.OrderSet cParamSet = cParamSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param), 
										cParamSet._linked_ControlParam._keyName, 
										CATEGORY.Param_Item, 
										cParamSet._linked_ControlParam, 
										selectedObject,
										false, _rootUnit_Param,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddControlParameter), CATEGORY.Param_Add, null, false, _rootUnit_Param);


			//8. 애니메이션을 넣자
			//기존 : 리스트 그대로
			//List<apAnimClip> animClips = Editor._portrait._animClips;
			//for (int i = 0; i < animClips.Count; i++)
			//{
			//	apAnimClip animClip = animClips[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation), animClip._name, CATEGORY.Animation_Item, animClip, false, _rootUnit_Animation);
			//}

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> animClipSets = Editor._portrait._objectOrders.AnimClips;
			for (int i = 0; i < animClipSets.Count; i++)
			{
				apObjectOrders.OrderSet animClipSet = animClipSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation), 
										animClipSet._linked_AnimClip._name, 
										CATEGORY.Animation_Item, 
										animClipSet._linked_AnimClip, 
										selectedObject,
										false, _rootUnit_Animation,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddAnimationClip), CATEGORY.Animation_Add, null, false, _rootUnit_Animation);
		}


		private void AddUnit_SubMeshGroup(apMeshGroup parentMeshGroup, apEditorHierarchyUnit parentUnit, object selectedObject)
		{
			for (int iChild = 0; iChild < parentMeshGroup._childMeshGroupTransforms.Count; iChild++)
			{
				if (parentMeshGroup._childMeshGroupTransforms[iChild]._meshGroup != null)
				{
					apMeshGroup childMeshGroup = parentMeshGroup._childMeshGroupTransforms[iChild]._meshGroup;
					apEditorHierarchyUnit hierarchyUnit = AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
																				childMeshGroup._name, 
																				CATEGORY.MeshGroup_Item, 
																				childMeshGroup, 
																				selectedObject,
																				false, parentUnit,
																				RIGHTCLICK_MENU.Enabled,
																				ORDER_CHANGABLE.None);

					if (childMeshGroup._childMeshGroupTransforms.Count > 0)
					{
						AddUnit_SubMeshGroup(childMeshGroup, hierarchyUnit, selectedObject);
					}
				}
			}
		}

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon, string text, CATEGORY savedKey, object savedObj, bool isRoot, apEditorHierarchyUnit parent)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);
			

			newUnit.SetEvent(OnUnitClick);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private enum RIGHTCLICK_MENU
		{
			None, Enabled
		}

		private enum ORDER_CHANGABLE
		{
			None, Changable
		}

		private apEditorHierarchyUnit AddUnit_ToggleButton(	Texture2D icon, string text, 
															CATEGORY savedKey, object savedObj, object curSelectedObj,
															bool isRoot, apEditorHierarchyUnit parent, 
															RIGHTCLICK_MENU rightClickSupported,
															ORDER_CHANGABLE isOrderChangable = ORDER_CHANGABLE.Changable)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown)
			//							);

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted,
										_guiContent_OrderUp,
										_guiContent_OrderDown
										);
			

			if(isOrderChangable == ORDER_CHANGABLE.Changable)
			{
				newUnit.SetEvent(OnUnitClick, null, OnUnitClickOrderChanged);
			}
			else
			{
				newUnit.SetEvent(OnUnitClick);
			}

			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}
			

			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj, false);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}

			//추가 20.7.6 : 선택 여부를 추가할때 알려줘야 한다.
			newUnit.SetSelected(curSelectedObj == savedObj && curSelectedObj != null, true);

			return newUnit;
		}

		private apEditorHierarchyUnit AddUnit_OnlyButton(Texture2D icon, string text, CATEGORY savedKey, object savedObj, bool isRoot, apEditorHierarchyUnit parent)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);


			newUnit.SetEvent(OnUnitClick);
			newUnit.SetOnlyButton(icon, text, (int)savedKey, savedObj);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || _isNeedReset)
			{
				ResetAllUnits();

				return;
			}

			ReloadGUIContent();

			//Pool 초기화
			if(!_unitPool.IsInitialized)
			{
				_unitPool.Init();
			}


			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			
			//0. 루트 유닛들을 검색하자
			//이전
			//List<apRootUnit> rootUnits = Editor._portrait._rootUnits;

			//변경
			List<apObjectOrders.OrderSet> rootUnitSets = Editor._portrait._objectOrders.RootUnits;
			for (int i = 0; i < rootUnitSets.Count; i++)
			{
				//이전
				//apRootUnit rootUnit = Editor._portrait._rootUnits[i];
				
				//변경
				apRootUnit rootUnit = rootUnitSets[i]._linked_RootUnit;
				RefreshUnit(CATEGORY.Overall_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root),
								rootUnit,
								"Root Unit " + i + " (" + rootUnit.Name + ")",//<<변경
								Editor.Select.RootUnit,
								_rootUnit_Overall, 
								i,
								RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apRootUnit>(deletedUnits, CATEGORY.Overall_Item, rootUnits);

			//변경
			CheckRemovableUnits<apRootUnit>(deletedUnits, CATEGORY.Overall_Item, Editor._portrait._rootUnits);


			//1. 이미지 파일들을 검색하자 -> 있는건 없애고, 없는건 만들자
			//이전
			//List<apTextureData> textures = Editor._portrait._textureData;

			//변경
			List<apObjectOrders.OrderSet> textureSets = Editor._portrait._objectOrders.Images;
			for (int i = 0; i < textureSets.Count; i++)
			{
				//이전
				//apTextureData textureData = textures[i];

				//변경
				apTextureData textureData = textureSets[i]._linked_Image;
				RefreshUnit(CATEGORY.Images_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image),
								textureData,
								textureData._name,
								Editor.Select.TextureData,
								_rootUnit_Image,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apTextureData>(deletedUnits, CATEGORY.Images_Item, textures);

			//변경
			CheckRemovableUnits<apTextureData>(deletedUnits, CATEGORY.Images_Item, Editor._portrait._textureData);



			//2. 메시 들을 검색하자
			//이전
			//List<apMesh> meshes = Editor._portrait._meshes;
			
			//변경
			List<apObjectOrders.OrderSet> mesheSets = Editor._portrait._objectOrders.Meshes;
			for (int i = 0; i < mesheSets.Count; i++)
			{
				//이전
				//apMesh mesh = meshes[i];

				//변경
				apMesh mesh = mesheSets[i]._linked_Mesh;
				RefreshUnit(CATEGORY.Mesh_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh),
								mesh,
								mesh._name,
								Editor.Select.Mesh,
								_rootUnit_Mesh,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apMesh>(deletedUnits, CATEGORY.Mesh_Item, meshes);
			
			//변경
			CheckRemovableUnits<apMesh>(deletedUnits, CATEGORY.Mesh_Item, Editor._portrait._meshes);


			//3. Mesh Group들을 검색하자
			//이전
			//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
			//변경
			List<apObjectOrders.OrderSet> meshGroupSets = Editor._portrait._objectOrders.MeshGroups;

			for (int i = 0; i < meshGroupSets.Count; i++)
			{
				//이건 재귀 함수 -_-;
				apMeshGroup meshGroup = meshGroupSets[i]._linked_MeshGroup;
				if (meshGroup._parentMeshGroup == null)
				{
					RefreshUnit_MeshGroup(meshGroup, _rootUnit_MeshGroup, i);
				}
			}
			//이전
			//CheckRemovableUnits<apMeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, meshGroups);

			//변경
			CheckRemovableUnits<apMeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, Editor._portrait._meshGroups);


			//7. 파라미터들을 검색하자
			//이전
			//List<apControlParam> cParams = Editor.ParamControl._controlParams;
			
			//변경
			List<apObjectOrders.OrderSet> cParamSets = Editor._portrait._objectOrders.ControlParams;
			for (int i = 0; i < cParamSets.Count; i++)
			{
				//이전
				//apControlParam cParam = cParams[i];

				//변경
				apControlParam cParam = cParamSets[i]._linked_ControlParam;
				RefreshUnit(CATEGORY.Param_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param),
								cParam,
								cParam._keyName,
								Editor.Select.Param,
								_rootUnit_Param,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.Param_Item, cParams);
			
			//변경
			CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.Param_Item, Editor.ParamControl._controlParams);


			//8. 애니메이션을 넣자
			//이전
			//List<apAnimClip> animClips = Editor._portrait._animClips;

			List<apObjectOrders.OrderSet> animClipSets = Editor._portrait._objectOrders.AnimClips;
			for (int i = 0; i < animClipSets.Count; i++)
			{
				//이전
				//apAnimClip animClip = animClips[i];
				//변경
				apAnimClip animClip = animClipSets[i]._linked_AnimClip;
				RefreshUnit(CATEGORY.Animation_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation),
								animClip,
								animClip._name,
								Editor.Select.AnimClip,
								_rootUnit_Animation,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apAnimClip>(deletedUnits, CATEGORY.Animation_Item, animClips);
			
			//변경
			CheckRemovableUnits<apAnimClip>(deletedUnits, CATEGORY.Animation_Item, Editor._portrait._animClips);

			//삭제할 유닛을 체크하고 계산하자
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

				//20.3.18 그냥 삭제하지 말고 Pool에 넣자
				_unitPool.PushUnit(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root.Count; i++)
			{
				SortUnit_Recv(_units_Root[i]);
			}
		}



		private void RefreshUnit_MeshGroup(apMeshGroup parentMeshGroup, apEditorHierarchyUnit refreshedHierarchyUnit, int indexPerParent)
		{
			apEditorHierarchyUnit unit = RefreshUnit(CATEGORY.MeshGroup_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup),
								parentMeshGroup,
								parentMeshGroup._name,
								Editor.Select.MeshGroup,
								refreshedHierarchyUnit,
								indexPerParent,
								RIGHTCLICK_MENU.Enabled,
								(parentMeshGroup._parentMeshGroup == null) ? ORDER_CHANGABLE.Changable : ORDER_CHANGABLE.None//Root MeshGroup만 순서 바꿀 수 있다.
								);

			if (parentMeshGroup._childMeshGroupTransforms.Count > 0)
			{

				for (int i = 0; i < parentMeshGroup._childMeshGroupTransforms.Count; i++)
				{
					apMeshGroup childMeshGroup = parentMeshGroup._childMeshGroupTransforms[i]._meshGroup;
					if (childMeshGroup != null)
					{
						RefreshUnit_MeshGroup(childMeshGroup, unit, i);
					}
				}
			}
		}


		private apEditorHierarchyUnit RefreshUnit(	CATEGORY category, 
													Texture2D iconImage, 
													object obj, 
													string objName, 
													object selectedObj, 
													apEditorHierarchyUnit parentUnit,
													int indexPerParent,
													RIGHTCLICK_MENU rightClickMenu,
													ORDER_CHANGABLE orderChangable = ORDER_CHANGABLE.Changable)
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
					unit.SetSelected(false, true);
				}

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
			}
			else
			{
				unit = AddUnit_ToggleButton(iconImage, objName, category, obj, selectedObj, false, parentUnit, rightClickMenu, orderChangable);
			}

			//추가 3.29 : Refresh의 경우 Index를 외부에서 지정한다.
			unit._indexPerParent = indexPerParent;

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


		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if (a._savedKey == b._savedKey)
					{
						return a._indexPerParent - b._indexPerParent;
					}
					return a._savedKey - b._savedKey;
				});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv(unit._childUnits[i]);
				}
			}
		}

		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift)
		{
			if (Editor == null)
			{
				return;
			}

			apEditorHierarchyUnit selectedUnit = null;

			
			//여기서 이벤트를 설정해주자
			bool isAnyAdded = false;//추가 20.7.14 : 뭔가 추가가 되었다면, Refresh를 해야한다.
			CATEGORY category = (CATEGORY)savedKey;
			//Debug.Log("Unit Select : " + category);
			switch (category)
			{
				case CATEGORY.Overall_Name:
				case CATEGORY.Images_Name:
				case CATEGORY.Mesh_Name:
				case CATEGORY.MeshGroup_Name:
				//case CATEGORY.Face_Name:
				case CATEGORY.Animation_Name:
				case CATEGORY.Param_Name:
					break;

				case CATEGORY.Overall_Item:
					//전체 선택
					apRootUnit rootUnit = savedObj as apRootUnit;
					if (rootUnit != null)
					{
						Editor.Select.SetOverall(rootUnit);
						if (Editor.Select.RootUnit == rootUnit)
						{
							selectedUnit = eventUnit;
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = savedObj as apTextureData;
						if (textureData != null)
						{
							Editor.Select.SetImage(textureData);//<< 선택하자
							if (Editor.Select.TextureData == textureData)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.Images_Add:
					{
						Editor.Controller.AddImage();
						isAnyAdded = true;
					}
					break;

				case CATEGORY.Images_AddPSD://추가 : PSD 로드
					{
						Editor.Controller.ShowPSDLoadDialog();
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = savedObj as apMesh;
						if (mesh != null)
						{
							Editor.Select.SetMesh(mesh);//<< 선택하자

							if (Editor.Select.Mesh == mesh)
							{
								selectedUnit = eventUnit;

								//추가 20.7.6 : 여기서 메시를 열때에만
								//PSD 파일로부터 열린 메시의 버텍스를 초기화할 것인지 물어보기
								if(mesh._isNeedToAskRemoveVertByPSDImport)
								{
									//물어보기 요청
									Editor._isRequestRemoveVerticesIfImportedFromPSD_Step1 = true;
									Editor._isRequestRemoveVerticesIfImportedFromPSD_Step2 = false;
									Editor._requestMeshRemoveVerticesIfImportedFromPSD = mesh;
								}
							}
						}
					}
					break;

				case CATEGORY.Mesh_Add:
					{
						apMesh newMesh = Editor.Controller.AddMesh();
						if(newMesh != null)
						{
							//추가 : 새로 Mesh를 추가한 경우, 다음에 이 Mesh를 선택할 때에는 (새거이므로)
							//MeshEdit 탭을 Setting으로 설정해야한다.
							if(!Editor.Select._createdNewMeshes.Contains(newMesh))
							{
								Editor.Select._createdNewMeshes.Add(newMesh);
							}
							
							//추가 21.3.6 : 새로 생성된 메시는 이미지를 할당할 수도 있다.
							Editor.Controller.CheckAndSetImageToMeshAutomatically(newMesh);
							

							Editor.Select.SetMesh(newMesh);//<< 선택하자
						}
						isAnyAdded = true;
					}
					
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = savedObj as apMeshGroup;
						if (meshGroup != null)
						{
							Editor.Select.SetMeshGroup(meshGroup);

							if (Editor.Select.MeshGroup == meshGroup)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Add:
					{
						apMeshGroup newMeshGroup = Editor.Controller.AddMeshGroup();
						if (newMeshGroup != null)
						{
							Editor.Select.SetMeshGroup(newMeshGroup);
						}
						isAnyAdded = true;
					}
					
					break;

				//case CATEGORY.Face_Item:
				//	break;

				//case CATEGORY.Face_Add:
				//	break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = savedObj as apAnimClip;
						if (animClip != null)
						{
							Editor.Select.SetAnimClip(animClip);
							if (Editor.Select.AnimClip == animClip)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.Animation_Add:
					{
						//데모 기능 제한
						//Param 개수는 2개로 제한되며, 이걸 넘어가면 추가할 수 없다.
						if (apVersion.I.IsDemo)
						{
							if (Editor._portrait._animClips.Count >= 2)
							{
								//이미 2개를 넘었다.
								EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddAnimation),
									Editor.GetText(TEXT.Okay)
									);

								break;
							}
						}

						Editor.Controller.AddAnimClip();

						isAnyAdded = true;
					}
					
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = savedObj as apControlParam;
						if (cParam != null)
						{
							Editor.Select.SetParam(cParam);

							if (Editor.Select.Param == cParam)
							{
								selectedUnit = eventUnit;
							}
						}
					}
					break;

				case CATEGORY.Param_Add:
					{
						//데모 기능 제한
						//Param 개수는 2개로 제한되며, 이걸 넘어가면 추가할 수 없다.
						if (apVersion.I.IsDemo)
						{
							if (Editor.ParamControl._controlParams.Count >= 2)
							{
								//이미 2개를 넘었다.
								EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(TEXT.Okay)
									);

								break;
							}
						}

						//Param 추가
						Editor.Controller.AddParam();

						isAnyAdded = true;
					}
					
					break;
			}

			if (isAnyAdded)
			{
				RefreshUnits();
			}
			else
			{
				if (selectedUnit != null)
				{
					for (int i = 0; i < _units_All.Count; i++)
					{
						if (_units_All[i] == selectedUnit)
						{
							//_units_All[i]._isSelected = true;
							_units_All[i].SetSelected(true, true);
						}
						else
						{
							//_units_All[i]._isSelected = false;
							_units_All[i].SetSelected(false, true);
						}
					}
				}
				else
				{
					for (int i = 0; i < _units_All.Count; i++)
					{
						//_units_All[i]._isSelected = false;
						_units_All[i].SetSelected(false, true);
					}
				}
			}
		}


		public void OnUnitClickOrderChanged(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isOrderUp)
		{
			//Hierarchy의 항목 순서를 바꾸자
			if (Editor == null || Editor._portrait == null)
			{
				return;
			}
			apObjectOrders orders = Editor._portrait._objectOrders;

			bool isChanged = false;
			bool isResult = false;
			CATEGORY category = (CATEGORY)savedKey;
			switch (category)
			{
				case CATEGORY.Overall_Item:
					{
						apRootUnit rootUnit = savedObj as apRootUnit;
						if(rootUnit != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = savedObj as apTextureData;
						if(textureData != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Image, textureData._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = savedObj as apMesh;
						if(mesh != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Mesh, mesh._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = savedObj as apMeshGroup;
						if(meshGroup != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = savedObj as apAnimClip;
						if(animClip != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.AnimClip, animClip._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = savedObj as apControlParam;
						if(cParam != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.ControlParam, cParam._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;
			}

			if(isChanged)
			{
				apEditorUtil.SetEditorDirty();
				Editor.RefreshControllerAndHierarchy(false);
			}
		}


		// 우클릭 이벤트
		//--------------------------------------------------------------------------------
		//추가 21.6.12
		//우클릭시 메뉴가 마우스에서 나온다.
		public void OnUnitRightClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj)
		{
			//Debug.Log("우클릭");
			if(_rightMenu != null && savedObj != null)
			{
				//우클릭 메뉴를 열자 (해당되는 항목만)
				CATEGORY category = (CATEGORY)savedKey;
				switch (category)
				{
					case CATEGORY.Overall_Item:
						{
							//루트유닛
							_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
						}
						break;
					case CATEGORY.Images_Item:
						{
							//이미지
							_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
						}
						break;

					case CATEGORY.Mesh_Item:
						{
							//메시
							_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
						}
						break;
					case CATEGORY.MeshGroup_Item:
						{
							apMeshGroup meshGroup = savedObj as apMeshGroup;
							if(meshGroup != null && meshGroup._parentMeshGroup != null)
							{
								//Child MeshGroup인 경우
								_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
							}
							else
							{
								//Root MeshGroup인 경우
								_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
							}
						}
						break;
					case CATEGORY.Animation_Item:
						{
							_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
						}
						break;
					case CATEGORY.Param_Item:
						{
							_rightMenu.ShowMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate
												| apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove,
												savedKey, savedObj);
						}
					break;
				}
				
			}
		}


		//우클릭 메뉴에서 항목을 선택했을 때
		private void OnSelectRightMenu(apEditorHierarchyMenu.MENU_ITEM_HIERARCHY menuType, int hierachyUnitType, object requestedObj)
		{
			if(_editor == null || _editor._portrait == null)
			{
				return;
			}
			apObjectOrders orders = _editor._portrait._objectOrders;//위치 변경용
			CATEGORY category = (CATEGORY)hierachyUnitType;
			switch (category)
			{
				case CATEGORY.Overall_Item:
					{
						apRootUnit rootUnit = requestedObj as apRootUnit;
						if(rootUnit != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, true);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, false);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//Unregister
										apMeshGroup targetRootMeshGroup = rootUnit._childMeshGroup;
										if (targetRootMeshGroup != null)
										{
											bool isNeedToNone = _editor.Select.RootUnit == rootUnit;
											apEditorUtil.SetRecord_PortraitMeshGroup(apUndoGroupData.ACTION.Portrait_SetMeshGroup, _editor, _editor._portrait, targetRootMeshGroup, false, true, apEditorUtil.UNDO_STRUCT.StructChanged);

											_editor._portrait._mainMeshGroupIDList.Remove(targetRootMeshGroup._uniqueID);
											_editor._portrait._mainMeshGroupList.Remove(targetRootMeshGroup);

											_editor._portrait._rootUnits.Remove(rootUnit);

											if (isNeedToNone)
											{
												_editor.Select.SetNone();
											}
										}
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = requestedObj as apTextureData;
						if(textureData != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, textureData, textureData._name, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Image, textureData._uniqueID, true);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Image, textureData._uniqueID, false);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//경고 메시지를 먼저 보여준다.

										string strDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				textureData,
																				5,
																				_editor.GetTextFormat(TEXT.RemoveImage_Body, textureData._name),
																				_editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveImage_Title),
																strDialogInfo,
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));

										if (isResult)
										{
											_editor.Controller.RemoveTexture(textureData);
										}
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = requestedObj as apMesh;
						if(mesh != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, mesh, mesh._name, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Mesh, mesh._uniqueID, true);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Mesh, mesh._uniqueID, false);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										_editor.Controller.DuplicateMesh(mesh);
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				mesh,
																				5,
																				_editor.GetTextFormat(TEXT.RemoveMesh_Body, mesh._name),
																				_editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																				);

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveMesh_Title),
																						strRemoveDialogInfo,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));

										if (isResult)
										{
											_editor.Controller.RemoveMesh(mesh);
										}
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = requestedObj as apMeshGroup;
						if(meshGroup != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, meshGroup, meshGroup._name, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp://Root MeshGroup인 경우
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, true);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown://Root MeshGroup인 경우
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, false);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										_editor.Controller.DuplicateMeshGroup(meshGroup, null, meshGroup._parentMeshGroup == null, true);
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				meshGroup,
																				5,
																				Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, meshGroup._name),
																				Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																				);

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveMeshGroup_Title),
																						strRemoveDialogInfo,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel)
																						);
										if (isResult)
										{
											_editor.Controller.RemoveMeshGroup(meshGroup);
										}
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = requestedObj as apAnimClip;
						if (animClip != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, animClip, animClip._name, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.AnimClip, animClip._uniqueID, true);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.AnimClip, animClip._uniqueID, false);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										_editor.Controller.DuplicateAnimClip(animClip);
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.RemoveAnimClip_Title),
																						_editor.GetTextFormat(TEXT.RemoveAnimClip_Body, animClip._name),
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));
										if (isResult)
										{
											_editor.Controller.RemoveAnimClip(animClip);
										}
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = requestedObj as apControlParam;
						if(cParam != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, cParam, cParam._keyName, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.ControlParam, cParam._uniqueID, true);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.ControlParam, cParam._uniqueID, false);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										_editor.Controller.DuplicateParam(cParam);
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveParamText = Editor.Controller.GetRemoveItemMessage(
														_editor._portrait,
														cParam,
														5,
														_editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
														_editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
														);

										bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.RemoveControlParam_Title),
																						strRemoveParamText,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));
										if (isResult)
										{
											_editor.Controller.RemoveParam(cParam);
										}
									}
									break;
							}
						}
					}
					break;
			}

			//메뉴를 선택하면 변경이 될 것이므로 Dirty
			apEditorUtil.SetEditorDirty();
			Editor.RefreshControllerAndHierarchy(false);
		}

		//오브젝트 이름 바꾸기 다이얼로그 결과
		public void OnObjectRenamed(bool isSuccess, object loadKey, object targetObject, string name)
		{
			if(!isSuccess
				|| loadKey == null
				|| _loadKey_Rename != loadKey
				|| targetObject == null
				|| string.IsNullOrEmpty(name)
				|| _editor == null
				|| _editor._portrait == null)
			{
				_loadKey_Rename = null;
				return;
			}

			_loadKey_Rename = null;

			//대상에 따라 다르다.
			if(targetObject is apTextureData)
			{
				apTextureData textureData = targetObject as apTextureData;

				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
													_editor, 
													_editor._portrait, 
													//textureData, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				textureData._name = name;
			}
			else if(targetObject is apMesh)
			{
				apMesh mesh = targetObject as apMesh;

				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged, 
												_editor, 
												mesh, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				mesh._name = name;
			}
			else if(targetObject is apMeshGroup)
			{
				apMeshGroup meshGroup = targetObject as apMeshGroup;

				//메시 그룹의 이름은 아래 함수를 이용한다.
				_editor.Controller.RenameMeshGroup(meshGroup, name);
			}
			else if(targetObject is apAnimClip)
			{
				apAnimClip animClip = targetObject as apAnimClip;

				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_SettingChanged, 
													_editor, 
													_editor._portrait, 
													//animClip, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);					
				animClip._name = name;
			}
			else if(targetObject is apControlParam)
			{
				apControlParam controlParam = targetObject as apControlParam;

				if (Editor.ParamControl.FindParam(name) != null)
				{
					//이미 사용중인 이름이다.
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.ControlParamNameError_Title),
												_editor.GetText(TEXT.ControlParamNameError_Body_Used),
												_editor.GetText(TEXT.Close));
				}
				else
				{
					_editor.Controller.ChangeParamName(controlParam, name);
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
		}


		//오브젝트 찾기 다이얼로그 결과
		private void OnObjectSearched(object loadKey, object targetObject)
		{
			
			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null)
			{
				_loadKey_Search = null;
				return;
			}
			if(targetObject == null)
			{
				return;
			}

			
			if(targetObject is apRootUnit)
			{
				//루트유닛
				apRootUnit rootUnit = targetObject as apRootUnit;
				if(rootUnit != null
					&& _editor._portrait != null
					&& _editor._portrait._rootUnits.Contains(rootUnit))
				{
					Editor.Select.SetOverall(rootUnit);
				}
			}
			else if(targetObject is apTextureData)
			{
				//텍스쳐
				apTextureData textureData = targetObject as apTextureData;
				if(textureData != null 
					&& _editor._portrait._textureData != null
					&& _editor._portrait._textureData.Contains(textureData))
				{
					Editor.Select.SetImage(textureData);//<< 선택하자
				}
			}
			else if(targetObject is apMesh)
			{
				//메시
				apMesh mesh = targetObject as apMesh;
				if(mesh != null 
					&& _editor._portrait._meshes != null
					&& _editor._portrait._meshes.Contains(mesh))
				{
					Editor.Select.SetMesh(mesh);//<< 선택하자
				}
			}
			else if(targetObject is apMeshGroup)
			{
				//메시 그룹
				apMeshGroup meshGroup = targetObject as apMeshGroup;
				if(meshGroup != null 
					&& _editor._portrait._meshGroups != null
					&& _editor._portrait._meshGroups.Contains(meshGroup))
				{
					Editor.Select.SetMeshGroup(meshGroup);
				}
			}
			else if(targetObject is apAnimClip)
			{
				//애니메이션 클립
				apAnimClip animClip = targetObject as apAnimClip;
				if(animClip != null 
					&& _editor._portrait._animClips != null
					&& _editor._portrait._animClips.Contains(animClip))
				{
					Editor.Select.SetAnimClip(animClip);
				}
			}
			else if(targetObject is apControlParam)
			{
				//컨트롤 파라미터
				apControlParam cParam = targetObject as apControlParam;
				if(cParam != null 
					&& _editor._portrait._controller != null
					&& _editor._portrait._controller._controlParams != null
					&& _editor._portrait._controller._controlParams.Contains(cParam))
				{
					Editor.Select.SetParam(cParam);
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
		}

		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public void GUI_RenderHierarchy(int width, apEditor.HIERARCHY_FILTER hierarchyFilter, Vector2 scroll, int scrollLayoutHeight, bool isOrderChanged)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			bool isUnitRenderable = false;
			for (int i = 0; i < _units_Root.Count; i++)
			{
				CATEGORY category = (CATEGORY)_units_Root[i]._savedKey;
				isUnitRenderable = false;

				switch (category)
				{
					case CATEGORY.Overall_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.RootUnit) != 0;
						break;
					case CATEGORY.Images_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Image) != 0;
						break;
					case CATEGORY.Mesh_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Mesh) != 0;
						break;
					case CATEGORY.MeshGroup_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.MeshGroup) != 0;
						break;
					case CATEGORY.Animation_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Animation) != 0;
						break;
					case CATEGORY.Param_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Param) != 0;
						break;
				}
				if (isUnitRenderable)
				{
					GUI_RenderUnit(_units_Root[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent, isOrderChanged);

					GUILayout.Space(10);
					_curUnitPosY += 10;
				}
			}
			GUILayout.Space(20);

		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private void GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent, bool isOrderChanged)
		{
			//unit.GUI_Render(_curUnitPosY, level * 10, width, scroll, scrollLayoutHeight, isGUIEvent, level, isOrderChanged);//이전
			unit.GUI_Render(_curUnitPosY, width, scroll, scrollLayoutHeight, isGUIEvent, level, isOrderChanged);//변경

			//_curUnitPosY += 20;//Height만큼 증가
			_curUnitPosY += apEditorHierarchyUnit.HEIGHT;

			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						GUI_RenderUnit(unit._childUnits[i], level + 1, width, scroll, scrollLayoutHeight, isGUIEvent, isOrderChanged);
						
					}
				}
			}
		}
	}

}