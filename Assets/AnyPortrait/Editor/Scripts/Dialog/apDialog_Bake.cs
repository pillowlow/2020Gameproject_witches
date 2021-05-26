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

	public class apDialog_Bake : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_Bake s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;

		private string[] _colorSpaceNames = new string[] { "Gamma", "Linear" };

		
#if UNITY_2019_1_OR_NEWER
		private string[] _renderPipelineNames = new string[] { "Default", "Scriptable Render Pipeline" };
#endif

		private string[] _sortingLayerNames = null;
		private int[] _sortingLayerIDs = null;

		private string[] _billboardTypeNames = new string[] { "None", "Billboard", "Billboard with fixed Up Vector" };

		private string[] _vrSupportModeLabel = new string[] { "None", "Single Camera and Eye Textures (Unity VR)", "Multiple Cameras" };

		private string[] _flippedMeshOptionLabel = new string[] { "Check excluding Rigged Meshes", "Check All"};

		private string[] _rootBoneScaleOptionLabel = new string[] { "Default", "Non-Uniform Scale"};
		
#if UNITY_2019_1_OR_NEWER
		private string[] _vrRTSizeLabel = new string[] { "By Mesh Setting", "By Eye Texture Setting" };
#else
		private string[] _vrRTSizeLabel = new string[] { "By Mesh Setting", "By Eye Texture Setting (Not Supported)" };
#endif

		//추가 : 탭으로 분류하자
		//private Vector2 _scroll_Bake = Vector2.zero;
		private Vector2 _scroll_Options = Vector2.zero;

		private enum TAB
		{
			Bake,
			Options
		}
		private TAB _tab = TAB.Bake;


		// 추가 19.11.20 : GUIContent
		private apGUIContentWrapper _guiContent_Setting_IsImportant = null;
		private apGUIContentWrapper _guiContent_Setting_FPS = null;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_Bake), true, "Bake", true);
			apDialog_Bake curTool = curWindow as apDialog_Bake;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 380;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, portrait, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);
				}
				s_window = null;
			}
		}

		// Init
		//------------------------------------------------------------------
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;


		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}




			//Sorting Layer를 추가하자
			if (_sortingLayerNames == null || _sortingLayerIDs == null)
			{
				_sortingLayerNames = new string[SortingLayer.layers.Length];
				_sortingLayerIDs = new int[SortingLayer.layers.Length];
			}
			else if (_sortingLayerNames.Length != SortingLayer.layers.Length
				|| _sortingLayerIDs.Length != SortingLayer.layers.Length)
			{
				_sortingLayerNames = new string[SortingLayer.layers.Length];
				_sortingLayerIDs = new int[SortingLayer.layers.Length];
			}

			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				_sortingLayerNames[i] = SortingLayer.layers[i].name;
				_sortingLayerIDs[i] = SortingLayer.layers[i].id;
			}


			int width_2Btn = (width - 14) / 2;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Bake), _tab == TAB.Bake, width_2Btn, 25))
			{
				_tab = TAB.Bake;
			}
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Setting), _tab == TAB.Options, width_2Btn, 25))
			{
				_tab = TAB.Options;
			}
			EditorGUILayout.EndHorizontal();


			if (_tab == TAB.Bake)
			{
				GUILayout.Space(5);

				// 1. Bake에 대한 UI
				//Bake 설정
				//EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_BakeSetting));//"Bake Setting"
				//GUILayout.Space(5);

				EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Portrait), _targetPortrait, typeof(apPortrait), true);//"Portait"

				GUILayout.Space(5);

				//"Bake Scale"
				float prevBakeScale = _targetPortrait._bakeScale;
				_targetPortrait._bakeScale = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_BakeScale), _targetPortrait._bakeScale);

				//"Z Per Depth"
				float prevBakeZSize = _targetPortrait._bakeZSize;
				_targetPortrait._bakeZSize = EditorGUILayout.FloatField(_editor.GetText(TEXT.DLG_ZPerDepth), _targetPortrait._bakeZSize);

				if (_targetPortrait._bakeZSize < 0.5f)
				{
					_targetPortrait._bakeZSize = 0.5f;
				}

				if (prevBakeScale != _targetPortrait._bakeScale ||
					prevBakeZSize != _targetPortrait._bakeZSize)
				{
					apEditorUtil.SetEditorDirty();
				}


				//Bake 버튼
				GUILayout.Space(10);
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Bake), GUILayout.Height(45)))//"Bake"
				{
					GUI.FocusControl(null);

					//CheckChangedProperties(nextRootScale, nextZScale);
					apEditorUtil.SetEditorDirty();

					//-------------------------------------
					// Bake 함수를 실행한다. << 중요오오오오
					//-------------------------------------

					apBakeResult bakeResult = _editor.Controller.Bake();

					if (bakeResult != null)
					{
						_editor.Notification("[" + _targetPortrait.name + "] is Baked", false, false);

						if (bakeResult.NumUnlinkedExternalObject > 0)
						{
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
								_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
								_editor.GetText(TEXT.Okay));
						}

						//추가 3.29 : Bake 후에 Ambient를 체크하자
						CheckAmbientAndCorrection();
					}
					else
					{
						//추가 20.11.7 : Bake가 실패한 경우
						_editor.Notification("Bake is canceled", false, false);
					}

					
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);


				//최적화 Bake
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_OptimizedBaking));//"Optimized Baking"

				//"Target"
				apPortrait nextOptPortrait = (apPortrait)EditorGUILayout.ObjectField(_editor.GetText(TEXT.DLG_Target), _targetPortrait._bakeTargetOptPortrait, typeof(apPortrait), true);

				if (nextOptPortrait != _targetPortrait._bakeTargetOptPortrait)
				{
					//타겟을 바꾸었다.
					bool isChanged = false;
					if (nextOptPortrait != null)
					{
						//1. 다른 Portrait를 선택했다.
						if (!nextOptPortrait._isOptimizedPortrait)
						{
							//1-1. 최적화된 객체가 아니다.
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
														_editor.GetText(TEXT.OptBakeError_NotOptTarget_Body),
														_editor.GetText(TEXT.Close));
						}
						else if (nextOptPortrait._bakeSrcEditablePortrait != _targetPortrait)
						{
							//1-2. 다른 대상으로부터 Bake된 Portrait같다. (물어보고 계속)
							bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.OptBakeError_Title),
														_editor.GetText(TEXT.OptBakeError_SrcMatchError_Body),
														_editor.GetText(TEXT.Okay),
														_editor.GetText(TEXT.Cancel));

							if (isResult)
							{
								//뭐 선택하겠다는데요 뭐..
								isChanged = true;

							}
						}
						else
						{
							//1-3. 오케이. 변경 가능
							isChanged = true;
						}
					}
					else
					{
						//2. 선택을 해제했다.
						isChanged = true;
					}

					if (isChanged)
					{
						//Target을 변경한다.
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
						_targetPortrait._bakeTargetOptPortrait = nextOptPortrait;
					}

				}

				string optBtnText = "";
				if (_targetPortrait._bakeTargetOptPortrait != null)
				{
					//optBtnText = "Optimized Bake to\n[" + _targetPortrait._bakeTargetOptPortrait.gameObject.name + "]";
					optBtnText = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_OptimizedBakeTo), _targetPortrait._bakeTargetOptPortrait.gameObject.name);
				}
				else
				{
					//optBtnText = "Optimized Bake\n(Make New GameObject)";
					optBtnText = _editor.GetText(TEXT.DLG_OptimizedBakeMakeNew);
				}
				GUILayout.Space(10);

				if (GUILayout.Button(optBtnText, GUILayout.Height(45)))
				{
					GUI.FocusControl(null);

					//CheckChangedProperties(nextRootScale, nextZScale);


					//Optimized Bake를 하자
					apBakeResult bakeResult = _editor.Controller.OptimizedBake(_targetPortrait, _targetPortrait._bakeTargetOptPortrait);

					if (bakeResult != null)
					{
						if (bakeResult.NumUnlinkedExternalObject > 0)
						{
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.BakeWarning_Title),
								_editor.GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
								_editor.GetText(TEXT.Okay));
						}

						_editor.Notification("[" + _targetPortrait.name + "] is Baked (Optimized)", false, false);

						//추가 3.29 : Bake 후에 Ambient를 체크하자
						CheckAmbientAndCorrection();
					}
					else
					{
						//Bake가 취소되었다. (20.11.7)
						_editor.Notification("Bake is canceled (Optimized)", false, false);
					}
				}



			}
			else
			{
				//Vector2 curScroll = (_tab == TAB.Bake) ? _scroll_Bake : _scroll_Options;

				_scroll_Options = EditorGUILayout.BeginScrollView(_scroll_Options, false, true, GUILayout.Width(width), GUILayout.Height(height - 30));

				EditorGUILayout.BeginVertical(GUILayout.Width(width - 24));
				GUILayout.Space(5);

				width -= 24;

				// 2. Option에 대한 UI
				//1. Gamma Space Space			
				bool prevBakeGamma = _editor._isBakeColorSpaceToGamma;
				int iPrevColorSpace = prevBakeGamma ? 0 : 1;
				int iNextColorSpace = EditorGUILayout.Popup(_editor.GetUIWord(UIWORD.ColorSpace), iPrevColorSpace, _colorSpaceNames);
				if (iNextColorSpace != iPrevColorSpace)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					if (iNextColorSpace == 0)
					{
						//Gamma
						_editor._isBakeColorSpaceToGamma = true;
					}
					else
					{
						//Linear
						_editor._isBakeColorSpaceToGamma = false;
					}
				}

				GUILayout.Space(10);

				//2. Sorting Layer
				int prevSortingLayerID = _editor._portrait._sortingLayerID;
				int prevSortingOrder = _editor._portrait._sortingOrder;
				apPortrait.SORTING_ORDER_OPTION prevSortingLayerOption = _editor._portrait._sortingOrderOption;

				int layerIndex = -1;
				for (int i = 0; i < SortingLayer.layers.Length; i++)
				{
					if (SortingLayer.layers[i].id == _editor._portrait._sortingLayerID)
					{
						//찾았다.
						layerIndex = i;
						break;
					}
				}
				if (layerIndex < 0)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);

					//어라 레이어가 없는데용..
					//초기화해야겠다.
					_editor._portrait._sortingLayerID = -1;
					if (SortingLayer.layers.Length > 0)
					{
						_editor._portrait._sortingLayerID = SortingLayer.layers[0].id;
						layerIndex = 0;
					}
				}
				int nextIndex = EditorGUILayout.Popup(_editor.GetText(TEXT.SortingLayer), layerIndex, _sortingLayerNames);//"Sorting Layer"
				if (nextIndex != layerIndex)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					//레이어가 변경되었다.
					if (nextIndex >= 0 && nextIndex < SortingLayer.layers.Length)
					{
						//LayerID 변경
						_editor._portrait._sortingLayerID = SortingLayer.layers[nextIndex].id;
					}
				}

				//추가 19.8.18 : Sorting Order를 지정하는 방식을 3가지 + 미적용 1가지로 더 세분화
				apPortrait.SORTING_ORDER_OPTION nextSortingLayerOption = (apPortrait.SORTING_ORDER_OPTION)EditorGUILayout.EnumPopup(_editor.GetText(TEXT.SortingOrderOption), _editor._portrait._sortingOrderOption);
				if (nextSortingLayerOption != _editor._portrait._sortingOrderOption)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_editor._portrait._sortingOrderOption = nextSortingLayerOption;
				}

				if (_editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
				{
					//Set Order인 경우에만 한정
					int nextOrder = EditorGUILayout.IntField(_editor.GetText(TEXT.SortingOrder), _editor._portrait._sortingOrder);//"Sorting Order"
					if(nextOrder != _editor._portrait._sortingOrder)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
						_editor._portrait._sortingOrder = nextOrder;
					}
				}
				else if(_editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder 
					|| _editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					//추가 21.1.31 : Depth To Order일때, 1씩만 증가하는게 아닌 더 큰값으로 증가할 수도 있게 만들자
					int nextOrderPerDepth = EditorGUILayout.IntField(_editor.GetText(TEXT.OrderPerDepth), _editor._portrait._sortingOrderPerDepth);
					if(nextOrderPerDepth != _editor._portrait._sortingOrderPerDepth)
					{
						if(nextOrderPerDepth < 1)
						{
							nextOrderPerDepth = 1;
						}

						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
						_editor._portrait._sortingOrderPerDepth = nextOrderPerDepth;
					}
				}

				GUILayout.Space(10);

				//3. 메카님 사용 여부

				//EditorGUILayout.LabelField("Animation Settings");
				bool prevIsUsingMecanim = _targetPortrait._isUsingMecanim;
				string prevMecanimPath = _targetPortrait._mecanimAnimClipResourcePath;
				_targetPortrait._isUsingMecanim = EditorGUILayout.Toggle(_editor.GetText(TEXT.IsMecanimAnimation), _targetPortrait._isUsingMecanim);//"Is Mecanim Animation"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.AnimationClipExportPath));//"Animation Clip Export Path"

				GUIStyle guiStyle_ChangeBtn = new GUIStyle(GUI.skin.button);
				guiStyle_ChangeBtn.margin = GUI.skin.textField.margin;
				guiStyle_ChangeBtn.border = GUI.skin.textField.border;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.TextField(_targetPortrait._mecanimAnimClipResourcePath, GUILayout.Width(width - (70 + 15)));
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), guiStyle_ChangeBtn, GUILayout.Width(70), GUILayout.Height(18)))
				{
					string nextPath = EditorUtility.SaveFolderPanel("Select to export animation clips", "", "");
					if (!string.IsNullOrEmpty(nextPath))
					{
						if (apEditorUtil.IsInAssetsFolder(nextPath))
						{
							//유효한 폴더인 경우
							//중요 : 경로가 절대 경로로 찍힌다.
							//상대 경로로 바꾸자
							apEditorUtil.PATH_INFO_TYPE pathInfoType = apEditorUtil.GetPathInfo(nextPath);
							if(pathInfoType == apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder)
							{
								//절대 경로 + Asset 폴더 안쪽이라면
								//Debug.LogError("절대 경로가 리턴 되었다. : " + nextPath);
								nextPath = apEditorUtil.AbsolutePath2RelativePath(nextPath);
								//Debug.LogError(">> 상대 경로로 변경 : " + nextPath);
							}

							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _editor, _targetPortrait, _targetPortrait, false);

							_targetPortrait._mecanimAnimClipResourcePath = nextPath;
						}
						else
						{
							//유효한 폴더가 아닌 경우
							//EditorUtility.DisplayDialog("Invalid Folder Path", "Invalid Clip Path", "Close");
							EditorUtility.DisplayDialog(
								_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
								_editor.GetText(TEXT.DLG_AnimClipSavePathResetError_Body),
								_editor.GetText(TEXT.Close));
						}
					}

					GUI.FocusControl(null);

				}
				EditorGUILayout.EndHorizontal();


				

				GUILayout.Space(10);

				if(_guiContent_Setting_IsImportant == null)
				{
					_guiContent_Setting_IsImportant = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_IsImportant), false, "When this setting is on, it always updates and the physics effect works.");
				}
				if(_guiContent_Setting_FPS == null)
				{
					_guiContent_Setting_FPS = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_FPS), false, "This setting is used when <Important> is off");
				}
				
				

				//4. Important
				//"Is Important"
				bool nextImportant = EditorGUILayout.Toggle(_guiContent_Setting_IsImportant.Content, _targetPortrait._isImportant);
				if(nextImportant != _targetPortrait._isImportant)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._isImportant = nextImportant;
				}

				//"FPS (Important Off)"
				int nextFPS = EditorGUILayout.DelayedIntField(_guiContent_Setting_FPS.Content, _targetPortrait._FPS);
				if (_targetPortrait._FPS != nextFPS)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					if (nextFPS < 10)
					{
						nextFPS = 10;
					}
					_targetPortrait._FPS = nextFPS;
				}

				GUILayout.Space(10);

				//5. Billboard + Perspective
				
				apPortrait.BILLBOARD_TYPE nextBillboardType = (apPortrait.BILLBOARD_TYPE)EditorGUILayout.Popup(_editor.GetText(TEXT.DLG_Billboard), (int)_targetPortrait._billboardType, _billboardTypeNames);
				if(nextBillboardType != _targetPortrait._billboardType)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._billboardType = nextBillboardType;
				}
		
				//추가 19.9.24 : Billboard인 경우 카메라의 SortMode를 OrthoGraphic으로 강제할지 여부
				if(_targetPortrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
				{
					GUILayout.Space(2);
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width));

					int width_Value = 30;
					int width_Label = width - (width_Value + 10);
					GUIStyle guiStyle_LabelWrapText = new GUIStyle(GUI.skin.label);
					guiStyle_LabelWrapText.wordWrap = true;

					GUILayout.Space(5);
					EditorGUILayout.LabelField(_editor.GetText(TEXT.SetSortMode2Orthographic), guiStyle_LabelWrapText, GUILayout.Width(width_Label));

					bool nextForceSortModeToOrtho = EditorGUILayout.Toggle(_targetPortrait._isForceCamSortModeToOrthographic, GUILayout.Width(width_Value), GUILayout.Height(20));
					if(nextForceSortModeToOrtho != _targetPortrait._isForceCamSortModeToOrthographic)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
						_targetPortrait._isForceCamSortModeToOrthographic = nextForceSortModeToOrtho;
					}

					EditorGUILayout.EndHorizontal();
					
				}


				GUILayout.Space(10);

				//6. Shadow

				apPortrait.SHADOW_CASTING_MODE nextChastShadows = (apPortrait.SHADOW_CASTING_MODE)EditorGUILayout.EnumPopup(_editor.GetUIWord(UIWORD.CastShadows), _targetPortrait._meshShadowCastingMode);
				bool nextReceiveShaodw = EditorGUILayout.Toggle(_editor.GetUIWord(UIWORD.ReceiveShadows), _targetPortrait._meshReceiveShadow);
				if(nextChastShadows != _targetPortrait._meshShadowCastingMode
					|| nextReceiveShaodw != _targetPortrait._meshReceiveShadow)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._meshShadowCastingMode = nextChastShadows;
					_targetPortrait._meshReceiveShadow = nextReceiveShaodw;
				}

				GUILayout.Space(10);

				//#if UNITY_2018_1_OR_NEWER
				//변경 19.6.22 : LWRP 기능은 삭제
				//Material Library를 열도록 하자
				//>> 다시 변경 19.8.5 : Clipped Mesh 땜시 다시 열자

				//7. LWRP
				//LWRP 쉐이더를 쓸지 여부와 다시 강제로 생성하기 버튼을 만들자. : 이건 2019부터 적용 (그 전에는 SRP용 처리가 안된다.)
#if UNITY_2019_1_OR_NEWER
				bool prevUseLWRP = _editor._isUseSRP;
				int iPrevUseLWRP = prevUseLWRP ? 1 : 0;
				int iNextUseLWRP = EditorGUILayout.Popup(_editor.GetText(TEXT.RenderPipeline), iPrevUseLWRP, _renderPipelineNames);//"Render Pipeline"
				if (iNextUseLWRP != iPrevUseLWRP)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					if (iNextUseLWRP == 0)
					{
						//사용 안함
						_editor._isUseSRP = false;
					}
					else
					{
						//LWRP 사용함
						_editor._isUseSRP = true;
					}
				}
				GUILayout.Space(10);
#endif
				//if(GUILayout.Button("Generate Lightweight Shaders"))
				//{
				//	apShaderGenerator shaderGenerator = new apShaderGenerator();
				//	shaderGenerator.GenerateLWRPShaders();
				//}

				//GUILayout.Space(10);
				//#endif

				//8. VR Supported 19.9.24 추가
				//VR Supported
				int iNextVRSupported = EditorGUILayout.Popup(_editor.GetText(TEXT.VROption), (int)_targetPortrait._vrSupportMode, _vrSupportModeLabel);
				if(iNextVRSupported != (int)_targetPortrait._vrSupportMode)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._vrSupportMode = (apPortrait.VR_SUPPORT_MODE)iNextVRSupported;
				}

				if(_targetPortrait._vrSupportMode == apPortrait.VR_SUPPORT_MODE.SingleCamera)
				{
					//Single Camera인 경우, Clipping Mask의 크기를 결정해야한다.
					int iNextVRRTSize = EditorGUILayout.Popup(_editor.GetUIWord(UIWORD.MaskTextureSize), (int)_targetPortrait._vrRenderTextureSize, _vrRTSizeLabel);
					if(iNextVRRTSize != (int)_targetPortrait._vrRenderTextureSize)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
						_targetPortrait._vrRenderTextureSize = (apPortrait.VR_RT_SIZE)iNextVRRTSize;
					}

				}


				GUILayout.Space(10);


				//추가 20.8.11 : Flipped Mesh를 체크하는 방법을 정하도록 만들자
				int iNextFlippedMeshOption = EditorGUILayout.Popup(_editor.GetText(TEXT.Setting_FlippedMesh), (int)_targetPortrait._flippedMeshOption, _flippedMeshOptionLabel);
				if(iNextFlippedMeshOption != (int)_targetPortrait._flippedMeshOption)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._flippedMeshOption = (apPortrait.FLIPPED_MESH_CHECK)iNextFlippedMeshOption;
				}

				GUILayout.Space(10);


				//추가 20.8.14 : 루트 본의 스케일 옵션을 직접 정한다. [Skew 문제]
				int iNextRootBoneScaleOption = EditorGUILayout.Popup(_editor.GetText(TEXT.Setting_ScaleOfRootBone), (int)_targetPortrait._rootBoneScaleMethod, _rootBoneScaleOptionLabel);
				if(iNextRootBoneScaleOption != (int)_targetPortrait._rootBoneScaleMethod)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
					_targetPortrait._rootBoneScaleMethod = (apPortrait.ROOT_BONE_SCALE_METHOD)iNextRootBoneScaleOption;

					//모든 본에 대해서 ScaleOption을 적용해야한다.
					_editor.Controller.RefreshBoneScaleMethod_Editor();
				}

				GUILayout.Space(10);


				


				//11.7 추가 : Ambient Light를 검은색으로 만든다.
				
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_AmbientToBlack), GUILayout.Height(20)))
				{
					MakeAmbientLightToBlack();
				}

				//CheckChangedProperties(nextRootScale, nextZScale);
				if (prevSortingLayerID != _editor._portrait._sortingLayerID ||
					prevSortingOrder != _editor._portrait._sortingOrder ||
					prevSortingLayerOption != _editor._portrait._sortingOrderOption ||
					prevIsUsingMecanim != _targetPortrait._isUsingMecanim ||
					!string.Equals(prevMecanimPath, _targetPortrait._mecanimAnimClipResourcePath) ||
					prevBakeGamma != _editor._isBakeColorSpaceToGamma
#if UNITY_2019_1_OR_NEWER
					 || prevUseLWRP != _editor._isUseSRP
#endif
					 )
				{
					apEditorUtil.SetEditorDirty();
					_editor.SaveEditorPref();
				}

				GUILayout.Space(height + 500);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndScrollView();
			}

			GUILayout.Space(5);
		}
		

		private void MakeAmbientLightToBlack()
		{	
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight = Color.black;
			apEditorUtil.SetEditorDirty();
		}


		private void CheckAmbientAndCorrection()
		{
			if(_editor == null)
			{
				return;
			}
			if(!_editor._isAmbientCorrectionOption)
			{
				//Ambient 보정 옵션이 False이면 처리를 안함
				return;
			}

			//현재 Ambient 색상과 모드를 확인하자
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			Color ambientColor = RenderSettings.ambientLight;
			if(ambientMode == UnityEngine.Rendering.AmbientMode.Flat &&
				ambientColor.r <= 0.001f &&
				ambientColor.g <= 0.001f &&
				ambientColor.b <= 0.001f)
			{
				//Ambient가 검은색이다.
				return;
			}
			//이전
			////Ambient 색상을 바꿀 것인지 물어보자
#region [미사용 코드]
			//int iBtn = EditorUtility.DisplayDialogComplex(
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Title),
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Body),
			//										_editor.GetText(TEXT.Okay),
			//										_editor.GetText(TEXT.DLG_AmbientColorCorrection_Ignore),
			//										_editor.GetText(TEXT.Cancel)
			//										);

			//if(iBtn == 0)
			//{
			//	//색상을 바꾸자
			//	MakeAmbientLightToBlack();
			//}
			//else if(iBtn == 1)
			//{
			//	//무시하자
			//	_editor._isAmbientCorrectionOption = false;
			//	_editor.SaveEditorPref();
			//} 
#endregion

			//조건문 추가 19.6.22 : 현재의 기본 Material Set이 Ambient Color 색상이 검은색을 필요로할 경우
			if(_targetPortrait != null)
			{
				apMaterialSet defaultMatSet = _targetPortrait.GetDefaultMaterialSet();
				if(defaultMatSet != null)
				{
					if(!defaultMatSet._isNeedToSetBlackColoredAmbient)
					{
						//현재의 Default MatSet이 검은색을 필요로 하지 않는 경우
						return;
					}
				}
			}
			//이후 : 별도의 다이얼로그 표시
			apDialog_AmbientCorrection.ShowDialog(_editor, (int)position.x, (int)position.y);
		}
	}

}