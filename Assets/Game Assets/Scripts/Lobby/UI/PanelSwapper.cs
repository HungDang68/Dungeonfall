using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PanelSwapper : SingletomMonobehavior<PanelSwapper>
{
    public List<Panel> panels = new List<Panel>();

    [Server]
    public void SwapPanel(string panelName)
    {
        SwapPanelRPC(panelName);
    }

    [ClientRpc]
    private void SwapPanelRPC(string panelName)
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
