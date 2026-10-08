using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Carquois : propose toujours une flèche à prendre.
/// Dès que le joueur en attrape une, une nouvelle apparaît.
/// </summary>
public class Carquois : MonoBehaviour
{
    [Tooltip("Prefab de la flèche proposée (changera avec les améliorations)")]
    [SerializeField] private Fleche flechePrefab;
    [Tooltip("Endroit où la flèche apparaît (axe Z bleu = direction de la pointe)")]
    [SerializeField] private Transform pointApparition;
    [Tooltip("Délai avant qu'une nouvelle flèche apparaisse (s)")]
    [SerializeField] private float delaiReapparition = 0.5f;

    private Fleche flecheEnAttente;

    /// <summary>Change le type de flèche (utilisé plus tard par les améliorations).</summary>
    public Fleche FlechePrefab
    {
        get => flechePrefab;
        set => flechePrefab = value;
    }

    private void Start()
    {
        CreerFleche();
    }

    private void CreerFleche()
    {
        if (flechePrefab == null || pointApparition == null)
        {
            Debug.LogWarning("Carquois : flechePrefab ou pointApparition non assigné.");
            return;
        }

        flecheEnAttente = Instantiate(flechePrefab, pointApparition.position, pointApparition.rotation);
        flecheEnAttente.Grab.selectEntered.AddListener(OnFlechePrise);
    }

    private void OnFlechePrise(SelectEnterEventArgs args)
    {
        flecheEnAttente.Grab.selectEntered.RemoveListener(OnFlechePrise);
        flecheEnAttente = null;
        Invoke(nameof(CreerFleche), delaiReapparition);
    }
}