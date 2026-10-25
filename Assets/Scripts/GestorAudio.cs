using UnityEngine;

/// <summary>
/// Música de fondo en bucle y efectos de sonido del juego. Usa dos AudioSource:
/// uno para la música (Loop) y otro para los efectos (PlayOneShot, que permite encimarlos).
/// Se suscribe a los eventos del GestorPreguntas. Va en un GameObject llamado "Audio".
/// </summary>
public class GestorAudio : MonoBehaviour
{
    [SerializeField] private GestorPreguntas juego;
    [SerializeField] private AudioSource fuenteMusica;
    [SerializeField] private AudioSource fuenteEfectos;

    [Header("Clips")]
    [SerializeField] private AudioClip musica;
    [SerializeField] private AudioClip sonidoInicio;
    [SerializeField] private AudioClip sonidoCorrecto;
    [SerializeField] private AudioClip sonidoIncorrecto;
    [SerializeField] private AudioClip sonidoVictoria;

    [Header("Volumen")]
    [Range(0f, 1f)] [SerializeField] private float volumenMusica = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float volumenMusicaVictoria = 0.1f;

    void OnEnable()
    {
        juego.AlCambiarEstado += AlCambiarEstado;
        juego.AlResponder += AlResponder;
        juego.AlGanar += AlGanar;
    }

    void OnDisable()
    {
        juego.AlCambiarEstado -= AlCambiarEstado;
        juego.AlResponder -= AlResponder;
        juego.AlGanar -= AlGanar;
    }

    void Start()
    {
        if (musica != null)
        {
            fuenteMusica.clip = musica;
            fuenteMusica.loop = true;
            fuenteMusica.volume = volumenMusica;
            fuenteMusica.Play();
        }
    }

    private void AlCambiarEstado(GestorPreguntas.Estado estado)
    {
        if (estado == GestorPreguntas.Estado.Jugando)
        {
            fuenteMusica.volume = volumenMusica;
            Reproducir(sonidoInicio);
        }
        else if (estado == GestorPreguntas.Estado.Victoria)
        {
            fuenteMusica.volume = volumenMusicaVictoria;
        }
        else
        {
            fuenteMusica.volume = volumenMusica;
        }
    }

    private void AlResponder(GestorPreguntas.Resultado resultado, Tarjeta tarjeta)
    {
        if (resultado == GestorPreguntas.Resultado.Incorrecto)
        {
            Reproducir(sonidoIncorrecto);
        }
        else if (resultado == GestorPreguntas.Resultado.Correcto && juego.Racha < juego.Meta)
        {
            // El tercer acierto no suena aquí: lo cubre la fanfarria de victoria.
            Reproducir(sonidoCorrecto);
        }
    }

    private void AlGanar(int intentos)
    {
        Reproducir(sonidoVictoria);
    }

    private void Reproducir(AudioClip clip)
    {
        if (clip != null)
        {
            fuenteEfectos.PlayOneShot(clip);
        }
    }
}
