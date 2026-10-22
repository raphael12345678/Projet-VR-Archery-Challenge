using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>Un score enregistré.</summary>
[Serializable]
public class EntreeScore
{
    public string pseudo;
    public int score;
    public string niveau;
    public string date;
}

/// <summary>Contenu du fichier JSON (JsonUtility a besoin d'un objet racine).</summary>
[Serializable]
public class ListeScores
{
    public List<EntreeScore> scores = new List<EntreeScore>();
}

/// <summary>
/// Sauvegarde des scores dans un fichier JSON local (scores.json),
/// et lecture du classement par niveau. L'entraînement n'est jamais enregistré.
/// </summary>
public static class SauvegardeScores
{
    private const string NomFichier = "scores.json";

    public static string Chemin => Path.Combine(Application.persistentDataPath, NomFichier);

    private const string ClePseudo = "PseudoJoueur";

    /// <summary>Pseudo choisi dans le menu, mémorisé entre les sessions ("" si aucun).</summary>
    public static string PseudoMemorise
    {
        get => PlayerPrefs.GetString(ClePseudo, "");
        set
        {
            PlayerPrefs.SetString(ClePseudo, value ?? "");
            PlayerPrefs.Save();
            PseudoChange?.Invoke(); // ex. : les boutons du menu affichent la progression du nouveau joueur
        }
    }

    /// <summary>Déclenché quand le pseudo change.</summary>
    public static event Action PseudoChange;

    public static ListeScores Charger()
    {
        try
        {
            if (File.Exists(Chemin))
            {
                ListeScores liste = JsonUtility.FromJson<ListeScores>(File.ReadAllText(Chemin));
                if (liste != null && liste.scores != null) return liste;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SauvegardeScores : lecture impossible ({e.Message}), classement remis à zéro.");
        }
        return new ListeScores();
    }

    /// <summary>Le dernier enregistrement a-t-il battu (ou créé) le record du joueur ?</summary>
    public static bool DernierEstNouveauRecord { get; private set; }

    /// <summary>
    /// Enregistre un score. Un seul record par pseudo et par niveau :
    /// il n'est remplacé que si le nouveau score est meilleur.
    /// Retourne l'entrée conservée dans le classement.
    /// </summary>
    public static EntreeScore Ajouter(string pseudo, int score, string niveau)
    {
        pseudo = string.IsNullOrWhiteSpace(pseudo) ? "Anonyme" : pseudo.Trim();
        ListeScores liste = Charger();
        Dedoublonner(liste);

        EntreeScore existante = liste.scores.FirstOrDefault(
            e => e.niveau == niveau && MemePseudo(e.pseudo, pseudo));

        EntreeScore gardee;
        if (existante == null)
        {
            gardee = new EntreeScore { pseudo = pseudo, score = score, niveau = niveau, date = Maintenant() };
            liste.scores.Add(gardee);
            DernierEstNouveauRecord = true;
        }
        else if (score > existante.score)
        {
            existante.score = score;
            existante.date = Maintenant();
            gardee = existante;
            DernierEstNouveauRecord = true;
        }
        else
        {
            gardee = existante; // ancien record conservé
            DernierEstNouveauRecord = false;
        }

        Ecrire(liste);
        return gardee;
    }

    /// <summary>Les meilleurs scores d'un niveau (un par joueur), du plus haut au plus bas.</summary>
    public static List<EntreeScore> Meilleurs(string niveau, int nombre = 10)
    {
        ListeScores liste = Charger();
        Dedoublonner(liste);
        return liste.scores
            .Where(s => s.niveau == niveau)
            .OrderByDescending(s => s.score)
            .Take(nombre)
            .ToList();
    }

    /// <summary>Meilleur score d'un pseudo sur un niveau (-1 s'il n'a jamais joué ce niveau).</summary>
    public static int MeilleurScore(string pseudo, string niveau)
    {
        if (string.IsNullOrWhiteSpace(pseudo)) return -1;
        int meilleur = -1;
        foreach (EntreeScore e in Charger().scores)
            if (e.niveau == niveau && MemePseudo(e.pseudo, pseudo) && e.score > meilleur)
                meilleur = e.score;
        return meilleur;
    }

    /// <summary>
    /// Si ce pseudo existe déjà dans le classement (majuscules ignorées), renvoie l'orthographe enregistrée ;
    /// sinon renvoie le pseudo tel quel. « raph » devient « Raph » s'il a déjà joué.
    /// </summary>
    public static string PseudoExistant(string pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo)) return pseudo;
        EntreeScore e = Charger().scores.FirstOrDefault(s => MemePseudo(s.pseudo, pseudo));
        return e != null ? e.pseudo : pseudo.Trim();
    }

    // Garde uniquement le meilleur score de chaque joueur par niveau (nettoie les anciens fichiers)
    private static void Dedoublonner(ListeScores liste)
    {
        liste.scores = liste.scores
            .GroupBy(e => (e.niveau, (e.pseudo ?? "").Trim().ToLowerInvariant()))
            .Select(g => g.OrderByDescending(e => e.score).First())
            .ToList();
    }

    private static bool MemePseudo(string a, string b) =>
        string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Maintenant() => DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    // Écriture dans un fichier temporaire puis remplacement : le classement n'est jamais corrompu
    private static void Ecrire(ListeScores liste)
    {
        try
        {
            string temporaire = Chemin + ".tmp";
            File.WriteAllText(temporaire, JsonUtility.ToJson(liste, true));
            if (File.Exists(Chemin)) File.Delete(Chemin);
            File.Move(temporaire, Chemin);
            Debug.Log($"Classement enregistré dans {Chemin}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SauvegardeScores : écriture impossible ({e.Message}).");
        }
    }
}
