using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Modules.ProjectSettings;
using Modules.Save_System.Save;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public enum ChipEditorMode
{
    Create,
    Update
};

public class Manager : MonoBehaviour
{

    public static Manager instance;
    private ChipEditorMode _chipEditorMode;

    private  ChipEditorMode ChipEditorMode
    {
        get => _chipEditorMode;
        set
        {
            _chipEditorMode = value;
            OnEditorModeChange?.Invoke(_chipEditorMode);
        }
    }

    //Event
    public event Action<ChipEditorMode> OnEditorModeChange;
    public event Action<SpawnableChip> customChipCreated;
    public event Action<SpawnableChip> customChipUpdated;
    public event Action OnEditorClear; 
    

    public ChipEditor chipEditorPrefab;
    public Wire wirePrefab;
    public Chip[] SpawnableBuiltinChips;
    public List<SpawnableChip> SpawnableCustomChips;
    
    [FormerlySerializedAs("UIManager")] public MenuManager menuManager;

    private ChipEditor activeEditor;
    //Interaction Access
    public static ChipEditor ActiveEditor => instance.activeEditor;
    public static PinAndWireInteraction PinAndWireInteraction => instance.activeEditor.pinAndWireInteraction;
    public static ChipInteraction ChipInteraction => instance.activeEditor.chipInteraction;
    public static ChipInterfaceEditor InputsEditor => instance.activeEditor.inputsEditor;
    public static ChipInterfaceEditor OutputsEditor => instance.activeEditor.outputsEditor;

    void Awake()
    {
        instance = this;
        SaveSystem.Init();
        ProjectSettings.FolderSystem.Init();
    }

    void Start()
    {
        SpawnableCustomChips = new List<SpawnableChip>();
        activeEditor = FindObjectOfType<ChipEditor>();
        SaveSystem.LoadAllChips(this);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Y)) return;

        Pin[] unconnectedInputs =
            activeEditor.chipInteraction.UnconnectedInputPins;
        Pin[] unconnectedOutputs =
            activeEditor.chipInteraction.UnconnectedOutputPins;
        if (unconnectedInputs.Length > 0)
        {
            Debug.Log("Found " + unconnectedInputs.Length +
                      " unconnected input pins!");
        }

        if (unconnectedOutputs.Length > 0)
        {
            Debug.Log("Found " + unconnectedOutputs.Length +
                      " unconnected output pins!");
        }
    }



    public Chip GetChipPrefab(Chip chip)
    {
        foreach (Chip prefab in SpawnableBuiltinChips)
            if (chip.Name == prefab.Name)
                return prefab;

        return SpawnableCustomChips.FirstOrDefault(prefab => chip.Name == prefab.Name);
    }

    public static Chip GetChipByName(string name)
    {
        return instance.SpawnableCustomChips.FirstOrDefault(chip => name == chip.Name);
    }

    public Chip LoadCustomChip(ChipInstanceHolder instanceHolder)
    {
        if (instanceHolder == null) return null;

        activeEditor.CurrentChip = instanceHolder.Info;
        ScalingManager.i.SetScale(instanceHolder.Info.scale);
        ChipEditorOptions.instance.SetUIValues(activeEditor);

        Chip loadedChip = PackageCustomChip();
        if (loadedChip is CustomChip custom)
            custom.ApplyWireModes();

        ClearEditor();
        return loadedChip;
    }

    public void ViewChip(Chip chip)
    {
        ClearEditor();
        ChipEditorMode = ChipEditorMode.Update;
        ChipInstanceHolder chipInstanceHolder = ChipLoader.GetChipInstanceData(chip.Name, activeEditor);
        ActiveEditor.CurrentChip = chipInstanceHolder.Info;

       ChipBarUI.instance.DeactivateUnsafeToPalaceChip(chip.Name);


        menuManager.SetEditingChipName(chipInstanceHolder.Info.name);
        ScalingManager.i.SetScale(chipInstanceHolder.Info.scale);
        ChipEditorOptions.instance.SetUIValues(activeEditor);
    }

    public void SaveAndPackageChip()
    {
        ChipSaver.Save(activeEditor);
        PackageCustomChip();
        ClearEditor();
    }

    public void UpdateChip()
    {
        SpawnableChip updatedChip =
            ChipPackageSpawner.i.TryPackageAndReplaceChip(SpawnableCustomChips, activeEditor.CurrentChip.name);
        customChipUpdated?.Invoke(updatedChip);
        ChipSaver.Update(activeEditor, updatedChip);
        ChipEditorMode = ChipEditorMode.Create;
        ClearEditor();
        menuManager.SetEditingChipName("");
    }

    internal void DeleteChip(string nameBeforeChanging)
    {
        SpawnableCustomChips = SpawnableCustomChips.Where(x => !string.Equals(x.Name, nameBeforeChanging)).ToList();
    }

    internal void RenameChip(string nameBeforeChanging, string nameAfterChanging)
    {
        SpawnableCustomChips.First(x => string.Equals(x.Name, nameBeforeChanging)).Name = nameAfterChanging;
    }


    //Generate Package from current editing chip
    SpawnableChip PackageCustomChip()
    {
        var customChip = ChipPackageSpawner.i.GenerateCustomPackageAndChip();

        customChipCreated?.Invoke(customChip);
        SpawnableCustomChips.Add(customChip);
        return customChip;
    }


    public void ResetEditor()
    {
        ChipEditorMode = ChipEditorMode.Create;
        ClearEditor();
    }

    public GameObject ImplamentationHolder;

    private void ClearEditor()
    {
        //TODO: Don't destroy all the editor
        if (activeEditor)
        {
            // if (activeChipEditor.gameObject.transform.childCount > 2)
            // {
            //     var ImplementationHolder = activeChipEditor.gameObject.transform.GetChild(2).gameObject;
            //     Destroy(ImplementationHolder);
            // }
            //
            // activeChipEditor.chipImplementationHolder = Instantiate(ImplamentationHolder, activeChipEditor.gameObject.transform).transform;
            // activeChipEditor.Data = new ChipData();

            Destroy(activeEditor.gameObject);
        }

        activeEditor = Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);

        activeEditor.inputsEditor.CurrentEditor = activeEditor;
        activeEditor.outputsEditor.CurrentEditor = activeEditor;

        OnEditorClear?.Invoke();
        ChipEditorOptions.instance.SetUIValues(activeEditor);
        ChipBarUI.instance.ActivateAllButton();

    }


    public void ChipButtonHandler(Chip chip)
    {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        activeEditor.chipInteraction.ChipButtonInteraction(chip);
    }

    public void ExitButton()
    {
        if (ChipEditorMode == ChipEditorMode.Update)
        {
            ChipEditorMode = ChipEditorMode.Create;
            ClearEditor();
        }
        else
        {
            ProjectSettings.FolderSystem.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public List<string> AllChipNames(bool builtin = true, bool custom = true)
    {
        List<string> allChipNames = new List<string>();

        if (builtin) allChipNames.AddRange(SpawnableBuiltinChips.Select(chip => chip.Name));
        if (custom) allChipNames.AddRange(SpawnableCustomChips.Select(chip => chip.Name));

        return allChipNames;
    }

    public Dictionary<string, Chip> AllSpawnableChipDic()
    {
        var allChips = new List<Chip>(SpawnableBuiltinChips);
        allChips.AddRange(SpawnableCustomChips);

        return allChips.ToDictionary(chip => chip.Name);
    }

    public void ChangeFolderToChip(string ChipName, int index)
    {
        if (SpawnableCustomChips.First(x => string.Equals(x.name, ChipName)) is CustomChip customChip)
            customChip.FolderIndex = index;
        ChipSaver.ChangeFolder(ChipName, index);
    }
}