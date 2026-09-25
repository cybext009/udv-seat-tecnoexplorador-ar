using UnityEngine;

/// <summary>
/// Representa la información asociada a un marcador (image target).
/// El nombre se configura desde el inspector de Unity gracias a [SerializeField].
/// </summary>
public class Tarjeta : MonoBehaviour
{
    [SerializeField] private string nombre;

    public string GetNombre()
    {
        return nombre;
    }
}
