using System.Collections.Generic;
using System.Linq;
using Interaction.Signal.Display;
using UnityEngine;

namespace Interaction.Signal
{
    public class SignalReferenceHolder
    {
        public ChipSignal ChipSignal;
        public HandleEvent HandleEvent;
        public SignalHandlerDisplay HandleDisplay;

        public SignalReferenceHolder(ChipSignal signal)
        {
            ChipSignal = signal;
            HandleEvent = ChipSignal.GetComponentInChildren<HandleEvent>(true);
            HandleDisplay = ChipSignal.GetComponentInChildren<SignalHandlerDisplay>(true);
            
            HandleDisplay.RegisterToHandleGroup(HandleEvent);
        }
    }


    public class SignalReferenceHolderList : List<SignalReferenceHolder>
    {
        private bool ValidCache = false;

        public List<ChipSignal> ChipSignals;

        public SignalReferenceHolderList(int groupSize) : base(groupSize)
        {
            ChipSignals = new List<ChipSignal>(groupSize);
        }


        public SignalReferenceHolder AddSignals(ChipSignal spawnedSignal)
        {
            var SRH = new SignalReferenceHolder(spawnedSignal);

            foreach (var display in this.Select(x => x.HandleDisplay))
            {
                display.RegisterToHandleGroup(SRH.HandleEvent);
            }

            foreach (var handler in this.Select(x => x.HandleEvent))
            {
                SRH.HandleDisplay.RegisterToHandleGroup(handler);
            }
            
            AddH(SRH);

            return SRH;
        }

        public void RemoveSignals()
        {
            var index = Count - 1;
            var e = this[index];
            foreach (var display in this.Select(x => x.HandleDisplay))
            {
                display.UnregisterToHandleGroup(e.HandleEvent);
            }

            foreach (var handler in this.Select(x => x.HandleEvent))
            {
                e.HandleDisplay.UnregisterToHandleGroup(handler);
            }

            RemoveAtH(index);
        }


        private void AddH(SignalReferenceHolder h)
        {
            Add(h);
            ChipSignals.Add(h.ChipSignal);
        }

        private void RemoveAtH(int index)
        {
            var e = this[index];
            ChipSignals.Remove(e.ChipSignal);
            GameObject.Destroy(e.ChipSignal.gameObject);
            RemoveAt(index);
        }
    }
}