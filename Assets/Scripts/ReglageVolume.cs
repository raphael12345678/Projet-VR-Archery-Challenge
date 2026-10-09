using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Relie un curseur (Slider) à un volume de l'Audio Mixer, et mémorise le réglage.
/// Un composant par curseur : Général, Effets, Ambiance.
/// </summary>
[RequireComponent(typeof(Slider))]
public class ReglageVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [Tooltip("Nom du paramètre exposé dans le mixer (ex. VolumeGeneral, VolumeEffets, VolumeAmbiance)")]
    [SerializeField] private string parametre = "VolumeGeneral";

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    private void Start()
    {
        // Réglage mémorisé d'une partie à l'autre (1 = volume maximal par défaut)
        float valeur = PlayerPrefs.GetFloat(parametre, 1f);
        slider.SetValueWithoutNotify(valeur);
        Appliquer(mixer, parametre, valeur);
        slider.onValueChanged.AddListener(Changer);
    }

    private void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(Changer);
    }

    private void Changer(float valeur)
    {
        Appliquer(mixer, parametre, valeur);
        PlayerPrefs.SetFloat(parametre, valeur);
        PlayerPrefs.Save();
    }

    /// <summary>Convertit un volume 0-1 en décibels (le mixer travaille en dB : 0 dB = normal, -80 dB = muet).</summary>
    public static void Appliquer(AudioMixer mixer, string parametre, float valeur)
    {
        if (mixer == null) return;
        float db = valeur <= 0.0001f ? -80f : Mathf.Log10(valeur) * 20f;
        mixer.SetFloat(parametre, db);
    }
}
