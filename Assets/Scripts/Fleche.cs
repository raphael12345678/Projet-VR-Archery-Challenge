using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Flèche :
/// - se prend à la main (XR Grab Interactable) ;
/// - se pose sur l'encoche de l'arc (socket) ;
/// - vole avec le moteur physique une fois tirée ;
/// - se plante dans ce qu'elle touche et prévient la cible.
/// IMPORTANT : la pointe doit être orientée vers l'axe Z (bleu) du prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class Fleche : MonoBehaviour
{
    [Header("Caractéristiques de la flèche")]
    [Tooltip("Multiplie la vitesse donnée par l'arc (flèches plus rapides = valeur plus grande)")]
    [SerializeField] private float multiplicateurVitesse = 1f;
    [Tooltip("Durée (s) avant que la flèche tirée disparaisse")]
    [SerializeField] private float dureeDeVie = 10f;

    private Rigidbody rb;
    private XRGrabInteractable grab;
    private Collider[] mesColliders;
    private bool enVol;

    public bool EstPlantee { get; private set; }
    public bool EstEncochee { get; private set; }
    public XRGrabInteractable Grab => grab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        mesColliders = GetComponentsInChildren<Collider>();

        // C'est l'arc qui donne la vitesse, pas le mouvement de la main
        grab.throwOnDetach = false;

        // Immobile dans le carquois tant qu'on ne la prend pas
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnPrise);
        grab.selectExited.AddListener(OnLachee);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnPrise);
        grab.selectExited.RemoveListener(OnLachee);
    }

    private void OnPrise(SelectEnterEventArgs args)
    {
        EstEncochee = args.interactorObject is XRSocketInteractor;

        // Encochée : on coupe ses colliders pour que la main attrape la corde, pas la flèche
        if (EstEncochee)
            ActiverColliders(false);
    }

    private void OnLachee(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor)
        {
            EstEncochee = false;
            ActiverColliders(true);
            return;
        }

        // Lâchée par la main : si l'encoche ne la récupère pas, elle tombe
        if (!enVol && !EstPlantee)
            StartCoroutine(TomberSiLibre());
    }

    private IEnumerator TomberSiLibre()
    {
        yield return null; // laisse une image à l'encoche pour la récupérer
        if (!grab.isSelected && !enVol && !EstPlantee)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    /// <summary>Appelée par l'arc au moment du tir.</summary>
    public void Lancer(Vector3 vitesse, Collider[] aIgnorer = null)
    {
        // Une flèche en vol ne peut plus être attrapée
        grab.enabled = false;
        ActiverColliders(true);

        // Évite que la flèche percute l'arc au départ
        if (aIgnorer != null)
            foreach (Collider c1 in mesColliders)
                foreach (Collider c2 in aIgnorer)
                    Physics.IgnoreCollision(c1, c2);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = vitesse * multiplicateurVitesse;
        enVol = true;

        Destroy(gameObject, dureeDeVie);
    }

    private void FixedUpdate()
    {
        // La pointe suit la courbe de la trajectoire
        if (enVol && rb.linearVelocity.sqrMagnitude > 0.1f)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enVol) return;

        enVol = false;
        EstPlantee = true;

        // On fige la flèche dans l'objet touché
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;

        // Devient enfant de l'objet touché : suit les cibles mobiles
        transform.SetParent(collision.transform, true);

        // Si l'objet touché fait partie d'une cible, on calcule le score
        Vector3 pointImpact = collision.GetContact(0).point;
        Cible cible = collision.collider.GetComponentInParent<Cible>();
        if (cible != null)
            cible.EnregistrerImpact(pointImpact, collision.collider);
    }

    private void ActiverColliders(bool actif)
    {
        foreach (Collider c in mesColliders)
            c.enabled = actif;
    }
}