using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponentInChildren<PlayerStats>();
        if (stats == null)
            stats = other.GetComponentInParent<PlayerStats>();

        if (stats != null)
            stats.Kill();
    }
}
