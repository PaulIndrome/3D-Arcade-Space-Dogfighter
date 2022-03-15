using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class ScreenSpacePlacer : MonoBehaviour
{

    public enum OffScreenBehaviour {
        Free,
        DontDraw,
        PlaneEdge,
        PlaneEdgeRotate,
        PointArrow
    }

    [Header("Scene references")]
    [SerializeField] private Transform transformToFollow;
    [SerializeField] private Image parentImage;
    [SerializeField] private RectTransform drawPlane;

    [Header("Settings")]
    [SerializeField] private OffScreenBehaviour offScreenBehaviour = OffScreenBehaviour.PlaneEdge;

    [Header("Internals")]
    [ReadOnly, SerializeField] private Vector2 screenPos, halfSizeDelta;
    [ReadOnly, SerializeField] private Vector2 drawPlaneMin, drawPlaneMax;
    [ReadOnly, SerializeField] private Vector2 screenPosUnclamped;


    private Camera mainCam;
    private RectTransform rect;
    private Rect drawPanelPixelPos;
    private Canvas parentCanvas;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        mainCam = Camera.main;
        if(!drawPlane){
            drawPlane = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }

        rect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        drawPanelPixelPos = RectTransformUtility.PixelAdjustRect(drawPlane, parentCanvas);
    }
    

    // Update is called once per frame
    void Update()
    {
        screenPos = mainCam.WorldToScreenPoint(transformToFollow.position) / parentCanvas.scaleFactor;
        screenPosUnclamped = screenPos;
        // screenPos = rect.anchoredPosition;

        ExecuteOffScreenBehaviour();

        rect.anchoredPosition = screenPos;
    }

    void ExecuteOffScreenBehaviour(){
        parentImage.gameObject.SetActive(offScreenBehaviour != OffScreenBehaviour.DontDraw);
        if(offScreenBehaviour == OffScreenBehaviour.DontDraw) return;
        
        drawPlaneMin = drawPlane.anchoredPosition - (drawPlane.sizeDelta * drawPlane.pivot);
        drawPlaneMax = drawPlaneMin + drawPlane.sizeDelta;

        halfSizeDelta = rect.sizeDelta * 0.5f;

        switch(offScreenBehaviour){
            case OffScreenBehaviour.PointArrow:
                break;
            case OffScreenBehaviour.PlaneEdge:
                screenPos.x = Mathf.Clamp(screenPos.x, drawPlaneMin.x + halfSizeDelta.x, drawPlaneMax.x - halfSizeDelta.x);
                screenPos.y = Mathf.Clamp(screenPos.y, drawPlaneMin.y + halfSizeDelta.y, drawPlaneMax.y - halfSizeDelta.y);
                break;
            case OffScreenBehaviour.PlaneEdgeRotate:
                break;
        }
    }

    void OnValidate()
    {
        if(parentImage?.gameObject == gameObject){
            Debug.LogError("ScreenSpacePlacer's and its parentImage gameobject can not be identical. Resetting.", this);
            parentImage = null;
        }
    }
}
