using System;

/// <summary>
/// Una pregunta del juego y la clave de la tarjeta que la responde.
/// Es serializable para poder editar el banco de preguntas desde el inspector.
/// </summary>
[Serializable]
public class Pregunta
{
    public string texto;
    public string claveRespuesta;
}
