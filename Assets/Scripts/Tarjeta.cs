using UnityEngine;

/// <summary>
/// Información asociada a un marcador (image target).
/// nombre: texto que se muestra al niño. clave: identificador con el que se valida la respuesta.
/// Ambos se configuran desde el inspector gracias a [SerializeField].
/// </summary>
public class Tarjeta : MonoBehaviour
{
    [SerializeField] private string nombre;
    [SerializeField] private string clave;

    public string GetNombre()
    {
        return nombre;
    }

    public string GetClave()
    {
        return clave;
    }
}
