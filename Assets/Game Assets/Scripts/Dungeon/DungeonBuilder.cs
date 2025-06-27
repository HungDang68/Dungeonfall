using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine.Tilemaps;
using Mirror.BouncyCastle.Asn1.Misc;
using Unity.VisualScripting;
using Unity.Collections;

public class DungeonBuilder : SingletomMonobehavior<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

        // GameResources.Instance.dimedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevel currentDungeonLevel)
    {
        if (!NetworkServer.active)
        {
            Debug.Log("Only the host can generate the dungeon.");
            return false;
        }
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        LoadRoomTemplatesSIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttemps = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttemps < Settings.maxDungeonBuildAttemps)
        {
            dungeonBuildAttemps++;

            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttempForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (!dungeonBuildSuccessful && dungeonRebuildAttempForNodeGraph <= Settings.maxDungeonRebuildAttempsForRoomGraph)
            {
                ClearDungeon();

                dungeonRebuildAttempForNodeGraph++;

                dungeonBuildSuccessful = AttempToBuildRandomDungeon(roomNodeGraph);
            }

            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameObject();
            }
        }

        return dungeonBuildSuccessful;
    }

    private void InstantiateRoomGameObject()
    {
        foreach (KeyValuePair<string, Room> keyvaluePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluePair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            instantiatedRoom.Initialise(roomGameObject);

            room.instatiatedRoom = instantiatedRoom;
        }
    }

    private bool AttempToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance node");
            return false;
        }

        bool noRoomOverLaps = true;

        noRoomOverLaps = ProcessRooomInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverLaps);

        if (openRoomNodeQueue.Count == 0 && noRoomOverLaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ProcessRooomInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverLaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverLaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childeRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childeRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(roomNode.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverLaps = CanPlaceRoomWithNoOverLaps(roomNode, parentRoom);
            }

        }
        return noRoomOverLaps;
    }

    private bool CanPlaceRoomWithNoOverLaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }


            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomtemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomtemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }
        return true;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorWay(doorwayParent, room.doorwayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }

        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjusment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjusment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjusment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjusment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjusment = new Vector2Int(1, 0);
                break;
            default:
                break;
        }

        room.lowerBounds = parentDoorwayPosition + adjusment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverLap(room);

        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isUnavailable = true;
            doorway.isConnected = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;

            return false;
        }
    }

    private Room CheckForRoomOverLap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    private bool IsOverLappingRoom(Room roomToTest, Room room)
    {
        bool isOverLappingX = IsOverLappingInterval(roomToTest.lowerBounds.x, roomToTest.upperBounds.x, room.lowerBounds.x, room.upperBounds.x);

        bool isOverLappingY = IsOverLappingInterval(roomToTest.lowerBounds.y, roomToTest.upperBounds.y, room.lowerBounds.y, room.upperBounds.y);

        if (isOverLappingX && isOverLappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsOverLappingInterval(int x1, int x2, int x3, int x4)
    {
        if (Mathf.Max(x1, x3) <= Mathf.Min(x2, x4))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Doorway GetOppositeDoorWay(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomtemplate;
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.enemiesByLevelList = roomTemplate.enemysByLevelList;
        room.roomLevelEnemySpawnParameterList = roomTemplate.roomEnemySpawnParameterList;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorWayList(roomTemplate.doorwayList);


        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPrevouslyVisited = true;

            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
        if (room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0)
        {
            room.isClearedOfEnermies = true;
        }
        return room;
    }
    private List<Doorway> CopyDoorWayList(List<Doorway> oldDoorWayList)
    {
        List<Doorway> newDoorWayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorWayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorWayList.Add(newDoorway);
        }

        return newDoorWayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach (string stringVlaue in oldStringList)
        {
            newStringList.Add(stringVlaue);
        }

        return newStringList;
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }
    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instatiatedRoom != null)
                {
                    Destroy(room.instatiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];

        }
        else
        {
            Debug.Log("No Room node graph in list");
            return null;
        }
    }

    private void LoadRoomTemplatesSIntoDictionary()
    {
        roomTemplateDictionary.Clear();


        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Tempplate Key In " + roomTemplateList);
            }
        }
    }
}
