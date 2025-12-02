using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Resources.Scripts; 

public class SmartAStarChaser : NetworkBehaviour
{
    [Header("A* Chaser Settings")]
    public float moveSpeed = 4f;
    public float chaseSpeed = 5f;
    public float recalculatePathInterval = 0.5f;
    public float catchDistance = 0.5f;
    public float pathFollowingDistance = 0.3f;
    public float catchCooldown = 2f;
    private float nextCatchTime = 0f;
    
    [Header("Vision Settings")]
    public float visionRange = 8f;
    public float visionAngle = 180f;
    public float loseSightRange = 12f;
    public float directDetectionRange = 7f;
    public LayerMask wallLayer;
    
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolPointDistance = 6f;
    public float idleTime = 2f;
    public float patrolRecalculateInterval = 2f;
    
    private Transform currentTarget;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private float lastPathRecalculationTime;
    private float lastPatrolPathRecalculationTime;
    private bool seesPlayer;
    
    // Movement
    private Vector2 currentVelocity;
    public float smoothTime = 0.1f;
    
    // Patrol state
    private enum ChaserState { Patrolling, Chasing, Returning }
    private ChaserState currentState = ChaserState.Patrolling;
    private Vector3 patrolStartPosition;
    private Vector3 currentPatrolTarget;
    private float idleTimer = 0f;
    private bool isIdle = false;

    // UI references
    private GameObject loseUI;
    private GameObject mainMenuUI;
    private TextMeshProUGUI deadText;
    private UnityEngine.UI.Button menuButton;
    private UnityEngine.UI.Button quitButton;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            enabled = false;
            return;
        }
        
        currentPath = new List<Vector3>();
        patrolStartPosition = transform.position;
        currentState = ChaserState.Patrolling;
        
        // Set initial patrol target
        SetRandomPatrolTarget(); 
    } 
    
    void Update()
    {   
        // State machine
        switch (currentState)
        {
            case ChaserState.Patrolling:
                UpdatePatrolling();
                break;
            case ChaserState.Chasing:
                UpdateChasing();
                break;
            case ChaserState.Returning:
                UpdateReturning();
                break;
        } 
    }
    
    void UpdatePatrolling()
    {
        // Check if players are in sight
        CheckForPlayersInSight();
        
        if (isIdle)
        {
            // Resting state
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                isIdle = false;
                SetRandomPatrolTarget();
            }
            return;
        }
        
        // Check if patrol path needs recalculation
        if (currentPath == null || currentPath.Count == 0 || 
            Time.time - lastPatrolPathRecalculationTime > patrolRecalculateInterval)
        {
            RecalculatePatrolPath();
        }
        
        // Follow patrol path
        FollowPatrolPath();
    }
    
    void RecalculatePatrolPath()
    {
        currentPath = AStarPathfinder.Instance.FindPath(transform.position, currentPatrolTarget);
        currentPathIndex = 0;
        lastPatrolPathRecalculationTime = Time.time;
        
        if (currentPath != null && currentPath.Count > 0)
        {
            Debug.Log($"Found patrol path with {currentPath.Count} points");
        }
        else
        {
            Debug.LogWarning("A* cannot find path to patrol target, setting new target");
            SetRandomPatrolTarget();
        }
    }
    
    void FollowPatrolPath()
    {
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            // Recalculate if no path
            RecalculatePatrolPath();
            return;
        }
        
        // Follow path points
        Vector3 targetPosition = currentPath[currentPathIndex];
        
        // Use SmoothDamp for smooth movement
        Vector2 newPosition = Vector2.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime, 
            patrolSpeed
        );
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        
        // Check if reached current path point
        if (Vector2.Distance(transform.position, targetPosition) < pathFollowingDistance)
        {
            currentPathIndex++;
            
            // If reached path end, set new patrol target
            if (currentPathIndex >= currentPath.Count)
            {
                // Reached patrol point, take a rest
                isIdle = true;
                idleTimer = 0f;
                currentPath = new List<Vector3>();
            }
        }
    }
    
    void UpdateChasing()
    {
        // Check if target is lost
        if (!currentTarget || !IsTargetInRange(currentTarget) ||
            (currentTarget.GetComponent<Character>()?.CurrentHealth.Value ?? 0) <= 0)
        {
            currentState = ChaserState.Returning;
            currentTarget = null;
            currentPath = new List<Vector3>();
            return;
        } 
        
        seesPlayer = CheckPlayerVision();
        
        if (Time.time - lastPathRecalculationTime > recalculatePathInterval || 
            (seesPlayer && (currentPath == null || currentPath.Count == 0)))
        {
            RecalculatePath();
            lastPathRecalculationTime = Time.time;
        }
        
        FollowPath();
        CheckCatchPlayer();
    }
    
    void UpdateReturning()
    {
        // Return to start position
        if (currentPath == null || currentPath.Count == 0)
        {
            // Calculate return path
            currentPath = AStarPathfinder.Instance?.FindPath(transform.position, patrolStartPosition);
            currentPathIndex = 0;
        }
        
        if (currentPath != null && currentPath.Count > 0 && currentPathIndex < currentPath.Count)
        {
            // Follow return path
            Vector3 targetPosition = currentPath[currentPathIndex];
            
            Vector2 newPosition = Vector2.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref currentVelocity, 
                smoothTime, 
                moveSpeed
            );
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            
            if (Vector2.Distance(transform.position, targetPosition) < pathFollowingDistance)
            {
                currentPathIndex++;
            }
            
            if (currentPathIndex >= currentPath.Count)
            {
                // Reached start position, start patrolling
                currentState = ChaserState.Patrolling;
                currentPath = new List<Vector3>();
                SetRandomPatrolTarget();
            }
        }
        else
        {
            // Cannot find return path, teleport to start position
            transform.position = patrolStartPosition;
            currentState = ChaserState.Patrolling;
            SetRandomPatrolTarget();
        }
        
        // Still check for players while returning
        CheckForPlayersInSight();
    }
    
    void CheckForPlayersInSight()
    { 
        // Check all players if they are in vision range
        foreach (Character player in Main.Players)
        {
            if (player.CurrentHealth.Value <= 0) continue; // skip dead players
            if (IsTargetInSightRange(player.transform) && CheckSinglePlayerVision(player.transform))
            {
                // Found player, start chasing
                currentTarget = player.transform;
                currentState = ChaserState.Chasing;
                return;
            }
        }
    }
    
    bool IsTargetInRange(Transform target)
    {  
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= loseSightRange;
    }
    
    bool IsTargetInSightRange(Transform target)
    { 
        var character = target.GetComponent<Character>();
        if (character != null && character.CurrentHealth.Value <= 0) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= visionRange;
    }
    
    void SetRandomPatrolTarget()
    {
        // Set random patrol target around start position
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            Vector3 potentialTarget = patrolStartPosition + (Vector3)randomDirection * patrolPointDistance;
            
            // Use raycast to check if target point is walkable (no walls)
            Vector2 direction = potentialTarget - transform.position;
            float distance = direction.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, wallLayer);
            
            if (!hit.collider)
            {
                currentPatrolTarget = potentialTarget;
                return;
            }
        }
        
        // If no suitable point found, use default method
        float randomAngleFinal = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 randomDirectionFinal = new Vector2(Mathf.Cos(randomAngleFinal), Mathf.Sin(randomAngleFinal));
        currentPatrolTarget = patrolStartPosition + (Vector3)randomDirectionFinal * patrolPointDistance;
    }
    
    bool CheckSinglePlayerVision(Transform player)
    { 
        var character = player.GetComponent<Character>();
        if (character != null && character.CurrentHealth.Value <= 0) return false;

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        
        // If within 7 meters, ignore angle check, only check wall obstruction
        if (distance <= directDetectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
            bool canSee = (hit.collider == null);
            return canSee;
        }
        
        // If outside 7m but still in vision range, do full vision check
        if (distance > visionRange) return false;
        
        float angle = Vector2.Angle(transform.up, toPlayer.normalized);
        if (angle > visionAngle / 2) return false;
        
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
        return (!hit2.collider);
    }
     

    void RecalculatePath()
    {
        if (!AStarPathfinder.Instance || !currentTarget) return;
        
        Vector3 targetPosition = seesPlayer ? currentTarget.position : GetStrategicPosition();
        
        currentPath = AStarPathfinder.Instance.FindPath(transform.position, targetPosition);
        currentPathIndex = 0;
        
        if (currentPath != null && currentPath.Count > 0)
        {
            Debug.Log($"Found chase path with {currentPath.Count} points");
        }
        else
        {
            Debug.LogWarning("A* cannot find path to target position");
            currentPath = new List<Vector3>();
        }
    }
    
    Vector3 GetStrategicPosition()
    {
        if (!currentTarget) return transform.position;
        return currentTarget.position;
    }
    
    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            // No path, use simple chasing
            if (seesPlayer && currentTarget != null)
            {
                Vector2 moveDirection = (currentTarget.position - transform.position).normalized;
                transform.position += (Vector3)moveDirection * ((seesPlayer ? chaseSpeed : moveSpeed) * Time.deltaTime);
            }
            else
            {
                // No path and cannot see player, recalculate path
                if (Time.frameCount % 120 == 0) RecalculatePath();
            }
            return;
        }
        
        // Follow path points
        Vector3 targetPosition = currentPath[currentPathIndex];
        
        // Use SmoothDamp for smooth movement
        Vector2 newPosition = Vector2.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime, 
            seesPlayer ? chaseSpeed : moveSpeed
        );
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        
        // Check if reached current path point
        if (Vector2.Distance(transform.position, targetPosition) < pathFollowingDistance)
        {
            currentPathIndex++;
            
            if (currentPathIndex >= currentPath.Count && seesPlayer)
            {
                RecalculatePath();
            }
        }
    }
    
    bool CheckPlayerVision()
    {
        if (!currentTarget) return false;
        
        Vector2 toPlayer = currentTarget.position - transform.position;
        float distance = toPlayer.magnitude;
        
        // If within 7 meters, ignore angle check
        if (distance <= directDetectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
            bool canSee = !hit.collider;
            
            return canSee;
        }
        
        // Normal vision check outside 7m range
        if (distance > visionRange) return false;
        
        // Check vision angle
        float angle = Vector2.Angle(transform.up, toPlayer.normalized);
        if (angle > visionAngle / 2) return false;
        
        // Check if line of sight is blocked
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
        bool canSee2 = (hit2.collider == null);
        
        return canSee2;
    }

    void CheckCatchPlayer(){
        if(!currentTarget) return;

        float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);
        if(distanceToPlayer <= catchDistance && Time.time >= nextCatchTime){
            var obj = currentTarget.GetComponent<NetworkObject>();
            if (obj) CatchPlayerClientRpc(obj.OwnerClientId); // notify the caught player and minus health
            nextCatchTime = Time.time + catchCooldown;
        }
    }

    // all client received the RPC call but only the one with matching player id will deduct health
    [ClientRpc]
    void CatchPlayerClientRpc(ulong playerID) 
    {
        if (NetworkManager.Singleton.LocalClientId != playerID) return;

        var player = NetworkManager.Singleton.LocalClient.PlayerObject; // find the player
        var stats = player.GetComponent<Character>();

        if (player != null && stats != null) stats.ChangeHealthServerRpc(); // minus heatlh
    }

    void OnDrawGizmos()
    {
        if (!IsServer) return;
        
        // Draw vision range
        Gizmos.color = seesPlayer ? Color.red : (currentState == ChaserState.Chasing ? Color.yellow : Color.green);
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        // Draw 7m direct detection range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, directDetectionRange);
        
        // Draw lose sight range
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
        
        // Draw patrol target
        if (currentState == ChaserState.Patrolling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
            Gizmos.DrawSphere(currentPatrolTarget, 0.3f);
        }
        
        // Draw current path
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = currentState == ChaserState.Chasing ? Color.red : 
                          currentState == ChaserState.Patrolling ? Color.green : Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
            
            // Draw current target point
            if (currentPathIndex < currentPath.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(currentPath[currentPathIndex], 0.2f);
            }
        }
    }
}
