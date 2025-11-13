using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 4f, -8f);
    public float followSpeed = 5f;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (!target) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 부드럽게 이동 (위치만)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / followSpeed
        );

    }
}
