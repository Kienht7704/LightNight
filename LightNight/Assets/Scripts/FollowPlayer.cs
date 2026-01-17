using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    // Reference to the player GameObject
    public GameObject player;
    private Vector3 offset = new Vector3(0, 6, -9);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Follow camera 
        transform.position = player.transform.position + offset;
    }
}
