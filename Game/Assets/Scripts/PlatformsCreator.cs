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
    private List<GameObject> platforms = new List<GameObject>();
    private Vector2 lastPlatformPosition;
    private Transform player;
    private float nextGenerationHeight;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        minX = -areaWidth / 2f;
        maxX = areaWidth / 2f;
        GenerateStartPlatform();
        GenerateMorePlatforms();
    }
    void Update()
    {
        if (player == null) return;

        if (player.position.y > nextGenerationHeight - 10f)
        {
            GenerateMorePlatforms();
        }

        RemoveOldPlatforms();
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
        Vector2 newPos = new Vector2(randomX, randomY);
        lastPlatformPosition = newPos;
        nextGenerationHeight = Mathf.Max(nextGenerationHeight, newPos.y + 6f);
        GameObject platform = Instantiate(platformPrefab, newPos, Quaternion.identity, transform);
        platforms.Add(platform);

        // ГЕНЕРАЦИЯ МОНЕТКИ
        if (coinGenerator != null)
        {
            coinGenerator.TrySpawnCoinOnPlatform(newPos, platformWidth);
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
    }
}