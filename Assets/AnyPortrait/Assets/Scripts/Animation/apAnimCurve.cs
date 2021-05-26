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
	/// 애니메이션 및 컨트롤러 보간에서 사용하는 보간용 커브
	/// 값 자체는 커브에 해당하는 한쪽의 키값을 저장한다.
	/// Static 함수나 멤버 함수로 "다른 키와의 보간값"을 리턴해준다.
	/// </summary>
	[Serializable]
	public class apAnimCurve
	{
		// Members
		//-------------------------------------------------
		/// <summary>
		/// 한쪽의 사이드에 대한 보간 탄젠트 타입
		/// </summary>
		public enum TANGENT_TYPE
		{
			/// <summary>A->B가 직선으로 이어지며 컨트롤 포인트가 사라진다.</summary>
			Linear = 0,
			/// <summary>A->B에 컨트롤 포인트가 포함된다. 커브 형태로 생성된다.</summary>
			Smooth = 1,
			/// <summary>A.... > B 방식으로 A가 유지되다가 B로 갑자기 바뀐다. 한쪽이 Constant면 무조건 Constant가 된다.</summary>
			Constant = 2
		}

		//Curve 자체에
		//  <-   ㅁ   -> 양쪽에 대한 값을 가진다.
		//(Prev) ㅁ (Next)

		//컨트롤 포인트의 값
		//X ( 연결된 다른 Curve 까지의 차이에 대한 비율 (0~1). 기본값 0.3 )
		//Y ( 기본값은 0. 0일수록 해당 ITP의 결과에 가깝게 되고, 1이면 연결된 다른 Curve Point 


		[SerializeField]
		public TANGENT_TYPE _prevTangentType = TANGENT_TYPE.Smooth;

		public float _prevSmoothX = 0.3f;
		public float _prevSmoothY = 0.0f;

		[NonSerialized]
		public apAnimCurve _prevLinkedCurveKey = null;

		//[NonSerialized]
		//public bool _isPrevKeyUseDummyIndex = false;//<Prev CurveKey가 더미인덱스를 사용하는가


		[SerializeField]
		public TANGENT_TYPE _nextTangentType = TANGENT_TYPE.Smooth;

		public float _nextSmoothX = 0.3f;
		public float _nextSmoothY = 0.0f;



		#region [미사용 코드]
		//public Vector4 _nextSmoothAngle = Vector4.zero;
		//public Vector4 _nextSmoothYOffset = Vector4.zero;
		//public float _nextDeltaX = 1.0f;
		//public Vector4 _nextDeltaY = Vector4.zero; 
		#endregion

		[NonSerialized]
		public apAnimCurve _nextLinkedCurveKey = null;

		#region [미사용 코드]
		//[NonSerialized]
		//public bool _isNextKeyUseDummyIndex = false;

		//[SerializeField]
		//public KEY_ITP_TYPE _keyItpType = KEY_ITP_TYPE.AutoSmooth; 
		#endregion

		[SerializeField]
		public int _keyIndex = 0;

		//[SerializeField]
		//public float _dummyKeyIndex = 0.0f;

		//변경
		//기존의 인덱스를 [기본] / [더미]로 쓰던걸
		//그냥 Prev / Index / Next로 바꾸고
		//그 값을 바로 쓸 수 있도록 외부에서 설정해서 가져올 것
		[SerializeField]
		public int _prevIndex = 0;

		[SerializeField]
		public int _nextIndex = 0;


		#region [미사용 코드]
		//[SerializeField]
		//public Vector4 _keyValue = Vector4.zero;

		//[SerializeField]
		//public int _dimension = 1;

		//추가
		//Morph같이 다중 키값이 들어오면 보간시 "데이터 값"을 정의할 수 없다.
		//public bool _isRelativeKeyValue = false;

		//이 경우는 Smooth 값은 별도로 가진다. (Weight 형식으로 가지며 0 ~ 0.2의 값을 가진다.)
		//여기선 Smooth Weight만 처리한다.
		//public float _smoothRelativeWeight = 0.0f;


		//[SerializeField]
		//private float _loopSize = 0.0f;


		#endregion


		//public const float CONTROL_POINT_X_OFFSET = 0.3f;
		//public const float CONTROL_POINT_X_OFFSET = 1.0f;//꽤 부드러운 커브를 기본값으로 한다.
		public const float CONTROL_POINT_X_OFFSET = 0.5f;//1로 하니 너무 빡세게 부드럽다. 절충하자
		
		public const float MIN_DELTA_X = 0.0001f;
		//public const float MIN_ANGLE = -89.5f;
		//public const float MAX_ANGLE = 89.5f;

		public const float MIN_RELATIVE_WEIGHT = 0.0f;
		public const float MAX_RELATIVE_WEIGHT = 0.2f;


		public enum KEY_POS
		{
			PREV = 0, NEXT = 1
		}

		//변경 19.5.22 : 용량 최적화를 위해서 AnimCurveResult를 NonSerialized로 변경
		//[SerializeField]
		[NonSerialized, NonBackupField]
		public apAnimCurveResult _prevCurveResult = new apAnimCurveResult();

		//[SerializeField]
		[NonSerialized, NonBackupField]
		public apAnimCurveResult _nextCurveResult = new apAnimCurveResult();

		//[NonSerialized]
		//private apAnimCurveTable _prevTable = null;

		//[NonSerialized]
		//private apAnimCurveTable _nextTable = null;

		

		// Init
		//-------------------------------------------------
		public apAnimCurve()
		{
			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			Init();
		}


		public apAnimCurve(apAnimCurve srcCurve, int keyIndex)
		{
			_prevTangentType = srcCurve._prevTangentType;

			_prevSmoothX = srcCurve._prevSmoothX;
			_prevSmoothY = srcCurve._prevSmoothY;

			#region [미사용 코드]
			//_prevSmoothAngle = srcCurve._prevSmoothAngle;
			//_prevSmoothYOffset = srcCurve._prevSmoothYOffset;
			//_prevDeltaX = srcCurve._prevDeltaX;
			//_prevDeltaY = srcCurve._prevDeltaY; 
			#endregion

			_prevLinkedCurveKey = srcCurve._prevLinkedCurveKey;
			_nextTangentType = srcCurve._nextTangentType;

			_nextSmoothX = srcCurve._nextSmoothX;
			_nextSmoothY = srcCurve._nextSmoothY;

			#region [미사용 코드]
			//_nextSmoothAngle = srcCurve._nextSmoothAngle;
			//_nextSmoothYOffset = srcCurve._nextSmoothYOffset;
			//_nextDeltaX = srcCurve._nextDeltaX;
			//_nextDeltaY = srcCurve._nextDeltaY; 
			#endregion

			_nextLinkedCurveKey = srcCurve._nextLinkedCurveKey;
			//_keyItpType = srcCurve._keyItpType;

			_keyIndex = keyIndex;//<키 인덱스만 따로 분리해서 복사한다.

			_prevIndex = keyIndex;
			_nextIndex = keyIndex;

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			#region [미사용 코드]
			//_keyValue = srcCurve._keyValue;

			//_dimension = srcCurve._dimension;
			//_isRelativeKeyValue = srcCurve._isRelativeKeyValue;

			//_smoothRelativeWeight = srcCurve._smoothRelativeWeight; 
			#endregion
		}


		public apAnimCurve(apAnimCurve srcCurve_Prev, apAnimCurve srcCurve_Next, int keyIndex)
		{
			//Prev는 Next의 Prev를 사용한다.
			_prevTangentType = srcCurve_Next._prevTangentType;

			_prevSmoothX = srcCurve_Next._prevSmoothX;
			_prevSmoothY = srcCurve_Next._prevSmoothY;

			

			_prevLinkedCurveKey = srcCurve_Next._prevLinkedCurveKey;

			//Next는 Prev의 Next를 사용한다.
			_nextTangentType = srcCurve_Prev._nextTangentType;

			_nextSmoothX = srcCurve_Prev._nextSmoothX;
			_nextSmoothY = srcCurve_Prev._nextSmoothY;

			

			_nextLinkedCurveKey = srcCurve_Prev._nextLinkedCurveKey;
			//_keyItpType = srcCurve._keyItpType;

			_keyIndex = keyIndex;//<키 인덱스만 따로 분리해서 복사한다.

			_prevIndex = keyIndex;
			_nextIndex = keyIndex;

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			
		}

		public void Init()
		{

			//_keyItpType = KEY_ITP_TYPE.AutoSmooth;
			_prevTangentType = TANGENT_TYPE.Smooth;
			_nextTangentType = TANGENT_TYPE.Smooth;

			_prevSmoothX = CONTROL_POINT_X_OFFSET;
			_prevSmoothY = 0.0f;

			_nextSmoothX = CONTROL_POINT_X_OFFSET;
			_nextSmoothY = 0.0f;

			#region [미사용 코드]
			//SetSmoothAngle(Vector4.zero, KEY_POS.PREV);
			//SetSmoothAngle(Vector4.zero, KEY_POS.NEXT); 
			#endregion

			_prevLinkedCurveKey = null;
			_nextLinkedCurveKey = null;

			#region [미사용 코드]
			//_prevDeltaX = 1.0f;
			//_prevDeltaY = Vector4.zero;

			//_nextDeltaX = 1.0f;
			//_nextDeltaY = Vector4.zero;

			//_isRelativeKeyValue = false;
			//_smoothRelativeWeight = MIN_RELATIVE_WEIGHT; 
			#endregion

			//_prevTable = new apAnimCurveTable(this);
			//_nextTable = new apAnimCurveTable(this);

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}

			
		}



		// Functions
		//-------------------------------------------------

		// Set Key Value (값 타입에 맞게 오버로드)
		//-------------------------------------------------------------------------------
		#region [미사용 코드]
		//public void SetLoopSize(int loopSize)
		//{
		//	_loopSize = loopSize;
		//} 
		#endregion

		/// <summary>
		/// 이 커브 정보가 입력될 위치값을 지정한다(키프레임)
		/// </summary>
		/// <param name="keyIndex"></param>
		public void SetKeyIndex(int keyIndex)
		{
			_keyIndex = keyIndex;
			//_dummyKeyIndex = dummyKeyIndex;

			Refresh();
		}

		/// <summary>
		/// Curve 정보가 바뀌면 항상 호출해야하는 함수
		/// 보간 처리에 필요한 Table을 갱신한다.
		/// </summary>
		public void Refresh()
		{
			//_prevTable.MakeTable();
			//_nextTable.MakeTable();
			_prevCurveResult.MakeCurve();
			_nextCurveResult.MakeCurve();
		}


		#region [미사용 코드]
		//public void SetKeyValue(float keyValue)
		//{
		//	_keyValue = new Vector4(keyValue, 0.0f, 0.0f, 0.0f);
		//	_dimension = 1;

		//	_isRelativeKeyValue = false;
		//}
		//public void SetKeyValue(Vector2 keyValue)
		//{
		//	_keyValue = new Vector4(keyValue.x, keyValue.y, 0.0f, 0.0f);
		//	_dimension = 2;

		//	_isRelativeKeyValue = false;
		//}
		//public void SetKeyValue(Vector3 keyValue)
		//{
		//	_keyValue = new Vector4(keyValue.x, keyValue.y, keyValue.z, 0.0f);
		//	_dimension = 3;

		//	_isRelativeKeyValue = false;
		//}
		//public void SetKeyValue(Vector4 keyValue)
		//{
		//	_keyValue = keyValue;
		//	_dimension = 4;

		//	_isRelativeKeyValue = false;
		//}
		//public void SetKeyValue(Color keyValue)
		//{
		//	_keyValue = new Vector4(keyValue.r, keyValue.g, keyValue.b, keyValue.a);
		//	_dimension = 5;

		//	_isRelativeKeyValue = false;
		//}


		//public void SetKeyRelative()
		//{
		//	_isRelativeKeyValue = true;
		//}


		//public float GetIndex(bool isDummy)
		//{
		//	if(isDummy)
		//	{
		//		return _dummyKeyIndex;
		//	}
		//	else
		//	{
		//		return _keyIndex;
		//	}
		//} 
		#endregion

		//-------------------------------------------------------------------------------
		public void SetLinkedIndex(int prevIndex, int nextIndex)
		{
			_prevIndex = prevIndex;
			_nextIndex = nextIndex;

			Refresh();
		}

		public void SetLinkedCurveKey(apAnimCurve prevLinkedCurveKey, apAnimCurve nextLinkedCurveKey,
										int prevIndex, int nextIndex
			
										//bool isMakeCurveForce = false//<< 삭제 19.5.20 : 이게 문제가 아닌데??
									//, bool isPrevDummyIndex, bool isNextDummyIndex
									)
		{
			_prevLinkedCurveKey = prevLinkedCurveKey;
			_nextLinkedCurveKey = nextLinkedCurveKey;

			_prevIndex = prevIndex;
			_nextIndex = nextIndex;

			#region [미사용 코드]
			//_isPrevKeyUseDummyIndex = isPrevDummyIndex;
			//_isNextKeyUseDummyIndex = isNextDummyIndex;

			//Delta값은 항상 B-A 방식으로 한다.

			//float prevKeyIndex = _keyIndex;
			//float nextKeyIndex = _keyIndex;

			//if (_prevLinkedCurveKey == null)
			//{
			//	_prevDeltaX = 1.0f;
			//	_prevDeltaY = Vector4.zero;
			//}
			//else
			//{
			//	//prevKeyIndex = _prevLinkedCurveKey.GetIndex(_isPrevKeyUseDummyIndex);

			//	_prevDeltaX = (_keyIndex - _prevIndex);
			//	_prevDeltaY = (_keyValue - _prevLinkedCurveKey._keyValue);
			//}


			//if(_nextLinkedCurveKey == null)
			//{
			//	_nextDeltaX = 1.0f;
			//	_nextDeltaY = Vector4.zero;
			//}
			//else
			//{
			//	//nextKeyIndex = _nextLinkedCurveKey.GetIndex(_isNextKeyUseDummyIndex);

			//	//_nextDeltaX = (nextKeyIndex - _keyIndex);
			//	_nextDeltaX = (_nextIndex - _keyIndex);
			//	_nextDeltaY = (_nextLinkedCurveKey._keyValue - _keyValue);
			//}

			//_prevDeltaX = Mathf.Max(_prevDeltaX, MIN_DELTA_X);
			//_nextDeltaX = Mathf.Max(_nextDeltaX, MIN_DELTA_X);

			//각도를 가지고 YOffset을 다시 계산하자
			//CalculateSmooth(); 
			#endregion

			//이전
			//_prevCurveResult.Link(prevLinkedCurveKey, this, false, !(Application.isPlaying) || isMakeCurveForce);
			//_nextCurveResult.Link(this, nextLinkedCurveKey, true, !(Application.isPlaying) || isMakeCurveForce);

			//변경 : 19.5.20 : 최적화 작업
			_prevCurveResult.Link(prevLinkedCurveKey, this, false);
			_nextCurveResult.Link(this, nextLinkedCurveKey, true);

			
		}



		#region [미사용 코드]
		//public void SetKeyItpType(KEY_ITP_TYPE keyItpType)
		//{	
		//	switch (keyItpType)
		//	{
		//		case KEY_ITP_TYPE.AutoSmooth:
		//			//여기서는 값을 처리하지 않는다.
		//			//Flat 처럼 값이 들어가지만 따로 계산하여 사용
		//			{
		//				if(_keyItpType != KEY_ITP_TYPE.AutoSmooth)
		//				{
		//					//바뀔때는 기본적으로 Flat을 기본으로 하며, 실시간으로 비슷하게 세팅
		//					SetSmoothAngle(Vector4.zero, KEY_POS.PREV);
		//					SetSmoothAngle(Vector4.zero, KEY_POS.NEXT);

		//					CalculateSmooth();
		//				}
		//			}
		//			break;

		//		case KEY_ITP_TYPE.FreeSmooth:
		//			//기존 값을 활용/절충한다.
		//			{
		//				if (_keyItpType != KEY_ITP_TYPE.FreeSmooth)
		//				{
		//					Vector4 avgAngle = (_prevSmoothAngle + _nextSmoothAngle) * 0.5f;
		//					SetSmoothAngle(avgAngle, KEY_POS.PREV);
		//					SetSmoothAngle(avgAngle, KEY_POS.NEXT);
		//				}
		//			}
		//			break;

		//		case KEY_ITP_TYPE.FlatSmooth:
		//			{
		//				//언제나 0이다.
		//				SetSmoothAngle(Vector4.zero, KEY_POS.PREV);
		//				SetSmoothAngle(Vector4.zero, KEY_POS.NEXT);
		//			}
		//			break;

		//		case KEY_ITP_TYPE.Broken:
		//			{
		//				if(_keyItpType != KEY_ITP_TYPE.Broken)
		//				{
		//					//Broken이 아니었다면
		//					//양쪽 Tangent는 모두 Smooth였을 것이다.
		//					_prevTangentType = TANGENT_TYPE.Smooth;
		//					_nextTangentType = TANGENT_TYPE.Smooth;
		//				}
		//			}
		//			break;
		//	}
		//	_keyItpType = keyItpType;
		//}




		//public void SetSmoothAngle(Vector4 angle, KEY_POS keyPos)
		//{
		//	if (keyPos == KEY_POS.PREV)
		//	{
		//		//Prev
		//		_prevSmoothAngle.x = Mathf.Clamp(angle.x, MIN_ANGLE, MAX_ANGLE);
		//		if (_dimension > 1) { _prevSmoothAngle.y = Mathf.Clamp(angle.y, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 2) { _prevSmoothAngle.z = Mathf.Clamp(angle.z, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 3) { _prevSmoothAngle.w = Mathf.Clamp(angle.w, MIN_ANGLE, MAX_ANGLE); }

		//		_prevSmoothYOffset.x = -1.0f * Mathf.Tan(_prevSmoothAngle.x * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _prevDeltaX;
		//		if (_dimension > 1) { _prevSmoothYOffset.y = -1.0f * Mathf.Tan(_prevSmoothAngle.y * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _prevDeltaX; }
		//		if (_dimension > 2) { _prevSmoothYOffset.z = -1.0f * Mathf.Tan(_prevSmoothAngle.z * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _prevDeltaX; }
		//		if (_dimension > 3) { _prevSmoothYOffset.w = -1.0f * Mathf.Tan(_prevSmoothAngle.w * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _prevDeltaX; }
		//	}
		//	else
		//	{
		//		//Next
		//		_nextSmoothAngle.x = Mathf.Clamp(angle.x, MIN_ANGLE, MAX_ANGLE);
		//		if (_dimension > 1) { _nextSmoothAngle.y = Mathf.Clamp(angle.y, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 2) { _nextSmoothAngle.z = Mathf.Clamp(angle.z, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 3) { _nextSmoothAngle.w = Mathf.Clamp(angle.w, MIN_ANGLE, MAX_ANGLE); }

		//		_nextSmoothYOffset.x = Mathf.Tan(_nextSmoothAngle.x * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _nextDeltaX;
		//		if (_dimension > 1) { _nextSmoothYOffset.y = Mathf.Tan(_nextSmoothAngle.y * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _nextDeltaX; }
		//		if (_dimension > 2) { _nextSmoothYOffset.z = Mathf.Tan(_nextSmoothAngle.z * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _nextDeltaX; }
		//		if (_dimension > 3) { _nextSmoothYOffset.w = Mathf.Tan(_nextSmoothAngle.w * Mathf.Deg2Rad) * CONTROL_POINT_X_OFFSET * _nextDeltaX; }
		//	}
		//}

		//public void SetSmoothYOffset(Vector4 yOffset, KEY_POS keyPos)
		//{
		//	if(keyPos == KEY_POS.PREV)
		//	{
		//		//Prev
		//		float deltaX = Mathf.Max(_prevDeltaX, 0.0001f);
		//		_prevSmoothYOffset = yOffset;
		//		_prevSmoothAngle.x = Mathf.Clamp(Mathf.Atan2(-_prevSmoothYOffset.x, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE);
		//		if (_dimension > 1) { _prevSmoothAngle.y = Mathf.Clamp(Mathf.Atan2(-_prevSmoothYOffset.y, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 2) { _prevSmoothAngle.z = Mathf.Clamp(Mathf.Atan2(-_prevSmoothYOffset.z, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 3) { _prevSmoothAngle.w = Mathf.Clamp(Mathf.Atan2(-_prevSmoothYOffset.w, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//	}
		//	else
		//	{
		//		//Next
		//		float deltaX = Mathf.Max(_nextDeltaX, 0.0001f);
		//		_nextSmoothYOffset = yOffset;
		//		_nextSmoothAngle.x = Mathf.Clamp(Mathf.Atan2(_nextSmoothYOffset.x, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE);
		//		if (_dimension > 1) { _nextSmoothAngle.y = Mathf.Clamp(Mathf.Atan2(_nextSmoothYOffset.y, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 2) { _nextSmoothAngle.z = Mathf.Clamp(Mathf.Atan2(_nextSmoothYOffset.z, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		if (_dimension > 3) { _nextSmoothAngle.w = Mathf.Clamp(Mathf.Atan2(_nextSmoothYOffset.w, CONTROL_POINT_X_OFFSET * deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//	}
		//} 
		#endregion


		public void SetTangentType(TANGENT_TYPE tangentType, KEY_POS keyPos)
		{
			#region [미사용 코드]
			//if(_isRelativeKeyValue) { return; }

			//if(_keyItpType != KEY_ITP_TYPE.Broken)
			//{
			//	//Broken 타입이 아니면 무조건 Smooth 타입이다.
			//	_prevTangentType = TANGENT_TYPE.Smooth;
			//	_nextTangentType = TANGENT_TYPE.Smooth;
			//	return;
			//} 
			#endregion


			if (keyPos == KEY_POS.PREV)
			{
				//Prev
				_prevTangentType = tangentType;
			}
			else
			{
				//Next
				_nextTangentType = tangentType;
			}

			Refresh();
		}

		#region [미사용 코드]
		//public void CalculateSmooth()
		//{
		//	//TODO : 여기서 Loop인 경우, KeyIndex가 Length만큼 돌아서 처리되는 걸 막아야한다.

		//	if(_isRelativeKeyValue)
		//	{
		//		_smoothRelativeWeight = Mathf.Clamp(_smoothRelativeWeight, MIN_RELATIVE_WEIGHT, MAX_RELATIVE_WEIGHT);
		//		return;
		//	}

		//	float curKeyframeIndex = _keyIndex;
		//	//float prevKeyframeIndex = _keyIndex;
		//	//float nextKeyframeIndex = _keyIndex;

		//	if(_prevLinkedCurveKey != null)
		//	{
		//		//prevKeyframeIndex = _prevLinkedCurveKey.GetIndex(_isPrevKeyUseDummyIndex);
		//	}

		//	if(_nextLinkedCurveKey != null)
		//	{
		//		//nextKeyframeIndex = _nextLinkedCurveKey.GetIndex(_isNextKeyUseDummyIndex);
		//	}


		//	if(_keyItpType == KEY_ITP_TYPE.AutoSmooth)
		//	{
		//		//AutoSmooth라면 주위 값을 비교하여 계산해야한다.
		//		Vector4 resultAngle = Vector4.zero;
		//		if(_prevLinkedCurveKey != null && _nextLinkedCurveKey != null)
		//		{
		//			//float deltaX = Mathf.Max(_nextLinkedCurveKey._keyIndex - _prevLinkedCurveKey._keyIndex, MIN_DELTA_X);

		//			//float deltaX = Mathf.Max(nextKeyframeIndex - prevKeyframeIndex, MIN_DELTA_X);
		//			float deltaX = Mathf.Max(_nextIndex - _prevIndex, MIN_DELTA_X);

		//			Vector4 deltaY = _nextLinkedCurveKey._keyValue - _prevLinkedCurveKey._keyValue;

		//			resultAngle.x = Mathf.Clamp(Mathf.Atan2(deltaY.x, deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE);
		//			if (_dimension > 1) { resultAngle.y = Mathf.Clamp(Mathf.Atan2(deltaY.y, deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 2) { resultAngle.z = Mathf.Clamp(Mathf.Atan2(deltaY.z, deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 3) { resultAngle.w = Mathf.Clamp(Mathf.Atan2(deltaY.w, deltaX) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		}
		//		else if(_prevLinkedCurveKey != null)
		//		{
		//			float deltaX = Mathf.Max(_prevDeltaX, 0.0001f);
		//			resultAngle.x = Mathf.Clamp(Mathf.Atan2(deltaX, -_prevDeltaY.x) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE);
		//			if (_dimension > 1) { resultAngle.y = Mathf.Clamp(Mathf.Atan2(deltaX, -_prevDeltaY.y) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 2) { resultAngle.z = Mathf.Clamp(Mathf.Atan2(deltaX, -_prevDeltaY.z) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 3) { resultAngle.w = Mathf.Clamp(Mathf.Atan2(deltaX, -_prevDeltaY.w) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		}

		//		else if(_nextLinkedCurveKey != null)
		//		{
		//			float deltaX = Mathf.Max(_nextDeltaX, 0.0001f);
		//			resultAngle.x = Mathf.Clamp(Mathf.Atan2(deltaX, _nextDeltaY.x) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE);
		//			if (_dimension > 1) { resultAngle.y = Mathf.Clamp(Mathf.Atan2(deltaX, _nextDeltaY.y) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 2) { resultAngle.z = Mathf.Clamp(Mathf.Atan2(deltaX, _nextDeltaY.z) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//			if (_dimension > 3) { resultAngle.w = Mathf.Clamp(Mathf.Atan2(deltaX, _nextDeltaY.w) * Mathf.Rad2Deg, MIN_ANGLE, MAX_ANGLE); }
		//		}
		//		else
		//		{
		//			resultAngle = Vector4.zero;
		//		}

		//		SetSmoothAngle(resultAngle, KEY_POS.PREV);
		//		SetSmoothAngle(resultAngle, KEY_POS.PREV);
		//	}
		//	else
		//	{
		//		//AutoSmooth가 아니면 기존 Angle값을 그대로 유지하여 다시 계산한다.
		//		SetSmoothAngle(_prevSmoothAngle, KEY_POS.PREV);
		//		SetSmoothAngle(_nextSmoothAngle, KEY_POS.PREV);
		//	}
		//} 
		#endregion

		// Get / Set
		//-------------------------------------------------
		public TANGENT_TYPE PrevTangent
		{
			get
			{
				return _prevTangentType;
			}
		}

		public TANGENT_TYPE NextTangent
		{
			get
			{
				return _nextTangentType;
				//if(_keyItpType == KEY_ITP_TYPE.Broken)	{ return ; }
				//else									{ return TANGENT_TYPE.Smooth; }
			}
		}

		public float GetItp_Float(float curKeyframe, bool isWithPrevKey, int iCurKeyframe)
		{
			if (isWithPrevKey)
			{
				return _prevCurveResult.GetInterpolation(curKeyframe, iCurKeyframe);
			}
			else
			{
				return _nextCurveResult.GetInterpolation(curKeyframe, iCurKeyframe);
			}
		}

		public float GetItp_Int(int curKeyframe, bool isWithPrevKey)
		{
			if (isWithPrevKey)
			{
				//return _prevCurveResult.GetInterpolation_Int(curKeyframe);
				return _prevCurveResult.GetInterpolation((float)curKeyframe, curKeyframe);
			}
			else
			{
				//return _nextCurveResult.GetInterpolation_Int(curKeyframe);
				return _nextCurveResult.GetInterpolation((float)curKeyframe, curKeyframe);
			}
		}

		//private static float _tmpDeltaX = 0.0f;
		//private static float _tmpItp = 0.0f;
		private static float _tmpRevItp = 0.0f;

		//private static float _tmpT = 0.0f;
		//private static float _tmpRevT = 0.0f;

		//private static apAnimCurve _tmpKeyA = null;
		//private static apAnimCurve _tmpKeyB = null;

		


		/// <summary>
		/// 일반적으로 부드러운 Interpolation 변형 (Key 없이 사용할 수 있다)
		/// [A의 입장에서 Interpolation을 꺼내고자 한다면 (1-결과값)을 해야한다.]
		/// </summary>
		/// <param name="itp"></param>
		/// <returns></returns>
		public static float GetSmoothInterpolation(float itp)
		{
			_tmpRevItp = 1.0f - itp;
			return  //0.0f * (_tmpRevItp * _tmpRevItp * _tmpRevItp) +
					//0.0f * (3.0f * _tmpRevItp * _tmpRevItp * itp) +
					1.0f * (3.0f * _tmpRevItp * itp * itp) +
					1.0f * (itp * itp * itp);
		}


	}
}