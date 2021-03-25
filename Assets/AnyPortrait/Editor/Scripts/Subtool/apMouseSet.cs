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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// apMouse의 통합 처리를 위한 클래스
	/// 공통 변수를 묶고, 업데이트를 한번에 처리하기 위해서 추가되었다.
	/// ActionID라는 개념이 새로 도입되었다.
	/// </summary>
	public class apMouseSet
	{
		// Members
		//--------------------------------------
		public enum Button : int
		{
			Left = 0,
			Right = 1,
			Middle = 2,//<<Event의 버튼 순서
			LeftNoBound = 3,//Left이지만 범위에 관계없이 움직이는 경우
		}

		private apMouse[] _mouseBtn = null;
		private const int NUM_MOUSE_BTN = 4;
		private const int LEFT_NO_BOUND = 3;

		private Vector2 _mousePos = Vector2.zero;
		private Vector2 _mousePos_NotBound = Vector2.zero;
		private Vector2 _mousePos_Last = Vector2.zero;

		private int _wheelValue = 0;
		private bool _isAnyButtonUpEvent = false;//추가 20.3.31 : 버튼 중 하나라도 Up 이벤트가 있다면 일부 조건문을 통과해야한다.
		public bool IsAnyButtonUpEvent {  get { return _isAnyButtonUpEvent; } }


		//특정 액션에 대해 마우스 입력이 사용이 되고 있다면 원하는 키가 입력 되었더라도 사용이 불가능하다.
		//모든 키가 Released 상태라면 해제된다.
		public enum ACTION
		{
			None,
			ControllerGL,
			ScreenMove_MidBtn,
			MeshEdit_Modify,
			MeshEdit_Make,
			MeshEdit_Pivot,
			MeshEdit_AutoGen,
			Brush,
			MeshGroup_Setting,
			MeshGroup_Bone,
			MeshGroup_Modifier,
			MeshGroup_Animation,
			//TODO:여기에 추가해서 사용하자
		}
		private ACTION _actionID = ACTION.None;// 0 미만일 때에는 점유중인 액션이 없다.

		private int _curButtonIndex = -1;
		

		// Init
		//--------------------------------------
		public apMouseSet()
		{
			Init();
		}

		public void Init()
		{	
			_mouseBtn = new apMouse[4];
			for (int i = 0; i < NUM_MOUSE_BTN; i++)
			{
				_mouseBtn[i] = new apMouse();
				_mouseBtn[i].Init();
			}

			_mousePos = Vector2.zero;
			_actionID = ACTION.None;

			_curButtonIndex = -1;
			_isAnyButtonUpEvent = false;
		}

		/// <summary>
		/// 이벤트와 무관한 일부 변수를 초기화한다.
		/// </summary>
		public void InitMetaData()
		{
			_isAnyButtonUpEvent = false;
		}

		public void ReadyToUpdate()
		{
			for (int i = 0; i < NUM_MOUSE_BTN; i++)
			{
				_mouseBtn[i].ReadyToUpdate();
			}
			_wheelValue = 0;
			_isAnyButtonUpEvent = false;
		}

		public void SetMousePos(Vector2 mousePos, Vector2 mousePos_NotBound)
		{
			_mousePos = mousePos;
			_mousePos_Last = mousePos;
			_mousePos_NotBound = mousePos_NotBound;
		}


		public void Update_Wheel(int wheelOffset)
		{
			_wheelValue += wheelOffset;
		}

		public void Update_Button(EventType mouseEventType, int buttonIndex, bool isMouseInGUI)
		{
			switch (mouseEventType)
			{
				case EventType.MouseDown:
					{
						if (isMouseInGUI)
						{
							_mouseBtn[buttonIndex].Update_Pressed(_mousePos);
						}

						if (buttonIndex == 0)
						{
							//범위에 상관없이 왼쪽 클릭 체크
							_mouseBtn[LEFT_NO_BOUND].Update_Pressed(_mousePos);
						}
					}
					break;

				case EventType.MouseUp:
					{
						if(_mouseBtn[buttonIndex].Update_Released())
						{
							_isAnyButtonUpEvent = true;//Up 이벤트가 발생했다.
						}

						if (buttonIndex == 0)
						{
							//범위에 상관없이 왼쪽 클릭 체크
							_mouseBtn[LEFT_NO_BOUND].Update_Released();
						}
					}
					break;

				case EventType.MouseMove:
				case EventType.MouseDrag:
					{
						_mouseBtn[buttonIndex].Update_Moved();

						if (buttonIndex == 0)
						{
							//범위에 상관없이 왼쪽 클릭 체크
							_mouseBtn[LEFT_NO_BOUND].Update_Moved();
						}
					}
					break;
			}

			if(_curButtonIndex != buttonIndex)
			{
				//이전에 누른 마우스 버튼과 다르다.

				//나머지 전부 다 초기화한다.
				_curButtonIndex = buttonIndex;
				for (int i = 0; i < NUM_MOUSE_BTN; i++)
				{
					if(i != buttonIndex)
					{
						_mouseBtn[i].EndUpdate();
					}
				}
			}

			//하나라도 눌려있으면 actionID 유지, 그렇지 않다면 actionID 초기화
			bool isAnyPressed = false;
			isAnyPressed = _mouseBtn[0].IsPressed ? true : isAnyPressed;
			isAnyPressed = _mouseBtn[1].IsPressed ? true : isAnyPressed;
			isAnyPressed = _mouseBtn[2].IsPressed ? true : isAnyPressed;

			if(!isAnyPressed)
			{
				_actionID = ACTION.None;//<<Action ID 초기화
			}
		}


		public void Update_NoEvent()
		{
			//현재 마우스 이벤트가 아니라면
			//이전) 그냥 무시되는 프레임
			//변경)
			//- Up 이벤트가 있다면 Released로 모두 전환해야한다. 그 외에는 변환 없음
			//- 휠값은 0으로 초기화된다.
			//- 모든 버튼값이 눌리지 않았다면, ActionID를 초기화한다.
			for (int i = 0; i < NUM_MOUSE_BTN; i++)
			{
				_mouseBtn[i].SetReleasedIfUpStatus();//<<Up상태일때 Released로 전환하는 함수
			}

			bool isAnyPressed = false;
			isAnyPressed = _mouseBtn[0].IsPressed ? true : isAnyPressed;
			isAnyPressed = _mouseBtn[1].IsPressed ? true : isAnyPressed;
			isAnyPressed = _mouseBtn[2].IsPressed ? true : isAnyPressed;

			if(!isAnyPressed)
			{
				_actionID = ACTION.None;//<<Action ID 초기화
			}

			_isAnyButtonUpEvent = false;
			//_wheelValue = 0;
		}

		


		//어떤 액션을 했다면 ActionID를 등록하거나 키값을 갱신해야한다.
		public void UseWheel()
		{
			_wheelValue = 0;
			//휠 스크롤은 ActionID가 따로 없다.
		}

		/// <summary>
		/// 사용한 마우스 버튼의 ActionID를 등록하고, 다른 액션의 사용이 불가능하게 만든다.
		/// </summary>
		/// <param name="button"></param>
		/// <param name="actionID"></param>
		public void UseMouseButton(Button button, ACTION actionID)
		{
			if (_actionID == ACTION.None)
			{
				_actionID = actionID;
			}
		}

		/// <summary>
		/// UseMouseButton 함수의 Drag 버전. Mouse Down 위치를 현재 위치로 갱신한다.
		/// </summary>
		/// <param name="button"></param>
		/// <param name="actionID"></param>
		public void UseMouseDrag(Button button, ACTION actionID)
		{
			if (_actionID == ACTION.None)
			{
				_actionID = actionID;
			}

			_mouseBtn[(int)button].UseMouseDrag(_mousePos);
		}

		// Get / Set
		//-------------------------------------------------------------------
		public apMouse.MouseBtnStatus GetStatus(Button button, ACTION actionID)
		{
			if(_actionID == ACTION.None || _actionID == actionID)
			{
				return _mouseBtn[(int)button].Status;
			}
			return apMouse.MouseBtnStatus.Released;//<<유효하지 않은 처리다.
		}

		public apMouse.MouseBtnStatus GetStatusWithoutActionID(Button button)
		{
			return _mouseBtn[(int)button].Status;
		}

		public bool IsPressed(Button button, ACTION actionID)
		{
			if(_actionID == ACTION.None || _actionID == actionID)
			{
				return _mouseBtn[(int)button].Status == apMouse.MouseBtnStatus.Down ||
					_mouseBtn[(int)button].Status == apMouse.MouseBtnStatus.Pressed;
			}
			return false;
		}

		public bool IsValidAction(ACTION actionID)
		{
			return (_actionID == ACTION.None || _actionID == actionID);
		}

		public Vector2 Pos { get { return _mousePos; } }

		public Vector2 GetPosDelta(Button button)
		{
			return _mousePos - _mouseBtn[(int)button].PosDown;
		}

		public Vector2 PosLast { get { return _mousePos_Last; } }
		public Vector2 PosNotBound { get { return _mousePos_NotBound; } }

		public int Wheel {  get {  return _wheelValue; } }

	}
}