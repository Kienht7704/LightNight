using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Physics & Visuals")]
    public Rigidbody sphere;
    public float yOffset = 0.4f;

    [Header("Car Stats")]
    public float acceleration = 150f;
    public float steering = 150f;
    public float gravity = 40f;

    private float currentSpeed;
    private float currentRotate;

    void Start()
    {
        if (sphere != null)
        {
            sphere.transform.parent = null;
        }
    }

    void Update()
    {
        // Tính toán tốc độ tiến/lùi
        float targetSpeed = Input.GetAxis("Vertical") * acceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);

        // Tính toán góc cua
        float targetRotate = Input.GetAxis("Horizontal") * steering;
        currentRotate = Mathf.Lerp(currentRotate, targetRotate, Time.deltaTime * 5f);

        // Xoay vỏ xe ổn định, không triệt tiêu lực tiến
        transform.Rotate(0, currentRotate * Time.deltaTime, 0);
    }

    private void FixedUpdate()
    {
        if (sphere == null) return;

        sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (sphere != null)
        {
            transform.position = sphere.transform.position - new Vector3(0, yOffset, 0);
        }
    }
}