using UnityEngine;

/// <summary>
/// Va en cada image target. Se conecta a los eventos del Default Observer Event Handler:
/// On Target Found -> AlEncontrar, On Target Lost -> AlPerder (Runtime Only).
/// Consulta al GestorPreguntas y anima el modelo 3D según el resultado.
/// </summary>
public class RespuestaRA : MonoBehaviour
{
    [SerializeField] private GestorPreguntas gestor;
    [SerializeField] private AnimacionModelo animacion;

    private Tarjeta tarjeta;

    void Awake()
    {
        tarjeta = GetComponent<Tarjeta>();
        if (animacion == null)
        {
            animacion = GetComponentInChildren<AnimacionModelo>(true);
        }
    }

    void OnEnable()
    {
        gestor.AlGanar += Celebrar;
    }

    void OnDisable()
    {
        gestor.AlGanar -= Celebrar;
    }

    public void AlEncontrar()
    {
        Debug.Log("[RA] Marcador enfocado: " + tarjeta.GetNombre());
        GestorPreguntas.Resultado resultado = gestor.Verificar(tarjeta);

        if (animacion == null)
        {
            return;
        }
        if (resultado == GestorPreguntas.Resultado.Correcto)
        {
            animacion.Saltar();
        }
        else if (resultado == GestorPreguntas.Resultado.Incorrecto)
        {
            animacion.Sacudir();
        }
    }

    public void AlPerder()
    {
        gestor.MarcadorPerdido(tarjeta);
    }

    private void Celebrar(int intentos)
    {
        if (animacion != null)
        {
            animacion.Celebrar();
        }
    }
}
