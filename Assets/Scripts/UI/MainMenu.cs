using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
  public static MainMenu instance;

  public TMP_InputField projectNameField;
  public Button confirmProjectButton;
  public Toggle fullscreenToggle;
  public TMP_Dropdown vSyncRate;
  public TMP_Dropdown fpsTarget;

  void Awake() {
    instance = this;
    fullscreenToggle.onValueChanged.AddListener(SetFullScreen);
    
    // We opt to use vSync by default with vSyncCount of 1/2
    vSyncRate.value = PlayerPrefs.GetInt("vSyncRate", 2);
    // By default TargetFrames is 0 (-1)
    fpsTarget.value = PlayerPrefs.GetInt("fpsTarget", 0);
  }
  
  public void SetVSyncRatio(System.Int32 value)
  {
    // Clear fpsTarget
    PlayerPrefs.SetInt("fpsTarget", 0);
    fpsTarget.value = 0;
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
    fpsTarget.value = value;
		
    Application.targetFrameRate = value != 0 ? value * 10 : -1;
  }

  void LateUpdate() {
    confirmProjectButton.interactable = projectNameField.text.Trim().Length > 0;
    if (fullscreenToggle.isOn != Screen.fullScreen) {
      fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }
  }

  public void StartNewProject() {
    string projectName = projectNameField.text;
    SaveSystem.ActiveProjectName = projectName;
    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
  }

  public void SetResolution16x9(int width) {
    Screen.SetResolution(width, Mathf.RoundToInt(width * (9 / 16f)),
                         Screen.fullScreenMode);
  }

  public void SetFullScreen(bool fullscreenOn) {
    // Screen.fullScreen = fullscreenOn;
    var nativeRes = Screen.resolutions[Screen.resolutions.Length - 1];
    if (fullscreenOn) {
      Screen.SetResolution(nativeRes.width, nativeRes.height,
                           FullScreenMode.FullScreenWindow);
    } else {
      float windowedScale = 0.75f;
      int x = nativeRes.width / 16;
      int y = nativeRes.height / 9;
      int m = (int)(Mathf.Min(x, y) * windowedScale);
      Screen.SetResolution(16 * m, 9 * m, FullScreenMode.Windowed);
    }
  }

  public void Quit() { Application.Quit(); }
}
