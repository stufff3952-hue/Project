using UnityEngine;
using System.Collections.Generic;

public class SpringGenerator : MonoBehaviour
{
    [Header("ПРЕФАБ")]
    public GameObject springPrefab;

    [Header("НАСТРОЙКИ")]
    [Range(0, 100)] public float spawnChance = 30f; // Увеличим шанс для теста
    public float heightAbovePlatform = 0.3f;

    [Header("ОТЛАДКА")]
    public bool debugLogs = true;

    private List<GameObject> spawnedSprings = new List<GameObject>();

    void Start()
    {
        if (debugLogs)
        {
            Debug.Log("SpringGenerator запущен");
            Debug.Log("Префаб: " + (springPrefab != null ? "назначен" : "НЕ назначен!"));
        }
    }

    // Основной метод для спавна пружинок
    public void TrySpawnSpringOnPlatform(Vector2 platformPosition, float platformWidth)
    {
        if (springPrefab == null)
        {
            if (debugLogs) Debug.LogError("springPrefab не назначен!");
            return;
        }

        // Всегда логируем вызов
        if (debugLogs) Debug.Log($"Вызов TrySpawnSpringOnPlatform на позиции: {platformPosition}");

        // Проверяем шанс
        float randomChance = Random.Range(0f, 100f);
        if (debugLogs) Debug.Log($"Рандом: {randomChance}, нужно < {spawnChance}");

        if (randomChance > spawnChance)
        {
            if (debugLogs) Debug.Log("Шанс не выпал");
            return;
        }

        // Создаем пружинку
        float randomX = Random.Range(-platformWidth / 3f, platformWidth / 3f);
        Vector2 springPos = new Vector2(
            platformPosition.x + randomX,
            platformPosition.y + heightAbovePlatform
        );

        GameObject spring = Instantiate(springPrefab, springPos, Quaternion.identity, transform);
        spawnedSprings.Add(spring);

        if (debugLogs) Debug.Log($"✅ Пружинка создана на: {springPos}");
    }

    // Очистка старых пружинок
    public void CleanupOldSprings(float belowHeight)
    {
        if (debugLogs) Debug.Log($"Очистка пружинок ниже {belowHeight}");

        for (int i = spawnedSprings.Count - 1; i >= 0; i--)
        {
            if (spawnedSprings[i] == null)
            {
                spawnedSprings.RemoveAt(i);
            }
            else if (spawnedSprings[i].transform.position.y < belowHeight)
            {
                if (debugLogs) Debug.Log($"Удаление пружинки на {spawnedSprings[i].transform.position.y}");
                Destroy(spawnedSprings[i]);
                spawnedSprings.RemoveAt(i);
            }
        }
    }
}