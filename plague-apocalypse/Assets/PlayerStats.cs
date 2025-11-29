using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{

    public float fireRate = 1f;
    public bool bouncingBullets = false;

    public static PlayerStats Instance;

    public float lifeStealPerHit = 0f;

    [Header("Points")]
    public int points = 0;
    public TextMeshProUGUI pointsText;

    public float shotPointsMultiplier = 1f;
    public float deathPointsMultiplier = 1f;
    public int maxShootPointsPerEnemy = 100;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdateUI();
    }

    public void RemovePoints(int amount)
    {
        points = Mathf.Max(0, points - amount);
        UpdateUI();
    }

    public void SetPoints(int amount)
    {
        points = Mathf.Max(0, amount);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = $"{points}";
    }

    public void MultiplyFireRate(float multiplier)
    {
        fireRate *= multiplier;
    }
}