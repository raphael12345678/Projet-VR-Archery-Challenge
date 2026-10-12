using UnityEngine;

/// <summary>
/// Fait bouger une cible en va-et-vient.
/// La vitesse et l'amplitude de base sont fixées par le PartieManager selon le niveau.
/// Chaque cible part d'un point aléatoire de sa course, dans un sens au hasard,
/// avec sa propre vitesse et sa propre amplitude, et change de vitesse de temps en temps :
/// les cibles ne bougent jamais au même rythme.
/// À placer sur la racine d'une cible (avec le script Cible).
/// </summary>
public class CibleMobile : MonoBehaviour
{
    public enum Axe { Horizontal, Vertical }

    [Tooltip("Direction du déplacement, par rapport à l'orientation de la cible")]
    [SerializeField] private Axe axe = Axe.Horizontal;
    [Tooltip("Vitesse (m/s), remplacée par le niveau au lancement")]
    [SerializeField] private float vitesse = 1f;
    [Tooltip("Distance de chaque côté (m), remplacée par le niveau au lancement")]
    [SerializeField] private float amplitude = 1.5f;

    [Header("Variété (pour casser le rythme)")]
    [Tooltip("Variation aléatoire de la vitesse : 0.3 = entre 70 % et 130 % de la vitesse du niveau")]
    [Range(0f, 0.8f)] [SerializeField] private float variationVitesse = 0.3f;
    [Tooltip("Variation aléatoire de l'amplitude : 0.3 = entre 70 % et 130 %")]
    [Range(0f, 0.8f)] [SerializeField] private float variationAmplitude = 0.3f;
    [Tooltip("Mouvement fluide (ralentit aux extrémités) au lieu d'un aller-retour sec")]
    [SerializeField] private bool mouvementFluide = true;
    [Tooltip("Change de vitesse de temps en temps pendant la partie")]
    [SerializeField] private bool vitesseChangeante = true;
    [Tooltip("Intervalle min / max (s) entre deux changements de vitesse")]
    [SerializeField] private Vector2 intervalleChangement = new Vector2(2f, 5f);

    private Vector3 positionDepart;
    private float phase;              // position dans le cycle (0 à 1)
    private float facteurVitesse = 1f;
    private float facteurVitesseCible = 1f;
    private float facteurAmplitude = 1f;
    private float prochainChangement;
    private float sens = 1f;          // +1 ou -1 : part vers la droite ou vers la gauche

    public bool Active { get; set; } = true;

    private void Awake()
    {
        positionDepart = transform.position;

        // Un Rigidbody cinématique indique à la physique que ce collider bouge
        if (!TryGetComponent(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        TirerAuHasard();
    }

    /// <summary>Appelée par le PartieManager selon le niveau choisi.</summary>
    public void Configurer(float nouvelleVitesse, float nouvelleAmplitude, bool active)
    {
        vitesse = nouvelleVitesse;
        amplitude = nouvelleAmplitude;
        Active = active;
        TirerAuHasard();
        if (!active) transform.position = positionDepart;
    }

    private void TirerAuHasard()
    {
        phase = Random.value;                         // point de départ différent pour chaque cible
        sens = Random.value < 0.5f ? -1f : 1f;        // sens de départ au hasard
        facteurAmplitude = 1f + Random.Range(-variationAmplitude, variationAmplitude);
        facteurVitesse = facteurVitesseCible = NouveauFacteurVitesse();
        prochainChangement = Time.time + Random.Range(intervalleChangement.x, intervalleChangement.y);
    }

    private float NouveauFacteurVitesse()
    {
        return 1f + Random.Range(-variationVitesse, variationVitesse);
    }

    private void Update()
    {
        if (!Active || amplitude <= 0f || vitesse <= 0f) return;

        // De temps en temps, la cible accélère ou ralentit (en douceur)
        if (vitesseChangeante && Time.time >= prochainChangement)
        {
            facteurVitesseCible = NouveauFacteurVitesse();
            prochainChangement = Time.time + Random.Range(intervalleChangement.x, intervalleChangement.y);
        }
        facteurVitesse = Mathf.MoveTowards(facteurVitesse, facteurVitesseCible, Time.deltaTime * 0.5f);

        // Avancement dans le cycle aller-retour (longueur du cycle = 4 x amplitude)
        float amp = amplitude * facteurAmplitude;
        phase += vitesse * facteurVitesse * Time.deltaTime / (4f * amp);
        phase %= 1f;

        // Position entre -1 et +1
        float t = mouvementFluide
            ? Mathf.Sin(phase * Mathf.PI * 2f)
            : Mathf.PingPong(phase * 4f, 2f) - 1f;

        Vector3 direction = axe == Axe.Horizontal ? transform.right : Vector3.up;
        transform.position = positionDepart + direction * (t * amp * sens);
    }

    // Montre la course de la cible dans la vue Scene
    private void OnDrawGizmosSelected()
    {
        Vector3 depart = Application.isPlaying ? positionDepart : transform.position;
        Vector3 direction = axe == Axe.Horizontal ? transform.right : Vector3.up;
        float amp = amplitude * (1f + variationAmplitude);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(depart - direction * amp, depart + direction * amp);
    }
}
