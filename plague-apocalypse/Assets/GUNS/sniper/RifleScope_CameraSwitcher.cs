using UnityEngine;
using UnityEngine.UI; // ESSENTIEEL: Voor toegang tot de Canvas component

public class RifleScope_CameraSwitcher : MonoBehaviour {

    [Header("1. Camera To Switch")]
    [Tooltip("De hoofdcamera van de speler (First Person Camera).")]
    [SerializeField] private Camera playerCamera; 

    [Tooltip("De camera die het gezoomde beeld geeft (Main Scene Camera/Scope Camera).")]
    [SerializeField] private Camera scopeCamera; 
    
    [Header("2. UI Element")]
    [Tooltip("Het Canvas GameObject met de scope/reticle overlay.")]
    [SerializeField] private GameObject scopeOverlayObject; 
    
    // De Canvas component die we nodig hebben om de World Camera te wisselen
    private Canvas scopeCanvasComponent;
    
    // De AudioListener van de Player Camera
    private AudioListener playerCameraListener;

    private void Awake() {
        // --- 1. Controleer Toewijzingen ---
        if (playerCamera == null || scopeCamera == null || scopeOverlayObject == null) {
            Debug.LogError("Één of meer vereiste elementen zijn NIET toegewezen in de Inspector! Stop het script.");
            enabled = false;
            return;
        }

        // --- 2. Haal Componenten op ---
        
        // Haal Canvas Component op
        scopeCanvasComponent = scopeOverlayObject.GetComponent<Canvas>();
        if (scopeCanvasComponent == null) {
            Debug.LogError("Scope Overlay GameObject heeft GEEN Canvas component! Voeg deze toe.");
            enabled = false;
            return;
        }

        // Haal AudioListener op
        playerCameraListener = playerCamera.GetComponent<AudioListener>();
        
        // --- 3. Waarschuwingen voor Foutieve Configuraties ---
        
        if (scopeCamera.GetComponent<AudioListener>() != null) {
            Debug.LogError("Scope Camera mag GEEN AudioListener component hebben! Verwijder deze handmatig uit de Inspector.");
        }
    }

    private void Start() {
        // Zorg ervoor dat we starten met alles UIT
        SetScopeActive(false);
    }

    private void Update() {
        // Rechter muisknop ingedrukt houden -> scope aan
        if (Input.GetMouseButton(1)) 
            SetScopeActive(true);

        // Rechter muisknop loslaten -> scope uit
        if (Input.GetMouseButtonUp(1))
            SetScopeActive(false);
    }

    private void SetScopeActive(bool scoped) {
        if (playerCamera == null || scopeCamera == null || scopeOverlayObject == null || scopeCanvasComponent == null) {
            Debug.LogError("Één van de componenten is null! Check de Inspector toewijzingen.");
            return;
        }

        // 1. Schakel de Camera componenten
        scopeCamera.enabled = scoped;
        playerCamera.enabled = !scoped;
        
        // 2. Schakel het Scope Overlay GameObject AAN of UIT
        scopeOverlayObject.SetActive(scoped);
        
        // 3. Zorg dat Canvas op Screen Space - Overlay staat (geen camera nodig)
        if (scoped) {
            scopeCanvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            scopeCanvasComponent.sortingOrder = 100; // Voorop alles
        }

        // 4. Schakel de AudioListener component
        if (playerCameraListener != null) {
            playerCameraListener.enabled = playerCamera.enabled;
        }

        Debug.Log(scoped 
            ? $"✓ SCOPE AAN -> ScopeCamera={scopeCamera.enabled}, PlayerCamera={playerCamera.enabled}, Overlay={scopeOverlayObject.activeSelf}, Canvas RenderMode={scopeCanvasComponent.renderMode}"
            : $"✓ SCOPE UIT -> ScopeCamera={scopeCamera.enabled}, PlayerCamera={playerCamera.enabled}");
    }
}