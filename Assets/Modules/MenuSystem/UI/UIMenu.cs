using System;
using UnityEngine;
using UnityEngine.Events;

public class UIMenu : MonoBehaviour
{
    public bool showBG;
    public MenuType menuType;
    public UnityEvent onClickBG;
    [HideInInspector]
    public bool isActive = false;
    event Action OnMenuUIClose;

    public void Open()
    {
        isActive = true;
        gameObject.SetActive(isActive);
    }

    public void Close()
    {
        isActive = false;
        gameObject.SetActive(isActive);
        OnMenuUIClose?.Invoke();
    }

    public void RegisterOnUIClose(Action action)
    {
        OnMenuUIClose += action;
    }

}
