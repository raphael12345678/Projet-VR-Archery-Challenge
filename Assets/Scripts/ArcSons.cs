using UnityEngine;

/// <summary>
/// Sons 3D de l'arc :
/// - grincement de la corde pendant qu'on la tend (plus fort quand on tire vite, plus aigu quand elle est tendue) ;
/// - claquement de la corde au relâchement.
/// À placer sur ARC. Les AudioSource sont de préférence sur PointCorde pour que le son vienne de la corde.
/// </summary>
public class ArcSons : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Arc arc;
    [Tooltip("AudioSource en boucle pour le grincement (sur PointCorde)")]
    [SerializeField] private AudioSource sourceTension;
    [Tooltip("AudioSource pour le claquement (sur PointCorde)")]
    [SerializeField] private AudioSource sourceRelachement;

    [Header("Sons")]
    [Tooltip("Grincement de bois / corde, en boucle")]
    [SerializeField] private AudioClip sonTension;
    [Tooltip("Claquement sec de la corde")]
    [SerializeField] private AudioClip sonRelachement;

    [Header("Réglages")]
    [Tooltip("Plus la valeur est grande, plus le grincement réagit aux petits mouvements")]
    [SerializeField] private float sensibilite = 6f;
    [SerializeField] private float pitchMin = 0.8f;
    [SerializeField] private float pitchMax = 1.3f;

    private float tensionPrecedente;
    private float volumeLisse;

    private void Awake()
    {
        if (arc == null) arc = GetComponent<Arc>();
        Configurer3D(sourceTension, true);
        Configurer3D(sourceRelachement, false);

        if (sourceTension != null && sonTension != null)
        {
            sourceTension.clip = sonTension;
            sourceTension.volume = 0f;
            sourceTension.Play();
        }
    }

    private void OnEnable()  { if (arc != null) arc.OnTir += JouerRelachement; }
    private void OnDisable() { if (arc != null) arc.OnTir -= JouerRelachement; }

    private void Update()
    {
        if (arc == null || sourceTension == null) return;

        // Le grincement suit la vitesse de tirage : silence quand la corde ne bouge pas
        float tension = arc.Tension;
        float vitesseTirage = Mathf.Abs(tension - tensionPrecedente) / Mathf.Max(Time.deltaTime, 0.0001f);
        tensionPrecedente = tension;

        float volumeCible = tension > 0.01f ? Mathf.Clamp01(vitesseTirage * sensibilite * 0.1f) : 0f;
        volumeLisse = Mathf.Lerp(volumeLisse, volumeCible, Time.deltaTime * 12f);

        sourceTension.volume = volumeLisse;
        sourceTension.pitch = Mathf.Lerp(pitchMin, pitchMax, tension);
    }

    private void JouerRelachement(float tension)
    {
        volumeLisse = 0f;
        if (sourceRelachement == null || sonRelachement == null) return;

        sourceRelachement.pitch = Random.Range(0.95f, 1.05f);
        sourceRelachement.PlayOneShot(sonRelachement, Mathf.Lerp(0.4f, 1f, tension));
    }

    /// <summary>Force un son spatialisé (3D), même si l'AudioSource a été mal réglée.</summary>
    public static void Configurer3D(AudioSource source, bool boucle)
    {
        if (source == null) return;
        source.spatialBlend = 1f;
        source.playOnAwake = false;
        source.loop = boucle;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 0.5f;
        source.maxDistance = 30f;
    }
}
