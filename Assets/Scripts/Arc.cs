using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Arc tenu en VR.
/// - Se saisit par le point "Attach".
/// - Gâchette maintenue = tension de la corde, gâchette relâchée = tir.
/// - La flèche part du "FirePoint", dans la direction de son axe Z (flèche bleue).
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class Arc : MonoBehaviour
{
    [Header("Points de l'arc")]
    [Tooltip("Endroit où la main tient l'arc")]
    [SerializeField] private Transform attachPoint;
    [Tooltip("Endroit d'où part la flèche (axe Z bleu = direction du tir)")]
    [SerializeField] private Transform firePoint;

    [Header("Flèche")]
    [SerializeField] private Fleche flechePrefab;

    [Header("Caractéristiques de l'arc")]
    [Tooltip("Vitesse de la flèche avec une tension minimale (m/s)")]
    [SerializeField] private float vitesseMin = 5f;
    [Tooltip("Vitesse de la flèche avec une tension maximale (m/s)")]
    [SerializeField] private float vitesseMax = 30f;
    [Tooltip("Temps (s) pour atteindre la tension maximale")]
    [SerializeField] private float tempsTensionMax = 1.5f;
    [Tooltip("Délai minimum entre deux tirs (s)")]
    [SerializeField] private float delaiEntreTirs = 0.3f;

    private XRGrabInteractable grab;
    private Collider[] collidersArc;
    private bool enTension;
    private float debutTension;
    private float dernierTir = -10f;

    /// <summary>Tension actuelle entre 0 et 1 (utile pour l'UI, le son ou l'animation de la corde).</summary>
    public float Tension => enTension
        ? Mathf.Clamp01((Time.time - debutTension) / tempsTensionMax)
        : 0f;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        collidersArc = GetComponentsInChildren<Collider>();

        // La main saisit l'arc au niveau du point Attach
        if (attachPoint != null)
            grab.attachTransform = attachPoint;
    }

    private void OnEnable()
    {
        grab.activated.AddListener(OnGachettePressee);
        grab.deactivated.AddListener(OnGachetteRelachee);
        grab.selectExited.AddListener(OnArcLache);
    }

    private void OnDisable()
    {
        grab.activated.RemoveListener(OnGachettePressee);
        grab.deactivated.RemoveListener(OnGachetteRelachee);
        grab.selectExited.RemoveListener(OnArcLache);
    }

    private void OnGachettePressee(ActivateEventArgs args)
    {
        if (Time.time - dernierTir < delaiEntreTirs) return;
        enTension = true;
        debutTension = Time.time;
    }

    private void OnGachetteRelachee(DeactivateEventArgs args)
    {
        if (!enTension) return;

        float vitesse = Mathf.Lerp(vitesseMin, vitesseMax, Tension);
        enTension = false;
        Tirer(vitesse);
    }

    private void OnArcLache(SelectExitEventArgs args)
    {
        // Si on lâche l'arc pendant la tension, on annule le tir
        enTension = false;
    }

    private void Tirer(float vitesse)
    {
        if (flechePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Arc : flechePrefab ou firePoint non assigné.");
            return;
        }

        Fleche fleche = Instantiate(flechePrefab, firePoint.position, firePoint.rotation);
        fleche.Lancer(firePoint.forward * vitesse, collidersArc);
        dernierTir = Time.time;
    }
}