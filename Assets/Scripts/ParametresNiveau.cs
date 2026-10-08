using UnityEngine;

/// <summary>
/// Réglages d'un niveau de difficulté (Entraînement, Facile, Normal, Difficile).
/// Chaque niveau est un fichier dans le Project : clic droit > Create > VR Archery > Niveau.
/// </summary>
[CreateAssetMenu(fileName = "Niveau", menuName = "VR Archery/Niveau")]
public class ParametresNiveau : ScriptableObject
{
    [Header("Identité")]
    public string nom = "Normal";
    [Tooltip("Mode entraînement : pas de chrono, cibles fixes, score non sauvegardé")]
    public bool estEntrainement;

    [Header("Temps")]
    [Tooltip("Durée de la partie en secondes (ignorée en entraînement)")]
    public float duree = 90f;

    [Header("Cibles")]
    [Tooltip("Taille des cibles : 1 = taille d'origine, 1.5 = 50 % plus grandes")]
    public float echelleCibles = 1f;
    [Tooltip("Les cibles équipées de CibleMobile bougent-elles ?")]
    public bool ciblesMobiles = true;
    [Tooltip("Vitesse de déplacement des cibles mobiles (m/s)")]
    public float vitesseCibles = 1f;
    [Tooltip("Distance parcourue de chaque côté de la position de départ (m)")]
    public float amplitudeCibles = 1.5f;

    [Header("Progression")]
    [Tooltip("Score à atteindre pour réussir le niveau (débloque les améliorations)")]
    public int scoreObjectif = 50;
}
