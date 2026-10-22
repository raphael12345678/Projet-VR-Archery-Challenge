using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bouton de niveau du menu : grisé et marqué « verrouillé » tant que le niveau n'est pas débloqué,
/// avec l'objectif à atteindre au niveau précédent.
/// À placer sur chaque bouton de niveau (Entraînement, Facile, Normal, Difficile).
/// </summary>
[RequireComponent(typeof(Button))]
public class BoutonNiveauMenu : MonoBehaviour
{
    [SerializeField] private ParametresNiveau niveau;
    [Tooltip("Niveau à réussir pour débloquer celui-ci (pour afficher l'objectif)")]
    [SerializeField] private ParametresNiveau niveauPrecedent;
    [SerializeField] private TMP_Text texte;

    private void Awake()
    {
        if (texte == null) texte = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        SauvegardeScores.PseudoChange += Rafraichir;
        Rafraichir();
    }

    private void OnDisable()
    {
        SauvegardeScores.PseudoChange -= Rafraichir;
    }

    /// <summary>Met à jour le bouton selon la progression du joueur actuel.</summary>
    public void Rafraichir()
    {
        if (niveau == null) return;

        bool debloque = Progression.EstDebloque(niveau);
        GetComponent<Button>().interactable = debloque;

        if (texte == null) return;
        if (debloque)
        {
            texte.text = niveau.nom;
        }
        else
        {
            string condition = niveauPrecedent != null
                ? $"{niveauPrecedent.scoreObjectif} pts en {niveauPrecedent.nom}"
                : "à débloquer";
            texte.text = $"{niveau.nom}  <size=60%>(verrouillé : {condition})</size>";
        }
    }
}
