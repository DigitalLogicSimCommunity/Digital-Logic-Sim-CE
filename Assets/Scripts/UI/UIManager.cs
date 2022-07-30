using System.Collections.Generic;
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
    RenameFolderMenu = 6
}
;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("References")]
    public GameObject createButton;
    public GameObject updateButton;
    public List<UIMenu> menus;
    public GameObject outsideMenuArea;

    public Palette palette;
    public bool IsAnyMenuOpen => OpenedMenu != null;
    UIMenu OpenedMenu;
    MenuType currentMenuType = MenuType.None;
    void Awake()
    {
        instance = this;
        foreach (UIMenu menu in menus)
            menu.Close();
        outsideMenuArea.SetActive(false);
    }

    public static void NewSubmitMenu(string header, string text,
                                     UnityAction onSubmit)
    {
        SubmitMenu submitMenu = instance.GetMenuByType(MenuType.SubmitMenu).GetComponent<SubmitMenu>();
        submitMenu.SetHeaderText(header);
        submitMenu.SetContentText(text);
        submitMenu.SetOnSubmitAction(onSubmit);
        instance.OpenMenu(MenuType.SubmitMenu);
    }

    public static Palette Palette => instance.palette;

    public void OpenMenu(int index) => OpenMenu((MenuType)index);
    public void OpenMenu(MenuType menuType)
    {
        UIMenu newMenu = GetMenuByType(menuType);
        if (OpenedMenu && OpenedMenu != newMenu)
            CloseMenu();
        SetCurrentMenuState(newMenu, menuType);

        if (OpenedMenu.showBG)
            outsideMenuArea.SetActive(true);


        SetMenuPosition();
        OpenedMenu.Open();

        FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(false);
        FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(false);
    }

    private void SetCurrentMenuState(UIMenu newMenu, MenuType menuType)
    {
        OpenedMenu = newMenu;
        currentMenuType = menuType;
    }


    public void CloseMenu()
    {
        if (OpenedMenu)
        {
            OpenedMenu.Close();
            SetCurrentMenuState(null, MenuType.None);
        }
        outsideMenuArea.SetActive(false);
        FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(true);
        FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(true);
    }

    public void SetEditorMode(ChipEditorMode newMode)
    {
        createButton.SetActive(newMode == ChipEditorMode.Create);
        updateButton.SetActive(newMode == ChipEditorMode.Update);
    }

    void SetMenuPosition()
    {
        if (currentMenuType == MenuType.EditChipMenu)
        {
            SetChipEditMenuPosition();
        }
        if (currentMenuType == MenuType.ClockMenu)
        {
            SetClockMenuPosition();
        }
    }

    void SetChipEditMenuPosition()
    {
        if (currentMenuType != MenuType.EditChipMenu) return;
        foreach (GameObject obj in InputHelper.GetUIObjectsUnderMouse())
        {
            ButtonText buttonText = obj.GetComponent<ButtonText>();
            if (buttonText != null)
            {
                FindObjectOfType<EditChipMenu>().EditChipInit(buttonText.buttonText.text);
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
            FindObjectOfType<ClockMenu>().SetClockToEdit(Clock);
            OpenedMenu.transform.position = new Vector3(Clock.transform.position.x, Clock.transform.position.y, OpenedMenu.transform.position.z);
            RectTransform rect = OpenedMenu.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(Mathf.Clamp(rect.anchoredPosition.x, -800, 800), 350);
        }
    }


    public void OnClickOutsideMenu()
    {
        if (OpenedMenu.onClickBG != null)
        {
            OpenedMenu.onClickBG.Invoke();
        }
    }

    public UIMenu GetMenuByIndex(int index)
    {
        return index + 1 > menus.Count ? null : menus[index];
    }

    public UIMenu GetMenuByType(MenuType menuType)
    {
        return menus[(int)menuType];
    }
}
