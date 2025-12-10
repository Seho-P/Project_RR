using UnityEngine;


public class EnemyHpTop : MonoBehaviour
{
    // [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);

    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;

        //자연스럽게 타겟 위치로 이동
        Vector3 worldPos = transform.parent.position;
        transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 10f);
    }
}