using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 10.0f;
    [SerializeField] private float _lifeTimeSeconds = 10f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _smokeTrail;

    [Header("Explosion Settings")]
    // **BELANGRIJK: Hier sleep je de Explosie Prefab naartoe in de Inspector**
    [SerializeField] private GameObject _explosionPrefab; 
    [SerializeField] private float _explosionForce = 20f; 
    [SerializeField] private float _radius = 5f; 
    [SerializeField] private float _upwardModifier = 3.0F; 

    private BoxCollider _boxCollider;
    private MeshRenderer _meshRenderer;
    private Rigidbody _rb;
    private Vector3 _direction;
    private bool _initialized;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        // Opmerking: GameObject.Find("Explosion") is verwijderd omdat we een Prefab gebruiken.
    }

    void Start()
    {
        // Geen auto-fire op load: alleen bewegen na expliciete Initialize
        if (!_initialized && _rb != null)
        {
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
        }
        if (!_initialized && _smokeTrail != null)
        {
            _smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Zorg ervoor dat deze methode PUBLIC is, zodat RPGFiring deze kan aanroepen.
    public void Initialize(Vector3 direction) 
    {
        // Gebruik de meegegeven richting; fallback naar prefab forward
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward.normalized;
        _initialized = true;

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.linearVelocity = _direction * _speed;
        }

        if (_smokeTrail != null)
        {
            _smokeTrail.Play();
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // Arm pas na korte delay zodat we niet direct botsen met de launcher
            if (_boxCollider != null)
            {
                _boxCollider.enabled = false;
                StartCoroutine(ArmColliderAfter(0.1f));
            }
        }

        // Verwijder rocket automatisch na lifespan
        Destroy(gameObject, _lifeTimeSeconds);
    }

    void FixedUpdate()
    {
        if (!_initialized)
            return;

        // Backup movement als er geen rigidbody is
        if (_rb == null && _initialized)
        {
            transform.Translate(_direction * Time.fixedDeltaTime * _speed, Space.World);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Rocket hit: " + collision.gameObject.name);

        // Stop de beweging van de kogel
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
        }
        
        // --- Explosie Logica (Visueel & Kracht) ---
        Vector3 explosionPos = transform.position;

        // 1. Toon het visuele effect door de PREFAB te instantieren
        if (_explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);
            
            ParticleSystem ps = explosionInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                // Vernietig het explosie-object zodra het effect is uitgespeeld.
                Destroy(explosionInstance, ps.main.duration + ps.main.startLifetime.constantMax); 
            }
            else
            {
                 // Als er geen ParticleSystem is, vernietig het dan na 3 seconden
                 Destroy(explosionInstance, 3f); 
            }
        }
        
        // 2. Pas de fysieke kracht toe
        Collider[] colliders = Physics.OverlapSphere(explosionPos, _radius);
        
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(_explosionForce, explosionPos, _radius, _upwardModifier, ForceMode.Impulse);
            }
        }
        // --- Einde Explosie Logica ---

        // Ruim de kogel op
        _speed = 0;
        if (_boxCollider != null) _boxCollider.enabled = false;
        if (_meshRenderer != null) _meshRenderer.enabled = false;
        if (_smokeTrail != null) _smokeTrail.Stop();
        
        // Verwijder de rocket na collision
        Destroy(gameObject, 0.1f);
    }

    private System.Collections.IEnumerator ArmColliderAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_boxCollider != null)
        {
            _boxCollider.enabled = true;
        }
    }
}