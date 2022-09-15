using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public static Simulation instance;

    public static int simulationFrame { get; private set; }

    InputSignal[] inputSignals;
    ChipEditor chipEditor;
    public bool active = false;

    public float minStepTime = 0.075f;
    float lastStepTime;

    List<CustomChip> standaloneChips = new List<CustomChip>();

    public void ToogleActive()
    {
        // Method called by the "Run/Stop" button that toogles simulation
        // active/inactive
        active = !active;

        simulationFrame++;
        if (active)
            ResumeSimulation();
        else
            StopSimulation();

    }

    void Awake()
    {
        instance = this;
        simulationFrame = 0;
    }

    void Update()
    {
        // If simulation is off StepSimulation is not executed.
        if (Time.time - lastStepTime > minStepTime && active)
        {
            lastStepTime = Time.time;
            simulationFrame++;
            StepSimulation();
        }
    }

    void StepSimulation()
    {
        RefreshChipEditorReference();
        ClearOutputSignals();
        InitChips();
        ProcessInputs();
    }

    public void ResetSimulation()
    {
        StopSimulation();
        simulationFrame = 0;

        if (active)
        {
            FindObjectOfType<RunButton>().SetOff();
            active = false;
        }
    }

    private void ClearOutputSignals()
    {
        List<ChipSignal> outputSignals = chipEditor.outputsEditor.signals;
        for (int i = 0; i < outputSignals.Count; i++)
        {
            outputSignals[i].SetDisplayState(0);
            outputSignals[i].currentState = 0;
        }
    }

    private void ProcessInputs()
    {
        List<ChipSignal> inputSignals = chipEditor.inputsEditor.signals;
        for (int i = 0; i < inputSignals.Count; i++)
        {
            ((InputSignal)inputSignals[i]).SendSignal();
        }
        foreach (Chip chip in chipEditor.chipInteraction.allChips)
        {
            if (chip is CustomChip custom)
            {
                // if (custom.HasNoInputs) {
                // 	custom.ProcessOutputNoInputs();
                // }
                custom.pseudoInput?.ReceiveSignal(0);
                if (custom.pseudoInput != null)
                {
                }
            }
        }
    }

    void StopSimulation()
    {
        RefreshChipEditorReference();

        var allWires = chipEditor.pinAndWireInteraction.allWires;
        // Tell all wires the simulation is inactive makes them all inactive (gray
        // colored)
        foreach (Wire wire in allWires)
            wire.tellWireSimIsOff();
        foreach (Pin pin in chipEditor.pinAndWireInteraction.AllVisiblePins())
            pin.tellPinSimIsOff();

        // If sim is not active all output signals are set with a temporal value of
        // 0 (group signed/unsigned displayed value) and get gray colored (turned
        // off)
        ClearOutputSignals();
    }

    void ResumeSimulation()
    {
        StepSimulation();

        foreach (Pin pin in chipEditor.pinAndWireInteraction.AllVisiblePins())
            pin.tellPinSimIsOn();

        var allWires = chipEditor.pinAndWireInteraction.allWires;

        // Tell all wires the simulation is active makes them all active (dynamic
        // colored based on the circuits logic)
        foreach (Wire wire in allWires)
            wire.tellWireSimIsOn();
    }

    private void InitChips()
    {
        var allChips = chipEditor.chipInteraction.allChips;

        foreach (Chip chip in allChips)
            chip.InitSimulationFrame();
    }

    void RefreshChipEditorReference()
    {
        if (chipEditor == null)
            chipEditor = Manager.ActiveChipEditor;
    }
}
