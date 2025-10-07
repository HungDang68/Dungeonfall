using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    [SerializeField] private GameObject defaultPlayerGameplayPrefab; // Add a default prefab reference
    [Scene]
    [SerializeField] private string onlineScene = "";
    public List<Panel> panels = new List<Panel>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void OnSinglePlayButtonClicked()
    {
        if (CustomNetworkManager.singleton.playerPrefab != defaultPlayerGameplayPrefab)
        {
            Debug.LogWarning("playerGameplayPrefab is not assigned. Assigning default prefab.");
            CustomNetworkManager.singleton.playerPrefab = defaultPlayerGameplayPrefab;
        }
        // Set maxConnections to 0 to prevent others from joining
        CustomNetworkManager.singleton.maxConnections = 0; Debug.Log("Single Play: Hosting game with maxConnections set to 0.");

        CustomNetworkManager.singleton.onlineScene = onlineScene;
        // Start hosting the game
        CustomNetworkManager.singleton.StartHost();
    }
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
    public void QuitGame()
    {
        Application.Quit();
    }
}