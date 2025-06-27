using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeSO", menuName = "Dungeon/NodeGraph/RoomNodeTypeSO")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    public bool displayInNodeGraphEditor = true;
    public bool isCorridor;
    public bool isCorridorEW;
    public bool isCorridorNS;
    public bool isEntrance;
    public bool isBossRoom;
    public bool isNone;
    private void OnValidate()
    {   
        HelpfulUtility.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
}
