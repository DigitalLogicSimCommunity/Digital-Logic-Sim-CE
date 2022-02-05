using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
	public static ProgressBar instance;
    public GameObject loadingScreen;
	public Slider progressBar;
	public Image fill;
	public TMP_Text titleText;
	public TMP_Text infoText;
	public TMP_Text indicatorText;

	public Color[] suggestedColours;

	void Awake() {
		instance = this;
	}

	public static ProgressBar New(string title = "Loading...", bool wholeNumbers = false) {
		int suggestedColourIndex = Random.Range (0, instance.suggestedColours.Length);
		Color randomColor = instance.suggestedColours[suggestedColourIndex];
		randomColor.a = 1;
		instance.fill.color = randomColor;
		instance.progressBar.wholeNumbers = wholeNumbers;
		
		instance.infoText.text = "Start Loading...";
		instance.titleText.text = title;
		instance.loadingScreen.SetActive(true);
		return instance;
	}

	public void Open(float minValue, float maxValue) {
		progressBar.minValue = minValue;
		progressBar.maxValue = maxValue;
		loadingScreen.SetActive(true);
	}

	public void Close() {
		loadingScreen.SetActive(false);
	}

	public void SetValue(float value, string info = "") {
		infoText.text = info;
		indicatorText.text = value.ToString() + "/" + progressBar.maxValue.ToString();
		progressBar.value = value;
	}
}
