using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Data.Common;
public class PlayerHandler : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    public bool isReady = false;
    public Button readyButton;
    public TextMeshProUGUI nameText;

    void Start()
    {
        readyButton.interactable = isLocalPlayer;
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        readyButton.interactable = true;
        isReady = false;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        LobbyUIManeger.instance.RegisterPlayer(this);
    }
    [Command]
    void CmdSetReady()
    {
        isReady = !isReady;
        OnReadyStatusChanged(!isReady, isReady);
    }
    public void OnReadyButtonClicked()
    {
        CmdSetReady();
    }
    void OnReadyStatusChanged(bool oldValue, bool newValue)
    {
        if (NetworkServer.active)
        {
            LobbyUIManeger.instance.CheckAllPlayerReady();
        }
        if (isReady)
        {
            SetSelectedButtonColor(Color.green);
        }
        else
        {
            SetSelectedButtonColor(Color.white);
        }
    }
    void SetSelectedButtonColor(Color color)
    {
        ColorBlock cb = readyButton.colors;
        cb.normalColor = color;
        cb.selectedColor = color;
        readyButton.colors = cb;
    }


}
