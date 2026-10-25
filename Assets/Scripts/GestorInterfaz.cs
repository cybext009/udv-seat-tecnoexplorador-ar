using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra el panel que corresponde al estado del juego (Menú, Juego, Victoria) y actualiza
/// en tiempo real la pregunta, la racha con sus estrellas, el mensaje y el marcador enfocado.
/// Se suscribe a los eventos del GestorPreguntas. Va en el Canvas.
/// </summary>
public class GestorInterfaz : MonoBehaviour
{
    [SerializeField] private GestorPreguntas juego;

    [Header("Paneles")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelJuego;
    [SerializeField] private GameObject panelVictoria;

    [Header("Pantalla de juego")]
    [SerializeField] private TMP_Text textoPregunta;
    [SerializeField] private TMP_Text textoRacha;
    [SerializeField] private Image[] estrellas;
    [SerializeField] private Image fondoMensaje;
    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private TMP_Text textoMarcador;

    [Header("Victoria")]
    [SerializeField] private TMP_Text textoResumen;

    [Header("Colores")]
    [SerializeField] private Color estrellaLlena = new Color(0.96f, 0.72f, 0f);
    [SerializeField] private Color estrellaVacia = new Color(0.79f, 0.81f, 0.85f);
    [SerializeField] private Color mensajeNeutro = new Color(1f, 1f, 1f, 0.92f);
    [SerializeField] private Color mensajeCorrecto = new Color(0.18f, 0.62f, 0.31f, 0.95f);
    [SerializeField] private Color mensajeIncorrecto = new Color(0.75f, 0.22f, 0.17f, 0.95f);
    [SerializeField] private Color textoOscuro = new Color(0f, 0.1f, 0.27f);

    void OnEnable()
    {
        juego.AlCambiarEstado += MostrarEstado;
        juego.AlCambiarPregunta += MostrarPregunta;
        juego.AlCambiarRacha += MostrarRacha;
        juego.AlResponder += MostrarResultado;
        juego.AlEnfocarMarcador += MostrarMarcador;
        juego.AlGanar += MostrarVictoria;
    }

    void OnDisable()
    {
        juego.AlCambiarEstado -= MostrarEstado;
        juego.AlCambiarPregunta -= MostrarPregunta;
        juego.AlCambiarRacha -= MostrarRacha;
        juego.AlResponder -= MostrarResultado;
        juego.AlEnfocarMarcador -= MostrarMarcador;
        juego.AlGanar -= MostrarVictoria;
    }

    private void MostrarEstado(GestorPreguntas.Estado estado)
    {
        panelMenu.SetActive(estado == GestorPreguntas.Estado.Menu);
        panelJuego.SetActive(estado != GestorPreguntas.Estado.Menu);
        panelVictoria.SetActive(estado == GestorPreguntas.Estado.Victoria);
    }

    private void MostrarPregunta(Pregunta pregunta)
    {
        textoPregunta.text = pregunta.texto;
        Mensaje("Busca la tarjeta que responde la pregunta", mensajeNeutro, textoOscuro);
    }

    private void MostrarRacha(int racha, int meta)
    {
        textoRacha.text = "Racha " + racha + " / " + meta;
        for (int i = 0; i < estrellas.Length; i++)
        {
            estrellas[i].color = i < racha ? estrellaLlena : estrellaVacia;
        }
    }

    private void MostrarResultado(GestorPreguntas.Resultado resultado, Tarjeta tarjeta)
    {
        if (resultado == GestorPreguntas.Resultado.Correcto)
        {
            int faltan = juego.Meta - juego.Racha;
            string extra = faltan == 1 ? " ¡Te falta 1!" : faltan > 1 ? " Te faltan " + faltan + "." : "";
            Mensaje("¡Correcto! Es " + tarjeta.GetNombre() + "." + extra, mensajeCorrecto, Color.white);
        }
        else if (resultado == GestorPreguntas.Resultado.Incorrecto)
        {
            Mensaje("Ups, enfocaste " + tarjeta.GetNombre() + ". Tu racha vuelve a 0", mensajeIncorrecto, Color.white);
        }
    }

    private void MostrarMarcador(Tarjeta tarjeta)
    {
        textoMarcador.text = tarjeta == null ? "Marcador: ninguno" : "Marcador: " + tarjeta.GetNombre();
    }

    private void MostrarVictoria(int intentos)
    {
        textoResumen.text = juego.Meta + " respuestas correctas seguidas\nen " + intentos + (intentos == 1 ? " intento" : " intentos");
    }

    private void Mensaje(string texto, Color fondo, Color letra)
    {
        textoMensaje.text = texto;
        textoMensaje.color = letra;
        fondoMensaje.color = fondo;
    }
}
