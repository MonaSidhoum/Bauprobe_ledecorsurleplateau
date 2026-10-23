using UnityEngine;

public class Rotationobjet : MonoBehaviour
{
    public float rot_x = -90f;
    public float rot_y = 180f;
    public float rot_z = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Quaternion quater = transform.rotation;
        Vector3 rot = quater.eulerAngles;
        rot.x += rot_x;
        rot.y += rot_y;
        rot.z += rot_z;
        transform.rotation = Quaternion.Euler(rot);

    }
}
