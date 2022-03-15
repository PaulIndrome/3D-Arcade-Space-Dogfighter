using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
public class UIRectClamp : MonoBehaviour
{

    [ReadOnly] public Vector3 clampedPosition;
    [ReadOnly] public Vector2 clampRectAnchorMin, clampRectAnchorMax;
    [ReadOnly] public Rect clampRectPixelRect;
    [ReadOnly] public Vector2 clampRectScreenSpaceMin, clampRectScreenSpaceMax;
    public RectTransform clampRect;

    private RectTransform thisRect;
    private Canvas canvas;

    void Awake()
    {
        thisRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if(!clampRect) return;
        if(!canvas) canvas = GetComponentInParent<Canvas>();

        clampRectAnchorMin = clampRect.anchorMax;
        clampRectAnchorMin = clampRect.anchorMin;

        clampRectPixelRect = RectTransformUtility.PixelAdjustRect(clampRect, canvas);
        
        clampRectScreenSpaceMin = clampRect.anchoredPosition;
        clampRectScreenSpaceMax = clampRect.anchoredPosition + clampRect.sizeDelta;

        clampedPosition = thisRect.anchoredPosition;
        
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, clampRectScreenSpaceMin.x, clampRectScreenSpaceMax.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, clampRectScreenSpaceMin.y, clampRectScreenSpaceMax.y);

        thisRect.anchoredPosition = clampedPosition;
    }

}
