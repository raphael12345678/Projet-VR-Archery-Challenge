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

    private PartieManager partie;
    private int scoreFinal;

    private void Start()
    {
        if (panneauClavier != null) panneauClavier.SetActive(false);
        if (panneauClassement != null) panneauClassement.SetActive(false);

        partie = PartieManager.Instance;
        if (partie != null) partie.OnFinPartie += OnFinPartie;
        if (clavier != null) clavier.OnValider += OnPseudoValide;
    }

    private void OnDestroy()
    {
        if (partie != null) partie.OnFinPartie -= OnFinPartie;
        if (clavier != null) clavier.OnValider -= OnPseudoValide;
    }

    private void OnFinPartie(int score)
    {
        if (partie.EstEntrainement) return; // pas de classement en entraînement
        scoreFinal = score;
        StartCoroutine(AfficherSaisie());
    }

    private IEnumerator AfficherSaisie()
    {
        // On attend une image : le HUD affiche les boutons de fin au même moment, on les recache
        yield return null;
        if (boutonsFin != null) boutonsFin.SetActive(false);

        if (clavier != null) clavier.Reinitialiser();
        if (panneauClavier != null) panneauClavier.SetActive(true);
    }

    private void OnPseudoValide(string pseudo)
    {
        EntreeScore entree = SauvegardeScores.Ajouter(pseudo, scoreFinal, partie.Niveau.nom);

        if (panneauClavier != null) panneauClavier.SetActive(false);
        if (panneauClassement != null) panneauClassement.SetActive(true);
        if (classement != null) classement.Afficher(partie.Niveau.nom, entree);
        if (boutonsFin != null) boutonsFin.SetActive(true);
    }
}
