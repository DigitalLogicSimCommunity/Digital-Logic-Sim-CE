using System.Collections;
using System.Collections.Generic;
using SebInput;
using UnityEngine;

public class ChipInterfaceEvent : MonoBehaviour
{
    public MouseInteraction<ChipInterfaceEditor> mouseInteraction;
    ChipInterfaceEditor chipInterface;

    void Awake()
    {
        chipInterface = GetComponentInParent<ChipInterfaceEditor>();
        mouseInteraction = new MouseInteraction<ChipInterfaceEditor>(gameObject,chipInterface);
    }
}
