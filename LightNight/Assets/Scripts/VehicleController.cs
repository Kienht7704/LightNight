using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Bánh xe (Wheel Colliders)")]
    public WheelCollider flCollider; // Bánh trước trái
    public WheelCollider frCollider; // Bánh trước phải
    public WheelCollider rlCollider; // Bánh sau trái
    public WheelCollider rrCollider; // Bánh sau phải

    [Header("Hình ảnh 3D (Meshes)")]
    public Transform flMesh;
    public Transform frMesh;
    public Transform rlMesh;
    public Transform rrMesh;

    [Header("Thông số Động cơ & Tốc độ")]
    public float motorForce = 2500f;       // Lực kéo động cơ
    public float maxSpeedForward = 160f;   // Tốc độ tiến tối đa (km/h)
    public float maxSpeedReverse = 20f;    // Tốc độ lùi tối đa (km/h)

    [Header("Thông số Hệ thống Lái")]
    public float maxSteerAngle = 45f;      // Góc bẻ lái tối đa
    public float steerSpeed = 30f;         // Tốc độ xoay bánh (độ/giây)
    private float _currentSteerAngle = 0f; // Góc lái hiện tại (dùng để làm mượt)

    [Header("Thông số Phanh & Trượt (Quán tính)")]
    public float brakeForce = 8000f;       // Lực phanh để khóa cứng bánh
    public float skidStiffness = 0.3f;     // Độ bám đường khi trượt (Càng nhỏ càng trượt xa)

    [Header("Hiệu ứng Khói/Bụi bánh xe")]
    public ParticleSystem flSmoke;
    public ParticleSystem frSmoke;
    public ParticleSystem rlSmoke;
    public ParticleSystem rrSmoke;
    [Tooltip("Ngưỡng trượt để bắt đầu xịt khói (0.1 đến 1)")]
    public float slipThreshold = 0.4f;

    // --- CÁC BIẾN NỘI BỘ (Private) ---
    private WheelFrictionCurve _defaultFriction;
    private Rigidbody _rb;
    private float _vInput;
    private float _hInput;
    private bool _isBraking;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // HẠ TRỌNG TÂM: Để xe không bị lật khi văng quán tính ở tốc độ cao
        _rb.centerOfMass = new Vector3(0, -0.6f, 0);

        // Lưu lại thông số ma sát ngang mặc định của 2 bánh sau
        _defaultFriction = rlCollider.sidewaysFriction;
    }

    void Update()
    {
        // 1. Nhận lệnh từ người chơi
        _vInput = Input.GetAxis("Vertical");   // W/S
        _hInput = Input.GetAxis("Horizontal"); // A/D
        _isBraking = Input.GetKey(KeyCode.Space);

        // 2. Cập nhật hình ảnh bánh xe xoay theo vật lý
        UpdateWheelVisuals(flCollider, flMesh);
        UpdateWheelVisuals(frCollider, frMesh);
        UpdateWheelVisuals(rlCollider, rlMesh);
        UpdateWheelVisuals(rrCollider, rrMesh);
    }

    void FixedUpdate()
    {
        HandleSteering();
        HandleMotorAndSpeed();
        HandleBrakingAndInertia();
        HandleParticleEffects(); // Kiểm tra xịt khói
    }

    private void HandleSteering()
    {
        // Tính toán góc lái mục tiêu
        float targetSteerAngle = _hInput * maxSteerAngle;

        // Xoay bánh xe từ từ với tốc độ 30 độ/giây
        _currentSteerAngle = Mathf.MoveTowards(_currentSteerAngle, targetSteerAngle, steerSpeed * Time.fixedDeltaTime);

        // Áp dụng góc lái vào 2 bánh trước
        flCollider.steerAngle = _currentSteerAngle;
        frCollider.steerAngle = _currentSteerAngle;
    }

    private void HandleMotorAndSpeed()
    {
        float currentSpeedKmh = _rb.velocity.magnitude * 3.6f;
        float moveDirection = Vector3.Dot(transform.forward, _rb.velocity);
        float currentTorque = 0f;

        if (_vInput > 0) // Nhấn W (Tiến)
        {
            if (currentSpeedKmh < maxSpeedForward || moveDirection < -0.5f)
            {
                currentTorque = _vInput * motorForce;
            }
        }
        else if (_vInput < 0) // Nhấn S (Lùi)
        {
            if (currentSpeedKmh < maxSpeedReverse || moveDirection > 0.5f)
            {
                currentTorque = _vInput * motorForce;
            }
        }

        // Dẫn động cầu trước
        flCollider.motorTorque = currentTorque;
        frCollider.motorTorque = currentTorque;
    }

    private void HandleBrakingAndInertia()
    {
        if (_isBraking)
        {
            // Phanh cứng 4 bánh
            flCollider.brakeTorque = frCollider.brakeTorque = rlCollider.brakeTorque = rrCollider.brakeTorque = brakeForce;

            // Tạo quán tính trượt: Giảm độ bám đường ngang của 2 bánh sau
            WheelFrictionCurve skidFriction = _defaultFriction;
            skidFriction.stiffness = skidStiffness;
            rlCollider.sidewaysFriction = rrCollider.sidewaysFriction = skidFriction;

            // Ngắt lực ga
            flCollider.motorTorque = frCollider.motorTorque = 0f;
        }
        else
        {
            // Nhả phanh
            flCollider.brakeTorque = frCollider.brakeTorque = rlCollider.brakeTorque = rrCollider.brakeTorque = 0f;

            // Trả lại độ bám đường bình thường
            rlCollider.sidewaysFriction = rrCollider.sidewaysFriction = _defaultFriction;
        }
    }

    private void HandleParticleEffects()
    {
        CheckAndPlaySmoke(flCollider, flSmoke);
        CheckAndPlaySmoke(frCollider, frSmoke);
        CheckAndPlaySmoke(rlCollider, rlSmoke);
        CheckAndPlaySmoke(rrCollider, rrSmoke);
    }

    private void CheckAndPlaySmoke(WheelCollider collider, ParticleSystem smokeParticle)
    {
        if (smokeParticle == null) return;

        WheelHit hit;
        if (collider.GetGroundHit(out hit))
        {
            // Kiểm tra độ trượt (Slip) của bánh xe
            if (Mathf.Abs(hit.sidewaysSlip) > slipThreshold || Mathf.Abs(hit.forwardSlip) > slipThreshold)
            {
                var emission = smokeParticle.emission;
                emission.enabled = true;
            }
            else
            {
                var emission = smokeParticle.emission;
                emission.enabled = false;
            }
        }
        else
        {
            var emission = smokeParticle.emission;
            emission.enabled = false;
        }
    }

    private void UpdateWheelVisuals(WheelCollider collider, Transform mesh)
    {
        if (mesh == null) return;
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}