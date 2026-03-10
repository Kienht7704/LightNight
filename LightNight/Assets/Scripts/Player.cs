using UnityEngine;

public class Player : MonoBehaviour
{
    // =====================================================================
    // PHYSICS & VISUALS
    // =====================================================================
    [Header("Vật lý (Physics)")]
    [Tooltip("Rigidbody hình cầu dùng để di chuyển xe (tách rời khỏi xe)")]
    public Rigidbody sphere;

    [Tooltip("Khoảng cách offset theo trục Y giữa tâm sphere và thân xe")]
    public float yOffset = 0.4f;

    // =====================================================================
    // CAR STATS
    // =====================================================================
    [Header("Thông số xe")]
    [Tooltip("Lực tăng tốc tiến/lùi")]
    public float acceleration = 150f;

    [Tooltip("Tốc độ xoay thân xe (đổi hướng)")]
    public float steering = 80f;

    [Tooltip("Lực kéo xuống giả lập trọng lực")]
    public float gravity = 40f;

    // =====================================================================
    // WHEEL REFERENCES
    // =====================================================================
    [Header("Bánh xe (Wheels)")]
    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform wheelRearLeft;
    public Transform wheelRearRight;

    // =====================================================================
    // STEERING SETTINGS
    // =====================================================================
    [Header("Đánh lái (Steering Visual)")]
    public float maxSteerAngle = 30f;
    public float wheelSteerSpeed = 8f;

    // =====================================================================
    // ROLLING SETTINGS
    // =====================================================================
    [Header("Lăn bánh (Rolling Visual)")]
    public float wheelRollFactor = 150f;

    // =====================================================================
    // PRIVATE VARIABLES
    // =====================================================================
    private float currentSpeed;     
    private float currentRotate;    

    public void addCurrentSpeed(float addSpeed)
    {
        currentSpeed += addSpeed;
    }

    // Hai biến mới để quản lý phép xoay an toàn (Tránh lỗi Gimbal Lock)
    private float wheelRollAngle = 0f;      // Tích lũy góc lăn lốp xe
    private float frontWheelSteerAngle = 0f;// Góc bẻ lái hiện tại của lốp trước


    // =====================================================================
    // START
    // =====================================================================
    void Start()
    {
        if (sphere != null)
            sphere.transform.parent = null;
    }

    // =====================================================================
    // UPDATE — Xử lý hình ảnh (Visuals)
    // =====================================================================
    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // --- Lerp tốc độ tiến/lùi ---
        float targetSpeed = verticalInput * acceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);

        Vector3 currentVelocity = sphere != null ? sphere.linearVelocity : Vector3.zero;

        // Xác định xe đang tiến hay lùi dựa trên vector vận tốc thực tế
        float moveDirection = Vector3.Dot(transform.forward, currentVelocity) >= 0 ? 1f : -1f;

        // --- Tính tốc độ xoay thân xe ---
        float targetRotate = horizontalInput * steering * moveDirection;
        currentRotate = Mathf.Lerp(currentRotate, targetRotate, Time.deltaTime * 5f);

        // SỬA LỖI: Cho phép đánh lái dựa trên tốc độ thực của xe thay vì phím bấm (giúp bẻ lái khi thả trớn)
        if (currentVelocity.magnitude > 0.5f)
        {
            transform.Rotate(0f, currentRotate * Time.deltaTime, 0f, Space.World);
        }

        // --- Đánh lái bánh trước ---
        float targetSteerAngle = horizontalInput * maxSteerAngle;
        frontWheelSteerAngle = Mathf.Lerp(frontWheelSteerAngle, targetSteerAngle, Time.deltaTime * wheelSteerSpeed);

        // --- Lăn bánh (Tính toán góc tích lũy) ---
        float forwardSpeed = Vector3.Dot(currentVelocity, transform.forward);
        wheelRollAngle += forwardSpeed * wheelRollFactor * Time.deltaTime;

        // --- SỬA LỖI GIMBAL LOCK: Áp dụng phép xoay bằng Quaternion ---
        UpdateWheelsVisuals();
    }

    private void UpdateWheelsVisuals()
    {
        // Tính toán ma trận xoay cho bánh trước (bao gồm bẻ lái trục Y + lăn trục X)
        Quaternion frontRotation = Quaternion.Euler(0f, frontWheelSteerAngle, 0f) * Quaternion.Euler(wheelRollAngle, 0f, 0f);
        
        // Tính toán ma trận xoay cho bánh sau (chỉ lăn trục X)
        Quaternion rearRotation = Quaternion.Euler(wheelRollAngle, 0f, 0f);

        if (wheelFrontLeft) wheelFrontLeft.localRotation = frontRotation;
        if (wheelFrontRight) wheelFrontRight.localRotation = frontRotation;
        
        if (wheelRearLeft) wheelRearLeft.localRotation = rearRotation;
        if (wheelRearRight) wheelRearRight.localRotation = rearRotation;
    }

    // =====================================================================
    // FIXED UPDATE — Xử lý vật lý (Physics)
    // =====================================================================
    private void FixedUpdate()
    {
        if (sphere == null) return;

        // Đẩy lực tiến/lùi theo hướng xe đang nhìn
        sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);

        // Giả lập trọng lực bổ sung để xe bám đất tốt hơn
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    // =====================================================================
    // LATE UPDATE — Đồng bộ vị trí xe với sphere
    // =====================================================================
    private void LateUpdate()
    {
        if (sphere == null) return;

        transform.position = sphere.transform.position - new Vector3(0f, yOffset, 0f);
    }
}