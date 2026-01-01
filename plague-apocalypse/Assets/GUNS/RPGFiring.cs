using UnityEngine;


public class RPGFiring : MonoBehaviour
{
    [SerializeField] private GameObject _rocket;
    [SerializeField] private GameObject _rocketProp;  // Blijft altijd zichtbaar, wordt NIET verwijderd
    [SerializeField] private GameObject _dummyKogel;   // Extra visuele kogel die uit moet bij schot
    [SerializeField] private Transform _rocketPosition;
    
    [Header("Reload")]
    [SerializeField] private float _reloadTime = 3f;
    private bool _isReloading = false;

    void Start()
    {
        // Stop de smoke trail van de zichtbare rocket prop
        if (_rocketProp != null)
        {
            ParticleSystem smokeTrail = _rocketProp.GetComponentInChildren<ParticleSystem>();
            if (smokeTrail != null)
            {
                smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        // Zorg dat de extra dummy kogel zichtbaar staat bij start
        if (_dummyKogel != null)
        {
            _dummyKogel.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isReloading)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Fire Missle");

            if (_rocket == null)
            {
                Debug.LogError("[RPG] _rocket prefab is NULL! Wijs een PREFAB toe (uit je Assets folder), NIET een scene object!");
                return;
            }
            
            if (_rocketPosition == null)
            {
                Debug.LogError("[RPG] _rocketPosition is NULL!");
                return;
            }

            // BELANGRIJK: Check of _rocket een prefab is en niet een scene object
            if (_rocket.scene.IsValid())
            {
                Debug.LogError("[RPG] FOUT! _rocket is een SCENE object, geen PREFAB!");
                Debug.LogError("[RPG] Je moet een PREFAB uit je Assets toewijzen, niet de rocket die op de gun zit!");
                return;
            }

            Debug.Log($"[RPG] Spawning rocket prefab: {_rocket.name}");
            
            // Spawn exact op projectile/muzzle positie (eventueel mini offset als nodig)
            Vector3 spawnPos = _rocketPosition.position; // + _rocketPosition.forward * 0.05f;
            GameObject rocketObj = Instantiate(_rocket, spawnPos, _rocketPosition.rotation);
            
            Debug.Log($"[RPG] Rocket spawned successfully!");

            Rocket rocket = rocketObj.GetComponent<Rocket>();
            if (rocket != null)
            {
                // Richting = forward van projectile/muzzle
                rocket.Initialize(_rocketPosition.right);
            }
            else
            {
                Debug.LogWarning("Geïnstantieerde rocket mist Rocket-script");
            }

            // Zet de extra dummy kogel uit bij schot
            if (_dummyKogel != null)
            {
                _dummyKogel.SetActive(false);
            }

            // Start reload timer
            StartReload();
        }
    }

    private void StartReload()
    {
        if (_isReloading) return;
        StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        yield return new WaitForSeconds(_reloadTime);
        RestoreDummyRocket();
        _isReloading = false;
    }

    // Call this after reload to show the dummy rocket again
    public void RestoreDummyRocket()
    {
        if (_dummyKogel != null)
        {
            _dummyKogel.SetActive(true);
        }
    }
}