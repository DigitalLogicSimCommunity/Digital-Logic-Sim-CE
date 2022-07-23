using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Clock : BuiltinChip
{
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
        outputPins[0].ReceiveSignal(1);
        yield return Waiter;
        outputPins[0].ReceiveSignal(0);
        StartCoroutine(ClockTick());
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            UIManager.instance.OpenMenu(MenuType.ClockMenu);
    }
}
