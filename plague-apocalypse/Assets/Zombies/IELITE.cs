using UnityEngine;

public interface IElite
{
    void ApplyStats(
        int newHealth,
        float newSpeed,
        RoundManager roundManager,
        float fireRateMult,
        float damageMult,
        float phase2HealthMult,
        float phase2SpeedMult
    );
    void TakeDamage(int damageAmount);
}
