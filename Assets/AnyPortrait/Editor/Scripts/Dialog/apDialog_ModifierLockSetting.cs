/*
*	Copyright (c) 2017-2020. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee].
*
*	Unless this file is downloaded from the Unity Asset Store or RainyRizzle homepage, 
*	this file and its users are illegal.
*	In that case, the act may be subject to legal penalties.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	public class apDialog_ModifierLockSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_ModifierLockSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		

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



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ModifierLockSetting), true, "Modifier Lock", true);
			apDialog_ModifierLockSetting curTool = curWindow as apDialog_ModifierLockSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 620;
				int height = 470;
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
			bool isChanged = false;

			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				//Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}

			//int height_top = 30;
			//int height_bottom = 35 + 40;
			//int height_middle = height - (height_top + height_bottom);
			int height_middle = 370;
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ModLockSettings), GUILayout.Height(25));//"Modifier Lock Settings"

			

			GUI.Box(new Rect(0, 30, width / 2, 300), "");
			GUI.Box(new Rect(width / 2 - 1, 30, width / 2 + 1, 300), "");


			Texture2D img_Lock = _editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock);
			Texture2D img_Unlock = _editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock);

			GUIStyle guiStyle_Title = new GUIStyle(GUI.skin.label);
			guiStyle_Title.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			int width_half = (width - 10) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height_middle));

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));

			GUILayout.Space(5);
			//Lock Mode / Unlock Mode
			EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetText(TEXT.DLG_ModLockMode), img_Lock), guiStyle_Title, GUILayout.Width(width_half), GUILayout.Height(40));
			EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetText(TEXT.DLG_ModUnlockMode), img_Unlock), guiStyle_Title, GUILayout.Width(width_half), GUILayout.Height(40));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			Color prevColor = GUI.backgroundColor;
			
			//1. 설명

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(60));
			GUILayout.Space(5);
			GUI.backgroundColor = new Color(prevColor.r * 1.8f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
			//"Other than the selected Modifier\nwill not be executed."
			GUILayout.Box(_editor.GetText(TEXT.DLG_ModLockDescription), guiStyle_Box, GUILayout.Width(width_half - 5), GUILayout.Height(60));

			GUILayout.Space(5);

			GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.8f, prevColor.b * 1.8f, 1.0f);
			//"Except Modifiers which are of the same type\nor can not be edited at the same time,\nothers are executed."
			GUILayout.Box(_editor.GetText(TEXT.DLG_ModUnlockDescription), guiStyle_Box, GUILayout.Width(width_half - 5), GUILayout.Height(60));

			GUI.backgroundColor = prevColor;

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);


			//2. 어떤 객체가 Modifier나 Timeline Layer에 등록 안된 경우, 배타적인 계산을 하지 않는다.
			//Lock On인 경우 없다.
			//"Calculating transformations\nof unregistered objects"
			string strOpt_CalculateIfNotAddedOther_On = _editor.GetText(TEXT.DLG_ModLockCalculateUnregisteredObj);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));
			GUILayout.Space(5 + width_half + 5);
			if(apEditorUtil.ToggledButton_2Side(strOpt_CalculateIfNotAddedOther_On, 
												strOpt_CalculateIfNotAddedOther_On,
												_editor._modLockOption_CalculateIfNotAddedOther,
												true, 
												width_half - 5, 40))
			{
				_editor._modLockOption_CalculateIfNotAddedOther = !_editor._modLockOption_CalculateIfNotAddedOther;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);


			//3. Color Preview
			//"Render Calculated Colors"
			string strOpt_ColorPreview = _editor.GetText(TEXT.DLG_ModLockRenderCalculatedColors);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(strOpt_ColorPreview, strOpt_ColorPreview,
												_editor._modLockOption_ColorPreview_Lock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_ColorPreview_Lock = !_editor._modLockOption_ColorPreview_Lock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			GUILayout.Space(5);

			if(apEditorUtil.ToggledButton_2Side(strOpt_ColorPreview, strOpt_ColorPreview,
												_editor._modLockOption_ColorPreview_Unlock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_ColorPreview_Unlock = !_editor._modLockOption_ColorPreview_Unlock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			//4. Bone Preview
			//"Preview Calculated Bones"
			string strOpt_BonePreview = _editor.GetText(TEXT.DLG_ModLockPreviewCalculatedBones);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(strOpt_BonePreview, strOpt_BonePreview,
												_editor._modLockOption_BonePreview_Lock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_BonePreview_Lock = !_editor._modLockOption_BonePreview_Lock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			GUILayout.Space(5);

			if(apEditorUtil.ToggledButton_2Side(strOpt_BonePreview, strOpt_BonePreview,
												_editor._modLockOption_BonePreview_Unlock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_BonePreview_Unlock = !_editor._modLockOption_BonePreview_Unlock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);


			//5. Mesh Preview
			//string strOpt_MeshPreview = "Preview Calculated Meshes";
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			//GUILayout.Space(5);
			//if(apEditorUtil.ToggledButton_2Side(strOpt_MeshPreview, strOpt_MeshPreview,
			//									_editor._modLockOption_MeshPreview_Lock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_MeshPreview_Lock = !_editor._modLockOption_MeshPreview_Lock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//GUILayout.Space(5);

			//if(apEditorUtil.ToggledButton_2Side(strOpt_MeshPreview, strOpt_MeshPreview,
			//									_editor._modLockOption_MeshPreview_Unlock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_MeshPreview_Unlock = !_editor._modLockOption_MeshPreview_Unlock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);


			//6. Modifier List UI
			//"Show Modifier List"
			string strOpt_ModifierListUI = _editor.GetText(TEXT.DLG_ModLockShowModifierList);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(strOpt_ModifierListUI, strOpt_ModifierListUI,
												_editor._modLockOption_ModListUI_Lock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_ModListUI_Lock = !_editor._modLockOption_ModListUI_Lock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			GUILayout.Space(5);

			if(apEditorUtil.ToggledButton_2Side(strOpt_ModifierListUI, strOpt_ModifierListUI,
												_editor._modLockOption_ModListUI_Unlock,
												true,
												width_half - 5, 30))
			{
				_editor._modLockOption_ModListUI_Unlock = !_editor._modLockOption_ModListUI_Unlock;
				isChanged = true;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(15);


			//7. Preview 색상
			//"Preview Color"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ModLockPreviewColor));
			//try
			//{
			//	Color nextColor = EditorGUILayout.ColorField("Mesh Preview Color", _editor._modLockOption_MeshPreviewColor);
			//	if(!IsSameColor(nextColor, _editor._modLockOption_MeshPreviewColor))
			//	{
			//		_editor._modLockOption_MeshPreviewColor = nextColor;
			//		isChanged = true;
			//		_editor.SaveEditorPref();
			//	}
			//}
			//catch(Exception) { }

			try
			{
				//"Bone Preview Color"
				_editor._modLockOption_BonePreviewColor = EditorGUILayout.ColorField(_editor.GetText(TEXT.DLG_ModLockBonePreviewColor), _editor._modLockOption_BonePreviewColor);
			}
			catch(Exception) { }

			EditorGUILayout.EndVertical();

			
			//"Restore Settings"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_ModLockRestoreSettings), GUILayout.Height(20)))
			{
				//TODO
				_editor._modLockOption_CalculateIfNotAddedOther = false;			
				_editor._modLockOption_ColorPreview_Lock =		false;
				_editor._modLockOption_ColorPreview_Unlock =	true;//<< True 기본값
				_editor._modLockOption_BonePreview_Lock =		false;
				_editor._modLockOption_BonePreview_Unlock =		true;//<< True 기본값
				//_editor._modLockOption_MeshPreview_Lock =		false;
				//_editor._modLockOption_MeshPreview_Unlock =		false;
				_editor._modLockOption_ModListUI_Lock =			false;
				_editor._modLockOption_ModListUI_Unlock =		false;

				//_editor._modLockOption_MeshPreviewColor = apEditor.DefauleColor_ModLockOpt_MeshPreview;
				_editor._modLockOption_BonePreviewColor = apEditor.DefauleColor_ModLockOpt_BonePreview;
				isChanged = true;
				_editor.SaveEditorPref();
			}


			if(isChanged)
			{
				if(_editor.Select != null)
				{
					_editor.Select.RefreshModifierExclusiveEditing();
					_editor.Select.RefreshAnimEditingLayerLock();
				}
			}

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(35)))
			{
				_editor.SaveEditorPref();
				CloseDialog();
			}

			
		}

		private bool IsSameColor(Color colorA, Color colorB)
		{
			float bias = 0.001f;
			if(Mathf.Abs(colorA.r - colorB.r) < bias &&
				Mathf.Abs(colorA.g - colorB.g) < bias &&
				Mathf.Abs(colorA.b - colorB.b) < bias &&
				Mathf.Abs(colorA.a - colorB.a) < bias)
			{
				return true;
			}
			return false;
		}
	}
}