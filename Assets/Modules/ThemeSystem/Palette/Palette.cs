using System;
using System.Collections.ObjectModel;
using System.Linq;
using DLS.Core.Simulation;
using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette")]
public class Palette : ScriptableObject
{
    public PinInteractionPalette PinInteractionPalette;


    public Color nonInteractableCol;
    [SerializeField] int defaultIndex;


    [SerializeField] public VoltageColour[] voltageColours;
    public ReadOnlyCollection<VoltageColour> Colours => new(voltageColours);
    public VoltageColour GetDefaultTheme() => voltageColours[defaultIndex];

    public int DefaultIndex
    {
        get => defaultIndex;
        set => defaultIndex = value < 0 ? voltageColours.Length - 1 : value % voltageColours.Length;
    }


    public VoltageColour GetTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return GetDefaultTheme();

        return voltageColours.FirstOrDefault(x => x.Name.Equals(themeName)) ?? GetDefaultTheme();
    }


    [System.Serializable]
    public class VoltageColour
    {
        [FormerlySerializedAs("name")] public string Name;
        public Color High;
        public Color HighBus;
        public Color Low;
        public Color Tristate;
        public float DisplayPriority;


        public Color GetColour(PinStates states, Pin.WireType wireType = Pin.WireType.Simple,
            bool useTriStateCol = true)
        {
            if (states.Count == 1)
                return states[0] switch
                {
                    PinState.HIGH => GetHigh(wireType),
                    PinState.LOW => Low,
                    _ => useTriStateCol ? Tristate : Low
                };

            var e = states.Where(x => x == PinState.HIGH).ToList();
            return e.Count > 0 ? GetHigh(wireType) : Low;
        }


        public Color GetHigh(Pin.WireType startPinWireType)
        {
            return startPinWireType != Pin.WireType.Simple ? HighBus : High;
        }
    }
}