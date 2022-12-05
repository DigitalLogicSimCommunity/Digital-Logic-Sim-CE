using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private Interactable InteractableWhitFocus;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (InteractableWhitFocus == null) return;

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Backspace, KeyCode.Delete) || Input.GetMouseButton(2))
            InteractableWhitFocus.DeleteCommand();
    }

    public bool HadFocus(Interactable interactable) => InteractableWhitFocus == interactable;
    public void ReleaseFocus(Interactable interactable)
    {
        if (HadFocus(interactable))
        {
            InteractableWhitFocus.HasFocus = false;
            InteractableWhitFocus = null;
        }
    }

    public bool RequestFocus(Interactable interactable)
    {

        if (InteractableWhitFocus == null)
            SetInteragibleWhitFocus(interactable);
        else if (interactable != InteractableWhitFocus)
        {
            if (InteractableWhitFocus.CanReleaseFocus())
            {
                InteractableWhitFocus.HasFocus = false;
                InteractableWhitFocus.FocusLostHandler();
                SetInteragibleWhitFocus(interactable);
            }
            else
                return false;
        }

        return true;
    }

    private void SetInteragibleWhitFocus(Interactable interactable)
    {
        InteractableWhitFocus = interactable;
        InteractableWhitFocus.HasFocus = true;
    }
}
