using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
public class PanelSwapper : NetworkBehaviour
{
    public List<Panel> panels = new List<Panel>();

    [Server]
    public void SwapPanel(string panelName)
    {
        SwapPanelRPC(panelName);
    }


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
    public void QuitGame()
    {
        Application.Quit();
    }
}
