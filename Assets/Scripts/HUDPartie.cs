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

    [Tooltip("Le temps passe en rouge en dessous de ce nombre de secondes")]
    [SerializeField] private float alerteTemps = 10f;
    [SerializeField] private Color couleurNormale = Color.white;
    [SerializeField] private Color couleurAlerte = Color.red;

    private PartieManager partie;

    private void Start()
    {
        partie = PartieManager.Instance;
        if (partie == null) return;

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
        if (partie == null || texteTemps == null || partie.Niveau == null) return;

        if (partie.EstEntrainement)
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
        if (boutonsFin != null) boutonsFin.SetActive(niveau.estEntrainement);
    }

    private void AfficherFin(int score)
    {
        if (boutonsFin != null) boutonsFin.SetActive(true);
        if (texteFin == null) return;

        bool reussi = score >= partie.Niveau.scoreObjectif;
        texteFin.text = reussi
            ? $"Niveau réussi !\n{score} points"
            : $"Temps écoulé\n{score} / {partie.Niveau.scoreObjectif} points";
        texteFin.gameObject.SetActive(true);
    }
}
