using UnityEngine;
using TMPro;

/// <summary>
/// Enlaza la Tarjeta del image target con una etiqueta TextMeshPro de la interfaz.
/// Se conecta a los eventos On Target Found (Saludar) y On Target Lost (LimpiarTexto)
/// del componente Default Observer Event Handler de Vuforia.
/// </summary>
public class PresentadorTexto : MonoBehaviour
{
    [SerializeField] private TMP_Text TMPEtiqueta;
    private Tarjeta tarjeta;

    void Awake()
    {
        tarjeta = GetComponent<Tarjeta>();
    }

    public void Saludar()
    {
        Debug.Log("Hola " + tarjeta.GetNombre());
        TMPEtiqueta.text = tarjeta.GetNombre();
    }

    public void LimpiarTexto()
    {
        TMPEtiqueta.text = "...";
    }
}
