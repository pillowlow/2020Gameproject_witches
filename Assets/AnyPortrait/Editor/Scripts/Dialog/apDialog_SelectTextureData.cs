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

	public class apDialog_SelectTextureData : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		public delegate void FUNC_SELECT_TEXTUREDATA_RESULT(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData);

		private static apDialog_SelectTextureData s_window = null;

		private apEditor _editor = null;
		private apMesh _targetMesh = null;
		private object _loadKey = null;
		private FUNC_SELECT_TEXTUREDATA_RESULT _funcResult = null;

		private List<apTextureData> _textureData = new List<apTextureData>();
		private Vector2 _scrollList = new Vector2();
		private apTextureData _curSelectedTextureData = null;



		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMesh targetMesh, FUNC_SELECT_TEXTUREDATA_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectTextureData), true, "Select Texture", true);
			apDialog_SelectTextureData curTool = curWindow as apDialog_SelectTextureData;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, targetMesh, loadKey, funcResult);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		private static void CloseDialog()
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
		public void Init(apEditor editor, apMesh targetMesh, object loadKey, FUNC_SELECT_TEXTUREDATA_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetMesh = targetMesh;

			_funcResult = funcResult;

			_textureData = _editor._portrait._textureData;
		}



		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 37, width, height - 90), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SelectImage), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼//"Select Image"
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 90));

			GUILayout.Space(20);

			int imageUnitHeight = 200;

			int scrollWidth = width - 16;
			int imageUnitWidth = (scrollWidth / 3) - 12;
			for (int iTex = 0; iTex < _textureData.Count; iTex += 3)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth), GUILayout.Height(imageUnitHeight));
				GUILayout.Space(5);
				if (iTex < _textureData.Count)
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(imageUnitWidth), GUILayout.Height(imageUnitHeight));
					DrawTextureUnit(_textureData[iTex], imageUnitWidth, imageUnitHeight);
					EditorGUILayout.EndVertical();
				}

				GUILayout.Space(2);

				if (iTex + 1 < _textureData.Count)
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(imageUnitWidth), GUILayout.Height(imageUnitHeight));
					DrawTextureUnit(_textureData[iTex + 1], imageUnitWidth, imageUnitHeight);
					EditorGUILayout.EndVertical();
				}

				GUILayout.Space(2);

				if (iTex + 2 < _textureData.Count)
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(imageUnitWidth), GUILayout.Height(imageUnitHeight));
					DrawTextureUnit(_textureData[iTex + 2], imageUnitWidth, imageUnitHeight);
					EditorGUILayout.EndVertical();
				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);
			}

			GUILayout.Space(height - 90);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				_funcResult(true, _targetMesh, _loadKey, _curSelectedTextureData);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private void DrawTextureUnit(apTextureData textureData, int width, int height)
		{
			int btnHeight = 25;
			int imageSlotHeight = height - (btnHeight + 2);

			float baseAspectRatio = (float)width / (float)imageSlotHeight;

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
			if (textureData._image == null)
			{
				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box("Empty Image", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
			}
			else
			{
				int imgWidth = textureData._width;
				if (imgWidth <= 0)
				{ imgWidth = 1; }

				int imgHeight = textureData._height;
				if (imgHeight <= 0)
				{ imgHeight = 1; }

				float aspectRatio = (float)imgWidth / (float)imgHeight;

				//가로를 채울 것인가, 세로를 채울 것인가
				if (aspectRatio > baseAspectRatio)
				{
					//비율상 가로가 더 길다.
					//가로에 맞추고 세로를 줄이자
					imgWidth = width;
					imgHeight = (int)((float)imgWidth / aspectRatio);
				}
				else
				{
					//비율상 세로가 더 길다.
					//세로에 맞추고 가로를 줄이다.
					imgHeight = imageSlotHeight;
					imgWidth = (int)((float)imageSlotHeight * aspectRatio);
				}
				int margin = (imageSlotHeight - imgHeight) / 2;
				if (margin > 0)
				{
					//GUILayout.Space(margin);
				}
				GUIStyle guiStyle_Img = new GUIStyle(GUI.skin.box);
				guiStyle_Img.alignment = TextAnchor.MiddleCenter;

				Color prevColor = GUI.backgroundColor;
				Color boxColor = prevColor;
				if (_curSelectedTextureData == textureData)
				{
					boxColor.r = prevColor.r * 0.8f;
					boxColor.g = prevColor.g * 0.8f;
					boxColor.b = prevColor.b * 1.2f;
				}

				GUI.backgroundColor = boxColor;

				//if(GUILayout.Button(new GUIContent(textureData._image), guiStyle_Img, GUILayout.Width(imgWidth), GUILayout.Height(imgHeight)))
				if (GUILayout.Button(new GUIContent(textureData._image), guiStyle_Img, GUILayout.Width(width), GUILayout.Height(imageSlotHeight)))
				{
					_curSelectedTextureData = textureData;
				}

				GUI.backgroundColor = prevColor;
			}
			EditorGUILayout.EndVertical();

			GUIStyle guiStyle_label = new GUIStyle(GUI.skin.label);
			guiStyle_label.alignment = TextAnchor.MiddleCenter;

			EditorGUILayout.LabelField(textureData._name, guiStyle_label, GUILayout.Width(width), GUILayout.Height(20));

			//if(apEditorUtil.ToggledButton(textureData._name, textureData == _curSelectedTextureData, width, btnHeight))
			//{
			//	_curSelectedTextureData = textureData;
			//}
		}
		// 
		//------------------------------------------------------------------
	}

}