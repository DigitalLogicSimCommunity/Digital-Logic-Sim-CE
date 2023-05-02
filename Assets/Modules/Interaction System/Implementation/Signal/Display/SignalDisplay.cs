using System;
using DLS.Simulation;
using UI.ThemeSystem;
using UnityEngine;
using UnityEngine.Serialization;
using static Pin;

namespace Interaction.Display
{
    [RequireComponent(typeof(ChipSignal))]
    public class SignalDisplay : MonoBehaviour, IThemeSettable
    {
        public Palette signalPalette;

        public Palette.VoltageColour CurrentTheme;

        //Signals
        public MeshRenderer indicatorRenderer;
        public Transform indicator;
        [FormerlySerializedAs("pin")] public Transform PinDisplay;
        public Transform Connection;


        private bool Interactable;

        private void Awake()
        {
            signalPalette = ThemeManager.Palette;
            CurrentTheme = signalPalette.GetDefaultTheme();

            var e = GetComponent<ChipSignal>();
            e.OnStateChange += (wireType, state) => DrawSignals(state, wireType);

            ScalingManager.i.OnScaleChange += UpdateScale;
        }

        private void Start()
        {
            UpdateScale();
        }

        private void OnDestroy()
        {
            ScalingManager.i.OnScaleChange -= UpdateScale;
        }

        [Header("Scaling")] public float IndicatoMultiplayer = 2.8f;
        public float Pinfactor = 1.5f;
        public float PinOffset = 0;
        public float Connectfactor = 1f;
        public float ConnectOffset = 0f;

        private void UpdateScale()
        {
            var pinSize = ScalingManager.PinSize;

            Connection.localScale = new Vector3(pinSize, ScalingManager.WireThickness / 10, 1);
            indicator.localScale = new Vector3(pinSize * IndicatoMultiplayer, pinSize * IndicatoMultiplayer, 1);

            PinDisplay.localPosition = new Vector3(pinSize * Pinfactor + PinOffset, PinDisplay.localPosition.y,
                PinDisplay.localPosition.z);
            Connection.localPosition = new Vector3(pinSize * Connectfactor + ConnectOffset, Connection.localPosition.y,
                Connection.localPosition.z);
        }


        private void OnValidate()
        {
            if (ScalingManager.i != null)
                UpdateScale();
        }


        private WireType WireType;
        private PinStates SavedState;

        private PinStates State
        {
            get => SavedState ??= PinStates.AllLow(WireType);
            set => SavedState = value;
        }

        private void DrawSignals(PinStates state, WireType wireType = WireType.Simple)
        {
            WireType = wireType;
            State = state;
            if (!indicatorRenderer) return;


            indicatorRenderer.material.color = CurrentTheme.GetColour(State, wireType);
        }

        public void SetTheme(Palette.VoltageColour voltageColour)
        {
            CurrentTheme = voltageColour;
            DrawSignals(State, WireType);
        }
    }
}