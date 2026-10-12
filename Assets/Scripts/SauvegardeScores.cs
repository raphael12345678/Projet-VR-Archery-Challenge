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

    /// <summary>Ajoute un score et l'enregistre. Retourne l'entrée créée.</summary>
    public static EntreeScore Ajouter(string pseudo, int score, string niveau)
    {
        ListeScores liste = Charger();
        var entree = new EntreeScore
        {
            pseudo = string.IsNullOrWhiteSpace(pseudo) ? "Anonyme" : pseudo.Trim(),
            score = score,
            niveau = niveau,
            date = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };
        liste.scores.Add(entree);

        try
        {
            File.WriteAllText(Chemin, JsonUtility.ToJson(liste, true));
            Debug.Log($"Score enregistré dans {Chemin}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SauvegardeScores : écriture impossible ({e.Message}).");
        }
        return entree;
    }

    /// <summary>Les meilleurs scores d'un niveau, du plus haut au plus bas.</summary>
    public static List<EntreeScore> Meilleurs(string niveau, int nombre = 10)
    {
        return Charger().scores
            .Where(s => s.niveau == niveau)
            .OrderByDescending(s => s.score)
            .Take(nombre)
            .ToList();
    }
}
