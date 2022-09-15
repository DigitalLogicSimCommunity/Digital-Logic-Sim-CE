using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum MenuType
{
    None = -1,
    CreateChipMenu = 0,
    EditChipMenu = 1,
    LoggingMenu = 2,
    NewFolderMenu = 3,
    SubmitMenu = 4,
    ClockMenu = 5,
    EditFolderMenu = 6,
};

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("References")]
    public GameObject createButton;
    public GameObject updateButton ;
    public GameObject outsideMenuArea;

    public MenuDictionary Menus;

    public Palette palette;
    public bool IsAnyMenuOpen => OpenedMenu != null;
    MenuType currentMenuType = MenuType.None;
    
    UIMenu OpenedMenu;

    ClockMenu ClockMenu;
    EditChipMenu editChipMenu;

    void Awake()
    {
        instance = this;
        foreach (var menu in Menus)
            menu.Value.Close();
        outsideMenuArea.SetActive(false);

        ClockMenu = FindObjectOfType<ClockMenu>(true);
        editChipMenu = FindObjectOfType<EditChipMenu>(true);

    }

    public static void NewSubmitMenu(string header, string text,
                                     UnityAction onSubmit)
    {
        SubmitMenu submitMenu = instance.Menus[MenuType.SubmitMenu].GetComponent<SubmitMenu>();
        submitMenu.SetHeaderText(header);
        submitMenu.SetContentText(text);
        submitMenu.SetOnSubmitAction(onSubmit);
        instance.OpenMenu(MenuType.SubmitMenu);
    }

    public static Palette Palette => instance.palette;

    public void OpenCreateChipMenu() => OpenMenu(MenuType.CreateChipMenu);
    public void OpenMenu(MenuType menuType)
    {
        UIMenu newMenu = Menus[menuType];
        if (OpenedMenu && OpenedMenu != newMenu)
            CloseMenu();
        SetCurrentMenuState(newMenu, menuType);

        if (OpenedMenu.showBG)
            outsideMenuArea.SetActive(true);

        SetMenuPosition();
        OpenedMenu.Open();

        SetActiveInteraction(false);
    }
    public void CloseMenu()
    {
        if (OpenedMenu)
        {
            OpenedMenu.Close();
            SetCurrentMenuState(null, MenuType.None);
        }
        outsideMenuArea.SetActive(false);
        SetActiveInteraction(true);
    }

    private void SetActiveInteraction(bool IsActive)
    {
        FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(IsActive);
        FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(IsActive);
    }

    public void SetEditorMode(ChipEditorMode newMode)
    {
        createButton.SetActive(newMode == ChipEditorMode.Create);
        updateButton.SetActive(newMode == ChipEditorMode.Update);
    }
    private void SetCurrentMenuState(UIMenu newMenu, MenuType menuType)
    {
        OpenedMenu = newMenu;
        currentMenuType = menuType;
    }
    void SetMenuPosition()
    {
        if (currentMenuType == MenuType.EditChipMenu)
            SetChipEditMenuPosition();
        if (currentMenuType == MenuType.ClockMenu)
            SetClockMenuPosition();
    }

    void SetChipEditMenuPosition()
    {
        if (currentMenuType != MenuType.EditChipMenu) return;
        foreach (GameObject obj in InputHelper.GetUIObjectsUnderMouse())
        {
            ButtonText buttonText = obj.GetComponent<ButtonText>();
            if (buttonText != null)
            {
                editChipMenu.EditChipInit(buttonText.buttonText.text);
                OpenedMenu.transform.position = new Vector3(obj.transform.position.x, OpenedMenu.transform.position.y, OpenedMenu.transform.position.z);
                RectTransform rect = OpenedMenu.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(Mathf.Clamp(rect.anchoredPosition.x, -800, 800), rect.anchoredPosition.y);
                break;
            }
        }
    }

    void SetClockMenuPosition()
    {
        var Clock = InputHelper.GetObjectUnderMouse2D(1 << LayerMask.NameToLayer("Chip")).GetComponent<Clock>();
        if (Clock != null)
        {
            ClockMenu.SetClockToEdit(Clock);
            OpenedMenu.transform.position = new Vector3(Clock.transform.position.x, Clock.transform.position.y-2, OpenedMenu.transform.position.z);
        }
    }


    public void OnClickOutsideMenu()
    {
        if (OpenedMenu != null && OpenedMenu.onClickBG != null)
            OpenedMenu.onClickBG.Invoke();
    }
}
