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
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	//ModifiedMesh의 Extra Option을 설정한다.
	public class apDialog_ExtraOption : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		//public delegate void FUNC_EXTRA_OPTION_CHANGED(object loadKey, 
		//	bool isAnimEdit,
		//	apMeshGroup meshGroup, 
		//	apModifierBase modifier, 
		//	apModifiedMesh modMesh,
		//	bool isExtraOptionEnabled,

		//	);

		private static apDialog_ExtraOption s_window = null;

		private apEditor _editor;
		private apPortrait _portrait;
		
		private apMeshGroup _meshGroup;
		private apModifierBase _modifier;
		private apModifiedMesh _modMesh;
		private apRenderUnit _renderUnit;

		private bool _isAnimEdit;
		private apAnimClip _animClip = null;
		private apAnimKeyframe _keyframe = null;

		private Vector2 _scrollList = new Vector2();//<<Depth 바꿀때 쓰는 리스트

		private apSelection.SELECTION_TYPE _selectionType = apSelection.SELECTION_TYPE.None;

		private int _targetDepth = 0;

		private Texture2D _img_DepthCursor = null;
		private Texture2D _img_DepthMidCursor = null;
		private Texture2D _img_MeshTF = null;
		private Texture2D _img_MeshGroupTF = null;

		private apTextureData _srcTexureData = null;
		private apTextureData _dstTexureData = null;
		private bool _isImageChangable = false;

		
		private enum TAB
		{
			Depth,
			Image
		}
		private TAB _tab = TAB.Depth;

		public class SubUnit
		{
			public bool _isRoot = false;
			public int _level = 0;
			public bool _isTarget = false;
			public string _name = null;
			public int _depth = 0;
			public bool _isMeshTransform = false;
			public List<SubUnit> _childUnits = new List<SubUnit>();

			public SubUnit(apRenderUnit renderUnit, int level, bool isTarget, bool isRoot)
			{
				_isRoot = isRoot;
				_level = level;
				_isTarget = isTarget;
				_name = renderUnit.Name;
				_depth = renderUnit.GetDepth();

				_isMeshTransform = (renderUnit._meshTransform != null);

				_childUnits.Clear();
			}
		}

		
		private List<SubUnit> _subUnits_All = new List<SubUnit>();

		private enum DEPTH_CURSOR_TYPE
		{
			None,
			Mid,
			Target
		}


		// GUIContents 들
		apGUIContentWrapper _guiContent_DepthMidCursor = null;
		apGUIContentWrapper _guiContent_DepthCursor = null;
		apGUIContentWrapper _guiContent_MeshIcon = null;
		apGUIContentWrapper _guiContent_MeshGroupIcon = null;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(	apEditor editor, 
											apPortrait portrait,
											apMeshGroup meshGroup, 
											apModifierBase modifier, 
											apModifiedMesh modMesh, 
											apRenderUnit renderUnit,
											bool isAnimEdit,
											apAnimClip animClip,
											apAnimKeyframe keyframe)
		{
			CloseDialog();

			if (editor == null 
				|| editor._portrait == null 
				|| meshGroup == null 
				|| modifier == null 
				|| modMesh == null)
			{
				return null;
			}




			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ExtraOption), true, "Extra Properties", true);
			apDialog_ExtraOption curTool = curWindow as apDialog_ExtraOption;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				
				int height = 620;
				if(isAnimEdit)
				{
					height = 715;
				}
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, portrait, meshGroup, modifier, modMesh, renderUnit, isAnimEdit, animClip, keyframe);

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
		//------------------------------------------------------------------------------------------------
		private void Init(apEditor editor,
							apPortrait portrait,
							apMeshGroup meshGroup,
							apModifierBase modifier,
							apModifiedMesh modMesh,
							apRenderUnit renderUnit,
							bool isAnimEdit,
							apAnimClip animClip,
							apAnimKeyframe keyframe)
		{
			_editor = editor;
			_portrait = portrait;
			_meshGroup = meshGroup;
			_modifier = modifier;
			_modMesh = modMesh;
			_renderUnit = modMesh._renderUnit;
			

			_isAnimEdit = isAnimEdit;
			_animClip = animClip;
			_keyframe = keyframe;
			_selectionType = _editor.Select.SelectionType;

			_targetDepth = renderUnit.GetDepth();

			_img_DepthCursor = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_DepthCursor);
			_img_DepthMidCursor = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_DepthMidCursor);
			_img_MeshTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_MeshGroupTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);

			_subUnits_All.Clear();


			apRenderUnit parentUnit = _renderUnit._parentRenderUnit;
			apRenderUnit curRenderUnit = null;
			for (int i = 0; i < _meshGroup._renderUnits_All.Count; i++)
			{
				curRenderUnit = _meshGroup._renderUnits_All[i];

				//Parent가 같은 형제 렌더 유닛에 대해서만 처리한다.
				//단, MeshTransform일 때, Clipping Child는 생략한다.
				if(curRenderUnit._meshTransform != null && curRenderUnit._meshTransform._isClipping_Child)
				{
					continue;
				}

				if (curRenderUnit._parentRenderUnit != parentUnit)
				{
					continue;
				}
				
				SubUnit subUnit = new SubUnit(curRenderUnit, curRenderUnit._level, (curRenderUnit == _renderUnit), (curRenderUnit == _meshGroup._rootRenderUnit));
				_subUnits_All.Add(subUnit);
			}

			_subUnits_All.Sort(delegate(SubUnit a, SubUnit b)
			{
				return b._depth - a._depth;
			});

			//여기서는 실제 Depth보다 상대적 Depth만 고려한다.
			int curDepth = 0;
			for (int i = _subUnits_All.Count - 1; i >= 0; i--)
			{
				_subUnits_All[i]._depth = curDepth;
				if(_subUnits_All[i]._isTarget)
				{
					_targetDepth = curDepth;
				}
				curDepth++;
			}

			//이미지를 바꿀 수 있는가
			RefreshImagePreview();
			
		}
		
		private void RefreshImagePreview()
		{
			_srcTexureData = null;
			_dstTexureData = null;
			_isImageChangable = false;

			if(_modMesh._isMeshTransform && _modMesh._transform_Mesh != null)
			{
				if(_modMesh._transform_Mesh._mesh != null
					&& _modMesh._transform_Mesh._mesh._textureData_Linked != null)
				{
					apTextureData linkedTextureData = _modMesh._transform_Mesh._mesh._textureData_Linked;
					
					_isImageChangable = true;

					_srcTexureData = linkedTextureData;

					if(_modMesh._extraValue._textureDataID >= 0)
					{
						_dstTexureData = _portrait.GetTexture(_modMesh._extraValue._textureDataID);
						if(_dstTexureData == null)
						{
							_modMesh._extraValue._textureDataID = -1;
						}
					}

				}
			}
		}


		// GUI
		//------------------------------------------------------------------------------------------------
		private void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;

			width -= 10;

			//만약 이 다이얼로그가 켜진 상태로 뭔가 바뀌었다면 종료
			bool isClose = false;
			bool isMoveAnimKeyframe = false;
			bool isMoveAnimKeyframeToNext = false;
			if(_editor == null || _meshGroup == null || _modifier == null || _modMesh == null || _renderUnit == null)
			{
				//데이터가 없다.
				isClose = true;
			}
			else if(_editor.Select.SelectionType != _selectionType)
			{
				//선택 타입이 바뀌었다.
				isClose = true;
			}
			else
			{
				
				if(!_isAnimEdit)
				{
					//1. 일반 모디파이어 편집시
					//- 현재 선택된 MeshGroup이 바뀌었다면
					//- 현재 선택된 Modifier가 바뀌었다면
					//- Modifier 메뉴가 아니라면
					//- ExEditingMode가 꺼졌다면
					//> 해제
					if(_editor.Select.ExEditingMode == apSelection.EX_EDIT.None
						|| _editor.Select.MeshGroup != _meshGroup
						|| _editor.Select.Modifier != _modifier
						|| _editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Modifier)
					{
						isClose = true;
					}
				}
				else
				{
					//2. 애니메이션 편집 시
					//- 현재 선택된 AnimationClip이 바뀌었다면
					//- 현재 선택된 MeshGroup이 바뀌었다면 (AnimClip이 있을 때)
					//- AnimExEditingMode가 꺼졌다면
					//- 재생 중이라면
					//> 해제
					if(_editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None
						|| _editor.Select.AnimClip != _animClip
						|| _animClip == null
						|| _keyframe == null
						|| _editor.Select.AnimClip._targetMeshGroup != _meshGroup
						|| _editor.Select.IsAnimPlaying)
					{
						isClose = true;
					}
				}
			}

			if(isClose)
			{
				CloseDialog();
				return;
			}


			//------------------------------------------------------------
			
			//1. 선택된 객체 정보
			//- RenderUnit 아이콘과 이름
			
			//2. <애니메이션인 경우>
			//- 현재 키프레임과 키프레임 이동하기

			//3. 가중치
			//- [일반] Weight CutOut
			//- [Anim] Prev / Next CutOut

			// (구분선)

			//4. Depth
			//- 아이콘과 Chainging Depth 레이블
			//- On / Off 버튼
			//- Depth 증감과 리스트 (좌우에 배치)

			//5. Texture (RenderUnit이 MeshTransform인 경우)
			//- 현재 텍스쳐
			//- 바뀔 텍스쳐
			//- 텍스쳐 선택하기 버튼

			//1. 선택된 객체 정보
			//- RenderUnit 아이콘과 이름
			//int iconSize = 25;
			GUIStyle guiStyle_TargetBox = new GUIStyle(GUI.skin.box);
			guiStyle_TargetBox.alignment = TextAnchor.MiddleCenter;
			Color prevColor = GUI.backgroundColor;

			// RenderUnit 이름
			Texture2D iconRenderUnit = null;
			if(_renderUnit._meshTransform != null)
			{
				iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			}
			else
			{
				iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			}
			GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
			GUILayout.Box(new GUIContent("  " + _renderUnit.Name, iconRenderUnit), guiStyle_TargetBox, GUILayout.Width(width), GUILayout.Height(30));
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);

			//"Extra Property ON", "Extra Property OFF"
			if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_ExtraPropertyOn), _editor.GetText(TEXT.ExtraOpt_ExtraPropertyOff), _modMesh._isExtraValueEnabled, true, width, 25))
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);

				_modMesh._isExtraValueEnabled = !_modMesh._isExtraValueEnabled;
				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<옵션의 형태가 바뀌면 Modifier의 Link를 다시 해야한다.
				apEditorUtil.ReleaseGUIFocus();
			}

			GUILayout.Space(5);

			if(_isAnimEdit)
			{
				GUILayout.Space(10);

				//2. <애니메이션인 경우>
				//- 현재 키프레임과 키프레임 이동하기

				//"Target Frame"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_TargetFrame));
				int frameBtnSize = 20;
				int frameCurBtnWidth = 100;
				int frameMoveBtnWidth = (width - (10 + frameCurBtnWidth)) / 2;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(frameBtnSize));
				GUILayout.Space(5);
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame), GUILayout.Width(frameMoveBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					//이전 프레임으로 이동하기
					isMoveAnimKeyframe = true;
					isMoveAnimKeyframeToNext = false;
				}
				if(GUILayout.Button(_keyframe._frameIndex.ToString(), GUILayout.Width(frameCurBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					_animClip.SetFrame_Editor(_keyframe._frameIndex);
					_editor.SetRepaint();
				}
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame), GUILayout.Width(frameMoveBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					//다음 프레임으로 이동하기
					isMoveAnimKeyframe = true;
					isMoveAnimKeyframeToNext = true;
				}

				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(10);
			
			//3. 가중치
			//- [일반] Weight CutOut
			//- [Anim] Prev / Next CutOut
			
			EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_WeightSettings));//"Weight Settings"
			GUILayout.Space(5);
			if(!_isAnimEdit)
			{
				//일반이면 CutOut이 1개
				float cutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_Offset), _modMesh._extraValue._weightCutout);//"Offset (0~1)"

				if(cutOut != _modMesh._extraValue._weightCutout)
				{
					cutOut = Mathf.Clamp01(cutOut);
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);

					_modMesh._extraValue._weightCutout = cutOut;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			else
			{
				//애니메이션이면 CutOut이 2개
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_Offset));
				float animPrevCutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_OffsetPrevKeyframe), _modMesh._extraValue._weightCutout_AnimPrev);//"Prev Keyframe"
				float animNextCutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_OffsetNextKeyframe), _modMesh._extraValue._weightCutout_AnimNext);//"Next Keyframe"

				if(animPrevCutOut != _modMesh._extraValue._weightCutout_AnimPrev
					|| animNextCutOut != _modMesh._extraValue._weightCutout_AnimNext
					)
				{
					animPrevCutOut = Mathf.Clamp01(animPrevCutOut);
					animNextCutOut = Mathf.Clamp01(animNextCutOut);
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);

					_modMesh._extraValue._weightCutout_AnimPrev = animPrevCutOut;
					_modMesh._extraValue._weightCutout_AnimNext = animNextCutOut;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			int tabBtnWidth = ((width - 10) / 2);
			int tabBtnHeight = 25;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(tabBtnHeight));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_Tab_Depth), _tab == TAB.Depth, tabBtnWidth, tabBtnHeight))//"Depth"
			{
				_tab = TAB.Depth;
			}
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_Tab_Image), _tab == TAB.Image, tabBtnWidth, tabBtnHeight))//"Image"
			{
				_tab = TAB.Image;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (_tab == TAB.Depth)
			{

				//4. Depth
				//- 아이콘과 Chainging Depth 레이블
				//- On / Off 버튼
				//- Depth 증감과 리스트 (좌우에 배치)

				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_ChangingDepth));//"Changing Depth"
				GUILayout.Space(5);

				//"Depth Option ON", "Depth Option OFF"
				if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_DepthOptOn), _editor.GetText(TEXT.ExtraOpt_DepthOptOff), _modMesh._extraValue._isDepthChanged, _modMesh._isExtraValueEnabled, width, 25))
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);

					_modMesh._extraValue._isDepthChanged = !_modMesh._extraValue._isDepthChanged;
					_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
					_editor.SetRepaint();
				}
				GUILayout.Space(5);

				bool isDepthAvailable = _modMesh._extraValue._isDepthChanged && _modMesh._isExtraValueEnabled;

				int depthListWidth_Left = 80;
				int depthListWidth_Right = width - (10 + depthListWidth_Left);
				int depthListWidth_RightInner = depthListWidth_Right - 20;
				int depthListHeight = 276;
				//int depthListHeight_LeftBtn = (depthListHeight - 40) / 2;
				int depthListHeight_LeftBtn = 40;
				int depthListHeight_LeftSpace = (depthListHeight - (40 + depthListHeight_LeftBtn * 2)) / 2;
				int depthListHeight_RightList = 20;

				//리스트 배경
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if(!isDepthAvailable)
				{
					GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);
				}
				GUI.Box(new Rect(5 + depthListWidth_Left + 8, lastRect.y + 8, depthListWidth_Right, depthListHeight), "");
				if(!isDepthAvailable)
				{
					GUI.backgroundColor = prevColor;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(depthListHeight));
				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight));
				// Depth List의 왼쪽
				// Depth 증감 버튼과 값
				GUILayout.Space(depthListHeight_LeftSpace);

				Texture2D img_AddWeight = _editor.ImageSet.Get(apImageSet.PRESET.Rig_AddWeight);
				Texture2D img_SubtractWeight = _editor.ImageSet.Get(apImageSet.PRESET.Rig_SubtractWeight);
				

				//if (GUILayout.Button(, GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight_LeftBtn)))
				if(apEditorUtil.ToggledButton(img_AddWeight, false, isDepthAvailable, depthListWidth_Left, depthListHeight_LeftBtn))
				{
					//Depth 증가
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);
					_modMesh._extraValue._deltaDepth++;
					_editor.SetRepaint();
					apEditorUtil.ReleaseGUIFocus();
				}

				//"Delta Depth"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_DeltaDepth), GUILayout.Width(depthListWidth_Left));
				int deltaDepth = EditorGUILayout.DelayedIntField(_modMesh._extraValue._deltaDepth, GUILayout.Width(depthListWidth_Left));
				if (deltaDepth != _modMesh._extraValue._deltaDepth)
				{
					if (isDepthAvailable)
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);
						_modMesh._extraValue._deltaDepth = deltaDepth;
						_editor.SetRepaint();
						apEditorUtil.ReleaseGUIFocus();
					}
				}

				//if (GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Rig_SubtractWeight), GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight_LeftBtn)))
				if(apEditorUtil.ToggledButton(img_SubtractWeight, false, isDepthAvailable, depthListWidth_Left, depthListHeight_LeftBtn))
				{
					//Depth 감소
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);
					_modMesh._extraValue._deltaDepth--;
					_editor.SetRepaint();
					apEditorUtil.ReleaseGUIFocus();
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_Right), GUILayout.Height(depthListHeight));
				// RenderUnit 리스트와 변환될 Depth 위치
				_scrollList = EditorGUILayout.BeginScrollView(_scrollList, false, true, GUILayout.Width(depthListWidth_Right), GUILayout.Height(depthListHeight));

				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_RightInner), GUILayout.Height(depthListHeight));
				GUILayout.Space(5);

				SubUnit curSubUnit = null;



				//int cursorDepth = _renderUnit.GetDepth() + _modMesh._extraValue._deltaDepth;
				int cursorDepth = _targetDepth + _modMesh._extraValue._deltaDepth;

				//GUI Content 생성 [11.16 수정]
				if(_guiContent_DepthMidCursor == null)
				{
					_guiContent_DepthMidCursor = apGUIContentWrapper.Make(_img_DepthMidCursor);
				}
				if(_guiContent_DepthCursor == null)
				{
					_guiContent_DepthCursor = apGUIContentWrapper.Make(_img_DepthCursor);
				}
				if(_guiContent_MeshIcon == null)
				{
					_guiContent_MeshIcon = apGUIContentWrapper.Make(_img_MeshTF);
				}
				if(_guiContent_MeshGroupIcon == null)
				{
					_guiContent_MeshGroupIcon = apGUIContentWrapper.Make(_img_MeshGroupTF);
				}
				//이전 코드
				//GUIContent guiContent_DepthMidCursor = new GUIContent(_img_DepthMidCursor);
				//GUIContent guiContent_DepthCursor = new GUIContent(_img_DepthCursor);
				//GUIContent guiContent_MeshIcon = new GUIContent(_img_MeshTF);
				//GUIContent guiContent_MeshGroupIcon = new GUIContent(_img_MeshGroupTF);

				int depthCursorSize = depthListHeight_RightList;

				for (int i = 0; i < _subUnits_All.Count; i++)
				{
					curSubUnit = _subUnits_All[i];

					if (curSubUnit._isTarget)
					{
						//타겟이면 배경색을 그려주자
						lastRect = GUILayoutUtility.GetLastRect();
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						}

						int yOffset = 6;
						if (i == 0)
						{
							yOffset = 7 - depthListHeight_RightList;
						}
						GUI.Box(new Rect(lastRect.x, lastRect.y + depthListHeight_RightList + yOffset, depthListWidth_RightInner + 10, depthListHeight_RightList), "");
						GUI.backgroundColor = prevColor;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(depthListWidth_RightInner), GUILayout.Height(depthListHeight_RightList));


					//TODO : Depth 커서 그려주기
					//GUILayout.Space(20);

					DEPTH_CURSOR_TYPE depthCursorType = DEPTH_CURSOR_TYPE.None;
					if (!curSubUnit._isTarget)
					{
						if (cursorDepth != _targetDepth)
						{
							if (cursorDepth == curSubUnit._depth)
							{
								depthCursorType = DEPTH_CURSOR_TYPE.Target;
							}
							else
							{
								if (cursorDepth > _targetDepth)
								{
									//Depth가 증가했을 때
									if (_targetDepth < curSubUnit._depth && curSubUnit._depth < cursorDepth)
									{
										depthCursorType = DEPTH_CURSOR_TYPE.Mid;
									}
								}
								else
								{
									//Depth가 감소했을 때
									if (cursorDepth < curSubUnit._depth && curSubUnit._depth < _targetDepth)
									{
										depthCursorType = DEPTH_CURSOR_TYPE.Mid;
									}
								}
							}
						}
					}
					else
					{
						if (cursorDepth != _targetDepth)
						{
							depthCursorType = DEPTH_CURSOR_TYPE.Mid;
						}
						else
						{
							depthCursorType = DEPTH_CURSOR_TYPE.Target;
						}
					}
					GUILayout.Space(5);
					switch (depthCursorType)
					{
						case DEPTH_CURSOR_TYPE.None:
							GUILayout.Space(depthCursorSize + 4);
							break;

						case DEPTH_CURSOR_TYPE.Mid:
							EditorGUILayout.LabelField(_guiContent_DepthMidCursor.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
							break;

						case DEPTH_CURSOR_TYPE.Target:
							EditorGUILayout.LabelField(_guiContent_DepthCursor.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
							break;
					}

					EditorGUILayout.LabelField(curSubUnit._isMeshTransform ? _guiContent_MeshIcon.Content : _guiContent_MeshGroupIcon.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
					EditorGUILayout.LabelField(curSubUnit._depth.ToString(), GUILayout.Width(20), GUILayout.Height(depthListHeight_RightList));
					EditorGUILayout.LabelField(curSubUnit._name,
						GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
						GUILayout.Height(depthListHeight_RightList)
						);

					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(depthListHeight + 100);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//5. Texture (RenderUnit이 MeshTransform인 경우)
				//- 현재 텍스쳐
				//- 바뀔 텍스쳐
				//- 텍스쳐 선택하기 버튼

				//"Changing Image"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_ChangingImage));
				GUILayout.Space(5);

				//"Image Option ON", "Image Option OFF"
				if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_ImageOptOn), _editor.GetText(TEXT.ExtraOpt_ImageOptOff), _modMesh._extraValue._isTextureChanged, _isImageChangable && _modMesh._isExtraValueEnabled, width, 25))
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);

					_modMesh._extraValue._isTextureChanged = !_modMesh._extraValue._isTextureChanged;
					_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
					_editor.SetRepaint();
				}
				GUILayout.Space(5);

				bool isTextureAvailable = _modMesh._extraValue._isTextureChanged && _isImageChangable && _modMesh._isExtraValueEnabled;

				int imageSlotSize = 170;
				int imageSlotSpaceSize = width - (imageSlotSize * 2 + 6 + 10);
				int imageSlotHeight = imageSlotSize + 50;

				Texture2D img_Src = null;
				Texture2D img_Dst = null;
				string strSrcName = "< None >";
				string strDstName = "< None >";

				if (_srcTexureData != null && _srcTexureData._image != null)
				{
					img_Src = _srcTexureData._image;
					strSrcName = _srcTexureData._name;
				}
				if (_dstTexureData != null && _dstTexureData._image != null)
				{
					img_Dst = _dstTexureData._image;
					strDstName = _dstTexureData._name;
				}

				GUIStyle guiStyle_ImageSlot = new GUIStyle(GUI.skin.box);
				guiStyle_ImageSlot.alignment = TextAnchor.MiddleCenter;

				GUIStyle guiStyle_ImageName = new GUIStyle(GUI.skin.label);
				guiStyle_ImageName.alignment = TextAnchor.MiddleCenter;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
				GUILayout.Space(5);

				//이미지 슬롯 1 : 원래 이미지
				EditorGUILayout.BeginVertical(GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotHeight));
				//"Original"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_SlotOriginal), GUILayout.Width(imageSlotSize));
				GUILayout.Box(img_Src, guiStyle_ImageSlot, GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strSrcName, guiStyle_ImageName, GUILayout.Width(imageSlotSize));
				EditorGUILayout.EndVertical();

				GUILayout.Space(imageSlotSpaceSize);

				//이미지 슬롯 1 : 원래 이미지
				EditorGUILayout.BeginVertical(GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotHeight));
				//"Changed"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_SlotChanged), GUILayout.Width(imageSlotSize));
				GUILayout.Box(img_Dst, guiStyle_ImageSlot, GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strDstName, guiStyle_ImageName, GUILayout.Width(imageSlotSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
				//"Set Image"
				if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_SelectImage), false, isTextureAvailable, width, 30))
				{
					//이미지 열기 열기
					_loadKey_TextureSelect = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
				}
				//"Reset Image"
				if (GUILayout.Button(_editor.GetText(TEXT.ExtraOpt_ResetImage), GUILayout.Width(width), GUILayout.Height(20)))
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);
					_modMesh._extraValue._textureDataID = -1;
					_modMesh._extraValue._linkedTextureData = null;
					
					RefreshImagePreview();

					Repaint();
				}
			}
			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
			//"Close"
			if(GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Height(30)))
			{
				isClose = true;
			}
			if(isClose)
			{
				CloseDialog();
			}

			if(isMoveAnimKeyframe)
			{
				//키프레임 이동의 경우,
				//타임라인 레이어를 따라서 전,후로 이동한다.
				//이때 ModMesh가 아예 바뀌기 때문에 여기서 처리해야한다.
				if(_keyframe != null && _keyframe._parentTimelineLayer != null && _animClip != null)
				{
					apAnimKeyframe moveKeyframe = (isMoveAnimKeyframeToNext ? _keyframe._nextLinkedKeyframe : _keyframe._prevLinkedKeyframe);
					if(moveKeyframe != null && moveKeyframe._linkedModMesh_Editor != null)
					{
						_keyframe = moveKeyframe;
						_modMesh = _keyframe._linkedModMesh_Editor;
						_animClip.SetFrame_Editor(moveKeyframe._frameIndex);

						RefreshImagePreview();

						apEditorUtil.ReleaseGUIFocus();

						Repaint();
						_editor.SetRepaint();
					}
				}
			}
		}


		//텍스쳐 선택 이벤트
		//-------------------------------------------------------------------------------------
		private object _loadKey_TextureSelect = null;
		private void OnTextureDataSelected(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if(!isSuccess)
			{
				_loadKey_TextureSelect = null;
				return;
			}

			if(_loadKey_TextureSelect != loadKey)
			{
				_loadKey_TextureSelect = null;
				return;
			}

			_loadKey_TextureSelect = null;

			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged, _editor, _modifier, _modMesh._extraValue, false);
			
			//일단 초기화
			_modMesh._extraValue._textureDataID = -1;
			_modMesh._extraValue._linkedTextureData = null;
			_dstTexureData = null;
			

			if(_modMesh != null && _modMesh._isMeshTransform && _modMesh._transform_Mesh != null)
			{
				
				if(resultTextureData != null)
				{
					_modMesh._extraValue._textureDataID = resultTextureData._uniqueID;
				}
			}


			if (_modMesh._extraValue._textureDataID >= 0)
			{
				_dstTexureData = _portrait.GetTexture(_modMesh._extraValue._textureDataID);
				_modMesh._extraValue._linkedTextureData = _dstTexureData;

				if (_dstTexureData == null)
				{
					_modMesh._extraValue._textureDataID = -1;
				}
			}

			RefreshImagePreview();

			Repaint();

			
			_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
			_editor.SetRepaint();
		}
		
	}
}