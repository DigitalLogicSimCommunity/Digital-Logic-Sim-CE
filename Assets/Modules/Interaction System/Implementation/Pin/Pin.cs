using System;
using System.Collections.Generic;
using DLS.Core.Simulation;
using UnityEngine;

public class Pin : MonoBehaviour
{
    public event Action<PinStates, WireType> OnStateChange;

    public enum WireType
    {
        Simple,
        Bus4,
        Bus8,
        Bus16,
        Bus32
    }

    public enum PinType
    {
        ChipInput,
        ChipOutput
    }

    public PinType pinType;

    public WireType wireType;

    // The chip that this pin is attached to (either as an input or output
    // terminal)
    public Chip chip;
    public string pinName;

    [HideInInspector] public bool cyclic;

    // Index of this pin in its associated chip's input or output pin array
    [HideInInspector] public int index;

    // The pin from which this pin receives its input signal
    // (multiple inputs not allowed in order to simplify simulation)
    [HideInInspector] public Pin parentPin;

    // The pins which this pin forwards its signal to
    [HideInInspector] public List<Pin> childPins = new List<Pin>();

    // Current state of the pin: -1 = HighZ 0 == LOW, 1 == HIGH 
    private PinStates currentState;
    
    // Get the current state of the pin: 0 == LOW, 1 == HIGH
    public PinStates State => currentState ??= PinStates.AllLow(wireType);


    private void Awake()
    {
        chip = GetComponentInParent<Chip>();
    }

    public static int NumBits(WireType type)
    {
        return type switch
        {
            WireType.Bus4 => 4,
            WireType.Bus8 => 8,
            WireType.Bus16 => 16,
            WireType.Bus32 => 32,
            _ => 1
        };
    }

    void Start()
    {
        currentState = PinStates.Zero;
        Simulation.instance.OnSimulationToggle += (_) => NotifyStateChange();
    }


    // Note that for ChipOutput pins, the chip itself is considered the parent, so
    // will always return true Otherwise, only true if the parentPin of this pin
    // has been set
    public bool HasParent => parentPin != null || pinType == PinType.ChipOutput;

    // Receive signal: 0 == LOW, 1 = HIGH
    // Sets the current state to the signal
    // Passes the signal on to any connected pins / electronic component
    public void ReceiveSignal(PinStates State)
    {
        // if(chip == null) return;
        currentState = State;
        switch (pinType)
        {
            case PinType.ChipInput when !cyclic:
                chip.ReceiveInputSignal(this);
                break;
            case PinType.ChipOutput:
            {
                foreach (var t in childPins)
                    t.ReceiveSignal(State);
                break;
            }
        }

        NotifyStateChange();
    }

    public void ReceiveZero()
    {
        ReceiveSignal(PinStates.AllLow(wireType));
    }

    public void ReceiveOne()
    {
        ReceiveSignal(PinStates.AllHigh(wireType));
    }




    public void NotifyStateChange()
    {
        OnStateChange?.Invoke(currentState, wireType);
    }


    public static void MakeConnection(Pin pinA, Pin pinB)
    {
        if (!IsValidConnection(pinA, pinB)) return;

        Pin parentPin = (pinA.pinType == PinType.ChipOutput) ? pinA : pinB;
        Pin childPin = (pinA.pinType == PinType.ChipInput) ? pinA : pinB;

        parentPin.childPins.Add(childPin);
        childPin.parentPin = parentPin;
    }

    public static void RemoveConnection(Pin pinA, Pin pinB)
    {
        Pin parentPin = (pinA.pinType == PinType.ChipOutput) ? pinA : pinB;
        Pin childPin = (pinA.pinType == PinType.ChipInput) ? pinA : pinB;

        parentPin.childPins.Remove(childPin);
        childPin.parentPin = null;
    }

    public static bool IsValidConnection(Pin pinA, Pin pinB)
    {
        // Connection failes when pin wire types are different
        if (pinA.wireType != pinB.wireType)
            return false;
        // Connection is valid if one pin is an output pin, and the other is an
        // input pin
        return pinA.pinType != pinB.pinType;
    }

    public static bool TryConnect(Pin pinA, Pin pinB)
    {
        if (pinA.pinType == pinB.pinType) return false;

        Pin parentPin = (pinA.pinType == PinType.ChipOutput) ? pinA : pinB;
        Pin childPin = (parentPin == pinB) ? pinA : pinB;
        parentPin.childPins.Add(childPin);
        childPin.parentPin = parentPin;
        return true;
    }
}