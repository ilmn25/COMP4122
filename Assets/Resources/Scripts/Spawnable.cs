using System.Collections.Generic;
using UnityEngine;

public class Spawnable: MonoBehaviour
{
    public int minCount = 1;
    public int maxCount = 1;
    public List<Vector3> spawnPoints = new List<Vector3>();
}