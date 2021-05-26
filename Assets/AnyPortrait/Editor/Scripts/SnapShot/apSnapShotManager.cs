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
		
		private apSnapShotStackUnit _clipboard_Keyframe = null;
		private apSnapShotStackUnit _clipboard_VertRig = null;
		//이전
		//private apSnapShotStackUnit _clipboard_ModMesh = null;
		//private apSnapShotStackUnit _clipboard_ModBone = null;

		//슬롯 4개로 변경
		private const int NUM_MOD_CLIPBOARD = 4;
		private apSnapShotStackUnit[] _clipboard_ModMeshes = null;
		private apSnapShotStackUnit[] _clipboard_ModBones = null;

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
			Clear();
		}



		public void Clear()
		{
			//변경 > Null + 생성은 불합리하다. 
			if(_clipboard_Keyframe == null) { _clipboard_Keyframe = new apSnapShotStackUnit(); }
			_clipboard_Keyframe.Clear();
			
			if(_clipboard_VertRig == null) { _clipboard_VertRig = new apSnapShotStackUnit(); }
			_clipboard_VertRig.Clear();
			

			//이전
			//_clipboard_ModMesh = null;
			//_clipboard_ModBone = null;
			
			//변경 21.3.19
			if(_clipboard_ModMeshes == null) { _clipboard_ModMeshes = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD]; }
			if(_clipboard_ModBones == null) { _clipboard_ModBones = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD]; }

			for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
			{
				if (_clipboard_ModBones[i] == null) { _clipboard_ModBones[i] = new apSnapShotStackUnit(); }
				if (_clipboard_ModMeshes[i] == null) { _clipboard_ModMeshes[i] = new apSnapShotStackUnit(); }
				_clipboard_ModBones[i].Clear();
				_clipboard_ModMeshes[i].Clear();
			}

			_snapShotList.Clear();
			//_curSnapShot = null;
			//_iCurSnapShot = -1;
			//_restoredSnapShot = false;

			//키프레임 복사하기
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
		public void Copy_ModMesh(apModifiedMesh modMesh, string snapShotName, int iSlot)
		{
			//변경 21.3.19 : 1개가 아닌 4개의 슬롯에 저장할 수 있다.
			if(_clipboard_ModMeshes == null)
			{
				_clipboard_ModMeshes = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD];
				for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
				{
					_clipboard_ModMeshes[i] = new apSnapShotStackUnit();
				}
			}

			
			if(_clipboard_ModMeshes[iSlot] == null)
			{
				_clipboard_ModMeshes[iSlot] = new apSnapShotStackUnit();
			}
			apSnapShotStackUnit curUnit = _clipboard_ModMeshes[iSlot];
			curUnit.Clear();
			curUnit.SetName(snapShotName);
			
			bool result = curUnit.SetSnapShot_ModMesh(modMesh, "Clipboard");
			if (!result)
			{
				curUnit.Clear();//<<저장 불가능하다.
			}
		}


		//1개의 Slot을 복사하는 경우
		public bool Paste_ModMesh_Single(apModifiedMesh targetModMesh, int iSlot, bool isMorphMod)
		{
			if (targetModMesh == null
				|| _clipboard_ModMeshes == null
				|| _clipboard_ModMeshes[iSlot] == null)
			{
				return false;
			}
			
			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = false;
			if(isMorphMod)
			{
				isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_MorphMod(targetModMesh);
			}
			else
			{
				isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_TFMod(targetModMesh);
			}
			
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModMeshes[iSlot].Load(targetModMesh);
		}


		//2개 이상의 Slot을 복사하는 경우
		public bool Paste_ModMesh_Multiple(apModifiedMesh targetModMesh,
											bool isMorhMod, int iMainSlot, bool[] slots, int methodType)
		{
			if (targetModMesh == null
				|| _clipboard_ModMeshes == null)
			{
				return false;
			}

			List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();

			//슬롯을 하나씩 체크한다.
			apSnapShotStackUnit curUnit = null;
			for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
			{
				if (i == iMainSlot || slots[i])
				{
					curUnit = _clipboard_ModMeshes[i];
					if (curUnit != null)
					{
						if(
							(isMorhMod && curUnit.IsKeySyncable_MorphMod(targetModMesh))
							|| (!isMorhMod && curUnit.IsKeySyncable_TFMod(targetModMesh))
							)
						{
							//다중 복사에 적용
							pastableUnits.Add(curUnit);
						}
					}
				}
			}

			if(pastableUnits.Count == 0)
			{
				//복사가 불가능하다.
				return false;
			}

			if(pastableUnits.Count == 1)
			{
				//1개라면 Single과 동일
				return pastableUnits[0].Load(targetModMesh);
			}

			//이제 다중 복사를 해보자
			//가상의 유닛을 만들고,
			//값을 누적시키자.
			//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
			apSnapShot_ModifiedMesh tmpModMeshSnapShot = new apSnapShot_ModifiedMesh();
			tmpModMeshSnapShot.Clear();
			tmpModMeshSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);
			
			//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
			float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

			for (int i = 0; i < pastableUnits.Count; i++)
			{
				//값을 누적시킨다.
				tmpModMeshSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedMesh, weight, methodType == 0);
			}

			//누적된 값을 ModMesh에 적용
			return tmpModMeshSnapShot.Load(targetModMesh);
		}








		public string GetClipboardName_ModMesh(int iSlot)
		{
			if (_clipboard_ModMeshes == null
				|| _clipboard_ModMeshes[iSlot] == null
				|| !_clipboard_ModMeshes[iSlot]._isDataSaved)
			{
				return null;
			}

			return _clipboard_ModMeshes[iSlot].Name;
		}


		public bool IsPastable_TF(apModifiedMesh targetModMesh, int iSlot)
		{
			if (targetModMesh == null
				|| _clipboard_ModMeshes == null
				|| _clipboard_ModMeshes[iSlot] == null)
			{
				return false;
			}
			

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_TFMod(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}

		public bool IsPastable_Morph(apModifiedMesh targetModMesh, int iSlot)
		{
			if (targetModMesh == null
				|| _clipboard_ModMeshes == null
				|| _clipboard_ModMeshes[iSlot] == null)
			{
				return false;
			}
			

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_MorphMod(targetModMesh);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 1-2. ModBone
		//--------------------------------------------------------------------
		public void Copy_ModBone(apModifiedBone modBone, string snapShotName, int iSlot)
		{
			if(_clipboard_ModBones == null)
			{
				_clipboard_ModBones = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD];
				for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
				{
					_clipboard_ModBones[i] = new apSnapShotStackUnit();
				}
			}
			if(_clipboard_ModBones[iSlot] == null)
			{
				_clipboard_ModBones[iSlot] = new apSnapShotStackUnit();
			}

			apSnapShotStackUnit curUnit = _clipboard_ModBones[iSlot];
			curUnit.Clear();
			curUnit.SetName(snapShotName);

			bool result = curUnit.SetSnapShot_ModBone(modBone, "Clipboard");
			if (!result)
			{
				curUnit.Clear();//<<저장 불가능하다.
			}
		}

		public bool Paste_ModBone_Single(apModifiedBone targetModBone, int iSlot)
		{
			if (targetModBone == null
				|| _clipboard_ModBones == null
				|| _clipboard_ModBones[iSlot] == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBones[iSlot].IsKeySyncable_TFMod(targetModBone);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_ModBones[iSlot].Load(targetModBone);
		}


		public bool Paste_ModBone_Multiple(apModifiedBone targetModBone, int iMainSlot, bool[] slots, int methodType)
		{
			if (targetModBone == null
				|| _clipboard_ModBones == null)
			{
				return false;
			}

			List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();

			//슬롯을 하나씩 체크한다.
			apSnapShotStackUnit curUnit = null;
			for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
			{
				if (i == iMainSlot || slots[i])
				{
					curUnit = _clipboard_ModBones[i];
					if (curUnit != null)
					{
						if(curUnit.IsKeySyncable_TFMod(targetModBone))
						{
							//다중 복사에 적용
							pastableUnits.Add(curUnit);
						}
					}
				}
			}

			if(pastableUnits.Count == 0)
			{
				//복사가 불가능하다.
				return false;
			}

			if(pastableUnits.Count == 1)
			{
				//1개라면 Single과 동일
				return pastableUnits[0].Load(targetModBone);
			}

			//이제 다중 복사를 해보자
			//가상의 유닛을 만들고,
			//값을 누적시키자.
			//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
			apSnapShot_ModifiedBone tmpModBoneSnapShot = new apSnapShot_ModifiedBone();
			tmpModBoneSnapShot.Clear();
			tmpModBoneSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);
			
			//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
			float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

			for (int i = 0; i < pastableUnits.Count; i++)
			{
				//값을 누적시킨다.
				tmpModBoneSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedBone, weight, methodType == 0);
			}

			//누적된 값을 ModMesh에 적용
			return tmpModBoneSnapShot.Load(targetModBone);
		}






		public string GetClipboardName_ModBone(int iSlot)
		{
			if (_clipboard_ModBones == null
				|| _clipboard_ModBones[iSlot] == null
				|| !_clipboard_ModBones[iSlot]._isDataSaved)
			{
				return null;
			}
			return _clipboard_ModBones[iSlot].Name;
		}

		public bool IsPastable(apModifiedBone targetModBone, int iSlot)
		{
			if (targetModBone == null
				|| _clipboard_ModBones == null
				|| _clipboard_ModBones[iSlot] == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_ModBones[iSlot].IsKeySyncable_TFMod(targetModBone);
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
			if(_clipboard_Keyframe == null)
			{
				_clipboard_Keyframe = new apSnapShotStackUnit();
			}
			_clipboard_Keyframe.Clear();
			_clipboard_Keyframe.SetName(snapShotName);
			bool result = _clipboard_Keyframe.SetSnapShot_Keyframe(keyframe, "Clipboard");
			if (!result)
			{
				_clipboard_Keyframe = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_Keyframe(apAnimKeyframe targetKeyframe)
		{
			if (targetKeyframe == null
				|| _clipboard_Keyframe == null)
			{
				return false;
			}

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
			if (_clipboard_Keyframe == null
				|| !_clipboard_Keyframe._isDataSaved)
			{
				return "";
			}
			return _clipboard_Keyframe.Name;
		}

		public bool IsPastable(apAnimKeyframe keyframe)
		{
			if (keyframe == null
				|| _clipboard_Keyframe == null)
			{
				return false;
			}

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

				apSnapShotStackUnit newUnit = new apSnapShotStackUnit();
				newUnit.Clear();
				newUnit.SetName("Keyframe");
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
			if(_clipboard_VertRig == null)
			{
				_clipboard_VertRig = new apSnapShotStackUnit();
			}
			_clipboard_VertRig.Clear();
			_clipboard_VertRig.SetName(snapShotName);
			bool result = _clipboard_VertRig.SetSnapShot_VertRig(modVertRig, "Clipboard");
			if (!result)
			{
				_clipboard_VertRig = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_VertRig(apModifiedVertexRig targetModVertRig)
		{
			if (targetModVertRig == null
				|| _clipboard_VertRig == null)
			{
				return false;
			}

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
			if (vertRig == null
				|| _clipboard_VertRig == null)
			{
				return false;
			}

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