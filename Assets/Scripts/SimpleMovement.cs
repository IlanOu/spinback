using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 120f;

    void Update()
    {
        // Déplacement avant/arrière avec les touches Z et S (ou W et S)
        float verticalInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * verticalInput * moveSpeed * Time.deltaTime);

        // Déplacement gauche/droite avec les touches Q et D (ou A et D)
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * moveSpeed * Time.deltaTime);

        /*
        // Rotation avec les touches E et A (ou E et Q)
        if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        */
    }
}