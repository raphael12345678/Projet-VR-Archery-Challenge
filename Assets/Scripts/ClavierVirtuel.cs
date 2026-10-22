using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clavier virtuel AZERTY pour saisir un pseudo en VR.
/// Les touches sont créées automatiquement au lancement à partir d'un bouton modèle.
/// À placer sur un panneau (dans un Canvas World Space).
/// </summary>
public class ClavierVirtuel : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Texte qui affiche le pseudo en cours de saisie")]
    [SerializeField] private TMP_Text champPseudo;
    [Tooltip("Bouton modèle (Button - TextMeshPro), copié pour chaque touche. Il est caché au lancement.")]
    [SerializeField] private Button toucheModele;
    [Tooltip("Objet vide qui recevra les rangées de touches")]
    [SerializeField] private RectTransform conteneurTouches;

    [Header("Réglages")]
    [SerializeField] private int longueurMax = 12;
    [SerializeField] private Vector2 tailleTouche = new Vector2(60, 60);
    [SerializeField] private float espacement = 8f;

    /// <summary>Déclenché quand le joueur appuie sur Valider, avec le pseudo saisi.</summary>
    public event Action<string> OnValider;

    public string Pseudo { get; private set; } = "";

    private static readonly string[] Rangees =
    {
        "AZERTYUIOP",
        "QSDFGHJKLM",
        "WXCVBN-_",
        "0123456789",
    };

    private bool construit;

    private void Awake()
    {
        Construire();
        MettreAJour();
    }

    private void Construire()
    {
        if (construit || toucheModele == null || conteneurTouches == null) return;
        construit = true;
        toucheModele.gameObject.SetActive(false);

        // Une colonne de rangées
        if (!conteneurTouches.TryGetComponent(out VerticalLayoutGroup colonne))
            colonne = conteneurTouches.gameObject.AddComponent<VerticalLayoutGroup>();
        colonne.spacing = espacement;
        colonne.childAlignment = TextAnchor.MiddleCenter;
        colonne.childControlWidth = colonne.childControlHeight = false;
        colonne.childForceExpandWidth = colonne.childForceExpandHeight = false;

        foreach (string rangee in Rangees)
        {
            RectTransform ligne = CreerRangee();
            foreach (char c in rangee)
            {
                char lettre = c;
                CreerTouche(ligne, lettre.ToString(), tailleTouche.x, () => Ajouter(lettre));
            }
        }

        // Dernière rangée : touches spéciales
        RectTransform speciale = CreerRangee();
        CreerTouche(speciale, "Effacer", tailleTouche.x * 2.5f, Effacer);
        CreerTouche(speciale, "Espace", tailleTouche.x * 3f, () => Ajouter(' '));
        CreerTouche(speciale, "Valider", tailleTouche.x * 2.5f, Valider);
    }

    private RectTransform CreerRangee()
    {
        var go = new GameObject("Rangee", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var rt = (RectTransform)go.transform;
        rt.SetParent(conteneurTouches, false);
        rt.sizeDelta = new Vector2(0, tailleTouche.y);

        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = espacement;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = h.childControlHeight = false;
        h.childForceExpandWidth = h.childForceExpandHeight = false;

        // La rangée prend la largeur de ses touches
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    private void CreerTouche(RectTransform rangee, string texte, float largeur, Action action)
    {
        Button touche = Instantiate(toucheModele, rangee);
        touche.gameObject.SetActive(true);
        touche.name = "Touche_" + texte;
        ((RectTransform)touche.transform).sizeDelta = new Vector2(largeur, tailleTouche.y);

        TMP_Text label = touche.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = texte;

        touche.onClick.RemoveAllListeners();
        touche.onClick.AddListener(() => action());
    }

    private void Ajouter(char c)
    {
        if (Pseudo.Length >= longueurMax) return;
        if (c == ' ' && Pseudo.Length == 0) return; // pas d'espace au début
        Pseudo += c;
        MettreAJour();
    }

    private void Effacer()
    {
        if (Pseudo.Length == 0) return;
        Pseudo = Pseudo.Substring(0, Pseudo.Length - 1);
        MettreAJour();
    }

    private void Valider()
    {
        if (Pseudo.Trim().Length == 0) return; // pseudo obligatoire
        OnValider?.Invoke(Pseudo.Trim());
    }

    /// <summary>Pré-remplit le champ (ex. avec le pseudo mémorisé).</summary>
    public void DefinirPseudo(string texte)
    {
        Pseudo = string.IsNullOrEmpty(texte) ? "" : texte.Substring(0, Mathf.Min(texte.Length, longueurMax));
        MettreAJour();
    }

    /// <summary>Vide le champ (avant une nouvelle saisie).</summary>
    public void Reinitialiser()
    {
        Pseudo = "";
        MettreAJour();
    }

    private void MettreAJour()
    {
        if (champPseudo != null)
            champPseudo.text = Pseudo.Length > 0 ? Pseudo : "<color=#888888>Ton pseudo...</color>";
    }
}
