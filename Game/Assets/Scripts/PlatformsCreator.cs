using UnityEngine;
using System.Collections.Generic;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Платформы")]
    public GameObject platformPrefab;
    public int platformsOnScreen = 20;

    [Header("Область Генерации")]
    public float areaWidth = 6f;
    public float minX = -3f;
    public float maxX = 3f;

    [Header("Дистанция")]
    public float minDistanceX = 1.5f;
    public float maxDistanceX = 3.0f;
    public float minDistanceY = 1.0f;
    public float maxDistanceY = 2.2f;

    [Header("Случайность")]
    [Range(0, 100)] public float chanceSideJump = 25f;

    [Header("Монетки")]
    public CoinGenerator coinGenerator;
    public float platformWidth = 1f;

    [Header("Пружинки")]
    public GameObject springPrefab; // ПРЕФАБ ПРУЖИНКИ
    [Range(0, 100)] public float springSpawnChance = 30f; // Шанс спавна пружинки
    public float springHeightAbovePlatform = 0.3f;
    public float springXOffsetRange = 0.3f;
    public float minDistanceBetweenSprings = 3f; // Минимальное расстояние между пружинками

    [Header("Избегание врагов")]
    public bool avoidEnemies = true;
    public float minDistanceToEnemy = 2f;

    private List<GameObject> platforms = new List<GameObject>();
    private List<GameObject> spawnedSprings = new List<GameObject>(); // Список созданных пружинок
    private List<Vector2> springPositions = new List<Vector2>(); // Позиции пружинок
    private Vector2 lastPlatformPosition;
    private Transform player;
    private float nextGenerationHeight;
    private SpawnerEnemy enemySpawner;

    // Для последовательности спавна пружинок
    private int platformsSinceLastSpring = 0;
    private int targetPlatformsBetweenSprings = 0;
    private const int MIN_PLATFORMS_BETWEEN_SPRINGS = 3;
    private const int MAX_PLATFORMS_BETWEEN_SPRINGS = 8;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        enemySpawner = FindAnyObjectByType<SpawnerEnemy>();

        minX = -areaWidth / 2f;
        maxX = areaWidth / 2f;

        // Инициализируем случайное значение для последовательности пружинок
        targetPlatformsBetweenSprings = Random.Range(MIN_PLATFORMS_BETWEEN_SPRINGS, MAX_PLATFORMS_BETWEEN_SPRINGS + 1);

        GenerateStartPlatform();
        GenerateMorePlatforms();

        Debug.Log("PlatformGenerator запущен");
        Debug.Log("Префаб пружинки: " + (springPrefab != null ? "назначен" : "НЕ назначен!"));
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.y > nextGenerationHeight - 10f)
        {
            GenerateMorePlatforms();
        }

        RemoveOldPlatforms();

        // Очистка старых монеток
        if (coinGenerator != null)
        {
            coinGenerator.CleanupOldCoins(player.position.y - 20f);
        }

        // Очистка старых пружинок
        CleanupOldSprings(player.position.y - 20f);
    }

    void GenerateStartPlatform()
    {
        Vector2 startPos = player != null ?
            new Vector2(player.position.x, player.position.y - 1.5f) :
            new Vector2(0, 0);
        startPos.x = Mathf.Clamp(startPos.x, minX, maxX);
        GameObject platform = Instantiate(platformPrefab, startPos, Quaternion.identity, transform);
        platforms.Add(platform);
        lastPlatformPosition = startPos;
        nextGenerationHeight = startPos.y + 8f;

        Debug.Log($"Стартовая платформа создана: {startPos}");
    }

    void GenerateMorePlatforms()
    {
        while (platforms.Count < platformsOnScreen)
        {
            GenerateNextPlatform();
        }
    }

    void GenerateNextPlatform()
    {
        Vector2 newPos;
        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            newPos = CalculatePlatformPosition();
            attempts++;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"Не удалось найти позицию для платформы после {maxAttempts} попыток");
                break;
            }
        }
        while (!IsPositionSafeForPlatform(newPos));

        lastPlatformPosition = newPos;
        nextGenerationHeight = Mathf.Max(nextGenerationHeight, newPos.y + 6f);

        GameObject platform = Instantiate(platformPrefab, newPos, Quaternion.identity, transform);
        platforms.Add(platform);

        // Генерация монеток
        if (coinGenerator != null)
        {
            coinGenerator.TrySpawnCoinOnPlatform(newPos, platformWidth);
        }

        // Генерация пружинок
        TrySpawnSpringOnPlatform(newPos, platformWidth);
    }

    Vector2 CalculatePlatformPosition()
    {
        float randomX;
        if (Random.Range(0f, 100f) < chanceSideJump)
        {
            randomX = Random.Range(minX, maxX);
        }
        else
        {
            float direction = Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
            float offsetX = Random.Range(minDistanceX, maxDistanceX) * direction;
            randomX = lastPlatformPosition.x + offsetX;

            if (randomX < minX || randomX > maxX)
            {
                direction *= -1;
                offsetX = Random.Range(minDistanceX, maxDistanceX) * direction;
                randomX = lastPlatformPosition.x + offsetX;
            }

            randomX = Mathf.Clamp(randomX, minX, maxX);
        }

        float randomY = lastPlatformPosition.y + Random.Range(minDistanceY, maxDistanceY);
        return new Vector2(randomX, randomY);
    }

    bool IsPositionSafeForPlatform(Vector2 position)
    {
        if (!avoidEnemies || enemySpawner == null) return true;

        // Проверяем расстояние до других платформ
        foreach (GameObject platform in platforms)
        {
            if (platform == null) continue;

            float distance = Vector2.Distance(position, platform.transform.position);
            if (distance < 1.5f)
            {
                return false;
            }
        }

        // Проверяем, не слишком ли близко к зоне спавна врагов
        float enemySpawnHeight = player.position.y + enemySpawner.spawnHeightAbovePlayer;

        if (Mathf.Abs(position.y - enemySpawnHeight) < 3f)
        {
            // Проверяем горизонтальное расстояние до врагов
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null) continue;

                float horizontalDist = Mathf.Abs(position.x - enemy.transform.position.x);
                float verticalDist = Mathf.Abs(position.y - enemy.transform.position.y);

                if (horizontalDist < minDistanceToEnemy && verticalDist < 3f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // МЕТОД ДЛЯ ГЕНЕРАЦИИ ПРУЖИНОК
    void TrySpawnSpringOnPlatform(Vector2 platformPosition, float platformWidth)
    {
        if (springPrefab == null)
        {
            Debug.LogError("springPrefab не назначен! Пружинки не будут создаваться.");
            return;
        }

        // Система последовательности: пропускаем платформы между пружинками
        platformsSinceLastSpring++;

        if (platformsSinceLastSpring < targetPlatformsBetweenSprings)
        {
            Debug.Log($"Пропуск платформы для пружинки ({platformsSinceLastSpring}/{targetPlatformsBetweenSprings})");
            return;
        }

        // Проверяем шанс
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > springSpawnChance)
        {
            Debug.Log($"Шанс на пружинку не выпал: {randomChance:F1} > {springSpawnChance}");
            ResetSpringSequence();
            return;
        }

        // Рассчитываем позицию пружинки
        Vector2 springPos = CalculateSpringPosition(platformPosition, platformWidth);

        // Проверяем, можно ли разместить пружинку
        if (!IsSpringPositionValid(springPos))
        {
            Debug.Log($"Нельзя разместить пружинку в позиции {springPos} (слишком близко к другой)");
            ResetSpringSequence();
            return;
        }

        // Создаем пружинку
        CreateSpring(springPos);

        // Сбрасываем счетчик и устанавливаем новую цель
        ResetSpringSequence();
        targetPlatformsBetweenSprings = Random.Range(MIN_PLATFORMS_BETWEEN_SPRINGS, MAX_PLATFORMS_BETWEEN_SPRINGS + 1);

        Debug.Log($"✅ Пружинка создана! Следующая через {targetPlatformsBetweenSprings} платформ");
    }

    Vector2 CalculateSpringPosition(Vector2 platformPosition, float platformWidth)
    {
        float randomX = Random.Range(-platformWidth / 2f * springXOffsetRange,
                                    platformWidth / 2f * springXOffsetRange);

        return new Vector2(
            platformPosition.x + randomX,
            platformPosition.y + springHeightAbovePlatform
        );
    }

    bool IsSpringPositionValid(Vector2 position)
    {
        // Проверка расстояния до других пружинок
        foreach (Vector2 springPos in springPositions)
        {
            if (Vector2.Distance(position, springPos) < minDistanceBetweenSprings)
            {
                return false;
            }
        }

        return true;
    }

    void CreateSpring(Vector2 position)
    {
        GameObject spring = Instantiate(springPrefab, position, Quaternion.identity, transform);
        spawnedSprings.Add(spring);
        springPositions.Add(position);

        Debug.Log($"✅ Пружинка создана на позиции: {position}");
    }

    void ResetSpringSequence()
    {
        platformsSinceLastSpring = 0;
    }

    void CleanupOldSprings(float belowHeight)
    {
        for (int i = spawnedSprings.Count - 1; i >= 0; i--)
        {
            if (spawnedSprings[i] == null)
            {
                spawnedSprings.RemoveAt(i);
                if (i < springPositions.Count)
                {
                    springPositions.RemoveAt(i);
                }
            }
            else if (spawnedSprings[i].transform.position.y < belowHeight)
            {
                Debug.Log($"Удаление пружинки на высоте {spawnedSprings[i].transform.position.y}");
                Destroy(spawnedSprings[i]);
                spawnedSprings.RemoveAt(i);
                if (i < springPositions.Count)
                {
                    springPositions.RemoveAt(i);
                }
            }
        }

        // Также очищаем старые позиции
        CleanupOldSpringPositions(belowHeight);
    }

    void CleanupOldSpringPositions(float belowHeight)
    {
        for (int i = springPositions.Count - 1; i >= 0; i--)
        {
            if (springPositions[i].y < belowHeight)
            {
                springPositions.RemoveAt(i);
            }
        }
    }

    public void ClearAreaForEnemy(float centerX, float centerY, float radius)
    {
        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            if (platforms[i] == null) continue;

            Vector2 platformPos = platforms[i].transform.position;
            float distance = Vector2.Distance(new Vector2(centerX, centerY), platformPos);

            if (distance < radius)
            {
                Destroy(platforms[i]);
                platforms.RemoveAt(i);
            }
        }
    }

    void RemoveOldPlatforms()
    {
        if (player == null) return;

        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            if (platforms[i] == null)
            {
                platforms.RemoveAt(i);
            }
            else if (platforms[i].transform.position.y < player.position.y - 20f)
            {
                Destroy(platforms[i]);
                platforms.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        float centerX = (minX + maxX) / 2f;
        float width = maxX - minX;

        Vector3 center = new Vector3(centerX, 0, 0);
        Vector3 size = new Vector3(width, 0.1f, 0);
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(minX, -100, 0), new Vector3(minX, 100, 0));
        Gizmos.DrawLine(new Vector3(maxX, -100, 0), new Vector3(maxX, 100, 0));

        // Визуализация созданных пружинок (только в Play Mode)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            foreach (GameObject spring in spawnedSprings)
            {
                if (spring != null)
                {
                    Gizmos.DrawWireSphere(spring.transform.position, 0.3f);
                }
            }
        }
    }
}