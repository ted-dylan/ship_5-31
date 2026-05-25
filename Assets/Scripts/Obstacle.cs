using UnityEngine;

/// <summary>
/// 장애물(어뢰/암초/부표 등) 충돌 처리.
/// BoatProbes 처럼 자식 collider 를 가진 보트도 잡도록 attachedRigidbody 기반.
/// </summary>
public class Obstacle : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        var rootGo = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
        if (rootGo.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCollision();
        }
    }
}
