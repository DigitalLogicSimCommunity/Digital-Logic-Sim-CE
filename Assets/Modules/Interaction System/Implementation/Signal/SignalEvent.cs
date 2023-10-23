using System;
using System.Collections;
using System.Collections.Generic;
using SebInput;
using SebInput.Internal;
using UnityEngine;

[RequireComponent(typeof(MouseInteractionListener))]
public class SignalEvent : MonoBehaviour
{
    public MouseInteractionListener MouseInteraction;


    private void Awake()
    {
        MouseInteraction = GetComponent<MouseInteractionListener>();
    }
}
