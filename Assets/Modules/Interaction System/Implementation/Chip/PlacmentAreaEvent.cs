using System;
using System.Collections;
using System.Collections.Generic;
using SebInput;
using SebInput.Internal;
using UnityEngine;
using UnityEngine.Scripting;

public class PlacmentAreaEvent : MonoBehaviour
{
    public MouseInteraction<PlacmentAreaEvent> MouseInteraction;

    private void Awake()
    {
        MouseInteraction = new MouseInteraction<PlacmentAreaEvent>(gameObject,this);
    }
}