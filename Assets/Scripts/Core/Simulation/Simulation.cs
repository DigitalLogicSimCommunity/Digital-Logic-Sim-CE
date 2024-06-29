using System;
using System.Collections.Generic;
using UnityEngine;


namespace DLS.Core.Simulation
{
    public class Simulation : MonoBehaviour
    {
        public static bool IsSimulationActive => instance.active;

        public event Action<bool> OnSimulationToggle;
        public static Simulation instance;

        public static int simulationFrame { get; private set; }

        InputSignal[] inputSignals;
        ChipEditor chipEditor => Manager.ActiveEditor;

        private bool active = false;

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
            foreach (var outsignal in chipEditor.OutputSignals)
                outsignal.ClearStates();
        }

        private void ProcessInputs()
        {
            foreach (var inputSignal in chipEditor.InputSignals)
                inputSignal.SendSignal();
        }

        void StopSimulation()
        {
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
            {
                chip.InitSimulationFrame();

                if (chip is BuiltinChip { AnyInput: false } buildInChip)
                {
                    buildInChip.ProcessOutput();
                }
                else if (chip is CustomChip customChip && customChip.AnyUnconnectedPin)
                {
                    customChip.ProcessOutput();
                }
            }
        }
    }
}