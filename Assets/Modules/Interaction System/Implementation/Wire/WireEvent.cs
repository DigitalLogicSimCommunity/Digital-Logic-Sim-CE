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



}