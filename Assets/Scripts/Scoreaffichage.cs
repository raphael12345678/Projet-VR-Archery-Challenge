using TMPro;
using UnityEngine;

/// <summary>
/// Affiche le score en temps réel dans un texte TextMeshPro (Canvas en World Space).
/// </summary>
public class ScoreAffichage : MonoBehaviour
{
    [SerializeField] private TMP_Text texteScore;

    private void Start()
    {
        if (ScoreManager.Instance == null) return;
        ScoreManager.Instance.OnScoreChange += MettreAJour;
        MettreAJour(ScoreManager.Instance.Score);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChange -= MettreAJour;
    }

    private void MettreAJour(int score)
    {
        texteScore.text = $"Score : {score}";
    }
}