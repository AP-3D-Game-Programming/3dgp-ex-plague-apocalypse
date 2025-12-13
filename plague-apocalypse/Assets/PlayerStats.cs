using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public struct CurrentPlayerStats
{
    public float totalDamageMultiplier;
    public float totalFireRateMultiplier;
    public float totalReloadSpeedMultiplier;
    public int totalMagazineSizeBonus;
    public float totalLuckMultiplier;
    public float totalLifeSteal;
    public bool hasBouncingBullets;
}
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public float fireRate = 1f;
    public bool bouncingBullets = false;
    public float lifeStealPerHit = 0f;

    [Header("Points")]
    public int points = 0;
    public TextMeshProUGUI pointsText;

    private float currentDisplayedPoints = 0;
    private Coroutine pointCoroutine;
    private Vector3 originalScale;

    public float shotPointsMultiplier = 1f;
    public float deathPointsMultiplier = 1f;
    public int maxShootPointsPerEnemy = 100;
    public int totalPointsEarned = 0;
    [Header("Global Weapon Modifiers")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float reloadSpeedMultiplier = 1f;
    public int magazineSizeBonus = 0;

    public Dictionary<WeaponType, float> typeDamageMults = new Dictionary<WeaponType, float>();
    public Dictionary<WeaponType, float> typeFireRateMults = new Dictionary<WeaponType, float>();
    private RoundManager _roundManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            // If the key doesn't exist yet, add it
            if (!typeDamageMults.ContainsKey(type)) typeDamageMults.Add(type, 1f);
            if (!typeFireRateMults.ContainsKey(type)) typeFireRateMults.Add(type, 1f);
        }
    }

    void Start()
    {
        _roundManager = FindObjectOfType<RoundManager>();
        if (pointsText != null) originalScale = pointsText.transform.localScale;
        UpdateUIInstant();
    }

    public float GetTotalDamageMult(WeaponType type)
    {
        if (typeDamageMults.ContainsKey(type))
            return damageMultiplier * typeDamageMults[type];

        return damageMultiplier;
    }


    public float GetTotalFireRateMult(WeaponType type)
    {
        if (typeFireRateMults.ContainsKey(type))
            return fireRateMultiplier * typeFireRateMults[type];

        return fireRateMultiplier;
    }


    public void AddPoints(int amount)
    {
        points += amount;

        if (_roundManager != null)
        {
            _roundManager.AddPointsToTotal(amount);
        }

        RefreshUIAnimated();
    }

    public void RemovePoints(int amount)
    {
        points = Mathf.Max(0, points - amount);
        RefreshUIAnimated();
    }

    public void SetPoints(int amount)
    {
        points = Mathf.Max(0, amount);
        RefreshUIAnimated();
    }

    private void RefreshUIAnimated()
    {
        if (pointCoroutine != null) StopCoroutine(pointCoroutine);
        pointCoroutine = StartCoroutine(AnimatePoints());
        StartCoroutine(PulseText());
    }

    private void UpdateUIInstant()
    {
        currentDisplayedPoints = points;
        if (pointsText != null) pointsText.text = $"{points}";
    }

    IEnumerator AnimatePoints()
    {
        while (Mathf.Abs(currentDisplayedPoints - points) > 0.5f)
        {
            currentDisplayedPoints = Mathf.Lerp(currentDisplayedPoints, points, Time.deltaTime * 10f);
            if (pointsText != null)
                pointsText.text = $"{Mathf.RoundToInt(currentDisplayedPoints)}";
            yield return null;
        }
        currentDisplayedPoints = points;
        if (pointsText != null) pointsText.text = $"{points}";
    }

    IEnumerator PulseText()
    {
        if (pointsText == null) yield break;
        float timer = 0;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            pointsText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, timer / 0.1f);
            yield return null;
        }
        timer = 0;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            pointsText.transform.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, timer / 0.2f);
            yield return null;
        }
        pointsText.transform.localScale = originalScale;
    }

    public void MultiplyFireRate(float multiplier)
    {
        fireRateMultiplier *= multiplier;
    }
    public CurrentPlayerStats GetCurrentPlayerMultipliers()
    {
        RoundManager rm = FindObjectOfType<RoundManager>();
        float currentLuck = (rm != null) ? rm.playerLuck : 1.0f;


        return new CurrentPlayerStats
        {

            totalDamageMultiplier = damageMultiplier,
            totalFireRateMultiplier = fireRateMultiplier,
            totalReloadSpeedMultiplier = reloadSpeedMultiplier,
            totalMagazineSizeBonus = magazineSizeBonus,

            totalLuckMultiplier = currentLuck,
            totalLifeSteal = lifeStealPerHit,
            hasBouncingBullets = bouncingBullets
        };
    }
}