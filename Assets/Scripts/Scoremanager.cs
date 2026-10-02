using System;
using UnityEngine;

/// <summary>
/// Garde le score de la partie. Un seul dans la scène (sur un objet "GameManager").
/// Les autres scripts y accèdent via ScoreManager.Instance.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }

    /// <summary>Déclenché à chaque changement de score (nouveau total).</summary>
    public event Action<int> OnScoreChange;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AjouterPoints(int points)
    {
        Score += points;
        OnScoreChange?.Invoke(Score);
    }

    public void Reinitialiser()
    {
        Score = 0;
        OnScoreChange?.Invoke(Score);
    }
}