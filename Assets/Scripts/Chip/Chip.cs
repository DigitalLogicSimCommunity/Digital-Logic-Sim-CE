using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DLS.Core.Simulation;

public class Chip : MonoBehaviour
{


    public string chipName = "Untitled";
    public List<Pin> inputPins;
    public List<Pin> outputPins;
    public bool Editable = false;
    // Number of input signals received (on current simulation step)
    int numInputSignalsReceived;
    int lastSimulatedFrame;
    int lastSimulationInitFrame;

    // Cached components
    [HideInInspector]
    public BoxCollider2D bounds;

    protected virtual void Awake() { bounds = GetComponent<BoxCollider2D>(); }

    protected virtual void Start() { SetPinIndices(); }

    public void InitSimulationFrame()
    {
        if (lastSimulationInitFrame == Simulation.simulationFrame) return;
        
        lastSimulationInitFrame = Simulation.simulationFrame;
        ProcessCycleAndUnconnectedInputs();
    }

    // Receive input signal from pin: either pin has power, or pin does not have
    // power. Once signals from all input pins have been received, calls the
    // ProcessOutput() function.
    public virtual void ReceiveInputSignal(Pin pin)
    {
        // Reset if on new step of simulation
        if (lastSimulatedFrame != Simulation.simulationFrame)
        {
            lastSimulatedFrame = Simulation.simulationFrame;
            numInputSignalsReceived = 0;
            InitSimulationFrame();
        }

        numInputSignalsReceived++;

        if (numInputSignalsReceived == inputPins.Count)
        {
            ProcessOutput();
        }
    }

    void ProcessCycleAndUnconnectedInputs()
    {
        foreach (var pin in inputPins)
        {
            if (pin.cyclic)
            {
                ReceiveInputSignal(pin);
            }
            else if (!pin.HasParent)
            {
                pin.ReceiveZero();
                // ReceiveInputSignal (inputPins[i]);
            }
        }
    }

    // Called once all inputs to the component are known.
    // Sends appropriate output signals t o output pins
    protected virtual void ProcessOutput() { }

    void SetPinIndices()
    {
        try
        {
            for (int i = 0; i < inputPins.Count; i++)
            {
                inputPins[i].index = i;
            }

            for (int i = 0; i < outputPins.Count; i++)
            {
                outputPins[i].index = i;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(chipName);
            throw;
        }
    }

    public Vector2 BoundsSize => bounds.size;
}
