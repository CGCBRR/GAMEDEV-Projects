using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("PlayerMovement script has started.");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 5, 0);
            Debug.Log("Player jumped.");
        }
        if (Input.GetKey("up"))
        {
            GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 5);
            Debug.Log("Player moved forward.");
        }
        if (Input.GetKey("right"))
        {
            GetComponent<Rigidbody>().linearVelocity = new Vector3(5, 0, 0);
            Debug.Log("Player moved right.");
        }
        
    }
}
