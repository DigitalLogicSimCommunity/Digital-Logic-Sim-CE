using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class ReadVersion : MonoBehaviour
{
    TMP_Text versionText;

    void Awake()
    {
        versionText = GetComponent<TMP_Text>();
    }
    void Start()
    {
        versionText.text = $"Version {GameConstant.GAMEVERSION} - CE <size=25><color=#505050> ({GameConstant.LASTEDIT})</color></size>";
    }


}
