using UnityEngine;

/// <summary>
/// Fait bouger une cible en va-et-vient à vitesse constante.
/// La vitesse et l'amplitude sont fixées par le PartieManager selon le niveau.
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
    [Tooltip("Décalage pour que toutes les cibles ne bougent pas en même temps (0 à 1)")]
    [Range(0f, 1f)] [SerializeField] private float decalage;

    private Vector3 positionDepart;
    private float temps;

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
    }

    public void Configurer(float nouvelleVitesse, float nouvelleAmplitude, bool active)
    {
        vitesse = nouvelleVitesse;
        amplitude = nouvelleAmplitude;
        Active = active;
        if (!active) transform.position = positionDepart;
    }

    private void Update()
    {
        if (!Active || amplitude <= 0f) return;

        temps += Time.deltaTime;
        float course = amplitude * 2f;
        float offset = Mathf.PingPong(temps * vitesse + decalage * course * 2f, course) - amplitude;

        Vector3 direction = axe == Axe.Horizontal ? transform.right : Vector3.up;
        transform.position = positionDepart + direction * offset;
    }

    // Montre la course de la cible dans la vue Scene
    private void OnDrawGizmosSelected()
    {
        Vector3 depart = Application.isPlaying ? positionDepart : transform.position;
        Vector3 direction = axe == Axe.Horizontal ? transform.right : Vector3.up;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(depart - direction * amplitude, depart + direction * amplitude);
    }
}
