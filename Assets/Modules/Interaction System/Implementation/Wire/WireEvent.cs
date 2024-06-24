using System;
using System.Collections;
using System.Collections.Generic;
using SebInput;
using SebInput.Internal;
using UnityEngine;
using UnityEngine.Serialization;

//Note: this can be probably substitute with SebInput System
public class WireEvent : MonoBehaviour
{
    public Wire wire;
    public event Action OnWireEnter;
    public event Action OnWireExit;
    public MouseInteraction<Wire> MouseInteraction;

    private void Awake()
    {
        wire = GetComponentInParent<Wire>();
        MouseInteraction = new MouseInteraction<Wire>(gameObject, wire);
    }

    private void Start()
    {
        Manager.PinAndWireInteraction.RegisterWire(MouseInteraction);
    }


    private void OnMouseEnter()
    {
        OnWireEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        OnWireExit?.Invoke();
    }
}