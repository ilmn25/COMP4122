using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

public class SmartAStarChaser : NetworkBehaviour
{
    [Header("A* Chaser Settings")]
    public float moveSpeed = 4f;
    public float chaseSpeed = 5f;
    public float recalculatePathInterval = 0.5f;
    public float catchDistance = 0.5f;
    public float pathFollowingDistance = 0.3f;
    
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
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button mainMenuButton;
    
    private List<Transform> players = new List<Transform>();
    private Transform currentTarget;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private float lastPathRecalculationTime;
    private float lastPatrolPathRecalculationTime;
    private bool seesPlayer;
    private bool gameOver = false;
    
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

    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            Debug.Log("Chaser: Client mode, AI disabled");
            enabled = false;
            return;
        }
        
        Debug.Log("Chaser: Server mode, initializing AI");
        currentPath = new List<Vector3>();
        patrolStartPosition = transform.position;
        currentState = ChaserState.Patrolling;
        
        // Set initial patrol target
        SetRandomPatrolTarget();
        
        // Ensure players are initialized
        Invoke(nameof(InitializeChaser), 1f);
    }
    
    void InitializeChaser()
    {
        FindAllPlayers();
        Debug.Log($"SmartAStarChaser initialized, found {players.Count} players");
        Debug.Log($"Chaser initial position: {transform.position}, state: {currentState}");
    }
    
    void Update()
    {
        if (!IsServer || gameOver) return;
        
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
        
        // Debug info every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Chaser state: {currentState}, position: {transform.position}, target: {currentTarget?.name ?? "none"}");
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
        if (AStarPathfinder.Instance == null) 
        {
            Debug.LogError("AStarPathfinder instance is null!");
            return;
        }
        
        Debug.Log($"Calculating patrol path: {transform.position} -> {currentPatrolTarget}");
        
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
            Debug.Log($"Reached patrol path point {currentPathIndex-1}/{currentPath.Count}");
            
            // If reached path end, set new patrol target
            if (currentPathIndex >= currentPath.Count)
            {
                // Reached patrol point, take a rest
                isIdle = true;
                idleTimer = 0f;
                currentPath = new List<Vector3>();
                Debug.Log("Reached patrol point, resting...");
            }
        }
    }
    
    void UpdateChasing()
    {
        // Check if target is lost
        if (currentTarget == null || !IsTargetInRange(currentTarget))
        {
            Debug.Log("Target lost, returning to patrol");
            currentState = ChaserState.Returning;
            currentTarget = null;
            currentPath = new List<Vector3>();
            return;
        }
        
        // Original chasing logic
        if (players.Count == 0 || currentTarget == null)
        {
            FindAllPlayers();
            if (players.Count == 0) return;
        }
        
        CheckCatchPlayer();
        seesPlayer = CheckPlayerVision();
        
        if (Time.time - lastPathRecalculationTime > recalculatePathInterval || 
            (seesPlayer && (currentPath == null || currentPath.Count == 0)))
        {
            RecalculatePath();
            lastPathRecalculationTime = Time.time;
        }
        
        FollowPath();
    }
    
    void UpdateReturning()
    {
        // Return to start position
        if (currentPath == null || currentPath.Count == 0)
        {
            // Calculate return path
            currentPath = AStarPathfinder.Instance?.FindPath(transform.position, patrolStartPosition);
            currentPathIndex = 0;
            
            if (currentPath != null && currentPath.Count > 0)
            {
                Debug.Log("Starting return to start position");
            }
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
                Debug.Log("Returned to start position, starting patrol");
            }
        }
        else
        {
            // Cannot find return path, teleport to start position
            transform.position = patrolStartPosition;
            currentState = ChaserState.Patrolling;
            SetRandomPatrolTarget();
            Debug.Log("Teleported back to start position");
        }
        
        // Still check for players while returning
        CheckForPlayersInSight();
    }
    
    void CheckForPlayersInSight()
    {
        if (players.Count == 0) 
        {
            FindAllPlayers();
            if (players.Count == 0) return;
        }
        
        // Check all players if they are in vision range
        foreach (Transform player in players)
        {
            if (player != null && IsTargetInSightRange(player) && CheckSinglePlayerVision(player))
            {
                // Found player, start chasing
                currentTarget = player;
                currentState = ChaserState.Chasing;
                Debug.Log($"Found player {player.name}, starting chase!");
                return;
            }
        }
    }
    
    bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= loseSightRange;
    }
    
    bool IsTargetInSightRange(Transform target)
    {
        if (target == null) return false;
        
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
            
            if (hit.collider == null)
            {
                currentPatrolTarget = potentialTarget;
                Debug.Log($"Set new patrol target: {currentPatrolTarget}");
                return;
            }
        }
        
        // If no suitable point found, use default method
        float randomAngleFinal = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 randomDirectionFinal = new Vector2(Mathf.Cos(randomAngleFinal), Mathf.Sin(randomAngleFinal));
        currentPatrolTarget = patrolStartPosition + (Vector3)randomDirectionFinal * patrolPointDistance;
        Debug.Log($"Set default patrol target: {currentPatrolTarget}");
    }
    
    bool CheckSinglePlayerVision(Transform player)
    {
        if (player == null) return false;
        
        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        
        // If within 7 meters, ignore angle check, only check wall obstruction
        if (distance <= directDetectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
            bool canSee = (hit.collider == null);
            
            if (canSee && Time.frameCount % 60 == 0)
                Debug.Log($"Player detected within 7m range: {player.name}, distance: {distance}");
                
            return canSee;
        }
        
        // If outside 7m but still in vision range, do full vision check
        if (distance > visionRange) return false;
        
        float angle = Vector2.Angle(transform.up, toPlayer.normalized);
        if (angle > visionAngle / 2) return false;
        
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
        return (hit2.collider == null);
    }
    
    void FindAllPlayers()
    {
        players.Clear();
        
        if (NetworkManager.Singleton != null && IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null && client.PlayerObject.CompareTag("Player"))
                {
                    players.Add(client.PlayerObject.transform);
                }
            }
        }
        
        if (players.Count == 0)
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObj in playerObjects)
            {
                if (playerObj != null)
                {
                    players.Add(playerObj.transform);
                }
            }
        }
    }

    void RecalculatePath()
    {
        if (AStarPathfinder.Instance == null) 
        {
            Debug.LogError("AStarPathfinder instance is null!");
            return;
        }
        
        if (currentTarget == null) 
        {
            Debug.LogWarning("Current target is null, cannot calculate path");
            return;
        }
        
        Vector3 targetPosition = seesPlayer ? currentTarget.position : GetStrategicPosition();
        
        Debug.Log($"Calculating chase path: {transform.position} -> {targetPosition}");
        
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
        if (currentTarget == null) return transform.position;
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
                transform.position += (Vector3)moveDirection * (seesPlayer ? chaseSpeed : moveSpeed) * Time.deltaTime;
                Debug.Log($"No path, direct chasing target, speed: {(seesPlayer ? chaseSpeed : moveSpeed)}");
            }
            else
            {
                // No path and cannot see player, recalculate path
                if (Time.frameCount % 120 == 0)
                {
                    Debug.Log("No path and cannot see player, recalculating path");
                    RecalculatePath();
                }
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
            Debug.Log($"Reached chase path point {currentPathIndex-1}/{currentPath.Count}");
            
            if (currentPathIndex >= currentPath.Count && seesPlayer)
            {
                RecalculatePath();
            }
        }
    }
    
    bool CheckPlayerVision()
    {
        if (currentTarget == null) return false;
        
        Vector2 toPlayer = currentTarget.position - transform.position;
        float distance = toPlayer.magnitude;
        
        // If within 7 meters, ignore angle check
        if (distance <= directDetectionRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
            bool canSee = (hit.collider == null);
            
            if (Time.frameCount % 120 == 0)
                Debug.Log($"Chasing within 7m range: distance={distance}, canSeePlayer={canSee}");
                
            return canSee;
        }
        
        // Normal vision check outside 7m range
        if (distance > visionRange) 
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log($"Player out of vision range: {distance} > {visionRange}");
            return false;
        }
        
        // Check vision angle
        float angle = Vector2.Angle(transform.up, toPlayer.normalized);
        if (angle > visionAngle / 2) 
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log($"Player out of vision angle: {angle} > {visionAngle/2}");
            return false;
        }
        
        // Check if line of sight is blocked
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, wallLayer);
        bool canSee2 = (hit2.collider == null);
        
        if (Time.frameCount % 120 == 0)
            Debug.Log($"Vision check: distance={distance}, angle={angle}, canSeePlayer={canSee2}");
            
        return canSee2;
    }
    
    void CheckCatchPlayer()
    {
        if (currentTarget == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToPlayer <= catchDistance)
        {
            Debug.Log($"Caught player! Distance: {distanceToPlayer}");
            CatchPlayerServerRpc();
        }
    }
    
    [ServerRpc]
    void CatchPlayerServerRpc()
    {
        GameOverClientRpc();
    }
    
    [ClientRpc]
    void GameOverClientRpc()
    {
        GameOver();
    }
    
    void GameOver()
    {
        gameOver = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (gameOverText != null)
            gameOverText.text = "Game Over! The chaser caught you!";
        
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name, 
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "MainMenu", 
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
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