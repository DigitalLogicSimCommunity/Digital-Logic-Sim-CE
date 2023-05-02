using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEditor.UI;
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
            OnEditorModeChage?.Invoke(_chipEditorMode);
        }
    }

    //Event
    public event Action<ChipEditorMode> OnEditorModeChage;
    public event Action<SpawnableChip> customChipCreated;
    public event Action<SpawnableChip> customChipUpdated;
    public event Action OnEditorClear; 
    

    public ChipEditor chipEditorPrefab;
    public Wire wirePrefab;
    public Chip[] SpawnableBuiltinChips;
    public List<SpawnableChip> SpawnableCustomChips;
    
    [FormerlySerializedAs("UIManager")] public MenuManager menuManager;

    private ChipEditor activeChipEditor;
    //Interaction Access
    public static ChipEditor ActiveChipEditor => instance.activeChipEditor;
    public static PinAndWireInteraction PinAndWireInteraction => instance.activeChipEditor.pinAndWireInteraction;
    public static ChipInteraction ChipInteraction => instance.activeChipEditor.chipInteraction;
    public static ChipInterfaceEditor InputsEditor => instance.activeChipEditor.inputsEditor;
    public static ChipInterfaceEditor OutputsEditor => instance.activeChipEditor.outputsEditor;

    void Awake()
    {
        instance = this;
        SaveSystem.Init();
        FolderSystem.Init();
    }

    void Start()
    {
        SpawnableCustomChips = new List<SpawnableChip>();
        activeChipEditor = FindObjectOfType<ChipEditor>();
        SaveSystem.LoadAllChips(this);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Y)) return;

        Pin[] unconnectedInputs =
            activeChipEditor.chipInteraction.UnconnectedInputPins;
        Pin[] unconnectedOutputs =
            activeChipEditor.chipInteraction.UnconnectedOutputPins;
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
            if (chip.chipName == prefab.chipName)
                return prefab;

        return SpawnableCustomChips.FirstOrDefault(prefab => chip.chipName == prefab.chipName);
    }

    public static Chip GetChipByName(string name)
    {
        return instance.SpawnableCustomChips.FirstOrDefault(chip => name == chip.chipName);
    }

    public Chip LoadCustomChip(ChipInstanceHolder instanceHolder)
    {
        if (instanceHolder == null) return null;

        activeChipEditor.Data = instanceHolder.Data;
        ScalingManager.i.SetScale(instanceHolder.Data.scale);
        ChipEditorOptions.instance.SetUIValues(activeChipEditor);

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
        ChipInstanceHolder chipInstanceHolder = ChipLoader.GetChipInstanceData(chip, activeChipEditor);
        ActiveChipEditor.Data = chipInstanceHolder.Data;

        menuManager.SetEditingChipName(chipInstanceHolder.Data.name);
        ScalingManager.i.SetScale(chipInstanceHolder.Data.scale);
        ChipEditorOptions.instance.SetUIValues(activeChipEditor);
    }

    public void SaveAndPackageChip()
    {
        ChipSaver.Save(activeChipEditor);
        PackageCustomChip();
        ClearEditor();
    }

    public void UpdateChip()
    {
        SpawnableChip updatedChip =
            ChipPackageSpawner.i.TryPackageAndReplaceChip(SpawnableCustomChips, activeChipEditor.Data.name);
        customChipUpdated?.Invoke(updatedChip);
        ChipSaver.Update(activeChipEditor, updatedChip);
        ChipEditorMode = ChipEditorMode.Create;
        ClearEditor();
        menuManager.SetEditingChipName("");
    }

    internal void DeleteChip(string nameBeforeChanging)
    {
        SpawnableCustomChips = SpawnableCustomChips.Where(x => !string.Equals(x.chipName, nameBeforeChanging)).ToList();
    }

    internal void RenameChip(string nameBeforeChanging, string nameAfterChanging)
    {
        SpawnableCustomChips.First(x => string.Equals(x.chipName, nameBeforeChanging)).chipName = nameAfterChanging;
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
        if (activeChipEditor)
        {
            // if (activeChipEditor.gameObject.transform.childCount > 2)
            // {
            //     var ImplementationHolder = activeChipEditor.gameObject.transform.GetChild(2).gameObject;
            //     Destroy(ImplementationHolder);
            // }
            //
            // activeChipEditor.chipImplementationHolder = Instantiate(ImplamentationHolder, activeChipEditor.gameObject.transform).transform;
            // activeChipEditor.Data = new ChipData();

            Destroy(activeChipEditor.gameObject);
        }

        activeChipEditor = Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);

        activeChipEditor.inputsEditor.CurrentEditor = activeChipEditor;
        activeChipEditor.outputsEditor.CurrentEditor = activeChipEditor;

        OnEditorClear?.Invoke();
        ChipEditorOptions.instance.SetUIValues(activeChipEditor);

    }


    public void ChipButtonHanderl(Chip chip)
    {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        activeChipEditor.chipInteraction.ChipButtonInteraction(chip);
    }

    public void LoadMainMenu()
    {
        if (ChipEditorMode == ChipEditorMode.Update)
        {
            ChipEditorMode = ChipEditorMode.Create;
            ClearEditor();
        }
        else
        {
            FolderSystem.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public List<string> AllChipNames(bool builtin = true, bool custom = true)
    {
        List<string> allChipNames = new List<string>();

        if (builtin) allChipNames.AddRange(SpawnableBuiltinChips.Select(chip => chip.chipName));
        if (custom) allChipNames.AddRange(SpawnableCustomChips.Select(chip => chip.chipName));

        return allChipNames;
    }

    public Dictionary<string, Chip> AllSpawnableChipDic()
    {
        var allChips = new List<Chip>(SpawnableBuiltinChips);
        allChips.AddRange(SpawnableCustomChips);

        return allChips.ToDictionary(chip => chip.chipName);
    }

    public void ChangeFolderToChip(string ChipName, int index)
    {
        if (SpawnableCustomChips.First(x => string.Equals(x.name, ChipName)) is CustomChip customChip)
            customChip.FolderIndex = index;
        ChipSaver.ChangeFolder(ChipName, index);
    }
}