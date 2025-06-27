using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Dungeon/NodeGraph/RoomNodeGraph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector]
    public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector]
    public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    [HideInInspector] public RoomNodeSO roomNodeToDrawlineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            roomNodeDictionary[roomNode.id] = roomNode;

        }
    }
    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO node in roomNodeList)
        {
            if (node.roomNodeType == roomNodeType)
            {
                return node;
            }
        }
        return null;
    }
    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }


    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childNodeID);
        }
    }
    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawlineFrom = node;
        linePosition = position;
    }
    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }
}
