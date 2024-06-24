using System;
using System.Collections.Generic;
using System.Linq;
using Interaction.Signal;
using Interaction.Signal.Display;
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
        set => currentEditorName = value.CurrentChip.name;
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
        PreviewSignal =
            new SignalInteractionPreview(
                SignalBuilder.Build(InputHelper.MouseWorldPos.y, 1, Pin.WireType.Simple, -1,false).obj, transform);
        DesiredGroupSize = 1;
        ScalingManager.i.OnScaleChange += UpdateScale;
        CreateGroup.i.onGroupSizeSettingPressed += OnGroupSizeSettingPressed;
    }


    private void OnDestroy()
    {
        CreateGroup.i.onGroupSizeSettingPressed -= OnGroupSizeSettingPressed;
        ScalingManager.i.OnScaleChange -= UpdateScale;
        PreviewSignal?.UnregisterEvent();
    }


    private void OnGroupSizeSettingPressed(int x)
    {
        DesiredGroupSize = x;
    }

    public ChipSignal LoadSignal(ChipSignal signal, float y, Palette.VoltageColour theme)
    {
        var signalInteraction = SignalsByID.GetValueOrDefault(signal.GroupId);
        ChipSignal chipSignal;

        if (signalInteraction is not null)
            chipSignal = signalInteraction.AddOneSignal().ChipSignal;
        else
            chipSignal = CreateSignalInteractionGroup(y, 1, signal.wireType,signal.GroupId, false).Signals.ChipSignals[0];


        chipSignal.GetComponentInChildren<SignalDisplay>().CurrentTheme = theme;
        return chipSignal;
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


        CreateSignalInteractionGroup(InputHelper.MouseWorldPos.y, DesiredGroupSize);
        DesiredGroupSize = 1;


        OnChipsAddedOrDeleted?.Invoke();
    }

    private SignalInteraction CreateSignalInteractionGroup(float yPos, int groupSize,
        Pin.WireType wireType = Pin.WireType.Simple, int id = -1,
        bool focusRequired = true)
    {
        var Interactable = SignalBuilder.Build(yPos, groupSize, wireType,id ,focusRequired);
        SignalsByID.Add(Interactable.id, Interactable.obj);
        return Interactable.obj;
    }

    private void UpdateScale()
    {
        transform.localPosition =
            new Vector3(
                ScalingManager.IoBarDistance * (editorInterfaceType == EditorInterfaceType.Input ? -1f : 1f),
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
        foreach (var e in SignalsByID.Values.Where(e => e != null))
            res.AddRange(e.Signals.ChipSignals);

        return res;
    }

    public void SetSignalCenter(Dictionary<int, float> signalGroupCenter)
    {

        foreach (var centerById in signalGroupCenter)
        {
            if (SignalsByID.TryGetValue(centerById.Key, out var signalInteraction))
                signalInteraction.SetGroupCenter(centerById.Value);
        }
    }
}