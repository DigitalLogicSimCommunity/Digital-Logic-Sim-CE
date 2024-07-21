using System;
using System.Collections.Generic;
using System.Linq;
using Interaction.Signal.Display;
using UnityEngine;

namespace Interaction.Signal
{
    public class SignalReferenceHolder
    {
        public ChipSignal ChipSignal;
        public HandlerEvent HandlerEvent;
        public SignalHandlerDisplay HandleDisplay;

        public SignalReferenceHolder(ChipSignal signal)
        {
            ChipSignal = signal;
            HandlerEvent = ChipSignal.GetComponentInChildren<HandlerEvent>(true);
            HandleDisplay = ChipSignal.GetComponentInChildren<SignalHandlerDisplay>(true);
            
            HandleDisplay.RegisterToHandleGroup(HandlerEvent);
        }
    }


    public class SignalReferenceHolderList : List<SignalReferenceHolder>
    {

        public readonly List<ChipSignal> ChipSignals;
        private Action<Chip> OnDeleteSignal;

        public SignalReferenceHolderList(Action<Chip> onDeleteSignal) : base()
        {
            ChipSignals = new List<ChipSignal>();
            OnDeleteSignal = onDeleteSignal;
        }


        public SignalReferenceHolder AddSignals(ChipSignal spawnedSignal)
        {
            var SRH = new SignalReferenceHolder(spawnedSignal);

            foreach (var display in this.Select(x => x.HandleDisplay))
            {
                display.RegisterToHandleGroup(SRH.HandlerEvent);
            }

            foreach (var handler in this.Select(x => x.HandlerEvent))
            {
                SRH.HandleDisplay.RegisterToHandleGroup(handler);
            }
            
            AddSignalReference(SRH);

            return SRH;
        }

        public void RemoveSignals()
        {
            var index = Count - 1;
            var signalReferenceHolder = this[index];
            foreach (var display in this.Select(x => x.HandleDisplay))
            {
                display.UnregisterToHandleGroup(signalReferenceHolder.HandlerEvent);
            }

            foreach (var handler in this.Select(x => x.HandlerEvent))
            {
                signalReferenceHolder.HandleDisplay.UnregisterToHandleGroup(handler);
            }

            RemoveAtIndex(index);
        }


        private void AddSignalReference(SignalReferenceHolder signalReferenceHolder)
        {
            Add(signalReferenceHolder);
            ChipSignals.Add(signalReferenceHolder.ChipSignal);
        }

        private void RemoveAtIndex(int index)
        {
            var signalReferenceHolder = this[index];
            ChipSignals.Remove(signalReferenceHolder.ChipSignal);
            OnDeleteSignal?.Invoke(signalReferenceHolder.ChipSignal);
            GameObject.Destroy(signalReferenceHolder.ChipSignal.gameObject);
            RemoveAt(index);
        }

        public void ClearSignal()
        {
            foreach (var signal in ChipSignals)
            {
                OnDeleteSignal?.Invoke(signal);
                GameObject.Destroy(signal.gameObject);
            }
            OnDeleteSignal = null;
            Clear();
        }
    }
}