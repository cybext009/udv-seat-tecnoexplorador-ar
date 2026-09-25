// Script de editor que arma la escena del juego "TecnoExplorador AR" (Evaluación parcial 1)
// sobre la escena base del laboratorio. Menú: UDV > Construir escena Juego. Batch mode:
//   Unity -batchmode -projectPath <proyecto> -executeMethod UDV.ConstruirEscenaJuego.Construir -quit
// Idempotente: reutiliza los objetos que ya existen y ajusta solo lo que falta.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vuforia;

namespace UDV
{
    public static class ConstruirEscenaJuego
    {
        const string RutaEscena = "Assets/Scenes/SampleScene.unity";

        // Banco de preguntas para niños de 7 a 10 años. Clave = nombre del image target.
        static readonly string[,] Preguntas =
        {
            { "¿Con cuál escribes tus tareas?", "teclado" },
            { "¿Cuál tiene teclas con letras y números?", "teclado" },
            { "¿Cuál mueves con la mano para señalar en la pantalla?", "raton" },
            { "¿Con cuál haces clic?", "raton" },
            { "¿Cuál tiene una ruedita para subir y bajar la página?", "raton" },
            { "¿Cuál te da internet en tu casa?", "wifi" },
            { "¿Cuál tiene antenas y luces que parpadean?", "wifi" },
            { "¿Cuál conecta tu tablet a internet sin cables?", "wifi" },
            { "¿Cuál tiene la pantalla donde ves tus videos?", "computadora" },
            { "¿Cuál guarda tus juegos y programas?", "computadora" },
        };

        [MenuItem("UDV/Construir escena Juego")]
        public static void Construir()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            // 1. Los targets del laboratorio se apagan: el juego usa los cuatro nuevos.
            Apagar("servidor");
            Apagar("switch");

            // 2. Cuatro image targets con su figura primitiva.
            var computadora = ConstruirEscenaRA.ObtenerTarget("computadora", new Vector3(0, 0, 0));
            var teclado = ConstruirEscenaRA.ObtenerTarget("teclado", new Vector3(2, 0, 0));
            var raton = ConstruirEscenaRA.ObtenerTarget("raton", new Vector3(0, 0, -2));
            var wifi = ConstruirEscenaRA.ObtenerTarget("wifi", new Vector3(2, 0, -2));

            var figComputadora = ConstruirEscenaRA.ObtenerHijoPrimitivo(computadora, "Figura", PrimitiveType.Cube);
            var figTeclado = ConstruirEscenaRA.ObtenerHijoPrimitivo(teclado, "Figura", PrimitiveType.Cube);
            figTeclado.transform.localScale = new Vector3(0.6f, 0.08f, 0.25f);
            figTeclado.transform.localPosition = new Vector3(0, 0.04f, 0);
            var figRaton = ConstruirEscenaRA.ObtenerHijoPrimitivo(raton, "Figura", PrimitiveType.Sphere);
            var figWifi = ConstruirEscenaRA.ObtenerHijoPrimitivo(wifi, "Figura", PrimitiveType.Cylinder);
            figWifi.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
            figWifi.transform.localPosition = new Vector3(0, 0.1f, 0);

            // 3. Interfaz: pregunta arriba, mensaje abajo (reutiliza la etiqueta del laboratorio), puntaje arriba a la derecha.
            var canvas = GameObject.Find("Canvas");
            var etiquetaLab = canvas.transform.Find("Mensaje") ?? canvas.transform.Find("Text (TMP)");
            TextMeshProUGUI mensaje;
            if (etiquetaLab != null)
            {
                mensaje = etiquetaLab.GetComponent<TextMeshProUGUI>();
                mensaje.gameObject.name = "Mensaje";
                mensaje.fontSize = 56;
            }
            else
            {
                mensaje = ObtenerTexto(canvas, "Mensaje", new Vector2(0.5f, 0f), new Vector2(0, 60), new Vector2(1200, 120), 56, TextAlignmentOptions.Center);
            }
            var pregunta = ObtenerTexto(canvas, "Pregunta", new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(1700, 160), 64, TextAlignmentOptions.Center);
            var puntaje = ObtenerTexto(canvas, "Puntaje", new Vector2(1f, 1f), new Vector2(-40, -240), new Vector2(700, 80), 40, TextAlignmentOptions.Right);
            pregunta.color = new Color(1f, 0.92f, 0.3f);

            // 4. Objeto Juego con el gestor de preguntas.
            var juego = GameObject.Find("Juego") ?? new GameObject("Juego");
            var gestor = juego.GetComponent<GestorPreguntas>() ?? juego.AddComponent<GestorPreguntas>();
            var sg = new SerializedObject(gestor);
            sg.FindProperty("etiquetaPregunta").objectReferenceValue = pregunta;
            sg.FindProperty("etiquetaMensaje").objectReferenceValue = mensaje;
            sg.FindProperty("etiquetaPuntaje").objectReferenceValue = puntaje;
            sg.FindProperty("segundosEntrePreguntas").floatValue = 2.5f;
            var lista = sg.FindProperty("preguntas");
            lista.arraySize = Preguntas.GetLength(0);
            for (int i = 0; i < lista.arraySize; i++)
            {
                var p = lista.GetArrayElementAtIndex(i);
                p.FindPropertyRelative("texto").stringValue = Preguntas[i, 0];
                p.FindPropertyRelative("claveRespuesta").stringValue = Preguntas[i, 1];
            }
            sg.ApplyModifiedPropertiesWithoutUndo();

            // 5. Cada target: Tarjeta (nombre + clave) y RespuestaRA enlazada a los eventos de Vuforia.
            Enlazar(computadora, "la computadora", "computadora", figComputadora, gestor);
            Enlazar(teclado, "el teclado", "teclado", figTeclado, gestor);
            Enlazar(raton, "el ratón", "raton", figRaton, gestor);
            Enlazar(wifi, "el router Wi-Fi", "wifi", figWifi, gestor);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[UDV] Escena Juego construida y guardada: " + RutaEscena);
        }

        static void Apagar(string nombre)
        {
            var go = GameObject.Find(nombre);
            if (go != null) go.SetActive(false);
        }

        static TextMeshProUGUI ObtenerTexto(GameObject canvas, string nombre, Vector2 ancla, Vector2 posicion, Vector2 tamano, float fuente, TextAlignmentOptions alineacion)
        {
            var t = canvas.transform.Find(nombre);
            TextMeshProUGUI tmp;
            if (t != null)
            {
                tmp = t.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                var go = new GameObject(nombre, typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                tmp = go.AddComponent<TextMeshProUGUI>();
            }
            var rt = tmp.rectTransform;
            rt.anchorMin = ancla;
            rt.anchorMax = ancla;
            rt.pivot = ancla;
            rt.anchoredPosition = posicion;
            rt.sizeDelta = tamano;
            tmp.fontSize = fuente;
            tmp.alignment = alineacion;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.text = "";
            if (tmp.font == null && TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
                tmp.fontSharedMaterial = tmp.font.material;
            }
            return tmp;
        }

        static void Enlazar(GameObject target, string nombre, string clave, GameObject figura, GestorPreguntas gestor)
        {
            // El presentador del laboratorio ya no aplica en los targets del juego.
            var viejo = target.GetComponent<PresentadorTexto>();
            if (viejo != null) Object.DestroyImmediate(viejo);

            var tarjeta = target.GetComponent<Tarjeta>() ?? target.AddComponent<Tarjeta>();
            var st = new SerializedObject(tarjeta);
            st.FindProperty("nombre").stringValue = nombre;
            st.FindProperty("clave").stringValue = clave;
            st.ApplyModifiedPropertiesWithoutUndo();

            var respuesta = target.GetComponent<RespuestaRA>() ?? target.AddComponent<RespuestaRA>();
            var sr = new SerializedObject(respuesta);
            sr.FindProperty("gestor").objectReferenceValue = gestor;
            sr.FindProperty("figura").objectReferenceValue = figura.GetComponent<Renderer>();
            sr.ApplyModifiedPropertiesWithoutUndo();

            var handler = target.GetComponent<DefaultObserverEventHandler>();
            ConstruirEscenaRA.Reemplazar(handler.OnTargetFound, respuesta, respuesta.AlEncontrar);
            ConstruirEscenaRA.Reemplazar(handler.OnTargetLost, respuesta, respuesta.AlPerder);
        }
    }
}
