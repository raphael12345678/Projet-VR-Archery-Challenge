using TMPro;
using UnityEngine;

/// <summary>
/// Choix du pseudo dans le menu : affiche le pseudo actuel et ouvre un clavier virtuel pour le changer.
/// Le pseudo est mémorisé et utilisé automatiquement pour enregistrer les scores en fin de partie.
/// À placer sur le Canvas du menu (ou un objet du menu).
/// </summary>
public class ChoixPseudo : MonoBehaviour
{
    [Tooltip("Texte qui affiche « Joueur : ... »")]
    [SerializeField] private TMP_Text textePseudo;
    [Tooltip("Panneau contenant le clavier virtuel (caché par défaut)")]
    [SerializeField] private GameObject panneauClavier;
    [SerializeField] private ClavierVirtuel clavier;

    private void Start()
    {
        if (panneauClavier != null) panneauClavier.SetActive(false);
        if (clavier != null) clavier.OnValider += Valider;
        Afficher();
    }

    private void OnDestroy()
    {
        if (clavier != null) clavier.OnValider -= Valider;
    }

    /// <summary>Bouton « Changer de pseudo » : ouvre le clavier, pré-rempli avec le pseudo actuel.</summary>
    public void OuvrirClavier()
    {
        if (clavier != null) clavier.DefinirPseudo(SauvegardeScores.PseudoMemorise);
        if (panneauClavier != null) panneauClavier.SetActive(true);
    }

    /// <summary>Bouton « Annuler » (optionnel) : ferme le clavier sans changer le pseudo.</summary>
    public void FermerClavier()
    {
        if (panneauClavier != null) panneauClavier.SetActive(false);
    }

    private void Valider(string pseudo)
    {
        // Pseudo déjà dans le classement : on reprend son orthographe (et donc sa progression)
        SauvegardeScores.PseudoMemorise = SauvegardeScores.PseudoExistant(pseudo);
        FermerClavier();
        Afficher();
    }

    private void Afficher()
    {
        if (textePseudo == null) return;
        string pseudo = SauvegardeScores.PseudoMemorise;
        textePseudo.text = string.IsNullOrWhiteSpace(pseudo) ? "Joueur : (aucun pseudo)" : $"Joueur : {pseudo}";
    }
}
