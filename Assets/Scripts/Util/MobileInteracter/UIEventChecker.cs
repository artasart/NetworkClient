using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// 해당 UI의 클릭 상태와 드래그 상태를 체크해주는 컴포넌트입니다.
/// 상태 체크를 하고 싶은 UI 게임 오브젝트에 추가한 후 해당 클래스를 참조하여 사용해주세요.
/// 
/// 작성자: 변고경
/// 작성일자: 2021/8/5
/// </summary>
public class UIEventChecker : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler
{
    [Tooltip("현재 포인터 포지션")]
    public Vector2 pointerPosition;
    [Tooltip("현재 포인터의 델타 포지션")]
    public Vector2 pointerDeltaPosition;
    [Tooltip("포인터 다운 포지션")]
    public Vector2 pointerDownPosition;
    [Tooltip("포인터 업 포지션")]
    public Vector2 pointerUpPosition;

    [Tooltip("해당 UI를 드래그 중이면 True.")]
    public bool dragging;
    [Tooltip("해당 UI를 클릭하면 True.")]
    public bool clicked;

    [Tooltip("해당 RectTransform을 인터렉션하고 있으면 True.")]
    public bool interect;

    public bool focusWhenClick;

    [FormerlySerializedAs("clickPoint")] public float reclickDelay = 0.2f;
    [FormerlySerializedAs("dragThreshold")] [FormerlySerializedAs("dragThreshhold")] public float redragDelay = 0.1f;

    public TouchPhase phase = TouchPhase.Stationary;

    [FormerlySerializedAs("clearWhenDisalbe")] public bool clearWhenDisable = true;

    public UnityEvent<PointerEventData> onBeginDrag = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onDrag = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onEndDrag = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerClick = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerDown = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerUp = new UnityEvent<PointerEventData>();

    private void OnDisable()
    {
        if (!clearWhenDisable) return;

        pointerPosition = Vector2.zero;
        pointerDeltaPosition = Vector2.zero;
        pointerDownPosition = Vector2.zero;
        pointerUpPosition = Vector2.zero;
        dragging = false;
        clicked = false;
        interect = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //StartCoroutine(WaitDragEnable(dragThreshhold));
        onBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        pointerPosition = eventData.position;
        pointerDeltaPosition = eventData.delta;
        
        onDrag?.Invoke(eventData);
        
        dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(WaitDragEnable(redragDelay, false));
        onEndDrag?.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (focusWhenClick) EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);
        
        onPointerClick?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), eventData.position)) interect = true;
        
        pointerDownPosition = eventData.position;
        pointerPosition = eventData.position;

        switch (phase)
        {
            case TouchPhase.Began:
                clicked = true;
                break;
            case TouchPhase.Stationary:
                dragging = false;
                clicked = false;
                break;
            case TouchPhase.Moved:
                dragging = true;
                clicked = false;
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                clicked = false;
                break;
        }
        
        onPointerDown?.Invoke(eventData);
        
        //clicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerUpPosition = eventData.position;
        pointerPosition = eventData.position;

        switch (phase)
        {
            case TouchPhase.Began:
                clicked = false;
                break;
            case TouchPhase.Stationary:
                if (dragging) break;

                clicked = true;
                StartCoroutine(WaitClickDisable(reclickDelay));
                break;
            case TouchPhase.Moved:
                clicked = false;
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                clicked = true;
                StartCoroutine(WaitClickDisable(reclickDelay));
                break;
        }
        
        onPointerUp?.Invoke(eventData);

        //interect = false;
        //dragging = false;
        //clicked = false;
    }

    private void Update()
    {
        if (!dragging) pointerDeltaPosition = Vector2.zero;

        //#if (!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR
        //        pointerPosition = Input.mousePosition;
        //#endif
    }

    IEnumerator WaitClickDisable(float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        clicked = false;
        interect = false;
    }

    IEnumerator WaitDragEnable(float sec, bool enable = true)
    {
        yield return new WaitForSecondsRealtime(sec);
        dragging = enable;
        interect = false;
    }
}