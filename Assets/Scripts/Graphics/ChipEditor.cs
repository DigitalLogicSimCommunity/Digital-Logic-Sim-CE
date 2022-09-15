using UnityEngine;

public class ChipEditor : MonoBehaviour
{
    public Transform chipImplementationHolder;
    public Transform wireHolder;

    public ChipInterfaceEditor inputsEditor;
    public ChipInterfaceEditor outputsEditor;
    public ChipInteraction chipInteraction;
    public PinAndWireInteraction pinAndWireInteraction;

    public PinNameDisplayManager pinNameDisplayManager ;

    public ChipData Data;

    void Awake()
    {
        Data = new ChipData()
        {
            FolderIndex = 0,
            scale = 1
        };


        pinAndWireInteraction.Init(chipInteraction, inputsEditor, outputsEditor);
        pinAndWireInteraction.onConnectionChanged += OnChipNetworkModified;
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }

    void LateUpdate()
    {
        inputsEditor.OrderedUpdate();
        outputsEditor.OrderedUpdate();
        pinAndWireInteraction.OrderedUpdate();
        chipInteraction.OrderedUpdate();
    }

    void OnChipNetworkModified() { CycleDetector.MarkAllCycles(this); }

    public void LoadFromSaveData(ChipSaveData saveData)
    {
        Data = saveData.Data;
        ScalingManager.scale = Data.scale;

        // Load component chips
        foreach (Chip componentChip in saveData.componentChips)
        {
            if (componentChip is InputSignal inp)
            {
                inp.wireType = inp.outputPins[0].wireType;
                inputsEditor.LoadSignal(inp);
            }
            else if (componentChip is OutputSignal outp)
            {
                outp.wireType = outp.inputPins[0].wireType;
                outputsEditor.LoadSignal(outp);
            }
            else
            {
                chipInteraction.LoadChip(componentChip);
            }
        }

        // Load wires
        if (saveData.wires != null)
        {
            foreach (Wire wire in saveData.wires)
            {
                pinAndWireInteraction.LoadWire(wire);
            }
        }

        ChipEditorOptions.instance.SetUIValues(this);
    }

    public void UpdateChipSizes()
    {
        foreach (Chip chip in chipInteraction.allChips)
        {
            ChipPackage package = chip.GetComponent<ChipPackage>();
            if (package)
            {
                package.SetSizeAndSpacing(chip);
            }
        }
    }

    void OnDestroy()
    {
        chipInteraction.visiblePins.Clear();
        inputsEditor.visiblePins.Clear();
        outputsEditor.visiblePins.Clear();
    }
}
