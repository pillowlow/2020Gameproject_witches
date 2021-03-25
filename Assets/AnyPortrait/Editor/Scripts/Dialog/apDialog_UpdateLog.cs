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
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	//업데이트 로그를 출력하고 알려준다.
	public class apDialog_UpdateLog : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_UpdateLog s_window = null;

		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;
		//내용은 그냥 코드로 적자

		private Vector2 _scroll = Vector2.zero;
		private string _info = "";

		private string _str_GotoHomepage = "";
		private string _str_OpenAssetStore = "";
		private string _str_Close = "";

		private GUIStyle _guiStyle_Text = null;


		// Show Window
		//------------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/Update Log", false, 51)]
		public static void ShowDialog()
		{
			ShowDialog(null);
		}
		
		public static void ShowDialog(apEditor editor)
		{
			
			CloseDialog();

			
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_UpdateLog), true, "Update Log", true);
			apDialog_UpdateLog curTool = curWindow as apDialog_UpdateLog;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 800;
				int height = 700;
				s_window = curTool;
				if (editor != null)
				{
					s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
													(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
													width, height);
				}
				else
				{
					s_window.position = new Rect(50, 50,
												width, height);
				}
				s_window.Init();
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
		public void Init()
		{
			_language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);

			GetText();
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;


			if(_guiStyle_Text == null)
			{
				_guiStyle_Text = new GUIStyle(GUI.skin.label);
				_guiStyle_Text.richText = true;
				_guiStyle_Text.wordWrap = true;
				_guiStyle_Text.alignment = TextAnchor.UpperLeft;
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width + 10), GUILayout.Height(height - 85));
			EditorGUILayout.BeginVertical(GUILayout.Width(width - 15));
			
			
			EditorGUILayout.TextArea(_info, _guiStyle_Text, GUILayout.Width(width - 25));
			GUILayout.Space(height + 500);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			bool isClose = false;
			if(GUILayout.Button(_str_GotoHomepage, GUILayout.Height(25)))
			{
				//홈페이지를 엽시다.
				if(_language == apEditor.LANGUAGE.Korean)
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-kor");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-kor");
				}
				else if(_language == apEditor.LANGUAGE.Japanese)
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-jp");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-jp");
				}
				else
				{
					//Application.OpenURL("https://www.rainyrizzle.com/anyportrait-updatenote-eng");
					Application.OpenURL("https://www.rainyrizzle.com/ap-updatenotelist-eng");
				}
				isClose = true;
			}
			if(GUILayout.Button(_str_OpenAssetStore, GUILayout.Height(25)))
			{
				//에셋 스토어 페이지를 엽니다.
				apEditorUtil.OpenAssetStorePage();
				isClose = true;
			}
			if(GUILayout.Button(_str_Close, GUILayout.Height(25)))
			{
				isClose = true;
			}

			EditorGUILayout.EndVertical();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private void GetText()
		{
			_info = "";

			//TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apUpdateLog.txt");
			TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.MakePath_Text("apUpdateLog.txt"));
			string strDelimeter = "-----------------------------------------";
			string[] strInfoPerLanguages = textAsset_Dialog.text.Split(new string[] { strDelimeter }, StringSplitOptions.None);

			if (strInfoPerLanguages.Length < (int)apEditor.LANGUAGE.Polish + 1)
			{
				//개수가 부족한데염
				Debug.Log("UpdateLog 개수 부족");
				return;
			}

			_info = strInfoPerLanguages[(int)_language];
			_info = _info.Replace("\r\n", "\n");

			//디버그 경고 (혹시 모르니)
			if(_info.Contains("\t"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Tab] [" + _language + "]");
				int iText = _info.IndexOf("\t");
				if(iText >= 0 && iText < _info.Length)
				{
					int iStart = Mathf.Max(iText - 10, 0);
					int iEnd = Mathf.Min(iText + 10, _info.Length - 1);
					Debug.LogError("[" + _info.Substring(iStart, iEnd - iStart + 1) + "]");
				}
			}

			if(_info.Contains("<color = blue>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Color=Blue] [" + _language + "]");
			}

			if(_info.Contains("<color = red>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [Color=Red] [" + _language + "]");
			}

			if(_info.Contains("</ color>"))
			{
				Debug.LogError("Update Log 텍스트 확인 필요 [/Color] [" + _language + "]");
			}

			_info = _info.Replace("\t", "    ");
			_info = _info.Replace("<color = blue>", "<color=blue>");
			_info = _info.Replace("<color = red>", "<color=red>");
			_info = _info.Replace("</ color>", "</color>");
			
			
			if(EditorGUIUtility.isProSkin)
			{
				_info = _info.Replace("<color=blue>", "<color=yellow>");
				_info = _info.Replace("<color=red>", "<color=lime>");
			}
			if (_info.Length > 0)
			{
				if (_info.Substring(0, 1) == "\n")
				{
					_info = _info.Substring(1);
				}
			}

			//첫줄을 삭제한다. (언어 이름이 써있다.)
			int firstCR = _info.IndexOf("\n");
			_info = _info.Substring(firstCR);

			switch (_language)
			{
				case apEditor.LANGUAGE.English:
					_str_GotoHomepage = "Go to Homepage";
					_str_OpenAssetStore = "Open Asset Store";
					_str_Close = "Close";
					break;

				case apEditor.LANGUAGE.Korean:
					_str_GotoHomepage = "홈페이지로 가기";
					_str_OpenAssetStore = "에셋 스토어 열기";
					_str_Close = "닫기";
					break;

				case apEditor.LANGUAGE.French:
					_str_GotoHomepage = "Aller à la page d'accueil";
					_str_OpenAssetStore = "Ouvrir Asset Store";
					_str_Close = "Fermer";
					break;

				case apEditor.LANGUAGE.German:
					_str_GotoHomepage = "Gehe zur Startseite";
					_str_OpenAssetStore = "Asset Store öffnen";
					_str_Close = "Schließen";
					break;

				case apEditor.LANGUAGE.Spanish:
					_str_GotoHomepage = "Ir a la página de inicio";
					_str_OpenAssetStore = "Abrir Asset Store";
					_str_Close = "Cerca";
					break;

				case apEditor.LANGUAGE.Italian:
					_str_GotoHomepage = "Vai alla pagina principale";
					_str_OpenAssetStore = "Apri Asset Store";
					_str_Close = "Vicino";
					break;

				case apEditor.LANGUAGE.Danish:
					_str_GotoHomepage = "Gå til Hjemmeside";
					_str_OpenAssetStore = "Åbn Asset Store";
					_str_Close = "Tæt";
					break;

				case apEditor.LANGUAGE.Japanese:
					_str_GotoHomepage = "ホームページへ";
					_str_OpenAssetStore = "アセットストアを開く";
					_str_Close = "閉じる";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					_str_GotoHomepage = "去首頁";
					_str_OpenAssetStore = "打開[資產商店]";
					_str_Close = "關";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					_str_GotoHomepage = "去首页";
					_str_OpenAssetStore = "打开[资产商店]";
					_str_Close = "关";
					break;

				case apEditor.LANGUAGE.Polish:
					_str_GotoHomepage = "Wróć do strony głównej";
					_str_OpenAssetStore = "Otwórz Asset Store";
					_str_Close = "Blisko";
					break;
			}

			
		}
	}
}