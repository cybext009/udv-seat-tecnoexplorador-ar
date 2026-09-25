using UnityEngine;

/// <summary>
/// Va en cada image target. Se conecta a los eventos del Default Observer Event Handler:
/// On Target Found -> AlEncontrar, On Target Lost -> AlPerder (Runtime Only).
/// Consulta al GestorPreguntas y colorea la figura primitiva hija según el resultado.
/// </summary>
public class RespuestaRA : MonoBehaviour
{
    [SerializeField] private GestorPreguntas gestor;
    [SerializeField] private Renderer figura;

    private static readonly Color ColorReposo = Color.white;
    private static readonly Color ColorCorrecto = new Color(0.2f, 0.8f, 0.3f);
    private static readonly Color ColorIncorrecto = new Color(0.9f, 0.2f, 0.2f);

    private Tarjeta tarjeta;

    void Awake()
    {
        tarjeta = GetComponent<Tarjeta>();
        if (figura == null)
        {
            figura = GetComponentInChildren<Renderer>();
        }
    }

    public void AlEncontrar()
    {
        Debug.Log("[RA] Marcador enfocado: " + tarjeta.GetNombre());
        GestorPreguntas.Resultado resultado = gestor.Verificar(tarjeta);

        switch (resultado)
        {
            case GestorPreguntas.Resultado.Correcto:
                Pintar(ColorCorrecto);
                break;
            case GestorPreguntas.Resultado.Incorrecto:
                Pintar(ColorIncorrecto);
                break;
            default:
                Pintar(ColorReposo);
                break;
        }
    }

    public void AlPerder()
    {
        Pintar(ColorReposo);
        gestor.LimpiarMensaje();
    }

    private void Pintar(Color color)
    {
        if (figura != null)
        {
            figura.material.color = color;
        }
    }
}
