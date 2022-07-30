using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableCompatibilityOption : MonoBehaviour
{
    void Start()
    {
        var a = ChipBarUI.instance.chipButtonHolders;
        if (a != null && a.Count > 0)
        {
            var chipButtonHolders = a[0];
            var CompatibilityOption = transform.GetChild(1).GetComponent<Toggle>();
            if (CompatibilityOption != null)
                CompatibilityOption.interactable = chipButtonHolders.childCount != 0;
        }
    }
}
