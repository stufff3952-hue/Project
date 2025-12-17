using UnityEngine;
using System.Collections.Generic;

public class CoinGenerator : MonoBehaviour
{
    [Header("МОНЕТКИ")]
    public GameObject coinPrefab;
    [Range(0, 100)] public float spawnChance = 15f;

    [Header("ПОЗИЦИЯ НАД ПЛАТФОРМОЙ")]
    public float heightAbovePlatform = 2.5f;
    public float xOffsetRange = 0.4f;

    [Header("ПОСЛЕДОВАТЕЛЬНОСТЬ")]
    public bool enablePatternSpawning = true;
    private int platformsSinceLastCoin = 0;
    public int minPlatformsBetweenCoins = 3;
    public int maxPlatformsBetweenCoins = 8;
    private int targetPlatformsBetweenCoins = 0;

    [Header("ОГРАНИЧЕНИЯ")]
    public float minDistanceBetweenCoins = 3f;

    [Header("ОТЛАДКА")]
    public bool debugLogs = false;
    private Transform player;
    private List<GameObject> spawnedCoins = new List<GameObject>();
    private List<Vector2> previousCoinPositions = new List<Vector2>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        targetPlatformsBetweenCoins = Random.Range(minPlatformsBetweenCoins, maxPlatformsBetweenCoins + 1);

        if (debugLogs)
        {
            Debug.Log("CoinGenerator запущен (редкие бонусы)");
        }
    }
    void Update()
    {
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            if (spawnedCoins[i] == null)
            {
                spawnedCoins.RemoveAt(i);
            }
        }
    }

    public void TrySpawnCoinOnPlatform(Vector2 platformPosition, float platformWidth)
    {
        if (coinPrefab == null)
        {
            Debug.LogError("coinPrefab не назначен в CoinGenerator!");
            return;
        }

        if (enablePatternSpawning)
        {
            platformsSinceLastCoin++;

            if (platformsSinceLastCoin < targetPlatformsBetweenCoins)
            {
                if (debugLogs) Debug.Log($"Пропуск платформы ({platformsSinceLastCoin}/{targetPlatformsBetweenCoins})");
                return;
            }
        }
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > spawnChance)
        {
            if (debugLogs) Debug.Log($"Шанс не выпал: {randomChance:F1} > {spawnChance}");
            ResetSequence();
            return;
        }
        Vector2 coinPos = CalculateCoinPosition(platformPosition, platformWidth);

        if (!IsPositionValid(coinPos))
        {
            if (debugLogs) Debug.Log($"Позиция занята или невалидна: {coinPos}");
            ResetSequence();
            return;
        }
        CreateCoin(coinPos);
        ResetSequence();
        targetPlatformsBetweenCoins = Random.Range(minPlatformsBetweenCoins, maxPlatformsBetweenCoins + 1);

        if (debugLogs) Debug.Log($"Монетка создана! Следующая через {targetPlatformsBetweenCoins} платформ");
    }
    Vector2 CalculateCoinPosition(Vector2 platformPosition, float platformWidth)
    {
        float randomX = Random.Range(-platformWidth / 2f * xOffsetRange,
                                    platformWidth / 2f * xOffsetRange);

        return new Vector2(
            platformPosition.x + randomX,
            platformPosition.y + heightAbovePlatform
        );
    }
    bool IsPositionValid(Vector2 position)
    {
        foreach (Vector2 prevPos in previousCoinPositions)
        {
            if (Vector2.Distance(position, prevPos) < minDistanceBetweenCoins)
            {
                return false;
            }
        }

        return true;
    }
    void CreateCoin(Vector2 position)
    {
        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity, transform);
        spawnedCoins.Add(coin);
        previousCoinPositions.Add(position);

        if (debugLogs) Debug.Log($"✅ Создана монетка на позиции: {position}");

    }

    void ResetSequence()
    {
        platformsSinceLastCoin = 0;
    }

    void CleanupOldPositions()
    {
        if (player == null) return;

        for (int i = previousCoinPositions.Count - 1; i >= 0; i--)
        {
            if (previousCoinPositions[i].y < player.position.y - 25f)
            {
                previousCoinPositions.RemoveAt(i);
            }
        }
    }
    public void CleanupOldCoins(float belowHeight)
    {
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            if (spawnedCoins[i] == null)
            {
                spawnedCoins.RemoveAt(i);
            }
            else if (spawnedCoins[i].transform.position.y < belowHeight)
            {
                Destroy(spawnedCoins[i]);
                spawnedCoins.RemoveAt(i);
            }
        }

        CleanupOldPositions();
    }
    void OnDrawGizmosSelected()
    {
        if (!debugLogs) return;

        Gizmos.color = Color.yellow;
        foreach (GameObject coin in spawnedCoins)
        {
            if (coin != null)
            {
                Gizmos.DrawWireSphere(coin.transform.position, 0.3f);
            }
        }
    }
}