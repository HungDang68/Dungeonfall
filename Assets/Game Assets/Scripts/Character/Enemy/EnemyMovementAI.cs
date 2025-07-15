using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[DisallowMultipleComponent]
[RequireComponent(typeof(Enemy))]
public class EnemyMovementAI : NetworkBehaviour
{
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;

    void Awake()
    {
        enemy = GetComponent<Enemy>();


    }
    void Start()
    {
        moveSpeed = enemy.stats.GetSpeed();
        Debug.Log("Enemy speed = " + moveSpeed);
        waitForFixedUpdate = new WaitForFixedUpdate();

        playerReferencePosition = GetNearestPLayerPosition();
    }

    private void Update()
    {
        if (!isServer) return;
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        if (!chasePlayer && Vector3.Distance(transform.position, GetNearestPLayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        if (!chasePlayer) { return; }

        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GetNearestPLayerPosition()) > Settings.playerMoveDistanceToRebuildPath))
        {
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCoolDown;

            playerReferencePosition = GetNearestPLayerPosition();

            CreatePath();

            if (movementSteps != null)
            {
                if (moveEnemyRoutine != null)
                {
                    StopCoroutine(moveEnemyRoutine);
                }
            }
            moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
        }
    }
    private Vector3 GetNearestPLayerPosition()
    {
        List<Player> players = GameManager.Instance.GetAllPlayers();

        Vector3 nearestPlayerPosition = players[0].GetPlayerPosition();

        float distance = Vector3.Distance(transform.position, nearestPlayerPosition);

        for (int i = 1; i < players.Count; i++)
        {
            if (distance > Vector3.Distance(transform.position, players[i].GetPlayerPosition()))
            {
                distance = Vector3.Distance(transform.position, players[i].GetPlayerPosition());
                nearestPlayerPosition = players[i].GetPlayerPosition();
            }
        }
        return nearestPlayerPosition;
    }

    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition - transform.position).normalized);

                yield return waitForFixedUpdate;
            }

            yield return waitForFixedUpdate;
        }
    }

    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instatiatedRoom.grid;

        Vector3Int playerGridPosition = GetNearestNonObstaclePosition(currentRoom);

        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        if (movementSteps == null || movementSteps.Count == 0)
        {
            Debug.LogWarning("Failed to create a valid path for the enemy.");
            movementSteps = new Stack<Vector3>(); // Initialize an empty stack to avoid null references
        }
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
    }

    private Vector3Int GetNearestNonObstaclePosition(Room currentRoom)
    {
        Vector3 playerPosition = GetNearestPLayerPosition();

        Vector3Int playerCellPosition = currentRoom.instatiatedRoom.grid.WorldToCell(playerPosition);

        Vector2Int adjustedPlayerCellPostion = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x, playerCellPosition.y - currentRoom.templateLowerBounds.y);

        int obstacle = currentRoom.instatiatedRoom.aStarMovementPenalty[adjustedPlayerCellPostion.x, adjustedPlayerCellPostion.y];

        if (obstacle != 0)
        {
            return playerCellPosition;
        }
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) { continue; }

                    try
                    {
                        obstacle = currentRoom.instatiatedRoom.aStarMovementPenalty[adjustedPlayerCellPostion.x + i, adjustedPlayerCellPostion.y + j];
                        if (obstacle != 0) { return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0); }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return playerCellPosition;
        }
    }

}
