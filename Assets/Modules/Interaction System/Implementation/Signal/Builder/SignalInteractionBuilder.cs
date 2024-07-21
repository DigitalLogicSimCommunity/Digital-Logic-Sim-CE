using System;
using System.Collections.Generic;
using Interaction.Signal;
using Interaction.Signal.Display;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Interaction.Signal
{
    public class SignalInteractionBuilder
    {
        SignalInteraction SignalInteractablePref;
        Transform SignalHolder;
        List<int> UsedGroupIDs = new List<int>();
        Dictionary<int, int> RedirectedIDs = new Dictionary<int, int>();
        event Action<Chip> OnDeleteChip;
        float BoundsBottom;
        float BoundsTop;
        private float xContainer;
        private float zContainer;


        EditorInterfaceType editorInterfaceType;

        public SignalInteractionBuilder(SignalInteraction signalInteractablePref, Transform signalHolder,
            Action<Chip> onDeleteChip, float boundsBottom, float boundsTop, float _xContainer, float _zContainer,
            EditorInterfaceType _editorInterfaceType)
        {
            SignalInteractablePref = signalInteractablePref;
            SignalHolder = signalHolder;
            OnDeleteChip = onDeleteChip;

            BoundsBottom = boundsBottom;
            BoundsTop = boundsTop;
            editorInterfaceType = _editorInterfaceType;
            xContainer = _xContainer;
            zContainer = _zContainer;
        }


        public (int id, SignalInteraction obj) Build(float yPos, int desiredGroupSize,
            Pin.WireType wireType = Pin.WireType.Simple, int id = -1
            , bool requiredFocus = true, bool displayEnabled = true)
        {
            int GroupID = SelectNewID(id);


            var ContaierPosition = new Vector3(xContainer, yPos, zContainer);
            var SignalInteractable = GameObject.Instantiate(SignalInteractablePref, SignalHolder);
            SignalInteractable.transform.SetPositionAndRotation(ContaierPosition,
                SignalInteractable.transform.rotation);
            SignalInteractable.Init(wireType, GroupID, BoundsBottom, BoundsTop, editorInterfaceType, ContaierPosition,
                displayEnabled);
            SignalInteractable.SetUpCreation(OnDeleteChip, desiredGroupSize, requiredFocus);


            UsedGroupIDs.Add(GroupID);
            return (GroupID, SignalInteractable);
        }

        private int SelectNewID(int Manualid)
        {
            if (Manualid < 0) return Random.Range(Int32.MinValue, Int32.MaxValue);

            if (!UsedGroupIDs.Contains(Manualid))
                return Manualid;
            if (RedirectedIDs.TryGetValue(Manualid, out var redirectedId))
                return redirectedId;

            int newRedirectedId;
            do newRedirectedId = Random.Range(1, Int32.MaxValue);
            while (UsedGroupIDs.Contains(newRedirectedId));

            RedirectedIDs.Add(Manualid, newRedirectedId);
            return newRedirectedId;
        }
    }
}