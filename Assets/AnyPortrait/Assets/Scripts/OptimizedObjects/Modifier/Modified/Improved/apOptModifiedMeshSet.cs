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
	/// apOptModifiedMesh의 개편된 버전
	/// 데이터들을 모두 가지고 있는게 아니라, 실제 필요한 데이터만 가지고 있다.
	/// 큰 형태는 apOptModifiedMesh와 유사하다.
	/// </summary>
	[Serializable]
	public class apOptModifiedMeshSet
	{
		// Members
		//---------------------------------------------------------------------------------
		public apPortrait _portrait = null;

		[Flags]
		public enum DATA_TYPE
		{
			None = 0,
			Vertex = 1,
			Transform = 2,
			Rigging = 4,
			Physics = 8,
			Color = 16,
			Extra = 32,
		}

		public DATA_TYPE _dataType = DATA_TYPE.None;

		
		//에디터와 달리 바로 Monobehaviour를 저장하자.
		public apOptMesh _targetMesh = null;
		public apOptTransform _targetTransform = null;
		public apOptTransform _rootTransform = null;

		public int _rootMeshGroupUniqueID = -1;

		public int _meshUniqueID = -1;
		public int _transformUniqueID = -1;

		public bool _isMeshTransform = true;

		//분리된 데이터들
		//데이터가 없는 경우 아예 저장하지 않도록 배열처럼 만들어야 한다. (인덱스는 하나만 들어간다.)

		//1. Vertex
		[SerializeField]
		private apOptModifiedMesh_Vertex[] _subModMesh_Vertex = null;

		//2. Transform
		[SerializeField]
		private apOptModifiedMesh_Transform[] _subModMesh_Transform = null;

		//3. Rigging
		[SerializeField]
		private apOptModifiedMesh_VertexRig[] _subModMesh_Rigging = null;

		//4. Physics
		[SerializeField]
		private apOptModifiedMesh_Physics[] _subModMesh_Physics = null;

		//5. Color
		[SerializeField]
		private apOptModifiedMesh_Color[] _subModMesh_Color = null;

		//6. Extra
		[SerializeField]
		private apOptModifiedMesh_Extra[] _subModMesh_Extra = null;


		// Init
		//---------------------------------------------------------------------------------
		public apOptModifiedMeshSet()
		{

		}

		// Bake
		//---------------------------------------------------------------------------------
		public bool Bake(	apModifierBase srcModifier, 
							apModifierParamSetGroup srcModParamSetGroup, 
							apModifiedMesh srcModMesh, 
							apPortrait portrait)
		{
			_portrait = portrait;
			_rootMeshGroupUniqueID = srcModMesh._meshGroupUniqueID_Modifier;

			_meshUniqueID = srcModMesh._meshUniqueID;
			_transformUniqueID = srcModMesh._transformUniqueID;

			//_boneUniqueID = srcModMesh._boneUniqueID;

			_isMeshTransform = srcModMesh._isMeshTransform;

			apOptTransform rootTransform = _portrait.GetOptTransformAsMeshGroup(_rootMeshGroupUniqueID);
			apOptTransform targetTransform = _portrait.GetOptTransform(_transformUniqueID);

			if (targetTransform == null)
			{
				Debug.LogError("Bake 실패 : 찾을 수 없는 연결된 OptTransform [" + _transformUniqueID + "]");
				Debug.LogError("이미 삭제된 객체에 연결된 ModMesh가 아닌지 확인해보세염");
				return false;
			}
			apOptMesh targetMesh = null;
			if (targetTransform._unitType == apOptTransform.UNIT_TYPE.Mesh)
			{
				targetMesh = targetTransform._childMesh;
			}



			if (rootTransform == null)
			{
				Debug.LogError("ModifiedMesh 연동 에러 : 알수 없는 RootTransform");
				return false;
			}

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;
			
			
			//각각의 타입
			_dataType = DATA_TYPE.None;

			//1. Vertex 
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				Bake_Vertex(srcModMesh._vertices);
			}

			//2. Transform
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				Bake_Transform(srcModMesh._transformMatrix);
			}

			//3. Rigging
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				Bake_VertexRigs(_portrait, srcModMesh._vertRigs);
			}

			//4. Physics
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
				&& srcModMesh._isUsePhysicParam)
			{
				Bake_Physics(srcModMesh.PhysicParam, srcModMesh._vertWeights, portrait);
			}


			if (((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
				|| ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix) != 0)
				)
			{
				//Morph, Transform 모디파이어에서..

				//5. Color
				if (srcModifier._isColorPropertyEnabled
					&& srcModParamSetGroup._isColorPropertyEnabled)
				{
					//색상 옵션이 켜진 경우
					Color meshColor = srcModMesh._meshColor;
					if (!srcModMesh._isVisible)
					{
						meshColor.a = 0.0f;
					}

					Bake_Color(meshColor, srcModMesh._isVisible);
				}

				//6. Extra
				if(srcModMesh._isExtraValueEnabled
					&& (srcModMesh._extraValue._isDepthChanged || srcModMesh._extraValue._isTextureChanged)
					)
				{
					//Extra 옵션이 켜진 경우
					Bake_ExtraValue(srcModMesh);
				}
			}
			

			return true;
		}


		//서브 Bake 함수들
		//---------------------------------------------------------------------------------
		public void Bake_Vertex(//apOptMesh targetMesh, 
								List<apModifiedVertex> modVerts)
		{
			_dataType |= DATA_TYPE.Vertex;

			_subModMesh_Vertex = new apOptModifiedMesh_Vertex[1];
			_subModMesh_Vertex[0] = new apOptModifiedMesh_Vertex();
			apOptModifiedMesh_Vertex subMod_Vertex = _subModMesh_Vertex[0];


			//if (_targetMesh == null)
			//{
			//	Debug.LogError("Vert Morph인데 Target Mesh가 Null");
			//	Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			//}

			subMod_Vertex.Bake(modVerts);
		}


		public void Bake_Transform(apMatrix transformMatrix)
		{
			_dataType |= DATA_TYPE.Transform;

			_subModMesh_Transform = new apOptModifiedMesh_Transform[1];
			_subModMesh_Transform[0] = new apOptModifiedMesh_Transform();
			apOptModifiedMesh_Transform subMod_Transform = _subModMesh_Transform[0];

			subMod_Transform.Bake(transformMatrix);
		}

		public void Bake_VertexRigs(apPortrait portrait, List<apModifiedVertexRig> modVertRigs)
		{
			_dataType |= DATA_TYPE.Rigging;

			_subModMesh_Rigging = new apOptModifiedMesh_VertexRig[1];
			_subModMesh_Rigging[0] = new apOptModifiedMesh_VertexRig();
			apOptModifiedMesh_VertexRig subMod_Riggingm = _subModMesh_Rigging[0];

			subMod_Riggingm.Bake(portrait, modVertRigs);
		}

		public void Bake_Physics(apPhysicsMeshParam srcPhysicParam, List<apModifiedVertexWeight> modVertWeights, apPortrait portrait)
		{
			_dataType |= DATA_TYPE.Physics;

			_subModMesh_Physics = new apOptModifiedMesh_Physics[1];
			_subModMesh_Physics[0] = new apOptModifiedMesh_Physics();
			apOptModifiedMesh_Physics subMod_Physics = _subModMesh_Physics[0];

			subMod_Physics.Bake(srcPhysicParam, modVertWeights, portrait);
		}

		public void Bake_Color(Color meshColor, bool isVisible)
		{
			_dataType |= DATA_TYPE.Color;

			_subModMesh_Color = new apOptModifiedMesh_Color[1];
			_subModMesh_Color[0] = new apOptModifiedMesh_Color();
			apOptModifiedMesh_Color subMod_Color = _subModMesh_Color[0];

			subMod_Color.Bake(meshColor, isVisible);
		}

		public void Bake_ExtraValue(apModifiedMesh srcModMesh)
		{
			_dataType |= DATA_TYPE.Extra;

			_subModMesh_Extra = new apOptModifiedMesh_Extra[1];
			_subModMesh_Extra[0] = new apOptModifiedMesh_Extra();
			apOptModifiedMesh_Extra subMod_Extra = _subModMesh_Extra[0];

			subMod_Extra.Bake(srcModMesh);
		}



		// Link
		//---------------------------------------------------------------------------------
		public void Link(apPortrait portrait)
		{
			_portrait = portrait;

			if(IsVertex)
			{
				apOptModifiedMesh_Vertex sub_Vertex = SubModMesh_Vertex;
				if(sub_Vertex != null)
				{
					sub_Vertex.Link(this);
				}
			}

			if(IsTransform)
			{
				apOptModifiedMesh_Transform sub_Transform = SubModMesh_Transform;
				if(sub_Transform != null)
				{
					sub_Transform.Link(this);
				}
			}

			if(IsRigging)
			{
				apOptModifiedMesh_VertexRig sub_Rigging = SubModMesh_Rigging;
				if(sub_Rigging != null)
				{
					sub_Rigging.Link(portrait, this);
				}
			}

			if(IsPhysics)
			{
				apOptModifiedMesh_Physics sub_Physics = SubModMesh_Physics;
				if(sub_Physics != null)
				{
					sub_Physics.Link(portrait, this);
				}
			}

			if(IsColor)
			{
				apOptModifiedMesh_Color sub_Color = SubModMesh_Color;
				if(sub_Color != null)
				{
					sub_Color.Link(this);
				}
			}

			if(IsExtra)
			{
				apOptModifiedMesh_Extra sub_Extra = SubModMesh_Extra;
				if(sub_Extra != null)
				{
					sub_Extra.Link(portrait, this);
				}
			}
		}





		


		// Functions
		//---------------------------------------------------------------------------------


		// Get / Set
		//---------------------------------------------------------------------------------
		public bool IsVertex		{ get { return (int)(_dataType & DATA_TYPE.Vertex) != 0; } }
		public bool IsTransform		{ get { return (int)(_dataType & DATA_TYPE.Transform) != 0; } }
		public bool IsRigging		{ get { return (int)(_dataType & DATA_TYPE.Rigging) != 0; } }
		public bool IsPhysics		{ get { return (int)(_dataType & DATA_TYPE.Physics) != 0; } }
		public bool IsColor			{ get { return (int)(_dataType & DATA_TYPE.Color) != 0; } }
		public bool IsExtra			{ get { return (int)(_dataType & DATA_TYPE.Extra) != 0; } }

		public apOptModifiedMesh_Vertex SubModMesh_Vertex
		{
			get
			{
				if(!IsVertex || _subModMesh_Vertex == null) { return null; }
				if(_subModMesh_Vertex.Length == 0) { return null; }

				return _subModMesh_Vertex[0];
			}
		}

		public apOptModifiedMesh_Transform SubModMesh_Transform
		{
			get
			{
				if(!IsTransform || _subModMesh_Transform == null) { return null; }
				if(_subModMesh_Transform.Length == 0) { return null; }

				return _subModMesh_Transform[0];
			}
		}

		public apOptModifiedMesh_VertexRig SubModMesh_Rigging
		{
			get
			{
				if(!IsRigging || _subModMesh_Rigging == null) { return null; }
				if(_subModMesh_Rigging.Length == 0) { return null; }

				return _subModMesh_Rigging[0];
			}
		}

		public apOptModifiedMesh_Physics SubModMesh_Physics
		{
			get
			{
				if(!IsPhysics || _subModMesh_Physics == null) { return null; }
				if(_subModMesh_Physics.Length == 0) { return null; }

				return _subModMesh_Physics[0];
			}
		}

		public apOptModifiedMesh_Color SubModMesh_Color
		{
			get
			{
				if(!IsColor || _subModMesh_Color == null) { return null; }
				if(_subModMesh_Color.Length == 0) { return null; }

				return _subModMesh_Color[0];
			}
		}

		public apOptModifiedMesh_Extra SubModMesh_Extra
		{
			get
			{
				if(!IsExtra || _subModMesh_Extra == null) { return null; }
				if(_subModMesh_Extra.Length == 0) { return null; }

				return _subModMesh_Extra[0];
			}
		}


		////1. Vertex
		//[SerializeField]
		//private apOptModifiedMesh_Vertex[] _subModMesh_Vertex = null;

		////2. Transform
		//[SerializeField]
		//private apOptModifiedMesh_Transform[] _subModMesh_Transform = null;

		////3. Rigging
		//[SerializeField]
		//private apOptModifiedMesh_VertexRig[] _subModMesh_Rigging = null;

		////4. Physics
		//[SerializeField]
		//private apOptModifiedMesh_Physics[] _subModMesh_Physics = null;

		////5. Color
		//[SerializeField]
		//private apOptModifiedMesh_Color[] _subModMesh_Color = null;

		////6. Extra
		//[SerializeField]
		//private apOptModifiedMesh_Extra[] _subModMesh_Extra = null;


		//---------------------------------------------------------------------------------
	}
}