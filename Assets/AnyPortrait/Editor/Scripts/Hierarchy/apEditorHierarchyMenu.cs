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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Hierarchy에서 우클릭을 할 때 나오는 메뉴.
	/// 콜백과 파라미터 등이 많아서 기능의 일부를 대행한다.
	/// Hierachy마다 멤버로서 존재한다.
	/// </summary>
	public class apEditorHierarchyMenu
	{
		// Members
		//---------------------------------------------
		private apEditor _editor;

		//- 검색
		//- 이름 변경
		//- (위치 변경)
		//- 복제
		//- 삭제 등
		//이 중에서 필요한 걸 선택해서 메뉴 호출을 요청하자
		[Flags]
		public enum MENU_ITEM_HIERARCHY : int
		{
			None = 0,			
			Rename = 1,
			MoveUp = 2,
			MoveDown = 4,
			Search = 8,
			SelectAll = 16,
			Duplicate = 32,
			Remove = 64,
		}

		private int _hierarchyUnitType = 0;
		private object _requestedObj = null;


		//public class MenuCallbackParam
		//{
		//	public MENU_ITEM_HIERARCHY _selectedMenu = MENU_ITEM_HIERARCHY.None;
		//	public object _requestedObj = 
		//}

		public delegate void FUNC_MENU_SELECTED(MENU_ITEM_HIERARCHY menuType, int hierachyUnitType, object requestedObj);
		private FUNC_MENU_SELECTED _funcMenuSelected = null;
		private const string STR_EMPTY = "";

		// Init
		//---------------------------------------------
		public apEditorHierarchyMenu(apEditor editor, FUNC_MENU_SELECTED funcMenuSelected)
		{
			_editor = editor;
			_funcMenuSelected = funcMenuSelected;
		}


		// Functions
		//---------------------------------------------
		public void ShowMenu(MENU_ITEM_HIERARCHY menus, int hierachyUnitType, object requestObj)
		{
			_hierarchyUnitType = hierachyUnitType;
			_requestedObj = requestObj;

			GenericMenu newMenu = new GenericMenu();

			MENU_ITEM_HIERARCHY lastMenu = MENU_ITEM_HIERARCHY.None;

			//이름
			if((int)(menus & MENU_ITEM_HIERARCHY.Rename) != 0)
			{
				//검색
				//"Rename"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Rename)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Rename);
				lastMenu = MENU_ITEM_HIERARCHY.Rename;
			}

			//이동 (Up, Down)
			if ((int)(menus & MENU_ITEM_HIERARCHY.MoveUp) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Move Up"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.MoveUp)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.MoveUp);
				lastMenu = MENU_ITEM_HIERARCHY.MoveUp;
			}
			if ((int)(menus & MENU_ITEM_HIERARCHY.MoveDown) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None
					&& lastMenu != MENU_ITEM_HIERARCHY.MoveUp)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Move Down"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.MoveDown)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.MoveDown);
				lastMenu = MENU_ITEM_HIERARCHY.MoveDown;
			}

			//검색
			if((int)(menus & MENU_ITEM_HIERARCHY.Search) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Search"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Search)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Search);
			}
			//모두 선택
			if((int)(menus & MENU_ITEM_HIERARCHY.SelectAll) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None
					&& lastMenu != MENU_ITEM_HIERARCHY.Search)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Select All"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.SelectAll)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.SelectAll);
			}
			

			//복제
			if((int)(menus & MENU_ITEM_HIERARCHY.Duplicate) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Duplicate"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Duplicate)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Duplicate);
			}

			//삭제
			if((int)(menus & MENU_ITEM_HIERARCHY.Remove) != 0)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Remove"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Remove)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Remove);
			}

			newMenu.ShowAsContext();
		}

		private void OnMenuSelected(object obj)
		{
			if(_requestedObj == null
				|| _funcMenuSelected == null)
			{
				return;
			}
			if(!(obj is MENU_ITEM_HIERARCHY))
			{
				return;
			}

			MENU_ITEM_HIERARCHY menuType = (MENU_ITEM_HIERARCHY)obj;
			
			_funcMenuSelected(menuType, _hierarchyUnitType, _requestedObj);

		}
	}
}