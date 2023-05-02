using System.Collections.Generic;
using Core;
using DLS.Simulation;
using UnityEngine;
using SFB;

public class EEPROM : BuiltinChip
{
	public static byte[] contents;

	public Pin writePin;

	public bool autoSaveAndLoad = false;

	
	public override void Init()
	{
		base.Init();
		ChipType = ChipType.Miscellaneous;
		PackageGraphicData = new PackageGraphicData()
		{
			PackageColour = new Color(127, 127, 127, 255),
			NameTextColor = new Color(237, 141, 255,1)
		};
		inputPins = new List<Pin>(13);
		outputPins = new List<Pin>(8);
		chipName = "HARD DRIVE";
	}

	protected override void Awake()
	{
		base.Awake();
		contents = new byte[((long)1 << 16) * 2];
		// Debug.Log("EEPROM contents " + contents.Length);
		if (autoSaveAndLoad)
			SaveSystem.LoadEEPROMContents().CopyTo(contents, 0);
	}

	protected override void Start()
	{
		/*
		var _addrBusSize = addrBusSize;
		var _dataBusSize = dataBusSize;

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
		Destroy(dataOutPinPrefab.gameObject);*/

	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
			MenuManager.instance.OpenMenu(MenuType.EEPROMMenu);
	}

	public void OpenAndFlashBinary()
	{
		var extensions = new[] {
			new ExtensionFilter("Binary file", "bin"),
		};

		StandaloneFileBrowser.OpenFilePanelAsync(
		  "Open binary file", "", extensions, true, (string[] paths) =>
		  {
			  if (paths.Length > 0 && paths[0] != null && paths[0] != "")
			  {
				  FlashBinary(paths[0]);
			  }
		  });
	}

	public void FlashBinary(string path)
	{
		byte[] bytes = System.IO.File.ReadAllBytes(path);
		for(int i = 0; i < System.Math.Min(bytes.Length, contents.Length); i++) contents[i] = bytes[i];
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
			  if (!string.IsNullOrEmpty(path))
			  {
				  System.IO.File.WriteAllBytes(path, contents);
			  }
		  });
	}

	protected override void ProcessOutput()
	{
		PinStates address = inputPins[1].State;
		uint index = address.ToUInt() * 2;
		uint data = (uint)(contents[index] << 8 | contents[index + 1]);

		//reading
		outputPins[0].ReceiveSignal(new PinStates(data));

		if (inputPins[0].State[0] !=PinState.HIGH) return;
		//writing
		uint newData = inputPins[2].State.ToUInt();

		if (newData == data) return;
		
		contents[index] = (byte)(newData >> 8);
		contents[index + 1] = (byte)(newData & 0xFF);
		if (autoSaveAndLoad)
			SaveSystem.SaveEEPROMContents(contents);
	}
}
