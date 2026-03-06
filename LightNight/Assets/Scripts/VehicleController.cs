using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Visuals")]
    public Transform flMesh;
    public Transform frMesh;
    public Transform rlMesh;
    public Transform rrMesh;

    [Header("Car Settings")]
    public float motorTorque = 1500f; // Lực động cơ
    public float brakeTorque = 3000f; // Lực phanh
    public float maxSteerAngle = 35f; // Góc đánh lái tối đa

    private float _horizontalInput;
    private float _verticalInput;
    private bool _isBraking;

    private void Update()
    {
        // 1. Lấy Input từ người dùng
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        _isBraking = Input.GetKey(KeyCode.Space);

        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        // 2. Xử lý vật lý trong FixedUpdate
        HandleMotor();
        HandleSteering();
    }

    private void HandleMotor()
    {
        // Truyền lực vào 2 bánh sau (RWD) hoặc cả 4 bánh (AWD)
        float currentTorque = _verticalInput * motorTorque;

        rearLeft.motorTorque = currentTorque;
        rearRight.motorTorque = currentTorque;

        // Xử lý phanh
        float currentBrake = _isBraking ? brakeTorque : 0f;
        ApplyBraking(currentBrake);
    }

    private void ApplyBraking(float force)
    {
        frontLeft.brakeTorque = force;
        frontRight.brakeTorque = force;
        rearLeft.brakeTorque = force;
        rearRight.brakeTorque = force;
    }

    private void HandleSteering()
    {
        float steer = _horizontalInput * maxSteerAngle;
        frontLeft.steerAngle = steer;
        frontRight.steerAngle = steer;
    }

    private void UpdateWheelVisuals()
    {
        UpdateSingleWheel(frontLeft, flMesh);
        UpdateSingleWheel(frontRight, frMesh);
        UpdateSingleWheel(rearLeft, rlMesh);
        UpdateSingleWheel(rearRight, rrMesh);
    }

    private void UpdateSingleWheel(WheelCollider wheel, Transform visual)
    {
        Vector3 pos;
        Quaternion rot;
        wheel.GetWorldPose(out pos, out rot);
        visual.position = pos;
        visual.rotation = rot;
    }
}