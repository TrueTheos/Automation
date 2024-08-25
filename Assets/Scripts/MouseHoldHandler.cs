using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MouseHoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnMouseDownEvent;
    public UnityEvent OnMouseUpEvent;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnMouseDownEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnMouseUpEvent.Invoke();
    }
}
