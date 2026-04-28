using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public PlayerSystemType affectedSystem = PlayerSystemType.Core;
    public float damagePerSecond = 10f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerCondition playerCondition = other.GetComponent<PlayerCondition>();

        if (playerCondition != null)
        {
            playerCondition.DamageSystem(affectedSystem, damagePerSecond * Time.deltaTime);
        }
    }
}