using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuBarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public GameObject dropdown;

	public GameObject[] subMenus;

	void Start() {
		dropdown.SetActive(false);
		foreach(GameObject subMenu in subMenus) {
			subMenu.SetActive(false);
		}
	}

	public void OnPointerEnter (PointerEventData eventData) {
		dropdown.SetActive(true);
	}

	public void OnPointerExit (PointerEventData eventData) {
		dropdown.SetActive(false);
		foreach(GameObject subMenu in subMenus) {
			subMenu.SetActive(false);
		}
	}

}
