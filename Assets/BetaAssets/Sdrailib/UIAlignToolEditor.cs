using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIAlignTool))]
[CanEditMultipleObjects]
public class UIAlignToolEditor : Editor
{
    SerializedProperty alignmentType;
    SerializedProperty alignmnetFields;
    SerializedProperty gridFields;
    SerializedProperty polygonFields;
    SerializedProperty autoApply;
    private void OnEnable()
    {
        alignmentType = serializedObject.FindProperty("AlignmentType");
        alignmnetFields = serializedObject.FindProperty("alignmnetFields");
        gridFields = serializedObject.FindProperty("gridFields");
        polygonFields = serializedObject.FindProperty("polygonFields");
        autoApply = serializedObject.FindProperty("AutoApply");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(alignmentType, true);
        switch ((target as UIAlignTool).AlignmentType)
        {
            case UIAlignTool.Type_T.Alignment:
                {
                    EditorGUILayout.PropertyField(alignmnetFields,true);
                    break;
                }
            case UIAlignTool.Type_T.Grid:
                {
                    EditorGUILayout.PropertyField(gridFields,true);
                    break;
                }
            case UIAlignTool.Type_T.Polygon:
                {
                    EditorGUILayout.PropertyField(polygonFields, true);
                    break;
                }
        }

        EditorGUILayout.PropertyField(autoApply, true);
        if (GUILayout.Button("Apply"))
        {
            ButtonDown();
        }
        serializedObject.ApplyModifiedProperties();
        (target as UIAlignTool).UpdateAutoApply();
    }

    void ButtonDown()
    {
        (target as UIAlignTool).Apply();
    }
}
