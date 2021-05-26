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
using System.Collections.Generic;
using System;
using System.IO;
using Ntreev.Library.Psd;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 : 6.23에 코드가 작성되었다.
	/// PSDDialog에서 모두 처리 되었던 PSD Load/Atlas Bake 처리를 이 클래스에서 한다.
	/// Reimport에서도 사용하기 위함
	/// </summary>
	public class apPSDLoader
	{
		// Members
		//--------------------------------------
		private apEditor _editor = null;

		public bool _isFileLoaded = false;
		public string _fileFullPath = "";
		public string _fileNameOnly = "";

		//저장 경로 (에셋)
		public string _bakeDstPath = "";
		public string _bakeDstPathRelative = "";

		public int _imageWidth = -1;
		public int _imageHeight = -1;
		public Vector2 _imageCenterPosOffset = Vector2.zero;

		private const int PSD_IMAGE_FILE_MAX_SIZE = 5000;

		private List<apPSDLayerData> _layerDataList = new List<apPSDLayerData>();

		//Bake 요청 정보
		private bool _bakeReq_IsCalculated = false;
		private int _bakeReq_Width = -1;
		private int _bakeReq_Height = -1;
		private int _bakeReq_MaxNumAtlas = 2;
		private int _bakeReq_Padding = 4;
		private bool _bakeReq_BlurOption = true;

		private List<apPSDBakeData> _bakeDataList = new List<apPSDBakeData>();
		private List<apPSDLayerBakeParam> _bakeParams = new List<apPSDLayerBakeParam>();

		
		//Calculate 계산 결과
		private int _bakeCal_SizePerIndex = 0;
		private int _bakeCal_AtlasCount = 0;//실제로 Bake된 Atlas
		private int _bakeCal_ResizeX100 = 100;

		//Bake 결과
		private FUNC_BAKE_RESULT _funcBakeResult = null;
		private object _loadKeyBeforeBake = null;

		private bool _bakeResult_IsBaked = false;
		private int _bakeResult_AtlasCount = 0;
		private int _bakeResult_BakeResizeX100 = 0;
		private int _bakeResult_Padding = 0;

		//Convert 결과
		private FUNC_CONVERT_RESULT _funcConvertResult = null;


		public enum LOAD_STEP
		{
			Step0_Ready,
			Step1_PSDLoaded,
			Step2_Calculated,
			Step3_Baked,
			Step4_ConvertToAnyPortrait
		}
		private LOAD_STEP _loadStep = LOAD_STEP.Step0_Ready;

		private apPSDProcess _workProcess = new apPSDProcess();
		private bool _isImageBaking = false;
		private string _curProcessLabel = "";

		private apPortrait _portrait = null;
		private apPSDSet _reimportPSDSet = null;
		private Dictionary<apTransform_Mesh, apPSDLayerData> _meshTransform2PSDLayer = null;

		private List<string> _bakedTextureAssetPathList = new List<string>();


		[Serializable]
		public enum BAKE_SIZE
		{
			s256 = 0,
			s512 = 1,
			s1024 = 2,
			s2048 = 3,
			s4096 = 4
		}

		
		// Init
		//--------------------------------------
		public apPSDLoader(apEditor editor)
		{
			_editor = editor;

			_loadStep = LOAD_STEP.Step0_Ready;
			Clear();
		}

		

		public void Clear()
		{
			_isFileLoaded = false;
			_fileFullPath = "";
			_fileNameOnly = "";
			_imageWidth = -1;
			_imageHeight = -1;
			_imageCenterPosOffset = Vector2.zero;
			_layerDataList.Clear();

			_bakeReq_Width = -1;
			_bakeReq_Height = -1;
			_bakeReq_MaxNumAtlas = 2;
			_bakeReq_Padding = 4;
			_bakeReq_BlurOption = true;

			_bakeDataList.Clear();
			_bakeParams.Clear();

			_bakeCal_SizePerIndex = 0;
			_bakeCal_AtlasCount = 0;//실제로 Bake된 Atlas
			_bakeCal_ResizeX100 = 100;

			_bakeReq_IsCalculated = false;
			_bakeResult_IsBaked = false;

			_workProcess.Clear();
			_isImageBaking = false;
			_curProcessLabel = "";
			
			_portrait = null;
			_reimportPSDSet = null;
			_meshTransform2PSDLayer = null;

			_bakedTextureAssetPathList.Clear();

		}


		public void CloseProcess()
		{
			_workProcess.Clear();
			_isImageBaking = false;
			_curProcessLabel = "";
		}


		// Update
		//----------------------------------------------------------------------
		public void Update()
		{
			if(_loadStep == LOAD_STEP.Step2_Calculated
				|| _loadStep == LOAD_STEP.Step3_Baked)
			{
				if(_workProcess.IsRunning)
				{
					_workProcess.Run();
					//_isImageBaking = true;
					if(!_workProcess.IsRunning)
					{
						_isImageBaking = false;
					}
				}
			}
		}

		// Functions
		//----------------------------------------------------------------------
		// Step 1 - Load PSD File
		//----------------------------------------------------------------------
		public bool Step1_LoadPSDFile(string filePath, string prevFileNameOnly)
		{
			_loadStep = LOAD_STEP.Step0_Ready;

			
			PsdDocument psdDoc = null;
			try
			{
				ClearPsdFile();

				psdDoc = PsdDocument.Create(filePath);
				if (psdDoc == null)
				{
					//EditorUtility.DisplayDialog("PSD Load Failed", "No File Loaded [" + filePath + "]", "Okay");
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
													_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadPath, filePath),
													_editor.GetText(TEXT.Close)
													);
					return false;
				}
				_fileFullPath = filePath;

				_fileNameOnly = "";

				if (_fileFullPath.Length > 4)
				{
					for (int i = _fileFullPath.Length - 5; i >= 0; i--)
					{
						string curChar = _fileFullPath.Substring(i, 1);
						if (curChar == "\\" || curChar == "/")
						{
							break;
						}
						_fileNameOnly = curChar + _fileNameOnly;
					}
				}

				if(!string.IsNullOrEmpty(prevFileNameOnly))
				{
					_fileNameOnly = prevFileNameOnly;
				}
				

				_imageWidth = psdDoc.FileHeaderSection.Width;
				_imageHeight = psdDoc.FileHeaderSection.Height;
				_imageCenterPosOffset = new Vector2((float)_imageWidth * 0.5f, (float)_imageHeight * 0.5f);

				if (_imageWidth > PSD_IMAGE_FILE_MAX_SIZE || _imageHeight > PSD_IMAGE_FILE_MAX_SIZE)
				{
					//EditorUtility.DisplayDialog("PSD Load Failed", 
					//	"Image File is Too Large [ " + _imageWidth + " x " + _imageHeight + " ] (Maximum 5000 x 5000)", 
					//	"Okay");

					EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
													_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadSize, _imageWidth, _imageHeight),
													_editor.GetText(TEXT.Close)
													);
					ClearPsdFile();
					return false;
				}

				int curLayerIndex = 0;

				RecursiveAddLayer(psdDoc.Childs, 0, null, curLayerIndex);

				//클리핑이 가능한가 체크
				CheckClippingValidation();

				_isFileLoaded = true;

				psdDoc.Dispose();
				psdDoc = null;
				System.GC.Collect();

				_loadStep = LOAD_STEP.Step1_PSDLoaded;
				return true;
			}
			catch (Exception ex)
			{
				ClearPsdFile();

				if (psdDoc != null)
				{
					psdDoc.Dispose();
					System.GC.Collect();
				}

				Debug.LogError("Load PSD File Exception : " + ex);

				//EditorUtility.DisplayDialog("PSD Load Failed", "Error Occured [" + ex.ToString() + "]", "Okay");
				EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
												_editor.GetTextFormat(TEXT.PSDBakeError_Body_ErrorCode, ex.ToString()),
												_editor.GetText(TEXT.Close)
												);

			}

			_loadStep = LOAD_STEP.Step0_Ready;
			return false;
		}



		private void ClearPsdFile()
		{
			Clear();

			//_isFileLoaded = false;
			//_fileFullPath = "";
			//_fileNameOnly = "";
			//_imageWidth = -1;
			//_imageHeight = -1;
			//_imageCenterPosOffset = Vector2.zero;

			//_layerDataList.Clear();
			//_selectedLayerData = null;

			//_bakeDataList.Clear();
			//_selectedBakeData = null;

			////_isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
			//_bakeWidth = BAKE_SIZE.s1024;
			//_bakeHeight = BAKE_SIZE.s1024;
			//_bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
			//_bakeMaximumNumAtlas = 2;
			//_bakePadding = 4;
			//_bakeBlurOption = true;

			//_isNeedBakeCheck = true;
			////_needBakeResizeX100 = 100;
			//_bakeParams.Clear();

			//_loadKey_CheckBake = null;
			//_loadKey_Bake = null;

			//_resultAtlasCount = 0;
			//_resultBakeResizeX100 = 0;
			//_resultPadding = 0;
		}


		



		private int RecursiveAddLayer(IPsdLayer[] layers, int level, apPSDLayerData parentLayerData, int curLayerIndex)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				IPsdLayer curLayer = layers[i];
				if (curLayer == null)
				{
					continue;
				}

				//추가 : 만약 이미지가 없다 + Child도 없다 -> 이건 패스한다.
				if(!curLayer.HasImage && (curLayer.Childs == null || curLayer.Childs.Length == 0))
				{
					//빈 레이어
					//Debug.Log("비어있는 레이어 : " + curLayer.Name);
					continue;
				}

				apPSDLayerData newLayerData = new apPSDLayerData(curLayerIndex, curLayer, _imageWidth, _imageHeight);
				
				newLayerData.SetLevel(level);
				if (parentLayerData != null)
				{
					parentLayerData.AddChildLayer(newLayerData);
				}

				curLayerIndex++;

				//재귀 호출을 하자
				if (curLayer.Childs != null && curLayer.Childs.Length > 0)
				{
					curLayerIndex = RecursiveAddLayer(curLayer.Childs, level + 1, newLayerData, curLayerIndex);
				}

				_layerDataList.Add(newLayerData);
			}
			return curLayerIndex;
		}


		public void CheckClippingValidation()
		{
			//Debug.Log("-- CheckClippingValidation --");
			//클리핑이 가능한가 체크
			//어떤 클리핑 옵션이 나올때
			//"같은 레벨에서" ㅁ CC[C] 까지는 Okay / ㅁCCC..[C]는 No
			//> 클리핑 개수 제한이 사라졌다.
			//앞으로 계속 검색 가능
			for (int i = 0; i < _layerDataList.Count; i++)
			{
				apPSDLayerData curLayerData = _layerDataList[i];
				curLayerData._isClippingValid = true;

				if (curLayerData._isImageLayer && curLayerData._isClipping)
				{	
					//앞으로 체크해보자.
					int curIndex = i;
					bool isClippingValid = false;

					//Debug.Log(">> 클리핑 체크 대상 : " + curLayerData._name + " / Index : " + curIndex + " / Hierarchy Level : " + curLayerData._hierarchyLevel);

					if (curIndex > 0)
					{
						for (int iPrevInex = curIndex - 1; iPrevInex >= 0; iPrevInex--)
						{
							apPSDLayerData prevLayerData = _layerDataList[iPrevInex];

							if(prevLayerData == null || prevLayerData == curLayerData)
							{
								continue;
							}

							//Debug.Log(" [" + iPrevInex + "] : " + prevLayerData._name + " / Hierarchy Level : " + prevLayerData._hierarchyLevel);

							if(prevLayerData._isBakable 
								&& prevLayerData._isImageLayer 
								&& !prevLayerData._isClipping 
								&& prevLayerData._hierarchyLevel == curLayerData._hierarchyLevel)
							{
								//한번이라도 Clipping이 가능하다면.. (Clipping할 대상이 있다)
								isClippingValid = true;
								break;
							}
						}
					}

					//Debug.Log("클리핑 결과 : " + isClippingValid);

					curLayerData._isClippingValid = isClippingValid;

					#region [미사용 코드 : 3개 제한의 Clipping 체크 코드]
					//apPSDLayerData prev1_Layer = null;
					//apPSDLayerData prev2_Layer = null;
					//apPSDLayerData prev3_Layer = null;

					//if (i - 1 >= 0)
					//{ prev1_Layer = _layerDataList[i - 1]; }
					//if (i - 2 >= 0)
					//{ prev2_Layer = _layerDataList[i - 2]; }
					//if (i - 3 >= 0)
					//{ prev3_Layer = _layerDataList[i - 3]; }

					//bool isValiePrev1 = (prev1_Layer != null && prev1_Layer._isBakable && prev1_Layer._isImageLayer && !prev1_Layer._isClipping && prev1_Layer._hierarchyLevel == curLevel);
					//bool isValiePrev2 = (prev2_Layer != null && prev2_Layer._isBakable && prev2_Layer._isImageLayer && !prev2_Layer._isClipping && prev2_Layer._hierarchyLevel == curLevel);
					//bool isValiePrev3 = (prev3_Layer != null && prev3_Layer._isBakable && prev3_Layer._isImageLayer && !prev3_Layer._isClipping && prev3_Layer._hierarchyLevel == curLevel);
					//if (isValiePrev1 || isValiePrev2 || isValiePrev3)
					//{
					//	curLayerData._isClippingValid = true;
					//}
					//else
					//{
					//	//Clipping의 대상이 없다면 문제가 있다.
					//	//Debug.LogError("Find Invalid Clipping [" + curLayerData._name + "]");
					//	curLayerData._isClippingValid = false;
					//} 
					#endregion
				}
			}
		}

		//----------------------------------------------------------------------
		// Step 2 - Calculate
		//----------------------------------------------------------------------
		public delegate void FUNC_CALCULTE_RESULT(bool isSuccess, object loadKey, bool isWarning, string warningMsg);
		public void Step2_Calculate(	string bakeDstPath,
										string bakeDstRelative,
										int bakeWidth,
										int bakeHeight,
										int bakeMaxNumAtlas,
										int bakePadding,
										bool bakeBlurOption,
										FUNC_CALCULTE_RESULT funcResult)
		{
			_bakeReq_IsCalculated = false;

			if(funcResult == null)
			{
				return;
			}
			if(_loadStep == LOAD_STEP.Step0_Ready || !_isFileLoaded)
			{
				
				funcResult(false, null, true, "[Error] Calculate Failed");
				return;
			}

			//Bake 설정 저장
			_bakeDstPath = bakeDstPath;
			_bakeDstPathRelative = bakeDstRelative;

			_bakeReq_Width = bakeWidth;
			_bakeReq_Height = bakeHeight;
			_bakeReq_MaxNumAtlas = bakeMaxNumAtlas;
			_bakeReq_Padding = bakePadding;
			_bakeReq_BlurOption = bakeBlurOption;

			_bakeCal_SizePerIndex = -1;
			_bakeCal_AtlasCount = 0;
			_bakeCal_ResizeX100 = 100;

			if (_bakeReq_MaxNumAtlas <= 0)
			{
				funcResult(false, null, true, "[Maximum Atlas] is less than 0");
				return;
			}

			if (_bakeReq_Padding < 0)
			{
				funcResult(false, null, true, "[Padding] is less than 0");
				return;
			}

			//1. Path 미지정
			if (string.IsNullOrEmpty(_bakeDstPath))
			{
				funcResult(false, null, true, "[Save Path] is Empty");
				return;
			}

			//2. 크기를 비교하자
			//W,H 합계, 최대값, 최소값, 영역 전체의 합
			int nLayer = 0;
			int sumWidth = 0;
			int sumHeight = 0;
			int maxWidth = -1;
			int maxHeight = -1;
			int minWidth = -1;
			int minHeight = -1;
			double sumArea = 0;

			apPSDLayerData curLayer = null;
			List<apPSDLayerData> bakableLayersX = new List<apPSDLayerData>(); //X축 큰거부터 체크
			List<apPSDLayerData> bakableLayersY = new List<apPSDLayerData>(); //Y축 큰거부터 체크
			for (int i = 0; i < _layerDataList.Count; i++)
			{
				curLayer = _layerDataList[i];
				if (!curLayer._isBakable || curLayer._image == null)
				{
					continue;
				}
				bakableLayersX.Add(curLayer);
				bakableLayersY.Add(curLayer);
				nLayer++;
				int curWidth = curLayer._width + (_bakeReq_Padding * 2);
				int curHeight = curLayer._height + (_bakeReq_Padding * 2);

				sumWidth += curWidth;
				sumHeight += curHeight;

				if (maxWidth < 0 || curWidth > maxWidth)
				{
					maxWidth = curWidth;
				}
				if (maxHeight < 0 || curHeight > maxHeight)
				{
					maxHeight = curHeight;
				}
				if (minWidth < 0 || curWidth < minWidth)
				{
					minWidth = curWidth;
				}
				if (minHeight < 0 || curHeight < minHeight)
				{
					minHeight = curHeight;
				}
				sumArea += (curWidth * curHeight);
			}

			//_needBakeResizeX100 = 100;

			_bakeParams.Clear();

			if (sumWidth < 10 || sumHeight < 10)
			{
				funcResult(false, null, true, "[Image Size] is Too Small");
				return;
			}

			//이제 본격적으로 만들어보자
			//Slot이라는 개념을 만들자.
			//Slot의 최소 크기는 최소 W,H의 크기값을 기준으로 한다.
			//minWH의 1/10의 값을 기준으로 4~32의 값을 가진다.


			//시작 Resize 값을 결정한다. (기본 100)
			//만약 maxWH가 요청한 Bake사이즈보다 크다면 -> 그만큼 리사이즈를 먼저 한다.
			//반복 수행
			//만약 maxWH가 요청한 Bake 사이즈보다 작다면 리사이즈가 없다는 가정으로 정사각형으로 만든다. (최대 Atlas 결과값이 오버가 되면 리사이즈를 해서 다시 수행한다.)
			int curResizeX100 = 100;
			float curResizeRatio = 1.0f;
			int slotSize = Mathf.Clamp((Mathf.Min(minWidth, minHeight) / 10), 4, 32);
			_bakeCal_SizePerIndex = slotSize;//<<이 값을 곱해서 실제 위치를 구한다.

			int numSlotAxisX = _bakeReq_Width / slotSize;
			int numSlotAxisY = _bakeReq_Height / slotSize;

			bool isSuccess = false;

			float baseRatioW = (float)_bakeReq_Width / (float)maxWidth;
			float baseRatioH = (float)_bakeReq_Height / (float)maxHeight;
			int baseRatioX100 = (int)((Mathf.Max(baseRatioW, baseRatioH) + 0.5f) * 100.0f);

			if (baseRatioX100 % 5 != 0)
			{
				baseRatioX100 = ((baseRatioX100 + 5) / 5) * 5;
			}

			if (baseRatioX100 < 100)
			{
				//maxW 또는 maxH가 이미지 크기를 넘었다.
				//리사이즈를 해야한다.
				//스케일은 5단위로 한다.
				curResizeX100 = baseRatioX100;
			}

			List<int[,]> atlasSlots = new List<int[,]>();
			for (int i = 0; i < _bakeReq_MaxNumAtlas; i++)
			{
				atlasSlots.Add(new int[numSlotAxisX, numSlotAxisY]);
			}

			//크기가 큰 이미지부터 내림차순
			bakableLayersX.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
			{
				return b._width - a._width;
			});

			bakableLayersY.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
			{
				return b._height - a._height;
			});


			List<apPSDLayerData> checkLayersX = new List<apPSDLayerData>();
			List<apPSDLayerData> checkLayersY = new List<apPSDLayerData>();
			while (true)
			{
				curResizeRatio = (float)curResizeX100 / 100.0f;
				if (curResizeX100 < 10)
				{
					isSuccess = false;
					break;//실패다.
				}

				//일단 슬롯을 비워두자
				for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
				{
					int[,] slots = atlasSlots[iAtlas];
					for (int iX = 0; iX < numSlotAxisX; iX++)
					{
						for (int iY = 0; iY < numSlotAxisY; iY++)
						{
							slots[iX, iY] = -1;//<인덱스 할당 정보 초기화
						}
					}
				}

				//계산할 LayerData를 다시 리셋하자.
				checkLayersX.Clear();
				checkLayersY.Clear();
				for (int i = 0; i < bakableLayersX.Count; i++)
				{
					checkLayersX.Add(bakableLayersX[i]);
					checkLayersY.Add(bakableLayersY[i]);
				}

				_bakeParams.Clear();

				bool isCheckX = true;
				//X, Y축을 번갈아가면서 체크한다.
				//X일땐 X축부터 체크하면서 빈칸을 채운다.


				apPSDLayerData nextLayer = null;

				isSuccess = false;
				while (true)
				{
					if (checkLayersX.Count == 0 || checkLayersY.Count == 0)
					{
						//다 넣었다.
						isSuccess = true;
						break;
					}

					//다음 Layer를 꺼내서 슬롯을 체크한다.
					if (isCheckX)
					{
						nextLayer = checkLayersX[0];
					}
					else
					{
						nextLayer = checkLayersY[0];
					}

					//꺼낸 값은 Layer에서 삭제한다.
					checkLayersX.Remove(nextLayer);
					checkLayersY.Remove(nextLayer);


					int layerIndex = nextLayer._layerIndex;
					//Slot Width, Height를 계산하자
					int slotWidth = (int)(((float)nextLayer._width * curResizeRatio) + (_bakeReq_Padding * 2)) / slotSize;
					int slotHeight = (int)(((float)nextLayer._height * curResizeRatio) + (_bakeReq_Padding * 2)) / slotSize;
					//이제 빈칸을 찾자!

					//Atlas 앞부터 시작해서
					//Check X인 경우는 : Y -> X순서
					//Check Y인 경우는 : X -> Y순서
					bool isAddedSuccess = false;
					int iAddedX = -1;
					int iAddedY = -1;
					int iAddedAtlas = -1;
					for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
					{
						int[,] slots = atlasSlots[iAtlas];


						bool addResult = false;


						if (isCheckX)
						{
							//X먼저 계산할 때
							for (int iY = 0; iY < numSlotAxisY; iY++)
							{
								for (int iX = 0; iX < numSlotAxisX; iX++)
								{
									addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
									if (addResult)
									{
										iAddedX = iX;
										iAddedY = iY;
										iAddedAtlas = iAtlas;
										break;
									}
								}

								if (addResult)
								{ break; }
							}
						}
						else
						{
							//Y먼저 계산할 때
							for (int iX = 0; iX < numSlotAxisX; iX++)
							{
								for (int iY = 0; iY < numSlotAxisY; iY++)
								{
									addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
									if (addResult)
									{
										iAddedX = iX;
										iAddedY = iY;
										iAddedAtlas = iAtlas;
										break;
									}
								}

								if (addResult)
								{ break; }
							}
						}

						if (addResult)
						{
							isAddedSuccess = true;
							break;
						}
					}

					if (isAddedSuccess)
					{
						//TODO : BakeParam에 Leftover인지 아닌지 여부 설정해야함

						//적당히 넣었다.
						apPSDLayerBakeParam newBakeParam = new apPSDLayerBakeParam(nextLayer, iAddedAtlas, iAddedX, iAddedY, false);
						_bakeParams.Add(newBakeParam);

						//실제로 작성된 Atlas의 개수를 확장한다.
						if (iAddedAtlas + 1 > _bakeCal_AtlasCount)
						{
							_bakeCal_AtlasCount = iAddedAtlas + 1;
						}
					}
					else
					{
						//하나라도 실패하면 돌아간다.
						isSuccess = false;
						break;
					}


					isCheckX = !isCheckX;//토글!
										 //다음 이미지를 넣어보자 -> 루프
				}

				//모두 넣었다면
				if (isSuccess)
				{
					break;
				}

				curResizeX100 -= 5;
			}

			if (nLayer > 0 && _bakeCal_AtlasCount == 0)
			{
				isSuccess = false;
				//_isBakeWarning = true;
				//_bakeWarningMsg = "No Baked Atlas";
				funcResult(false, null, true, "No Atlas to be baked");
				return;
			}


			if (!isSuccess)
			{
				//_isBakeWarning = true;//<이게 True이면 Bake가 불가능하다.
				//_bakeWarningMsg = "Need to increase [Number of Maximum Atlas]";
				funcResult(false, null, true, "Need to increase [Number of Maximum Atlas]");
				return;
			}

			_bakeCal_ResizeX100 = curResizeX100;
			//_loadKey_CheckBake = new object();//마지막으로 Bake Check가 끝났다는 Key를 만들어주자
			_loadStep = LOAD_STEP.Step2_Calculated;//<<처리 끝

			_bakeReq_IsCalculated = true;

			funcResult(true, new object(), false, "");
			
		}


		//슬롯에 레이어를 넣을 수 있는지 확인하자
		private bool AddToSlot(int startPosX, int startPosY, int slotWidth, int slotHeight, int[,] targetSlot, int slotSizeX, int slotSizeY, int addedLayerIndex)
		{
			if (targetSlot[startPosX, startPosY] >= 0)
			{
				//시작점에 뭔가가 있다.
				return false;
			}

			if (startPosX + slotWidth >= slotSizeX ||
				startPosY + slotHeight >= slotSizeY)
			{
				//영역을 벗어난다.
				return false;
			}

			for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
			{
				for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
				{
					if (targetSlot[iX, iY] >= 0)
					{
						return false;//뭔가가 있다.
					}
				}
			}

			//넣어봤는데 괜찮네요
			for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
			{
				for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
				{
					targetSlot[iX, iY] = addedLayerIndex;
				}
			}
			return true;
		}



		//----------------------------------------------------------------------
		// Step 3 - Bake
		//----------------------------------------------------------------------
		public delegate void FUNC_BAKE_RESULT(bool isSuccess, object loadKey);
		public bool Step3_Bake(object loadKey_Bake, FUNC_BAKE_RESULT funcBakeResult, object loadKey_Calculated)
		{
			if(loadKey_Calculated == null)
			{
				//Debug.Log("Bake Error [ loadKey_Calculated is Null ]");
				return false;
			}
			if(!_bakeReq_IsCalculated)
			{
				//Debug.Log("Bake Error [ _bakeReq_IsCalculated is False ]");
				return false;
			}
			if(_bakeCal_SizePerIndex <= 0 || _bakeCal_ResizeX100 <= 0)
			{
				//Debug.Log("Bake Error [ _bakeCal_SizePerIndex : " + _bakeCal_SizePerIndex + " / _bakeCal_ResizeX100 : " +  _bakeCal_ResizeX100 + " ]");
				return false;
			}

			CloseProcess();

			//Debug.Log("Start Bake Work Process");
			_bakeResult_IsBaked = false;
		
			_curProcessLabel = "Bake Atlas..";
			_funcBakeResult = funcBakeResult;
			_loadKeyBeforeBake = loadKey_Calculated;

			_bakeDataList.Clear();

			_workProcess.Add(Work_Bake_1, _bakeCal_AtlasCount);
			_workProcess.Add(Work_Bake_2, _bakeParams.Count);
			_workProcess.Add(Work_Bake_3, _bakeDataList.Count);
			_workProcess.Add(Work_Bake_4, 1);

			_workProcess.StartRun("Bake Atlas");
			_isImageBaking = true;

			return true;
		}


		private bool Work_Bake_1(int index)
		{
			if (_bakeCal_SizePerIndex <= 0 || _bakeCal_ResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _bakeCal_AtlasCount)
			{
				Debug.LogError("Work_Bake_1 Exception : Index Over (" + index + " / " + _bakeCal_AtlasCount + ")");
				return false;
			}

			//Bake Data를 만들자.
			//Process Index당 한개씩
			apPSDBakeData newBakeData = new apPSDBakeData(index, _bakeReq_Width, _bakeReq_Height, false);//<기본은 PSD에 있는 것만
			newBakeData.ReadyToBake();
			_bakeDataList.Add(newBakeData);

			//WorkProcess 갱신
			_workProcess.ChangeCount(2, _bakeDataList.Count);
			return true;

		}

		private bool Work_Bake_2(int index)
		{	
			if (_bakeCal_SizePerIndex <= 0 || _bakeCal_ResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _bakeParams.Count)
			{
				Debug.LogError("Work_Bake_2 Exception : Index Over (" + index + " / " + _bakeParams.Count + ")");
				return false;
			}

			float bakeResizeRatio = Mathf.Clamp01(((float)_bakeCal_ResizeX100 / 100.0f));

			apPSDLayerBakeParam bakeParam = _bakeParams[index];
			apPSDLayerData targetLayer = bakeParam._targetLayer;
			if (targetLayer._image == null)
			{
				Debug.LogError("Work_Bake_2 : No Image");
				return true;
			}

			//일단 레이어에 Bake 정보를 입력하자
			targetLayer._bakedAtalsIndex = bakeParam._atlasIndex;
			targetLayer._bakedImagePos_Left = bakeParam._posOffset_X * _bakeCal_SizePerIndex;
			targetLayer._bakedImagePos_Top = bakeParam._posOffset_Y * _bakeCal_SizePerIndex;
			targetLayer._bakedWidth = (int)((float)targetLayer._width * bakeResizeRatio + 0.5f);
			targetLayer._bakedHeight = (int)((float)targetLayer._height * bakeResizeRatio + 0.5f);

			//Bake Image에 값을 넣자
			apPSDBakeData targetBakeData = _bakeDataList[bakeParam._atlasIndex];
			bool isResult = targetBakeData.AddImage(targetLayer,
														targetLayer._bakedImagePos_Left,
														targetLayer._bakedImagePos_Top,
														bakeResizeRatio,
														targetLayer._bakedWidth,
														targetLayer._bakedHeight,
														_bakeReq_Padding);

			//Debug.Log("Bake [AddImage] : " + index + " >> " + bakeParam._atlasIndex);

			return isResult;
		}

		private bool Work_Bake_3(int index)
		{
			//if (_loadKey_CheckBake == null)
			//{
			//	return false;
			//}
			if (_bakeCal_SizePerIndex <= 0 || _bakeCal_ResizeX100 <= 0)
			{
				return false;
			}
			if (index >= _bakeDataList.Count)
			{
				Debug.LogError("Work_Bake_3 Exception : Index Over (" + index + " / " + _bakeDataList.Count + ")");
				return false;
			}

			//이제 실제로 Texture2D로 바꾸어주자
			_bakeDataList[index].EndToBake(_bakeReq_BlurOption, _bakeReq_Padding);
			//Debug.Log("EndToBake : " + index);
			return true;
		}

		private bool Work_Bake_4(int index)
		{
			if(_funcBakeResult != null)
			{
				_funcBakeResult(true, _loadKeyBeforeBake);
			}
			//_loadKey_Bake = _loadKey_CheckBake;//체크했던 Bake 값이 같음을 설정해주자
			_bakeResult_AtlasCount = _bakeCal_AtlasCount;
			_bakeResult_BakeResizeX100 = _bakeCal_ResizeX100;
			_bakeResult_Padding = _bakeReq_Padding;

			_loadStep = LOAD_STEP.Step3_Baked;
			_bakeResult_IsBaked = true;

			return true;
		}





		//----------------------------------------------------------------------
		// Step 4 - ConvertToAnyPortrait
		//----------------------------------------------------------------------
		public delegate void FUNC_CONVERT_RESULT(bool isSuccess);
		public bool Step4_ConvertToAnyPortrait(FUNC_CONVERT_RESULT funcConvertResult, apPortrait portrait, apPSDSet reimportPSDSet, Dictionary<apTransform_Mesh, apPSDLayerData> meshTransform2PSDLayer)
		{
			CloseProcess();

			_funcConvertResult = funcConvertResult;
			_portrait = portrait;
			_reimportPSDSet = reimportPSDSet;
			_meshTransform2PSDLayer = meshTransform2PSDLayer;

			_bakedTextureAssetPathList.Clear();

			_curProcessLabel = "Convert PSD Data to Editor..";
			_workProcess.Add(Work_BakdImageSave_0, 1);//<<추가
			_workProcess.Add(Work_BakedImageSave_1, _bakeDataList.Count);
			_workProcess.Add(Work_BakedImageSave_2, _bakeDataList.Count);
			_workProcess.Add(Work_BakedImageSave_3, _bakeDataList.Count);
			_workProcess.Add(Work_BakedImageSave_4, 1);
			_workProcess.StartRun("Convert PSD Data To Editor");

			return true;
		}

		private bool Work_BakdImageSave_0(int index)
		{
			
			//OnLoadComplete(true);
			
			//예상되는 Bake Texture에 대해서
			//만약 겹치는 TextureData가 있다면 적절히 처리해야한다.
			//1. New Bake인 경우
			//- 이름이 같은 TextureData가 있다면, 새로운 TextureData의 이름을 바꾸어 중복을 피한다.

			//2. Reimport인 경우 (portrait, reimportSet이 있고 연결되는 linkedMeshGroup이 있다. 그 외에는 NewBake)
			//덮어씌워질(=삭제되고 그 이름으로 새 Atlas가 저장되는) TextureData를 찾는다.
			// - apSet에 선택된 TextureData를 검색한다.
			// - 선택된 MeshGroup의 모든 MeshTransform(Mesh)에 대해서, 각각의 현재 연결된 TextureData를 찾는다.
			// 1) Reimport 되지 않는 Mesh가 하나도 없는 (=완벽하게 덮어 씌워질) 텍스쳐
			//		: 텍스쳐 파일의 이름과 경로를 바꾼다. ("Unused" 폴더 만들고, 파일 이름에 "_Unused" 추가 및 중복 없게)
			//		: TextureData는 삭제한다.
			//		: "덮어쓰기 가능한 텍스쳐 에셋 이름"에 추가한다.
			//		: "삭제할 이미지 에셋"에 저장하고 나중에 다이얼로그로 확인하여 직접 삭제
			// 2) Reimport 되지 않는 Mesh가 하나라도 참조한 텍스쳐
			//		: 텍스쳐 파일의 이름은 유지한다.
			//		: TextureData는 유지한다.
			// - 새로 추가되는 TextureData는 "덮어쓰기 가능한 텍스쳐 에셋 이름" 또는 "폴더에서 사용 가능한 이름"으로 설정한다.
			
			//작업 목표는 "이름을 바꾸고 파일 경로를 Unused로 옮긴뒤, 삭제될 수도 있는 텍스쳐"를 구분하는 것
			//그 뒤에, 이름 중복 없이 텍스쳐 생성하는 것이 작업 과정이다.
			
			if(_portrait == null || _reimportPSDSet == null || _reimportPSDSet._linkedTargetMeshGroup == null || _meshTransform2PSDLayer == null)
			{
				return true;//>>Reimport가 안되므로 걍 진행한다.
			}

			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Image_PSDImport, _editor, _portrait, _portrait, false);

			apMeshGroup targetMeshGroup = _reimportPSDSet._linkedTargetMeshGroup;
			List<apTextureData> texDataList_Target = new List<apTextureData>();

			for (int i = 0; i < _reimportPSDSet._targetTextureDataList.Count; i++)
			{
				apTextureData linkedTexData = _reimportPSDSet._targetTextureDataList[i]._linkedTextureData;
				if(linkedTexData != null && !texDataList_Target.Contains(linkedTexData))
				{
					texDataList_Target.Add(linkedTexData);
				}
			}

			
			List<apTextureData> preservedTexDataList = new List<apTextureData>();//<<Mesh 하나라도 Reimport되지 않다면 이 리스트에 추가된다.
			//List<apTextureData> perfectPreservedTexDataList = new List<apTextureData>();//<<Mesh가 모두 Reimport되지 않다면 이 리스트에 추가된다. Reimport와 전혀 연관성이 없다.
			List<apTextureData> unusedTexDataList = new List<apTextureData>();//<<Remapped Atlas에 의해서 덮어 씌워질 텍스쳐 데이터이다.
			//List<apTextureData> partPreservedTexDataList = new List<apTextureData>();//Perfect가 아닌 "Mesh의 일부가 Reimport되지 않고, 일부는 Reimport되는" 텍스쳐 데이터. Preserved 폴더에 추가된다.


			apRenderUnit curRenderUnit = null;
			apTransform_Mesh curMeshTransform = null;
			apTextureData curTexData = null;


			////Perfect Preserved 리스트에는 넣자
			//for (int i = 0; i < texDataList_Target.Count; i++)
			//{
			//	perfectPreservedTexDataList.Add(texDataList_Target[i]);
			//}

			for (int i = 0; i < targetMeshGroup._renderUnits_All.Count; i++)
			{
				curRenderUnit = targetMeshGroup._renderUnits_All[i];
				if(curRenderUnit == null || curRenderUnit._meshTransform == null)
				{
					continue;
				}
				curMeshTransform = curRenderUnit._meshTransform;

				//이 MeshTransform이 Remap 대상인가
				if(_meshTransform2PSDLayer.ContainsKey(curMeshTransform))
				{
					//Remap 대상이다.
					////Perfect Preserved TexData에서 제외하자
					//if(curMeshTransform._mesh != null && curMeshTransform._mesh._textureData_Linked != null)
					//{
					//	curTexData = curMeshTransform._mesh._textureData_Linked;
					//	if (!perfectPreservedTexDataList.Contains(curTexData))
					//	{
					//		perfectPreservedTexDataList.Remove(curTexData);
					//	}
					//}
					
				}
				else
				{
					//Remap 대상이 아니다.
					//이 MeshTransform이 참조하는 TextureData를 reservedTexDataList에 넣는다.
					//단, Remap 대상이 되는 TextureData여야 한다.
					if(curMeshTransform._mesh != null && curMeshTransform._mesh._textureData_Linked != null)
					{
						curTexData = curMeshTransform._mesh._textureData_Linked;
						if(!preservedTexDataList.Contains(curTexData) && texDataList_Target.Contains(curTexData))
						{
							preservedTexDataList.Add(curTexData);
						}
					}
				}
			}

			////Part Preserved 리스트를 만들자
			////Preserved - Perfect_Preserved
			//for (int i = 0; i < preservedTexDataList.Count; i++)
			//{
			//	curTexData = preservedTexDataList[i];
			//	if(!perfectPreservedTexDataList.Contains(curTexData))
			//	{
			//		//Preserved이지만 Perfect가 아니라면 "전체가 아닌 일부가 Reimport 되는 것"이다.
			//		partPreservedTexDataList.Add(curTexData);
			//	}
				
			//}

			for (int i = 0; i < texDataList_Target.Count; i++)
			{
				//Reserved TexData List에 없는 Texture들은 이동/삭제될 것들이다.
				curTexData = texDataList_Target[i];
				if(!preservedTexDataList.Contains(curTexData))
				{
					unusedTexDataList.Add(curTexData);
				}
			}

			//unusedTexDataList 가 완성되었다.
			//이 파일들은 일단 이동하자.
			//각 파일들의 폴더의 하위에 "Unused" 폴더를 확인하고, 없으면 생성
			//그 폴더로 이동한다.
			//이동할 시, 폴더 내의 이름 중복이 없도록 한다.
			Texture2D curTexture = null;
			




			//1) Unused Texture를 Unused 폴더로 이동한다.
			//2) Part Preserved Texture를 PartlyPreserved 폴더로 이동한다.
			if (unusedTexDataList.Count > 0)
			{
				string usedFolder = CheckAndMakeFolder(_bakeDstPathRelative, "Unused");
				for (int i = 0; i < unusedTexDataList.Count; i++)
				{
					curTexture = unusedTexDataList[i]._image;

					#region [미사용 코드 : Move Asset 함수로 통합되었다]
					//if(curTexture == null)
					//{
					//	//Debug.LogError("이미지가 없다.");
					//	continue;
					//}

					//string assetPath = AssetDatabase.GetAssetPath(curTexture);
					//if(string.IsNullOrEmpty(assetPath))
					//{
					//	//Debug.LogError("경로문제 1");
					//	continue;
					//}

					//int iLastSlash = assetPath.LastIndexOf("/");
					//if(iLastSlash < 0)
					//{
					//	//Debug.LogError("경로문제 2");
					//	continue;
					//}
					//string fileNameWithExp = assetPath.Substring(iLastSlash + 1);
					//string baseFolder = assetPath.Substring(0, iLastSlash);
					//string unusedFolderPath = assetPath.Substring(0, iLastSlash) + "/Unused";
					//if(!AssetDatabase.IsValidFolder(unusedFolderPath))
					//{
					//	//Debug.Log("폴더가 없다. 폴더를 추가한다. [" + unusedFolderPath + "]");
					//	AssetDatabase.CreateFolder(baseFolder, "Unused");
					//	AssetDatabase.Refresh();
					//}
					//AssetFileDataSet newPathSet = MakeUniqueAssetFileName(unusedFolderPath, fileNameWithExp, " (Unused)");
					//string newPath = newPathSet._fullPath;
					////AssetDatabase.MoveAsset()
					//if(string.IsNullOrEmpty(newPath))
					//{
					//	Debug.LogError("파일이름 생성 에러 [" + newPath + "]");
					//	continue;
					//}

					////파일을 Unused 폴더로 이동한다.
					//AssetDatabase.MoveAsset(assetPath, newPath); 
					#endregion

					string movedPath = MoveAsset(curTexture, usedFolder, " (Unused)");
					if (string.IsNullOrEmpty(movedPath))
					{
						//에러 발생
						continue;
					}

					//이 TexData를 Portrait에서 제거한다.
					_portrait._textureData.Remove(unusedTexDataList[i]);
				}
			}

			if (preservedTexDataList.Count > 0)
			{
				string preservedFolder = CheckAndMakeFolder(_bakeDstPathRelative, "Preserved");
				for (int i = 0; i < preservedTexDataList.Count; i++)
				{
					curTexture = preservedTexDataList[i]._image;

					string movedPath = MoveAsset(curTexture, preservedFolder, "");
					if (string.IsNullOrEmpty(movedPath))
					{
						//에러 발생
						continue;
					}
				}
			}
			//----------

			return true;
		}

		


		private bool Work_BakedImageSave_1(int index)
		{
			if (_bakeDataList.Count == 0)
			{
				return false;
			}
			if (string.IsNullOrEmpty(_bakeDstPath) || string.IsNullOrEmpty(_fileNameOnly) || string.IsNullOrEmpty(_bakeDstPathRelative))
			{
				return false;
			}
			if (index >= _bakeDataList.Count)
			{
				Debug.LogError("Work BakedImageSave - 1 : Index Over (" + index + " / " + _bakeDataList.Count + ")");
				return false;
			}

			SaveBakeImage(index);

			return true;
		}

		private bool Work_BakedImageSave_2(int index)
		{
			//Save 다음에 Reimport를 따로 하자
			ReimportBakedImage(index);
			return true;
		}

		private bool Work_BakedImageSave_3(int index)
		{
			//Save 다음에 Reimport를 따로 하자
			ReimportBakedImageToHD(index);
			return true;
		}
		
		private bool Work_BakedImageSave_4(int index)
		{
			//OnLoadComplete(true);
			_loadStep = LOAD_STEP.Step4_ConvertToAnyPortrait;

			if(_funcConvertResult != null)
			{
				_funcConvertResult(true);
			}
			return true;
		}





		private void SaveBakeImage(int iBakeDataList)
		{
			try
			{
				apPSDBakeData curBakeData = _bakeDataList[iBakeDataList];
				byte[] data = curBakeData._bakedImage.EncodeToPNG();

				//F:/MainWorks/UnityProjects/AnyPortrait/AnyPortrait/Assets/Sample
				//겹치지 않는 이름으로 생성한다.
				//기존 : < _fileNameOnly + "_" + iBakeDataList > 으로 이름을 검색한다.
				//변경 : (_portrait가 있는 경우) iBakeDataList에 상관없이 _portrait의 TextureData를 기준으로 겹치지 않는 이름으로 0부터 네이밍을 한다.
				int nextFileNameNumber = iBakeDataList;
				if(_portrait != null)
				{
					//Debug.Log("인덱스 0부터 올려서 이름 체크 [" + iBakeDataList + "] >> (" + _portrait._textureData.Count + ")");
					int curNumber = 0;
					while(true)
					{
						//Debug.Log(_fileNameOnly + "_" + curNumber + " : 체크");
						if(curNumber > 99999)
						{
							break;
						}
						bool isExistInTextureData = _portrait._textureData.Exists(delegate(apTextureData a)
													{
														return string.Equals(a._name, _fileNameOnly + "_" + curNumber);
													});
						bool isExistInBakedList = _bakedTextureAssetPathList.Contains(_fileNameOnly + "_" + curNumber);
						if (isExistInTextureData || isExistInBakedList)
						{
							//같은게 있는가 > 다음거 검색
							//Debug.Log("[" + curNumber + "] 사용중인 이름이다. (" + _fileNameOnly + "_" + curNumber + ")");
							curNumber++;
						}
						else
						{
							//같은게 없다. > 이걸 사용
							//Debug.LogError("[" + curNumber + "] 저장 가능하다. (" + _fileNameOnly + "_" + curNumber + ")");
							break;
						}
					}
					nextFileNameNumber = curNumber;
				}

				AssetFileDataSet newPathSet = MakeUniqueAssetFileName(_bakeDstPathRelative, _fileNameOnly + "_" + nextFileNameNumber + ".png", "");

				//겹치지 않게 이름 저장
				_bakedTextureAssetPathList.Add(newPathSet._fileNameOnly);

				//string path = _bakeDstPath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
				//string relPath = _bakeDstPathRelative + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";

				string path = _bakeDstPath + "/" + newPathSet._fileNameWithExp;
				string relPath = newPathSet._fullPath;

				for (int iLayer = 0; iLayer < curBakeData._bakedLayerData.Count; iLayer++)
				{
					curBakeData._bakedLayerData[iLayer]._textureAssetPath = relPath;
					curBakeData._bakedLayerData[iLayer]._bakedData = curBakeData;
				}
				curBakeData._textureAssetPath = relPath;

				File.WriteAllBytes(path, data);
				

				//AssetDatabase.CreateAsset(curBakeData._bakedImage, relPath);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
				
			}
			catch (Exception)
			{
				return;
			}
		}

		private void ReimportBakedImage(int iBakeDataList)
		{
			try
			{
				//string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
				//string relPath = _bakeDstPathRelative + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
				apPSDBakeData curBakeData = _bakeDataList[iBakeDataList];
				string relPath = curBakeData._textureAssetPath;

				AssetDatabase.SaveAssets();

				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


				//-------------------------------------------------------------------
				// Unity 5.5부터는 TextureImporter를 호출하기 전에
				// AssetDatabase에서 한번 열어서 Apply를 해줘야 한다.
				//-------------------------------------------------------------------
				Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(relPath);
				if (tex2D != null)
				{
					try
					{
						//tex2D.Apply(false, true);
						AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceUpdate);
						
						tex2D.Apply();
					}
					catch(Exception)
					{
						//Debug.LogError("Sub Exception : Texture Apply : " + ex2);
					}
				}
				//-------------------------------------------------------------------

				TextureImporter ti = TextureImporter.GetAtPath(relPath) as TextureImporter;
				

				if (ti == null)
				{
					Debug.LogError("Bake Error : Path : " + relPath);
				}
				else
				{
					if(apEditorUtil.IsGammaColorSpace())
					{
						ti.sRGBTexture = true;//Gamma인 경우 sRGB를 사용한다.
					}
					else
					{
						ti.sRGBTexture = false;//Linear인 경우 sRGB를 사용하지 않는다.
					}

					ti.SaveAndReimport();
					AssetDatabase.Refresh();
				}

			}
			catch (Exception)
			{
				//Debug.LogError("Reimport Exception : " + ex);
			}
		}


		private void ReimportBakedImageToHD(int iBakeDataList)
		{
			try
			{
				apPSDBakeData curBakeData = _bakeDataList[iBakeDataList];
				string relPath = curBakeData._textureAssetPath;

				//-------------------------------------------------------------------

				TextureImporter ti = TextureImporter.GetAtPath(relPath) as TextureImporter;
				

				if (ti == null)
				{
					Debug.LogError("Bake Error : Path : " + relPath);
				}
				else
				{
					//Mipmap 끄고, 압축 해제
					ti.wrapMode = TextureWrapMode.Clamp;
					ti.mipmapEnabled = false;
					ti.textureCompression = TextureImporterCompression.Uncompressed;

					ti.SaveAndReimport();
					AssetDatabase.Refresh();
				}

			}
			catch (Exception)
			{
				//Debug.LogError("Reimport Exception : " + ex);
			}
		}





		public struct AssetFileDataSet
		{
			public string _fullPath;
			public string _fileNameOnly;
			public string _fileNameWithExp;
			public string _folderPath;
		}
		public AssetFileDataSet MakeUniqueAssetFileName(string baseFolderPath, string fileNameWithAsset, string addedFileName)
		{
			string fileNameOnly = "";
			string exp = "";
			if(fileNameWithAsset.Contains("."))
			{
				//0, 1, 2(.), 3, 4
				int iDot = fileNameWithAsset.IndexOf(".");

				fileNameOnly = fileNameWithAsset.Substring(0, iDot);
				if(iDot < fileNameWithAsset.Length - 1)
				{
					exp = fileNameWithAsset.Substring(iDot + 1);
				}
				else
				{
					exp = "";
				}
				
			}
			else
			{
				fileNameOnly = fileNameWithAsset;
				exp = "";
			}
			if(!string.IsNullOrEmpty(addedFileName))
			{
				fileNameOnly += addedFileName;
			}
			
			string newPath = baseFolderPath + "/" + fileNameOnly;
			if(exp.Length > 0)
			{
				newPath += "." + exp;
			}
			string fullPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

			

			int iLastSlash = fullPath.LastIndexOf("/");
			string newFileNameWithExp = fileNameWithAsset;
			
			if(iLastSlash >= 0)
			{
				newFileNameWithExp = fullPath.Substring(iLastSlash + 1);
			}
			string newFileNameOnly = newFileNameWithExp;
			if(newFileNameWithExp.Contains("."))
			{
				newFileNameOnly = newFileNameWithExp.Substring(0, newFileNameWithExp.IndexOf("."));
			}
		
			AssetFileDataSet result;

			result._fullPath = fullPath;
			result._fileNameOnly = newFileNameOnly;
			result._fileNameWithExp = newFileNameWithExp;
			result._folderPath = baseFolderPath;

			return result;
		}


		private string CheckAndMakeFolder(string baseFolderPath, string folderName)
		{
			string folderFullPath = baseFolderPath + "/" + folderName;
			if (!AssetDatabase.IsValidFolder(folderFullPath))
			{
				//Debug.Log("폴더가 없다. 폴더를 추가한다. [" + unusedFolderPath + "]");
				AssetDatabase.CreateFolder(baseFolderPath, folderName);
				AssetDatabase.Refresh();
			}
			return folderFullPath;
		}


		private string MoveAsset(UnityEngine.Object assetObject, string nextFolderPath, string addedFileName)
		{
			string assetPath = AssetDatabase.GetAssetPath(assetObject);
			if (string.IsNullOrEmpty(assetPath))
			{
				//Debug.LogError("경로문제 1");
				return null;
			}

			int iLastSlash = assetPath.LastIndexOf("/");
			if (iLastSlash < 0)
			{
				//Debug.LogError("경로문제 2");
				return null;
			}
			string fileNameWithExp = assetPath.Substring(iLastSlash + 1);
			string baseFolder = assetPath.Substring(0, iLastSlash);

			//만약 이미 옮기고자 하는 폴더에 있는 거라면 패스
			if(string.Equals(baseFolder, nextFolderPath))
			{
				//Debug.Log("MoveAsset : 이미 해당 폴더에 있다. [" + assetPath + "]");
				return assetPath;
			}

			AssetFileDataSet newPathSet = MakeUniqueAssetFileName(nextFolderPath, fileNameWithExp, addedFileName);
			string newPath = newPathSet._fullPath;
			//AssetDatabase.MoveAsset()
			if (string.IsNullOrEmpty(newPath))
			{
				//Debug.LogError("파일이름 생성 에러 [" + newPath + "]");
				return null;
			}

			//파일을 Unused 폴더로 이동한다.
			AssetDatabase.MoveAsset(assetPath, newPath);
			return newPath;
		}


		// Event
		//--------------------------------------

		// Get / Set
		//--------------------------------------
		public bool IsFileLoaded { get {  return _isFileLoaded; } }
		public string FileFullPath {  get {  return _fileFullPath; } }
		public string FileName {  get {  return _fileNameOnly; } }
		public string DstPath { get { return _bakeDstPath; } }
		public string DstPathRelative {  get {  return _bakeDstPathRelative; } }
		public int PSDImageWidth {  get {  return _imageWidth; } }
		public int PSDImageHeight {  get {  return _imageHeight; } }
		public Vector2 PSDCenterOffset { get { return _imageCenterPosOffset; } }
		public List<apPSDLayerData> PSDLayerDataList { get { return _layerDataList; } }

		public void SetFileName(string fileName)
		{
			_fileNameOnly = fileName;
		}

		public bool IsProcessRunning
		{
			get
			{
				return _workProcess.IsRunning;
			}
		}
		public float GetImageBakingRatio()
		{
			if(!_isImageBaking || !_workProcess.IsRunning)
			{
				return 0.0f;
			}
			
			return Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);
		}

		public string GetProcessLabel()
		{
			return _curProcessLabel;
		}

		public bool IsImageBaking { get { return _isImageBaking; } }
		public List<apPSDBakeData> BakeDataList
		{
			get
			{
				return _bakeDataList;
			}
		}

		public bool IsCalculated { get {  return _bakeReq_IsCalculated; } }
		public int CalculatedAtlasCount {  get {  return _bakeCal_AtlasCount; } }
		public int CalculatedResizeX100 {  get {  return _bakeCal_ResizeX100; } }

		public bool IsBaked {  get {  return _bakeResult_IsBaked; } }
		public int BakedAtlasCount {  get {  return _bakeResult_AtlasCount; } }
		public int BakedResizeX100 {  get {  return _bakeResult_BakeResizeX100; } }
		public int BakedPadding {  get {  return _bakeResult_Padding; } }

		public LOAD_STEP LoadStep
		{
			get {  return _loadStep; }
		}
	}

}