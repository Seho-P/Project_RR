using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class RoomEnterTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public event Action<Collider2D> PlayerEntered;

    /// <summary>
    /// 트리거 콜라이더를 기본값으로 설정한다.
    /// </summary>
    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 플레이어가 트리거에 진입하면 방 진입 이벤트를 발행한다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag))
        {
            return;
        }

        PlayerEntered?.Invoke(other);
    }
}
