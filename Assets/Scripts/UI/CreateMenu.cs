using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateMenu : MonoBehaviour {

  public TMP_InputField chipNameField;
  public TMP_Dropdown folderDropdown;
  public Button doneButton;
  public Slider hueSlider;
  public Slider saturationSlider;
  public Slider valueSlider;
  [Range(0, 1)]
  public float textColThreshold = 0.5f;

  public Color[] suggestedColours;
  int suggestedColourIndex;

  string validChars =
      "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]-";

  List<string> allChipNames = new List<string>();

  void Awake() {
    suggestedColourIndex = Random.Range(0, suggestedColours.Length);
  }

  void Update() {
    if (UIManager.instance.GetMenuByType(MenuType.CreateChipMenu).isActive) {
      // Force name input field to remain focused
      if (!chipNameField.isFocused) {
        chipNameField.Select();
        // Put caret at end of text (instead of selecting the text, which is
        // annoying in this case)
        chipNameField.caretPosition = chipNameField.text.Length;
      }
    }
  }

  public void SelectFolder() {
    Manager.ActiveChipEditor.chipFolder =
        folderDropdown.options[folderDropdown.value].text;
  }

  public void ColourSliderChanged() {
    Color chipCol = Color.HSVToRGB(hueSlider.value, saturationSlider.value,
                                   valueSlider.value);
    UpdateColour(chipCol);
  }

  public void ChipNameFieldChanged(bool endEdit = false) {
    string text = chipNameField.text.ToUpper();
    string validName = "";
    for (int i = 0; i < text.Length; i++) {
      if (i < 12 && validChars.Contains(text[i].ToString())) {
        validName += text[i];
      }
    }
    validName = endEdit ? validName.Trim() : validName.TrimStart();

    if (IsAvailableName(validName) && validName.Length > 0) {
      Manager.ActiveChipEditor.chipName = validName;
      doneButton.interactable = true;
    } else {
      doneButton.interactable = false;
    }
    chipNameField.text = validName;
  }

  bool IsAvailableName(string chipName) {
    return !allChipNames.Contains(chipName);
  }

  public void Prepare() {
    allChipNames = Manager.instance.AllChipNames();
    folderDropdown.ClearOptions();
    folderDropdown.AddOptions(
        ChipBarUI.instance.selectedFolderDropdown.options.GetRange(
            2, ChipBarUI.instance.selectedFolderDropdown.options.Count - 3));
    folderDropdown.value = ChipBarUI.selectedFolderIndex > 1
                               ? ChipBarUI.selectedFolderIndex - 2
                               : 0;
    doneButton.interactable = false;
    chipNameField.SetTextWithoutNotify("");
    SetSuggestedColour();
  }

  public void FinishCreation() {
    Manager.ActiveChipEditor.chipFolder =
        folderDropdown.options[folderDropdown.value].text;
    Manager.ActiveChipEditor.scale = ScalingManager.scale;
    Manager.instance.SaveAndPackageChip();
  }

  void SetSuggestedColour() {
    Color suggestedChipColour = suggestedColours[suggestedColourIndex];
    suggestedChipColour.a = 1;
    suggestedColourIndex = (suggestedColourIndex + 1) % suggestedColours.Length;

    float hue;
    float sat;
    float val;
    Color.RGBToHSV(suggestedChipColour, out hue, out sat, out val);
    hueSlider.SetValueWithoutNotify(hue);
    saturationSlider.SetValueWithoutNotify(sat);
    valueSlider.SetValueWithoutNotify(val);
    UpdateColour(suggestedChipColour);
  }

  void UpdateColour(Color chipCol) {
    var cols = chipNameField.colors;
    cols.normalColor = chipCol;
    cols.highlightedColor = chipCol;
    cols.selectedColor = chipCol;
    cols.pressedColor = chipCol;
    chipNameField.colors = cols;

    float luma = chipCol.r * 0.213f + chipCol.g * 0.715f + chipCol.b * 0.072f;
    Color chipNameCol = (luma > textColThreshold) ? Color.black : Color.white;
    chipNameField.textComponent.color = chipNameCol;

    Manager.ActiveChipEditor.chipColour = chipCol;
    Manager.ActiveChipEditor.chipNameColour = chipNameField.textComponent.color;
  }
}
