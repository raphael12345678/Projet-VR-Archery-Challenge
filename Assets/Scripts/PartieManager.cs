using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Choix du niveau, partagé entre les scènes (le menu le remplit, la scène de jeu le lit).
/// </summary>
public static class ChoixNiveau
{
    public static ParametresNiveau Niveau;
}

/// <summary>
/// Gère une partie : applique le niveau (taille et vitesse des cibles), décompte le temps,
/// et déclare la fin de partie. Un seul dans la scène, sur GameManager.
/// </summary>
public class PartieManager : MonoBehaviour
{
    public static PartieManager Instance { get; private set; }

    [Tooltip("Niveau utilisé si aucun n'a été choisi dans le menu (pratique pour tester)")]
    [SerializeField] private ParametresNiveau niveauParDefaut;
    [Tooltip("Nom exact de la scène du menu")]
    [SerializeField] private string sceneMenu = "Menu";

    public ParametresNiveau Niveau { get; private set; }
    public float TempsRestant { get; private set; }
    public bool EnCours { get; private set; }
    public bool EstEntrainement => Niveau != null && Niveau.estEntrainement;
    /// <summary>Pas de chrono : entraînement, ou durée laissée à 0.</summary>
    public bool TempsIllimite => Niveau != null && (Niveau.estEntrainement || Niveau.duree <= 0f);

    /// <summary>Le joueur peut-il tirer ? (non une fois la partie terminée)</summary>
    public bool TirAutorise => EnCours;

    /// <summary>Déclenché au lancement de la partie.</summary>
    public event Action<ParametresNiveau> OnDebutPartie;
    /// <summary>Déclenché à la fin du temps, avec le score final.</summary>
    public event Action<int> OnFinPartie;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Niveau = ChoixNiveau.Niveau != null ? ChoixNiveau.Niveau : niveauParDefaut;
        if (Niveau == null)
        {
            Debug.LogError("PartieManager : aucun niveau assigné (champ Niveau Par Defaut).");
            return;
        }
        DemarrerPartie();
    }

    public void DemarrerPartie()
    {
        AppliquerNiveau();
        TempsRestant = TempsIllimite ? 0f : Niveau.duree;
        EnCours = true;
        if (!Niveau.estEntrainement && Niveau.duree <= 0f)
            Debug.LogWarning($"PartieManager : le niveau {Niveau.nom} a une durée de 0 et n'est pas coché « Est Entrainement » : chrono désactivé.");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.Reinitialiser();

        OnDebutPartie?.Invoke(Niveau);
        Debug.Log($"Partie lancée : {Niveau.nom}");
    }

    private void Update()
    {
        if (!EnCours || TempsIllimite) return;

        TempsRestant -= Time.deltaTime;
        if (TempsRestant <= 0f)
        {
            TempsRestant = 0f;
            TerminerPartie();
        }
    }

    public void TerminerPartie()
    {
        if (!EnCours) return;
        EnCours = false;

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        OnFinPartie?.Invoke(score);
        Debug.Log($"Partie terminée : {score} points (objectif {Niveau.scoreObjectif})");
    }

    /// <summary>Recharge la scène pour rejouer le même niveau.</summary>
    public void Rejouer()
    {
        ChoixNiveau.Niveau = Niveau;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Retourne au menu principal (bouton de fin de partie ou de pause).</summary>
    public void RetourMenu()
    {
        SceneManager.LoadScene(sceneMenu);
    }

    private void AppliquerNiveau()
    {
        // Disposition des cibles : on n'active que celle du niveau choisi.
        // (À faire en premier : les cibles mémorisent leur taille et position à leur activation.)
        DispositionNiveau[] dispositions = FindObjectsByType<DispositionNiveau>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool trouvee = false;
        foreach (DispositionNiveau disposition in dispositions)
        {
            bool estLaBonne = disposition.Niveau == Niveau;
            disposition.gameObject.SetActive(estLaBonne);
            trouvee |= estLaBonne;
        }
        if (dispositions.Length > 0 && !trouvee)
            Debug.LogWarning($"PartieManager : aucune disposition de cibles pour le niveau {Niveau.nom}.");

        // Taille des cibles
        foreach (Cible cible in FindObjectsByType<Cible>(FindObjectsSortMode.None))
        {
            Vector3 echelleOrigine = cible.EchelleOrigine;
            cible.transform.localScale = echelleOrigine * Niveau.echelleCibles;
        }

        // Déplacement des cibles mobiles (fixes en entraînement)
        bool mobiles = Niveau.ciblesMobiles && !Niveau.estEntrainement;
        foreach (CibleMobile mobile in FindObjectsByType<CibleMobile>(FindObjectsSortMode.None))
            mobile.Configurer(Niveau.vitesseCibles, Niveau.amplitudeCibles, mobiles);
    }
}
