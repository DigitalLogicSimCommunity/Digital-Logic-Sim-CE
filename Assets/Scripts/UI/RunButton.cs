using UnityEngine;
using UnityEngine.UI;

public class RunButton : MonoBehaviour
{
    public Button button;
    public Simulation sim;
    public Color onCol;
    public Color offCol;

    void Start() { button.targetGraphic.color = sim.active ? onCol : offCol; }

    public void ToggleSimulationActive()
    {
        sim.ToogleActive();
        button.targetGraphic.color = sim.active ? onCol : offCol;
    }

    public void SetOff() { button.targetGraphic.color = offCol; }

    void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
}
