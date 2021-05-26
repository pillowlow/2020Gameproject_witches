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

	public class apEditorTroubleShooting : EditorWindow
	{
		//private static apEditor2 s_window = null;

		//--------------------------------------------------------------
		//우선순위가 10 이상 차이가 나면 구분자가 생긴다.

		[MenuItem("Window/AnyPortrait/Reset Editor", false, 21)]
		public static void ShowWindow()
		{
			apEditor.CloseEditor();
			Debug.LogWarning("AnyPortrait Editor is Closed.");
		}
		
		//--------------------------------------------------------------
		public static apEditor.LANGUAGE GetLanguage()
		{
			return (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
		}
		//public static bool IsKorean()
		//{
		//	apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
		//	return language == apEditor.LANGUAGE.Korean;
		//}


		[MenuItem("Window/AnyPortrait/Homepage", false, 41)]
		public static void OpenHomepage()
		{
			Application.OpenURL("https://www.rainyrizzle.com/");
		}

		[MenuItem("Window/AnyPortrait/Getting Started", false, 42)]
		public static void OpenGettingStarted()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-kor";
				url = "https://rainyrizzle.github.io/kr/GettingStarted.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-jp";
				url = "https://rainyrizzle.github.io/jp/GettingStarted.html";//주소 변경
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-eng";
				url = "https://rainyrizzle.github.io/en/GettingStarted.html";//주소 변경
			}
			Application.OpenURL(url);
		}


		[MenuItem("Window/AnyPortrait/Advanced Manual", false, 43)]
		public static void OpenAdvancedManul()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-kor";
				url = "https://rainyrizzle.github.io/kr/AdManual.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-jp";
				url = "https://rainyrizzle.github.io/jp/AdManual.html";//주소 변경
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-eng";
				url = "https://rainyrizzle.github.io/en/AdManual.html";//주소 변경
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Scripting", false, 44)]
		public static void OpenScripting()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-kor";
				url = "https://rainyrizzle.github.io/kr/Script.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-jp";
				url = "https://rainyrizzle.github.io/jp/Script.html";//주소 변경				
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-eng";
				url = "https://rainyrizzle.github.io/en/Script.html";//주소 변경
			}
			Application.OpenURL(url);
		}


		//이 기능은 뺍시다.
		//[MenuItem("Window/AnyPortrait/Submit a Survey (Demo)", false, 81)]
		//public static void OpenSubmitASurvey()
		//{
		//	Application.OpenURL("https://goo.gl/forms/xZqTaXTesYq6v1Ba2");
		//}


		[MenuItem("Window/AnyPortrait/Report a Bug or Suggestion", false, 82)]
		public static void OpenReportABug()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-kor";
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-jp";
			}
			else
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-eng";
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Ask a Question", false, 83)]
		public static void OpenAskQuestion()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-kor";
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-jp";
			}
			else
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-eng";
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Forum", false, 84)]
		public static void OpenForum()
		{
			Application.OpenURL("https://www.rainyrizzle.com/ap-forum");
		}


		[MenuItem("Window/AnyPortrait/Open Asset Store Page", false, 100)]
		public static void OpenAssetStorePage()
		{
			apEditorUtil.OpenAssetStorePage();
		}
	}

}