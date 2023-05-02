using System.Collections;
using System.Collections.Generic;
using Core;
using DLS.Simulation;
using UnityEngine;
using TMPro;
public class Clock : BuiltinChip
{
    
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Miscellaneous;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new Color(185, 62, 62, 255),
            OverrideWidthAndHeight = true,
            Height = 1.2f,
            Width = 1.2f
        };
        inputPins = new List<Pin>(0);
        outputPins = new List<Pin>(1);
        chipName = "CLOCK";
    }

    
    private WaitForSeconds Waiter;
    [SerializeField]
    private float _hz = 1f;
    public float Hz
    {
        get => _hz;
        set
        {
            _hz = value;
            HzThext.text = $"{_hz}Hz";
            Waiter = new WaitForSeconds((1 / Hz) / 2);
            StopAllCoroutines();
            StartCoroutine(ClockTick());
        }
    }

    [SerializeField]
    private TMP_Text HzThext;
    [SerializeField]
    private GameObject HzEditor;
    protected override void Start()
    {
        base.Start();
        HzThext.text = $"{_hz}Hz";
        StartCoroutine(ClockTick());
        Waiter = new WaitForSeconds((1 / Hz) / 2);
    }
    protected override void ProcessOutput()
    {

    }
    private IEnumerator ClockTick()
    {
        yield return Waiter;
        outputPins[0].ReceiveOne();
        yield return Waiter;
        outputPins[0].ReceiveZero();
        StartCoroutine(ClockTick());
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            MenuManager.instance.OpenMenu(MenuType.ClockMenu);
    }
}
