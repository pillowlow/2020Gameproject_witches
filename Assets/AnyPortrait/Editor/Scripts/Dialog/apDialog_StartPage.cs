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
	//[데모용] Start Page 다이얼로그
	//에디터 시작시 나온다.
	//데모 버전 : 매 시작시마다 나온다.
	
	//내용
	//풀 버전 : 로고 / 버전 / 홈페이지 /  닫기 / "다시 보이지 않음" (Toogle)
	//데모 버전 : 로고 / 버전 / 데모와 정품 차이 안내<- 이거 한글만 되어있다. / 닫기 / AssetStore 페이지

	public class apDialog_StartPage : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_StartPage s_window = null;

		private apEditor _editor = null;
		private Texture2D _img_Logo = null;

		private bool _isFullVersion = false;
		

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, Texture2D img_Logo, bool isFullVersion)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}

			string strTitle = "Start Page";
			if(!isFullVersion)
			{
				strTitle = "Demo Start Page";
			}

			
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_StartPage), true, strTitle, true);
			apDialog_StartPage curTool = curWindow as apDialog_StartPage;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 280;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, img_Logo, isFullVersion);
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
		public void Init(apEditor editor, Texture2D img_Logo, bool isFullVersion)
		{
			_editor = editor;
			_img_Logo = img_Logo;
			_isFullVersion = isFullVersion;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;
			//if (_editor == null)
			//{
			//	CloseDialog();
			//	return;
			//}

			////만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			//if (_editor != apEditor.CurrentEditor)
			//{
			//	CloseDialog();
			//	return;
			//}


			//1. 로고
			//2. 버전
			//3. 데모 기능 제한 확인하기

			int logoWidth = _img_Logo.width;
			int logoHeight = _img_Logo.height;
			int boxHeight = (int)((float)width * ((float)logoHeight / (float)logoWidth));
			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = Color.black;
			GUILayout.Box(_img_Logo, GUILayout.Width(width), GUILayout.Height(boxHeight));

			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);

			if (_isFullVersion)
			{
				EditorGUILayout.LabelField(string.Format("Build : {0}", apVersion.I.APP_VERSION));
			}
			else
			{ 
				//"Demo Version : " + apVersion.I.APP_VERSION
				EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_DemoVersion), apVersion.I.APP_VERSION));
			}
			GUILayout.Space(10);

			//풀 버전 : 로고 / 버전 / 홈페이지 /  닫기 / "다시 보이지 않음" (Toogle)
			//데모 버전 : 로고 / 버전 / 데모와 정품 차이 안내<- 이거 한글만 되어있다. / 닫기 / AssetStore 페이지
			
			if (_isFullVersion)
			{
				//홈페이지
				//데모 다운로드 안내
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_StartPage_Hompage), GUILayout.Width(width), GUILayout.Height(40)))//"Check Limitations"
				{
					//홈페이지로 갑시다.
					Application.OpenURL("https://www.rainyrizzle.com");
					CloseDialog();
				}
			}
			else
			{
				//데모 다운로드 안내
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_CheckLimitations), GUILayout.Width(width), GUILayout.Height(40)))//"Check Limitations"
				{
					//홈페이지로 갑시다.
					if(_editor._language == apEditor.LANGUAGE.Korean)
					{
						Application.OpenURL("https://www.rainyrizzle.com/ap-demodownload-kor");
					}
					else
					{
						Application.OpenURL("https://www.rainyrizzle.com/ap-demodownload-eng");
					}
					
					CloseDialog();
				}
			}
			
			GUILayout.Space(5);

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width), GUILayout.Height(25)))//"Close"
			{
				CloseDialog();
			}

			//왼쪽 : 에셋 스토어 또는 계속 시작

			GUILayout.Space(10);
			if(_isFullVersion)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_StartPage_AlawysOn), GUILayout.Width(width - (12 + 20)), GUILayout.Height(20));
				bool isShow = EditorGUILayout.Toggle(_editor._startScreenOption_IsShowStartup, GUILayout.Width(20), GUILayout.Height(20));
				if(_editor._startScreenOption_IsShowStartup != isShow)
				{
					_editor._startScreenOption_IsShowStartup = isShow;
					_editor.SaveEditorPref();
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{
				if(GUILayout.Button("AssetStore", GUILayout.Width(width), GUILayout.Height(20)))
				{
					//AssetStore 등록하면 여기에 넣자
					Application.OpenURL("http://u3d.as/16c7");
					CloseDialog();
				}
			}
			
		}
	}

}