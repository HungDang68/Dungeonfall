using System.Collections.Generic;
using System.Xml.Serialization;
using Mirror.BouncyCastle.Asn1.Misc;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Level")]
public class DungeonLevel : ScriptableObject
{
    public string levelName;

    public List<RoomTemplateSO> roomTemplateList;

    public List<RoomNodeGraphSO> roomNodeGraphList;

    private void OnValidate()
    {
        HelpfulUtility.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelpfulUtility.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
        {
            return;
        }
        if (HelpfulUtility.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
        {
            return;
        }


        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
            {
                return;
            }
            if (roomTemplateSO.roomNodeType.isEntrance)
            {
                isEntrance = true;
            }
            if (roomTemplateSO.roomNodeType.isCorridorEW)
            {
                isEWCorridor = true;
            }
            if (roomTemplateSO.roomNodeType.isCorridorNS)
            {
                isNSCorridor = true;
            }
        }

        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " :No EW corridor");
        }
        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " :No Entrance corridor");
        }
        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " :No NS corridor");
        }


        foreach (RoomNodeGraphSO roomNodeGraphSO in roomNodeGraphList)
        {
            if (roomNodeGraphSO == null)
            {
                return;
            }

            foreach (RoomNodeSO roomNodeSO in roomNodeGraphSO.roomNodeList)
            {
                if (roomNodeSO == null)
                {
                    continue;
                }
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorNS || roomNodeSO.roomNodeType.isCorridorEW
                || roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                {
                    continue;
                }
                bool isROomNodeTypeFound = false;
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                    {
                        continue;
                    }
                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isROomNodeTypeFound = true;
                        break;
                    }

                }

                if (!isROomNodeTypeFound)
                {
                    Debug.Log("In " + this.name.ToString() + "Not Found");
                }
            }
        }
    }
}
