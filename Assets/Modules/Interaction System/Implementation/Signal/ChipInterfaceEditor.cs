using System;
using System.Collections.Generic;
using Interaction.Signal;
using UnityEngine;

// Allows player to add/remove/move/rename inputs or outputs of a chip.
public enum EditorInterfaceType
{
    Input,
    Output
}

public class ChipInterfaceEditor : MonoBehaviour
{
    const int maxGroupSize = 16;

    [SerializeField] private SignalInteraction SignalInteractablePref;

    public event Action<Chip> OnDeleteChip;
    public event Action OnChipsAddedOrDeleted;

    private SignalInteractionPreview PreviewSignal;


    public EditorInterfaceType editorInterfaceType;

    [Header("References")] public Transform chipContainer;


    public Transform signalHolder;
    public Transform barGraphic;
    public ChipInterfaceEditor otherEditor;

    public bool showPreviewSignal;

    string currentEditorName;

    public ChipEditor CurrentEditor
    {
        set => currentEditorName = value.Data.name;
    }

    public SignalInteraction selectedSignals { get; private set; }

    // Grouping
    private int DesiredGroupSize
    {
        get => _desiredGroupSize;
        set
        {
            _desiredGroupSize = Mathf.Clamp(value, 1, maxGroupSize);
            PreviewSignal.SetGroupSize(_desiredGroupSize);
        }
    }

    private int _desiredGroupSize = 1;


    private Dictionary<int, SignalInteraction> SignalsByID = new Dictionary<int, SignalInteraction>();
    private SignalInteractionBuilder SignalBuilder;

    void Awake()
    {
        float BoundsTop = transform.position.y + (transform.localScale.y / 2);
        float BoundsBottom = transform.position.y - transform.localScale.y / 2f;
        // Handles spawning if user clicks, otherwise displays preview
        float containerX = chipContainer.position.x +
                           chipContainer.localScale.x / 2 *
                           ((editorInterfaceType == EditorInterfaceType.Input) ? -1 : 1);

        SignalBuilder = new SignalInteractionBuilder(SignalInteractablePref, signalHolder, OnDeleteChip, BoundsBottom,
            BoundsTop, containerX, chipContainer.position.z, editorInterfaceType);
    }


    private void Start()
    {
        PreviewSignal = new SignalInteractionPreview(SignalBuilder.Build(InputHelper.MouseWorldPos.y, 1,Pin.WireType.Simple,false).obj, transform);
        DesiredGroupSize = 1;
        ScalingManager.i.OnScaleChange += UpdateScale;
        CreateGroup.i.onGroupSizeSettingPressed += OnGroupSizeSettingPressed;
    }


    private void OnDestroy()
    {
        CreateGroup.i.onGroupSizeSettingPressed -= OnGroupSizeSettingPressed;
        ScalingManager.i.OnScaleChange -= UpdateScale;
        PreviewSignal.UnregisterEvent();
    }


    private void OnGroupSizeSettingPressed(int x)
    {
        DesiredGroupSize = x;
    }

    public ChipSignal LoadSignal(ChipSignal signal, float y)
    {
        var Chip = AddSignal(y, 1,signal.wireType, false).Signals.ChipSignals[0];
        return Chip;
    }


    private void OnMouseEnter()
    {
        PreviewSignal?.Enable();
    }

    private void OnMouseOver()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Plus, KeyCode.KeypadPlus, KeyCode.Equals, KeyCode.R))
            DesiredGroupSize++;
        else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Minus, KeyCode.KeypadMinus, KeyCode.Underscore, KeyCode.F))
            DesiredGroupSize--;

        PreviewSignal.AdjustYPosition();
    }

    private void OnMouseExit()
    {
        PreviewSignal?.Disable();
    }

    private void OnMouseDown()
    {
        if (InputHelper.MouseOverUIObject()) return;

        HandleSpawning();
    }

    void HandleSpawning()
    {
        if (InputHelper.MouseOverUIObject())
            return;


        // Spawn on mouse down
        if (!Input.GetMouseButtonDown(0)) return;

        if (InputHelper.CompereTagObjectUnderMouse2D(ProjectTags.InterfaceMask, ProjectLayer.Default)) return;


        AddSignal(InputHelper.MouseWorldPos.y,  DesiredGroupSize);
        DesiredGroupSize = 1;


        OnChipsAddedOrDeleted?.Invoke();
    }

    private SignalInteraction AddSignal(float yPos, int groupSize, Pin.WireType wireType = Pin.WireType.Simple ,bool focusRequired = true)
    {
        var Interactable = SignalBuilder.Build(yPos, groupSize,wireType, focusRequired);
        SignalsByID.Add(Interactable.id, Interactable.obj);
        return Interactable.obj;
    }

    private void UpdateScale()
    {
        transform.localPosition =
            new Vector3(ScalingManager.IoBarDistance * (editorInterfaceType == EditorInterfaceType.Input ? -1f : 1f),
                transform.localPosition.y, transform.localPosition.z);
        barGraphic.localScale = new Vector3(ScalingManager.IoBarGraphicWidth, 1, 1);
        GetComponent<BoxCollider2D>().size = new Vector2(ScalingManager.IoBarGraphicWidth, 1);
        
        
        float containerX = chipContainer.position.x +
                           chipContainer.localScale.x / 2 *
                           ((editorInterfaceType == EditorInterfaceType.Input) ? -1 : 1);
        
        PreviewSignal.UpdatePositionWithScale(containerX);


    }


    public List<ChipSignal> GetAllSignals()
    {
        var res = new List<ChipSignal>();
        foreach (var e in SignalsByID.Values)
        {
            res.AddRange(e.Signals.ChipSignals);
        }

        return res;
    }
}