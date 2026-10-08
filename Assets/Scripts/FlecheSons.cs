using UnityEngine;

/// <summary>
/// Sons 3D de la flèche :
/// - sifflement au départ, qui voyage avec la flèche (effet Doppler) ;
/// - impact différent selon qu'elle touche une cible ou autre chose (sol, décor).
/// À placer sur le prefab FLECHE, avec une AudioSource sur le même objet.
/// </summary>
[RequireComponent(typeof(Fleche))]
[RequireComponent(typeof(AudioSource))]
public class FlecheSons : MonoBehaviour
{
    [Header("Sons")]
    [Tooltip("Sifflement / « whoosh » au départ de la flèche")]
    [SerializeField] private AudioClip sonDepart;
    [Tooltip("Impact sourd dans une cible")]
    [SerializeField] private AudioClip sonImpactCible;
    [Tooltip("Impact dans le sol ou le décor (optionnel)")]
    [SerializeField] private AudioClip sonImpactAutre;

    [Header("Réglages")]
    [Tooltip("Vitesse (m/s) à partir de laquelle le sifflement est au volume maximal")]
    [SerializeField] private float vitessePleinVolume = 30f;

    private Fleche fleche;
    private AudioSource source;

    private void Awake()
    {
        fleche = GetComponent<Fleche>();
        source = GetComponent<AudioSource>();
        ArcSons.Configurer3D(source, false);
        source.dopplerLevel = 1f;
    }

    private void OnEnable()
    {
        fleche.OnLancee += JouerDepart;
        fleche.OnPlantee += JouerImpact;
    }

    private void OnDisable()
    {
        fleche.OnLancee -= JouerDepart;
        fleche.OnPlantee -= JouerImpact;
    }

    private void JouerDepart(float vitesse)
    {
        if (sonDepart == null) return;
        source.pitch = Random.Range(0.95f, 1.1f);
        source.clip = sonDepart;
        source.volume = Mathf.Clamp01(vitesse / vitessePleinVolume);
        source.Play();
    }

    private void JouerImpact(Collider touche, Vector3 point, bool estCible)
    {
        source.Stop(); // coupe le sifflement
        source.volume = 1f;

        AudioClip clip = estCible ? sonImpactCible : (sonImpactAutre != null ? sonImpactAutre : sonImpactCible);
        if (clip == null) return;

        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(clip);
    }
}
