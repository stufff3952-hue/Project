using UnityEngine;
using UnityEngine.UI;
public class SimpleCoinCounter : MonoBehaviour
{
    [Header("Ссылка на текст")]
    public Text coinText;

    private int currentCoins = 0;

    void Start()
    {
        if (coinText == null)
        {
            coinText = GetComponentInChildren<Text>();
            Debug.Log("Найден текст: " + coinText?.name);
        }
        UpdateDisplay();
    }

    public void AddCoin()
    {
        currentCoins++;
        UpdateDisplay();
        Debug.Log("Монеток: " + currentCoins);
    }
    void UpdateDisplay()
    {
        if (coinText != null)
        {
            coinText.text = $"×{currentCoins}";
        }
        else
        {
            Debug.LogError("coinText не назначен!");
        }
    }
    public void ResetCounter()
    {
        currentCoins = 0;
        UpdateDisplay();
    }
}
