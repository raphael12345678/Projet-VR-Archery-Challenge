using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Affiche le classement (meilleurs scores) d'un niveau dans un texte.
/// Dans le menu : boutons Précédent / Suivant pour passer d'une difficulté à l'autre.
/// En fin de partie : affiche le classement du niveau joué, avec la ligne du joueur en couleur.
/// L'entraînement n'a pas de classement.
/// </summary>
public class AffichageClassement : MonoBehaviour
{
    [SerializeField] private TMP_Text texteTitre;
    [SerializeField] private TMP_Text texteClassement;
    [Tooltip("Niveaux consultables avec Précédent / Suivant (sans l'entraînement)")]
    [SerializeField] private ParametresNiveau[] niveaux;
    [SerializeField] private int nombreLignes = 10;
    [SerializeField] private Color couleurJoueur = new Color(1f, 0.8f, 0.2f);
    [Tooltip("Scène de jeu : affiche automatiquement le classement du niveau en cours")]
    [SerializeField] private bool suivreNiveauEnCours;

    private int indexNiveau;
    private PartieManager partie;
    private bool niveauEnCoursAffiche;

    private void Start()
    {
        if (!suivreNiveauEnCours) return;
        partie = PartieManager.Instance;
        if (partie == null) return;
        partie.OnDebutPartie += AfficherNiveauEnCours;
        if (partie.Niveau != null) AfficherNiveauEnCours(partie.Niveau);
    }

    private void OnDestroy()
    {
        if (partie != null) partie.OnDebutPartie -= AfficherNiveauEnCours;
    }

    private void OnEnable()
    {
        if (suivreNiveauEnCours)
        {
            // Réactivé pendant la partie : on remet le niveau en cours
            if (partie != null && partie.Niveau != null && !niveauEnCoursAffiche) AfficherNiveauEnCours(partie.Niveau);
            return;
        }
        if (niveaux != null && niveaux.Length > 0) AfficherNiveau(indexNiveau);
    }

    private void AfficherNiveauEnCours(ParametresNiveau niveau)
    {
        niveauEnCoursAffiche = true;
        if (niveau.estEntrainement)
        {
            if (texteTitre != null) texteTitre.text = "Classement";
            if (texteClassement != null) texteClassement.text = "Pas de classement en entraînement";
            return;
        }
        // Précédent / Suivant partent du niveau en cours
        if (niveaux != null)
        {
            int i = System.Array.IndexOf(niveaux, niveau);
            if (i >= 0) indexNiveau = i;
        }
        Afficher(niveau.nom, null);
    }

    public void Suivant()
    {
        if (niveaux == null || niveaux.Length == 0) return;
        indexNiveau = (indexNiveau + 1) % niveaux.Length;
        AfficherNiveau(indexNiveau);
    }

    public void Precedent()
    {
        if (niveaux == null || niveaux.Length == 0) return;
        indexNiveau = (indexNiveau - 1 + niveaux.Length) % niveaux.Length;
        AfficherNiveau(indexNiveau);
    }

    private void AfficherNiveau(int index)
    {
        if (niveaux[index] != null) Afficher(niveaux[index].nom, null);
    }

    /// <summary>Affiche le classement d'un niveau ; « joueur » est mis en évidence s'il est dans la liste.</summary>
    public void Afficher(string niveau, EntreeScore joueur)
    {
        if (texteTitre != null)
        {
            texteTitre.text = $"Classement - {niveau}";
            if (joueur != null)
                texteTitre.text += SauvegardeScores.DernierEstNouveauRecord
                    ? "\n<size=70%><color=#FFD54A>Nouveau record !</color></size>"
                    : $"\n<size=70%>Ton record : {joueur.score} pts</size>";
        }
        if (texteClassement == null) return;

        List<EntreeScore> meilleurs = SauvegardeScores.Meilleurs(niveau, nombreLignes);
        if (meilleurs.Count == 0)
        {
            texteClassement.text = "Aucun score pour l'instant";
            return;
        }

        string couleur = ColorUtility.ToHtmlStringRGB(couleurJoueur);
        string pseudoJoueur = SauvegardeScores.PseudoMemorise.Trim();
        var sb = new StringBuilder("<mspace=0.55em>"); // police à chasse fixe : colonnes alignées
        for (int i = 0; i < meilleurs.Count; i++)
        {
            EntreeScore e = meilleurs[i];
            string ligne = $"{i + 1,2}. {e.pseudo,-12}  {e.score,4} pts";
            // Ligne du joueur en couleur (un seul record par pseudo : on compare le pseudo)
            string pseudoCherche = joueur != null ? joueur.pseudo : pseudoJoueur;
            bool estJoueur = !string.IsNullOrEmpty(pseudoCherche) && string.Equals(
                (e.pseudo ?? "").Trim(), pseudoCherche.Trim(), System.StringComparison.OrdinalIgnoreCase);
            sb.AppendLine(estJoueur ? $"<color=#{couleur}>{ligne}</color>" : ligne);
        }
        texteClassement.text = sb.ToString();
    }
}
