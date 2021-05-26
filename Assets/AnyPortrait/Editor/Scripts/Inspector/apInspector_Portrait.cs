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
using UnityEditor;
//using UnityEngine.Profiling;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Timeline;
using UnityEngine.Playables;
#endif

using AnyPortrait;

namespace AnyPortrait
{

	[CustomEditor(typeof(apPortrait))]
	public class apInspector_Portrait : Editor
	{
		private apPortrait _targetPortrait = null;
		private apControlParam.CATEGORY _curControlCategory = apControlParam.CATEGORY.Etc;
		private bool _showBaseInspector = false;
		private List<apControlParam> _controlParams = null;

		//private bool _isFold_BasicSettings = false;
		private bool _isFold_RootPortraits = false;
		private bool _isFold_AnimationClips = false;

		//추가 3.4
#if UNITY_2017_1_OR_NEWER
		private bool _isFold_Timeline = false;
		private int _nTimelineTrackSet = 0;
#endif
		//private bool _isFold_ConrolParameters = false;

		//3.7 추가 : 이미지들
		private bool _isImageLoaded = false;
		private Texture2D _img_EditorIsOpen = null;

		private Texture2D _img_OpenEditor = null;
		private Texture2D _img_QuickBake = null;
		private Texture2D _img_RefreshMeshes = null;
		private Texture2D _img_PrefabAsset = null;

		private Texture2D _img_BasicSettings = null;
		private Texture2D _img_Prefab = null;
		private Texture2D _img_RootPortraits = null;
		private Texture2D _img_AnimationSettings = null;
		private Texture2D _img_Mecanim = null;
#if UNITY_2017_1_OR_NEWER
		private Texture2D _img_Timeline = null;
#endif
		private Texture2D _img_ControlParams = null;

		private GUIContent _guiContent_EditorIsOpen = null;
		private GUIContent _guiContent_OpenEditor = null;
		private GUIContent _guiContent_QuickBake = null;
		private GUIContent _guiContent_RefreshMeshes = null;
		private GUIContent _guiContent_PrefabAsset = null;

		private GUIContent _guiContent_BasicSettings = null;
		private GUIContent _guiContent_Prefab = null;
		private GUIContent _guiContent_RootPortraits = null;
		private GUIContent _guiContent_AnimationSettings = null;

		private GUIContent _guiContent_Mecanim = null;
#if UNITY_2017_1_OR_NEWER
		private GUIContent _guiContent_Timeline = null;
#endif

		private GUIContent _guiContent_ControlParams = null;
		
		private GUIStyle _guiStyle_buttonIcon = null;
		private GUIStyle _guiStyle_subTitle = null;
		private GUIStyle _guiStyle_subBox = null;
		
		private apGUIContentWrapper _guiContent_Category = null;
		

		//추가 20.4.21 : 이미지 경로
		private apPathSetting _pathSetting = null;
		private string _basePath = "";


		//추가 20.9.14 : 대상 Portrait의 프리팹 여부
		private bool _isPrefabAsset = false;
		private bool _isPrefabInstance = false;
		private apEditorUtil.PREFAB_STATUS _prefabStatus = apEditorUtil.PREFAB_STATUS.NoPrefab;
		private UnityEngine.Object _srcPrefabObject = null;
		private GameObject _rootGameObjAsPrefabInstance = null;


		void OnEnable()
		{
			_targetPortrait = null;

			//_isFold_BasicSettings = true;
			_isFold_RootPortraits = true;
			_isFold_AnimationClips = true;
			//_isFold_ConrolParameters = true;

			//추가 3.4
#if UNITY_2017_1_OR_NEWER
			_isFold_Timeline = true;//<<
			_nTimelineTrackSet = 0;
#endif
		}

		private void LoadImages()
		{
			
			if (_isImageLoaded)
			{
				return;
			}

			if(_pathSetting == null)
			{
				_pathSetting = new apPathSetting();
			}
			_basePath = _pathSetting.Load();

			_img_EditorIsOpen = LoadImage("InspectorIcon_EditorIsOpen");

			_img_OpenEditor = LoadImage("InspectorIcon_OpenEditor");
			_img_QuickBake = LoadImage("InspectorIcon_QuickBake");
			_img_RefreshMeshes = LoadImage("InspectorIcon_RefreshMeshes");
			_img_PrefabAsset = LoadImage("InspectorIcon_PrefabAsset");

			_img_BasicSettings = LoadImage("InspectorIcon_BasicSettings");
			_img_Prefab = LoadImage("InspectorIcon_Prefab");
			_img_RootPortraits = LoadImage("InspectorIcon_RootPortraits");
			_img_AnimationSettings = LoadImage("InspectorIcon_AnimationSettings");
			_img_Mecanim = LoadImage("InspectorIcon_Mecanim");
#if UNITY_2017_1_OR_NEWER
			_img_Timeline = LoadImage("InspectorIcon_Timeline");
#endif
			_img_ControlParams = LoadImage("InspectorIcon_ControlParams");

			


			_guiContent_EditorIsOpen = new GUIContent("  Editor is opened", _img_EditorIsOpen);
			_guiContent_OpenEditor = new GUIContent(_img_OpenEditor);
			_guiContent_QuickBake = new GUIContent(_img_QuickBake);
			_guiContent_RefreshMeshes = new GUIContent(_img_RefreshMeshes);
			_guiContent_PrefabAsset = new GUIContent("  Prefab Asset is selected", _img_PrefabAsset);

			_guiContent_BasicSettings = new GUIContent("  Basic Settings", _img_BasicSettings);
			_guiContent_Prefab = new GUIContent("  Prefab", _img_Prefab);
			_guiContent_RootPortraits = new GUIContent("  Root Portraits", _img_RootPortraits);
			_guiContent_AnimationSettings = new GUIContent("  Animation Settings", _img_AnimationSettings);

			_guiContent_Mecanim = new GUIContent("  Mecanim Settings", _img_Mecanim);
#if UNITY_2017_1_OR_NEWER
			_guiContent_Timeline = new GUIContent("  Timeline Settings", _img_Timeline);
#endif

			_guiContent_ControlParams = new GUIContent("  Control Parameters", _img_ControlParams);

			
			_guiStyle_buttonIcon = new GUIStyle(GUI.skin.label);
			_guiStyle_buttonIcon.alignment = TextAnchor.MiddleCenter;

			_guiStyle_subTitle = new GUIStyle(GUI.skin.box);
			_guiStyle_subTitle.alignment = TextAnchor.MiddleCenter;
			_guiStyle_subTitle.margin = new RectOffset(0, 0, 0, 0);
			_guiStyle_subTitle.padding = new RectOffset(0, 0, 0, 0);

			_guiStyle_subBox = new GUIStyle(GUI.skin.box);
			_guiStyle_subBox.alignment = TextAnchor.MiddleCenter;
			_guiStyle_subBox.padding = new RectOffset(0, 0, 2, 2);

			_isImageLoaded = true;
		}

		public override void OnInspectorGUI()
		{
			//return;
			LoadImages();
			

			//base.OnInspectorGUI();
			apPortrait targetPortrait = target as apPortrait;

			if (targetPortrait != _targetPortrait)
			{
				_targetPortrait = targetPortrait;
				Init();
			}
			if (_targetPortrait == null)
			{
				//Profiler.EndSample();
				return;
			}

			//Profiler.BeginSample("anyPortrait Inspector GUI");


			//return;
			if (apEditor.IsOpen())
			{
				//에디터가 작동중에는 안보이도록 하자
				//EditorGUILayout.LabelField("Editor is opened");
				GUILayout.Space(10);
				
				EditorGUILayout.LabelField(_guiContent_EditorIsOpen, GUILayout.Height(36));

				//Profiler.EndSample();

				return;
			}

			try
			{
				bool request_OpenEditor = false;
				bool request_QuickBake = false;
				bool request_RefreshMeshes = false;
				bool prevImportant = _targetPortrait._isImportant;
				MonoBehaviour prevAnimEventListener = _targetPortrait._optAnimEventListener;
				int prevSortingLayerID = _targetPortrait._sortingLayerID;
				apPortrait.SORTING_ORDER_OPTION prevSortingOrderOption = _targetPortrait._sortingOrderOption;
				int prevSortingOrder = _targetPortrait._sortingOrder;
				int prevOrderPerDepth = _targetPortrait._sortingOrderPerDepth;//추가 21.1.31

				if (_isPrefabAsset)
				{
					//추가 20.9.15 : 만약 프리팹 에셋이라면 에디터를 열 수 없다.
					GUILayout.Space(10);
				
					EditorGUILayout.LabelField(_guiContent_PrefabAsset, GUILayout.Height(36));

					Color prevBackColor = GUI.backgroundColor;
							GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							GUILayout.Box("Prefab Assets cannot be edited.\nPlace the Prefab in the Scene as an Instance.",
								_guiStyle_subTitle,
								GUILayout.Width((int)EditorGUIUtility.currentViewWidth - 36), GUILayout.Height(40));
							GUI.backgroundColor = prevBackColor;
				}
				else
				{
					if (!EditorApplication.isPlaying)
					{
						int iconWidth = 32;
						int iconHeight = 34;
						int buttonHeight = 34;

						//추가 19.5.26 : 용량 최적화 기능이 추가되었는가
						if (!_targetPortrait._isSizeOptimizedV117)
						{
							GUILayout.Space(10);

							Color prevBackColor = GUI.backgroundColor;
							GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							GUILayout.Box("[Bake] was not executed.\nExecute the [Bake] again.",
								_guiStyle_subTitle,
								GUILayout.Width((int)EditorGUIUtility.currentViewWidth - 36), GUILayout.Height(40));
							GUI.backgroundColor = prevBackColor;
						}

						if (!_targetPortrait._isOptimizedPortrait)
						{
							GUILayout.Space(10);

							EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							EditorGUILayout.LabelField(_guiContent_OpenEditor, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							if (GUILayout.Button("Open Editor and Select", GUILayout.Height(buttonHeight)))
							{
								request_OpenEditor = true;
							}
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							EditorGUILayout.LabelField(_guiContent_QuickBake, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							if (GUILayout.Button("Quick Bake", GUILayout.Height(buttonHeight)))
							{
								request_QuickBake = true;
							}
							EditorGUILayout.EndHorizontal();
						}
						else
						{
							GUILayout.Space(10);

							EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							EditorGUILayout.LabelField(_guiContent_OpenEditor, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
							GUILayout.Space(5);
							if (GUILayout.Button("Open Editor (Not Selectable)", GUILayout.Height(buttonHeight)))
							{
								//열기만 하고 선택은 못함
								request_OpenEditor = true;
							}
							EditorGUILayout.EndHorizontal();
						}
						//추가 12.18 : Mesh를 리프레시 하자

						EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_guiContent_RefreshMeshes, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						if (GUILayout.Button("Refresh Meshes", GUILayout.Height(buttonHeight)))
						{
							request_RefreshMeshes = true;
						}
						EditorGUILayout.EndHorizontal();


					}
				}
				

				GUILayout.Space(10);

				
				int width = (int)EditorGUIUtility.currentViewWidth;
				int subTitleWidth = width - 44;
				int subTitleHeight = 26;


				//BasicSettings
				//-----------------------------------------------------------------------------
				//"Basic Settings"
				
				
				
				GUILayout.Box(_guiContent_BasicSettings, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));


				_targetPortrait._isImportant = EditorGUILayout.Toggle("Is Important", _targetPortrait._isImportant);
				_targetPortrait._optAnimEventListener = (MonoBehaviour)EditorGUILayout.ObjectField("Event Listener", _targetPortrait._optAnimEventListener, typeof(MonoBehaviour), true);


				GUILayout.Space(5);
				//추가3.22
				//Sorting Layer
				string[] sortingLayerName = new string[SortingLayer.layers.Length];
				int layerIndex = -1;
				for (int i = 0; i < SortingLayer.layers.Length; i++)
				{
					sortingLayerName[i] = SortingLayer.layers[i].name;
					if (SortingLayer.layers[i].id == _targetPortrait._sortingLayerID)
					{
						layerIndex = i;
					}
				}
				int nextLayerIndex = EditorGUILayout.Popup("Sorting Layer", layerIndex, sortingLayerName);
				apPortrait.SORTING_ORDER_OPTION nextSortingOption = (apPortrait.SORTING_ORDER_OPTION)EditorGUILayout.EnumPopup("Sorting Order Option", _targetPortrait._sortingOrderOption);

				int nextLayerOrder = _targetPortrait._sortingOrder;
				if (_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
				{
					nextLayerOrder = EditorGUILayout.IntField("Sorting Order", _targetPortrait._sortingOrder);

					if (nextLayerOrder != _targetPortrait._sortingOrder)
					{
						_targetPortrait.SetSortingOrder(nextLayerOrder);
					}
				}
				else if(_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder 
					|| _targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					//추가 21.1.31 : Depth To Order일때, 1씩만 증가하는게 아닌 더 큰값으로 증가할 수도 있게 만들자
					int nextOrderPerDepth = EditorGUILayout.IntField("Order Per Depth", _targetPortrait._sortingOrderPerDepth);
					if(nextOrderPerDepth != _targetPortrait._sortingOrderPerDepth)
					{
						if(nextOrderPerDepth < 1)
						{
							nextOrderPerDepth = 1;
						}

						_targetPortrait._sortingOrderPerDepth = nextOrderPerDepth;

						//변경된 Sorting Order Option에 따라서 바로 Sorting을 해야한다.
						_targetPortrait.ApplySortingOptionToOptRootUnits();
					}
				}
				

				if (nextLayerIndex != layerIndex)
				{
					//Sorting Layer를 바꾸자
					if (nextLayerIndex >= 0 && nextLayerIndex < SortingLayer.layers.Length)
					{
						string nextLayerName = SortingLayer.layers[nextLayerIndex].name;
						_targetPortrait.SetSortingLayer(nextLayerName);
					}
				}
				if(nextSortingOption != _targetPortrait._sortingOrderOption)
				{
					_targetPortrait._sortingOrderOption = nextSortingOption;
					//변경된 Sorting Order Option에 따라서 바로 Sorting을 해야한다.
					_targetPortrait.ApplySortingOptionToOptRootUnits();

					switch (_targetPortrait._sortingOrderOption)
					{
						case apPortrait.SORTING_ORDER_OPTION.SetOrder:
							_targetPortrait.SetSortingOrder(_targetPortrait._sortingOrder);
							break;

						case apPortrait.SORTING_ORDER_OPTION.DepthToOrder:
						case apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder:
							_targetPortrait.SetSortingOrderChangedAutomatically(true);
							_targetPortrait.RefreshSortingOrderByDepth();
							break;
					}
				}
				



				if (prevImportant != _targetPortrait._isImportant ||
					prevAnimEventListener != _targetPortrait._optAnimEventListener ||
					prevSortingLayerID != _targetPortrait._sortingLayerID ||
					prevSortingOrderOption != _targetPortrait._sortingOrderOption ||
					prevSortingOrder != _targetPortrait._sortingOrder ||
					prevOrderPerDepth != _targetPortrait._sortingOrderPerDepth)
				{
					apEditorUtil.SetEditorDirty();
				}


				GUILayout.Space(5);

				//빌보드
				apPortrait.BILLBOARD_TYPE nextBillboard = (apPortrait.BILLBOARD_TYPE)EditorGUILayout.EnumPopup("Billboard Type", _targetPortrait._billboardType);
				if (nextBillboard != _targetPortrait._billboardType)
				{
					_targetPortrait._billboardType = nextBillboard;
					apEditorUtil.SetEditorDirty();
				}

				GUILayout.Space(20);



				// Prefab (유효한 경우만)
				//-----------------------------------------------------------------------------
				if (_isPrefabInstance)
				{	
					GUILayout.Box(_guiContent_Prefab, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));

					
					Color prevBackColor = GUI.backgroundColor;

					//연결 상태를 보여주자
					string strStatus = null;
					switch(_prefabStatus)
					{
						case apEditorUtil.PREFAB_STATUS.Connected:
							GUI.backgroundColor = new Color(0.7f, 1.0f, 1.0f, 1.0f);
							strStatus = "Source Prefab";
							break;

						case apEditorUtil.PREFAB_STATUS.Disconnected:
							GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							strStatus = "(Disconnected)";
							break;

						case apEditorUtil.PREFAB_STATUS.Missing:
						default:
							GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							strStatus = "(Missing)";
							break;
					}
					EditorGUILayout.ObjectField(strStatus, _srcPrefabObject, typeof(UnityEngine.Object), false);
					EditorGUILayout.ObjectField("Root GameObject", _rootGameObjAsPrefabInstance, typeof(GameObject), false);
					GUI.backgroundColor = prevBackColor;

					int width_PrefabButtons = (width / 2) - 24;

					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(5);
					if (GUILayout.Button("Apply", GUILayout.Width(width_PrefabButtons)))
					{
						//프리팹 변경 내용을 저장하자
						apEditorUtil.ApplyPrefab(_targetPortrait);
						RefreshPrefabStatus();
					}
					if (GUILayout.Button("Refresh", GUILayout.Width(width_PrefabButtons)))
					{
						//프리팹 연결 정보를 갱신한다.
						RefreshPrefabStatus();
					}

					
					EditorGUILayout.EndHorizontal();

					//Disconnect를 할 수 있다.
					//Legacy : 단순 Disconnect를 할 수 있다.
					//2018.3 : Disconnect를 한 후, 복원 정보를 모두 삭제할 수 있다.
#if UNITY_2018_3_OR_NEWER
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(5);
					
					if (GUILayout.Button("Disconnect", GUILayout.Width(width_PrefabButtons)))
					{
						//Disconnect를 하되, 연결 정보는 남겨둔다.
						if(EditorUtility.DisplayDialog(
												"Disconnecting from Prefab", 
												"Are you sure you want to disconnect this Portrait from the Prefab Asset?", 
												"Disconnect", "Cancel"))
						{
							apEditorUtil.CheckAndRefreshPrefabInfo(_targetPortrait);
							apEditorUtil.DisconnectPrefab(_targetPortrait);
							RefreshPrefabStatus();
						}
						
					}

					if (GUILayout.Button("Clear", GUILayout.Width(width_PrefabButtons)))
					{
						//Disconnect를 하고, 연결 정보를 삭제한다.
						if(EditorUtility.DisplayDialog(
												"Disconnecting from Prefab", 
												"Are you sure you want to disconnect this Portrait from the Prefab Asset?\nThis completely deletes the connection data with the Prefab.", 
												"Disconnect and Clear", "Cancel"))
						{
							apEditorUtil.DisconnectPrefab(_targetPortrait, true);
							RefreshPrefabStatus();
						}
					}
					EditorGUILayout.EndHorizontal();
#else
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(5);
					if (GUILayout.Button("Disconnect", GUILayout.Width(width - 45)))
					{
						//Disconnect를 하되, 연결 정보는 남겨둔다.
						if(EditorUtility.DisplayDialog(
												"Disconnecting from Prefab", 
												"Are you sure you want to disconnect this Portrait from the Prefab Asset?", 
												"Disconnect", "Cancel"))
						{
							apEditorUtil.CheckAndRefreshPrefabInfo(_targetPortrait);
							apEditorUtil.DisconnectPrefab(_targetPortrait);
							RefreshPrefabStatus();
						}
					}
					EditorGUILayout.EndHorizontal();
#endif

					GUILayout.Space(20);
				}
				



				// Root Portraits
				//-----------------------------------------------------------------------------
				GUILayout.Box(_guiContent_RootPortraits, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));

				_isFold_RootPortraits = EditorGUILayout.Foldout(_isFold_RootPortraits, "Portraits");
				if(_isFold_RootPortraits)
				{
					for (int i = 0; i < _targetPortrait._optRootUnitList.Count; i++)
					{
						apOptRootUnit rootUnit = _targetPortrait._optRootUnitList[i];
						EditorGUILayout.ObjectField("[" + i + "]", rootUnit, typeof(apOptRootUnit), true);
					}
				}

				GUILayout.Space(20);


				// Animation Settings
				//-----------------------------------------------------------------------------

				GUILayout.Box(_guiContent_AnimationSettings, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));

				_isFold_AnimationClips = EditorGUILayout.Foldout(_isFold_AnimationClips, "Animation Clips");
				if(_isFold_AnimationClips)
				{
					for (int i = 0; i < _targetPortrait._animClips.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(5);
						apAnimClip animClip = _targetPortrait._animClips[i];
						if(animClip._uniqueID == _targetPortrait._autoPlayAnimClipID)
						{
							EditorGUILayout.LabelField("[" + i + "] (Auto)", GUILayout.Width(80));
						}
						else
						{
							EditorGUILayout.LabelField("[" + i + "]", GUILayout.Width(80));
						}
						EditorGUILayout.TextField(animClip._name);
						try
						{
							AnimationClip nextAnimationClip = EditorGUILayout.ObjectField(animClip._animationClipForMecanim, typeof(AnimationClip), false) as AnimationClip;
							if(nextAnimationClip != animClip._animationClipForMecanim)
							{
								UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
								Undo.IncrementCurrentGroup();
								Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Changed");
								
								animClip._animationClipForMecanim = nextAnimationClip;
							}
						}
						catch (Exception)
						{ }
						
						EditorGUILayout.EndHorizontal();
					}
				}

				GUILayout.Space(10);

				AnimationClip nextEmptyAnimClip = EditorGUILayout.ObjectField("Empty Anim Clip", _targetPortrait._emptyAnimClipForMecanim, typeof(AnimationClip), false) as AnimationClip;
				if (nextEmptyAnimClip != _targetPortrait._emptyAnimClipForMecanim)
				{
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
					Undo.IncrementCurrentGroup();
					Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Changed");

					_targetPortrait._emptyAnimClipForMecanim = nextEmptyAnimClip;
				}

				GUILayout.Space(10);

				//EditorGUILayout.LabelField("Mecanim Settings");
				EditorGUILayout.LabelField(_guiContent_Mecanim, GUILayout.Height(24));
				
				bool isNextUsingMecanim = EditorGUILayout.Toggle("Use Mecanim", _targetPortrait._isUsingMecanim);
				if (_targetPortrait._isUsingMecanim != isNextUsingMecanim)
				{
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
					Undo.IncrementCurrentGroup();
					Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

					_targetPortrait._isUsingMecanim = isNextUsingMecanim;
				}


				if(_targetPortrait._isUsingMecanim)
				{
					//GUILayout.Space(10);
					try
					{
						Animator nextAnimator = EditorGUILayout.ObjectField("Animator", _targetPortrait._animator, typeof(Animator), true) as Animator;
						if (nextAnimator != _targetPortrait._animator)
						{
							//하위에 있는 Component일 때에만 변동 가능
							if (nextAnimator == null)
							{
								_targetPortrait._animator = null;
							}
							else
							{
								if (nextAnimator == _targetPortrait.GetComponent<Animator>())
								{
									_targetPortrait._animator = nextAnimator;
								}
								else
								{
									EditorUtility.DisplayDialog("Invalid Animator", "Invalid Animator. Only the Animator, which is its own component, is valid.", "Okay");

								}
							}

						}
					}
					catch(Exception)
					{

					}
					if (_targetPortrait._animator == null)
					{
						//1. Animator가 없다면
						// > 생성하기
						// > 생성되어 있다면 다시 링크
						GUIStyle guiStyle_WarningText = new GUIStyle(GUI.skin.label);
						guiStyle_WarningText.normal.textColor = Color.red;
						EditorGUILayout.LabelField("Warning : No Animator!", guiStyle_WarningText);
						GUILayout.Space(5);

						if(GUILayout.Button("Add / Check Animator", GUILayout.Height(25)))
						{
							UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

							Animator animator = _targetPortrait.gameObject.GetComponent<Animator>();
							if(animator == null)
							{
								animator = _targetPortrait.gameObject.AddComponent<Animator>();
							}
							_targetPortrait._animator = animator;
						}
					}
					else
					{
						//2. Animator가 있다면
						if (GUILayout.Button("Refresh Layers", GUILayout.Height(25)))
						{
							UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

							//Animator의 Controller가 있는지 체크해야한다.
								
							if(_targetPortrait._animator.runtimeAnimatorController == null)
							{
								//AnimatorController가 없다면 Layer는 초기화
								_targetPortrait._animatorLayerBakedData.Clear();
							}
							else
							{
								//AnimatorController가 있다면 레이어에 맞게 설정
								_targetPortrait._animatorLayerBakedData.Clear();
								UnityEditor.Animations.AnimatorController animatorController = _targetPortrait._animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

								if(animatorController != null && animatorController.layers.Length > 0)
								{
									for (int iLayer = 0; iLayer < animatorController.layers.Length; iLayer++)
									{
										apAnimMecanimData_Layer newLayerData = new apAnimMecanimData_Layer();
										newLayerData._layerIndex = iLayer;
										newLayerData._layerName = animatorController.layers[iLayer].name;
										newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Unknown;
										switch (animatorController.layers[iLayer].blendingMode)
										{
											case UnityEditor.Animations.AnimatorLayerBlendingMode.Override:
												newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Override;
												break;

											case UnityEditor.Animations.AnimatorLayerBlendingMode.Additive:
												newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Additive;
												break;
										}

										_targetPortrait._animatorLayerBakedData.Add(newLayerData);
									}
								}
							}
						}
						GUILayout.Space(5);
						EditorGUILayout.LabelField("Animator Controller Layers");
						for (int i = 0; i < _targetPortrait._animatorLayerBakedData.Count; i++)
						{
							apAnimMecanimData_Layer layer = _targetPortrait._animatorLayerBakedData[i];
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(5);
							EditorGUILayout.LabelField("[" + layer._layerIndex + "]", GUILayout.Width(50));
							EditorGUILayout.TextField(layer._layerName);
							apAnimMecanimData_Layer.MecanimLayerBlendType nextBlendType = (apAnimMecanimData_Layer.MecanimLayerBlendType)EditorGUILayout.EnumPopup(layer._blendType);
							EditorGUILayout.EndHorizontal();

							if (nextBlendType != layer._blendType)
							{
								UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
								Undo.IncrementCurrentGroup();
								Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

								_targetPortrait._animatorLayerBakedData[i]._blendType = nextBlendType;
							}
						}
					}
						
				}


				GUILayout.Space(20);


				//추가 3.4 : 타임라인 설정
#if UNITY_2017_1_OR_NEWER

				EditorGUILayout.LabelField(_guiContent_Timeline, GUILayout.Height(24));

				_isFold_Timeline = EditorGUILayout.Foldout(_isFold_Timeline, "Track Data");
				if(_isFold_Timeline)
				{
					
					int nextTimelineTracks = EditorGUILayout.DelayedIntField("Size", _nTimelineTrackSet);
					if(nextTimelineTracks != _nTimelineTrackSet)
					{
						//TimelineTrackSet의 개수가 바뀌었다. 
						UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
						Undo.IncrementCurrentGroup();
						Undo.RegisterCompleteObjectUndo(_targetPortrait, "Track Setting Changed");
						_nTimelineTrackSet = nextTimelineTracks;
						if(_nTimelineTrackSet < 0)
						{
							_nTimelineTrackSet = 0;
						}

						//일단 이전 개수만큼 복사를 한다.
						int nPrev = 0;
						List<apPortrait.TimelineTrackPreset> prevSets = new List<apPortrait.TimelineTrackPreset>();
						if(targetPortrait._timelineTrackSets != null && targetPortrait._timelineTrackSets.Length > 0)
						{
							for (int i = 0; i < targetPortrait._timelineTrackSets.Length; i++)
							{
								prevSets.Add(targetPortrait._timelineTrackSets[i]);
							}
							nPrev = targetPortrait._timelineTrackSets.Length;
						}
						
						//배열을 새로 만들자
						targetPortrait._timelineTrackSets = new apPortrait.TimelineTrackPreset[_nTimelineTrackSet];

						//가능한 이전 소스를 복사한다.
						for (int i = 0; i < _nTimelineTrackSet; i++)
						{
							if(i < nPrev)
							{
								targetPortrait._timelineTrackSets[i] = new apPortrait.TimelineTrackPreset();
								targetPortrait._timelineTrackSets[i]._playableDirector = prevSets[i]._playableDirector;
								targetPortrait._timelineTrackSets[i]._trackName = prevSets[i]._trackName;
								targetPortrait._timelineTrackSets[i]._layer = prevSets[i]._layer;
								targetPortrait._timelineTrackSets[i]._blendMethod = prevSets[i]._blendMethod;
							}
							else
							{
								targetPortrait._timelineTrackSets[i] = new apPortrait.TimelineTrackPreset();
							}
						}


						apEditorUtil.ReleaseGUIFocus();
						
					}

					GUILayout.Space(5);

					if(targetPortrait._timelineTrackSets != null)
					{
						apPortrait.TimelineTrackPreset curTrackSet = null;
						for (int i = 0; i < targetPortrait._timelineTrackSets.Length; i++)
						{
							//트랙을 하나씩 적용
							curTrackSet = targetPortrait._timelineTrackSets[i];
							
							EditorGUILayout.LabelField("[" + i + "] : " + (curTrackSet._playableDirector == null ? "<None>" : curTrackSet._playableDirector.name));
							PlayableDirector nextDirector = EditorGUILayout.ObjectField("Director", curTrackSet._playableDirector, typeof(PlayableDirector), true) as PlayableDirector;
							string nextTrackName = EditorGUILayout.DelayedTextField("Track Name", curTrackSet._trackName);
							int nextLayer = EditorGUILayout.DelayedIntField("Layer", curTrackSet._layer);
							apAnimPlayUnit.BLEND_METHOD nextBlendMethod = (apAnimPlayUnit.BLEND_METHOD)EditorGUILayout.EnumPopup("Blend", curTrackSet._blendMethod);

							if(nextDirector != curTrackSet._playableDirector 
								|| nextTrackName != curTrackSet._trackName
								|| nextLayer != curTrackSet._layer
								|| nextBlendMethod != curTrackSet._blendMethod
								)
							{
								UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
								Undo.IncrementCurrentGroup();
								Undo.RegisterCompleteObjectUndo(_targetPortrait, "Track Setting Changed");

								curTrackSet._playableDirector = nextDirector;
								curTrackSet._trackName = nextTrackName;
								curTrackSet._layer = nextLayer;
								curTrackSet._blendMethod = nextBlendMethod;

								apEditorUtil.ReleaseGUIFocus();
							}

							GUILayout.Space(5);
						}
					}
				}

				GUILayout.Space(20);
#endif
				
				bool isChanged = false;

				// Control Parameters
				//-----------------------------------------------------------------------------

				if(_guiContent_Category == null)
				{
					_guiContent_Category = apGUIContentWrapper.Make("Category", false);
				}
				

				GUILayout.Box(_guiContent_ControlParams, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));

#if UNITY_2017_3_OR_NEWER
				_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumFlagsField(_guiContent_Category.Content, _curControlCategory);
#else
				_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(_guiContent_Category.Content, _curControlCategory);
#endif

				EditorGUILayout.Space();
				//1. 컨르롤러를 제어할 수 있도록 하자
					
				if (_controlParams != null)
				{
					for (int i = 0; i < _controlParams.Count; i++)
					{
						if ((int)(_controlParams[i]._category & _curControlCategory) != 0)
						{
							if (GUI_ControlParam(_controlParams[i]))
							{
								isChanged = true;
							}
						}
					}
				}

				GUILayout.Space(30);

				//2. 토글 버튼을 두어서 기본 Inspector 출력 여부를 결정하자.
				string strBaseButton = "Show All Properties";
				if (_showBaseInspector)
				{
					strBaseButton = "Hide Properties";
				}

				if (GUILayout.Button(strBaseButton, GUILayout.Height(20)))
				{
					_showBaseInspector = !_showBaseInspector;
				}

				if (_showBaseInspector)
				{
					base.OnInspectorGUI();
				}


				if (!Application.isPlaying && isChanged)
				{
					//플레이 중이라면 자동으로 업데이트 될 것이다.
					_targetPortrait.UpdateForce();
				}

				if (_targetPortrait != null)
				{	
					if (request_OpenEditor)
					{
						if(_targetPortrait._isOptimizedPortrait)
						{
							RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.Open);
						}
						else
						{
							RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.OpenAndSet);
						}
						//apEditor anyPortraitEditor = apEditor.ShowWindow();
						//if (anyPortraitEditor != null && !_targetPortrait._isOptimizedPortrait)
						//{
						//	anyPortraitEditor.SetPortraitByInspector(_targetPortrait, false);
						//}
					}
					else if (request_QuickBake)
					{
						RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.QuickBake);
						//apEditor anyPortraitEditor = apEditor.ShowWindow();
						//if (anyPortraitEditor != null)
						//{
						//	anyPortraitEditor.SetPortraitByInspector(_targetPortrait, true);

						//	Selection.activeObject = _targetPortrait.gameObject;
						//}
					}
					else if(request_RefreshMeshes)
					{
						_targetPortrait.OnMeshResetInEditor();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("apInspector_Portrait Exception : " + ex);
			}

			//Profiler.EndSample();
		}



		private void Init()
		{
			_curControlCategory = apControlParam.CATEGORY.Head |
									apControlParam.CATEGORY.Body |
									apControlParam.CATEGORY.Face |
									apControlParam.CATEGORY.Hair |
									apControlParam.CATEGORY.Equipment |
									apControlParam.CATEGORY.Force |
									apControlParam.CATEGORY.Etc;

			_showBaseInspector = false;

			//_isFold_BasicSettings = true;
			//_isFold_BasicSettings = true;
			_isFold_RootPortraits = true;
			//_isFold_AnimationSettings = true;
			_isFold_AnimationClips = true;
			//_isFold_ConrolParameters = true;

			_controlParams = null;
			if (_targetPortrait._controller != null)
			{
				_controlParams = _targetPortrait._controller._controlParams;
			}


			_requestPortrait = null;
			_requestType = REQUEST_TYPE.None;
			_coroutine = null;

#if UNITY_2017_1_OR_NEWER
			_nTimelineTrackSet = (_targetPortrait._timelineTrackSets == null) ? 0 :_targetPortrait._timelineTrackSets.Length;
#endif

			EditorApplication.update -= ExecuteCoroutine;


			RefreshPrefabStatus();
		}


		private void RefreshPrefabStatus()
		{
			//추가 20.9.14 : 프리팹 여부 체크하기
			_isPrefabAsset = false;
			_isPrefabInstance = false;
			_srcPrefabObject = null;
			_rootGameObjAsPrefabInstance = null;

			//프리팹 에셋이면 아무것도 편집 불가
			_isPrefabAsset = apEditorUtil.IsPrefabAsset(_targetPortrait.gameObject);
			if(_isPrefabAsset)
			{
				return;
			}


			_prefabStatus = apEditorUtil.GetPrefabStatus(_targetPortrait.gameObject);

			switch (_prefabStatus)
			{
				case apEditorUtil.PREFAB_STATUS.NoPrefab:
#if UNITY_2018_3_OR_NEWER
					//이 상태에서 2018.3버전의 프리팹인 경우, 복원 정보가 저장되어 있다면 Disconnected로 변경한다.
					//만약 Disconnected라면 이 정보가 없을 수 있다. (Connected라면 연결되어 있을까?)
					//복원용 정보를 입력하고 Disconnected 상태로 만들자.
					_srcPrefabObject = _targetPortrait._srcPrefabAssetForRestore;
					_rootGameObjAsPrefabInstance = _targetPortrait._rootGameObjectAsPrefabInstanceForRestore;

					if(_srcPrefabObject != null && _rootGameObjAsPrefabInstance != null)
					{
						//둘다 있는 경우에 한해서 Disconnected로 변경
						_isPrefabInstance = true;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					else
					{
						//그렇지 않다면 초기화
						_srcPrefabObject = null;
						_rootGameObjAsPrefabInstance = null;
					}
#endif
					break;

				case apEditorUtil.PREFAB_STATUS.Connected:
				case apEditorUtil.PREFAB_STATUS.Disconnected:
					_isPrefabInstance = true;
					_srcPrefabObject = apEditorUtil.GetPrefabObject(_targetPortrait.gameObject);
					_rootGameObjAsPrefabInstance = apEditorUtil.GetRootGameObjectAsPrefabInstance(_targetPortrait.gameObject);
					
					//만약 Disconnected라면 이 정보가 없을 수 있다. (Connected라면 연결되어 있을까?)
					//복원용 정보를 입력하고 Disconnected 상태로 만들자.
					if(_srcPrefabObject == null)
					{
						_srcPrefabObject = _targetPortrait._srcPrefabAssetForRestore;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					if(_rootGameObjAsPrefabInstance == null)
					{
						_rootGameObjAsPrefabInstance = _targetPortrait._rootGameObjectAsPrefabInstanceForRestore;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					break;

				case apEditorUtil.PREFAB_STATUS.Missing:
					_isPrefabInstance = true;
					break;
			}
		}



		private bool GUI_ControlParam(apControlParam controlParam)
		{
			if (controlParam == null)
			{ return false; }

			bool isChanged = false;

			EditorGUILayout.LabelField(controlParam._keyName);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool bPrev = controlParam._bool_Cur;
				//		controlParam._bool_Cur = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(bPrev != controlParam._bool_Cur)
				//		{
				//			isChanged = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int iPrev = controlParam._int_Cur;
						controlParam._int_Cur = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);

						if (iPrev != controlParam._int_Cur)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float fPrev = controlParam._float_Cur;
						controlParam._float_Cur = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						if (Mathf.Abs(fPrev - controlParam._float_Cur) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 v2Prev = controlParam._vec2_Cur;
						controlParam._vec2_Cur.x = EditorGUILayout.Slider(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						controlParam._vec2_Cur.y = EditorGUILayout.Slider(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						if (Mathf.Abs(v2Prev.x - controlParam._vec2_Cur.x) > 0.0001f ||
							Mathf.Abs(v2Prev.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

			}

			GUILayout.Space(5);

			return isChanged;
		}


		private apPortrait _requestPortrait = null;
		private enum REQUEST_TYPE
		{
			None,
			Open,
			OpenAndSet,
			QuickBake
		}
		private REQUEST_TYPE _requestType = REQUEST_TYPE.None;
		private IEnumerator _coroutine = null;
		

		private void RequestDelayedOpenEditor(apPortrait portrait, REQUEST_TYPE requestType)
		{
			if(_coroutine != null)
			{
				return;
			}

			_requestPortrait = portrait;
			_requestType = requestType;
			_coroutine = Crt_RequestEditor();

			EditorApplication.update -= ExecuteCoroutine;
			EditorApplication.update += ExecuteCoroutine;
		}

		private void ExecuteCoroutine()
		{
			if(_coroutine == null)
			{
				_requestType = REQUEST_TYPE.None;
				_requestPortrait = null;

				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

			//Debug.Log("Update Coroutine");
			bool isResult = _coroutine.MoveNext();
			
			if(!isResult)
			{
				_coroutine = null;
				_requestType = REQUEST_TYPE.None;
				_requestPortrait = null;
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}
		}
		private IEnumerator Crt_RequestEditor()
		{
			yield return new WaitForEndOfFrame();
			Selection.activeObject = null;

			yield return new WaitForEndOfFrame();

			if (_requestPortrait != null)
			{	
				try
				{
					
					apEditor anyPortraitEditor = apEditor.ShowWindow();
					if (_requestType == REQUEST_TYPE.OpenAndSet)
					{
						anyPortraitEditor.SetPortraitByInspector(_requestPortrait, false);
					}
					else if (_requestType == REQUEST_TYPE.QuickBake)
					{
						anyPortraitEditor.SetPortraitByInspector(_requestPortrait, true);
						Selection.activeObject = _requestPortrait.gameObject;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Open Editor Error : " + ex);
				}
			}
			_requestType = REQUEST_TYPE.None;
			_requestPortrait = null;
		}


		private Texture2D LoadImage(string iconName)
		{
			//이전
			//return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AnyPortrait/Editor/Images/Inspector/" + iconName + ".png");
			//변경 20.4.21 : 경로 변경 설정을 따른다.
			//기본 경로는 "Assets/AnyPortrait/"이므로
			//경로면은 _basePath + "Editor/Images/Inspector/" + iconName + ".png"가 된다.
			return AssetDatabase.LoadAssetAtPath<Texture2D>(_basePath + "Editor/Images/Inspector/" + iconName + ".png");
		}
	}

}