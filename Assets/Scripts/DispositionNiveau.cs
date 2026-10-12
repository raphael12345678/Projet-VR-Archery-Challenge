using UnityEngine;

/// <summary>
/// Groupe de cibles propre à un niveau.
/// À placer sur un objet vide qui contient les cibles de ce niveau.
/// Au lancement, le PartieManager active uniquement la disposition du niveau choisi.
/// </summary>
public class DispositionNiveau : MonoBehaviour
{
    [Tooltip("Niveau pour lequel cette disposition est utilisée")]
    [SerializeField] private ParametresNiveau niveau;

    public ParametresNiveau Niveau => niveau;
}
