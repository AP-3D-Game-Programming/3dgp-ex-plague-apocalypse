using UnityEngine;
using TMPro;
using System.Collections;

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

        if (pointsText != null) originalScale = pointsText.transform.localScale;
        UpdateUIInstant();
    }

    public void AddPoints(int amount)
    {
        points += amount;
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
        fireRate *= multiplier;
    }
}