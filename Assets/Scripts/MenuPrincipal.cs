using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal : lance la scène de jeu avec le niveau choisi.
/// Chaque bouton de niveau appelle ChoisirNiveau avec son numéro (0 = premier de la liste).
/// À placer sur le Canvas du menu.
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Tooltip("Les 4 niveaux, dans l'ordre des boutons : Entraînement, Facile, Normal, Difficile")]
    [SerializeField] private ParametresNiveau[] niveaux;
    [Tooltip("Nom exact de la scène de jeu")]
    [SerializeField] private string sceneJeu = "Niv1";

    /// <summary>Appelée par les boutons de niveau (Entraînement = 0, Facile = 1, ...).</summary>
    public void ChoisirNiveau(int index)
    {
        if (niveaux == null || index < 0 || index >= niveaux.Length || niveaux[index] == null)
        {
            Debug.LogError($"MenuPrincipal : aucun niveau à l'index {index}.");
            return;
        }

        ChoixNiveau.Niveau = niveaux[index];
        SceneManager.LoadScene(sceneJeu);
    }

    public void Quitter()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
