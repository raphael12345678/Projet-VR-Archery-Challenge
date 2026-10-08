using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
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

    [Header("Impact")]
    [Tooltip("Objet vide placé au bout de la pointe (optionnel) : la flèche se plante exactement à cet endroit")]
    [SerializeField] private Transform pointe;

    private Rigidbody rb;
    private XRGrabInteractable grab;
    private Collider[] mesColliders;
    private bool enVol;
    private Vector3 positionPrecedente;
    private readonly List<Collider> collidersIgnores = new List<Collider>();

    public bool EstPlantee { get; private set; }
    public bool EstEncochee { get; private set; }

    /// <summary>Déclenché au départ de la flèche, avec sa vitesse (m/s). Utilisé pour le son.</summary>
    public event System.Action<float> OnLancee;
    /// <summary>Déclenché à l'impact : (objet touché, point d'impact, est-ce une cible ?).</summary>
    public event System.Action<Collider, Vector3, bool> OnPlantee;
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

        // La flèche ne doit percuter ni l'arc, ni le corps et les mains du joueur
        if (aIgnorer != null)
            collidersIgnores.AddRange(aIgnorer);
        XROrigin joueur = FindFirstObjectByType<XROrigin>();
        if (joueur != null)
            collidersIgnores.AddRange(joueur.GetComponentsInChildren<Collider>(true));

        foreach (Collider c1 in mesColliders)
            foreach (Collider c2 in collidersIgnores)
                if (c2 != null) Physics.IgnoreCollision(c1, c2);

        // Elle vit maintenant dans le monde, indépendante de tout parent
        transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = vitesse * multiplicateurVitesse;
        positionPrecedente = rb.position;
        enVol = true;
        OnLancee?.Invoke(rb.linearVelocity.magnitude);

        Destroy(gameObject, dureeDeVie);
    }

    private void FixedUpdate()
    {
        if (!enVol) return;

        // Anti-traversée : on vérifie le trajet parcouru depuis le dernier pas physique.
        // Une flèche très rapide peut sinon passer à travers une cible fine.
        Vector3 trajet = rb.position - positionPrecedente;
        float distance = trajet.magnitude;
        if (distance > 0.001f &&
            TrouverObstacle(positionPrecedente, trajet / distance, distance, out RaycastHit impact))
        {
            Planter(impact.collider, impact.point);
            return;
        }
        positionPrecedente = rb.position;

        // La pointe suit la courbe de la trajectoire
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enVol || EstIgnore(collision.collider)) return;
        Planter(collision.collider, collision.GetContact(0).point);
    }

    private bool TrouverObstacle(Vector3 depart, Vector3 direction, float distance, out RaycastHit plusProche)
    {
        plusProche = default;
        float meilleure = float.MaxValue;
        bool trouve = false;

        foreach (RaycastHit hit in Physics.RaycastAll(depart, direction, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (EstIgnore(hit.collider) || hit.distance >= meilleure) continue;
            meilleure = hit.distance;
            plusProche = hit;
            trouve = true;
        }
        return trouve;
    }

    private bool EstIgnore(Collider c)
    {
        return System.Array.IndexOf(mesColliders, c) >= 0 || collidersIgnores.Contains(c);
    }

    private void Planter(Collider touche, Vector3 pointImpact)
    {
        enVol = false;
        EstPlantee = true;

        // On fige la flèche à l'endroit de l'impact
        rb.linearVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        // La pointe se place exactement sur le point d'impact
        if (pointe != null)
            transform.position = pointImpact + (transform.position - pointe.position);

        // Ne suit que les cibles (mobiles ou non), jamais le joueur ni le décor
        Cible cible = touche.GetComponentInParent<Cible>();
        if (cible != null)
        {
            transform.SetParent(touche.transform, true);
            cible.EnregistrerImpact(pointImpact, touche);
        }
        OnPlantee?.Invoke(touche, pointImpact, cible != null);
    }

    private void ActiverColliders(bool actif)
    {
        foreach (Collider c in mesColliders)
            c.enabled = actif;
    }
}