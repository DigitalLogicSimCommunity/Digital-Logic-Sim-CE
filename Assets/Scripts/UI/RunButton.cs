using UnityEngine;
using UnityEngine.UI;
using DLS.Core.Simulation;

public class RunButton : MonoBehaviour
{
    public Button button;
    public Color onCol;
    public Color offCol;
    
    
    private Simulation _simulation;

    void Start()
    {
        _simulation = Simulation.instance;
        button.targetGraphic.color = _simulation.active ? onCol : offCol;
    }

    public void ToggleSimulationActive()
    {
        _simulation.ToggleActive();
        button.targetGraphic.color = _simulation.active ? onCol : offCol;
    }

    public void SetOff() { button.targetGraphic.color = offCol; }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
}
