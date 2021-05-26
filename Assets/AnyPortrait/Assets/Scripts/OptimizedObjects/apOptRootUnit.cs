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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// This class is the basic unit for performing updates including meshes.
	/// (You can refer to this in your script, but we do not recommend using it directly.)
	/// </summary>
	public class apOptRootUnit : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait _portrait = null;

		public apOptTransform _rootOptTransform = null;
		
		[HideInInspector]
		public Transform _transform = null;

		//빠른 처리를 위해 OptBone을 리스트로 저장해두자. 오직 참조용
		[SerializeField, HideInInspector]
		private List<apOptBone> _optBones = new List<apOptBone>();

		[SerializeField, HideInInspector]
		private List<apOptTransform> _optTransforms = new List<apOptTransform>();

		//빠른 참조를 위한 Dictionary
		[NonSerialized]
		private Dictionary<string, apOptBone> _optBonesMap = new Dictionary<string, apOptBone>();

		[NonSerialized]
		private Dictionary<string, apOptTransform> _optTransformMap = new Dictionary<string, apOptTransform>();

		public List<apOptTransform> OptTransforms
		{
			get
			{
				return _optTransforms;
			}
		}

		//추가 12.6 : SortedBuffer
		[SerializeField, NonBackupField]//백업하진 않는다. 다시 Bake하세염
		public apOptSortedRenderBuffer _sortedRenderBuffer = new apOptSortedRenderBuffer();

		//추가 2.25 : Flipped 체크
		private bool _isFlipped_X = false;
		private bool _isFlipped_Y = false;
		//private bool _isFlipped_X_Prev = false;
		//private bool _isFlipped_Y_Prev = false;
		//private bool _isFlipped = false;

		private bool _isFirstFlippedCheck = true;
		//private Vector3 _defaultScale = Vector3.one;

		//추가 21.4.3 : 가시성 정보를 멤버로 가진다. 루트유닛 전환시 에러가 발생하는걸 방지하기 위함
		[NonSerialized]
		public bool _isVisible = false;
		



		// Init
		//------------------------------------------------
		void Awake()
		{
			_transform = transform;

			_isFlipped_X = false;
			_isFlipped_Y = false;
			//_isFlipped_X_Prev = false;
			//_isFlipped_Y_Prev = false;
			//_isFlipped = false;

			_isFirstFlippedCheck = true;
		}

		void Start()
		{
			this.enabled = false;//<<업데이트를 하진 않습니다.

			_isFirstFlippedCheck = true;

			
		}

		// Update
		//------------------------------------------------
		void Update()
		{

		}

		void LateUpdate()
		{

		}


		// Bake
		//-----------------------------------------------
		public void ClearChildLinks()
		{
			if(_optBones == null)
			{
				_optBones = new List<apOptBone>();
			}
			_optBones.Clear();

			if(_optTransforms == null)
			{
				_optTransforms = new List<apOptTransform>();
			}
			_optTransforms.Clear();
		}

		public void AddChildBone(apOptBone bone)
		{
			_optBones.Add(bone);
		}

		public void AddChildTransform(apOptTransform optTransform, apSortedRenderBuffer.BufferData srcBufferData)//파라미터 추가
		{
			_optTransforms.Add(optTransform);
			_sortedRenderBuffer.Bake_AddTransform(optTransform, srcBufferData);//<< 부분 추가 12.6
		}

		//추가 12.6
		//Sorted Render Buffer에 대한 Bake
		public void BakeSortedRenderBuffer(apPortrait portrait, apRootUnit srcRootUnit)
		{
			if(_sortedRenderBuffer == null)
			{
				_sortedRenderBuffer = new apOptSortedRenderBuffer();
			}
			_sortedRenderBuffer.Bake_Init(portrait, this, srcRootUnit);
		}

		public void BakeComplete()
		{
			_sortedRenderBuffer.Bake_Complete();
		}

		
		// Link
		//------------------------------------------------
		public void Link(apPortrait portrait)
		{
			_sortedRenderBuffer.Link(portrait, this);

			for (int i = 0; i < _optTransforms.Count; i++)
			{
				_optTransforms[i].SetExtraDepthChangedEvent(OnExtraDepthChanged);
			}
		}

		//추가 19.5.28 : Async용으로 다시 작성된 함수
		public IEnumerator LinkAsync(apPortrait portrait, apAsyncTimer asyncTimer)
		{
			yield return _sortedRenderBuffer.LinkAsync(portrait, this, asyncTimer);

			for (int i = 0; i < _optTransforms.Count; i++)
			{
				_optTransforms[i].SetExtraDepthChangedEvent(OnExtraDepthChanged);
			}

			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
		}

		// Functions
		//------------------------------------------------
		public void UpdateTransforms(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			//추가 : Flipped 체크
			CheckFlippedTransform();
			


			//---------------------------------------------------------
//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Ready To Update Bones");
//#endif

			//추가 12.6
			//Sorted Buffer 업데이트 준비
			_sortedRenderBuffer.ReadyToUpdate();

			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif
			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Update Modifier");
//#endif
			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Pre");
//#endif
			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			
			//추가 12.6
			//Extra-Depth Changed 이벤트 있을 경우 처리 - Pre에서 다 계산되었을 것이다.
			_sortedRenderBuffer.UpdateDepthChangedEventAndBuffers();

			//---------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Update Bones World Matrix");
//#endif

			//Bone World Matrix Update
			//인자로는 지글본에 적용될 물리 값(시간과 여부)을 넣는다.
			_rootOptTransform.UpdateBonesWorldMatrix(	_portrait.PhysicsDeltaTime, 
														_portrait._isImportant && _portrait._isPhysicsPlay_Opt
													);

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			//------------------------------------------------------------

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Post (Modifier)");
//#endif

			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

//#if UNITY_EDITOR
//			Profiler.BeginSample("Root Unit - Calculate Post (Update)");
//#endif
			_rootOptTransform.UpdateCalculate_Post();//Post Calculate
			_rootOptTransform.UpdateMeshes();//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			
			
		}




		/// <summary>
		/// UpdateTransform의 Bake버전
		/// Bone 관련 부분 처리가 조금 다르다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdateTransformsForBake(float tDelta)
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			
			//추가
			//본 업데이트 1단계
			_rootOptTransform.ReadyToUpdateBones();

			//1. Modifer부터 업데이트 (Pre)
			_rootOptTransform.UpdateModifier_Pre(tDelta);
			//---------------------------------------------------------

			_rootOptTransform.ReadyToUpdateBones();

			//2. 실제로 업데이트
			_rootOptTransform.ReadyToUpdate();
			_rootOptTransform.UpdateCalculate_Pre();//Post 작성할 것


			//---------------------------------------------------------


			//Bone World Matrix Update
			_rootOptTransform.UpdateBonesWorldMatrix(0.0f, true);
			//_rootOptTransform.UpdateBonesWorldMatrixForBake();//<<이게 다르다


			//------------------------------------------------------------


			//Modifier 업데이트 (Post)
			_rootOptTransform.UpdateModifier_Post(tDelta);

			_rootOptTransform.UpdateCalculate_Post();//Post Calculate
			_rootOptTransform.UpdateMeshes();//추가 20.4.2 : UpdateCalculate_Post() 함수의 일부 코드가 뒤로 빠졌다.

		}



		public void UpdateTransformsOnlyMaskMesh()
		{
			if (_rootOptTransform == null)
			{
				return;
			}
			_rootOptTransform.UpdateMaskMeshes();
		}






		public void Show()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Show(true);

			_isVisible = true;//추가 21.4.3
		}

		public void ShowWhenBake()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ShowWhenBake(true);

			_isVisible = true;//추가 21.4.3
		}



		public void Hide()
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.Hide(true);
			
			_isVisible = false;//추가 21.4.3

		}

		public void ResetCommandBuffer(bool isRegistToCamera)
		{
			if (_rootOptTransform == null)
			{
				return;
			}

			_rootOptTransform.ResetCommandBuffer(isRegistToCamera);
		}


		// 추가 12.7 : Extra Option 관련 처리
		//------------------------------------------------------------------------------------
		private void OnExtraDepthChanged(apOptTransform optTransform, int deltaDepth)
		{
			if(deltaDepth == 0)
			{
				return;
			}

			_sortedRenderBuffer.OnExtraDepthChanged(optTransform, deltaDepth);
		}


		// 추가 2.25 : Flipped 관련 처리
		//-------------------------------------------------------------
		private void CheckFlippedTransform()
		{
			if(_isFirstFlippedCheck)
			{
				_transform = transform;
				//_defaultScale = _transform.localScale;

				//_isFlipped_X_Prev = _isFlipped_X;
				//_isFlipped_Y_Prev = _isFlipped_Y;
			}

			_isFlipped_X = _portrait._transform.lossyScale.x < 0.0f;
			_isFlipped_Y = _portrait._transform.lossyScale.y < 0.0f;

			//Debug.Log("Check Flipped Transform : " + _portrait._transform.lossyScale);

			//일단 이부분 미적용
			//if (_isFlipped_X_Prev != _isFlipped_X ||
			//	_isFlipped_Y_Prev != _isFlipped_Y ||
			//	_isFirstFlippedCheck)
			//{
			//	_transform.localScale = new Vector3(
			//		(_isFlipped_X ? -_defaultScale.x : _defaultScale.x),
			//		(_isFlipped_Y ? -_defaultScale.y : _defaultScale.y),
			//		_defaultScale.z
			//		);

			//	_isFlipped_X_Prev = _isFlipped_X;
			//	_isFlipped_Y_Prev = _isFlipped_Y;

			//	_rootOptTransform.SetRootFlipped(_isFlipped_X, _isFlipped_Y);

			//	//Debug.LogWarning("Flip Changed");
			//}

			_isFirstFlippedCheck = false;
		}
		
		public bool IsFlippedX { get { return _isFlipped_X; } }
		public bool IsFlippedY { get { return _isFlipped_Y; } }


		// 추가 19.8.19 : SortingOption 관련
		//---------------------------------------------------
		public void SetSortingOrderChangedAutomatically(bool isEnabled)
		{
			if(_sortedRenderBuffer == null)
			{
				return;
			}
			_sortedRenderBuffer.SetSortingOrderChangedAutomatically(isEnabled);
		}

		public bool RefreshSortingOrderByDepth()
		{
			if(_sortedRenderBuffer == null)
			{
				return false;
			}
			_sortedRenderBuffer.Link(_portrait, this);

			return _sortedRenderBuffer.RefreshSortingOrderByDepth();
		}

		public void SetSortingOrderOption(apPortrait.SORTING_ORDER_OPTION sortingOrderOption, int sortingOrderPerDepth)
		{
			if(_sortedRenderBuffer == null)
			{
				return;
			}
			_sortedRenderBuffer.SetSortingOrderOption(sortingOrderOption, sortingOrderPerDepth);
		}

		// Get / Set
		//------------------------------------------------
		public apOptBone GetBone(string name)
		{
			//일단 빠른 검색부터
			if(_optBonesMap.ContainsKey(name))
			{
				return _optBonesMap[name];
			}

			apOptBone resultBone = _optBones.Find(delegate (apOptBone a)
			{
				return string.Equals(a._name, name);
			});

			if(resultBone == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optBonesMap.Add(name, resultBone);

			return resultBone;
		}

		public apOptTransform GetTransform(string name)
		{
			//일단 빠른 검색부터
			if(_optTransformMap.ContainsKey(name))
			{
				return _optTransformMap[name];
			}

			apOptTransform resultTransform = _optTransforms.Find(delegate (apOptTransform a)
			{
				return string.Equals(a._name, name);
			});

			if(resultTransform == null)
			{
				return null;
			}

			//빠른 검색 리스트에 넣고
			_optTransformMap.Add(name, resultTransform);

			return resultTransform;
		}
		
		public apOptTransform GetTransform(int transformID)
		{
			return _optTransforms.Find(delegate(apOptTransform a)
			{
				return a._transformID == transformID;
			});
		}
	}

}