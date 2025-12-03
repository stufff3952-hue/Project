using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    [Header("МОНЕТКИ")]
    public GameObject coinPrefab;
    public float spawnChance = 30f;

    [Header("ПОЗИЦИЯ")]
    public float minHeight = 0.5f;
    public float maxHeight = 1.5f;
    public void TrySpawnCoinOnPlatform(Vector2 platformPosition, float platformWidth)
    {
        if (coinPrefab == null) return;

        // Случайный шанс
        if (Random.Range(0f, 100f) > spawnChance) return;

        float randomX = Random.Range(-platformWidth / 2f, platformWidth / 2f);
        float randomY = Random.Range(minHeight, maxHeight);

        Vector2 coinPos = new Vector2(
            platformPosition.x + randomX,
            platformPosition.y + randomY
        );
        Instantiate(coinPrefab, coinPos, Quaternion.identity);

        Debug.Log($"Монетка создана на позиции: {coinPos}");
    }
}