using UnityEngine;

public class WheelAnimator : MonoBehaviour
{
    [Header("Gắn 4 bánh xe vào đây")]
    public Transform leftFrontWheel;
    public Transform rightFrontWheel;
    public Transform leftRearWheel;
    public Transform rightRearWheel;

    [Header("Tốc độ quay bánh xe (dựa theo tốc độ di chuyển)")]
    public float wheelRotationSpeed = 360f; // Bánh xe xoay 360 độ mỗi giây nếu đi với tốc độ tiêu chuẩn

    [Header("Góc bẻ lái tối đa của 2 bánh trước")]
    public float maxSteerAngle = 30f;

    private Vector3 lastPosition;
    private VehicleController vehicle;

    void Start()
    {
        lastPosition = transform.position;
        // Tìm script di chuyển của chiếc xe hiện tại
        vehicle = GetComponentInParent<VehicleController>();
    }

    void Update()
    {
        // 1. ANIAMTION BÁNH LĂN KHI XE TIẾN / LÙI
        // Tính quãng đường xe vừa di chuyển trong frame này
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        // Xác định xe đang tiến hay lùi (dọt hướng forward)
        float direction = Vector3.Dot(transform.forward, transform.position - lastPosition) > 0 ? 1f : -1f;

        // Xoay cả 4 bánh quanh trục X (nếu trục trọn bánh xe của model bạn hướng khác, có thể đổi Vector3.right thành Vector3.up hoặc Vector3.forward)
        float rotationAmount = distanceMoved * wheelRotationSpeed * direction;
        
        if (leftFrontWheel != null) leftFrontWheel.Rotate(rotationAmount, 0, 0, Space.Self);
        if (rightFrontWheel != null) rightFrontWheel.Rotate(rotationAmount, 0, 0, Space.Self);
        if (leftRearWheel != null) leftRearWheel.Rotate(rotationAmount, 0, 0, Space.Self);
        if (rightRearWheel != null) rightRearWheel.Rotate(rotationAmount, 0, 0, Space.Self);

        lastPosition = transform.position;

        // 2. ANIMATION BÁNH TRƯỚC BẺ LÁI KHI RẼ TRÁI / PHẢI
        if (vehicle != null && vehicle.IsOwner)
        {
            float steerInput = 0f;
            if (Input.GetKey(KeyCode.A)) steerInput = -1f;
            if (Input.GetKey(KeyCode.D)) steerInput = 1f;

            // Xoay 2 bánh trước theo trục Y
            float currentSteerAngle = steerInput * maxSteerAngle;
            //hi
            if (leftFrontWheel != null)
            {
                Vector3 localEuler = leftFrontWheel.localEulerAngles;
                localEuler.y = currentSteerAngle;
                leftFrontWheel.localEulerAngles = localEuler;
            }
            if (rightFrontWheel != null)
            {
                Vector3 localEuler = rightFrontWheel.localEulerAngles;
                localEuler.y = currentSteerAngle;
                rightFrontWheel.localEulerAngles = localEuler;
            }
        }
    }
}
