using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;
using VitoBarra.Utils.T;

public class HandleEvent : MonoBehaviour
{
    public event Action OnHandleEnter;
    public event Action OnHandleExit;
    public event Action OnHandleClick;
    public event Action<Vector3> OnStartDrag;
    public event Action OnDrag;
    public event Action OnStopDrag;

    private bool isDragging = false;
    public Delayer delayer = new Delayer(0.1f);



    private void OnMouseEnter()
    {
        OnHandleEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        OnHandleExit?.Invoke();
    }

    private void OnMouseDown()
    {
        delayer.StartCount();
        OnHandleClick?.Invoke();
    }

    private void OnMouseUp()
    {
        if (!isDragging)
        {
            OnStopDrag?.Invoke();
        }

        isDragging = false;
    }

    private void OnMouseDrag()
    {
        if (!isDragging && delayer.IsDelayPassed)
        {
            OnStartDrag?.Invoke(transform.position);
            isDragging = true;
        }

        if (isDragging)
        {
            OnDrag?.Invoke();
        }
    }
}