using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PinAndWireInteraction : Interactable
{

    public event System.Action onConnectionChanged;
    public event System.Action<Pin> onMouseOverPin;
    public event System.Action<Pin> onMouseExitPin;

    public enum State { None, PlacingWire, PasteWires }
    public LayerMask pinMask;
    public LayerMask wireMask;
    public Transform wireHolder;
    public Wire wirePrefab;

    State _currentState;
    Pin pinUnderMouse;
    Pin wireStartPin;
    Wire wireToPlace;
    Wire highlightedWire;
    Dictionary<Pin, Wire> wiresByChipInputPin;
    public List<Wire> allWires { get; private set; }

    ChipInteraction chipInteraction;
    ChipInterfaceEditor inputEditor;
    ChipInterfaceEditor outputEditor;

    List<Wire> wiresToPaste;
    public State CurrentState => _currentState;

    void Awake()
    {
        allWires = new List<Wire>();
        wiresToPaste = new List<Wire>();
        wiresByChipInputPin = new Dictionary<Pin, Wire>();
    }


    public void Init(ChipInteraction chipInteraction,
                     ChipInterfaceEditor inputEditor,
                     ChipInterfaceEditor outputEditor)
    {
        this.chipInteraction = chipInteraction;
        this.inputEditor = inputEditor;
        this.outputEditor = outputEditor;
        chipInteraction.onDeleteChip += DeleteChipWires;
        inputEditor.OnDeleteChip += DeleteChipWires;
        outputEditor.OnDeleteChip += DeleteChipWires;
    }

    public override void OrderedUpdate()
    {
        bool mouseOverUI = InputHelper.MouseOverUIObject();

        if (!mouseOverUI)
        {
            HandlePinHighlighting();

            switch (_currentState)
            {
                case State.None:
                    HandleWireHighlighting();
                    //HandleWireDeletion();
                    HandleWireCreation();
                    break;
                case State.PlacingWire:
                    HandleWirePlacement();
                    break;
                case State.PasteWires:
                    HandlePasteWires();
                    break;
            }
        }
    }

    public void PasteWires(List<WireInformation> wires, List<Chip> chips)
    {
        wiresToPaste.Clear();
        foreach (WireInformation wire in wires)
        {
            Wire newWire = Instantiate(wirePrefab, parent: wireHolder);
            allWires.Add(newWire);
            newWire.Connect(
                chips[wire.startChipIndex].outputPins[wire.startChipPinIndex],
                chips[wire.endChipIndex].inputPins[wire.endChipPinIndex]);
            newWire.SetAnchorPoints(wire.anchorPoints);
            newWire.endPin.parentPin = newWire.startPin;
            newWire.startPin.childPins.Add(newWire.endPin);
            wiresByChipInputPin.Add(newWire.ChipInputPin, newWire);
            wiresToPaste.Add(newWire);
        }
        _currentState = State.PasteWires;
    }

    public void LoadWire(Wire wire)
    {
        wire.transform.parent = wireHolder;
        allWires.Add(wire);
        wiresByChipInputPin.Add(wire.ChipInputPin, wire);
    }

    public List<Pin> AllVisiblePins()
    {
        List<Pin> pins = new List<Pin>();
        pins.AddRange(chipInteraction.visiblePins);
        pins.AddRange(inputEditor.visiblePins);
        pins.AddRange(outputEditor.visiblePins);
        return pins;
    }

    void HandleWireHighlighting()
    {
        var wireUnderMouse = InputHelper.GetObjectUnderMouse2D(wireMask);
        if (wireUnderMouse && pinUnderMouse == null)
        {
            if (highlightedWire)
                highlightedWire.SetSelectionState(false);
            
            highlightedWire = wireUnderMouse.GetComponent<Wire>();
            highlightedWire.SetSelectionState(true);

        }
        else if (highlightedWire)
        {
            highlightedWire.SetSelectionState(false);
            highlightedWire = null;
        }
    }



    void HandleWirePlacement()
    {
        // Cancel placing wire
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace,
                                           KeyCode.Delete) || Input.GetMouseButtonDown(1))
        {
            StopPlacingWire();
        }
        // Update wire position and check if user wants to try connect the wire
        else
        {
            Vector2 mousePos = InputHelper.MouseWorldPos;

            wireToPlace.UpdateWireEndPoint(mousePos);

            // Left mouse press
            if (Input.GetMouseButtonDown(0))
            {
                // If mouse pressed over pin, try connecting the wire to that pin
                if (pinUnderMouse)
                {
                    TryPlaceWire(wireStartPin, pinUnderMouse);
                }
                // If mouse pressed over empty space, add anchor point to wire
                else
                {
                    wireToPlace.AddAnchorPoint(mousePos);
                }
            }
            // Left mouse release
            else if (Input.GetMouseButtonUp(0))
            {
                if (pinUnderMouse && pinUnderMouse != wireStartPin)
                {
                    TryPlaceWire(wireStartPin, pinUnderMouse);
                }
            }
        }
    }

    void HandlePasteWires()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace,
                                           KeyCode.Delete) || Input.GetMouseButtonDown(1))
        {
            foreach (Wire wire in wiresToPaste)
                DestroyWire(wire);

            wiresToPaste.Clear();
            _currentState = State.None;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            wiresToPaste.Clear();
            _currentState = State.None;
            Invoke(nameof(ConnectionChanged), 0.01f);
        }
    }

    public Wire GetWire(Pin childPin) => wiresByChipInputPin.ContainsKey(childPin) ? wiresByChipInputPin[childPin] : null;


    void TryPlaceWire(Pin startPin, Pin endPin)
    {

        if (Pin.IsValidConnection(startPin, endPin))
        {
            Pin chipInputPin =
                (startPin.pinType == Pin.PinType.ChipInput) ? startPin : endPin;
            RemoveConflictingWire(chipInputPin);

            wireToPlace.Place(endPin);
            Pin.MakeConnection(startPin, endPin);
            allWires.Add(wireToPlace);
            wiresByChipInputPin.Add(chipInputPin, wireToPlace);
            wireToPlace = null;
            _currentState = State.None;

            onConnectionChanged?.Invoke();
        }
        else
        {
            StopPlacingWire();
        }
    }

    // Pin cannot have multiple inputs, so when placing a new wire, first remove
    // the wire that already goes to that pin (if there is one)
    void RemoveConflictingWire(Pin chipInputPin)
    {
        if (wiresByChipInputPin.ContainsKey(chipInputPin))
            DestroyWire(wiresByChipInputPin[chipInputPin]);
    }

    void DestroyWire(Wire wire)
    {
        wiresByChipInputPin.Remove(wire.ChipInputPin);
        allWires.Remove(wire);
        Pin.RemoveConnection(wire.startPin, wire.endPin);
        wire.endPin.ReceiveSignal(0);
        Destroy(wire.gameObject);
    }

    void HandleWireCreation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Wire can be created from a pin, or from another wire (in which case it
            // uses that wire's start pin)
            if ((pinUnderMouse || highlightedWire) && RequestFocus())
            {
                _currentState = State.PlacingWire;
                // wirePrefab.GetComponent<Wire>().thickness =
                // ScalingManager.wireThickness * 1.5f;
                wireToPlace = Instantiate(wirePrefab, parent: wireHolder);

                // Creating new wire starting from pin
                if (pinUnderMouse)
                {
                    wireStartPin = pinUnderMouse;
                    wireToPlace.ConnectToFirstPin(wireStartPin);
                }
                // Creating new wire starting from existing wire
                else if (highlightedWire)
                {
                    wireStartPin = highlightedWire.ChipOutputPin;
                    wireToPlace.ConnectToFirstPinViaWire(wireStartPin, highlightedWire,
                                                         InputHelper.MouseWorldPos);
                }
            }
        }
    }

    void HandlePinHighlighting()
    {
        Vector2 mousePos = InputHelper.MouseWorldPos;
        Collider2D pinCollider = Physics2D.OverlapCircle(
            mousePos, Pin.interactionRadius - Pin.radius, pinMask);
        if (pinCollider)
        {
            Pin newPinUnderMouse = pinCollider.GetComponent<Pin>();
            if (pinUnderMouse != newPinUnderMouse)
            {
                if (pinUnderMouse != null)
                {
                    pinUnderMouse.MouseExit();
                    onMouseExitPin?.Invoke(pinUnderMouse);
                }
                newPinUnderMouse.MouseEnter();
                pinUnderMouse = newPinUnderMouse;
                onMouseOverPin?.Invoke(pinUnderMouse);
            }
        }
        else
        {
            if (pinUnderMouse)
            {
                pinUnderMouse.MouseExit();
                onMouseExitPin?.Invoke(pinUnderMouse);
                pinUnderMouse = null;
            }
        }
    }

    // Delete all wires connected to given chip
    void DeleteChipWires(Chip chip)
    {
        var wiresToDestroy = new List<Wire>();

        foreach (var outputPin in chip.outputPins)
            foreach (var childPin in outputPin.childPins)
                wiresToDestroy.Add(wiresByChipInputPin[childPin]);

        foreach (var inputPin in chip.inputPins)
            if (inputPin.parentPin)
                wiresToDestroy.Add(wiresByChipInputPin[inputPin]);

        foreach (var wire in wiresToDestroy)
            DestroyWire(wire);

        onConnectionChanged?.Invoke();
    }

    void StopPlacingWire()
    {
        if (wireToPlace)
        {
            Destroy(wireToPlace.gameObject);
            wireToPlace = null;
            wireStartPin = null;
        }
        _currentState = State.None;
    }

    public override void FocusLostHandler()
    {
        if (pinUnderMouse)
        {
            pinUnderMouse.MouseExit();
            pinUnderMouse = null;
        }

        if (highlightedWire)
        {
            highlightedWire.SetSelectionState(false);
            highlightedWire = null;
        }

        _currentState = State.None;
    }
    void ConnectionChanged() { onConnectionChanged?.Invoke(); }


    public override bool CanReleaseFocus() => _currentState != State.PlacingWire && !pinUnderMouse;

    public override void DeleteComand()
    {
        if (_currentState == State.None)
            HandleWireDeletion();
    }

    void HandleWireDeletion()
    {
        if (!highlightedWire) return;

        DestroyWire(highlightedWire);
        onConnectionChanged?.Invoke();
    }
}
