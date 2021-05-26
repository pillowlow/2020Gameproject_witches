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
	/// <summary>
	/// apSortedRenderBuffer의 Opt 버전의 클래스
	/// MeshGroup이 아닌 OptRootUnit의 멤버로서 작동한다.
	/// 렌더링 순서를 계산한 뒤, 각각의 OptTransform의 Local-Z를 계산하는 역할을 한다.
	/// "원래의 Depth/Level/Local Z"를 기억하고 캐싱도 수행한다.
	/// (이전 버전에서 그대로 열었을 경우 이 기능이 정상적으로 작동하지 않을 수 있다.)
	/// </summary>
	[Serializable]
	public class apOptSortedRenderBuffer
	{
		// Members
		//----------------------------------------------------------
		// 기존과 달리 Depth Sorted를 계속 하는 것도 아니고, 처음에 Bake된 값을 그대로 사용할 수도 있다.
		// Depth가 변경된 이벤트(Depth가 초기화 되었을 때에도)를 별도로 체크하여 Root Unit에 보낸다.
		// [초기 상태] -> 이벤트 없으며 동작 안함 -> 이전 버전은 이상태
		// [Depth가 변경된 상태] -> 이벤트가 발생하여 OptTransform들의 Local-Z가 모두 바뀜
		// [다시 초기 상태] -> 이때도 이벤트가 발생한다! -> 초기의 Local-Z로 돌아온다.
		
		//기본적인 BufferData는 Bake때 만든다. GroupSize 때문에..
		[Serializable]
		public class OptBufferData
		{
			public int _indexOriginal = 0;
			public int _indexChanged = 0;
			public int _level = 0;
			public int _groupSize = 0;
			public int _optTransformID = -1;

			[NonSerialized]
			public apOptTransform _optTransform = null;

			public bool _isMesh = false;
			public bool _isClippedChild = false;
			public bool _isClippedParent = false;

			[NonSerialized]
			public OptBufferData _parentBuffer = null;
			[NonSerialized]
			public List<OptBufferData> _childBuffers = new List<OptBufferData>();

			//Opt에서 추가 : 기본 Z값
			public float _defaultLocalZ = 0.0f;

			[NonSerialized]
			public float _calculatedLocalZ = 0.0f;

			public OptBufferData()
			{
				_parentBuffer = null;
				if (_childBuffers == null)
				{
					_childBuffers = new List<OptBufferData>();
				}
				_childBuffers.Clear();
			}

			public void Bake(apSortedRenderBuffer.BufferData srcBufferData, apOptTransform linkedOptTransform)
			{
				_indexOriginal = srcBufferData._indexOriginal;
				_indexChanged = srcBufferData._indexChanged;
				_level = srcBufferData._level;
				_groupSize = srcBufferData._groupSize;
				_optTransformID = linkedOptTransform._transformID;
				_optTransform = linkedOptTransform;
				_isMesh = srcBufferData._isMesh;
				_isClippedChild = srcBufferData._isClippedChild;
				_isClippedParent = srcBufferData._isClippingParent;

				_defaultLocalZ = linkedOptTransform.transform.localPosition.z;
			}

			public void Link(apOptTransform linkedOptTransform)
			{
				_optTransform = linkedOptTransform;
			}

			public void ResetLink()
			{
				_parentBuffer = null;
				if (_childBuffers == null)
				{
					_childBuffers = new List<OptBufferData>();
				}
				_childBuffers.Clear();
			}

			public void SetParent(OptBufferData parent)
			{
				_parentBuffer = parent;
				if (!_parentBuffer._childBuffers.Contains(this))
				{
					_parentBuffer._childBuffers.Add(this);
				}
			}

			

			public void CalculateLocalDepthZ(float ZPerDepth)
			{
				if(_parentBuffer != null)
				{
					_calculatedLocalZ = -1.0f * (_indexChanged - _parentBuffer._indexChanged) * ZPerDepth;
				}
				else
				{
					_calculatedLocalZ = -1.0f * _indexChanged * ZPerDepth;
				}
			}


		}

		[NonSerialized]
		private Dictionary<apOptTransform, OptBufferData> _optTransform2Buff = new Dictionary<apOptTransform, OptBufferData>();

		[SerializeField]
		private int _nOptTransforms = 0;
		[SerializeField]
		private OptBufferData[] _buffers = null;
		[SerializeField]
		private OptBufferData[] _buffers_DepthChanged = null;

		[NonSerialized]
		private List<OptBufferData> _bakedBuffers = new List<OptBufferData>();//<<이건 오로지 Bake를 위한 변수이다.
		
		[NonSerialized]
		private bool _isDepthChanged_Prev = false;
		[NonSerialized]
		private bool _isDepthChanged = false;
		[NonSerialized]
		private bool _isNeedToSortDepthChangedBuffers = true;
		[NonSerialized]
		private bool _isNeedToApplyDepthChangedBuffers = true;
		[NonSerialized]
		private int _nDepthChangedRequest = 0;
		[NonSerialized]
		private Dictionary<apOptTransform, int> _depthChangedRequests = new Dictionary<apOptTransform, int>();
		[NonSerialized]
		private Dictionary<apOptTransform, int> _depthChangedCache = new Dictionary<apOptTransform, int>();

		//Opt에서 추가
		//[NonSerialized]
		//private apOptRootUnit _parentRootUnit = null;

		[SerializeField]
		private bool _isBaked = false;//<<이건 Bake 한번만 하면 True가 된다.

		[SerializeField]
		private float _ZPerDepth = 0.0f;

		//추가 19.8.19
		[SerializeField]
		private apPortrait.SORTING_ORDER_OPTION _sortingOrderOption = apPortrait.SORTING_ORDER_OPTION.SetOrder;

		//추가 21.1.31
		[SerializeField]
		private int _sortingOrderPerDepth = 1;

		[NonSerialized]
		private bool _isAutoSortingOrderEnabled = true;//_sortingOrderOption에 따라서 Sorting Order를 자동을 변경하는 것을 외부에서 막을 수 있다.
		[NonSerialized]
		private bool _isAutoSortingOrder = true;//Sorting Option과 Enabled에 따라서 자동으로 Sorting Order를 설정해야하는가

		// Init / Bake
		//----------------------------------------------------------
		public apOptSortedRenderBuffer()
		{
			Clear();
		}

		public void Clear()
		{
			_optTransform2Buff.Clear();
			_nOptTransforms = 0;

			_buffers = null;
			_buffers_DepthChanged = null;
			

			_isDepthChanged = false;
			_isDepthChanged_Prev = false;
			_isNeedToSortDepthChangedBuffers = true;//<<캐시가 초기화되었으므로 일단 무조건 다시 Sort해야한다.
			_isNeedToApplyDepthChangedBuffers = true;
			_nDepthChangedRequest = 0;
			_depthChangedRequests.Clear();
			_depthChangedCache.Clear();

			SetSortingOrderChangedAutomatically(false);
		}


		// Bake
		//----------------------------------------------------------
		//Render Buffer는 Bake가 3단계로 이루어진다.
		//데이터가 절차적으로 생성되는 바람에.. 어쩔 수 없이... 힁
		public void Bake_Init(apPortrait portrait, apOptRootUnit rootUnit, apRootUnit srcRootUnit)
		{
			Clear();
			
			_bakedBuffers.Clear();
			_ZPerDepth = portrait._bakeZSize;

			_sortingOrderOption = portrait._sortingOrderOption;//추가 19.8.19

			//추가 21.1.31
			_sortingOrderPerDepth = portrait._sortingOrderPerDepth;
			if(_sortingOrderPerDepth < 1)
			{
				_sortingOrderPerDepth = 1;
			}

			SetSortingOrderChangedAutomatically(true);
		}

		public void Bake_AddTransform(apOptTransform bakedOptTransform, apSortedRenderBuffer.BufferData srcBufferData)
		{
			OptBufferData newBuff = new OptBufferData();
			newBuff.Bake(srcBufferData, bakedOptTransform);
			_bakedBuffers.Add(newBuff);
		}

		public void Bake_Complete()
		{
			//Bake된 BufferData를 정렬
			//Index 순으로 정렬하고, 다시 Index를 부여한다.
			_bakedBuffers.Sort(delegate(OptBufferData a, OptBufferData b)
			{
				return a._indexOriginal - b._indexOriginal;
			});
			for (int i = 0; i < _bakedBuffers.Count; i++)
			{
				_bakedBuffers[i]._indexOriginal = i;
				_bakedBuffers[i]._indexChanged = i;
			}

			_nOptTransforms = _bakedBuffers.Count;
			_buffers = new OptBufferData[_nOptTransforms];
			_buffers_DepthChanged = new OptBufferData[_nOptTransforms];

			OptBufferData curBuff = null;
			for (int i = 0; i < _bakedBuffers.Count; i++)
			{
				curBuff = _bakedBuffers[i];
				_buffers[i] = curBuff;
				_buffers_DepthChanged[i] = curBuff;
			}

			_isBaked = true;

			//추가 19.8.19 : 옵션에 따라 이 단계에서 각각의 메시의 SortingOrder를 지정해야할 수도있다.

			SetSortingOrderChangedAutomatically(true);


			if(_isAutoSortingOrder)
			{
				//Debug.Log("Set Sorting Order (Bake)");

				Vector3 prevPos = Vector3.zero;
				for (int i = 0; i < _nOptTransforms; i++)
				{
					curBuff = _buffers[i];

					//추가 19.8.19 : 여기서 Depth 옵션 적용
					if(curBuff._optTransform._childMesh != null)
					{
						try
						{
							if (_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(curBuff._indexOriginal * _sortingOrderPerDepth);
								//Debug.Log("[" + i + "] : " + curBuff._optTransform._childMesh.name + " >> " + curBuff._indexOriginal);
							}
							else if (_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(-curBuff._indexOriginal * _sortingOrderPerDepth);
								//Debug.Log("[" + i + "] : " + curBuff._optTransform._childMesh.name + " >> " + (-curBuff._indexOriginal));
							}
						}
						catch(Exception)
						{
							//Debug.LogError("Exception : " + ex);
						}
					}
				}
			}

		}


		// Link
		//--------------------------------------------------------------------------
		//OptTransform을 연결해줘야 한다.
		//단, Bake가 안되었다면 이 코드는 패스한다.
		public void Link(apPortrait portrait, apOptRootUnit rootUnit)
		{
			if(!_isBaked)
			{
				return;
			}
			//_parentRootUnit = rootUnit;
			if(_nOptTransforms == 0)
			{
				return;
			}
			OptBufferData curBuff = null;
			apOptTransform linkedOptTransform = null;

			if(_optTransform2Buff == null)
			{
				_optTransform2Buff = new Dictionary<apOptTransform, OptBufferData>();
			}
			_optTransform2Buff.Clear();

			for (int i = 0; i < _nOptTransforms; i++)
			{
				curBuff = _buffers[i];
				linkedOptTransform = rootUnit.GetTransform(curBuff._optTransformID);
				if(linkedOptTransform == null)
				{
					curBuff._optTransform = null;
					continue;
				}

				curBuff._optTransform = linkedOptTransform;
				curBuff.ResetLink();

				//OptTransform2Buff에도 연결
				if (_optTransform2Buff.ContainsKey(linkedOptTransform))
				{
					continue;
				}

				_optTransform2Buff.Add(linkedOptTransform, curBuff);
				
				
			}

			//OptTransform을 돌면서 부모/자식 관계를 연결해준다.
			apOptTransform parentOptTransform = null;
			OptBufferData parentBuff = null;
			for (int i = 0; i < _nOptTransforms; i++)
			{
				curBuff = _buffers[i];
				linkedOptTransform = curBuff._optTransform;
				if(linkedOptTransform == null)
				{
					continue;
				}

				parentOptTransform = linkedOptTransform._parentTransform;
				if(parentOptTransform != null)
				{
					if(_optTransform2Buff.ContainsKey(parentOptTransform))
					{
						parentBuff = _optTransform2Buff[parentOptTransform];
						curBuff.SetParent(parentBuff);//<<부모로 연결
					}
				}
			}
			
			_isNeedToSortDepthChangedBuffers = true;
			_isNeedToApplyDepthChangedBuffers = true;
			//현재 상태 + 이전 상태에 대해서 모두 값을 가지고 "상태가 바뀔 때" 이벤트를 호출해야한다.
			_isDepthChanged = false;
			_isDepthChanged_Prev = false;//<<에디터와 다르게 이 변수에 따라서 "다시 초기화"라는 이벤트를 만들어야 한다.

			if(_sortingOrderPerDepth < 1)
			{
				_sortingOrderPerDepth = 1;
			}
			//옵션에 따라 자동으로 Mesh의 Sorting Order를 설정해야한다.
			SetSortingOrderChangedAutomatically(true);
		}


		public IEnumerator LinkAsync(apPortrait portrait, apOptRootUnit rootUnit, apAsyncTimer asyncTimer)
		{
			if(!_isBaked)
			{
				//return;
				yield break;
			}
			//_parentRootUnit = rootUnit;
			if(_nOptTransforms == 0)
			{
				//return;
				yield break;
			}
			OptBufferData curBuff = null;
			apOptTransform linkedOptTransform = null;

			if(_optTransform2Buff == null)
			{
				_optTransform2Buff = new Dictionary<apOptTransform, OptBufferData>();
			}
			_optTransform2Buff.Clear();

			for (int i = 0; i < _nOptTransforms; i++)
			{
				curBuff = _buffers[i];
				linkedOptTransform = rootUnit.GetTransform(curBuff._optTransformID);
				if(linkedOptTransform == null)
				{
					curBuff._optTransform = null;
					continue;
				}

				curBuff._optTransform = linkedOptTransform;
				curBuff.ResetLink();

				//OptTransform2Buff에도 연결
				if (_optTransform2Buff.ContainsKey(linkedOptTransform))
				{
					continue;
				}

				_optTransform2Buff.Add(linkedOptTransform, curBuff);
				
				//Async Wait
				if(asyncTimer.IsYield())
				{
					yield return asyncTimer.WaitAndRestart();
				}
			}

			//OptTransform을 돌면서 부모/자식 관계를 연결해준다.
			apOptTransform parentOptTransform = null;
			OptBufferData parentBuff = null;
			for (int i = 0; i < _nOptTransforms; i++)
			{
				curBuff = _buffers[i];
				linkedOptTransform = curBuff._optTransform;
				if(linkedOptTransform == null)
				{
					continue;
				}

				parentOptTransform = linkedOptTransform._parentTransform;
				if(parentOptTransform != null)
				{
					if(_optTransform2Buff.ContainsKey(parentOptTransform))
					{
						parentBuff = _optTransform2Buff[parentOptTransform];
						curBuff.SetParent(parentBuff);//<<부모로 연결
					}
				}
			}
			
			_isNeedToSortDepthChangedBuffers = true;
			_isNeedToApplyDepthChangedBuffers = true;
			//현재 상태 + 이전 상태에 대해서 모두 값을 가지고 "상태가 바뀔 때" 이벤트를 호출해야한다.
			_isDepthChanged = false;
			_isDepthChanged_Prev = false;//<<에디터와 다르게 이 변수에 따라서 "다시 초기화"라는 이벤트를 만들어야 한다.

			//옵션에 따라 자동으로 Mesh의 Sorting Order를 설정해야한다.
			SetSortingOrderChangedAutomatically(true);

			//Async Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
		}



		// Get / Set
		//--------------------------------------------------------------------
		public void SetSortingOrderChangedAutomatically(bool isEnabled)
		{
			_isAutoSortingOrderEnabled = isEnabled;
			if (!_isAutoSortingOrderEnabled)
			{
				_isAutoSortingOrder = false;
			}
			else
			{
				if(_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder ||
					_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					_isAutoSortingOrder = true;
				}
				else
				{
					_isAutoSortingOrder = false;
				}
				
			}
		}
		

		public bool RefreshSortingOrderByDepth()
		{
			
			if (_sortingOrderOption != apPortrait.SORTING_ORDER_OPTION.DepthToOrder
				&& _sortingOrderOption != apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
			{
				Debug.LogError("AnyPortrait : This function cannot be executed unless [Sorting Order Option] is [Depth To Order] or [Reverse Depth To Order].");
				return false;
			}
			if(_isAutoSortingOrder)
			{
				OptBufferData curBuff = null;
				Vector3 prevPos = Vector3.zero;
				for (int i = 0; i < _nOptTransforms; i++)
				{
					curBuff = _buffers[i];
					if(curBuff._optTransform == null)
					{
						//Debug.LogError("Not Linked");
						continue;
					}
					//추가 19.8.19 : 여기서 Depth 옵션 적용
					if(curBuff._optTransform._childMesh != null)
					{
						try
						{
							if (_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(curBuff._indexOriginal * _sortingOrderPerDepth);
								//Debug.Log("[" + i + "] : " + curBuff._optTransform._childMesh.name + " >> " + curBuff._indexOriginal);
							}
							else if (_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(-curBuff._indexOriginal * _sortingOrderPerDepth);
								//Debug.Log("[" + i + "] : " + curBuff._optTransform._childMesh.name + " >> " + (-curBuff._indexOriginal));
							}
						}
						catch(Exception ex)
						{
							Debug.LogError("AnyPortrait : Processing of [Sorting Order Option] is failed / Exception : " + ex);
							return false;
						}
					}
				}
				return true;
			}

			Debug.LogError("AnyPortrait : [Sorting Order] is not ready for change.");
			return false;
		}

		public void SetSortingOrderOption(apPortrait.SORTING_ORDER_OPTION sortingOrderOption, int sortingOrderPerDepth)
		{
			_sortingOrderOption = sortingOrderOption;
			_sortingOrderPerDepth = sortingOrderPerDepth;
		}

		// Functions
		//----------------------------------------------------------
		/// <summary>
		/// 업데이트 초반에 호출하자
		/// </summary>
		public void ReadyToUpdate()
		{
			_isDepthChanged = false;
			if(_nDepthChangedRequest > 0)
			{
				_nDepthChangedRequest = 0;
				_depthChangedRequests.Clear();
			}
			
		}



		/// <summary>
		/// Extra 이벤트에 의해서 Depth를 바꿔야 하는 경우 호출
		/// </summary>
		/// <param name="optTransform"></param>
		/// <param name="deltaDepth"></param>
		public void OnExtraDepthChanged(apOptTransform optTransform, int deltaDepth)
		{
			if(!_isBaked || deltaDepth == 0)
			{
				return;
			}
			if(optTransform._childMesh != null && optTransform._childMesh._isMaskChild)
			{
				//Clipping Child의 Depth 이동은 허용하지 않는다.
				return;
			}

			//Debug.Log("OnExtraDepthChanged [" + optTransform.name + " - "+ deltaDepth + "]");

			//일단 DepthChanged 이벤트가 발생했음을 알리고
			_isDepthChanged = true;
			_nDepthChangedRequest++;

			//어떤 RenderUnit의 Depth가 바뀌었는지 저장한다.
			if(_depthChangedRequests.ContainsKey(optTransform))
			{
				//키가 있을리가 없는데..
				_depthChangedRequests[optTransform] = deltaDepth;
			}
			else
			{
				_depthChangedRequests.Add(optTransform, deltaDepth);
			}

			//캐시 미스 여부를 찾는다.
			if(!_depthChangedCache.ContainsKey(optTransform))
			{
				//만약 캐시에 없는 거라면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				_isNeedToApplyDepthChangedBuffers = true;
			}
			else if(_depthChangedCache[optTransform] != deltaDepth)
			{
				//만약 캐시와 값이 다르다면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				_isNeedToApplyDepthChangedBuffers = true;
			}
			//else
			//{
			//	Debug.LogWarning(">> Cache Hit");
			//}
		}







		/// <summary>
		/// 모든 RenderUnit 업데이트가 끝나고, Depth 이벤트에 따라 출력 순서를 바꾸어야 하는지 확인한다.
		/// </summary>
		public void UpdateDepthChangedEventAndBuffers()
		{
			if(!_isBaked || _nOptTransforms == 0 || !Application.isPlaying)
			{
				//렌더 유닛이 없당..
				//+ 어플리케이션 실행 중이 아닐때는 작동하지 않도록 하자
				_isNeedToApplyDepthChangedBuffers = true;
				return;
			}
			
			if(!_isDepthChanged)
			{
				_isNeedToApplyDepthChangedBuffers = true;//<<다음에 실행될 때에는 캐시 여부상관없이 적용해야한다.
			}
			if(_isDepthChanged == _isDepthChanged_Prev)
			{
				//Opt 전용 : Depth 변화 여부가 같다면..
				if (!_isDepthChanged || _nDepthChangedRequest == 0)
				{
					//Depth가 바뀐 적이 없다.
					return;
				}
			}
			
			_isDepthChanged_Prev = _isDepthChanged;
			if (_isDepthChanged)
			{
				if (!_isNeedToSortDepthChangedBuffers)
				{
					//Depth 변경 캐시가 모두 히트했을 때,
					//개수까지 같아야 인정.
					if (_depthChangedRequests.Count != _depthChangedCache.Count)
					{
						_isNeedToSortDepthChangedBuffers = true;
					}
				}

				
				//재정렬을 해야한다.
				if (_isNeedToSortDepthChangedBuffers)
				{

					//일단 재정렬을 할 예정이니 캐시는 현재 값으로 갱신
					
					_depthChangedCache.Clear();
					foreach (KeyValuePair<apOptTransform, int> request in _depthChangedRequests)
					{
						_depthChangedCache.Add(request.Key, request.Value);
					}

					//이제 다시 정렬을 해보자
					//- 인덱스 스왑만 먼저 한다.
					//- 버퍼 정렬을 하려면.. Array가 필요하당..



					//먼저 Buffer_DepthChanged를 복사하여 붙여넣는다.
					//이때, Index_Changed는 원래대로 돌려놓는다.
					OptBufferData curBufferData = null;
					for (int i = 0; i < _nOptTransforms; i++)
					{
						curBufferData = _buffers[i];
						curBufferData._indexChanged = curBufferData._indexOriginal;
						_buffers_DepthChanged[i] = curBufferData;
					}


					apOptTransform curRenderUnit = null;

					//Debug.Log("------------------------------");
					//DebugBuffers(_buffers, "원래 순서", false);

					int deltaDepth = 0;
					foreach (KeyValuePair<apOptTransform, int> request in _depthChangedRequests)
					{
						curRenderUnit = request.Key;
						curBufferData = _optTransform2Buff[curRenderUnit];
						deltaDepth = request.Value;
						if (deltaDepth == 0)
						{
							continue;
						}

						//Debug.Log("> Request : [" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 (현재 Index : " + curBufferData._indexChanged + " / Group Size : " + curBufferData._groupSize + ")");

						//TODO
						//BufferData의 인덱스 스왑을 한 후,
						//_buffers_DepthChanged에 변경된 인덱스에 맞게 넣는다.
						//얼마큼 많이 이동하는지 체크
						//실제 이동되는 인덱스
						int realMovedOffset = 0;
						//Depth만큼 이동하기 위한 카운트와 최대치
						int depthCount = 0;
						int maxDepthCount = Mathf.Abs(deltaDepth);
						int moveDir = (deltaDepth > 0) ? 1 : -1;


						int iCheck = (deltaDepth > 0) ? (curBufferData._indexChanged + curBufferData._groupSize) : (curBufferData._indexChanged - 1);

						OptBufferData nextBuff = null;
						while (true)
						{

							if (iCheck < 0 || iCheck >= _nOptTransforms)
							{
								//렌더 유닛 범위를 넘어갔다면
								break;
							}
							if (depthCount >= maxDepthCount)
							{
								//Depth 카운트를 모두 셌다면
								break;
							}

							nextBuff = _buffers_DepthChanged[iCheck];

							//- 자신보다 Level이 높은 경우(하위인 경우) : 카운트하지 않고 이동한다.
							//- 자신보다 Level이 같고 같은 Parent를 공유하는 경우 : 카운트 1개 하고 이동한다. 카운트 찬 경우 종료
							//- 자신보다 Level이 낮은 경우(상위인 경우) 또는 Level이 같아도 Parent를 공유하지 않는 경우(에러) : 이동 종료
							//- 만약 이동 도중 ClippingChild를 만나면 : 카운트하지 않고 이동한다.
							if (nextBuff._level > curBufferData._level)
							{
								//Level이 높거나(하위 레벨)이라면 패스
								realMovedOffset += moveDir;
								iCheck += moveDir;
								//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Next가 하위 레벨이다. (" + nextBuff._level + " > " + curBufferData._level + ")");
							}
							else if (nextBuff._level == curBufferData._level)
							{
								if (nextBuff._isClippedChild)
								{
									//같은 레벨의 ClippedChild라면 패스
									realMovedOffset += moveDir;
									iCheck += moveDir;

									//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Clipped Child이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
								}
								else if (nextBuff._parentBuffer != curBufferData._parentBuffer)
								{
									//Level이 같지만 Parent가 다르다면 사촌이다. 이 경우 이동 종료
									//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] Parent가 다른 사촌이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
									break;
								}
								else
								{
									//Level이 같고 Parent를 공유하면 같은 형제이므로 Depth 카운트를 하나 올리고 이동
									depthCount++;
									realMovedOffset += moveDir;
									iCheck += moveDir;
									//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트하고 이동(" + depthCount + ")] 형제 Unit이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
								}
							}
							else
							{
								//상위 레벨이라면 바로 종료
								//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] 상위 레벨이다. (" + nextBuff._level + " < " + curBufferData._level + ")");
								break;
							}

						}

						if (realMovedOffset == 0)
						{
							continue;
						}

						//Debug.Log("- 이동 범위 : " + realMovedOffset + " (Depth : " + deltaDepth + ")");

						//"이동할 간격"이 결정되면, 그 영역만큼 Index를 바꾸자
						int swappedIndex_Start = 0;
						int swappedIndex_End = 0;
						int nSwapped = Mathf.Abs(realMovedOffset);



						if (deltaDepth > 0)
						{
							swappedIndex_Start = curBufferData._indexChanged + curBufferData._groupSize;
							swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

							//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (-curBufferData._groupSize));

							//Depth가 증가했다면, 상대 위치는 GroupSize 만큼 감소해야한다.
							for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
							{
								_buffers_DepthChanged[i]._indexChanged -= curBufferData._groupSize;
							}
						}
						else
						{
							swappedIndex_Start = curBufferData._indexChanged - nSwapped;
							swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

							//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (+curBufferData._groupSize));

							//Depth가 감소했다면, 상대 위치는 GroupSize 만큼 증가해야한다.
							for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
							{
								_buffers_DepthChanged[i]._indexChanged += curBufferData._groupSize;
							}
						}

						//이제 해당 그룹을 이동시키자
						int groupIndex_Start = curBufferData._indexChanged;
						int groupIndex_End = curBufferData._indexChanged + (curBufferData._groupSize - 1);

						//Debug.Log(">> 움직이는 Group 범위 [" + groupIndex_Start + "~" + groupIndex_End + " : " + realMovedOffset);

						for (int i = groupIndex_Start; i <= groupIndex_End; i++)
						{
							_buffers_DepthChanged[i]._indexChanged += realMovedOffset;
						}

						//리스트 순서를 다시 정리
						//이번에는 변화된 위치로 직접 넣는다.
						OptBufferData movedBuf = null;

						//일단 배열은 초기화
						for (int i = 0; i < _nOptTransforms; i++)
						{
							_buffers_DepthChanged[i] = null;
						}


						for (int i = 0; i < _nOptTransforms; i++)
						{
							movedBuf = _buffers[i];
							_buffers_DepthChanged[movedBuf._indexChanged] = movedBuf;
						}

						//DebugBuffers(_buffers_DepthChanged, "[" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 후 결과", true);
					}

					//Debug.Log("------------------------------");
					_nDepthChangedRequest = 0;
					_depthChangedRequests.Clear();

					//리스트에도 넣자
					//_renderUnits_Sorted.Clear();

					//BufferData sortedBufferData = null;
					//for (int i = 0; i < _buffers_DepthChanged.Length; i++)
					//{
					//	sortedBufferData = _buffers_DepthChanged[i];
					//	if (sortedBufferData != null && sortedBufferData._renderUnit != null)
					//	{
					//		_renderUnits_Sorted.Add(sortedBufferData._renderUnit);
					//	}
					//	else
					//	{
					//		//Debug.LogError("Sort 에러 : Null값이 발생했다.");
					//	}
					//}

					//> Opt에서는 리스트에 넣는 대신 이벤트를 호출한다.

					
				}
				//else
				//{
				//	//Depth는 바뀌었으나 이전과 동일한 정렬값을 사용한다.
				//	//고로 또 호출할 필요가 없다.
				//}

				if(_isNeedToSortDepthChangedBuffers || _isNeedToApplyDepthChangedBuffers)
				{
					//Depth가 바뀌었다.
					//> Depth에 맞게 Z를 다시 바꾸어야 한다.
					//Debug.Log("Depth Changed");

					
					OptBufferData curBuff = null;
					Vector3 prevPos = Vector3.zero;
					for (int i = 0; i < _nOptTransforms; i++)
					{
						//새로운 인덱스만큼 Depth를 줘야 한다.
						curBuff = _buffers_DepthChanged[i];
						curBuff.CalculateLocalDepthZ(_ZPerDepth);

						prevPos = curBuff._optTransform._transform.localPosition;
						prevPos.z = curBuff._calculatedLocalZ;
						curBuff._optTransform._transform.localPosition = prevPos;

						//추가 19.8.19 : 여기서 Depth 옵션 적용
						if(_isAutoSortingOrder && curBuff._optTransform._childMesh != null)
						{
							if(_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(curBuff._indexChanged * _sortingOrderPerDepth);
							}
							else if(_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
							{
								curBuff._optTransform._childMesh.SetSortingOrder(-curBuff._indexChanged * _sortingOrderPerDepth);
							}
						}
						
						//Debug.Log("[" + curBuff._indexChanged + "] " + curBuff._optTransform.name + " (" + curBuff._indexOriginal + " > " + curBuff._indexChanged + " )");

						
						//단 재귀적으로 줘야 한다. > Level 이용하면 되는거 아닌가염 > 아니넹.. -_-
					}

					for (int i = 0; i < _nOptTransforms; i++)
					{
						curBuff = _buffers_DepthChanged[i];
						//빌보드가 계산된 후 Depth가 바뀌면 그 프레임에선 빌보드가 이상하게 보인다.
						//다시 Matrix를 계산하도록 만든다.
						//if(curBuff._indexOriginal != curBuff._indexChanged)
						{
							curBuff._optTransform.CorrectBillboardMatrix();
						}
					}
				}

				_isNeedToSortDepthChangedBuffers = false;
				_isNeedToApplyDepthChangedBuffers = false;
			}
			else
			{
				//Depth가 바뀌지 않았다.
				//> 원래 Z 값으로 회귀해야한다.
				//Debug.Log("Reset to Initialized Depth");

				OptBufferData curBuff = null;
				Vector3 prevPos = Vector3.zero;
				for (int i = 0; i < _nOptTransforms; i++)
				{
					curBuff = _buffers[i];

					//기존에 저장했던 위치로 이동
					prevPos = curBuff._optTransform._transform.localPosition;
					prevPos.z = curBuff._defaultLocalZ;
					curBuff._optTransform._transform.localPosition = prevPos;

					//추가 19.8.19 : 여기서 Depth 옵션 적용
					if(_isAutoSortingOrder && curBuff._optTransform._childMesh != null)
					{
						if(_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder)
						{
							curBuff._optTransform._childMesh.SetSortingOrder(curBuff._indexOriginal * _sortingOrderPerDepth);
						}
						else if(_sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
						{
							curBuff._optTransform._childMesh.SetSortingOrder(-curBuff._indexOriginal * _sortingOrderPerDepth);
						}
					}
				}

				for (int i = 0; i < _nOptTransforms; i++)
				{
					curBuff = _buffers[i];
					//빌보드가 계산된 후 Depth가 바뀌면 그 프레임에선 빌보드가 이상하게 보인다.
					//다시 Matrix를 계산하도록 만든다.
					curBuff._optTransform.CorrectBillboardMatrix();
				}

				//다음에 다시 Depth Changed가 되었을 때 적용이 되도록 플래그 On
				_isNeedToApplyDepthChangedBuffers = true;
			}
		}

		// Get / Set
		//----------------------------------------------------------
	}


}