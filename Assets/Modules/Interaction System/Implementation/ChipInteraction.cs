using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VitoBarra.System.Interaction;

public class ChipInteraction : Interactable
{
    enum State
    {
        None,
        PlacingNewChips,
        MovingOldChips,
        SelectingChips,
        PasteNewChips
    }

    public event System.Action<Chip> onDeleteChip;
    public event System.Action onChipMovement;


    public BoxCollider2D chipArea;
    public Transform chipHolder;
    public LayerMask chipMask;
    public Material selectionBoxMaterial;
    public Color selectionBoxCol;
    public Color invalidPlacementCol;

    const float dragDepth = -50;
    const float chipDepth = -0.2f;

    public List<Chip> allChips { get; private set; }

    State currentState;
    List<Chip> newChipsToPlace;
    List<KeyValuePair<Chip, Vector3>> newChipsToPaste;
    public static List<Chip> selectedChips;
    Vector2 selectionBoxStartPos;
    Mesh selectionMesh;
    Vector3[] selectedChipsOriginalPos;

    [HideInInspector] public List<Pin> visiblePins;

    List<Chip> chipsToPaste;

    float PinRadius => PinDisplay.radius / 4;
    float PinInteraction => PinRadius * PinDisplay.IteractionFactor;

    void Awake()
    {
        newChipsToPlace = new List<Chip>();
        newChipsToPaste = new List<KeyValuePair<Chip, Vector3>>();
        chipsToPaste = new List<Chip>();
        selectedChips = new List<Chip>();
        allChips = new List<Chip>();
        visiblePins = new List<Pin>();
        MeshShapeCreator.CreateQuadMesh(ref selectionMesh);

        OnFocusLost += FocusLostHandler;
    }

    public override void OrderedUpdate()
    {
        switch (currentState)
        {
            case State.None:
                HandleSelection();
                break;
            case State.PlacingNewChips:
                HandleNewChipPlacement();
                break;
            case State.PasteNewChips:
                HandlePasteChipPlacement();
                break;
            case State.SelectingChips:
                HandleSelectionBox();
                break;
            case State.MovingOldChips:
                HandleChipMovement();
                break;
        }

        DrawSelectedChipBounds();
    }

    public Pin[] UnconnectedInputPins => (from chip in allChips
        from pin in chip.inputPins
        where pin.wireType == Pin.WireType.Simple && !pin.HasParent
        select pin).ToArray();

    public Pin[] UnconnectedOutputPins =>
        (from chip in allChips from pin in chip.outputPins where pin.childPins.Count == 0 select pin).ToArray();

    public Chip LoadChip(Chip chipPref, Vector2 pos)
    {
        Chip chip = Instantiate(chipPref, pos, Quaternion.identity);

        chip.transform.parent = chipHolder;
        allChips.Add(chip);
        visiblePins.AddRange(chip.inputPins);
        visiblePins.AddRange(chip.outputPins);
        foreach (Pin pin in chip.outputPins)
        {
            pin.NotifyStateChange();
        }

        return chip;
    }

    public List<Chip> SelectedChips => selectedChips;

    public List<Chip> PasteChips(List<KeyValuePair<Chip, Vector3>> clipboard)
    {
        currentState = State.PasteNewChips;
        if (newChipsToPaste.Count == 0)
            selectedChips.Clear();
        // newChipsToPaste.Clear();
        // chipsToPaste.Clear();

        foreach (KeyValuePair<Chip, Vector3> clipboardItem in clipboard)
        {
            var newChip = Instantiate(clipboardItem.Key, clipboardItem.Value, Quaternion.identity);
            newChip.transform.SetParent(chipHolder);
            newChip.gameObject.SetActive(true);
            selectedChips.Add(newChip);
            newChipsToPaste.Add(
                new KeyValuePair<Chip, Vector3>(newChip, clipboardItem.Value));
            chipsToPaste.Add(newChip);
        }

        return chipsToPaste;
    }

    public void ChipButtonInteraction(Chip chip)
    {
        if (!RequestFocus()) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Spawn chip
            currentState = State.PlacingNewChips;
            if (newChipsToPlace.Count == 0)
            {
                selectedChips.Clear();
            }

            var newChip = Instantiate(chip, chipHolder);
            newChip.gameObject.SetActive(true);
            selectedChips.Add(newChip);
            newChipsToPlace.Add(newChip);
        }
        else if (Input.GetMouseButtonDown(1) && chip.Editable)
        {
            MenuManager.instance.OpenMenu(MenuType.EditChipMenu);
        }
    }

    public bool IsSelecting;

    void HandleSelection()
    {
        Vector2 mousePos = InputHelper.MouseWorldPos;

        // Left mouse down. Handle selecting a chip, or starting to draw a selection
        // box.
        if (!Input.GetMouseButtonDown(0) || InputHelper.MouseOverUIObject() ||
            InputHelper.CompereTagObjectUnderMouse2D(ProjectTags.InterfaceMask, ProjectLayer.Default))
        {
            IsSelecting = false;
            return;
        }

        if (!RequestFocus()) return;

        IsSelecting = true;

        selectionBoxStartPos = mousePos;
        var objectUnderMouse = InputHelper.GetObjectUnderMouse2D(chipMask);

        // If clicked on nothing, clear selected items and start drawing
        // selection box
        if (objectUnderMouse == null)
        {
            currentState = State.SelectingChips;
            selectedChips.Clear();
        }
        // If clicked on a chip, select that chip and allow it to be moved
        else
        {
            currentState = State.MovingOldChips;
            Chip chipUnderMouse = objectUnderMouse.GetComponent<Chip>();
            // If object is already selected, then selection of any other chips
            // should be maintained so they can be moved as a group. But if object
            // is not already selected, then any currently selected chips should
            // be deselected.
            if (!selectedChips.Contains(chipUnderMouse))
            {
                selectedChips.Clear();
                selectedChips.Add(chipUnderMouse);
            }

            // Record starting positions of all selected chips for movement
            selectedChipsOriginalPos = new Vector3[selectedChips.Count];
            for (var i = 0; i < selectedChips.Count; i++)
            {
                selectedChipsOriginalPos[i] = selectedChips[i].transform.position;
            }
        }
    }


    public void DeleteChip(Chip chip)
    {
        onDeleteChip?.Invoke(chip);
        allChips.Remove(chip);

        foreach (Pin pin in chip.inputPins)
            visiblePins.Remove(pin);
        foreach (Pin pin in chip.outputPins)
            visiblePins.Remove(pin);

        Destroy(chip.gameObject);
    }

    void HandleSelectionBox()
    {
        Vector2 mousePos = InputHelper.MouseWorldPos;
        // While holding mouse down, keep drawing selection box
        if (Input.GetMouseButton(0))
        {
            var pos =
                (Vector3)(selectionBoxStartPos + mousePos) / 2 + Vector3.back * 0.5f;
            var scale =
                new Vector3(Mathf.Abs(mousePos.x - selectionBoxStartPos.x),
                    Mathf.Abs(mousePos.y - selectionBoxStartPos.y), 1);
            selectionBoxMaterial.color = selectionBoxCol;
            Graphics.DrawMesh(selectionMesh,
                Matrix4x4.TRS(pos, Quaternion.identity, scale),
                selectionBoxMaterial, 0);
        }

        // Mouse released, so selected all chips inside the selection box
        if (Input.GetMouseButtonUp(0))
        {
            currentState = State.None;

            // Select all objects under selection box
            Vector2 boxSize =
                new Vector2(Mathf.Abs(mousePos.x - selectionBoxStartPos.x),
                    Mathf.Abs(mousePos.y - selectionBoxStartPos.y));
            var allObjectsInBox = Physics2D.OverlapBoxAll(
                (selectionBoxStartPos + mousePos) / 2, boxSize, 0, chipMask);
            selectedChips.Clear();
            foreach (var item in allObjectsInBox)
            {
                if (item.GetComponent<Chip>())
                {
                    selectedChips.Add(item.GetComponent<Chip>());
                }
            }
        }
    }

    void HandleChipMovement()
    {
        var mousePos = InputHelper.MouseWorldPos;

        if (Input.GetMouseButton(0))
        {
            // Move selected objects
            Vector2 deltaMouse = mousePos - selectionBoxStartPos;
            for (int i = 0; i < selectedChips.Count; i++)
            {
                selectedChips[i].transform.position =
                    (Vector2)selectedChipsOriginalPos[i] + deltaMouse;
                SetDepth(selectedChips[i], dragDepth + selectedChipsOriginalPos[i].z);
            }

            onChipMovement?.Invoke();
        }

        // Mouse released, so stop moving chips
        if (!Input.GetMouseButtonUp(0)) return;


        currentState = State.None;

        if (SelectedChipsWithinPlacementArea())
        {
            const float chipMoveThreshold = 0.001f;
            Vector2 deltaMouse = mousePos - selectionBoxStartPos;

            // If didn't end up moving the chips, then select just the one under the
            // mouse
            if (selectedChips.Count > 1 &&
                deltaMouse.magnitude < chipMoveThreshold)
            {
                var objectUnderMouse = InputHelper.GetObjectUnderMouse2D(chipMask);

                if (!objectUnderMouse?.GetComponent<Chip>()) return;

                selectedChips.Clear();
                selectedChips.Add(objectUnderMouse.GetComponent<Chip>());
            }
            else
            {
                for (int i = 0; i < selectedChips.Count; i++)
                {
                    SetDepth(selectedChips[i], selectedChipsOriginalPos[i].z);
                }
            }
        }
        // If any chip ended up outside of placement area, then put all chips back
        // to their original positions
        else
        {
            for (int i = 0; i < selectedChipsOriginalPos.Length; i++)
            {
                selectedChips[i].transform.position = selectedChipsOriginalPos[i];
            }
        }
    }

    // Handle placement of newly spawned chips
    void HandleNewChipPlacement()
    {
        // Cancel placement if esc or right mouse down
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace, KeyCode.Delete) ||
            Input.GetMouseButtonDown(1))
        {
            CancelPlacement(newChipsToPlace.ToArray());
            newChipsToPlace.Clear();
        }
        // Move selected chip/s and place them on left mouse down
        else
        {
            Vector2 mousePos = InputHelper.MouseWorldPos;
            float offsetY = 0;

            foreach (var chipToPlace in newChipsToPlace)
            {
                chipToPlace.transform.position = mousePos + Vector2.down * offsetY;
                SetDepth(chipToPlace, dragDepth);
                offsetY += chipToPlace.BoundsSize.y + ScalingManager.ChipStackSpace;
            }

            // Place object
            if (!Input.GetMouseButtonDown(0) || !SelectedChipsWithinPlacementArea() ||
                InputHelper.MouseOverUIObject()) return;

            PlaceNewChips(newChipsToPlace.ToArray());
            newChipsToPlace.Clear();
        }
    }

    void HandlePasteChipPlacement()
    {
        // Cancel placement if esc or right mouse down
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace,
                KeyCode.Delete) ||
            Input.GetMouseButtonDown(1))
        {
            CancelPlacement(chipsToPaste.ToArray());
            newChipsToPaste.Clear();
            chipsToPaste.Clear();
        }
        // Move selected chip/s and place them on left mouse down
        else
        {
            Vector3 mousePos = new Vector3(InputHelper.MouseWorldPos.x,
                InputHelper.MouseWorldPos.y, 0);

            foreach (KeyValuePair<Chip, Vector3> chipToPaste in newChipsToPaste)
            {
                chipToPaste.Key.transform.position = chipToPaste.Value + mousePos;
                SetDepth(chipToPaste.Key, dragDepth);
            }

            // Place object
            if (Input.GetMouseButtonDown(0) && SelectedChipsWithinPlacementArea() &&
                !InputHelper.MouseOverUIObject())
            {
                PlaceNewChips(chipsToPaste.ToArray());
                newChipsToPaste.Clear();
                chipsToPaste.Clear();
            }
        }
    }

    void PlaceNewChips(Chip[] chipsToPlace)
    {
        float startDepth = (allChips.Count > 0)
            ? allChips[^1].transform.position.z
            : 0;
        for (int i = 0; i < chipsToPlace.Length; i++)
        {
            SetDepth(chipsToPlace[i],
                startDepth + (newChipsToPlace.Count - i) * chipDepth);
        }

        allChips.AddRange(chipsToPlace);
        foreach (Chip chip in chipsToPlace)
        {
            visiblePins.AddRange(chip.inputPins);
            visiblePins.AddRange(chip.outputPins);
            foreach (Pin pin in chip.outputPins)
            {
                pin.NotifyStateChange();
            }
        }

        selectedChips.Clear();
        currentState = State.None;
    }

    void CancelPlacement(Chip[] chipsToPlace)
    {
        for (int i = chipsToPlace.Length - 1; i >= 0; i--)
        {
            Destroy(chipsToPlace[i].gameObject);
        }

        selectedChips.Clear();
        currentState = State.None;
    }

    void DrawSelectedChipBounds()
    {
        selectionBoxMaterial.color = SelectedChipsWithinPlacementArea() ? selectionBoxCol : invalidPlacementCol;

        foreach (var item in selectedChips)
        {
            var pos = item.transform.position + Vector3.forward * -0.5f;
            float sizeX = item.BoundsSize.x +
                          (PinRadius + ScalingManager.ChipInteractionBoundsBorder * 0.75f);
            float sizeY =
                item.BoundsSize.y + ScalingManager.ChipInteractionBoundsBorder;
            Matrix4x4 matrix =
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(sizeX, sizeY, 1));
            Graphics.DrawMesh(selectionMesh, matrix, selectionBoxMaterial, 0);
        }
    }

    bool SelectedChipsWithinPlacementArea()
    {
        float bufferX = PinRadius + ScalingManager.ChipInteractionBoundsBorder * 0.75f;
        float bufferY = ScalingManager.ChipInteractionBoundsBorder;
        Bounds area = chipArea.bounds;

        for (int i = 0; i < selectedChips.Count; i++)
        {
            Chip chip = selectedChips[i];
            float left = chip.transform.position.x - (chip.BoundsSize.x + bufferX) / 2;
            float right =
                chip.transform.position.x + (chip.BoundsSize.x + bufferX) / 2;
            float top = chip.transform.position.y + (chip.BoundsSize.y + bufferY) / 2;
            float bottom =
                chip.transform.position.y - (chip.BoundsSize.y + bufferY) / 2;

            if (left < area.min.x || right > area.max.x || top > area.max.y ||
                bottom < area.min.y)
            {
                return false;
            }
        }

        return true;
    }

    void SetDepth(Chip chip, float depth)
    {
        chip.transform.position = new Vector3(chip.transform.position.x,
            chip.transform.position.y, depth);
    }

    public override bool CanReleaseFocus() =>
        currentState != State.PlacingNewChips && currentState != State.MovingOldChips && !IsSelecting;

    private void FocusLostHandler()
    {
        currentState = State.None;
        selectedChips.Clear();
    }

    public override void DeleteCommand()
    {
        if (MenuManager.instance.IsAnyMenuOpen) return;

        // Delete any selected chips
        foreach (var SelectedChip in selectedChips)
            DeleteChip(SelectedChip);

        selectedChips.Clear();
        newChipsToPlace.Clear();
        newChipsToPaste.Clear();
        chipsToPaste.Clear();
    }

    public void NotifyMovement()
    {
        onChipMovement?.Invoke();
    }
}