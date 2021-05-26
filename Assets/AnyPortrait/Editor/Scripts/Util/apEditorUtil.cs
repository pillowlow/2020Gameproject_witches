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

	public static class apEditorUtil
	{


		//----------------------------------------------------------------------------------------------------------
		// GUI Delimeter
		//----------------------------------------------------------------------------------------------------------
		public static void GUI_DelimeterBoxV(int height)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }
			
			GUILayout.Box(apGUIContentWrapper.Empty.Content, WhiteGUIStyle_Box, apGUILOFactory.I.Width(4), apGUILOFactory.I.Height(height));
			GUI.backgroundColor = prevColor;
		}

		public static void GUI_DelimeterBoxH(int width)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }

			GUILayout.Box(apGUIContentWrapper.Empty.Content, WhiteGUIStyle_Box, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(4));
			GUI.backgroundColor = prevColor;
		}


		//----------------------------------------------------------------------------------------------------------
		// 색상 공간
		//----------------------------------------------------------------------------------------------------------
		public static ColorSpace GetColorSpace()
		{
			return QualitySettings.activeColorSpace;
		}

		public static bool IsGammaColorSpace()
		{
			return QualitySettings.activeColorSpace != ColorSpace.Linear;
		}

		//----------------------------------------------------------------------------------------------------------
		// Vector
		//----------------------------------------------------------------------------------------------------------
		private static Vector2 _infVector2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		public static Vector2 InfVector2
		{
			get
			{
				return _infVector2;
			}
		}
		//----------------------------------------------------------------------------------------------------------
		// Set Record 계열 함수
		//----------------------------------------------------------------------------------------------------------

		//private static int _lastUndoID = -1;
		
		private static void SetRecordMeshGroupRecursive(apUndoGroupData.ACTION action, apMeshGroup meshGroup, apMeshGroup rootGroup)
		{
			if(meshGroup == null)
			{
				return;
			}
			if (meshGroup != rootGroup)
			{
				Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));
			}

			for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			{
				apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransforms[i]._meshGroup;
				if(childMeshGroup == meshGroup || childMeshGroup == rootGroup)
				{
					continue;
				}

				SetRecordMeshGroupRecursive(action, childMeshGroup, rootGroup);

			}

			////Prefab Apply
			//SetPortraitPrefabApply(meshGroup._parentPortrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Portrait(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
			
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION action,
														apEditor editor,
														apPortrait portrait,
														apMeshGroup meshGroup,
														object keyObject,
														bool isCallContinuous,
														bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			
			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}


			//Undo.FlushUndoRecordObjects();
			
			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}
			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();
			
			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					//Undo.RecordObject(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					
				}
			}

			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Mesh(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}


		


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshAndMeshGroups(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									List<apMeshGroup> meshGroups,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			List<UnityEngine.Object> recordObjects = new List<UnityEngine.Object>();
			recordObjects.Add(mesh);
			//Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			if (meshGroups != null && meshGroups.Count > 0)
			{
				for (int i = 0; i < meshGroups.Count; i++)
				{
					//Undo.RegisterCompleteObjectUndo(meshGroups[i], apUndoGroupData.GetLabel(action));
					recordObjects.Add(meshGroups[i]);
				}
			}

			Undo.RegisterCompleteObjectUndo(recordObjects.ToArray(), apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}

		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous,
									bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}
			
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}
			

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(modifier == null)
			{
				return;
			}
			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int i = 0; i < meshGroup._modifierStack._modifiers.Count; i++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				
			}

			//Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Modifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			Undo.FlushUndoRecordObjects();

			////Prefab Apply
			//SetPortraitPrefabApply(editor._portrait);
		}


		public static void SetRecordBeforeCreateOrDestroyObject(apPortrait portrait, string label)
		{
			EditorSceneManager.MarkAllScenesDirty();
			Undo.IncrementCurrentGroup();

			//Portrait, Mesh, MeshGroup, Modifier를 저장하자
			Undo.RegisterCompleteObjectUndo(portrait, label);
			//Mesh와 MeshGroup 상태 저장
			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				Undo.RegisterCompleteObjectUndo(portrait._meshes[i], label);
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], label);

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], label);
				}
			}

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Monobehaviour 객체가 생성되니 Undo로 기록할 때 호출하는 함수
		/// </summary>
		/// <param name="createdMonoObject"></param>
		/// <param name="label"></param>
		public static void SetRecordCreateMonoObject(MonoBehaviour createdMonoObject, string label)
		{
			if (createdMonoObject == null)
			{
				return;
			}
			
			Undo.RegisterCreatedObjectUndo(createdMonoObject.gameObject, label);
		

			
			//Undo.FlushUndoRecordObjects();

			
		}



		public static void SetRecordDestroyMonoObject(MonoBehaviour destroyableMonoObject, string label)
		{
			if(destroyableMonoObject == null)
			{
				return;
			}
			
			Undo.DestroyObjectImmediate(destroyableMonoObject.gameObject);

			//Undo.FlushUndoRecordObjects();

			
		}


		public static void SetRecordDestroyMonoObjects(List<MonoBehaviour> destroyableMonoObjects, string label)
		{
			if(destroyableMonoObjects == null || destroyableMonoObjects.Count == 0)
			{
				return;
			}

			for (int i = 0; i < destroyableMonoObjects.Count; i++)
			{
				Undo.DestroyObjectImmediate(destroyableMonoObjects[i].gameObject);
			}
			

			//Undo.FlushUndoRecordObjects();
		}

		


		public static int SetRecordBeforeCreateOrDestroyMultipleObjects(apPortrait portrait, string label)
		{
			EditorSceneManager.MarkAllScenesDirty();
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName(label);
			int undoID = Undo.GetCurrentGroup();
			
			//Portrait, Mesh, MeshGroup, Modifier를 저장하자
			Undo.RegisterCompleteObjectUndo(portrait, label);

			//Mesh와 MeshGroup 상태 저장
			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				Undo.RegisterCompleteObjectUndo(portrait._meshes[i], "");
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], "");

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], "");
				}
			}

			////Prefab Apply
			//SetPortraitPrefabApply(portrait);

			return undoID;
		}


		/// <summary>
		/// 여러개의 오브젝트를 한번에 생성할때 쓰이는 함수
		/// </summary>
		/// <param name="createdMonoObjects"></param>
		/// <param name="label"></param>
		public static void SetRecordCreateMultipleMonoObjects(List<MonoBehaviour> createdMonoObjects, string label, bool isBeforeFuncCalled, int undoID)
		{
			if (createdMonoObjects == null || createdMonoObjects.Count == 0)
			{
				return;
			}

			if (!isBeforeFuncCalled)
			{
				Undo.IncrementCurrentGroup();
				Undo.SetCurrentGroupName(label);
				undoID = Undo.GetCurrentGroup();
			}
			MonoBehaviour curMono = null;
			for (int i = 0; i < createdMonoObjects.Count; i++)
			{
				curMono = createdMonoObjects[i];
				if(curMono == null)
				{
					continue;
				}
				Undo.RegisterCreatedObjectUndo(createdMonoObjects[i].gameObject, "");
			}
			Undo.CollapseUndoOperations(undoID);
			
			//Undo.FlushUndoRecordObjects();
		}




		


		public static void SetEditorDirty()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		/// <summary>
		/// Undo는 "같은 메뉴"에서만 가능하다. 메뉴를 전환할 때에는 Undo를 초기화해야한다.
		/// </summary>
		public static void ResetUndo(apEditor editor)
		{
			//apUndoManager.I.Clear();
			if (editor._portrait != null)
			{
				//Undo.ClearUndo(editor._portrait);//이건 일단 빼보자
				apUndoGroupData.I.Clear();
			}
		}


		public static void OnUndoRedoPerformed()
		{
			apUndoGroupData.I.Clear();
		}
		


		


		//----------------------------------------------------------------------------------------------------------
		// Prefab Check
		//----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// 추가 20.9.14 : 이 Portrait가 Prefab으로 연결된 상태라면, 연결 정보를 복원을 위해 저장하자.
		/// Diconnected 상태에서 는 갱신하지 않는다. (null로 만들지도 않는다.)
		/// 이 정보를 삭제할 때는 Inspector에서 강제로 지정해야한다.
		/// </summary>
		/// <param name="portrait"></param>
		public static void CheckAndRefreshPrefabInfo(apPortrait portrait)
		{
			if (portrait == null || portrait.gameObject == null)
			{
				return;
			}

			if(!IsPrefabConnected(portrait.gameObject))
			{
				//연결되지 않았다면 프리팹 복원을 위한 정보를 갱신하지 않는다.
				return;
			}

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			GameObject rootGameObj = PrefabUtility.GetNearestPrefabInstanceRoot(portrait.gameObject);
#else
			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
#endif
			if (rootGameObj != null)
			{
				portrait._rootGameObjectAsPrefabInstanceForRestore = rootGameObj;
			}

			
#if UNITY_2018_1_OR_NEWER
			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif

			if(prefabObj != null)
			{
				portrait._srcPrefabAssetForRestore = prefabObj;
			}

			SetEditorDirty();
		}


		public static void DisconnectPrefab(apPortrait portrait, bool isClearData = false)
		{
			if (portrait == null || portrait.gameObject == null)
			{
				return;
			}

			//미리 정보를 삭제한다.
			if(isClearData)
			{
				portrait._rootGameObjectAsPrefabInstanceForRestore = null;
				portrait._srcPrefabAssetForRestore = null;
				SetEditorDirty();
			}


			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			//Unity 2018.3 전용
			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(portrait.gameObject);
			if(prefabInstanceStatus == PrefabInstanceStatus.Disconnected)
			{
				//이미 끊어졌다.
				Debug.LogError("Arleady Disconnected");
				return;
			}
#else
			PrefabType prefabType = PrefabUtility.GetPrefabType(portrait.gameObject);
			if(prefabType == PrefabType.DisconnectedPrefabInstance)
			{
				//이미 끊어졌다.
				//Debug.LogError("Arleady Disconnected");
				return;
			}
#endif

			//<< 유니티 2018.3 관련 API 분기 >>
			GameObject rootGameObj = GetRootGameObjectAsPrefabInstance(portrait.gameObject);

			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return;
			}


#if UNITY_2018_1_OR_NEWER
			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				return;
			}

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			PrefabUtility.UnpackPrefabInstance(rootGameObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);//차이가 있다. Disconnect가 아니라 정말 끊긴다. 그래서 다시 Apply가 안된다...
#else
			PrefabUtility.DisconnectPrefabInstance(rootGameObj);
#endif
			SetEditorDirty();

		}


		///// <summary>
		///// Set Record를 하면서 Prefab인 경우 Apply를 자동으로 한다.
		///// </summary>
		///// <param name="portrait"></param>
		//public static void SetPortraitPrefabApply(apPortrait portrait)
		//{
		//	return;
		//	//if(portrait == null || portrait.gameObject == null)
		//	//{
		//	//	return;
		//	//}
		//	//if(!IsPrefab(portrait.gameObject))
		//	//{
		//	//	return;
		//	//}
		//	////ApplyPrefab(portrait.gameObject);

		//	//GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
		//	//if (rootGameObj == null)
		//	//{
		//	//	//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
		//	//	return;
		//	//}
		//	////Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

		//	//UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);

		//	//if(prefabObj == null)
		//	//{
		//	//	//Debug.LogError("연결된 프리팹이 없습니다.");
		//	//	return;
		//	//}

		//	//PrefabUtility.RecordPrefabInstancePropertyModifications(rootGameObj);
		//	//EditorSceneManager.MarkAllScenesDirty();
		//}


		/// <summary>
		/// 프리팹과 연결된 객체이며, 동기화(Connected)가 된 상태인가.
		/// Legacy에서는 이 상태에서는 편집할 수 없다.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static bool IsPrefabConnected(GameObject gameObject)
		{
			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			//Unity 2018.3 전용

			GameObject rootObject = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
			if(rootObject == null)
			{
				//프리팹에 해당하는 RootObject가 없다.
				return false;
			}

			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootObject);
			if(prefabInstanceStatus != PrefabInstanceStatus.Connected)
			{
				//프리팹이 아니다.
				return false;
			}
#else

			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
			if(rootGameObj == null)
			{
				//프리팹에 해당하는 RootObject가 없다.
				return false;
			}

			PrefabType prefabType = PrefabUtility.GetPrefabType(rootGameObj);

			if (prefabType != PrefabType.PrefabInstance)
			{
				//Debug.LogError("프리팹이 아닙니다. : " + prefabType);
				return false;
			}
#endif
			return true;
		}


		/// <summary>
		/// 선택된 객체가 프리팹 에셋이라면, 에디터로 열면 안된다.
		/// Inspector UI에서 확인한다.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static bool IsPrefabAsset(GameObject gameObject)
		{
			if(gameObject == null)
			{
				return false;
			}
#if UNITY_2018_3_OR_NEWER
			if(PrefabUtility.IsPartOfPrefabAsset(gameObject))
			{
				return true;
			}


#if UNITY_2020_1_OR_NEWER
			//2020.1부터는 Nested Prefab Stage를 검출할 수 있다.
			Stage stage = StageUtility.GetStage(gameObject);
			if(stage != null)
			{	
				if(!string.IsNullOrWhiteSpace(stage.assetPath) && stage.assetPath.EndsWith(".prefab"))
				{
					//이 객체가 위치한 씬이 Prefab Stage이다.
					//프리팹 스테이지이므로 수정 불가
					return true;
				}
			}
#endif

#else
			GameObject rootObject = GetRootGameObjectAsPrefabInstance(gameObject);
			PrefabType prefabType = PrefabUtility.GetPrefabType(rootObject);
			if(prefabType == PrefabType.Prefab
				|| prefabType == PrefabType.ModelPrefab)
			{
				//프리팹이다.
				return true;
			}
#endif
			return false;
		}


		public enum PREFAB_STATUS
		{
			NoPrefab, Connected, Disconnected, Missing
		}

		/// <summary>
		/// IsPrefabConnected 함수와 비슷하게 프리팹과 연결된 상태를 확인한다.
		/// 다만, Legacy에서 Disconnected 상태인 경우에도 True를 리턴한다.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static PREFAB_STATUS GetPrefabStatus(GameObject gameObject)
		{	
			//<< 유니티 2018.3 관련 API 분기 >>
			

#if UNITY_2018_3_OR_NEWER
			//Unity 2018.3 전용

			GameObject rootObject = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
			if(rootObject == null)
			{
				//프리팹에 해당하는 RootObject가 없다.
				return PREFAB_STATUS.NoPrefab;
			}

			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootObject);
			
			switch (prefabInstanceStatus)
			{
				case PrefabInstanceStatus.NotAPrefab:
					return PREFAB_STATUS.NoPrefab;

				case PrefabInstanceStatus.Connected:
					return PREFAB_STATUS.Connected;

				case PrefabInstanceStatus.Disconnected:
					return PREFAB_STATUS.Disconnected;

				case PrefabInstanceStatus.MissingAsset:
					return PREFAB_STATUS.Missing;
			}

#else

			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
			if(rootGameObj == null)
			{
				//프리팹에 해당하는 RootObject가 없다.
				return PREFAB_STATUS.NoPrefab;
			}

			PrefabType prefabType = PrefabUtility.GetPrefabType(rootGameObj);

			switch (prefabType)
			{
				case PrefabType.PrefabInstance:
					return PREFAB_STATUS.Connected;

				case PrefabType.DisconnectedPrefabInstance:
					return PREFAB_STATUS.Disconnected;

				case PrefabType.MissingPrefabInstance:
					return PREFAB_STATUS.Missing;
			}
#endif
			return PREFAB_STATUS.NoPrefab;
		}



		public static GameObject GetRootGameObjectAsPrefabInstance(GameObject gameObject)
		{
			if(gameObject == null)
			{
				return null;
			}
			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			return PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);//변경 20.9.14
#else
			return PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
#endif
		}


		public static void ApplyPrefab(apPortrait targetPortrait, bool isReplaceNameBased = false)
		{
			//<< 유니티 2018.3 관련 API 분기 >>
			GameObject rootGameObj = null;
			UnityEngine.Object prefabObj = null;

			//1차적으로 Unity API를 활용하고, 2차로는 복원 정보를 활용하자. (둘다 안되면 실패)
			rootGameObj = GetRootGameObjectAsPrefabInstance(targetPortrait.gameObject);
			if(rootGameObj == null)
			{
				//복원 정보 활용
				rootGameObj = targetPortrait._rootGameObjectAsPrefabInstanceForRestore;
			}

			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				Debug.LogError("AnyPortrait : There is no Root GameObject as a Prefab instance.");
				return;
			}

			

			
#if UNITY_2018_1_OR_NEWER
			prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif


			if(prefabObj == null)
			{
				//복원 정보 활용
				prefabObj = targetPortrait._srcPrefabAssetForRestore;
			}

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				Debug.LogError("AnyPortrait : There is no Prefab Asset that is the source of the target Portrait.");
				return;
			}


			//<< 유니티 2018.3 관련 API 분기 >>
			//Debug.Log("연결된 프리팹 : " + prefabObj.name);
#if UNITY_2018_3_OR_NEWER
			bool isApplyComplete = false;
			try
			{
				PrefabUtility.ApplyPrefabInstance(rootGameObj, InteractionMode.UserAction);
				isApplyComplete = true;
			}
			catch(Exception)
			{
				isApplyComplete = false;
			}

			if(!isApplyComplete)
			{
				//실패시에는 두번째 방법
				try
				{
					string strPrefabPath = AssetDatabase.GetAssetPath(prefabObj);
					PrefabUtility.SaveAsPrefabAssetAndConnect(rootGameObj, strPrefabPath, InteractionMode.UserAction);
					isApplyComplete = true;
				}
				catch(Exception)
				{
					isApplyComplete = false;
				}
			}

			if(!isApplyComplete)
			{
				Debug.LogError("AnyPortrait : It is impossible to apply it as the Prefab. Please update the Prefab manually.");
			}
#else
			if (isReplaceNameBased)
			{
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
			}
			else
			{
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab);
			}
#endif

			//연결 정보를 갱신하자
			targetPortrait._srcPrefabAssetForRestore = prefabObj;
			targetPortrait._rootGameObjectAsPrefabInstanceForRestore = rootGameObj;


			SetEditorDirty();
		}



		public static UnityEngine.Object GetPrefabObject(GameObject gameObject)
		{
			//<< 유니티 2018.3 관련 API 분기 >>
			GameObject rootGameObj = GetRootGameObjectAsPrefabInstance(gameObject);

			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return null;
			}
			//Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			//UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#if UNITY_2018_1_OR_NEWER
			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif

			return prefabObj;
		}


		//----------------------------------------------------------------------------------------------------------
		// GUI : Toggle Button
		//----------------------------------------------------------------------------------------------------------


		public static void ReleaseGUIFocus()
		{
			GUI.FocusControl(null);
		}

		/// <summary>
		/// 원격으로 포커스를 지정하고싶은 텍스트 필드 직전에 이 함수를 호출하고 ID를 입력한다.
		/// ID는 apStringFactory에 정의하자
		/// </summary>
		/// <param name="strGUIID"></param>
		public static void SetNextGUIID(string strGUIID)
		{
			GUI.SetNextControlName(strGUIID);
		}

		/// <summary>
		/// 텍스트 필드로 포커스를 지정하고 바로 글을 작성할 수 있게 만든다.
		/// </summary>
		/// <param name="strGUIID"></param>
		public static void SetGUIFocus_TextField(string strGUIID)
		{
			ReleaseGUIFocus();
			EditorGUI.FocusTextInControl(strGUIID);
		}

		// 추가 20.12.4
		/// <summary>
		/// DelayedTextField가 켜진 상태에서 다른 객체로 바뀌었을때,
		/// GUI가 초기화되면서 변경된 텍스트가 이상한 대상으로 적용되는 경우가 있다.
		/// 원래는 Enter키를 눌러야 하지만, 그렇지 않고 입력이 취소되는 경우 볼 수 있는 문제
		/// ID가 있다면 이 문제를 해결할 수 있다.
		/// </summary>
		/// <param name="strGUIID"></param>
		/// <returns></returns>
		public static bool IsDelayedTextFieldEventValid(string strGUIID)
		{
			return string.Equals(GUI.GetNameOfFocusedControl(), strGUIID);
		}










		public static Color BoxTextColor
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					//return Color.white;
					return GUI.skin.label.normal.textColor;
				}
				else
				{
					return GUI.skin.box.normal.textColor;
				}
			}
		}
		
		public static Color ToggleBoxColor_Selected
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					return new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}
			}
		}

		public static Color ToggleBoxColor_SelectedWithImage
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					return new Color(0.3f, 1.0f, 1.0f, 1.0f);
				}
			}
		}



		public static Color ToggleBoxColor_NotAvailable
		{
			get
			{

				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.1f, 0.1f, 0.1f, 1.0f);
				}
				else
				{
					return new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
			}
		}

		private static apGUIContentWrapper _sGUIContentWrapper = new apGUIContentWrapper(false);


		public static bool ToggledButton(string strText, bool isSelected, int width)
		{
			return ToggledButton(strText, isSelected, width, 20);
		}

		public static bool ToggledButton(string strText, bool isSelected, int width, int height)
		{
			if (isSelected)
			{
				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = ToggleBoxColor_Selected;
				
				//이전
				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.white;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//if(EditorGUIUtility.isProSkin)
				//{
				//	//밝은 파랑 + 하늘색
				//	guiStyle.normal.textColor = Color.cyan;
				//}
				//else
				//{
				//	//짙은 남색
				//	//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				//}

				//GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				
				//변경
				GUILayout.Box(	strText, 
								apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan,//<<이건 ProUI일때와 기본의 색이 다르다.
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				return GUILayout.Button(strText, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//}
					//else
					//{
					//	textColor = Color.white;
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//}
					//else
					//{
					//	//짙은 남색 + 흰색
					//	textColor = Color.white;
					//}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				//변경
				GUILayout.Button(strText, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//이전
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//return GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				return GUILayout.Button(strText, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//}
					//else
					//{
					//	textColor = Color.white;
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//}
					//else
					//{
					//	//짙은 남색 + 흰색
					//	textColor = Color.white;
					//}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				//변경 19.11.20
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				GUILayout.Button(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경 19.11.20
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(texture, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				
				return GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;
				
				//이전
				//GUILayout.Box(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				
				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton_VerticalMargin0(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
				}
				
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
								apGUIStyleWrapper.I.Box_MiddleCenter_VerticalMargin0,
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, 
										apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, 
										apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, null);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));


				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, null);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton(Texture2D texture, int nSpace, string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, null);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));


				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, null);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, int nSpace, string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_Ctrl(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}




		public static bool ToggledButton_Ctrl_VerticalMargin0(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				
				if (!isAvailable)
				{
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
				}
				
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				GUILayout.Box(	_sGUIContentWrapper.Content, 
								apGUIStyleWrapper.I.Box_MiddleCenter_VerticalMargin0_White2Cyan,
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}






		public static bool ToggledButton_Ctrl(string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}


		public static bool ToggledButton_Ctrl(Texture2D texture, int nSpace, string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
				}
				

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strText, texture, toolTip);

				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}





		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton_2Side_VerticalMargin0(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}					
				}

				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, 
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(	_sGUIContentWrapper.Content, 
											apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, 
											apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}
				
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				bool isBtn = GUILayout.Button(textureSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(textureNotSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}
				
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(textureSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureSelected, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(textureNotSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureNotSelected, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton_2Side(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				
				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, null);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, null);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton_2Side_VerticalMargin0(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, null);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, 
					apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0_White2Cyan,
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, null);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		public static bool ToggledButton_2Side(Texture2D texture, int nSpace, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextSelected, texture, null);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextNotSelected, texture, null);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton_2Side(	Texture2D texture, 
												string strTextSelected, string strTextNotSelected, 
												bool isSelected, 
												bool isAvailable, 
												int width, int height, 
												string toolTip 
												//GUIStyle alignmentStyle = null//기존 > 따로 함수를 나누자
												)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}


				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_2Side(	Texture2D texture, 
												int nSpace, string strTextSelected, string strTextNotSelected, 
												bool isSelected, 
												bool isAvailable, 
												int width, int height, 
												string toolTip 
												//GUIStyle alignmentStyle = null//기존 > 따로 함수를 나누자
												)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}




		//추가 19.11.21 : 왼쪽 배열의 버튼의 경우
		public static bool ToggledButton_2Side_LeftAlign(	Texture2D texture, 
															string strTextSelected, string strTextNotSelected, 
															bool isSelected, 
															bool isAvailable, 
															int width, int height, 
															string toolTip 
														)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan),
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_2Side_LeftAlign(	Texture2D texture, 
															int nSpace, string strTextSelected, string strTextNotSelected, 
															bool isSelected, 
															bool isAvailable, 
															int width, int height, 
															string toolTip 
														)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan),
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}


		public static bool ToggledButton_2Side(string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;
				
				bool isBtn = GUILayout.Button(strTextSelected, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				return GUILayout.Button(strTextNotSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}

		public static bool ToggledButton_2Side(string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;
				
				bool isBtn = GUILayout.Button(strText, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				return GUILayout.Button(strText, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}







		public static bool ToggledButton_2Side_LeftAlign(	Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
															string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
															bool isSelected, bool isAvailable, int width, int height, string tooltip)
		{
			Color prevColor = GUI.backgroundColor;
			//Color textColor = Color.white;

			if (!isAvailable)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//textColor = Color.black;
					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					//textColor = Color.white;
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;
				//guiStyle.margin = GUI.skin.button.margin;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotAvailable, textureNotAvailable, tooltip);

				GUILayout.Box(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Box_MiddleLeft_BtnMargin_White2Black, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					//textColor = Color.cyan;
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					//textColor = Color.white;
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, textureSelected, tooltip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, textureNotSelected, tooltip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}




		public static bool ToggledButton_2Side_LeftAlign(	Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
															int nSpace, string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
															bool isSelected, bool isAvailable, int width, int height, string tooltip)
		{
			Color prevColor = GUI.backgroundColor;
			//Color textColor = Color.white;

			if (!isAvailable)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//textColor = Color.black;
					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					//textColor = Color.white;
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				//이전
				//GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextNotAvailable, textureNotAvailable, tooltip);

				GUILayout.Box(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Box_MiddleLeft_BtnMargin_White2Black, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextSelected, textureSelected, tooltip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(nSpace, strTextNotSelected, textureNotSelected, tooltip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}




		//추가 : Ctrl을 누르면 색상이 바뀐다.
		public static bool ToggledButton_2Side_Ctrl(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif
			if (isSelected || !isAvailable || isCtrl)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}




		//추가 : Ctrl을 누르면 색상이 바뀐다.
		public static bool ToggledButton_2Side_Ctrl_VerticalMargin0(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif
			if (isSelected || !isAvailable || isCtrl)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, 
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(	_sGUIContentWrapper.Content, 
											apGUIStyleWrapper.I.Button_MiddleCenter_VerticalMargin0, 
											apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}





		public static bool ToggledButton_2Side_Ctrl(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(textureSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureSelected, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;

				if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(textureNotSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureNotSelected, toolTip);
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}




		//추가 : 다중 편집시 [동기화가 안된 상태]도 표기해야한다. 동기화가 안된 경우 (Enabled의 텍스트) + (다른 색상)으로 나온다.
		public static bool ToggledButton_2Side_Sync(	string strTextSelected, 
														string strTextNotSelected, 
														bool isEnabled, bool isAvailable, bool isSync, 
														int width, int height)
		{
			if (isEnabled || !isAvailable || !isSync)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(!isSync)
				{
					//동기화가 안된 경우
					if (EditorGUIUtility.isProSkin)
					{
						//보라색 + 하늘색
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//보라색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 1.0f, prevColor.g * 0.2f, prevColor.b * 1.1f, 1.0f);
					}
				}
				else if (isEnabled)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				bool isBtn = GUILayout.Button(strTextSelected, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				return GUILayout.Button(strTextNotSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}








		/// <summary>
		/// 추가 21.3.19 : 3가지 종류의 버튼(메인 선택, 서브 선택, 선택 안됨) / 선택 불가인 상태가 나온다.
		/// 텍스트도 두가지 상태가 색상으로 보여지며, 툴팁이 포함된다.
		/// Ctrl키를 누르면 색이 모두 바뀐다.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="selectionType">0은 기본, 1은 메인 선택, 2는 서브 선택</param>
		/// <param name="isAvailable"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="toolTip"></param>
		/// <param name="isCtrlKey"></param>
		/// <param name="isCommandKey"></param>
		/// <returns></returns>
		public static bool ToggledButton_3Side_Ctrl(	string text, 
														int selectionType, 
														bool isAvailable, 
														//bool isColoredText,
														int width, int height, 
														string toolTip, 
														bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif
			if (selectionType != 0 || !isAvailable || isCtrl)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (selectionType == 1)
				{
					//메인 선택 : 파란색 또는 청록색
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}
				else if (selectionType == 2)
				{
					//서브 선택 : 보라색
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.8f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//보라색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 1.1f, prevColor.g * 0.3f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(text, null, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, 
					apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding,
					//isColoredText ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_Orange2Yellow : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, 
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(text, null, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, 
					//isColoredText ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_Orange2Yellow : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding,
					apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding,
					apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			}
		}



		//----------------------------------------------------------------------------------------------------------
		// Delayed Vector2Field
		//----------------------------------------------------------------------------------------------------------
		

		public static Vector2 DelayedVector2Field(Vector2 vectorValue, int width)
		{
			Vector2 result = vectorValue;

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			if (width > 100)
			{
				int widthLabel = 15;
				int widthData = ((width - ((15 + 2) * 2)) / 2) - 2;
				EditorGUILayout.LabelField(Text_X, apGUILOFactory.I.Width(widthLabel));
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, apGUILOFactory.I.Width(widthData));

				EditorGUILayout.LabelField(Text_Y, apGUILOFactory.I.Width(widthLabel));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, apGUILOFactory.I.Width(widthData));
			}
			else
			{
				int widthData = (width / 2) - 2;
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, apGUILOFactory.I.Width(widthData));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, apGUILOFactory.I.Width(widthData));
			}



			EditorGUILayout.EndHorizontal();

			return result;
		}



		

		//----------------------------------------------------------------------------------------------------------
		// 스크롤을 하는 경우, 해당 UI가 출력될지 여부를 리턴한다
		//----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// 스크롤안에 아이템을 출력하는 경우, 출력 영역 안쪽인지 바깥쪽인이 확인할 필요가 있다.
		/// 항목간에 여백이 없어야 한다.
		/// 약간의 여유를 가지고 리턴한다.
		/// </summary>
		/// <param name="itemPosY">항목의 Y값 (따로 계산해둘것)</param>
		/// <param name="itemHeight">항목의 Height</param>
		/// <param name="scroll">스크롤값 (Y값만 사용함)</param>
		/// <param name="scrollLayoutHeight">스크롤 레이아웃의 Height</param>
		/// <returns></returns>
		public static bool IsItemInScroll(int itemPosY, int itemHeight, Vector2 scroll, int scrollLayoutHeight)
		{
			//기본
			if (itemPosY < scroll.y - (itemHeight * 2))
			{
				return false;
			}

			if (itemPosY > (scroll.y + scrollLayoutHeight) + itemHeight)
			{
				return false;
			}

			//테스트
			//if (itemPosY - 50 < scroll.y)
			//{
			//	return false;
			//}

			//if(itemPosY > (scroll.y + scrollLayoutHeight) - 50)
			//{
			//	return false;
			//}

			return true;
		}

		//----------------------------------------------------------------------------------------------------------
		// Slider Float (짧은 경우)
		//----------------------------------------------------------------------------------------------------------
		public static float FloatSlider(string label, float value, float minValue, float maxValue, int totalWidth, int labelWidth)
		{

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(totalWidth), apGUILOFactory.I.Height(25));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(label, apGUILOFactory.I.Width(labelWidth));

			int width_Slider = (int)((totalWidth - (5 + labelWidth)) * 0.6f);
			int width_Field = totalWidth - (5 + labelWidth + width_Slider + 7);
			value = GUILayout.HorizontalSlider(value, minValue, maxValue, apGUILOFactory.I.Width(width_Slider), apGUILOFactory.I.Height(25));
			float nextValue = EditorGUILayout.FloatField(value, apGUILOFactory.I.Width(width_Field));

			EditorGUILayout.EndHorizontal();

			return Mathf.Clamp(nextValue, minValue, maxValue);
		}

		public static int IntSlider(string label, int value, int minValue, int maxValue, int totalWidth, int labelWidth)
		{
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(totalWidth), apGUILOFactory.I.Height(25));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(label, apGUILOFactory.I.Width(labelWidth));

			int width_Slider = (int)((totalWidth - (5 + labelWidth)) * 0.6f);
			int width_Field = totalWidth - (5 + labelWidth + width_Slider + 7);
			
			float fValue = GUILayout.HorizontalSlider(value, minValue, maxValue, apGUILOFactory.I.Width(width_Slider), apGUILOFactory.I.Height(25));
			value = (int)(fValue + 0.5f);
			int nextValue = EditorGUILayout.IntField(value, apGUILOFactory.I.Width(width_Field));

			EditorGUILayout.EndHorizontal();

			return Mathf.Clamp(nextValue, minValue, maxValue);
		}

		//----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Editor의 Left, Right 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 Left-Right로 접을 수 있다.
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_H(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldType, bool isLeftUI)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), Text_EMPTY, WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//Left UI일 때
			//- Unfolded 상태에서 : 우측에 "<<" 아이콘
			//- Folded 상태에서 : 우측에 ">>" 아이콘

			//Right UI일 때
			//- Unfolded 상태에서 : 좌측에 ">>" 아이콘
			//- Folded 상태에서 : 좌측에 "<<" 아이콘

			int leftMargin = 0;
			Texture2D img_FoldH = null;
			if(isLeftUI)
			{
				leftMargin = width - 22;
				if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }
				else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
			}
			else
			{
				leftMargin = 2;
				if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
				else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }
			}

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldH, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Horizontal;
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}

		/// <summary>
		/// Editor의 Right1_Upper 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 가로, 세로로 접을 수 있다.
		/// Right1_Upper 한정
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_HV(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldTypeH, apEditor.UI_FOLD_TYPE foldTypeV)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), Text_EMPTY, WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//이 함수는 무조건 RightUI이다.
			//foldTypeH가 우선된다.

			//foldTypeH 상태
			//- Unfolded : 좌측에 ">>" 아이콘, 우측에 "-" 또는 "ㅁ" 아이콘
			//- Folded : 좌측에 "<<" 아이콘

			//foldTypeV 상태 (foldTypeH가 Unfolded일때만)
			//- Unfolded : 우측에 "-" 아이콘
			//- Folded : 우측에 "ㅁ" 아이콘

			int leftMargin = 2;
			int middleMargin = width - (44);
			Texture2D img_FoldH = null;
			Texture2D img_FoldV = null;

			if(foldTypeH == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
			else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }

			if(foldTypeV == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVHide_x16); }
			else											{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVShow_x16); }
			

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldH, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Horizontal;
			}
			if (foldTypeH == apEditor.UI_FOLD_TYPE.Unfolded)
			{
				GUILayout.Space(middleMargin);
				if (GUILayout.Button(img_FoldV, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(height+1)))
				{
					result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Vertical;
				}
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}


		/// <summary>
		/// Editor의 Right1_Lower 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 세로로만 접을 수 있다.
		/// Right1_Lower 한정
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_V(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldType)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), Text_EMPTY, WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//- Unfolded 상태에서 : 우측에 "-" 아이콘
			//- Folded 상태에서 : 우측에 "ㅁ" 아이콘

			int leftMargin = width - 22;
			Texture2D img_FoldV = null;
			if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVHide_x16); }
			else											{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVShow_x16); }

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldV, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Vertical;
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}


		//----------------------------------------------------------------------------------------------------------
		// White Color Texture
		//----------------------------------------------------------------------------------------------------------
		//private static Texture2D _whiteSmallTexture = null;
		public static Texture2D WhiteTexture
		{
			get
			{
				return EditorGUIUtility.whiteTexture;
			}
		}

		private static GUIStyle _whiteGUIStyle = null;
		public static GUIStyle WhiteGUIStyle
		{
			get
			{
				if(_whiteGUIStyle == null)
				{
					_whiteGUIStyle = new GUIStyle(GUIStyle.none);
					_whiteGUIStyle.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle;
			}
		}

		private static GUIStyle _whiteGUIStyle_Box = null;
		public static GUIStyle WhiteGUIStyle_Box
		{
			get
			{
				if(_whiteGUIStyle_Box == null)
				{
					_whiteGUIStyle_Box = new GUIStyle(GUI.skin.box);
					_whiteGUIStyle_Box.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle_Box;
			}
		}

		

		//----------------------------------------------------------------------------------------------------------
		// 미리 정의된 텍스트 (추가적인 생성 없게)
		//----------------------------------------------------------------------------------------------------------
		private static string s_Text_X = "X";
		private static string s_Text_Y = "Y";
		private static string s_Text_EMPTY = "";
		private static string s_Text_None = "None";
		private static string s_Text_NoneName = "<None>";

		/// <summary>"X"</summary>
		public static string Text_X { get { return s_Text_X; } }

		/// <summary>"Y"</summary>
		public static string Text_Y { get { return s_Text_Y; } }

		/// <summary>""</summary>
		public static string Text_EMPTY { get { return s_Text_EMPTY; } }

		/// <summary>"None"</summary>
		public static string Text_None { get { return s_Text_None; } }

		/// <summary>"(None)"</summary>
		public static string Text_NoneName { get { return s_Text_NoneName; } }

		//----------------------------------------------------------------------------------------------------------
		// Graphics Functions
		//----------------------------------------------------------------------------------------------------------
		public static float DistanceFromLine(Vector2 posA, Vector2 posB, Vector2 posTarget)
		{
			//float lineLen = Vector2.Distance(posA, posB);
			//if(lineLen < 0.1f)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}

			//float proj = (posTarget.x - posA.x) * (posB.x - posA.x) + (posTarget.y - posA.y) * (posB.y - posA.y);
			//if(proj < 0)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}
			//else if(proj > lineLen)
			//{
			//	return Vector2.Distance(posB, posTarget);
			//}

			//return Mathf.Abs((-1) * (posTarget.x - posA.x) * (posB.y - posA.y) + (posTarget.y - posA.y) * (posB.x - posA.x)) / lineLen;

			//float lineLen = Vector2.Distance(posA, posB);
			float dotA = Vector2.Dot(posTarget - posA, (posB - posA).normalized);
			float dotB = Vector2.Dot(posTarget - posB, (posA - posB).normalized);

			if (dotA < 0.0f)
			{
				return Vector2.Distance(posA, posTarget);
			}

			if (dotB < 0.0f)
			{
				return Vector2.Distance(posB, posTarget);
			}

			return Vector2.Distance((posA + (posB - posA).normalized * dotA), posTarget);
		}

		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh, apMatrix3x3 matrixWorldToMeshLocal)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = matrixWorldToMeshLocal.MultiplyPoint(mousePosW);

			//Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsMouseInRenderUnitMesh(Vector2 mousePos, apRenderUnit meshRenderUnit)
		{
			if (meshRenderUnit._meshTransform == null)
			{
				return false;
			}

			if (meshRenderUnit._meshTransform._mesh == null || meshRenderUnit._renderVerts.Count == 0)
			{
				return false;
			}

			apMesh targetMesh = meshRenderUnit._meshTransform._mesh;
			List<apRenderVertex> rVerts = meshRenderUnit._renderVerts;

			Vector2 mousePosW = apGL.GL2World(mousePos);

			apRenderVertex rVert0, rVert1, rVert2;
			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					rVert0 = rVerts[tri._verts[0]._index];
					rVert1 = rVerts[tri._verts[1]._index];
					rVert2 = rVerts[tri._verts[2]._index];

					if (apMeshTri.IsPointInTri(mousePosW,
												rVert0._pos_World,
												rVert1._pos_World,
												rVert2._pos_World))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsPointInTri(Vector2 point, Vector2 triPoint0, Vector2 triPoint1, Vector2 triPoint2)
		{
			float s = triPoint0.y * triPoint2.x - triPoint0.x * triPoint2.y + (triPoint2.y - triPoint0.y) * point.x + (triPoint0.x - triPoint2.x) * point.y;
			float t = triPoint0.x * triPoint1.y - triPoint0.y * triPoint1.x + (triPoint0.y - triPoint1.y) * point.x + (triPoint1.x - triPoint0.x) * point.y;

			if ((s < 0) != (t < 0))
			{
				return false;
			}

			var A = -triPoint1.y * triPoint2.x + triPoint0.y * (triPoint2.x - triPoint1.x) + triPoint0.x * (triPoint1.y - triPoint2.y) + triPoint1.x * triPoint2.y;
			if (A < 0.0)
			{
				s = -s;
				t = -t;
				A = -A;
			}
			return s > 0 && t > 0 && (s + t) <= A;

		}
		//----------------------------------------------------------------------------------------------------
		public static apImageSet.PRESET GetModifierIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.Modifier_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.Modifier_AnimatedMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.Modifier_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.Modifier_Physic;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.Modifier_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.Modifier_AnimatedTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.Modifier_FFD;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.Modifier_AnimatedFFD;

			}
			return apImageSet.PRESET.Modifier_Volume;
		}

		public static apImageSet.PRESET GetPhysicsPresetIconType(apPhysicsPresetUnit.ICON iconType)
		{
			switch (iconType)
			{
				case apPhysicsPresetUnit.ICON.Cloth1:
					return apImageSet.PRESET.Physic_PresetCloth1;
				case apPhysicsPresetUnit.ICON.Cloth2:
					return apImageSet.PRESET.Physic_PresetCloth2;
				case apPhysicsPresetUnit.ICON.Cloth3:
					return apImageSet.PRESET.Physic_PresetCloth3;
				case apPhysicsPresetUnit.ICON.Flag:
					return apImageSet.PRESET.Physic_PresetFlag;
				case apPhysicsPresetUnit.ICON.Hair:
					return apImageSet.PRESET.Physic_PresetHair;
				case apPhysicsPresetUnit.ICON.Ribbon:
					return apImageSet.PRESET.Physic_PresetRibbon;
				case apPhysicsPresetUnit.ICON.RubberHard:
					return apImageSet.PRESET.Physic_PresetRubberHard;
				case apPhysicsPresetUnit.ICON.RubberSoft:
					return apImageSet.PRESET.Physic_PresetRubberSoft;
				case apPhysicsPresetUnit.ICON.Custom1:
					return apImageSet.PRESET.Physic_PresetCustom1;
				case apPhysicsPresetUnit.ICON.Custom2:
					return apImageSet.PRESET.Physic_PresetCustom2;
				case apPhysicsPresetUnit.ICON.Custom3:
					return apImageSet.PRESET.Physic_PresetCustom3;
			}
			return apImageSet.PRESET.Physic_PresetCustom3;
		}


		public static apImageSet.PRESET GetControlParamPresetIconType(apControlParam.ICON_PRESET iconType)
		{
			switch (iconType)
			{
				case apControlParam.ICON_PRESET.None:
					return apImageSet.PRESET.Hierarchy_Param;
				case apControlParam.ICON_PRESET.Head:
					return apImageSet.PRESET.ParamPreset_Head;
				case apControlParam.ICON_PRESET.Body:
					return apImageSet.PRESET.ParamPreset_Body;
				case apControlParam.ICON_PRESET.Hand:
					return apImageSet.PRESET.ParamPreset_Hand;
				case apControlParam.ICON_PRESET.Face:
					return apImageSet.PRESET.ParamPreset_Face;
				case apControlParam.ICON_PRESET.Eye:
					return apImageSet.PRESET.ParamPreset_Eye;
				case apControlParam.ICON_PRESET.Hair:
					return apImageSet.PRESET.ParamPreset_Hair;
				case apControlParam.ICON_PRESET.Equipment:
					return apImageSet.PRESET.ParamPreset_Equip;
				case apControlParam.ICON_PRESET.Cloth:
					return apImageSet.PRESET.ParamPreset_Cloth;
				case apControlParam.ICON_PRESET.Force:
					return apImageSet.PRESET.ParamPreset_Force;
				case apControlParam.ICON_PRESET.Etc:
					return apImageSet.PRESET.ParamPreset_Etc;
			}
			return apImageSet.PRESET.ParamPreset_Etc;
		}

		public static apControlParam.ICON_PRESET GetControlParamPresetIconTypeByCategory(apControlParam.CATEGORY category)
		{
			switch (category)
			{
				case apControlParam.CATEGORY.Head:
					return apControlParam.ICON_PRESET.Head;
				case apControlParam.CATEGORY.Body:
					return apControlParam.ICON_PRESET.Body;
				case apControlParam.CATEGORY.Face:
					return apControlParam.ICON_PRESET.Face;
				case apControlParam.CATEGORY.Hair:
					return apControlParam.ICON_PRESET.Hair;
				case apControlParam.CATEGORY.Equipment:
					return apControlParam.ICON_PRESET.Equipment;
				case apControlParam.CATEGORY.Force:
					return apControlParam.ICON_PRESET.Force;
				case apControlParam.CATEGORY.Etc:
					return apControlParam.ICON_PRESET.Etc;
			}
			return apControlParam.ICON_PRESET.Etc;

		}


		public static apImageSet.PRESET GetSmallModIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.SmallMod_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.SmallMod_AnimMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.SmallMod_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.SmallMod_Physics;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.SmallMod_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.SmallMod_AnimTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;
			}
			return apImageSet.PRESET.Modifier_Volume;
		}
		//----------------------------------------------------------------------------------------------------

		public class NameAndIndexPair
		{
			public string _strName = "";
			public int _index = 0;
			public int _indexStrLength = 0;
			public NameAndIndexPair(string strName, string strIndex)
			{
				_strName = strName;
				if (strIndex.Length > 0)
				{
					_index = Int32.Parse(strIndex);
					_indexStrLength = strIndex.Length;
				}
				else
				{
					_index = 0;
					_indexStrLength = 0;
				}
			}
			public string MakeNewName(int index)
			{
				string strIndex = index + "";
				if (strIndex.Length < _indexStrLength)
				{
					int dLength = _indexStrLength - strIndex.Length;
					//0을 붙여주자
					for (int i = 0; i < dLength; i++)
					{
						strIndex = "0" + strIndex;
					}
				}

				return _strName + strIndex;
			}
		}

		public static NameAndIndexPair ParseNumericName(string srcName)
		{
			if (string.IsNullOrEmpty(srcName))
			{
				return new NameAndIndexPair("<None>", "");
			}

			//1. 이름 내에 "숫자로 된 부분"이 있다면, 그중 가장 "뒤의 숫자"를 1 올려서 갱신한다.
			string strName_First = "", strName_Index = "";
			int strMode = 1;//0 : First, 1 : Index
			for (int i = srcName.Length - 1; i >= 0; i--)
			{
				string curStr = srcName.Substring(i, 1);
				switch (strMode)
				{
					case 1:
						{
							if (IsNumericString(curStr))
							{
								strName_Index = curStr + strName_Index;
							}
							else
							{
								strName_First = curStr + strName_First;
								strMode = 0;
							}
						}
						break;

					case 0:
						strName_First = curStr + strName_First;
						break;
				}
			}
			return new NameAndIndexPair(strName_First, strName_Index);
		}


		private static bool IsNumericString(string str)
		{
			if (str == "0" || str == "1" || str == "2" ||
				str == "3" || str == "4" || str == "5" ||
				str == "6" || str == "7" || str == "8" ||
				str == "9")
			{
				return true;
			}
			return false;
		}


		//---------------------------------------------------------------------------------------
		public static T[] AddItemToArray<T>(T addItem, T[] srcArray)
		{
			if (srcArray == null || srcArray.Length == 0)
			{
				return new T[] { addItem };
			}

			int prevArraySize = srcArray.Length;
			int nextArraySize = prevArraySize + 1;

			T[] nextArray = new T[nextArraySize];
			for (int i = 0; i < prevArraySize; i++)
			{
				nextArray[i] = srcArray[i];
			}
			nextArray[nextArraySize - 1] = addItem;
			return nextArray;
		}

		//---------------------------------------------------------------------------------------
		private static string[] s_renderTextureNames = null;
		public static string[] GetRenderTextureSizeNames()
		{
		//	public enum RENDER_TEXTURE_SIZE
		//{
		//	s_64, s_128, s_256, s_512, s_1024
		//}
			if(s_renderTextureNames == null)
			{
				s_renderTextureNames = new string[] { "64", "128", "256", "512", "1024" };
			}
			return s_renderTextureNames;
		}

		//---------------------------------------------------------------------------------------
		//색상 관련 (Hue 방식)
		
		public static Color GetSimilarColor(Color srcColor, float minSaturation, float maxSaturation, float minValue, float maxValue, bool isAdaptMinOffset)
		{
			float hue = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			Color.RGBToHSV(srcColor, out hue, out sat, out value);

			//HSV 상태에서 랜덤을 주는게 비슷한 색상을 유지하는게 좋다.
			//> 개선 20.3.24 : 그냥 랜덤으로 하면 완전히 같을 수가 있다. (isAdaptMinOffset 옵션이 true인 경우)
			//- [증/감]을 랜덤으로 각각 결정하고
			//- Hue, Sat, Val 중에서 MinOffset을 적용할 대상을 정하자. (각각의 확률에 따라 한개의 채널만 적용)
			//- 위에서 결정된 사항을 통해서 랜덤 범위를 결정하고 계산하자.

			float randHue = 0.0f;
			float randSat = 0.0f;
			float randVal = 0.0f;

			if (isAdaptMinOffset)
			{
				bool isInc_Hue = (UnityEngine.Random.Range(0, 10) < 5);
				bool isInc_Sat = false;
				bool isInc_Val = false;

				//Sat/Val이 Max에 가깝다면 무조건 감소, Min에 가깝다면 무조건 증가해야한다.
				//Sat 정하기
				if (sat > (minSaturation * 0.2f + maxSaturation * 0.8f))
				{
					//위쪽 80%의 이상이다.
					isInc_Sat = false;
				}
				else if (sat < (minSaturation * 0.8f + maxSaturation * 0.2f))
				{
					//아래쪽 80%의 이하이다.
					isInc_Sat = true;
				}
				else
				{
					//확률로 정하자
					isInc_Sat = (UnityEngine.Random.Range(0, 10) < 5);
				}

				//Val 정하기
				if (value > (minValue * 0.2f + maxValue * 0.8f))
				{
					//위쪽 80% 이상이다.
					isInc_Val = false;
				}
				else if (value < (minValue * 0.8f + maxValue * 0.2f))
				{
					//아래쪽 80% 이하이다.
					isInc_Val = true;
				}
				else
				{
					//확률로 정하자.
					isInc_Val = (UnityEngine.Random.Range(0, 10) < 5);
				}

				//어느 채널에 MinOffset을 적용할 것인가.
				bool isMinOffset_Hue = false;
				bool isMinOffset_Sat = false;
				bool isMinOffset_Val = false;
				int iRandMinOffset = UnityEngine.Random.Range(0, 100);
				if (iRandMinOffset < 60)
				{
					//60% > Hue
					isMinOffset_Hue = true;
				}
				else if (iRandMinOffset < 80)
				{
					//20% > Sat
					isMinOffset_Sat = true;
				}
				else
				{
					//20% > Val
					isMinOffset_Val = true;
				}

				

				//Hue 값을 정하자.
				if (isMinOffset_Hue)
				{
					randHue = UnityEngine.Random.Range(0.1f, 0.2f);
				}
				else
				{
					randHue = UnityEngine.Random.Range(0.0f, 0.05f);
				}

				if (!isInc_Hue)
				{
					randHue *= -1;
				}

				//Sat 값을 정하자
				if (isMinOffset_Sat)
				{
					randSat = UnityEngine.Random.Range(0.02f, 0.05f);
				}
				else
				{
					randSat = UnityEngine.Random.Range(0.0f, 0.05f);
				}

				if (!isInc_Sat)
				{
					randSat *= -1;
				}

				//Value 값을 정하자
				if (isMinOffset_Val)
				{
					randVal = UnityEngine.Random.Range(0.05f, 0.2f);
				}
				else
				{
					randVal = UnityEngine.Random.Range(0.0f, 0.1f);
				}

				if (!isInc_Val)
				{
					randVal *= -1;
				}
			}
			else
			{
				randHue = UnityEngine.Random.Range(-0.05f, 0.05f);
				randSat = UnityEngine.Random.Range(-0.05f, 0.05f);
				randVal = UnityEngine.Random.Range(-0.1f, 0.1f);
			}

			//float randHue = UnityEngine.Random.Range(-0.05f, 0.05f);
			//float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			//float randValue = UnityEngine.Random.Range(-0.2f, 0.2f);
			
			float newHue = hue + randHue;
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			while (newHue > 1.0f)
			{
				newHue -= 1.0f;
			}

			float newSaturation = Mathf.Clamp(sat + randSat, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(value + randVal, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		//20.3.24 : 비슷한 색상이지만, 입력된 색상과는 비교적 차이가 크도록 만든다.
		public static Color GetSimilarColorButDiff(Color srcColor, Color exceptColor, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue_Src = 0.0f;
			float sat_Src = 0.0f;
			float val_Src = 0.0f;

			float hue_Exc = 0.0f;
			float sat_Exc = 0.0f;
			float val_Exc = 0.0f;

			Color.RGBToHSV(srcColor, out hue_Src, out sat_Src, out val_Src);
			Color.RGBToHSV(exceptColor, out hue_Exc, out sat_Exc, out val_Exc);

			//1) 두개의 색상이 비슷하지 않다면, 그냥 일반적인 srcColor의 바리에이션을 입력하고
			//2) 두개의 색상이 비슷하다면, 그걸 감안하여 랜덤 영역을 결정해야한다.

			//두개의 색상이 비슷한가.
			float dif_Hue = Mathf.Abs(hue_Src - hue_Exc);
			float dif_Sat = Mathf.Abs(sat_Src - sat_Exc);
			float dif_Val = Mathf.Abs(val_Src - val_Exc);

			//Dif_Hue 범위를 -0.5 ~ 0.5로 바꾸자. (Rotation 방식의 값이므로, 차이가 큰것 처럼 보이지만, 의외로 가까울 수 있다.)
			if(dif_Hue > 0.5f)
			{
				dif_Hue -= 1.0f;
			}
			dif_Hue = Mathf.Abs(dif_Hue);

			float simRange_Hue = 0.1f;
			float simRange_Sat = (maxSaturation - minSaturation) * 0.2f;
			float simRange_Val = (maxValue - minValue) * 0.2f;

			bool isSimilar_Hue = (dif_Hue < simRange_Hue);
			bool isSimilar_Sat = (dif_Sat < simRange_Sat);
			bool isSimilar_Val = (dif_Val < simRange_Val);

			float randHue = 0.0f;
			float randSat = 0.0f;
			float randVal = 0.0f;

			//Min-Max 범위와 상관없이 겹치지 않는 방향으로 증/감을 정한다.
			//반대로, 각 항목별로 유사하지 않는다면 일반 랜덤 바리에이션을 한다.
			//단, 모든 채널에 대해서 "비슷하지 않게" 만들 필요는 없으며,
			//Hue > Val > Sat 순으로 일단 하나라도 비슷하지 않게 만들었다면 다른 값들은 유사해도 괜찮다.
			bool isDiffCorrected = false;

			//Hue 체크
			if(isSimilar_Hue)
			{
				//Hue값이 비슷하다면,
				randHue = simRange_Hue + UnityEngine.Random.Range(simRange_Hue * 0.4f, simRange_Hue * 1.0f);

				if(hue_Src < hue_Exc)
				{
					//감소하는 방향으로.
					randHue *= -1;
				}

				isDiffCorrected = true;
			}
			else
			{
				//일반 랜덤
				randHue = UnityEngine.Random.Range(-0.05f, 0.05f);
			}

			//Val 체크
			if(isSimilar_Val && !isDiffCorrected)
			{
				//Sat값이 비슷하다면,
				randVal = simRange_Val + UnityEngine.Random.Range(simRange_Val * 0.1f, simRange_Val * 0.6f);

				if(val_Src < val_Exc)
				{
					//감소하는 방향으로.
					randVal *= -1;
				}

				isDiffCorrected = true;
			}
			else
			{
				//일반 랜덤
				randVal = UnityEngine.Random.Range(-0.1f, 0.1f);
			}

			//Sat 체크
			if(isSimilar_Sat && !isDiffCorrected)
			{
				//Sat값이 비슷하다면,
				randSat = simRange_Sat + UnityEngine.Random.Range(simRange_Sat * 0.1f, simRange_Sat * 0.6f);

				if(sat_Src < sat_Exc)
				{
					//감소하는 방향으로.
					randSat *= -1;
				}

				isDiffCorrected = true;
			}
			else
			{
				//일반 랜덤
				randSat = UnityEngine.Random.Range(-0.05f, 0.05f);
			}


			float newHue = hue_Src + randHue;
			while (newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			while (newHue > 1.0f)
			{
				newHue -= 1.0f;
			}

			float newSaturation = Mathf.Clamp(sat_Src + randSat, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(val_Src + randVal, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>Saturation과 Value는 비슷하지만 Hue가 차이가 많은 색상을 리턴한다.</summary>
		public static Color GetDiffierentColor(Color srcColor, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			Color.RGBToHSV(srcColor, out hue, out sat, out value);
			
			//Hue의 기준점을 옮겨주자
			//최소 (0.16), 최대 300 (0.83) > Rotate
			float randHue = UnityEngine.Random.Range(0.16f, 0.83f);
			
			//Saturation은 비슷하게
			float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randValue = 0.0f;
			if(value < 0.2f)
			{
				//너무 어두우면 > 밝은 방향으로 강제
				randValue = UnityEngine.Random.Range(0.3f, 0.7f);
			}
			else
			{
				//그 외에는 비슷한 명도로
				randValue = UnityEngine.Random.Range(-0.02f, 0.02f);
			}
			
			
			float newHue = hue + randHue;
			while(newHue > 1.0f)
			{
				newHue -= 1.0f;
			}
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			float newSaturation = Mathf.Clamp(randSaturation + randSaturation, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(randValue + randValue, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>
		/// GetDiffierentColor 함수의 변형. 두개의 SrcColor로 부터 다른 색상을 구한다. 기본적으로 SrcColor2는 Hue만 비교한다.
		/// </summary>
		/// <param name="srcColor1"></param>
		/// <param name="srcColor2"></param>
		/// <param name="minSaturation"></param>
		/// <param name="maxSaturation"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public static Color GetDiffierentColor(Color srcColor1, Color srcColor2, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue1 = 0.0f;
			float hue2 = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			//사용하진 않음
			float sat2 = 0.0f;
			float value2 = 0.0f;

			Color.RGBToHSV(srcColor1, out hue1, out sat, out value);
			Color.RGBToHSV(srcColor2, out hue2, out sat2, out value2);
			
			
			
			//Saturation은 비슷하게
			float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randValue = 0.0f;
			if(value < 0.2f)
			{
				//너무 어두우면 > 밝은 방향으로 강제
				randValue = UnityEngine.Random.Range(0.3f, 0.7f);
			}
			else
			{
				//그 외에는 비슷한 명도로
				randValue = UnityEngine.Random.Range(-0.02f, 0.02f);
			}
			
			//Hue의 기준점을 옮겨주자
			//최소 (0.16), 최대 300 (0.83) > Rotate
			float newHue = 0.0f;

			//SrcColor2 기준으로 랜덤 가능한 범위를 정하자.
			//영역은 무조건 2개로 나뉠 것
			if(hue2 < hue1)
			{
				hue2 += 1.0f;
			}

			//가능한 영역
			float hueArea_A = hue1 + 0.16f;
			float hueArea_B = hue1 + 0.83f;

			//불가능한 영역 (조금 좁다)
			float hueLimit_A = hue2 - 0.1f;
			float hueLimit_B = hue2 + 0.1f;

			//랜덤 가능한 영역
			float hueRandArea_A = 0.0f;
			float hueRandArea_B = 0.0f;
			float hueRandArea_A2 = 0.0f;
			float hueRandArea_B2 = 0.0f;
			bool isCheck2Area = false;

			if (hueLimit_B < hueArea_A || hueLimit_A > hueArea_B)
			{
				//제한 영역이 완전히 밖으로 나갈 때
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueArea_B;
			}
			else if (hueLimit_A < hueArea_A && hueArea_A < hueLimit_B)
			{
				//A쪽에 제한 영역이 줄어들었을 때
				hueRandArea_A = hueLimit_B;
				hueRandArea_B = hueArea_B;
			}
			else if (hueLimit_A < hueArea_B && hueArea_B < hueLimit_B)
			{
				//B쪽에 제한 영역이 줄어들었을 때
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueLimit_A;
			}
			else if (hueArea_A < hueLimit_A && hueLimit_B < hueArea_B)
			{
				//A와 B 안쪽에 들어왔을 때 > 영역이 2개
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueLimit_A;

				hueRandArea_A2 = hueLimit_B;
				hueRandArea_B2 = hueArea_B;

				isCheck2Area = true;

				if (hueRandArea_B2 < hueRandArea_A2)
				{
					hueRandArea_B2 = hueRandArea_A2 + 0.1f;
				}
			}
			else
			{
				//그 외의 경우
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueArea_B;
			}
			if(hueRandArea_B < hueRandArea_A)
			{
				hueRandArea_B = hueRandArea_A + 0.1f;
			}
			
			if(isCheck2Area)
			{
				//랜덤 돌릴 영역이 2개일 때 > 50% 확률로 결정
				if(UnityEngine.Random.Range(0, 10) < 5)
				{
					newHue = UnityEngine.Random.Range(hueRandArea_A, hueRandArea_B);
				}
				else
				{
					newHue = UnityEngine.Random.Range(hueRandArea_A2, hueRandArea_B2);
				}
			}
			else
			{
				newHue = UnityEngine.Random.Range(hueRandArea_A, hueRandArea_B);
			}
			while(newHue > 1.0f)
			{
				newHue -= 1.0f;
			}
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			float newSaturation = Mathf.Clamp(randSaturation + randSaturation, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(randValue + randValue, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>HSV 방식에서 Saturation(채도), Value(명도)의 범위만 정하고 랜덤한 색상을 구하자.</summary>
		/// <returns></returns>
		public static Color GetRandomColor(float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float randSaturation = Mathf.Clamp01(UnityEngine.Random.Range(minSaturation, maxSaturation));
			float randValue = Mathf.Clamp01(UnityEngine.Random.Range(minValue, maxValue));

			float randHue = UnityEngine.Random.Range(0.0f, 1.0f);

			Color resultColor = Color.HSVToRGB(randHue, randSaturation, randValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		//본 색상 프리셋에 관련하여
		public static int BoneColorPresetsCount { get { return 6; } }
		public static Color GetBoneColorPreset(int iPreset)
		{
			switch (iPreset)
			{
				case 0: return new Color(1.0f, 0.3f, 0.0f, 1.0f);//완전 붉은색..은 아니고 살짝 주홍빛의 붉은색
				case 1: return new Color(1.0f, 1.0f, 0.0f, 1.0f);//노란색	
				case 2: return new Color(0.5f, 1.0f, 0.3f, 1.0f);//밝은 초록
				case 3: return new Color(0.0f, 1.0f, 1.0f, 1.0f);//하늘색	
				case 4: return new Color(0.0f, 0.2f, 1.0f, 1.0f);//파란색
				case 5: return new Color(1.0f, 0.3f, 1.0f, 1.0f);//보라색
			}
			return Color.white;
		}



		//---------------------------------------------------------------------------------------
		//private static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
		//private static string _stopWatchMsg = "";
		//public static void StartCodePerformanceCheck(string stopWatchMsg)
		//{
		//	_stopWatchMsg = stopWatchMsg;
		//	_stopwatch.Reset();
		//	_stopwatch.Start();
		//}

		//public static void StopCodePerformanceCheck()
		//{
		//	_stopwatch.Stop();
		//	long mSec = _stopwatch.ElapsedMilliseconds;
		//	Debug.LogError("Performance [" + _stopWatchMsg + "] : " + (mSec / 1000) + "." + (mSec % 1000) + " secs");
		//	//return _stopwatch.ElapsedTicks + " Ticks";
		//}

		//-------------------------------------------------------------------------------------------
		//변경 20.3.17 : Path를 바꿀 수 있게 되면서 기존의 상수 문자열에서 변수로 변경
		private static string _resourcePath_Material = "Assets/AnyPortrait/Editor/Materials/";
		private static string _resourcePath_Text = "Assets/AnyPortrait/Editor/Scripts/Util/";
		//private static string _resourcePath_TextWithoutAssets = "AnyPortrait/Editor/Scripts/Util/";
		private static string _resourcePath_Icon = "Assets/AnyPortrait/Editor/Images/";

		private const int PATH_MAX_LENGTH = 256;
		private static apStringWrapper _strWrapper_Path = new apStringWrapper(PATH_MAX_LENGTH);

		private const string TEXT_PATH_PROSKIN = "ProSkin/";
		private const string TEXT_EXP_PNG = ".png";
		

		public static void SetPackagePath(string rootPath)
		{
			//유효한지 판정하자
			bool isValidPath = true;
			if(rootPath.Length < 19)
			{
				//너무 짧다.
				//기본 경로인 "Assets/AnyPortrait/"가 19 글자이다.
				isValidPath = false;
			}
			else if(!rootPath.EndsWith("AnyPortrait/"))
			{
				//AnyPortrait 폴더로 끝나야 한다.
				isValidPath = false;
			}
			else if(!rootPath.StartsWith("Assets/"))
			{
				//Assets 폴더로 시작해야한다.
				isValidPath = false;
			}

			if(!isValidPath)
			{
				//유효하지 않으면 기본 값을 이용하자
				Debug.LogError("AnyPortrait : Invalid Package Path. [" + rootPath + "]");
				_resourcePath_Material = "Assets/AnyPortrait/Editor/Materials/";
				_resourcePath_Text = "Assets/AnyPortrait/Editor/Scripts/Util/";
				//_resourcePath_TextWithoutAssets = "AnyPortrait/Editor/Scripts/Util/";
				_resourcePath_Icon = "Assets/AnyPortrait/Editor/Images/";
			}
			else
			{
				_resourcePath_Material = rootPath + "Editor/Materials/";
				_resourcePath_Text = rootPath + "Editor/Scripts/Util/";
				//_resourcePath_TextWithoutAssets = rootPath.Substring(7) + "Editor/Scripts/Util/";
				_resourcePath_Icon = rootPath + "Editor/Images/";
			}
		}


		///// <summary>
		///// "Assets/AnyPortrait/Editor/Materials/"
		///// </summary>
		///// <returns></returns>
		//public static string ResourcePath_Material
		//{
		//	get
		//	{
		//		//return "Assets/AnyPortrait/Editor/Materials/";
		//		return _resourcePath_Material;
		//	}
		//}

		///// <summary>
		///// "Assets/AnyPortrait/Editor/Scripts/Util/"
		///// </summary>
		//public static string ResourcePath_Text
		//{
		//	get
		//	{
		//		//return "Assets/AnyPortrait/Editor/Scripts/Util/";
		//		return _resourcePath_Text;
		//	}
		//}

		///// <summary>
		///// "AnyPortrait/Editor/Scripts/Util/"
		///// </summary>
		//public static string ResourcePath_Text_WithoutAssets
		//{
		//	get
		//	{
		//		//return "AnyPortrait/Editor/Scripts/Util/";
		//		return _resourcePath_TextWithoutAssets;
		//	}
		//}

		///// <summary>
		///// "Assets/AnyPortrait/Editor/Images/"
		///// </summary>
		//public static string ResourcePath_Icon
		//{
		//	get
		//	{
		//		//return "Assets/AnyPortrait/Editor/Images/";
		//		return _resourcePath_Icon;
		//	}
		//}


		public static bool IsInAssetsFolder(string folderPath)
		{
			System.IO.DirectoryInfo targetDirInfo = new System.IO.DirectoryInfo(folderPath);
			System.IO.DirectoryInfo assetsDirInfo = new System.IO.DirectoryInfo(Application.dataPath);

			if(targetDirInfo == null || !targetDirInfo.Exists)
			{
				return false;
			}
			if(assetsDirInfo == null || !assetsDirInfo.Exists)
			{
				return false;
			}
			//target의 parent->parent...->parent가 assetDirInfo인지 체크
			System.IO.DirectoryInfo curDir = targetDirInfo;
			while(true)
			{
				if(string.Equals(curDir.FullName, assetsDirInfo.FullName))
				{
					return true;
				}

				curDir = curDir.Parent;
				if(curDir.Parent == null || !curDir.Exists)
				{
					return false;
				}
			}
		}

		//추가 20.3.17 : 경로를 생성하는 함수를 별도로 만들자.
		public static string MakePath_Material(string fileName)
		{
			if(_strWrapper_Path == null) { _strWrapper_Path = new apStringWrapper(PATH_MAX_LENGTH); }
			_strWrapper_Path.Clear();
			_strWrapper_Path.Append(_resourcePath_Material, false);
			_strWrapper_Path.Append(fileName, true);

			return _strWrapper_Path.ToString();
		}

		public static string MakePath_Text(string fileName)
		{
			if(_strWrapper_Path == null) { _strWrapper_Path = new apStringWrapper(PATH_MAX_LENGTH); }
			_strWrapper_Path.Clear();
			_strWrapper_Path.Append(_resourcePath_Text, false);
			_strWrapper_Path.Append(fileName, true);

			return _strWrapper_Path.ToString();
		}

		//public static string MakePath_Text_WithoutAssets(string fileName)
		//{
		//	if(_strWrapper_Path == null) { _strWrapper_Path = new apStringWrapper(PATH_MAX_LENGTH); }
		//	_strWrapper_Path.Clear();
		//	_strWrapper_Path.Append(_resourcePath_TextWithoutAssets, false);
		//	_strWrapper_Path.Append(fileName, true);

		//	return _strWrapper_Path.ToString();
		//}

		public static string MakePath_Icon(string fileName, bool isProSkin)
		{
			if(_strWrapper_Path == null) { _strWrapper_Path = new apStringWrapper(PATH_MAX_LENGTH); }
			_strWrapper_Path.Clear();
			_strWrapper_Path.Append(_resourcePath_Icon, false);
			if(isProSkin)
			{
				//ProSkin/
				_strWrapper_Path.Append(TEXT_PATH_PROSKIN, false);
			}
			_strWrapper_Path.Append(fileName, false);
			_strWrapper_Path.Append(TEXT_EXP_PNG, true);//.png

			return _strWrapper_Path.ToString();
		}

		

		//--------------------------------------------------------------------------------
		public static int GetAspectRatio_Height(int srcWidth, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect
			//H = W / Aspect <<

			return (int)(((float)srcWidth / targetAspectRatio) + 0.5f);
		}

		public static int GetAspectRatio_Width(int srcHeight, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect <<
			//H = W / Aspect

			return (int)(((float)srcHeight * targetAspectRatio) + 0.5f);
		}

		//--------------------------------------------------------------------------------------
		public delegate void FUNC_CHECK_CURRENT_VERSION(bool isSuccess, string currentVersion);
		private static FUNC_CHECK_CURRENT_VERSION _funcCheckCurrentVersion = null;
		private static IEnumerator _coroutine = null;
		private static System.Diagnostics.Stopwatch _coroutineStopWatch = null;
		
		public static void RequestCurrentVersion(FUNC_CHECK_CURRENT_VERSION funcCurrentVersion)
		{
			if (funcCurrentVersion == null)
			{
				return;
			}
			_funcCheckCurrentVersion = funcCurrentVersion;
			_coroutine = Crt_RequestCurrentVersion();
			_coroutineStopWatch = null;

			EditorApplication.update -= ExecuteCoroutine;
			EditorApplication.update += ExecuteCoroutine;
		}


		private static void ExecuteCoroutine()
		{
			if(_coroutine == null)
			{
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

			//Debug.Log("Update Coroutine");
			bool isResult = _coroutine.MoveNext();
			
			if(!isResult)
			{
				_coroutine = null;
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

		}
		//private static void StartCheck()
		//{
		//	if(_coroutineStopWatch == null)
		//	{
		//		_coroutineStopWatch = new System.Diagnostics.Stopwatch();
		//	}
		//	_coroutineStopWatch.Stop();
		//	_coroutineStopWatch.Reset();
		//	_coroutineStopWatch.Start();
		//}
		private static bool CheckWaitTime(float time)
		{
			if(_coroutineStopWatch == null)
			{
				//새로 체크한다 => 타이머 시작
				_coroutineStopWatch = new System.Diagnostics.Stopwatch();
				_coroutineStopWatch.Stop();
				_coroutineStopWatch.Reset();
				_coroutineStopWatch.Start();
				return false;
			}
			//타이머가 가동중이라면 => 시간 체크 => 지정된 시간을 넘은 경우 리턴 True 및 타이머 삭제
			if(_coroutineStopWatch.Elapsed.TotalSeconds > time)
			{
				
				_coroutineStopWatch.Stop();
				_coroutineStopWatch = null;
				return true;
			}
			return false;
		}

		private static IEnumerator Crt_RequestCurrentVersion()
		{
			
			string url = "https://homepi12.wixsite.com/referencedata/anyportrait";

			

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
#else
			WWW www = new WWW(url);
#endif
			//Debug.Log("Start Request");

#if UNITY_2018_3_OR_NEWER
			yield return www.SendWebRequest();
#else
			yield return www;
#endif

			float totalTime = 0.0f;//<<전체 시간이 오래 걸리면 포기
			
			while(true)
			{

				//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
				//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
				if(www.isDone || www.downloadProgress >= 1.0f)
#else
				if(www.isDone || www.progress >= 1.0f)
#endif
				
				{
					//Debug.Log("Progress >> Completed : " + www.progress + " (" + totalTime + ")");
					break;
				}

				//yield return new WaitForSeconds(1.0f);//<<이게 작동을 안한다.
				if (CheckWaitTime(1.0f))
				{
					totalTime += 1.0f;
					//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
					if (totalTime > 20.0f)
					{
						yield break;
					}
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}

			if (!CheckWaitTime(2.0f))
			{
				//2초 대기
				yield return new WaitForEndOfFrame();	
			}
			//yield return new WaitForSeconds(2.0f);//실제

			//실제:
			//주석 해제할 것
			totalTime = 0.0f;
			while (true)
			{
				//Is Done 한번 더 체크
				if (www.isDone)
				{
					break;
				}
				if (CheckWaitTime(1.0f))
				{
					totalTime += 1.0f;
					//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
					if (totalTime > 20.0f)
					{
						yield break;
					}
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}


			if (!www.isDone)
			{
				//처리가 되지 않았다.
				//Debug.LogError("Request > Not Downloading : " + www.progress + " / " + www.isDone);
				yield break;
			}

			//Debug.Log("Request > Finished : " + www.progress + " / " + www.isDone);

			if(_funcCheckCurrentVersion == null)
			{
				yield break;
			}
			try
			{	

				if (string.IsNullOrEmpty(www.error))
				{
					//성공
					//"[AnyPortrait-CurrentVersion]:[1.0.3]"
					string strKey = "[AnyPortrait-CurrentVersion]";

					//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
					string downloadedText = www.downloadHandler.text;
#else
					string downloadedText = www.text;
#endif
					if(downloadedText.Contains(strKey))
					{
						//Debug.LogWarning("Result\n" + www.text);
						int textLength = downloadedText.Length;
						int iStart = downloadedText.IndexOf(strKey);
						int sampleLength = strKey.Length + 20;
						if(iStart + sampleLength > textLength)
						{
							sampleLength = textLength - iStart;
						}
						
						string strSubText = downloadedText.Substring(iStart, sampleLength);
						//Debug.Log("Sub String : " + strSubText + "(" +iStart + ":" + sampleLength +")");

						System.Text.StringBuilder result = new System.Text.StringBuilder();
						string curText = "";
						for (int i = strKey.Length; i < strSubText.Length; i++)
						{
							curText = strSubText.Substring(i, 1);
							if(curText == "]")
							{
								break;
							}
							if(curText == "0" || curText == "1" || curText == "2" || curText == "3"
								|| curText == "4" || curText == "5" || curText == "6" || curText == "7"
								|| curText == "8" || curText == "9" || curText == ".")
							{
								result.Append(curText);
							}
						}

						_funcCheckCurrentVersion(true, result.ToString());
						yield break;
					}
					//실패
					_funcCheckCurrentVersion(false, "Parsing Failed");
					yield break;
				}

				_funcCheckCurrentVersion(false, www.error);
			}
			catch(Exception ex)
			{
				//에러
				_funcCheckCurrentVersion(false, ex.ToString());
				//Debug.LogError("Exception : " + ex);
			}
		}

		//------------------------------------------------------------------------------
		/// <summary>
		/// AssetStore에서 AnyPortrait 페이지를 엽니다.
		/// </summary>
		public static void OpenAssetStorePage()
		{
			UnityEditorInternal.AssetStore.Open("content/111584");
		}



		// 절대 경로 <-> 상대 경로 (Asset에 대하여)
		//-------------------------------------------------------------------------------------------
		public static string GetProjectAssetPath()
		{
			return Application.dataPath;
		}

		

		public enum PATH_INFO_TYPE
		{
			//유효하지 않다
			//절대 경로이며 Asset 폴더 밖에 있다.
			//절대 경로이며 Asset 폴더 안에 있다.
			//상대 경로이며 Asset 폴더 밖에 있다.
			//상대 경로이며 Asset 폴더 안에 있다.
			NotValid,
			Absolute_OutAssetFolder,
			Absolute_InAssetFolder,
			Relative_OutAssetFolder,
			Relative_InAssetFolder,

		}
		public static PATH_INFO_TYPE GetPathInfo(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return PATH_INFO_TYPE.NotValid;
			}

			path = path.Replace("\\", "/");
			//Debug.Log("GetPathInfo : " + path);

			//1. path가 Relative인지 확인
			System.IO.DirectoryInfo di_AssetFolder = new System.IO.DirectoryInfo(Application.dataPath);

			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

			//System.Uri uri_target = new Uri(path);
			
			
			//Debug.Log("Asset Path : " + di_AssetFolder.FullName);
			//Debug.Log("Target Path : " + di.FullName);

			if(di.Exists && !path.StartsWith("Assets"))
			{
				//유효한 경로이다 >> 절대 경로이다.
				//Asset 폴더 안쪽에 있는지 확인
				if(di.FullName.StartsWith(di_AssetFolder.FullName))
				{
					//Asset Folder의 경로로 부터 시작한다면
					//>> 절대 경로 + Asset 폴더 안쪽
					//Debug.Log(">>> 절대 경로 + Asset 폴더 안쪽");
					return PATH_INFO_TYPE.Absolute_InAssetFolder;
				}
				else
				{
					//>> 절대 경로 + Asset 폴더 바깥쪽
					//Debug.Log(">>> 절대 경로 + Asset 폴더 바깥쪽");
					return PATH_INFO_TYPE.Absolute_OutAssetFolder;
				}
				
			}
			else
			{
				//유효하지 않다면 상대 경로일 수 있다.
				//string checkPath = Application.dataPath + "/" + path;
				//Debug.Log("Check Path : " + checkPath);
				string projectPath = Application.dataPath;
				projectPath = projectPath.Substring(0, projectPath.Length - 6);//"Assets 글자를 뒤에서 뺀다"

				string fullPath = projectPath + path;
				//Debug.Log("FullPath : " + fullPath);
				//다시 체크
				di = new System.IO.DirectoryInfo(fullPath);
				if(di.Exists)
				{
					//Asset 폴더를 기준으로 하는 상대 경로가 맞다.
					//안쪽에 있는지 확인
					bool isInAssetFoler = IsInFolder(di_AssetFolder.FullName, di.FullName);
					if(isInAssetFoler)
					{
						//>> 상대 경로 + Asset 폴더 안쪽
						//Debug.Log(">>> 상대 경로 + Asset 폴더 안쪽");
						return PATH_INFO_TYPE.Relative_InAssetFolder;
					}
					else
					{
						//>> 상대 경로 + Asset 폴더 바깥쪽
						//Debug.Log(">>> 상대 경로 + Asset 폴더 바깥쪽");
						return PATH_INFO_TYPE.Relative_OutAssetFolder;
					}
				}
				else
				{
					//그냥 잘못된 경로네요..
					
				}

			}
			
			//Debug.Log(">>> 잘못된 경로");
			return PATH_INFO_TYPE.NotValid;
		}

		private static bool IsInFolder(string parentPath, string childPath)
		{
			System.IO.DirectoryInfo di_Parent = new System.IO.DirectoryInfo(parentPath);
			System.IO.DirectoryInfo di_Child = new System.IO.DirectoryInfo(childPath);
			if(!di_Parent.Exists || !di_Child.Exists)
			{
				return false;
			}

			string parentPath_Lower = di_Parent.FullName.ToLower();
			string rootPath_Lower = di_Child.Root.FullName.ToLower();
			System.IO.DirectoryInfo di_Cur = new System.IO.DirectoryInfo(childPath);

			string curPath_Lower = "";
			while (true)
			{
				curPath_Lower = di_Cur.FullName.ToLower();
				//Root에 도달했으면 종료
				if(curPath_Lower.Equals(rootPath_Lower))
				{
					break;
				}

				if (curPath_Lower.Equals(parentPath_Lower))
				{
					//Parent Path가 나왔다.
					return true;
				}



				try
				{
					//위 폴더로 이동해보자
					di_Cur = di_Cur.Parent;
					if(!di_Cur.Exists)
					{
						break;
					}
				}
				catch(Exception)
				{
					//에러 발생
					break;
				}
			}

			return false;
		}


		public static string AbsolutePath2RelativePath(string absPath)
		{
			Uri uri_Asset = new Uri(Application.dataPath);
			Uri uri_AbsPath = new Uri(absPath);

			Uri uri_Relative = uri_Asset.MakeRelativeUri(uri_AbsPath);
			
			string resultPath = uri_Relative.ToString();

			//Debug.LogError("Abs > Rel : " + resultPath);
			resultPath = resultPath.Replace("\\", "/");

			
			//Debug.Log("Prev : [" + resultPath + "]");
			resultPath = DecodeURLEmptyWord(resultPath);
			//Debug.LogError(">>> " + resultPath);
			//Debug.Log("Next : [" + resultPath + "]");
			return resultPath;
		}

		public static string DecodeURLEmptyWord(string urlPath)
		{
			//return urlPath;
			return urlPath.Replace("%20", " ");
		}
		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------
		// 파일 열거나 저장할 때 마지막으로 접근한 디렉토리 경로 알려주기 (바로 다시 열 수 있게)
		//-------------------------------------------------------------------------------------------
		//private static string s_lastOpenSaveFileDirectoryPath = "";
		public enum SAVED_LAST_FILE_PATH
		{
			PSD_ExternalFile,
			BackupFile,
			BoneAnimExport,
			Rotoscoping
		}
		/// <summary>
		/// 파일을 디렉토리로부터 "열거나 저장할 때", 해당 파일의 경로를 저장한다. 
		/// 나중에 바로 그 폴더를 열 수 있다.
		/// 오직 외부의 OpenFileDialog/SaveFileDialog용으로만 사용한다.
		/// </summary>
		public static void SetLastExternalOpenSaveFilePath(string fileFullPath, SAVED_LAST_FILE_PATH filePathType)
		{
			if(string.IsNullOrEmpty(fileFullPath))
			{
				return;
			}
			System.IO.FileInfo fi = new System.IO.FileInfo(fileFullPath);
			if(!fi.Exists)
			{
				//존재하지 않는 경로
				return;
			}
			//적절하게 이름을 가공한다.
			//s_lastOpenSaveFileDirectoryPath = (fi.Directory.FullName).Replace('\\', '/');
			
			//EditorPrefs에 저장하자
			EditorPrefs.SetString(GetFilePathKey(filePathType), (fi.Directory.FullName).Replace('\\', '/'));
			
		}

		/// <summary>
		/// 마지막으로 "OpenFileDialog"를 사용했을때의 디렉토리 경로
		/// </summary>
		public static string GetLastOpenSaveFileDirectoryPath(SAVED_LAST_FILE_PATH filePathType)
		{
			//return s_lastOpenSaveFileDirectoryPath;
			return EditorPrefs.GetString(GetFilePathKey(filePathType), "");
		}

		private static string GetFilePathKey(SAVED_LAST_FILE_PATH filePathType)
		{
			switch (filePathType)
			{
				case SAVED_LAST_FILE_PATH.PSD_ExternalFile:	return "AnyPortrait_LastFilePath__PSD_ExternalFile";
				case SAVED_LAST_FILE_PATH.BackupFile:		return "AnyPortrait_LastFilePath__BackupFile";
				case SAVED_LAST_FILE_PATH.BoneAnimExport:	return "AnyPortrait_LastFilePath__BoneAnimExport";
				case SAVED_LAST_FILE_PATH.Rotoscoping:		return "AnyPortrait_LastFilePath__Rotoscoping";

			}
			return "AnyPortrait_LastFilePath__Common";

		}


		//-------------------------------------------------------------------------------------------
		// 마지막 AutoKey를 EditorPref를 이용하여 저장한다.
		//-------------------------------------------------------------------------------------------
		public static void SaveAnimAutoKeyValueToPref(bool isAnimAutoKey)
		{
			if(!isAnimAutoKey)
			{
				EditorPrefs.DeleteKey("AnyPortrait_LastAnimAutoKeyValue");//False일 때는 값을 삭제한다.
			}
			else
			{
				EditorPrefs.SetBool("AnyPortrait_LastAnimAutoKeyValue", true);
			}
		}
		public static bool GetAnimAutoKeyValueFromPref()
		{
			return EditorPrefs.GetBool("AnyPortrait_LastAnimAutoKeyValue", false);//값이 없다면 False이다.
		}
	}
}