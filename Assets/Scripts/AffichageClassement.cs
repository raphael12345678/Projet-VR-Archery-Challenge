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

    private int indexNiveau;

    private void OnEnable()
    {
        if (niveaux != null && niveaux.Length > 0) AfficherNiveau(indexNiveau);
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
        if (texteTitre != null) texteTitre.text = $"Classement - {niveau}";
        if (texteClassement == null) return;

        List<EntreeScore> meilleurs = SauvegardeScores.Meilleurs(niveau, nombreLignes);
        if (meilleurs.Count == 0)
        {
            texteClassement.text = "Aucun score pour l'instant";
            return;
        }

        string couleur = ColorUtility.ToHtmlStringRGB(couleurJoueur);
        var sb = new StringBuilder("<mspace=0.55em>"); // police à chasse fixe : colonnes alignées
        for (int i = 0; i < meilleurs.Count; i++)
        {
            EntreeScore e = meilleurs[i];
            string ligne = $"{i + 1,2}. {e.pseudo,-12}  {e.score,4} pts";
            bool estJoueur = joueur != null && e.pseudo == joueur.pseudo
                             && e.score == joueur.score && e.date == joueur.date;
            sb.AppendLine(estJoueur ? $"<color=#{couleur}>{ligne}</color>" : ligne);
        }
        texteClassement.text = sb.ToString();
    }
}
