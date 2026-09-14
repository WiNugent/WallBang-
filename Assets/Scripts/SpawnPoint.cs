using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Vector2 spawnDirection;

    private void Awake()
    {
        GameManager.Instance.AddSpawnpointToList(this);
        Debug.Log("Spawn added for " + this.gameObject.transform.position);
    }
}
