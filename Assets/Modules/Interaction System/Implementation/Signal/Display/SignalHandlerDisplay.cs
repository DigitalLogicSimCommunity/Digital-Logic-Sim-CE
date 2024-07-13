using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;
using static EditorInterfaceType;


namespace Interaction.Signal.Display
{
    public enum HandleState
    {
        Default,
        Highlighted,
        Focused
    }

    public class SignalHandlerDisplay : MonoBehaviour
    {
        private Renderer HandlerRender;
        private SignalInteraction Interaction;
        public EditorInterfaceType mode;

        private bool ColorCanChange;

        private void Awake()
        {
            Interaction = GetComponentInParent<SignalInteraction>();
            HandlerRender = GetComponent<Renderer>();
            var Lissener = GetComponentInParent<HandlerEvent>();
            RegisterToHandleGroup(Lissener);

            if (Interaction == null) return;

            Interaction.OnFocusObtained += OnFocusObtainedHandler;
            Interaction.OnFocusLost += OnFocusLostHandler;


            ScalingManager.i.OnScaleChange += UpdateScale;
        }

        private void OnFocusObtainedHandler()
        {
            ChangeHandleColor(HandleState.Focused);
            ColorCanChange = false;
        }

        private void OnFocusLostHandler()
        {
            ChangeHandleColor(HandleState.Default);
            ColorCanChange = true;
        }

        private void Start()
        {
            UpdateScale();
        }

        private void OnDestroy()
        {
            Interaction.OnFocusObtained -= OnFocusObtainedHandler;
            Interaction.OnFocusLost -= OnFocusLostHandler;
            ScalingManager.i.OnScaleChange -= UpdateScale;
        }

        public float offset = 0.33f;
        public float fac = 1.1f;

        private void UpdateScale()
        {
            transform.localScale =
                new Vector3(transform.localScale.x, ScalingManager.HandleSizeY, transform.localScale.z);
            var x = -ScalingManager.HandleSizeY / fac + offset;
            transform.localPosition = new Vector3(x, transform.localPosition.y,
                transform.localPosition.z);
        }

        private void OnValidate()
        {
            if (ScalingManager.i != null)
                UpdateScale();
        }

        public void RegisterToHandleGroup(HandlerEvent Lissener)
        {
            Lissener.OnHandleEnter += HandlerOnOnHandleEnter;
            Lissener.OnHandleExit += HandlerOnOnHandleExit;
        }

        public void UnregisterToHandleGroup(HandlerEvent Lissener)
        {
            Lissener.OnHandleEnter -= HandlerOnOnHandleEnter;
            Lissener.OnHandleExit -= HandlerOnOnHandleExit;
        }

        private void HandlerOnOnHandleEnter()
        {
            CheckedChangeHandleColor(HandleState.Highlighted);
        }

        private void HandlerOnOnHandleExit()
        {
            CheckedChangeHandleColor(HandleState.Default);
        }


        private void CheckedChangeHandleColor(HandleState handleState = HandleState.Default)
        {
            if (ColorCanChange)
                ChangeHandleColor(handleState);
        }

        private void ChangeHandleColor(HandleState handleState = HandleState.Default)
        {
            var materialReference = ThemeManager.Palette.interactionPalette;
            HandlerRender.material.color = handleState switch
            {
                HandleState.Highlighted => materialReference.HighlightedHandleCol,
                HandleState.Focused => materialReference.FocusedHandleCol,
                _ => materialReference.handleCol
            };
        }
    }
}