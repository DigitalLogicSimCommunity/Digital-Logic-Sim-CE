using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavBar : MonoBehaviour
{
    public GameObject[] tabs;
    public int activeTab = 0;

    public void NexTab() {
      Step(1); 
    }

    public void PrevTab() {
      Step(-1); 
    }

    void Step(int step) {
      tabs[activeTab].SetActive(false);
      activeTab = Mathf.Abs((activeTab + step) % tabs.Length);
      tabs[activeTab].SetActive(true);
    }
}
