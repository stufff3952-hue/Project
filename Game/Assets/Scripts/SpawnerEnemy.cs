using UnityEngine;
using System.Collections.Generic;

public class SpawnerEnemy : MonoBehaviour
{
    [Header("ПРЕФАБ ВРАГА")]
    public GameObject enemyPrefab;

    [Header("НАСТРОЙКИ СПАВНА")]
    public int maxEnemiesOnScreen = 3;
    public float spawnHeightAbovePlayer = 12f;
    public float minSpawnHeightStep = 15f;
    public float spawnXRange = 3f;
    [Header("РАСПРЕДЕЛЕНИЕ ВРАГОВ")]
    public float minDistanceBetweenEnemies = 3f;
    public int maxSpawnAttempts = 10;

    [Header("СЛУЧАЙНОСТЬ")]
    [Range(0, 100)] public float spawnChance = 60f;

    private Transform player;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float nextSpawnHeight;
    private float highestPlayerY;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            highestPlayerY = player.position.y;
            nextSpawnHeight = player.position.y + minSpawnHeightStep;
        }
        else
        {
            Debug.LogError("Не найден объект с тегом 'Player'!");
        }

        Enemy.minSpawnDistance = minDistanceBetweenEnemies;

        Debug.Log($"Спавнер врагов запущен. Максимум врагов: {maxEnemiesOnScreen}");
    }

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        if (player.position.y > highestPlayerY)
        {
            highestPlayerY = player.position.y;
        }
        if (highestPlayerY > nextSpawnHeight - 5f)
        {
            TrySpawnEnemy();
        }
        CleanupOldEnemies();
    }

    void TrySpawnEnemy()
    {
        if (spawnedEnemies.Count >= maxEnemiesOnScreen)
        {
            Debug.Log($"Достигнут лимит врагов: {spawnedEnemies.Count}/{maxEnemiesOnScreen}");
            nextSpawnHeight = highestPlayerY + minSpawnHeightStep;
            return;
        }

        if (Random.Range(0f, 100f) > spawnChance)
        {
            Debug.Log("Шанс спавна не выпал, пропускаем");
            nextSpawnHeight = highestPlayerY + minSpawnHeightStep;
            return;
        }

        if (SpawnSingleEnemy())
        {
            nextSpawnHeight = highestPlayerY + minSpawnHeightStep;
        }
        else
        {
            nextSpawnHeight = highestPlayerY + (minSpawnHeightStep * 0.7f);
        }
    }

    bool SpawnSingleEnemy()
    {
        Vector3 spawnPosition = FindSafeEnemyPosition();

        if (spawnPosition == Vector3.zero)
        {
            Debug.Log("Не удалось найти безопасное место для врага");
            return false;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);

        Debug.Log($"Враг создан на позиции: {spawnPosition}. Всего врагов: {spawnedEnemies.Count}");
        return true;
    }
    Vector3 FindSafeEnemyPosition()
    {
        float safeZoneMinY = highestPlayerY + spawnHeightAbovePlayer - 3f;
        float safeZoneMaxY = highestPlayerY + spawnHeightAbovePlayer + 3f;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float randomY = Random.Range(safeZoneMinY, safeZoneMaxY);
            float randomX = player.position.x + Random.Range(-spawnXRange, spawnXRange);

            Vector3 testPosition = new Vector3(randomX, randomY, 0);
            if (IsPositionSafeForEnemy(testPosition))
            {
                return testPosition;
            }
        }

        return Vector3.zero;
    }

    bool IsPositionSafeForEnemy(Vector3 position)
    {
        if (!Enemy.CanSpawnAtPosition(position))
        {
            return false;
        }

        if (IsPositionBlockingPath(position))
        {
            return false;
        }

        if (IsInPlatformCorridor(position))
        {
            return false;
        }

        if (HasTooManyEnemiesNearby(position))
        {
            return false;
        }

        return true;
    }
    bool IsPositionBlockingPath(Vector3 position)
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        List<GameObject> platformsAbove = new List<GameObject>();
        List<GameObject> platformsBelow = new List<GameObject>();

        foreach (GameObject platform in platforms)
        {
            if (platform == null) continue;

            float verticalDist = platform.transform.position.y - position.y;

            if (verticalDist > 0 && verticalDist < 8f)
            {
                platformsAbove.Add(platform);
            }
            else if (verticalDist < 0 && verticalDist > -8f)
            {
                platformsBelow.Add(platform);
            }
        }
        if (platformsAbove.Count > 0 && platformsBelow.Count > 0)
        {
            foreach (GameObject platformBelow in platformsBelow)
            {
                foreach (GameObject platformAbove in platformsAbove)
                {
                    float horizontalDistBelow = Mathf.Abs(position.x - platformBelow.transform.position.x);
                    float horizontalDistAbove = Mathf.Abs(position.x - platformAbove.transform.position.x);
                    if (horizontalDistBelow < 1.5f && horizontalDistAbove < 1.5f)
                    {
                        Debug.Log("Враг блокирует прямой путь между платформами!");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool IsInPlatformCorridor(Vector3 position)
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");

        foreach (GameObject platform1 in platforms)
        {
            if (platform1 == null) continue;

            foreach (GameObject platform2 in platforms)
            {
                if (platform2 == null || platform1 == platform2) continue;
                if (Mathf.Abs(platform1.transform.position.y - platform2.transform.position.y) < 2f)
                {
                    float minX = Mathf.Min(platform1.transform.position.x, platform2.transform.position.x);
                    float maxX = Mathf.Max(platform1.transform.position.x, platform2.transform.position.x);
                    float corridorY = (platform1.transform.position.y + platform2.transform.position.y) / 2f;
                    if (position.x > minX && position.x < maxX &&
                        Mathf.Abs(position.y - corridorY) < 2f)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool HasTooManyEnemiesNearby(Vector3 position)
    {
        int enemiesNearby = 0;
        float checkRadius = 5f;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < checkRadius)
            {
                enemiesNearby++;
                if (enemiesNearby >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }
    void CleanupOldEnemies()
    {
        if (player == null) return;

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
            else if (spawnedEnemies[i].transform.position.y < player.position.y - 25f)
            {
                Destroy(spawnedEnemies[i]);
                spawnedEnemies.RemoveAt(i);
            }
        }
    }
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Vector3 spawnCenter = new Vector3(
                player.position.x,
                highestPlayerY + spawnHeightAbovePlayer,
                0
            );
            Gizmos.DrawWireSphere(spawnCenter, 3f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(-100, nextSpawnHeight, 0),
                new Vector3(100, nextSpawnHeight, 0)
            );
        }
    }
}