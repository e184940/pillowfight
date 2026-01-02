using UnityEngine;
using UnityEngine.SceneManagement;

public class BallController : MonoBehaviour
{
    public float speed;
    Rigidbody rb;
    float xInp;
    float yInp;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // start() kalles før første frame update
    // update() kalle hver frame
    void Update()
    {
        if (transform.position.y < -5f)
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    void FixedUpdate()
    {
        xInp = Input.GetAxis("Horizontal");
        yInp = Input.GetAxis("Vertical");
        
        rb.AddForce(xInp * speed, 0, yInp * speed);
    }
}
