using System.Collections;
using UnityEngine;

/// <summary>
/// Animaciones por código del modelo 3D según lo que pasa en el juego: salto al acertar,
/// sacudida al fallar y giro de celebración al ganar. Va en el objeto "Pivote" de cada marcador;
/// el flotado en reposo lo hace el Animator del hijo "Modelo" (clip Flotar, creado en el editor),
/// así las dos animaciones no se pelean por el mismo Transform.
/// </summary>
public class AnimacionModelo : MonoBehaviour
{
    [Header("Salto (correcto)")]
    [SerializeField] private float alturaSalto = 0.25f;
    [SerializeField] private float escalaSalto = 1.3f;
    [SerializeField] private float duracionSalto = 0.5f;

    [Header("Sacudida (incorrecto)")]
    [SerializeField] private float amplitudSacudida = 0.12f;
    [SerializeField] private float duracionSacudida = 0.6f;

    [Header("Celebración (victoria)")]
    [SerializeField] private float vueltasCelebracion = 2f;
    [SerializeField] private float duracionCelebracion = 1.4f;

    private Vector3 posicionBase;
    private Vector3 escalaBase;
    private Quaternion rotacionBase;
    private Coroutine animacionActual;

    void Awake()
    {
        posicionBase = transform.localPosition;
        escalaBase = transform.localScale;
        rotacionBase = transform.localRotation;
    }

    public void Saltar()
    {
        Iniciar(CoSaltar());
    }

    public void Sacudir()
    {
        Iniciar(CoSacudir());
    }

    public void Celebrar()
    {
        Iniciar(CoCelebrar());
    }

    private void Iniciar(IEnumerator animacion)
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }
        Restaurar();
        animacionActual = StartCoroutine(animacion);
    }

    private IEnumerator CoSaltar()
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracionSalto)
        {
            float curva = Mathf.Sin(t * Mathf.PI);   // 0 -> 1 -> 0
            transform.localPosition = posicionBase + Vector3.up * alturaSalto * curva;
            transform.localScale = escalaBase * (1f + (escalaSalto - 1f) * curva);
            yield return null;
        }
        Restaurar();
    }

    private IEnumerator CoSacudir()
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracionSacudida)
        {
            float vaiven = Mathf.Sin(t * Mathf.PI * 6f) * (1f - t);   // tres idas y vueltas que se apagan
            transform.localPosition = posicionBase + Vector3.right * amplitudSacudida * vaiven;
            yield return null;
        }
        Restaurar();
    }

    private IEnumerator CoCelebrar()
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracionCelebracion)
        {
            float avance = 1f - Mathf.Pow(1f - t, 3f);   // arranca rápido y frena al final
            transform.localRotation = rotacionBase * Quaternion.Euler(0f, 360f * vueltasCelebracion * avance, 0f);
            transform.localScale = escalaBase * (1f + 0.2f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        Restaurar();
    }

    private void Restaurar()
    {
        transform.localPosition = posicionBase;
        transform.localScale = escalaBase;
        transform.localRotation = rotacionBase;
    }
}
