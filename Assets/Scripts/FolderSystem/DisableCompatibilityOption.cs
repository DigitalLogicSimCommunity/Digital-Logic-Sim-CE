using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableCompatibilityOption : MonoBehaviour
{
    void Start()
    {
        var ChipButtonHolder = ChipBarUI.instance.chipButtonHolders;
        if (ChipButtonHolder != null && ChipButtonHolder.Count > 0)
        {
            var chipButtonHolders = ChipButtonHolder[0];
            var CompatibilityOption = transform.GetChild(1).GetComponent<Toggle>();
            if (CompatibilityOption != null)
                CompatibilityOption.interactable = chipButtonHolders.Holder.childCount != 0;
        }
    }
}
