using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Dungeon/NodeGraph/RoomNodeTypeListSO")]
public class RoomNodeTypeListSO : ScriptableObject
{
    [Space(10)]
    public List<RoomNodeTypeSO> list;
    private void OnValidate()
    {
        HelpfulUtility.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
}
