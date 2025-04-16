using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    private void OnEnable()
    {
        spawnPoints.Add(this);

    }
    private void OnDisable()
    {
        spawnPoints.Remove(this);

    }
    public static Vector3 GetRandomSpawnPos()
    {
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;

    }
    private void OnDrawGizmosSelected()
    {
        Debug.Log("Gizmos should be visible!");
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1);
    }


}
