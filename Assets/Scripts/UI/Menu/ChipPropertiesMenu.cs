using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipPropertiesMenu : MonoBehaviour
{
    [SerializeField]
    private RectTransform propertiesUI;
    public Vector2 propertiesHeightMinMax;

    //fuctionality
    public TMPro.TMP_InputField nameField;
    public UnityEngine.UI.Button deleteButton;
    public UnityEngine.UI.Toggle twosComplementToggle;
    public TMPro.TMP_Dropdown modeDropdown;

    ChipInterfaceEditor CurrentInterface;

    private bool Opened = false;


    private void Awake()
    {
        propertiesUI = (RectTransform)transform.GetChild(0);
    }
    // Start is called before the first frame update
    void Start()
    {
        deleteButton.onClick.AddListener(Delete);
        modeDropdown.onValueChanged.AddListener(OnValueDropDownChange);
    }

    public void SetActive(bool b)
    {
        propertiesUI.gameObject.SetActive(b);
    }


    public void EnableUI(ChipInterfaceEditor chipInterfaceEditor, string signalName, bool isGroup, bool useTwosComplement, string currentEditorName, string signalToDragName, int wireType)
    {
        SetActive(true);
        nameField.text = signalName;
        nameField.Select();
        nameField.caretPosition = nameField.text.Length;
        twosComplementToggle.gameObject.SetActive(isGroup);
        twosComplementToggle.isOn = useTwosComplement;
        modeDropdown.gameObject.SetActive(!isGroup);
        deleteButton.interactable = ChipSaver.IsSignalSafeToDelete(currentEditorName, signalToDragName);
        modeDropdown.SetValueWithoutNotify(wireType);
        CurrentInterface = chipInterfaceEditor;

        var SizeDelta = new Vector2(propertiesUI.sizeDelta.x, (isGroup) ? propertiesHeightMinMax.y : propertiesHeightMinMax.x);
        propertiesUI.sizeDelta = SizeDelta;

        Opened = true;
    }
    
    public void DisableUI()
    {
        if (!Opened) return;
        SetActive(false);
        SaveProperty();
        ResetC();
    }

    private void ResetC()
    {
        nameField.text = "";
        CurrentInterface = null;
        Opened = false;
    }

    public void SetPosition(Vector3 centre, ChipInterfaceEditor.EditorType editorType)
    {
        float propertiesUIX = ScalingManager.propertiesUIX * (editorType == ChipInterfaceEditor.EditorType.Input ? 1 : -1);
        propertiesUI.transform.position = new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
    }


    void SaveProperty()
    {
        if (CurrentInterface != null)
            CurrentInterface.UpdateGroupProperty(nameField.text, twosComplementToggle.isOn);
    }

    void Delete()
    {
        CurrentInterface.DeleteCommand();
    }
    void OnValueDropDownChange(int mode)
    {
        CurrentInterface.ModeChanged(mode);
    }
}
