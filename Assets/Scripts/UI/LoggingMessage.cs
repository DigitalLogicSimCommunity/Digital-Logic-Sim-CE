using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoggingMessage : MonoBehaviour {

    public Image iconImage;
    public TMP_Text headerText;
    public TMP_Text contentText;

    public GameObject contentHolder;

    public Sprite arrowDown;
    public Sprite arrowUp;

    public Button dropDownButon;

    bool open = false;

    public void ToggleOpen() {
        open = !open;
        dropDownButon.image.sprite = open ? arrowUp : arrowDown;
        contentHolder.gameObject.SetActive(open);
    }

}
