using UnityEngine;

public static class Settings
{
    public const int maxDungeonRebuildAttempsForRoomGraph = 1000;
    public const int maxDungeonBuildAttemps = 10;

    public const int defaultAStarMovementPenalty = 40;
    public const int preferedPathAStarMovementPenalty = 1;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCoolDown = 2f;

    public const int maxChildCorridors = 3;

    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";

}
