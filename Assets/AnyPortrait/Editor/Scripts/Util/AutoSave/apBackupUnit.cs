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

using System.Collections;
//using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AnyPortrait;
using System.Linq;

namespace AnyPortrait
{
	/// <summary>
	/// 백업시 필드, 멤버 정보가 저장되는 유닛
	/// </summary>
	public class apBackupUnit
	{
		public bool _isRoot = false;
		public bool _isListArrayItem = false;
		public string _typeName_Full = "";
		public string _typeName_Assembly = "";
		public string _typeName_Partial = "";
		public string _fieldName = "";
		public int _itemIndex = -1;
		public object _value = null;

		//private string _strEncoded = null;

		public int _monoInstanceID = -1;
		public string _monoName = "";
		public Vector3 _monoPosition = Vector3.zero;
		public Quaternion _monoQuat = Quaternion.identity;
		public Vector3 _monoScale = Vector3.zero;
		public string _monoAssetPath = "";

		public int _parsedNumChild = 0;

		public enum FIELD_CATEGORY
		{
			Primitive = 1,
			Enum = 2,
			String = 3,
			Vector2 = 4,
			Vector3 = 5,
			Vector4 = 6,
			Color = 7,
			Matrix4x4 = 8,
			Matrix3x3 = 9,
			UnityMonobehaviour = 10,
			UnityGameObject = 11,
			UnityObject = 12,//<< Monobehaviour 제외
			Texture2D = 13,//Texture2D 또는 Texture : 이건 링크를 하는거라 따로 값을 저장
			CustomShader = 14,
			Instance = 15,
			List = 16,
			Array = 17
		}
		public FIELD_CATEGORY _fieldCategory = FIELD_CATEGORY.Primitive;

		//1. List의 컨테이너-Item인 경우
		// Item의 입장에서 List 또는 Array를 참조할 때
		public apBackupUnit _parentContainer = null;

		// List 또는 Array 입장에서 Item을 참조할 때
		public List<apBackupUnit> _childItems = null;


		//2. Instance 타입인 경우
		// Instance의 Field 입장에서의 Instance를 참조할 때
		public apBackupUnit _parentInstance = null;

		// Instance가 자신의 Field를 참조할 때
		public List<apBackupUnit> _childFields = null;

		public int _level = 0;

		public apBackupUnit()
		{

		}

		public void SetRoot()
		{
			_isRoot = true;
			_isListArrayItem = false;
			_level = 0;
		}

		public void SetField(FieldInfo fieldInfo, object value, apBackupUnit parentInstance, apBackupTable table)
		{
			_isRoot = false;
			_isListArrayItem = false;
			_typeName_Full = fieldInfo.FieldType.FullName;
			_typeName_Assembly = fieldInfo.FieldType.Assembly.FullName;
			_typeName_Partial = _typeName_Full + ", " + _typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(","));
			_fieldName = fieldInfo.Name;
			_itemIndex = -1;
			_value = value;

			_level = parentInstance._level + 1;

			System.Type fType = fieldInfo.FieldType;



			if (fType.IsPrimitive)						{ _fieldCategory = FIELD_CATEGORY.Primitive; }
			else if(fType.IsEnum)						{ _fieldCategory = FIELD_CATEGORY.Enum; }
			else if (fType.IsArray)						{ _fieldCategory = FIELD_CATEGORY.Array; }
			else if (fType.IsGenericType)				{ _fieldCategory = FIELD_CATEGORY.List; }
			else if (fType.Equals(typeof(string)))		{ _fieldCategory = FIELD_CATEGORY.String; }
			else if (fType.Equals(typeof(Vector2)))		{ _fieldCategory = FIELD_CATEGORY.Vector2; }
			else if (fType.Equals(typeof(Vector3)))		{ _fieldCategory = FIELD_CATEGORY.Vector3; }
			else if (fType.Equals(typeof(Vector4)))		{ _fieldCategory = FIELD_CATEGORY.Vector4; }
			else if (fType.Equals(typeof(Color)))		{ _fieldCategory = FIELD_CATEGORY.Color; }
			else if (fType.Equals(typeof(Matrix4x4)))	{ _fieldCategory = FIELD_CATEGORY.Matrix4x4; }
			else if (fType.Equals(typeof(apMatrix3x3))) { _fieldCategory = FIELD_CATEGORY.Matrix3x3; }
			else if (fType.Equals(typeof(Texture2D)))	{ _fieldCategory = FIELD_CATEGORY.Texture2D; }
			else if (fType.Equals(typeof(Texture)))		{ _fieldCategory = FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (fType.Equals(typeof(Shader)))		{ _fieldCategory = FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(fType, typeof(UnityEngine.MonoBehaviour)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(fType, typeof(UnityEngine.GameObject)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(fType, typeof(UnityEngine.Object)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityObject;
				}
				else
				{
					_fieldCategory = FIELD_CATEGORY.Instance;
				}

			}

			SetParentInstance(parentInstance);

			//추가 : 테이블에 등록한다.
			table.AddFieldName(_fieldName);
			table.AddTypeName(_typeName_Partial);
		}

		public void SetItem(object value, apBackupUnit parentListOrArray, int itemIndex, apBackupTable table)
		{
			_isRoot = false;
			_isListArrayItem = true;
			Type valueType = value.GetType();
			_typeName_Full = valueType.FullName;
			_typeName_Assembly = valueType.Assembly.FullName;
			_typeName_Partial = _typeName_Full + ", " + _typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(","));

			_fieldName = "";//<<필드명은 없죠..
			_itemIndex = itemIndex;
			_value = value;

			_level = parentListOrArray._level + 1;

			System.Type vType = value.GetType();

			if (vType.IsPrimitive)						{ _fieldCategory = FIELD_CATEGORY.Primitive; }
			else if (vType.IsEnum)						{ _fieldCategory = FIELD_CATEGORY.Enum; }
			else if (vType.IsArray)						{ _fieldCategory = FIELD_CATEGORY.Array; }//<이게 들어가면 2중 리스트가 된다.
			else if (vType.IsGenericType)				{ _fieldCategory = FIELD_CATEGORY.List; }
			else if (vType.Equals(typeof(string)))		{ _fieldCategory = FIELD_CATEGORY.String; }
			else if (vType.Equals(typeof(Vector2)))		{ _fieldCategory = FIELD_CATEGORY.Vector2; }
			else if (vType.Equals(typeof(Vector3)))		{ _fieldCategory = FIELD_CATEGORY.Vector3; }
			else if (vType.Equals(typeof(Vector4)))		{ _fieldCategory = FIELD_CATEGORY.Vector4; }
			else if (vType.Equals(typeof(Color)))		{ _fieldCategory = FIELD_CATEGORY.Color; }
			else if (vType.Equals(typeof(Matrix4x4)))	{ _fieldCategory = FIELD_CATEGORY.Matrix4x4; }
			else if (vType.Equals(typeof(apMatrix3x3)))	{ _fieldCategory = FIELD_CATEGORY.Matrix3x3; }
			else if (vType.Equals(typeof(Texture2D)))	{ _fieldCategory = FIELD_CATEGORY.Texture2D; }
			else if (vType.Equals(typeof(Texture)))		{ _fieldCategory = FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (vType.Equals(typeof(Shader)))		{ _fieldCategory = FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(vType, typeof(UnityEngine.MonoBehaviour)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(vType, typeof(UnityEngine.GameObject)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(vType, typeof(UnityEngine.Object)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityObject;
				}
				else
				{
					_fieldCategory = FIELD_CATEGORY.Instance;
				}
			}

			SetParentListArray(parentListOrArray);

			//추가 : 테이블에 등록한다. (Item의 필드명은 없다)
			table.AddTypeName(_typeName_Partial);
		}


		public static FIELD_CATEGORY GetFieldCategory(System.Type targetType)
		{
			if (targetType.IsPrimitive)						{ return FIELD_CATEGORY.Primitive; }
			else if(targetType.IsEnum)						{ return FIELD_CATEGORY.Enum; }
			else if (targetType.IsArray)						{ return FIELD_CATEGORY.Array; }
			else if (targetType.IsGenericType)				{ return FIELD_CATEGORY.List; }
			else if (targetType.Equals(typeof(string)))		{ return FIELD_CATEGORY.String; }
			else if (targetType.Equals(typeof(Vector2)))		{ return FIELD_CATEGORY.Vector2; }
			else if (targetType.Equals(typeof(Vector3)))		{ return FIELD_CATEGORY.Vector3; }
			else if (targetType.Equals(typeof(Vector4)))		{ return FIELD_CATEGORY.Vector4; }
			else if (targetType.Equals(typeof(Color)))		{ return FIELD_CATEGORY.Color; }
			else if (targetType.Equals(typeof(Matrix4x4)))	{ return FIELD_CATEGORY.Matrix4x4; }
			else if (targetType.Equals(typeof(apMatrix3x3))) { return FIELD_CATEGORY.Matrix3x3; }
			else if (targetType.Equals(typeof(Texture2D)))	{ return FIELD_CATEGORY.Texture2D; }
			else if (targetType.Equals(typeof(Texture)))		{ return FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (targetType.Equals(typeof(Shader)))		{ return FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(targetType, typeof(UnityEngine.MonoBehaviour)))
				{
					return FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(targetType, typeof(UnityEngine.GameObject)))
				{
					return FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(targetType, typeof(UnityEngine.Object)))
				{
					return FIELD_CATEGORY.UnityObject;
				}
				else
				{
					return FIELD_CATEGORY.Instance;
				}
			}
		}

		private static bool IsInherited(System.Type targetType, System.Type baseType)
		{
			System.Type curType = targetType;
			int nCount = 0;
			while (true)
			{
				if (nCount > 10)
				{
					//10번 이상 위로 올라갈 수 있나?
					return false;
				}
				if (curType == typeof(object))
				{
					return false;
				}

				if (curType.Equals(baseType))
				{
					//찾았슴다.
					return true;
				}
				if (curType.BaseType == null)
				{
					//위로 올라갈 수 없네요
					return false;
				}
				if (curType.Equals(curType.BaseType))
				{
					//더이상 올라갈 수 없다.
					return false;
				}
				curType = curType.BaseType;
				nCount++;
			}
		}

		private void SetParentListArray(apBackupUnit parentListOrArray)
		{
			_parentContainer = parentListOrArray;
			if (parentListOrArray._childItems == null)
			{
				parentListOrArray._childItems = new List<apBackupUnit>();
			}
			parentListOrArray._childItems.Add(this);
		}

		private void SetParentInstance(apBackupUnit parentInstance)
		{
			_parentInstance = parentInstance;
			if (parentInstance._childFields == null)
			{
				parentInstance._childFields = new List<apBackupUnit>();
			}
			parentInstance._childFields.Add(this);
		}

		public void PrintDebugRecursive()
		{
			string strIndent = "";
			for (int i = 0; i < _level; i++)
			{
				strIndent += "    ";
			}
			if(_isRoot)
			{
				Debug.Log(strIndent + ": Root Unit");
			}
			else if(_isListArrayItem)
			{
				Debug.Log(strIndent + "[" + _typeName_Partial + " - Item] / (" + _fieldCategory +")");
			}
			else
			{
				Debug.Log(strIndent + "[" + _typeName_Partial + " : " + _fieldName + "] / (" + _fieldCategory +")");
			}


			if(_childFields != null && _childFields.Count > 0)
			{
				Debug.Log(strIndent + ">> Child Fields ------------------------");
				for (int i = 0; i < _childFields.Count; i++)
				{
					_childFields[i].PrintDebugRecursive();
				}
				Debug.Log(strIndent + ">>--------------------------------------");
			}
			else if(_childItems != null && _childItems.Count > 0)
			{
				Debug.Log(strIndent + ">> Item Fields ------------------------");
				for (int i = 0; i < _childItems.Count; i++)
				{
					_childItems[i].PrintDebugRecursive();
				}
				Debug.Log(strIndent + ">>--------------------------------------");
			}
		}

		//------------------------------------------------------------------
		// Encode
		//------------------------------------------------------------------
		public string GetEncodingString(apBackupTable table)
		{
			//수정 -> Table을 이용하여 데이터 자체는 간략하게 한다.
			//[Level:3] [Root/Item/None] [000FieldName-Index] [000Type-Index] [Category (00)] [00000 Value]
			

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			//1.Level:3 입력
			sb.Append(Int2String(_level, 3));

			//2.Root/Item/Node 입력
			if(_isRoot)
			{
				sb.Append("R");
			}
			else if(_isListArrayItem)
			{
				sb.Append("I");
			}
			else
			{
				sb.Append("N");
			}

			//Root 타입은 여기서 끝
			if(_isRoot)
			{
				return sb.ToString();
			}

			//FieldName의 Index 버전 입력
			string strFieldNameIndex = null;
			if(_isListArrayItem)
			{
				//List Item이라면 Field 이름이 없다. 인덱스로 지정
				strFieldNameIndex = _itemIndex.ToString();
			}
			else
			{
				strFieldNameIndex = table.GetFieldIndex(_fieldName).ToString();
			}
				
			string strTypeNameIndex = table.GetTypeIndex(_typeName_Partial).ToString();

			sb.Append(Int2String(strFieldNameIndex.Length, 3));
			sb.Append(strFieldNameIndex);

			//TypeName의 Index 버전 입력
			sb.Append(Int2String(strTypeNameIndex.Length, 3));
			sb.Append(strTypeNameIndex);

			//Category의 Int형 직접 입력
			sb.Append(Int2String(((int)_fieldCategory), 2));

			//이제 value를 문자열로 입력해야한다.
			string strValue = "";


			switch (_fieldCategory)
			{
				case FIELD_CATEGORY.Primitive:
					strValue = _value.ToString();
					break;

				case FIELD_CATEGORY.Enum:
					strValue = ((int)_value).ToString();
					break;

				case FIELD_CATEGORY.String:
					strValue = _value.ToString();
					
					//여기서 주의
					//개행 문자는 여기서 바꿔준다.
					//에러가 나도 어쩔 수 없다.
					strValue = strValue.Replace("\n", "[]");
					strValue = strValue.Replace("\r", "");
					break;

				case FIELD_CATEGORY.Vector2:
					{
						Vector2 vec2 = (Vector2)_value;
						strValue = vec2.x + "," + vec2.y;
					}
					break;

				case FIELD_CATEGORY.Vector3:
					{
						Vector3 vec3 = (Vector3)_value;
						strValue = vec3.x + "," + vec3.y + "," + vec3.z;
					}

					break;

				case FIELD_CATEGORY.Vector4:
					{
						Vector4 vec4 = (Vector4)_value;
						strValue = vec4.x + "," + vec4.y + "," + vec4.z + "," + vec4.w;
					}
					break;

				case FIELD_CATEGORY.Color:
					{
						Color color = (Color)_value;
						strValue = color.r + "," + color.g + "," + color.b + "," + color.a;
					}

					break;

				case FIELD_CATEGORY.Matrix4x4:
					{
						Matrix4x4 mat4 = (Matrix4x4)_value;

						strValue =	mat4.m00 + "," + mat4.m10 + "," + mat4.m20 + "," + mat4.m30 + "," +
									mat4.m01 + "," + mat4.m11 + "," + mat4.m21 + "," + mat4.m31 + "," +
									mat4.m02 + "," + mat4.m12 + "," + mat4.m22 + "," + mat4.m32 + "," +
									mat4.m03 + "," + mat4.m13 + "," + mat4.m23 + "," + mat4.m33;
					}

					break;

				case FIELD_CATEGORY.Matrix3x3:
					{
						apMatrix3x3 mat3 = (apMatrix3x3)_value;

						strValue =	mat3._m00 + "," + mat3._m10 + "," + mat3._m20 + "," +
									mat3._m01 + "," + mat3._m11 + "," + mat3._m21 + "," +
									mat3._m02 + "," + mat3._m12 + "," + mat3._m22 + ",";
					}
					break;

				case FIELD_CATEGORY.UnityMonobehaviour:
					{
						MonoBehaviour monoBehaviour = (MonoBehaviour)_value;

						strValue =	monoBehaviour.GetInstanceID() + "," +
									monoBehaviour.name + "," +
									monoBehaviour.transform.position.x + "," + monoBehaviour.transform.position.y + "," + monoBehaviour.transform.position.z + "," +
									monoBehaviour.transform.localRotation.x + "," + monoBehaviour.transform.localRotation.y + "," + monoBehaviour.transform.localRotation.z + "," + monoBehaviour.transform.localRotation.w + "," +
									monoBehaviour.transform.localScale.x + "," + monoBehaviour.transform.localScale.y + "," + monoBehaviour.transform.localScale.z;
						
					}
					break;

				case FIELD_CATEGORY.UnityGameObject:
					{
						GameObject gameObj = (GameObject)_value;

						strValue =	gameObj.GetInstanceID() + "," +
									gameObj.name + "," +
									gameObj.transform.position.x + "," + gameObj.transform.position.y + "," + gameObj.transform.position.z + "," +
									gameObj.transform.localRotation.x + "," + gameObj.transform.localRotation.y + "," + gameObj.transform.localRotation.z + "," + gameObj.transform.localRotation.w + "," +
									gameObj.transform.localScale.x + "," + gameObj.transform.localScale.y + "," + gameObj.transform.localScale.z;
					}
					break;
				case FIELD_CATEGORY.UnityObject:
					{
						UnityEngine.Object uObj = (UnityEngine.Object)_value;

						strValue = uObj.GetInstanceID().ToString();
					}
					break;

				case FIELD_CATEGORY.Texture2D:
					{
						Texture2D tex = _value as Texture2D;
						if (tex == null)
						{
							strValue = "-1, ,  ";
						}
						else
						{
							strValue =	tex.GetInstanceID() + "," +
										tex.name + "," +
										AssetDatabase.GetAssetPath(tex);
						}
					}
					break;

				case FIELD_CATEGORY.CustomShader:
					{
						Shader customShader = _value as Shader;
						if (customShader == null)
						{
							strValue = "-1, , ";
						}
						else
						{
							strValue =	customShader.GetInstanceID() + "," +
										customShader.name + "," +
										AssetDatabase.GetAssetPath(customShader);
						}
					}
					break;

				case FIELD_CATEGORY.Instance:
					{
						//Instance는 하위의 필드 개수를 넣어준다.
						int nChildFields = 0;
						if (_childFields != null)
						{
							nChildFields = _childFields.Count;

						}

						strValue = nChildFields.ToString();
					}
					break;

				case FIELD_CATEGORY.List:
				case FIELD_CATEGORY.Array:
					{
						//Array/List는 개수를 넣어준다.
						int nChildItems = 0;
						if (_childItems != null)
						{
							nChildItems = _childItems.Count;
						}

						strValue = nChildItems.ToString();
					}
					break;
			}

			sb.Append(Int2String(strValue.Length, 5));
			sb.Append(strValue);

			return sb.ToString();

			#region [미사용 코드]
			////return _level + "-";
			////구분자대신 {한글자 키워드:길이:값} 방식으로 조합 (레벨 제외)
			////1. Level
			////2. [H] Root / Item / None
			////3. [C] FieldCategory
			////4. [T] TypeName
			////5. [F] FieldName (Item이면 인덱스)
			////6. [V] Value < 여기에 : 가 들어갈 수 있으니 이건 구분자를 받지 않도록 주의
			//string strIndent = "";
			//for (int i = 0; i < _level; i++)
			//{
			//	strIndent += "   ";
			//}
			//if(_isRoot)
			//{
			//	return strIndent + _level + GetEncodingSet("H", "Root");
			//}


			//System.Text.StringBuilder sb = new System.Text.StringBuilder();
			//sb.Append(_level + GetEncodingSet("H", (_isListArrayItem) ? "Item" : "None"));
			//sb.Append(GetEncodingSet("F", (_isListArrayItem) ? _itemIndex.ToString() : _fieldName));
			//sb.Append(GetEncodingSet("T", _typeName_Partial));
			//sb.Append(GetEncodingSet("C", _fieldCategory.ToString()));



			//switch (_fieldCategory)
			//{
			//	case FIELD_CATEGORY.Primitive:
			//		sb.Append(GetEncodingSet("V", _value.ToString()));
			//		break;

			//	case FIELD_CATEGORY.Enum:
			//		sb.Append(GetEncodingSet("V", ((int)_value).ToString()));
			//		break;

			//	case FIELD_CATEGORY.String:
			//		sb.Append(GetEncodingSet("V", _value.ToString()));
			//		break;

			//	case FIELD_CATEGORY.Vector2:
			//		{
			//			Vector2 vec2 = (Vector2)_value;
			//			sb.Append(GetEncodingSet("V", vec2.x + "," + vec2.y));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Vector3:
			//		{
			//			Vector3 vec3 = (Vector3)_value;
			//			sb.Append(GetEncodingSet("V", vec3.x + "," + vec3.y + "," + vec3.z));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Vector4:
			//		{
			//			Vector4 vec4 = (Vector4)_value;
			//			sb.Append(GetEncodingSet("V", vec4.x + "," + vec4.y + "," + vec4.z + "," + vec4.w));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Color:
			//		{
			//			Color color = (Color)_value;
			//			sb.Append(GetEncodingSet("V", color.r + "," + color.g + "," + color.b + "," + color.a));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Matrix4x4:
			//		{
			//			Matrix4x4 mat4 = (Matrix4x4)_value;


			//			sb.Append(GetEncodingSet("V", 
			//				mat4.m00 + "," + mat4.m10 + "," + mat4.m20 + "," + mat4.m30 + "," + 
			//				mat4.m01 + "," + mat4.m11 + "," + mat4.m21 + "," + mat4.m31 + "," + 
			//				mat4.m02 + "," + mat4.m12 + "," + mat4.m22 + "," + mat4.m32 + "," + 
			//				mat4.m03 + "," + mat4.m13 + "," + mat4.m23 + "," + mat4.m33));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Matrix3x3:
			//		{
			//			apMatrix3x3 mat3 = (apMatrix3x3)_value;

			//			sb.Append(GetEncodingSet("V",
			//				mat3._m00 + "," + mat3._m10 + "," + mat3._m20 + "," +
			//				mat3._m01 + "," + mat3._m11 + "," + mat3._m21 + "," +
			//				mat3._m02 + "," + mat3._m12 + "," + mat3._m22 + ","));
			//		}
			//		break;

			//	case FIELD_CATEGORY.UnityMonobehaviour:
			//		{
			//			MonoBehaviour monoBehaviour = (MonoBehaviour)_value;
			//			sb.Append(GetEncodingSet("V",
			//				monoBehaviour.GetInstanceID() + "," +
			//				monoBehaviour.name + "," +
			//				monoBehaviour.transform.position.x + "," + monoBehaviour.transform.position.y + "," + monoBehaviour.transform.position.z + "," +
			//				monoBehaviour.transform.localRotation.x + "," + monoBehaviour.transform.localRotation.y + "," + monoBehaviour.transform.localRotation.z + "," + monoBehaviour.transform.localRotation.w + "," + 
			//				monoBehaviour.transform.localScale.x + "," + monoBehaviour.transform.localScale.y + "," + monoBehaviour.transform.localScale.z
			//				));
			//		}
			//		break;

			//	case FIELD_CATEGORY.UnityGameObject:
			//		{
			//			GameObject gameObj = (GameObject)_value;
			//			sb.Append(GetEncodingSet("V",
			//				gameObj.GetInstanceID() + "," +
			//				gameObj.name + "," +
			//				gameObj.transform.position.x + "," + gameObj.transform.position.y + "," + gameObj.transform.position.z + "," +
			//				gameObj.transform.localRotation.x + "," + gameObj.transform.localRotation.y + "," + gameObj.transform.localRotation.z + "," + gameObj.transform.localRotation.w + "," + 
			//				gameObj.transform.localScale.x + "," + gameObj.transform.localScale.y + "," + gameObj.transform.localScale.z
			//				));
			//		}
			//		break;
			//	case FIELD_CATEGORY.UnityObject:
			//		{
			//			UnityEngine.Object uObj = (UnityEngine.Object)_value;

			//			sb.Append(GetEncodingSet("V", uObj.GetInstanceID().ToString()));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Texture2D:
			//		{
			//			Texture2D tex = _value as Texture2D;
			//			if (tex == null)
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					-1 + "," +
			//					" " + "," +
			//					" "
			//					));
			//			}
			//			else
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					tex.GetInstanceID() + "," +
			//					tex.name + "," +
			//					AssetDatabase.GetAssetPath(tex)
			//					));
			//			}
			//		}
			//		break;

			//	case FIELD_CATEGORY.CustomShader:
			//		{
			//			Shader customShader = _value as Shader;
			//			if (customShader == null)
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					-1 + "," +
			//					" " + "," +
			//					" "
			//					));
			//			}
			//			else
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					customShader.GetInstanceID() + "," +
			//					customShader.name + "," +
			//					AssetDatabase.GetAssetPath(customShader)
			//					));
			//			}
			//		}
			//		break;

			//	case FIELD_CATEGORY.Instance:
			//		{
			//			//Instance는 하위의 필드 개수를 넣어준다.
			//			int nChildFields = 0;
			//			if (_childFields != null)
			//			{
			//				nChildFields = _childFields.Count;

			//			}
			//			sb.Append(GetEncodingSet("V", nChildFields.ToString()));
			//		}
			//		break;

			//	case FIELD_CATEGORY.List:
			//	case FIELD_CATEGORY.Array:
			//		{	
			//			//Array/List는 개수를 넣어준다.
			//			int nChildItems = 0;
			//			if(_childItems != null)
			//			{
			//				nChildItems = _childItems.Count;
			//			}
			//			sb.Append(GetEncodingSet("V", nChildItems.ToString()));
			//		}
			//		break;
			//}


			//return strIndent + sb.ToString(); 
			#endregion
		}

		/// <summary>
		/// int -> string 변환시 자리수를 강제로 맞춘다.
		/// 12 -> 012로 바꾼다.
		/// 자리수는 2, 3, 4, 5를 지원한다.
		/// </summary>
		/// <param name="iValue"></param>
		/// <param name="nCipher"></param>
		/// <returns></returns>
		private string Int2String(int iValue, int nCipher)
		{
			switch (nCipher)
			{
				case 2:
					if(iValue < 10) { return "0" + iValue; }
					else			{ return iValue.ToString(); }
					
				case 3:
					if(iValue < 10)			{ return "00" + iValue; }
					else if(iValue < 100)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }

				case 4:
					if(iValue < 10)			{ return "000" + iValue; }
					else if(iValue < 100)	{ return "00" + iValue; }
					else if(iValue < 1000)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }

				case 5:
					if(iValue < 10)			{ return "0000" + iValue; }
					else if(iValue < 100)	{ return "000" + iValue; }
					else if(iValue < 1000)	{ return "00" + iValue; }
					else if(iValue < 10000)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }
			}

			return iValue.ToString();
			
		}

		private string GetEncodingSet(string strKeyword, string strEncode)
		{
			return "{" + strKeyword + ":" + strEncode.Length + ":" + strEncode + "}\t"; 
		}


		//------------------------------------------------------------------
		// Encode
		//------------------------------------------------------------------
		public bool Decode(string strEncoded, apBackupTable table)
		{
			try
			{

				// Table을 이용한 코드로 바꾸자
				//[Level:3] [R,I,N] [000 - FieldName Index] [000 - Type Index] [Category:2] [00000 Value]

				//[Level:3] [R,I,N]
				string strLevel = strEncoded.Substring(0, 3);
				string strRIN = strEncoded.Substring(3, 1);

				_level = int.Parse(strLevel);
				_isRoot = false;
				_isListArrayItem = false;

				if(strRIN.Equals("R"))
				{
					_isRoot = true;
				}
				else if(strRIN.Equals("I"))
				{
					_isListArrayItem = true;
				}

				if(_isRoot)
				{
					//Root 타입은 여기서 끝
					return true;
				}

				// [000 - FieldName Index]
				int cursor = 4;//<<여기서부터 커서 이동 시작
				int fieldNameLength = int.Parse(strEncoded.Substring(cursor, 3));
				cursor += 3;

				int fieldNameIndex = int.Parse(strEncoded.Substring(cursor, fieldNameLength));
				cursor += fieldNameLength;

				if (_isListArrayItem)
				{
					_itemIndex = fieldNameIndex;
					_fieldName = _itemIndex.ToString();
				}
				else
				{
					_fieldName = table.GetFieldName(fieldNameIndex);//Table에서 필드 이름을 가져오자
				}
				

				// [000 - Type Index]
				int typeNameLength = int.Parse(strEncoded.Substring(cursor, 3));
				cursor += 3;

				int typeNameIndex = int.Parse(strEncoded.Substring(cursor, typeNameLength));
				cursor += typeNameLength;

				_typeName_Partial = table.GetTypeName(typeNameIndex);
				Type parseType = table.GetTypeParsed(typeNameIndex);


				// [Category:2]
				_fieldCategory = (FIELD_CATEGORY)int.Parse(strEncoded.Substring(cursor, 2));
				cursor += 2;

				// [00000 Value]
				int valueLength = int.Parse(strEncoded.Substring(cursor, 5));
				cursor += 5;

				_value = null;
				if (valueLength > 0)
				{
					string strValue = strEncoded.Substring(cursor, valueLength);


					//이제 Value 파싱을 하자
					switch (_fieldCategory)
					{
						case FIELD_CATEGORY.Primitive:
							{
								try
								{
									if (parseType.Equals(typeof(bool)))			{ _value = bool.Parse(strValue); }
									else if (parseType.Equals(typeof(int)))		{ _value = int.Parse(strValue); }
									else if (parseType.Equals(typeof(float)))
									{
										//추가 20.9.13 : 실수에 .대신 ,이 들어가는 버그가 있다.
										_value = float.Parse(strValue.Replace(',', '.'));
									}
									else if (parseType.Equals(typeof(double)))
									{
										//추가 20.9.13 : 실수에 .대신 ,이 들어가는 버그가 있다.
										_value = double.Parse(strValue.Replace(',', '.'));
									}
									else if (parseType.Equals(typeof(byte)))	{ _value = byte.Parse(strValue); }
									else if (parseType.Equals(typeof(char)))	{ _value = char.Parse(strValue); }
									else
									{
										Debug.LogError("알 수 없는 Primitive 타입 : " + _typeName_Partial);
										return false;
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Enum:
							{
								_value = int.Parse(strValue);
							}
							break;
						case FIELD_CATEGORY.String:
							{
								_value = strValue.ToString();
							}
							break;
						case FIELD_CATEGORY.Vector2:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 2)
									{
										Debug.LogError("Vector2 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 4)
									{
										if(!strValues[0].Contains(".")
											&& !strValues[1].Contains(".")
											&& !strValues[2].Contains(".")
											&& !strValues[3].Contains("."))
										{
											//- 개수가 두배이며, . 이 없다.
											isFloatCommaBug = true;
										}
									}

									if (isFloatCommaBug)
									{
										//콤마 버그
										_value = new Vector2(	float.Parse(strValues[0] + "." + strValues[1]),
																float.Parse(strValues[2] + "." + strValues[3])
																);
									}
									else
									{
										//일반적인 경우
										_value = new Vector2(	float.Parse(strValues[0]),
																float.Parse(strValues[1])
																);
									}
									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Vector3:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 3)
									{
										Debug.LogError("Vector3 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 6)
									{
										if(!strValues[0].Contains(".")
											&& !strValues[1].Contains(".")
											&& !strValues[2].Contains(".")
											&& !strValues[3].Contains(".")
											&& !strValues[4].Contains(".")
											&& !strValues[5].Contains(".")
											)
										{
											//- 개수가 두배이며, . 이 없다.
											isFloatCommaBug = true;
										}
									}

									if (isFloatCommaBug)
									{
										//콤마 버그
										_value = new Vector3(	float.Parse(strValues[0] + "." + strValues[1]),
																float.Parse(strValues[2] + "." + strValues[3]),
																float.Parse(strValues[4] + "." + strValues[5])
																);
									}
									else
									{
										//일반적인 경우
										_value = new Vector3(	float.Parse(strValues[0]),
																float.Parse(strValues[1]),
																float.Parse(strValues[2])
																);
									}

									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Vector4:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 4)
									{
										Debug.LogError("Vector4 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 8)
									{
										if(!strValues[0].Contains(".")
											&& !strValues[1].Contains(".")
											&& !strValues[2].Contains(".")
											&& !strValues[3].Contains(".")
											&& !strValues[4].Contains(".")
											&& !strValues[5].Contains(".")
											&& !strValues[6].Contains(".")
											&& !strValues[7].Contains(".")
											)
										{
											//- 개수가 두배이며, . 이 없다.
											isFloatCommaBug = true;
										}
									}

									if (isFloatCommaBug)
									{
										//콤마 버그
										_value = new Vector4(	float.Parse(strValues[0] + "." + strValues[1]),
																float.Parse(strValues[2] + "." + strValues[3]),
																float.Parse(strValues[4] + "." + strValues[5]),
																float.Parse(strValues[6] + "." + strValues[7])
																);
									}
									else
									{
										//일반적인 경우
										_value = new Vector4(	float.Parse(strValues[0]),
																float.Parse(strValues[1]),
																float.Parse(strValues[2]),
																float.Parse(strValues[3])
																);
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Color:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 4)
									{
										Debug.LogError("Color 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 8)
									{
										if(!strValues[0].Contains(".")
											&& !strValues[1].Contains(".")
											&& !strValues[2].Contains(".")
											&& !strValues[3].Contains(".")
											&& !strValues[4].Contains(".")
											&& !strValues[5].Contains(".")
											&& !strValues[6].Contains(".")
											&& !strValues[7].Contains(".")
											)
										{
											//- 개수가 두배이며, . 이 없다.
											isFloatCommaBug = true;
										}
									}

									if (isFloatCommaBug)
									{
										//콤마 버그
										_value = new Color(		float.Parse(strValues[0] + "." + strValues[1]),
																float.Parse(strValues[2] + "." + strValues[3]),
																float.Parse(strValues[4] + "." + strValues[5]),
																float.Parse(strValues[6] + "." + strValues[7])
																);
									}
									else
									{
										//일반적인 경우
										_value = new Color(	float.Parse(strValues[0]),
															float.Parse(strValues[1]),
															float.Parse(strValues[2]),
															float.Parse(strValues[3])
															);
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Matrix4x4:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 16)
									{
										Debug.LogError("Matrix4x4 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 32)
									{
										//- 개수가 두배이며, . 이 없는지 체크
										isFloatCommaBug = true;

										for (int iCheckFloatData = 0; iCheckFloatData < 32; iCheckFloatData++)
										{
											if(strValues[iCheckFloatData].Contains("."))
											{
												//하나라도 .이 있으면 이 버그가 아니다.
												isFloatCommaBug = false;
												break;
											}
										}
									}

									Matrix4x4 mat4 = new Matrix4x4();

									if (isFloatCommaBug)
									{
										//콤마 버그
										mat4.m00 = float.Parse(strValues[0] + "." + strValues[1]);
										mat4.m10 = float.Parse(strValues[2] + "." + strValues[3]);
										mat4.m20 = float.Parse(strValues[4] + "." + strValues[5]);
										mat4.m30 = float.Parse(strValues[6] + "." + strValues[7]);

										mat4.m01 = float.Parse(strValues[8] + "." + strValues[9]);
										mat4.m11 = float.Parse(strValues[10] + "." + strValues[11]);
										mat4.m21 = float.Parse(strValues[12] + "." + strValues[13]);
										mat4.m31 = float.Parse(strValues[14] + "." + strValues[15]);

										mat4.m02 = float.Parse(strValues[16] + "." + strValues[17]);
										mat4.m12 = float.Parse(strValues[18] + "." + strValues[19]);
										mat4.m22 = float.Parse(strValues[20] + "." + strValues[21]);
										mat4.m32 = float.Parse(strValues[22] + "." + strValues[23]);

										mat4.m03 = float.Parse(strValues[24] + "." + strValues[25]);
										mat4.m13 = float.Parse(strValues[26] + "." + strValues[27]);
										mat4.m23 = float.Parse(strValues[28] + "." + strValues[29]);
										mat4.m33 = float.Parse(strValues[30] + "." + strValues[31]);
									}
									else
									{
										//일반적인 경우
										mat4.m00 = float.Parse(strValues[0]);
										mat4.m10 = float.Parse(strValues[1]);
										mat4.m20 = float.Parse(strValues[2]);
										mat4.m30 = float.Parse(strValues[3]);

										mat4.m01 = float.Parse(strValues[4]);
										mat4.m11 = float.Parse(strValues[5]);
										mat4.m21 = float.Parse(strValues[6]);
										mat4.m31 = float.Parse(strValues[7]);

										mat4.m02 = float.Parse(strValues[8]);
										mat4.m12 = float.Parse(strValues[9]);
										mat4.m22 = float.Parse(strValues[10]);
										mat4.m32 = float.Parse(strValues[11]);

										mat4.m03 = float.Parse(strValues[12]);
										mat4.m13 = float.Parse(strValues[13]);
										mat4.m23 = float.Parse(strValues[14]);
										mat4.m33 = float.Parse(strValues[15]);
									}
									_value = mat4;
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Matrix3x3:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 9)
									{
										Debug.LogError("Matrix3x3 파싱 실패 [" + strValue + "]");
										return false;
									}

									//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
									//그 경우엔 데이터가 두배로 보일 것
									bool isFloatCommaBug = false;

									if(strValues.Length >= 18)
									{
										//- 개수가 두배이며, . 이 없는지 체크
										isFloatCommaBug = true;

										for (int iCheckFloatData = 0; iCheckFloatData < 18; iCheckFloatData++)
										{
											if(strValues[iCheckFloatData].Contains("."))
											{
												//하나라도 .이 있으면 이 버그가 아니다.
												isFloatCommaBug = false;
												break;
											}
										}
									}

									apMatrix3x3 mat3 = new apMatrix3x3();

									if (isFloatCommaBug)
									{
										//콤마 버그
										mat3._m00 = float.Parse(strValues[0] + "." + strValues[1]);
										mat3._m10 = float.Parse(strValues[2] + "." + strValues[3]);
										mat3._m20 = float.Parse(strValues[4] + "." + strValues[5]);

										mat3._m01 = float.Parse(strValues[6] + "." + strValues[7]);
										mat3._m11 = float.Parse(strValues[8] + "." + strValues[9]);
										mat3._m21 = float.Parse(strValues[10] + "." + strValues[11]);

										mat3._m02 = float.Parse(strValues[12] + "." + strValues[13]);
										mat3._m12 = float.Parse(strValues[14] + "." + strValues[15]);
										mat3._m22 = float.Parse(strValues[16] + "." + strValues[17]);
									}
									else
									{
										//일반적인 경우										
										mat3._m00 = float.Parse(strValues[0]);
										mat3._m10 = float.Parse(strValues[1]);
										mat3._m20 = float.Parse(strValues[2]);

										mat3._m01 = float.Parse(strValues[3]);
										mat3._m11 = float.Parse(strValues[4]);
										mat3._m21 = float.Parse(strValues[5]);

										mat3._m02 = float.Parse(strValues[6]);
										mat3._m12 = float.Parse(strValues[7]);
										mat3._m22 = float.Parse(strValues[8]);
									}
									_value = mat3;
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.UnityMonobehaviour:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 12)
									{
										Debug.LogError("Unity MonoBehaviour 파싱 실패 [" + strValue + "]");
										return false;
									}

									//Value 대신 Mono 값을 넣자
									//_value = mat3;

									_monoInstanceID = int.Parse(strValues[0]);
									_monoName = strValues[1].ToString();
									_monoPosition = new Vector3(float.Parse(strValues[2]),
																	float.Parse(strValues[3]),
																	float.Parse(strValues[4]));

									_monoQuat = new Quaternion(float.Parse(strValues[5]),
																float.Parse(strValues[6]),
																float.Parse(strValues[7]),
																float.Parse(strValues[8]));

									_monoScale = new Vector3(float.Parse(strValues[9]),
																float.Parse(strValues[10]),
																float.Parse(strValues[11]));
									_monoAssetPath = "";


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.UnityGameObject:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 12)
									{
										Debug.LogError("Unity GameObject 파싱 실패 [" + strValue + "]");
										return false;
									}

									//Value 대신 Mono 값을 넣자
									//_value = mat3;

									_monoInstanceID = int.Parse(strValues[0]);
									_monoName = strValues[1].ToString();
									_monoPosition = new Vector3(float.Parse(strValues[2]),
																	float.Parse(strValues[3]),
																	float.Parse(strValues[4]));

									_monoQuat = new Quaternion(float.Parse(strValues[5]),
																float.Parse(strValues[6]),
																float.Parse(strValues[7]),
																float.Parse(strValues[8]));

									_monoScale = new Vector3(float.Parse(strValues[9]),
																float.Parse(strValues[10]),
																float.Parse(strValues[11]));
									_monoAssetPath = "";


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.UnityObject:
							{
								try
								{
									_monoInstanceID = int.Parse(strValue);

									//string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									//if(strValues.Length < 2)
									//{
									//	Debug.LogError("Unity Object 파싱 실패 [" + strValue + "]");
									//	return false;
									//}

									////Value 대신 Mono 값을 넣자
									////_value = mat3;

									//_monoInstanceID = int.Parse(strValues[0]);
									//_monoName = strValues[1].ToString();


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Texture2D:
						case FIELD_CATEGORY.CustomShader:
							{
								try
								{
									string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									if (strValues.Length < 3)
									{
										Debug.LogError("Unity Texture2D/Shader 파싱 실패 [" + strValue + "]");
										return false;
									}

									//Value 대신 Mono 값을 넣자
									//_value = mat3;

									_monoInstanceID = int.Parse(strValues[0]);
									_monoName = strValues[1].ToString();
									_monoAssetPath = strValues[2].ToString();


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Instance:
						case FIELD_CATEGORY.List:
						case FIELD_CATEGORY.Array:
							{
								try
								{
									_parsedNumChild = int.Parse(strValue);
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
					}

				}

			}
			catch(Exception ex)
			{
				Debug.LogError("Decode Exception [" + strEncoded + "] : " + ex);
				return false;
			}

			return true;


			// 이전 코드

			////Step 1 : {가 나올때 까지 숫자를 저장. => Level
			////Step 2 : 남은 글자 중에서 {를 찾고 그 다음 글자부터 파싱 시작
			////Step 3 : "1글자 타입", ":",
			////Step 4 : "값 길이" -> ":" 
			////Step 5 : 값 길이만큼 파싱 후 타입별로 값을 처리한다.
			////-> Step 2부터 반복

			//try
			//{

			//	//Debug.Log("Decode [" + strEncoded + "]");
			//	int iStr = 0;
			//	int iStep = 1;

			//	int strLength = strEncoded.Length;
			//	string subStr = null;

			//	bool isParse_Level = false;
			//	bool isParse_Header = false;
			//	bool isParse_Category = false;
			//	bool isParse_Type = false;
			//	bool isParse_Field = false;
			//	bool isParse_Value = false;

			//	bool isEnd = false;

			//	int iSetType = 0;
			//	int nValueLength = 0;
			//	string strValue = null;
				

			//	while (true)
			//	{
			//		if (string.IsNullOrEmpty(strEncoded))
			//		{
			//			break;
			//		}

			//		switch (iStep)
			//		{
			//			case 1:
			//				{
			//					//Step 1 : {가 나올때 까지 숫자를 저장. => Level
			//					//Debug.Log("Step1:" + strEncoded);

			//					iStr = strEncoded.IndexOf("{");
								
			//					if (iStr <= 0)
			//					{
			//						isEnd = true;
			//						break;
			//					}
			//					subStr = strEncoded.Substring(0, iStr);
			//					try
			//					{
			//						_level = Int32.Parse(subStr);
			//						isParse_Level = true;
			//					}
			//					catch (Exception ex)
			//					{
			//						Debug.LogError("Level Parse Exception : " + ex);
			//						isEnd = true;
			//						break;
			//					}

			//					//<0, 1, 2>, [3]

			//					strEncoded = strEncoded.Substring(iStr);//"{"를 포함한 그 이후 부분만 남긴다.
			//					iStep = 2;
			//				}
			//				break;

			//			case 2:
			//				{
			//					//Step 2 : 남은 글자 중에서 {를 찾고 그 다음 글자부터 파싱 시작
			//					//Debug.Log("Step2:" + strEncoded);

			//					iStr = strEncoded.IndexOf("{");
								
			//					if (iStr < 0)
			//					{
			//						isEnd = true;
			//						break;
			//					}
			//					//(0, 1, 2), [3 { ], <4 ...>
			//					if (iStr + 1 >= strEncoded.Length)
			//					{
			//						//더이상 파싱할 길이가 없다.
			//						isEnd = true;
			//						break;
			//					}


			//					strEncoded = strEncoded.Substring(iStr + 1);//"{"의 다음 부분만 남긴다.
			//					iSetType = -1;
			//					nValueLength = 0;
			//					iStep = 3;

			//				}
			//				break;

			//			case 3:
			//				{
			//					//Step 3 : "1글자 타입", ":",
			//					//Debug.Log("Step3:" + strEncoded);

			//					if (strEncoded.Length < 2)
			//					{
			//						isEnd = true;
			//						break;
			//					}
			//					subStr = strEncoded.Substring(0, 1);

			//					if (subStr.Equals("H"))
			//					{
			//						//Header 값이다.
			//						iSetType = 0;
			//						isParse_Header = true;

			//					}
			//					else if (subStr.Equals("C"))
			//					{
			//						//Category 값이다.
			//						iSetType = 1;
			//						isParse_Category = true;

			//					}
			//					else if (subStr.Equals("T"))
			//					{
			//						//Type 값이다.
			//						iSetType = 2;
			//						isParse_Type = true;
			//					}
			//					else if (subStr.Equals("F"))
			//					{
			//						//Field 값이다.
			//						iSetType = 3;
			//						isParse_Field = true;
			//					}
			//					else if (subStr.Equals("V"))
			//					{
			//						//Vaule 값이다.
			//						iSetType = 4;
			//						isParse_Value = true;
			//					}
			//					else
			//					{
			//						//?? 알수 없는 타입
			//						Debug.LogError("Unknown Type : " + subStr);
			//						isEnd = true;
			//						break;
			//					}

			//					// <0>, <1 ":">, 2... 다음 파싱을 위한 위치로 이동
			//					strEncoded = strEncoded.Substring(2);
			//					iStep = 4;

			//				}
			//				break;

			//			case 4:
			//				{
			//					//Step 4 : "값 길이" -> ":" 
			//					//Debug.Log("Step4:" + strEncoded);

			//					iStr = strEncoded.IndexOf(":");
			//					if (iStr < 0)
			//					{
			//						isEnd = true;
			//						break;
			//					}
			//					subStr = strEncoded.Substring(0, iStr);
			//					try
			//					{
			//						nValueLength = Int32.Parse(subStr);
			//						isParse_Level = true;
			//					}
			//					catch (Exception ex)
			//					{
			//						Debug.LogError("Length Parse Exception [" + subStr + "] : " + ex);
			//						isEnd = true;
			//						break;
			//					}

			//					//(0, 1, 2), [3 { ], <4 ...>
			//					strEncoded = strEncoded.Substring(iStr + 1);//":"의 다음 부분만 남긴다.
			//					iStep = 5;

			//				}
			//				break;

			//			case 5:
			//				{
			//					//Step 5 : 값 길이만큼 파싱 후 타입별로 값을 처리한다.
			//					//-> Step 2부터 반복
			//					//Debug.Log("Step5:" + strEncoded);

			//					subStr = strEncoded.Substring(0, nValueLength);

			//					//어떤 타입에 대한 값인가.
			//					switch (iSetType)
			//					{
			//						case 0:
			//							// H - Header
			//							if(subStr.Equals("Root"))
			//							{
			//								_isRoot = true;
			//								_isListArrayItem = false;
			//							}
			//							else if(subStr.Equals("Item"))
			//							{
			//								_isRoot = false;
			//								_isListArrayItem = true;
			//							}
			//							else if(subStr.Equals("None"))
			//							{
			//								_isRoot = false;
			//								_isListArrayItem = false;
			//							}
			//							else
			//							{
			//								Debug.LogError("알수 없는 헤더 : [" + subStr + "]");
			//								return false;
			//							}
			//							break;

			//						case 1:
			//							// C - Category
			//							{
			//								try
			//								{
			//									_fieldCategory = (FIELD_CATEGORY)Enum.Parse(typeof(FIELD_CATEGORY), subStr);

			//									if(_fieldCategory == FIELD_CATEGORY.Instance)
			//									{
			//										if(_childFields == null)
			//										{
			//											_childFields = new List<apBackupUnit>();
			//										}
			//									}
			//									else if(_fieldCategory == FIELD_CATEGORY.Array ||
			//										_fieldCategory == FIELD_CATEGORY.List)
			//									{
			//										if(_childItems == null)
			//										{
			//											_childItems = new List<apBackupUnit>();
			//										}
			//									}
			//								}
			//								catch(Exception)
			//								{
			//									Debug.LogError("Field Category Parse Exception [" + subStr + "]");
			//									return false;
			//								}
			//							}
			//							break;

			//						case 2:
			//							// T - Type
			//							{
			//								_typeName_Partial = subStr;
			//							}
			//							break;

			//						case 3:
			//							// F - Field
			//							{
			//								_fieldName = subStr;
			//							}
			//							break;

			//						case 4:
			//							// V - Value
			//							{
			//								_value = null;

			//								strValue = subStr;

			//							}
			//							break;
			//					}

			//					strEncoded = strEncoded.Substring(nValueLength + 1);
			//					iStep = 2;
			//				}
			//				break;
			//		}

			//		if (isEnd)
			//		{
			//			break;
			//		}
			//	}

			//	//레벨, 헤더가 파싱 안되었다면 실패
			//	if (!isParse_Level || !isParse_Header)
			//	{
			//		return false;
			//	}

			//	//Root 타입이 아닌데 나머지가 파싱 안되었다면 실패
			//	if (!_isRoot)
			//	{
			//		if (!isParse_Category || !isParse_Type || !isParse_Field || !isParse_Value)
			//		{
			//			return false;
			//		}

			//		//이제 Value 파싱을 하자
			//		switch (_fieldCategory)
			//		{
			//			case FIELD_CATEGORY.Primitive:
			//				{
			//					Type parseType = Type.GetType(_typeName_Partial);
			//					//Debug.Log("Primitive Type [" + _typeName + " >> " + parseType + "]");
			//					try
			//					{
			//						if (parseType.Equals(typeof(bool))) { _value = bool.Parse(strValue); }
			//						else if (parseType.Equals(typeof(int))) { _value = int.Parse(strValue); }
			//						else if (parseType.Equals(typeof(float))) { _value = float.Parse(strValue); }
			//						else if (parseType.Equals(typeof(double))) { _value = double.Parse(strValue); }
			//						else if (parseType.Equals(typeof(byte))) { _value = byte.Parse(strValue); }
			//						else if (parseType.Equals(typeof(char))) { _value = char.Parse(strValue); }
			//						else
			//						{
			//							Debug.LogError("알 수 없는 Primitive 타입 : " + _typeName_Partial);
			//							return false;
			//						}
			//					}
			//					catch (Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.Enum:
			//				{
			//					_value = int.Parse(strValue);
			//				}
			//				break;
			//			case FIELD_CATEGORY.String:
			//				{
			//					_value = strValue.ToString();
			//				}
			//				break;
			//			case FIELD_CATEGORY.Vector2:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 2)
			//						{
			//							Debug.LogError("Vector2 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						_value = new Vector2(
			//							float.Parse(strValues[0]),
			//							float.Parse(strValues[1])
			//							);
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;
			//			case FIELD_CATEGORY.Vector3:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 3)
			//						{
			//							Debug.LogError("Vector3 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						_value = new Vector3(
			//							float.Parse(strValues[0]),
			//							float.Parse(strValues[1]),
			//							float.Parse(strValues[2])
			//							);
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;
			//			case FIELD_CATEGORY.Vector4:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 4)
			//						{
			//							Debug.LogError("Vector4 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						_value = new Vector4(
			//							float.Parse(strValues[0]),
			//							float.Parse(strValues[1]),
			//							float.Parse(strValues[2]),
			//							float.Parse(strValues[3])
			//							);
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;
			//			case FIELD_CATEGORY.Color:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 4)
			//						{
			//							Debug.LogError("Color 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						_value = new Color(
			//							float.Parse(strValues[0]),
			//							float.Parse(strValues[1]),
			//							float.Parse(strValues[2]),
			//							float.Parse(strValues[3])
			//							);
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.Matrix4x4:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 16)
			//						{
			//							Debug.LogError("Matrix4x4 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						Matrix4x4 mat4 = new Matrix4x4();
			//						mat4.m00 = float.Parse(strValues[0]);
			//						mat4.m10 = float.Parse(strValues[1]);
			//						mat4.m20 = float.Parse(strValues[2]);
			//						mat4.m30 = float.Parse(strValues[3]);

			//						mat4.m01 = float.Parse(strValues[4]);
			//						mat4.m11 = float.Parse(strValues[5]);
			//						mat4.m21 = float.Parse(strValues[6]);
			//						mat4.m31 = float.Parse(strValues[7]);

			//						mat4.m02 = float.Parse(strValues[8]);
			//						mat4.m12 = float.Parse(strValues[9]);
			//						mat4.m22 = float.Parse(strValues[10]);
			//						mat4.m32 = float.Parse(strValues[11]);

			//						mat4.m03 = float.Parse(strValues[12]);
			//						mat4.m13 = float.Parse(strValues[13]);
			//						mat4.m23 = float.Parse(strValues[14]);
			//						mat4.m33 = float.Parse(strValues[15]);

			//						_value = mat4;
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.Matrix3x3:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 9)
			//						{
			//							Debug.LogError("Matrix3x3 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}
			//						apMatrix3x3 mat3 = new apMatrix3x3();
			//						mat3._m00 = float.Parse(strValues[0]);
			//						mat3._m10 = float.Parse(strValues[1]);
			//						mat3._m20 = float.Parse(strValues[2]);

			//						mat3._m01 = float.Parse(strValues[3]);
			//						mat3._m11 = float.Parse(strValues[4]);
			//						mat3._m21 = float.Parse(strValues[5]);

			//						mat3._m02 = float.Parse(strValues[6]);
			//						mat3._m12 = float.Parse(strValues[7]);
			//						mat3._m22 = float.Parse(strValues[8]);
									
			//						_value = mat3;
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;
			//			case FIELD_CATEGORY.UnityMonobehaviour:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 12)
			//						{
			//							Debug.LogError("Unity MonoBehaviour 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}

			//						//Value 대신 Mono 값을 넣자
			//						//_value = mat3;

			//						_monoInstanceID = int.Parse(strValues[0]);
			//						_monoName = strValues[1].ToString();
			//						_monoPosition = new Vector3(	float.Parse(strValues[2]),
			//														float.Parse(strValues[3]),
			//														float.Parse(strValues[4]));

			//						_monoQuat = new Quaternion(	float.Parse(strValues[5]),
			//													float.Parse(strValues[6]),
			//													float.Parse(strValues[7]),
			//													float.Parse(strValues[8]));

			//						_monoScale = new Vector3(	float.Parse(strValues[9]),
			//													float.Parse(strValues[10]),
			//													float.Parse(strValues[11]));
			//						_monoAssetPath = "";
									
									
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.UnityGameObject:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 12)
			//						{
			//							Debug.LogError("Unity GameObject 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}

			//						//Value 대신 Mono 값을 넣자
			//						//_value = mat3;

			//						_monoInstanceID = int.Parse(strValues[0]);
			//						_monoName = strValues[1].ToString();
			//						_monoPosition = new Vector3(	float.Parse(strValues[2]),
			//														float.Parse(strValues[3]),
			//														float.Parse(strValues[4]));

			//						_monoQuat = new Quaternion(	float.Parse(strValues[5]),
			//													float.Parse(strValues[6]),
			//													float.Parse(strValues[7]),
			//													float.Parse(strValues[8]));

			//						_monoScale = new Vector3(	float.Parse(strValues[9]),
			//													float.Parse(strValues[10]),
			//													float.Parse(strValues[11]));
			//						_monoAssetPath = "";
									
									
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.UnityObject:
			//				{
			//					try
			//					{
			//						_monoInstanceID = int.Parse(strValue);

			//						//string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						//if(strValues.Length < 2)
			//						//{
			//						//	Debug.LogError("Unity Object 파싱 실패 [" + strValue + "]");
			//						//	return false;
			//						//}

			//						////Value 대신 Mono 값을 넣자
			//						////_value = mat3;

			//						//_monoInstanceID = int.Parse(strValues[0]);
			//						//_monoName = strValues[1].ToString();
									
									
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.Texture2D:
			//			case FIELD_CATEGORY.CustomShader:
			//				{
			//					try
			//					{
			//						string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			//						if(strValues.Length < 3)
			//						{
			//							Debug.LogError("Unity Texture2D/Shader 파싱 실패 [" + strValue + "]");
			//							return false;
			//						}

			//						//Value 대신 Mono 값을 넣자
			//						//_value = mat3;

			//						_monoInstanceID = int.Parse(strValues[0]);
			//						_monoName = strValues[1].ToString();
			//						_monoAssetPath = strValues[2].ToString();
									
									
			//					}
			//					catch(Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;

			//			case FIELD_CATEGORY.Instance:
			//			case FIELD_CATEGORY.List:
			//			case FIELD_CATEGORY.Array:
			//				{
			//					try
			//					{
			//						_parsedNumChild = int.Parse(strValue);
			//					}
			//					catch (Exception exParse)
			//					{
			//						Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
			//						return false;
			//					}
			//				}
			//				break;
			//		}
					
			//	}



			//	//Debug.Log("Parse Complete (L" + _level + ")[" + _fieldCategory + " / " + _fieldName + "(" + _typeName + ")");
			//	return true;
			//}
			//catch (Exception ex)
			//{
			//	Debug.LogError("Decode Exception : " + ex);

			//	return false;
			//}
		}
		
	}
}