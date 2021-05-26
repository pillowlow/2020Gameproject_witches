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
using System.Runtime.InteropServices;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Core class with animations, meshes, and texture data
	/// </summary>
	public class apPortrait : MonoBehaviour
	{
		// Members
		//-----------------------------------------------------
		//public int _testVar = 0;
		
		//텍스쳐 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apTextureData> _textureData = new List<apTextureData>();

		//메시 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apMesh> _meshes = new List<apMesh>();

		//메시 그룹 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apMeshGroup> _meshGroups = new List<apMeshGroup>();


		//컨트롤 파라미터 등록 정보 [이건 Editor / Opt Realtime에 모두 적용된다.]
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public apController _controller = new apController();

		//애니메이션 등록 정보 [이건 Editor / Opt Runtime에 모두 적용된다]
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apAnimClip> _animClips = new List<apAnimClip>();


		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public apAnimPlayManager _animPlayManager = new apAnimPlayManager();

		//추가 20.11.23 : 애니메이션 처리 가속화를 위한 별도의 업데이트와 매핑 클래스
		//실시간으로 생성되며 Opt용이다.
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public apAnimPlayMapping _animPlayMapping = null;

		
		//RootUnit으로 적용되는 MainMeshGroup을 여러개를 둔다.
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<int> _mainMeshGroupIDList = new List<int>();

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public List<apMeshGroup> _mainMeshGroupList = new List<apMeshGroup>();



		// 루트 유닛을 여러 개를 둔다 (루트 유닛은 애니메이션이 적용되는 MeshGroup이다)
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apRootUnit> _rootUnits = new List<apRootUnit>();


		

		// 유니크 IDs
		[NonBackupField]
		private apIDManager _IDManager = new apIDManager();


		//추가 21.1.22 : 편집용 VisibiliePreset을 여기에 추가한다.
		[NonBackupField, SerializeField]
		private apVisibilityPresets _visiblePreset = new apVisibilityPresets();

		public apVisibilityPresets VisiblePreset { get { if (_visiblePreset == null) { _visiblePreset = new apVisibilityPresets(); } return _visiblePreset; } }




		//Runtime 계열 Members
		// 이후 "최적화" 버전에서는 이하의 Member만 적용한다.
		// Runtime 계열의 인스턴스들은 모두 opt를 앞에 붙인다.
		// 중요) 백업은 하지 않는다.
		//---------------------------------------------
		/// <summary>A List of Optimized Root Units executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptRootUnit> _optRootUnitList = new List<apOptRootUnit>();

		/// <summary>Currently selected and running Root Unit</summary>
		[NonSerialized]
		public apOptRootUnit _curPlayingOptRootUnit = null;//<<현재 재생중인 OptRootUnit
		
		/// <summary>A List of Optimized Transforms executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptTransform> _optTransforms = null;

		/// <summary>A List of Optimized Meshes executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptMesh> _optMeshes = null;

		

		// Opt Texture를 Bake하여 관리한다.
		/// <summary>[Please do not use it] A List of Optimized Texture Data executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptTextureData> _optTextureData = null;


		//추가
		// Material 중에서 Batch가 될만한 것들은 중앙에서 관리를 한다.
		/// <summary>[Please do not use it] A List of Batched Materials executed at runtime</summary>
		[SerializeField, NonBackupField]
		public apOptBatchedMaterial _optBatchedMaterial = null;



		/// <summary>
		/// Listener to receive animation events.
		/// Since it is called by "UnitySendMessage", its object must be a class inherited from "MonoBehaviour".
		/// </summary>
		[SerializeField, NonBackupField]
		public MonoBehaviour _optAnimEventListener = null;
		

		public enum INIT_STATUS
		{
			Ready,
			AsyncLoading,
			Completed
		}
		[NonSerialized]
		private INIT_STATUS _initStatus = INIT_STATUS.Ready;

		/// <summary>Status of Initialization</summary>
		public INIT_STATUS InitializationStatus { get { return _initStatus; } }

		//비동기 로딩이 끝났을때 발생하는 이벤트
		/// <summary>The delegate type of the function to be called when the asynchronous initialization finishes.</summary>
		/// <param name="portrait">Initialized Portrait</param>
		public delegate void OnAsyncLinkCompleted(apPortrait portrait);
		private OnAsyncLinkCompleted _funcAyncLinkCompleted = null;


		//기본 데이터가 저장될 하위 GameObject
		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup = null;

		//작업 저장 효율성을 위해서 일부 메인 데이터를 GameObject 형식으로 저장한다. (크윽 Undo)
		//저장되는 타겟은 Mesh와 MeshGroup
		//- Mesh는 Vertex 데이터가 많아서 저장 필요성이 있다.
		//- MeshGroup은 ModMesh의 데이터가 많아서 저장 필요성이 있다.
		//직렬화가 가능하거나 연동 전용 객체인 RootUnit과 Image은 GameObject로 만들지 않는다.
		//AnimClip과 Param은 Realtime과 연동하는 객체이므로 별도로 분리하지 않는다.
		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_Mesh = null;

		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_MeshGroup = null;

		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_Modifier = null;


		/// <summary>[Please do not use it] Frame Per Seconds</summary>
		[SerializeField]
		public int _FPS = 30;

		//이전 : Important가 아닐 때의 타이머 > UpdateToken 방식으로 변경
		//[NonBackupField]
		//private float _timePerFrame = 1.0f / 30.0f;

		//[NonBackupField]
		//private float _tDelta = 0.0f;

		//추가 2.28 : Important가 아닌 업데이트를 위한 토큰
		[NonSerialized, NonBackupField]
		private apOptUpdateChecker.UpdateToken _updateToken = null;

		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public float _bakeScale = 0.01f;//Bake시 0.01을 곱한다.

		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public float _bakeZSize = 1.0f;//<<현재 Depth에 따라 1 차이를 준다.

		

		//이미지 저장 경로를 저장하자
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public string _imageFilePath_Thumbnail = "";

		[NonSerialized]
		public Texture2D _thumbnailImage = null;




		public enum SHADER_TYPE
		{
			/// <summary>(Default) Alpha Blended Interpolation</summary>
			AlphaBlend = 0,
			/// <summary>Additive</summary>
			Additive = 1,
			/// <summary>Soft Additive</summary>
			SoftAdditive = 2,
			/// <summary>2X Multiplicative</summary>
			Multiplicative = 3
		}

		
		//물리 옵션 - Editor / Opt (기본값은 On)
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public bool _isPhysicsPlay_Editor = true;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public bool _isPhysicsSupport_Editor = true;//<<옵션과 관계없이 지금 물리를 지원하는가


		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public bool _isPhysicsPlay_Opt = true;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public int _updateCount = 0;

		//추가 20.7.9 : 물리에서 사용되는 타이머를 여기서 공통으로 만들어서 사용한다.
		//Opt 포함
		[NonSerialized, NonBackupField]
		private System.Diagnostics.Stopwatch _physicsTimer = null;
		[NonSerialized, NonBackupField]
		private float _physicsDeltaTime = 0.0f;
		public float PhysicsDeltaTime { get { return _physicsDeltaTime; } }
		private const float PHYSICS_MAX_DELTA_TIME = 0.05f;//20FPS보다 낮은 FPS에서는 물리 시간이 고정이다.
		

		//물리에 주는 외력을 관리하는 객체
		//저장되는 값은 없고, API만 제공한다.
		//Editor/Runtime 모두 사용 가능
		private apForceManager _forceManager = new apForceManager();

		/// <summary>Manager controlling physical effects</summary>
		public apForceManager ForceManager { get { return _forceManager; } }

		/// <summary>[Please do not use it]</summary>
		[HideInInspector, NonBackupField]
		public GameObject _bakeUnlinkedGroup = null;

		/// <summary>
		/// [Please do not use it]
		/// Instead of setting this variable, use function "SetImportant(bool isImportant)" instead.
		/// </summary>
		[SerializeField]
		public bool _isImportant = true;

		
		//자동 시작하는 AnimClipID
		//-1이면 자동으로 시작되는 AnimClip은 없다.
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public int _autoPlayAnimClipID = -1;

		//Inititialize 직후에 이 값을 True로 한다.
		//Show RootUnit에서 이 값이 True일때 자동 재생을 검토한다. (단 한번 실행된다.)
		[NonSerialized]
		private bool _isAutoPlayCheckable = false;


		//최적화된 Portrait
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public bool _isOptimizedPortrait = false;//<<이게 True이면 에디터로 작업할 수 없다.

		//기본 Portrait라면..
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector, NonBackupField]//Mono 타입으로 저장은 하되 백업은 안됨
		public apPortrait _bakeTargetOptPortrait = null;//Opt Target Bake시 타겟이 되는 Portrait

		//최적화 Portrait라면..
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector, NonBackupField]//Mono 타입으로 저장은 하되 백업은 안됨
		public apPortrait _bakeSrcEditablePortrait = null;//Opt Target Bake시 (자신이 OptPortrait 일때) 그 소스가 되는 Portrait (타겟이 불확실할 경우 경고 메시지를 주기 위함)


		//추가 3.22 : SortingLayer 관련
		//모든 Mesh는 동일한 Sorting Layer Name/Order를 가진다.
		//Bake할 때 그 값이 같아야 한다.
		[SerializeField]
		public int _sortingLayerID = 0;

		[SerializeField]
		public int _sortingOrder = 0;

		//추가 19.8.18 : Sorting Option이 나뉜다.
		public enum SORTING_ORDER_OPTION
		{	
			SetOrder = 0,				//[기본값] _sortingOrder를 공통적으로 적용
			Disabled = 1,				//Sorting Order를 적용하지 않음
			DepthToOrder = 2,			//Depth의 값을 Order로 적용 (실시간)
			ReverseDepthToOrder = 3,	//(Max-Depth)의 값을 Order로 적용 (실시간)
		}
		[SerializeField]
		public SORTING_ORDER_OPTION _sortingOrderOption = SORTING_ORDER_OPTION.SetOrder;

		//추가 21.1.31		
		[SerializeField]
		public int _sortingOrderPerDepth = 1;//Depth마다 SortingOrder가 몇씩 증가하는가 (최소 1)



		// 추가 4.26 : Mecanim 설정
		[SerializeField]
		public bool _isUsingMecanim = false;

		[SerializeField]
		public string _mecanimAnimClipResourcePath = "";

		//추가 6.7
		//Empty State용 Animation Clip을 만든다.
		//Motion이 비어있으면 제어가 안된다.
		[SerializeField, NonBackupField]//백업은 안된다.
		public AnimationClip _emptyAnimClipForMecanim = null;

		//연결된 메카님 정보
		[SerializeField, NonBackupField]//백업은 안된다.
		public Animator _animator = null;
		

		
		//AnimClip 의 Asset과 연결 데이터
		//[SerializeField, NonBackupField]
		//public List<apAnimMecanimData_AssetPair> _animClipAssetPairs = new List<apAnimMecanimData_AssetPair>();
		
		//메카님 레이어 정보 (Blend 포함)
		[SerializeField, NonBackupField]
		public List<apAnimMecanimData_Layer> _animatorLayerBakedData = new List<apAnimMecanimData_Layer>();

		[SerializeField, NonBackupField]
		public List<apPSDSet> _bakedPsdSets = new List<apPSDSet>();

		//추가 9.19
		//빌보드 처리를 위한 카메라
		public enum BILLBOARD_TYPE
		{
			None = 0,
			Billboard = 1,
			UpFixed = 2
		}
		[SerializeField, HideInInspector]
		public BILLBOARD_TYPE _billboardType = BILLBOARD_TYPE.None;
		[NonSerialized]
		public Transform _transform = null;

		#region [미사용 코드] 이전 코드 : 단일 카메라만 지원
		//[NonSerialized]
		//private Camera _curCamera = null;
		//[NonSerialized]
		//private Transform _curCameraTransform = null;
		//[NonSerialized]
		//private float _zDepthOnPerspectiveCam = 0.0f; 
		#endregion

		//변경 : 1개 또는 다수의 카메라를 자동으로 탐색하여 처리
		[NonSerialized]
		private apOptMainCamera _mainCamera = null;

		//추가 19.9.24 : 멀티 카메라를 지원하기 위한 옵션 (VR인 경우에만 지원한다.)
		public enum VR_SUPPORT_MODE
		{
			None = 0,
			SingleCamera = 1,
			MultiCamera = 2,
		}
		[SerializeField]
		public VR_SUPPORT_MODE _vrSupportMode = VR_SUPPORT_MODE.None;

		//VR이 켜진 경우 렌더 텍스쳐의 크기 (최적화가 안되므로)
		public enum VR_RT_SIZE
		{
			ByMeshSettings = 0,
			ByEyeTextureSize = 1
		}

		[SerializeField]
		public VR_RT_SIZE _vrRenderTextureSize = VR_RT_SIZE.ByMeshSettings;



		//추가 19.9.24 : 빌보드인 경우, 카메라의 SortMode를 강제로 Orthographic 고정할 것인지 여부 (기본값은 True)
		[SerializeField]
		public bool _isForceCamSortModeToOrthographic = true;

		//추가 9.25 : 그림자 생성 모드 > 유니티 Enum( UnityEngine.Rendering.ShadowCastingMode과 동일하지만 일부러 따로 만듬
		public enum SHADOW_CASTING_MODE
		{
			Off = 0, On = 1, TwoSided = 2, ShadowsOnly = 3
		}
		[SerializeField]
		public SHADOW_CASTING_MODE _meshShadowCastingMode = SHADOW_CASTING_MODE.Off;
		[SerializeField]
		public bool _meshReceiveShadow = false;


		
		//Unity 2017 이상 : Timeline 연동 : 별도의 함수 없이 바로 시작
#if UNITY_2017_1_OR_NEWER
		[Serializable]
		public class TimelineTrackPreset
		{
			[SerializeField, NonBackupField]
			public PlayableDirector _playableDirector;

			[SerializeField, NonBackupField]
			public string _trackName;

			[SerializeField, NonBackupField]
			public int _layer = 0;

			[SerializeField, NonBackupField]
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive;
		}
		[SerializeField, NonBackupField]
		public TimelineTrackPreset[] _timelineTrackSets;
#endif


		//추가 3.29 : 에디터의 Hierarchy에서 보여지는 순서에 대한 ID 리스트 클래스
		[SerializeField, NonBackupField]
		public apObjectOrders _objectOrders = new apObjectOrders();

		//추가 19.5.26 : ModMeshSet을 사용한 "v1.1.7에 적용된용량 최적화 빌드가 되었는가"
		[SerializeField, NonBackupField]
		public bool _isSizeOptimizedV117 = false;

		
		//추가 19.6.2 : MaterialSet를 저장하자. Bake시 이용함.
		[SerializeField]
		public List<apMaterialSet> _materialSets = new List<apMaterialSet>();


		//20.8.11 : 메시의 플립 체크시, "리깅 본을 검사"하는 항목을 추가할 수 있다.
		//- 리깅 여부 관계없이 체크 (Check regardless of Rigging) : 대신 다소 느려질 수 있음
		//- 리깅된 메시는 제외 (Excluding mesh with Rigging applied)
		public enum FLIPPED_MESH_CHECK : int
		{
			/// <summary>트랜스폼만 체크한다.(기본값)</summary>
			TransformOnly = 0,
			/// <summary>부모 트랜스폼과 리깅 본을 모두 체크</summary>
			All = 1,
			
		}
		[SerializeField]
		public FLIPPED_MESH_CHECK _flippedMeshOption = FLIPPED_MESH_CHECK.TransformOnly;
		
		//20.8.5 본과 자식 메시 그룹의 행렬 계산 모드를 결정할 수 있다.
		public enum ROOT_BONE_SCALE_METHOD : int
		{
			Default = 0,
			SkewScale = 1
		}

		[SerializeField]
		public ROOT_BONE_SCALE_METHOD _rootBoneScaleMethod = ROOT_BONE_SCALE_METHOD.Default;


		//추가 20.9.14 : 만약 작업 전에 프리팹으로서 생성된 객체였다면,
		//Diconnect 하기 전에 "프리팹으로서의 Root Game Object"와 "프리팹 에셋"을 "복구용"으로 지정하자
		//Optimized 객체도 별도로 프리팹이 될 수 있으므로, 따로 작업한다.
		//이값은 에디터에서 처음 조회할 때 갱신한다.
		//[Legacy] : Disconnect 하더라도 프리팹 정보가 남아있다. 복구시 이 변수들이 필요 없을 수 있다.
		//[2018.3] : Disconnect 하면 프리팹 정보가 완전히 사라진다. 복구시 이 변수들이 꼭 필요하며, Inspector에서 이 정보도 날릴 수 있다.
		
		[SerializeField, NonBackupField, HideInInspector]
		public GameObject _rootGameObjectAsPrefabInstanceForRestore = null;

		[SerializeField, NonBackupField, HideInInspector]
		public UnityEngine.Object _srcPrefabAssetForRestore = null;
		
		//추가 20.9.15 : 이전 프레임에서의 위치와 현재 프레임의 InvRotation Matrix를 계산하자.
		//빌보드 처리를 위함
		[NonSerialized]
		private Vector3 _posW_Prev1F = Vector3.zero;

		[NonSerialized]
		private Matrix4x4 _rotationOnlyMatrixIfBillboard = Matrix4x4.identity;

		[NonSerialized]
		private Matrix4x4 _invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;


		// Init
		//-----------------------------------------------------
		void Awake()
		{
			if (Application.isPlaying)
			{
				if (_FPS < 10)
				{
					_FPS = 10;
				}
				//_isImportant = true;

				//이전 > UpdateToken으로 변경
				//_timePerFrame = 1.0f / (float)_FPS;
				//_tDelta = _timePerFrame * UnityEngine.Random.Range(0.0f, 1.0f);
				
				
				if (_initStatus == INIT_STATUS.Ready)
				{
					//_initStatus = INIT_STATUS.Ready;
					_funcAyncLinkCompleted = null;
				}
			}
		}



		void Start()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
#endif
				if (_FPS < 10)
				{
					_FPS = 10;
				}
				
				//이전 > UpdateToken 방식으로 변경
				//_timePerFrame = 1.0f / (float)_FPS;
				//_tDelta = _timePerFrame * UnityEngine.Random.Range(0.0f, 1.0f);


				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();

					//자동으로 시작을 해보자
					//ShowRootUnit(); //<< Initialize에 이미 ShowRootUnit이 포함되어 있다.

					_updateCount = 0;
					//_updateKeyIndex = 0;
				}

				_controller.InitRequest();
#if UNITY_EDITOR
			}
#endif

		}

		//추가 : 삭제될 때
		void OnDestroy()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
#endif
				//생성된 재질을 삭제하자.
				apOptSharedMaterial.I.OnPortraitDestroyed(this);
				_optBatchedMaterial.Clear(true);
#if UNITY_EDITOR
			}
#endif
		}

		// Update
		//-----------------------------------------------------
		void Update()
		{
#if UNITY_EDITOR
			try
			{
				if (Application.isPlaying)
				{
#endif
					

					if (_curPlayingOptRootUnit == null)
					{
						return;
					}

					//추가 2.28 : 
					if(!_isImportant)
					{
						_updateToken = apOptUpdateChecker.I.AddRequest(_updateToken, _FPS, Time.deltaTime);
					}

					
#region [미사용 코드 : LateUpdate로 넘어감]
//					_tDelta += Time.deltaTime;
//					//if (_tDelta > _timePerFrame)
//					//if(true)
//					{
//						//_tDelta -= _timePerFrame;//아래에 갱신한 부분이 있다.

//						//전체 업데이트하는 코드

//						//일정 프레임마다 업데이트를 한다.
//#if UNITY_EDITOR
//						Profiler.BeginSample("Portrait - Update Transform");
//#endif




//						//_optRootUnit.UpdateTransforms(_tDelta);
//						//_curPlayingOptRootUnit.UpdateTransforms(_tDelta);
//						//_curPlayingOptRootUnit.UpdateTransforms(_timePerFrame);


//						//원래는 이 코드
//						_curPlayingOptRootUnit.UpdateTransforms(Time.deltaTime);//<

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif


//						//mask Mesh의 업데이트는 모든 Mesh 처리가 끝나고 한다.
//						if (_isAnyMaskedMeshes)
//						{

//#if UNITY_EDITOR
//							Profiler.BeginSample("Portrait - Post Update <Mask>");
//#endif

//							//Mask Parent 중심의 업데이트 삭제 -> Child 중심의 업데이트로 변경
//							//for (int i = 0; i < _optMaskedMeshes.Count; i++)
//							//{
//							//	_optMaskedMeshes[i].RefreshMaskedMesh();
//							//}

//							for (int i = 0; i < _optClippedMeshes.Count; i++)
//							{
//								_optClippedMeshes[i].RefreshClippedMesh();
//							}

//#if UNITY_EDITOR
//							Profiler.EndSample();
//#endif

//						}

//						_tDelta -= _timePerFrame;
//						//_tDelta = 0.0f;//Delatyed tDelta라면 0으로 바꾸자 


//					}
#endregion


#if UNITY_EDITOR
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}



		void LateUpdate()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif

			if (_initStatus != INIT_STATUS.Completed)
			{
				//로딩이 다 되지 않았다면 처리를 하지 않는다.
				return;
			}

			//추가 21.4.3 : 출력할게 없다면 스크립트를 중단한다.
			if (_curPlayingOptRootUnit == null)
			{
				return;
			}

			//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
			CalculatePhysicsTimer();


			#region [핵심 코드 >>> Update에서 넘어온 코드]
			//_tDelta += Time.deltaTime;//<<이전 방식 (Important가 아닌 경우)

			#region [사용 : 1프레임 지연 없이 사용하는 경우. 단, 외부 처리에 대해서는 Request 방식으로 처리해야한다.]
			//힘 관련 업데이트
			ForceManager.Update(Time.deltaTime);

			//애니메이션 업데이트
			_animPlayManager.Update(Time.deltaTime);

			//추가 20.11.23 : 애니메이션 정보가 모디파이어 처리에 반영되도록 매핑 클래스를 동작시킨다.
			_animPlayMapping.Update();


			//추가 : 애니메이션 업데이트가 끝났다면 ->
			//다른 스크립트에서 요청한 ControlParam 수정 정보를 반영한다.
			_controller.CompleteRequests();
			#endregion


			//if (_tDelta > _timePerFrame)
			//if(true)
			if (_curPlayingOptRootUnit != null)
			{
				//추가 9.19 : Camera 체크
				//if(_billboardType != BILLBOARD_TYPE.None)
				//{
				//	CheckAndRefreshCameras();
				//} >> 이전 : 빌보드가 아닌 경우 생략

				//변경 : 언제나
				CheckAndRefreshCameras();


				//전체 업데이트하는 코드
				//일정 프레임마다 업데이트를 한다.
				//#if UNITY_EDITOR
				//					Profiler.BeginSample("Portrait - Update Transform");
				//#endif
				if (_isImportant)
				{
					_curPlayingOptRootUnit.UpdateTransforms(Time.deltaTime);
				}
				else
				{
					//이전 방식 : 랜덤값이 포함된 간헐적 업데이트
					//if (_tDelta > _timePerFrame)
					//{
					//	//Important가 꺼진다면 프레임 FPS를 나누어서 처리한다.
					//	_curPlayingOptRootUnit.UpdateTransforms(_timePerFrame);

					//	_tDelta -= _timePerFrame;
					//}
					//else
					//{
					//	//추가 4.8
					//	//만약 Important가 꺼진 상태에서 MaskMesh가 있다면
					//	//Mask Mesh의 RenderTexture가 매 프레임 갱신 안될 수 있다.
					//	//따라서 RenderTexture 만큼은 매 프레임 갱신해야한다.
					//	_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
					//}

					//새로운 방식 : 중앙에서 관리하는 토큰 업데이트
					if (apOptUpdateChecker.I.GetUpdatable(_updateToken))
					{
						_curPlayingOptRootUnit.UpdateTransforms(_updateToken.ResultElapsedTime);
						//_tDelta -= _timePerFrame;
					}
					else
					{
						_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
					}
				}

				//#if UNITY_EDITOR
				//					Profiler.EndSample();
				//#endif			
			}

			PostUpdate();//추가 20.9.15 : 현재 프레임의 위치등을 저장하자.
			#endregion
		}


		public void UpdateForce()
		{
			//강제로 업데이트를 한다.
#if UNITY_EDITOR
			try
			{
#endif
				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();
				}

				
				if(_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 끝나지 않았다면 처리를 하지 않는다.
					return;
				}

				//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
				CalculatePhysicsTimer();


				if (_animPlayManager.IsPlaying_Editor)
				{
					_animPlayManager.Update_Editor(0.0f);

				}
				else
				{
					if (_curPlayingOptRootUnit != null)
					{
						_curPlayingOptRootUnit.UpdateTransforms(0.0f);
					}
				}

				//일정 프레임마다 업데이트를 한다.
				//_optRootUnit.UpdateTransforms(_tDelta);
				

#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}


#if UNITY_2017_1_OR_NEWER
		//추가 3.9 : 타임라인을 포함한 씬에서의 시뮬레이션을 위한 함수 (초기화와 업데이트)
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void InitializeAsSimulating()
		{
			//추가) AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);
		}
#endif

#if UNITY_2017_1_OR_NEWER
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="deltaTime"></param>
		public void UpdateForceAsSimulating(float deltaTime)
		{
			//강제로 업데이트를 한다.
			//시뮬레이션을 위한 것이므로, 애니메이션이 포함된다.
#if UNITY_EDITOR
			try
			{
#endif
				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();
				}

				
				if(_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 끝나지 않았다면 처리를 하지 않는다.
					return;
				}

				//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
				CalculatePhysicsTimer();

				//물리 업데이트
				ForceManager.Update(deltaTime);

				//애니메이션 업데이트
				_animPlayManager.Update(deltaTime);

				if (_curPlayingOptRootUnit != null)
				{
					//추가 9.19 : Camera 체크
					CheckAndRefreshCameras();
					//if(_billboardType != BILLBOARD_TYPE.None)
					//{
					//	CheckCameraAndBillboard();
					//}

					_curPlayingOptRootUnit.UpdateTransforms(0.0f);
				}

				//일정 프레임마다 업데이트를 한다.
				//_optRootUnit.UpdateTransforms(_tDelta);
				

#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}
#endif

		//변경 20.7.9 [EnterPlayMode 기능 관련 개선 코드]
		//유니티 2019.3에서 EnterPlayMode에 들어갈 때 Domain Reload를 스킵할 수 있다.
		//문제는, Link를 하기 위해서 _initStatus가 초기화 되어야 하는 apPortrait인데, 이게 Completed 상태로 그냥 남아버릴 수 있다.
		//여러가지 처리가 있을 수 있지만, 그냥 Domain Reload가 스킵된 상태로 게임에 진입하면 모든 apPortrait를 찾아서
		//강제 초기화 함수를 실행하도록 만들자.
#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitForceWhenDomainReloadSkipped()
		{
			//Debug.LogWarning("InitForceWhenSkipDomainReload");
			//존재하는 모든 apPortrait를 찾는다.
			apPortrait[] portraitsInScene = GameObject.FindObjectsOfType<apPortrait>();
			if(portraitsInScene != null && portraitsInScene.Length > 0)
			{
				for (int i = 0; i < portraitsInScene.Length; i++)
				{
					portraitsInScene[i].ResetInitStatusToReadyWhenDomainReloadSkipped();
				}
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		private void ResetInitStatusToReadyWhenDomainReloadSkipped()
		{
			//Debug.LogError("Init Status [" + _initStatus + " > INIT_STATUS.Ready] (" + gameObject.name + ")");
			_initStatus = INIT_STATUS.Ready;
		}
#endif

		// Event
		//-----------------------------------------------------
		
#if UNITY_EDITOR
		
		//추가 12.13
		//apOptMesh의 OnValidate에서 호출되는 함수.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void OnMeshResetInEditor()
		{
			

			if (Application.isEditor && !Application.isPlaying)
			{
				//19.10.26 : 빌보드를 일단 끈다.
				apPortrait.BILLBOARD_TYPE billboardType = _billboardType;

				try
				{
					if (_optMeshes != null && _optMeshes.Count > 0)
					{
						//Debug.LogError("OnMeshResetInEditor : " + this.name);

						for (int i = 0; i < _optMeshes.Count; i++)
						{
							_optMeshes[i].InitMesh(true);
							_optMeshes[i].ResetMeshAndMaterialIfMissing();
						}
						UpdateForce();
					}
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : Refresh Meshes Failed\n" + ex);
				}

				_billboardType = billboardType;//복구
			}
		}
#endif


		// Functions
		//-----------------------------------------------------

		/// <summary>
		/// Show one of the Root Units. 
		/// The Root Unit can have an animation clip that starts automatically, or it can be the first Root Unit.
		/// </summary>
		public void ShowRootUnit()
		{
			//RootUnit 플레이 조건
			//1. 자동 시작 AnimClip이 있다면 그걸 가지고 있는 RootUnit을 시작한다.
			//2. 없다면 0번 RootUnit을 재생
			
			apOptRootUnit targetOptRootUnit = null;
			apAnimClip firstPlayAnimClip = null;

			if (_isAutoPlayCheckable && _autoPlayAnimClipID >= 0)
			{
				apAnimClip curAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					curAnimClip = _animClips[i];
					if(curAnimClip._uniqueID == _autoPlayAnimClipID
						&& curAnimClip._targetOptTranform != null)
					{
						if(curAnimClip._targetOptTranform._rootUnit != null)
						{
							//자동 재생할 Root Unit을 찾았다.
							targetOptRootUnit = curAnimClip._targetOptTranform._rootUnit;
							firstPlayAnimClip = curAnimClip;

							break;
						}
					}
				}
			}

			_isAutoPlayCheckable = false;

			//없다면 0번 RootUnit 을 선택한다.
			if(targetOptRootUnit == null)
			{
				if(_optRootUnitList.Count > 0)
				{
					targetOptRootUnit = _optRootUnitList[0];
				}
			}


			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}

			//자동 재생을 한다.
			if(firstPlayAnimClip != null)
			{
				PlayNoDebug(firstPlayAnimClip._name);
			}

			//만약 숨어있다가 나타날때 위치가 바뀌어있었다면 워프 가능성이 있다.
			//이 경우를 대비해서 물리 위치를 현재 위치로 갱신해두자
			if(_transform != null)
			{
				_transform = transform;
			}
			_posW_Prev1F = _transform.position;
		}

		/// <summary>
		/// Makes the input Root Unit visible. 
		/// If it has an animation clip that plays automatically, this animation clip will play automatically.
		/// </summary>
		/// <param name="targetOptRootUnit">Root Unit to be visible</param>
		public void ShowRootUnit(apOptRootUnit targetOptRootUnit)
		{
			apAnimClip firstPlayAnimClip = null;
			if(_isAutoPlayCheckable && _autoPlayAnimClipID >= 0)
			{
				//자동 재생은 제한적으로 실행한다.
				//targetOptRootUnit에 포함된 AnimClip만 실행된다.
				apAnimClip curAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					curAnimClip = _animClips[i];
					if(curAnimClip._uniqueID == _autoPlayAnimClipID
						&& curAnimClip._targetOptTranform != null)
					{
						if(curAnimClip._targetOptTranform._rootUnit != null
							&& curAnimClip._targetOptTranform._rootUnit == targetOptRootUnit)
						{
							//자동 재생할 AnimClip을 찾았다.
							firstPlayAnimClip = curAnimClip;
							break;
						}
					}
				}
			}

			_isAutoPlayCheckable = false;

			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}


			//자동 재생을 한다.
			if(firstPlayAnimClip != null)
			{
				Play(firstPlayAnimClip._name);
			}


		}

		/// <summary>
		/// Hide all Root Units
		/// </summary>
		public void HideRootUnits()
		{
			StopAll();

			//추가 21.4.3
			//StopAll이 적용되려면 업데이트가 한번 되어야 한다.
			//Hide되면 애니메이션이 업데이트되지 않으므로, 여기서 강제로 업데이트를 한번 더 하자
			//_animPlayManager.Update(0.0f);
			_animPlayManager.ReleaseAllPlayUnitAndQueues();


			//모두 숨기기
			_curPlayingOptRootUnit = null;

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].Hide();
			}
		}



		//추가 21.3.14 : 실행중인 RootUnit을 리턴한다.
		/// <summary>
		/// Return the currently playing Root Unit.
		/// </summary>
		/// <returns>Root Unit currently playing. If not, return null</returns>
		public apOptRootUnit GetCurrentRootUnit()
		{
			return _curPlayingOptRootUnit;
		}

		/// <summary>
		/// Return the index of the currently playing Root Unit.
		/// </summary>
		/// <returns>Index of the currently playing Root Unit. If not, return -1</returns>
		public int GetCurrentRootUnitIndex()
		{
			if(_curPlayingOptRootUnit == null)
			{
				return -1;
			}
			return _optRootUnitList.IndexOf(_curPlayingOptRootUnit);
		}



		/// <summary>
		/// Initializes the command buffer for clipping mask processing.
		/// </summary>
		/// <param name="targetOptRootUnit">Target Root Unit</param>
		/// <param name="isRegistToCamera">If True, re-register the command buffers to the camera after initialization.</param>
		public void ResetMeshCommandBuffer(apOptRootUnit targetOptRootUnit, bool isRegistToCamera)
		{
			if(targetOptRootUnit == null)
			{
				return;
			}
			targetOptRootUnit.ResetCommandBuffer(isRegistToCamera);
		}

		/// <summary>
		/// [Please do not use it]
		/// Bake Function likes "ShowRootUnit" using Default Visible Value.
		/// </summary>
		/// <param name="targetOptRootUnit">Target Root Unit</param>
		public void ShowRootUnitWhenBake(apOptRootUnit targetOptRootUnit)
		{
			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.ShowWhenBake();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}
		}


		
		/// <summary>Turn physical effects on or off.</summary>
		/// <param name="isPhysicEnabled"></param>
		public void SetPhysicEnabled(bool isPhysicEnabled)
		{
			_isPhysicsPlay_Opt = isPhysicEnabled;
		}


		//--------------------------------------------------------------------------------------
		// Runtime Optimized
		//--------------------------------------------------------------------------------------
		//첫 Bake 후 또는 시작후 로딩시 Modifier -> 해당 OptTransform을 연결한다.
		/// <summary>
		/// Initialize before updating. 
		/// This is done automatically if you do not call the function directly. 
		/// "AsyncInitialize()" is recommended when it takes a lot of execution time.
		/// </summary>
		public bool Initialize()
		{
			//Debug.Log("LinkModifierAndMeshGroups_Opt");
			if(_initStatus != INIT_STATUS.Ready)
			{
				//엥 비동기 로딩 중이거나 로딩이 끝났네염
				//Debug.LogError(">>> 이미 로딩이 된 상태. 초기화 필요");
				return false;
			}

			if(_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;


			HideRootUnits();

			_funcAyncLinkCompleted = null;
			_isAutoPlayCheckable = true;

			//추가 20.7.5 : 컨트롤 파라미터를 초기화 (이게 왜 없었지)
			_controller.InitRuntime();
			


			//추가 20.11.23 : 모디파이어 최적화를 위한 애니메이션 매핑 클래스
			//생성과 동시에 링크가 된다.
			if(_animPlayMapping == null)
			{
				_animPlayMapping = new apAnimPlayMapping(this);
			}
			else
			{
				//다시 링크를 하자
				_animPlayMapping.Link(this);
			}
			


			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				_optRootUnitList[iOptRootUnit].Link(this);
			}

			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];
				optTransform.ClearResultParams(false);
			}

			//추가 : BatchedMat도 연결
			_optBatchedMaterial.Link(this);

			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);//<<이때 ShowHide도 결정된다.
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
			}

			HideRootUnits();

			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];

				List<apOptModifierUnitBase> modifiers = optTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apOptModifierUnitBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.Link(this, optTransform);

					//mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

					////삭제 조건1 - MeshGroup이 없다
					//if (mod._meshGroup == null)
					//{
					//	continue;
					//}

					List<apOptParamSetGroup> paramSetGroups = mod._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apOptParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								//paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamName);
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								//Debug.LogError("TODO : KeyFrame 방식 연동");
								break;
						}


						List<apOptParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apOptParamSet paramSet = paramSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup, this);



#region [미사용 코드] Editor와 달리 여기서는 Monobehaviour인 것도 있고, Bake에서 이미 1차적으로 연결을 완료했다.
							//List<apOptModifiedMesh> meshData = paramSet._meshData;
							//for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
							//{
							//	apOptModifiedMesh modMesh = meshData[iMesh];

							//	//modMesh._meshGroupUniqueID = meshGroup._uniqueID;

							//	switch (modMesh._targetType)
							//	{
							//		case apModifiedMesh.TARGET_TYPE.VertexMorph:
							//			{
							//				modMesh.Link
							//				meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
							//				if (meshTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshTransform);
							//					modMesh.Link_VertexMorph(meshGroup, meshTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.MeshTransform:
							//			{
							//				meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
							//				if (meshTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshTransform);
							//					modMesh.Link_MeshTransform(meshGroup, meshTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.MeshGroupTransform:
							//			{
							//				meshGroupTransform = meshGroup.GetMeshGroupTransform(modMesh._transformUniqueID);
							//				if (meshGroupTransform != null)
							//				{
							//					renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
							//					modMesh.Link_MeshGroupTransform(meshGroup, meshGroupTransform, renderUnit);
							//				}
							//			}
							//			break;

							//		case apModifiedMesh.TARGET_TYPE.Bone:
							//			{
							//				//TODO : Bone 처리도 해주자
							//				modMesh.Link_Bone();
							//			}
							//			break;
							//	}

							//}

							//paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
							//{
							//	return a._meshGroup == null;//<<연동이 안된..
							//});

							//List<apModifiedBone> boneData = paramSet._boneData;
							////TODO : 본 연동 
#endregion
						}
					}
				}

				//이전
				//optTransform.RefreshModifierLink();//이 코드는 사용되지 않는다. Root OptTransform에서만 호출해야한다.
			}

			//변경 10.3
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				apOptRootUnit rootUnit = _optRootUnitList[i];

				rootUnit._rootOptTransform.ClearResultParams(true);
				rootUnit._rootOptTransform.RefreshModifierLink(true, true);

				//추가 20.8.30
				rootUnit._rootOptTransform.Initialize(true, true);
			}
			
			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkOpt(this);
			}

			//AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);

			

			
			//로딩 끝
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras();

			ShowRootUnit();

			return true;
		}


		//비동기 방식의 로딩
		/// <summary>
		/// Initialize asynchronously using coroutine. It does the same thing as the "Initialize ()" function.
		/// </summary>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.</returns>
		public bool AsyncInitialize()
		{
			if(_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;
			
			if(_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			HideRootUnits();

			

			StartCoroutine(LinkOptCoroutine());

			return true;
		}

		
		/// <summary>
		/// Initialize asynchronously using coroutine. It does the same thing as the "Initialize ()" function.
		/// </summary>
		/// <param name="onAsyncLinkCompleted">Functions to receive callbacks when initialization is complete.</param>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.></returns>
		public bool AsyncInitialize(OnAsyncLinkCompleted onAsyncLinkCompleted)
		{
			if(_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;

			_funcAyncLinkCompleted = onAsyncLinkCompleted;

			//for (int i = 0; i < _optMeshes.Count; i++)
			//{
			//	_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
			//	_optMeshes[i].Hide();
			//}

			if(_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			HideRootUnits();

			StartCoroutine(LinkOptCoroutine());

			return true;

		}

		/// <summary>
		/// Initialize using coroutine. 
		/// This function runs at low CPU usage by setting the "time interval at which Yield is called" by the user. 
		/// However, the processing time may be very long.
		/// </summary>
		/// <param name="timePerYield">Time value for whether Yield is called every few milliseconds during initialization.(10ms ~ 1000ms)</param>
		/// <param name="onAsyncLinkCompleted">Functions to receive callbacks when initialization is complete.</param>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.></returns>
		public bool AsyncInitialize(int timePerYield, OnAsyncLinkCompleted onAsyncLinkCompleted = null)
		{
			if(_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;

			apAsyncTimer asyncTimer = new apAsyncTimer(timePerYield);

			_funcAyncLinkCompleted = onAsyncLinkCompleted;

			if(_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			HideRootUnits();

			StartCoroutine(LinkOptCoroutineWithAsyncTimer(asyncTimer));

			return true;

		}


		private IEnumerator LinkOptCoroutine()
		{
			//if(_initStatus != INIT_STATUS.Ready)
			//{
			//	yield break;
			//}

			//추가 20.7.5 : 컨트롤 파라미터를 초기화 (이게 왜 없었지)
			_controller.InitRuntime();

			//추가 20.11.23 : 모디파이어 최적화를 위한 애니메이션 매핑 클래스
			//생성과 동시에 링크가 된다.
			if(_animPlayMapping == null)
			{
				_animPlayMapping = new apAnimPlayMapping(this);
			}
			else
			{
				//다시 링크를 하자
				_animPlayMapping.Link(this);
			}


			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				_optRootUnitList[iOptRootUnit].Link(this);
			}

			
			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];
				optTransform.ClearResultParams(false);
			}

			HideRootUnits();

			//Wait
			yield return new WaitForEndOfFrame();

			//추가 : BatchedMat도 연결
			_optBatchedMaterial.Link(this);

			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
				_optMeshes[i].Hide();//<<비동기에서는 바로 Hide
			}

			int nLoad = 0;

			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];

				List<apOptModifierUnitBase> modifiers = optTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apOptModifierUnitBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.Link(this, optTransform);

					//Wait
					nLoad++;
					if (nLoad > 5)
					{
						nLoad = 0;
						yield return new WaitForEndOfFrame();
					}

					
					List<apOptParamSetGroup> paramSetGroups = mod._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apOptParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								break;
						}


						List<apOptParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apOptParamSet paramSet = paramSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup, this);
						}
					}

					//Wait
					nLoad++;
					if (nLoad > 5)
					{
						nLoad = 0;
						yield return new WaitForEndOfFrame();
					}
				}

				//이전
				//optTransform.RefreshModifierLink();
				
			}

			//변경 10.3
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				apOptRootUnit rootUnit = _optRootUnitList[i];
				
				rootUnit._rootOptTransform.ClearResultParams(true);
				rootUnit._rootOptTransform.RefreshModifierLink(true, true);

				//추가 20.8.30
				rootUnit._rootOptTransform.Initialize(true, true);
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkOpt(this);
			}



			//Wait
			yield return new WaitForEndOfFrame();
			

			//추가) AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);
			_isAutoPlayCheckable = true;

			//Wait
			yield return new WaitForEndOfFrame();

			
			//끝!
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//if(_optRootUnitList.Count > 0)
			//{
			//	ShowRootUnit(_optRootUnitList[0]);//일단 첫번째 RootUnit이 나온다.
			//}

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras();

			ShowRootUnit();

			

			if(_funcAyncLinkCompleted != null)
			{
				//콜백 이벤트 호출
				_funcAyncLinkCompleted(this);
				_funcAyncLinkCompleted = null;
			}


		}


		//추가 19.5.28 : AsyncTimer를 이용하여 LinkOpCoroutine를 개선한 버전.
		//실제로 실행 시간 타이머가 동작한다.
		private IEnumerator LinkOptCoroutineWithAsyncTimer(apAsyncTimer asyncTimer)
		{

			//랜덤하게 프레임을 쉬어주자
			int nWaitRandom = UnityEngine.Random.Range(0, 5);
			for (int i = 0; i < nWaitRandom; i++)
			{
				yield return new WaitForEndOfFrame();
			}

			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				yield return _optRootUnitList[iOptRootUnit].LinkAsync(this, asyncTimer);
			}

			
			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];
				optTransform.ClearResultParams(false);
			}

			HideRootUnits();

			//타이머에 의해서 Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
			

			//BatchedMat도 연결
			_optBatchedMaterial.Link(this);


			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
				_optMeshes[i].Hide();//<<비동기에서는 바로 Hide

				//타이머에 의해서 Wait
				if(asyncTimer.IsYield())
				{
					yield return asyncTimer.WaitAndRestart();
				}
			}

			
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				apOptTransform optTransform = _optTransforms[iOptTransform];

				List<apOptModifierUnitBase> modifiers = optTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apOptModifierUnitBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.Link(this, optTransform);

					//타이머에 의해서 Wait
					if(asyncTimer.IsYield())
					{
						yield return asyncTimer.WaitAndRestart();
					}

					
					List<apOptParamSetGroup> paramSetGroups = mod._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apOptParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								break;
						}


						List<apOptParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apOptParamSet paramSet = paramSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup, this);
						}

						//타이머에 의해서 Wait
						if(asyncTimer.IsYield())
						{
							yield return asyncTimer.WaitAndRestart();
						}
					}
				}
				
			}

			//RefreshModifierLink
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				apOptRootUnit rootUnit = _optRootUnitList[i];
				
				rootUnit._rootOptTransform.ClearResultParams(true);
				yield return rootUnit._rootOptTransform.RefreshModifierLinkAsync(true, true, asyncTimer);

				//추가 20.8.30
				yield return rootUnit._rootOptTransform.InitializeAsync(true, true, asyncTimer);

				
			}

			//타이머에 의해서 Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				yield return _animClips[i].LinkOptAsync(this, asyncTimer);
			}



			//타이머에 의해서 Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
			

			//추가) AnimPlayer를 추가했다.
			yield return _animPlayManager.LinkPortraitAsync(this, asyncTimer);
			_isAutoPlayCheckable = true;

			//Wait
			yield return new WaitForEndOfFrame();

			
			//끝!
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//if(_optRootUnitList.Count > 0)
			//{
			//	ShowRootUnit(_optRootUnitList[0]);//일단 첫번째 RootUnit이 나온다.
			//}

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras();

			ShowRootUnit();

			

			//AsyncTimer 끝
			asyncTimer.OnCompleted();
			asyncTimer = null;

			if(_funcAyncLinkCompleted != null)
			{
				//콜백 이벤트 호출
				_funcAyncLinkCompleted(this);
				_funcAyncLinkCompleted = null;
			}


		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void SetFirstInitializeAfterBake()
		{
			_initStatus = INIT_STATUS.Ready;

			if(_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;
		}


		//--------------------------------------------------------------------------------------
		// Editor
		//--------------------------------------------------------------------------------------



		// Get / Set
		//-----------------------------------------------------



		//--------------------------------------------------------------------------------------
		// API
		//--------------------------------------------------------------------------------------
		// Play
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Play the animation
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData Play(string animClipName,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.Play(animClipName, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Play the animation
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData Play(apAnimPlayData animPlayData,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.Play(animPlayData, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}


		


		private apAnimPlayData PlayNoDebug(string animClipName,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.Play(animClipName, layer, blendMethod, playOption, isAutoEndIfNotloop, false);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueued(string animClipName,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}
			
			return _animPlayManager.PlayQueued(animClipName, layer, blendMethod, isAutoEndIfNotloop);
		}



		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueued(apAnimPlayData animPlayData,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}
			
			return _animPlayManager.PlayQueued(animPlayData, layer, blendMethod, isAutoEndIfNotloop);
		}

		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFade(string animClipName,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFade(animClipName, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}



		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFade(apAnimPlayData animPlayData,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFade(animPlayData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueued(string animClipName,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeQueued(animClipName, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueued(apAnimPlayData animPlayData,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeQueued(animPlayData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}
		//----------------------------------------------------------------------------

		//추가 1.14 : 특정 프레임부터 재생을 한다. (Play, PlayQueued, CrossFade, CrossFadeQueued + At)

		//그 외에는 동일
		/// <summary>
		/// Play the animation at the specified frame.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayAt(string animClipName, int frame,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.PlayAt(animClipName, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Play the animation at the specified frame.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayAt(apAnimPlayData animPlayData,
									int frame,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.PlayAt(animPlayData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueuedAt(string animClipName, int frame,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}
			
			return _animPlayManager.PlayQueuedAt(animClipName, frame, layer, blendMethod, isAutoEndIfNotloop);
		}



		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueuedAt(apAnimPlayData animPlayData, int frame,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}
			
			return _animPlayManager.PlayQueuedAt(animPlayData, frame, layer, blendMethod, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Play the animation at the specified frame smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeAt(string animClipName, int frame,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeAt(animClipName, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}



		/// <summary>
		/// Play the animation at the specified frame smoothly.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeAt(apAnimPlayData animPlayData, int frame,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeAt(animPlayData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}

		
		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueuedAt(string animClipName, int frame,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeQueuedAt(animClipName, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueuedAt(apAnimPlayData animPlayData, int frame,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.CrossFadeQueuedAt(animPlayData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}
		//-------------------------------------------------------------

		/// <summary>
		/// End all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		/// <param name="fadeTime">Fade Time</param>
		public void StopLayer(int layer, float fadeTime = 0.0f)
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				return;
			}

			_animPlayManager.StopLayer(layer, fadeTime);
		}

		/// <summary>
		/// End all animations.
		/// </summary>
		/// <param name="fadeTime">Fade Time</param>
		public void StopAll(float fadeTime = 0.0f)
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				return;
			}

			_animPlayManager.StopAll(fadeTime);			
		}
		

		/// <summary>
		/// Pause all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void PauseLayer(int layer)
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return;
			}

			_animPlayManager.PauseLayer(layer);
		}

		/// <summary>
		/// Pause all animations.
		/// </summary>
		public void PauseAll()
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return;
			}

			_animPlayManager.PauseAll();
		}

		/// <summary>
		/// Resume all animations paused on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void ResumeLayer(int layer)
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return;
			}

			_animPlayManager.ResumeLayer(layer);
		}

		/// <summary>
		/// Resume all animations.
		/// </summary>
		public void ResumeAll()
		{
			if (_animPlayManager == null)
			{ return; }

			if(_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return;
			}

			_animPlayManager.ResumeAll();
		}

		/// <summary>
		/// Register the listener object to receive animation events. It must be a class inherited from MonoBehaviour.
		/// </summary>
		/// <param name="listenerObject">Listener</param>
		public void RegistAnimationEventListener(MonoBehaviour listenerObject)
		{
			_optAnimEventListener = listenerObject;
		}

		/// <summary>
		/// Animation PlayManager
		/// </summary>
		public apAnimPlayManager PlayManager
		{
			get
			{
				return _animPlayManager;
			}
		}

		/// <summary>
		/// Is Animation Clip Playing?
		/// </summary>
		/// <param name="animClipName"></param>
		/// <returns></returns>
		public bool IsPlaying(string animClipName)
		{
			return _animPlayManager.IsPlaying(animClipName);
		}


		


		/// <summary>
		/// Sets the speed of the animation.
		/// </summary>
		/// <param name="animClipName"></param>
		/// <param name="speed">Speed of Animation (Default : 1)</param>
		public void SetAnimationSpeed(string animClipName, float speed)
		{
			_animPlayManager.SetAnimSpeed(animClipName, speed);
		}

		/// <summary>
		/// Sets the speed of all animations.
		/// </summary>
		/// <param name="speed">Speed of Animation (Default : 1)</param>
		public void SetAnimationSpeed(float speed)
		{
			_animPlayManager.SetAnimSpeed(speed);
		}

		/// <summary>
		/// Restores the speed of all animations to their default values.
		/// </summary>
		public void ResetAnimationSpeed()
		{
			_animPlayManager.ResetAnimSpeed();
		}

		/// <summary>
		/// Returns a list of AnimPlayData instances with animation information.
		/// </summary>
		public List<apAnimPlayData> AnimationPlayDataList
		{
			get
			{
				return _animPlayManager.PlayDataList;
			}
		}

		/// <summary>
		/// Finds and returns an instance of AnimPlayData with animation information by name.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public apAnimPlayData GetAnimationPlayData(string animClipName)
		{
			return _animPlayManager.GetAnimPlayData_Opt(animClipName);
		}

		/// <summary>
		/// Returns the status in which the animation plays.
		/// Unlike the IsPlaying () function, it returns an Enum type.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public apAnimPlayData.AnimationPlaybackStatus GetAnimationPlaybackStatus(string animClipName)
		{
			return _animPlayManager.GetAnimationPlaybackStatus(animClipName);
		}





		//추가 3.6 : 타임라인 관련 함수들
		// 외부 제어 함수들 - 주로 Timeline
#if UNITY_2017_1_OR_NEWER
		/// <summary>
		/// Connect the Timeline to apPortrait.
		/// Enter the PlayableDirector with timeline and the name of Track.
		/// If the connection is successful, it will play automatically when the Timeline is played.
		/// </summary>
		/// <param name="playableDirector">PlayableDirector with Timeline</param>
		/// <param name="trackName">The name of the track to which apPortrait is connected.</param>
		/// <param name="layer">The index when multiple tracks are blended to play the animation. It starts from 0.</param>
		/// <param name="blendMethod">How multiple tracks are blended</param>
		/// <returns>If the connection fails for some reason, such as a wrong name or no PlaybleDirector, false is returned.</returns>
		public bool AddTimelineTrack(UnityEngine.Playables.PlayableDirector playableDirector, string trackName, int layer = 0, apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive)
		{
			return _animPlayManager.AddTimelineTrack(playableDirector, trackName, layer, blendMethod);
		}

		/// <summary>
		/// Any invalid Timeline track information is removed.
		/// </summary>
		public void RemoveInvalidTimelineTracks()
		{
			_animPlayManager.RemoveInvalidTimelineTracks();
		}

		/// <summary>
		/// All Timeline track data are removed.
		/// </summary>
		public void RemoveAllTimelineTracks()
		{
			_animPlayManager.RemoveAllTimelineTracks();
		}

		/// <summary>
		/// Track information containing the requested PlayableDirector will be deleted.
		/// </summary>
		/// <param name="playableDirector">A PlayableDirector that contains the track information you want to remove.</param>
		public void UnlinkTimelinePlayableDirector(UnityEngine.Playables.PlayableDirector playableDirector)
		{
			_animPlayManager.UnlinkTimelinePlayableDirector(playableDirector);
		}

		/// <summary>
		/// If False is entered, apPortrait is not under the control of the Timeline, even if it is associated with the Timeline.
		/// </summary>
		/// <param name="isEnabled">Whether to be controlled from the Timeline. (Default is True)</param>
		public void SetTimelineEnable(bool isEnabled)
		{
			_animPlayManager.SetTimelineEnable(isEnabled);
		}

#endif
		//---------------------------------------------------------------------------------------
		// 물리 제어
		//---------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes all forces and physical effects by touch.
		/// This function is equivalent to executing "ClearForce()" and "ClearTouch()" together.
		/// </summary>
		public void ClearForceAndTouch()
		{
			_forceManager.ClearAll();
		}

		/// <summary>Initialize all physical forces.</summary>
		public void ClearForce()
		{
			_forceManager.ClearForce();
		}


		/// <summary>
		/// Adds force applied radially at a specific point.
		/// </summary>
		/// <param name="pointPosW">Center position of force in world space</param>
		/// <param name="radius">Radius to which force is applied</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Point(Vector2 pointPosW, float radius)
		{
			return _forceManager.AddForce_Point(pointPosW, radius);
		}

		/// <summary>
		/// Add force with direction.
		/// </summary>
		/// <param name="directionW">Direction vector</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Direction(Vector2 directionW)
		{
			return _forceManager.AddForce_Direction(directionW);
		}

		/// <summary>
		/// Add a force that changes direction periodically.
		/// </summary>
		/// <param name="directionW">Direction vector</param>
		/// <param name="waveSizeX">How much the direction changes on the X axis</param>
		/// <param name="waveSizeY">How much the direction changes on the Y axis</param>
		/// <param name="waveTimeX">The time the force changes on the X axis</param>
		/// <param name="waveTimeY">The time the force changes on the Y axis</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Direction(Vector2 directionW, float waveSizeX, float waveSizeY, float waveTimeX, float waveTimeY)
		{
			return _forceManager.AddForce_Direction(directionW, new Vector2(waveSizeX, waveSizeY), new Vector2(waveTimeX, waveTimeY));
		}

		/// <summary>
		/// Is any force being applied?
		/// </summary>
		public bool IsAnyForceEvent
		{
			get { return _forceManager.IsAnyForceEvent; }
		}

		/// <summary>
		/// The force applied at the requested position is calculated
		/// </summary>
		/// <param name="targetPosW">Position in world space</param>
		/// <returns>Calculated Force</returns>
		public Vector2 GetForce(Vector2 targetPosW)
		{
			return _forceManager.GetForce(targetPosW);
		}

		/// <summary>
		/// Add a physics effect to pull meshes using the touch.
		/// </summary>
		/// <param name="posW">First touch position in world space</param>
		/// <param name="radius">Radius of pulling force</param>
		/// <returns>Added touch information with "TouchID"</returns>
		public apPullTouch AddTouch(Vector2 posW, float radius)
		{
			return _forceManager.AddTouch(posW, radius);
		}

		/// <summary>Initialize all physical forces by touch.</summary>
		public void ClearTouch()
		{
			_forceManager.ClearTouch();
		}

		/// <summary>
		/// Removes physical effects by touch with the requested ID.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		public void RemoveTouch(int touchID)
		{
			_forceManager.RemoveTouch(touchID);
		}

		/// <summary>
		/// Returns a physical effect by touch with the requested ID.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		/// <returns>Requested touch information (return null if touchID is not valid)</returns>
		public apPullTouch GetTouch(int touchID)
		{
			return _forceManager.GetTouch(touchID);
		}

		/// <summary>
		/// Update the position of the added touch.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		/// <param name="posW">World Position</param>
		public void SetTouchPosition(int touchID, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touchID, posW);
		}

		/// <summary>
		/// Update the position of the added touch.
		/// </summary>
		/// <param name="touch">Added Touch information</param>
		/// <param name="posW">World Position</param>
		public void SetTouchPosition(apPullTouch touch, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touch, posW);
		}

		/// <summary>
		/// Is any force by touch being applied?
		/// </summary>
		public bool IsAnyTouchEvent { get { return _forceManager.IsAnyTouchEvent; } }

		/// <summary>
		/// [Please do not use it] Temporary code used for touch calculations
		/// </summary>
		public int TouchProcessCode { get { return _forceManager.TouchProcessCode; } }



		//--------------------------------------------------------------------------------------
		// Control Param 제어 요청
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Set the value of the Control Parameter with an "Integer" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="intValue">Integer Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamInt(string controlParamName, int intValue, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueInt(intValue, overlapWeight);

			//controlParam._int_Cur = intValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			//}

			return true;
		}
		
		/// <summary>
		/// Set the value of the Control Parameter with an "Float" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="floatValue">Float Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamFloat(string controlParamName, float floatValue, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueFloat(floatValue, overlapWeight);

			//controlParam._float_Cur = floatValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			//}

			return true;
		}

		/// <summary>
		/// Set the value of the Control Parameter with an "Vector2" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="vec2Value">Vector2 Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamVector2(string controlParamName, Vector2 vec2Value, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueVector2(vec2Value, overlapWeight);

			//controlParam._vec2_Cur = vec2Value;
			////if(controlParam._isRange)
			//{
			//	controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
			//	controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			//}

			return true;
		}


		/// <summary>
		/// Set the value of the Control Parameter with an "Integer" value.
		/// </summary>
		/// <param name="controlParam">Tareget Control Parameter</param>
		/// <param name="intValue">Integer Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamInt(apControlParam controlParam, int intValue, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueInt(intValue, overlapWeight);

			//controlParam._int_Cur = intValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			//}

			return true;
		}

		/// <summary>
		/// Set the value of the Control Parameter with an "Float" value.
		/// </summary>
		/// <param name="controlParam">Target Control Parameter</param>
		/// <param name="floatValue">Float Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamFloat(apControlParam controlParam, float floatValue, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueFloat(floatValue, overlapWeight);

			//controlParam._float_Cur = floatValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			//}

			return true;
		}


		/// <summary>
		/// Set the value of the Control Parameter with an "Vector2" value.
		/// </summary>
		/// <param name="controlParam">Target Control Parameter</param>
		/// <param name="vec2Value">Vector2 Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamVector2(apControlParam controlParam, Vector2 vec2Value, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueVector2(vec2Value, overlapWeight);

			//controlParam._vec2_Cur = vec2Value;
			////if(controlParam._isRange)
			//{
			//	controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
			//	controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			//}

			return true;
		}


		/// <summary>
		/// Is there a parameter with the requested name?
		/// </summary>
		/// <param name="controlParamName">Name of Control Parameter</param>
		/// <returns></returns>
		public bool IsControlParamExist(string controlParamName)
		{
			return GetControlParam(controlParamName) != null;
		}

		/// <summary>
		/// Restores the value of the parameter to its default value.
		/// </summary>
		/// <param name="controlParamName">Name of Control Parameter</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamDefaultValue(string controlParamName)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					controlParam._int_Cur = controlParam._int_Def;
					break;

				case apControlParam.TYPE.Float:
					controlParam._float_Cur = controlParam._float_Def;
					break;

				case apControlParam.TYPE.Vector2:
					controlParam._vec2_Cur = controlParam._vec2_Def;
					break;
			}

			return true;
		}
		

		//--------------------------------------------------------------------------------------------------
		// Bone Transform 요청 (Rotation, Scale, Position-IK, LookAt)
		// 요청된 Bone을 검색하는 기능도 추가하고, 한번 검색된 Bone은 별도의 리스트로 넣어서 관리하자
		//--------------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the Bone with the requested name.
		/// (It first searches for the currently executing Root Unit, and returns the bones retrieved from all Root Units.)
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <returns>Optimized Bone</returns>
		public apOptBone GetBone(string boneName)
		{
			//일단 "현재 재생중인 RootUnit"에서 검색하고,
			//그 다음에 "전체 목록"에서 검색한다.
			apOptBone resultBone = null;
			if(_curPlayingOptRootUnit != null)
			{
				resultBone = _curPlayingOptRootUnit.GetBone(boneName);

				if(resultBone != null)
				{
					return resultBone;
				}
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				resultBone = _optRootUnitList[i].GetBone(boneName);
				if(resultBone != null)
				{
					return resultBone;
				}
			}

			return null;

		}

		/// <summary>
		/// Finds the Bone with the requested name in a specific Root Unit and returns it.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <returns>Optimized Bone</returns>
		public apOptBone GetBone(int rootUnitIndex, string boneName)
		{
			if(rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				Debug.LogError("AnyPortrait : GetBone() Failed. The index is out of the list range. [" + rootUnitIndex + " / " + _optRootUnitList.Count + "]");
				return null;
			}

			return _optRootUnitList[rootUnitIndex].GetBone(boneName);
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <returns></returns>
		public Transform GetBoneSocket(apOptBone optBone)
		{
			if(optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <returns></returns>
		public Transform GetBoneSocket(string boneName)
		{
			apOptBone optBone = GetBone(boneName);
			if(optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <returns></returns>
		public Transform GetBoneSocket(int rootUnitIndex, string boneName)
		{
			apOptBone optBone = GetBone(rootUnitIndex, boneName);
			if(optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }

			if(space == Space.World) { position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position); }
			optBone.SetPosition(position, weight);
			return true;
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(apOptBone optBone, float angle, Space space, float weight = 1.0f)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }

			if(space == Space.World) { angle -= optBone._parentOptTransform._rootUnit._transform.rotation.eulerAngles.z; }
			angle -= 90.0f;
			angle = apUtil.AngleTo180(angle);

			optBone.SetRotation(angle, weight);
			return true;
		}


		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(apOptBone optBone, Vector3 scale, Space space, float weight = 1.0f)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }

			if(space == Space.World)
			{
				scale.x /= optBone._parentOptTransform._rootUnit._transform.lossyScale.x;
				scale.y /= optBone._parentOptTransform._rootUnit._transform.lossyScale.y;
			}
			optBone.SetScale(scale, weight);
			return true;
		}

		// Overloads

		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBonePosition(GetBone(boneName), position, space, weight);
		}


		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBonePosition(GetBone(rootUnitIndex, boneName), position, space, weight);
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(string boneName, float angle, Space space, float weight = 1.0f)
		{
			return SetBoneRotation(GetBone(boneName), angle, space, weight);
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(int rootUnitIndex, string boneName, float angle, Space space, float weight = 1.0f)
		{
			return SetBoneRotation(GetBone(rootUnitIndex, boneName), angle, space, weight);
		}


		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(string boneName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetBoneScale(GetBone(boneName), scale, space, weight);
		}

		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(int rootUnitIndex, string boneName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetBoneScale(GetBone(rootUnitIndex, boneName), scale, space, weight);
		}

		// Bone IK

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			if(optBone == null)
			{
				Debug.LogError("AnyPortrait : No Opt Bone");
				return false;
			}
			if(optBone._parentOptTransform == null)
			{
				Debug.LogError("AnyPortrait : No Opt Transform");
				return false;
			}
			if(optBone._parentOptTransform._rootUnit == null)
			{
				Debug.LogError("AnyPortrait : No Opt Root Unit");
				return false;
			}

			if(space == Space.World)
			{
				position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position);
			}

			return optBone.RequestIK(position, Mathf.Clamp01(weight), isContinuous);
		}

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(string boneName, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			return SetBoneIK(GetBone(boneName), position, space, weight, isContinuous);
		}

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			return SetBoneIK(GetBone(rootUnitIndex, boneName), position, space, weight, isContinuous);
		}


		// Bone IK

		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }
			
			if(space == Space.World) { position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position); }

			

			

			//이전
			//float angle = Mathf.Atan2(position.y - optBone.PositionWithouEditing.y, position.x - optBone.PositionWithouEditing.x) * Mathf.Rad2Deg;
			//angle -= optBone._defaultMatrix._angleDeg;

			//변경
			//IKSpace로 옮겨야 한다.
			Vector2 targetPos = optBone._worldMatrix.ConvertForIK(position);
			Vector2 startPos = optBone._worldMatrix.ConvertForIK(optBone.PositionWithouEditing);
			//Vector2 startPos = optBone.PositionWithouEditing;

			float angle = Mathf.Atan2(targetPos.y - startPos.y, targetPos.x - startPos.x) * Mathf.Rad2Deg;

			//if (optBone._worldMatrix.Scale.y < 0.0f)
			//{
			//	angle += 90.0f;
			//}
			//else
			//{
			//	angle -= 90.0f;
			//}
			//angle -= optBone._defaultMatrix._angleDeg;
			//angle -= 90.0f;
			//angle = apUtil.AngleTo180(angle);

			//Debug.LogError("SetBoneLookAt [" + optBone._name + "] ");
			//Debug.Log("- World : " + optBone.PositionWithouEditing + " > " + position);
			//Debug.Log("- IKSpace : " + startPos + " > " + targetPos);
			//Debug.Log("- Result Angle " + angle + "( Before " + optBone._worldMatrix.Angle_IKSpace + ")");


			//optBone.SetRotation(angle, weight);//TODO : Rotation이 아니라 IK Angle을 직접 건드려야 한다.
			//optBone.AddIKAngle(apUtil.AngleTo180(angle - optBone._worldMatrix.Angle), 0.0f, weight);
			optBone.AddIKAngle(apUtil.AngleTo180(angle), 0.0f, weight);
			return true;
		}


		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBoneLookAt(GetBone(boneName), position, space, weight);
		}


		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBoneLookAt(GetBone(rootUnitIndex, boneName), position, space, weight);
		}


		//1.0.4 Functions
		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(apOptBone optBone, float positionValue, ConstraintBound constraintBound, Space space)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }

			if(space == Space.World)
			{
				Vector3 position = optBone._parentOptTransform._rootUnit._transform.position;
				switch (constraintBound)
				{
					case ConstraintBound.Xprefer:
					case ConstraintBound.Xmin:
					case ConstraintBound.Xmax:
						position.x = positionValue;
						break;

					case ConstraintBound.Yprefer:
					case ConstraintBound.Ymin:
					case ConstraintBound.Ymax:
						position.y = positionValue;
						break;
				}
				position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position);
				switch (constraintBound)
				{
					case ConstraintBound.Xprefer:
					case ConstraintBound.Xmin:
					case ConstraintBound.Xmax:
						positionValue = position.x;
						break;

					case ConstraintBound.Yprefer:
					case ConstraintBound.Ymin:
					case ConstraintBound.Ymax:
						positionValue = position.y;
						break;
				}
			}
			optBone.SetPositionConstraint(positionValue, constraintBound);
			return true;
		}

		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(string boneName, float positionValue, ConstraintBound constraintBound, Space space)
		{
			return SetBonePositionConstraint(GetBone(boneName), positionValue, constraintBound, space);
		}

		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(int rootUnitIndex, string boneName, float positionValue, ConstraintBound constraintBound, Space space)
		{
			return SetBonePositionConstraint(GetBone(rootUnitIndex, boneName), positionValue, constraintBound, space);
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(	apOptBone optBone, 
														float defaultSurfacePosition, 
														float currentSurfacePosition, 
														float minSurfacePosition, 
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			if(optBone == null) { return false; }
			if(optBone._parentOptTransform == null) { return false; }
			if(optBone._parentOptTransform._rootUnit == null) { return false; }

			if(space == Space.World)
			{
				Vector3 defaultPos = optBone._parentOptTransform._rootUnit._transform.position;
				Vector3 currentPos = defaultPos;
				Vector3 minPos = defaultPos;
				Vector3 maxPos = defaultPos;
				switch (constraintSurface)
				{
					case ConstraintSurface.Xsurface:
						defaultPos.x = defaultSurfacePosition;
						currentPos.x = currentSurfacePosition;
						minPos.x = minSurfacePosition;
						maxPos.x = maxSurfacePosition;
						break;

					case ConstraintSurface.Ysurface:
						defaultPos.y = defaultSurfacePosition;
						currentPos.y = currentSurfacePosition;
						minPos.y = minSurfacePosition;
						maxPos.y = maxSurfacePosition;
						break;
				}
				defaultPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(defaultPos);
				currentPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(currentPos);
				minPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(minPos);
				maxPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(maxPos);

				switch (constraintSurface)
				{
					case ConstraintSurface.Xsurface:
						defaultSurfacePosition = defaultPos.x;
						currentSurfacePosition = currentPos.x;
						minSurfacePosition = minPos.x;
						maxSurfacePosition = maxPos.x;
						break;

					case ConstraintSurface.Ysurface:
						defaultSurfacePosition = defaultPos.y;
						currentSurfacePosition = currentPos.y;
						minSurfacePosition = minPos.y;
						maxSurfacePosition = maxPos.y;
						break;
				}
			}
			optBone.SetPositionConstraintSurface(defaultSurfacePosition, currentSurfacePosition, minSurfacePosition, maxSurfacePosition, constraintSurface);
			return true;
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(	string boneName, 
														float defaultSurfacePosition, 
														float currentSurfacePosition, 
														float minSurfacePosition, 
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			return SetBonePositionConstraintBySurface(	GetBone(boneName), 
														defaultSurfacePosition, 
														currentSurfacePosition, 
														minSurfacePosition, 
														maxSurfacePosition,
														constraintSurface, space);
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// /// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(	int rootUnitIndex,
														string boneName, 
														float defaultSurfacePosition, 
														float currentSurfacePosition, 
														float minSurfacePosition, 
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			return SetBonePositionConstraintBySurface(	GetBone(rootUnitIndex, boneName), 
														defaultSurfacePosition, 
														currentSurfacePosition, 
														minSurfacePosition, 
														maxSurfacePosition,
														constraintSurface, space);
		}
		//-------------------------------------------------------------------------------------------------------
		// OptTransform에 대한 참조/제어
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Find the Optimized Transform and return it.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(int rootUnitIndex, string transformName)
		{
			if(rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				Debug.LogError("AnyPortrait : GetOptTransform() Failed. The index is out of the list range. [" + rootUnitIndex + " / " + _optRootUnitList.Count + "]");
				return null;
			}

			return _optRootUnitList[rootUnitIndex].GetTransform(transformName);
		}

		/// <summary>
		/// Find the Optimized Transform and return it.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(string transformName)
		{
			//일단 "현재 재생중인 RootUnit"에서 검색하고,
			//그 다음에 "전체 목록"에서 검색한다.
			apOptTransform resultTransform = null;
			if(_curPlayingOptRootUnit != null)
			{
				resultTransform = _curPlayingOptRootUnit.GetTransform(transformName);

				if(resultTransform != null)
				{
					return resultTransform;
				}
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				resultTransform = _optRootUnitList[i].GetTransform(transformName);
				if(resultTransform != null)
				{
					return resultTransform;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(apOptTransform optTransform)
		{
			if(optTransform == null) { return null; }
			return optTransform._socketTransform;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(string transformName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null) { return null; }
			return optTransform._socketTransform;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(int rootUnitIndex, string transformName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null) { return null; }
			return optTransform._socketTransform;
		}


		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(apOptTransform optTransform, Vector3 position, Space space, float weight = 1.0f)
		{
			if(optTransform == null) { return false; }
			if(optTransform._rootUnit == null) { return false; }
			
			if(space == Space.World) { position = optTransform._rootUnit._transform.InverseTransformPoint(position); }
			optTransform.SetPosition(position, weight);
			return true;
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(apOptTransform optTransform, float angle, Space space, float weight = 1.0f)
		{
			if(optTransform == null) { return false; }
			if(optTransform._rootUnit == null) { return false; }

			if(space == Space.World) { angle -= optTransform._rootUnit._transform.rotation.eulerAngles.z; }
			angle -= 90.0f;
			angle = apUtil.AngleTo180(angle);

			optTransform.SetRotation(angle, weight);
			return true;
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(apOptTransform optTransform, Vector3 scale, Space space, float weight = 1.0f)
		{
			if(optTransform == null) { return false; }
			if(optTransform._rootUnit == null) { return false; }

			if(space == Space.World)
			{
				scale.x /= optTransform._rootUnit._transform.lossyScale.x;
				scale.y /= optTransform._rootUnit._transform.lossyScale.y;
			}
			optTransform.SetScale(scale, weight);
			return true;
		}

		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(int rootUnitIndex, string transformName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetOptTransformPosition(GetOptTransform(rootUnitIndex, transformName), position, space, weight);
		}

		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(string transformName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetOptTransformPosition(GetOptTransform(transformName), position, space, weight);
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(int rootUnitIndex, string transformName, float angle, Space space, float weight = 1.0f)
		{
			return SetOptTransformRotation(GetOptTransform(rootUnitIndex, transformName), angle, space, weight);
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(string transformName, float angle, Space space, float weight = 1.0f)
		{
			return SetOptTransformRotation(GetOptTransform(transformName), angle, space, weight);
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(int rootUnitIndex, string transformName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetOptTransformScale(GetOptTransform(rootUnitIndex, transformName), scale, space, weight);
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(string transformName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetOptTransformScale(GetOptTransform(transformName), scale, space, weight);
		}



		//-------------------------------------------------------------------------------------------------------
		// 텍스쳐 교체. 
		// Opt Transform 하나만 바꾸거나
		// 전체 Atlas를 교체한다.
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Find and return the texture applied to Opt-Meshes.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <returns></returns>
		public apOptTextureData GetOptTextureData(string optTextureName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0)
			{
				return null;
			}

			return _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});
		}


		//추가 : Batched Material 속성 초기화 (다시 Shared로 유도하기 위해)
		/// <summary>
		/// Initializes the properties of the all materials in a batch.
		/// </summary>
		public void ResetMeshMaterialToBatchAll()
		{
			_optBatchedMaterial.ResetAllProperties();
		}

		//추가 : Batched Material 속성 초기화 (다시 Shared로 유도하기 위해)
		/// <summary>
		/// Initializes the properties of the material in a batch.
		/// </summary>
		/// <param name="optTextureName">Target Texture Name (same as "Image" Name of AnyPortrait Editor)</param>
		public void ResetMeshMaterialToBatchByTextureName(string optTextureName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }

			_optBatchedMaterial.ResetProperties(targetTextureData._textureID);
		}

		
		/// <summary>
		/// Replaces the main texture applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImageAll(string optTextureName, Texture2D texture)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			
			//targetTextureData.SetMeshTextureAll(texture);//<<이전 : 이 함수를 이용하면 모두 Instanced Material이 된다.

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshImageAll(targetTextureData._textureID, texture);
		}

		/// <summary>
		/// Change the texture of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImageAll(string optTextureName, Texture2D texture, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomImageAll(texture, propertyName);//<<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomImageAll(targetTextureData._textureID, texture, propertyName);
		}

		/// <summary>
		/// Replace the main color applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColorAll(string optTextureName, Color color2X)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetMeshColorAll(color2X);//<<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshColorAll(targetTextureData._textureID, color2X);
		}

		/// <summary>
		/// Replace the main color's alpha applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		public void SetMeshAlphaAll(string optTextureName, float alpha)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetMeshAlphaAll(alpha);//<<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshAlphaAll(targetTextureData._textureID, alpha);

		}


		/// <summary>
		/// Change the color of the shader in a batch.
		/// </summary>
		/// <param name="targetTextureName">Opt-Texture Name</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColorAll(string optTextureName, Color color, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomColorAll(color, propertyName);//<<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomColorAll(targetTextureData._textureID, color, propertyName);
		}

		/// <summary>
		/// Change the color's alpha of the shader in a batch.
		/// </summary>
		/// <param name="targetTextureName">Opt-Texture Name</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlphaAll(string optTextureName, float alpha, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomAlphaAll(alpha, propertyName);//이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomAlphaAll(targetTextureData._textureID, alpha, propertyName);
		}


		/// <summary>
		/// Change the float property of the shader in a batch.
		/// </summary>
		/// <param name="targetTextureName">Opt-Texture Name</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloatAll(string optTextureName, float floatValue, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomFloatAll(floatValue, propertyName);//<<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomFloatAll(targetTextureData._textureID, floatValue, propertyName);
		}


		/// <summary>
		/// Change the int property of the shader in a batch.
		/// </summary>
		/// <param name="targetTextureName">Opt-Texture Name</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomIntAll(string optTextureName, int intValue, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomIntAll(intValue, propertyName);//이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomIntAll(targetTextureData._textureID, intValue, propertyName);
		}


		/// <summary>
		/// Change the Vector4 property of the shader in a batch.
		/// </summary>
		/// <param name="targetTextureName">Opt-Texture Name</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4All(string optTextureName, Vector4 vector4Value, string propertyName)
		{
			if(_optTextureData == null || _optTextureData.Count == 0) { return; }
			apOptTextureData targetTextureData = _optTextureData.Find(delegate (apOptTextureData a)
			{
				return string.Equals(a._name, optTextureName);
			});

			if(targetTextureData == null) { return; }
			//targetTextureData.SetCustomVector4All(vector4Value, propertyName);//<이전

			//변경 12.13 : Batch Material쪽으로 변경
			_optBatchedMaterial.SetMeshCustomVector4All(targetTextureData._textureID, vector4Value, propertyName);
		}




#region [미사용 코드 : Material은 자동 Batch때문에 참조할 수 없다]
		//public Material GetOptTransformMaterial(apOptTransform optTransform)
		//{
		//	if(optTransform == null) { return null; }

		//	if(optTransform._childMesh == null) { return null; }

		//	return optTransform._childMesh._material;
		//}

		//public Material GetOptTransformMaterial(string transformName)
		//{
		//	apOptTransform optTransform = GetOptTransform(transformName);
		//	if(optTransform == null) { return null; }

		//	if(optTransform._childMesh == null) { return null; }

		//	return optTransform._childMesh._material;
		//}


		//public Material GetOptTransformMaterial(int portraitIndex, string transformName)
		//{
		//	apOptTransform optTransform = GetOptTransform(portraitIndex, transformName);
		//	if(optTransform == null) { return null; }

		//	if(optTransform._childMesh == null) { return null; }

		//	return optTransform._childMesh._material;
		//} 
#endregion

		//Mesh Material을 초기화하여 Batch가 되도록 만든다.
		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		public void ResetMeshMaterialToBatch(apOptTransform optTransform)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.ResetMaterialToBatch();
		}

		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="transformName">Opt-Transform Name</param>
		public void ResetMeshMaterialToBatch(string transformName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.ResetMaterialToBatch();
		}

		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		public void ResetMeshMaterialToBatch(int rootUnitIndex, string transformName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.ResetMaterialToBatch();
		}

		/// <summary>
		/// Initialize all Command Buffers for clipping mask processing.
		/// </summary>
		public void CleanUpMeshesCommandBuffers()
		{
			for (int i = 0; i < _optMeshes.Count; i++)
			{
				//_optMeshes[i].CleanUpMaskParent();//이전
				_optMeshes[i].ClearCameraData();//변경
			}
		}

		/// <summary>
		/// Initialize or re-register all Command Buffers for clipping mask processing.
		/// </summary>
		/// <param name="isOnlyActiveRootUnit">If True, all Command Buffers of Root Units except the currently executing are initialized. If false, re-register the buffers of all Root Units.</param>
		public void ResetMeshesCommandBuffers(bool isOnlyActiveRootUnit)
		{
			if (isOnlyActiveRootUnit)
			{
				//RootUnit 단위로 Reset을 한다.
				for (int i = 0; i < _optRootUnitList.Count; i++)
				{
					ResetMeshCommandBuffer(_optRootUnitList[i], _optRootUnitList[i] == _curPlayingOptRootUnit);
				}
			}
			else
			{
				//전부다 체크
				for (int i = 0; i < _optMeshes.Count; i++)
				{
					_optMeshes[i].ResetMaskParentSetting();
				}
			}
			
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(apOptTransform optTransform, Texture2D texture)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshTexture(texture);
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(string transformName, Texture2D texture)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshTexture(texture);
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(int rootUnitIndex, string transformName, Texture2D texture)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshTexture(texture);
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(apOptTransform optTransform, Color color2X)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshColor(color2X);
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(string transformName, Color color2X)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshColor(color2X);
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(int rootUnitIndex, string transformName, Color color2X)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null)
			{
				return;
			}
			optTransform._childMesh.SetMeshColor(color2X);
		}


		//Set Mesh Alpha 추가됨

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(apOptTransform optTransform, float alpha)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshAlpha(alpha);
		}

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(string transformName, float alpha)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetMeshAlpha(alpha);
		}

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(int rootUnitIndex, string transformName, float alpha)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null)
			{
				return;
			}
			optTransform._childMesh.SetMeshAlpha(alpha);
		}


		//추가 4.13 : 등록된 Image를 이용하는 방법 (제안 감사합니다.)

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(apOptTransform optTransform, string imageName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			apOptTextureData optTextureData = GetOptTextureData(imageName);
			if(optTextureData == null)
			{
				Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
				return;
			}
			optTransform._childMesh.SetMeshTexture(optTextureData._texture);
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(string transformName, string imageName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			apOptTextureData optTextureData = GetOptTextureData(imageName);
			if(optTextureData == null)
			{
				Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
				return;
			}
			optTransform._childMesh.SetMeshTexture(optTextureData._texture);
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(int rootUnitIndex, string transformName, string imageName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			apOptTextureData optTextureData = GetOptTextureData(imageName);
			if(optTextureData == null)
			{
				Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
				return;
			}
			optTransform._childMesh.SetMeshTexture(optTextureData._texture);
		}






		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImage(apOptTransform optTransform, Texture2D texture, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomTexture(texture, propertyName);
		}

		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColor(apOptTransform optTransform, Color color, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomColor(color, propertyName);
		}


		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlpha(apOptTransform optTransform, float alpha, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomAlpha(alpha, propertyName);
		}


		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloat(apOptTransform optTransform, float floatValue, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomFloat(floatValue, propertyName);
		}

		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomInt(apOptTransform optTransform, int intValue, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomInt(intValue, propertyName);
		}


		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4(apOptTransform optTransform, Vector4 vector4Value, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomVector4(vector4Value, propertyName);
		}


		//추가 12.02
		/// <summary>
		/// Set the TextureOffset property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureOffsetValue">TextureOffset Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureOffset(apOptTransform optTransform, Vector2 textureOffsetValue, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomTextureOffset(textureOffsetValue, propertyName);
		}


		/// <summary>
		/// Set the TextureScale property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureScaleValue">TextureScale Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureScale(apOptTransform optTransform, Vector2 textureScaleValue, string propertyName)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetCustomTextureScale(textureScaleValue, propertyName);
		}





		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(apOptTransform optTransform)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(false);
		}

		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(string transformName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(false);
		}


		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(int rootUnitIndex, string transformName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(false);
		}
		
		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(apOptTransform optTransform)
		{
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(true);
		}

		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(string transformName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(true);
		}


		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(int rootUnitIndex, string transformName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if(optTransform == null || optTransform._childMesh == null) { return; }
			optTransform._childMesh.SetHideForce(true);
		}

		/// <summary>
		/// Hide Portrait (All Root Units)
		/// </summary>
		public void Hide()
		{
			HideRootUnits();
		}

		/// <summary>
		/// Show Portrait (A default Root Unit)
		/// </summary>
		public void Show()
		{
			ShowRootUnit();
		}

		/// <summary>
		/// Set alpha for all meshes (main color)
		/// </summary>
		/// <param name="alpha"></param>
		public void SetMeshAlphaAll(float alpha)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetMeshAlpha(alpha);
					}
				}
			}
		}

		/// <summary>
		/// Set alpha for all meshes (custom property)
		/// </summary>
		/// <param name="alpha"></param>
		/// <param name="propertyName"></param>
		public void SetMeshCustomAlphaAll(float alpha, string propertyName)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetCustomAlpha(alpha, propertyName);
					}
				}
			}
		}

		/// <summary>
		/// Set color for all meshes (main color 2x)
		/// </summary>
		/// <param name="color2X"></param>
		public void SetMeshColorAll(Color color2X)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetMeshColor(color2X);
					}
				}
			}
		}

		/// <summary>
		/// Set color for all meshes (custom property)
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetMeshCustomColorAll(Color color, string propertyName)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetCustomColor(color, propertyName);
					}
				}
			}
		}

		/// <summary>
		/// Set Float Property of shader for all meshes
		/// </summary>
		/// <param name="floatValue"></param>
		/// <param name="propertyName"></param>
		public void SetMeshCustomFloatAll(float floatValue, string propertyName)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetCustomFloat(floatValue, propertyName);
					}
				}
			}
		}

		/// <summary>
		/// Set Int Property of shader for all meshes
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="propertyName"></param>
		public void SetMeshCustomIntAll(int intValue, string propertyName)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetCustomInt(intValue, propertyName);
					}
				}
			}
		}

		/// <summary>
		/// Set Vector4 Property of shader for all meshes
		/// </summary>
		/// <param name="vector4Value"></param>
		/// <param name="propertyName"></param>
		public void SetMeshCustomVector4All(Vector4 vector4Value, string propertyName)
		{
			apOptRootUnit optRootUnit;
			List<apOptTransform> optTransforms = null;
			for (int iRU = 0; iRU < _optRootUnitList.Count; iRU++)
			{
				optRootUnit = _optRootUnitList[iRU];
				optTransforms = optRootUnit.OptTransforms;
				for (int i = 0; i < optTransforms.Count; i++)
				{
					if(optTransforms[i]._childMesh != null)
					{
						optTransforms[i]._childMesh.SetCustomVector4(vector4Value, propertyName);
					}
				}
			}
		}
		





		//Added 3.22 (1.0.2)
		//Sorting Order Features
		/// <summary>
		/// Set the Sorting Layer.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(string sortingLayerName)
		{
			//이름으로부터 SortingLayerID를 찾자
			if(SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. There is no SortingLayer is this project.");
				return;
			}
			int targetSortingLayerID = -1;
			bool isTargetSortingLayerFound = false;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if(string.Equals(SortingLayer.layers[i].name, sortingLayerName))
				{
					isTargetSortingLayerFound = true;
					targetSortingLayerID = SortingLayer.layers[i].id;
					break;
				}
			}
			//못찾았다.
			if(!isTargetSortingLayerFound)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. Could not find layer with requested name. <" + sortingLayerName + ">");
				return;
			}

			//Sorting Layer 적용
			_sortingLayerID = targetSortingLayerID;
			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].SetSortingLayer(sortingLayerName, _sortingLayerID);
			}
		}

		/// <summary>
		/// Set the Sorting Order
		/// </summary>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(int sortingOrder)
		{
			_sortingOrder = sortingOrder;
			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].SetSortingOrder(sortingOrder);
			}
		}




		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(apOptTransform optTransform, string sortingLayerName)
		{
			//이름으로부터 SortingLayerID를 찾자
			if(SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. There is no SortingLayer is this project.");
				return;
			}
			if(optTransform == null || optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. OptTransform is null or it does not have a mesh.");
				return;
			}
			int targetSortingLayerID = -1;
			bool isTargetSortingLayerFound = false;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if(string.Equals(SortingLayer.layers[i].name, sortingLayerName))
				{
					isTargetSortingLayerFound = true;
					targetSortingLayerID = SortingLayer.layers[i].id;
					break;
				}
			}
			//못찾았다.
			if(!isTargetSortingLayerFound)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. Could not find layer with requested name. <" + sortingLayerName + ">");
				return;
			}

			//Sorting Layer 적용
			optTransform._childMesh.SetSortingLayer(sortingLayerName, targetSortingLayerID);
		}
		
		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(string transformName, string sortingLayerName)
		{
			SetSortingLayer(GetOptTransform(transformName), sortingLayerName);
		}

		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(int rootUnitIndex, string transformName, string sortingLayerName)
		{
			SetSortingLayer(GetOptTransform(rootUnitIndex, transformName), sortingLayerName);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(apOptTransform optTransform, int sortingOrder)
		{
			if(optTransform == null || optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : SetSortingOrder() Failed. OptTransform is null or it does not have a mesh.");
				return;
			}

			_sortingOrder = sortingOrder;
			optTransform._childMesh.SetSortingOrder(sortingOrder);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(string transformName, int sortingOrder)
		{
			SetSortingOrder(GetOptTransform(transformName), sortingOrder);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(int rootUnitIndex, string transformName, int sortingOrder)
		{
			SetSortingOrder(GetOptTransform(rootUnitIndex, transformName), sortingOrder);
		}


		/// <summary>
		/// Get Name of Sorting Layer
		/// If Failed, "Unknown Layer" is returned
		/// </summary>
		/// <returns></returns>
		public string GetSortingLayerName()
		{
			if(SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				return "Unknown Layer";
			}

			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if(SortingLayer.layers[i].id == _sortingLayerID)
				{
					return SortingLayer.layers[i].name;
				}
			}

			return "Unknown Layer";
		}

		/// <summary>
		/// Get Sorting Order
		/// </summary>
		/// <returns></returns>
		public int GetSortingOrder()
		{
			return _sortingOrder;
		}


		//추가 21.1.31
		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(apOptTransform optTransform)
		{
			if(optTransform == null)
			{
				Debug.LogError("AnyPortrait : GetSortingOrder() Failed. The OptTransform entered as an argument is null.");
				return -1;
			}
			if(optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : GetSortingOrder() Failed. The requested OptTransform does not have a mesh.");
				return -1;
			}
			return optTransform._childMesh.GetSortingOrder();
		}
		
		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(string transformName)
		{
			return GetSortingOrder(GetOptTransform(transformName));
		}

		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(int rootUnitIndex, string transformName)
		{
			return GetSortingOrder(GetOptTransform(rootUnitIndex, transformName));
		}



		

		//추가 19.8.19
		//Sorting Order Option에 관련된 함수들 추가
		/// <summary>
		/// Set whether the [Sorting Order] of the meshes will be changed automatically.
		/// This function works only when the value of [Sorting Order Option] of apPortrait is [Depth To Order] or [Reverse Depth To Order].
		/// </summary>
		/// <param name="isEnabled">Whether the sorting order is automatically updated (Default is true)</param>
		public void SetSortingOrderChangedAutomatically(bool isEnabled)
		{
			if(_optRootUnitList == null)
			{
				return;
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].SetSortingOrderChangedAutomatically(isEnabled);
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshSortingOrderByDepth()
		{
			if(_optRootUnitList == null)
			{
				return;
			}
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].RefreshSortingOrderByDepth();
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ApplySortingOptionToOptRootUnits()
		{
			if(_optRootUnitList == null)
			{
				return;
			}
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].SetSortingOrderOption(_sortingOrderOption, _sortingOrderPerDepth);
			}
		}
#endif

		//-------------------------------------------------------------------------------------------------------
		// 업데이트 관련 처리
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// If the "Important" setting is True, the physics effect and animation are activated and updated every frame.
		/// </summary>
		/// <param name="isImportant"></param>
		public void SetImportant(bool isImportant)
		{
			if (_isImportant != isImportant)
			{
				_isImportant = isImportant;
			}
		}

		/// <summary>
		/// If the "Important" option is off, the character is executed according to the specified FPS or lower.
		/// </summary>
		/// <param name="fps"></param>
		public void SetFPSForNotImportant(int fps)
		{
			_FPS = fps;
		}


		// 초기화
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ReadyToEdit()
		{
			//이전
			#region [미사용 코드]
			////ID 리스트 일단 리셋
			//ClearRegisteredUniqueIDs();

			////컨트롤 / 컨트롤 파라미터 리셋
			//_controller.Ready(this);
			//_controller.SetDefaultAll();


			//for (int iTexture = 0; iTexture < _textureData.Count; iTexture++)
			//{
			//	_textureData[iTexture].ReadyToEdit(this);
			//}

			//_meshes.RemoveAll(delegate(apMesh a)
			//{
			//	return a == null;
			//}); 
			#endregion

			//변경
			ReadyToEdit_Step1();


			//이전
			#region [미사용 코드]
			//for (int iMeshes = 0; iMeshes < _meshes.Count; iMeshes++)
			//{
			//	//내부 MeshComponent들의 레퍼런스를 연결하자
			//	_meshes[iMeshes].ReadyToEdit(this);

			//	//텍스쳐를 연결하자
			//	int textureID = -1;

			//	//이전 코드
			//	//if (_meshes[iMeshes]._textureData != null)
			//	//{
			//	//	textureID = _meshes[iMeshes]._textureData._uniqueID;
			//	//	_meshes[iMeshes]._textureData = GetTexture(textureID);
			//	//}

			//	//변경 코드 4.1
			//	if (!_meshes[iMeshes].IsTextureDataLinked)//연결이 안된 경우
			//	{
			//		textureID = _meshes[iMeshes].LinkedTextureDataID;
			//		_meshes[iMeshes].SetTextureData(GetTexture(textureID));
			//	}

			//	_meshes[iMeshes].LinkEdgeAndVertex();
			//} 
			#endregion

			//변경
			ReadyToEdit_Step2();


			//이전
			#region [미사용 코드]
			//_meshGroups.RemoveAll(delegate(apMeshGroup a)
			//{
			//	return a == null;
			//});


			////메시 그룹도 비슷하게 해주자
			////1. 메시/메시 그룹을 먼저 연결
			////2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//	meshGroup.Init(this);

			//	//1. Mesh 연결
			//	for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
			//	{
			//		meshGroup._childMeshTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

			//		int childIndex = meshGroup._childMeshTransforms[iChild]._meshUniqueID;
			//		if (childIndex >= 0)
			//		{
			//			apMesh existMesh = GetMesh(childIndex);
			//			if (existMesh != null)
			//			{
			//				meshGroup._childMeshTransforms[iChild]._mesh = existMesh;
			//			}
			//			else
			//			{
			//				meshGroup._childMeshTransforms[iChild]._mesh = null;
			//			}
			//		}
			//		else
			//		{
			//			meshGroup._childMeshTransforms[iChild]._mesh = null;
			//		}
			//	}

			//	//1-2. MeshGroup 연결
			//	for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
			//	{
			//		meshGroup._childMeshGroupTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

			//		int childIndex = meshGroup._childMeshGroupTransforms[iChild]._meshGroupUniqueID;
			//		if (childIndex >= 0)
			//		{
			//			apMeshGroup existMeshGroup = GetMeshGroup(childIndex);
			//			if (existMeshGroup != null)
			//			{
			//				meshGroup._childMeshGroupTransforms[iChild]._meshGroup = existMeshGroup;
			//			}
			//			else
			//			{
			//				meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
			//			}
			//		}
			//		else
			//		{
			//			meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
			//		}
			//	}
			//}

			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//	//2. 하위 MeshGroup 연결
			//	for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
			//	{
			//		apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

			//		if (childMeshGroupTransform._meshGroupUniqueID >= 0)
			//		{
			//			apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
			//			if (existMeshGroup != null)
			//			{
			//				childMeshGroupTransform._meshGroup = existMeshGroup;

			//				childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
			//				childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;


			//			}
			//			else
			//			{
			//				childMeshGroupTransform._meshGroup = null;
			//			}
			//		}
			//		else
			//		{
			//			childMeshGroupTransform._meshGroup = null;
			//		}
			//	}

			//	//다만, 없어진 Mesh Group은 정리해주자
			//	meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
			//	{
			//		return a._mesh == null;
			//	});
			//	meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
			//	{
			//		return a._meshGroup == null;
			//	});
			//} 
			#endregion

			//변경
			ReadyToEdit_Step3();



			//이전
			#region [미사용 코드]
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//	//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.
			//	for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
			//	{
			//		//연결하기 전에
			//		//Child는 초기화해준다.
			//		apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
			//		meshTransform._isClipping_Child = false;
			//		meshTransform._clipIndexFromParent = -1;
			//		meshTransform._clipParentMeshTransform = null;

			//		if (meshTransform._clipChildMeshes == null)
			//		{
			//			meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
			//		}

			//		meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
			//		{
			//			//조건에 맞지 않는 Clipping Child를 삭제한다.
			//			//1. ID가 맞지 않다.
			//			//2. MeshGroup에 존재하지 않다.
			//			return a._transformID < 0 || (meshGroup.GetMeshTransform(a._transformID) == null);
			//		});


			//		//추가 19.6.9 : Material Set 연결
			//		if (meshTransform._isUseDefaultMaterialSet)
			//		{
			//			//Default MatSet을 사용하는 경우
			//			meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
			//			if (meshTransform._linkedMaterialSet != null)
			//			{
			//				meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
			//			}
			//		}
			//		else
			//		{
			//			//별도의 MatSet을 설정한 경우
			//			if (meshTransform._materialSetID >= 0)
			//			{
			//				meshTransform._linkedMaterialSet = GetMaterialSet(meshTransform._materialSetID);
			//				if (meshTransform._linkedMaterialSet == null)
			//				{
			//					//존재하지 않는 Material Set
			//					meshTransform._materialSetID = -1;
			//					//Debug.LogError("Material Set 잘못 연결 후 초기화");
			//				}
			//			}
			//			else
			//			{
			//				meshTransform._linkedMaterialSet = null;
			//			}

			//			//만약 연결이 안된다면 > Default를 찾아서 무조건 연결한다.
			//			if (meshTransform._linkedMaterialSet == null)
			//			{
			//				meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
			//				if (meshTransform._linkedMaterialSet != null)
			//				{
			//					meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
			//				}
			//			}
			//		}
			//	}

			//	for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
			//	{
			//		apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
			//		if (meshTransform._isClipping_Parent)
			//		{
			//			//최대 3개의 하위 Mesh를 검색해서 연결한다.
			//			//찾은 이후엔 Sort를 해준다.

			//			for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
			//			{
			//				apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
			//				int childMeshID = clipSet._transformID;
			//				apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
			//				if (childMeshTF != null)
			//				{
			//					clipSet._meshTransform = childMeshTF;
			//					clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);
			//				}
			//				else
			//				{
			//					clipSet._meshTransform = null;
			//					clipSet._transformID = -1;
			//					clipSet._renderUnit = null;
			//				}
			//			}

			//			meshTransform._clipChildMeshes.RemoveAll(delegate(apTransform_Mesh.ClipMeshSet a)
			//			{
			//				return a._transformID < 0;
			//			});


			//		}
			//		else
			//		{
			//			meshTransform._clipChildMeshes.Clear();


			//		}

			//		meshTransform.SortClipMeshTransforms();
			//	}

			//} 
			#endregion

			//변경
			ReadyToEdit_Step4();


			//이전
			#region [미사용 코드]
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//	//2. 상위 MeshGroup 연결
			//	int parentUniqueID = meshGroup._parentMeshGroupID;
			//	if (parentUniqueID >= 0)
			//	{
			//		meshGroup._parentMeshGroup = GetMeshGroup(parentUniqueID);
			//		if (meshGroup._parentMeshGroup == null)
			//		{
			//			meshGroup._parentMeshGroupID = -1;
			//		}
			//	}
			//	else
			//	{
			//		meshGroup._parentMeshGroup = null;
			//	}
			//}

			////Bone 연결 
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//	//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
			//	//이후엔 Root -> Child 방식으로 순회
			//	meshGroup._boneList_Root.Clear();
			//	if (meshGroup._boneList_All != null)
			//	{
			//		for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
			//		{
			//			apBone bone = meshGroup._boneList_All[iBone];

			//			//먼저 ID를 ID Manager에 등록한다.
			//			RegistUniqueID(apIDManager.TARGET.Bone, bone._uniqueID);

			//			apBone parentBone = null;
			//			if (bone._parentBoneID >= 0)
			//			{
			//				parentBone = meshGroup.GetBone(bone._parentBoneID);
			//			}

			//			bone.Link(meshGroup, parentBone, this);

			//			if (parentBone == null)
			//			{
			//				//Parent가 없다면 Root 본이다.
			//				meshGroup._boneList_Root.Add(bone);
			//			}
			//		}

			//		//추가 5.9 : Bone의 Check Validation 함수를 호출해야 한다.
			//		for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
			//		{
			//			meshGroup._boneList_All[iBone].CheckIKControllerValidation();
			//		}
			//	}


			//	int curBoneIndex = 0;
			//	for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
			//	{
			//		apBone rootBone = meshGroup._boneList_Root[iRoot];
			//		//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
			//		rootBone.LinkRecursive(0);
			//		curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
			//	}
			//}

			////본 계층 / IK Chain도 다시 점검
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//} 
			#endregion

			//변경
			ReadyToEdit_Step5();

			//이전
			#region [미사용 코드]
			////Render Unit도 체크해주자
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];
			//	//meshGroup.SetAllRenderUnitForceUpdate();
			//	meshGroup.RefreshForce();
			//	meshGroup.SortRenderUnits(true);
			//	meshGroup.SortBoneListByLevelAndDepth();
			//} 
			#endregion

			//변경
			ReadyToEdit_Step6();


			//이전
			#region [미사용 코드]
			////Anim Clip 준비도 하자
			//_animClips.RemoveAll(delegate(apAnimClip a)
			//{
			//	return 
			//	a == null || //Null이거나
			//	(a._targetMeshGroupID >= 0 && GetMeshGroup(a._targetMeshGroupID) == null);//TargetMeshGroup ID는 있는데, MeshGroup은 존재하지 않는 경우
			//});

			//for (int i = 0; i < _animClips.Count; i++)
			//{
			//	_animClips[i].LinkEditor(this);
			//	_animClips[i].RemoveUnlinkedTimeline();
			//} 
			#endregion

			//변경
			ReadyToEdit_Step7();

			//이전
			#region [미사용 코드]
			////5. Modifier 세팅
			//LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AllObjects(null)); 
			#endregion



			//변경
			ReadyToEdit_Step8();

			//이전
			#region [미사용 코드]
			//// Main MeshGroup 연결
			//// 수정) "다중" MainMeshGroup으로 변경

			//if (_mainMeshGroupList == null)		{ _mainMeshGroupList = new List<apMeshGroup>(); }
			//else								{ _mainMeshGroupList.Clear(); }

			//if (_mainMeshGroupIDList == null)
			//{
			//	_mainMeshGroupIDList = new List<int>();
			//}


			//for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
			//{
			//	int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
			//	bool isValidMeshGroupID = false;

			//	if (mainMeshGroupID >= 0)
			//	{
			//		apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
			//		if (mainMeshGroup != null)
			//		{
			//			if (!_mainMeshGroupList.Contains(mainMeshGroup))
			//			{
			//				_mainMeshGroupList.Add(mainMeshGroup);
			//				isValidMeshGroupID = true;
			//			}
			//		}
			//	}
			//	if (!isValidMeshGroupID)
			//	{
			//		_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
			//	}
			//}

			////일단 유효하지 못한 ID는 삭제하자
			//_mainMeshGroupIDList.RemoveAll(delegate (int a)
			//{
			//	return a < 0;
			//});


			//_rootUnits.Clear();

			//for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
			//{
			//	apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];

			//	apRootUnit newRootUnit = new apRootUnit();

			//	newRootUnit.SetPortrait(this);
			//	newRootUnit.SetMeshGroup(meshGroup);

			//	_rootUnits.Add(newRootUnit);
			//} 
			#endregion

			//변경
			ReadyToEdit_Step9();
		}




		// ReadyToStep의 내용들을 각각의 서브 함수들로 분리한다.
		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step1()
		{
			//ID 리스트 일단 리셋
			ClearRegisteredUniqueIDs();

			//컨트롤 / 컨트롤 파라미터 리셋
			_controller.Ready(this);
			_controller.SetDefaultAll();


			for (int iTexture = 0; iTexture < _textureData.Count; iTexture++)
			{
				_textureData[iTexture].ReadyToEdit(this);
			}

			_meshes.RemoveAll(delegate(apMesh a)
			{
				return a == null;
			});
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step2()
		{
			for (int iMeshes = 0; iMeshes < _meshes.Count; iMeshes++)
			{
				//내부 MeshComponent들의 레퍼런스를 연결하자
				_meshes[iMeshes].ReadyToEdit(this);

				//텍스쳐를 연결하자
				int textureID = -1;

				if (!_meshes[iMeshes].IsTextureDataLinked)//연결이 안된 경우
				{
					textureID = _meshes[iMeshes].LinkedTextureDataID;
					_meshes[iMeshes].SetTextureData(GetTexture(textureID));
				}

				_meshes[iMeshes].LinkEdgeAndVertex();
			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step3()
		{
			_meshGroups.RemoveAll(delegate (apMeshGroup a)
			{
				return a == null;
			});


			//메시 그룹도 비슷하게 해주자
			//1. 메시/메시 그룹을 먼저 연결
			//2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				meshGroup.Init(this);

				//1. Mesh 연결
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					meshGroup._childMeshTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshTransforms[iChild]._meshUniqueID;
					if (childIndex >= 0)
					{
						apMesh existMesh = GetMesh(childIndex);
						if (existMesh != null)
						{
							meshGroup._childMeshTransforms[iChild]._mesh = existMesh;
						}
						else
						{
							meshGroup._childMeshTransforms[iChild]._mesh = null;
						}
					}
					else
					{
						meshGroup._childMeshTransforms[iChild]._mesh = null;
					}
				}

				//1-2. MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					meshGroup._childMeshGroupTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshGroupTransforms[iChild]._meshGroupUniqueID;
					if (childIndex >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childIndex);
						if (existMeshGroup != null)
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = existMeshGroup;
						}
						else
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
						}
					}
					else
					{
						meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
					}
				}
			}

			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 하위 MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

					if (childMeshGroupTransform._meshGroupUniqueID >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
						if (existMeshGroup != null)
						{
							childMeshGroupTransform._meshGroup = existMeshGroup;

							childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
							childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;


						}
						else
						{
							childMeshGroupTransform._meshGroup = null;
						}
					}
					else
					{
						childMeshGroupTransform._meshGroup = null;
					}
				}

				//다만, 없어진 Mesh Group은 정리해주자
				meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
				{
					return a._mesh == null;
				});
				meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
				{
					return a._meshGroup == null;
				});
			}
		}


		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step4()
		{
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					//연결하기 전에
					//Child는 초기화해준다.
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					meshTransform._isClipping_Child = false;
					meshTransform._clipIndexFromParent = -1;
					meshTransform._clipParentMeshTransform = null;

					if (meshTransform._clipChildMeshes == null)
					{
						meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
					}

					meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
					{
						//조건에 맞지 않는 Clipping Child를 삭제한다.
						//1. ID가 맞지 않다.
						//2. MeshGroup에 존재하지 않다.
						return a._transformID < 0 || (meshGroup.GetMeshTransform(a._transformID) == null);
					});


					//추가 19.6.9 : Material Set 연결
					if (meshTransform._isUseDefaultMaterialSet)
					{
						//Default MatSet을 사용하는 경우
						meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
						if (meshTransform._linkedMaterialSet != null)
						{
							meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
						}
					}
					else
					{
						//별도의 MatSet을 설정한 경우
						if (meshTransform._materialSetID >= 0)
						{
							meshTransform._linkedMaterialSet = GetMaterialSet(meshTransform._materialSetID);
							if (meshTransform._linkedMaterialSet == null)
							{
								//존재하지 않는 Material Set
								meshTransform._materialSetID = -1;
								//Debug.LogError("Material Set 잘못 연결 후 초기화");
							}
						}
						else
						{
							meshTransform._linkedMaterialSet = null;
						}

						//만약 연결이 안된다면 > Default를 찾아서 무조건 연결한다.
						if (meshTransform._linkedMaterialSet == null)
						{
							meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
							if (meshTransform._linkedMaterialSet != null)
							{
								meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
							}
						}
					}
				}

				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					if (meshTransform._isClipping_Parent)
					{
						//최대 3개의 하위 Mesh를 검색해서 연결한다.
						//찾은 이후엔 Sort를 해준다.

						for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
						{
							apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
							int childMeshID = clipSet._transformID;
							apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
							if (childMeshTF != null)
							{
								clipSet._meshTransform = childMeshTF;
								clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);
							}
							else
							{
								clipSet._meshTransform = null;
								clipSet._transformID = -1;
								clipSet._renderUnit = null;
							}
						}

						meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
						{
							return a._transformID < 0;
						});


					}
					else
					{
						meshTransform._clipChildMeshes.Clear();


					}

					meshTransform.SortClipMeshTransforms();
				}

			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step5()
		{
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 상위 MeshGroup 연결
				int parentUniqueID = meshGroup._parentMeshGroupID;
				if (parentUniqueID >= 0)
				{
					meshGroup._parentMeshGroup = GetMeshGroup(parentUniqueID);
					if (meshGroup._parentMeshGroup == null)
					{
						meshGroup._parentMeshGroupID = -1;
					}
				}
				else
				{
					meshGroup._parentMeshGroup = null;
				}
			}

			//Bone 연결 
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				meshGroup._boneList_Root.Clear();
				if (meshGroup._boneList_All != null)
				{
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];

						//먼저 ID를 ID Manager에 등록한다.
						RegistUniqueID(apIDManager.TARGET.Bone, bone._uniqueID);

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone, this);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}

					//추가 5.9 : Bone의 Check Validation 함수를 호출해야 한다.
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						meshGroup._boneList_All[iBone].CheckIKControllerValidation();
					}
				}

				
				int curBoneIndex = 0;
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					apBone rootBone = meshGroup._boneList_Root[iRoot];
					//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
					rootBone.LinkRecursive(0);
					curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
				}
			}

			////본 계층 / IK Chain도 다시 점검
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step6()
		{
			//Render Unit도 체크해주자
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				//meshGroup.SetAllRenderUnitForceUpdate();
				meshGroup.RefreshForce();
				meshGroup.SortRenderUnits(true);
				meshGroup.SortBoneListByLevelAndDepth();
			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step7()
		{
			//Anim Clip 준비도 하자
			_animClips.RemoveAll(delegate(apAnimClip a)
			{
				return 
				a == null || //Null이거나
				(a._targetMeshGroupID >= 0 && GetMeshGroup(a._targetMeshGroupID) == null);//TargetMeshGroup ID는 있는데, MeshGroup은 존재하지 않는 경우
			});

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkEditor(this);
				_animClips[i].RemoveUnlinkedTimeline();
			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step8()
		{
			//5. Modifier 세팅
			LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AllObjects(null));

			
		}


		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step9()
		{
			// Main MeshGroup 연결
			// 수정) "다중" MainMeshGroup으로 변경

			if (_mainMeshGroupList == null)		{ _mainMeshGroupList = new List<apMeshGroup>(); }
			else								{ _mainMeshGroupList.Clear(); }

			if (_mainMeshGroupIDList == null)
			{
				_mainMeshGroupIDList = new List<int>();
			}


			for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
			{
				int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
				bool isValidMeshGroupID = false;

				if (mainMeshGroupID >= 0)
				{
					apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
					if (mainMeshGroup != null)
					{
						if (!_mainMeshGroupList.Contains(mainMeshGroup))
						{
							_mainMeshGroupList.Add(mainMeshGroup);
							isValidMeshGroupID = true;
						}
					}
				}
				if (!isValidMeshGroupID)
				{
					_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
				}
			}

			//일단 유효하지 못한 ID는 삭제하자
			_mainMeshGroupIDList.RemoveAll(delegate (int a)
			{
				return a < 0;
			});

			_rootUnits.Clear();

			for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
			{
				apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];

				apRootUnit newRootUnit = new apRootUnit();

				newRootUnit.SetPortrait(this);
				newRootUnit.SetMeshGroup(meshGroup);

				_rootUnits.Add(newRootUnit);
			}
		}


		//-------------------------------------------------------------------------------------------------
		// LinkAndRefreshInEditor
		//-------------------------------------------------------------------------------------------------

		//Editor 상태에서
		//MeshGroup을 참조하는 객체들 간의 레퍼런스를 연결하고 갱신한다.
		//Editor 실행시와 객체 추가/삭제시 호출해주자
		//최적화 코드 추가 20.4.3
		//- 1. 모든 항목을 링크할 지, 특정 애니메이션만 링크를 다시 할지 결정한다. (애니메이션 편집 정보는 해당 메시 그룹 외에는 다른 영향이 없으므로)
		//- 2. Async 처리도 가능하도록 개선.
		/// <summary>[Please do not use it]</summary>
		/// <param name="curSelectedMeshGroup">현재 선택된 메시 그룹. RenderUnit과 Mod 정보를 한번 더 갱신한다. (이것 외의 링크를 제한하는 역할은 없음)</param>
		/// <param name="targetAnimClip">현재 편집중인 AnimClip. 이게 null이면 전체 갱신. 대상이 있다면 불필요한 링크 작업은 생략된다.(isResetLink가 false인 경우에 한해서)</param>
		public void LinkAndRefreshInEditor(bool isResetLink,	apUtil.LinkRefreshRequest linkRefreshRequest
																//apMeshGroup curSelectedMeshGroup, apAnimClip targetAnimClip
																)
		{
			
			//Debug.Log("LinkAndRefreshInEditor (isResetLink : " + isResetLink + " / MeshGroup : " + (curSelectedMeshGroup != null) + " / AnimClip : " + (targetAnimClip != null) + ")");
			//Debug.Log("LinkAndRefreshInEditor (isResetLink : " + isResetLink + " / Request : " + (linkRefreshRequest != null ? linkRefreshRequest.ToString() : "Null") + ")");

			//4.1 리셋이 필요한지 검사한다.
			//겸사겸사 불필요한 데이터도 삭제한다.

			
			int nTextureRemoved = _textureData.RemoveAll(delegate(apTextureData a)
			{
				return a == null;
			});
			int nMeshRemoved = _meshes.RemoveAll(delegate(apMesh a)
			{
				return a == null;
			});
			int nMeshGroupRemoved = _meshGroups.RemoveAll(delegate(apMeshGroup a)
			{
				return a == null;
			});
			int nAnimClipRemoved = _animClips.RemoveAll(delegate(apAnimClip a)
			{
				return a == null;
			});

			//TODO : 단순히 MeshGroup을 루틴을 돌아서 처리하면, Root Mesh Group보다 Child Mesh Group이 나중에 처리되서,
			//Root Mesh Group의 Link가 다 끊기게 된다.
			//따라서 Child 부터 처리를 해야한다.
			//<REV_MG>
			List<apMeshGroup> revMeshGroups = GetReverseMeshGroupList(_meshGroups);

			int nModRemoved = 0;

			//<REV_MG>
			//for (int i = 0; i < _meshGroups.Count; i++)
			for (int i = 0; i < revMeshGroups.Count; i++)
			{
				//int curNumModRemoved = _meshGroups[i]._modifierStack._modifiers.RemoveAll(delegate(apModifierBase a)
				int curNumModRemoved = revMeshGroups[i]._modifierStack._modifiers.RemoveAll(delegate(apModifierBase a)
				{
					return a == null;
				});
				nModRemoved += curNumModRemoved;
			}

			if(!isResetLink)
			{
				if(nTextureRemoved > 0 ||
					nMeshRemoved > 0 ||
					nMeshGroupRemoved > 0 ||
					nAnimClipRemoved > 0 ||
					nModRemoved > 0)
				{
					isResetLink = true;
				}
			}

			
			

			//4.1 추가
			// 만약 isResetLink= true라면
			// ReadyToEdit와 같이 
			if (isResetLink)
			{
				//변경. 이 코드를 isResetLink 안으로 이동시킴 (20.4.3)
				//--------------------------------------
				_controller.Ready(this);

				for (int iMesh = 0; iMesh < _meshes.Count; iMesh++)
				{
					_meshes[iMesh].LinkEdgeAndVertex();
				}
				//--------------------------------------

				//텍스쳐도 리셋
				for (int iTexture = 0; iTexture < _textureData.Count; iTexture++)
				{
					_textureData[iTexture].ReadyToEdit(this);
					//if(_textureData[iTexture]._image == null)//<<이게 Null이다.
					//{
					//	//Debug.LogError("?? Texture가 연결이 안되었는데용 [" + _textureData[iTexture]._name + "]");
					//}
				}

				
				for (int iMeshes = 0; iMeshes < _meshes.Count; iMeshes++)
				{
					//내부 MeshComponent들의 레퍼런스를 연결하자
					_meshes[iMeshes].ReadyToEdit(this);

					//텍스쳐를 연결하자
					int textureID = -1;

					//이전 코드
					//if (_meshes[iMeshes]._textureData != null)
					//{
					//	textureID = _meshes[iMeshes]._textureData._uniqueID;
					//	_meshes[iMeshes]._textureData = GetTexture(textureID);
					//}
					
					//변경 코드 4.1
					textureID = _meshes[iMeshes].LinkedTextureDataID;
					_meshes[iMeshes].SetTextureData(GetTexture(textureID));

					_meshes[iMeshes].LinkEdgeAndVertex();
				}

				//1. 메시/메시 그룹을 먼저 연결
				//2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
				//<REV_MG>
				//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)		
				for (int iMeshGroup = 0; iMeshGroup < revMeshGroups.Count; iMeshGroup++)
				{
					//apMeshGroup meshGroup = _meshGroups[iMeshGroup];
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					meshGroup.Init(this);

					meshGroup._childMeshTransforms.RemoveAll(delegate(apTransform_Mesh a)
					{
						return a == null;
					});

					//1. Mesh 연결 + Clipping 연결
					apTransform_Mesh meshTransform = null;
					for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
					{
						meshTransform = meshGroup._childMeshTransforms[iChild];
						meshTransform.RegistIDToPortrait(this);//추가 : ID를 알려주자

						int childIndex = meshTransform._meshUniqueID;
						if (childIndex >= 0)
						{
							if (meshTransform._mesh == null)
							{
								//Mesh가 연결 안된 경우
								apMesh existMesh = GetMesh(childIndex);
								if (existMesh != null)
								{
									meshTransform._mesh = existMesh;
								}
								else
								{
									meshTransform._mesh = null;
									//Debug.LogError("Mesh가 없는 MeshTransform 발견 : " + meshTransform._nickName);
								}
							}

							//--------------
							//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.

							
							if (meshTransform._clipChildMeshes == null)
							{
								meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
							}

							meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
							{
								//조건에 맞지 않는 Clipping Child를 삭제한다.
								//1. ID가 맞지 않다.
								//2. MeshGroup에 존재하지 않다.
								return a._transformID < 0 || (meshGroup.GetMeshTransform(a._transformID) == null);
							});
							
							//-------------
						}
						else
						{
							meshGroup._childMeshTransforms[iChild]._mesh = null;
							//Debug.LogError("Mesh ID가 유효하지 않은 MeshTransform 발견 : " + meshGroup._childMeshTransforms[iChild]._nickName);
						}

						//추가 19.6.9 : Material Set 연결
						if (meshTransform._isUseDefaultMaterialSet)
						{
							//기본값의 MatSet을 사용하자.
							meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
							if(meshTransform._linkedMaterialSet != null)
							{
								//ID도 바꿔주자.
								meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
							}
						}
						else
						{
							if (meshTransform._materialSetID >= 0)
							{
								meshTransform._linkedMaterialSet = GetMaterialSet(meshTransform._materialSetID);
								if (meshTransform._linkedMaterialSet == null)
								{
									//존재하지 않는 Material Set
									meshTransform._materialSetID = -1;
								}
							}
							else
							{
								meshTransform._linkedMaterialSet = null;
							}
						}
						
					}

					


					//1-2. MeshGroup 연결

					meshGroup._childMeshGroupTransforms.RemoveAll(delegate(apTransform_MeshGroup a)
					{
						return a == null;
					});

					for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
					{
						meshGroup._childMeshGroupTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

						int childIndex = meshGroup._childMeshGroupTransforms[iChild]._meshGroupUniqueID;
						if (childIndex >= 0)
						{	
							if (meshGroup._childMeshGroupTransforms[iChild]._meshGroup == null)
							{
								//MeshGroup이 연결이 안된 경우
								apMeshGroup existMeshGroup = GetMeshGroup(childIndex);
								if (existMeshGroup != null)
								{
									meshGroup._childMeshGroupTransforms[iChild]._meshGroup = existMeshGroup;
								}
								else
								{
									meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
									//Debug.LogError("MeshGroup이 없는 MeshGroupTransform 발견 : " + meshGroup._childMeshGroupTransforms[iChild]._nickName);
								}
							}
						}
						else
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
							//Debug.LogError("MeshGroup ID가 유효하지 않은 MeshGroupTransform 발견 : " + meshGroup._childMeshGroupTransforms[iChild]._nickName);
						}
					}
				}

				//<REV_MG>
				//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
				for (int iMeshGroup = 0; iMeshGroup < revMeshGroups.Count; iMeshGroup++)
				{
					//apMeshGroup meshGroup = _meshGroups[iMeshGroup];
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					//2. 하위 MeshGroup 연결
					for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
					{
						apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

						if (childMeshGroupTransform._meshGroupUniqueID >= 0)
						{
							apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
							if (existMeshGroup != null)
							{
								childMeshGroupTransform._meshGroup = existMeshGroup;

								childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
								childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;
							}
							else
							{
								childMeshGroupTransform._meshGroup = null;
							}
						}
						else
						{
							childMeshGroupTransform._meshGroup = null;
						}
					}

					//다만, 없어진 Mesh Group은 정리해주자
					meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
					{
						return a._mesh == null;
					});
					meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
					{
						return a._meshGroup == null;
					});
					
				}

				
				

				//<REV_MG>
				//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
				for (int iMeshGroup = 0; iMeshGroup < revMeshGroups.Count; iMeshGroup++)
				{
					//apMeshGroup meshGroup = _meshGroups[iMeshGroup];
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					meshGroup.SortRenderUnits(true);


					//추가 : Clipping 후속 처리를 한다.
					apTransform_Mesh meshTransform = null;
					for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
					{
						meshTransform = meshGroup._childMeshTransforms[iChild];

						if (meshTransform._isClipping_Parent)
						{
							//최대 3개의 하위 Mesh를 검색해서 연결한다.
							//찾은 이후엔 Sort를 해준다.

							for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
							{
								apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
								int childMeshID = clipSet._transformID;
								apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
								if (childMeshTF != null)
								{
									clipSet._meshTransform = childMeshTF;
									clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);
								}
								else
								{
									clipSet._meshTransform = null;
									clipSet._transformID = -1;
									clipSet._renderUnit = null;
								}
							}

							meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
							{
								return a._transformID < 0;
							});

						}
						else
						{
							meshTransform._clipChildMeshes.Clear();//<<이건 일단 초기화 하지말자
						}

						meshTransform.SortClipMeshTransforms();
					}


					meshGroup.ResetRenderUnitsWithoutRefreshEditor();
					meshGroup.RefreshAutoClipping();
					if(meshGroup._rootRenderUnit != null)
					{
						meshGroup._rootRenderUnit.ReadyToUpdate();
					}
					else
					{
						//Debug.LogError("Root Rendr Unit이 없다.");
					}
					
					
				}

				//Debug.LogWarning("<Link And Refresh In Editor> : Modifier Test");

				

				//3. MeshGroup -> Modifier를 돌면서 삭제된 meshTransform / meshGroupTransform / Bone을 잡고 있는 경우 삭제한다.
				//<REV_MG>
				//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
				for (int iMeshGroup = 0; iMeshGroup < revMeshGroups.Count; iMeshGroup++)
				{
					//apMeshGroup meshGroup = _meshGroups[iMeshGroup];
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					meshGroup._modifierStack._modifiers.RemoveAll(delegate(apModifierBase a)
					{
						return a == null;
					});


					for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
					{
						apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];
						if(modifier == null)
						{
							continue;
						}

						//여기서 Modifier Link를 다시 해야한다.
						
						//apMeshGroup meshGroupOfTransform = null;
						apMeshGroup meshGroupOfBone = null;

						for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
						{
							apModifierParamSetGroup modPSG = modifier._paramSetGroup_controller[iPSG];

							switch (modPSG._syncTarget)
							{
								case apModifierParamSetGroup.SYNC_TARGET.Bones:
								case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
									//안쓰는 값
									break;
								case apModifierParamSetGroup.SYNC_TARGET.Controller:
									//Controller 체크해볼 필요 있다.
									modPSG._keyControlParam = _controller.FindParam(modPSG._keyControlParamID);
									//if (modPSG._keyControlParam == null)
									//{
									//	Debug.LogError("주의 : Modifier ParamSetGroup에서 Control Param이 없다.");
									//}
									
									break;

								case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
									modPSG._keyAnimClip = GetAnimClip(modPSG._keyAnimClipID);
									modPSG._keyAnimTimeline = null;
									modPSG._keyAnimTimelineLayer = null;
									if(modPSG._keyAnimClip != null)
									{
										modPSG._keyAnimTimeline = modPSG._keyAnimClip.GetTimeline(modPSG._keyAnimTimelineID);
										if(modPSG._keyAnimTimeline != null)
										{
											modPSG._keyAnimTimelineLayer = modPSG._keyAnimTimeline.GetTimelineLayer(modPSG._keyAnimTimelineLayerID);
										}
									}
									
									//if(modPSG._keyAnimClip == null || modPSG._keyAnimTimeline == null || modPSG._keyAnimTimelineLayer == null)
									//{
									//	Debug.LogError("주의 : Modifier ParamSetGroup에서 AnimClip / TImeline / TimelineLayer가 연동이 안되었다.");
									//}
									break;
							}

							for (int iPS = 0; iPS < modPSG._paramSetList.Count; iPS++)
							{
								apModifierParamSet modPS = modPSG._paramSetList[iPS];
								
								if (modPS._meshData != null)
								{
									//하위의 MeshGroup Transform이 삭제될 수 있도록
									//적절하지 않은 MeshData를 삭제하자
									//int nRemoved = modPS._meshData.RemoveAll(delegate (apModifiedMesh a)
									modPS._meshData.RemoveAll(delegate (apModifiedMesh a)
									{
										//if (a._isRecursiveChildTransform)
										//{
										//	meshGroupOfTransform = GetMeshGroup(a._meshGroupUniqueID_Transform);
										//}
										//else
										//{
										//	meshGroupOfTransform = meshGroup;
										//}
										if (meshGroup != null)
										{
											if (a._isMeshTransform)
											{
												//MeshTransform이 유효한지 찾자
												a._transform_Mesh = meshGroup.GetMeshTransformRecursive(a._transformUniqueID);
												if (a._transform_Mesh == null || a._transform_Mesh._mesh == null)
												{
													//Mesh Transform이 없다. 삭제
													//Debug.LogError("ModMesh - Mesh : 삭제됨");
													return true;
												}

											}
											else
											{
												//MeshGroupTransform이 유효한지 찾자
												a._transform_MeshGroup = meshGroup.GetMeshGroupTransformRecursive(a._transformUniqueID);
												if (a._transform_MeshGroup == null || a._transform_MeshGroup._meshGroup == null)
												{
													//MeshGroup Transform이 없다. 삭제
													//Debug.LogError("ModMesh - MeshGroup : 삭제됨");
													return true;
												}
											}
										}

										return false;

									});
									//if (nRemoved > 0)
									//{
									//	Debug.LogError("Modifier [" + modifier.DisplayName + "] ModMesh " + nRemoved + "개 삭제됨");

									//}
								}

								//적절하지 않은 Bone Data를 삭제하자
								if (modPS._boneData != null)
								{
									modPS._boneData.RemoveAll(delegate (apModifiedBone a)
									{
										meshGroupOfBone = GetMeshGroup(a._meshGropuUniqueID_Bone);

										if(meshGroupOfBone != null)
										{
											a._bone = meshGroupOfBone.GetBone(a._boneID);
											if(a._bone == null)
											{
												//Bone이 없다. 삭제
												//Debug.LogError("ModBone - Bone : 삭제됨");
												return true;
											}
										}

										return false;
									});
								}
							}
						}
					}
				}
				
				//Root Unit도 갱신하자
				if (_mainMeshGroupList == null)		{ _mainMeshGroupList = new List<apMeshGroup>(); }
				else								{ _mainMeshGroupList.Clear(); }

				if (_mainMeshGroupIDList == null) { _mainMeshGroupIDList = new List<int>(); }

				for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
				{
					int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
					bool isValidMeshGroupID = false;

					if (mainMeshGroupID >= 0)
					{
						apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
						if (mainMeshGroup != null)
						{
							if (!_mainMeshGroupList.Contains(mainMeshGroup))
							{
								_mainMeshGroupList.Add(mainMeshGroup);
								isValidMeshGroupID = true;
							}
						}
					}
					if (!isValidMeshGroupID)
					{
						_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
					}
				}

				//일단 유효하지 못한 ID는 삭제하자
				_mainMeshGroupIDList.RemoveAll(delegate (int a)
				{
					return a < 0;
				});

				//기존의 RootUnit중 삭제할 것 먼저 빼자
				_rootUnits.RemoveAll(delegate(apRootUnit a)
				{
					//유효한 MeshGroup을 가지지 않는 경우
					return a._childMeshGroup == null 
							|| !_meshGroups.Contains(a._childMeshGroup)
							|| !_mainMeshGroupList.Contains(a._childMeshGroup);
				});

				//재활용을 위해서 리스트를 새로 만들자
				List<apRootUnit> prevRootUnits = new List<apRootUnit>();
				for (int iRootUnit = 0; iRootUnit < _rootUnits.Count; iRootUnit++)
				{
					prevRootUnits.Add(_rootUnits[iRootUnit]);
				}

				//리스트 클리어
				_rootUnits.Clear();
				for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
				{
					apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];
					
					//재활용 가능한지 확인하자
					apRootUnit existRootUnit = prevRootUnits.Find(delegate(apRootUnit a)
					{
						return a._childMeshGroup == meshGroup;
					});
					if (existRootUnit != null)
					{
						//있다. 리스트에 넣자
						existRootUnit.SetPortrait(this);
						_rootUnits.Add(existRootUnit);
					}
					else
					{
						//없다. 새로 추가
						apRootUnit newRootUnit = new apRootUnit();

						newRootUnit.SetPortrait(this);
						newRootUnit.SetMeshGroup(meshGroup);

						_rootUnits.Add(newRootUnit);
					}
				}
				

			}
			//isResetLink 끝-------------------
			
			//모든 모디파이어가 아닌 특정 AnimClip에 대한 모디파이어를 제외할 것인가.
			bool isLinkAllMeshGroups = false;
			bool isSkipAllAnimModifier = false;//모든 Anim 모디파이어 스킵
			bool isSkipUnselectedAnimPSGs = false;//선택되지 않은 Anim 모디파이어의 PSG 스킵
			apAnimClip selectedAnimClip = null;

			
			bool isNeedToRefreshOtherMeshGroups = false;
			apMeshGroup curSelectedMeshGroup = null;

			//메시 그룹 링크 다시 확인하는 코드
			//최적화 20.4.3 : 이 코드들이 에디터를 무겁게 한다.
			//만약 특정 메시 그룹을 선택했거나, 특정 AnimClip을 선택했다면, 그 외의 메시 그룹을 갱신하지 말자.
			//revMeshGroups에서 대상이 되는 메시 그룹의 부모/자식들은 제외한다.
			if(linkRefreshRequest != null)
			{
				if(linkRefreshRequest.Request_MeshGroup == apUtil.LR_REQUEST__MESHGROUP.SelectedMeshGroup && linkRefreshRequest.MeshGroup != null)
				{
					//대상이 되는 메시 그룹과 관련된 모든 메시 그룹을 찾자.
					List<apMeshGroup> targetMeshGroups = new List<apMeshGroup>();

					if(linkRefreshRequest.MeshGroup != null)
					{
						FindAllParentAndChildrenMeshGroups(linkRefreshRequest.MeshGroup, targetMeshGroups);
					}

					revMeshGroups = targetMeshGroups;//일부 메시 그룹만 선택을 한다.
					//Debug.LogWarning("Cur MeshGroups : " + revMeshGroups.Count);
				}

				//모든 객체를 대상으로 Link를 한다.
				isLinkAllMeshGroups = linkRefreshRequest.Request_MeshGroup == apUtil.LR_REQUEST__MESHGROUP.AllMeshGroups;

				//모든 Anim 모디파이어를 생략한다. (그 외의 모디파이어는 생략하지 않음)
				isSkipAllAnimModifier = linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods;

				//특정 AnimClip에 대한 PSG를 제외한 나머지를 생략한다. (Anim 모디파이어 중에서)
				isSkipUnselectedAnimPSGs = linkRefreshRequest.Request_PSG == apUtil.LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier;
				selectedAnimClip = linkRefreshRequest.AnimClip;

				isNeedToRefreshOtherMeshGroups = (revMeshGroups.Count > 1);
				curSelectedMeshGroup = linkRefreshRequest.MeshGroup;
			}
			else
			{
				isLinkAllMeshGroups = true;//Request가 없다면 모든 객체를 대상으로 Link 수행
			}



			//<REV_MG>
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			for (int iMeshGroup = 0; iMeshGroup < revMeshGroups.Count; iMeshGroup++)
			{
				//apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

				meshGroup._modifierStack.RefreshAndSort(false);


				//Bone 연결 
				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				meshGroup._boneList_Root.Clear();
				if (meshGroup._boneList_All != null)
				{
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];
						if (bone._childBones == null)
						{
							bone._childBones = new List<apBone>();
						}
						bone._childBones.Clear();
					}
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone, this);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}

					//추가 5.9 : Bone의 Check Validation 함수를 호출해야 한다.
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						meshGroup._boneList_All[iBone].CheckIKControllerValidation();
					}
				}

				int curBoneIndex = 0;
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					apBone rootBone = meshGroup._boneList_Root[iRoot];
					//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
					rootBone.LinkRecursive(0);
					curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
				}

				

				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apModifierBase mod = modifiers[iMod];

					//추가 : Portrait를 연결해준다.
					mod.LinkPortrait(this);



					mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

					//삭제 조건1 - MeshGroup이 없다
					if (mod._meshGroup == null)
					{
						//Debug.LogError("No MeshGroup Modifier");
						continue;
					}

					//>> 최적화 20.4.3
					if(isSkipAllAnimModifier && mod.IsAnimated)
					{
						//요청에 따라 Anim 모디파이어는 생략하자
						continue;
					}

					
					List<apModifierParamSetGroup> paramSetGroups = mod._paramSetGroup_controller;
					for (int iPSGroup = 0; iPSGroup < paramSetGroups.Count; iPSGroup++)
					{
						apModifierParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];



						//>> 최적화 20.4.3 : 만약 애니메이션 설정시, 해당 애니메이션에 관련된 것들만 갱신한다.
						if (isSkipUnselectedAnimPSGs && mod.IsAnimated)
						{
							if (paramSetGroup._keyAnimClip != linkRefreshRequest.AnimClip)
							{
								
								//특정 AnimClip을 제외한 다른 AnimClip에 대한 ParamSetGroup은 Link를 생략한다.
								//Debug.LogError("Skip Link > " + paramSetGroup._keyAnimClip._name);
								continue;
							}
						}



						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								//paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamName);
								paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								{
									//Debug.LogError("TODO : KeyFrame 방식 연동");
									//추가 : AnimClip과 연동을 먼저 한다.
									// ParamSetGroup -> AnimClip과 연동
									paramSetGroup._keyAnimClip = GetAnimClip(paramSetGroup._keyAnimClipID);
									if (paramSetGroup._keyAnimClip == null)
									{
										paramSetGroup._keyAnimClipID = -1;//<<삭제 하자
										break;
									}

									paramSetGroup._keyAnimTimeline = paramSetGroup._keyAnimClip.GetTimeline(paramSetGroup._keyAnimTimelineID);

									if (paramSetGroup._keyAnimTimeline == null)
									{
										paramSetGroup._keyAnimTimelineID = -1;
										break;
									}

									paramSetGroup._keyAnimTimelineLayer = paramSetGroup._keyAnimTimeline.GetTimelineLayer(paramSetGroup._keyAnimTimelineLayerID);

									if (paramSetGroup._keyAnimTimelineLayer == null)
									{
										paramSetGroup._keyAnimTimelineLayerID = -1;
										break;
									}

									//추가) 상호 연동을 해주자
									paramSetGroup._keyAnimTimelineLayer.LinkParamSetGroup(paramSetGroup);

									//키프레임이면 여기서 한번더 링크를 해주자
									for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
									{
										apModifierParamSet paramSet = paramSetGroup._paramSetList[iPS];
										int keyframeID = paramSet._keyframeUniqueID;

										apAnimKeyframe targetKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(keyframeID);
										if (targetKeyframe != null)
										{
											paramSet.LinkSyncKeyframe(targetKeyframe);
										}
										else
										{
											//Debug.LogError("Keyframe 연동 에러 [" + keyframeID + "]");
											paramSet._keyframeUniqueID = -1;//못찾았다.
										}

									}
									int nPrevParamSet = paramSetGroup._paramSetList.Count;
									//"키프레임 연동" 방식에서 비어있는 키프레임이라면?
									int nRemoved = paramSetGroup._paramSetList.RemoveAll(delegate (apModifierParamSet a)
									{
										return a._keyframeUniqueID < 0;
									});
									if (nRemoved > 0)
									{
										//Debug.LogError(nPrevParamSet + "개 중 " + nRemoved + "개의 Keyframe 과 연동되지 못한 ParamSet 삭제");
									}

									//추가
								}
								break;
						}



						
						List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < paramSets.Count; iParamSet++)
						{
							apModifierParamSet paramSet = paramSets[iParamSet];

							
							//Link를 해주자
							paramSet.LinkParamSetGroup(paramSetGroup);

							
							List<apModifiedMesh> meshData = paramSet._meshData;
							apTransform_Mesh meshTransform = null;
							apTransform_MeshGroup meshGroupTransform = null;
							apRenderUnit renderUnit = null;

							
							//1. ModMesh
							for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
							{
								apModifiedMesh modMesh = meshData[iMesh];

								//추가 : Modifier의 meshGroup과 Transform의 MeshGroup을 분리한다.
								apMeshGroup meshGroupOfTransform = null;

								if (modMesh._isRecursiveChildTransform)
								{
									//MeshGroup이 다르다
									//Debug.Log("Link RecursiveChildTransform : " + modMesh._meshGroupUniqueID_Transform);

									meshGroupOfTransform = GetMeshGroup(modMesh._meshGroupUniqueID_Transform);
									//if (meshGroupOfTransform == null)
									//{
									//	Debug.LogError("Recursive Child Transfrom Missing");
									//}
								}
								else
								{
									//동일한 MeshGroup이다.
									meshGroupOfTransform = meshGroup;
								}
								
								modMesh._meshGroupUniqueID_Modifier = meshGroup._uniqueID;


								
								//변경 : 타입 대신 값을 보고 판단한다.
								if (modMesh._transformUniqueID >= 0 && meshGroupOfTransform != null)
								{
									if (modMesh._isMeshTransform)
									{
										meshTransform = meshGroupOfTransform.GetMeshTransform(modMesh._transformUniqueID);
										
										if (meshTransform != null)
										{
											renderUnit = meshGroup.GetRenderUnit(meshTransform);
											//if(renderUnit == null)
											//{
											//	Debug.LogError("Render Unit이 없다. : " + meshTransform._nickName);
											//}
											modMesh.Link_MeshTransform(meshGroup, meshGroupOfTransform, meshTransform, renderUnit, this);
										}
										//else
										//{
										//	Debug.LogError("MeshTransform이 없다. : " + modMesh._transformUniqueID);
										//}
									}
									else
									{
										meshGroupTransform = meshGroupOfTransform.GetMeshGroupTransform(modMesh._transformUniqueID);
										
										if (meshGroupTransform != null)
										{
											renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
											//if(renderUnit == null)
											//{
											//	Debug.LogError("Render Unit이 없다. : " + meshTransform._nickName);
											//}
											modMesh.Link_MeshGroupTransform(meshGroup, meshGroupOfTransform, meshGroupTransform, renderUnit);
										}
										//else
										//{
										//	Debug.LogError("MeshTransform이 없다. : " + modMesh._transformUniqueID);
										//}
									}
								}

#region [미사용 코드]
								//switch (modMesh._targetType)
								//{
								//	case apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform:
								//		{
								//			meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
								//			if (meshTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshTransform);
								//				modMesh.Link_VertexMorph(meshGroup, meshTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.MeshTransformOnly:
								//		{
								//			meshTransform = meshGroup.GetMeshTransform(modMesh._transformUniqueID);
								//			if (meshTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshTransform);
								//				modMesh.Link_MeshTransform(meshGroup, meshTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly:
								//		{
								//			meshGroupTransform = meshGroup.GetMeshGroupTransform(modMesh._transformUniqueID);
								//			if (meshGroupTransform != null)
								//			{
								//				renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
								//				modMesh.Link_MeshGroupTransform(meshGroup, meshGroupTransform, renderUnit);
								//			}
								//		}
								//		break;

								//	case apModifiedMesh.TARGET_TYPE.Bone:
								//		{
								//			//TODO : Bone 처리도 해주자
								//			modMesh.Link_Bone();
								//		}
								//		break;
								//} 
#endregion

							}
							
							

							//int nRemove = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
							paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
							{
								return a._meshGroupOfModifier == null || a._meshGroupOfTransform == null;
							});
							//if(nRemove > 0)
							//{
							//	Debug.LogError("Mesh Data가 삭제되었다. (1)");
							//}

							
							//---------------------------------------------------------------------------------
							//2. Bone 연동을 하자
							List<apModifiedBone> boneData = paramSet._boneData;
							apModifiedBone modBone = null;

							for (int iModBone = 0; iModBone < boneData.Count; iModBone++)
							{
								modBone = boneData[iModBone];
								apMeshGroup meshGroupOfBone = GetMeshGroup(modBone._meshGropuUniqueID_Bone);
								apMeshGroup meshGroupOfModifier = GetMeshGroup(modBone._meshGroupUniqueID_Modifier);
								if (meshGroupOfBone == null || meshGroupOfModifier == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [MeshGroup]");
									continue;
								}

								apBone bone = meshGroupOfBone.GetBone(modBone._boneID);
								if (bone == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패");
									continue;
								}

								meshGroupTransform = meshGroupOfModifier.GetMeshGroupTransformRecursive(modBone._transformUniqueID);
								//meshGroupTransform = meshGroupOfBone.GetMeshGroupTransform();
								if (meshGroupTransform == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [MeshGroup Transform]");
									continue;
								}

								//renderUnit = meshGroupOfBone.GetRenderUnit(meshGroupTransform);
								renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform._transformUniqueID, false);
								if (renderUnit == null)
								{
									//Debug.LogError("Link Error : Mod Bone 링크 실패 [Render Unit]");
									//continue;
									//다시 체크 및 보정
									if (meshGroupOfBone == meshGroupOfModifier)
									{
										meshGroupTransform = meshGroupOfModifier._rootMeshGroupTransform;
									}
									else
									{
										meshGroupTransform = meshGroupOfModifier.FindChildMeshGroupTransform(meshGroupOfBone);
									}

									if (meshGroupTransform != null)
									{
										renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform._transformUniqueID, false);
										//Debug.LogError("잘못된 ModBone 연결이 보정되었다.");
										modBone.Init(meshGroupOfModifier._uniqueID, meshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
									}
								}

								modBone.Link(meshGroupOfModifier, meshGroupOfBone, bone, renderUnit, meshGroupTransform);
							}

							//연동 안된 ModBone은 삭제하자
							//---------------------------------------------------------------------------------
							boneData.RemoveAll(delegate (apModifiedBone a)
							{
								return a._bone == null || a._meshGroup_Bone == null || a._meshGroup_Modifier == null;
							});

						}
					}

					
					//mod.RefreshParamSet();
					if(mod.IsAnimated)
					{
						//애니메이션 타입이라면 > 일부 AnimClip에 대한 처리하자.
						mod.RefreshParamSet(linkRefreshRequest);//<<단순 변경시 linkRefreshRequest로 입력 되어야 한다.
						//mod.RefreshParamSet(null);//<<삭제시 이게 null로 입력되어야 한다.
					}
					else
					{
						mod.RefreshParamSet(null);
					}	
					
				}

				
				meshGroup._modifierStack._modifiers.RemoveAll(delegate (apModifierBase a)
				{
					return a._meshGroup == null;
				});


				//ModStack의 CalculateParam을 모두 지우고 다시 만들자
				
				//이 조건문 추가 20.4.3 : 모든 메시 그룹에 대해서 Refresh를 할 경우에만
				//단, 여러개의 메시그룹을 대상으로 하는 경우에는 타겟을 제외한 나머지 객체는 Refresh를 해야한다.

				

				if(isLinkAllMeshGroups)
				{
					//Debug.Log(">>>> All MeshGroups (Other)");
					meshGroup.RefreshModifierLink(null);
				}
				else if(curSelectedMeshGroup != null 
					&& isNeedToRefreshOtherMeshGroups
					&& curSelectedMeshGroup != meshGroup)
				{
					//Debug.Log(">>>> 타겟이 아닌 다른 메시 그룹의 모디파이어를 Refresh [" + meshGroup._name + "]");
					meshGroup.RefreshModifierLink(linkRefreshRequest);
				}

			}
			if(curSelectedMeshGroup != null && !isLinkAllMeshGroups)
			{
				//모든 메시 그룹을 대상으로 한게 아닌데 대상 메시 그룹이 있다면
				curSelectedMeshGroup.RefreshModifierLink(linkRefreshRequest);
			}



			if (isSkipUnselectedAnimPSGs && linkRefreshRequest.AnimClip != null)
			{
				//특정 AnimClip만 검사
				linkRefreshRequest.AnimClip.LinkEditor(this);
				linkRefreshRequest.AnimClip.RemoveUnlinkedTimeline();
			}
			else
			{
				//전체 검사
				for (int i = 0; i < _animClips.Count; i++)
				{
					_animClips[i].LinkEditor(this);
					_animClips[i].RemoveUnlinkedTimeline();
				}
			}
			
			
			//추가 9.30 : 만약, 선택한 MeshGroup에 하위 MeshGroup이 있다면,
			//전체적으로 하위 MeshGroup으로의 연결을 다시 해야한다.
			//위에서 연결이 흐트러졌기 때문
			
			if(linkRefreshRequest != null && curSelectedMeshGroup != null)
			{
				//하위에 메시 그룹이 있거나, 모든 메시 그룹을 대상으로 하지 않았을 경우
				if((curSelectedMeshGroup._childMeshGroupTransforms != null && curSelectedMeshGroup._childMeshGroupTransforms.Count > 0)
					|| !isLinkAllMeshGroups)
				{
					curSelectedMeshGroup.LinkModMeshRenderUnits(linkRefreshRequest);
					curSelectedMeshGroup.RefreshModifierLink(linkRefreshRequest);
				}
				

				//if(curSelectedMeshGroup._childMeshGroupTransforms != null
				//	&& curSelectedMeshGroup._childMeshGroupTransforms.Count > 0)
				//{
				//	//중요 : LinkAndRefreshInEditor 이후에는 다음 함수들을 꼭 호출해야한다.
				//	curSelectedMeshGroup.LinkModMeshRenderUnits(linkRefreshRequest);
				//	curSelectedMeshGroup.RefreshModifierLink(linkRefreshRequest);
				//	//curSelectedMeshGroup._modifierStack.RefreshAndSort(true);//이건 필요 없는 듯 한다.
				//}
			}


			System.GC.Collect();
		}





		// Bake
		//----------------------------------------------------------------



		// 참조용 리스트 관리
		//----------------------------------------------------------------
		/// <summary>
		/// 계층적으로 설계된 MeshGroup에 맞게 리스트를 다시 정리하여 리턴한다.
		/// 인덱스 앞쪽에는 Child가 위치하고, 뒤로 갈 수록 Parent/Root가 나타난다.
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <returns></returns>
		private List<apMeshGroup> GetReverseMeshGroupList(List<apMeshGroup> srcMeshGroups)
		{
			List<apMeshGroup> resultList = new List<apMeshGroup>();

			for (int i = 0; i < srcMeshGroups.Count; i++)
			{
				apMeshGroup curMeshGroup = srcMeshGroups[i];
				if (!resultList.Contains(curMeshGroup))
				{
					FindReverseMeshGroupListRecursive(curMeshGroup, resultList);
				}
			}
			return resultList;
		}


		private void FindReverseMeshGroupListRecursive(apMeshGroup curMeshGroup, List<apMeshGroup> resultList)
		{
			if(curMeshGroup._childMeshGroupTransforms != null && curMeshGroup._childMeshGroupTransforms.Count > 0)
			{
				for (int iChild = 0; iChild < curMeshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					apTransform_MeshGroup childMeshGroupTransform = curMeshGroup._childMeshGroupTransforms[iChild];
					apMeshGroup childMeshGroup = childMeshGroupTransform._meshGroup;
					if (childMeshGroup != null && childMeshGroup != curMeshGroup)
					{
						FindReverseMeshGroupListRecursive(childMeshGroup, resultList);
					}
				}
			}
			if (!resultList.Contains(curMeshGroup))
			{
				resultList.Add(curMeshGroup);
			}
		}


		/// <summary>
		/// 추가 20.4.3 : 입력된 메시 그룹을 포함하여 관련된 모든 메시 그룹을 찾는다. (부모/자식 모두)
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="resultList"></param>
		private void FindAllParentAndChildrenMeshGroups(apMeshGroup targetMeshGroup, List<apMeshGroup> resultList)
		{
			//먼저 최상위 부모를 찾자
			apMeshGroup rootParentMG = targetMeshGroup;
			if (rootParentMG._parentMeshGroup != null)
			{
				while (true)
				{
					if(rootParentMG._parentMeshGroup == null)
					{
						break;
					}
					rootParentMG = rootParentMG._parentMeshGroup;
				}
			}
			
			//Recursive 방식ㅇ로 Root MG
			FindReverseMeshGroupListRecursive(rootParentMG, resultList);
		}


		// ID 관리
		//----------------------------------------------------------------
		//유니크 아이디는 몇가지 타입에 맞게 통합해서 관리한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ClearRegisteredUniqueIDs()
		{
			_IDManager.Clear();
		}

		// 발급된 ID는 관리를 위해 회수한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="target"></param>
		/// <param name="ID"></param>
		public void RegistUniqueID(apIDManager.TARGET target, int ID)
		{
			_IDManager.RegistID(target, ID);
		}
#region [미사용 코드]
		//public void RegistUniqueID_Texture(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Texture.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Texture.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Vertex(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Vert.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Vert.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Mesh(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Mesh.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Mesh.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_MeshGroup(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_MeshGroup.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_MeshGroup.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Transform(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Transform.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Transform.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Moifier(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Modifier.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Modifier.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_ControlParam(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_ControlParam.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_ControlParam.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_AnimClip(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_AnimClip.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_AnimClip.Add(uniqueID);
		//	}
		//} 
#endregion



		// 새로운 ID를 발급한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public int MakeUniqueID(apIDManager.TARGET target)
		{
			int resultID = _IDManager.MakeUniqueID(target);
			//수정 20.1.16 : 발급 즉시 등록하자 (중복 막기 위함)
			if(resultID >= 0)
			{
				RegistUniqueID(target, resultID);
			}
			return resultID;
		}
#region [미사용 코드]
		//private int MakeUniqueID(List<int> IDList)
		//{
		//	int nextID = -1;
		//	int cntCheck = 0;
		//	while(true)
		//	{
		//		nextID = UnityEngine.Random.Range(1000, 99999999);
		//		if(!IDList.Contains(nextID))
		//		{
		//			IDList.Add(nextID);
		//			return nextID;
		//		}

		//		cntCheck++;
		//		//회수 제한에 걸렸다.
		//		if(cntCheck > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 99999999; i++)
		//	{
		//		if(!IDList.Contains(i))
		//		{
		//			IDList.Add(i);
		//			return i;
		//		}
		//	}
		//	return -1;//<< 실패
		//}
		//public int MakeUniqueID_Texture()		{ return MakeUniqueID(_registeredUniqueIDs_Texture); }
		//public int MakeUniqueID_Vertex()		{ return MakeUniqueID(_registeredUniqueIDs_Vert); }
		//public int MakeUniqueID_Mesh()			{ return MakeUniqueID(_registeredUniqueIDs_Mesh); }
		//public int MakeUniqueID_MeshGroup()		{ return MakeUniqueID(_registeredUniqueIDs_MeshGroup); }
		//public int MakeUniqueID_Transform()		{ return MakeUniqueID(_registeredUniqueIDs_Transform); }
		//public int MakeUniqueID_Modifier()		{ return MakeUniqueID(_registeredUniqueIDs_Modifier); }
		//public int MakeUniqueID_ControlParam()	{ return MakeUniqueID(_registeredUniqueIDs_ControlParam); }
		//public int MakeUniqueID_AnimClip()		{ return MakeUniqueID(_registeredUniqueIDs_AnimClip); } 
#endregion


		// 객체 삭제시 ID 회수
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="target"></param>
		/// <param name="unusedID"></param>
		public void PushUnusedID(apIDManager.TARGET target, int unusedID)
		{
			_IDManager.PushUnusedID(target, unusedID);
		}


		//모든 ID를 리셋하고 다시 등록한다.
		//Undo용
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshAllUniqueIDs()
		{
			_IDManager.Clear();
			
			
			//1. Texture
			for (int i = 0; i < _textureData.Count; i++)
			{
				if(_textureData[i] == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.Texture, _textureData[i]._uniqueID);
			}

			//2. Mesh + Vertex
			for (int i = 0; i < _meshes.Count; i++)
			{
				if(_meshes[i] == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.Mesh, _meshes[i]._uniqueID);
				_meshes[i].RefreshVertexID();//<<Vertex ID를 등록한다.
			}

			//3. MeshGroup + Transform + Modifier + Bone
			apMeshGroup meshGroup = null;
			for (int i = 0; i < _meshGroups.Count; i++)
			{
				meshGroup = _meshGroups[i];
				if(meshGroup == null) { continue; }
				

				_IDManager.RegistID(apIDManager.TARGET.MeshGroup, meshGroup._uniqueID);


				//MeshGroup -> Transform
				apTransform_Mesh meshTF = null;
				for (int iMeshTF = 0; iMeshTF < meshGroup._childMeshTransforms.Count; iMeshTF++)
				{
					meshTF = meshGroup._childMeshTransforms[iMeshTF];
					if(meshTF == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Transform, meshTF._transformUniqueID);
				}

				apTransform_MeshGroup mgTF = null;
				for (int iMGTF = 0; iMGTF < meshGroup._childMeshGroupTransforms.Count; iMGTF++)
				{
					mgTF = meshGroup._childMeshGroupTransforms[iMGTF];
					if(mgTF == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Transform, mgTF._transformUniqueID);
				}

				if(meshGroup._rootMeshGroupTransform != null)
				{
					_IDManager.RegistID(apIDManager.TARGET.Transform, meshGroup._rootMeshGroupTransform._transformUniqueID);
				}

				//MeshGroup -> Modifier
				apModifierBase modifier = null;
				for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
				{
					modifier = meshGroup._modifierStack._modifiers[iMod];
					if(modifier == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Modifier, modifier._uniqueID);
				}

				apBone bone = null;
				for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
				{
					bone = meshGroup._boneList_All[iBone];
					if(bone == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Bone, bone._uniqueID);

				}
			}

			//4. Control Param
			apControlParam controlParam = null;
			for (int i = 0; i < _controller._controlParams.Count; i++)
			{
				controlParam = _controller._controlParams[i];
				if(controlParam == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.ControlParam, controlParam._uniqueID);
			}

			//5. AnimClip + AnimTimeline + AnimTimeline Layer + AnimKeyframe
			apAnimClip animClip = null;
			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			apAnimKeyframe keyframe = null;
			for (int iAnimClip = 0; iAnimClip < _animClips.Count; iAnimClip++)
			{
				animClip = _animClips[iAnimClip];
				if(animClip == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.AnimClip, animClip._uniqueID);

				//Timeline
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					timeline = animClip._timelines[iTimeline];
					if(timeline == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.AnimTimeline, timeline._uniqueID);

					//Timeline Layer
					for (int iTimelineLayer = 0; iTimelineLayer < timeline._layers.Count; iTimelineLayer++)
					{
						timelineLayer = timeline._layers[iTimelineLayer];
						if(timelineLayer == null) { continue; }

						_IDManager.RegistID(apIDManager.TARGET.AnimTimelineLayer, timelineLayer._uniqueID);

						//Keyframe
						for (int iKeyframe = 0; iKeyframe < timelineLayer._keyframes.Count; iKeyframe++)
						{
							keyframe = timelineLayer._keyframes[iKeyframe];
							if(keyframe == null)
							{
								continue;
							}

							_IDManager.RegistID(apIDManager.TARGET.AnimKeyFrame, keyframe._uniqueID);

						}
					}
				}
			}


		}



		// 추가 20.7.9 : 물리 타이머 갱신 (런타임/에디터 갱신시 호출할 것)
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void CalculatePhysicsTimer()
		{
			if(_physicsTimer == null)
			{
				_physicsTimer = new System.Diagnostics.Stopwatch();
				_physicsTimer.Start();
			}
			float nextDeltaTime = (float)(_physicsTimer.ElapsedMilliseconds / 1000.0f);
			if(nextDeltaTime > 0.0f)
			{
				_physicsDeltaTime = nextDeltaTime;
				if(_physicsDeltaTime > PHYSICS_MAX_DELTA_TIME)
				{
					_physicsDeltaTime = PHYSICS_MAX_DELTA_TIME;
				}
				_physicsTimer.Stop();
				_physicsTimer.Reset();
				_physicsTimer.Start();
			}
		}

		//화면 캡쳐시에는 물리 시간이 강제된다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void SetPhysicsTimerWhenCapture(float tDelta)
		{
			if(_physicsTimer == null)
			{
				_physicsTimer = new System.Diagnostics.Stopwatch();
				_physicsTimer.Start();
			}

			_physicsDeltaTime = tDelta;
			_physicsTimer.Stop();
			_physicsTimer.Reset();
			_physicsTimer.Start();
		}


#region [미사용 코드]
		//public void PushUniqueID_Texture(int uniquedID)			{ _registeredUniqueIDs_Texture.Remove(uniquedID); }
		//public void PushUniqueID_Vertex(int uniquedID)			{ _registeredUniqueIDs_Vert.Remove(uniquedID); }
		//public void PushUniqueID_Mesh(int uniquedID)			{ _registeredUniqueIDs_Mesh.Remove(uniquedID); }
		//public void PushUniqueID_MeshGroup(int uniquedID)		{ _registeredUniqueIDs_MeshGroup.Remove(uniquedID); }
		//public void PushUniqueID_Transform(int uniquedID)		{ _registeredUniqueIDs_Transform.Remove(uniquedID); }
		//public void PushUniqueID_Modifier(int uniquedID)		{ _registeredUniqueIDs_Modifier.Remove(uniquedID); }
		//public void PushUniqueID_ControlParam(int uniquedID)	{ _registeredUniqueIDs_ControlParam.Remove(uniquedID); }
		//public void PushUniqueID_AnimClip(int uniquedID)		{ _registeredUniqueIDs_AnimClip.Remove(uniquedID); } 
#endregion

		//카메라 관련
		//-------------------------------------------------------------------------------------------------------
		private void CheckAndRefreshCameras()
		{
			#region [미사용 코드] 이전 방식 : 단일 카메라만 지원
			//if(_billboardType == BILLBOARD_TYPE.None)
			//{
			//	_curCamera = null;
			//	_curCameraTransform = null;
			//	return;
			//}

			////빌보드라면 카메라를 계산해야한다.
			////일단 카메라 검사
			//if(_curCamera != null)
			//{
			//	//현재 카메라의 마스크가 현재 레이어를 참조하는가.
			//	if(_curCamera.cullingMask == (_curCamera.cullingMask | (1 << gameObject.layer)) && _curCamera.enabled)
			//	{
			//		//유지
			//	}
			//	else
			//	{
			//		//문제가 있다.
			//		_curCamera = null;
			//		_curCameraTransform = null;


			//	}
			//}
			//if(_curCamera == null)
			//{
			//	//씬에 있는 카메라를 가져온다.
			//	Camera[] cameras = Camera.allCameras;
			//	if(cameras == null || cameras.Length == 0)
			//	{	
			//		_curCamera = null;
			//		_curCameraTransform = null;
			//	}
			//	else
			//	{
			//		Camera cam = null;
			//		for (int i = 0; i < cameras.Length; i++)
			//		{
			//			cam = cameras[i];
			//			if(!cam.enabled)
			//			{
			//				continue;
			//			}
			//			if(cam.cullingMask == (cam.cullingMask | (1 << gameObject.layer)))
			//			{
			//				_curCamera = cam;
			//				_curCameraTransform = _curCamera.transform;
			//				//Debug.LogError("Camera Changed >> " + _curCamera.gameObject.name);
			//				break;
			//			}
			//		}
			//	}
			//}
			////체크 후에도 카메라가 없다면 패스
			//if(_curCamera == null)
			//{
			//	_zDepthOnPerspectiveCam = 0;
			//	return;
			//} 
			#endregion


			if(_mainCamera == null)
			{
				_mainCamera = new apOptMainCamera(this);
			}

			if(_transform == null)
			{
				_transform = transform;
			}

			//변경 2019.9.24 : 멀티 카메라도 지원하도록 래핑
			if(_billboardType == BILLBOARD_TYPE.None)
			{
				//빌보드가 아니라면 단순 카메라 리스트 검사(false, false)
				_mainCamera.Refresh(false, false);
				return;
			}

			
			//빌보드라면 카메라의 매트릭스까지 계산(false, true)
			_mainCamera.Refresh(false, true);

			if(_billboardType == BILLBOARD_TYPE.Billboard)
			{
				//전체 빌보드
				//_transform.rotation = _curCameraTransform.rotation;//이전
				_transform.rotation = _mainCamera.Rotation;//변경
			}
			else
			{
				//Up 고정 빌보드
				//_transform.rotation = Quaternion.LookRotation(_curCameraTransform.forward, Vector3.up);//이전
				_transform.rotation = Quaternion.LookRotation(_mainCamera.Forward, Vector3.up);//변경
			}

			
			//카메라 좌표계에서의 Z값 (ZDepth)
			//_zDepthOnPerspectiveCam = _curCamera.worldToCameraMatrix.MultiplyPoint3x4(_transform.position).z;//미사용 코드

			//TODO : 옵션으로 설정
			//여기선 Orthographic SortMode로 해야한다.
			//_curCamera.transparencySortMode = TransparencySortMode.Orthographic;

			//추가 20.9.15
			//Rotation 전용의 행렬/역행렬을 준비한다.
			_rotationOnlyMatrixIfBillboard = Matrix4x4.TRS(Vector3.zero, _transform.rotation, Vector3.one);
			_invRotationOnlyMatrixIfBillboard = _rotationOnlyMatrixIfBillboard.inverse;
		}

		//추가. 업데이트가 끝나면 이 함수를 호출하자.
		private void PostUpdate()
		{
			//빌보드인 경우, 현재 프레임에서의 위치를 저장한다. (나중에 "이전 프레임의 위치"로서 가져올 수 있게)
			_posW_Prev1F = _transform.position;
		}

		



		//이전
		//public Camera GetCamera()
		//{
		//	return _curCamera;
		//}

		//public float GetZDepth()
		//{
		//	return _zDepthOnPerspectiveCam;
		//}

		//변경
		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		/// <returns></returns>
		public apOptMainCamera GetMainCamera()
		{
			return _mainCamera;
		}


		//카메라를 직접 지정하는 함수
		/// <summary>
		/// Reset to automatically detect cameras in the scene.
		/// </summary>
		public void FindRenderingCamerasAutomatically()
		{
			if(_mainCamera == null)
			{
				Debug.LogError("AnyPortrait - The camera module has not been initialized yet.");
				return;
			}

			if (_mainCamera.SetRefreshAutomatically())
			{
				//기존에는 자동이 아니었는데 이번에 자동으로 갱신되도록 변경되었다.
				//>> 강제로 갱신
				_mainCamera.Refresh(true, _billboardType != BILLBOARD_TYPE.None);
			}
		}

		/// <summary>
		/// Manually set the cameras that render this character.
		/// When this function is called, the character does not automatically recognize the change even if the cameras in the scene change.
		/// Call the "FindRenderingCamerasAutomatically()" function to make the character automatically recognize the cameras again.
		/// </summary>
		/// <param name="cameras">Cameras placed in the scene you want to set</param>
		/// <returns>The number of cameras that can actually render the character. Returns -1 if an error occurs</returns>
		public int SetRenderingCameras(params Camera[] cameras)
		{
			if(_mainCamera == null)
			{
				Debug.LogError("AnyPortrait - The camera module has not been initialized yet.");
				return -1;
			}

			return _mainCamera.SetCameras(cameras);
		}
		


		//추가 20.9.15 : 지글본의 좌표계 변환 처리를 위한 특별 함수
		//이 함수의 내용은 지글본 코드(apOptBone)를 확인하자
		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		public Vector3 OffsetPos2World_Prev(Vector3 posOffset)
		{
			//Debug.Log("Offset Pos Check : Cur : " + _transform.position.x + " / Prev : " + _posW_Prev1F.x);
			return _rotationOnlyMatrixIfBillboard.MultiplyPoint3x4(posOffset) + _posW_Prev1F;
		}

		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		public Vector3 WorldPos2OffsetPos(Vector3 worldPos)
		{
			return _invRotationOnlyMatrixIfBillboard.MultiplyPoint3x4(worldPos - _transform.position);
		}


		// ID로 오브젝트 참조
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apTextureData GetTexture(int uniqueID)
		{
			return _textureData.Find(delegate (apTextureData a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apMesh GetMesh(int uniqueID)
		{
			return _meshes.Find(delegate (apMesh a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			return _meshGroups.Find(delegate (apMeshGroup a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apControlParam GetControlParam(int uniqueID)
		{
			return _controller._controlParams.Find(delegate (apControlParam a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		/// <summary>
		/// Get Control Parameter
		/// </summary>
		/// <param name="controlParamName">Control Parameter Name</param>
		/// <returns></returns>
		public apControlParam GetControlParam(string controlParamName)
		{
			return _controller._controlParams.Find(delegate (apControlParam a)
			{
				return string.Equals(a._keyName, controlParamName);
			});
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apAnimClip GetAnimClip(int uniqueID)
		{
			return _animClips.Find(delegate (apAnimClip a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		// ID로 오브젝트 참조 - RealTime
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="transformID"></param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(int transformID)
		{
			if (transformID < -1)
			{
				return null;
			}

			if (_optTransforms == null)
			{
				return null;
			}
			return _optTransforms.Find(delegate (apOptTransform a)
			{
				return a._transformID == transformID;
			});
		}


		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="meshGroupUniqueID"></param>
		/// <returns></returns>
		public apOptTransform GetOptTransformAsMeshGroup(int meshGroupUniqueID)
		{
			//Debug.Log("GetOptTransformAsMeshGroup [" + meshGroupUniqueID + "]");
			if (meshGroupUniqueID < 0)
			{
				Debug.LogError("ID < 0");
				return null;
			}
			if (_optTransforms == null)
			{
				Debug.LogError("OptTranforms is Null");
				return null;
			}

			//for (int i = 0; i < _optTransforms.Count; i++)
			//{
			//	Debug.Log("[" + i + "] : " + _optTransforms[i]._transformID + " / " + _optTransforms[i]._meshGroupUniqueID);
			//}

			return _optTransforms.Find(delegate (apOptTransform a)
			{
				return a._meshGroupUniqueID == meshGroupUniqueID;
			});
		}


		/// <summary>
		/// Get Root Unit with Index
		/// </summary>
		/// <param name="rootUnitIndex"></param>
		/// <returns></returns>
		public apOptRootUnit GetOptRootUnit(int rootUnitIndex)
		{
			if(_optRootUnitList.Count == 0)
			{
				return null;
			}
			if(rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				return null;
			}
			return _optRootUnitList[rootUnitIndex];
		}


		//추가 19.6.3 : MaterialSet에 관련
		public apMaterialSet GetMaterialSet(int uniqueID)
		{
			return _materialSets.Find(delegate(apMaterialSet a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apMaterialSet GetDefaultMaterialSet()
		{
			return _materialSets.Find(delegate (apMaterialSet a)
			{
				return a._isDefault;
			});
		}
	}

}