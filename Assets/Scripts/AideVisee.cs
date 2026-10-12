using UnityEngine;

/// <summary>
/// Aide à la visée : pendant qu'on tend la corde, dessine la trajectoire prévue de la flèche
/// (courbe due à la gravité) et un repère au point d'impact.
/// Disponible uniquement en entraînement, et activable / désactivable (bouton du HUD).
/// À placer sur ARC, avec un Line Renderer sur un objet enfant.
/// </summary>
public class AideVisee : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Arc arc;
    [Tooltip("Line Renderer qui dessine la trajectoire")]
    [SerializeField] private LineRenderer ligne;
    [Tooltip("Petit objet (sphère) placé au point d'impact prévu (optionnel)")]
    [SerializeField] private Transform repereImpact;

    [Header("Réglages")]
    [Tooltip("Afficher aussi en dehors de l'entraînement (pour tester)")]
    [SerializeField] private bool toujoursDisponible;
    [Tooltip("Durée de vol simulée (s)")]
    [SerializeField] private float dureeMax = 3f;
    [Tooltip("Nombre de points de la courbe")]
    [Range(10, 120)] [SerializeField] private int nombrePoints = 60;
    [Tooltip("Layers que la trajectoire peut toucher")]
    [SerializeField] private LayerMask layersTouchables = ~0;

    /// <summary>Choix du joueur, mémorisé entre les parties de la session.</summary>
    public static bool ActiveeParJoueur = true;

    private Transform joueur;

    private void Awake()
    {
        if (arc == null) arc = GetComponent<Arc>();
        if (ligne != null) ligne.useWorldSpace = true;
        var origine = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (origine != null) joueur = origine.transform;
        Cacher();
    }

    /// <summary>Pour un bouton du HUD : active / désactive l'aide.</summary>
    public void Basculer()
    {
        ActiveeParJoueur = !ActiveeParJoueur;
    }

    private bool Disponible()
    {
        if (!ActiveeParJoueur) return false;
        if (toujoursDisponible) return true;
        PartieManager partie = PartieManager.Instance;
        return partie != null && partie.EstEntrainement;
    }

    private void LateUpdate()
    {
        if (ligne == null || arc == null || !Disponible()
            || !arc.PredireTir(out Vector3 position, out Vector3 vitesse))
        {
            Cacher();
            return;
        }

        // Simulation du vol, pas à pas, comme le moteur physique (gravité seule)
        float pas = dureeMax / nombrePoints;
        Vector3 gravite = Physics.gravity;
        int compte = 0;
        bool impact = false;
        Vector3 pointImpact = Vector3.zero;

        ligne.positionCount = nombrePoints + 1;
        ligne.SetPosition(compte++, position);

        for (int i = 0; i < nombrePoints; i++)
        {
            Vector3 suivante = position + vitesse * pas + 0.5f * gravite * pas * pas;
            vitesse += gravite * pas;

            // Arrêt sur le premier obstacle rencontré (cible, sol, décor)
            Vector3 segment = suivante - position;
            if (TrouverObstacle(position, segment, out RaycastHit hit))
            {
                ligne.SetPosition(compte++, hit.point);
                impact = true;
                pointImpact = hit.point;
                break;
            }

            ligne.SetPosition(compte++, suivante);
            position = suivante;
        }

        ligne.positionCount = compte;
        ligne.enabled = true;

        if (repereImpact != null)
        {
            repereImpact.gameObject.SetActive(impact);
            if (impact) repereImpact.position = pointImpact;
        }
    }

    // Premier obstacle sur le segment, en ignorant l'arc lui-même et le joueur
    private bool TrouverObstacle(Vector3 depart, Vector3 segment, out RaycastHit plusProche)
    {
        plusProche = default;
        float meilleure = float.MaxValue;
        bool trouve = false;

        foreach (RaycastHit hit in Physics.RaycastAll(depart, segment.normalized, segment.magnitude,
                                                      layersTouchables, QueryTriggerInteraction.Ignore))
        {
            Transform t = hit.collider.transform;
            if (t.IsChildOf(arc.transform) || (joueur != null && t.IsChildOf(joueur))) continue;
            if (hit.distance >= meilleure) continue;
            meilleure = hit.distance;
            plusProche = hit;
            trouve = true;
        }
        return trouve;
    }

    private void Cacher()
    {
        if (ligne != null) ligne.enabled = false;
        if (repereImpact != null) repereImpact.gameObject.SetActive(false);
    }
}
