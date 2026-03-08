using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target; // Kéo xe VF3 vào đây

    [Header("Cài đặt vị trí Camera")]
    public float distance = 5f; // Khoảng cách từ xe đến camera
    public float height = 2f;   // Độ cao của camera

    [Header("Độ mượt (CÀNG NHỎ CÀNG MƯỢT)")]
    public float smoothTime = 0.15f; // Độ trễ của lò xo (thử 0.1 đến 0.3)
    public float rotationSmoothSpeed = 10f; // Tốc độ xoay cổ

    // Biến nội bộ để SmoothDamp tính toán gia tốc (không cần chỉnh)
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. TÍNH VỊ TRÍ ĐÍCH
        Vector3 desiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);

        // 2. DI CHUYỂN BẰNG SMOOTH DAMP (Mượt hơn Lerp rất nhiều)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // 3. XOAY CAMERA MƯỢT MÀ
        Vector3 directionToTarget = target.position - transform.position;
        if (directionToTarget != Vector3.zero) // Tránh lỗi vặt khi trùng tọa độ
        {
            Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }
}