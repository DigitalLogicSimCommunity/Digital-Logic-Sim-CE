using System;
using System.Collections;
using System.Collections.Generic;
using SebInput;
using SebInput.Internal;
using UnityEngine;

//Note: this can be probably substitute with SebInput System
public class PinEvent : MonoBehaviour
{
    public MouseInteraction<Pin> MouseInteraction;

    private void Awake()
    {
        var pinReference = GetComponentInParent<Pin>();
        MouseInteraction = new MouseInteraction<Pin>(gameObject, pinReference);
    }

    private void Start()
    {
        Manager.PinAndWireInteraction.RegisterPin(MouseInteraction);
    }
}