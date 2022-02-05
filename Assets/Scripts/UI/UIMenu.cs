using UnityEngine;
using UnityEngine.Events;

public class UIMenu : MonoBehaviour {
	public bool showBG;
	public UnityEvent onClickBG;

	[HideInInspector] public bool isActive = false;

	public void Open() {
		isActive = true;
		gameObject.SetActive(isActive);
	}

	public void Close() {
		isActive = false;
		gameObject.SetActive(isActive);
	}
    
}
