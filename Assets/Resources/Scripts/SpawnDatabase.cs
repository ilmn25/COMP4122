using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpawnDatabase")]
public class SpawnDatabase : ScriptableObject
{
    public List<GameObject> entries = new List<GameObject>();
}