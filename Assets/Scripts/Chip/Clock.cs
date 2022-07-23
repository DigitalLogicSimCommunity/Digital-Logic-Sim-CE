using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Clock : BuiltinChip
{
    [SerializeField]
    private float _hz = 1f;
    public float Hz
    {
        get => _hz;
        set
        {
            _hz = value;
            HzThext.text = $"{_hz}Hz";
            StopAllCoroutines();
            StartCoroutine(ClockTick());
        }
    }
    [SerializeField]
    private TMP_Text HzThext;
    [SerializeField]
    private GameObject HzEditor;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Start()
    {
        StartCoroutine(ClockTick());
    }
    protected override void ProcessOutput()
    {

    }
    private IEnumerator ClockTick()
    {
        yield return new WaitForSeconds(1 / Hz);
        outputPins[0].ReceiveSignal(1);
        yield return new WaitForSeconds(0.01f);
        outputPins[0].ReceiveSignal(0);
        StartCoroutine(ClockTick());
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            UIManager.instance.OpenMenu(MenuType.ClockMenu);
    }
}
