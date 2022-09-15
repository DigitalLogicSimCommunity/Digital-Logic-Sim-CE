using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;

public class ImportExport : MonoBehaviour
{
    public static ImportExport instance;
    ChipBarUI chipBar;

    void Awake() { instance = this; }

    void Start() { chipBar = FindObjectOfType<ChipBarUI>(); }

    public void ExportChip(Chip chip)
    {
        string path = StandaloneFileBrowser.SaveFilePanel(
            "Export chip design", "", chip.chipName + ".dls", "dls");
        if (path.Length != 0)
            ChipSaver.Export(chip, path);
    }

    public void ImportChip()
    {
        var extensions = new[] {
      new ExtensionFilter("Chip design", "dls"),
    };

        StandaloneFileBrowser.OpenFilePanelAsync(
            "Import chip design", "", extensions, true, (string[] paths) =>
            {
                if (paths[0] != null && paths[0] != "")
                {
                    ChipLoader.Import(paths[0]);
                    EditChipBar();
                }
            });
    }

    void EditChipBar()
    {
        chipBar.ReloadChipButton();
        SaveSystem.LoadAllChips(Manager.instance);
    }
}
