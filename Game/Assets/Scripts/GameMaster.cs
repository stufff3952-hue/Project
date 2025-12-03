using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Интерфейс")]
    public Text scoreText;
    public Text coinText;
    public Text highScoreText;

    [Header("Счетчик монеток")]
    public SimpleCoinCounter coinCounter;

    [Header("ДАННЫЕ")]
    public int score = 0;
    public int coins = 0;
    public int totalCoins = 0;
    public int highScore = 0;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        UpdateUI();
    }

    // Добавить очки
    public void AddScore(int points)
    {
        score += points;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateUI();
    }

    // Добавить монетку
    public void AddCoin()
    {
        coins++;
        totalCoins++;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        AddScore(10);
        UpdateUI();
        if (coinCounter != null)
            coinCounter.AddCoin();

        Debug.Log($"Монеток собрано: {coins} (всего: {totalCoins})");
    }
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Очки: {score}";

        if (coinText != null)
            coinText.text = $"Монеты: {coins}";

        if (highScoreText != null)
            highScoreText.text = $"Рекорд: {highScore}";
    }
    // Сбросить текущую сессию (при смерти)
    public void ResetSession()
    {
        score = 0;
        coins = 0;
        UpdateUI();
        if (coinCounter != null)
            coinCounter.ResetCounter();
    }
    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}