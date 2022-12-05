using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for systems that handle user input:

// Only one system can have focus at a time. If a system wants focus, it must
// call RequestFocus(). The system that currently has focus will determine if it
// CanReleaseFocus(). If yes: • FocusLost() will be called on that system. •
// Requesting system will have HasFocus set to true.

public abstract class Interactable : MonoBehaviour
{
    // Does this system currently have focus?
    public bool HasFocus { get; set; } = false;


    public abstract void OrderedUpdate();

    public abstract void DeleteCommand();




    public virtual void FocusLostHandler() { }
    public virtual bool CanReleaseFocus() => true;

    protected bool RequestFocus() => InteractionManager.Instance.RequestFocus(this);
    protected void ReleaseFocusNotHandled() => InteractionManager.Instance.ReleaseFocus(this);
    protected void ReleaseFocus() { InteractionManager.Instance.ReleaseFocus(this);FocusLostHandler(); }
}
