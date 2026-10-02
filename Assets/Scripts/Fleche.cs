using UnityEngine;

/// <summary>
/// Flèche tirée par l'arc.
/// - Vol géré par le moteur physique (Rigidbody + gravité).
/// - La pointe suit la trajectoire pendant le vol.
/// - Se plante dans ce qu'elle touche (et suit les cibles mobiles).
/// IMPORTANT : la pointe de la flèche doit être orientée vers l'axe Z (bleu) du prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Fleche : MonoBehaviour
{
    [Header("Caractéristiques de la flèche")]
    [Tooltip("Multiplie la vitesse donnée par l'arc (flèches plus rapides = valeur plus grande)")]
    [SerializeField] private float multiplicateurVitesse = 1f;
    [Tooltip("Durée (s) avant que la flèche disparaisse")]
    [SerializeField] private float dureeDeVie = 10f;

    private Rigidbody rb;
    private bool enVol;

    public bool EstPlantee { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Immobile tant qu'elle n'est pas tirée
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
    }

    /// <summary>Appelée par l'arc au moment du tir.</summary>
    public void Lancer(Vector3 vitesse, Collider[] aIgnorer = null)
    {
        // Évite que la flèche percute l'arc au départ
        if (aIgnorer != null)
        {
            Collider[] mesColliders = GetComponentsInChildren<Collider>();
            foreach (Collider c1 in mesColliders)
                foreach (Collider c2 in aIgnorer)
                    Physics.IgnoreCollision(c1, c2);
        }

        rb.isKinematic = false;
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
}