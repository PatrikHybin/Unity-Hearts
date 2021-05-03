using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        SpawnSystem.AddSpawnPoint(transform);
    }

    private void OnDestroy()
    {
        SpawnSystem.RemoveSpawnPoint(transform);
    }
}
