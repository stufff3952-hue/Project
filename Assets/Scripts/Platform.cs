using UnityEngine;

public class Platform : MonoBehaviour
{
    void Start()
    {
        gameObject.tag = "Platform";
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }
}