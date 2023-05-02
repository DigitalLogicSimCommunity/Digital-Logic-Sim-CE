using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using Interaction.Signal;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VitoBarra.Utils.T;

public class SignalPropertiesMenu : MonoBehaviour
{
    [SerializeField] private RectTransform propertiesUI;
    public Vector2 propertiesHeightMinMax;

    //reference
    public TMPro.TMP_InputField nameField;
    public UnityEngine.UI.Button deleteButton;
    public UnityEngine.UI.Toggle twosComplementToggle;
    public TMPro.TMP_Dropdown WireTypeDropdown;
    public TMPro.TMP_InputField BusValueField;
    public TMPro.TMP_InputField GroupSizeField;

    //Working field
    private SignalInteraction SignalInteraction;
    private bool UnDone = false;
    private Pin.WireType StartWireType;

    private Delayer delayer = new Delayer(0.1f);

    private void Awake()
    {
        propertiesUI = (RectTransform)transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        deleteButton.onClick.AddListener(Delete);
        WireTypeDropdown.onValueChanged.AddListener(OnValueDropDownChange);

        MenuManager.instance.RegisterFinalizer(MenuType.SignalPropertiesMenu, OnCloseUIHandler);

        nameField.onSelect.AddListener((_) => DisableDeleteCommand());
        nameField.onDeselect.AddListener((_) => EnableDeleteCommand());

        BusValueField.onSelect.AddListener((_) => DisableDeleteCommand());
        BusValueField.onDeselect.AddListener((_) => EnableDeleteCommand());
        BusValueField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        BusValueField.onValueChanged.AddListener(OnBusInputValueChanged);

        GroupSizeField.onDeselect.AddListener((_) => EnableDeleteCommand());
        GroupSizeField.onSelect.AddListener((_) => DisableDeleteCommand());
        GroupSizeField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        GroupSizeField.onValueChanged.AddListener(OnGroupInputValueChanged);
    }


    private void Update()
    {
        if (SignalInteraction == null) return;
        var selectedInput = GetSelectedInputField();

        void UpdateWithValue(int t)
        {
            selectedInput.text = (int.Parse(selectedInput.text) + t).ToString();
            delayer.StartCount();
        }

        if (selectedInput != null)
        {
            if (Input.GetKey(KeyCode.UpArrow) && delayer.IsDelayPassed)
                UpdateWithValue(1);
            else if (Input.GetKey(KeyCode.DownArrow) && delayer.IsDelayPassed)
                UpdateWithValue(-1);
        }

        if (!nameField.isFocused && InputHelper.AnyOfTheseKeysDown(KeyCode.Return, KeyCode.Space))
            MenuManager.instance.CloseMenu();
        else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape))
        {
            UnDone = true;
            MenuManager.instance.CloseMenu();
        }
    }

    private TMP_InputField GetSelectedInputField()
    {
        if (GroupSizeField.isFocused) return GroupSizeField;
        if (BusValueField.isFocused) return BusValueField;
        return null;
    }


    private void OnGroupInputValueChanged(string newValue)
    {
        if (SignalInteraction.WireType != Pin.WireType.Simple) return;
        InputFieldMaxValue(GroupSizeField, newValue, SignalInteraction.MaxGroupSize);
    }


    private void OnBusInputValueChanged(string newValue)
    {
        if (SignalInteraction.WireType == Pin.WireType.Simple) return;

        var maxValue = Math.Pow(2, SignalInteraction.Signals[0].ChipSignal.State.Count) - 1;
        InputFieldMaxValue(BusValueField, newValue, maxValue);
    }

    private void InputFieldMaxValue(TMP_InputField inputField, string newValue, double maxValue)
    {
        if (!int.TryParse(newValue, out var intValue) || intValue < 0)
            inputField.text = 0.ToString();
        else if (intValue > maxValue)
            inputField.text = maxValue.ToString();
    }


    public void SetUpUI(SignalInteraction signalInteraction)
    {
        propertiesUI.gameObject.SetActive(true);
        SignalInteraction = signalInteraction;
        StartWireType = signalInteraction.WireType;

        nameField.text = SignalInteraction.SignalName;
        nameField.caretPosition = nameField.text.Length;

        twosComplementToggle.gameObject.SetActive(signalInteraction.IsGroup);
        twosComplementToggle.isOn = signalInteraction.UseTwosComplement;
        WireTypeDropdown.gameObject.SetActive(!signalInteraction.IsGroup);
        WireTypeDropdown.SetValueWithoutNotify((int)signalInteraction.WireType);

        var SizeDelta = new Vector2(propertiesUI.sizeDelta.x,
            (signalInteraction.IsGroup) ? propertiesHeightMinMax.y : propertiesHeightMinMax.x);
        propertiesUI.sizeDelta = SizeDelta;

        ToggleBusValueActivation(signalInteraction.WireType, signalInteraction.EditorInterfaceType);
        ToggleGroupValueActivation(signalInteraction.WireType);

        SetPosition(signalInteraction.GroupCenter, signalInteraction.EditorInterfaceType);


        MenuManager.instance.OpenMenu(MenuType.SignalPropertiesMenu);
    }

    private void ToggleGroupValueActivation(Pin.WireType wireType)
    {
        var parentGameObject = GroupSizeField.transform.parent.gameObject;
        if (wireType != Pin.WireType.Simple)
            parentGameObject.SetActive(false);
        else
        {
            parentGameObject.SetActive(true);
            GroupSizeField.text = SignalInteraction.GroupSize.ToString();
        }
    }

    private void ToggleBusValueActivation(Pin.WireType wireType, EditorInterfaceType InterfaceType)
    {
        var parentGameObject = BusValueField.transform.parent.gameObject;
        if (wireType == Pin.WireType.Simple || InterfaceType == EditorInterfaceType.Output)
            parentGameObject.SetActive(false);
        else
        {
            parentGameObject.SetActive(true);
            BusValueField.text = SignalInteraction.Signals[0].ChipSignal.State.ToUInt().ToString();
        }
    }

    private void OnCloseUIHandler()
    {
        SaveProperty();
        SignalInteraction = null;
    }

    private void SetPosition(Vector3 centre, EditorInterfaceType editorInterfaceType)
    {
        float propertiesUIX =
            ScalingManager.PropertiesUIX * (editorInterfaceType == EditorInterfaceType.Input ? 1 : -1);
        propertiesUI.transform.position =
            new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
    }


    void SaveProperty()
    {
        if (SignalInteraction == null) return;

        if (UnDone)
        {
            SignalInteraction.SetWireType(StartWireType);
            UnDone = false;
        }
        else
        {
            SignalInteraction.UpdateGroupProperty(nameField.text, twosComplementToggle.isOn);

            if ((Pin.WireType)WireTypeDropdown.value != Pin.WireType.Simple)
                SignalInteraction.SetBusValue(int.Parse(BusValueField.text));
            else
                SignalInteraction.SetGroupSize(int.Parse(GroupSizeField.text));
        }
    }


    void Delete()
    {
        SignalInteraction.DeleteCommand();
    }

    void DeleteFinalizer()
    {
        UnregisterSignalGroup(SignalInteraction);
        MenuManager.instance.CloseMenu();
    }

    void OnValueDropDownChange(int mode)
    {
        var wireType = (Pin.WireType)mode;
        if (SignalInteraction != null)
            SignalInteraction.SetWireType(wireType);
        ToggleBusValueActivation(wireType, SignalInteraction.EditorInterfaceType);
        ToggleGroupValueActivation(wireType);
    }

    public void RegisterSignalGroup(SignalInteraction signalInteraction)
    {
        signalInteraction.OnDragig += SetPosition;
        signalInteraction.OnDeleteInteraction += DeleteFinalizer;
    }

    private void UnregisterSignalGroup(SignalInteraction signalInteraction)
    {
        signalInteraction.OnDragig -= SetPosition;
        signalInteraction.OnDeleteInteraction -= DeleteFinalizer;
    }

    private void DisableDeleteCommand()
    {
        if (SignalInteraction == null) return;
        SignalInteraction.SilenceDeleteCommand();
    }

    private void EnableDeleteCommand()
    {
        if (SignalInteraction == null) return;
        SignalInteraction.EnableDeleteCommand();
    }
}