using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Core.Simulation;
using JetBrains.Annotations;
using UnityEngine;
using VitoBarra.System.Interaction;
using VitoBarra.Utils.TextVerifier;

namespace Interaction.Signal
{
    public class SignalInteraction : Interactable
    {
        public const int MaxGroupSize = 16;
        private int GroupID = -1;

        public event Action OnGroupSizeChange;
        public event Action OnPropertyChange;

        //Editor 
        [SerializeField] private ChipSignal signalPrefab;

        private DecimalDisplay DecimalDisplay;


        //Work Variable
        public SignalReferenceHolderList Signals { get; private set; }

        private int _groupSize =0;

        public int GroupSize
        {
            get => _groupSize;
            private set => _groupSize = value < 1 ? 1 : value % MaxGroupSize;
        }


        float BoundsTop;
        float BoundsBottom;
        private Vector3 PinContainers;

        public EditorInterfaceType EditorInterfaceType { get; private set; }


        public event Action<Vector3, EditorInterfaceType> OnDragging;
        [CanBeNull] public event Action OnDeleteInteraction;


        //Property 
        public bool IsGroup => GroupSize > 1;
        public string SignalName => Signals.ChipSignals[0].signalName;
        public bool UseTwosComplement = true;
        public Pin.WireType WireType;
        public bool DisplayEnabled;

        public Vector3 GroupCenter => CalculateCenter(Signals.ChipSignals[0].transform.position,
            Signals.ChipSignals[^1].transform.position);

        public static Vector2 CalculateCenter(Vector2 a, Vector2 b) => (a + b) / 2;

        private void Awake()
        {
            DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);
        }

        private void Start()
        {
            ScalingManager.i.OnScaleChange += UpdateScale;
        }

        private void OnDestroy()
        {
            ScalingManager.i.OnScaleChange -= UpdateScale;
        }

        public void Init(Pin.WireType wireType, int _groupID, float _boundsBottom, float _boundsTop,
            EditorInterfaceType _editorInterfaceType, Vector3 _pinContainers, bool displayEnabled = true)
        {
            WireType = wireType;
            EditorInterfaceType = _editorInterfaceType;
            BoundsBottom = _boundsBottom;
            BoundsTop = _boundsTop;
            PinContainers = _pinContainers;
            DisplayEnabled = displayEnabled;
            GroupID = _groupID;
        }


        public void SetUpCreation(Action<Chip> _onDeleteChip, int _groupSize, bool RequireFocus = true)
        {
            if (!DecimalDisplay)
                DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);
            Signals = new SignalReferenceHolderList(_onDeleteChip);


            MenuManager.instance.signalPropertiesMenu.RegisterSignalGroup(this);
            OnGroupSizeChange += OnGroupSizeChangeHandle;

            SetGroupSize(WireType != Pin.WireType.Simple ? 1 : _groupSize);
            SetPinInteractable(true);

            if (RequireFocus)
                RequestFocus();
        }

        private void OnGroupSizeChangeHandle()
        {
            SetGroupCenter(GroupCenter.y);
            DecimalDisplay.gameObject.SetActive(IsGroup && DisplayEnabled);
            StateChangeHandle();
        }

        public void StateChangeHandle()
        {
            if (IsGroup && DisplayEnabled)
                DecimalDisplay.UpdateDecimalDisplay(Signals.ChipSignals, UseTwosComplement);
        }

        private SignalReferenceHolder SpawnSignal(Pin.WireType wireType, bool DisplayEnabled)
        {
            var spawnedSignal = Instantiate(signalPrefab, PinContainers, Quaternion.identity, transform);

            spawnedSignal.GroupId = GroupID;

            var signalReferenceHolder = Signals.AddSignals(spawnedSignal);
            signalReferenceHolder.ChipSignal.wireType = wireType;
            RegisterHandler(signalReferenceHolder.HandlerEvent);

            spawnedSignal.OnStateChange += (_, _) => StateChangeHandle();
            return signalReferenceHolder;
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


        private void RegisterHandler(HandlerEvent handlerEvent)
        {
            handlerEvent.OnHandleLeftDown += () =>
            {
                NotifyMovement();
                RequestFocus();
            };

            handlerEvent.OnHandleRightClick += ()=> MenuManager.instance.signalPropertiesMenu.SetUpSignalPropertyUI(this);;

            handlerEvent.OnStartDrag += (pos) =>
            {
                DragStartY = pos.y;
                centerDragStartDistance = DragStartY - GroupCenter.y;
                DragCancelled = false;
            };

            handlerEvent.OnDrag += Drag;
            handlerEvent.OnStopDrag += () => DragStartY = 0;
        }


        private void NotifyMovement()
        {
            OnDragging?.Invoke(GroupCenter, EditorInterfaceType);
            Manager.ActiveEditor?.chipInteraction.NotifyMovement();
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

            SetGroupCenter(handleNewY);
            NotifyMovement();
            // Cancel drag and deselect
            if (DragCancelled) ReleaseFocus();
        }


        private void UpdateScale()
        {
            SetGroupCenter(GroupCenter.y);
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

        public void SetGroupCenter(float NewYcenter)
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
                if (pin is null) return;
                pin.wireType = newWireType;
                Manager.PinAndWireInteraction.DestroyConnectedWires(pin);
                signal.SetState(PinStates.AllLow(newWireType));
            }
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
            var grupSizeDif = desiredGroupSize - GroupSize;

            if (grupSizeDif == 0) return list;
            for (var i = 0; i < Math.Abs(grupSizeDif); i++)
            {
                if (grupSizeDif > 0)
                    list.Add(AddSignal());
                else
                    RemoveSignal();
            }


            SetGroupCenter(GroupCenter.y);
            OnGroupSizeChange?.Invoke();

            NotifyMovement();

            return list;
        }


        public SignalReferenceHolder AddOneSignal()
        {
            return SetGroupSize(GroupSize + 1)[^1];
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
            Signals.ClearSignal();
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