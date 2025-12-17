using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("ДВИЖЕНИЕ")]
    public float moveSpeed = 2f;
    public float moveDistance = 2f;
    public bool moveHorizontally = true; // Движение по горизонтали (влево-вправо)
    public bool moveVertically = false; // Движение по вертикали (вверх-вниз)

    [Header("ПОВОРОТ")]
    public bool rotateToMovement = true;
    public RotationMode rotationMode = RotationMode.FlipSprite;
    public float rotationSpeed = 5f;

    public enum RotationMode
    {
        FlipSprite,
        RotateTransform,
        RotateGraphics
    }

    [Header("ВИЗУАЛИЗАЦИЯ")]
    public Color enemyColor = Color.red;
    public float blinkSpeed = 5f;
    public Transform graphicsTransform;

    [Header("СМЕРТЕЛЬНОСТЬ")]
    public bool instantKill = true;
    public static float minSpawnDistance = 2.5f;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private static List<Enemy> allEnemies = new List<Enemy>();
    private Vector2 lastPosition;
    private Vector2 movementDirection;
    private bool facingRight = true;
    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
        if (graphicsTransform == null && transform.childCount > 0)
        {
            graphicsTransform = transform.GetChild(0);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);
        }
        InitializeRigidbody();
        allEnemies.Add(this);
        Debug.Log($"Враг создан. Всего врагов: {allEnemies.Count}");
    }

    void InitializeRigidbody()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }
    void Update()
    {
        Vector2 currentPosition = transform.position;
        MoveEnemy();
        movementDirection = (currentPosition - lastPosition).normalized;
        lastPosition = currentPosition;

        if (rotateToMovement && movementDirection.magnitude > 0.01f)
        {
            UpdateRotation();
        }
        UpdateBlinking();
    }
    void MoveEnemy()
    {
        float newX = transform.position.x;
        float newY = transform.position.y;

        if (moveHorizontally)
        {
            newX = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        }

        if (moveVertically)
        {
            newY = startPosition.y + Mathf.Cos(Time.time * moveSpeed * 0.8f) * moveDistance * 0.5f;
        }
        transform.position = new Vector3(newX, newY, 0);
    }

    void UpdateRotation()
    {
        switch (rotationMode)
        {
            case RotationMode.FlipSprite:
                FlipSpriteBasedOnDirection();
                break;

            case RotationMode.RotateTransform:
                RotateTransformBasedOnDirection();
                break;

            case RotationMode.RotateGraphics:
                RotateGraphicsBasedOnDirection();
                break;
        }
    }
    void FlipSpriteBasedOnDirection()
    {
        if (spriteRenderer == null) return;
        bool shouldFaceRight = movementDirection.x > 0;
        if (Mathf.Abs(movementDirection.x) < 0.1f)
        {
        }
        else
        {
            facingRight = shouldFaceRight;
        }
        spriteRenderer.flipX = !facingRight;
    }

    void RotateTransformBasedOnDirection()
    {
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void RotateGraphicsBasedOnDirection()
    {
        if (graphicsTransform == null)
        {
            RotateTransformBasedOnDirection();
            return;
        }

        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        graphicsTransform.rotation = Quaternion.Lerp(graphicsTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void UpdateBlinking()
    {
        if (spriteRenderer != null && blinkSpeed > 0)
        {
            float alpha = 0.7f + Mathf.Sin(Time.time * blinkSpeed) * 0.3f;
            spriteRenderer.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, alpha);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок коснулся врага! Смерть...");
            KillPlayer(other.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        StartCoroutine(DeathEffectCoroutine(player));
    }

    System.Collections.IEnumerator DeathEffectCoroutine(GameObject player)
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        Doodlik playerScript = player.GetComponent<Doodlik>();
        if (playerScript != null)
        {
            playerScript.Die();
        }
        else
        {
            Debug.LogWarning("У игрока нет скрипта Doodlik! Перезагружаем уровень напрямую...");
            yield return new WaitForSeconds(0.5f);
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    void OnDestroy()
    {
        allEnemies.Remove(this);
    }
    public void SetMovement(Vector2 direction, float speed)
    {
        movementDirection = direction.normalized;
        moveSpeed = speed;
    }
    public static bool CanSpawnAtPosition(Vector3 position)
    {
        if (allEnemies.Count == 0) return true;

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null) continue;
            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < minSpawnDistance)
            {
                Debug.Log($"Нельзя спавнить: слишком близко к другому врагу (дистанция: {distance})");
                return false;
            }
            float verticalDistance = Mathf.Abs(position.y - enemy.transform.position.y);
            if (verticalDistance < 1f && Mathf.Abs(position.x - enemy.transform.position.x) < 1f)
            {
                Debug.Log($"Нельзя спавнить: прямо над/под другим врагом");
                return false;
            }
        }
        return true;
    }
    public static Vector3 FindSafeSpawnPosition(Vector3 desiredPosition, float searchRadius = 3f, int maxAttempts = 10)
    {
        Vector3 bestPosition = desiredPosition;
        float bestDistance = 0f;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-searchRadius, searchRadius),
                UnityEngine.Random.Range(-searchRadius * 0.5f, searchRadius * 0.5f),
                0
            );
            Vector3 testPosition = desiredPosition + randomOffset;
            float minDistance = float.MaxValue;
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(testPosition, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            if (minDistance > bestDistance)
            {
                bestDistance = minDistance;
                bestPosition = testPosition;
            }
            if (minDistance >= minSpawnDistance)
            {
                Debug.Log($"Найдена безопасная позиция на попытке {i + 1}");
                return testPosition;
            }
        }

        Debug.Log($"Лучшая найденная позиция на расстоянии {bestDistance} от других врагов");
        return bestPosition;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, minSpawnDistance);
        Gizmos.color = Color.yellow;
        if (moveHorizontally)
        {
            Vector3 leftPos = new Vector3(startPosition.x - moveDistance, transform.position.y, 0);
            Vector3 rightPos = new Vector3(startPosition.x + moveDistance, transform.position.y, 0);
            Gizmos.DrawLine(leftPos, rightPos);
        }

        if (moveVertically)
        {
            Vector3 upPos = new Vector3(transform.position.x, startPosition.y + moveDistance * 0.5f, 0);
            Vector3 downPos = new Vector3(transform.position.x, startPosition.y - moveDistance * 0.5f, 0);
            Gizmos.DrawLine(upPos, downPos);
        }
        if (movementDirection.magnitude > 0.1f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, (Vector3)movementDirection);
        }
    }
    void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (graphicsTransform == null && transform.childCount > 0)
        {
            graphicsTransform = transform.GetChild(0);
        }
    }
}