using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cerebro del juego: guarda el banco de preguntas, elige una al azar, valida la clave
/// del marcador enfocado y lleva la racha. Para ganar hay que acertar "meta" veces seguidas;
/// un error regresa la racha a cero.
/// No conoce la interfaz, el audio ni las animaciones: solo avisa con eventos y ellos se suscriben.
/// Va en un GameObject vacío llamado "Juego".
/// </summary>
public class GestorPreguntas : MonoBehaviour
{
    public enum Resultado { Correcto, Incorrecto, Ignorado }
    public enum Estado { Menu, Jugando, Victoria }

    [Header("Banco de preguntas")]
    [SerializeField] private List<Pregunta> preguntas = new List<Pregunta>();

    [Header("Reglas del juego")]
    [SerializeField] private int meta = 3;
    [SerializeField] private float segundosEntrePreguntas = 2.5f;

    public event Action<Estado> AlCambiarEstado;
    public event Action<Pregunta> AlCambiarPregunta;
    public event Action<int, int> AlCambiarRacha;            // racha actual, meta
    public event Action<Resultado, Tarjeta> AlResponder;
    public event Action<Tarjeta> AlEnfocarMarcador;          // null = la cámara no ve ningún marcador
    public event Action<int> AlGanar;                         // intentos usados

    public Estado EstadoActual { get; private set; } = Estado.Menu;
    public int Racha { get; private set; }
    public int Intentos { get; private set; }
    public int Meta => meta;

    private Pregunta actual;
    private bool enEspera;

    void Start()
    {
        CambiarEstado(Estado.Menu);
    }

    /// <summary>Botón Jugar / Jugar de nuevo: racha en cero y primera pregunta.</summary>
    public void IniciarJuego()
    {
        CancelInvoke();
        Racha = 0;
        Intentos = 0;
        actual = null;
        enEspera = false;
        CambiarEstado(Estado.Jugando);
        AlCambiarRacha?.Invoke(Racha, meta);
        SiguientePregunta();
    }

    /// <summary>Botón Menú: detiene la partida y muestra el menú principal.</summary>
    public void VolverAlMenu()
    {
        CancelInvoke();
        enEspera = false;
        CambiarEstado(Estado.Menu);
    }

    /// <summary>Botón Salir: cierra la aplicación (en el editor detiene el modo Play).</summary>
    public void Salir()
    {
        Debug.Log("[Juego] Salir");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Elige una pregunta al azar distinta de la anterior y la anuncia.</summary>
    public void SiguientePregunta()
    {
        if (EstadoActual != Estado.Jugando)
        {
            return;
        }
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
                nueva = preguntas[UnityEngine.Random.Range(0, preguntas.Count)];
            }
        }

        actual = nueva;
        enEspera = false;
        Debug.Log("[Juego] Pregunta: " + actual.texto);
        AlCambiarPregunta?.Invoke(actual);
    }

    /// <summary>Valida la tarjeta enfocada contra la pregunta vigente y actualiza la racha.</summary>
    public Resultado Verificar(Tarjeta tarjeta)
    {
        AlEnfocarMarcador?.Invoke(tarjeta);

        if (EstadoActual != Estado.Jugando || actual == null || enEspera)
        {
            return Resultado.Ignorado;
        }

        Intentos++;
        bool correcto = tarjeta.GetClave() == actual.claveRespuesta;
        Resultado resultado = correcto ? Resultado.Correcto : Resultado.Incorrecto;

        if (correcto)
        {
            Racha++;
            Debug.Log("[Juego] Correcto: " + tarjeta.GetNombre() + " (racha " + Racha + "/" + meta + ")");
        }
        else
        {
            Racha = 0;
            Debug.Log("[Juego] Incorrecto: " + tarjeta.GetNombre() + " (racha vuelve a 0)");
        }

        AlCambiarRacha?.Invoke(Racha, meta);
        AlResponder?.Invoke(resultado, tarjeta);

        if (correcto)
        {
            enEspera = true;
            if (Racha >= meta)
            {
                Ganar();
            }
            else
            {
                Invoke(nameof(SiguientePregunta), segundosEntrePreguntas);
            }
        }

        return resultado;
    }

    /// <summary>Se llama cuando la cámara deja de ver un marcador.</summary>
    public void MarcadorPerdido(Tarjeta tarjeta)
    {
        AlEnfocarMarcador?.Invoke(null);
    }

    private void Ganar()
    {
        Debug.Log("[Juego] ¡Ganó! " + meta + " seguidas en " + Intentos + " intentos");
        CambiarEstado(Estado.Victoria);
        AlGanar?.Invoke(Intentos);
    }

    private void CambiarEstado(Estado nuevo)
    {
        EstadoActual = nuevo;
        Debug.Log("[Juego] Estado: " + nuevo);
        AlCambiarEstado?.Invoke(nuevo);
    }
}
