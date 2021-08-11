#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
[ExecuteInEditMode]
public class UIAlignTool : MonoBehaviour
{
    private RectTransform rect;
    public enum Type_T
    {
        Alignment,
        Grid,
        Polygon,
        None
    }
    public Type_T AlignmentType;
    [Serializable]
    public enum AlignmentType_T
    {
        Right,
        Left,
        Top,
        Bottom,
        Vertical,
        Horizontal
    }
    [Serializable]
    public class AlignmentFields
    {
        public GameObject Target;
        public AlignmentType_T Alignment;
        public Vector2 Offset;
    }
    public AlignmentFields alignmnetFields;

    [Serializable]
    public class GridFields
    {
        public GameObject[] Elements;
        public int Width;
        public int Height;
        public float WidthStride;
        public float HeightStride;
    }
    public GridFields gridFields;

    [Serializable]
    public class PolygonFields
    {
        public enum Direction_T
        {
            Right,Left
        }
        public GameObject[] Elements;
        public int Sides;
        public float AngleOffset;
        public float Radius;
        public Direction_T Direction;
    }
    public PolygonFields polygonFields;
    [Tooltip("This may affect performance in edit mode")]
    public bool AutoApply = false;
    public void Apply()
    {
        rect = GetComponent<RectTransform>();
        switch(AlignmentType)
        {
            case Type_T.Alignment:
                {
                    ApplyAlignment();
                    break;
                }
            case Type_T.Grid:
                {
                    ApplyGrid();
                    break;
                }
            case Type_T.Polygon:
                {
                    ApplyPolygon();
                    break;
                }
        }

    }
    void ApplyAlignment()
    {
        if(alignmnetFields.Target == null)
        {
            Debug.LogError("Please specify a Target.");
            return;
        }
        RectTransform targetRect = alignmnetFields.Target.GetComponent<RectTransform>();
        switch (alignmnetFields.Alignment)
        {
            case AlignmentType_T.Right:
                {
                    float RightAxis = alignmnetFields.Target.transform.position.x + targetRect.rect.width / 2;
                    float newCenterX = RightAxis - rect.rect.width / 2;
                    transform.position = new Vector3(newCenterX, transform.position.y, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
            case AlignmentType_T.Left:
                {
                    float LeftAxis = alignmnetFields.Target.transform.position.x - targetRect.rect.width / 2;
                    float newCenterX = LeftAxis + rect.rect.width / 2;
                    transform.position = new Vector3(newCenterX, transform.position.y, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
            case AlignmentType_T.Top:
                {
                    float TopAxis = alignmnetFields.Target.transform.position.y - targetRect.rect.height / 2;
                    float newCenterY = TopAxis + rect.rect.width / 2;
                    transform.position = new Vector3(transform.position.x, newCenterY, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
            case AlignmentType_T.Bottom:
                {
                    float BottomAxis = alignmnetFields.Target.transform.position.y + targetRect.rect.height / 2;
                    float newCenterY = BottomAxis - rect.rect.width / 2;
                    transform.position = new Vector3(transform.position.x, newCenterY, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
            case AlignmentType_T.Vertical:
                {
                    float VerticalAxis = alignmnetFields.Target.transform.position.x;
                    transform.position = new Vector3(VerticalAxis, transform.position.y, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
            case AlignmentType_T.Horizontal:
                {
                    float HorizontalAxis = alignmnetFields.Target.transform.position.y;
                    transform.position = new Vector3(transform.position.x, HorizontalAxis, transform.position.z) + (Vector3)alignmnetFields.Offset;
                    break;
                }
        }

    }

    void ApplyGrid()
    {
        for (int j = 0; j < gridFields.Height; j++)
        {
            for (int i = 0; i < gridFields.Width; i++)
            {
                int index = i + j * gridFields.Width;
                if (index >= gridFields.Elements.Length)
                {
                    return;
                }
                if(gridFields.Elements[index] == null)
                {
                    continue;
                }
                float xPos = transform.position.x + i * gridFields.WidthStride;
                float yPos = transform.position.y - j* gridFields.HeightStride;
                GameObject target = gridFields.Elements[index];
                target.transform.position = new Vector3(xPos, yPos, target.transform.position.z);
            }
        }
    }

    void ApplyPolygon()
    {
        if(polygonFields.Sides < 3)
        {
            Debug.LogError("Sides of the polygon must greater than 3");
        }
        float angle;
        float unit;
        if (polygonFields.Direction == PolygonFields.Direction_T.Left)
        {
            angle = -polygonFields.AngleOffset;
            unit = -Mathf.PI * 2 / polygonFields.Sides;
        }
        else
        {
            angle = polygonFields.AngleOffset;
            unit = Mathf.PI * 2 / polygonFields.Sides;
        }

        Vector2 Origin;
        //First One
        float xPos = Mathf.Cos(angle) * polygonFields.Radius;
        float yPos = Mathf.Sin(angle) * polygonFields.Radius;
        Origin = new Vector2(polygonFields.Elements[0].transform.position.x - xPos, polygonFields.Elements[0].transform.position.y - yPos);

        //The Rest
        for (int i = 1; i < polygonFields.Sides; i++)
        {
            if(i >= polygonFields.Elements.Length)
            {
                return;
            }
            angle += unit;
            if (polygonFields.Elements[i] == null)
            {
                continue;
            }
            xPos = Mathf.Cos(angle) * polygonFields.Radius + Origin.x;
            yPos = Mathf.Sin(angle) * polygonFields.Radius + Origin.y;
            polygonFields.Elements[i].transform.position = new Vector3(xPos, yPos, polygonFields.Elements[i].transform.position.z);
        }
    }


    WaitForSeconds wait = new WaitForSeconds(200);
    public void UpdateAutoApply()
    {
        if(AutoApply)
        {
            StartCoroutine(nameof(autoApplyCoroutine));
        }
    }
    IEnumerator autoApplyCoroutine()
    {
        while(AutoApply)
        {
            Apply();
            yield return wait;
        }
    }
}

#endif