# RPG7 gebruiksnotities

## Componenten
- **RPGFiring (script)**
  - `_rocket`: prefab van de echte projectile (met explosie/smoke). Kies uit Assets, niet uit de scene.
  - `_rocketProp`: visuele rocket op de launcher (dummy, geen script). Blijft op de gun.
  - `_dummyKogel`: extra visuele kogel die uit gaat bij schieten en terugkomt na reload.
  - `_rocketPosition`: muzzle/ProjectilePos transform; zijn forward bepaalt schietrichting en spawnlocatie.
  - `_reloadTime`: tijd in seconden voor herladen (default 3s).

## Werking
1) Bij Start: dummy’s zichtbaar, smoke op prop uit.
2) Bij klik (Mouse Button 1):
   - Spawn rocket prefab op `_rocketPosition.position` met `_rocketPosition.rotation`.
   - Richting = `_rocketPosition.forward`.
   - `_dummyKogel` wordt uitgezet.
   - Reload start; tijdens reload kun je niet schieten.
3) Na `_reloadTime`: `RestoreDummyRocket()` wordt intern aangeroepen; `_dummyKogel` weer aan, schieten weer mogelijk.

## Instellen in Unity
1. Voeg `RPGFiring` toe aan je RPG wapen object.
2. Vul in de Inspector:
   - `_rocket`: drag de prefab "rpg 7 bullet" (met explosie/smoke).
   - `_rocketProp`: het zichtbare rocket-mesh op de gun (dummy).
   - `_dummyKogel`: tweede visuele bullet (dummy) die je tijdelijk wilt verbergen.
   - `_rocketPosition`: het muzzle/ProjectilePos child; zorg dat zijn **blue Z (forward)** naar buiten wijst.
   - `_reloadTime`: bijvoorbeeld 3.
3. Test Play:
   - Klik: echte raket schiet vooruit; dummy wordt onzichtbaar.
   - Na reload: dummy komt terug.

## Tips
- Als de raket van de zijkant schiet, controleer de forward-as van `_rocketPosition` (Z+ moet naar buiten).
- Pas snelheid/force aan in `Rocket` prefab (velden `_speed`, `_explosionForce`).
- `_rocket` moet een prefab zijn, geen scene object; anders krijg je een console error.
