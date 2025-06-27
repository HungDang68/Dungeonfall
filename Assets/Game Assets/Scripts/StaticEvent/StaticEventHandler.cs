using UnityEngine;
using System;

public static class StaticEventHandler
{
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        if (room == null || room.instatiatedRoom == null)
        {
            Debug.LogError("Room or InstantiatedRoom is null when calling OnRoomChanged.");
        }
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }
}
public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}
