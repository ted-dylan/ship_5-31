using UnityEngine;

/// <summary>
/// 위험 구역 (안개/얕은 수심 등). 진입 시 지속 감점 또는 즉시 실패.
/// </summary>
public class DangerZone : MonoBehaviour
{
    [SerializeField] private bool instantFail = false;  // true면 즉시 실패
    [SerializeField] private float damageInterval = 1f; // 감점 간격
    private float damageTimer;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (instantFail && GameManager.Instance != null)
        {
            GameManager.Instance.Lose();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || instantFail) return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCollision();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer = 0f;
        }
    }
}

