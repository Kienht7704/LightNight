using UnityEngine;

public class Player : MonoBehaviour
{
    // =====================================================================
    // PHYSICS & VISUALS
    // =====================================================================
    [Header("Vật lý (Physics)")]
    public Rigidbody sphere;
    public float yOffset = 0.4f;
    public float gravity = 40f;

    [Header("Động cơ (Engine)")]
    public float topSpeedKmh = 160f;
    public float maxReverseSpeedKmh = 30f;
    public float motorForce = 150f;
    public float brakeForce = 200f;

    [Header("Đánh lái & Bánh xe")]
    public float steering = 80f;
    public float maxSteerAngle = 30f;
    public float wheelSteerSpeed = 8f;
    public float wheelRollFactor = 150f;

    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform wheelRearLeft;
    public Transform wheelRearRight;

    // =====================================================================
    // DRIFT SYSTEM (ĐÃ NÂNG CẤP)
    // =====================================================================
    [Header("Cơ chế Drift (Trượt lốp)")]
    [Tooltip("Hệ số bẻ lái nhanh hơn khi đang giữ Space")]
    public float driftSteerMultiplier = 1.5f;

    [Tooltip("Lực cản khi drift (Càng nhỏ xe lướt đi càng xa, cảm giác như đi trên băng)")]
    public float driftDrag = 0.5f;

    [Tooltip("Lực cản khi chạy bình thường (Giúp xe bám đường)")]
    public float normalDrag = 2.5f;

    [Tooltip("Tốc độ lấy lại bám đường khi nhả Space (Giúp xe dứt drift và lao thẳng)")]
    public float tractionSpeed = 5f;

    // =====================================================================
    // HIỆU ỨNG (EFFECTS)
    // =====================================================================
    [Header("Hiệu ứng (Vết bánh xe)")]
    public TrailRenderer skidmarkLeft;
    public TrailRenderer skidmarkRight;

    [Header("Debug")]
    public float currentKmh;
    public bool isDrifting;

    // Biến nội bộ
    private float currentRotate;
    private float wheelRollAngle = 0f;
    private float frontWheelSteerAngle = 0f;
    private float verticalInput;
    private float horizontalInput;

    void Start()
    {
        if (sphere != null)
        {
            sphere.transform.parent = null;
        }
    }

    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        // Nhận diện phím Drift (Spacebar) - Chỉ drift khi xe chạy nhanh hơn 20km/h
        isDrifting = Input.GetKey(KeyCode.Space) && currentKmh > 20f;

        // Bật/tắt vệt bánh xe khi Drift
        if (skidmarkLeft) skidmarkLeft.emitting = isDrifting;
        if (skidmarkRight) skidmarkRight.emitting = isDrifting;

        Vector3 currentVelocity = sphere != null ? sphere.linearVelocity : Vector3.zero;
        float moveDirection = Vector3.Dot(transform.forward, currentVelocity) >= 0 ? 1f : -1f;

        float activeSteering = isDrifting ? steering * driftSteerMultiplier : steering;

        float targetRotate = horizontalInput * activeSteering * moveDirection;
        currentRotate = Mathf.Lerp(currentRotate, targetRotate, Time.deltaTime * 5f);

        if (currentVelocity.magnitude > 0.5f)
        {
            transform.Rotate(0f, currentRotate * Time.deltaTime, 0f, Space.World);
        }

        float targetSteerAngle = horizontalInput * maxSteerAngle;
        frontWheelSteerAngle = Mathf.Lerp(frontWheelSteerAngle, targetSteerAngle, Time.deltaTime * wheelSteerSpeed);

        float forwardSpeed = Vector3.Dot(currentVelocity, transform.forward);
        wheelRollAngle += forwardSpeed * wheelRollFactor * Time.deltaTime;

        UpdateWheelsVisuals();
    }

    private void UpdateWheelsVisuals()
    {
        Quaternion frontRotation = Quaternion.Euler(wheelRollAngle, frontWheelSteerAngle, 0f);
        Quaternion rearRotation = Quaternion.Euler(wheelRollAngle, 0f, 0f);

        if (wheelFrontLeft) wheelFrontLeft.localRotation = frontRotation;
        if (wheelFrontRight) wheelFrontRight.localRotation = frontRotation;

        if (wheelRearLeft) wheelRearLeft.localRotation = rearRotation;
        if (wheelRearRight) wheelRearRight.localRotation = rearRotation;
    }

    private void FixedUpdate()
    {
        if (sphere == null) return;

        float forwardSpeedMs = Vector3.Dot(sphere.linearVelocity, transform.forward);
        currentKmh = Mathf.Abs(forwardSpeedMs * 3.6f);

        // XỬ LÝ VẬT LÝ KHI DRIFT (Lướt xe)
        if (isDrifting)
        {
            // Ép ma sát xuống cực thấp -> Thân xe trượt dài theo quán tính
            sphere.linearDamping = Mathf.Lerp(sphere.linearDamping, driftDrag, Time.deltaTime * 5f);
        }
        else
        {
            // Trả lại ma sát bám đường
            sphere.linearDamping = Mathf.Lerp(sphere.linearDamping, normalDrag, Time.deltaTime * 5f);

            // BÍ QUYẾT LẤY LẠI THĂNG BẰNG SAU KHI DRIFT
            // Khi nhả Space, tự động bẻ cong vector quán tính về hướng mũi xe đang nhìn
            if (currentKmh > 10f)
            {
                Vector3 targetVelocity = transform.forward * sphere.linearVelocity.magnitude * Mathf.Sign(forwardSpeedMs);
                sphere.linearVelocity = Vector3.Lerp(sphere.linearVelocity, targetVelocity, Time.deltaTime * tractionSpeed);
            }
        }

        // Vẫn cho phép đạp ga nhẹ khi đang drift để giữ trớn (Duy trì lướt)
        if (Mathf.Abs(verticalInput) > 0.05f)
        {
            bool isAccelerating = Mathf.Sign(verticalInput) == Mathf.Sign(forwardSpeedMs) || Mathf.Abs(forwardSpeedMs) < 0.1f;

            if (isAccelerating)
            {
                float currentMaxSpeed = (verticalInput > 0) ? topSpeedKmh : maxReverseSpeedKmh;

                if (currentKmh < currentMaxSpeed)
                {
                    float speedRatio = currentKmh / currentMaxSpeed;
                    float appliedForce = motorForce * verticalInput * (1f - speedRatio);

                    sphere.AddForce(transform.forward * appliedForce, ForceMode.Acceleration);
                }
            }
            else
            {
                sphere.AddForce(transform.forward * brakeForce * verticalInput, ForceMode.Acceleration);
            }
        }

        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (sphere == null) return;
        transform.position = sphere.transform.position - new Vector3(0f, yOffset, 0f);
    }
}