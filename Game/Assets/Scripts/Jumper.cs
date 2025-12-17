using UnityEngine;

public class Spring : MonoBehaviour
{
    [Header("Настройки")]
    public float jumpForce = 25f;
    public Sprite normalSprite;
    public Sprite compressedSprite;

    [Header("Отладка")]
    public bool debugLogs = true;

    private SpriteRenderer spriteRenderer;
    private bool isActive = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
        else
        {
            Debug.LogWarning("Не назначен нормальный спрайт для пружинки!");
            // Создаем простой цветной квадрат
            spriteRenderer.color = Color.cyan;
        }

        // Добавляем коллайдер
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.5f);

        if (debugLogs) Debug.Log("Пружинка создана: " + transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (debugLogs) Debug.Log("Пружинка: касание с " + other.tag);

        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Меняем спрайт на сжатый
                if (compressedSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = compressedSprite;
                }
                else if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.yellow; // Визуальная обратная связь
                }

                // Подбрасываем игрока
                Vector2 velocity = playerRb.linearVelocity;
                velocity.y = 0;
                playerRb.linearVelocity = velocity;
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                if (debugLogs) Debug.Log($"Пружинка! Сила прыжка: {jumpForce}");

                // Деактивируем на время (0.5 секунды)
                isActive = false;
                Invoke("ReactivateSpring", 0.5f);
            }
        }
    }

    void ReactivateSpring()
    {
        isActive = true;
        if (normalSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Стрелка, показывающая силу прыжка
        Gizmos.color = Color.green;
        Vector3 endPoint = transform.position + Vector3.up * (jumpForce / 50f);
        Gizmos.DrawLine(transform.position, endPoint);
    }
}