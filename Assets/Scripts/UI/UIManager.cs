using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MenuType {
    CreateChipMenu = 0,
    EditChipMenu = 1,
    LoggingMenu = 2,
    NewFolderMenu = 3,
    SubmitMenu = 4
};

public class UIManager : MonoBehaviour {
    public static UIManager instance;

    [Header("References")]
    public GameObject createButton;
    public GameObject updateButton;
    public List<UIMenu> menus;
    public GameObject outsideMenuArea;

    public Palette palette;

    UIMenu currentOpenedMenu;


    void Awake() {
        instance = this;
        foreach (UIMenu menu in menus) {
            menu.Close();
        }
        outsideMenuArea.SetActive(false);
    }

    public static void NewSubmitMenu(string header, string text, UnityAction onSubmit) {
        SubmitMenu submitMenu = instance.GetMenuByType(MenuType.SubmitMenu).GetComponent<SubmitMenu>();
        submitMenu.SetHeaderText(header);
        submitMenu.SetContentText(text);
        submitMenu.SetOnSubmitAction(onSubmit);
        instance.OpenMenu(MenuType.SubmitMenu);
    }

    public static Palette Palette {
        get {
            return instance.palette;
        }
    }

    public void OpenMenu(MenuType menuType) {
        SetMenuPosition(menuType);
        UIMenu newMenu = GetMenuByType(menuType);
        if (currentOpenedMenu && currentOpenedMenu != newMenu) {
            CloseMenu();
        }

        if (newMenu.showBG) {
            outsideMenuArea.SetActive(true);
        }

        currentOpenedMenu = newMenu;
        currentOpenedMenu.Open();

        FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(false);
        FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(false);
    }

    public void OpenMenu(int index) {
        OpenMenu((MenuType)index);
    }

    public void CloseMenu() {
        if (currentOpenedMenu) {
            currentOpenedMenu.Close();
            currentOpenedMenu = null;
        }
        outsideMenuArea.SetActive(false);
        FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(true);
        FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(true);
    }

    public void SetEditorMode(ChipEditorMode newMode) {
        createButton.SetActive(newMode == ChipEditorMode.Create);
        updateButton.SetActive(newMode == ChipEditorMode.Update);
    }

    void SetMenuPosition(MenuType menuType) {
        if (menuType == MenuType.EditChipMenu) {
            SetChipEditMenuPosition();
        }
    }

    void SetChipEditMenuPosition() {
        foreach (GameObject obj in InputHelper.GetUIObjectsUnderMouse()) {
            ButtonText buttonText = obj.GetComponent<ButtonText>();
            if (buttonText != null) {
                FindObjectOfType<EditChipMenu>().EditChip(buttonText.buttonText.text);
                menus[1].transform.position = new Vector3(
                    obj.transform.position.x,
                    menus[1].transform.position.y,
                    menus[1].transform.position.z
                );
                RectTransform rect = menus[1].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2 (
                    Mathf.Clamp(rect.anchoredPosition.x, -800, 800),
                    rect.anchoredPosition.y
                );
                break;
            }
        }
    }

    public void OnClickOutsideMenu() {
        if (currentOpenedMenu.onClickBG != null) {
            currentOpenedMenu.onClickBG.Invoke();
        }
    }

    public UIMenu GetMenuByIndex(int index) {
        return index + 1 > menus.Count ? null : menus[index];
    }

    public UIMenu GetMenuByType(MenuType menuType) {
        return menus[(int)menuType];
    }

}
