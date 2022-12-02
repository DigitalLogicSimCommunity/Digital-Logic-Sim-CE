using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetFrameRate : MonoBehaviour {
	public TMP_Dropdown vSyncRate;
	public TMP_Dropdown FpsTarget;

	void Awake () {
	    // Its a dead simple app. There's no need for 120 fps
	    // By default the vSyncCount is 2 (1/2 of max fps, ex. 120/2 = 60)
	    PlayerPrefs.DeleteAll();

	    // We opt to use vSync by default with vSyncCount of 1/2
	    vSyncRate.value = PlayerPrefs.GetInt("vSyncRate", 2);
	    QualitySettings.vSyncCount = vSyncRate.value;
	    
	    // By default TargetFrames is 0 (-1)
	    FpsTarget.value = PlayerPrefs.GetInt("fpsTarget", 0);
	    Application.targetFrameRate =  FpsTarget.value != 0 ? FpsTarget.value * 10 : -1;
	}
	
	public void SetVSyncRatio(System.Int32 value)
	{
		// Clear fpsTarget
		PlayerPrefs.SetInt("fpsTarget", 0);
		FpsTarget.value = 0;
		Application.targetFrameRate = -1;
		
		if (value == 0)
		{
			SetFpsTarget(3);
		}
		
		PlayerPrefs.SetInt("vSyncRate", value);
		vSyncRate.value = value;
		
		QualitySettings.vSyncCount = value;
	}

	public void SetFpsTarget(System.Int32 value)
	{
		// Clear vSync Count
		PlayerPrefs.SetInt("vSyncRate", 0);
		vSyncRate.value = 0;
		QualitySettings.vSyncCount = 0;
		
		if (value == 0)
		{
			SetVSyncRatio(2);
		}
		
		PlayerPrefs.SetInt("fpsTarget", value);
		FpsTarget.value = value;
		
		Application.targetFrameRate = value != 0 ? value * 10 : -1;
	}
}
