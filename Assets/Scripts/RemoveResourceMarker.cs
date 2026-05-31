using UnityEngine;

public class GeneratedResourceMarker : MonoBehaviour
{
    public GeneratedWorldSpawner spawner;
    public Vector2Int cell;

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.RemoveResourceAtCell(cell);
        }
    }
}