using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Allows player to add/remove/move/rename inputs or outputs of a chip.
public class ChipInterfaceEditor : Interactable
{

    const int maxGroupSize = 16;

    public event System.Action<Chip> OnDeleteChip;
    public event System.Action OnChipsAddedOrDeleted;

    public enum EditorType { Input, Output }
    public enum HandleState { Default, Highlighted, Selected, SelectedAndFocused }
    const float forwardDepth = -0.1f;

    public List<ChipSignal> signals { get; private set; }

    public EditorType editorType;

    [Header("References")]
    public Transform chipContainer;
    public ChipSignal signalPrefab;

    public ChipPropertiesMenu PropertiesMenu;

    public Transform signalHolder;
    public Transform barGraphic;
    public ChipInterfaceEditor otherEditor;

    [Header("Appearance")]
    public Color handleCol;
    public Color highlightedHandleCol;
    public Color selectedHandleCol;
    public Color selectedAndFocusedHandleCol;

    public bool showPreviewSignal;

    [HideInInspector]
    public List<Pin> visiblePins;

    const float handleSizeX = 0.15f;

    string currentEditorName;
    public ChipEditor CurrentEditor
    {
        set => currentEditorName = value.Data.name;
    }

    ChipSignal highlightedSignal;
    public List<ChipSignal> selectedSignals { get; private set; }
    ChipSignal[] previewSignals;

    BoxCollider2D inputBounds;

    Mesh quadMesh;
    Material handleMat;
    Material highlightedHandleMat;
    Material selectedHandleMat;
    Material selectedAndhighlightedHandle;
    bool mouseInInputBounds;

    // Dragging
    bool isDragging;
    float dragHandleStartY;
    float dragMouseStartY;

    // Grouping
    int currentGroupSize = 1;
    int currentGroupID;
    Dictionary<int, ChipSignal[]> groupsByID;


    void Awake()
    {
        signals = new List<ChipSignal>();
        selectedSignals = new List<ChipSignal>();
        groupsByID = new Dictionary<int, ChipSignal[]>();
        visiblePins = new List<Pin>();

        inputBounds = GetComponent<BoxCollider2D>();
        MeshShapeCreator.CreateQuadMesh(ref quadMesh);
        handleMat = MaterialUtility.CreateUnlitMaterial(handleCol);
        highlightedHandleMat = MaterialUtility.CreateUnlitMaterial(highlightedHandleCol);
        selectedHandleMat = MaterialUtility.CreateUnlitMaterial(selectedHandleCol);
        selectedAndhighlightedHandle = MaterialUtility.CreateUnlitMaterial(selectedAndFocusedHandleCol);

        previewSignals = new ChipSignal[maxGroupSize];
        for (int i = 0; i < maxGroupSize; i++)
        {
            var previewSignal = Instantiate(signalPrefab);
            previewSignal.SetInteractable(false);
            previewSignal.gameObject.SetActive(false);
            previewSignal.signalName = "Preview";
            previewSignal.transform.SetParent(transform, true);
            previewSignals[i] = previewSignal;
        }

        FindObjectOfType<CreateGroup>().onGroupSizeSettingPressed += SetGroupSize;

    }

    public void Start()
    {
        PropertiesMenu.DisableUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !InputHelper.MouseOverUIObject())
            ReleaseFocus();
    }

    public override void FocusLostHandler()
    {

        highlightedSignal = null;
        PropertiesMenu.DisableUI();
        ClearSelectedSignals();

        HidePreviews();
        //currentGroupSize = 1;
    }


    // Event handler when changed input or output pin wire type
    public void ModeChanged(int mode)
    {
        if (!IsSomethingSelected)
            return;

        // Change output pin wire mode
        foreach (var pin in selectedSignals.SelectMany(x => x.inputPins))
        {
            pin.wireType = (Pin.WireType)mode;
        }

        // Change input pin wire mode
        if (selectedSignals[0] is InputSignal)
        {
            foreach (InputSignal signal in selectedSignals)
            {
                var pin = signal.outputPins[0];
                if (pin == null)
                    return;
                pin.wireType = (Pin.WireType)mode;
                // Turn off input pin
                if (pin.State == 1)
                    signal.ToggleActive();
            }
        }
    }

    public override void OrderedUpdate()
    {
        if (!InputHelper.MouseOverUIObject())
        {
            HandleInput();
        }
        else if (HasFocus)
        {
            ReleaseFocusNotHandled();
            HidePreviews();
        }
        DrawSignalHandles();

    }

    void SetGroupSize(int groupSize) => currentGroupSize = groupSize;

    public void LoadSignal(InputSignal signal)
    {
        signal.transform.parent = signalHolder;
        signals.Add(signal);

        signal.signalName = signal.outputPins[0].pinName;
        visiblePins.Add(signal.outputPins[0]);
    }

    public void LoadSignal(OutputSignal signal)
    {
        signal.transform.parent = signalHolder;
        signals.Add(signal);

        signal.signalName = signal.inputPins[0].pinName;
        visiblePins.Add(signal.inputPins[0]);
    }

    void HandleInput()
    {
        Vector2 mousePos = InputHelper.MouseWorldPos;

        mouseInInputBounds = inputBounds.OverlapPoint(mousePos);



        highlightedSignal = GetSignalUnderMouse();

        if (mouseInInputBounds && highlightedSignal != null && Input.GetMouseButtonDown(0))
            RequestFocus();
        else if (!IsSomethingSelected)
        {
            ReleaseFocusNotHandled();
            isDragging = false;
        }

        if (HasFocus)
        {
            otherEditor.ClearSelectedSignals();

            if (Input.GetMouseButtonDown(0))
                SelectSignal(highlightedSignal);

            // If a signal is selected, handle movement/renaming/deletion
            if (IsSomethingSelected)
            {
                if (isDragging)
                {
                    float handleNewY = mousePos.y + (dragHandleStartY - dragMouseStartY);
                    bool cancel = Input.GetKeyDown(KeyCode.Escape);

                    if (cancel) handleNewY = dragHandleStartY;

                    for (int i = 0; i < selectedSignals.Count; i++)
                    {
                        float y = CalcY(handleNewY, selectedSignals.Count, i);
                        selectedSignals[i].transform.SetYPos(y);
                    }

                    if (Input.GetMouseButtonUp(0)) isDragging = false;


                    // Cancel drag and deselect
                    if (cancel) FocusLostHandler();
                }

                UpdatePropertyUIPosition();

                // Finished with selected signal, so deselect it
                if (Input.GetKeyDown(KeyCode.Return)) FocusLostHandler();

            }

        }
        HidePreviews();
        if (highlightedSignal == null && !isDragging)
        {
            if (mouseInInputBounds && !InputHelper.MouseOverUIObject())
            {

                if (InputHelper.AnyOfTheseKeysDown(KeyCode.Plus, KeyCode.KeypadPlus,
                                                   KeyCode.Equals))
                {
                    currentGroupSize =
                        Mathf.Clamp(currentGroupSize + 1, 1, maxGroupSize);
                }
                else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Minus,
                                                          KeyCode.KeypadMinus,
                                                          KeyCode.Underscore))
                {
                    currentGroupSize =
                        Mathf.Clamp(currentGroupSize - 1, 1, maxGroupSize);
                }

                HandleSpawning();
            }
        }
    }

    public void ClearSelectedSignals() { selectedSignals.Clear(); }

    float CalcY(float mouseY, int groupSize, int index)
    {
        float centreY = mouseY;
        float halfExtent = ScalingManager.groupSpacing * (groupSize - 1f);
        float maxY = centreY + halfExtent + ScalingManager.handleSizeY / 2f;
        float minY = centreY - halfExtent - ScalingManager.handleSizeY / 2f;

        if (maxY > BoundsTop)
        {
            centreY -= (maxY - BoundsTop);
        }
        else if (minY < BoundsBottom)
        {
            centreY += (BoundsBottom - minY);
        }

        float t = (groupSize > 1) ? index / (groupSize - 1f) : 0.5f;
        t = t * 2 - 1;
        float posY = centreY - t * halfExtent;
        return posY;
    }

    float ClampY(float y)
    {
        return Mathf.Clamp(y, BoundsBottom + ScalingManager.handleSizeY / 2f,
                           BoundsTop - ScalingManager.handleSizeY / 2f);
    }

    public ChipSignal[][] GetGroups()
    {
        var keys = groupsByID.Keys;
        ChipSignal[][] groups = new ChipSignal[keys.Count][];
        int i = 0;
        foreach (var key in keys)
        {
            groups[i] = groupsByID[key];
            i++;
        }
        return groups;
    }

    // Handles spawning if user clicks, otherwise displays preview
    void HandleSpawning()
    {

        if (InputHelper.MouseOverUIObject())
            return;

        float containerX = chipContainer.position.x +
                           chipContainer.localScale.x / 2 *
                               ((editorType == EditorType.Input) ? -1 : 1);
        float centreY = ClampY(InputHelper.MouseWorldPos.y);

        // Spawn on mouse down
        if (Input.GetMouseButtonDown(0))
        {
            ChipSignal[] spawnedSignals = new ChipSignal[currentGroupSize];

            var isGroup = currentGroupSize > 1;

            for (int i = 0; i < currentGroupSize; i++)
            {
                float posY = CalcY(InputHelper.MouseWorldPos.y, currentGroupSize, i);
                Vector3 spawnPos = new Vector3(containerX, posY, chipContainer.position.z + forwardDepth);

                ChipSignal spawnedSignal = Instantiate(signalPrefab, spawnPos, Quaternion.identity, signalHolder);
                spawnedSignal.GetComponent<IOScaler>().UpdateScale();
                if (isGroup)
                {
                    spawnedSignal.GroupID = currentGroupID;
                    spawnedSignal.displayGroupDecimalValue = true;
                }
                signals.Add(spawnedSignal);
                visiblePins.AddRange(spawnedSignal.inputPins);
                visiblePins.AddRange(spawnedSignal.outputPins);
                spawnedSignals[i] = spawnedSignal;
                
            }

            if (isGroup)
            {
                groupsByID.Add(currentGroupID, spawnedSignals);
                // Reset group size after spawning
                currentGroupSize = 1;
                // Generate new ID for next group
                // This will be used to identify which signals were created together as
                // a group
                currentGroupID++;
            }
            SelectSignal(signals[signals.Count - 1]);
            OnChipsAddedOrDeleted?.Invoke();
        }
        // Draw handle and signal previews
        else
        {
            for (int i = 0; i < currentGroupSize; i++)
            {
                float posY = CalcY(InputHelper.MouseWorldPos.y, currentGroupSize, i);
                Vector3 spawnPos = new Vector3(containerX, posY, chipContainer.position.z + forwardDepth);
                DrawHandle(posY, HandleState.Highlighted);
                if (showPreviewSignal)
                {
                    previewSignals[i].gameObject.SetActive(true);
                    previewSignals[i].transform.position =
                        spawnPos - Vector3.forward * forwardDepth;
                }
            }
        }
    }



    void HidePreviews()
    {
        foreach (ChipSignal PrevSig in previewSignals)
            PrevSig.gameObject.SetActive(false);
    }

    float BoundsTop => transform.position.y + (transform.localScale.y / 2);

    float BoundsBottom => transform.position.y - transform.localScale.y / 2f;

    public bool IsSomethingSelected => selectedSignals.Count > 0;

    public override bool CanReleaseFocus() => !isDragging && !mouseInInputBounds;



    void UpdatePropertyUIPosition()
    {
        if (IsSomethingSelected)
        {
            Vector3 centre =
                (selectedSignals[0].transform.position + selectedSignals[selectedSignals.Count - 1].transform.position) / 2;

            PropertiesMenu.SetPosition(centre, editorType);
        }
    }

    public void UpdateGroupProperty(string NewName, bool twosComplementToggle)
    {
        // Update signal properties
        foreach (ChipSignal Signal in selectedSignals)
        {
            Signal.UpdateSignalName(NewName);
            Signal.useTwosComplement = twosComplementToggle;
        }
    }

    void DrawSignalHandles()
    {
        foreach (ChipSignal singnal in signals)
        {
            HandleState handleState = HandleState.Default;

            if (selectedSignals.Contains(singnal))
                handleState = HasFocus ? HandleState.SelectedAndFocused : HandleState.Selected;
            else if(singnal == highlightedSignal)
                handleState = HandleState.Highlighted;

            DrawHandle(singnal.transform.position.y, handleState);
        }
    }

    ChipSignal GetSignalUnderMouse()
    {
        ChipSignal signalUnderMouse = null;
        float nearestDst = float.MaxValue;

        for (int i = 0; i < signals.Count; i++)
        {
            ChipSignal currentSignal = signals[i];
            float handleY = currentSignal.transform.position.y;

            Vector2 handleCentre = new Vector2(transform.position.x, handleY);
            Vector2 mousePos = InputHelper.MouseWorldPos;

            const float selectionBufferY = 0.1f;

            float halfSizeX = handleSizeX;
            float halfSizeY = (ScalingManager.handleSizeY + selectionBufferY) / 2f;
            bool insideX = mousePos.x >= handleCentre.x - halfSizeX &&
                           mousePos.x <= handleCentre.x + halfSizeX;
            bool insideY = mousePos.y >= handleCentre.y - halfSizeY &&
                           mousePos.y <= handleCentre.y + halfSizeY;

            if (insideX && insideY)
            {
                float dst = Mathf.Abs(mousePos.y - handleY);
                if (dst < nearestDst)
                {
                    nearestDst = dst;
                    signalUnderMouse = currentSignal;
                }
            }
        }
        return signalUnderMouse;
    }

    // Select signal (starts dragging, shows rename field)
    void SelectSignal(ChipSignal signalToDrag)
    {
        if (signalToDrag == null) return;
        // Dragging
        SelectAllSignalsInTheSameGroup(signalToDrag);

        isDragging = true;


        dragMouseStartY = InputHelper.MouseWorldPos.y;
        if (selectedSignals.Count % 2 == 0)
        {
            int indexA = Mathf.Max(0, selectedSignals.Count / 2 - 1);
            int indexB = selectedSignals.Count / 2;
            dragHandleStartY = (selectedSignals[indexA].transform.position.y +
                                selectedSignals[indexB].transform.position.y) /
                               2f;
        }
        else
        {
            dragHandleStartY = selectedSignals[selectedSignals.Count / 2].transform.position.y;
        }

        PropertiesMenu.EnableUI(this, selectedSignals[0].signalName, selectedSignals.Count > 1, selectedSignals[0].useTwosComplement,
                                   currentEditorName, signalToDrag.signalName, (int)selectedSignals[0].wireType);
        RequestFocus();

        UpdatePropertyUIPosition();
    }

    private void SelectAllSignalsInTheSameGroup(ChipSignal signalToDrag)
    {
        ClearSelectedSignals();

        foreach (ChipSignal sig in signals)
        {
            if (sig == signalToDrag || ChipSignal.InSameGroup(sig, signalToDrag))
                selectedSignals.Add(sig);
        }
    }



    void DrawHandle(float y, HandleState handleState = HandleState.Default)
    {
        float renderZ = forwardDepth;
        Material currentHandleMat;
        switch (handleState)
        {
            case HandleState.Highlighted:
                currentHandleMat = highlightedHandleMat;
                break;
            case HandleState.Selected:
                currentHandleMat = selectedHandleMat;
                renderZ = forwardDepth * 2;
                break;
            case HandleState.SelectedAndFocused:
                currentHandleMat = selectedAndhighlightedHandle;
                renderZ = forwardDepth * 2;
                break;
            default:
                currentHandleMat = handleMat;
                break;
        }

        Vector3 scale = new Vector3(handleSizeX, ScalingManager.handleSizeY, 1);
        Vector3 pos3D = new Vector3(transform.position.x, y, transform.position.z + renderZ);
        Matrix4x4 handleMatrix = Matrix4x4.TRS(pos3D, Quaternion.identity, scale);
        Graphics.DrawMesh(quadMesh, handleMatrix, currentHandleMat, 0);
    }


    public void UpdateColours()
    {
        handleMat.color = handleCol;
        highlightedHandleMat.color = highlightedHandleCol;
        selectedHandleMat.color = selectedHandleCol;
        selectedAndhighlightedHandle.color = selectedAndFocusedHandleCol;
    }

    public void UpdateScale()
    {
        transform.localPosition =
            new Vector3(ScalingManager.ioBarDistance *
                            (editorType == EditorType.Input ? -1f : 1f),
                        transform.localPosition.y, transform.localPosition.z);
        barGraphic.localScale = new Vector3(ScalingManager.ioBarGraphicWidth, 1, 1);
        GetComponent<BoxCollider2D>().size = new Vector2(ScalingManager.ioBarGraphicWidth, 1);

        foreach (ChipSignal chipSignal in previewSignals)
        {
            chipSignal.GetComponent<IOScaler>().UpdateScale();
        }

        foreach (ChipSignal[] group in groupsByID.Values)
        {
            float yPos = 0;
            foreach (ChipSignal sig in group)
            {
                yPos += sig.transform.localPosition.y;
            }
            float handleNewY = yPos /= group.Length;

            for (int i = 0; i < group.Length; i++)
            {
                float y = CalcY(handleNewY, group.Length, i);
                group[i].transform.SetYPos(y);
            }
        }
        UpdatePropertyUIPosition();
    }



    public override void DeleteCommand()
    {
        if (!Input.GetKeyDown(KeyCode.Backspace))
            DeleteSelected();
    }
    private void DeleteSelected()
    {
        foreach (ChipSignal selectedSignal in selectedSignals)
        {
            if (groupsByID.ContainsKey(selectedSignal.GroupID))
                groupsByID.Remove(selectedSignal.GroupID);

            OnDeleteChip?.Invoke(selectedSignal);
            signals.Remove(selectedSignal);

            foreach (Pin pin in selectedSignal.inputPins)
                visiblePins.Remove(pin);
            foreach (Pin pin in selectedSignal.outputPins)
                visiblePins.Remove(pin);

            Destroy(selectedSignal.gameObject);
        }
        OnChipsAddedOrDeleted?.Invoke();
        ReleaseFocus();
    }
}
