using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(SafeAreaSetter))]
public sealed class MasterCanvas : MonoBehaviour
{
    public static MasterCanvas instance;
    
    private Canvas _canvas;
    private CanvasScaler _canvasScaler;
    public GraphicRaycaster _graphicRaycaster { get; private set; }
    private SafeAreaSetter _safeAreaSetter;

    [HideInInspector]
    public CanvasGroup _canvasGroup;

    public bool manuallySetVisible = false;

    //public float match = 1f;

    private void Awake()
    {
        instance = this;
        
        GetCanvasComponents();
        
        SetUIVisible(false);

        Initialize();
    }

    private void OnRectTransformDimensionsChange()
    {
        SetMatch();
    }

    private void GetCanvasComponents()
    {
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasScaler = GetComponent<CanvasScaler>();
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
        _safeAreaSetter = GetComponent<SafeAreaSetter>();
    }

    private void Initialize()
    {
        //_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 0;

        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = new Vector2(1280f, 720f);
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        SetMatch();
        
        _canvasScaler.referencePixelsPerUnit = 100;

        if (_safeAreaSetter.canvas == null) _safeAreaSetter.canvas = _canvas;

        if (!manuallySetVisible) SetUIVisible(true);
    }

    public void SetMatch()
    {
        if (!_canvasScaler) return;
        
        float match = 1;

        var screenRatio = (float) Screen.width / (float) Screen.height;
        var scalerRatio = _canvasScaler.referenceResolution.x / _canvasScaler.referenceResolution.y;
        
        if (screenRatio > scalerRatio)
        {
            match = 1.0f;
        }
        else
        {
            match = 0f;
        }
         
        if(_canvasScaler) _canvasScaler.matchWidthOrHeight = match;
    }

    public void SetControllerUI(bool enable)
    {
        var joystick = transform.Search("view_JoystickZone");
        // var joystickLeft = joystick.Search("joystick_left");
        // var joystickJump = joystick.Search("joystick_Jump");
        // var joystickDash = joystick.Search("joystick_Dash");
        // var joystickEmoji = joystick.Search("joystick_Emoji");

        if(joystick) joystick.gameObject.SetActive(enable);
    }

    public void SetUIVisible(bool enable)
    {
        _canvasGroup.alpha = enable ? 1 : 0;
        _canvasGroup.interactable = enable;
        _canvasGroup.blocksRaycasts = enable;
        GetComponent<GraphicRaycaster>().enabled = enable;
    }
}
