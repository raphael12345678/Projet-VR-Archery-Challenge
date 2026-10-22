using System.Collections;
using UnityEngine;

/// <summary>
/// Fin de partie (hors entraînement) :
/// 1. affiche le clavier virtuel pour saisir le pseudo ;
/// 2. enregistre le score dans le fichier JSON ;
/// 3. affiche le classement du niveau, puis les boutons Rejouer / Menu.
/// À placer sur le Canvas du HUD (scène de jeu).
/// </summary>
public class EnregistrementScore : MonoBehaviour
{
    [SerializeField] private GameObject panneauClavier;
    [SerializeField] private ClavierVirtuel clavier;
    [SerializeField] private GameObject panneauClassement;
    [SerializeField] private AffichageClassement classement;
    [Tooltip("Boutons Rejouer / Menu : cachés pendant la saisie du pseudo")]
    [SerializeField] private GameObject boutonsFin;
    [Tooltip("Si un pseudo est déjà choisi dans le menu : enregistrer directement, sans afficher le clavier")]
    [SerializeField] private bool utiliserPseudoMemorise = true;
    [Tooltip("Classement du niveau visible pendant toute la partie (caché en entraînement)")]
    [SerializeField] private bool classementVisiblePendantPartie = true;

    private PartieManager partie;
    private int scoreFinal;

    private void Start()
    {
        if (panneauClavier != null) panneauClavier.SetActive(false);
        if (panneauClassement != null) panneauClassement.SetActive(false);

        partie = PartieManager.Instance;
        if (partie != null)
        {
            partie.OnDebutPartie += OnDebutPartie;
            partie.OnFinPartie += OnFinPartie;
            if (partie.Niveau != null) OnDebutPartie(partie.Niveau);
        }
        if (clavier != null) clavier.OnValider += OnPseudoValide;
    }

    private void OnDebutPartie(ParametresNiveau niveau)
    {
        if (panneauClassement != null)
            panneauClassement.SetActive(classementVisiblePendantPartie && !niveau.estEntrainement);
    }

    private void OnDestroy()
    {
        if (partie != null)
        {
            partie.OnDebutPartie -= OnDebutPartie;
            partie.OnFinPartie -= OnFinPartie;
        }
        if (clavier != null) clavier.OnValider -= OnPseudoValide;
    }

    private void OnFinPartie(int score)
    {
        if (partie.EstEntrainement) return; // pas de classement en entraînement
        scoreFinal = score;

        // Pseudo déjà connu : enregistrement immédiat (avant un éventuel changement de niveau)
        string memorise = SauvegardeScores.PseudoMemorise;
        if (utiliserPseudoMemorise && !string.IsNullOrWhiteSpace(memorise))
        {
            OnPseudoValide(memorise);
            return;
        }
        StartCoroutine(AfficherSaisie());
    }

    private IEnumerator AfficherSaisie()
    {
        // On attend une image : le HUD affiche les boutons de fin au même moment, on les recache
        yield return null;
        string memorise = SauvegardeScores.PseudoMemorise;
        if (boutonsFin != null) boutonsFin.SetActive(false);
        if (clavier != null) clavier.DefinirPseudo(memorise);
        if (panneauClavier != null) panneauClavier.SetActive(true);
    }

    private void OnPseudoValide(string pseudo)
    {
        pseudo = SauvegardeScores.PseudoExistant(pseudo);
        SauvegardeScores.PseudoMemorise = pseudo; // retenu pour les prochaines parties
        // La progression dépend du pseudo : on débloque le niveau suivant pour CE joueur
        if (partie.NiveauReussi) Progression.Debloquer(partie.Niveau.niveauSuivant);
        EntreeScore entree = SauvegardeScores.Ajouter(pseudo, scoreFinal, partie.Niveau.nom);

        if (panneauClavier != null) panneauClavier.SetActive(false);
        if (panneauClassement != null) panneauClassement.SetActive(true);
        if (classement != null) classement.Afficher(partie.Niveau.nom, entree);
        if (boutonsFin != null) boutonsFin.SetActive(true);
    }
}
