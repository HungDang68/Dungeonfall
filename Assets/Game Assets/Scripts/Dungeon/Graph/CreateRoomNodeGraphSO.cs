using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "RoomNodeGraphGenerator", menuName = "Dungeon/NodeGraph/RoomNodeGraphGenerator")]
public class RoomNodeGraphGeneratorSO : ScriptableObject
{
    [Header("Room Type Counts")]
    [Tooltip("Number of small rooms to generate.")]
    public int numberOfSmallRooms = 3;

    [Tooltip("Number of medium rooms to generate.")]
    public int numberOfMediumRooms = 2;

    [Tooltip("Number of large rooms to generate.")]
    public int numberOfLargeRooms = 1;

    [Tooltip("Number of chest rooms to generate.")]
    public int numberOfChestRooms = 1;

    [Tooltip("Room Node Type List (must be assigned).")]
    public RoomNodeTypeListSO roomNodeTypeList;

    [Tooltip("Generated Room Node Graph.")]
    public RoomNodeGraphSO generatedRoomNodeGraph;

    public void GenerateRoomNodeGraph()
    {
        if (roomNodeTypeList == null)
        {
            Debug.LogError("RoomNodeTypeListSO is not assigned!");
            return;
        }

        // Create a new RoomNodeGraphSO instance
        generatedRoomNodeGraph = ScriptableObject.CreateInstance<RoomNodeGraphSO>();
        generatedRoomNodeGraph.roomNodeTypeList = roomNodeTypeList;

        // Create entrance node
        RoomNodeSO entranceNode = CreateRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance), Vector2.zero);
        generatedRoomNodeGraph.roomNodeList.Add(entranceNode);

        // Create room nodes for each type
        List<RoomNodeSO> allRooms = new List<RoomNodeSO>();
        allRooms.AddRange(CreateRooms(roomNodeTypeList.list.Find(x => x.roomNodeTypeName == "SmallRoom"), numberOfSmallRooms, new Vector2(200, 200)));
        allRooms.AddRange(CreateRooms(roomNodeTypeList.list.Find(x => x.roomNodeTypeName == "MediumRoom"), numberOfMediumRooms, new Vector2(400, 400)));
        allRooms.AddRange(CreateRooms(roomNodeTypeList.list.Find(x => x.roomNodeTypeName == "LargeRoom"), numberOfLargeRooms, new Vector2(600, 600)));
        allRooms.AddRange(CreateRooms(roomNodeTypeList.list.Find(x => x.roomNodeTypeName == "ChestRoom"), numberOfChestRooms, new Vector2(800, 800)));

        // Create boss room
        RoomNodeSO bossRoom = CreateRoomNode(roomNodeTypeList.list.Find(x => x.isBossRoom), new Vector2(1000, 1000));
        generatedRoomNodeGraph.roomNodeList.Add(bossRoom);

        // Connect rooms with corridors
        ConnectRooms(entranceNode, allRooms, bossRoom);

        // Validate and load the dictionary
        generatedRoomNodeGraph.OnValidate();

        // Save the generated graph as an asset
        SaveGeneratedGraph();
    }

    private List<RoomNodeSO> CreateRooms(RoomNodeTypeSO roomNodeType, int count, Vector2 startPosition)
    {
        List<RoomNodeSO> rooms = new List<RoomNodeSO>();
        for (int i = 0; i < count; i++)
        {
            Vector2 position = startPosition + new Vector2(i * 200, 0); // Offset each room
            RoomNodeSO roomNode = CreateRoomNode(roomNodeType, position);
            generatedRoomNodeGraph.roomNodeList.Add(roomNode);
            rooms.Add(roomNode);
        }
        return rooms;
    }

    private RoomNodeSO CreateRoomNode(RoomNodeTypeSO roomNodeType, Vector2 position)
    {
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        roomNode.Initialise(new Rect(position, new Vector2(140, 60)), generatedRoomNodeGraph, roomNodeType);
        return roomNode;
    }

    private void ConnectRooms(RoomNodeSO entranceNode, List<RoomNodeSO> allRooms, RoomNodeSO bossRoom)
    {
        RoomNodeSO previousNode = entranceNode;

        // Connect entrance to the first room
        if (allRooms.Count > 0)
        {
            RoomNodeSO firstRoom = allRooms[0];
            CreateCorridorBetweenNodes(previousNode, firstRoom);
            previousNode = firstRoom;
        }

        // Connect all rooms sequentially
        for (int i = 1; i < allRooms.Count; i++)
        {
            RoomNodeSO currentRoom = allRooms[i];
            CreateCorridorBetweenNodes(previousNode, currentRoom);
            previousNode = currentRoom;
        }

        // Connect the last room to the boss room
        CreateCorridorBetweenNodes(previousNode, bossRoom);
    }

    private void CreateCorridorBetweenNodes(RoomNodeSO parentNode, RoomNodeSO childNode)
    {
        RoomNodeSO corridorNode = CreateRoomNode(roomNodeTypeList.list.Find(x => x.isCorridor), (parentNode.rect.position + childNode.rect.position) / 2);
        generatedRoomNodeGraph.roomNodeList.Add(corridorNode);

        parentNode.AddChildRoomNodeIDToRoomNode(corridorNode.id);
        corridorNode.AddParentRoomNodeIDToRoomNode(parentNode.id);

        corridorNode.AddChildRoomNodeIDToRoomNode(childNode.id);
        childNode.AddParentRoomNodeIDToRoomNode(corridorNode.id);
    }

    private void SaveGeneratedGraph()
    {
        string path = "Assets/GeneratedRoomNodeGraph.asset";
        AssetDatabase.CreateAsset(generatedRoomNodeGraph, path);

        foreach (RoomNodeSO roomNode in generatedRoomNodeGraph.roomNodeList)
        {
            AssetDatabase.AddObjectToAsset(roomNode, generatedRoomNodeGraph);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Room Node Graph generated and saved at {path}");
    }
}