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
	public class apSyncPlay
	{
		// Members
		//--------------------------------------------
		private apPortrait _parentPortrait = null;
		private apPortrait _targetPortrait = null;

		private bool _isSync_Anim = false;
		private bool _isSync_ControlParam = false;

		//연동된 데이터들
		public List<apSyncSet_AnimClip> _syncSet_AnimClip = null;
		public Dictionary<apAnimClip, apSyncSet_AnimClip> _animClip2SyncSet = null;
		public int _nSyncSet_AnimClip = 0;

		public List<apSyncSet_ControlParam> _sync_ControlParam = null;
		public Dictionary<apControlParam, apSyncSet_ControlParam> _controlParam2SyncSet = null;
		public int _nSyncSet_ControlParam = 0;
		
		// Init
		//--------------------------------------------
		public apSyncPlay(	apPortrait parentPortrait,
							apPortrait targetPortrait,
							bool isSyncAnim,
							bool isSyncControlParam)
		{
			_parentPortrait = parentPortrait;
			_targetPortrait = targetPortrait;
			_isSync_Anim = isSyncAnim;
			_isSync_ControlParam = isSyncControlParam;

			//연결을 하자
			if(_isSync_Anim 
				&& _parentPortrait._animClips != null
				&& _targetPortrait._animClips != null)
			{
				_syncSet_AnimClip = new List<apSyncSet_AnimClip>();
				_animClip2SyncSet = new Dictionary<apAnimClip, apSyncSet_AnimClip>();

				//전체 AnimClip을 돌면서 이름이 같은걸 찾자
				List<apAnimClip> animClips = _parentPortrait._animClips;
				List<apAnimClip> targetAnimClips = _targetPortrait._animClips;

				int nSrcAnimClips = animClips.Count;
				apAnimClip srcAnimClip = null;
				apAnimClip dstAnimClip = null;

				for (int iSrc = 0; iSrc < nSrcAnimClips; iSrc++)
				{
					srcAnimClip = animClips[iSrc];
					dstAnimClip = targetAnimClips.Find(delegate(apAnimClip a)
					{
						return string.Equals(srcAnimClip._name, a._name);
					});

					apSyncSet_AnimClip newSyncSet = new apSyncSet_AnimClip(srcAnimClip, dstAnimClip);
					_syncSet_AnimClip.Add(newSyncSet);
					_animClip2SyncSet.Add(srcAnimClip, newSyncSet);
				}

				_nSyncSet_AnimClip = _syncSet_AnimClip.Count;
			}
			else
			{
				_syncSet_AnimClip = null;
				_animClip2SyncSet = null;
				_nSyncSet_AnimClip = 0;
			}
			

			if(_isSync_ControlParam
				&& _parentPortrait._controller._controlParams != null
				&& _targetPortrait._controller._controlParams != null)
			{
				_sync_ControlParam = new List<apSyncSet_ControlParam>();
				_controlParam2SyncSet = new Dictionary<apControlParam, apSyncSet_ControlParam>();

				//전체 Control Param을 돌면서 이름이 같은걸 찾자
				List<apControlParam> controlParams = _parentPortrait._controller._controlParams;
				List<apControlParam> targetControlParams = _targetPortrait._controller._controlParams;

				int nSrcAnimClips = controlParams.Count;
				apControlParam srcAnimClip = null;
				apControlParam dstAnimClip = null;

				for (int iSrc = 0; iSrc < nSrcAnimClips; iSrc++)
				{
					srcAnimClip = controlParams[iSrc];
					dstAnimClip = targetControlParams.Find(delegate(apControlParam a)
					{
						return string.Equals(srcAnimClip._keyName, a._keyName)
								&& srcAnimClip._valueType == a._valueType;//타입도 같아야 한다.
					});

					apSyncSet_ControlParam newSyncSet = new apSyncSet_ControlParam(srcAnimClip, dstAnimClip);
					_sync_ControlParam.Add(newSyncSet);
					_controlParam2SyncSet.Add(srcAnimClip, newSyncSet);
				}
				_nSyncSet_ControlParam = _sync_ControlParam.Count;
			}
			else
			{
				_sync_ControlParam = null;
				_controlParam2SyncSet = null;
				_nSyncSet_ControlParam = 0;
			}
			

		}
		// Functions
		//--------------------------------------------
		public void SyncControlParams()
		{
			if(_nSyncSet_ControlParam == 0)
			{
				return;
			}

			for (int i = 0; i < _nSyncSet_ControlParam; i++)
			{
				_sync_ControlParam[i].Sync();
			}
		}
	}
}