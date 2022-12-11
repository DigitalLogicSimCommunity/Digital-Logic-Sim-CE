using System.Collections.Generic;
using UnityEngine;
using SFB;

public class EEPROM : BuiltinChip
{
    public static byte[] contents;

    public Pin writePin;

    public int addrBusSize = 2;
    public Pin addrPinPrefab;
    public int dataBusSize = 2;
    public Pin dataInPinPrefab;
    public Pin dataOutPinPrefab;

    public bool autoSaveAndLoad = false;

    private float pinSpacing = 0.15f;
    private float busSpacing = 0.2f;

    protected override void Awake()
    {
        base.Awake();
        contents = new byte[((long)1 << (addrBusSize * 8)) * dataBusSize];
        // Debug.Log("EEPROM contents " + contents.Length);
        if (autoSaveAndLoad)
            SaveSystem.LoadEEPROMContents().CopyTo(contents, 0);
    }

    protected override void Start()
    {

        var _addrBusSize = addrBusSize * 8;
        var _dataBusSize = dataBusSize * 8;

        var package = GetComponent<ChipPackage>();
        var xoffset = 1f;
        var yoffset = (busSpacing * 4 + (_addrBusSize + _dataBusSize) * pinSpacing) / 2f;
        if (package != null)
        {
            package.override_width_and_height = true;
            xoffset = package.override_width / 2f;
            package.override_height = yoffset * 2f;
            package.SetSizeAndSpacing(this);
        }
        yoffset -= busSpacing;

        inputPins = new Pin[_addrBusSize + _dataBusSize + 1];
        inputPins[0] = Instantiate(writePin, transform);
        inputPins[0].transform.localPosition = new Vector3(-xoffset, yoffset, 0);

        yoffset -= busSpacing + pinSpacing;

        for (int i = 0; i < _addrBusSize; i++)
        {
            var nextPin = Instantiate(addrPinPrefab, transform);
            nextPin.transform.localPosition = new Vector3(-xoffset, yoffset - i * pinSpacing, 0);
            nextPin.pinName += i.ToString("X");
            inputPins[i + 1] = nextPin;
        }

        yoffset -= busSpacing + 16 * pinSpacing;

        for (int i = 0; i < _dataBusSize; i++)
        {
            var nextPin = Instantiate(dataInPinPrefab, transform);
            nextPin.transform.localPosition = new Vector3(-xoffset, yoffset - i * pinSpacing, 0);
            nextPin.pinName += i.ToString("X");
            inputPins[i + _addrBusSize + 1] = nextPin;
        }



        outputPins = new Pin[_dataBusSize];
        for (int i = 0; i < _dataBusSize; i++)
        {
            var nextPin = Instantiate(dataOutPinPrefab, transform);
            nextPin.transform.localPosition = new Vector3(xoffset, yoffset - i * pinSpacing, 0);
            nextPin.pinName += i.ToString("X");
            outputPins[i] = nextPin;
        }

        Destroy(writePin.gameObject);
        Destroy(addrPinPrefab.gameObject);
        Destroy(dataInPinPrefab.gameObject);
        Destroy(dataOutPinPrefab.gameObject);

    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            UIManager.instance.OpenMenu(MenuType.EEPROMMenu);
    }

    public void OpenAndFlashBinary()
    {
        Debug.Log("OpenAndFlashBinary");
        var extensions = new[] {
        new ExtensionFilter("Binary file", "bin"),
      };

        StandaloneFileBrowser.OpenFilePanelAsync(
          "Open binary file", "", extensions, true, (string[] paths) =>
          {
              if (paths[0] != null && paths[0] != "")
              {
                  FlashBinary(paths[0]);
              }
          });
    }

    public void FlashBinary(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
        contents = bytes;
        SaveSystem.SaveEEPROMContents(contents);
    }

    public void DumpBinary()
    {
        var extensions = new[] {
      new ExtensionFilter("Binary file", "bin"),
    };

        StandaloneFileBrowser.SaveFilePanelAsync(
          "Save binary file", "", "", extensions, (string path) =>
          {
              if (path != null && path != "")
              {
                  System.IO.File.WriteAllBytes(path, contents);
              }
          });
    }

    protected override void ProcessOutput()
    {
        int address = 0;
        for (int i = 0; i < (addrBusSize * 8); i++)
        {
            address <<= 1;
            address += inputPins[i + 1].State;
        }
        int index = address * dataBusSize;
        int data = 0;
        try
        {
            for (int i = 0; i < dataBusSize; i++)
            {
                data <<= 8;
                data += contents[index + i];
                // Debug.Log("EEPROM read " + contents[index + i] + " at index " + (index + i) + " resulting in data " + data + "");
            }
        }
        catch
        {
            Debug.Log("EEPROM read error at address " + (address));
        }

        //reading
        for (int i = 0; i < outputPins.Length; i++)
        {
            outputPins[i].ReceiveSignal(data & (1<<(outputPins.Length-i-1)));
        }
        if (inputPins[0].State > 0)
        {
            //writing
            int newData = 0;
            for (int i = 0; i < (dataBusSize * 8); i++)
            {
                newData <<= 1;
                newData += inputPins[i + 1 + addrBusSize * 8].State;
            }

            if (newData != data)
            {
                for (int i = dataBusSize - 1; i >= 0; i--)
                {
                    try
                    {
                        contents[index + i] = (byte)newData;
                    }
                    catch
                    {
                        Debug.Log("EEPROM write error at index " + (index + i));
                    }
                    newData >>= 8;
                }
                if (autoSaveAndLoad)
                    SaveSystem.SaveEEPROMContents(contents);
            }
        }
    }
}
