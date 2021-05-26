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
using System.IO;
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// AnyPortrait 패키지의 위치를 저장하고 여는 클래스.
	/// 별도의 텍스트 파일에 저장하는 간단한 역할을 수행한다.
	/// apEditor의 멤버 변수이다.
	/// </summary>
	public class apPathSetting
	{
		// Members
		//-----------------------------------------------------
		public const string DEFAULT_PATH = "Assets/AnyPortrait/";
		private string _curPath = DEFAULT_PATH;

		private bool _isFirstLoaded = false;//처음으로 로드를 했는가



		// Init
		//-----------------------------------------------------
		public apPathSetting()
		{
			_curPath = DEFAULT_PATH;
			_isFirstLoaded = false;//처음으로 로드를 했는가
		}

		// Functions
		//-----------------------------------------------------
		public string Load()
		{
			_curPath = DEFAULT_PATH;
			string filePath = Application.dataPath + "/../AnyPortrait_EditorPath.txt";

			//저장 파일이 있는가
			FileInfo fi = new FileInfo(filePath);
			if (!fi.Exists)
			{
				_curPath = DEFAULT_PATH;
				_isFirstLoaded = true;//일단 실패했어도 로드 함수가 실행되었으니 오케이
				return DEFAULT_PATH;
			}

			//열어서 경로를 읽자
			FileStream fs = null;
			StreamReader sr = null;
			try
			{
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs);

				_curPath = sr.ReadLine();

				sr.Close();
				fs.Close();

				sr = null;
				fs = null;


				_isFirstLoaded = true;

				return _curPath;

			}
			catch (Exception)
			{
				if (sr != null)
				{
					sr.Close();
					sr = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				_curPath = DEFAULT_PATH;

				_isFirstLoaded = true;//일단 실패했어도 로드 함수가 실행되었으니 오케이

				return DEFAULT_PATH;
			}
		}

		public bool Save(string path)
		{
			_curPath = path;
			string filePath = Application.dataPath + "/../AnyPortrait_EditorPath.txt";

			FileStream fs = null;
			StreamWriter sw = null;
			try
			{
				fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs);

				sw.WriteLine(path);
				sw.Flush();

				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return true;

			}
			catch (Exception)
			{
				if (sw != null)
				{
					sw.Close();
					sw = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				return false;
			}
		}

		public void SetDefaultPath()
		{
			Save(DEFAULT_PATH);
		}


		public bool IsFirstLoaded
		{
			get
			{
				return _isFirstLoaded;
			}
		}

		public string CurrentPath
		{
			get
			{
				return _curPath;
			}
		}
	}
}