using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Ambiance sonore spatialisée :
/// - des sons ponctuels (oiseaux, branches, vent...) joués de temps en temps
///   à des positions aléatoires autour du joueur, pour qu'on les entende venir de partout.
/// Les sons de fond en boucle (vent, rivière...) se font avec de simples AudioSource 3D placées dans le décor.
/// À placer sur un objet vide "Ambiance".
/// </summary>
public class AmbianceSonore : MonoBehaviour
{
    [Header("Sons ponctuels")]
    [SerializeField] private AudioClip[] sons;
    [Tooltip("Délai minimum entre deux sons (s)")]
    [SerializeField] private float delaiMin = 3f;
    [Tooltip("Délai maximum entre deux sons (s)")]
    [SerializeField] private float delaiMax = 8f;
    [Tooltip("Distance autour du joueur où apparaissent les sons (m)")]
    [SerializeField] private float distanceMin = 5f;
    [SerializeField] private float distanceMax = 15f;
    [Tooltip("Hauteur min / max des sons (m), par exemple dans les arbres")]
    [SerializeField] private Vector2 hauteur = new Vector2(1f, 6f);
    [Range(0f, 1f)] [SerializeField] private float volume = 0.6f;

    [Header("Mixage (optionnel)")]
    [Tooltip("Groupe de l'Audio Mixer (Ambiance), pour le réglage du volume dans le menu")]
    [SerializeField] private AudioMixerGroup groupe;

    private const int TaillePool = 4;
    private AudioSource[] pool;
    private int prochaineSource;
    private float prochainSon;
    private Transform joueur;

    private void Start()
    {
        joueur = Camera.main != null ? Camera.main.transform : transform;

        // Quelques AudioSource réutilisées, pour que plusieurs sons puissent se chevaucher
        pool = new AudioSource[TaillePool];
        for (int i = 0; i < TaillePool; i++)
        {
            var go = new GameObject($"SonAmbiance_{i}");
            go.transform.SetParent(transform);
            pool[i] = go.AddComponent<AudioSource>();
            ArcSons.Configurer3D(pool[i], false);
            pool[i].maxDistance = distanceMax * 2f;
            pool[i].outputAudioMixerGroup = groupe;
        }

        PlanifierProchainSon();
    }

    private void Update()
    {
        if (sons == null || sons.Length == 0 || Time.time < prochainSon) return;

        AudioSource source = pool[prochaineSource];
        prochaineSource = (prochaineSource + 1) % TaillePool;

        // Position aléatoire autour du joueur
        Vector2 direction = Random.insideUnitCircle.normalized * Random.Range(distanceMin, distanceMax);
        source.transform.position = new Vector3(
            joueur.position.x + direction.x,
            Random.Range(hauteur.x, hauteur.y),
            joueur.position.z + direction.y);

        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(sons[Random.Range(0, sons.Length)], volume);

        PlanifierProchainSon();
    }

    private void PlanifierProchainSon()
    {
        prochainSon = Time.time + Random.Range(delaiMin, delaiMax);
    }
}
