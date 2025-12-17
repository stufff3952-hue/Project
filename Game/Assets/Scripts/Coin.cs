using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("НАСТРОЙКИ")]
    public int coinValue = 10;
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;

    [Header("ЭФФЕКТЫ")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance не найден! Монетка может не работать корректно.");
        }
    }
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
    void CollectCoin()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(coinValue);
            GameManager.Instance.AddCoin();
        }
        else
        {
            Debug.LogError("GameManager.Instance равен null! Очки не добавлены.");
        }

        PlayCollectionEffect();
        Destroy(gameObject);
    }
    void PlayCollectionEffect()
    {
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        Debug.Log($"Монетка собрана! +{coinValue} очков");
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}