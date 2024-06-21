using System;
using System.Collections.Generic;
using UnityEngine;


namespace DLS.Core.Simulation
{
    public class Simulation : MonoBehaviour
    {
        public event Action<bool> OnSimulationToggle;
        public static Simulation instance;

        public static int simulationFrame { get; private set; }

        InputSignal[] inputSignals;
        ChipEditor chipEditor;
        public bool active = false;

        public float minStepTime = 0.075f;
        float lastStepTime;

        
        public void ToggleActive()
        {
            // Method called by the "Run/Stop" button that toggles simulation
            // active/inactive
            active = !active;
            OnSimulationToggle?.Invoke(active);

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

        private void Start()
        {
            Manager.instance.OnEditorClear += ResetSimulation;
        }

        void Update()
        {
            // If simulation is off StepSimulation is not executed.
            if (!(Time.time - lastStepTime > minStepTime) || !active) return;

            lastStepTime = Time.time;
            simulationFrame++;
            StepSimulation();
        }

        void StepSimulation()
        {
            RefreshChipEditorReference();
            ClearOutputSignals();
            InitChips();
            ProcessInputs();
        }

        private void ResetSimulation()
        {
            StopSimulation();
            simulationFrame = 0;

            if (!active) return;
            FindObjectOfType<RunButton>().SetOff();
            active = false;
        }

        private void ClearOutputSignals()
        {
            List<ChipSignal> outputSignals = chipEditor.outputsEditor.GetAllSignals();
            foreach (var outsignal in outputSignals)
                outsignal.ClearStates();
        }

        private void ProcessInputs()
        {
            List<ChipSignal> inputSignals = chipEditor.inputsEditor.GetAllSignals();
            foreach (var inputSignal in inputSignals)
            {
                ((InputSignal)inputSignal).SendSignal();
            }
        }

        void StopSimulation()
        {
            RefreshChipEditorReference();
            ClearOutputSignals();
        }

        private void ResumeSimulation()
        {
            StepSimulation();
        }

        private void InitChips()
        {
            var allChips = chipEditor.chipInteraction.allChips;

            foreach (global::Chip chip in allChips)
                chip.InitSimulationFrame();
        }

        private void RefreshChipEditorReference()
        {
            if (chipEditor == null)
                chipEditor = Manager.ActiveChipEditor;
        }
    }
}