using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VitoBarra.System.Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance;

        private Interactable InteractableWhitFocus;

        //CommandExecutes ExecutionComand;

        private void Awake()
        {
            Instance = this;
            //ExecutionComand = new CommandExecutes();
        }

        private void Update()
        { 
            //TODO: generalize
            if (InteractableWhitFocus == null) return;
            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Backspace, KeyCode.Delete) || Input.GetMouseButton(2))
                InteractableWhitFocus.DeleteCommand();
            //END TODO
            
            //ExecutionCommand.Execute();
            
        }

        protected bool HadFocus(Interactable interactable) => InteractableWhitFocus == interactable;

        public void ReleaseFocus(Interactable interactable)
        {
            if (!HadFocus(interactable)) return;
            InteractableWhitFocus = null;
        }

        public bool RequestFocus(Interactable interactable)
        {
            if (HadFocus(interactable)) return true;
            if (InteractableWhitFocus == null)
            {
                SetIntelligibleWhitFocus(interactable);
                return true;
            }

            if (!InteractableWhitFocus.CanReleaseFocus()) return false;


            InteractableWhitFocus.ReleaseFocus();
            SetIntelligibleWhitFocus(interactable);
            return true;
        }

        private void SetIntelligibleWhitFocus(Interactable interactable)
        {
            InteractableWhitFocus = interactable;
        }

        private void OnDestroy()
        {
           // ExecutionComand.UnregisterEvent();
        }
    }
}