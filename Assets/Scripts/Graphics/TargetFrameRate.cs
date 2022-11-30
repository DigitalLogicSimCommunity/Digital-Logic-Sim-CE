using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetFrameRate : MonoBehaviour {
	public TMP_Dropdown vSyncRate;
	
	void Awake () {
	    // Its a dead simple app. There's no need for 120 fps
	    // By default the vSyncCount is 4 (1/4 of max fps, ex. 120/4 = 30)
	    vSyncRate.value = PlayerPrefs.GetInt("vSyncRate", 3);
	    QualitySettings.vSyncCount = vSyncRate.value + 1;
	}
	
	public void SetVSyncRatio(System.Int32 value)
	{
		PlayerPrefs.SetInt("vSyncRate", value);
		QualitySettings.vSyncCount = value + 1;
		vSyncRate.value = value;
	}
}
