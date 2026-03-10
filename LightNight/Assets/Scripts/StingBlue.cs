using UnityEngine;

public class StingBlue : MonoBehaviour
{
    public float AddSpeed = 200f;

    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.addCurrentSpeed(200);
                Destroy(gameObject);
                
            }
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
