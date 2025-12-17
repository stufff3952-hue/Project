using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadZoneCollider : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошёл в мёртвую зону!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    [ContextMenu("Автонастройка коллайдера")]
    void SetupCollider()
    {
        Camera cam = Camera.main;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        if (cam != null && collider != null)
        {
            float screenWidth = cam.orthographicSize * cam.aspect * 2f;
            float screenHeight = 1f; // Высота зоны
            collider.size = new Vector2(screenWidth + 2f, screenHeight);
            Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0));
            transform.position = new Vector3(
                bottomCenter.x,
                bottomCenter.y - screenHeight,
                0
            );

            Debug.Log($"Коллайдер настроен: размер={collider.size}, позиция={transform.position}");
        }
    }
}