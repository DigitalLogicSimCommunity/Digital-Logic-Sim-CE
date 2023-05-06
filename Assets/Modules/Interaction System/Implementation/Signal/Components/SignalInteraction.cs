using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using JetBrains.Annotations;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;
using VitoBarra.System.Interaction;
using VitoBarra.Utils.TextVerifier;

namespace Interaction.Signal
{
    public class SignalInteraction : Interactable
    {
        public const int MaxGroupSize = 16;

        public event Action OnGroupSizeChange;
        public event Action OnPropertyChange;

        //Editor 
        [SerializeField] private ChipSignal signalPrefab;

        private DecimalDisplay DecimalDisplay;


        //Work Variable
        public SignalReferenceHolderList Signals { get; private set; }

        private int _groupSize;

        public int GroupSize
        {
            get => _groupSize;
            private set
            {
                _groupSize = value switch
                {
                    < 0 => 0,
                    <= MaxGroupSize => value,
                    _ => MaxGroupSize
                };
            }
        }

        private int ID;


        float BoundsTop;
        float BoundsBottom;
        private Vector3 PinContainers;

        public EditorInterfaceType EditorInterfaceType { get; private set; }


        //Event
        public event Action<Chip> OnDeleteChip;
        public event Action<Vector3, EditorInterfaceType> OnDragig;
        [CanBeNull] public event Action OnDeleteInteraction;


        //Property 
        public bool IsGroup => GroupSize > 1;
        public string SignalName => Signals.ChipSignals[0].signalName;
        public bool UseTwosComplement = true;
        public Pin.WireType WireType;
        public bool DisplayEnabled;

        public Vector3 GroupCenter => (Signals.ChipSignals[0].transform.position +
                                       Signals.ChipSignals[^1].transform.position) / 2;


        private void Awake()
        {
            DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);
        }

        private void Start()
        {
            ScalingManager.i.OnScaleChange += UpdateCenterPosition;
        }

        private void OnDestroy()
        {
            ScalingManager.i.OnScaleChange -= UpdateCenterPosition;
        }

        public void init(Pin.WireType wireType, float _boundsBottom, float _boundsTop,
            EditorInterfaceType _editorInterfaceType, Vector3 _pinContainers, bool displayEnabled = true)
        {
            WireType = wireType;
            EditorInterfaceType = _editorInterfaceType;
            BoundsBottom = _boundsBottom;
            BoundsTop = _boundsTop;
            PinContainers = _pinContainers;
            DisplayEnabled = displayEnabled;
        }


        public void SetUpCreation(Action<Chip> _onDeleteChip, int _groupSize, bool RequireFocus = true)
        {
            if (!DecimalDisplay)
                DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);

            GroupSize = WireType != Pin.WireType.Simple ? 1 : _groupSize;


            Signals = new SignalReferenceHolderList(GroupSize);
            for (var i = 0; i < GroupSize; i++)
                SpawnSignal(WireType, DisplayEnabled);

            if (IsGroup && DisplayEnabled)
                DecimalDisplay.gameObject.SetActive(true);


            UpdateCenterPosition();
            SetPinInteractable(true);

            OnDeleteChip += _onDeleteChip;

            MenuManager.instance.signalPropertiesMenu.RegisterSignalGroup(this);
            OnFocusLost += MenuManager.instance.CloseMenu;
            OnFocusObtained += OpenPropertyMenu;

            if (RequireFocus)
                RequestFocus();
        }

        private SignalReferenceHolder SpawnSignal(Pin.WireType wireType, bool DisplayEnabled)
        {
            var spawnedSignal = Instantiate(signalPrefab, PinContainers, Quaternion.identity, transform);

            var e = Signals.AddSignals(spawnedSignal);
            e.ChipSignal.wireType = wireType;
            RegisterHandler(e.HandleEvent);

            if (!IsGroup || !DisplayEnabled) return e;

            spawnedSignal.OnStateChange += (_, _) =>
                DecimalDisplay.UpdateDecimalDisplay(Signals.ChipSignals, UseTwosComplement);

            return e;
        }

        private SignalReferenceHolder AddSignal()
        {
            GroupSize++;
            return SpawnSignal(WireType, DisplayEnabled);
        }

        private void RemoveSignal()
        {
            Signals.RemoveSignals();
            GroupSize--;
        }


        private void RegisterHandler(HandleEvent HandleEvent)
        {
            HandleEvent.OnHandleClick += () =>
            {
                NotifyMovement();
                RequestFocus();
            };

            HandleEvent.OnStartDrag += (pos) =>
            {
                DragStartY = pos.y;
                centerDragStartDistance = DragStartY - GroupCenter.y;
                DragCancelled = false;
            };

            HandleEvent.OnDrag += Drag;
            HandleEvent.OnStopDrag += () => DragStartY = 0;
        }


        private void NotifyMovement()
        {
            OnDragig?.Invoke(GroupCenter, EditorInterfaceType);
            Manager.ActiveChipEditor.chipInteraction.NotifyMovement();
        }


        #region Positioning

        private float DragStartY;
        private float centerDragStartDistance;
        private bool DragCancelled = false;


        private void Drag()
        {
            if (DragCancelled) return;

            Vector2 mousePos = InputHelper.MouseWorldPos;
            float handleNewY = mousePos.y - centerDragStartDistance;
            DragCancelled = Input.GetKeyDown(KeyCode.Escape);

            if (DragCancelled) handleNewY = DragStartY - centerDragStartDistance;

            MoveCenterYPosition(handleNewY);
            NotifyMovement();
            // Cancel drag and deselect
            if (DragCancelled) ReleaseFocus();
        }


        private void UpdateCenterPosition()
        {
            MoveCenterYPosition(GroupCenter.y);
        }

        private float GetYForGroupMember(float DesideredCeterY, int index)
        {
            var handleSizeY = ScalingManager.HandleSizeY;
            var GroupSpacing = ScalingManager.GroupSpacing;


            float halfExtent = GroupSpacing * (GroupSize - 1f);
            float maxY = DesideredCeterY + halfExtent + handleSizeY / 2f;
            float minY = DesideredCeterY - halfExtent - handleSizeY / 2f;

            if (maxY > BoundsTop)
                DesideredCeterY -= (maxY - BoundsTop);
            else if (minY < BoundsBottom)
                DesideredCeterY += (BoundsBottom - minY);

            float t = (GroupSize > 1) ? index / (GroupSize - 1f) : 0.5f;
            t = t * 2 - 1;
            float posY = DesideredCeterY - t * halfExtent;
            return posY;
        }


        float ClampYBetweenBorder(float y)
        {
            var HandleSizeY = ScalingManager.HandleSizeY;
            return Mathf.Clamp(y, BoundsBottom + HandleSizeY / 2f,
                BoundsTop - HandleSizeY / 2f);
        }

        public void MoveCenterYPosition(float NewYcenter)
        {
            for (var i = 0; i < Signals.Count; i++)
                Signals[i].ChipSignal.transform.SetYPos(GetYForGroupMember(NewYcenter, i));

            if (DecimalDisplay)
                DecimalDisplay.transform.SetYPos(GroupCenter.y);
        }

        #endregion


        public void SetPinInteractable(bool togle = true)
        {
            foreach (var sig in Signals.ChipSignals)
                sig.SetInteractable(togle);
        }


        #region Property

        public void SetWireType(Pin.WireType newWireType)
        {
            WireType = newWireType;
            // Change output pin wire mode
            foreach (var sig in Signals.ChipSignals)
                sig.wireType = newWireType;

            foreach (var pin in Signals.ChipSignals.SelectMany(x => x.inputPins))
            {
                pin.wireType = newWireType;
                Manager.PinAndWireInteraction.DestroyConnectedWires(pin);
            }

            // Change input pin wire mode
            if (Signals.ChipSignals[0] is not InputSignal) return;

            foreach (InputSignal signal in Signals.ChipSignals)
            {
                var pin = signal.outputPins[0];
                if (pin == null) return;
                pin.wireType = newWireType;
                Manager.PinAndWireInteraction.DestroyConnectedWires(pin);
                signal.SetState(PinStates.AllLow(newWireType));
            }
        }

        void OpenPropertyMenu()
        {
            MenuManager.instance.signalPropertiesMenu.SetUpUI(this);
        }


        public void UpdateGroupProperty(string NewName, bool twosComplementToggle)
        {
            // Update signal properties
            foreach (var Signal in Signals.ChipSignals)
            {
                Signal.UpdateSignalName(NewName);
                UseTwosComplement = twosComplementToggle;
            }

            if (IsGroup)
                DecimalDisplay.UpdateDecimalDisplay(Signals.ChipSignals, UseTwosComplement);
            
            OnPropertyChange?.Invoke();
        }

        public void SetBusValue(int state)
        {
            if (IsGroup) return;
            if (Signals.ChipSignals[0] is InputSignal inputSignal)
                inputSignal.SetBusStatus(state < 0 ? 0 : (uint)state);
        }
        
        public List<SignalReferenceHolder> SetGroupSize(int desiredGroupSize)
        {
            var list = new List<SignalReferenceHolder>();
            var e = desiredGroupSize - GroupSize;
            switch (e)
            {
                case < 0:
                    for (var i = 0; i < -e; i++)
                        RemoveSignal();
                    break;
                case > 0:
                    for (var i = 0; i < e; i++)
                        list.Add(AddSignal());

                    break;
            }

            MoveCenterYPosition(GroupCenter.y);
            OnGroupSizeChange?.Invoke();

            return list;
        }

        #endregion


        public bool Contains(ChipSignal chip)
        {
            return Signals.ChipSignals.Contains(chip);
        }

        #region Interaction

        public override void OrderedUpdate()
        {
        }

        private bool DeleteAllowed = true;

        public override void DeleteCommand()
        {
            if (!DeleteAllowed) return;
            foreach (var selectedSignal in Signals.ChipSignals)
            {
                OnDeleteChip?.Invoke(selectedSignal);

                Destroy(selectedSignal.gameObject);
            }

            OnDeleteInteraction?.Invoke();
            Destroy(gameObject);
        }

        public void SilenceDeleteCommand()
        {
            DeleteAllowed = false;
        }

        public void EnableDeleteCommand()
        {
            DeleteAllowed = true;
        }

        #endregion

        private void OnDrawGizmos()
        {
            var dragStart = new Vector2(transform.position.x, DragStartY);
            if (DragStartY != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(dragStart, 0.05f);
            }

            Gizmos.color = Color.magenta;
            var center = new Vector2(transform.position.x, GroupCenter.y);

            Gizmos.DrawSphere(center, 0.05f);
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(center, dragStart);
        }

       
    }
}