using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    void Update()
    {
        if (target != null)
        {
            transform.position = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                offset.z
            );
        }
    }
}