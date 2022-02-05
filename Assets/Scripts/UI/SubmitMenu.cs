using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class SubmitMenu : UIMenu {

    public TMP_Text headerText;
    public TMP_Text contentText;
    public Button submitButton;

    public void SetHeaderText(string text) {
        headerText.text = text;
    }

    public void SetContentText(string text) {
        contentText.text = text;
    }

    public void SetOnSubmitAction(UnityAction action) {
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(action);
    }

}
