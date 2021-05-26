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
	/// apRenderUnit을 정렬할 때 사용되는 버퍼.
	/// 1차적으로 정렬된 리스트를 입력하고, "Depth 변경"이벤트를 입력하면 적당히 정렬된 리스트를 제공한다.
	/// </summary>
	public class apSortedRenderBuffer
	{
		// Members
		//--------------------------------------------------

		//"정렬된" RenderUnit을 받으면, Unit은 1차적으로 동일한 순서로 생성되어 리스트에 정리됨과 동시에
		//나중에 Depth Swap에 사용될 "GroupSize"를 계산한다.
		//GroupSize만 있다면 순서 정렬이 쉽다.
		public class BufferData
		{
			public int _indexOriginal = 0;
			public int _indexChanged = 0;
			public int _level = 0;
			public int _groupSize = 0;
			public apRenderUnit _renderUnit = null;
			public bool _isMesh = false;
			public bool _isClippedChild = false;
			public bool _isClippingParent = false;
			public BufferData _parent = null;//상위 MeshGroup
			
			public BufferData(int index, apRenderUnit renderUnit)
			{
				_indexOriginal = index;
				_indexChanged = index;
				_renderUnit = renderUnit;
				_level = renderUnit._level;

				_isMesh = (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh);
				_isClippedChild = false;
				_isClippingParent = false;

				if (_isMesh)
				{
					_isClippedChild = _renderUnit._meshTransform._isClipping_Child;
					_isClippingParent = _renderUnit._meshTransform._isClipping_Parent;
				}
				_parent = null;
			}
			
		}

		private Dictionary<apRenderUnit, BufferData> _renderUnit2Buff = new Dictionary<apRenderUnit, BufferData>();
		private int _nRenderUnits = 0;
		private BufferData[] _buffers = null;
		private BufferData[] _buffers_DepthChanged = null;
		
		private bool _isDepthChanged = false;
		private bool _isNeedToSortDepthChangedBuffers = true;
		private int _nDepthChangedRequest = 0;
		private Dictionary<apRenderUnit, int> _depthChangedRequests = new Dictionary<apRenderUnit, int>();
		private Dictionary<apRenderUnit, int> _depthChangedCache = new Dictionary<apRenderUnit, int>();
		
		//참조를 위한 RednerUnit 리스트 변수
		//처음 설정과 정렬시 갱신된다.
		private List<apRenderUnit> _renderUnits_Original = new List<apRenderUnit>();
		private List<apRenderUnit> _renderUnits_Sorted = new List<apRenderUnit>();


		// Init
		//--------------------------------------------------
		public apSortedRenderBuffer()
		{
			Init();
		}

		public void Init()
		{
			_renderUnit2Buff.Clear();
			_nRenderUnits = 0;

			_buffers = null;
			_buffers_DepthChanged = null;
			

			_isDepthChanged = false;
			_isNeedToSortDepthChangedBuffers = true;//<<캐시가 초기화되었으므로 일단 무조건 다시 Sort해야한다.
			_nDepthChangedRequest = 0;
			_depthChangedRequests.Clear();
			_depthChangedCache.Clear();

			_renderUnits_Original.Clear();
			_renderUnits_Sorted.Clear();
		}


		// Functions
		//--------------------------------------------------
		/// <summary>
		/// MeshGroup의 _renderUnits_All이 생성되어 정렬된 직후 이 함수를 호출해야한다.
		/// "기본적인 RenderUnit 순서" 데이터를 생성한다.
		/// 이때 "RenderUnit의 그룹 크기"를 미리 생성하기 때문에 꼭 필요하다.
		/// </summary>
		/// <param name="renderUnits"></param>
		public void SetSortedRenderUnits(List<apRenderUnit> renderUnits)
		{
			Init();

			apRenderUnit renderUnit = null;
			_nRenderUnits = renderUnits.Count;
			if(_nRenderUnits == 0)
			{
				//렌더링할 게 없는데용..
				return;
			}
			
			//버퍼를 생성한다.
			_isNeedToSortDepthChangedBuffers = true;
			_buffers = new BufferData[_nRenderUnits];
			_buffers_DepthChanged = new BufferData[_nRenderUnits];
			

			for (int i = 0; i < renderUnits.Count; i++)
			{
				renderUnit = renderUnits[i];

				//일단 순서대로 버퍼에 넣는다.
				//RenderUnit -> Buffer를 참조하기 위한 매핑 리스트에도 추가
				BufferData newBuff = new BufferData(i, renderUnit);
				_buffers[i] = newBuff;
				_renderUnit2Buff.Add(renderUnit, newBuff);

				//리스트에도 넣는다.
				_renderUnits_Original.Add(renderUnit);
			}

			
			//Buffer Unit을 앞에서부터 돌면서 "Group Size"와 "Parent"를 계산한다.
			BufferData curBuf = null;
			for (int i = 0; i < _buffers.Length; i++)
			{
				curBuf = _buffers[i];

				//Parent 먼저 연결한다.
				if (curBuf._renderUnit._parentRenderUnit != null)
				{
					if (_renderUnit2Buff.ContainsKey(curBuf._renderUnit._parentRenderUnit))
					{
						BufferData parentBuf = _renderUnit2Buff[curBuf._renderUnit._parentRenderUnit];
						curBuf._parent = parentBuf;
					}
				}

				curBuf._groupSize = 1;//일단 자기 자신 포함
									  //GroupSize

				if (curBuf._renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh
					&& curBuf._renderUnit._meshTransform != null)
				{
					//1. Mesh인 경우 : Clipping Parent일 때 -> Clipped Child 만큼이 GroupSize
					if (curBuf._renderUnit._meshTransform._isClipping_Parent)
					{
						//Clipped되는 메시 개수만큼 추가
						curBuf._groupSize += curBuf._renderUnit._meshTransform._clipChildMeshes.Count;
					}
				}
				else if (curBuf._renderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode
					&& curBuf._renderUnit._meshGroupTransform != null)
				{
					//2. MeshGroup인 경우 : 자식들 모두 확인
					//다음 리스트를 확인하면서
					//"자식들(더 큰 Level)을 카운트"한다.
					//자신과 동일/상위 레벨 (같거나 작은 Level)인 경우 카운트를 중단한다.
					int curIndex = curBuf._indexOriginal + 1;
					BufferData nextBuff = null;

					while (true)
					{
						if (curIndex >= _buffers.Length)
						{
							break;
						}
						nextBuff = _buffers[curIndex];
						if (nextBuff._level > curBuf._level)
						{
							//자식이면 카운트
							curBuf._groupSize++;
						}
						else
						{
							//형제거나 부모라면 카운트 중지
							break;
						}

						curIndex++;
					}
				}


			}
		}


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
		/// <param name="renderUnit"></param>
		/// <param name="deltaDepth"></param>
		public void OnExtraDepthChanged(apRenderUnit renderUnit, int deltaDepth)
		{
			if(deltaDepth == 0)
			{
				return;
			}
			if(renderUnit._meshTransform != null && renderUnit._meshTransform._isClipping_Child)
			{
				//Clipping Child의 Depth 이동은 허용하지 않는다.
				return;
			}

			//Debug.Log("OnExtraDepthChanged [" + renderUnit.Name + " - "+ deltaDepth + "]");

			//일단 DepthChanged 이벤트가 발생했음을 알리고
			_isDepthChanged = true;
			_nDepthChangedRequest++;

			//어떤 RenderUnit의 Depth가 바뀌었는지 저장한다.
			if(_depthChangedRequests.ContainsKey(renderUnit))
			{
				//키가 있을리가 없는데..
				_depthChangedRequests[renderUnit] = deltaDepth;
			}
			else
			{
				_depthChangedRequests.Add(renderUnit, deltaDepth);
			}

			//캐시 미스 여부를 찾는다.
			if(!_depthChangedCache.ContainsKey(renderUnit))
			{
				//만약 캐시에 없는 거라면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				//Debug.LogError(">> Cache Miss (New Data)");
			}
			else if(_depthChangedCache[renderUnit] != deltaDepth)
			{
				//만약 캐시와 값이 다르다면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				//Debug.LogError(">> Cache Miss (Delta Changed)");
			}
		}
		

		/// <summary>
		/// 모든 RenderUnit 업데이트가 끝나고, Depth 이벤트에 따라 출력 순서를 바꾸어야 하는지 확인한다.
		/// </summary>
		public void UpdateDepthChangedEventAndBuffers()
		{
			if(_nRenderUnits == 0)
			{
				//렌더 유닛이 없당..
				return;
			}
			if(!_isDepthChanged || _nDepthChangedRequest == 0)
			{
				//Depth가 바뀐 적이 없다.
				return;
			}

			//Debug.LogWarning("Extra Depth 변경됨 : " + _nDepthChangedRequest + "개의 요청");
			//foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			//{
			//	Debug.LogWarning("[" + request.Key.Name + "] Depth : " + request.Value);
			//}

			if(!_isNeedToSortDepthChangedBuffers)
			{
				//Depth 변경 캐시가 모두 히트했을 때,
				//개수까지 같아야 인정.
				if(_depthChangedRequests.Count != _depthChangedCache.Count)
				{
					_isNeedToSortDepthChangedBuffers = true;
				}
			}


			if(!_isNeedToSortDepthChangedBuffers)
			{
				//재정렬을 할 필요가 없다. 예쓰!
				//Debug.Log("ReSort -> Cache Hit");
				return;
			}

			//Debug.LogError("Cache Miss : 다시 정렬을 해야한다. [" + _nDepthChangedRequest + "개의 요청]");
			
			
			//재정렬을 해야한다.
			//흐아앙
			
			//_buffers_DepthChanged.Clear();

			//일단 재정렬을 할 예정이니 캐시는 현재 값으로 갱신
			_isNeedToSortDepthChangedBuffers = false;
			_depthChangedCache.Clear();
			foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			{
				_depthChangedCache.Add(request.Key, request.Value);
			}

			//이제 다시 정렬을 해보자
			//- 인덱스 스왑만 먼저 한다.
			//- 버퍼 정렬을 하려면.. Array가 필요하당..

			

			//먼저 Buffer_DepthChanged를 복사하여 붙여넣는다.
			//이때, Index_Changed는 원래대로 돌려놓는다.
			BufferData curBufferData = null;
			for (int i = 0; i < _nRenderUnits; i++)
			{
				curBufferData = _buffers[i];
				curBufferData._indexChanged = curBufferData._indexOriginal;
				_buffers_DepthChanged[i] = curBufferData;
			}


			apRenderUnit curRenderUnit = null;
			
			//Debug.Log("------------------------------");
			//DebugBuffers(_buffers, "원래 순서", false);

			int deltaDepth = 0;
			foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			{
				curRenderUnit = request.Key;
				curBufferData = _renderUnit2Buff[curRenderUnit];
				deltaDepth = request.Value;
				if(deltaDepth == 0)
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

				BufferData nextBuff = null;
				while(true)
				{
					
					if(iCheck < 0 || iCheck >= _nRenderUnits)
					{
						//렌더 유닛 범위를 넘어갔다면
						break;
					}
					if(depthCount >= maxDepthCount)
					{
						//Depth 카운트를 모두 셌다면
						break;
					}

					nextBuff = _buffers_DepthChanged[iCheck];
					
					//- 자신보다 Level이 높은 경우(하위인 경우) : 카운트하지 않고 이동한다.
					//- 자신보다 Level이 같고 같은 Parent를 공유하는 경우 : 카운트 1개 하고 이동한다. 카운트 찬 경우 종료
					//- 자신보다 Level이 낮은 경우(상위인 경우) 또는 Level이 같아도 Parent를 공유하지 않는 경우(에러) : 이동 종료
					//- 만약 이동 도중 ClippingChild를 만나면 : 카운트하지 않고 이동한다.
					if(nextBuff._level > curBufferData._level)
					{
						//Level이 높거나(하위 레벨)이라면 패스
						realMovedOffset += moveDir;
						iCheck += moveDir;
						//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Next가 하위 레벨이다. (" + nextBuff._level + " > " + curBufferData._level + ")");
					}
					else if(nextBuff._level == curBufferData._level)
					{
						if(nextBuff._isClippedChild)
						{
							//같은 레벨의 ClippedChild라면 패스
							realMovedOffset += moveDir;
							iCheck += moveDir;

							//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Clipped Child이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
						}
						else if(nextBuff._parent != curBufferData._parent)
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

				if(realMovedOffset == 0)
				{
					continue;
				}

				//Debug.Log("- 이동 범위 : " + realMovedOffset + " (Depth : " + deltaDepth + ")");

				//"이동할 간격"이 결정되면, 그 영역만큼 Index를 바꾸자
				int swappedIndex_Start = 0;
				int swappedIndex_End = 0;
				int nSwapped = Mathf.Abs(realMovedOffset);



				if(deltaDepth > 0)
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
				BufferData movedBuf = null;

				//일단 배열은 초기화
				for (int i = 0; i < _nRenderUnits; i++)
				{
					_buffers_DepthChanged[i] = null;
				}


				for (int i = 0; i < _nRenderUnits; i++)
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
			_renderUnits_Sorted.Clear();

			BufferData sortedBufferData = null;
			for (int i = 0; i < _buffers_DepthChanged.Length; i++)
			{
				sortedBufferData = _buffers_DepthChanged[i];
				if (sortedBufferData != null && sortedBufferData._renderUnit != null)
				{
					_renderUnits_Sorted.Add(sortedBufferData._renderUnit);
				}
				else
				{
					//Debug.LogError("Sort 에러 : Null값이 발생했다.");
				}
			}
		}



		private void DebugBuffers(BufferData[] buffers, string label, bool isImportant)
		{
			string strText = "[ " + label + " ]\n";
			BufferData curBuff = null;
			for (int i = buffers.Length - 1; i >= 0; i--)
			{
				curBuff = buffers[i];
				if(curBuff == null)
				{
					strText += "< Null > \n";
				}
				else
				{
					if(curBuff._renderUnit._level == 1) { strText += "- "; }
					else if(curBuff._renderUnit._level == 2) { strText += "-- "; }
					else if(curBuff._renderUnit._level == 3) { strText += "--- "; }
					else if(curBuff._renderUnit._level == 4) { strText += "---- "; }
					else { strText += "---- "; }

					strText += curBuff._renderUnit.Name + "  (" + curBuff._indexOriginal + " > " + curBuff._indexChanged + " | Group : " + curBuff._groupSize + ")\n";
				}
				
			}
			if(isImportant)
			{
				Debug.LogWarning(strText);
				
			}
			else
			{
				Debug.Log(strText);
			}
			
		}


		// Iteration
		//--------------------------------------------------------------------------
		

		// Get / Set
		//--------------------------------------------------
		public bool IsDepthChanged
		{
			get { return _isDepthChanged; }
		}

		/// <summary>
		/// RenderUnit 리스트를 가져온다. Depth가 바뀌었다면 다시 정렬된 리스트가 리턴된다.
		/// </summary>
		public List<apRenderUnit> SortedRenderUnits
		{
			get
			{
				if(_isDepthChanged)
				{
					return _renderUnits_Sorted;
				}
				else
				{
					return _renderUnits_Original;
				}
			}
		}

		public BufferData GetBufferData(apRenderUnit renderUnit)
		{
			for (int i = 0; i < _buffers.Length; i++)
			{
				if(_buffers[i]._renderUnit == renderUnit)
				{
					return _buffers[i];
				}
			}
			return null;
		}
	}
}