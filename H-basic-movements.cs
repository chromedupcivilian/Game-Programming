using UnityEngine;

public class CubeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x, 0, z) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Color Change
        if (Input.GetKeyDown(KeyCode.R))
            GetComponent<Renderer>().material.color = Color.red;
        if (Input.GetKeyDown(KeyCode.G))
            GetComponent<Renderer>().material.color = Color.green;
        if (Input.GetKeyDown(KeyCode.B))
            GetComponent<Renderer>().material.color = Color.blue;
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }
}
