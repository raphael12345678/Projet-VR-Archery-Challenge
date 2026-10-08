using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Arc avec geste de tir naturel :
/// 1. Une main tient l'arc (point Attach).
/// 2. On pose une flèche sur l'encoche (socket sur la corde).
/// 3. L'autre main attrape la corde (PointCorde) et tire vers l'arrière.
/// 4. On relâche : la flèche part, d'autant plus vite que la corde était tendue.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class Arc : MonoBehaviour
{
    [Header("Points de l'arc")]
    [Tooltip("Endroit où la main tient l'arc")]
    [SerializeField] private Transform attachPoint;
    [Tooltip("Direction du tir (axe Z bleu vers la cible)")]
    [SerializeField] private Transform firePoint;

    [Header("Corde")]
    [Tooltip("Point au milieu de la corde, que la main attrape pour tirer")]
    [SerializeField] private XRSimpleInteractable pointCorde;
    [Tooltip("Socket enfant de PointCorde, où se pose la flèche")]
    [SerializeField] private XRSocketInteractor encoche;

    [Header("Affichage de la corde (optionnel)")]
    [SerializeField] private LineRenderer ligneCorde;
    [SerializeField] private Transform hautCorde;
    [SerializeField] private Transform basCorde;
    [Tooltip("Mesh de corde d'origine, caché quand la ligne est utilisée")]
    [SerializeField] private GameObject cordeOriginale;

    [Header("Caractéristiques de l'arc")]
    [Tooltip("Distance maximale de tirage de la corde (mètres)")]
    [SerializeField] private float tirageMax = 0.5f;
    [Tooltip("Vitesse de la flèche à tension maximale (m/s)")]
    [SerializeField] private float vitesseMax = 40f;
    [Tooltip("Tension minimale (0 à 1) pour que la flèche parte")]
    [Range(0f, 1f)] [SerializeField] private float tensionMinTir = 0.1f;

    private XRGrabInteractable grab;
    private Collider[] collidersArc;
    private Vector3 positionReposLocale;
    private IXRSelectInteractor mainSurCorde;
    private float tirage;

    /// <summary>Tension actuelle de 0 à 1 (pour sons, vibrations, UI).</summary>
    public float Tension => tirageMax > 0f ? tirage / tirageMax : 0f;

    /// <summary>Déclenché à chaque tir, avec la tension utilisée.</summary>
    public event Action<float> OnTir;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        collidersArc = GetComponentsInChildren<Collider>();
        positionReposLocale = pointCorde.transform.localPosition;

        if (attachPoint != null)
            grab.attachTransform = attachPoint;

        if (ligneCorde != null)
        {
            ligneCorde.useWorldSpace = true;
            ligneCorde.positionCount = 3;
            if (cordeOriginale != null) cordeOriginale.SetActive(false);
        }
    }

    private void OnEnable()
    {
        pointCorde.selectEntered.AddListener(OnCordeAttrapee);
        pointCorde.selectExited.AddListener(OnCordeRelachee);
        grab.selectExited.AddListener(OnArcLache);
    }

    private void OnDisable()
    {
        pointCorde.selectEntered.RemoveListener(OnCordeAttrapee);
        pointCorde.selectExited.RemoveListener(OnCordeRelachee);
        grab.selectExited.RemoveListener(OnArcLache);
    }

    private void Update()
    {
        // L'encoche est toujours orientée dans la direction du tir
        encoche.transform.rotation = firePoint.rotation;

        if (mainSurCorde != null && grab.isSelected)
        {
            Vector3 repos = transform.TransformPoint(positionReposLocale);
            Vector3 arriere = -firePoint.forward;
            Vector3 main = mainSurCorde.GetAttachTransform(pointCorde).position;

            // On ne garde que le recul le long de l'axe de tir
            tirage = Mathf.Clamp(Vector3.Dot(main - repos, arriere), 0f, tirageMax);
            pointCorde.transform.position = repos + arriere * tirage;
        }

        MettreAJourLigne();
    }

    private void OnCordeAttrapee(SelectEnterEventArgs args)
    {
        mainSurCorde = args.interactorObject;
    }

    private void OnCordeRelachee(SelectExitEventArgs args)
    {
        float tension = Tension;
        mainSurCorde = null;

        bool tirAutorise = PartieManager.Instance == null || PartieManager.Instance.TirAutorise;
        if (tirAutorise && grab.isSelected && tension >= tensionMinTir && encoche.hasSelection)
        {
            IXRSelectInteractable selection = encoche.firstInteractableSelected;
            Fleche fleche = selection.transform.GetComponent<Fleche>();
            if (fleche != null)
            {
                // On désactive l'encoche pour qu'elle ne rattrape pas la flèche
                encoche.socketActive = false;
                encoche.interactionManager.SelectExit(encoche, selection);

                fleche.Lancer(firePoint.forward * vitesseMax * tension, collidersArc);
                OnTir?.Invoke(tension);
                StartCoroutine(ReactiverEncoche());
            }
        }

        // La corde revient en place
        tirage = 0f;
        pointCorde.transform.localPosition = positionReposLocale;
    }

    private void OnArcLache(SelectExitEventArgs args)
    {
        // Si on lâche l'arc pendant qu'on tient la corde : pas de tir, la corde se relâche
        if (mainSurCorde != null)
            pointCorde.interactionManager.SelectExit(mainSurCorde, pointCorde);
    }

    private IEnumerator ReactiverEncoche()
    {
        yield return new WaitForSeconds(0.5f);
        encoche.socketActive = true;
    }

    private void MettreAJourLigne()
    {
        if (ligneCorde == null || hautCorde == null || basCorde == null) return;
        ligneCorde.SetPosition(0, hautCorde.position);
        ligneCorde.SetPosition(1, pointCorde.transform.position);
        ligneCorde.SetPosition(2, basCorde.position);
    }
}