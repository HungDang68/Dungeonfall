using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;


public class RoomNoteGraphEditor : EditorWindow
{
    private Vector2 graphOffset;
    private Vector2 graphDrag;
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeList;
    private const float nodeHeight = 60f;
    private const float nodeWidth = 140f;
    private const int roomNodePadding = 20;
    private const int roomNodeBorder = 12;
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;
    private const float gridlarge = 100f;
    private const float gridSmall = 25f;


    private RoomNodeSO currentRoomNode = null;

    [MenuItem("Room Note Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNoteGraphEditor>("Room Node Graph Editor");
    }
    void OnEnable()
    {
        Selection.selectionChanged += InpectorSelectionChanged;
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(roomNodePadding, roomNodePadding, roomNodePadding, roomNodePadding);
        roomNodeStyle.border = new RectOffset(roomNodeBorder, roomNodeBorder, roomNodeBorder, roomNodeBorder);



        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(roomNodePadding, roomNodePadding, roomNodePadding, roomNodePadding);
        roomNodeSelectedStyle.border = new RectOffset(roomNodeBorder, roomNodeBorder, roomNodeBorder, roomNodeBorder);


        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    private void OnDisable()
    {
        Selection.selectionChanged -= InpectorSelectionChanged;
    }



    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int InstanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(InstanceID) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }
        return false;
    }
    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            DrawBackGroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackGroundGrid(gridlarge, 0.2f, Color.gray);


            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomNodeConnection();

            DrawRoomNodes();
        }
        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackGroundGrid(float gridSize, float v, Color color)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(color.r, color.g, color.b, v);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }
        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawlineFrom.rect.center, currentRoomNodeGraph.linePosition,
            currentRoomNodeGraph.roomNodeToDrawlineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);

        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        graphDrag = Vector2.zero;



        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawlineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }

        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawlineFrom != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawlineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawlineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightButtonDownEvent(currentEvent);
        }
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 delta)
    {
        graphDrag = delta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(delta);
        }

        GUI.changed = true;
    }

    private void ProcessRightButtonDownEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawlineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNode();
        }
    }



    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);


        menu.ShowAsContext();
    }

    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);

                    }
                }
            }
        }

        while (roomNodeDeletionQueue.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            DestroyImmediate(roomNodeToDelete, true);

            AssetDatabase.SaveAssets();
        }
    }

    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNode();
    }

    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
        currentRoomNodeGraph.OnValidate();
    }
    private void ClearAllSelectedRoomNode()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }
    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawlineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;

        GUI.changed = true;
    }
    private void DrawRoomNodeConnection()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        Vector2 midPoint = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;

        Vector2 arrowTailPoint1 = midPoint - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPoint + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowHeadPoint = midPoint + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1,
                        arrowHeadPoint, arrowTailPoint1,
                        Color.white, null, connectingLineWidth);

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2,
                        arrowHeadPoint, arrowTailPoint2,
                        Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPosition, endPosition,
                        startPosition, endPosition,
                        Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void InpectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;

            GUI.changed = true;
        }
    }
}
