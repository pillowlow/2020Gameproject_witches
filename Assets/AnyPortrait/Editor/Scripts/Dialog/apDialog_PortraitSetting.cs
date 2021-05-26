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

	public class apDialog_PortraitSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_PortraitSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;


		private enum TAB
		{
			PortriatSetting,
			EditorSetting,
			About
		}

		private TAB _tab = TAB.PortriatSetting;
		private Vector2 _scroll = Vector2.zero;

		private int _width = 0;
		private int _height = 0;


		private string[] _strLanguageName = new string[]
		{
			"English",//"English" 0
			"한국어",//Korean 1
			"Français",//French 2
			"Deutsch",//German 3
			"Español",//Spanish 4
			"Dansk",//Danish 6
			"日本語",//Japanese 7
			"繁體中文",//Chinese_Traditional 8
			"簡體中文",//Chinese_Simplified 9
			"Italiano",//Italian 5 -> 현재 미지원
			"Polski",//Polish 10 -> 현재 미지원

		};

		//실제로 지원하는 언어 인덱스를 적는다.
		//0 -> 0 (English) 이런 방식
		//현재 Italian (5), Polish (10) 제외됨
		private int[] _validLanguageIndex = new int[]
		{
			0,	//English
			1,	//Korean
			2,	//French
			3,	//German
			4,	//Spanish
			6,	//Danish
			7,	//Japanese
			8,	//Chinese-Trad
			9,	//Chinese-Simp
			5,	//Italian
			10,	//Polish
		};
		private apGUIContentWrapper _guiContent_IsImportant = null;
		private apGUIContentWrapper _guiContent_FPS = null;

		
		private string[] _strBoneGUIRenderTypeNames = new string[]
		{
			"Arrowhead (v1)",
			"Needle (v2)"
		};



		private string[] _rootBoneScaleOptionLabel = new string[] { "Default", "Non-Uniform Scale"};

		private GUIStyle _guiStyle_WrapLabel_Default = null;
		private GUIStyle _guiStyle_WrapLabel_Changed = null;

		//기본값을 비교하여, 기본값과 다르면 다른 색으로 표시하자.
		private GUIStyle _guiStyle_Label_Default = null;
		private GUIStyle _guiStyle_Label_Changed = null;

		private GUIStyle _guiStyle_Text_About = null;
		private string _aboutText_1_PSD = null;
		private string _aboutText_2_NGif = null;
		private string _aboutText_3_Font = null;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PortraitSetting), true, "Setting", true);
			apDialog_PortraitSetting curTool = curWindow as apDialog_PortraitSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//이전 크기
				//int width = 400;
				//int height = 500;

				//변경 20.3.26
				int width = 500;
				int height = 600;
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

			_aboutText_1_PSD = null;
			_aboutText_2_NGif = null;
			_aboutText_3_Font = null;

			MakeAboutText();
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			try
			{
				_width = (int)position.width;
				_height = (int)position.height;

				if (_editor == null || _targetPortrait == null)
				{
					//Debug.LogError("Exit - Editor / Portrait is Null");
					CloseDialog();
					return;
				}

				//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
				if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
				{
					//Debug.LogError("Exit - Editor / Portrait Missmatch");
					CloseDialog();
					return;

				}

				if (_guiStyle_WrapLabel_Default == null)
				{
					_guiStyle_WrapLabel_Default = new GUIStyle(GUI.skin.label);
					_guiStyle_WrapLabel_Default.wordWrap = true;
					_guiStyle_WrapLabel_Default.alignment = TextAnchor.MiddleLeft;
				}


				if (_guiStyle_WrapLabel_Changed == null)
				{
					_guiStyle_WrapLabel_Changed = new GUIStyle(GUI.skin.label);
					_guiStyle_WrapLabel_Changed.wordWrap = true;
					_guiStyle_WrapLabel_Changed.alignment = TextAnchor.MiddleLeft;
					if (EditorGUIUtility.isProSkin)
					{
						//어두운 색이면 > 노란색
						_guiStyle_WrapLabel_Changed.normal.textColor = Color.yellow;
					}
					else
					{
						//밝은 색이면 보라색
						_guiStyle_WrapLabel_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.8f, 1.0f);
					}
				}


				//기본값을 비교하여, 기본값과 다르면 다른 색으로 표시하자.
				if (_guiStyle_Label_Default == null)
				{
					_guiStyle_Label_Default = new GUIStyle(GUI.skin.label);
					_guiStyle_Label_Default.alignment = TextAnchor.UpperLeft;
				}
				if (_guiStyle_Label_Changed == null)
				{
					_guiStyle_Label_Changed = new GUIStyle(GUI.skin.label);
					_guiStyle_Label_Changed.alignment = TextAnchor.UpperLeft;
					if (EditorGUIUtility.isProSkin)
					{
						//어두운 색이면 > 노란색
						_guiStyle_Label_Changed.normal.textColor = Color.yellow;
					}
					else
					{
						//밝은 색이면 진한 보라색
						_guiStyle_Label_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.8f, 1.0f);
					}
				}



				//탭
				int tabBtnHeight = 25;
				int tabBtnWidth = ((_width - 10) / 3) - 4;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(_width), GUILayout.Height(tabBtnHeight));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Portrait), _tab == TAB.PortriatSetting, tabBtnWidth, tabBtnHeight))//"Portrait"
				{
					_tab = TAB.PortriatSetting;
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Editor), _tab == TAB.EditorSetting, tabBtnWidth, tabBtnHeight))//"Editor"
				{
					_tab = TAB.EditorSetting;
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_About), _tab == TAB.About, tabBtnWidth, tabBtnHeight))//"About"
				{
					_tab = TAB.About;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				int scrollHeight = _height - 40;
				_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(_width), GUILayout.Height(scrollHeight));
				_width -= 25;
				GUILayout.BeginVertical(GUILayout.Width(_width));

				if (_guiContent_IsImportant == null)
				{
					_guiContent_IsImportant = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_IsImportant), false, "When this setting is on, it always updates and the physics effect works.");
				}
				if (_guiContent_FPS == null)
				{
					_guiContent_FPS = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_FPS), false, "This setting is used when <Important> is off");
				}


				switch (_tab)
				{
					case TAB.PortriatSetting:
						{
							//Portrait 설정
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PortraitSetting));//"Portrait Settings"


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							string nextName = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Name), _targetPortrait.name);//"Name"
							if (nextName != _targetPortrait.name)
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
								_targetPortrait.name = nextName;

							}

							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							//"Is Important"
							bool nextImportant = EditorGUILayout.Toggle(_guiContent_IsImportant.Content, _targetPortrait._isImportant);
							if (nextImportant != _targetPortrait._isImportant)
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
								_targetPortrait._isImportant = nextImportant;
							}

							//"FPS (Important Off)"
							int nextFPS = EditorGUILayout.DelayedIntField(_guiContent_FPS.Content, _targetPortrait._FPS);
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
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//추가 20.9.2 : 스케일 옵션을 넣자
							//추가 20.8.14 : 루트 본의 스케일 옵션을 직접 정한다. [Skew 문제]
							int iNextRootBoneScaleOption = EditorGUILayout.Popup(_editor.GetText(TEXT.Setting_ScaleOfRootBone), (int)_targetPortrait._rootBoneScaleMethod, _rootBoneScaleOptionLabel);
							if (iNextRootBoneScaleOption != (int)_targetPortrait._rootBoneScaleMethod)
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged, _editor, _targetPortrait, null, false);
								_targetPortrait._rootBoneScaleMethod = (apPortrait.ROOT_BONE_SCALE_METHOD)iNextRootBoneScaleOption;

								//모든 본에 대해서 ScaleOption을 적용해야한다.
								_editor.Controller.RefreshBoneScaleMethod_Editor();
							}





							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//수동으로 백업하기
							if (GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_ManualBackUp), GUILayout.Height(30)))//"Save Backup (Manual)"
							{
								if (_editor.Backup.IsAutoSaveWorking())
								{
									EditorUtility.DisplayDialog(_editor.GetText(TEXT.BackupError_Title),
																_editor.GetText(TEXT.BackupError_Body),
																_editor.GetText(TEXT.Okay));
								}
								else
								{
									string defaultBackupFileName = _targetPortrait.name + "_backup_" + apBackup.GetCurrentTimeString();
									string savePath = EditorUtility.SaveFilePanel("Backup File Path",
																					apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile), defaultBackupFileName,
																					"bck");
									if (string.IsNullOrEmpty(savePath))
									{
										_editor.Notification("Backup Canceled", true, false);
									}
									else
									{
										_editor.Backup.SaveBackupManual(savePath, _targetPortrait);
										_editor.Notification("Backup Saved [" + savePath + "]", false, true);

										apEditorUtil.SetLastExternalOpenSaveFilePath(savePath, apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile);//추가 21.3.1
									}

									CloseDialog();
								}
							}

						}
						break;

					case TAB.EditorSetting:
						{

							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_EditorSetting));//"Editor Settings"

							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);



							apEditor.LANGUAGE prevLanguage = _editor._language;
							apEditor.LANGUAGE defLanguage = apEditor.DefaultLanguage;
							int prevLangIndex = -1;
							int defLangIndex = -1;
							for (int i = 0; i < _validLanguageIndex.Length; i++)
							{
								if (_validLanguageIndex[i] == (int)prevLanguage)
								{
									prevLangIndex = i;
								}

								if (_validLanguageIndex[i] == (int)defLanguage)
								{
									defLangIndex = i;
								}
							}
							if (prevLangIndex < 0)
							{ prevLangIndex = 0; }//English 강제
							if (defLangIndex < 0)
							{ defLangIndex = 0; }//English 강제



							bool prevGUIFPS = _editor._guiOption_isFPSVisible;
							bool prevGUIStatistics = _editor._guiOption_isStatisticsVisible;


							Color prevColor_Background = _editor._colorOption_Background;
							Color prevColor_GridCenter = _editor._colorOption_GridCenter;
							Color prevColor_Grid = _editor._colorOption_Grid;

							Color prevColor_MeshEdge = _editor._colorOption_MeshEdge;
							Color prevColor_MeshHiddenEdge = _editor._colorOption_MeshHiddenEdge;
							Color prevColor_Outline = _editor._colorOption_Outline;
							Color prevColor_TFBorder = _editor._colorOption_TransformBorder;
							Color prevColor_VertNotSelected = _editor._colorOption_VertColor_NotSelected;
							Color prevColor_VertSelected = _editor._colorOption_VertColor_Selected;

							Color prevColor_GizmoFFDLine = _editor._colorOption_GizmoFFDLine;
							Color prevColor_GizmoFFDInnerLine = _editor._colorOption_GizmoFFDInnerLine;

							//Color prevColor_ToneColor = _editor._colorOption_OnionToneColor;//<<이거 빠집니더


							bool prevBackup_IsAutoSave = _editor._backupOption_IsAutoSave;
							string prevBackup_Path = _editor._backupOption_BaseFolderName;
							int prevBackup_Time = _editor._backupOption_Minute;



							apEditor.BONE_DISPLAY_METHOD prevBoneGUIOption_RenderType = _editor._boneGUIOption_RenderType;
							int prevBoneGUIOption_SizeRatio_Index = _editor._boneGUIOption_SizeRatio_Index;
							bool prevBoneGUIOption_ScaledByZoom = _editor._boneGUIOption_ScaledByZoom;
							apEditor.NEW_BONE_COLOR prevBoneGUIOption_NewBoneColor = _editor._boneGUIOption_NewBoneColor;

							int prevRigGUIOption_VertRatio_Index = _editor._rigGUIOption_VertRatio_Index;
							bool prevRigGUIOption_ScaledByZoom = _editor._rigGUIOption_ScaledByZoom;
							int prevRigGUIOption_VertRatio_Selected_Index = _editor._rigGUIOption_VertRatio_Selected_Index;
							apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE prevRigGUIOption_SelectedWeightType = _editor._rigGUIOption_SelectedWeightGUIType;
							apEditor.NOLINKED_BONE_VISIBILITY prevRigGUIOption_NoLinkedBoneVisibility = _editor._rigGUIOption_NoLinkedBoneVisibility;
							apEditor.RIG_WEIGHT_GRADIENT_COLOR prevRigGUIOption_WeightGradientColor = _editor._rigGUIOption_WeightGradientColor;

							string prevBonePose_Path = _editor._bonePose_BaseFolderName;



							//1. 기본 설정 (언어, FPS, Statistics)
							//"Language"
							int nextLangIndex = Layout_Popup(TEXT.DLG_Setting_Language, prevLangIndex, _strLanguageName, defLangIndex);
							_editor._language = (apEditor.LANGUAGE)_validLanguageIndex[nextLangIndex];

							GUILayout.Space(10);
							_editor._guiOption_isFPSVisible = Layout_Toggle(TEXT.DLG_Setting_ShowFPS, _editor._guiOption_isFPSVisible, apEditor.DefaultGUIOption_ShowFPS);//"Show FPS"
							_editor._guiOption_isStatisticsVisible = Layout_Toggle(TEXT.DLG_Setting_ShowStatistics, _editor._guiOption_isStatisticsVisible, apEditor.DefaultGUIOption_ShowStatistics);// "Show Statistics"


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//2. 자동 백업 설정

							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_AutoBackupSetting));//"Auto Backup Option"
							GUILayout.Space(10);

							_editor._backupOption_IsAutoSave = Layout_Toggle(TEXT.DLG_Setting_AutoBackup, _editor._backupOption_IsAutoSave, apEditor.DefaultBackupOption_IsAutoSave);//"Auto Backup"

							if (_editor._backupOption_IsAutoSave)
							{
								//경로와 시간
								//"Time (Min)"
								_editor._backupOption_Minute = Layout_IntField(TEXT.DLG_Setting_BackupTime, _editor._backupOption_Minute, apEditor.DefaultBackupOption_Minute);

								//이전 방식
								#region [미사용 코드]
								//EditorGUILayout.BeginHorizontal(GUILayout.Width(_width), GUILayout.Height(18));
								//GUILayout.Space(5);
								////"Save Path"
								//_editor._backupOption_BaseFolderName = EditorGUILayout.TextField(_editor.GetText(TEXT.DLG_Setting_BackupPath), _editor._backupOption_BaseFolderName, GUILayout.Width(_width - 100), GUILayout.Height(18));
								//if(GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(18)))//"Change"
								//{
								//	string pathResult = EditorUtility.SaveFolderPanel("Set the Backup Folder", _editor._backupOption_BaseFolderName, "");
								//	if(!string.IsNullOrEmpty(pathResult))
								//	{
								//		//Debug.Log("백업 폴더 경로 [" + pathResult + "] - " + Application.dataPath);
								//		Uri targetUri = new Uri(pathResult);
								//		Uri baseUri = new Uri(Application.dataPath);

								//		string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();
								//		_editor._backupOption_BaseFolderName = relativePath;
								//		//Debug.Log("상대 경로 [" + relativePath + "]");
								//		apEditorUtil.SetEditorDirty();

								//	}
								//}
								//EditorGUILayout.EndHorizontal(); 
								#endregion

								//변경된 방식 20.3.27
								string nextBackupOptionBaseFolderName = null;
								bool isBackupPathButtonDown = false;

								Layout_TextFieldAndButton(TEXT.DLG_Setting_BackupPath, _editor._backupOption_BaseFolderName, apEditor.DefaultBackupOption_BaseFolderName, TEXT.DLG_Change, 90, out nextBackupOptionBaseFolderName, out isBackupPathButtonDown);
								_editor._backupOption_BaseFolderName = string.IsNullOrEmpty(nextBackupOptionBaseFolderName) ? "" : nextBackupOptionBaseFolderName;
								if (isBackupPathButtonDown)
								{
									string pathResult = EditorUtility.SaveFolderPanel("Set the Backup Folder", _editor._backupOption_BaseFolderName, "");
									if (!string.IsNullOrEmpty(pathResult))
									{
										//Debug.Log("백업 폴더 경로 [" + pathResult + "] - " + Application.dataPath);
										Uri targetUri = new Uri(pathResult);
										Uri baseUri = new Uri(Application.dataPath);

										string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();
										_editor._backupOption_BaseFolderName = relativePath;
										//Debug.Log("상대 경로 [" + relativePath + "]");
										apEditorUtil.SetEditorDirty();

									}
								}

							}


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//3. 포즈 저장 옵션

							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_PoseSnapshotSetting));//"Pose Snapshot Option"
							GUILayout.Space(10);

							//이전 방식
							#region [미사용 코드]
							//EditorGUILayout.BeginHorizontal(GUILayout.Width(_width), GUILayout.Height(18));
							//GUILayout.Space(5);
							////"Save Path"
							//_editor._bonePose_BaseFolderName = EditorGUILayout.TextField(_editor.GetText(TEXT.DLG_Setting_BackupPath), _editor._bonePose_BaseFolderName, GUILayout.Width(_width - 100), GUILayout.Height(18));

							//if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(18)))//"Change"
							//{
							//	string pathResult = EditorUtility.SaveFolderPanel("Set the Pose Folder", _editor._bonePose_BaseFolderName, "");
							//	if (!string.IsNullOrEmpty(pathResult))
							//	{
							//		Uri targetUri = new Uri(pathResult);
							//		Uri baseUri = new Uri(Application.dataPath);

							//		string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();

							//		_editor._bonePose_BaseFolderName = relativePath;

							//		apEditorUtil.SetEditorDirty();

							//	}
							//}
							//EditorGUILayout.EndHorizontal(); 
							#endregion

							string nextBonePoseBaseFolderName = null;
							bool isBonePoseFolderChangeButtonDown = false;

							//변경된 방식
							//"Save Path"
							Layout_TextFieldAndButton(TEXT.DLG_Setting_BackupPath, _editor._bonePose_BaseFolderName, apEditor.DefaultBonePoseOption_BaseFolderName, TEXT.DLG_Change, 90, out nextBonePoseBaseFolderName, out isBonePoseFolderChangeButtonDown);
							_editor._bonePose_BaseFolderName = string.IsNullOrEmpty(nextBonePoseBaseFolderName) ? "" : nextBonePoseBaseFolderName;

							if (isBonePoseFolderChangeButtonDown)//"Change"
							{
								string pathResult = EditorUtility.SaveFolderPanel("Set the Pose Folder", _editor._bonePose_BaseFolderName, "");
								if (!string.IsNullOrEmpty(pathResult))
								{
									Uri targetUri = new Uri(pathResult);
									Uri baseUri = new Uri(Application.dataPath);

									string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();

									_editor._bonePose_BaseFolderName = relativePath;

									apEditorUtil.SetEditorDirty();

								}
							}


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							//4. 색상 옵션
							try
							{
								//int width_Btn = 65;
								//int width_Color = width - (width_Btn + 8);

								//int height_Color = 18;
								EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_BackgroundColors));//"Background Colors"
								GUILayout.Space(10);

								//"Background"
								_editor._colorOption_Background = ColorUI(TEXT.DLG_Setting_Background, _editor._colorOption_Background, apEditor.DefaultColor_Background);

								//"Grid Center"
								_editor._colorOption_GridCenter = ColorUI(TEXT.DLG_Setting_GridCenter, _editor._colorOption_GridCenter, apEditor.DefaultColor_GridCenter);

								//"Grid"
								_editor._colorOption_Grid = ColorUI(TEXT.DLG_Setting_Grid, _editor._colorOption_Grid, apEditor.DefaultColor_Grid);

								//"Atlas Border"
								_editor._colorOption_AtlasBorder = ColorUI(TEXT.DLG_Setting_AtlasBorder, _editor._colorOption_AtlasBorder, apEditor.DefaultColor_AtlasBorder);


								GUILayout.Space(15);
								EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_MeshGUIColors));//"Mesh GUI Colors"
								GUILayout.Space(10);

								//"Mesh Edge"
								_editor._colorOption_MeshEdge = ColorUI(TEXT.DLG_Setting_MeshEdge, _editor._colorOption_MeshEdge, apEditor.DefaultColor_MeshEdge);

								//"Mesh Hidden Edge"
								_editor._colorOption_MeshHiddenEdge = ColorUI(TEXT.DLG_Setting_MeshHiddenEdge, _editor._colorOption_MeshHiddenEdge, apEditor.DefaultColor_MeshHiddenEdge);

								//"Outline"
								_editor._colorOption_Outline = ColorUI(TEXT.DLG_Setting_Outline, _editor._colorOption_Outline, apEditor.DefaultColor_Outline);

								//"Transform Border"
								_editor._colorOption_TransformBorder = ColorUI(TEXT.DLG_Setting_TransformBorder, _editor._colorOption_TransformBorder, apEditor.DefaultColor_TransformBorder);

								//"Vertex"
								_editor._colorOption_VertColor_NotSelected = ColorUI(TEXT.DLG_Setting_Vertex, _editor._colorOption_VertColor_NotSelected, apEditor.DefaultColor_VertNotSelected);

								//"Selected Vertex"
								_editor._colorOption_VertColor_Selected = ColorUI(TEXT.DLG_Setting_SelectedVertex, _editor._colorOption_VertColor_Selected, apEditor.DefaultColor_VertSelected);


								GUILayout.Space(15);
								EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_GizmoColors));//"Gizmo Colors"
								GUILayout.Space(10);

								//"FFD Line"
								_editor._colorOption_GizmoFFDLine = ColorUI(TEXT.DLG_Setting_FFDLine, _editor._colorOption_GizmoFFDLine, apEditor.DefaultColor_GizmoFFDLine);

								//"FFD Inner Line"
								_editor._colorOption_GizmoFFDInnerLine = ColorUI(TEXT.DLG_Setting_FFDInnerLine, _editor._colorOption_GizmoFFDInnerLine, apEditor.DefaultColor_GizmoFFDInnerLine);

							}
							catch (Exception)
							{

							}


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//추가 20.3.20 : 
							//5. 본의 GUI 렌더링 옵션

							EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_BoneOpt_Appearance));//"Appearance of Bones"
							GUILayout.Space(10);

							_editor._boneGUIOption_RenderType = (apEditor.BONE_DISPLAY_METHOD)Layout_Popup(TEXT.Setting_BoneOpt_DisplayMethod, (int)_editor._boneGUIOption_RenderType, _strBoneGUIRenderTypeNames, (int)apEditor.DefaultBoneGUIOption_RenderType);//"Display Method"
							_editor._boneGUIOption_SizeRatio_Index = Layout_Popup(TEXT.Setting_SizeRatio, _editor._boneGUIOption_SizeRatio_Index, _editor._boneRigSizeNameList, apEditor.DefaultBoneGUIOption_SizeRatio_Index);//"Size (%)"
							_editor._boneGUIOption_ScaledByZoom = Layout_Toggle(TEXT.Setting_ScaledByZoom, _editor._boneGUIOption_ScaledByZoom, apEditor.DefaultBoneGUIOption_ScaedByZoom);//"Scaled by the Zoom"
							_editor._boneGUIOption_NewBoneColor = (apEditor.NEW_BONE_COLOR)Layout_EnumPopup(TEXT.Setting_NewBoneColor, _editor._boneGUIOption_NewBoneColor, apEditor.DefaultBoneGUIOption_NewBoneColor);


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);



							//추가 20.3.20 : 
							//6. 리깅에서의 GUI 렌더링 옵션

							EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_RigOpt));//"Appearance of Vertices during Rigging"
							GUILayout.Space(10);

							_editor._rigGUIOption_VertRatio_Index = Layout_Popup(TEXT.Setting_RigOpt_SizeCirVert, _editor._rigGUIOption_VertRatio_Index, _editor._boneRigSizeNameList, apEditor.DefaultRigGUIOption_VertRatio_Index);//"Size (%)"
							_editor._rigGUIOption_VertRatio_Selected_Index = Layout_Popup(TEXT.Setting_RigOpt_SizeSelectedCirVert, _editor._rigGUIOption_VertRatio_Selected_Index, _editor._boneRigSizeNameList, apEditor.DefaultRigGUIOption_VertRatio_Selected_Index);//Size of selected circular vertices
							_editor._rigGUIOption_ScaledByZoom = Layout_Toggle(TEXT.Setting_RigOpt_ScaledCirVertByZoom, _editor._rigGUIOption_ScaledByZoom, apEditor.DefaultRigGUIOption_ScaledByZoom);//"Scaled by the Zoom"
																																																	   //_editor._rigGUIOption_SelectedWeightGUIType = (apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE)Layout_EnumMaskPopup(TEXT.Setting_RigOpt_DisplaySelectedWeight, _editor._rigGUIOption_SelectedWeightGUIType, (int)_editor._rigGUIOption_SelectedWeightGUIType, (int)apEditor.DefaultRigGUIOption_SelectedWeightGUIType);
							_editor._rigGUIOption_SelectedWeightGUIType = (apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE)Layout_EnumPopup(TEXT.Setting_RigOpt_DisplaySelectedWeight, _editor._rigGUIOption_SelectedWeightGUIType, apEditor.DefaultRigGUIOption_SelectedWeightGUIType);
							//_editor._rigGUIOption_NoLinkedBoneVisibility = (apEditor.RIG_NOLINKED_BONE_VISIBILITY)Layout_EnumPopup(TEXT.Setting_RigOpt_DisplayNoRiggedBones, _editor._rigGUIOption_NoLinkedBoneVisibility);//>>이 옵션은 리깅 화면으로 이동
							_editor._rigGUIOption_WeightGradientColor = (apEditor.RIG_WEIGHT_GRADIENT_COLOR)Layout_EnumPopup(TEXT.Setting_RigOpt_GradientColor, _editor._rigGUIOption_WeightGradientColor, apEditor.DefaultRigGUIOption_WeightGradientColor);




							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							//7. 단축키 설정창 열기 (추가 20.11.30)
							//TODO 언어
							if (GUILayout.Button(_editor.GetText(TEXT.Setting_ShortcutsSettings), apGUILOFactory.I.Height(25)))//"Customize Shortcuts"
							{
								apDialog_HotkeyMapping.ShowDialog(_editor);
							}


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							//추가 21.2.13 : 모디파이어 편집 옵션 열기
							if (GUILayout.Button(_editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), apGUILOFactory.I.Height(25)))
							{
								apDialog_ModifierLockSetting.ShowDialog(_editor, _editor._portrait);
							}


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);


							//8. 고급 옵션들

							//변경 3.22 : 이하는 고급 옵션으로 분리한다.
							//텍스트를 길게 작성할 수 있게 만든다.
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_Advanced));
							GUILayout.Space(10);


							//int labelWidth = _width - (30 + 20);
							//int toggleWidth = 20;

							bool prevStartupScreen = _editor._startScreenOption_IsShowStartup;
							_editor._startScreenOption_IsShowStartup = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_ShowStartPageOn, _editor._startScreenOption_IsShowStartup, apEditor.DefaultStartScreenOption_IsShowStartup);

							//GUILayout.Space(10);
							bool prevCheckVersion = _editor._isCheckLiveVersion_Option;
							_editor._isCheckLiveVersion_Option = Layout_Toggle_AdvOpt(TEXT.CheckLatestVersionOption, _editor._isCheckLiveVersion_Option, apEditor.DefaultCheckLiverVersionOption);

							//추가 3.1 : 유휴 상태에서는 업데이트 빈도를 낮춤
							bool prevLowCPUOption = _editor._isLowCPUOption;
							_editor._isLowCPUOption = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_LowCPU, _editor._isLowCPUOption, apEditor.DefaultLowCPUOption);

							//추가 3.29 : Ambient 자동으로 보정하기 기능
							bool prevAmbientCorrection = _editor._isAmbientCorrectionOption;

							_editor._isAmbientCorrectionOption = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_AmbientColorCorrection, _editor._isAmbientCorrectionOption, apEditor.DefaultAmbientCorrectionOption);

							//추가 19.6.28 : 자동으로 Controller Tab으로 전환할 지 여부 (Mod, Anim)
							bool prevAutoSwitchController_Mod = _editor._isAutoSwitchControllerTab_Mod;
							_editor._isAutoSwitchControllerTab_Mod = Layout_Toggle_AdvOpt(TEXT.Setting_SwitchContTab_Mod, _editor._isAutoSwitchControllerTab_Mod, apEditor.DefaultAutoSwitchControllerTab_Mod);

							bool prevAutoSwitchController_Anim = _editor._isAutoSwitchControllerTab_Anim;
							_editor._isAutoSwitchControllerTab_Anim = Layout_Toggle_AdvOpt(TEXT.Setting_SwitchContTab_Anim, _editor._isAutoSwitchControllerTab_Anim, apEditor.DefaultAutoSwitchControllerTab_Anim);

							//추가 19.6.28 : 작업 종료시 메시의 작업용 보이기/숨기기를 초기화 할 지 여부
							bool prevTempMeshVisibility = _editor._isRestoreTempMeshVisibilityWhenTaskEnded;
							_editor._isRestoreTempMeshVisibilityWhenTaskEnded = Layout_Toggle_AdvOpt(TEXT.Setting_TempVisibilityMesh, _editor._isRestoreTempMeshVisibilityWhenTaskEnded, apEditor.DefaultRestoreTempMeshVisibiilityWhenTaskEnded);

							//추가 20.7.6 : PSD 파일로부터 연 경우, 해당 메시를 선택할때 버텍스 초기화를 할 지 물어본다.
							bool prevRemoveVertsIfImportedFromPSD = _editor._isNeedToAskRemoveVertByPSDImport;
							_editor._isNeedToAskRemoveVertByPSDImport = Layout_Toggle_AdvOpt(TEXT.Setting_AskRemoveVerticesImportedFromPSD, _editor._isNeedToAskRemoveVertByPSDImport, apEditor.DefaultNeedToAskRemoveVertByPSDImport);



							//추가 19.8.13 : 리깅 관련 옵션 > 다른 변수로 변경 (20.3.26)
							#region [미사용 코드]
							//bool prevRigOpt_ColorLikeParent = _editor._rigOption_NewChildBoneColorIsLikeParent;
							//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
							//GUILayout.Space(5);
							//_editor._rigOption_NewChildBoneColorIsLikeParent = EditorGUILayout.Toggle(_editor._rigOption_NewChildBoneColorIsLikeParent, GUILayout.Width(toggleWidth));
							//GUILayout.Space(5);
							//EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_RigOpt_ColorLikeParent), guiStyle_WrapLabel, GUILayout.Width(labelWidth));
							//EditorGUILayout.EndHorizontal(); 
							#endregion


							//추가 21.1.20 : View 버튼들이 기본적으로 없어지는데, 이걸 다시 보이게 만들 수 있다.
							bool prevShowViewBtns = _editor._option_ShowPrevViewMenuBtns;
							_editor._option_ShowPrevViewMenuBtns = Layout_Toggle_AdvOpt(TEXT.Setting_ShowPrevViewMenuBtns, _editor._option_ShowPrevViewMenuBtns, apEditor.DefaultShowPrevViewMenuBtns);

							//추가 21.3.6 : 이미지가 한개인 경우 메시 생성시 자동으로 이미지 할당하기
							bool prevSetAutoImageToMesh = _editor._option_SetAutoImageToMeshIfOnlyOneImageExist;
							_editor._option_SetAutoImageToMeshIfOnlyOneImageExist = Layout_Toggle_AdvOpt(TEXT.Setting_AutoImageSetToMeshCreation, _editor._option_SetAutoImageToMeshIfOnlyOneImageExist, apEditor.DefaultSetAutoImageToMeshIfOnlyOneImageExist);

							//추가 21.3.7 : 애니메이션의 AutoKey를 항상 초기화하는지 여부
							bool prevIsTurnOffAnimAutoKey = _editor._option_IsTurnOffAnimAutoKey;
							_editor._option_IsTurnOffAnimAutoKey = Layout_Toggle_AdvOpt(TEXT.Setting_InitAutoKeyframeOption, _editor._option_IsTurnOffAnimAutoKey, apEditor.DefaultIsTurnOffAnimAutoKey);

							GUILayout.Space(10);

							//선택 잠금에 대해서 > 이거를 다른 메뉴로 치환한다. 21.2.13 > 그냥 여기도 두자. 두군데 있으면 좋지 뭐
							EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
							GUILayout.Space(34);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_EnableSelectionLockEditMode), _guiStyle_WrapLabel_Default);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(10);

							bool prevSelectionEnableOption_RigPhy = _editor._isSelectionLockOption_RiggingPhysics;
							bool prevSelectionEnableOption_Morph = _editor._isSelectionLockOption_Morph;
							bool prevSelectionEnableOption_Transform = _editor._isSelectionLockOption_Transform;
							bool prevSelectionEnableOption_ControlTimeline = _editor._isSelectionLockOption_ControlParamTimeline;
							_editor._isSelectionLockOption_Morph = Layout_Toggle_AdvOpt("- Morph " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Morph, apEditor.DefaultSelectionLockOption_Morph);
							_editor._isSelectionLockOption_Transform = Layout_Toggle_AdvOpt("- Transform " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Transform, apEditor.DefaultSelectionLockOption_Transform);
							_editor._isSelectionLockOption_RiggingPhysics = Layout_Toggle_AdvOpt("- Rigging/Physic " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_RiggingPhysics, apEditor.DefaultSelectionLockOption_RiggingPhysics);
							_editor._isSelectionLockOption_ControlParamTimeline = Layout_Toggle_AdvOpt("- Control Parameter " + _editor.GetUIWord(UIWORD.Timeline), _editor._isSelectionLockOption_ControlParamTimeline, apEditor.DefaultSelectionLockOption_ControlParamTimeline);


							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							//8. 기본값으로 복원

							//"Restore Editor Default Setting"
							if (GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_RestoreDefaultSetting), GUILayout.Height(20)))
							{
								//추가 20.4.1 : 설정을 복구할 것인지 물어보자
								bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RestoreEditorSetting_Title),
																			_editor.GetText(TEXT.DLG_RestoreEditorSetting_Body),
																			_editor.GetText(TEXT.Okay),
																			_editor.GetText(TEXT.Cancel));
								if (result)
								{
									_editor.RestoreEditorPref();
								}
							}


							if (prevLanguage != _editor._language ||
								prevGUIFPS != _editor._guiOption_isFPSVisible ||
								prevGUIStatistics != _editor._guiOption_isStatisticsVisible ||
								prevColor_Background != _editor._colorOption_Background ||
								prevColor_GridCenter != _editor._colorOption_GridCenter ||
								prevColor_Grid != _editor._colorOption_Grid ||

								prevColor_MeshEdge != _editor._colorOption_MeshEdge ||
								prevColor_MeshHiddenEdge != _editor._colorOption_MeshHiddenEdge ||
								prevColor_Outline != _editor._colorOption_Outline ||
								prevColor_TFBorder != _editor._colorOption_TransformBorder ||
								prevColor_VertNotSelected != _editor._colorOption_VertColor_NotSelected ||
								prevColor_VertSelected != _editor._colorOption_VertColor_Selected ||

								prevColor_GizmoFFDLine != _editor._colorOption_GizmoFFDLine ||
								prevColor_GizmoFFDInnerLine != _editor._colorOption_GizmoFFDInnerLine ||
								//prevColor_ToneColor != _editor._colorOption_OnionToneColor ||
								prevBackup_IsAutoSave != _editor._backupOption_IsAutoSave ||
								!prevBackup_Path.Equals(_editor._backupOption_BaseFolderName) ||
								prevBackup_Time != _editor._backupOption_Minute ||
								!prevBonePose_Path.Equals(_editor._bonePose_BaseFolderName) ||

								prevStartupScreen != _editor._startScreenOption_IsShowStartup ||
								prevCheckVersion != _editor._isCheckLiveVersion_Option ||
								prevLowCPUOption != _editor._isLowCPUOption ||
								prevAmbientCorrection != _editor._isAmbientCorrectionOption ||
								prevAutoSwitchController_Mod != _editor._isAutoSwitchControllerTab_Mod ||
								prevAutoSwitchController_Anim != _editor._isAutoSwitchControllerTab_Anim ||

								prevTempMeshVisibility != _editor._isRestoreTempMeshVisibilityWhenTaskEnded ||
								//prevRigOpt_ColorLikeParent != _editor._rigOption_NewChildBoneColorIsLikeParent ||//>>_editor._boneGUIOption_NewBoneColor으로 변경
								prevRemoveVertsIfImportedFromPSD != _editor._isNeedToAskRemoveVertByPSDImport ||

								prevShowViewBtns != _editor._option_ShowPrevViewMenuBtns ||
								prevSetAutoImageToMesh != _editor._option_SetAutoImageToMeshIfOnlyOneImageExist ||
								prevIsTurnOffAnimAutoKey != _editor._option_IsTurnOffAnimAutoKey ||

								prevSelectionEnableOption_RigPhy != _editor._isSelectionLockOption_RiggingPhysics ||
								prevSelectionEnableOption_Morph != _editor._isSelectionLockOption_Morph ||
								prevSelectionEnableOption_Transform != _editor._isSelectionLockOption_Transform ||
								prevSelectionEnableOption_ControlTimeline != _editor._isSelectionLockOption_ControlParamTimeline ||

								prevBoneGUIOption_RenderType != _editor._boneGUIOption_RenderType ||
								prevBoneGUIOption_SizeRatio_Index != _editor._boneGUIOption_SizeRatio_Index ||
								prevBoneGUIOption_ScaledByZoom != _editor._boneGUIOption_ScaledByZoom ||
								prevBoneGUIOption_NewBoneColor != _editor._boneGUIOption_NewBoneColor ||
								prevRigGUIOption_VertRatio_Index != _editor._rigGUIOption_VertRatio_Index ||
								prevRigGUIOption_ScaledByZoom != _editor._rigGUIOption_ScaledByZoom ||
								prevRigGUIOption_VertRatio_Selected_Index != _editor._rigGUIOption_VertRatio_Selected_Index ||
								prevRigGUIOption_SelectedWeightType != _editor._rigGUIOption_SelectedWeightGUIType ||
								prevRigGUIOption_NoLinkedBoneVisibility != _editor._rigGUIOption_NoLinkedBoneVisibility ||
								prevRigGUIOption_WeightGradientColor != _editor._rigGUIOption_WeightGradientColor

									)
							{
								bool isLanguageChanged = (prevLanguage != _editor._language);

								_editor.SaveEditorPref();
								apEditorUtil.SetEditorDirty();

								//apGL.SetToneColor(_editor._colorOption_OnionToneColor);

								if (isLanguageChanged)
								{

									_editor.ResetHierarchyAll();

									//이전
									//_editor.RefreshTimelineLayers(true);
									//_editor.RefreshControllerAndHierarchy();

									//변경 19.5.21
									_editor.RefreshControllerAndHierarchy(true);//<<True를 넣으면 RefreshTimelineLayer 함수가 같이 호출된다.
								}

								//apEditorUtil.ReleaseGUIFocus();
							}

						}
						break;

					case TAB.About:
						{
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_About));//"About"

							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							EditorGUILayout.LabelField("[AnyPortrait]");
							EditorGUILayout.LabelField("Build : " + apVersion.I.APP_VERSION_NUMBER_ONLY);



							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							EditorGUILayout.LabelField("[Open Source Library License]");
							GUILayout.Space(20);

							//이전 : LabelField 사용
							//EditorGUILayout.LabelField("[PSD File Import Library]");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("Ntreev Photoshop Document Parser for .Net");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("Released under the MIT License.");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("Copyright (c) 2015 Ntreev Soft co., Ltd.");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("Permission is hereby granted, free of charge,");
							//EditorGUILayout.LabelField("to any person obtaining a copy of this software");
							//EditorGUILayout.LabelField("and associated documentation files (the \"Software\"),");
							//EditorGUILayout.LabelField("to deal in the Software without restriction,");
							//EditorGUILayout.LabelField("including without limitation the rights ");
							//EditorGUILayout.LabelField("to use, copy, modify, merge, publish, distribute,");
							//EditorGUILayout.LabelField("sublicense, and/or sell copies of the Software, ");
							//EditorGUILayout.LabelField("and to permit persons to whom the Software is furnished");
							//EditorGUILayout.LabelField("to do so, subject to the following conditions:");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("The above copyright notice and ");
							//EditorGUILayout.LabelField("this permission notice shall be included");
							//EditorGUILayout.LabelField("in all copies or substantial portions of the Software.");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT ");
							//EditorGUILayout.LabelField("WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, ");
							//EditorGUILayout.LabelField("INCLUDING BUT NOT LIMITED TO THE WARRANTIES ");
							//EditorGUILayout.LabelField("OF MERCHANTABILITY, FITNESS FOR A PARTICULAR ");
							//EditorGUILayout.LabelField("PURPOSE AND NONINFRINGEMENT. ");
							//EditorGUILayout.LabelField("IN NO EVENT SHALL THE AUTHORS OR ");
							//EditorGUILayout.LabelField("COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES ");
							//EditorGUILayout.LabelField("OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, ");
							//EditorGUILayout.LabelField("TORT OR OTHERWISE, ARISING FROM, OUT OF OR ");
							//EditorGUILayout.LabelField("IN CONNECTION WITH THE SOFTWARE OR ");
							//EditorGUILayout.LabelField("THE USE OR OTHER DEALINGS IN THE SOFTWARE.");

							//GUILayout.Space(10);
							//apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("[GIF Export Library]");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("NGif, Animated GIF Encoder for .NET");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("Released under the CPOL 1.02.");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("The Code Project Open License (CPOL) is intended");
							//EditorGUILayout.LabelField("to provide developers who choose to share their code");
							//EditorGUILayout.LabelField("with a license that protects them and provides users");
							//EditorGUILayout.LabelField("of their code with a clear statement regarding ");
							//EditorGUILayout.LabelField("how the code can be used.");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("The CPOL is our gift to the community.");
							//EditorGUILayout.LabelField("We encourage everyone to use this license");
							//EditorGUILayout.LabelField("if they wish regardless of whether the code is posted");
							//EditorGUILayout.LabelField("on CodeProject.com.");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("Detailed License Information : ");
							//EditorGUILayout.LabelField("https://www.codeproject.com/info/cpol10.aspx");

							//GUILayout.Space(10);
							//apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("[Font for Demo scenes]");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("Spoqa Han Sans");
							//GUILayout.Space(10);

							//EditorGUILayout.LabelField("Spoqa Han Sans fonts are open source.");
							//EditorGUILayout.LabelField("All Spoqa Han Sans fonts are published under");
							//EditorGUILayout.LabelField("the SIL Open Font License, Version 1.1.");
							//GUILayout.Space(10);
							//EditorGUILayout.LabelField("SIL Open Font License (OFL) 1.1");
							//EditorGUILayout.LabelField("Detailed License Information : ");
							//EditorGUILayout.LabelField("http://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL");


							//변경 : TextArea 사용
							if (_guiStyle_Text_About == null)
							{
								_guiStyle_Text_About = new GUIStyle(GUI.skin.label);
								_guiStyle_Text_About.richText = true;
								_guiStyle_Text_About.wordWrap = true;
								_guiStyle_Text_About.alignment = TextAnchor.UpperLeft;
							}

							if (_aboutText_1_PSD == null ||
								_aboutText_2_NGif == null ||
								_aboutText_3_Font == null)
							{
								MakeAboutText();
							}

							EditorGUILayout.TextArea(_aboutText_1_PSD, _guiStyle_Text_About, GUILayout.Width(_width - 25));

							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							EditorGUILayout.TextArea(_aboutText_2_NGif, _guiStyle_Text_About, GUILayout.Width(_width - 25));

							GUILayout.Space(10);
							apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
							GUILayout.Space(10);

							EditorGUILayout.TextArea(_aboutText_3_Font, _guiStyle_Text_About, GUILayout.Width(_width - 25));

						}
						break;
				}



				GUILayout.Space(_height);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
			}
			catch(Exception ex)
			{
				//추가 21.3.17 : Try-Catch 추가. Mac에서 에러가 발생하기 쉽다.
				Debug.LogError("AnyPortrait : Exception occurs : " + ex);
			}
		}


		//--------------------------------------------------------------------------------------------------
		// UI 함수 래핑 (레이블 길이때문에..)
		//--------------------------------------------------------------------------------------------------
		private const int LEFT_MARGIN = 5;
		private const int LABEL_WIDTH = 250;
		private const int LAYOUT_HEIGHT = 18;

		private int Layout_Popup(TEXT label, int index, string[] names, int defaultIndex)
		{	
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (index == defaultIndex ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.Popup(index, names);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private bool Layout_Toggle(TEXT label, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (isValue == defaultValue ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			bool result = EditorGUILayout.Toggle(isValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private int Layout_IntField(TEXT label, int intValue, int defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (intValue == defaultValue ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.IntField(intValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private string Layout_TextField(TEXT label, string strValue, string defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (string.Equals(strValue, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			string result = EditorGUILayout.TextField(strValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}

		//TextField + Button
		private void Layout_TextFieldAndButton(TEXT label, string strValue, string defaultValue, TEXT buttonName, int buttonWidth, out string strResult, out bool isButtonDown)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (string.Equals(strValue, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			strResult = EditorGUILayout.TextField(strValue, GUILayout.Width(_width - (LABEL_WIDTH + buttonWidth + 10)));
			isButtonDown = GUILayout.Button(_editor.GetText(buttonName), GUILayout.Width(buttonWidth), GUILayout.Height(LAYOUT_HEIGHT));

			EditorGUILayout.EndHorizontal();
		}



		private Color ColorUI(TEXT label, Color srcColor, Color defaultColor)
		{
			int width_Btn = 65;
			int width_Color = _width - (LABEL_WIDTH + width_Btn + 10);

			bool isDefaultColor =	Mathf.Abs(srcColor.r - defaultColor.r) < 0.005f
								&& Mathf.Abs(srcColor.g - defaultColor.g) < 0.005f
								&& Mathf.Abs(srcColor.b - defaultColor.b) < 0.005f
								&& Mathf.Abs(srcColor.a - defaultColor.a) < 0.005f;


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			Color result = srcColor;
			try
			{
				EditorGUILayout.LabelField(_editor.GetText(label), (isDefaultColor ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
				result = EditorGUILayout.ColorField(srcColor, GUILayout.Width(width_Color), GUILayout.Height(LAYOUT_HEIGHT));
			}
			catch (Exception)
			{
			}

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Default), GUILayout.Width(width_Btn), GUILayout.Height(LAYOUT_HEIGHT)))//"Default"
			{
				result = defaultColor;
			}
			EditorGUILayout.EndHorizontal();
			return result;
		}

		


		private Enum Layout_EnumPopup(TEXT label, Enum selected, Enum defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (Enum.Equals(selected, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			Enum result = EditorGUILayout.EnumPopup(selected);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private Enum Layout_EnumMaskPopup(TEXT label, Enum selected, int intSelected, int defaultValue)
		{	
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), ((intSelected == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));

#if UNITY_2017_3_OR_NEWER
			Enum result = EditorGUILayout.EnumFlagsField(selected);
#else		
			Enum result = EditorGUILayout.EnumMaskPopup("", selected);
#endif
			
			EditorGUILayout.EndHorizontal();

			return result;
		}



		private bool Layout_Toggle_AdvOpt(TEXT label, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
			GUILayout.Space(LEFT_MARGIN);
			bool result = EditorGUILayout.Toggle(isValue, GUILayout.Width(20));//고급 옵션은 토글이 앞쪽
			EditorGUILayout.LabelField(_editor.GetText(label), (isValue == defaultValue ? _guiStyle_WrapLabel_Default : _guiStyle_WrapLabel_Changed), GUILayout.Width(_width - (50)));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);//여백이 아예 포함됨다.

			return result;
		}

		private bool Layout_Toggle_AdvOpt(string strLabel, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
			GUILayout.Space(LEFT_MARGIN);
			bool result = EditorGUILayout.Toggle(isValue, GUILayout.Width(20));//고급 옵션은 토글이 앞쪽
			EditorGUILayout.LabelField(strLabel, (isValue == defaultValue ? _guiStyle_WrapLabel_Default : _guiStyle_WrapLabel_Changed), GUILayout.Width(_width - (50)));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);//여백이 아예 포함됨다.

			return result;
		}

		//텍스트 만들기
		//----------------------------------------------------------------------------------------
		private void MakeAboutText()
		{
			_aboutText_1_PSD = null;
			_aboutText_2_NGif = null;
			_aboutText_3_Font = null;

			_aboutText_1_PSD = 
				"[PSD File Import Library]"
				+ "\n\nNtreev Photoshop Document Parser for .Net"
				+ "\n(https://github.com/NtreevSoft/psd-parser)"
				+ "\n\nCopyright (c) 2015 Ntreev Soft co., Ltd."
				+ "\n\nReleased under the MIT License."
				+ "\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:"
				+ "\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software."
				+ "\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

			_aboutText_2_NGif = 
				"[GIF Export Library]"
				+ "\n\nNGif, Animated GIF Encoder for .NET"
				+ "\n(https://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET)"
				+ "\n\nReleased under the CPOL 1.02."
				+ "\n\nThe Code Project Open License (CPOL) is intended to provide developers who choose to share their code with a license that protects them and provides users of their code with a clear statement regarding how the code can be used."
				+ "\n\nThe CPOL is our gift to the community. We encourage everyone to use this license if they wish regardless of whether the code is posted on CodeProject.com."
				+ "\n\n> Detailed License Information : "
				+ "\nhttps://www.codeproject.com/info/cpol10.aspx";

			_aboutText_3_Font = 
				"[Font for Demo Scenes]"
				+ "\n\nSpoqa Han Sans"
				+ "\n(https://spoqa.github.io/spoqa-han-sans/en-US/)"
				+ "\n\nSpoqa Han Sans fonts are open source. All Spoqa Han Sans fonts are published under the SIL Open Font License, Version 1.1."
				+ "\n\nSIL Open Font License (OFL) 1.1"
				+ "\n\n> Detailed License Information : "
				+ "\nhttp://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL";
		}
		
	}


}