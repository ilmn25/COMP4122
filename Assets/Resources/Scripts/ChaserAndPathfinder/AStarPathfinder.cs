using System.Collections.Generic;
using Resources.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance;
    
    [Header("A* Settings")]
    public LayerMask obstacleLayer;
    
    [Header("Grid Settings")]
    [Tooltip("Grid width (number of nodes)")]
    public int gridWidth = 50;
    [Tooltip("Grid height (number of nodes)")]
    public int gridHeight = 50;
    
    private Node[,] _grid;
    private Vector2Int _gridWorldSize;
    private Vector3Int _gridBottomLeft;
    
    private void Start()
    {
        Instance = this;

        _gridWorldSize = new Vector2Int(gridWidth, gridHeight);
        _grid = new Node[gridWidth, gridHeight];
        _gridBottomLeft = new Vector3Int(-gridWidth / 2, -gridHeight / 2, 0);
        Tilemap tilemapWall = GameObject.Find("Wall").GetComponent<Tilemap>();
        Tilemap tilemapFurniture = GameObject.Find("Top").GetComponent<Tilemap>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int cellPos = _gridBottomLeft + new Vector3Int(x, y, 0);
                _grid[x, y] = new Node(!tilemapWall.GetTile(cellPos) && !tilemapFurniture.GetTile(cellPos), tilemapWall.CellToWorld(cellPos) + tilemapWall.cellSize / 2f, x, y);
            }
        }
    }
    
    bool IsPositionWalkable(Vector3 worldPosition)
    {
        return Physics2D.OverlapBoxNonAlloc(worldPosition, Vector2.one* 0.99f, 0, Main.ColliderArray, Main.MaskStatic) == 0; 
    }
    
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);
        
        if (startNode == null)
            return null;
        
        if (targetNode == null)
            return null;
        
        if (!startNode.Walkable)
        {
            startNode = GetNearestWalkableNode(startNode);
            if (startNode == null)
                return null;
        }
        
        if (!targetNode.Walkable)
        {
            targetNode = GetNearestWalkableNode(targetNode);
            if (targetNode == null)
                return null;
        }
        
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        ResetNodes();
        
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);
        
        int maxIterations = gridWidth * gridHeight;
        int iterations = 0;
        
        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || 
                   (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
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
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    continue;
                
                int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                {
                    neighbour.GCost = newMovementCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, targetNode);
                    neighbour.Parent = currentNode;
                    
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
        
        List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();
        
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
        
            for (int i = 0; i < 4; i++)
            {
                int checkX = node.GridX + dx[i];
                int checkY = node.GridY + dy[i];
            
                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbours.Add(_grid[checkX, checkY]);
                }
            }
        
            return neighbours;
        }
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
                    
                    int checkX = targetNode.GridX + x;
                    int checkY = targetNode.GridY + y;
                    
                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                    {
                        Node node = _grid[checkX, checkY];
                        if (node.Walkable)
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
                _grid[x, y].GCost = int.MaxValue;
                _grid[x, y].HCost = 0;
                _grid[x, y].Parent = null;
            }
        }
    }
    
    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode.WorldPosition);
            currentNode = currentNode.Parent;
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
     
    
    Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - _gridBottomLeft;
        int x = Mathf.FloorToInt(localPos.x);
        int y = Mathf.FloorToInt(localPos.y);
        
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return _grid[x, y];
        
        return null;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
        
        return 10 * (dstX + dstY);
    }
    
    void OnDrawGizmos()
    {
        if (_grid != null)
        {
            foreach (Node n in _grid)
            {
                if (n.Walkable)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.1f);
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawCube(n.WorldPosition, Vector3.one);
                }
                
                Gizmos.DrawWireCube(n.WorldPosition, Vector3.one * 0.1f);
            }
        }
        
        // Draw grid boundaries
        Gizmos.color = Color.blue;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(_gridWorldSize.x, _gridWorldSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}

public class Node
{
    public readonly bool Walkable;
    public Vector3 WorldPosition;
    public readonly int GridX;
    public readonly int GridY;
    
    public int GCost;
    public int HCost;
    public Node Parent;
    
    public int FCost { get { return GCost + HCost; } }
    
    public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY)
    {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = _gridX;
        GridY = _gridY;
    }
}