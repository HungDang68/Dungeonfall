using UnityEngine;
using System.Collections.Generic;

public class PanelSwapperOff : MonoBehaviour
{
    public List<Panel> panels = new List<Panel>();
    public void SwapPanel(string panelName)
    {
        foreach (Panel panel in panels)
        {
            if (panel.PanelName == panelName)
            {
                panel.gameObject.SetActive(true);
            }
            else
            {
                panel.gameObject.SetActive(false);
            }
        }
    }
}
