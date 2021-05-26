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
using System.Collections.Generic;
using System;
//using UnityEngine.Profiling;


using AnyPortrait;

namespace AnyPortrait
{

	public class apGizmos
	{
		// Members
		//--------------------------------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		public enum CONTROL_TYPE { Select, Move, Rotate, Scale }
		private CONTROL_TYPE _controlType = CONTROL_TYPE.Select;

		//public enum COORDINATE_TYPE { World, Parent, Local }
		public enum COORDINATE_TYPE { World, Local }
		private COORDINATE_TYPE _coordinate = COORDINATE_TYPE.World;


		//선택된 축
		public enum SELECTED_AXIS { None, Axis_X, Axis_Y, All }
		private SELECTED_AXIS _selectedAxis = SELECTED_AXIS.None;

		private bool _isGizmoDrag = false;
		private Vector2 _mousePosGL_Down = Vector2.zero;
		private Vector2 _mousePosW_Down = Vector2.zero;
		private Vector2 _mousePosW_Prev = Vector2.zero;
		private float _rotateAngle_Down = 0.0f;//<<첫 각도부터 얼마나 바뀌었는지 GUI로 표시하기 위함
		private float _rotateAngle_Prev = 0.0f;

		//영역 컨트롤
		//영역으로 잡아서 선택이 가능한가 + 컨트롤 포인트를 이용해서 왜곡이 가능한가
		private bool _isAreaSelectable = false;

		//현재 영역 선택 중인가
		private bool _isAreaSelecting = false;
		private SELECT_TYPE _areaSelectType = SELECT_TYPE.New;
		private Vector2 _areaPosStart_GL = Vector2.zero;
		private Vector2 _areaPosEnd_GL = Vector2.zero;
		private Vector2 _areaPosStart_W = Vector2.zero;
		private Vector2 _areaPosEnd_W = Vector2.zero;

		private bool _isCtrlKey = false;
		private bool _isShiftKey = false;
		private bool _isAltKey = false;

		private bool _isMouseEventProcessing = false;
		private apMouse.MouseBtnStatus _curBtnStatus_Left = apMouse.MouseBtnStatus.Up;
		private apMouse.MouseBtnStatus _curBtnStatus_Right = apMouse.MouseBtnStatus.Up;

		//Transformation 관련
		private int _nFFDPointX = 3;
		private int _nFFDPointY = 3;
		private bool _isFFDMode = false;
		private bool _isFFDModeAvailable = false;
		public bool IsFFDModeAvailable { get { return _isAreaSelectable && _isFFDModeAvailable && (!IsSoftSelectionMode && !IsBrushMode); } }//여러개를 선택할 수 있을때 + 다른 Mode가 꺼져있을 때 FFD가 가능하다
		public bool IsFFDMode { get { return _isFFDMode && _isFFDModeAvailable; } }

		//FFD 계산용 변수
		private List<object> _resultFFD_Objects = new List<object>();
		private List<Vector2> _resultFFD_Pos = new List<Vector2>();

		private bool _isSoftSelectionMode = false;
		private bool _isSoftSelectionModeAvailable = false;
		public bool IsSoftSelectionModeAvailable { get { return _isAreaSelectable && _isSoftSelectionModeAvailable && (!IsFFDMode && !IsBrushMode); } }//여러개를 선택할 수 있을때 + 다른 Mode가 꺼져있을 때 Soft Selection이 가능하다
		public bool IsSoftSelectionMode { get { return _isSoftSelectionMode && _isSoftSelectionModeAvailable; } }

		private bool _isBrushMode = false;
		private bool _isBrushModeAvailable = false;
		public bool IsBrushModeAvailable { get { return _isAreaSelectable && _isBrushModeAvailable && (!IsFFDMode && !IsSoftSelectionMode); } }//여러개를 선택할 수 있을때 + 다른 Mode가 꺼져있을 때 Blur가 가능하다
		public bool IsBrushMode { get { return _isBrushMode && _isBrushModeAvailable && _funcGizmoSyncBrushStatus != null && _funcGizmoPressBrush != null; } }

		//public const int MAX_SOFT_SELECTION_RADIUS = 500;//이전
		public const int MAX_SOFT_SELECTION_RADIUS = 1500;//변경 20.9.13 : 최대 크기가 커졌다.
		private int _softSelectionRadius = 50;
		private int _softSelectionCurveRatio = 0;//-100 (오목) ~ 0 (선형) - 100 (볼록)
		public int SoftSelectionRadius { get { return _softSelectionRadius; } }
		public int SoftSelectionCurveRatio { get { return _softSelectionCurveRatio; } }
		public apHotKey.HotKeyResult IncreaseSoftSelectionRadius(object paramObject)
		{
			if (_isSoftSelectionMode)
			{
				_softSelectionRadius = Mathf.Clamp(_softSelectionRadius + 10, 0, MAX_SOFT_SELECTION_RADIUS);
				RefreshSoftSelectionValue(_softSelectionRadius, _softSelectionCurveRatio);
				return apHotKey.HotKeyResult.MakeResult();
			}
			return null;
		}


		public apHotKey.HotKeyResult DecreaseSoftSelectionRadius(object paramObject)
		{
			if (_isSoftSelectionMode)
			{
				_softSelectionRadius = Mathf.Clamp(_softSelectionRadius - 10, 0, MAX_SOFT_SELECTION_RADIUS);
				RefreshSoftSelectionValue(_softSelectionRadius, _softSelectionCurveRatio);
				return apHotKey.HotKeyResult.MakeResult();
			}
			return null;
		}

		//public const int MAX_BRUSH_RADIUS = 500;//이전
		public const int MAX_BRUSH_RADIUS = 1500;//변경 20.9.13 : 최대 크기가 커졌다.
		private float _brushRadius = 50;
		private float _brushIntensity = 50;//초당 적용 정도 (중심부 기준)
		public float BrushRadius { get { return _brushRadius; } }
		public float BrushRadiusGL { get { return _brushRadius * apGL.Zoom; } }
		public float BrushIntensity { get { return _brushIntensity; } }

		//GUI용
		public enum BRUSH_COLOR_MODE
		{
			Increase_Lv1, Increase_Lv2, Increase_Lv3,
			Default,
			Decrease_Lv1, Decrease_Lv2, Decrease_Lv3,
		}
		private BRUSH_COLOR_MODE _brushColorMode = BRUSH_COLOR_MODE.Default;
		private Texture2D _brushImage = null;
		public BRUSH_COLOR_MODE BrushColorMode { get { return _brushColorMode; } }
		public Texture2D BrushImage { get { return _brushImage; } }
		private float _brushColorTime = 0.0f;
		private const float BRUSH_COLOR_TIME_LENGTH = 1.7f;
		public float BrushColorLerp { get { return Mathf.Sin(Mathf.PI * 2.0f * (_brushColorTime / BRUSH_COLOR_TIME_LENGTH)) * 0.5f + 0.5f; } }
		//public void IncreaseBrushRadius(object paramObject) { if (_isBrushMode) { _brushRadius = Mathf.Clamp(_brushRadius + 10, 0, MAX_BRUSH_RADIUS); RefreshBlurValue(_brushRadius, _brushIntensity); } }
		//public void DecreaseBrushRadius(object paramObject) { if (_isBrushMode) { _brushRadius = Mathf.Clamp(_brushRadius - 10, 0, MAX_BRUSH_RADIUS); RefreshBlurValue(_brushRadius, _brushIntensity); } }



		public class TransformControlPoint
		{
			public bool _isSelected = false;
			public Vector2 _normalizePos = Vector2.zero;
			public Vector2 _worldPos = Vector2.zero;
			public TransformControlPoint(Vector2 normalizePos, Vector2 worldPos)
			{
				_isSelected = false;
				_normalizePos = normalizePos;
				_worldPos = worldPos;
			}
		}

		public class TransformedObject
		{
			public object _srcObject = null;
			public Vector2 _prevWorldPos = Vector2.zero;
			public Vector2 _prevOrgData = Vector2.zero;//추가 20.7.22 : 저장된 원래의 데이터. World로 변형되지 않는다.
			public Vector2 _nextWorldPos = Vector2.zero;
			public Vector2 _normalizePos = Vector2.zero;

			public TransformedObject(object srcObject, Vector2 prevWorldPos, Vector2 orgData)
			{
				_srcObject = srcObject;
				_prevWorldPos = prevWorldPos;
				_nextWorldPos = prevWorldPos;
				_prevOrgData = orgData;
			}
		}
		private List<TransformControlPoint> _transformControlPoints = new List<TransformControlPoint>();
		private List<TransformedObject> _transformedObjects = new List<TransformedObject>();




		//-----------------------------------------------------------------------------------
		/// <summary>
		/// Select 이벤트시 리턴을 int형 개수로만 하지 않고, 어떤 데이터가 들어있는지도 확인한다.
		/// 여러개가 들어가있는 경우 List형 오브젝트 하나를 받고, 오브젝트 내의 아이템을 리스트로 옮긴다.
		/// </summary>
		public class SelectResult
		{
			private object _obj = null;
			private List<object> _objList = new List<object>();
			private int _nSelected = 0;

			public int NumSelected { get { return _nSelected; } }

			private static SelectResult _instanceMain = new SelectResult();
			private static SelectResult _instancePrev = new SelectResult();

			public static SelectResult Main { get { return _instanceMain; } }
			public static SelectResult Prev { get { return _instancePrev; } }

			public SelectResult()
			{
				Init();
			}
			public SelectResult Init()
			{
				_obj = null;
				_objList.Clear();
				_nSelected = 0;
				return this;
			}

			public SelectResult SetResult(SelectResult result)
			{
				if (result == null)
				{
					return Init();
				}
				_obj = result._obj;
				_objList.Clear();
				if (result._objList.Count > 0)
				{
					for (int i = 0; i < result._objList.Count; i++)
					{
						_objList.Add(result._objList[i]);
					}
				}
				_nSelected = result._nSelected;
				return this;
			}

			public SelectResult SetSingle(object obj)
			{
				_obj = obj;

				_objList.Clear();
				if (_obj != null)
				{
					_nSelected = 1;
				}
				else
				{
					_nSelected = 1;
				}
				return this;
			}
			public SelectResult SetMultiple(List<object> objList)
			{
				if (objList.Count == 0)
				{ return Init(); }
				if (objList.Count == 1)
				{ return SetSingle(objList[0]); }

				_obj = objList;
				_objList.Clear();
				for (int i = 0; i < objList.Count; i++)
				{
					_objList.Add(objList[i]);
				}
				_nSelected = _objList.Count;
				return this;
			}
			public SelectResult SetMultiple<T>(List<T> objList)
			{
				if (objList.Count == 0)
				{ return Init(); }
				if (objList.Count == 1)
				{ return SetSingle(objList[0]); }

				_obj = objList;
				_objList.Clear();
				for (int i = 0; i < objList.Count; i++)
				{
					_objList.Add(objList[i]);
				}
				_nSelected = _objList.Count;
				return this;
			}
			public static bool IsSameResult(SelectResult a, SelectResult b)
			{
				if (a == null && b == null)
				{ return true; }
				if (a == null || b == null)
				{ return false; }

				if (a._nSelected != b._nSelected)
				{
					return false;
				}
				if (a._obj != b._obj)
				{
					return false;
				}
				if (a._nSelected == 1)
				{
					if (a._obj != b._obj)
					{
						return false;
					}
				}
				else if (a._nSelected > 0)
				{
					if (a._objList.Count != b._objList.Count)
					{
						return false;
					}

					//리스트 전체를 봐야한다.
					//순서도 같아야함 (변경이 없어야하므로)
					for (int i = 0; i < a._nSelected; i++)
					{
						if (a._objList[i] != b._objList[i])
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		//-----------------------------------------------------------------------------------
		//이벤트 / 저장된 객체
		//public object _linkedObj = null;
		public bool _isGizmoEventRegistered = false;
		public bool _isGizmoRenderable = false;

		public enum SELECT_RESULT
		{
			None,
			NewSelected,
			SameSelected,
		}

		//private SELECT_RESULT _lastSelectResult = SELECT_RESULT.None;
		//private int _numSelected = 0;


		/// <summary>해당 위치에서 새로 선택을 한다. 동일한 객체를 선택했으면 True를 리턴한다. (새로 선택하거나 실패했으면 False)</summary>
		public delegate SelectResult FUNC_GIZMO_EVENT__SELECT(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, SELECT_TYPE selectType);
		public delegate void FUNC_GIZMO_EVENT_UNSELECT();//<<추가 : 마우스 우클릭을 하는 경우 Unselect를 할 수 있다. Unselect를 지원하는 경우 이 함수를 호출하도록 한다
		public delegate void FUNC_GIZMO_EVENT__MOVE(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove);
		public delegate void FUNC_GIZMO_EVENT__ROTATE(float deltaAngleW, bool isFirstRotate);
		public delegate void FUNC_GIZMO_EVENT__SCALE(Vector2 deltaScaleW, bool isFirstScale);

		//추가 20.1.26 : 키보드 입력으로 기즈모를 제어할 수 있다.
		//이 함수는 실패할 경우엔 HotKey나 다른 GUI 이벤트를 인식하도록 해야하므로, 리턴값을 받는다.
		public delegate bool FUNC_GIZMO_EVENT__KEYBOARD_MOVE(Vector2 deltaMoveW, bool isFirstMove);
		public delegate bool FUNC_GIZMO_EVENT__KEYBOARD_ROTATE(float deltaAngleW, bool isFirstRotate);
		public delegate bool FUNC_GIZMO_EVENT__KEYBOARD_SCALE(Vector2 deltaScaleL, bool isFirstScale);

		/// <summary>Link 직후에 만약 "Gizmo가 출력되는 상황"이 되면 numSelected = 0이므로 모순된 상황이 된다. 따라서 Link 직후에 혹시 바로 선택되는게 있는지 확인해야한다.</summary>
		/// <returns>선택된 객체의 개수</returns>
		public delegate SelectResult FUNC_GIZMO_EVENT__GET_NUM_SELECT_AFTER_LINK();

		//추가 : 영역 선택과 Transform
		public enum SELECT_TYPE
		{
			New,
			Add, Subtract
		}
		public delegate SelectResult FUNC_GIZMO_EVENT__MULTIPLE_SELECT(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType);


		public delegate void FUNC_TRANSFORM__POSITION(Vector2 pos);
		public delegate void FUNC_TRANSFORM__ROTATE(float angle);
		public delegate void FUNC_TRANSFORM__SCALE(Vector2 scale);
		public delegate void FUNC_TRANSFORM__DEPTH(int depth);
		public delegate void FUNC_TRANSFORM__COLOR(Color color, bool isVisible);
		public delegate void FUNC_TRANSFORM__EXTRA();
		//public delegate void FUNC_TRANSFORM__BONE_IK_MIXWeight(float boneIKMixWeight);//<<이거 다시 삭제

		public enum FFD_ASSIGN_TYPE { WorldPos, LocalData }

		public delegate bool FUNC_GIZMO_EVENT__FFD_TRANSFORM(List<object> srcObjects, List<Vector2> posData, FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord);
		public delegate bool FUNC_GIZMO_EVENT__START_FFD_TRANSFORM();

		//Soft Selection 이벤트...
		//선택을 했거나 선택 영역이 바뀌었을때 "기준 Vert"와 "간접 Vert"를 갱신하는 처리를 한다.
		//TRS에서는 직접 다른 이벤트 내에서 처리를 해줘야함
		public delegate bool FUNC_GIZMO_EVENT__START_SOFT_SELECTION();

		//Blur 이벤트
		//마우스를 우클릭했을 때의 이벤트 + 브러시의 값을 받아오는 이벤트 (업데이트에서 사용)
		public class BrushInfo
		{
			public float _radius = 0.0f;
			public float _intensity = 0.0f;
			public BRUSH_COLOR_MODE _colorMode = BRUSH_COLOR_MODE.Default;
			public Texture2D _image = null;

			private static BrushInfo _instance = new BrushInfo();
			public static BrushInfo MakeInfo(float radius, float intensity, BRUSH_COLOR_MODE colorMode, Texture2D image)
			{
				_instance._radius = radius;
				_instance._intensity = intensity;
				_instance._colorMode = colorMode;
				_instance._image = image;
				return _instance;
			}
			private BrushInfo() { }
		}
		public delegate BrushInfo FUNC_GIZMO_EVENT__SYNC_BRUSH_STATUS(bool isEnded);
		public delegate bool FUNC_GIZMO_EVENT__PRESS_BRUSH(Vector2 pos, float tDelta, bool isFirstMove);

		//추가 3.24 : 단축키를 등록하는 이벤트를 받는다.
		//별다른 처리를 하지는 않고, 단축키 등록 타이밍만 알려줄 뿐이다.
		//추가 20.1.24 : 기즈모의 현재 상태에 따라서 단축키가 다를 수 있으므로, 현재 상태를 인자로 넘겨주자
		public delegate void FUNC_GIZMO_EVENT__ADD_HOTKEYS(bool isGizmoRenderable, CONTROL_TYPE controlType, bool isFFDMode);

		public enum BASIC_HOTKEY_PARAM
		{
			LeftArrow, RightArrow, UpArrow, DownArrow
		}


		[Flags]
		public enum TRANSFORM_UI
		{
			None = 0,
			Position2D = 1,//<<Depth가 빠져있다. (나중에 이름 바꾸자)
			Rotation = 2,
			Scale = 4,
			Depth = 8,
			Color = 32,
			Extra = 64,
			Vertex_Transform = 128,
			BoneIKController = 256,//<<추가
			TRS_NoDepth = 1 | 2 | 4,
			TRS_WithDepth = 1 | 2 | 4 | 8,
		}

		private TRANSFORM_UI _transformUIVisible = TRANSFORM_UI.None;
		private TRANSFORM_UI _curValidTransformUI = TRANSFORM_UI.None;

		public enum TRANSFORM_UI_VALID
		{
			Hide,
			ShowAndDisabled,
			ShowAndEnabled
		}

		public TRANSFORM_UI_VALID TransformUI_Position { get { return GetTransformUIValid(TRANSFORM_UI.Position2D); } }
		public TRANSFORM_UI_VALID TransformUI_Rotation { get { return GetTransformUIValid(TRANSFORM_UI.Rotation); } }
		public TRANSFORM_UI_VALID TransformUI_Scale { get { return GetTransformUIValid(TRANSFORM_UI.Scale); } }
		public TRANSFORM_UI_VALID TransformUI_Depth { get { return GetTransformUIValid(TRANSFORM_UI.Depth); } }//<<Position에서 분리됨

		public TRANSFORM_UI_VALID TransformUI_Color { get { return GetTransformUIValid(TRANSFORM_UI.Color); } }
		public TRANSFORM_UI_VALID TransformUI_Extra { get { return GetTransformUIValid(TRANSFORM_UI.Extra); } }//<<추가됨

		public TRANSFORM_UI_VALID TransformUI_VertexTransform { get { return GetTransformUIValid(TRANSFORM_UI.Vertex_Transform); } }
		public TRANSFORM_UI_VALID TransformUI_BoneIKController { get { return GetTransformUIValid(TRANSFORM_UI.BoneIKController); } }

		private TRANSFORM_UI_VALID GetTransformUIValid(TRANSFORM_UI uiType)
		{
			if ((int)(_transformUIVisible & uiType) != 0)
			{
				if ((int)(_curValidTransformUI & uiType) != 0)
				{
					return TRANSFORM_UI_VALID.ShowAndEnabled;
				}
				else
				{
					return TRANSFORM_UI_VALID.ShowAndDisabled;
				}
			}
			else
			{
				return TRANSFORM_UI_VALID.Hide;
			}
		}


		public class TransformParam
		{
			private static TransformParam s_param = new TransformParam();

			public Vector2 _posW = Vector2.zero;
			public float _angle = 0.0f;
			public Vector2 _scale = Vector2.one;
			public int _depth = 0;
			public Color _color = Color.black;
			public bool _isVisible = true;

			public apMatrix3x3 _matrixToWorld = apMatrix3x3.identity;

			public bool _isMultipleSelected = false;


			public TRANSFORM_UI _curValidTransformUI = TRANSFORM_UI.None;

			//GUI용 TRS는 따로 출력하자
			public Vector2 _pos_GUI = Vector2.zero;
			public float _angle_GUI = 0.0f;
			public Vector2 _scale_GUI = Vector2.zero;

			//추가 5.10 : Bone IK Controller
			//public float _boneIKMixWeight = 0.0f;

			public static TransformParam Make(Vector2 posW,
												float angle,
												Vector2 scale,
												int depth,
												Color color,
												bool isVisible,
												apMatrix3x3 matrixWorld,
												bool isMultipleSelected,
												TRANSFORM_UI curValidTransformUI,
												Vector2 pos_GUI,
												float angle_GUI,
												Vector2 scale_GUI
												//float boneIKMixWeight
												)
			{
				s_param.SetParam(posW, angle, scale, depth, color, isVisible, matrixWorld, isMultipleSelected, curValidTransformUI, pos_GUI, angle_GUI, scale_GUI);
				return s_param;
			}

			private TransformParam()
			{

			}

			private void SetParam(Vector2 posW,
									float angle,
									Vector2 scale,
									int depth,
									Color color,
									bool isVisible,
									apMatrix3x3 matrixWorld,
									bool isMultipleSelected,
									TRANSFORM_UI curValidTransformUI,
									Vector2 pos_GUI,
									float angle_GUI,
									Vector2 scale_GUI
								//float boneIKMixWeight
								)
			{
				_posW = posW;
				_angle = angle;
				_scale = scale;
				_depth = depth;
				_color = color;
				_isVisible = isVisible;
				_matrixToWorld = matrixWorld;
				_isMultipleSelected = isMultipleSelected;
				_curValidTransformUI = curValidTransformUI;
				_pos_GUI = pos_GUI;
				_angle_GUI = angle_GUI;
				_scale_GUI = scale_GUI;
				//_boneIKMixWeight = boneIKMixWeight;
			}
		}
		public delegate TransformParam FUNC_PIVOT_RETURM();

		private FUNC_GIZMO_EVENT__GET_NUM_SELECT_AFTER_LINK _funcGizmoGetNumSelectAfterLink = null;
		private FUNC_GIZMO_EVENT__SELECT _funcGizmoSelect = null;
		private FUNC_GIZMO_EVENT_UNSELECT _funcGizmoUnselect = null;
		private FUNC_GIZMO_EVENT__MOVE _funcGizmoMove = null;
		private FUNC_GIZMO_EVENT__ROTATE _funcGizmoRotate = null;
		private FUNC_GIZMO_EVENT__SCALE _funcGizmoScale = null;
		private FUNC_TRANSFORM__POSITION _funcTransformPosition = null;
		private FUNC_TRANSFORM__ROTATE _funcTransformRotate = null;
		private FUNC_TRANSFORM__SCALE _funcTransformScale = null;
		private FUNC_TRANSFORM__DEPTH _funcTransformDepth = null;
		private FUNC_TRANSFORM__COLOR _funcTransformColor = null;
		private FUNC_TRANSFORM__EXTRA _funcTransformExtra = null;
		//private FUNC_TRANSFORM__BONE_IK_MIXWeight _funcTransformBoneIKMixWeight = null;//<<추가
		private FUNC_PIVOT_RETURM _funcPivotReturn = null;


		//Multiple 추가
		private FUNC_GIZMO_EVENT__MULTIPLE_SELECT _funcGizmoMultipleSelect = null;

		//Transformation 추가
		private FUNC_GIZMO_EVENT__FFD_TRANSFORM _funcGizmoFFD = null;
		private FUNC_GIZMO_EVENT__START_FFD_TRANSFORM _funcGizmoFFDStart = null;


		private FUNC_GIZMO_EVENT__START_SOFT_SELECTION _funcGizmoSoftSelection = null;
		private FUNC_GIZMO_EVENT__SYNC_BRUSH_STATUS _funcGizmoSyncBrushStatus = null;
		private FUNC_GIZMO_EVENT__PRESS_BRUSH _funcGizmoPressBrush = null;
		//[추가] Transform을 참조할 지 여부 결정

		//추가 3.24 : 단축키 등록을 위한 이벤트 패스
		private FUNC_GIZMO_EVENT__ADD_HOTKEYS _func_AddHotKeys = null;

		//추가 20.1.26 : 키보드 입력에 의한 편집
		//이 이벤트는 HotKey 클래스와 같이 구현되어야 한다.
		private FUNC_GIZMO_EVENT__KEYBOARD_MOVE _func_KeyboardMove = null;
		private FUNC_GIZMO_EVENT__KEYBOARD_ROTATE _func_KeyboardRotate = null;
		private FUNC_GIZMO_EVENT__KEYBOARD_SCALE _func_KeyboardScale = null;

		//추가
		//강제 업데이트
		private float _tForceFlagRequest = -10.0f;

		private bool _isForceUpdateFlag = false;
		private bool _isForceDrawFlag = false;

		public void SetUpdate()
		{
			_tForceFlagRequest = 0.3f;
		}

		public void CheckUpdate(float tDelta)
		{
			if (_tForceFlagRequest < 0.0f)
			{
				return;
			}

			if (Event.current.type == EventType.Repaint)
			{
				_tForceFlagRequest -= tDelta;
				_isForceUpdateFlag = true;
			}
		}

		public bool IsDrawFlag { get { return _isForceDrawFlag; } }


		//------------------------------------------------------------------------------------
		// Images
		public enum IMAGE_TYPE
		{
			Origin_None,
			Origin_Axis,
			Transform_Move,
			Transform_Rotate,
			Transform_Scale,
			Helper,
			Bone_Origin,
			Bone_Body,
			TransformController
		}
		//public Dictionary<IMAGE_TYPE, Texture2D> _Images = new Dictionary<IMAGE_TYPE, Texture2D>();
		public Dictionary<IMAGE_TYPE, Vector2> _ImageSize = new Dictionary<IMAGE_TYPE, Vector2>();

		public Texture2D GetImage(IMAGE_TYPE gizmosImageType)
		{
			//if(_Images.Count == 0 || _Images[gizmosImageType] == null)
			//{
			//	_Images.Clear();
			//	LoadResources();
			//}

			//return _Images[gizmosImageType];

			switch (gizmosImageType)
			{
				case IMAGE_TYPE.Origin_None: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_OriginNone);
				case IMAGE_TYPE.Origin_Axis: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_OriginAxis);
				case IMAGE_TYPE.Transform_Move: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Transform_Move);
				case IMAGE_TYPE.Transform_Rotate: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Transform_Rotate);
				case IMAGE_TYPE.Transform_Scale: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Transform_Scale);
				case IMAGE_TYPE.Helper: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Helper);
				case IMAGE_TYPE.Bone_Origin: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Bone_Origin);
				case IMAGE_TYPE.Bone_Body: return Editor.ImageSet.Get(apImageSet.PRESET.Gizmo_Bone_Body);
				case IMAGE_TYPE.TransformController: return Editor.ImageSet.Get(apImageSet.PRESET.TransformControlPoint);
			}
			return null;
		}

		public Vector2 GetImageSize(IMAGE_TYPE imageType)
		{
			return _ImageSize[imageType];
		}


		public enum COLOR_PRESET
		{
			Axis_X,
			Axis_Y,
			Axis_Selected,
			Rotate,
			Rotate_Selected,
			All,
			All_Selected,
			Origin,
			Origin_Selected,
			Bone,
			Bone_Selected,
			Helper,
			Helper_Selected,
			TransformController,
			TransformController_Selected,
		}


		private Dictionary<COLOR_PRESET, Color> _imageColorPreset = new Dictionary<COLOR_PRESET, Color>();
		public Color GetColor(COLOR_PRESET colorPreset) { return _imageColorPreset[colorPreset]; }
		public Color GetColorFor2X(COLOR_PRESET colorPreset)
		{
			Color resultColor = _imageColorPreset[colorPreset];
			resultColor.r /= 2;
			resultColor.g /= 2;
			resultColor.b /= 2;

			return resultColor;
		}

		//public bool _isGizmoLock = false;




		// 컨트롤을 위한 변수들
		//----------------------------------------------------------------------------------
		private Vector2 _originPos = Vector2.zero;
		private apMatrix3x3 _mtrx_origin = apMatrix3x3.identity;
		private apMatrix3x3 _mtrx_localRotate = apMatrix3x3.identity;
		private apMatrix3x3 _mtrx_move_axisY = apMatrix3x3.identity;
		private apMatrix3x3 _mtrx_move_axisX = apMatrix3x3.identity;
		private apMatrix3x3 _mtrx_scale_axisY = apMatrix3x3.identity;
		private apMatrix3x3 _mtrx_scale_axisX = apMatrix3x3.identity;
		private Vector2 _axisImageSize_Move = Vector2.one;
		private Vector2 _axisImageSize_Scale = Vector2.one;

		//private Vector2 _axisDir_Move_AxisX = Vector2.zero;
		//private Vector2 _axisDir_Move_AxisY = Vector2.zero;
		//private Vector2 _axisDir_Scale_AxisX = Vector2.zero;
		//private Vector2 _axisDir_Scale_AxisY = Vector2.zero;

		//private object _selectedObject = null;
		//private Vector2 _mouseDownPos = Vector2.zero;//<<이걸 안쓴다고?
		//private Vector2 _mousePos = Vector2.zero;//<<이것도 안써요?

		private bool _isUpdatedPerFrame = false;

		private TransformParam _curTransformParam = null;

		private bool _isMovePressed = false;

		private float _tMouseUpdate = 0.0f;
		//private Vector2 _checkPrevMousePos = Vector2.zero;

		private float _tMouseUpdate_Brush = 0.0f;


		//추가 20.1.27 : 연속된 키 입력인지 체크하자 (undo에 등록시키기 위해서)
		//동일한 입력, 동일한 이벤트가 동시에 발생하는지 체크하며, 마우스 이벤트나 다른 이벤트가 발생하면 초기화된다.
		//다른걸 선택해도 초기화
		private enum KEYBOARD_CONT_EVENT
		{
			None, 
			Move_FFD, Rotate_FFD, Scale_FFD,
			Move_Normal, Rotate_Normal, Scale_Normal
		}
		private KEYBOARD_CONT_EVENT _keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
		private KEYBOARD_CONT_EVENT _prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;

		// Init
		//--------------------------------------------------------------------------
		public apGizmos(apEditor editor)
		{
			_editor = editor;
		}

		public void LoadResources()
		{
			if (_ImageSize.Count == 0)
			{

				_ImageSize.Clear();

				_ImageSize.Add(IMAGE_TYPE.Origin_None, new Vector2(24, 24));
				_ImageSize.Add(IMAGE_TYPE.Origin_Axis, new Vector2(64, 64));

				_ImageSize.Add(IMAGE_TYPE.Transform_Move, new Vector2(32, 64));
				_ImageSize.Add(IMAGE_TYPE.Transform_Rotate, new Vector2(128, 128));
				_ImageSize.Add(IMAGE_TYPE.Transform_Scale, new Vector2(32, 64));

				_ImageSize.Add(IMAGE_TYPE.Helper, new Vector2(128, 128));

				_ImageSize.Add(IMAGE_TYPE.Bone_Origin, new Vector2(64, 64));
				_ImageSize.Add(IMAGE_TYPE.Bone_Body, new Vector2(32, 256));
				_ImageSize.Add(IMAGE_TYPE.TransformController, new Vector2(26, 26));

				_imageColorPreset.Clear();

				_imageColorPreset.Add(COLOR_PRESET.Axis_X, new Color(1.0f, 0.05f, 0.05f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.Axis_Y, new Color(0.05f, 1.0f, 0.05f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.Axis_Selected, new Color(1.0f, 1.0f, 0.05f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.Rotate, new Color(0.0f, 0.6f, 0.8f, 0.7f));
				_imageColorPreset.Add(COLOR_PRESET.Rotate_Selected, new Color(0.0f, 1.0f, 1.0f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.All, new Color(1.0f, 0.9f, 0.05f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.All_Selected, new Color(1.0f, 1.0f, 0.05f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.Origin, new Color(0.1f, 0.9f, 1.0f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.Origin_Selected, new Color(1.0f, 0.9f, 0.05f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.Bone, new Color(0.9f, 0.2f, 1.0f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.Bone_Selected, new Color(1.0f, 0.9f, 0.05f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.Helper, new Color(1.0f, 0.5f, 0.0f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.Helper_Selected, new Color(1.0f, 0.9f, 0.05f, 1.0f));

				_imageColorPreset.Add(COLOR_PRESET.TransformController, new Color(0.5f, 0.5f, 0.5f, 1.0f));
				_imageColorPreset.Add(COLOR_PRESET.TransformController_Selected, new Color(1.0f, 1.0f, 0.0f, 1.0f));




				//	Axis_Y,
				//Axis_Selected,
				//All,
				//All_Selected,
				//Origin,
				//Bone_Selected,
				//Helper_Selected,
			}
		}

		//---------------------------------------------------------------------------
		public class GizmoEventSet
		{
			private static GizmoEventSet _instance = null;
			public static GizmoEventSet I
			{
				get
				{
					if (_instance == null)
					{
						_instance = new GizmoEventSet();
					}
					return _instance;
				}
			}


			public FUNC_GIZMO_EVENT__GET_NUM_SELECT_AFTER_LINK _funcGizmoGetNumSelectAfterLink;
			public FUNC_GIZMO_EVENT__SELECT _funcGizmoSelect;
			public FUNC_GIZMO_EVENT_UNSELECT _funcGizmoUnselect;
			public FUNC_GIZMO_EVENT__MOVE _funcGizmoMove;
			public FUNC_GIZMO_EVENT__ROTATE _funcGizmoRotate;
			public FUNC_GIZMO_EVENT__SCALE _funcGizmoScale;
			public FUNC_TRANSFORM__POSITION _funcTransformPosition;
			public FUNC_TRANSFORM__ROTATE _funcTransformRotate;
			public FUNC_TRANSFORM__SCALE _funcTransformScale;
			public FUNC_TRANSFORM__DEPTH _funcTransformDepth;
			public FUNC_TRANSFORM__COLOR _funcTransformColor;
			public FUNC_TRANSFORM__EXTRA _funcTransformExtra;
			public FUNC_PIVOT_RETURM _funcPivotReturn;
			public FUNC_GIZMO_EVENT__MULTIPLE_SELECT _funcGizmoMultipleSelect;
			public FUNC_GIZMO_EVENT__FFD_TRANSFORM _funcGizmoFFD;
			public FUNC_GIZMO_EVENT__START_FFD_TRANSFORM _funcGizmoFFDStart;
			public FUNC_GIZMO_EVENT__START_SOFT_SELECTION _funcGizmoSoftSelection = null;
			public FUNC_GIZMO_EVENT__SYNC_BRUSH_STATUS _funcGizmoGetBrushInfo = null;
			public FUNC_GIZMO_EVENT__PRESS_BRUSH _funcGizmoPressBrush = null;
			public FUNC_GIZMO_EVENT__ADD_HOTKEYS _func_AddHotKeys = null;
			public FUNC_GIZMO_EVENT__KEYBOARD_MOVE _func_KeyboardMove = null;
			public FUNC_GIZMO_EVENT__KEYBOARD_ROTATE _func_KeyboardRotate = null;
			public FUNC_GIZMO_EVENT__KEYBOARD_SCALE _func_KeyboardScale = null;
			//public FUNC_TRANSFORM__BONE_IK_MIXWeight _funcTransformBoneIKMixWeight = null;

			public TRANSFORM_UI _transformUIVisible;

			public GizmoEventSet()
			{
				Clear();
			}

			public void Clear()
			{
				_funcGizmoGetNumSelectAfterLink = null;
				_funcGizmoSelect = null;
				_funcGizmoUnselect = null;
				_funcGizmoMove = null;
				_funcGizmoRotate = null;
				_funcGizmoScale = null;
				_funcTransformPosition = null;
				_funcTransformRotate = null;
				_funcTransformScale = null;
				_funcTransformDepth = null;
				_funcTransformColor = null;
				_funcTransformExtra = null;
				_funcPivotReturn = null;
				_funcGizmoMultipleSelect = null;
				_funcGizmoFFD = null;
				_funcGizmoFFDStart = null;
				_funcGizmoSoftSelection = null;
				_funcGizmoGetBrushInfo = null;
				_funcGizmoPressBrush = null;
				_func_AddHotKeys = null;
				_func_KeyboardMove = null;
				_func_KeyboardRotate = null;
				_func_KeyboardScale = null;
			}

			public GizmoEventSet SetEvent_1_Basic(FUNC_GIZMO_EVENT__SELECT funcGizmoSelect,
													FUNC_GIZMO_EVENT_UNSELECT funcGizmoUnselect,
													FUNC_GIZMO_EVENT__MOVE funcGizmoMove,
													FUNC_GIZMO_EVENT__ROTATE funcGizmoRotate,
													FUNC_GIZMO_EVENT__SCALE funcGizmoScale,
													FUNC_PIVOT_RETURM funcPivotReturn

									)
			{
				_funcGizmoSelect = funcGizmoSelect;
				_funcGizmoUnselect = funcGizmoUnselect;
				_funcGizmoMove = funcGizmoMove;
				_funcGizmoRotate = funcGizmoRotate;
				_funcGizmoScale = funcGizmoScale;
				_funcPivotReturn = funcPivotReturn;

				return this;
			}

			public GizmoEventSet SetEvent_2_TransformGUI(FUNC_TRANSFORM__POSITION funcTransformPosition,
															FUNC_TRANSFORM__ROTATE funcTransformRotate,
															FUNC_TRANSFORM__SCALE funcTransformScale,
															FUNC_TRANSFORM__DEPTH funcTransformDepth,
															FUNC_TRANSFORM__COLOR funcTransformColor,
															FUNC_TRANSFORM__EXTRA funcTransformExtra,
															TRANSFORM_UI transformUIVisible)
			{
				_funcTransformPosition = funcTransformPosition;
				_funcTransformRotate = funcTransformRotate;
				_funcTransformScale = funcTransformScale;
				_funcTransformDepth = funcTransformDepth;
				_funcTransformColor = funcTransformColor;
				_funcTransformExtra = funcTransformExtra;
				_transformUIVisible = transformUIVisible;
				return this;
			}

			public GizmoEventSet SetEvent_3_Tools(FUNC_GIZMO_EVENT__MULTIPLE_SELECT funcGizmoMultipleSelect,
													FUNC_GIZMO_EVENT__FFD_TRANSFORM funcGizmoTransform,
													FUNC_GIZMO_EVENT__START_FFD_TRANSFORM funcGizmoTransformStart,
													FUNC_GIZMO_EVENT__START_SOFT_SELECTION funcGizmoSoftSelection,
													FUNC_GIZMO_EVENT__SYNC_BRUSH_STATUS funcGizmoGetBrushInfo,
													FUNC_GIZMO_EVENT__PRESS_BRUSH funcGizmoPressBrush)
			{
				_funcGizmoMultipleSelect = funcGizmoMultipleSelect;
				_funcGizmoFFD = funcGizmoTransform;
				_funcGizmoFFDStart = funcGizmoTransformStart;
				_funcGizmoSoftSelection = funcGizmoSoftSelection;

				_funcGizmoGetBrushInfo = funcGizmoGetBrushInfo;
				_funcGizmoPressBrush = funcGizmoPressBrush;

				return this;
			}

			public GizmoEventSet SetEvent_4_EtcAndKeyboard(FUNC_GIZMO_EVENT__GET_NUM_SELECT_AFTER_LINK funcGizmoGetNumSelectAfterLink,
															FUNC_GIZMO_EVENT__ADD_HOTKEYS func_AddHotKeys,
															FUNC_GIZMO_EVENT__KEYBOARD_MOVE func_KeyboardMove,
															FUNC_GIZMO_EVENT__KEYBOARD_ROTATE func_KeyboardRotate,
															FUNC_GIZMO_EVENT__KEYBOARD_SCALE func_KeyboardScale)
			{
				_funcGizmoGetNumSelectAfterLink = funcGizmoGetNumSelectAfterLink;

				_func_AddHotKeys = func_AddHotKeys;

				_func_KeyboardMove = func_KeyboardMove;
				_func_KeyboardRotate = func_KeyboardRotate;
				_func_KeyboardScale = func_KeyboardScale;

				return this;
			}

			//public GizmoEventSet(FUNC_GIZMO_EVENT__SELECT funcGizmoSelect,
			//						FUNC_GIZMO_EVENT_UNSELECT funcGizmoUnselect,
			//						FUNC_GIZMO_EVENT__MOVE funcGizmoMove,
			//						FUNC_GIZMO_EVENT__ROTATE funcGizmoRotate,
			//						FUNC_GIZMO_EVENT__SCALE funcGizmoScale,
			//						FUNC_TRANSFORM__POSITION funcTransformPosition,
			//						FUNC_TRANSFORM__ROTATE funcTransformRotate,
			//						FUNC_TRANSFORM__SCALE funcTransformScale,
			//						FUNC_TRANSFORM__DEPTH funcTransformDepth,
			//						FUNC_TRANSFORM__COLOR funcTransformColor,
			//						FUNC_TRANSFORM__EXTRA funcTransformExtra,
			//						//FUNC_TRANSFORM__BONE_IK_MIXWeight funcTransformBoneIKMixWeight,//추가
			//						FUNC_PIVOT_RETURM funcPivotReturn,
			//						FUNC_GIZMO_EVENT__MULTIPLE_SELECT funcGizmoMultipleSelect,
			//						FUNC_GIZMO_EVENT__FFD_TRANSFORM funcGizmoTransform,
			//						FUNC_GIZMO_EVENT__START_FFD_TRANSFORM funcGizmoTransformStart,
			//						FUNC_GIZMO_EVENT__START_SOFT_SELECTION funcGizmoSoftSelection,
			//						FUNC_GIZMO_EVENT__SYNC_BRUSH_STATUS funcGizmoGetBrushInfo,
			//						FUNC_GIZMO_EVENT__PRESS_BRUSH funcGizmoPressBrush,
			//						TRANSFORM_UI transformUIVisible,
			//						FUNC_GIZMO_EVENT__GET_NUM_SELECT_AFTER_LINK funcGizmoGetNumSelectAfterLink,
			//						FUNC_GIZMO_EVENT__ADD_HOTKEYS func_AddHotKeys,
			//						FUNC_GIZMO_EVENT__KEYBOARD_MOVE func_KeyboardMove,
			//						FUNC_GIZMO_EVENT__KEYBOARD_ROTATE func_KeyboardRotate,
			//						FUNC_GIZMO_EVENT__KEYBOARD_SCALE func_KeyboardScale
			//	)
			//{
			//	_funcGizmoSelect = funcGizmoSelect;
			//	_funcGizmoUnselect = funcGizmoUnselect;
			//	_funcGizmoMove = funcGizmoMove;
			//	_funcGizmoRotate = funcGizmoRotate;
			//	_funcGizmoScale = funcGizmoScale;
			//	_funcTransformPosition = funcTransformPosition;
			//	_funcTransformRotate = funcTransformRotate;
			//	_funcTransformScale = funcTransformScale;
			//	_funcTransformDepth = funcTransformDepth;
			//	_funcTransformColor = funcTransformColor;
			//	_funcTransformExtra = funcTransformExtra;
			//	//_funcTransformBoneIKMixWeight = funcTransformBoneIKMixWeight;
			//	_funcPivotReturn = funcPivotReturn;
			//	_funcGizmoMultipleSelect = funcGizmoMultipleSelect;
			//	_funcGizmoFFD = funcGizmoTransform;
			//	_funcGizmoFFDStart = funcGizmoTransformStart;
			//	_funcGizmoSoftSelection = funcGizmoSoftSelection;

			//	_funcGizmoGetBrushInfo = funcGizmoGetBrushInfo;
			//	_funcGizmoPressBrush = funcGizmoPressBrush;

			//	_transformUIVisible = transformUIVisible;
			//	_funcGizmoGetNumSelectAfterLink = funcGizmoGetNumSelectAfterLink;

			//	_func_AddHotKeys = func_AddHotKeys;

			//	_func_KeyboardMove = func_KeyboardMove;
			//	_func_KeyboardRotate = func_KeyboardRotate;
			//	_func_KeyboardScale = func_KeyboardScale;
			//}
		}

		public void LinkObject(GizmoEventSet gizmoEventSet)
		{
			//_linkedObj = linkedObj;//<<이건 Null이 될 수도 있다.
			_funcGizmoSelect = gizmoEventSet._funcGizmoSelect;
			_funcGizmoUnselect = gizmoEventSet._funcGizmoUnselect;
			_funcGizmoMove = gizmoEventSet._funcGizmoMove;
			_funcGizmoRotate = gizmoEventSet._funcGizmoRotate;
			_funcGizmoScale = gizmoEventSet._funcGizmoScale;
			_funcTransformPosition = gizmoEventSet._funcTransformPosition;
			_funcTransformRotate = gizmoEventSet._funcTransformRotate;
			_funcTransformScale = gizmoEventSet._funcTransformScale;
			_funcTransformDepth = gizmoEventSet._funcTransformDepth;
			_funcTransformColor = gizmoEventSet._funcTransformColor;
			_funcTransformExtra = gizmoEventSet._funcTransformExtra;
			//_funcTransformBoneIKMixWeight = gizmoEventSet._funcTransformBoneIKMixWeight;
			_funcPivotReturn = gizmoEventSet._funcPivotReturn;

			_funcGizmoMultipleSelect = gizmoEventSet._funcGizmoMultipleSelect;
			_funcGizmoFFD = gizmoEventSet._funcGizmoFFD;
			_funcGizmoFFDStart = gizmoEventSet._funcGizmoFFDStart;

			_funcGizmoSoftSelection = gizmoEventSet._funcGizmoSoftSelection;

			_funcGizmoSyncBrushStatus = gizmoEventSet._funcGizmoGetBrushInfo;
			_funcGizmoPressBrush = gizmoEventSet._funcGizmoPressBrush;

			_transformUIVisible = gizmoEventSet._transformUIVisible;

			_funcGizmoGetNumSelectAfterLink = gizmoEventSet._funcGizmoGetNumSelectAfterLink;


			_func_AddHotKeys = gizmoEventSet._func_AddHotKeys;

			_func_KeyboardMove = gizmoEventSet._func_KeyboardMove;
			_func_KeyboardRotate = gizmoEventSet._func_KeyboardRotate;
			_func_KeyboardScale = gizmoEventSet._func_KeyboardScale;

			_isFFDModeAvailable = false;
			_isSoftSelectionModeAvailable = false;
			_isBrushModeAvailable = false;

			if (_funcGizmoMultipleSelect == null)
			{
				_isAreaSelectable = false;//여러개를 동시에 선택할 수 없다.
			}
			else
			{
				_isAreaSelectable = true;

				if (_funcGizmoFFD != null)
				{
					_isFFDModeAvailable = true;
				}

				if (_funcGizmoSoftSelection != null)
				{
					_isSoftSelectionModeAvailable = true;
				}

				if (_funcGizmoPressBrush != null)
				{
					_isBrushModeAvailable = true;
				}
			}

			_isGizmoEventRegistered = true;
			_selectedAxis = SELECTED_AXIS.None;
			_isGizmoDrag = false;
			//Debug.Log("LinkObject");

			//_lastSelectResult = SELECT_RESULT.None;

			//_numSelected = 0;//<SelectResult로 변경
			SelectResult.Main.Init();
			SelectResult.Prev.Init();


			_curTransformParam = null;

			_isMouseEventProcessing = false;

			_isFFDMode = false;
			_isSoftSelectionMode = false;
			_isBrushMode = false;

			_transformControlPoints.Clear();
			_transformedObjects.Clear();

			_isMovePressed = false;

			if (_funcGizmoGetNumSelectAfterLink != null)
			{
				SelectResult result = _funcGizmoGetNumSelectAfterLink();
				if (result == null)
				{
					SelectResult.Main.Init();
				}

				//Main을 Prev에 적용
				SelectResult.Prev.SetResult(SelectResult.Main);

				//_numSelected = _funcGizmoGetNumSelectAfterLink();
			}

			//추가 20.1.27
			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
			_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
		}

		public void Unlink()
		{	
			if (_isFFDMode)
			{
				//만약 Transform 중이었다면
				//Cancel은 코드상 어렵겠지;
				bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.AdaptFFDTransformEdit_Title),
																_editor.GetText(TEXT.AdaptFFDTransformEdit_Body),
																_editor.GetText(TEXT.AdaptFFDTransformEdit_Okay),
																_editor.GetText(TEXT.AdaptFFDTransformEdit_No)
																);
				if (isResult)
				{
					AdaptTransformObjects(null);
				}
				else
				{
					RevertTransformObjects(null);
				}
			}

			//_linkedObj = null;
			_funcGizmoSelect = null;
			_funcGizmoUnselect = null;
			_funcGizmoMove = null;
			_funcGizmoRotate = null;
			_funcGizmoScale = null;
			_funcTransformPosition = null;
			_funcTransformRotate = null;
			_funcTransformScale = null;
			_funcTransformDepth = null;
			_funcTransformColor = null;
			_funcTransformExtra = null;
			//_funcTransformBoneIKMixWeight = null;
			_funcPivotReturn = null;

			_funcGizmoMultipleSelect = null;
			_isAreaSelectable = false;


			_funcGizmoFFD = null;
			_funcGizmoFFDStart = null;
			_funcGizmoSoftSelection = null;
			_funcGizmoSyncBrushStatus = null;
			_funcGizmoPressBrush = null;

			_func_AddHotKeys = null;

			_func_KeyboardMove = null;
			_func_KeyboardRotate = null;
			_func_KeyboardScale = null;

			//_lastSelectResult = SELECT_RESULT.None;
			//_numSelected = 0;
			SelectResult.Main.Init();
			SelectResult.Prev.Init();

			_isGizmoEventRegistered = false;
			_selectedAxis = SELECTED_AXIS.None;
			_isGizmoDrag = false;
			//Debug.LogError("Unlink");

			_curTransformParam = null;

			_isMouseEventProcessing = false;

			_isFFDMode = false;
			_isFFDModeAvailable = false;
			_transformControlPoints.Clear();
			_transformedObjects.Clear();


			_isMovePressed = false;

			_areaSelectType = SELECT_TYPE.New;
			_curBtnStatus_Left = apMouse.MouseBtnStatus.Released;
			_curBtnStatus_Right = apMouse.MouseBtnStatus.Released;
			_isFFDMode = false;
			_isFFDModeAvailable = false;

			_isGizmoRenderable = false;
			//_numSelected = 0;

			//Debug.Log("Gizmo Unlink");

			_transformUIVisible = TRANSFORM_UI.None;

			//추가 20.1.27
			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
			_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;

		}


		/// <summary>
		/// Undo 후에 이걸 호출해야 Undo 후 저장이 안되는 문제를 막을 수 있다.
		/// </summary>
		public void ResetEventAfterUndo()
		{
			//키보드 입력 초기화
			_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;

			//SoftSelection이 켜진 경우엔 Undo 직후에 SoftSelection 이벤트를 한번 호출해야
			//Weighted 리스트를 생성한다.
			if(_funcGizmoSoftSelection != null && IsSoftSelectionMode)
			{
				_funcGizmoSoftSelection();
			}

			//Undo일 때도 이걸 호출해서 선택된 객체를 가져온다.
			OnSelectedObjectsChanged();
		}



		//추가 20.7.5
		/// <summary>
		/// 만약 Gizmo가 아닌 외부에서 오브젝트가 선택되었을 때 (또는 해제되었을 때).
		/// Gizmo가 시작될 때도 LinkObject 함수가 뒤늦게 동작한다면 이 함수를 호출해주자.
		/// 편집모드 시작시에 이 함수 호출하는걸 추천.
		/// </summary>
		public void OnSelectedObjectsChanged()
		{
			if(_funcGizmoGetNumSelectAfterLink == null)
			{
				return;
			}
			SelectResult result = _funcGizmoGetNumSelectAfterLink();
			if (result == null)
			{
				SelectResult.Main.Init();
			}

			//Main을 Prev에 적용
			SelectResult.Prev.SetResult(SelectResult.Main);
		}

		public bool IsVisible { get { return _isGizmoEventRegistered && _isGizmoRenderable; } }
		public bool IsUpdatable { get { return _isGizmoEventRegistered; } }
		//--------------------------------------------------------------------------
		public void SetControlType(CONTROL_TYPE controlType)
		{
			_controlType = controlType;
		}

		public CONTROL_TYPE ControlType { get { return _controlType; } }

		public void SetCoordinateType(COORDINATE_TYPE coordinate)
		{
			_coordinate = coordinate;
		}

		public COORDINATE_TYPE Coordinate { get { return _coordinate; } }


		//public void SetLock(bool isLock)
		//{
		//	if(_isGizmoLock != isLock)
		//	{
		//		//Release();
		//		Unlink();
		//	}
		//	_isGizmoLock = isLock;
		//}

		//public bool IsLock {  get { return _isGizmoLock; } }
		//--------------------------------------------------------------------------
		public void ReadyToUpdate()
		{

			//if (!Event.current.isMouse )
			//{
			//	return;
			//}

			if (Event.current.isMouse
				|| Event.current.rawType == EventType.MouseDown
				|| Event.current.rawType == EventType.MouseDrag
				|| Event.current.rawType == EventType.MouseMove
				|| Event.current.rawType == EventType.MouseUp)
			{
				_isUpdatedPerFrame = false;
				_isGizmoRenderable = false;
			}
		}

		//public void Select(object target)
		//{
		//	if(_selectedObject != target)
		//	{
		//		Release();
		//		_selectedObject = target;
		//	}
		//}

		public bool Update(float tDelta,
							apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus,
							Vector2 mousePos, 
							bool isCtrlKey, bool isShiftKey, bool isAltKey,
							bool isIgnoredUp//UpEvent인데 Ignored가 발생한 경우 (주로 작업영역 밖에서 Up 이벤트가 발생했다.)
							)
		{

			if (_isForceUpdateFlag)
			{
				//Debug.LogError("Force Update");
				if (!_isFFDMode)
				{
					//1. 일반 Gizmo 모드일때
					if (_funcPivotReturn != null)
					{
						_curTransformParam = _funcPivotReturn();
					}
				}
				else
				{
					//2. Transform 모드일 때
					_curTransformParam = GetTransformControlPointParam();
				}

				_isForceUpdateFlag = false;
				_isForceDrawFlag = true;
			}

			if (_curTransformParam != null)
			{
				if ((int)(_curTransformParam._curValidTransformUI & TRANSFORM_UI.Vertex_Transform) != 0 ||
					(int)(_curTransformParam._curValidTransformUI & TRANSFORM_UI.Position2D) != 0 ||
					(int)(_curTransformParam._curValidTransformUI & TRANSFORM_UI.Rotation) != 0 ||
					(int)(_curTransformParam._curValidTransformUI & TRANSFORM_UI.Scale) != 0 ||
					(int)(_curTransformParam._curValidTransformUI & TRANSFORM_UI.TRS_NoDepth) != 0
					)
				{
					_isGizmoRenderable = true;
				}
				else
				{
					_isGizmoRenderable = false;
				}
			}
			else
			{
				_isGizmoRenderable = false;
			}

			//추가 3.24 : 단축키 등록을 위한 단계
			if (_func_AddHotKeys != null)
			{
				//이 이벤트를 호출하면, 단축키를 등록하는 코드가 실행될 것이다.
				//단축키가 실행되는건 아님
				_func_AddHotKeys(IsVisible, _controlType, _isFFDMode);
			}

			//추가 20.1.26 : 기본 단축키 이벤트가 존재하면 HotKey에 이벤트를 등록한다.
			//이 이벤트는 고정 Reserved 이벤트로 별도로 저장한다. (PopEvent 제외 대상)
			//컨트롤 타입 / FFD 여부에 따라서 이벤트 사용 여부가 결정된다.
			if (_isGizmoRenderable)
			{
				switch (_controlType)
				{
					case CONTROL_TYPE.Move:
						if (_isFFDMode && _isFFDModeAvailable)
						{	
							//FFD 이벤트
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Move, apHotKey.RESERVED_KEY.Arrow, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Move, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_EnterOrEscape, apHotKey.RESERVED_KEY.EnterOrEscape, null);
						}
						else
						{
							if(_func_KeyboardMove != null)
							{
								//일반 이벤트
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Move, apHotKey.RESERVED_KEY.Arrow, null);
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Move, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							}
							
						}
						break;

					case CONTROL_TYPE.Rotate:
						if (_isFFDMode && _isFFDModeAvailable)
						{	
							//FFD 이벤트
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Rotate, apHotKey.RESERVED_KEY.Arrow, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Rotate, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_EnterOrEscape, apHotKey.RESERVED_KEY.EnterOrEscape, null);
						}
						else
						{
							if(_func_KeyboardRotate != null)
							{
								//일반 이벤트
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Rotate, apHotKey.RESERVED_KEY.Arrow, null);
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Rotate, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							}
						}
						break;

					case CONTROL_TYPE.Scale:
						if (_isFFDMode && _isFFDModeAvailable)
						{	
							//FFD 이벤트
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Scale, apHotKey.RESERVED_KEY.Arrow, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_Scale, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							Editor.AddReservedHotKeyEvent(OnKeyboardEvent_FFD_EnterOrEscape, apHotKey.RESERVED_KEY.EnterOrEscape, null);
						}
						else
						{
							if(_func_KeyboardScale != null)
							{
								//일반 이벤트
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Scale, apHotKey.RESERVED_KEY.Arrow, null);
								Editor.AddReservedHotKeyEvent(OnKeyboardEvent_Normal_Scale, apHotKey.RESERVED_KEY.Arrow_Shift, null);
							}
						}
						break;
				}
			}

			bool isMouseEvent = Event.current.isMouse 
				|| Event.current.rawType == EventType.MouseDown
				|| Event.current.rawType == EventType.MouseMove
				|| Event.current.rawType == EventType.MouseDrag
				|| Event.current.rawType == EventType.MouseUp;

			if (!isMouseEvent
				&& Event.current.type != EventType.Repaint)
			{
				return false;
			}


			if (!IsUpdatable)
			{
				return false;
			}



			//if(leftBtnStatus == apMouse.MouseBtnStatus.Up)
			//{
			//	Debug.Log("Left Mouse - Up");
			//}



			//마우스 이벤트때 너무 빈번하게 처리가 되면 프레임이 떨어진다.
			//1) 마우스 이벤트 시간이 약간 지났거나
			//2) 정말 먼 거리를 움직였거나
			//3) 마우스 입력 상태가 바뀌었을때 업데이트한다.
			//실질적으로 Input
			bool isInputChanged = false;
			if (Event.current.type == EventType.Repaint)
			{
				_tMouseUpdate += tDelta;

				//브러시 모드일때 마우스 시간 업데이트
				if (IsBrushMode)
				{
					//브러시 크기와 강도 갱신
					BrushInfo brushInfo = _funcGizmoSyncBrushStatus(false);
					if (brushInfo != null)
					{
						_brushColorTime += tDelta;
						if (_brushColorTime > BRUSH_COLOR_TIME_LENGTH)
						{
							_brushColorTime -= BRUSH_COLOR_TIME_LENGTH;
						}
						_brushRadius = brushInfo._radius;
						_brushIntensity = brushInfo._intensity;
						_brushImage = brushInfo._image;
						_brushColorMode = brushInfo._colorMode;

						_tMouseUpdate_Brush += tDelta;
						isInputChanged = true;
					}
					else
					{
						_brushColorTime = 0.0f;
						_brushRadius = 0.0f;
						_tMouseUpdate_Brush = 0.0f;
						_brushImage = null;
						_brushColorMode = BRUSH_COLOR_MODE.Default;
					}
				}
			}
			//if (Event.current.isMouse)
			if(isMouseEvent)
			{
				if (_tMouseUpdate > 0.01f)
				{
					//50 FPS 기준 이상으로 작동함
					_tMouseUpdate = 0.0f;
					isInputChanged = true;
				}
			}

			apMouse.MouseBtnStatus prevLeftBtnStatus = _curBtnStatus_Left;
			apMouse.MouseBtnStatus prevRightBtnStatus = _curBtnStatus_Right;

			//마우스 로직은 MouseEvent에서만
			//if (Event.current.isMouse)
			if(isMouseEvent)
			{
				if (_isCtrlKey != isCtrlKey ||
					_isShiftKey != isShiftKey ||
					_isAltKey != isAltKey)
				{
					isInputChanged = true;
				}

				_isCtrlKey = isCtrlKey;
				_isShiftKey = isShiftKey;
				_isAltKey = isAltKey;

				if (leftBtnStatus == apMouse.MouseBtnStatus.Down ||
					leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					_isMouseEventProcessing = true;

					//추가 20.1.27 : 마우스 입력 > 키 입력 연속성 사라짐
					_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
					_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
				}

				if (leftBtnStatus == apMouse.MouseBtnStatus.Down ||
					leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if (_curBtnStatus_Left == apMouse.MouseBtnStatus.Released ||
						_curBtnStatus_Left == apMouse.MouseBtnStatus.Up)
					{
						_curBtnStatus_Left = apMouse.MouseBtnStatus.Down;
						_isAreaSelecting = false;
					}
					else
					{
						_curBtnStatus_Left = apMouse.MouseBtnStatus.Pressed;
					}
				}
				else
				{
					if (_curBtnStatus_Left == apMouse.MouseBtnStatus.Released ||
						_curBtnStatus_Left == apMouse.MouseBtnStatus.Up)
					{
						_curBtnStatus_Left = apMouse.MouseBtnStatus.Up;
					}
					else
					{
						_curBtnStatus_Left = apMouse.MouseBtnStatus.Released;
					}
				}

				if (rightBtnStatus == apMouse.MouseBtnStatus.Down ||
					rightBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if (_curBtnStatus_Right == apMouse.MouseBtnStatus.Released ||
						_curBtnStatus_Right == apMouse.MouseBtnStatus.Up)
					{
						_curBtnStatus_Right = apMouse.MouseBtnStatus.Down;
					}
					else
					{
						_curBtnStatus_Right = apMouse.MouseBtnStatus.Pressed;
					}

					//추가 20.1.27 : 마우스 입력 > 키 입력 연속성 사라짐
					_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
					_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
				}
				else
				{
					if (_curBtnStatus_Right == apMouse.MouseBtnStatus.Released ||
						_curBtnStatus_Right == apMouse.MouseBtnStatus.Up)
					{
						_curBtnStatus_Right = apMouse.MouseBtnStatus.Up;
					}
					else
					{
						_curBtnStatus_Right = apMouse.MouseBtnStatus.Released;
					}
				}


				if (prevLeftBtnStatus != _curBtnStatus_Left || prevRightBtnStatus != _curBtnStatus_Right)
				{
					isInputChanged = true;
				}

			}


			//GUI 로직은 둘다
			_curTransformParam = null;
			_curValidTransformUI = TRANSFORM_UI.None;

			if (!_isFFDMode)
			{
				//1. 일반 Gizmo 모드일때
				if (_funcPivotReturn != null)
				{
					_curTransformParam = _funcPivotReturn();
				}
			}
			else
			{
				//2. Transform 모드일 때
				_curTransformParam = GetTransformControlPointParam();
			}
			if (_curTransformParam != null)
			{
				_curValidTransformUI = _curTransformParam._curValidTransformUI;
			}

			if (isInputChanged)
			{
				//입력이 있을 때

				//1. 마우스 입력 이벤트
				//if (Event.current.isMouse)
				if(isMouseEvent)
				{
					//Profiler.BeginSample("apPortrait Gizmo Update");

					Vector2 mousePosW = apGL.GL2World(mousePos);

					if (IsSoftSelectionMode)
					{
						if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
						{
							EndSoftSelection();
						}
					}

					if (IsBrushMode)
					{
						//Blur 모드에서는 TRS보다 처리를 우선시한다.
						if (leftBtnStatus == apMouse.MouseBtnStatus.Down ||
							leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
						{
							if (_funcGizmoPressBrush != null)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									_tMouseUpdate_Brush = 0.0f;
								}

								//Debug.Log("Gizmo Brush : " + _tMouseUpdate_Brush + " (" + prevBrushTime + ") [" + leftBtnStatus + "]");
								_funcGizmoPressBrush(mousePos, _tMouseUpdate_Brush, !_isMovePressed);
								_tMouseUpdate_Brush = 0.0f;

								if (!_isMovePressed)
								{
									_isMovePressed = true;
								}

								Editor.SetRepaint();
							}
						}
						else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
						{
							EndBrush();
						}
					}
					else
					{

						switch (_controlType)
						{
							case CONTROL_TYPE.Select:
								Update_Select(_curBtnStatus_Left, _curBtnStatus_Right, mousePos, mousePosW);
								break;

							case CONTROL_TYPE.Move:
								Update_Move(_curBtnStatus_Left, _curBtnStatus_Right, mousePos, mousePosW);
								break;

							case CONTROL_TYPE.Rotate:
								Update_Rotate(_curBtnStatus_Left, _curBtnStatus_Right, mousePos, mousePosW);
								break;

							case CONTROL_TYPE.Scale:
								Update_Scale(_curBtnStatus_Left, _curBtnStatus_Right, mousePos, mousePosW);
								break;
						}
					}


					if (leftBtnStatus == apMouse.MouseBtnStatus.Released ||
						leftBtnStatus == apMouse.MouseBtnStatus.Up)
					{
						_isMouseEventProcessing = false;
					}

					if (_curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed)
					{
						Editor.SetRepaint();
						Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.
					}

					//Profiler.EndSample();
				}
				else if (Event.current.type == EventType.Repaint)
				{
					//Repaint 이벤트에서 : Mouse Pressed 이벤트때문에
					if (leftBtnStatus == apMouse.MouseBtnStatus.Down
						|| leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{
						if (IsBrushMode)
						{
							if (_funcGizmoPressBrush != null)
							{
								//float prevBrushTime = _tMouseUpdate_Brush;
								//if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								//{
								//	_tMouseUpdate_Brush = 0.0f;
								//}
								//Debug.Log("Gizmo Brush(Update) : " + _tMouseUpdate_Brush + " (" + prevBrushTime + ") [" + leftBtnStatus + "]");
								_funcGizmoPressBrush(mousePos, _tMouseUpdate_Brush, !_isMovePressed);
								_tMouseUpdate_Brush = 0.0f;

								if (!_isMovePressed)
								{
									_isMovePressed = true;
								}

								Editor.SetRepaint();
								//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.
							}
						}
					}
				}

				//추가 20.1.27 : 마우스 입력 > 키 입력 연속성 사라짐
				_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
				_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
			}

			if (_isMovePressed)
			{
				if (_controlType != CONTROL_TYPE.Move &&
					_controlType != CONTROL_TYPE.Rotate &&
					_controlType != CONTROL_TYPE.Scale &&
					!IsBrushMode)
				{
					_isMovePressed = false;
				}
			}

			if (_curTransformParam != null)
			{
				//_isGizmoRenderable = true;//<<위에서 세팅했다.

				apMatrix3x3 transformMtrx = _curTransformParam._matrixToWorld;

				_originPos = _curTransformParam._posW;
				Vector2 originPos2 = new Vector2(_originPos.x, _originPos.y);


				Vector2 unitVector = transformMtrx.MultiplyPoint(new Vector2(1, 0) + originPos2) - originPos2;
				unitVector.Normalize();


				float rotateAngle = _curTransformParam._angle;
				//Quaternion rotateQuat = Quaternion.Euler(0, 0, rotateAngle);

				apMatrix3x3 gizmoMatrix_World = apMatrix3x3.TRS(originPos2, 0, Vector3.one);
				apMatrix3x3 gizmoMatrix_Local = apMatrix3x3.TRS(originPos2, rotateAngle, Vector3.one);

				_axisImageSize_Move = GetImageSize(IMAGE_TYPE.Transform_Move) / apGL.Zoom;
				_axisImageSize_Scale = GetImageSize(IMAGE_TYPE.Transform_Move) / apGL.Zoom;

				apMatrix3x3 gizmoMatrix_Move = gizmoMatrix_World;

				//TODO : 이거 수정해야하는데..
				//Vector3 posAxisDir_World_X = new Vector3(1, 0, 0);
				//Vector3 posAxisDir_World_Y = new Vector3(0, 1, 0);

				//Vector3 posAxisDir_Local_X = gizmoMatrix_Local.MultiplyPoint3x4(posAxisDir_World_X).normalized;
				//Vector3 posAxisDir_Local_Y = gizmoMatrix_Local.MultiplyPoint3x4(posAxisDir_World_Y).normalized;

				_mtrx_localRotate = apMatrix3x3.TRS(Vector2.zero, rotateAngle, Vector2.one);

				//apMatrix3x3 mtrx_scale;
				switch (_coordinate)
				{
					//TODO : 좌표계에 따라서 다르게 보이려면...?
					case COORDINATE_TYPE.World:
						//transformMtrx = transformMtrx;
						{
							gizmoMatrix_Move = gizmoMatrix_World;

						}
						break;

					case COORDINATE_TYPE.Local:
						{
							gizmoMatrix_Move = gizmoMatrix_Local;
						}
						//transformMtrx = matrixLocal;
						break;
				}

				//_axisDir_Scale_AxisX = posAxisDir_Local_X;
				//_axisDir_Scale_AxisY = posAxisDir_Local_Y;


				_mtrx_origin = gizmoMatrix_Local;

				_mtrx_move_axisY = gizmoMatrix_Move * apMatrix3x3.TRS(new Vector2(0, _axisImageSize_Move.y / 2), 0, Vector2.one);
				_mtrx_move_axisX = gizmoMatrix_Move * apMatrix3x3.TRS(new Vector2(_axisImageSize_Move.y / 2, 0), -90, Vector2.one);

				if (_curTransformParam._scale.y >= 0.0f)
				{
					_mtrx_scale_axisY = gizmoMatrix_Local * apMatrix3x3.TRS(new Vector2(0, _axisImageSize_Scale.y / 2), 0, Vector2.one);//<<기본 식Y	
				}
				else
				{
					//Y 반전
					_mtrx_scale_axisY = gizmoMatrix_Local * apMatrix3x3.TRS(new Vector2(0, -_axisImageSize_Scale.y / 2), 180, Vector2.one);
				}

				if (_curTransformParam._scale.x >= 0.0f)
				{
					_mtrx_scale_axisX = gizmoMatrix_Local * apMatrix3x3.TRS(new Vector2(_axisImageSize_Scale.y / 2, 0), -90.0f, Vector2.one);//<<기본 식X
				}
				else
				{
					//X 반전
					_mtrx_scale_axisX = gizmoMatrix_Local * apMatrix3x3.TRS(new Vector2(-_axisImageSize_Scale.y / 2, 0), 90.0f, Vector2.one);
				}


				//테스트를 해보자
				//Scale이 반전되어 있다면 X, Y축 각각 반전해야함

			}



			_isUpdatedPerFrame = true;
			return true;
		}




		private SELECT_TYPE GetSelectType()
		{
			if (_isCtrlKey || _isShiftKey)
			{
				return SELECT_TYPE.Add;
			}
			else if (_isAltKey)
			{
				return SELECT_TYPE.Subtract;
			}
			return SELECT_TYPE.New;
		}



		private void Update_Select(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePosGL, Vector2 mousePosW)
		{
			switch (leftBtnStatus)
			{
				case apMouse.MouseBtnStatus.Down:
					{
						//int prevNumSelected = _numSelected;
						SelectResult selectResult = null;
						if (_isFFDMode)
						{
							//추가 : Transform 모드
							//_numSelected = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
							selectResult = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}
						}
						else
						{
							if (_funcGizmoSelect != null)
							{
								//_lastSelectResult = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
								//_numSelected = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
								selectResult = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());

								if (selectResult == null)
								{
									SelectResult.Main.Init();
								}
							}
							//Soft Selection 모드 선택시 새로 Vertex를 선택해주자
							if (IsSoftSelectionMode && _funcGizmoSoftSelection != null)
							{
								_funcGizmoSoftSelection();
							}
						}

						_mousePosGL_Down = mousePosGL;
						_mousePosW_Down = mousePosW;
						_mousePosW_Prev = mousePosW;

						//Debug.Log("Select -> Check [" + _numSelected + " <- " + prevNumSelected + "]");
						//CheckAndStartAreaSelect((_numSelected != prevNumSelected) && _numSelected != 0);

						//CheckAndStartAreaSelect(_numSelected, prevNumSelected);
						CheckAndStartAreaSelect(SelectResult.Main);//<<변경

						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);
					}
					break;

				case apMouse.MouseBtnStatus.Pressed:
					{
						_mousePosW_Prev = mousePosW;

						UpdateAreaInPressed(mousePosGL, mousePosW);
					}
					break;

				case apMouse.MouseBtnStatus.Released:
				case apMouse.MouseBtnStatus.Up:
					{
						if (_isMouseEventProcessing)
						{
							//Unselect는 없지만
							//축 선택은 초기화하자
							//_numSelected = ReleaseAreaSelect(mousePosGL, mousePosW);
							SelectResult selectResult = ReleaseAreaSelect(mousePosGL, mousePosW);
							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}

							//Main을 Prev에 적용
							SelectResult.Prev.SetResult(SelectResult.Main);

							_selectedAxis = SELECTED_AXIS.None;
							_isGizmoDrag = false;
						}
					}
					break;
			}
			if (rightBtnStatus == apMouse.MouseBtnStatus.Down && !_isMouseEventProcessing)
			{
				if (_funcGizmoUnselect != null)
				{
					if (Editor.Controller.IsMouseInGUI(mousePosGL))
					{
						_funcGizmoUnselect();//Unselect 함수 호출
					}
				}
			}
		}

		private void Update_Move(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePosGL, Vector2 mousePosW)
		{
			switch (leftBtnStatus)
			{
				case apMouse.MouseBtnStatus.Down:
					{
						//TODO : 
						//클릭한 위치에 따라
						//All / Axis-X / Axis-Y / 새로운 Select
						//결정해야 한다.
						//"새로운 Select"는 Gizmo 외부를 클릭했을 뿐,
						//다른 오브젝트는 아니어도 된다.
						//그 경우는 Select 이후 All Move로 판별한다.
						_selectedAxis = SELECTED_AXIS.None;
						_isGizmoDrag = false;

						bool isAxisClick_Origin = IsGizmoClickable_Move(mousePosW, SELECTED_AXIS.All);
						bool isAxisClick_X = IsGizmoClickable_Move(mousePosW, SELECTED_AXIS.Axis_X);
						bool isAxisClick_Y = IsGizmoClickable_Move(mousePosW, SELECTED_AXIS.Axis_Y);

						//SELECT_RESULT selectResult = SELECT_RESULT.None;
						bool isNewSelectable = false;

						//Debug.LogError("new Select : " + _lastSelectResult);

						//int prevNumSelected = _numSelected;

						SelectResult selectResult = null;

						if (//_numSelected == 0 
							SelectResult.Main.NumSelected == 0
							|| (!isAxisClick_Origin && !isAxisClick_X && !isAxisClick_Y)
							)
						{
							isNewSelectable = true;
						}

						if (isNewSelectable)
						{


							if (_isFFDMode)
							{
								//추가 : Transform 모드
								//_numSelected = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
								selectResult = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
							}
							else
							{
								if (_funcGizmoSelect != null)
								{
									//_numSelected = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
									selectResult = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
								}
								//Soft Selection 모드 선택시 새로 Vertex를 선택해주자
								if (IsSoftSelectionMode && _funcGizmoSoftSelection != null)
								{
									_funcGizmoSoftSelection();
								}
							}

							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}

							//if (_numSelected == 0)
							if (SelectResult.Main.NumSelected == 0)
							{
								_selectedAxis = SELECTED_AXIS.None;
								_isGizmoDrag = false;
							}
							else if (
								//_numSelected == 1 
								//&& (prevNumSelected == 0 || prevNumSelected == 1) 
								SelectResult.Main.NumSelected == 1
								&& (SelectResult.Prev.NumSelected == 0 || SelectResult.Prev.NumSelected == 1)
								&& GetSelectType() == SELECT_TYPE.New)
							{
								_isGizmoDrag = true;
								_selectedAxis = SELECTED_AXIS.All;
							}

						}
						else if (isAxisClick_Origin)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.All;
							//selectResult = SELECT_RESULT.SameSelected;
						}
						else if (isAxisClick_X)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.Axis_X;
							//selectResult = SELECT_RESULT.SameSelected;
						}
						else if (isAxisClick_Y)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.Axis_Y;
							//selectResult = SELECT_RESULT.SameSelected;
						}

						_mousePosGL_Down = mousePosGL;
						_mousePosW_Down = mousePosW;
						_mousePosW_Prev = mousePosW;

						//Debug.Log("Move -> Check [" + _numSelected + " <- " + prevNumSelected + "]");
						//CheckAndStartAreaSelect((_numSelected != prevNumSelected) && _numSelected != 0);
						//CheckAndStartAreaSelect(_numSelected, prevNumSelected);//<이전
						CheckAndStartAreaSelect(SelectResult.Main);


						if (_isGizmoDrag)
						{
							_isAreaSelecting = false;
						}

						_isMovePressed = false;


						//추가>>
						//Pressed가 아닌 Down에서 Undo 저장을 하기 위해 0의 위치 변화를 준다.
						if (_isFFDMode)
						{
							OnMoveTransformControlPoint(mousePosGL, mousePosW, Vector2.zero);
						}
						else
						{
							if (_funcGizmoMove != null)
							{
								_funcGizmoMove(mousePosGL, mousePosW, Vector2.zero, 0, !_isMovePressed);
							}
						}

						if (!_isMovePressed)
						{
							_isMovePressed = true;
						}


						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);
					}

					break;

				case apMouse.MouseBtnStatus.Pressed:
					{

						if (!_isGizmoDrag
							|| _selectedAxis == SELECTED_AXIS.None
							//|| _numSelected == 0
							|| SelectResult.Main.NumSelected == 0
							)
						{
							UpdateAreaInPressed(mousePosGL, mousePosW);
							break;
						}
						//마우스 이동(GL) -> 마우스 이동 (W)로 변환
						//단, 리턴하는 값은 조금 다른데,
						//축 제한이 있는 경우, 해당 축 값 외에는 0으로 바꾼다.
						Vector2 deltaMove = (mousePosW - _mousePosW_Prev);
						if (_coordinate == COORDINATE_TYPE.World)
						{
							if (_selectedAxis == SELECTED_AXIS.Axis_X)
							{
								deltaMove.y = 0;
							}
							else if (_selectedAxis == SELECTED_AXIS.Axis_Y)
							{
								deltaMove.x = 0;
							}
						}
						else
						{
							Vector2 deltaMoveLocal = _mtrx_localRotate.inverse.MultiplyPoint(deltaMove);
							if (_selectedAxis == SELECTED_AXIS.Axis_X)
							{
								deltaMoveLocal.y = 0;
							}
							else if (_selectedAxis == SELECTED_AXIS.Axis_Y)
							{
								deltaMoveLocal.x = 0;
							}
							//Vector3 deltaMove3 = _mtrx_localRotate.MultiplyPoint3x4(deltaMoveLocal);
							//deltaMove.x = deltaMove3.x;
							//deltaMove.y = deltaMove3.y;
							deltaMove = _mtrx_localRotate.MultiplyPoint(deltaMoveLocal);
						}

						if (Mathf.Abs(deltaMove.x) > 0.0f || Mathf.Abs(deltaMove.y) > 0.0f)
						{
							if (_isFFDMode)
							{
								OnMoveTransformControlPoint(mousePosGL, mousePosW, deltaMove);
							}
							else
							{
								if (_funcGizmoMove != null)
								{
									_funcGizmoMove(mousePosGL, mousePosW, deltaMove, 0, !_isMovePressed);
								}
							}
							_mousePosW_Prev = mousePosW;

							if (!_isMovePressed)
							{
								_isMovePressed = true;
							}
						}

						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);
					}
					break;

				case apMouse.MouseBtnStatus.Released:
				case apMouse.MouseBtnStatus.Up:
					{
						if (_isMouseEventProcessing)
						{
							//Unselect는 없지만
							//축 선택은 초기화하자
							//_numSelected = ReleaseAreaSelect(mousePosGL, mousePosW);
							SelectResult selectResult = ReleaseAreaSelect(mousePosGL, mousePosW);
							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}


							_selectedAxis = SELECTED_AXIS.None;
							_isGizmoDrag = false;

							//_lastSelectResult = multipleResult;

							//Main을 Prev에 적용
							SelectResult.Prev.SetResult(SelectResult.Main);
						}

						_isMovePressed = false;

					}
					break;
			}

			if (rightBtnStatus == apMouse.MouseBtnStatus.Down && !_isMouseEventProcessing)
			{
				if (_funcGizmoUnselect != null)
				{
					if (Editor.Controller.IsMouseInGUI(mousePosGL))
					{
						_funcGizmoUnselect();//Unselect 함수 호출
					}
				}
			}
		}

		private void Update_Rotate(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePosGL, Vector2 mousePosW)
		{
			switch (leftBtnStatus)
			{
				case apMouse.MouseBtnStatus.Down:
					{
						//TODO : 
						//클릭한 위치에 따라
						//All / Axis-X / Axis-Y / 새로운 Select
						//결정해야 한다.
						//"새로운 Select"는 Gizmo 외부를 클릭했을 뿐,
						//다른 오브젝트는 아니어도 된다.
						//그 경우는 Select 이후 All Move로 판별한다.
						_selectedAxis = SELECTED_AXIS.None;
						_isGizmoDrag = false;

						bool isAxisClick_Rotate = IsGizmoClickable_Rotate(mousePosW);

						//SELECT_RESULT selectResult = SELECT_RESULT.None;
						bool isNewSelectable = false;

						//int prevNumSelected = _numSelected;
						SelectResult selectResult = null;

						if (
							//_numSelected == 0 
							SelectResult.Main.NumSelected == 0
							|| !isAxisClick_Rotate
							)
						{
							isNewSelectable = true;
						}

						if (isNewSelectable)
						{
							if (_isFFDMode)
							{
								//추가 : Transform 모드
								//_numSelected = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
								selectResult = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
							}
							else
							{
								if (_funcGizmoSelect != null)
								{
									//_numSelected = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
									selectResult = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
								}
								//Soft Selection 모드 선택시 새로 Vertex를 선택해주자
								if (IsSoftSelectionMode && _funcGizmoSoftSelection != null)
								{
									_funcGizmoSoftSelection();
								}
							}

							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}

							//if(_numSelected == 0)
							if (SelectResult.Main.NumSelected == 0)
							{
								_selectedAxis = SELECTED_AXIS.None;
								_isGizmoDrag = false;
							}
						}
						else if (isAxisClick_Rotate)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.All;
							//selectResult = SELECT_RESULT.SameSelected;
						}

						_mousePosGL_Down = mousePosGL;
						_mousePosW_Down = mousePosW;
						_mousePosW_Prev = mousePosW;

						_rotateAngle_Down = GetMouseAngleFromOrigin(mousePosW);
						_rotateAngle_Prev = GetMouseAngleFromOrigin(mousePosW);

						//CheckAndStartAreaSelect((_numSelected != prevNumSelected) && _numSelected != 0);
						//CheckAndStartAreaSelect(_numSelected, prevNumSelected);
						CheckAndStartAreaSelect(SelectResult.Main);

						if (_isGizmoDrag)
						{
							_isAreaSelecting = false;
						}


						_isMovePressed = false;

						//추가>>
						//Pressed가 아닌 Down에서 Undo 저장을 하기 위해 0도짜리 회전을 준다. 
						if (_isFFDMode)
						{
							OnRotateTransformControlPoint(0.0f);
						}
						else
						{
							if (_funcGizmoRotate != null)
							{
								_funcGizmoRotate(0.0f, !_isMovePressed);
							}
						}
						_isMovePressed = true;


						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);
					}

					break;

				case apMouse.MouseBtnStatus.Pressed:
					{
						if (!_isGizmoDrag
							|| _selectedAxis == SELECTED_AXIS.None
							//|| _numSelected == 0
							|| SelectResult.Main.NumSelected == 0
							)
						{
							UpdateAreaInPressed(mousePosGL, mousePosW);
							break;
						}
						//마우스 이동(GL) -> 마우스 이동 (W)로 변환
						//단, 리턴하는 값은 조금 다른데,
						//축 제한이 있는 경우, 해당 축 값 외에는 0으로 바꾼다.
						float curAngle = GetMouseAngleFromOrigin(mousePosW);
						float deltaAngle = curAngle - _rotateAngle_Prev;

						while (deltaAngle < -180.0f) { deltaAngle += 360.0f; }
						while (deltaAngle > 180.0f) { deltaAngle -= 360.0f; }

						if (_isFFDMode)
						{
							OnRotateTransformControlPoint(deltaAngle);
						}
						else
						{
							if (_funcGizmoRotate != null)
							{
								_funcGizmoRotate(deltaAngle, !_isMovePressed);
							}
						}

						_mousePosW_Prev = mousePosW;
						_rotateAngle_Prev = curAngle;

						if (!_isMovePressed)
						{
							_isMovePressed = true;
						}
					}
					break;

				case apMouse.MouseBtnStatus.Released:
				case apMouse.MouseBtnStatus.Up:
					{
						if (_isMouseEventProcessing)
						{
							//Unselect는 없지만
							//축 선택은 초기화하자

							//_numSelected = ReleaseAreaSelect(mousePosGL, mousePosW);
							SelectResult selectResult = ReleaseAreaSelect(mousePosGL, mousePosW);
							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}

							_selectedAxis = SELECTED_AXIS.None;
							_isGizmoDrag = false;

							//_lastSelectResult = multipleResult;

							//Main을 Prev에 적용
							SelectResult.Prev.SetResult(SelectResult.Main);

							_isMovePressed = false;
						}
					}
					break;
			}

			if (rightBtnStatus == apMouse.MouseBtnStatus.Down && !_isMouseEventProcessing)
			{
				if (_funcGizmoUnselect != null)
				{
					if (Editor.Controller.IsMouseInGUI(mousePosGL))
					{
						_funcGizmoUnselect();//Unselect 함수 호출
					}
				}
			}
		}


		private float GetMouseAngleFromOrigin(Vector2 mousePosW)
		{
			Vector2 dir = mousePosW - _originPos;
			if (dir.sqrMagnitude == 0)
			{
				return 0;
			}

			return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		}

		private void Update_Scale(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePosGL, Vector2 mousePosW)
		{
			switch (leftBtnStatus)
			{
				case apMouse.MouseBtnStatus.Down:
					{
						//TODO : 
						//클릭한 위치에 따라
						//All / Axis-X / Axis-Y / 새로운 Select
						//결정해야 한다.
						//"새로운 Select"는 Gizmo 외부를 클릭했을 뿐,
						//다른 오브젝트는 아니어도 된다.
						//그 경우는 Select 이후 All Move로 판별한다.
						_selectedAxis = SELECTED_AXIS.None;
						_isGizmoDrag = false;

						bool isAxisClick_Origin = IsGizmoClickable_Scale(mousePosW, SELECTED_AXIS.All);
						bool isAxisClick_X = IsGizmoClickable_Scale(mousePosW, SELECTED_AXIS.Axis_X);
						bool isAxisClick_Y = IsGizmoClickable_Scale(mousePosW, SELECTED_AXIS.Axis_Y);

						//SELECT_RESULT selectResult = SELECT_RESULT.None;
						bool isNewSelectable = false;

						//int prevNumSelected = _numSelected;

						if (
							//_numSelected == 0 
							SelectResult.Main.NumSelected == 0
							|| (!isAxisClick_Origin && !isAxisClick_X && !isAxisClick_Y))
						{
							isNewSelectable = true;
						}

						SelectResult selectResult = null;
						if (isNewSelectable)
						{
							if (_isFFDMode)
							{
								//추가 : Transform 모드
								//_numSelected = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());
								selectResult = OnSelectTransformControlPoint(mousePosGL, mousePosW, GetSelectType());

							}
							else
							{
								if (_funcGizmoSelect != null)
								{
									//_numSelected = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
									selectResult = _funcGizmoSelect(mousePosGL, mousePosW, 0, GetSelectType());
								}
								//Soft Selection 모드 선택시 새로 Vertex를 선택해주자
								if (IsSoftSelectionMode && _funcGizmoSoftSelection != null)
								{
									_funcGizmoSoftSelection();
								}
							}

							if (selectResult == null)
							{
								SelectResult.Main.Init();
							}

							//if (_numSelected == 0)
							if (SelectResult.Main.NumSelected == 0)
							{
								_selectedAxis = SELECTED_AXIS.None;
								_isGizmoDrag = false;
							}
						}
						else if (isAxisClick_Origin)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.All;
							//selectResult = SELECT_RESULT.SameSelected;
						}
						else if (isAxisClick_X)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.Axis_X;
							//selectResult = SELECT_RESULT.SameSelected;
						}
						else if (isAxisClick_Y)
						{
							_isGizmoDrag = true;
							_selectedAxis = SELECTED_AXIS.Axis_Y;
							//selectResult = SELECT_RESULT.SameSelected;
						}

						_mousePosGL_Down = mousePosGL;
						_mousePosW_Down = mousePosW;
						_mousePosW_Prev = mousePosW;

						//CheckAndStartAreaSelect((_numSelected != prevNumSelected) && _numSelected != 0);
						//CheckAndStartAreaSelect(_numSelected, prevNumSelected);
						CheckAndStartAreaSelect(SelectResult.Main);

						if (_isGizmoDrag)
						{
							_isAreaSelecting = false;
						}

						_isMovePressed = false;

						//추가>>
						//Pressed가 아닌 Down에서 Undo 저장을 하기 위해 0의 크기 변화를 준다.
						if (_isFFDMode)
						{
							OnScaleTransformControlPoint(Vector2.zero);
						}
						else
						{
							if (_funcGizmoScale != null)
							{
								_funcGizmoScale(Vector2.zero, !_isMovePressed);
							}
						}


						if (!_isMovePressed)
						{
							_isMovePressed = true;
						}


						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);
					}

					break;

				case apMouse.MouseBtnStatus.Pressed:
					{
						if (!_isGizmoDrag
							|| _selectedAxis == SELECTED_AXIS.None
							//|| _numSelected == 0
							|| SelectResult.Main.NumSelected == 0
							)
						{
							UpdateAreaInPressed(mousePosGL, mousePosW);
							break;
						}
						//마우스 이동(GL) -> 마우스 이동 (W)로 변환
						//단, 리턴하는 값은 조금 다른데,
						//축 제한이 있는 경우, 해당 축 값 외에는 0으로 바꾼다.
						Vector2 deltaMove = (mousePosW - _mousePosW_Prev);
						//if (_coordinate == COORDINATE_TYPE.Local)
						{
							Vector2 deltaMoveLocal = _mtrx_localRotate.inverse.MultiplyPoint(deltaMove);
							if (_selectedAxis == SELECTED_AXIS.Axis_X)
							{
								deltaMoveLocal.y = 0;
							}
							else if (_selectedAxis == SELECTED_AXIS.Axis_Y)
							{
								deltaMoveLocal.x = 0;
							}
							//Vector3 deltaMove3 = _mtrx_localRotate.MultiplyPoint3x4(deltaMoveLocal);
							//deltaMove.x = deltaMove3.x;
							//deltaMove.y = deltaMove3.y;

							deltaMove.x = deltaMoveLocal.x;
							deltaMove.y = deltaMoveLocal.y;
						}

						if (_selectedAxis == SELECTED_AXIS.Axis_X)
						{
							deltaMove.y = 0;
						}
						else if (_selectedAxis == SELECTED_AXIS.Axis_Y)
						{
							deltaMove.x = 0;
						}
						else
						{
							//비율을 맞추어야 한다.
							//Max 값에 맞추자
							float ratioValue = 0.0f;
							if (deltaMove.x > 0.0f)
							{
								ratioValue = deltaMove.x + Mathf.Max(deltaMove.y, 0);
							}
							else
							{
								ratioValue = -Mathf.Abs(deltaMove.x) - Mathf.Max(-deltaMove.y, 0);
							}

							deltaMove.x = ratioValue;
							deltaMove.y = ratioValue;
						}

						Vector2 scaleValue = deltaMove / 100.0f;

						if (_isFFDMode)
						{
							OnScaleTransformControlPoint(scaleValue);
						}
						else
						{
							if (_funcGizmoScale != null)
							{
								_funcGizmoScale(scaleValue, !_isMovePressed);
							}
						}

						_mousePosW_Prev = mousePosW;

						if (!_isMovePressed)
						{
							_isMovePressed = true;
						}
					}
					break;

				case apMouse.MouseBtnStatus.Released:
				case apMouse.MouseBtnStatus.Up:
					{
						//Unselect는 없지만
						//축 선택은 초기화하자
						//_numSelected = ReleaseAreaSelect(mousePosGL, mousePosW);
						SelectResult selectResult = ReleaseAreaSelect(mousePosGL, mousePosW);
						if (selectResult == null)
						{
							SelectResult.Main.Init();
						}

						_selectedAxis = SELECTED_AXIS.None;
						_isGizmoDrag = false;

						//_lastSelectResult = multipleResult;

						//Main을 Prev에 적용
						SelectResult.Prev.SetResult(SelectResult.Main);

						_isMovePressed = false;
					}
					break;
			}


			if (rightBtnStatus == apMouse.MouseBtnStatus.Down && !_isMouseEventProcessing)
			{
				if (_funcGizmoUnselect != null)
				{
					if (Editor.Controller.IsMouseInGUI(mousePosGL))
					{
						_funcGizmoUnselect();//Unselect 함수 호출
					}
				}
			}
		}


		public void EndUpdate()
		{
			if (!_isUpdatedPerFrame)
			{
				//Release();
				//Unlink();
			}
		}


		private bool IsGizmoClickable_Move(Vector2 mousePosW, SELECTED_AXIS axis)
		{
			if(!_isGizmoRenderable)//추가 20.9.17 : 버그 해결. 안보이는 기즈모때문에 선택이 되지 않던 문제
			{
				//기즈모가 없다면 클릭할 수도 없다.
				//Debug.LogError("고스트 기즈모 클릭 - Move");
				return false;
			}
			Vector2 mousePosToGizmo = Vector2.zero;
			Vector2 imageSize = GetImageSize(IMAGE_TYPE.Origin_None);
			Vector2 checkSize = new Vector2(0.5f, 0.5f);
			switch (axis)
			{
				case SELECTED_AXIS.All:
					{
						mousePosToGizmo = (_mtrx_origin.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Origin_None);
					}
					break;

				case SELECTED_AXIS.Axis_X:
					{
						mousePosToGizmo = (_mtrx_move_axisX.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Transform_Move);
						checkSize.x = 0.2f;
					}
					break;

				case SELECTED_AXIS.Axis_Y:
					{
						mousePosToGizmo = (_mtrx_move_axisY.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Transform_Move);
						checkSize.x = 0.2f;
					}
					break;

				case SELECTED_AXIS.None:
					return false;
			}

			imageSize /= apGL.Zoom;

			if (mousePosToGizmo.x > -imageSize.x * checkSize.x && mousePosToGizmo.x < imageSize.x * checkSize.x
				&& mousePosToGizmo.y > -imageSize.y * checkSize.y && mousePosToGizmo.y < imageSize.y * checkSize.y)
			{
				return true;
			}
			return false;
		}

		private bool IsGizmoClickable_Rotate(Vector2 mousePosW)
		{
			if (!_isGizmoRenderable)//추가 20.9.17 : 버그 해결. 안보이는 기즈모때문에 선택이 되지 않던 문제
			{
				//기즈모가 없다면 클릭할 수도 없다.
				//Debug.LogError("고스트 기즈모 클릭 - Rotate");
				return false;
			}
			Vector2 mousePosToGizmo = (_mtrx_origin.inverse).MultiplyPoint(mousePosW);
			Vector2 imageSize = GetImageSize(IMAGE_TYPE.Transform_Rotate);

			imageSize /= apGL.Zoom;

			float distFromCenter = Mathf.Sqrt(mousePosToGizmo.x * mousePosToGizmo.x + mousePosToGizmo.y * mousePosToGizmo.y);
			if (distFromCenter < imageSize.x * 0.5f)
			{
				
				return true;
			}
			return false;
		}

		private bool IsGizmoClickable_Scale(Vector2 mousePosW, SELECTED_AXIS axis)
		{
			if(!_isGizmoRenderable)//추가 20.9.17 : 버그 해결. 안보이는 기즈모때문에 선택이 되지 않던 문제
			{
				//기즈모가 없다면 클릭할 수도 없다.
				//Debug.LogError("고스트 기즈모 클릭 - Scale");
				return false;
			}
			Vector2 mousePosToGizmo = Vector2.zero;
			Vector2 imageSize = GetImageSize(IMAGE_TYPE.Origin_None);
			Vector2 checkSize = new Vector2(0.5f, 0.5f);
			switch (axis)
			{
				case SELECTED_AXIS.All:
					{
						mousePosToGizmo = (_mtrx_origin.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Origin_None);
					}
					break;

				case SELECTED_AXIS.Axis_X:
					{
						mousePosToGizmo = (_mtrx_scale_axisX.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Transform_Scale);
						checkSize.x = 0.2f;
					}
					break;

				case SELECTED_AXIS.Axis_Y:
					{
						mousePosToGizmo = (_mtrx_scale_axisY.inverse).MultiplyPoint(mousePosW);
						imageSize = GetImageSize(IMAGE_TYPE.Transform_Scale);
						checkSize.x = 0.2f;
					}
					break;

				case SELECTED_AXIS.None:
					return false;
			}

			imageSize /= apGL.Zoom;

			if (mousePosToGizmo.x > -imageSize.x * checkSize.x && mousePosToGizmo.x < imageSize.x * checkSize.x
				&& mousePosToGizmo.y > -imageSize.y * checkSize.y && mousePosToGizmo.y < imageSize.y * checkSize.y)
			{
				return true;
			}
			return false;
		}
		//public void Release()
		//{
		//	_selectedObject = null;
		//	_selectedAxis = SELECTED_AXIS.None;
		//}

		// 다중 선택에 대한 처리
		//--------------------------------------------------------------------------
		//private bool CheckAndStartAreaSelect(SELECT_RESULT singleSelectResult)
		//private bool CheckAndStartAreaSelect(bool isSingleClickUsed)
		//private bool CheckAndStartAreaSelect(int curNumSelected, int prevNumSelected)
		private bool CheckAndStartAreaSelect(SelectResult curSelectResult)
		{
			bool isAreaStartable = false;
			//if(curNumSelected == 0)
			if (curSelectResult == null || curSelectResult.NumSelected == 0)
			{
				isAreaStartable = true;
			}
			//else if(curNumSelected == prevNumSelected)
			else if (SelectResult.IsSameResult(curSelectResult, SelectResult.Prev))
			{
				if (_isShiftKey || _isCtrlKey || _isAltKey)
				{
					isAreaStartable = true;
				}
			}
			//if (_isAreaSelectable && singleSelectResult == SELECT_RESULT.None)
			//if (_isAreaSelectable && !isSingleClickUsed)
			if (_isAreaSelectable && isAreaStartable)
			{
				//클릭으로 [단일 선택시] 아무것도 선택되지 않았고,
				//영역 선택이 가능하다면
				_isAreaSelecting = true;

				if (_isShiftKey || _isCtrlKey)
				{ _areaSelectType = SELECT_TYPE.Add; }
				else if (_isAltKey)
				{ _areaSelectType = SELECT_TYPE.Subtract; }
				else
				{ _areaSelectType = SELECT_TYPE.New; }


				_areaPosStart_GL = _mousePosGL_Down;
				_areaPosEnd_GL = _mousePosGL_Down;
				_areaPosStart_W = _mousePosW_Down;
				_areaPosEnd_W = _mousePosW_Down;

				//Debug.Log("Start Area Select");
				return true;
			}

			_isAreaSelecting = false;
			return false;
		}

		private bool UpdateAreaInPressed(Vector2 mousePosGL, Vector2 mousePosW)
		{
			if (!_isAreaSelecting)
			{
				return false;
			}

			_areaPosEnd_GL = mousePosGL;
			_areaPosEnd_W = mousePosW;
			return true;
		}

		private SelectResult ReleaseAreaSelect(Vector2 mousePosGL, Vector2 mousePosW)
		{
			if (!_isAreaSelecting)
			{
				_isAreaSelecting = false;
				//그대로 리턴
				return SelectResult.Main;
				//return _numSelected;
			}

			//int result = _numSelected;
			int result = SelectResult.Main.NumSelected;
			SelectResult selectResult = null;

			if (_isAreaSelecting)
			{

				_areaPosEnd_GL = mousePosGL;
				_areaPosEnd_W = mousePosW;


				//다중 선택 이벤트를 호출하자
				if (_isFFDMode)
				{
					//result = OnMultipleSelectTransformControlPoint(
					//		new Vector2(Mathf.Min(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Min(_areaPosStart_W.y, _areaPosEnd_W.y)),
					//		new Vector2(Mathf.Max(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Max(_areaPosStart_W.y, _areaPosEnd_W.y)),
					//		_areaSelectType
					//		);

					selectResult = OnMultipleSelectTransformControlPoint(
								new Vector2(Mathf.Min(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Min(_areaPosStart_W.y, _areaPosEnd_W.y)),
								new Vector2(Mathf.Max(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Max(_areaPosStart_W.y, _areaPosEnd_W.y)),
								_areaSelectType
								);
				}
				else
				{
					if (_funcGizmoMultipleSelect != null)
					{
						//result = _funcGizmoMultipleSelect(
						//	new Vector2(Mathf.Min(_areaPosStart_GL.x, _areaPosEnd_GL.x), Mathf.Min(_areaPosStart_GL.y, _areaPosEnd_GL.y)),
						//	new Vector2(Mathf.Max(_areaPosStart_GL.x, _areaPosEnd_GL.x), Mathf.Max(_areaPosStart_GL.y, _areaPosEnd_GL.y)),
						//	new Vector2(Mathf.Min(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Min(_areaPosStart_W.y, _areaPosEnd_W.y)),
						//	new Vector2(Mathf.Max(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Max(_areaPosStart_W.y, _areaPosEnd_W.y)),
						//	_areaSelectType
						//	);

						selectResult = _funcGizmoMultipleSelect(
							new Vector2(Mathf.Min(_areaPosStart_GL.x, _areaPosEnd_GL.x), Mathf.Min(_areaPosStart_GL.y, _areaPosEnd_GL.y)),
							new Vector2(Mathf.Max(_areaPosStart_GL.x, _areaPosEnd_GL.x), Mathf.Max(_areaPosStart_GL.y, _areaPosEnd_GL.y)),
							new Vector2(Mathf.Min(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Min(_areaPosStart_W.y, _areaPosEnd_W.y)),
							new Vector2(Mathf.Max(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Max(_areaPosStart_W.y, _areaPosEnd_W.y)),
							_areaSelectType
							);

						//Soft Selection 모드 선택시 새로 Vertex를 선택해주자
						if (IsSoftSelectionMode && _funcGizmoSoftSelection != null)
						{
							_funcGizmoSoftSelection();
						}
					}
				}

				//if(selectResult == null)
				//{
				//	SelectResult.Main.Init();
				//	selectResult = SelectResult.Main;
				//}

			}

			_isAreaSelecting = false;

			//Debug.Log("Multiple Release Result : " + result);

			//return result;
			return selectResult;
		}

		// Trasform 모드 [FFD]
		//--------------------------------------------------------------------------
		public bool StartTransformMode(apEditor editor, int numX = 3, int numY = 3)
		{
			if (!_isFFDModeAvailable)
			{
				return false;
			}

			if (_funcGizmoFFDStart == null || _funcGizmoFFD == null)
			{
				return false;
			}

			//Record를 해야한다. 버그가 있다고 합니다.
			apMesh targetMesh = null;
			apMeshGroup targetMeshGroup = null;
			apModifierBase targetModifier = null;
			if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.Mesh)
			{
				targetMesh = editor.Select.Mesh;

				if(targetMesh != null)
				{
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_VertexMoved, Editor, targetMesh, targetMesh, false);
				}
				
			}
			else if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				//MeshGroup인 경우
				targetMeshGroup = editor.Select.MeshGroup;
				if (targetMeshGroup != null)
				{
					targetModifier = editor.Select.Modifier;
				}

				if(targetMeshGroup != null && targetModifier != null)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_FFDStart, 
																editor, targetModifier, targetMeshGroup, false);
				}
			}
			else if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				apAnimClip animClip = editor.Select.AnimClip;
				if (animClip != null)
				{
					targetMeshGroup = animClip._targetMeshGroup;
					if (targetMeshGroup != null)
					{
						apAnimTimeline timeline = editor.Select.AnimTimeline;
						if (timeline != null && timeline._linkedModifier != null)
						{
							targetModifier = timeline._linkedModifier;
						}
					}

					if (targetMeshGroup != null && targetModifier != null)
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_FFDStart, 
																editor, targetModifier, targetMeshGroup, false);
					}
				}
			}

			//if (targetMeshGroup == null)
			//{
			//	//MeshGroup이 없다면 => 엥 작업이 가능해요?
			//	//Debug.LogError("No MeshGroup FFD Start");
			//}
			//else
			//{
			//	if (targetModifier != null)
			//	{
			//		apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_FFDStart, editor, targetMeshGroup, targetModifier, targetMeshGroup, false);
			//	}
			//	else
			//	{
			//		//사실 모디파이어가 없다면 그것도 말이 안되는데;;
			//		//Debug.LogError("No Modifier FFD Start");
			//	}
			//}


			_nFFDPointX = numX;
			_nFFDPointY = numY;
			if (_nFFDPointX < 2) { _nFFDPointX = 2; }
			if (_nFFDPointY < 2) { _nFFDPointY = 2; }

			_isFFDMode = false;
			_transformControlPoints.Clear();
			_transformedObjects.Clear();

			bool isStartResult = _funcGizmoFFDStart();
			if (isStartResult)
			{
				if (_transformedObjects.Count <= 1)
				{
					isStartResult = false;
				}
			}

			if (isStartResult)
			{
				Vector2 posMin = Vector2.zero;
				Vector2 posMax = Vector2.zero;

				for (int i = 0; i < _transformedObjects.Count; i++)
				{
					TransformedObject obj = _transformedObjects[i];
					if (i == 0)
					{
						posMin = obj._prevWorldPos;
						posMax = obj._prevWorldPos;
					}
					else
					{
						if (obj._prevWorldPos.x < posMin.x) { posMin.x = obj._prevWorldPos.x; }
						if (obj._prevWorldPos.y < posMin.y) { posMin.y = obj._prevWorldPos.y; }

						if (obj._prevWorldPos.x > posMax.x) { posMax.x = obj._prevWorldPos.x; }
						if (obj._prevWorldPos.y > posMax.y) { posMax.y = obj._prevWorldPos.y; }
					}
				}

				Vector2 lengthArea = new Vector2(posMax.x - posMin.x, posMax.y - posMin.y);

				if (lengthArea.x < 0.0001f)
				{ lengthArea.x = 0.0001f; }
				if (lengthArea.y < 0.0001f)
				{ lengthArea.y = 0.0001f; }

				//Min, Max에 대해서 컨트롤 포인트를 추가하자
				//총 9개
				//6, 7, 8
				//3, 4, 5
				//0, 1, 2

				//>> 수정
				//ControlPoint 개수를 커스텀하게 만들자

				//Debug.Log("Start Transform Mode [" + _nFFDPointX + " / " + _nFFDPointY + "]  - Min:" + posMin + " ~ Max:" + posMax);

				int iPoint = 0;
				for (int iY = 0; iY < _nFFDPointY; iY++)
				{
					float fY = (float)iY / (float)(_nFFDPointY - 1);

					float curV = (posMin.y * (1 - fY)) + (posMax.y * fY);

					for (int iX = 0; iX < _nFFDPointX; iX++)
					{
						float fX = (float)iX / (float)(_nFFDPointX - 1);
						float curU = (posMin.x * (1 - fX)) + (posMax.x * fX);

						_transformControlPoints.Add(new TransformControlPoint(new Vector2(fX, fY), new Vector2(curU, curV)));
						//Debug.Log("[" + iPoint + ":" + iX + ", " + iY + "] (" + fX + ", " + fY + ") (" + curU + ", " + curV + ")");
						iPoint++;
					}
				}


				for (int i = 0; i < _transformedObjects.Count; i++)
				{
					TransformedObject obj = _transformedObjects[i];

					obj._normalizePos = new Vector2(
						Mathf.Clamp01((obj._prevWorldPos.x - posMin.x) / lengthArea.x),
						Mathf.Clamp01((obj._prevWorldPos.y - posMin.y) / lengthArea.y)
						);
				}

				_isFFDMode = true;

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// StartTransformMode() 호출후 등록된 함수에 의해 호출되는 함수. 
		/// Transform으로 조작할 오브젝트 (주로 Vertex)와 그것들의 World Pos를 등록한다.
		/// </summary>
		/// <param name="srcObject"></param>
		/// <param name="worldPos"></param>
		public void RegistTransformedObjectList(List<object> srcObject, List<Vector2> worldPos, List<Vector2> orgData)
		{
			_transformControlPoints.Clear();
			_transformedObjects.Clear();

			if (srcObject.Count != worldPos.Count
				|| srcObject.Count != orgData.Count)
			{
				//크기가 안맞는데요..
				return;
			}
			for (int i = 0; i < srcObject.Count; i++)
			{
				_transformedObjects.Add(new TransformedObject(srcObject[i], worldPos[i], orgData[i]));
			}
		}

		public void RevertFFDTransformForce()
		{
			//이전
			//RefreshTransformObjects();

			//변경 20.4.11
			//RefreshFFDTransformForce 함수는 주로 객체가 바뀔때 호출된다.
			//Revert로 변경한다.
			if(IsFFDMode)
			{
				RevertTransformObjects(Editor);
			}
			
		}

		private void RefreshTransformObjects()
		{
			if (!_isFFDMode)
			{
				//Debug.LogError("Refreh FFD > Not FFD Mode");
				return;
			}
			//1. 컨트롤 포인트에 맞추어 ITP 계산을 한 뒤 Next Pos를 갱신하자

			if (Event.current.type == EventType.Used)
			{
				//Debug.LogError("Refreh FFD > Used");
				return;
			}
			
			//if(Event.current.type != EventType.Repaint)
			//{
			//	return;
			//}
			//6, 7, 8
			//3, 4, 5
			//0, 1, 2

			TransformControlPoint conPoint_LT = null;
			TransformControlPoint conPoint_RT = null;
			TransformControlPoint conPoint_LB = null;
			TransformControlPoint conPoint_RB = null;

			//기존 > 3x3 (4분면)에 포함되는지 확인
			//변경 > NxM ((n-1) x (m-1) 분면)에 포함되는지 확인
			int nSpaceX = _nFFDPointX - 1;
			int nSpaceY = _nFFDPointY - 1;



			for (int iObj = 0; iObj < _transformedObjects.Count; iObj++)
			{
				TransformedObject transformedObj = _transformedObjects[iObj];
				Vector2 norPos = transformedObj._normalizePos;
				Vector2 itp = new Vector2(norPos.x, norPos.y);

				//어느 평면에 위치하는지 인덱스를 검색하자
				int curXIndex = -1;
				int curYIndex = -1;

				for (int iX = 0; iX < nSpaceX; iX++)
				{
					float curPosX_Max = (float)(iX + 1) / (float)nSpaceX;
					if (norPos.x <= curPosX_Max)
					{
						curXIndex = iX;
						break;
					}
				}
				if (curXIndex < 0)
				{
					curXIndex = nSpaceX - 1;
				}

				for (int iY = 0; iY < nSpaceY; iY++)
				{
					float curPosY_Max = (float)(iY + 1) / (float)nSpaceY;
					if (norPos.y <= curPosY_Max)
					{
						curYIndex = iY;
						break;
					}
				}
				if (curYIndex < 0)
				{
					curYIndex = nSpaceY - 1;
				}

				itp.x -= (float)curXIndex / (float)nSpaceX;
				itp.y -= (float)curYIndex / (float)nSpaceY;

				itp.x = Mathf.Clamp01(itp.x * nSpaceX);
				itp.y = Mathf.Clamp01(itp.y * nSpaceY);



				//이제 어떤 ControlPoint인지 연결해주자
				//LT (0, +1) ----- RT (+1, +1)
				//LB (0, 0)  ----- RB (+1, 0)

				int iLB = (curXIndex + 0) + (curYIndex + 0) * _nFFDPointX;
				int iRB = (curXIndex + 1) + (curYIndex + 0) * _nFFDPointX;
				int iLT = (curXIndex + 0) + (curYIndex + 1) * _nFFDPointX;
				int iRT = (curXIndex + 1) + (curYIndex + 1) * _nFFDPointX;

				conPoint_LB = _transformControlPoints[iLB];
				conPoint_RB = _transformControlPoints[iRB];
				conPoint_LT = _transformControlPoints[iLT];
				conPoint_RT = _transformControlPoints[iRT];



				Vector2 pos_T = (conPoint_LT._worldPos * (1.0f - itp.x)) + (conPoint_RT._worldPos * itp.x);
				Vector2 pos_B = (conPoint_LB._worldPos * (1.0f - itp.x)) + (conPoint_RB._worldPos * itp.x);

				Vector2 nextPos = (pos_B * (1.0f - itp.y)) + (pos_T * itp.y);

				transformedObj._nextWorldPos = nextPos;
			}


			//2. Next Pos 갱신후 저장된 함수 호출
			//List<object> resultObject = new List<object>();
			//List<Vector2> resultPos = new List<Vector2>();

			if(_resultFFD_Objects == null) { _resultFFD_Objects = new List<object>(); }
			if(_resultFFD_Pos == null) { _resultFFD_Pos = new List<Vector2>(); }
			_resultFFD_Objects.Clear();
			_resultFFD_Pos.Clear();
			

			for (int i = 0; i < _transformedObjects.Count; i++)
			{
				_resultFFD_Objects.Add(_transformedObjects[i]._srcObject);
				_resultFFD_Pos.Add(_transformedObjects[i]._nextWorldPos);//Adapt : Next Pos 를 넣자
			}

			if (_funcGizmoFFD != null)
			{
				//bool isResult = _funcGizmoFFD(resultObject, resultPos, false);
				bool isResult = _funcGizmoFFD(_resultFFD_Objects, _resultFFD_Pos, FFD_ASSIGN_TYPE.WorldPos, false, false);
				//추가) 만약 처리 실패시 해당 데이터가 없는 걸로 처리하여 자동 Revert한다.
				if (!isResult)
				{
					//Debug.LogError("Revert On FFD");
					RevertTransformObjects(null);
				}
				else
				{
					//Debug.LogWarning("Refreshed On FFD");
				}
			}
		}

		public void AdaptTransformObjects(apEditor editor)
		{
			//Editor가 있다면 일단 Record를 한다.
			bool isRecordedComplete = false;
			if (editor != null)
			{
				//Record를 해야한다. 버그가 있다고 합니다.
				apMeshGroup targetMeshGroup = null;
				apModifierBase targetModifier = null;
				if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
				{
					//MeshGroup인 경우
					targetMeshGroup = editor.Select.MeshGroup;
					if (targetMeshGroup != null) { targetModifier = editor.Select.Modifier; }
				}
				else if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					apAnimClip animClip = editor.Select.AnimClip;
					if (animClip != null)
					{
						targetMeshGroup = animClip._targetMeshGroup;
						if (targetMeshGroup != null)
						{
							apAnimTimeline timeline = editor.Select.AnimTimeline;
							if (timeline != null && timeline._linkedModifier != null)
							{
								targetModifier = timeline._linkedModifier;
							}
						}
					}
				}

				if (targetMeshGroup == null)
				{
					//MeshGroup이 없다면 => 엥 작업이 가능해요?
					//Debug.LogError("No MeshGroup FFD Adapt");
				}
				else
				{
					if (targetModifier != null)
					{
						apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_FFDAdapt, editor, targetMeshGroup, targetModifier, targetMeshGroup, false);
						isRecordedComplete = true;
					}
					else
					{
						//사실 모디파이어가 없다면 그것도 말이 안되는데;;
						//Debug.LogError("No Modifier FFD Adapt");
					}
				}
			}

			//List<object> resultObject = new List<object>();
			//List<Vector2> resultPos = new List<Vector2>();

			if(_resultFFD_Objects == null) { _resultFFD_Objects = new List<object>(); }
			if(_resultFFD_Pos == null) { _resultFFD_Pos = new List<Vector2>(); }
			_resultFFD_Objects.Clear();
			_resultFFD_Pos.Clear();

			bool isNeedToRecord = isRecordedComplete ? false : true;//이미 위에서 Undo에 저장 했으면 Record를 할 필요가 없다.

			for (int i = 0; i < _transformedObjects.Count; i++)
			{
				_resultFFD_Objects.Add(_transformedObjects[i]._srcObject);
				_resultFFD_Pos.Add(_transformedObjects[i]._nextWorldPos);//Adapt : Next Pos 를 넣자
			}

			if (_funcGizmoFFD != null)
			{
				_funcGizmoFFD(_resultFFD_Objects, _resultFFD_Pos, FFD_ASSIGN_TYPE.WorldPos, true, isNeedToRecord);//Adapt는 WorldPos를 넣는다.
			}

			_transformControlPoints.Clear();
			_transformedObjects.Clear();
			_isFFDMode = false;
		}

		public void RevertTransformObjects(apEditor editor)
		{
			//Debug.Log("Revert Transform Objects");
			//Editor가 있다면 일단 Record를 한다.
			bool isRecordedComplete = false;
			if (editor != null)
			{
				//Record를 해야한다. 버그가 있다고 합니다.
				apMeshGroup targetMeshGroup = null;
				apModifierBase targetModifier = null;
				if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
				{
					//MeshGroup인 경우
					targetMeshGroup = editor.Select.MeshGroup;
					if (targetMeshGroup != null) { targetModifier = editor.Select.Modifier; }
				}
				else if (editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					apAnimClip animClip = editor.Select.AnimClip;
					if (animClip != null)
					{
						targetMeshGroup = animClip._targetMeshGroup;
						if (targetMeshGroup != null)
						{
							apAnimTimeline timeline = editor.Select.AnimTimeline;
							if (timeline != null && timeline._linkedModifier != null)
							{
								targetModifier = timeline._linkedModifier;
							}
						}
					}
				}

				if (targetMeshGroup == null)
				{
					//MeshGroup이 없다면 => 엥 작업이 가능해요?
					//Debug.LogError("No MeshGroup FFD Revert");
				}
				else
				{
					if (targetModifier != null)
					{
						apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_FFDRevert, editor, targetMeshGroup, targetModifier, targetMeshGroup, false);
						isRecordedComplete = true;
					}
					else
					{
						//사실 모디파이어가 없다면 그것도 말이 안되는데;;
						//Debug.LogError("No Modifier FFD Revert");
					}
				}
			}



			//List<object> resultObject = new List<object>();
			//List<Vector2> resultPos = new List<Vector2>();

			if(_resultFFD_Objects == null) { _resultFFD_Objects = new List<object>(); }
			if(_resultFFD_Pos == null) { _resultFFD_Pos = new List<Vector2>(); }
			_resultFFD_Objects.Clear();
			_resultFFD_Pos.Clear();

			bool isNeedToRecord = isRecordedComplete ? false : true;//이미 위에서 Undo에 저장 했으면 Record를 할 필요가 없다.

			
			for (int i = 0; i < _transformedObjects.Count; i++)
			{
				_resultFFD_Objects.Add(_transformedObjects[i]._srcObject);
				//_resultFFD_Pos.Add(_transformedObjects[i]._prevWorldPos);//Revert : Prev Pos를 넣자
				_resultFFD_Pos.Add(_transformedObjects[i]._prevOrgData);//변경 : Revert : 원래의 데이터를 넣자
			}

			if (_funcGizmoFFD != null)
			{
				_funcGizmoFFD(_resultFFD_Objects, _resultFFD_Pos, FFD_ASSIGN_TYPE.LocalData, true, isNeedToRecord);
			}

			_transformControlPoints.Clear();
			_transformedObjects.Clear();
			_isFFDMode = false;
		}


		private TransformParam GetTransformControlPointParam()
		{
			int nControlPoint = 0;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					centerPos += _transformControlPoints[i]._worldPos;
					nControlPoint++;
				}
			}

			if (nControlPoint == 0)
			{
				return null;
			}
			centerPos.x /= (float)nControlPoint;
			centerPos.y /= (float)nControlPoint;

			return TransformParam.Make(centerPos, 0.0f, Vector2.one,
										0, Color.gray, true,
										apMatrix3x3.TRS(centerPos, 0, Vector3.one),
										(nControlPoint > 1),
										TRANSFORM_UI.Position2D | TRANSFORM_UI.Vertex_Transform,
										centerPos, 0.0f, Vector2.one
										);
		}

		private Vector2 GetTransformControlCenterPos()
		{
			int nControlPoint = 0;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					centerPos += _transformControlPoints[i]._worldPos;
					nControlPoint++;
				}
			}

			if (nControlPoint == 0)
			{
				return Vector2.zero;
			}

			centerPos.x /= (float)nControlPoint;
			centerPos.y /= (float)nControlPoint;

			return centerPos;
		}

		private int GetNumSelectedTransformControlPoint()
		{
			int nControlPoint = 0;
			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					nControlPoint++;
				}
			}

			return nControlPoint;
		}
		private List<TransformControlPoint> GetSelectedTransformControlPoints()
		{
			List<TransformControlPoint> result = new List<TransformControlPoint>();
			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					result.Add(_transformControlPoints[i]);
				}
			}
			return result;
		}

		private SelectResult OnSelectTransformControlPoint(Vector2 mousePosGL, Vector2 mousePosW, SELECT_TYPE selectType)
		{
			Vector2 pointSize = GetImageSize(IMAGE_TYPE.TransformController);
			Vector2 pointSize_Half = pointSize * 0.5f / apGL.Zoom;

			if (selectType == SELECT_TYPE.New)
			{
				//일단 초기화
				for (int i = 0; i < _transformControlPoints.Count; i++)
				{
					_transformControlPoints[i]._isSelected = false;
				}
			}

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				//클릭한 영역에 있는 걸 찾자
				if (mousePosW.x > _transformControlPoints[i]._worldPos.x - pointSize_Half.x &&
					mousePosW.x < _transformControlPoints[i]._worldPos.x + pointSize_Half.x &&
					mousePosW.y > _transformControlPoints[i]._worldPos.y - pointSize_Half.y &&
					mousePosW.y < _transformControlPoints[i]._worldPos.y + pointSize_Half.y)
				{
					if (selectType == SELECT_TYPE.New || selectType == SELECT_TYPE.Add)
					{
						_transformControlPoints[i]._isSelected = true;
					}
					else
					{
						_transformControlPoints[i]._isSelected = false;
					}
					break;
				}
			}

			//return GetNumSelectedTransformControlPoint();
			return SelectResult.Main.SetMultiple<TransformControlPoint>(GetSelectedTransformControlPoints());
		}

		private void OnMoveTransformControlPoint(Vector2 mousePosGL, Vector2 mousePosW, Vector2 deltaMoveW)
		{
			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					_transformControlPoints[i]._worldPos += deltaMoveW;
				}
			}

			RefreshTransformObjects();
		}

		private void OnRotateTransformControlPoint(float deltaAngleW)
		{
			if (GetNumSelectedTransformControlPoint() == 0)
			{
				return;
			}

			Vector2 centerPos = GetTransformControlCenterPos();

			//if(deltaAngleW > 180.0f)
			//{
			//	deltaAngleW -= 360.0f;
			//}
			//else if(deltaAngleW < -180.0f)
			//{
			//	deltaAngleW += 360.0f;
			//}

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					//Vector2 worldPos3 = new Vector3(_transformControlPoints[i]._worldPos.x, _transformControlPoints[i]._worldPos.y, 0);

					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(_transformControlPoints[i]._worldPos);

					_transformControlPoints[i]._worldPos = nextWorldPos2;
				}
			}

			RefreshTransformObjects();
		}

		private void OnScaleTransformControlPoint(Vector2 deltaScale)
		{
			if (GetNumSelectedTransformControlPoint() == 0)
			{
				return;
			}

			Vector2 centerPos = GetTransformControlCenterPos();

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, new Vector2(1.0f + deltaScale.x, 1.0f + deltaScale.y))
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					//Vector3 worldPos3 = new Vector3(_transformControlPoints[i]._worldPos.x, _transformControlPoints[i]._worldPos.y, 0);
					//Vector3 nextWorldPos3 = matrix_Scale.MultiplyPoint3x4(worldPos3);
					Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(_transformControlPoints[i]._worldPos);

					_transformControlPoints[i]._worldPos = nextWorldPos;
				}
			}

			RefreshTransformObjects();
		}

		private SelectResult OnMultipleSelectTransformControlPoint(Vector2 areaPosW_Min, Vector2 areaPosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (areaSelectType == SELECT_TYPE.New)
			{
				//일단 초기화
				for (int i = 0; i < _transformControlPoints.Count; i++)
				{
					_transformControlPoints[i]._isSelected = false;
				}
			}

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				Vector2 worldPos = _transformControlPoints[i]._worldPos;
				//클릭한 영역에 있는 걸 찾자
				if (worldPos.x > areaPosW_Min.x && worldPos.x < areaPosW_Max.x &&
					worldPos.y > areaPosW_Min.y && worldPos.y < areaPosW_Max.y)
				{
					if (areaSelectType == SELECT_TYPE.New || areaSelectType == SELECT_TYPE.Add)
					{
						_transformControlPoints[i]._isSelected = true;
					}
					else
					{
						_transformControlPoints[i]._isSelected = false;
					}
				}
			}

			//return GetNumSelectedTransformControlPoint();
			return SelectResult.Main.SetMultiple<TransformControlPoint>(GetSelectedTransformControlPoints());
		}



		// Vertex Transform - Soft Selection
		//--------------------------------------------------------------------------
		public void StartSoftSelection()
		{
			if (!IsSoftSelectionModeAvailable)
			{
				return;
			}

			_isSoftSelectionMode = true;

			if (_funcGizmoSoftSelection != null)
			{
				_funcGizmoSoftSelection();
			}
		}

		public void RefreshSoftSelectionValue(int radius, int curveRatio)
		{
			if (!_isSoftSelectionMode)
			{
				return;
			}

			_softSelectionRadius = radius;
			_softSelectionCurveRatio = curveRatio;

			if (_funcGizmoSoftSelection != null)
			{
				_funcGizmoSoftSelection();
			}
		}

		public void EndSoftSelection()
		{
			_isSoftSelectionMode = false;
		}


		// Vertex Transform - Blur
		//--------------------------------------------------------------------------
		public void StartBrush()
		{
			if (!IsBrushModeAvailable)
			{
				return;
			}

			_isBrushMode = true;
			_tMouseUpdate_Brush = 0.0f;
		}

		//public void RefreshBlurValue(int radius, int intensity)
		//{
		//	_brushRadius = radius;
		//	_brushIntensity = intensity;
		//}

		public void EndBrush()
		{
			_isBrushMode = false;
			if (_funcGizmoSyncBrushStatus != null)
			{
				_funcGizmoSyncBrushStatus(true);
			}
		}




		//--------------------------------------------------------------------------
		public TransformParam GetCurTransformParam()
		{
			return _curTransformParam;
		}

		public void OnTransformChanged_Position(Vector2 pos)
		{
			if (_funcTransformPosition != null)
			{
				_funcTransformPosition(pos);
			}
		}

		public void OnTransformChanged_Rotate(float rotateAngle)
		{
			if (_funcTransformRotate != null)
			{
				_funcTransformRotate(rotateAngle);
			}
		}

		public void OnTransformChanged_Scale(Vector2 scale)
		{
			if (_funcTransformScale != null)
			{
				_funcTransformScale(scale);
			}
		}

		public void OnTransformChanged_Depth(int depth)
		{
			if (_funcTransformDepth != null)
			{
				_funcTransformDepth(depth);
			}
		}

		public void OnTransformChanged_Color(Color color, bool isVisible)
		{
			if (_funcTransformColor != null)
			{
				_funcTransformColor(color, isVisible);
			}
		}

		public void OnTransformChanged_Extra()
		{
			if (_funcTransformExtra != null)
			{
				_funcTransformExtra();
			}
		}


		//--------------------------------------------------------------------------
		/// <summary>
		/// 단축키나 외부의 함수에서 SelectResult를 갱신하는 경우..
		/// </summary>
		/// <param name="obj"></param>
		public void SetSelectResultForce_Single(object obj)
		{
			if (obj == null)
			{
				SelectResult.Main.Init();
			}
			else
			{
				SelectResult.Main.SetSingle(obj);
			}

			SelectResult.Prev.SetResult(SelectResult.Main);
		}

		public void SetSelectResultForce_Multiple<T>(List<T> objs)
		{
			if (objs == null)
			{
				SelectResult.Main.Init();
			}
			else
			{
				SelectResult.Main.SetMultiple(objs);
			}

			SelectResult.Prev.SetResult(SelectResult.Main);
		}
		//--------------------------------------------------------------------------

		//public void OnTransformChanged_BoneIKController(float boneIKMixWeight)
		//{
		//	if(_funcTransformBoneIKMixWeight != null)
		//	{
		//		_funcTransformBoneIKMixWeight(Mathf.Clamp01(boneIKMixWeight));
		//	}
		//}
		//--------------------------------------------------------------------------
		//public void GUI_Render_Controller(Vector2 localPosOffset, apMatrix3x3 matrixLocal, apMatrix3x3 matrixToParent, apMatrix3x3 matrixToWorld)
		public void GUI_Render_Controller(apEditor editor)
		{
			//if(_isGizmoLock || _selectedObject == null)
			//if(_isGizmoLock || !_isGizmoEventRegistered)

			//_isForceUpdateFlagRequest = false;

			if (!_isGizmoEventRegistered)
			{
				return;
			}

			if (_isFFDMode)
			{
				RenderControl_TransformControl(editor._colorOption_GizmoFFDLine, editor._colorOption_GizmoFFDInnerLine);
			}

			if (_isAreaSelecting)
			{
				RenderControl_AreaSelect();
			}

			if (!_isGizmoRenderable)
			{
				return;
			}
			//apMatrix3x3 transformMtrx = apMatrix3x3.identity;

			//switch (_coordinate)
			//{
			//	case COORDINATE_TYPE.World:
			//		transformMtrx = matrixToWorld;
			//		break;

			//	case COORDINATE_TYPE.Parent:
			//		transformMtrx = matrixToParent;
			//		break;

			//	case COORDINATE_TYPE.Local:
			//		transformMtrx = matrixLocal;
			//		break;
			//}

			switch (_controlType)
			{
				case CONTROL_TYPE.Move:
					//RenderControl_Move(localPosOffset, transformMtrx);
					RenderControl_Move();
					break;

				case CONTROL_TYPE.Rotate:
					RenderControl_Rotate();
					break;

				case CONTROL_TYPE.Scale:
					RenderControl_Scale();
					break;

				case CONTROL_TYPE.Select:
					RenderControl_Select();
					break;
			}

			if (_isForceDrawFlag)
			{
				if (Event.current.type == EventType.Repaint)
				{
					_isForceDrawFlag = false;
				}
			}

		}

		//--------------------------------------------------------------------------
		//private void RenderControl_Select(Vector2 pos, apMatrix3x3 matrix)
		private void RenderControl_Select()
		{
			Vector2 originSize = GetImageSize(IMAGE_TYPE.Origin_Axis) / apGL.Zoom;

			//apMatrix3x3 mtrx_origin = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y, 0), Quaternion.identity, Vector3.one);

			//중심
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Origin_Axis),
				_mtrx_origin,
				originSize.x, originSize.y,
				GetColorFor2X(COLOR_PRESET.Origin),
				0.0f
				);

		}

		//private void RenderControl_Move(Vector2 pos, apMatrix3x3 matrix)
		private void RenderControl_Move()
		{
			//Vector2 axisImageSize = GetImageSize(IMAGE_TYPE.Transform_Move) / apGL.Zoom;
			Vector2 originSize = GetImageSize(IMAGE_TYPE.Origin_None) / apGL.Zoom;

			//apMatrix3x3 mtrx_axisY = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y + axisImageSize.y / 2, 0), Quaternion.identity, Vector3.one);
			//apMatrix3x3 mtrx_axisX = matrix * apMatrix3x3.TRS(new Vector3(pos.x + axisImageSize.y / 2, pos.y, 0), Quaternion.Euler(0.0f, 0.0f, -90.0f), Vector3.one);
			//apMatrix3x3 mtrx_origin = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y, 0), Quaternion.identity, Vector3.one);

			Color originColor = GetColorFor2X(COLOR_PRESET.Origin);
			Color axisXColor = GetColorFor2X(COLOR_PRESET.Axis_X);
			Color axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Y);
			switch (_selectedAxis)
			{
				case SELECTED_AXIS.Axis_X:
					axisXColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					break;

				case SELECTED_AXIS.Axis_Y:
					axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					break;

				case SELECTED_AXIS.All:
					axisXColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					originColor = GetColorFor2X(COLOR_PRESET.Origin_Selected);
					break;
			}

			//X축
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Transform_Move),
				_mtrx_move_axisX,
				_axisImageSize_Move.x, _axisImageSize_Move.y,
				axisXColor,
				0.0f
				);

			//Y축
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Transform_Move),
				_mtrx_move_axisY,
				_axisImageSize_Move.x, _axisImageSize_Move.y,
				axisYColor,
				0.0f
				);

			//중심
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Origin_None),
				_mtrx_origin,
				originSize.x, originSize.y,
				originColor,
				0.0f
				);

		}


		//private void RenderControl_Rotate(Vector2 pos, apMatrix3x3 matrix)
		private void RenderControl_Rotate()
		{
			Vector2 originSize = GetImageSize(IMAGE_TYPE.Origin_Axis) / apGL.Zoom;
			Vector2 circleSize = GetImageSize(IMAGE_TYPE.Transform_Rotate) / apGL.Zoom;

			//apMatrix3x3 mtrx_origin = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y, 0), Quaternion.identity, Vector3.one);

			Color originColor = GetColorFor2X(COLOR_PRESET.Origin);
			Color rotateColor = GetColorFor2X(COLOR_PRESET.Rotate);
			switch (_selectedAxis)
			{
				case SELECTED_AXIS.All:
					rotateColor = GetColorFor2X(COLOR_PRESET.Rotate_Selected);
					originColor = GetColorFor2X(COLOR_PRESET.Origin_Selected);
					break;
			}


			//Y축
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Transform_Rotate),
				_mtrx_origin,
				circleSize.x, circleSize.y,
				rotateColor,
				0.0f
				);

			//중심
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Origin_Axis),
				_mtrx_origin,
				originSize.x, originSize.y,
				originColor,
				0.0f
				);

			switch (_selectedAxis)
			{
				case SELECTED_AXIS.All:
					{
						float lengthToMouse = (_originPos - _mousePosW_Prev).magnitude;
						Vector3 dirPrev = new Vector3(lengthToMouse * Mathf.Cos(_rotateAngle_Prev * Mathf.Deg2Rad), lengthToMouse * Mathf.Sin(_rotateAngle_Prev * Mathf.Deg2Rad), 0);
						Vector3 dirDown = new Vector3(lengthToMouse * Mathf.Cos(_rotateAngle_Down * Mathf.Deg2Rad), lengthToMouse * Mathf.Sin(_rotateAngle_Down * Mathf.Deg2Rad), 0);

						//dirPrev = _mtrx_origin.MultiplyPoint3x4(dirPrev);
						//dirDown = _mtrx_origin.MultiplyPoint3x4(dirDown);

						apGL.DrawBoldLine(_originPos, _originPos + new Vector2(dirPrev.x, dirPrev.y), 4, new Color(0.0f, 1.0f, 1.0f, 1.0f), true);
						apGL.DrawBoldLine(_originPos, _originPos + new Vector2(dirDown.x, dirDown.y), 4, new Color(0.0f, 0.5f, 1.0f, 1.0f), true);

						float deltaAngle = _rotateAngle_Prev - _rotateAngle_Down;
						if (deltaAngle < -180.0f)
						{
							deltaAngle += 360.0f;
						}
						if (deltaAngle > 180.0f)
						{
							deltaAngle -= 360.0f;
						}

						string strDeltaAngle = "";
						if (deltaAngle > 0.0f)
						{
							strDeltaAngle = "+" + (int)deltaAngle + "." + ((int)(deltaAngle * 10.0f) % 10);
						}
						else
						{
							deltaAngle = -deltaAngle;
							strDeltaAngle = "-" + (int)deltaAngle + "." + ((int)(deltaAngle * 10.0f) % 10);
						}
						int length = strDeltaAngle.Length * 7;

						Vector2 textPos_Org = apGL.World2GL(_originPos);
						Vector2 textPos_Dir = apGL.World2GL(_originPos + new Vector2(dirPrev.x, dirPrev.y));
						Vector2 textPos = textPos_Dir + (textPos_Dir - textPos_Org).normalized * 40.0f;

						textPos.x -= length * 0.5f;
						textPos.y -= 7.0f;

						apGL.DrawTextGL(strDeltaAngle, textPos, length, Color.yellow);
					}
					break;
			}

		}

		//private void RenderControl_Scale(Vector2 pos, apMatrix3x3 matrix)
		private void RenderControl_Scale()
		{
			//Vector2 axisImageSize = GetImageSize(IMAGE_TYPE.Transform_Scale) / apGL.Zoom;
			Vector2 originSize = GetImageSize(IMAGE_TYPE.Origin_None) / apGL.Zoom;

			//apMatrix3x3 mtrx_axisY = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y + axisImageSize.y / 2, 0), Quaternion.identity, Vector3.one);
			//apMatrix3x3 mtrx_axisX = matrix * apMatrix3x3.TRS(new Vector3(pos.x + axisImageSize.y / 2, pos.y, 0), Quaternion.Euler(0.0f, 0.0f, -90.0f), Vector3.one);
			//apMatrix3x3 mtrx_origin = matrix * apMatrix3x3.TRS(new Vector3(pos.x, pos.y, 0), Quaternion.identity, Vector3.one);


			Color originColor = GetColorFor2X(COLOR_PRESET.Origin);
			Color axisXColor = GetColorFor2X(COLOR_PRESET.Axis_X);
			Color axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Y);
			switch (_selectedAxis)
			{
				case SELECTED_AXIS.Axis_X:
					axisXColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					break;

				case SELECTED_AXIS.Axis_Y:
					axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					break;

				case SELECTED_AXIS.All:
					axisXColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					axisYColor = GetColorFor2X(COLOR_PRESET.Axis_Selected);
					originColor = GetColorFor2X(COLOR_PRESET.Origin_Selected);
					break;
			}


			//Y축
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Transform_Scale),
				_mtrx_scale_axisY,
				_axisImageSize_Scale.x, _axisImageSize_Scale.y,
				axisYColor,
				0.0f
				);

			//X축
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Transform_Scale),
				_mtrx_scale_axisX,
				_axisImageSize_Scale.x, _axisImageSize_Scale.y,
				axisXColor,
				0.0f
				);

			//중심
			apGL.DrawTexture(
				GetImage(IMAGE_TYPE.Origin_None),
				_mtrx_origin,
				originSize.x, originSize.y,
				originColor,
				0.0f
				);
		}

		private void RenderControl_AreaSelect()
		{

			Vector2 areaMin = new Vector2(Mathf.Min(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Min(_areaPosStart_W.y, _areaPosEnd_W.y));
			Vector2 areaMax = new Vector2(Mathf.Max(_areaPosStart_W.x, _areaPosEnd_W.x), Mathf.Max(_areaPosStart_W.y, _areaPosEnd_W.y));

			//Debug.Log("RenderControl_AreaSelect : " + _areaPosStart_W + " / " +_areaPosEnd_W );
			Color lineColor = Color.black;
			switch (_areaSelectType)
			{
				case SELECT_TYPE.New:
					lineColor = new Color(0.0f, 1.0f, 0.5f, 0.9f);
					break;

				case SELECT_TYPE.Add:
					lineColor = new Color(0.0f, 0.5f, 1.0f, 0.9f);
					break;

				case SELECT_TYPE.Subtract:
					lineColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
					break;
			}

			apGL.BeginBatch_ColoredPolygon();
			apGL.DrawBoldLine(new Vector2(areaMin.x, areaMin.y), new Vector2(areaMax.x, areaMin.y), 3.0f, lineColor, false);
			apGL.DrawBoldLine(new Vector2(areaMax.x, areaMin.y), new Vector2(areaMax.x, areaMax.y), 3.0f, lineColor, false);
			apGL.DrawBoldLine(new Vector2(areaMax.x, areaMax.y), new Vector2(areaMin.x, areaMax.y), 3.0f, lineColor, false);
			apGL.DrawBoldLine(new Vector2(areaMin.x, areaMax.y), new Vector2(areaMin.x, areaMin.y), 3.0f, lineColor, false);
			apGL.EndBatch();
		}

		private void RenderControl_TransformControl(Color FFDLineColor, Color FFDInnerLineColor)
		{
			if (!_isFFDMode
				//|| _transformControlPoints.Count < 9
				|| _transformControlPoints.Count < 4
				)
			{
				return;
			}

			Texture2D pointImg = GetImage(IMAGE_TYPE.TransformController);
			Color pointColor = GetColor(COLOR_PRESET.TransformController);
			Color pointColor_Selected = GetColor(COLOR_PRESET.TransformController_Selected);
			//Color lineColor = new Color(1.0f, 0.5f, 0.2f, 0.9f);
			//Color lineColor_Inner = new Color(1.0f, 0.7f, 0.2f, 0.7f);

			Vector2 pointSize = GetImageSize(IMAGE_TYPE.TransformController) / apGL.Zoom;

			//6, 7, 8
			//3, 4, 5
			//0, 1, 2

			apGL.BeginBatch_ColoredPolygon();


			int iPoint_LB = 0;
			int iPoint_RB = 0;
			int iPoint_LT = 0;
			int iPoint_RT = 0;
			//여기도 개수를 커스텀하게 설정한다.
			for (int iX = 0; iX < _nFFDPointX - 1; iX++)
			{
				for (int iY = 0; iY < _nFFDPointY - 1; iY++)
				{
					iPoint_LB = (iX + 0) + ((iY + 0) * _nFFDPointX);
					iPoint_RB = (iX + 1) + ((iY + 0) * _nFFDPointX);
					iPoint_LT = (iX + 0) + ((iY + 1) * _nFFDPointX);
					iPoint_RT = (iX + 1) + ((iY + 1) * _nFFDPointX);

					if (iY == 0)
					{
						apGL.DrawBoldLine(_transformControlPoints[iPoint_LB]._worldPos, _transformControlPoints[iPoint_RB]._worldPos, 4, FFDLineColor, false);
					}

					apGL.DrawBoldLine(_transformControlPoints[iPoint_RB]._worldPos, _transformControlPoints[iPoint_RT]._worldPos, 4, FFDLineColor, false);
					apGL.DrawBoldLine(_transformControlPoints[iPoint_RT]._worldPos, _transformControlPoints[iPoint_LT]._worldPos, 4, FFDLineColor, false);

					if (iX == 0)
					{
						apGL.DrawBoldLine(_transformControlPoints[iPoint_LT]._worldPos, _transformControlPoints[iPoint_LB]._worldPos, 4, FFDLineColor, false);
					}
				}
			}
			//apGL.DrawBoldLine(_transformControlPoints[0]._worldPos, _transformControlPoints[1]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[1]._worldPos, _transformControlPoints[2]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[2]._worldPos, _transformControlPoints[5]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[5]._worldPos, _transformControlPoints[8]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[8]._worldPos, _transformControlPoints[7]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[7]._worldPos, _transformControlPoints[6]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[6]._worldPos, _transformControlPoints[3]._worldPos, 4, FFDLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[3]._worldPos, _transformControlPoints[0]._worldPos, 4, FFDLineColor, false);

			//apGL.DrawBoldLine(_transformControlPoints[3]._worldPos, _transformControlPoints[4]._worldPos, 3, FFDInnerLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[4]._worldPos, _transformControlPoints[5]._worldPos, 3, FFDInnerLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[7]._worldPos, _transformControlPoints[4]._worldPos, 3, FFDInnerLineColor, false);
			//apGL.DrawBoldLine(_transformControlPoints[4]._worldPos, _transformControlPoints[1]._worldPos, 3, FFDInnerLineColor, false);

			apGL.EndBatch();

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				TransformControlPoint controlPoint = _transformControlPoints[i];
				if (controlPoint._isSelected)
				{
					apGL.DrawTexture(pointImg, controlPoint._worldPos, pointSize.x, pointSize.y, pointColor_Selected);
				}
				else
				{
					apGL.DrawTexture(pointImg, controlPoint._worldPos, pointSize.x, pointSize.y, pointColor);
				}

			}
		}


		// 키보드 이벤트
		//-------------------------------------------------------------------------------------------
		//일반 이벤트 (등록된 이벤트를 다시 호출해야한다.)
		private apHotKey.HotKeyResult OnKeyboardEvent_Normal_Move(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			//Debug.Log("OnKeyboardEvent_Normal_Move : " + keyCode + " / isShift : " + (isShift));
			if(_func_KeyboardMove != null)
			{
				_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Move_Normal;

				//1씩 이동 / Shift를 눌렀다면 10씩 이동
				Vector2 deltaMoveW = Vector2.zero;
				float moveSize = isShift ? 10.0f : 1.0f;

				switch (keyCode)
				{
					case KeyCode.LeftArrow:		deltaMoveW.x = -moveSize;	break;//-X
					case KeyCode.RightArrow:	deltaMoveW.x = moveSize;	break;//+X
					case KeyCode.UpArrow:		deltaMoveW.y = moveSize;	break;//+Y
					case KeyCode.DownArrow:		deltaMoveW.y = -moveSize;	break;//-Y
				}

				try
				{
					_func_KeyboardMove(deltaMoveW, _keyboardContEventStatus != _prevKeyboardContEventStatus);
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : HotKey Exception : " + ex);
				}
				

				_prevKeyboardContEventStatus = _keyboardContEventStatus;

				return apHotKey.HotKeyResult.MakeResult();
			}
			return null;
		}

		private apHotKey.HotKeyResult OnKeyboardEvent_Normal_Rotate(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			//Debug.Log("OnKeyboardEvent_Normal_Rotate : " + keyCode + " / isShift : " + (isShift));

			if (_func_KeyboardRotate != null)
			{
				_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Rotate_Normal;

				//각도를 1씩 이동 / Shift를 눌렀다면 10씩 이동
				float deltaAngle = 0.0f;
				float angleSize = isShift ? 10.0f : 1.0f;

				switch (keyCode)
				{
					case KeyCode.LeftArrow:		deltaAngle = angleSize;		break;//+X (CCW)
					case KeyCode.RightArrow:	deltaAngle = -angleSize;	break;//-X (CW)
				}

				try
				{
					_func_KeyboardRotate(deltaAngle, _keyboardContEventStatus != _prevKeyboardContEventStatus);
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : HotKey Exception : " + ex);
				}
				

				_prevKeyboardContEventStatus = _keyboardContEventStatus;

				return apHotKey.HotKeyResult.MakeResult();
			}

			return null;

			
		}

		private apHotKey.HotKeyResult OnKeyboardEvent_Normal_Scale(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			//Debug.Log("OnKeyboardEvent_Normal_Scale : " + keyCode + " / isShift : " + (isShift));

			if(_func_KeyboardScale != null)
			{
				_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Scale_Normal;

				//0.01씩 이동 / Shift를 눌렀다면 0.1씩 이동
				Vector2 deltaScale = Vector2.zero;
				float scaleSize = isShift ? 0.1f : 0.01f;

				switch (keyCode)
				{
					case KeyCode.LeftArrow:		deltaScale.x = -scaleSize;	break;//-X
					case KeyCode.RightArrow:	deltaScale.x = scaleSize;	break;//+X
					case KeyCode.UpArrow:		deltaScale.y = scaleSize;	break;//+Y
					case KeyCode.DownArrow:		deltaScale.y = -scaleSize;	break;//-Y
				}

				try
				{
					_func_KeyboardScale(deltaScale, _keyboardContEventStatus != _prevKeyboardContEventStatus);
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : HotKey Exception : " + ex);
				}


				_prevKeyboardContEventStatus = _keyboardContEventStatus;

				return apHotKey.HotKeyResult.MakeResult();
			}

			return null;
		}


		private apHotKey.HotKeyResult OnKeyboardEvent_FFD_Move(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			if (!_isFFDMode
				|| GetNumSelectedTransformControlPoint() == 0)
			{
				return null;
			}

			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Move_FFD;

			//Debug.Log("OnKeyboardEvent_FFD_Move : " + keyCode + " / isShift : " + (isShift));
			//1씩 이동 / Shift를 눌렀다면 10씩 이동
			Vector2 deltaMoveW = Vector2.zero;
			float moveSize = isShift ? 10.0f : 1.0f;

			switch (keyCode)
			{
				case KeyCode.LeftArrow:	deltaMoveW.x = -moveSize;	break;//-X
				case KeyCode.RightArrow:	deltaMoveW.x = moveSize;	break;//+X
				case KeyCode.UpArrow:	deltaMoveW.y = moveSize;	break;//+Y
				case KeyCode.DownArrow:	deltaMoveW.y = -moveSize;	break;//-Y
			}

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					_transformControlPoints[i]._worldPos += deltaMoveW;
				}
			}

			RefreshTransformObjects();

			_prevKeyboardContEventStatus = _keyboardContEventStatus;

			return apHotKey.HotKeyResult.MakeResult();
		}

		private apHotKey.HotKeyResult OnKeyboardEvent_FFD_Rotate(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			if (!_isFFDMode
				|| GetNumSelectedTransformControlPoint() == 0)
			{
				return null;
			}

			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Rotate_FFD;

			//Debug.Log("OnKeyboardEvent_FFD_Rotate : " + keyCode + " / isShift : " + (isShift));
			//각도를 1씩 이동 / Shift를 눌렀다면 10씩 이동
			float deltaAngle = 0.0f;
			float angleSize = isShift ? 10.0f : 1.0f;

			switch (keyCode)
			{
				case KeyCode.LeftArrow:		deltaAngle = angleSize;	break;//+X (CCW)
				case KeyCode.RightArrow:	deltaAngle = -angleSize;	break;//-X (CW)
			}

			Vector2 centerPos = GetTransformControlCenterPos();

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngle, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					//Vector2 worldPos3 = new Vector3(_transformControlPoints[i]._worldPos.x, _transformControlPoints[i]._worldPos.y, 0);

					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(_transformControlPoints[i]._worldPos);

					_transformControlPoints[i]._worldPos = nextWorldPos2;
				}
			}

			RefreshTransformObjects();

			_prevKeyboardContEventStatus = _keyboardContEventStatus;

			return apHotKey.HotKeyResult.MakeResult();
		}

		private apHotKey.HotKeyResult OnKeyboardEvent_FFD_Scale(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			if (!_isFFDMode
				|| GetNumSelectedTransformControlPoint() == 0)
			{
				return null;
			}

			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.Scale_FFD;

			//Debug.Log("OnKeyboardEvent_FFD_Scale : " + keyCode + " / isShift : " + (isShift));
			//0.01씩 이동 / Shift를 눌렀다면 0.1씩 이동
			Vector2 deltaScale = Vector2.zero;
			float scaleSize = isShift ? 0.1f : 0.01f;

			switch (keyCode)
			{
				case KeyCode.LeftArrow:		deltaScale.x = -scaleSize;	break;//-X
				case KeyCode.RightArrow:	deltaScale.x = scaleSize;	break;//+X
				case KeyCode.UpArrow:		deltaScale.y = scaleSize;	break;//+Y
				case KeyCode.DownArrow:		deltaScale.y = -scaleSize;	break;//-Y
			}

			Vector2 centerPos = GetTransformControlCenterPos();

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, new Vector2(1.0f + deltaScale.x, 1.0f + deltaScale.y))
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			for (int i = 0; i < _transformControlPoints.Count; i++)
			{
				if (_transformControlPoints[i]._isSelected)
				{
					Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(_transformControlPoints[i]._worldPos);

					_transformControlPoints[i]._worldPos = nextWorldPos;
				}
			}

			RefreshTransformObjects();

			_prevKeyboardContEventStatus = _keyboardContEventStatus;

			return apHotKey.HotKeyResult.MakeResult();
		}

		private apHotKey.HotKeyResult OnKeyboardEvent_FFD_EnterOrEscape(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_curBtnStatus_Left == apMouse.MouseBtnStatus.Down 
				|| _curBtnStatus_Left == apMouse.MouseBtnStatus.Pressed
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Down
				|| _curBtnStatus_Right == apMouse.MouseBtnStatus.Pressed)
			{
				//마우스 입력이 있는 중이라면 일단 무효
				return null;
			}

			if (!_isFFDMode)
			{
				return null;
			}
			//Debug.Log("OnKeyboardEvent_FFD_EnterOrEscape : " + keyCode + " / isShift : " + (isShift));
			if(keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter)
			{
				AdaptTransformObjects(_editor);
			}
			else if(keyCode == KeyCode.Escape)
			{
				RevertTransformObjects(_editor);
			}

			//Enter / Esc는 키보드 입력에 연속성이 없다.
			_keyboardContEventStatus = KEYBOARD_CONT_EVENT.None;
			_prevKeyboardContEventStatus = KEYBOARD_CONT_EVENT.None;

			return apHotKey.HotKeyResult.MakeResult();
		}



		
	}


}