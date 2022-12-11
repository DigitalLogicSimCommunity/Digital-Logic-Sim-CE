using System.Collections.Generic;
using UnityEngine;

public class EEPROM : BuiltinChip {
  public static Dictionary<string, List<int>> contents;

  public Pin writePin;

  public int addrBusSize = 16;
  public Pin addrPinPrefab;
  public int dataBusSize = 16;
  public Pin dataInPinPrefab;
  public Pin dataOutPinPrefab;

  private float pinSpacing = 0.15f;
  private float busSpacing = 0.2f;

  protected override void Awake() {
    base.Awake();
    contents = SaveSystem.LoadEEPROMContents();
  }

  protected override void Start() {

    var package = GetComponent<ChipPackage>();
    var xoffset = 1f;
    var yoffset = (busSpacing*4 + (addrBusSize + dataBusSize) * pinSpacing) / 2f;
    if (package != null) {
      package.override_width_and_height = true;
      xoffset = package.override_width / 2f;
      package.override_height = yoffset * 2f;
      package.SetSizeAndSpacing(this);
    }
    yoffset -= busSpacing;

    inputPins = new Pin[addrBusSize + dataBusSize + 1];
    inputPins[0] = Instantiate(writePin, transform);
    inputPins[0].transform.localPosition = new Vector3(-xoffset, yoffset, 0);

    yoffset -= busSpacing+pinSpacing;

    for (int i = 0; i < addrBusSize; i++)
    {
      var nextPin = Instantiate(addrPinPrefab, transform);
      nextPin.transform.localPosition = new Vector3(-xoffset, yoffset - i*pinSpacing, 0);
      nextPin.pinName += i.ToString("X");
      inputPins[i+1]=nextPin;
    }

    yoffset -= busSpacing+16*pinSpacing;

    for (int i = 0; i < dataBusSize; i++)
    {
      var nextPin = Instantiate(dataInPinPrefab, transform);
      nextPin.transform.localPosition = new Vector3(-xoffset, yoffset - i*pinSpacing, 0);
      nextPin.pinName += i.ToString("X");
      inputPins[i+addrBusSize+1]=nextPin;
    }



    outputPins = new Pin[dataBusSize];
    for (int i = 0; i < dataBusSize; i++)
    {
      var nextPin = Instantiate(dataOutPinPrefab, transform);
      nextPin.transform.localPosition = new Vector3(xoffset, yoffset - i*pinSpacing, 0);
      nextPin.pinName += i.ToString("X");
      outputPins[i]=nextPin;
    }

    Destroy(writePin.gameObject);
    Destroy(addrPinPrefab.gameObject);
    Destroy(dataInPinPrefab.gameObject);
    Destroy(dataOutPinPrefab.gameObject);

  }

  protected override void ProcessOutput() {
    switch (inputPins[0].State) {
    case 0:
      string binary = "";
      for (int i = 1; i < 5; i++) {
        binary += inputPins[i].State.ToString();
      }
      if (contents.ContainsKey(binary)) {
        for (int i = 0; i < outputPins.Length; i++) {
          outputPins[i].ReceiveSignal(contents[binary][i]);
        }
      } else {
        for (int i = 0; i < outputPins.Length; i++) {
          outputPins[0].ReceiveSignal(0);
        }
      }
      break;
    case 1:
      bool updateFile = false;
      string address = "";
      List<int> store = new List<int>();
      for (int i = 5; i < 13; i++) {
        store.Add(inputPins[i].State);
      }
      for (int i = 1; i < 5; i++) {
        address += inputPins[i].State;
      }
      if (contents.ContainsKey(address)) {
        if (contents[address] != store) {
          updateFile = true;
        }
        contents.Remove(address);
      } else {
        updateFile = true;
      }
      contents.Add(address, store);
      if (updateFile) {
        SaveSystem.SaveEEPROMContents(contents);
      }
      break;
    default:
      foreach (Pin i in outputPins) {
        i.ReceiveSignal(0);
      }
      break;
    }
  }
}
