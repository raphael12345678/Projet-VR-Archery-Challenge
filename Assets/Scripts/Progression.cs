using UnityEngine;

/// <summary>
/// Mémorise les niveaux débloqués d'une partie à l'autre (PlayerPrefs), SÉPARÉMENT POUR CHAQUE PSEUDO.
/// Un niveau est jouable s'il est « débloqué par défaut » ou si le joueur actuel l'a débloqué
/// en atteignant l'objectif du niveau précédent.
/// </summary>
public static class Progression
{
    /// <summary>Joueur dont on lit / écrit la progression (pseudo choisi dans le menu).</summary>
    public static string JoueurActuel
    {
        get
        {
            string pseudo = SauvegardeScores.PseudoMemorise;
            return string.IsNullOrWhiteSpace(pseudo) ? "(anonyme)" : pseudo.Trim().ToLowerInvariant();
        }
    }

    private static string Cle(ParametresNiveau niveau) => $"Debloque_{JoueurActuel}_{niveau.nom}";

    public static bool EstDebloque(ParametresNiveau niveau)
    {
        if (niveau == null) return false;
        if (niveau.debloqueParDefaut || PlayerPrefs.GetInt(Cle(niveau), 0) == 1) return true;

        // Pseudo déjà présent dans le classement : sa progression est retrouvée grâce à ses records
        if (DebloqueParClassement(niveau))
        {
            PlayerPrefs.SetInt(Cle(niveau), 1);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Vrai si le joueur actuel a, dans le classement, un record qui atteint l'objectif
    /// du niveau précédent (celui dont « Niveau Suivant » est ce niveau).
    /// </summary>
    private static bool DebloqueParClassement(ParametresNiveau niveau)
    {
        string pseudo = SauvegardeScores.PseudoMemorise;
        if (string.IsNullOrWhiteSpace(pseudo)) return false;

        foreach (ParametresNiveau precedent in Resources.FindObjectsOfTypeAll<ParametresNiveau>())
        {
            if (precedent == null || precedent.niveauSuivant != niveau || precedent.estEntrainement) continue;
            if (SauvegardeScores.MeilleurScore(pseudo, precedent.nom) >= precedent.scoreObjectif) return true;
        }
        return false;
    }

    /// <summary>Débloque un niveau pour le joueur actuel. Retourne true s'il vient d'être débloqué.</summary>
    public static bool Debloquer(ParametresNiveau niveau)
    {
        if (niveau == null || EstDebloque(niveau)) return false;
        PlayerPrefs.SetInt(Cle(niveau), 1);
        PlayerPrefs.Save();
        Debug.Log($"Niveau débloqué pour {JoueurActuel} : {niveau.nom}");
        return true;
    }

    /// <summary>Reverrouille les niveaux donnés pour le joueur actuel (test / démonstration).</summary>
    public static void Reinitialiser(ParametresNiveau[] niveaux)
    {
        foreach (ParametresNiveau niveau in niveaux)
            if (niveau != null) PlayerPrefs.DeleteKey(Cle(niveau));
        PlayerPrefs.Save();
    }
}
