using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance;
    
    [Header("A* Settings")]
    public LayerMask obstacleLayer;
    public float nodeSize = 1f;
    
    [Header("Grid Settings")]
    [Tooltip("Grid width (number of nodes)")]
    public int gridWidth = 50;
    [Tooltip("Grid height (number of nodes)")]
    public int gridHeight = 50;
    
    private Node[,] grid;
    private Vector2 gridWorldSize;
    private Vector3 gridBottomLeft;
    
    void Awake()
    {
        Instance = this;
        gridWorldSize = new Vector2(gridWidth * nodeSize, gridHeight * nodeSize);
        
        // Check LayerMask configuration
        int mapLayer = LayerMask.NameToLayer("Map");
        if (mapLayer != -1)
        {
            bool containsMapLayer = (obstacleLayer.value & (1 << mapLayer)) != 0;
            
            if (!containsMapLayer)
            {
                Debug.LogError("A* pathfinder not configured to detect Map layer! Please set Obstacle Layer to include Map layer in Inspector");
            }
        }
        else
        {
            Debug.LogError("Map layer does not exist! Please ensure Map layer is created");
        }
        
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];
        gridBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
        
        int walkableCount = 0;
        int unwalkableCount = 0;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPoint = gridBottomLeft + 
                    Vector3.right * (x * nodeSize + nodeSize / 2) + 
                    Vector3.up * (y * nodeSize + nodeSize / 2);
                
                bool walkable = IsPositionWalkable(worldPoint);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
                
                if (walkable) walkableCount++;
                else unwalkableCount++;
            }
        }
        
        Debug.Log($"A* grid created: Size={gridWidth}x{gridHeight}, Walkable={walkableCount}, Unwalkable={unwalkableCount}");
        
        if (unwalkableCount == 0)
        {
            Debug.LogError("Warning: No obstacles detected! Please check:\n" +
                          "1. Obstacle Layer includes Map layer\n" +
                          "2. Walls and furniture are on Map layer\n" +
                          "3. Walls and furniture have Collider2D components\n" +
                          "4. Node Size is appropriate");
        }
    }
    
    bool IsPositionWalkable(Vector3 worldPosition)
    {
        // Method 1: Use LayerMask detection
        float checkRadius = nodeSize / 2f;
        Collider2D hit = Physics2D.OverlapCircle(worldPosition, checkRadius, obstacleLayer);
        
        // Method 2: Fallback detection
        if (hit == null)
        {
            Collider2D[] allHits = Physics2D.OverlapCircleAll(worldPosition, checkRadius);
            foreach (Collider2D collider in allHits)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    hit = collider;
                    break;
                }
            }
        }
        
        return hit == null;
    }
    
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);
        
        if (startNode == null)
        {
            Debug.LogWarning($"Start node is null! Position: {startPos}");
            return null;
        }
        
        if (targetNode == null)
        {
            Debug.LogWarning($"Target node is null! Position: {targetPos}");
            return null;
        }
        
        if (!startNode.walkable)
        {
            Debug.LogWarning($"Start position not walkable! Position: {startPos}");
            startNode = GetNearestWalkableNode(startNode);
            if (startNode == null)
            {
                Debug.LogWarning("No walkable area around start position");
                return null;
            }
        }
        
        if (!targetNode.walkable)
        {
            Debug.LogWarning($"Target position not walkable! Position: {targetPos}");
            targetNode = GetNearestWalkableNode(targetNode);
            if (targetNode == null)
            {
                Debug.LogWarning("No walkable area around target position");
                return null;
            }
        }
        
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        ResetNodes();
        
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        
        int maxIterations = gridWidth * gridHeight;
        int iterations = 0;
        
        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            if (currentNode == targetNode)
            {
                List<Vector3> path = RetracePath(startNode, targetNode);
                return path;
            }
            
            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;
                
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        
        if (iterations >= maxIterations)
        {
            Debug.LogWarning($"Pathfinding timeout! Max iterations: {maxIterations}");
        }
        
        return null;
    }
    
    Node GetNearestWalkableNode(Node targetNode)
    {
        if (targetNode == null) return null;
        
        for (int radius = 1; radius < 10; radius++)
        {
            List<Node> walkableNodes = new List<Node>();
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) > radius) continue;
                    
                    int checkX = targetNode.gridX + x;
                    int checkY = targetNode.gridY + y;
                    
                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                    {
                        Node node = grid[checkX, checkY];
                        if (node.walkable)
                        {
                            walkableNodes.Add(node);
                        }
                    }
                }
            }
            
            if (walkableNodes.Count > 0)
            {
                Node nearest = walkableNodes[0];
                float nearestDistance = GetDistance(targetNode, nearest);
                
                foreach (Node node in walkableNodes)
                {
                    float distance = GetDistance(targetNode, node);
                    if (distance < nearestDistance)
                    {
                        nearest = node;
                        nearestDistance = distance;
                    }
                }
                
                return nearest;
            }
        }
        
        Debug.LogWarning("No walkable nodes found within radius 10");
        return null;
    }
    
    void ResetNodes()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y].gCost = int.MaxValue;
                grid[x, y].hCost = 0;
                grid[x, y].parent = null;
            }
        }
    }
    
    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        
        return SimplifyPath(path);
    }
    
    List<Vector3> SimplifyPath(List<Vector3> path)
    {
        if (path.Count < 3)
            return path;
            
        List<Vector3> simplifiedPath = new List<Vector3>();
        simplifiedPath.Add(path[0]);
        
        Vector2 oldDirection = Vector2.zero;
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 newDirection = ((Vector2)(path[i + 1] - path[i])).normalized;
            
            if (newDirection != oldDirection)
            {
                simplifiedPath.Add(path[i]);
            }
            
            oldDirection = newDirection;
        }
        
        simplifiedPath.Add(path[path.Count - 1]);
        return simplifiedPath;
    }
    
    List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        
        for (int i = 0; i < 4; i++)
        {
            int checkX = node.gridX + dx[i];
            int checkY = node.gridY + dy[i];
            
            if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }
        
        return neighbours;
    }
    
    Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - gridBottomLeft;
        int x = Mathf.FloorToInt(localPos.x / nodeSize);
        int y = Mathf.FloorToInt(localPos.y / nodeSize);
        
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        
        return null;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        return 10 * (dstX + dstY);
    }
    
    void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                if (n.walkable)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.1f);
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeSize);
                }
                
                Gizmos.DrawWireCube(n.worldPosition, Vector3.one * (nodeSize - 0.1f));
            }
        }
        
        // Draw grid boundaries
        Gizmos.color = Color.blue;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(gridWorldSize.x, gridWorldSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public int gCost;
    public int hCost;
    public Node parent;
    
    public int fCost { get { return gCost + hCost; } }
    
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}