using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Cerebro del juego: guarda el banco de preguntas, elige una al azar,
/// valida la clave del marcador enfocado y actualiza la interfaz y la consola.
/// Va en un GameObject vacío llamado "Juego".
/// </summary>
public class GestorPreguntas : MonoBehaviour
{
    public enum Resultado { Correcto, Incorrecto, Ignorado }

    [Header("Banco de preguntas")]
    [SerializeField] private List<Pregunta> preguntas = new List<Pregunta>();

    [Header("Interfaz")]
    [SerializeField] private TMP_Text etiquetaPregunta;
    [SerializeField] private TMP_Text etiquetaMensaje;
    [SerializeField] private TMP_Text etiquetaPuntaje;

    [Header("Parámetros")]
    [SerializeField] private float segundosEntrePreguntas = 2.5f;

    private Pregunta actual;
    private int aciertos;
    private int intentos;
    private bool enEspera;

    void Start()
    {
        ActualizarPuntaje();
        SiguientePregunta();
    }

    /// <summary>Elige una pregunta al azar distinta de la anterior y la muestra.</summary>
    public void SiguientePregunta()
    {
        if (preguntas.Count == 0)
        {
            Debug.LogWarning("[Juego] No hay preguntas configuradas.");
            return;
        }

        Pregunta nueva = actual;
        if (preguntas.Count == 1)
        {
            nueva = preguntas[0];
        }
        else
        {
            while (nueva == actual)
            {
                nueva = preguntas[Random.Range(0, preguntas.Count)];
            }
        }

        actual = nueva;
        enEspera = false;
        etiquetaPregunta.text = actual.texto;
        etiquetaMensaje.text = "";
        Debug.Log("[Juego] Pregunta: " + actual.texto);
    }

    /// <summary>Valida la tarjeta enfocada contra la pregunta vigente.</summary>
    public Resultado Verificar(Tarjeta tarjeta)
    {
        if (actual == null || enEspera)
        {
            return Resultado.Ignorado;
        }

        intentos++;
        bool correcto = tarjeta.GetClave() == actual.claveRespuesta;

        if (correcto)
        {
            aciertos++;
            enEspera = true;
            etiquetaMensaje.text = "¡Correcto! Es " + tarjeta.GetNombre();
            Debug.Log("[Juego] Correcto: " + tarjeta.GetNombre());
            Invoke(nameof(SiguientePregunta), segundosEntrePreguntas);
        }
        else
        {
            etiquetaMensaje.text = "Incorrecto, enfocaste " + tarjeta.GetNombre() + ". Intenta de nuevo";
            Debug.Log("[Juego] Incorrecto: " + tarjeta.GetNombre());
        }

        ActualizarPuntaje();
        return correcto ? Resultado.Correcto : Resultado.Incorrecto;
    }

    /// <summary>Se llama al perder el marcador; no borra el mensaje de un acierto en espera.</summary>
    public void LimpiarMensaje()
    {
        if (!enEspera)
        {
            etiquetaMensaje.text = "";
        }
    }

    private void ActualizarPuntaje()
    {
        etiquetaPuntaje.text = "Aciertos: " + aciertos + " / Intentos: " + intentos;
    }
}
