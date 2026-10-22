using TMPro;
using UnityEngine;

/// <summary>
/// Affiche les informations de la partie : niveau, temps restant, objectif, message de fin.
/// À placer sur le Canvas (World Space), à côté de ScoreAffichage. Tous les champs sont optionnels.
/// </summary>
public class HUDPartie : MonoBehaviour
{
    [SerializeField] private TMP_Text texteNiveau;
    [SerializeField] private TMP_Text texteTemps;
    [SerializeField] private TMP_Text texteObjectif;
    [Tooltip("Message affiché à la fin (caché pendant la partie)")]
    [SerializeField] private TMP_Text texteFin;
    [Tooltip("Objet contenant les boutons Rejouer / Menu, affiché à la fin (et toujours en entraînement)")]
    [SerializeField] private GameObject boutonsFin;
    [Tooltip("UN SEUL bouton « Niveau suivant » (hors de BoutonsFin) : visible dès que l'objectif est atteint, "
             + "pendant la partie comme à la fin. On Click → PartieManager.PasserAuSuivantMaintenant")]
    [SerializeField] private GameObject boutonSuivant;
    [Tooltip("Rejouer / Menu visibles aussi PENDANT la partie (sinon seulement à la fin)")]
    [SerializeField] private bool boutonsFinPendantPartie = true;
    [Tooltip("Optionnel : panneau du clavier ; le bouton est caché pendant la saisie du pseudo")]
    [SerializeField] private GameObject panneauClavier;

    [Tooltip("Le temps passe en rouge en dessous de ce nombre de secondes")]
    [SerializeField] private float alerteTemps = 10f;
    [SerializeField] private Color couleurNormale = Color.white;
    [SerializeField] private Color couleurAlerte = Color.red;

    private PartieManager partie;

    private void Start()
    {
        partie = PartieManager.Instance;
        if (partie == null) return;

        // Le bouton doit être visible pendant la partie : il ne peut pas rester dans BoutonsFin (caché)
        if (boutonSuivant != null && boutonsFin != null
            && boutonSuivant.transform.IsChildOf(boutonsFin.transform))
        {
            Debug.Log("HUDPartie : BoutonSuivant sorti de BoutonsFin pour être visible pendant la partie.");
            boutonSuivant.transform.SetParent(boutonsFin.transform.parent, true);
        }
        if (boutonSuivant != null) boutonSuivant.SetActive(false);

        partie.OnDebutPartie += AfficherDebut;
        partie.OnFinPartie += AfficherFin;

        if (partie.Niveau != null) AfficherDebut(partie.Niveau);
    }

    private void OnDestroy()
    {
        if (partie == null) return;
        partie.OnDebutPartie -= AfficherDebut;
        partie.OnFinPartie -= AfficherFin;
    }

    private void Update()
    {
        if (partie == null || partie.Niveau == null) return;

        if (boutonSuivant != null)
        {
            bool saisiePseudo = panneauClavier != null && panneauClavier.activeInHierarchy;
            bool visible = !saisiePseudo && (partie.EnCours
                ? partie.PeutPasserEnCours                                             // objectif atteint avant la fin
                : partie.NiveauReussi && partie.Niveau.niveauSuivant != null);         // partie finie et réussie
            if (boutonSuivant.activeSelf != visible) boutonSuivant.SetActive(visible);
        }

        if (texteTemps == null) return;

        if (partie.TempsIllimite)
        {
            texteTemps.text = "Temps libre";
            texteTemps.color = couleurNormale;
            return;
        }

        int secondes = Mathf.CeilToInt(partie.TempsRestant);
        texteTemps.text = $"Temps : {secondes / 60}:{secondes % 60:00}";
        texteTemps.color = partie.TempsRestant <= alerteTemps ? couleurAlerte : couleurNormale;
    }

    private void AfficherDebut(ParametresNiveau niveau)
    {
        if (texteNiveau != null) texteNiveau.text = $"Niveau : {niveau.nom}";
        if (texteObjectif != null)
            texteObjectif.text = niveau.estEntrainement ? "Entraînement libre" : $"Objectif : {niveau.scoreObjectif} pts";
        if (texteFin != null) texteFin.gameObject.SetActive(false);
        // En entraînement il n'y a pas de fin : le bouton Menu reste disponible
        if (boutonsFin != null) boutonsFin.SetActive(niveau.estEntrainement || boutonsFinPendantPartie);
    }

    private void AfficherFin(int score)
    {
        if (boutonsFin != null) boutonsFin.SetActive(true);
        // Le bouton Niveau suivant est géré dans Update
        if (texteFin == null) return;

        string texte = partie.NiveauReussi
            ? $"Niveau réussi !\n{score} points"
            : $"Temps écoulé\n{score} / {partie.Niveau.scoreObjectif} points";
        if (partie.VientDeDebloquer)
            texte += $"\n<color=#FFD54A>Niveau {partie.Niveau.niveauSuivant.nom} débloqué !</color>";
        texteFin.text = texte;
        texteFin.gameObject.SetActive(true);
    }
}
