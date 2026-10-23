using UnityEngine;

public class ObjectLiftController : MonoBehaviour
{
    public Rigidbody rb;
    public KeyCode liftKey = KeyCode.Space;

    private RigidbodyConstraints originalConstraints;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Sauvegarde des contraintes initiales
        originalConstraints = rb.constraints;
    }

    void Update()
    {
        if (Input.GetKeyDown(liftKey))
        {
            // Débloque la contrainte verticale (Y)
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }

        if (Input.GetKeyUp(liftKey))
        {
            // Restaure les contraintes initiales
            rb.constraints = originalConstraints;
        }
    }
}
