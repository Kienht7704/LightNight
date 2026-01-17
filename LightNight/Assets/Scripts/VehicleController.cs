using Unity.Netcode;
using UnityEngine;

public class VehicleController : NetworkBehaviour
{
    public float Speed = 10f;
    public float SteerSpeed = 50f;

    private bool _IsFinished = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            float startX = (int)OwnerClientId * 3.0f - 7.5f;
            transform.position = new Vector3(startX, 1f, 0f);
        }
    }

    void Update()
    {
        if (!IsOwner || _IsFinished)
            return;

        transform.Translate(Vector3.forward * Speed * Time.deltaTime);

        float moveX = 0f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        transform.Translate(Vector3.right * moveX * SteerSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Finish"))
        {
            _IsFinished = true;
            Debug.Log("Client " + OwnerClientId + " đã về đích!");

            SubmitFinishServerRpc();
        }
    } 

    [ServerRpc]
    void SubmitFinishServerRpc()
    {
        Debug.Log("Server xác nhận: Người chơi " + OwnerClientId + " đã hoàn thành!");
    }
}