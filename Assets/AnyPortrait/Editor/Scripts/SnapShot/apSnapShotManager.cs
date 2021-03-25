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

	//에디터에서 작업 객체의 값 복사나 저장을 위한 기능을 제공하는 매니저
	//Stack 방식으로 저장을 한다.
	//각 SnapShot 데이터는 실제로 적용되는 객체에서 관리한다.
	public class apSnapShotManager
	{
		// Singletone
		//-------------------------------------------
		private static apSnapShotManager _instance = new apSnapShotManager();
		private static readonly object _obj = new object();
		public static apSnapShotManager I { get { lock (_obj) { return _instance; } } }



		// Members
		//-------------------------------------------
		public enum SNAPSHOT_TARGET
		{
			Mesh, MeshGroup, ModifiedMesh, Portrait,//ETC.. Keyframe?
		}

		public enum SAVE_TYPE
		{
			Copy,
			Record
		}


		//Copy 타입 (Clipboard)
		private apSnapShotStackUnit _clipboard_ModMesh = null;
		private apSnapShotStackUnit _clipboard_Keyframe = null;
		private apSnapShotStackUnit _clipboard_VertRig = null;
		private apSnapShotStackUnit _clipboard_ModBone = null;

		//추가 3.29 : 여러개의 키프레임을 저장하기 위한 용도. Timeline에서 복사한 경우 해당한다.
		private apAnimClip _clipboard_AnimClipOfKeyframes = null;
		private List<apSnapShotStackUnit> _clipboard_Keyframes = null;
		private int _copied_keyframes_StartFrame = -1;
		private int _copied_keyframes_EndFrame = -1;

		//Record 타입
		private const int MAX_RECORD = 10;
		private List<apSnapShotStackUnit> _snapShotList = new List<apSnapShotStackUnit>();
		//이건 나중에 처리하자
		//private apSnapShotStackUnit _curSnapShot = null;
		//private int _iCurSnapShot = 0;
		//private bool _restoredSnapShot = false;

		//추가 19.8.9 : MultipleVertRig 타입
		private apSnapShot_MultipleVertRig _clipboard_MultipleVertRig = null;


		// Init
		//-------------------------------------------
		private apSnapShotManager()
		{

		}



		public void Clear()
		{
			_clipboard_ModMesh = null;
			_clipboard_Keyframe = null;
			_clipboard_VertRig = null;
			_clipboard_ModBone = null;

			_snapShotList.Clear();
			//_curSnapShot = null;
			//_iCurSnapShot = -1;
			//_restoredSnapShot = false;

			_clipboard_AnimClipOfKeyframes = null;
			_clipboard_Keyframes = null;
			_copied_keyframes_StartFrame = -1;
			_copied_keyframes_EndFrame = -1;

			//추가 19.8.9 : Rigging의 Pos-Copy 기능용 코드
			//이건 별도로 처리한다.
			if(_clipboard_MultipleVertRig == null)
			{
				_clipboard_MultipleVertRig = new apSnapShot_MultipleVertRig();
			}
			_clipboard_MultipleVertRig.Clear();
		}


		// Functions
		//-------------------------------------------

		// Copy / Paste
		//--------------------------------------------------------------------
		// 1. ModMesh
		//--------------------------------------------------------------------
		public void Copy_ModMesh(apModifiedMesh modMesh, string snapShotName)
		{
			_clipboard_ModMesh = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_ModMesh.SetSnapShot_ModMesh(modMesh, "Clipboard");
			if (!result)
			{
				_clipboard_ModMesh = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_ModMesh(apModifiedMesh targetModMesh)
		{
			if (targetModMesh == null)
			{ return false; }
			if (_clipboard_ModMesh == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMesh.IsKeySyncable(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModMesh.Load(targetModMesh);
		}

		public string GetClipboardName_ModMesh()
		{
			if (_clipboard_ModMesh == null)
			{
				return "";
			}
			return _clipboard_ModMesh._unitName;
		}

		public bool IsPastable(apModifiedMesh targetModMesh)
		{
			if (targetModMesh == null)
			{ return false; }
			if (_clipboard_ModMesh == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMesh.IsKeySyncable(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 1-2. ModBone
		//--------------------------------------------------------------------
		public void Copy_ModBone(apModifiedBone modBone, string snapShotName)
		{
			_clipboard_ModBone = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_ModBone.SetSnapShot_ModBone(modBone, "Clipboard");
			if (!result)
			{
				_clipboard_ModBone = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_ModBone(apModifiedBone targetModBone)
		{
			if (targetModBone == null)
			{ return false; }
			if (_clipboard_ModBone == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBone.IsKeySyncable(targetModBone);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModBone.Load(targetModBone);
		}

		public string GetClipboardName_ModBone()
		{
			if (_clipboard_ModBone == null)
			{
				return "";
			}
			return _clipboard_ModBone._unitName;
		}

		public bool IsPastable(apModifiedBone targetModBone)
		{
			if (targetModBone == null)
			{ return false; }
			if (_clipboard_ModBone == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBone.IsKeySyncable(targetModBone);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}

		//--------------------------------------------------------------------
		// 2. Keyframe
		//--------------------------------------------------------------------
		public void Copy_Keyframe(apAnimKeyframe keyframe, string snapShotName)
		{
			_clipboard_Keyframe = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_Keyframe.SetSnapShot_Keyframe(keyframe, "Clipboard");
			if (!result)
			{
				_clipboard_Keyframe = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_Keyframe(apAnimKeyframe targetKeyframe)
		{
			if (targetKeyframe == null)
			{ return false; }
			if (_clipboard_Keyframe == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(targetKeyframe);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_Keyframe.Load(targetKeyframe);
		}

		public string GetClipboardName_Keyframe()
		{
			if (_clipboard_Keyframe == null)
			{
				return "";
			}
			return _clipboard_Keyframe._unitName;
		}

		public bool IsPastable(apAnimKeyframe keyframe)
		{
			if (keyframe == null)
			{ return false; }
			if (_clipboard_Keyframe == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(keyframe);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 2.5. Keyframe 여러개 복사하기
		//--------------------------------------------------------------------
		//추가 3.29 : 타임라인 UI에서 키프레임들을 Ctrl+C로 복사하기
		public void Copy_KeyframesOnTimelineUI(apAnimClip animClip, List<apAnimKeyframe> keyframes)
		{
			if (animClip == null || keyframes == null || keyframes.Count == 0)
			{
				_clipboard_AnimClipOfKeyframes = null;
				_clipboard_Keyframes = null;
				_copied_keyframes_StartFrame = -1;
				_copied_keyframes_EndFrame = -1;
				return;
			}


			_clipboard_AnimClipOfKeyframes = animClip;
			if(_clipboard_Keyframes == null)
			{
				_clipboard_Keyframes = new List<apSnapShotStackUnit>();
			}
			else
			{
				_clipboard_Keyframes.Clear();
			}
			
			_copied_keyframes_StartFrame = -1;
			_copied_keyframes_EndFrame = -1;

			apAnimKeyframe srcKeyframe = null;
			for (int i = 0; i < keyframes.Count; i++)
			{
				srcKeyframe = keyframes[i];

				apSnapShotStackUnit newUnit = new apSnapShotStackUnit("Keyframe");
				newUnit.SetSnapShot_Keyframe(srcKeyframe, "Clipboard");
				_clipboard_Keyframes.Add(newUnit);

				if(i == 0)
				{
					_copied_keyframes_StartFrame = srcKeyframe._frameIndex;
					_copied_keyframes_EndFrame = srcKeyframe._frameIndex;
				}
				else
				{
					_copied_keyframes_StartFrame = Mathf.Min(_copied_keyframes_StartFrame, srcKeyframe._frameIndex);
					_copied_keyframes_EndFrame = Mathf.Max(_copied_keyframes_EndFrame, srcKeyframe._frameIndex);
				}
			}
		}

		public bool IsKeyframesPastableOnTimelineUI(apAnimClip animClip)
		{
			if(_clipboard_AnimClipOfKeyframes != null 
				&& animClip != null
				&& _clipboard_AnimClipOfKeyframes == animClip
				&& _clipboard_Keyframes != null
				&& _clipboard_Keyframes.Count > 0
				)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 다른 AnimClip으로 복사할 수 있는지 확인한다.
		/// </summary>
		/// <param name="animClip"></param>
		/// <returns></returns>
		public bool IsKeyframesPastableOnTimelineUI_ToOtherAnimClip(apAnimClip animClip)
		{
			if(_clipboard_AnimClipOfKeyframes != null 
				&& animClip != null
				&& animClip._targetMeshGroup != null
				&& _clipboard_AnimClipOfKeyframes._targetMeshGroup != null
				&& animClip._targetMeshGroup == _clipboard_AnimClipOfKeyframes._targetMeshGroup//최소한 MeshGroup은 같아야 한다.
				//&& _clipboard_AnimClipOfKeyframes == animClip
				&& _clipboard_Keyframes != null
				&& _clipboard_Keyframes.Count > 0
				)
			{
				return true;
			}
			return false;
		}

		public List<apSnapShotStackUnit> GetKeyframesOnTimelineUI()
		{
			return _clipboard_Keyframes;
		}

		public int StartFrameOfKeyframesOnTimelineUI
		{
			get
			{
				return _copied_keyframes_StartFrame;
			}
		}

		//--------------------------------------------------------------------
		// 3. Vertex Rigging
		//--------------------------------------------------------------------
		public void Copy_VertRig(apModifiedVertexRig modVertRig, string snapShotName)
		{
			_clipboard_VertRig = new apSnapShotStackUnit(snapShotName);
			bool result = _clipboard_VertRig.SetSnapShot_VertRig(modVertRig, "Clipboard");
			if (!result)
			{
				_clipboard_VertRig = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_VertRig(apModifiedVertexRig targetModVertRig)
		{
			if (targetModVertRig == null)
			{ return false; }
			if (_clipboard_VertRig == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(targetModVertRig);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_VertRig.Load(targetModVertRig);
		}

		public bool IsPastable(apModifiedVertexRig vertRig)
		{
			if (vertRig == null)
			{ return false; }
			if (_clipboard_VertRig == null)
			{ return false; }

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(vertRig);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		//3-2. Rigging의 Pos-Cppy 기능
		//--------------------------------------------------------------------
		public bool IsRiggingPosPastable(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			if(_clipboard_MultipleVertRig == null)
			{
				return false;
			}
			if(modRenderVerts == null || modRenderVerts.Count == 0)
			{
				return false;
			}
			return _clipboard_MultipleVertRig.IsPastable(keyMeshGroup);
		}

		public void Copy_MultipleVertRig(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			if(_clipboard_MultipleVertRig == null)
			{
				_clipboard_MultipleVertRig = new apSnapShot_MultipleVertRig();
			}

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();

			apSelection.ModRenderVert curModRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				curModRenderVert = modRenderVerts[i];
				if(curModRenderVert != null && curModRenderVert._modVertRig != null)
				{
					vertRigs.Add(curModRenderVert._modVertRig);
				}
			}

			_clipboard_MultipleVertRig.Copy(keyMeshGroup, vertRigs);
		}

		public bool Paste_MultipleVertRig(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			bool isPastable = IsRiggingPosPastable(keyMeshGroup, modRenderVerts);
			if(!isPastable)
			{
				return false;
			}
			
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();

			apSelection.ModRenderVert curModRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				curModRenderVert = modRenderVerts[i];
				if(curModRenderVert != null && curModRenderVert._modVertRig != null)
				{
					vertRigs.Add(curModRenderVert._modVertRig);
				}
			}
			if(vertRigs.Count == 0)
			{
				return false;
			}

			return _clipboard_MultipleVertRig.Paste(keyMeshGroup, vertRigs);
		}


		// Save / Load
		//--------------------------------------------------------------------




		// Get / Set
		//--------------------------------------------
	}
}