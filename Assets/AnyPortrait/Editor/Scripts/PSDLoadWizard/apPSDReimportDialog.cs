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
using System.Collections.Generic;
using Ntreev.Library.Psd;
using System.Threading;

using AnyPortrait;

namespace AnyPortrait
{
	public class apPSDReimportDialog : EditorWindow
	{
		// Menu
		//----------------------------------------------------------
		private static apPSDReimportDialog s_window = null;

		public static object ShowWindow(apEditor editor, FUNC_PSD_REIMPORT_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apPSDReimportDialog), false, "PSD Reload");
			apPSDReimportDialog curTool = curWindow as apPSDReimportDialog;
			if (curTool != null && curTool != s_window)
			{

				int width = 1100;
				int height = 700;

				object loadKey = new object();

				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, editor._portrait, funcResult, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		// Members
		//----------------------------------------------------------
		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;
		private FUNC_PSD_REIMPORT_RESULT _funcResult = null;
		public delegate void FUNC_PSD_REIMPORT_RESULT(	bool isSuccess, 
														object loadKey, string fileName, string filePath,
														List<apPSDLayerData> layerDataList, 
														int atlasScaleRatioX100, int meshGroupScaleRatioX100, int prevAtlasScaleRatioX100,
														int totalWidth, int totalHeight, 
														int padding, 
														int bakedTextureWidth, int bakedTextureHeight, int bakeMaximumNumAtlas, bool bakeBlurOption,
														float centerOffsetDeltaX, float centerOffsetDeltaY,
														string bakeDstFilePath, string bakeDstFileRelativePath,
														apPSDSet psdSet
														//float deltaScaleRatio
														);//<<나중에 처리 결과에 따라서 더 넣어주자


		private bool _isGUIEvent = false;
		private Dictionary<string, bool> _delayedGUIShowList = new Dictionary<string, bool>();
		private Dictionary<string, bool> _delayedGUIToggledList = new Dictionary<string, bool>();

		public enum RELOAD_STEP
		{
			Step1_SelectPSDSet,//PSD Set을 선택하거나 생성 (또는 삭제)
			Step2_FileLoadAndSelectMeshGroup,//PSD Set과 연결된 PSD 파일/MeshGroup/Atlas Texture Data를 선택 + 크기 오프셋 설정
			Step3_LinkLayerToTransform,//레이어 정보와 Mesh/MeshGroup Transform 연결 <GUI - 메시+레이어>
			Step4_ModifyOffset,//레이어의 위치 수정 <GUI - 메시+레이어>
			Step5_AtlasSetting,//아틀라스 굽기 (+PSD에서 삭제되면서 TextureData 갱신에 포함되지 않는 경우 포함) <GUI - Atlas>
		}

		private RELOAD_STEP _step = RELOAD_STEP.Step1_SelectPSDSet;

		private Color _glBackGroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

		private apPSDSet _selectedPSDSet = null;
		//private apPSDSetLayer _selectedPSDSetLayer = null;
		private apPSDSet.TextureDataSet _selectedTextureData = null;

		private const int PSD_IMAGE_FILE_MAX_SIZE = 5000;

		//PSD Loader
		private apPSDLoader _psdLoader = null;
		private apPSDLayerData _selectedPSDLayerData = null;
		private apPSDBakeData _selectedPSDBakeData = null;

		private apPSDLayerData _linkSrcLayerData = null;
		private bool _isLinkLayerToTransform = false;

		//MeshTransform, MeshGroupTransform -> apPSDLayerData으로 참조하는 데이터를 알 수 있어야 한다.
		public class TargetTransformData
		{
			public bool _isMeshTransform;
			public apMesh _mesh = null;
			public apTransform_Mesh _meshTransform = null;
			public apTransform_MeshGroup _meshGroupTransform = null;
			public bool _isClipped = false;
			public bool _isValidMesh = false;
			public int _transformID = -1;
			

			public TargetTransformData(apTransform_Mesh meshTransform, bool isValidMesh)
			{
				_isMeshTransform = true;
				_meshTransform = meshTransform;
				_mesh = meshTransform._mesh;
				_meshGroupTransform = null;
				_isValidMesh = isValidMesh;
				_transformID = meshTransform._transformUniqueID;
				_isClipped = _meshTransform._isClipping_Child;
			}

			public TargetTransformData(apTransform_MeshGroup meshGroupTransform)
			{
				_isMeshTransform = false;
				_meshTransform = null;
				_mesh = null;
				_meshGroupTransform = meshGroupTransform;
				_isValidMesh = true;
				_transformID = meshGroupTransform._transformUniqueID;
				_isClipped = false;
			}

			public string Name
			{
				get
				{
					if(_meshTransform != null)
					{
						return _meshTransform._nickName;
					}
					else if(_meshGroupTransform != null)
					{
						return _meshGroupTransform._nickName;
					}
					return "";
				}
			}
		}
		private List<TargetTransformData> _targetTransformList = new List<TargetTransformData>();
		private Dictionary<apTransform_Mesh, apPSDLayerData> _meshTransform2PSDLayer = new Dictionary<apTransform_Mesh, apPSDLayerData>();
		private Dictionary<apTransform_MeshGroup, apPSDLayerData> _meshGroupTransform2PSDLayer = new Dictionary<apTransform_MeshGroup, apPSDLayerData>();

		//PSD Remap List
		//private List<apPSDRemapData> _remapList = new List<apPSDRemapData>();
		//private Dictionary<apPSDLayerData, apPSDRemapData> _remapList_Psd2Map = new Dictionary<apPSDLayerData, apPSDRemapData>();

		// GUI
		//public int _iZoomX100 = 11;//11 => 100
		//public const int ZOOM_INDEX_DEFAULT = 11;
		//public int[] _zoomListX100 = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 85, 90, 95, 100/*(11)*/, 105, 110, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450, 500 };
		public int _iZoomX100 = 36;//36 => 100
		public const int ZOOM_INDEX_DEFAULT = 36;
		public int[] _zoomListX100 = new int[] {    4,	6,	8,	10, 12, 14, 16, 18, 20, 22, //9
													24, 26, 28, 30, 32, 34, 36, 38, 40, 42, //19
													44, 46, 48, 50, 52, 54, 56, 58, 60, 65, //29
													70, 75, 80, 85, 90, 95, 100, //39
													105, 110, 115, 120, 125, 130, 140, 150, 160, 180, 200,
													220, 240, 260, 280, 300, 350, 400, 450,
													500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000,
													2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, 2900, 3000 };

		private Vector2 _scroll_GUI = Vector2.zero;

		private apPSDMouse _mouse = new apPSDMouse();
		private apPSDGL _gl = new apPSDGL();

		private bool _isCtrlAltDrag = false;

		//private bool _isRender_PSD = true;
		private bool _isRender_MeshGroup = false;
		private bool _isRender_TextureData = false;
		private enum RENDER_MODE
		{
			Normal,
			Outline,
			Hide,
		}
		//private bool _isRender_PSDAlpha = true;
		private RENDER_MODE _renderMode_PSD = RENDER_MODE.Normal;
		private RENDER_MODE _renderMode_Mesh = RENDER_MODE.Normal;

		//private int _psdRenderPosOffset_X = 0;
		//private int _psdRenderPosOffset_Y = 0;

		private bool _isLinkGUIColoredList = false;
		private bool _isLinkOverlayColorRender = false;
		private Color _meshOverlayColor = new Color(1.0f, 0.0f, 0.0f, 0.85f);
		private Color _psdOverlayColor = new Color(0.0f, 1.0f, 0.8f, 0.85f);

		//private bool _isRenderOffset_PSD = true;
		//private bool _isRenderOffset_Mesh = true;
		private bool _isRenderMesh2PSD = true;

		// Scroll
		private Vector2 _scroll_Step1_Left = Vector2.zero;
		private Vector2 _scroll_Step2_Left = Vector2.zero;
		private Vector2 _scroll_Step3_Line1 = Vector2.zero;
		private Vector2 _scroll_Step3_Line2 = Vector2.zero;
		private Vector2 _scroll_Step4_Left = Vector2.zero;
		private Vector2 _scroll_Step5_Left = Vector2.zero;



		private bool _isRequestCloseDialog = false;
		private bool _isDialogEnded = false;

		
		private bool IsGUIUsable { get { return (!_psdLoader.IsProcessRunning); } }
		private bool IsProcessRunning { get { return _psdLoader.IsProcessRunning; } }

		private string[] _bakeDescription = new string[] { "256", "512", "1024", "2048", "4096" };
		private bool _isNeedBakeCheck = false;
		private bool _isBakeWarning = false;
		private string _bakeWarningMsg = "";
		private object _loadKey_Calculated = null;
		private object _loadKey_Bake = null;

		// Init
		//----------------------------------------------------------
		private void Init(apEditor editor, apPortrait portrait, FUNC_PSD_REIMPORT_RESULT funcResult, object loadKey)
		{
			_editor = editor;
			_portrait = portrait;
			_loadKey = loadKey;
			_funcResult = funcResult;

			_step = RELOAD_STEP.Step1_SelectPSDSet;

			Shader[] shaderSet_Normal = new Shader[4];
			Shader[] shaderSet_VertAdd = new Shader[4];
			//Shader[] shaderSet_Mask = new Shader[4];
			Shader[] shaderSet_Clipped = new Shader[4];
			for (int i = 0; i < 4; i++)
			{
				shaderSet_Normal[i] = editor._mat_Texture_Normal[i].shader;
				shaderSet_VertAdd[i] = editor._mat_Texture_VertAdd[i].shader;
				//shaderSet_Mask[i] = editor._mat_MaskedTexture[i].shader;
				shaderSet_Clipped[i] = editor._mat_Clipped[i].shader;
			}

			//_gl.SetMaterial(editor._mat_Color, editor._mat_Texture, editor._mat_MaskedTexture);
			_gl.SetShader(editor._mat_Color.shader,
							shaderSet_Normal,
							shaderSet_VertAdd,
							//shaderSet_Mask,
							editor._mat_MaskOnly.shader,
							shaderSet_Clipped,
							editor._mat_GUITexture.shader,
							editor._mat_ToneColor_Normal.shader,
							editor._mat_ToneColor_Clipped.shader,
							editor._mat_Alpha2White.shader,
							editor._mat_BoneV2.shader,
							editor._mat_Texture_VColorMul.shader,
							editor._mat_RigCircleV2.shader,
							editor._mat_Gray_Normal.shader,
							editor._mat_Gray_Clipped.shader);

			wantsMouseMove = true;

			//값 초기화
			_selectedPSDSet = null;
			//_selectedPSDSetLayer = null;
			_selectedTextureData = null;

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			_targetTransformList.Clear();
			_meshTransform2PSDLayer.Clear();
			_meshGroupTransform2PSDLayer.Clear();

			//for (int i = 0; i < _portrait._bakedPsdSets.Count; i++)
			//{
			//	_portrait._bakedPsdSets[i].ReadyToLoad();
			//}

			_isRequestCloseDialog = false;
			_isDialogEnded = false;

			_psdLoader = new apPSDLoader(editor);
			_selectedPSDLayerData = null;
			_selectedPSDBakeData = null;

			//_isRender_PSD = true;
			_isRender_MeshGroup = true;//<<두개가 보인다.
			_isRender_TextureData = false;
			//_isRender_PSDAlpha = true;//기본 알파
			_isLinkGUIColoredList = false;
			_isLinkOverlayColorRender = false;

			//_isRenderOffset_PSD = true;
			//_isRenderOffset_Mesh = true;
			_isRenderMesh2PSD = true;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;
			
			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;
		}


		public static void CloseDialog()
		{
			if (s_window != null)
			{
				//s_window.CloseThread();

				try
				{
					s_window._isRequestCloseDialog = false;
					s_window._isDialogEnded = true;
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}

		//?
		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			Repaint();

			if (_psdLoader != null)
			{
				_psdLoader.Update();
			}

			if(_isRequestCloseDialog)
			{
				CloseDialog();
			}
		}

		void OnGUI()
		{
			try
			{
				if (_editor == null || _editor._portrait == null)
				{
					CloseDialog();
					return;
				}

				if(_isDialogEnded)
				{
					return;
				}

				//GUI 이벤트인지 판별
				if (Event.current.type != EventType.Layout
					&& Event.current.type != EventType.Repaint)
				{
					_isGUIEvent = false;
				}
				else
				{
					_isGUIEvent = true;
				}

				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				int topHeight = 28;
				int bottomHeight = 46;
				int margin = 4;

				if (_selectedPSDSet == null && _step != RELOAD_STEP.Step1_SelectPSDSet)
				{
					//만약 에러가 나서 PSDSet 선택이 해제되면, 강제로 첫 화면으로 돌아가야한다.
					_step = RELOAD_STEP.Step1_SelectPSDSet;
				}

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));
				//-------------------------------------------------------------------------------------------------------
				int centerHeight = windowHeight - (topHeight + bottomHeight + margin * 2);

				// Top UI : Step을 차례로 보여준다.
				//-----------------------------------------------
				GUI.Box(new Rect(0, 0, windowWidth, topHeight), "");

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(topHeight));

				GUI_Top(windowWidth, topHeight - 6);

				EditorGUILayout.EndVertical();
				// Center UI : Step별 설정을 보여준다.
				//-----------------------------------------------
				GUILayout.Space(margin);

				// Center UI : 메인 에디터
				//-----------------------------------------------

				//EditorGUILayout.BeginVertical();
				Rect centerRect = new Rect(0, topHeight + margin - 2, windowWidth, centerHeight + 2);
				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));
				//Mouse Update
				//Mouse ScrollUpdate
				//GUI..
				GUI_Center(windowWidth, centerHeight, centerRect);
				

				EditorGUILayout.EndVertical();

				GUILayout.Space(4);

				// Bottom UI : 스텝 이동/확인/취소를 제어할 수 있다.
				//--------------------------------------------
				GUI.Box(new Rect(0, topHeight + margin + centerHeight + margin, windowWidth, bottomHeight), "");
				GUILayout.Space(margin);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(bottomHeight));
				GUI_Bottom(windowWidth, bottomHeight - 12);
				EditorGUILayout.EndVertical();
				//-------------------------------------------------------------------------------------------------------
				EditorGUILayout.EndVertical();

				//if(_isRequestCloseDialog)
				//{
				//	CloseDialog();
				//}
			}
			catch (Exception)
			{

			}
		}


		// Top
		private void GUI_Top(int width, int height)
		{
			int stepWidth = (width / 8) - 10;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			//GUILayout.Space(20);
			int totalContentWidth = ((stepWidth + 2) * 5) + (50 * 4);
			GUILayout.Space((width / 2) - (totalContentWidth / 2));

			Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = GUI.skin.box;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.normal.textColor = apEditorUtil.BoxTextColor;

			GUIStyle guiStyle_Next = GUI.skin.label;
			guiStyle_Next.alignment = TextAnchor.MiddleCenter;

			Color selectedColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
			Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			for (int iStep = 0; iStep < 5; iStep++)
			{
				RELOAD_STEP stepType = (RELOAD_STEP)iStep;
				if (_step == stepType)	{ GUI.backgroundColor = selectedColor; }
				else								{ GUI.backgroundColor = unselectedColor; }
				string strLabel = "";
				switch (stepType)
				{
					case RELOAD_STEP.Step1_SelectPSDSet:
						strLabel = _editor.GetText(TEXT.DLG_PSD_PSDSet);//PSD Set
						break;

					case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:
						strLabel = _editor.GetText(TEXT.DLG_PSD_BasicSetting);//BasicSetting
						break;

					case RELOAD_STEP.Step3_LinkLayerToTransform:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Mapping);//Mapping
						break;

					case RELOAD_STEP.Step4_ModifyOffset:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Adjust);//Adjust
						break;

					case RELOAD_STEP.Step5_AtlasSetting:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Atlas);//Atlas
						break;
				}
				GUILayout.Box(strLabel, guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

				if(iStep < 4)
				{
					GUILayout.Space(10);
					GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
					GUILayout.Space(10);
				}
			}

			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = prevColor;
		}


		// Bottom
		private void GUI_Bottom(int width, int height)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			int btnWidth = 120;
			int btnWidth_Cancel = 100;
			int margin = width - (btnWidth * 2 + btnWidth_Cancel + 12 + 30 + (120 * 3) + 10 + 2 + 20);

			GUILayout.Space(10);
			//Background Color / PSD Line Color / Mesh Line Color
			EditorGUILayout.BeginVertical(GUILayout.Width(120));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BackgroundColor), GUILayout.Width(120));//Background Color
			try
			{
				_glBackGroundColor = EditorGUILayout.ColorField(_glBackGroundColor, GUILayout.Width(80));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(120));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDOutlineColor), GUILayout.Width(120));//PSD Outline Color
			try
			{
				_psdOverlayColor = EditorGUILayout.ColorField(_psdOverlayColor, GUILayout.Width(80));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(120));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MeshOutlineColor), GUILayout.Width(120));//Mesh Outline Color
			try
			{
				_meshOverlayColor = EditorGUILayout.ColorField(_meshOverlayColor, GUILayout.Width(80));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();


			GUILayout.Space(margin);

			if (_step == RELOAD_STEP.Step1_SelectPSDSet)
			{
				GUILayout.Space(btnWidth + 4);
			}
			else
			{
				if (GUILayout.Button("< " + _editor.GetText(TEXT.DLG_PSD_Back), GUILayout.Width(btnWidth), GUILayout.Height(height)))//Back
				{
					//TODO
					//if (IsGUIUsable)
					//{
					//	MoveStep(false);
					//}
					MovePrev();
				}
			}

			if (_step == RELOAD_STEP.Step5_AtlasSetting)
			{
				if (_selectedPSDSet != null
					&& _psdLoader.IsFileLoaded
					&& _selectedPSDSet._targetMeshGroupID >= 0
					&& _selectedPSDSet._linkedTargetMeshGroup != null
					&& _psdLoader.IsCalculated
					&& _psdLoader.IsBaked
					)
				{
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height)))//Complete
					{
						//if (IsGUIUsable)
						//{
						//	StartBakedImageSave();
						//	//bool isTextureResult = SaveBakedImages();
						//	//if(!isTextureResult)
						//	//{
						//	//	EditorUtility.DisplayDialog("Texture Save Failed", "Texture Save Failed", "Okay");
						//	//}
						//	//else
						//	//{
						//	//	OnLoadComplete(true);
						//	//}
						//}

						_psdLoader.Step4_ConvertToAnyPortrait(OnConvertResult, _portrait, _selectedPSDSet, _meshTransform2PSDLayer);
					}
				}
				else
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height));//Complete

					GUI.backgroundColor = prevColor;
				}
				
			}
			else
			{
				bool isNextAvailable = false;
				if(_selectedPSDSet != null && _psdLoader.IsFileLoaded)
				{
					if(_step == RELOAD_STEP.Step1_SelectPSDSet)
					{
						isNextAvailable = true;
					}
					else
					{
						if(_selectedPSDSet._targetMeshGroupID >= 0 
							&& _selectedPSDSet._linkedTargetMeshGroup != null)
						{
							isNextAvailable = true;
						}
					}
				}
				
				if(isNextAvailable)
				{
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Next) + " >", GUILayout.Width(btnWidth), GUILayout.Height(height)))//Next
					{
						//TODO
						//if (IsGUIUsable)
						//{
						//	MoveStep(true);
						//}
						MoveNext();
					}
				}
				else
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Next) + " >", GUILayout.Width(btnWidth), GUILayout.Height(height));//Next

					GUI.backgroundColor = prevColor;
				}
			}

			GUILayout.Space(30);

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(btnWidth_Cancel), GUILayout.Height(height)))//Close
			{
				//TODO.
				//if (IsGUIUsable)
				//{
				//	//bool result = EditorUtility.DisplayDialog("Close", "Close PSD Load? (Data is Not Saved)", "Close", "Cancel");

				//	bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ClosePSDImport_Title),
				//												_editor.GetText(TEXT.ClosePSDImport_Body),
				//												_editor.GetText(TEXT.Close),
				//												_editor.GetText(TEXT.Cancel));

				//	if (result)
				//	{
				//		OnLoadComplete(false);
				//		//CloseDialog();
				//		_isRequestCloseDialog = true;//<<바로 Close하면 안된다.
				//	}
				//}
				if(IsGUIUsable)
				{
					bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ClosePSDImport_Title),
															_editor.GetText(TEXT.ClosePSDImport_Body),
															_editor.GetText(TEXT.Close),
															_editor.GetText(TEXT.Cancel));

					if (result)
					{
						_isRequestCloseDialog = true;
					}
				}
				
			}



			EditorGUILayout.EndHorizontal();
		}

		//Center
		private void GUI_Center(int width, int height, Rect centerRect)
		{
			
			
			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet://GUI가 없다.
					GUI_Center_1_SelectPSDSet(width, height, centerRect);
					break;

				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup://설정 + GUI (PSD / TextureData / MeshGroup)
					GUI_Center_2_FileLoadAndSelectMeshGroup(width, height, centerRect);
					break;

				case RELOAD_STEP.Step3_LinkLayerToTransform://레이어 + TF Hierarchy + GUI (Layer + Mesh) , 하단에 매핑 툴
					GUI_Center_3_LinkLayerToTransform(width, height, centerRect);
					break;

				case RELOAD_STEP.Step4_ModifyOffset://레이어 + GUI (Layer + Mesh) + 하단에 위치 보정 툴
					GUI_Center_4_ModifyOffset(width, height, centerRect);
					break;

				case RELOAD_STEP.Step5_AtlasSetting://GUI + Atlas 세팅
					GUI_Center_5_AtlasSetting(width, height, centerRect);
					break;
			}
		}


		private apGUIContentWrapper _guiContent_PSDSetIcon = null;

		// GUI Center 1 : Select PSD Set
		//-------------------------------------------------------------------------------
		private void GUI_Center_1_SelectPSDSet(int width, int height, Rect centerRect)
		{
			//PSD Set을 선택하는 화면
			//좌우 2등분
			//왼쪽 : PSD 리스트와 생성 버튼
			//오른쪽 : 기본 정보와 파일 경로 + 삭제
			int margin = 4;
			int width_Half = (width - margin) / 2;

			Texture2D imgPSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
			//GUIContent guiContent_PSDSetIcon = new GUIContent(imgPSDSet);//이전

			//변경 19.11.20
			if(_guiContent_PSDSetIcon == null)
			{
				_guiContent_PSDSetIcon = apGUIContentWrapper.Make(imgPSDSet);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Half, height), "");
			GUI.Box(new Rect(centerRect.xMin + width_Half + margin, centerRect.yMin, width_Half, height), "");

			//--------------------------------------
			// <1열 : PSD Set 리스트 또는 생성하기 >			
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half), GUILayout.Height(height));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectPSDSet), GUILayout.Width(width_Half));//Select PSD Set

			GUILayout.Space(5);
			_scroll_Step1_Left = EditorGUILayout.BeginScrollView(_scroll_Step1_Left, false, true, GUILayout.Width(width_Half), GUILayout.Height(height - 90));
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half - 20));

			int height_PSDSet = 20;
			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.MiddleLeft;
			guiStyle_Label.margin = GUI.skin.button.margin;

			GUIStyle guiStyle_TextBox = new GUIStyle(GUI.skin.textField);
			guiStyle_TextBox.alignment = TextAnchor.MiddleLeft;
			guiStyle_TextBox.margin = GUI.skin.button.margin;

			
			string str_NoFile = "< " + _editor.GetText(TEXT.DLG_PSD_NoFile) + " >";
			string str_NoPath = "< " + _editor.GetText(TEXT.DLG_PSD_NoPath) + " >";
			string str_InvalidFile = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidFile) + " >";
			string str_InvalidPath = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidPath) + " >";
			string str_Select = _editor.GetText(TEXT.DLG_PSD_Select);
			string str_Selected = _editor.GetText(TEXT.DLG_PSD_Selected);

			for (int i = 0; i < _portrait._bakedPsdSets.Count; i++)
			{
				apPSDSet psdSet = _portrait._bakedPsdSets[i];
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half - 20), GUILayout.Height(height_PSDSet));
				GUILayout.Space(5);
				string fileName = str_NoFile;
				string filePath = str_NoPath;
				if (!string.IsNullOrEmpty(psdSet._filePath))
				{
					FileInfo fi = new FileInfo(psdSet._filePath);
					if (fi.Exists)
					{
						fileName = fi.Name;
						filePath = fi.FullName;
					}
					else
					{
						fileName = str_InvalidFile;
						filePath = str_InvalidPath;
					}
				}
				EditorGUILayout.LabelField(_guiContent_PSDSetIcon.Content, guiStyle_Label, GUILayout.Width(height_PSDSet), GUILayout.Height(height_PSDSet));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(fileName, guiStyle_Label, GUILayout.Width(200), GUILayout.Height(height_PSDSet));
				EditorGUILayout.TextField(filePath, guiStyle_TextBox, GUILayout.Width(width_Half - (5 + height_PSDSet + 5 + 220 + 5 + 100 + 20)), GUILayout.Height(height_PSDSet));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton_2Side(str_Selected, str_Select, _selectedPSDSet == psdSet, true, 100, height_PSDSet))
				{
					if (psdSet != _selectedPSDSet)
					{
						//PSD Set을 교체한다
						SelectPSDSet(psdSet);
					}
					
				}
				EditorGUILayout.EndHorizontal();
			}
			

			GUILayout.Space(height + 200);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddNewPSDImportSet), GUILayout.Width(width_Half - 10), GUILayout.Height(35)))//"Add New PSD Import Set"
			{
				//_portrait._bakedPsdSets.Add(new apPSDSet())
				_editor.Controller.AddNewPSDSet(true);
			}

			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : 선택한 PSD Set 정보 + 파일 경로 설정 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half), GUILayout.Height(height));
			
			if (_selectedPSDSet != null)
			{
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectedPSDSet), GUILayout.Width(width_Half));//Selected PSD Set
				GUILayout.Space(5);
				//- 파일 이름 / 경로 (변경 가능)
				//- 로드하기 버튼
				//- 이미지 크기 (저장된)
				//- 레이어 리스트
				//- Bake 정보
				//- 데이터 삭제
				string selectedPSDFileName = "< " + _editor.GetText(TEXT.DLG_PSD_NotSelected) + " >";//"< Not Selected >";
				string selectedPSDPath = "< " + _editor.GetText(TEXT.DLG_PSD_NotSelected) + " >";// "< Not Selected >";

				if (_selectedPSDSet != null)
				{
					selectedPSDFileName = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >";//"< No PSD File >";
					selectedPSDPath = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >"; //"< No PSD File >";
					if (!string.IsNullOrEmpty(_selectedPSDSet._filePath))
					{
						FileInfo fi = new FileInfo(_selectedPSDSet._filePath);
						if (fi.Exists)
						{
							selectedPSDFileName = fi.Name;
							selectedPSDPath = fi.FullName;
						}
						else
						{
							selectedPSDFileName = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidFile) + " >"; //"< Invalid File >";
							selectedPSDPath = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidPath) + " >";//"< Invalid Path >";
						}
					}
				}
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileName) + " : " + selectedPSDFileName, GUILayout.Width(width_Half));//PSD File Name
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half), GUILayout.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.TextField(selectedPSDPath, guiStyle_TextBox, GUILayout.Width(width_Half - (90 + 25)));
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(20)))//Change
				{
					if (_selectedPSDSet != null)
					{
						//PSD 여는 Dialog
						try
						{
							//변경 21.3.1 : 이전 파일이 있는 디렉토리 경로를 가져오자
							string prevFileDir = "";
							System.IO.FileInfo fi = new FileInfo(_selectedPSDSet._filePath);
							if(fi.Exists)
							{
								prevFileDir = fi.Directory.FullName;
							}

							//string filePath = EditorUtility.OpenFilePanel("Open PSD File", _selectedPSDSet._filePath, "psd");//이전
							string filePath = EditorUtility.OpenFilePanel("Open PSD File", prevFileDir, "psd");//변경 21.3.1

							if (!string.IsNullOrEmpty(filePath))
							{
								//LoadPsdFile(filePath, _selectedPSDSet);
								_selectedPSDSet.SetPSDFilePath(filePath);

								if (_selectedPSDSet.IsValidPSDFile)
								{
									bool isResult = _psdLoader.Step1_LoadPSDFile(
										_selectedPSDSet._filePath, 
										(_selectedPSDSet._isLastBaked ? _selectedPSDSet._lastBakedAssetName : "")
										);
									if(isResult)
									{
										MakeRemappingList();
									}
								}
							}
						}
						catch (Exception ex)
						{
							Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				//선택한 PSD Set의 정보를 적자
				//int width_Label = 150;
				//int width_Value = width_Half - (width_Label + 15);
				//int height_Value = 18;

				int imageSizeWidth = 0;
				int imageSizeHeight = 0;

				bool lastBaked = false;
				int lastImageSizeWidth = 0;
				int lastImageSizeHeight = 0;

				int layerCount = 0;
				int lastLayerCount = 0;

				//int lastBakeOption_Width = 0;
				//int lastBakeOption_Height = 0;
				string lastBakeDstPath = "";
				int lastBakePadding = 0;
				int lastBakeScale = 0;
				
				string strBakeProperties = "";
				int bakeInfoBoxHeight = 50;
				Color infoBoxColor_PSDSet = new Color(0.5f, 1.5f, 0.8f, 1.0f);
				Color infoBoxColor_PSDFile = new Color(0.5f, 0.8f, 1.5f, 1.0f);

				

				GUILayout.Space(10);

				GUIStyle guiStyle_InfoBox = new GUIStyle(GUI.skin.box);
				guiStyle_InfoBox.alignment = TextAnchor.MiddleLeft;

				if (_selectedPSDSet != null)
				{
					lastBaked = _selectedPSDSet._isLastBaked;
					if (lastBaked)
					{
						lastImageSizeWidth = _selectedPSDSet._lastBaked_PSDImageWidth;
						lastImageSizeHeight = _selectedPSDSet._lastBaked_PSDImageHeight;

						lastLayerCount = _selectedPSDSet._layers.Count;

						//lastBakeOption_Width = _selectedPSDSet.GetBakeWidth();
						//lastBakeOption_Height = _selectedPSDSet.GetBakeHeight();
						lastBakeDstPath = _selectedPSDSet._bakeOption_DstFilePath;
						if (lastBakeDstPath.Length > 55)
						{
							//lastBakeDstPath = lastBakeDstPath.Substring(0, 55) + "..";
							//앞뒤로 끊자
							lastBakeDstPath = lastBakeDstPath.Substring(0, 25) + "  ..  " + lastBakeDstPath.Substring(lastBakeDstPath.Length - 30);

						}
						lastBakePadding = _selectedPSDSet._bakeOption_Padding;
						lastBakeScale = _selectedPSDSet._bakeScale100;
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + lastImageSizeWidth + " x " + lastImageSizeHeight);//Image Size
						sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_AssetPath) + " : " + lastBakeDstPath);//Asset Path
						sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Padding) + " : " + lastBakePadding);//Padding
						sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Scale) + " : " + lastBakeScale + "%");//Scale
						sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + lastLayerCount);//Layers

						strBakeProperties = sb.ToString();
						bakeInfoBoxHeight = 18 * 5;
					}
				}
				if(!lastBaked)
				{
					infoBoxColor_PSDSet = new Color(1.5f, 0.6f, 0.6f, 1.0f);
					strBakeProperties = "[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]";//ThereAreNoBakeData
					bakeInfoBoxHeight = 30;
				}

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakeSettings), GUILayout.Width(width_Half));//Bake Settings

				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = infoBoxColor_PSDSet;
				GUILayout.Box(strBakeProperties, guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(bakeInfoBoxHeight));
				GUI.backgroundColor = prevColor;

				if(_psdLoader.IsFileLoaded)
				{
					imageSizeWidth = _psdLoader.PSDImageWidth;
					imageSizeHeight = _psdLoader.PSDImageHeight;
					layerCount = _psdLoader.PSDLayerDataList.Count;
				}

				SetGUIVisible("PSD File Properties", _selectedPSDSet != null && _psdLoader.IsFileLoaded);
				bool isPSDProperties = IsDelayedGUIVisible("PSD File Properties");

				if (isPSDProperties)
				{
					GUILayout.Space(20);
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileProperties), GUILayout.Width(width_Half));//PSD File Properties
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + imageSizeWidth + " x " + imageSizeHeight);//ImageSize
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + layerCount);//Layers

					GUI.backgroundColor = infoBoxColor_PSDFile;
					GUILayout.Box(sb.ToString(), guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(50));
					GUI.backgroundColor = prevColor;
					
				}

				GUILayout.Space(20);
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_RemovePSDImportSet), GUILayout.Width(width_Half - 30)))//Remove PSD Import Set
				{
					//bool isResult = EditorUtility.DisplayDialog(	
																	//"Remove PSD Import Set", 
																	//"Are you sure you want to remove the selected PSD Import Set? You can not undo deleted data.", 
																	//"Remove", 
																	//"Cancel");
					bool isResult = EditorUtility.DisplayDialog(
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Title),
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Body),
															_editor.GetText(TEXT.Remove),
															_editor.GetText(TEXT.Cancel)
															);
					if(isResult)
					{
						_portrait._bakedPsdSets.Remove(_selectedPSDSet);
						_selectedPSDSet = null;
						//_selectedPSDSetLayer = null;
						_selectedPSDLayerData = null;
						_selectedPSDBakeData = null;
						_selectedTextureData = null;
						_psdLoader.Clear();

						_isNeedBakeCheck = true;
						_isBakeWarning = false;
						_bakeWarningMsg = "";
						_loadKey_Calculated = null;
						_loadKey_Bake = null;
					}
				}
			}
			else
			{
				GUILayout.Space((height / 2) - 10);
				GUIStyle guiStyle_LabelCenter = new GUIStyle(GUI.skin.label);
				guiStyle_LabelCenter.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectPSDSet), guiStyle_LabelCenter, GUILayout.Width(width_Half));//Select PSD Import Set
				
			}


			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
			
		}


		private apGUIContentWrapper _guiContent_Delete = null;


		private void GUI_Center_2_FileLoadAndSelectMeshGroup(int width, int height, Rect centerRect)
		{
			//PSD의 설정, MeshGroup과 TextureData를 선택하는 화면
			//좌우 2개. 오른쪽이 크다
			//왼쪽 : 선택된 PSD Set의 설정과 연결된 MeshGroup, TextureData
			//오른쪽 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI
			int margin = 4;
			int width_Left = 400;
			int width_Right = (width - (width_Left + margin));
			//int height_RightBottom = 40;
			int height_RightBottom = 70;
			int height_RightGUI = height - (height_RightBottom + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");


			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + height_RightGUI + margin, width_Right, height_RightBottom), "");

			GUILayout.Space(5);

			//--------------------------------------
			// <1열 : PSD Set 설정 + MeshGroup, TextureData 연결하기 >

			//- MeshGroup 연결
			//- 덮어쓰기할 TextureData 연결
			//- 이전 크기 vs 현재 크기와 비교 (Box)
			int width_Icon = 40;
			int width_LeftInScroll = width_Left - (24 + 5);
			int width_ItemValue = width_Left - (width_Icon + 25 + 10);
			//int width_ItemValueInScroll = width_LeftInScroll - (width_Icon + 25 + 10);

			GUIStyle guiStyle_SmallBtn = new GUIStyle(GUI.skin.button);
			guiStyle_SmallBtn.margin = GUI.skin.box.margin;

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_PMBtn = new GUIStyle(GUI.skin.button);
			guiStyle_PMBtn.margin = GUI.skin.textField.margin;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDImportSetSettings));//PSD Import Set Settings
			GUILayout.Space(5);

			//1. MeshGroup
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(50));//H1
			GUILayout.Space(5);
			//아이콘
			EditorGUILayout.LabelField(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(width_Icon), GUILayout.Height(32));
			GUILayout.Space(5);
			//오른쪽 항목 (이름)
			EditorGUILayout.BeginVertical(GUILayout.Width(width_ItemValue), GUILayout.Height(50));//V3
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_TargetMeshGroup));//Target MeshGroup
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_ItemValue), GUILayout.Height(18));//H2
			string targetMeshGroupName = _editor.GetText(TEXT.DLG_PSD_NoMeshGroup);//No Mesh Group
			if (_selectedPSDSet._linkedTargetMeshGroup != null)
			{
				targetMeshGroupName = _selectedPSDSet._linkedTargetMeshGroup._name;
				if (targetMeshGroupName.Length > 25)
				{
					targetMeshGroupName = targetMeshGroupName.Substring(0, 25) + "..";
				}
				GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.5f, prevColor.b * 1.0f, 1.0f);
			}
			else
			{
				GUI.backgroundColor = new Color(prevColor.r * 1.5f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
			}
			GUILayout.Box(targetMeshGroupName, guiStyle_Box, GUILayout.Width(width_ItemValue - (90 - 13)), GUILayout.Height(18));
			GUI.backgroundColor = prevColor;

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), guiStyle_SmallBtn, GUILayout.Width(85), GUILayout.Height(18)))//Change
			{
				_loadKey_SelectMeshGroup = apDialog_SelectLinkedMeshGroup.ShowDialog(_editor, null, OnMeshGroupSelected);
			}
			EditorGUILayout.EndHorizontal();//H2
			EditorGUILayout.EndVertical();//V3

			EditorGUILayout.EndHorizontal();//H1


			//2. TextureData
			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakedImages));//Baked Images
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddImage), GUILayout.Width(width_Left - 15), GUILayout.Height(20)))//Add Image
			{
				_loadKey_SelectTextureData = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddImagesAuto), GUILayout.Width(width_Left - 15), GUILayout.Height(30)))//Add Image Automatically
			{
				//_loadKey_SelectTextureData = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
				//자동으로 TextureData를 추가한다.
				if (_selectedPSDSet._linkedTargetMeshGroup != null)
				{
					AutoSelectTextureData(_selectedPSDSet);
				}
			}

			//GUIContent guiContent_TextureDataIcon = new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image));
			Texture2D img_TextureData = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);

			//이전
			//GUIContent guiContent_Delete = new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey));

			//변경
			if (_guiContent_Delete == null)
			{
				_guiContent_Delete = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey));
			}

			GUILayout.Space(5);
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 161 + 10, width_Left, 150), "");
			GUI.backgroundColor = prevColor;
			_scroll_Step2_Left = EditorGUILayout.BeginScrollView(_scroll_Step2_Left, false, true, GUILayout.Width(width_Left - 5), GUILayout.Height(150));

			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));//V1

			bool isRemoveTexData = false;
			int iRemoveTexData = -1;

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			GUILayout.Space(5);
			//Images
			GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_PSD_Images), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)), guiStyle_None, GUILayout.Width(width_LeftInScroll - 10), GUILayout.Height(20));
			GUILayout.Space(4);

			for (int iTex = 0; iTex < _selectedPSDSet._targetTextureDataList.Count; iTex++)
			{
				apPSDSet.TextureDataSet texDataSet = _selectedPSDSet._targetTextureDataList[iTex];

				GUIStyle curGUIStyle = guiStyle_None;

				if (_selectedTextureData == texDataSet)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}
					//GUI.Box(new Rect(lastRect.x, lastRect.y + 19, width_Left, 22), "");
					GUI.Box(new Rect(lastRect.x, lastRect.y + 2, width_Left, 24), "");
					GUI.backgroundColor = prevColor;
					curGUIStyle = guiStyle_Selected;
				}

				string texDataName = "";
				if (texDataSet._linkedTextureData != null)
				{
					texDataName = texDataSet._linkedTextureData._name;
					if (texDataName.Length > 25)
					{
						texDataName = texDataName.Substring(0, 25) + "..";
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(20));
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(" " + texDataName, img_TextureData), curGUIStyle, GUILayout.Width(width_LeftInScroll - (15 + 30)), GUILayout.Height(20)))
				{
					_selectedTextureData = texDataSet;
				}
				GUILayout.Space(5);

				if (GUILayout.Button(_guiContent_Delete.Content, guiStyle_None, GUILayout.Width(20), GUILayout.Height(20)))
				{
					//TODO
					//bool isResult = EditorUtility.DisplayDialog("Detach Image", "Do you want to detach the Image?", "Detach", "Cancel");

					bool isResult = EditorUtility.DisplayDialog(
						_editor.GetText(TEXT.DLG_PSD_DetachImage_Title),
						_editor.GetText(TEXT.DLG_PSD_DetachImage_Body),
						_editor.GetText(TEXT.DLG_PSD_Detach),
						_editor.GetText(TEXT.DLG_Cancel));


					if (isResult)
					{
						isRemoveTexData = true;
						iRemoveTexData = iTex;
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(4);
			}
			if (isRemoveTexData)
			{
				if (_selectedPSDSet._targetTextureDataList[iRemoveTexData] == _selectedTextureData)
				{
					_selectedTextureData = null;
				}
				_selectedPSDSet._targetTextureDataList.RemoveAt(iRemoveTexData);

			}

			GUILayout.Space(200);

			EditorGUILayout.EndVertical();//V1

			EditorGUILayout.EndScrollView();

			//3. PSD Bake 설정값들 (여기서 수정할 수 있는건 Bake된 비율. 위치를 맞추기 위해)
			GUILayout.Space(10);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakeSettingsForReimpot));//"Atlas Bake Settings for Reimporting"
			GUILayout.Space(5);
			if (!_selectedPSDSet._isLastBaked)
			{
				GUI.backgroundColor = new Color(1.5f, 0.6f, 0.6f, 1.0f);
				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]", guiStyle_Box, GUILayout.Width(width_Left - 20), GUILayout.Height(30));//There are no bake data
				GUI.backgroundColor = prevColor;
				GUILayout.Space(5);
			}
			int width_InfoLabel = 100;
			int width_InfoValue = width_Left - (width_InfoLabel + 20);
			int height_Info = 18;

			if (_selectedPSDSet._isLastBaked)
			{
				//Width
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(width_InfoLabel));//Image Width
				EditorGUILayout.LabelField(_selectedPSDSet._lastBaked_PSDImageWidth.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				//Height
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(width_InfoLabel));//Image height
				EditorGUILayout.LabelField(_selectedPSDSet._lastBaked_PSDImageHeight.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();
			}
			GUILayout.Space(5);
			//Bake Scale
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakeScale) + " (%) : ", GUILayout.Width(width_InfoLabel));//Bake Scale
			int nextBakeScale100 = EditorGUILayout.DelayedIntField(_selectedPSDSet._next_meshGroupScaleX100, GUILayout.Width(width_InfoValue - (28 * 6 + 40)));
			if (nextBakeScale100 != _selectedPSDSet._next_meshGroupScaleX100)
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(nextBakeScale100, 5, 1000);
			}
			GUILayout.Space(10);
			if (GUILayout.Button("-5", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 5, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("-2", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 2, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}

			if (GUILayout.Button("-1", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 1, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			GUILayout.Space(10);
			if (GUILayout.Button("+1", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 1, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("+2", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 2, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("+5", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 5, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			////추가 21.3.6
			////만약 BakeScale을 변경할 필요가 없는데 Canvas 사이즈가 변경되었다면 Offset을 수정하면 안된다.
			////이걸 명시해주자
			//if (_selectedPSDSet._lastBaked_PSDImageWidth != _psdLoader.PSDImageWidth &&
			//	_selectedPSDSet._lastBaked_PSDImageHeight != _psdLoader.PSDImageHeight)
			//{
			//	GUILayout.Space(5);
			//	//이미지 사이즈가 다르다.
			//	string strWarning = "Last PSD Size : " + _selectedPSDSet._lastBaked_PSDImageWidth + "x" + _selectedPSDSet._lastBaked_PSDImageHeight + "\n";
			//	strWarning += "Current PSD Size : " + _psdLoader.PSDImageWidth + "x" + _psdLoader.PSDImageHeight;
			//	GUILayout.Box(strWarning, apGUIStyleWrapper.I.Box_MiddleCenter, GUILayout.Height(50));
			//}




			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI >
			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI);
			UpdateAndDrawGUIBase(guiRect, new Vector2(6, 0.3f));
			
			//GL 렌더링
			//1. PSD
			
			if(_isRender_MeshGroup && _selectedPSDSet._linkedTargetMeshGroup != null)
			{
				DrawMeshGroup(_selectedPSDSet._linkedTargetMeshGroup);
			}
			if(_isRender_TextureData && _selectedTextureData != null)
			{
				DrawTextureData(_selectedTextureData._linkedTextureData, true);
			}
			if(_renderMode_PSD != RENDER_MODE.Hide)
			{
				DrawPSD(_renderMode_PSD == RENDER_MODE.Outline, null, _selectedPSDSet._nextBakeCenterOffsetDelta_X, _selectedPSDSet._nextBakeCenterOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100);
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));
			GUILayout.Space(height_RightGUI + margin);
			//오른쪽 하단
			//렌더 필터 버튼들과 PSD 위치 이동
			//렌더 여부 버튼

			int btnSize = height_RightBottom - (24+8);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(220), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingType), GUILayout.Width(150));//Rendering Type
			EditorGUILayout.BeginHorizontal(GUILayout.Width(220), GUILayout.Height(height_RightBottom - 24));

			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), _isRender_TextureData, true, 40, btnSize))
			{
				_isRender_TextureData = !_isRender_TextureData;
			}
			if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), _isRender_MeshGroup, true, 40, btnSize))
			{
				_isRender_MeshGroup = !_isRender_MeshGroup;
			}

			Texture2D img_PSDSet = null;
			bool isRenderPSD = _renderMode_PSD != RENDER_MODE.Hide;
			if(_renderMode_PSD == RENDER_MODE.Outline)
			{
				img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetOutline);
			}
			else
			{
				img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
			}

			if(apEditorUtil.ToggledButton_2Side(img_PSDSet, isRenderPSD, true, 40, btnSize))
			{
				//_isRender_PSD = !_isRender_PSD;
				switch (_renderMode_PSD)
				{
					case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
					case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
					case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
				}
			}
			//if(apEditorUtil.ToggledButton_2Side("Translucent", "Opaque", _isRender_PSDAlpha, true, 90, btnSize))
			//{
			//	_isRender_PSDAlpha = !_isRender_PSDAlpha;
			//}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Offset), GUILayout.Width(120));//Offset
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(120), GUILayout.Height(20));
			EditorGUILayout.LabelField("X", GUILayout.Width(15));
			float nextOffsetPosX = EditorGUILayout.FloatField(_selectedPSDSet._nextBakeCenterOffsetDelta_X, GUILayout.Width(40));
			if(nextOffsetPosX != _selectedPSDSet._nextBakeCenterOffsetDelta_X)
			{
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = nextOffsetPosX;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(15));
			float nextOffsetPosY = EditorGUILayout.FloatField(_selectedPSDSet._nextBakeCenterOffsetDelta_Y, GUILayout.Width(40));
			if(nextOffsetPosY != _selectedPSDSet._nextBakeCenterOffsetDelta_Y)
			{
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = nextOffsetPosY;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			//-------------------------------------------------------------------
			GUILayout.Space(10);

			//X,Y 제어 버튼

			//위치를 쉽게 제어하기 위해서 버튼으로 만들자
			//				Y:(+1, +10)
			//X:(-10, -1)					X:(+1, +10)
			//				Y:(-1, -10)
			int width_contBtn = 40;
			int height_contBtn = 22;
			int width_contBtnArea = width_contBtn * 2 + 4;
			int margin_contBtn = ((height_RightBottom - 4) - height_contBtn) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-X 버튼
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X - 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X - 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			//+-Y
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+Y 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y + 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y + 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(margin_contBtn / 2 - 2);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-Y 버튼
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y - 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y - 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+X 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				//X + 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X + 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{ 
				// X + 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X + 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();



			//-------------------------------------------------------------------
			

			EditorGUILayout.EndHorizontal();



			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}

		private object _loadKey_SelectMeshGroup = null;
		private void OnMeshGroupSelected(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if(!isSuccess)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}
			if(_loadKey_SelectMeshGroup != loadKey || _loadKey_SelectMeshGroup == null || loadKey == null)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}

			if(_selectedPSDSet != null)
			{
				_selectedPSDSet._linkedTargetMeshGroup = meshGroup;
				if(_selectedPSDSet._linkedTargetMeshGroup != null)
				{
					_selectedPSDSet._targetMeshGroupID = meshGroup._uniqueID;
				}
				else
				{
					_selectedPSDSet._targetMeshGroupID = -1;
				}
			}
			_loadKey_SelectMeshGroup = null;
		}

		private object _loadKey_SelectTextureData = null;
		private void OnTextureDataSelected(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if(!isSuccess)
			{
				_loadKey_SelectTextureData = null;
				return;
			}
			if(_loadKey_SelectTextureData != loadKey || _loadKey_SelectTextureData == null || loadKey == null)
			{
				_loadKey_SelectTextureData = null;
				return;
			}
			if(_selectedPSDSet != null && resultTextureData != null)
			{
				//등록되지 않은 데이터라면
				if(!_selectedPSDSet._targetTextureDataList.Exists(delegate(apPSDSet.TextureDataSet a)
				{
					return a._textureDataID == resultTextureData._uniqueID;
				}))
				{
					apPSDSet.TextureDataSet newSet = new apPSDSet.TextureDataSet();
					newSet._textureDataID = resultTextureData._uniqueID;
					newSet._linkedTextureData = resultTextureData;
					_selectedPSDSet._targetTextureDataList.Add(newSet);
				}
			}
			_loadKey_SelectTextureData = null;
		}

		private void AutoSelectTextureData(apPSDSet psdSet)
		{
			if(psdSet == null || psdSet._linkedTargetMeshGroup == null)
			{
				return;
			}
			List<apTextureData> result = new List<apTextureData>();
			FindTextureDataRecursive(psdSet._linkedTargetMeshGroup, result);

			result.Sort(delegate(apTextureData a, apTextureData b)
			{
				return string.Compare(a._name, b._name);
			});

			for (int i = 0; i < result.Count; i++)
			{
				if(!psdSet._targetTextureDataList.Exists(delegate(apPSDSet.TextureDataSet a)
				{
					return a._textureDataID == result[i]._uniqueID;
				}))
				{
					apPSDSet.TextureDataSet newTexSet = new apPSDSet.TextureDataSet();
					newTexSet._textureDataID = result[i]._uniqueID;
					newTexSet._linkedTextureData = result[i];
					psdSet._targetTextureDataList.Add(newTexSet);
				}

			}
		}
		private void FindTextureDataRecursive(apMeshGroup meshGroup, List<apTextureData> resultList)
		{
			for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			{
				apMesh mesh = meshGroup._childMeshTransforms[i]._mesh;
				if(mesh != null)
				{
					if(mesh._textureData_Linked != null && !resultList.Contains(mesh._textureData_Linked))
					{
						resultList.Add(mesh._textureData_Linked);
					}
				}
			}
			if(meshGroup._childMeshGroupTransforms != null)
			{
				for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
				{
					apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransforms[i]._meshGroup;
					if(childMeshGroup != null && childMeshGroup != meshGroup)
					{
						FindTextureDataRecursive(childMeshGroup, resultList);
					}
				}
			}
			
		}



		private void GUI_Center_3_LinkLayerToTransform(int width, int height, Rect centerRect)
		{
			//레이어, 트랜스폼을 연결하고, 선택된 레이어를 미리볼 수 있는 화면
			//1열 : 레이어 리스트
			//2열 : Transform 리스트 (Hierarchy)
			//3열 (넓음) : 선택된 Layer와 연결된 Mesh를 볼 수 있는 GUI
			//1, 2열 하단에는 매핑 도구가 있다.
			int margin = 4;
			int width_1 = 350;
			int width_2 = 300;
			int width_3 = (width - (width_1 + margin + width_2 + margin));
			int height_LeftLower = 36;
			int height_LeftUpper = height - (margin + height_LeftLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height_LeftUpper), "");
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 29, width_1, height_LeftUpper - 29), "");//리스트1 박스
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height_LeftUpper), "");


			if(_isLinkLayerToTransform)
			{
				GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.4f, prevColor.b * 0.7f, 1.0f);
			}
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin + 29, width_2, height_LeftUpper - 29), "");//리스트2 박스
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + height_LeftUpper + margin, width_1 + margin + width_2, height_LeftLower), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			GUI.backgroundColor = prevColor;
			
			
			
			//--------------------------------------
			// <1열 + 2열 + 1, 2열 하단>
			int height_ListHeight = 26;

			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			EditorGUILayout.BeginVertical(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height));
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftUpper));
			//GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper));
			// <1열 : 레이어 리스트 >
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);
			_scroll_Step3_Line1 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line1, false, true, GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper - 30));

			int width_Line1InScroll = (width_1) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line1InScroll));
			GUILayout.Space(5);
			//레이어 리스트
			apPSDLayerData curPSDLayer = null;
			int iList = 0;

			Texture2D imgBakeEnabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeEnabled);
			Texture2D imgBakeDisabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeDisabled);

			for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				//int level = curPSDLayer._hierarchyLevel;

				if (_selectedPSDLayerData == curPSDLayer)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					if (iList == 0)
					{
						GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
					}
					else
					{
						GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
					}

					GUI.backgroundColor = prevColor;
				}
				else if(_isLinkGUIColoredList)
				{
					//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
					//연결 안되면 안보여줌
					if (curPSDLayer._isBakable && curPSDLayer._isRemapSelected)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = curPSDLayer._randomGUIColor_Pro;
						}
						else
						{
							GUI.backgroundColor = curPSDLayer._randomGUIColor;
						}
						

						if (iList == 0)
						{
							GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						}
						else
						{
							GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						}

						GUI.backgroundColor = prevColor;
					}
				}
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line1InScroll), GUILayout.Height(height_ListHeight));
				
				int prefixMargin = 0;
				GUILayout.Space(5);
				
				prefixMargin = 5;
				if(curPSDLayer._isImageLayer)
				{
					if(curPSDLayer._isClipping)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}

				GUIStyle curGUIStyle = guiStyle_Btn;
				if (!curPSDLayer._isBakable)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (curPSDLayer == _selectedPSDLayerData)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}

				int btnWidth = width_Line1InScroll - (prefixMargin + 120);
				
				if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					_selectedPSDLayerData = curPSDLayer;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;
				}
				if(apEditorUtil.ToggledButton_2Side(imgBakeEnabled, imgBakeDisabled, curPSDLayer._isBakable, true, 20, height_ListHeight - 6))
				{
					curPSDLayer._isBakable = !curPSDLayer._isBakable;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;

					if(!curPSDLayer._isBakable)
					{
						//Bake를 끄는 경우 : 연결을 해제한다.
						UnlinkPSDLayer(curPSDLayer);
					}
				}

				bool isRemapSelected = false;
				bool isAvailable = true;
				string strRemapName = _editor.GetText(TEXT.DLG_PSD_NotSelected);//Not Selected
				if (curPSDLayer._isBakable)
				{
					if (curPSDLayer._isRemapSelected)
					{
						if (curPSDLayer._remap_MeshTransform != null)
						{
							strRemapName = curPSDLayer._remap_MeshTransform._nickName;
							isRemapSelected = true;
						}
						else if (curPSDLayer._remap_MeshGroupTransform != null)
						{
							strRemapName = curPSDLayer._remap_MeshGroupTransform._nickName;
							isRemapSelected = true;
						}
					}
				}
				else
				{
					isAvailable = false;
					strRemapName = _editor.GetText(TEXT.DLG_PSD_NotBakeable);//Not Bakeable
				}

				//만약 선택 중이라면
				if(_isLinkLayerToTransform)
				{
					if(curPSDLayer == _linkSrcLayerData)
					{
						isRemapSelected = true;
						strRemapName = ">>>";
						isAvailable = true;
					}
					else
					{
						isAvailable = false;
					}
				}

				if(apEditorUtil.ToggledButton_2Side(strRemapName, strRemapName, isRemapSelected, isAvailable, 85, height_ListHeight - 6))
				{
					//TODO.
					if(!_isLinkLayerToTransform)
					{
						//선택모드로 변경
						_linkSrcLayerData = curPSDLayer;
						_isLinkLayerToTransform = true;
						_selectedPSDLayerData = curPSDLayer;
					}
					else
					{
						//선택모드에서 해제
						_linkSrcLayerData = null;
						_isLinkLayerToTransform = false;
					}

					
					
				}
				

				EditorGUILayout.EndHorizontal();

				iList++;
			}
			

			GUILayout.Space(height_LeftUpper);
			EditorGUILayout.EndVertical();



			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper));
			// <2열 : Transform Hierarchy >
			GUILayout.Space(5);
			string meshGroupName = _selectedPSDSet._linkedTargetMeshGroup.name;
			if(meshGroupName.Length > 30)
			{
				meshGroupName = meshGroupName.Substring(0, 30) + "..";
			}
			EditorGUILayout.LabelField("  " + meshGroupName);
			GUILayout.Space(5);

			_scroll_Step3_Line2 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line2, false, true, GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper - 30));

			int width_Line2InScroll = (width_2) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));
			GUILayout.Space(5);
			//Transform 리스트

			Texture2D imgMeshTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			Texture2D imgMeshGroupTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);

			//apRenderUnit curRenderUnit = null;
			//for (int i = _selectedPSDSet._linkedTargetMeshGroup._renderUnits_All.Count - 1; i >= 0; i--)
			TargetTransformData curTransformData = null;
			iList = 0;
			for (int i = _targetTransformList.Count - 1; i >= 0; i--)
			{
				curTransformData = _targetTransformList[i];
				
				if(curTransformData._meshGroupTransform != null && 
					curTransformData._meshGroupTransform == _selectedPSDSet._linkedTargetMeshGroup._rootMeshGroupTransform)
				{
					continue;
				}

				bool isLinked = false;
				if (_selectedPSDLayerData != null &&
						_selectedPSDLayerData._isRemapSelected &&
						(
							(_selectedPSDLayerData._remap_MeshTransform != null && _selectedPSDLayerData._remap_MeshTransform == curTransformData._meshTransform)
							||
							(_selectedPSDLayerData._remap_MeshGroupTransform != null && _selectedPSDLayerData._remap_MeshGroupTransform == curTransformData._meshGroupTransform)
						)
					)
				{
					isLinked = true;
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					if (iList == 0)
					{
						GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
					}
					else
					{
						GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
					}

					GUI.backgroundColor = prevColor;
				}

				if (_isLinkGUIColoredList && !isLinked)
				{
					//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
					apPSDLayerData linkedLayerData = null;
					if(curTransformData._meshTransform != null)
					{
						if (_meshTransform2PSDLayer.ContainsKey(curTransformData._meshTransform))
						{
							linkedLayerData = _meshTransform2PSDLayer[curTransformData._meshTransform];
						}
					}
					else if(curTransformData._meshGroupTransform != null)
					{
						if(_meshGroupTransform2PSDLayer.ContainsKey(curTransformData._meshGroupTransform))
						{
							linkedLayerData = _meshGroupTransform2PSDLayer[curTransformData._meshGroupTransform];
						}
					}
					if(linkedLayerData != null && linkedLayerData._isBakable)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = linkedLayerData._randomGUIColor_Pro;
						}
						else
						{
							GUI.backgroundColor = linkedLayerData._randomGUIColor;
						}
						

						if (iList == 0)
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						}
						else
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						}

						GUI.backgroundColor = prevColor;
					}

				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(height_ListHeight));
				GUILayout.Space(5);
				int prefixMargin = 5;
				bool isRemap = false;
				string remapPSDLayerName = _editor.GetText(TEXT.DLG_PSD_Select);//Select

				if(curTransformData._meshTransform != null)
				{
					if(curTransformData._meshTransform._isClipping_Child)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(imgMeshTF), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;

					//연결된 PSD Layer를 체크하자
					if(_meshTransform2PSDLayer.ContainsKey(curTransformData._meshTransform))
					{
						isRemap = true;
						remapPSDLayerName = _meshTransform2PSDLayer[curTransformData._meshTransform]._name;
					}
					
				}
				else if(curTransformData._meshGroupTransform != null)
				{
					EditorGUILayout.LabelField(new GUIContent(imgMeshGroupTF), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;

					//연결된 PSD Layer를 체크하자
					if(_meshGroupTransform2PSDLayer.ContainsKey(curTransformData._meshGroupTransform))
					{
						isRemap = true;
						remapPSDLayerName = _meshGroupTransform2PSDLayer[curTransformData._meshGroupTransform]._name;
					}
				}
				
				GUIStyle curGUIStyle = guiStyle_Btn;
				if (curTransformData._isMeshTransform && !curTransformData._isValidMesh)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (isLinked)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}


				int btnWidth = width_Line2InScroll - (prefixMargin + 120);
				if (GUILayout.Button("  " + curTransformData.Name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					//_selectedPSDLayerData = curPSDLayer;
					//여기서..는 선택하지 않는다.
				}

				
				if ((_isLinkLayerToTransform && _linkSrcLayerData != null && _linkSrcLayerData._isImageLayer == curTransformData._isMeshTransform) 
					|| isRemap)
				{
					if(apEditorUtil.ToggledButton(remapPSDLayerName, !_isLinkLayerToTransform, 110, height_ListHeight - 6))
					{
						if (_isLinkLayerToTransform)
						{
							LinkPSDLayerAndTransform(_linkSrcLayerData, curTransformData);
							_isLinkLayerToTransform = false;
							_linkSrcLayerData = null;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				iList++;
			}
			
			


			GUILayout.Space(height_LeftUpper);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(margin);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftLower));
			// <1+2열 하단 : 매핑 툴>
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AutoMapping), GUILayout.Width(120), GUILayout.Height(height_LeftLower - 8)))//Auto Mapping
			{
				LinkTool_AutoMapping();
			}
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_EnableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Enable All
			{
				LinkTool_EnableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_DisableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Disable All
			{
				LinkTool_DisableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Reset), GUILayout.Width(110), GUILayout.Height(height_LeftLower - 8)))//Reset
			{
				LinkTool_Reset();
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_LinkView), _isLinkGUIColoredList, true, 40, height_LeftLower - 8))
			{
				_isLinkGUIColoredList = !_isLinkGUIColoredList;
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Overlay), _isLinkOverlayColorRender, true, 40, height_LeftLower - 8))
			{
				_isLinkOverlayColorRender = !_isLinkOverlayColorRender;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : 레이어, Mesh GUI >
			Rect guiRect = new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(15.0f, 0.3f));

			float meshScale = (float)_selectedPSDSet._next_meshGroupScaleX100 / (float)_selectedPSDSet._prev_bakeScale100;
			//float meshScale = 100.0f / (float)_selectedPSDSet._prev_bakeScale100;

			//DrawPSD(false, null, 0, 0, 100, false);
			if(_selectedPSDLayerData != null)
			{
				if(_selectedPSDLayerData._image != null)
				{
					DrawPSDLayer(_selectedPSDLayerData, 0, 0, _selectedPSDSet._next_meshGroupScaleX100, _isLinkOverlayColorRender);
				}
				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null)
				{
					if(_isLinkOverlayColorRender)
					{
						//DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, true, _meshOverlayColor);
						DrawMeshToneColor(_selectedPSDLayerData._remap_MeshTransform._mesh, false, meshScale);
					}
					else
					{
						DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, false, meshScale);
					}
					
				}
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}


		


		private void GUI_Center_4_ModifyOffset(int width, int height, Rect centerRect)
		{
			//레이어를 선택하고 새로운 이미지의 위치를 조정하는 과정
			//좌우 2개. 오른쪽이 크며, 오른쪽은 하단에 툴이 있다.
			//왼쪽 : 레이어 리스트
			//오른쪽 : 레이어 + 메시가 동시에 출력 (또는 전환되어 출력)되는 GUI와 하단의 위치 조정 툴
			int margin = 4;
			int width_Left = 200;
			int width_Right = (width - (width_Left + margin));
			int height_RightLower = 70;
			int height_RightUpper = height - (margin + height_RightLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + margin + height_RightUpper, width_Right, height_RightLower), "");

			//--------------------------------------
			// <1열 : 레이어 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);
			_scroll_Step4_Left = EditorGUILayout.BeginScrollView(_scroll_Step4_Left, false, true, GUILayout.Width(width_Left), GUILayout.Height(height - 30));
			int width_LeftInScroll = (width_Left) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));
			GUILayout.Space(5);
			//PSD 레이어를 출력한다. (Bake안되는것은 나오지 않는다)
			apPSDLayerData curPSDLayer = null;
			int iList = 0;
			int height_ListHeight = 26;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;



			for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				//int level = curPSDLayer._hierarchyLevel;


				if (_selectedPSDLayerData == curPSDLayer)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					}

					if (iList == 0)
					{
						GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
					}
					else
					{
						GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
					}

					GUI.backgroundColor = prevColor;
				}
				
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(height_ListHeight));
				
				int prefixMargin = 0;
				GUILayout.Space(5);
				
				prefixMargin = 5;
				if(curPSDLayer._isImageLayer)
				{
					if(curPSDLayer._isClipping)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}

				GUIStyle curGUIStyle = guiStyle_Btn;
				if (!curPSDLayer._isBakable)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (curPSDLayer == _selectedPSDLayerData)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}

				int btnWidth = width_LeftInScroll - (prefixMargin + 20);
				
				if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					_selectedPSDLayerData = curPSDLayer;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;

					apEditorUtil.ReleaseGUIFocus();

				}
				

				EditorGUILayout.EndHorizontal();
				


				iList++;
			}


			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : 선택한 레이어의 GUI와 위치 조절 툴 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));

			

			// <2열 상단 : 레이어와 메시의 GUI>
			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper);
			UpdateAndDrawGUIBase(guiRect, new Vector2(2.4f, 0.2f));
			if(_selectedPSDLayerData != null)
			{
				if (!_isRenderMesh2PSD)
				{
					//PSD -> [Mesh]
					if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
					{	
						DrawPSDLayer(_selectedPSDLayerData, _selectedPSDLayerData._remapPosOffsetDelta_X, _selectedPSDLayerData._remapPosOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100, _renderMode_PSD == RENDER_MODE.Outline);
					}
				}
				
				
				float meshScale = (float)_selectedPSDSet._next_meshGroupScaleX100 / (float)_selectedPSDSet._prev_bakeScale100;
				//float meshScale = 100.0f / (float)_selectedPSDSet._prev_bakeScale100;

				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null
					&& _renderMode_Mesh != RENDER_MODE.Hide)
				{
					if(_renderMode_Mesh == RENDER_MODE.Normal)
					{
						//Normal
						DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, false, meshScale);
					}
					else
					{
						//Outline
						DrawMeshToneColor(_selectedPSDLayerData._remap_MeshTransform._mesh, false, meshScale);
					}
				}

				if (_isRenderMesh2PSD)
				{
					//[Mesh] -> PSD
					if (_selectedPSDLayerData._image != null)
					{
						if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
						{
							DrawPSDLayer(_selectedPSDLayerData, _selectedPSDLayerData._remapPosOffsetDelta_X, _selectedPSDLayerData._remapPosOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100, _renderMode_PSD == RENDER_MODE.Outline);
						}
					}
				}
				
				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null)
				{
					DrawMeshEdgeOnly(_selectedPSDLayerData._remap_MeshTransform._mesh, meshScale);
				}
				
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightUpper));
			GUILayout.Space(height_RightUpper);
			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);

			//EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			// <2열 하단 : 위치 조정 툴> 
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(130), GUILayout.Height(height_RightLower));
			//렌더링 순서와 렌더링 방식
			// Label
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingNOrder), GUILayout.Width(130));//Rendering & Order
			EditorGUILayout.BeginHorizontal(GUILayout.Width(130), GUILayout.Height(height_RightLower - 24));
			
			//- 1번 렌더링 모드, Switch, 2번 렌더링 모드 (기본은 Mesh 위에 PSD Layer), 

			Texture2D imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			Texture2D imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
			
			if(_renderMode_Mesh == RENDER_MODE.Outline)
			{
				imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.PSD_MeshOutline);
			}
			if(_renderMode_PSD == RENDER_MODE.Outline)
			{
				imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetOutline);
			}

			Texture2D imgBtn_Mod1 = (_isRenderMesh2PSD ? imgBtn_Mesh : imgBtn_PSD);
			Texture2D imgBtn_Mod2 = (_isRenderMesh2PSD ? imgBtn_PSD : imgBtn_Mesh);
			bool isMod1Selected = (_isRenderMesh2PSD ? (_renderMode_Mesh != RENDER_MODE.Hide) : (_renderMode_PSD != RENDER_MODE.Hide));
			bool isMod2Selected = (_isRenderMesh2PSD ? (_renderMode_PSD != RENDER_MODE.Hide) : (_renderMode_Mesh != RENDER_MODE.Hide));
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod1, isMod1Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Switch)), GUILayout.Width(32), GUILayout.Height(height_RightLower - (24 + 8))))
			{
				_isRenderMesh2PSD = !_isRenderMesh2PSD;
			}
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod2, isMod2Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			
			//- 위치 보정 X값, Y값
			// 위치 보정 버튼들
			float offsetPosX = 0;
			float offsetPosY = 0;
			if(_selectedPSDLayerData != null)
			{
				offsetPosX = _selectedPSDLayerData._remapPosOffsetDelta_X;
				offsetPosY = _selectedPSDLayerData._remapPosOffsetDelta_Y;
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PositionOffset), GUILayout.Width(120));//Position Offset
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(120), GUILayout.Height(20));
			EditorGUILayout.LabelField("X", GUILayout.Width(15));
			float nextOffsetPosX = EditorGUILayout.FloatField(offsetPosX, GUILayout.Width(35));
			if(nextOffsetPosX != offsetPosX && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_X = nextOffsetPosX;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(15));
			float nextOffsetPosY = EditorGUILayout.FloatField(offsetPosY, GUILayout.Width(35));
			if(nextOffsetPosY != offsetPosY && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_Y = nextOffsetPosY;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
			//X,Y 제어 버튼

			//위치를 쉽게 제어하기 위해서 버튼으로 만들자
			//				Y:(+1, +10)
			//X:(-10, -1)					X:(+1, +10)
			//				Y:(-1, -10)
			int width_contBtn = 40;
			int height_contBtn = 22;
			int width_contBtnArea = width_contBtn * 2 + 4;
			int margin_contBtn = ((height_RightLower - 4) - height_contBtn) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-X 버튼
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			//+-Y
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+Y 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(margin_contBtn / 2 - 2);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-Y 버튼
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+X 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				//X + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{ 
				// X + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);
			

			//"이전" Bake 크기
			EditorGUILayout.BeginVertical(GUILayout.Width(140), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PrevAtlasScale) + " (%)", GUILayout.Width(140));//Prev Atlas Scale
			GUILayout.Space(1);
			int next_PrevBakeScale = EditorGUILayout.DelayedIntField(_selectedPSDSet._prev_bakeScale100, GUILayout.Width(130));
			if(next_PrevBakeScale != _selectedPSDSet._prev_bakeScale100)
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(next_PrevBakeScale, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			int width_scaleBtnWidth = 28;
			int height_scaleBtnWidth = 18;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(140), GUILayout.Height(height_scaleBtnWidth));
			GUILayout.Space(4);
			if(GUILayout.Button("-5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 -= 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 -= 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			GUILayout.Space(6);
			if(GUILayout.Button("+1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 += 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 += 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);

			EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height_RightLower));
			//Layer 전환
			// Label
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(100), GUILayout.Height(height_RightLower - 10));
			
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(false);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(true);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			//EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}


		private void SelectPSDLayer(bool isNext)
		{
			if (_selectedPSDLayerData == null)
			{
				if (_psdLoader.PSDLayerDataList.Count > 0)
				{
					//맨 마지막 부터 역순으로 이동
					for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
					{
						if(_psdLoader.PSDLayerDataList[i]._isBakable)
						{
							_selectedPSDLayerData = _psdLoader.PSDLayerDataList[i];
							return;
						}
					}
				}
				//0부터 시작해서 Bake되는 레이어를 찾자
			}
			else
			{
				int curIndex = _psdLoader.PSDLayerDataList.IndexOf(_selectedPSDLayerData);
				if(curIndex < 0)
				{
					return;
				}
				if(isNext)
				{
					//Index를 줄여가면서 확인
					curIndex -= 1;
					while(true)
					{
						if(curIndex < 0)
						{
							return;
						}

						if(_psdLoader.PSDLayerDataList[curIndex]._isBakable)
						{
							_selectedPSDLayerData = _psdLoader.PSDLayerDataList[curIndex];
							return;
						}

						curIndex--;
					}
				}
				else
				{
					//Index를 늘려가면서 호가인
					curIndex += 1;

					while(true)
					{
						if(curIndex >= _psdLoader.PSDLayerDataList.Count)
						{
							return;
						}

						if(_psdLoader.PSDLayerDataList[curIndex]._isBakable)
						{
							_selectedPSDLayerData = _psdLoader.PSDLayerDataList[curIndex];
							return;
						}

						curIndex++;
					}
				}
			}
				
		}




		private void GUI_Center_5_AtlasSetting(int width, int height, Rect centerRect)
		{
			//텍스쳐 아틀라스 Bake 설정
			//PSD와 동일하게 3개의 열로 구성된다.
			//1열 : 생성된 Atlas 미리보기 GUI
			//2열 : 생성된 Atlas 리스트
			//3열 : Atlas 정보
			int margin = 4;
			
			int width_2 = 200;
			int width_3 = 300;
			int width_1 = width - (width_2 + margin + width_3 + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height), "");
			GUI.backgroundColor = prevColor;
			
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height), "");
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			

			//--------------------------------------
			// <1열 : GUI : Atlas 미리보기 >
			Rect guiRect = new Rect(centerRect.xMin, centerRect.yMin, width_1, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(-3f, 3f));

			// Bake Atlas를 렌더링하자
			if (_psdLoader.BakeDataList.Count > 0 && !_psdLoader.IsImageBaking)
			{
				if (_selectedPSDBakeData == null)
				{
					apPSDBakeData curBakedData = null;
					//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
					for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
					{
						//curBakedData = _bakeDataList[i];//이전 코드
						curBakedData = _psdLoader.BakeDataList[i];

						Vector2 imgPosOffset = new Vector2(curBakedData._width * i, 0);

						_gl.DrawTexture(curBakedData._bakedImage,
											new Vector2(curBakedData._width / 2, curBakedData._height / 2) + imgPosOffset,
											curBakedData._width, curBakedData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											false);
					}
				}
				else
				{
					_gl.DrawTexture(_selectedPSDBakeData._bakedImage,
											new Vector2(_selectedPSDBakeData._width / 2, _selectedPSDBakeData._height / 2),
											_selectedPSDBakeData._width, _selectedPSDBakeData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											true);
				}
			}



			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : Atlas 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height));
			
			Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			int itemHeight = 30;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)		{ guiStyle_Btn_Selected.normal.textColor = Color.cyan; }
			else								{ guiStyle_Btn_Selected.normal.textColor = Color.white; }
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;


			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_2));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Deselect), GUILayout.Width(width_2 - 7), GUILayout.Height(18)))//Deselect
			{
				if (IsGUIUsable)
				{
					_selectedPSDBakeData = null;
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			_scroll_Step5_Left = EditorGUILayout.BeginScrollView(_scroll_Step5_Left, false, true, GUILayout.Width(width_2), GUILayout.Height(height - 41));
			
			int width_Line2InScroll = width_2 - 24;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));

			GUILayout.Space(1);
			int iList = 0;
			if (_psdLoader.BakeDataList.Count > 0)
			{
				apPSDBakeData curBakeData = null;
				//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
				for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_Btn;

					//curBakeData = _bakeDataList[i];//이전 코드
					curBakeData = _psdLoader.BakeDataList[i];

					if (_selectedPSDBakeData == curBakeData)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						}


						//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						if (iList == 0)
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 1, width_Line2InScroll + 10, itemHeight), "");
						}
						else
						{
							GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 30, width_Line2InScroll + 10, itemHeight), "");
						}

						GUI.backgroundColor = prevColor;

						curGUIStyle = guiStyle_Btn_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(itemHeight));

					GUILayout.Space(5);
					EditorGUILayout.LabelField(new GUIContent(icon_Image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));


					if (GUILayout.Button("  " + curBakeData.Name, curGUIStyle, GUILayout.Width(width_Line2InScroll - (5 + itemHeight)), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedPSDBakeData = curBakeData;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			GUILayout.Space(height + 20);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : Atlas Bake 설정 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));

			width_3 -= 20;

			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AssetName), GUILayout.Width(width_3));//Asset Name
			string next_fileNameOnly = EditorGUILayout.DelayedTextField(_psdLoader.FileName, GUILayout.Width(width_3));
			//if (IsGUIUsable)
			{
				//_fileNameOnly = next_fileNameOnly;
				_psdLoader.SetFileName(next_fileNameOnly);
			}

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SavePath), GUILayout.Width(width_3));//Save Path
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			string prev_bakeDstFilePath = _selectedPSDSet._bakeOption_DstFilePath;
			string next_bakeDstFilePath = EditorGUILayout.DelayedTextField(_selectedPSDSet._bakeOption_DstFilePath, GUILayout.Width(width_3 - 64));
			if (IsGUIUsable)
			{
				if (!string.Equals(_selectedPSDSet._bakeOption_DstFilePath, next_bakeDstFilePath))
				{
					_selectedPSDSet._bakeOption_DstFilePath = next_bakeDstFilePath;

					int subStartLength = Application.dataPath.Length;
					
					_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
					if (_selectedPSDSet._bakeOption_DstFilePath.Length > subStartLength)
					{
						_selectedPSDSet._bakeOption_DstFileRelativePath += _selectedPSDSet._bakeOption_DstFilePath.Substring(subStartLength);
					}
				}
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Set), GUILayout.Width(60)))//Set
			{
				if (IsGUIUsable)
				{
					string defaultPath = _selectedPSDSet._bakeOption_DstFileRelativePath;
					if(string.IsNullOrEmpty(defaultPath))
					{
						defaultPath = "Assets";
					}
					_selectedPSDSet._bakeOption_DstFilePath = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");

					if (!_selectedPSDSet._bakeOption_DstFilePath.StartsWith(Application.dataPath))
					{

						//EditorUtility.DisplayDialog("Bake Destination Path Error", "Bake Destination Path is have to be in Asset Folder", "Okay");
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
														_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
														_editor.GetText(TEXT.Close)
														);

						_selectedPSDSet._bakeOption_DstFilePath = "";
						_selectedPSDSet._bakeOption_DstFileRelativePath = "";
					}
					else
					{
						//앞의 걸 빼고 나면 (..../Assets) + ....가 된다.
						//Relatives는 "Assets/..."로 시작해야한다.
						int subStartLength = Application.dataPath.Length;
						_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
						if (_selectedPSDSet._bakeOption_DstFilePath.Length > subStartLength)
						{
							_selectedPSDSet._bakeOption_DstFileRelativePath += _selectedPSDSet._bakeOption_DstFilePath.Substring(subStartLength);
						}
					}
				}
				//_bakeDstFilePath = EditorUtility.SaveFilePanelInProject("Set Atlas Save Path with File Name", _fileNameOnly, "png", "Please enter a file name to save the atlas set.\nFiles are name with an index.");
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);


			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakingOption), GUILayout.Width(width_3));//Atlas Baking Option
			GUILayout.Space(10);


			apPSDLoader.BAKE_SIZE prev_bakeWidth = BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Width);
			apPSDLoader.BAKE_SIZE prev_bakeHeight = BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Height);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(120));//Atlas Width
			apPSDLoader.BAKE_SIZE next_bakeWidth = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_selectedPSDSet._bakeOption_Width, _bakeDescription, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Width = BakeSizeLoader2PSDSet(next_bakeWidth);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(120));//Atlas Height
			apPSDLoader.BAKE_SIZE next_bakeHeight = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_selectedPSDSet._bakeOption_Height, _bakeDescription, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Height = BakeSizeLoader2PSDSet(next_bakeHeight);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			int prev_bakeMaximumNumAtlas = _selectedPSDSet._bakeOption_MaximumNumAtlas;
			int prev_bakePadding = _selectedPSDSet._bakeOption_Padding;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MaximumAtlas) + " : ", GUILayout.Width(120));//Maximum Atlas
			int next_bakeMaximumNumAtlas = EditorGUILayout.DelayedIntField(_selectedPSDSet._bakeOption_MaximumNumAtlas, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_MaximumNumAtlas = next_bakeMaximumNumAtlas;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Padding) + " : ", GUILayout.Width(120));//Padding
			int next_bakePadding = EditorGUILayout.DelayedIntField(_selectedPSDSet._bakeOption_Padding, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Padding = next_bakePadding;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			bool prev_bakeBlurOption = _selectedPSDSet._bakeOption_BlurOption;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_FixBorderProblem) + " : ", GUILayout.Width(120));//Fix Border Problem
			bool next_bakeBlurOption = EditorGUILayout.Toggle(_selectedPSDSet._bakeOption_BlurOption, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_BlurOption = next_bakeBlurOption;
			}
			EditorGUILayout.EndHorizontal();


			//이제 Bake 가능한지 체크하자
			GUILayout.Space(10);
			if (
				//prev_isBakeResizable != _isBakeResizable ||
				prev_bakeWidth != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Width) ||
				prev_bakeHeight != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Height) ||
				prev_bakeMaximumNumAtlas != _selectedPSDSet._bakeOption_MaximumNumAtlas ||
				prev_bakePadding != _selectedPSDSet._bakeOption_Padding ||
				!string.Equals(prev_bakeDstFilePath, _selectedPSDSet._bakeOption_DstFilePath) ||
				prev_bakeBlurOption != _selectedPSDSet._bakeOption_BlurOption)
			{
				_isNeedBakeCheck = true;
			}

			if (_isNeedBakeCheck)
			{
				//이전 코드
				//CheckBakable();

				//Calculate를 하자
				_psdLoader.Step2_Calculate(
					_selectedPSDSet._bakeOption_DstFilePath, _selectedPSDSet._bakeOption_DstFileRelativePath,
					 GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
					 GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
					_selectedPSDSet._bakeOption_MaximumNumAtlas, 
					_selectedPSDSet._bakeOption_Padding,
					_selectedPSDSet._bakeOption_BlurOption,
					OnCalculateResult
					);
			}

			
			GUIStyle guiStyle_Result = new GUIStyle(GUI.skin.box);
			guiStyle_Result.alignment = TextAnchor.MiddleLeft;
			guiStyle_Result.normal.textColor = apEditorUtil.BoxTextColor;


			if (_isBakeWarning)
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;


				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//Warning
				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n" + _bakeWarningMsg, guiStyle_WarningBox, GUILayout.Width(width_3), GUILayout.Height(70));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Bake), GUILayout.Width(width_3), GUILayout.Height(40)))//Bake
				{
					if (IsGUIUsable)
					{
						//이전 코드
						//StartBake();

						_psdLoader.Step3_Bake(_loadKey_Bake, OnBakeResult, _loadKey_Calculated);
					}
				}
				if (_loadKey_Calculated != _loadKey_Bake)
				{
					GUILayout.Space(10);
					GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
					guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
					guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

					GUI.backgroundColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);

					//이전 : PSD Loader 사용 전
					//GUILayout.Box("[ Settings are changed ]"
					//				+ "\n  Expected Scale : " + _realBakeResizeX100 + " %"
					//				+ "\n  Expected Atlas : " + _realBakedAtlasCount,
					//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

					//GUILayout.Box("[ Settings are changed ]"
					//				+ "\n  Expected Scale : " + _psdLoader.CalculatedResizeX100 + " %"
					//				+ "\n  Expected Atlas : " + _psdLoader.CalculatedAtlasCount,
					//				guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));

					GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_SettingsAreChanged) + " ]"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedScale) + " : " + _psdLoader.CalculatedResizeX100 + " %"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedAtlas) + " : " + _psdLoader.CalculatedAtlasCount,
									guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));


					
					GUI.backgroundColor = prevColor;

				}


			}
			GUILayout.Space(10);
			if (_loadKey_Bake != null)
			{
				//Bake가 되었다면 => 그 정보를 넣어주자
				//이전 : PSD Loader 사용 전
				//GUILayout.Box("[ Baked Result ]"
				//				+ "\n  Scale Percent : " + _resultBakeResizeX100 + " %"
				//				+ "\n  Atlas : " + _resultAtlasCount,
				//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

				//GUILayout.Box("[ Baked Result ]"
				//				+ "\n  Scale Percent : " + _psdLoader.BakedResizeX100 + " %"
				//				+ "\n  Atlas : " + _psdLoader.BakedAtlasCount,
				//				guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));

				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_BakeResult) + " ]"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ScalePercent) + " : " + _psdLoader.BakedResizeX100 + " %"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_Atlas) + " : " + _psdLoader.BakedAtlasCount,
								guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));
				
			}


			GUILayout.Space(20);

			if (IsProcessRunning)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				Rect barRect = new Rect(lastRect.x + 5, lastRect.y + 30, width_3 - 5, 20);

				//이전 : PSD Loader 사용 전
				//float barRatio = Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);
				//EditorGUI.ProgressBar(barRect, barRatio, _threadProcessName);

				float barRatio = _psdLoader.GetImageBakingRatio();
				EditorGUI.ProgressBar(barRect, barRatio, _psdLoader.GetProcessLabel());

			}


			EditorGUILayout.EndVertical();
			//--------------------------------------


			EditorGUILayout.EndHorizontal();
		}


		private apPSDLoader.BAKE_SIZE BakeSizePSDSet2Loader(apPSDSet.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDSet.BAKE_SIZE.s256:	return apPSDLoader.BAKE_SIZE.s256;
				case apPSDSet.BAKE_SIZE.s512:	return apPSDLoader.BAKE_SIZE.s512;
				case apPSDSet.BAKE_SIZE.s1024:	return apPSDLoader.BAKE_SIZE.s1024;
				case apPSDSet.BAKE_SIZE.s2048:	return apPSDLoader.BAKE_SIZE.s2048;
				case apPSDSet.BAKE_SIZE.s4096:	return apPSDLoader.BAKE_SIZE.s4096;
			}
			return apPSDLoader.BAKE_SIZE.s4096;
		}

		private apPSDSet.BAKE_SIZE BakeSizeLoader2PSDSet(apPSDLoader.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDLoader.BAKE_SIZE.s256:	return apPSDSet.BAKE_SIZE.s256;
				case apPSDLoader.BAKE_SIZE.s512:	return apPSDSet.BAKE_SIZE.s512;
				case apPSDLoader.BAKE_SIZE.s1024:	return apPSDSet.BAKE_SIZE.s1024;
				case apPSDLoader.BAKE_SIZE.s2048:	return apPSDSet.BAKE_SIZE.s2048;
				case apPSDLoader.BAKE_SIZE.s4096:	return apPSDSet.BAKE_SIZE.s4096;
			}
			return apPSDSet.BAKE_SIZE.s4096;
		}

		private int GetBakeIntSize(apPSDSet.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDSet.BAKE_SIZE.s256:	return 256;
				case apPSDSet.BAKE_SIZE.s512:	return 512;
				case apPSDSet.BAKE_SIZE.s1024:	return 1024;
				case apPSDSet.BAKE_SIZE.s2048:	return 2048;
				case apPSDSet.BAKE_SIZE.s4096:	return 4096;
			}
			return 4096;

		}
		// Bake Events
		//------------------------------------------------------------------------------------------------------
		private void OnCalculateResult(bool isSuccess, object loadKey, bool isWarning, string warningMsg)
		{

			if (isSuccess)
			{
				_loadKey_Calculated = loadKey;
				_isBakeWarning = false;
				_bakeWarningMsg = "";
			}
			else
			{
				if (isWarning)
				{
					_isBakeWarning = true;
					_bakeWarningMsg = warningMsg;
				}
				else
				{
					_isBakeWarning = false;
					_bakeWarningMsg = "";
				}
				_loadKey_Calculated = null;
			}

			_isNeedBakeCheck = false;
		}

		private void OnBakeResult(bool isSuccess, object loadKey)
		{
			if (isSuccess)
			{
				_loadKey_Bake = loadKey;
			}
			else
			{
				_loadKey_Bake = null;
			}
		}


		private void OnConvertResult(bool isSuccess)
		{
			if(isSuccess)
			{
				OnLoadComplete(true);
			}
		}


		public void OnLoadComplete(bool isResult)
		{
			if (_funcResult != null && _selectedPSDSet != null)
			{
				if (isResult)
				{
					//float deltaScaleRatio = ((float)_psdLoader.BakedResizeX100 / (float)_selectedPSDSet._nextBakeScale100);//<<이전 Bake 확대 비율 대비 현재 Bake 확대 비율
					//Debug.Log("크기 변환 비율 : " + deltaScaleRatio + "(" + _psdLoader.BakedResizeX100 + " / " + _selectedPSDSet._nextBakeScale100 + ")");
					_funcResult(	isResult, 
									_loadKey, 
									_psdLoader.FileName, _psdLoader.FileFullPath,
									_psdLoader.PSDLayerDataList, 
									_psdLoader.BakedResizeX100, 
									_selectedPSDSet._next_meshGroupScaleX100,
									_selectedPSDSet._prev_bakeScale100,
									_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
									_psdLoader.BakedPadding, 
									GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
									GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
									_selectedPSDSet._bakeOption_MaximumNumAtlas,
									_selectedPSDSet._bakeOption_BlurOption,
									_selectedPSDSet._nextBakeCenterOffsetDelta_X,
									_selectedPSDSet._nextBakeCenterOffsetDelta_Y,
									_psdLoader._bakeDstPath,
									_psdLoader._bakeDstPathRelative,
									_selectedPSDSet 
									//deltaScaleRatio
									);
				}
				else
				{
					_funcResult(isResult, 
						_loadKey, 
						_psdLoader.FileName, _psdLoader.FileFullPath,
						null, 
						_psdLoader.BakedResizeX100, 
						_selectedPSDSet._next_meshGroupScaleX100,
						_selectedPSDSet._prev_bakeScale100,
						_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
						_psdLoader.BakedPadding, 
						GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
						GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
						_selectedPSDSet._bakeOption_MaximumNumAtlas,
						_selectedPSDSet._bakeOption_BlurOption,
						_selectedPSDSet._nextBakeCenterOffsetDelta_X,
						_selectedPSDSet._nextBakeCenterOffsetDelta_Y,
						_psdLoader._bakeDstPath,
						_psdLoader._bakeDstPathRelative,
						_selectedPSDSet
						//1.0f
						);
				}
			}
			//CloseDialog();
			_isRequestCloseDialog = true;
		}

		// GUI Base
		//--------------------------------------------------------------------------------------------------
		private void UpdateAndDrawGUIBase(Rect guiRect, Vector2 centerOffset)
		{
			MouseUpdate(guiRect);

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif
			bool isAlt = Event.current.alt;

			MouseScrollUpdate(guiRect, isCtrl, isAlt);

			//int scrollHeightOffset = 32;
			_scroll_GUI.y = GUI.VerticalScrollbar(new Rect(guiRect.xMin + guiRect.width - 15, guiRect.yMin, 20, guiRect.height - 15), _scroll_GUI.y, 5.0f, -100.0f, 100.0f + 5.0f);
			_scroll_GUI.x = GUI.HorizontalScrollbar(new Rect(guiRect.xMin, guiRect.yMin + (guiRect.height - 15), guiRect.width - 15, 20), _scroll_GUI.x, 5.0f, -100.0f, 100.0f + 5.0f);

			if (GUI.Button(new Rect(guiRect.xMin + guiRect.width - 15, guiRect.yMin + (guiRect.height - 15), 15, 15), ""))
			{
				_scroll_GUI = Vector2.zero;
				_iZoomX100 = ZOOM_INDEX_DEFAULT;
			}

			//Vector2 centerOffset = new Vector2(6, 0.3f);
			_gl.SetWindowSize(
						(int)guiRect.width, (int)guiRect.height,
						_scroll_GUI - centerOffset, (float)(_zoomListX100[_iZoomX100]) * 0.01f,
						(int)guiRect.x, (int)guiRect.y,
						(int)position.width, (int)position.height);

			

			_gl.DrawGrid();
			//_gl.DrawBoldLine(Vector2.zero, new Vector2(10, 10), 5, Color.yellow, true);
		}


		// Mouse Update
		//--------------------------------------------------------------------------------------------------
		private bool IsMouseInGUI(Vector2 mousePos, Rect mainGUIRect)
		{
			if (mousePos.x < 0 || mousePos.x > mainGUIRect.width
				|| mousePos.y < 0 || mousePos.y > mainGUIRect.height)
			{
				return false;
			}
			return true;
		}

		private void MouseUpdate(Rect mainGUIRect)
		{
			bool isMouseEvent = Event.current.rawType == EventType.ScrollWheel ||
				Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp;

			if (!isMouseEvent)
			{
				return;
			}

			Vector2 mousePos = Event.current.mousePosition - new Vector2(mainGUIRect.x, mainGUIRect.y);
			_mouse.SetMousePos(mousePos, Event.current.mousePosition);
			_mouse.ReadyToUpdate();

			if (Event.current.rawType == EventType.ScrollWheel)
			{
				Vector2 deltaValue = Event.current.delta;
				_mouse.Update_Wheel((int)(deltaValue.y * 10.0f));

			}
			else
			{
				int iMouse = -1;
				switch (Event.current.button)
				{
					case 0://Left
						iMouse = 0;
						break;

					case 1://Right
						iMouse = 1;
						break;

					case 2://Middle
						iMouse = 2;
						break;
				}


				if (iMouse >= 0)
				{
					_mouse.SetMouseBtn(iMouse);

					//GUI 기준 상대 좌표
					switch (Event.current.rawType)
					{
						case EventType.MouseDown:
							{
								if (IsMouseInGUI(mousePos, mainGUIRect))
								{
									//Editor._mouseBtn[iMouse].Update_Pressed(mousePos);
									_mouse.Update_Pressed();
								}
							}
							break;

						case EventType.MouseUp:
							{
								//Editor._mouseBtn[iMouse].Update_Released(mousePos);
								_mouse.Update_Released();
							}
							break;

						case EventType.MouseMove:
						case EventType.MouseDrag:
							{
								//Editor._mouseBtn[iMouse].Update_Moved(deltaValue);
								_mouse.Update_Moved();

							}
							break;

							//case EventType.ScrollWheel:
							//	{

							//	}
							//break;
					}

					_mouse.EndUpdate();
				}
			}
		}

		private bool MouseScrollUpdate(Rect mainGUIRect, bool isCtrl, bool isAlt)
		{
			if (_mouse.Wheel != 0)
			{
				//if(IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
				if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
				{
					if (_mouse.Wheel > 0)
					{
						//줌 아웃 = 인덱스 감소
						_iZoomX100--;
						if (_iZoomX100 < 0)
						{ _iZoomX100 = 0; }
					}
					else if (_mouse.Wheel < 0)
					{
						//줌 인 = 인덱스 증가
						_iZoomX100++;
						if (_iZoomX100 >= _zoomListX100.Length)
						{
							_iZoomX100 = _zoomListX100.Length - 1;
						}
					}

					//Editor.Repaint();
					//SetRepaint();
					//Debug.Log("Zoom [" + _zoomListX100[_iZoomX100] + "]");

					_mouse.UseWheel();
					_isCtrlAltDrag = false;
					return true;
				}
			}

			if (_mouse.ButtonIndex == 2)
			{
				if (_mouse.Status == apPSDMouse.MouseBtnStatus.Down ||
					_mouse.Status == apPSDMouse.MouseBtnStatus.Pressed)
				{
					//if (IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
					if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
					{
						Vector2 moveDelta = _mouse.PosDelta;
						//RealX = scroll * windowWidth * 0.1

						Vector2 sensative = new Vector2(
							1.0f / (mainGUIRect.width * 0.1f),
							1.0f / (mainGUIRect.height * 0.1f));

						_scroll_GUI.x -= moveDelta.x * sensative.x;
						_scroll_GUI.y -= moveDelta.y * sensative.y;


						_mouse.UseMouseDrag();
						_isCtrlAltDrag = false;
						return true;
					}
				}
			}


			//추가 : Ctrl+Alt 누르고 제어하기
			//Ctrl+Alt+좌클릭 드래그 : 화면 이동
			//Ctrl+Alt+우클릭 드래그 : 화면 확대
			if (isCtrl && isAlt)
			{
				if (_mouse.Status == apPSDMouse.MouseBtnStatus.Down ||
					_mouse.Status == apPSDMouse.MouseBtnStatus.Pressed)
				{
					if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
					{
						Vector2 moveDelta = _mouse.PosDelta;
						if(!_isCtrlAltDrag)
						{
							moveDelta = Vector2.zero;
						}
						if (_mouse.ButtonIndex == 0)
						{
							//Ctrl+Alt로 화면 이동
							Vector2 sensative = new Vector2(
							1.0f / (mainGUIRect.width * 0.1f),
							1.0f / (mainGUIRect.height * 0.1f));


							_scroll_GUI.x -= moveDelta.x * sensative.x;
							_scroll_GUI.y -= moveDelta.y * sensative.y;

							//처리 끝
							_mouse.UseMouseDrag();
							_isCtrlAltDrag = true;
							return true;
						}
						else if (_mouse.ButtonIndex == 1)
						{
							//Ctrl+Alt로 화면 확대/축소
							float wheelOffset = 0.0f;
							if(Mathf.Abs(moveDelta.x) * 1.5f > Mathf.Abs(moveDelta.y))
							{
								wheelOffset = moveDelta.x;
							}
							else
							{
								wheelOffset = moveDelta.y;
							}
							float zoomPrev = _zoomListX100[_iZoomX100] * 0.01f;
							if (wheelOffset < -1.3f)
							{
								//줌 아웃 = 인덱스 감소
								_iZoomX100--;
								if (_iZoomX100 < 0)	{ _iZoomX100 = 0; }
							}
							else if (wheelOffset > 1.3f)
							{
								//줌 인 = 인덱스 증가
								_iZoomX100++;
								if (_iZoomX100 >= _zoomListX100.Length)
								{
									_iZoomX100 = _zoomListX100.Length - 1;
								}
							}


							//마우스의 World 좌표는 같아야 한다.
							float zoomNext = _zoomListX100[_iZoomX100] * 0.01f;

							//중심점의 위치를 구하자 (Editor GL 기준)
							Vector2 scroll = new Vector2(_scroll_GUI.x * 0.1f * _gl.WindowSize.x,
														_scroll_GUI.y * 0.1f * _gl.WindowSize.y);
							Vector2 guiCenterPos = _gl.WindowSizeHalf - scroll;

							Vector2 deltaMousePos = _mouse.PosLast - guiCenterPos;//>>이후
							Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

							//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자

							//>>변경
							float nextScrollX = ((nextDeltaMousePos.x - _mouse.PosLast.x) + _gl.WindowSizeHalf.x) / (0.1f * _gl.WindowSize.x);
							float nextScrollY = ((nextDeltaMousePos.y - _mouse.PosLast.y) + _gl.WindowSizeHalf.y) / (0.1f * _gl.WindowSize.y);

							nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
							nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

							_scroll_GUI.x = nextScrollX;
							_scroll_GUI.y = nextScrollY;

							//처리 끝
							_mouse.UseMouseDrag();
							_isCtrlAltDrag = true;
							return true;
						}
						else
						{
							_isCtrlAltDrag = false;
						}
					}
				}
				
			}

			_isCtrlAltDrag = false;
			return false;
		}


		//------------------------------------------------------------------------------------
		private void MoveNext()
		{
			if (!IsGUIUsable)
			{
				return;
			}
			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet:				_step = RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup;	break;
				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:	_step = RELOAD_STEP.Step3_LinkLayerToTransform;	break;
				case RELOAD_STEP.Step3_LinkLayerToTransform:		_step = RELOAD_STEP.Step4_ModifyOffset;	break;
				case RELOAD_STEP.Step4_ModifyOffset:				_step = RELOAD_STEP.Step5_AtlasSetting;	break;
				case RELOAD_STEP.Step5_AtlasSetting:				_step = RELOAD_STEP.Step5_AtlasSetting;	break;
			}
			if(_step == RELOAD_STEP.Step3_LinkLayerToTransform)
			{
				//다음이 Step3라면
				//_selectedPSDSet._linkedTargetMeshGroup.ResetRenderUnits();
				RefreshTransform2PSDLayer();
				MakeTransformDataList();

				_isLinkGUIColoredList = false;
				_isLinkOverlayColorRender = false;

				if (_psdLoader.PSDLayerDataList != null && _psdLoader.PSDLayerDataList.Count > 0)
				{
					_selectedPSDLayerData = _psdLoader.PSDLayerDataList[_psdLoader.PSDLayerDataList.Count - 1];
				}
			}
			else if(_step == RELOAD_STEP.Step4_ModifyOffset)
			{
				RefreshTransform2PSDLayer();
				_selectedPSDLayerData = null;
				//_selectedPSDSetLayer = null;

				if (_psdLoader.PSDLayerDataList != null && _psdLoader.PSDLayerDataList.Count > 0)
				{
					_selectedPSDLayerData = _psdLoader.PSDLayerDataList[_psdLoader.PSDLayerDataList.Count - 1];
				}
			}

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;

			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;

			if(_step == RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup)
			{
				_renderMode_PSD = RENDER_MODE.Outline;//<<첫 화면은 Outline 모드
			}

			apEditorUtil.ReleaseGUIFocus();
		}

		private void MovePrev()
		{
			if (!IsGUIUsable)
			{
				return;
			}

			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet:				_step = RELOAD_STEP.Step1_SelectPSDSet;	break;
				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:	_step = RELOAD_STEP.Step1_SelectPSDSet;	break;
				case RELOAD_STEP.Step3_LinkLayerToTransform:		_step = RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup;	break;
				case RELOAD_STEP.Step4_ModifyOffset:				_step = RELOAD_STEP.Step3_LinkLayerToTransform;	break;
				case RELOAD_STEP.Step5_AtlasSetting:				_step = RELOAD_STEP.Step4_ModifyOffset;	break;
			}

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;

			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;

			apEditorUtil.ReleaseGUIFocus();
		}



		//-----------------------------------------------------------------------
		private void SetGUIVisible(string keyName, bool isVisible)
		{
			if (_delayedGUIShowList.ContainsKey(keyName))
			{
				if (_delayedGUIShowList[keyName] != isVisible)
				{
					_delayedGUIShowList[keyName] = isVisible;//Visible 조건 값

					//_delayedGUIToggledList는 "Visible 값이 바뀌었을때 그걸 바로 GUI에 적용했는지"를 저장한다.
					//바뀐 순간엔 GUI에 적용 전이므로 "Visible Toggle이 완료되었는지"를 저장하는 리스트엔 False를 넣어둔다.
					_delayedGUIToggledList[keyName] = false;
				}
			}
			else
			{
				_delayedGUIShowList.Add(keyName, isVisible);
				_delayedGUIToggledList.Add(keyName, false);
			}
		}

		public bool IsDelayedGUIVisible(string keyName)
		{
			//GUI Layout이 출력되려면
			//1. Visible 값이 True여야 한다.
			//2-1. GUI Event가 Layout/Repaint 여야 한다.
			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다.


			//1-1. GUI Layout의 Visible 여부를 결정하는 값이 없다면 -> False
			if (!_delayedGUIShowList.ContainsKey(keyName))
			{
				return false;
			}

			//1-2. GUI Layout의 Visible 값이 False라면 -> False
			if (!_delayedGUIShowList[keyName])
			{
				return false;
			}

			//2. (Toggle 처리가 완료되지 않은 상태에서..)
			if (!_delayedGUIToggledList[keyName])
			{
				//2-1. GUI Event가 Layout/Repaint라면 -> 다음 OnGUI까지 일단 보류합니다. False
				if (!_isGUIEvent)
				{
					return false;
				}

				// GUI Event가 유효하다면 Visible이 가능하다고 바꿔줍니다.
				//_delayedGUIToggledList [False -> True]
				_delayedGUIToggledList[keyName] = true;
			}

			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다. -> True
			return true;
		}


		// GL
		//-------------------------------------------------------------------------------------------------
		private void DrawPSD(bool isDrawToneOutline, apPSDLayerData selectedLayerData, float posX, float posY, int bakeScale100)
		{
			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}
			if(_psdLoader.PSDLayerDataList.Count == 0)
			{
				return;
			}
			Vector2 renderOffset = new Vector2(posX, posY);
			Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

			apPSDLayerData curImageLayer = null;
			apMatrix worldMat = new apMatrix();
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curImageLayer = _psdLoader.PSDLayerDataList[i];
				if(curImageLayer._image == null)
				{
					continue;
				}

				//bool isSelected = (curImageLayer == selectedLayerData);
				//apMatrix3x3 worldMat = apMatrix3x3.TRS((curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset, 0.0f, renderScale);
				//apMatrix3x3 worldMat = new apMatrix3x3();
				worldMat.SetIdentity();
				worldMat.SetPos((curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset, false);
				worldMat.RMultiply(Vector2.zero, 0.0f, renderScale, false);
				worldMat.MakeMatrix();
				
				Color meshColor = curImageLayer._transparentColor2X;
				//if(isAlpha)
				//{
				//	meshColor.a *= 0.5f;
				//}
				if(isDrawToneOutline)
				{
					meshColor = _psdOverlayColor;
					meshColor.a = 0.8f;
				}
				_gl.DrawTexture(curImageLayer._image,
										worldMat.MtrxToSpace,
										curImageLayer._width, curImageLayer._height,
										meshColor,
										0.0f, isDrawToneOutline);

				//_gl.DrawTexture(curImageLayer._image,
				//						//curImageLayer._posOffset - _imageCenterPosOffset,
				//						(curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset,
				//						curImageLayer._width, curImageLayer._height,
				//						curImageLayer._transparentColor2X,
				//						isSelected);
			}
		}

		private void DrawMeshGroup(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			//중요 > meshGroup의 RootUnit의 Transform을 역으로 만들어야 한다.

			meshGroup.RefreshForce();
			apMatrix rootMatrix = null;
			if(meshGroup._rootMeshGroupTransform != null)
			{
				//Debug.Log("Root Matrix : " + meshGroup._rootMeshGroupTransform._matrix.ToString());
				rootMatrix = meshGroup._rootMeshGroupTransform._matrix;
				
			}
			
			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				renderUnit.CalculateWorldPositionWithoutModifier();//<NoMod Pos를 계산한다.
			}

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						//if(!renderUnit._meshTransform._isVisible_Default)
						//{
						//	continue;
						//}

						if (renderUnit._meshTransform._isClipping_Parent)
						{
							//Profiler.BeginSample("Render - Mask Unit");

							if (renderUnit._meshTransform._isVisible_Default)
							{
								_gl.DrawRenderUnit_ClippingParent_Renew(renderUnit, renderUnit._meshTransform._clipChildMeshes, null, rootMatrix);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//렌더링은 생략한다.
						}
						else
						{
							//Profiler.BeginSample("Render - Normal Unit");

							if (renderUnit._meshTransform._isVisible_Default)
							{
								_gl.DrawRenderUnit(renderUnit, rootMatrix);
							}
						}

					}
				}
			}

			//Debug.Log("Render MeshGroup : " + nRendered);
		}

		private void DrawTextureData(apTextureData textureData, bool isDrawOutline)
		{
			if(textureData == null || textureData._image == null)
			{
				return;
			}

			_gl.DrawTexture(textureData._image,
										Vector2.zero,
										textureData._width, textureData._height,
										new Color(0.5f, 0.5f, 0.5f, 1.0f),
										isDrawOutline);
		}

		private void DrawMesh(apMesh mesh, bool isShowAllTexture, bool isDrawEdge, float scale)
		{
			if(mesh == null || mesh.LinkedTextureData == null)
			{
				return;
			}
			
			_gl.DrawMesh(mesh, apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)), new Color(0.5f, 0.5f, 0.5f, 1.0f), isShowAllTexture, true, isDrawEdge);
		}


		private void DrawMeshToneColor(apMesh mesh, bool isShowAllTexture, float scale)
		{
			if(mesh == null || mesh.LinkedTextureData == null)
			{
				return;
			}
			_gl.DrawMesh(mesh, apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)), _meshOverlayColor, isShowAllTexture, true, false, true);
		}

		//private void DrawMesh(apMesh mesh, bool isShowAllTexture, bool isDrawEdge, Color color)
		//{
		//	if(mesh == null || mesh.LinkedTextureData == null)
		//	{
		//		return;
		//	}
		//	_gl.DrawMesh(mesh, apMatrix3x3.identity, color, isShowAllTexture, true, isDrawEdge);
		//}
		private void DrawMeshEdgeOnly(apMesh mesh, float scale)
		{
			if(mesh == null || mesh.LinkedTextureData == null)
			{
				return;
			}
			_gl.DrawMeshEdgeOnly(mesh, apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)));
		}


		private void DrawPSDLayer(apPSDLayerData layerData, float posX, float posY, int bakeScale100, bool isDrawOutline)
		{
			Vector2 renderOffset = new Vector2(posX, posY);
			Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

			apMatrix worldMat = new apMatrix();

			worldMat.SetIdentity();
			//worldMat.SetPos((layerData._posOffset - _psdLoader.PSDCenterOffset) + renderOffset);
			worldMat.SetPos(renderOffset, false);
			worldMat.RMultiply(Vector2.zero, 0.0f, renderScale, false);
			worldMat.MakeMatrix();
				
			Color meshColor = layerData._transparentColor2X;
			if(isDrawOutline)
			{
				meshColor = _psdOverlayColor;
			}
			_gl.DrawTexture(layerData._image,
							worldMat.MtrxToSpace,
							layerData._width, layerData._height,
							meshColor,
							0.0f, isDrawOutline);
		}


		//private void DrawPSDLayer(apPSDLayerData layerData, float posX, float posY, int bakeScale100, Color color2X)
		//{
		//	Vector2 renderOffset = new Vector2(posX, posY);
		//	Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

		//	apMatrix worldMat = new apMatrix();

		//	worldMat.SetIdentity();
		//	//worldMat.SetPos((layerData._posOffset - _psdLoader.PSDCenterOffset) + renderOffset);
		//	worldMat.SetPos(renderOffset);
		//	worldMat.RMultiply(Vector2.zero, 0.0f, renderScale);
				
		//	_gl.DrawTexture(layerData._image,
		//					worldMat.MtrxToSpace,
		//					layerData._width, layerData._height,
		//					color2X,
		//					0.0f);
		//}
		//----------------------------------------------------------------------------------------------
		private void SelectPSDSet(apPSDSet psdSet)
		{
			if(_selectedPSDSet == psdSet)
			{
				return;
			}
			_selectedPSDSet = psdSet;
			_psdLoader.Clear();//<<PSD Loader 초기화
			_selectedTextureData = null;
			//_selectedPSDSetLayer = null;

			//MeshGroup/TextureData와 연결을 해주자
			if(_selectedPSDSet._targetMeshGroupID < 0)
			{
				_selectedPSDSet._linkedTargetMeshGroup = null;
			}
			else
			{
				_selectedPSDSet._linkedTargetMeshGroup = _portrait.GetMeshGroup(_selectedPSDSet._targetMeshGroupID);
				if(_selectedPSDSet._linkedTargetMeshGroup == null)
				{
					_selectedPSDSet._targetMeshGroupID = -1;
				}
			}

			if(_selectedPSDSet._targetTextureDataList == null)
			{
				_selectedPSDSet._targetTextureDataList = new List<apPSDSet.TextureDataSet>();
			}

			for (int iTex = 0; iTex < _selectedPSDSet._targetTextureDataList.Count; iTex++)
			{
				apPSDSet.TextureDataSet texDataSet = _selectedPSDSet._targetTextureDataList[iTex];
				if(texDataSet._textureDataID >= 0)
				{
					texDataSet._linkedTextureData = _portrait.GetTexture(texDataSet._textureDataID);
					if(texDataSet._linkedTextureData == null)
					{
						texDataSet._textureDataID = -1;
					}
				}
				else
				{
					texDataSet._linkedTextureData = null;
				}
			}
			_selectedPSDSet._targetTextureDataList.RemoveAll(delegate(apPSDSet.TextureDataSet a)
			{
				return a._textureDataID < 0;
			});


			//PSD File을 열자
			_selectedPSDSet.RefreshPSDFilePath();
			if (_selectedPSDSet.IsValidPSDFile)
			{
				//유효한 PSD File인 경우 PSD Loader로 열자
				_psdLoader.Step1_LoadPSDFile(_selectedPSDSet._filePath, 
					(_selectedPSDSet._isLastBaked ? _selectedPSDSet._lastBakedAssetName : "")
					);
			}

			if (_selectedPSDSet._isLastBaked)
			{
				_selectedPSDSet._next_meshGroupScaleX100 = _selectedPSDSet._lastBaked_MeshGroupScaleX100;//<<Bake 크기를 지정
				_selectedPSDSet._prev_bakeScale100 = _selectedPSDSet._bakeScale100;
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = _selectedPSDSet._lastBaked_PSDCenterOffsetDelta_X;
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = _selectedPSDSet._lastBaked_PSDCenterOffsetDelta_Y;
			}
			else
			{
				_selectedPSDSet._next_meshGroupScaleX100 = 100;
				_selectedPSDSet._prev_bakeScale100 = 100;
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = 0;
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = 0;
			}
			

			

			//LoadKey 모두 초기화
			_loadKey_SelectMeshGroup = null;
			_loadKey_SelectTextureData = null;

			//_psdRenderPosOffset_X = 0;
			//_psdRenderPosOffset_Y = 0;

			//_selectedPSDSetLayer = null;
			_selectedTextureData = null;
			_selectedPSDLayerData = null;

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			MakeRemappingList();
			MakeTransformDataList();
		}


		private void MakeRemappingList()
		{
			//_remapList.Clear();
			//_remapList_Psd2Map.Clear();

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			if(_selectedPSDSet == null)
			{
				return;
			}
			apPSDLayerData psdLayer = null;

			//일단 모두 초기화
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				psdLayer = _psdLoader.PSDLayerDataList[i];

				psdLayer._isRemapSelected = false;
				psdLayer._isBakable = true;
				psdLayer._remap_TransformID = -1;
				psdLayer._remap_MeshTransform = null;
				psdLayer._remap_MeshGroupTransform = null;
			}

			if(_selectedPSDSet._isLastBaked && _selectedPSDSet._linkedTargetMeshGroup != null)
			{
				//Debug.Log("PSD Set Layer Remap [" + _selectedPSDSet._layers.Count + "]");
				//1. 만약 Bake된게 있다면
				//- PSDSetLayer에 따라서 찾자
				//- 이름과 레이어 번호를 기준으로 하자
				//- 검색 순서는 PSDLayerData <- PSDSetLayer 역으로
				apPSDSetLayer setLayer = null;
				for (int iSetLayer = 0; iSetLayer < _selectedPSDSet._layers.Count; iSetLayer++)
				{
					setLayer = _selectedPSDSet._layers[iSetLayer];

					if(!setLayer._isBaked)
					{
						//일단 Baked된 것부터 찾고 연결하자.
						continue;
					}


					//1. 이름과 레이어가 같은 PSD LayerData를 찾자
					apPSDLayerData srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate(apPSDLayerData a)
					{
						return a._layerIndex == setLayer._layerIndex
								&& string.Equals(a._name, setLayer._name)
								&& a._isImageLayer == setLayer._isImageLayer;
								
					});

					if (srcLayerData == null)
					{
						//2. 없다면> 이름만이라도 같은게 있으면 오케이
						//- 1개라면 > 그것을 선택
						//- 크기가 같은거 선택
						//- 레이어 인덱스의 차이가 가장 작은거 선택
						List<apPSDLayerData> srcLayerDataList = _psdLoader.PSDLayerDataList.FindAll(delegate (apPSDLayerData a)
						{
							return string.Equals(a._name, setLayer._name)
							&& a._isImageLayer == setLayer._isImageLayer;
						});

						if(srcLayerDataList != null && srcLayerDataList.Count > 0)
						{
							//2-1. 1개인 경우
							if(srcLayerDataList.Count == 1)
							{
								srcLayerData = srcLayerDataList[0];
							}

							//2-2. 크기가 같은게 1개 있다면 선택
							if (srcLayerData == null)
							{	
								List<apPSDLayerData> srcLayerDataList_SameSize = srcLayerDataList.FindAll(delegate(apPSDLayerData a)
								{
									return a._width == setLayer._width && a._height == setLayer._height;
								});

								if(srcLayerDataList_SameSize != null && srcLayerDataList_SameSize.Count == 1)
								{
									srcLayerData = srcLayerDataList_SameSize[0];
								}
							}

							//레이어 인덱스 차이가 가장 작은거 선택
							if (srcLayerData == null)
							{
								int minLayerIndexDiff = 20;//<<최대치
								int iMinLayer = -1;
								for (int iSubLayer = 0; iSubLayer < srcLayerDataList.Count; iSubLayer++)
								{
									apPSDLayerData subLayer = srcLayerDataList[iSubLayer];
									int indexDiff = Mathf.Abs(subLayer._layerIndex - setLayer._layerIndex);
									if(indexDiff < minLayerIndexDiff)
									{
										minLayerIndexDiff = indexDiff;
										iMinLayer = iSubLayer;
									}
								}
								if(iMinLayer >= 0)
								{
									srcLayerData = srcLayerDataList[iMinLayer];
								}
							}
							
						}
					}

					if (srcLayerData != null)
					{
						//Debug.Log("PSD Set Layer >> Source Layer Data Recovered : " + setLayer._transformID);
						//Transform이 존재한다면연결을 해주자
						if (srcLayerData._isImageLayer)
						{
							//apTransform_Mesh meshTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshTransform(setLayer._transformID);//이전
							apTransform_Mesh meshTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshTransformRecursive(setLayer._transformID);//버그 수정
							if(meshTransform != null)
							{
								srcLayerData._isRemapSelected = true;
								srcLayerData._isBakable = true;
								srcLayerData._remap_TransformID = setLayer._transformID;

								srcLayerData._remap_MeshTransform = meshTransform;
								srcLayerData._remap_MeshGroupTransform = null;

								//Debug.Log("Target : " + meshTransform._nickName);
							}
							//else
							//{
							//	Debug.LogError("No MeshTransform");
							//}
						}
						else
						{
							//apTransform_MeshGroup meshGroupTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshGroupTransform(setLayer._transformID);//이전
							apTransform_MeshGroup meshGroupTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshGroupTransformRecursive(setLayer._transformID);//버그 수정
							if (meshGroupTransform != null)
							{
								srcLayerData._isRemapSelected = true;
								srcLayerData._isBakable = true;
								srcLayerData._remap_TransformID = setLayer._transformID;

								srcLayerData._remap_MeshTransform = null;
								srcLayerData._remap_MeshGroupTransform = meshGroupTransform;

								//Debug.Log("Target : " + meshGroupTransform._nickName);
							}
							//else
							//{
							//	Debug.LogError("No MeshGroupTransform");
							//}
						}
					}
					//else
					//{
					//	Debug.LogError("PSD Set Layer >> Not Recovered : " + setLayer._transformID);
					//}
				}

				//Baked가 안된 레이어 정보를 찾아서 연결한다.
				for (int iSetLayer = 0; iSetLayer < _selectedPSDSet._layers.Count; iSetLayer++)
				{
					setLayer = _selectedPSDSet._layers[iSetLayer];

					if (setLayer._isBaked)
					{
						continue;
					}

					//1. 이름과 레이어가 같은 PSD LayerData를 찾자 + Remap이 안된 것
					apPSDLayerData srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate(apPSDLayerData a)
					{
						return a._layerIndex == setLayer._layerIndex
								&& string.Equals(a._name, setLayer._name)
								&& a._isImageLayer == setLayer._isImageLayer
								&& !a._isRemapSelected
								&& a._isBakable;
								
					});

					//2. 레이어 번호가 같은게 없다면, 이름이라도 같은걸 찾자
					if (srcLayerData == null)
					{
						srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate (apPSDLayerData a)
						{
						return string.Equals(a._name, setLayer._name)
								&& a._isImageLayer == setLayer._isImageLayer
								&& !a._isRemapSelected
								&& a._isBakable;

						});
					}

					if(srcLayerData != null)
					{
						//다른건 없고 그냥 Bakable을 끈다.
						srcLayerData._isBakable = false;
					}
				}
			}

			

			RefreshTransform2PSDLayer();

			//연결이 되었다면 위치를 보정해주자
			//Load 후 단 한번만
			float meshGroupScaleRatio = (float)_selectedPSDSet._next_meshGroupScaleX100 * 0.01f;
			
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				psdLayer = _psdLoader.PSDLayerDataList[i];

				if(!psdLayer._isRemapSelected)
				{
					continue;
				}
				if(psdLayer._isRemapPosOffset_Initialized)
				{
					//이미 위치가 초기화 되었으면 패스
				}

				float prevLocalPos_X = 0;
				float prevLocalPos_Y = 0;
				float prevPosOffset_X = 0.0f;
				float prevPosOffset_Y = 0.0f;
				bool isLocalPosCalculatable = false;

				if(_selectedPSDSet._isLastBaked)
				{
					//1. 이전에 Bake된 적이 있다면
					// Bake되었을 때의 PosOffset을 기본적으로 사용하자
					
					apPSDSetLayer psdSetLayer = null;
					if(psdLayer._remap_MeshTransform != null)
					{
						psdSetLayer = _selectedPSDSet.GetLayer(psdLayer._remap_MeshTransform);
					}
					else if(psdLayer._remap_MeshGroupTransform != null)
					{
						psdSetLayer = _selectedPSDSet.GetLayer(psdLayer._remap_MeshGroupTransform);
					}
					if(psdSetLayer != null)
					{
						//prevPosOffset_X = psdSetLayer._bakedLocalPosOffset_X;
						//prevPosOffset_Y = psdSetLayer._bakedLocalPosOffset_Y;
						prevPosOffset_X = 0;
						prevPosOffset_Y = 0;
						prevLocalPos_X = 0.0f;
						prevLocalPos_Y = 0.0f;
						isLocalPosCalculatable = true;
					}
				}
				else
				{
					//2. 이전에 Bake된 적이 없었다면
					//현재 LocalPos를 기준으로
					//Offset = Cur Local Pos - Prev Local Pos이다.

					if(psdLayer._remap_MeshTransform != null)
					{
						if(psdLayer._hierarchyLevel == 0)
						{
							prevLocalPos_X = (psdLayer._remap_MeshTransform._matrix._pos.x / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.x + _selectedPSDSet._nextBakeCenterOffsetDelta_X);
							prevLocalPos_Y = (psdLayer._remap_MeshTransform._matrix._pos.y / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.y + _selectedPSDSet._nextBakeCenterOffsetDelta_Y);
						}
						else
						{
							prevLocalPos_X = psdLayer._remap_MeshTransform._matrix._pos.x / meshGroupScaleRatio;
							prevLocalPos_Y = psdLayer._remap_MeshTransform._matrix._pos.y / meshGroupScaleRatio;
						}
						prevPosOffset_X = psdLayer._posOffsetLocal.x;
						prevPosOffset_Y = psdLayer._posOffsetLocal.y;
						isLocalPosCalculatable = true;
					}
					else if(psdLayer._remap_MeshGroupTransform != null)
					{
						if(psdLayer._hierarchyLevel == 0)
						{
							prevLocalPos_X = (psdLayer._remap_MeshGroupTransform._matrix._pos.x / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.x + _selectedPSDSet._nextBakeCenterOffsetDelta_X);
							prevLocalPos_Y = (psdLayer._remap_MeshGroupTransform._matrix._pos.y / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.y + _selectedPSDSet._nextBakeCenterOffsetDelta_Y);
						}
						else
						{
							prevLocalPos_X = psdLayer._remap_MeshGroupTransform._matrix._pos.x / meshGroupScaleRatio;
							prevLocalPos_Y = psdLayer._remap_MeshGroupTransform._matrix._pos.y / meshGroupScaleRatio;
						}
						prevPosOffset_X = psdLayer._posOffsetLocal.x;
						prevPosOffset_Y = psdLayer._posOffsetLocal.y;
						isLocalPosCalculatable = true;
					}
					//psdLayer._remapPosOffsetDelta_X = psdLayer._posOffsetLocal.x;
				}
				
				if(isLocalPosCalculatable)
				{
					psdLayer._remapPosOffsetDelta_X = prevPosOffset_X - prevLocalPos_X;
					psdLayer._remapPosOffsetDelta_Y = prevPosOffset_Y - prevLocalPos_Y;
					psdLayer._isRemapPosOffset_Initialized = true;
				}
				else
				{
					psdLayer._remapPosOffsetDelta_X = 0;
					psdLayer._remapPosOffsetDelta_Y = 0;
					//psdLayer._isRemapPosOffset_Initialized = true;
				}

			}
		}

		private void MakeTransformDataList()
		{
			_targetTransformList.Clear();

			if(_selectedPSDSet == null ||
				_selectedPSDSet._linkedTargetMeshGroup == null)
			{
				return;
			}

			List<apMesh> meshes = new List<apMesh>();//<<MeshTransform의 중복 메시를 막기 위해

			//apRenderUnit curRenderUnit = null;

			//이전 버전
			//for (int i = 0; i < _selectedPSDSet._linkedTargetMeshGroup._renderUnits_All.Count; i++)
			//{
			//	curRenderUnit = _selectedPSDSet._linkedTargetMeshGroup._renderUnits_All[i];
			//	if(curRenderUnit._meshTransform != null)
			//	{
			//		if(curRenderUnit._meshTransform._mesh != null)
			//		{
			//			bool isValidMesh = !meshes.Contains(curRenderUnit._meshTransform._mesh);//유니크한 Mesh일때

			//			_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshTransform, isValidMesh));

			//			if(isValidMesh)
			//			{
			//				meshes.Add(curRenderUnit._meshTransform._mesh);
			//			}
			//		}
			//	}
			//	else if(curRenderUnit._meshGroupTransform != null)
			//	{
			//		//Root가 아니라면
			//		if (_selectedPSDSet._linkedTargetMeshGroup._rootMeshGroupTransform != curRenderUnit._meshGroupTransform)
			//		{
			//			_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshGroupTransform));
			//		}
			//	}
			//}

			MakeTransformDataListRecursive(_selectedPSDSet._linkedTargetMeshGroup._rootRenderUnit, _selectedPSDSet._linkedTargetMeshGroup._rootRenderUnit, meshes);
			
		}
		private void MakeTransformDataListRecursive(apRenderUnit curRenderUnit, apRenderUnit rootUnit, List<apMesh> uniqueMeshes)
		{
			if (curRenderUnit._childRenderUnits != null)
			{
				for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
				{
					//자식을 먼저 넣자
					MakeTransformDataListRecursive(curRenderUnit._childRenderUnits[i], rootUnit, uniqueMeshes);
				}
			}

			if(curRenderUnit != rootUnit)
			{
				if(curRenderUnit._meshTransform != null)
				{
					if(curRenderUnit._meshTransform._mesh != null)
					{
						bool isValidMesh = !uniqueMeshes.Contains(curRenderUnit._meshTransform._mesh);//유니크한 Mesh일때

						_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshTransform, isValidMesh));

						if(isValidMesh)
						{
							uniqueMeshes.Add(curRenderUnit._meshTransform._mesh);
						}
					}
				}
				else if(curRenderUnit._meshGroupTransform != null)
				{
					//Root가 아니라면
					if (_selectedPSDSet._linkedTargetMeshGroup._rootMeshGroupTransform != curRenderUnit._meshGroupTransform)
					{
						_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshGroupTransform));
					}
				}
			}
		}


		private void RefreshTransform2PSDLayer()
		{	
			_meshTransform2PSDLayer.Clear();
			_meshGroupTransform2PSDLayer.Clear();

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			if(_selectedPSDSet == null)
			{
				return;
			}
			apPSDLayerData psdLayer = null;

			//일단 모두 초기화
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				psdLayer = _psdLoader.PSDLayerDataList[i];
				if(!psdLayer._isRemapSelected)
				{
					continue;
				}
				if (psdLayer._isImageLayer)
				{
					if (psdLayer._remap_MeshTransform != null)
					{
						if(!_meshTransform2PSDLayer.ContainsKey(psdLayer._remap_MeshTransform))
						{
							_meshTransform2PSDLayer.Add(psdLayer._remap_MeshTransform, psdLayer);
						}
						else
						{
							//이미 존재한다면!?
							//다른곳에 이미 연결되었을 것이므로 이 데이터를 날린다.
							psdLayer._isRemapSelected = false;
							psdLayer._remap_TransformID = -1;
							psdLayer._remap_MeshTransform = null;
							psdLayer._remap_MeshGroupTransform = null;
						}
					}
				}
				else
				{
					if(psdLayer._remap_MeshGroupTransform != null)
					{
						if(!_meshGroupTransform2PSDLayer.ContainsKey(psdLayer._remap_MeshGroupTransform))
						{
							_meshGroupTransform2PSDLayer.Add(psdLayer._remap_MeshGroupTransform, psdLayer);
						}
						else
						{
							//이미 존재한다면!?
							//다른곳에 이미 연결되었을 것이므로 이 데이터를 날린다.
							psdLayer._isRemapSelected = false;
							psdLayer._remap_TransformID = -1;
							psdLayer._remap_MeshTransform = null;
							psdLayer._remap_MeshGroupTransform = null;
						}
					}
				}
			}
		}



		private void LinkPSDLayerAndTransform(apPSDLayerData psdLayerData, TargetTransformData transformData)
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded || psdLayerData == null || transformData == null)
			{
				return;
			}
			//일단, 이 TransformData (또는 공유하는 Mesh)를 참고하고 있는 것이 있다면 연결 해제
			apPSDLayerData curPSDLayer = null;
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				if(!curPSDLayer._isRemapSelected)
				{
					continue;
				}
				bool isRelease = false;
				if(transformData._meshTransform != null)
				{
					if(curPSDLayer._remap_MeshTransform != null)
					{
						//1. 같은거라면 해제
						//2. Mesh가 같아도 해제
						if(curPSDLayer._remap_MeshTransform == transformData._meshTransform)
						{
							isRelease = true;
						}
						else if(curPSDLayer._remap_MeshTransform._mesh == transformData._meshTransform._mesh)
						{
							isRelease = true;
						}
					}
				}
				else if(transformData._meshGroupTransform != null)
				{
					if(curPSDLayer._remap_MeshGroupTransform != null)
					{
						//같은거 해제
						if(curPSDLayer._remap_MeshGroupTransform == transformData._meshGroupTransform)
						{
							isRelease = true;
						}
					}
				}
				if (isRelease)
				{
					curPSDLayer._isRemapSelected = false;
					curPSDLayer._remap_MeshTransform = null;
					curPSDLayer._remap_TransformID = -1;
					curPSDLayer._remap_MeshGroupTransform = null;
				}
			}

			//선택된걸 연결하자
			if(psdLayerData._isImageLayer && transformData._meshTransform != null)
			{
				psdLayerData._isRemapSelected = true;
				psdLayerData._remap_MeshTransform = transformData._meshTransform;
				psdLayerData._remap_TransformID = transformData._meshTransform._transformUniqueID;
				psdLayerData._remap_MeshGroupTransform = null;
			}
			else if(!psdLayerData._isImageLayer && transformData._meshGroupTransform != null)
			{
				psdLayerData._isRemapSelected = true;
				psdLayerData._remap_MeshTransform = null;
				psdLayerData._remap_TransformID = transformData._meshGroupTransform._transformUniqueID;
				psdLayerData._remap_MeshGroupTransform = transformData._meshGroupTransform;
			}
			
			RefreshTransform2PSDLayer();
		}


		private void UnlinkPSDLayer(apPSDLayerData psdLayerData)
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded || psdLayerData == null)
			{
				return;
			}

			psdLayerData._isRemapSelected = false;
			psdLayerData._remap_MeshTransform = null;
			psdLayerData._remap_TransformID = -1;
			psdLayerData._remap_MeshGroupTransform = null;
			
			RefreshTransform2PSDLayer();
		}

		private void LinkTool_AutoMapping()
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Auto Mapping", "Do you want to automatically link layers to Mesh Group?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			RefreshTransform2PSDLayer();

			//연결이 안된 것들을 순회
			//연결안된 리스트를 취합하자
			apPSDLayerData curLayer = null;
			TargetTransformData curTransform = null;
			List<apPSDLayerData> _notLinkedPSDLayers = new List<apPSDLayerData>();
			List<TargetTransformData> _notLinkedTransforms = new List<TargetTransformData>();

			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				if(curLayer._isBakable)
				{
					if(!curLayer._isRemapSelected
						|| (curLayer._remap_MeshTransform == null && curLayer._remap_MeshGroupTransform == null)
						)
					{
						_notLinkedPSDLayers.Add(curLayer);
					}
				}
			}

			for (int i = 0; i < _targetTransformList.Count; i++)
			{
				curTransform = _targetTransformList[i];
				if(curTransform._meshTransform != null && curTransform._isValidMesh)
				{
					if(!_meshTransform2PSDLayer.ContainsKey(curTransform._meshTransform))
					{
						_notLinkedTransforms.Add(curTransform);
					}
				}
				else if(curTransform._meshGroupTransform != null)
				{
					if(!_meshGroupTransform2PSDLayer.ContainsKey(curTransform._meshGroupTransform))
					{
						_notLinkedTransforms.Add(curTransform);
					}
				}
				
			}

			//이제 연결을 하자
			//이름을 기준으로 처리
			//각 단계별로 처리 후 리스트에서 제외하자
			//1. 이름이 같은게 1개 있다면 그것을 선택
			//2. 이름이 같은 레이어가 여러개 있다면 인덱스 차이가 가장 적은것을 선택
			//3. 이름이 같은 레이어가 없는건 이름 문자열 비교해서 차이가 가장 적은 것을 선택
			TargetTransformData resultTransformData = null;
			for (int iPSD = 0; iPSD < _notLinkedPSDLayers.Count; iPSD++)
			{
				curLayer = _notLinkedPSDLayers[iPSD];
				
				List<TargetTransformData> sameNameTargets = _notLinkedTransforms.FindAll(delegate(TargetTransformData a)
				{
					return string.Equals(curLayer._name, a.Name)
							&& curLayer._isImageLayer == a._isMeshTransform;
				});

				resultTransformData = null;
				if(sameNameTargets.Count == 1)
				{
					//1. 같은게 한개가 있다. > 바로 연결
					resultTransformData = sameNameTargets[0];
					
				}
				else if(sameNameTargets.Count > 1)
				{
					//2. 같은게 여러개 있다. > 레이어 인덱스가 가장 적게 차이나는 것 선택
					int iMinIndex = -1;
					int minIndexDiff = 100;
					for (int iSub = 0; iSub < sameNameTargets.Count; iSub++)
					{
						int diff = Mathf.Abs(curLayer._layerIndex - _notLinkedTransforms.IndexOf(sameNameTargets[iSub]));
						if(diff < minIndexDiff)
						{
							minIndexDiff = diff;
							iMinIndex = iSub;
						}
					}
					if(iMinIndex >= 0)
					{
						resultTransformData = sameNameTargets[iMinIndex];
					}
				}

				if(resultTransformData != null)
				{
					curLayer._isRemapSelected = true;
					curLayer._remap_MeshTransform = resultTransformData._meshTransform;
					curLayer._remap_MeshGroupTransform = resultTransformData._meshGroupTransform;
					if (curLayer._remap_MeshTransform != null)
					{
						curLayer._remap_TransformID = curLayer._remap_MeshTransform._transformUniqueID;
					}
					else
					{
						curLayer._remap_TransformID = curLayer._remap_MeshGroupTransform._transformUniqueID;
					}

					_notLinkedTransforms.Remove(resultTransformData);//<<선택한건 리스트에서 제외
				}
			}

			//다음 처리를 위해 연결이 된 것들은 제외하자
			_notLinkedPSDLayers.RemoveAll(delegate(apPSDLayerData a)
			{
				return a._isRemapSelected;
			});

			//이름이 같은걸 못찾았으니 "유사한 이름 순으로 찾자"
			//유사도가 가장 높은걸 선택
			for (int iPSD = 0; iPSD < _notLinkedPSDLayers.Count; iPSD++)
			{
				curLayer = _notLinkedPSDLayers[iPSD];
				int iMaxSim = -1;
				int maxSim = -100;
				resultTransformData = null;
				for (int iTD = 0; iTD < _notLinkedTransforms.Count; iTD++)
				{
					curTransform = _notLinkedTransforms[iTD];
					if(curLayer._isImageLayer != curTransform._isMeshTransform)
					{
						continue;
					}
					int sim = GetNameSimilarity(curLayer._name, curTransform.Name);
					if (sim > 0)
					{
						if (iMaxSim < 0 || sim > maxSim)
						{
							iMaxSim = iTD;
							sim = maxSim;
						}
					}
				}
				if(iMaxSim >= 0)
				{
					resultTransformData = _notLinkedTransforms[iMaxSim];
				}

				if(resultTransformData != null)
				{
					curLayer._isRemapSelected = true;
					curLayer._remap_MeshTransform = resultTransformData._meshTransform;
					curLayer._remap_MeshGroupTransform = resultTransformData._meshGroupTransform;
					if (curLayer._remap_MeshTransform != null)
					{
						curLayer._remap_TransformID = curLayer._remap_MeshTransform._transformUniqueID;
					}
					else
					{
						curLayer._remap_TransformID = curLayer._remap_MeshGroupTransform._transformUniqueID;
					}

					_notLinkedTransforms.Remove(resultTransformData);//<<선택한건 리스트에서 제외
				}
			}

			RefreshTransform2PSDLayer();
		}


		private int GetNameSimilarity(string strA, string strB)
		{
			//1. 가장 길게 동일한 글자를 찾자
			int lengthSame = 0;
			if(strA.Length > strB.Length)
			{
				//처리를 위해서 A가 더 짧아야 한다.
				string strTmp = strA;
				strA = strB;
				strB = strTmp;
			}
			
			char curA;
			char curB;
			for (int iStartA = 0; iStartA < strA.Length; iStartA++)
			{
				for (int iStartB = 0; iStartB < strB.Length; iStartB++)
				{
					curA = strA[iStartA];
					curB = strB[iStartB];

					if(curA == curB)
					{
						//시작지점이 같다면 카운트 시작
						int iCount = 1;
						while(true)
						{
							if(iStartA + iCount >= strA.Length)
							{
								break;
							}
							if(iStartB + iCount >= strB.Length)
							{
								break;
							}

							curA = strA[iStartA + iCount];
							curB = strB[iStartB + iCount];
							if(curA != curB)
							{
								break;
							}
							iCount++;
						}

						if(iCount > lengthSame)
						{
							//동일한 글자 길이가 더 길면 갱신
							lengthSame = iCount;
						}
					}
				}
			}
			if(lengthSame == 1)
			{
				return -1;
			}

			

			return (lengthSame * 100) - Mathf.Abs(string.Compare(strA, strB));

		}


		


		private void LinkTool_EnableAll()
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Enable All", "Do you want to enable all layers?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isBakable = true;
			}

			RefreshTransform2PSDLayer();
			
		}

		private void LinkTool_DisableAll()
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Disable All", "Do you want to disable all layers?\n(All links will be disconnected.)", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isBakable = false;
				//Bakable가 False라면 연결 모두 해제
				curLayer._isRemapSelected = false;
				curLayer._remap_MeshGroupTransform = null;
				curLayer._remap_MeshTransform = null;
				curLayer._remap_TransformID = -1;
			}

			RefreshTransform2PSDLayer();
		}

		private void LinkTool_Reset()
		{
			if(_selectedPSDSet == null || !_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Reset", "Do you want to reset all layers?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isRemapSelected = false;
				curLayer._remap_TransformID = -1;
				curLayer._remap_MeshTransform = null;
				curLayer._remap_MeshGroupTransform = null;
			}

			RefreshTransform2PSDLayer();
		}

		

		private float GetCorrectedFloat(float value)
		{
			return (float)((int)(value * 1000.0f)) * 0.001f;
		}

		//----------------------------------------------------------------------------------------------
		//private bool LoadPsdFile(string filePath, apPSDSet psdSet)
		//{

		//	PsdDocument psdDoc = null;
		//	try
		//	{
		//		ClearPsdFile();

		//		psdDoc = PsdDocument.Create(filePath);
		//		if (psdDoc == null)
		//		{
		//			//EditorUtility.DisplayDialog("PSD Load Failed", "No File Loaded [" + filePath + "]", "Okay");
		//			EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//											_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadPath, filePath),
		//											_editor.GetText(TEXT.Close)
		//											);
		//			return false;
		//		}
		//		psdSet._filePath = filePath;
		//		psdSet._fileNameOnly = "";

		//		if (psdSet._filePath.Length > 4)
		//		{
		//			for (int i = psdSet._filePath.Length - 5; i >= 0; i--)
		//			{
		//				string curChar = psdSet._filePath.Substring(i, 1);
		//				if (curChar == "\\" || curChar == "/")
		//				{
		//					break;
		//				}
		//				psdSet._fileNameOnly = curChar + psdSet._fileNameOnly;
		//			}
		//		}
				
		//		psdSet._imageWidth = psdDoc.FileHeaderSection.Width;
		//		psdSet._imageHeight = psdDoc.FileHeaderSection.Height;
		//		psdSet._imageCenterPosOffset = new Vector2((float)psdSet._imageWidth * 0.5f, (float)psdSet._imageHeight * 0.5f);

		//		if (psdSet._imageWidth > PSD_IMAGE_FILE_MAX_SIZE || psdSet._imageHeight > PSD_IMAGE_FILE_MAX_SIZE)
		//		{
		//			//EditorUtility.DisplayDialog("PSD Load Failed", 
		//			//	"Image File is Too Large [ " + _imageWidth + " x " + _imageHeight + " ] (Maximum 5000 x 5000)", 
		//			//	"Okay");

		//			EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//											_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadSize, psdSet._imageWidth, psdSet._imageHeight),
		//											_editor.GetText(TEXT.Close)
		//											);
		//			ClearPsdFile();
		//			return false;
		//		}

				

		//		int curLayerIndex = 0;

		//		RecursiveAddLayer(psdDoc.Childs, 0, null, curLayerIndex);

		//		//클리핑이 가능한가 체크
		//		CheckClippingValidation();

		//		//파일 로드 성공
		//		psdSet._isFileLoaded = true;

		//		psdDoc.Dispose();
		//		psdDoc = null;
		//		System.GC.Collect();

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		ClearPsdFile();

		//		if (psdDoc != null)
		//		{
		//			psdDoc.Dispose();
		//			System.GC.Collect();
		//		}

		//		Debug.LogError("Load PSD File Exception : " + ex);

		//		//EditorUtility.DisplayDialog("PSD Load Failed", "Error Occured [" + ex.ToString() + "]", "Okay");
		//		EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//										_editor.GetTextFormat(TEXT.PSDBakeError_Body_ErrorCode, ex.ToString()),
		//										_editor.GetText(TEXT.Close)
		//										);

		//	}

		//	return false;
		//}

		//private void ClearPsdFile(apPSDSet psdSet)
		//{
		//	psdSet.ReadyToLoad();
		//	//psdSet._isFileLoaded = false;
		//	//_fileFullPath = "";
		//	//_fileNameOnly = "";
		//	//_imageWidth = -1;
		//	//_imageHeight = -1;
		//	//_imageCenterPosOffset = Vector2.zero;

		//	//_layerDataList.Clear();
		//	//_selectedLayerData = null;

		//	//_bakeDataList.Clear();
		//	//_selectedBakeData = null;

		//	////_isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
		//	//_bakeWidth = BAKE_SIZE.s1024;
		//	//_bakeHeight = BAKE_SIZE.s1024;
		//	//_bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
		//	//_bakeMaximumNumAtlas = 2;
		//	//_bakePadding = 4;
		//	//_bakeBlurOption = true;

		//	//_isNeedBakeCheck = true;
		//	////_needBakeResizeX100 = 100;
		//	//_bakeParams.Clear();

		//	//_loadKey_CheckBake = null;
		//	//_loadKey_Bake = null;

		//	//_resultAtlasCount = 0;
		//	//_resultBakeResizeX100 = 0;
		//	//_resultPadding = 0;
		//}

		//private int RecursiveAddLayer(IPsdLayer[] layers, int level, apPSDLayerData parentLayerData, int curLayerIndex)
		//{
		//	for (int i = 0; i < layers.Length; i++)
		//	{
		//		IPsdLayer curLayer = layers[i];
		//		if (curLayer == null)
		//		{
		//			continue;
		//		}

		//		apPSDLayerData newLayerData = new apPSDLayerData(curLayerIndex, curLayer, _imageWidth, _imageHeight);
		//		newLayerData.SetLevel(level);
		//		if (parentLayerData != null)
		//		{
		//			parentLayerData.AddChildLayer(newLayerData);
		//		}

		//		curLayerIndex++;

		//		//재귀 호출을 하자
		//		if (curLayer.Childs != null && curLayer.Childs.Length > 0)
		//		{
		//			curLayerIndex = RecursiveAddLayer(curLayer.Childs, level + 1, newLayerData, curLayerIndex);
		//		}

		//		_layerDataList.Add(newLayerData);
		//	}
		//	return curLayerIndex;
		//}

		//private void MakePosOffsetLocals(List<apPSDLayerData> layerList, int curLevel, apPSDLayerData parentLayer)
		//{
		//	for (int i = 0; i < layerList.Count; i++)
		//	{
		//		apPSDLayerData curLayer = layerList[i];
		//		if (curLayer._hierarchyLevel != curLevel)
		//		{
		//			continue;
		//		}

		//		if (parentLayer != null)
		//		{
		//			curLayer._posOffsetLocal = curLayer._posOffset - parentLayer._posOffset;
		//		}
		//		else
		//		{
		//			curLayer._posOffsetLocal = curLayer._posOffset;
		//		}
		//	}
		//}

		//private void CheckClippingValidation()
		//{
		//	//Debug.Log("CheckClippingValidation");
		//	//클리핑이 가능한가 체크
		//	//어떤 클리핑 옵션이 나올때
		//	//"같은 레벨에서" ㅁ CC[C] 까지는 Okay / ㅁCCC..[C]는 No
		//	for (int i = 0; i < _layerDataList.Count; i++)
		//	{
		//		apPSDLayerData curLayerData = _layerDataList[i];
		//		curLayerData._isClippingValid = true;

		//		if (curLayerData._isImageLayer && curLayerData._isClipping)
		//		{
		//			//앞으로 체크해보자.
		//			int curLevel = curLayerData._hierarchyLevel;

		//			apPSDLayerData prev1_Layer = null;
		//			apPSDLayerData prev2_Layer = null;
		//			apPSDLayerData prev3_Layer = null;

		//			if (i - 1 >= 0)
		//			{ prev1_Layer = _layerDataList[i - 1]; }
		//			if (i - 2 >= 0)
		//			{ prev2_Layer = _layerDataList[i - 2]; }
		//			if (i - 3 >= 0)
		//			{ prev3_Layer = _layerDataList[i - 3]; }

		//			bool isValiePrev1 = (prev1_Layer != null && prev1_Layer._isBakable && prev1_Layer._isImageLayer && !prev1_Layer._isClipping && prev1_Layer._hierarchyLevel == curLevel);
		//			bool isValiePrev2 = (prev2_Layer != null && prev2_Layer._isBakable && prev2_Layer._isImageLayer && !prev2_Layer._isClipping && prev2_Layer._hierarchyLevel == curLevel);
		//			bool isValiePrev3 = (prev3_Layer != null && prev3_Layer._isBakable && prev3_Layer._isImageLayer && !prev3_Layer._isClipping && prev3_Layer._hierarchyLevel == curLevel);
		//			if (isValiePrev1 || isValiePrev2 || isValiePrev3)
		//			{
		//				curLayerData._isClippingValid = true;
		//			}
		//			else
		//			{
		//				//Clipping의 대상이 없다면 문제가 있다.
		//				//Debug.LogError("Find Invalid Clipping [" + curLayerData._name + "]");
		//				curLayerData._isClippingValid = false;
		//			}
		//		}
		//	}
		//}
	}
}