using System;
using UnityEngine;

/// <summary>
/// Cible à plusieurs zones de score.
/// Les points dépendent de la distance entre l'impact et le centre.
/// Sélectionne la cible dans la scène : les zones sont dessinées en cercles de couleur
/// pour les aligner facilement sur la texture.
/// </summary>
public class Cible : MonoBehaviour
{
    [Serializable]
    public struct Zone
    {
        [Tooltip("Rayon de la zone en fraction du rayon total (0 = centre, 1 = bord)")]
        [Range(0f, 1f)] public float rayonRelatif;
        public int points;
    }

    [Header("Références")]
    [Tooltip("Objet vide placé au centre rouge, axe bleu (Z) perpendiculaire à la face")]
    [SerializeField] private Transform centre;
    [Tooltip("Collider de la face de la cible. Les impacts ailleurs (pied...) ne rapportent rien")]
    [SerializeField] private Collider faceCible;

    [Header("Zones de score")]
    [Tooltip("Rayon total de la cible, dans l'échelle de CIBLE. Ajuste-le jusqu'à ce que le cercle extérieur touche le bord")]
    [SerializeField] private float rayon = 5f;
    [Tooltip("Du centre vers l'extérieur")]
    [SerializeField] private Zone[] zones =
    {
        new Zone { rayonRelatif = 0.2f, points = 10 },
        new Zone { rayonRelatif = 0.4f, points = 8 },
        new Zone { rayonRelatif = 0.6f, points = 6 },
        new Zone { rayonRelatif = 0.8f, points = 4 },
        new Zone { rayonRelatif = 1.0f, points = 2 },
    };

    /// <summary>Taille de la cible dans la scène, avant l'effet du niveau de difficulté.</summary>
    public Vector3 EchelleOrigine { get; private set; }

    private void Awake()
    {
        EchelleOrigine = transform.localScale;
    }

    /// <summary>Déclenché à chaque impact : (points gagnés, point d'impact).</summary>
    public event Action<int, Vector3> OnTouchee;

    // Rayon en mètres : suit automatiquement l'échelle de la cible (utile pour les difficultés)
    private float RayonMonde => rayon * transform.lossyScale.x;

    /// <summary>Appelée par la flèche quand elle se plante. Retourne les points gagnés.</summary>
    public int EnregistrerImpact(Vector3 pointImpact, Collider colliderTouche)
    {
        if (faceCible != null && colliderTouche != faceCible) return 0;
        if (centre == null)
        {
            Debug.LogWarning($"{name} : le centre de la cible n'est pas assigné.");
            return 0;
        }

        // Distance au centre mesurée dans le plan de la cible
        Vector3 decalage = Vector3.ProjectOnPlane(pointImpact - centre.position, centre.forward);
        float distanceRelative = decalage.magnitude / RayonMonde;

        int points = CalculerPoints(distanceRelative);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AjouterPoints(points);

        OnTouchee?.Invoke(points, pointImpact);
        Debug.Log($"{name} touchée : {points} points (distance {distanceRelative:F2})");
        return points;
    }

    private int CalculerPoints(float distanceRelative)
    {
        foreach (Zone zone in zones)
            if (distanceRelative <= zone.rayonRelatif)
                return zone.points;
        return 0;
    }

    // Dessine les zones dans la vue Scene quand la cible est sélectionnée
    private void OnDrawGizmosSelected()
    {
        if (centre == null || zones == null) return;

        for (int i = 0; i < zones.Length; i++)
        {
            Gizmos.color = Color.Lerp(Color.yellow, Color.cyan, zones.Length > 1 ? (float)i / (zones.Length - 1) : 0f);
            DessinerCercle(centre.position, centre.right, centre.up, RayonMonde * zones[i].rayonRelatif);
        }
    }

    private static void DessinerCercle(Vector3 c, Vector3 axeX, Vector3 axeY, float r)
    {
        const int segments = 48;
        Vector3 precedent = c + axeX * r;
        for (int i = 1; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            Vector3 suivant = c + (axeX * Mathf.Cos(a) + axeY * Mathf.Sin(a)) * r;
            Gizmos.DrawLine(precedent, suivant);
            precedent = suivant;
        }
    }
}