// Renderiza a PNG las tres pantallas de la interfaz (Menú, Juego, Victoria) sin entrar en Play Mode,
// para revisar el diseño. No guarda la escena. Batch mode:
//   Unity -batchmode -projectPath <proyecto> -executeMethod UDV.VistaPreviaInterfaz.Generar -salida <carpeta> -quit

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Image = UnityEngine.UI.Image;

namespace UDV
{
    public static class VistaPreviaInterfaz
    {
        [MenuItem("UDV/Capturas/Vista previa de la interfaz")]
        public static void Generar()
        {
            string salida = Argumento("-salida") ?? "VistaPrevia";
            Directory.CreateDirectory(salida);

            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
            var canvasGo = GameObject.Find("Canvas");
            var canvas = canvasGo.GetComponent<Canvas>();

            var camGo = new GameObject("CamaraVistaPrevia");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.36f, 0.40f, 0.45f);
            cam.cullingMask = ~0;
            var rt = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = rt;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1f;

            var menu = Hijo(canvasGo, "PanelMenu");
            var juego = Hijo(canvasGo, "PanelJuego");
            var victoria = Hijo(canvasGo, "PanelVictoria");

            // Menú
            Mostrar(menu, juego, victoria, true, false, false);
            Render(cam, rt, Path.Combine(salida, "ui-menu.png"));

            // Juego: segundo acierto seguido
            Texto(juego, "BarraPregunta/TextoPregunta", "¿Con cuál escribes tus tareas?");
            Texto(juego, "PanelRacha/TextoRacha", "Racha 2 / 3");
            for (int i = 1; i <= 3; i++)
                Hijo(juego, "PanelRacha/Estrella" + i).GetComponent<Image>().color = i <= 2 ? new Color(0.96f, 0.72f, 0f) : new Color(0.79f, 0.81f, 0.85f);
            Hijo(juego, "PanelMensaje").GetComponent<Image>().color = new Color(0.18f, 0.62f, 0.31f, 0.95f);
            var msg = Texto(juego, "PanelMensaje/TextoMensaje", "¡Correcto! Es el teclado. ¡Te falta 1!");
            msg.color = Color.white;
            Texto(juego, "PanelMarcador/TextoMarcador", "Marcador: el teclado");
            Mostrar(menu, juego, victoria, false, true, false);
            Render(cam, rt, Path.Combine(salida, "ui-juego-correcto.png"));

            // Juego: error, la racha vuelve a 0
            Texto(juego, "PanelRacha/TextoRacha", "Racha 0 / 3");
            for (int i = 1; i <= 3; i++)
                Hijo(juego, "PanelRacha/Estrella" + i).GetComponent<Image>().color = new Color(0.79f, 0.81f, 0.85f);
            Hijo(juego, "PanelMensaje").GetComponent<Image>().color = new Color(0.75f, 0.22f, 0.17f, 0.95f);
            Texto(juego, "PanelMensaje/TextoMensaje", "Ups, enfocaste el ratón. Tu racha vuelve a 0");
            Texto(juego, "PanelMarcador/TextoMarcador", "Marcador: el ratón");
            Render(cam, rt, Path.Combine(salida, "ui-juego-incorrecto.png"));

            // Victoria
            Texto(victoria, "TarjetaVictoria/TextoResumen", "3 respuestas correctas seguidas\nen 5 intentos");
            Mostrar(menu, juego, victoria, false, true, true);
            Render(cam, rt, Path.Combine(salida, "ui-victoria.png"));

            UnityEngine.Object.DestroyImmediate(camGo);
            Debug.Log("[UDV] Vista previa de la interfaz en " + Path.GetFullPath(salida));
        }

        static void Mostrar(GameObject menu, GameObject juego, GameObject victoria, bool m, bool j, bool v)
        {
            menu.SetActive(m);
            juego.SetActive(j);
            victoria.SetActive(v);
        }

        static void Render(Camera cam, RenderTexture rt, string ruta)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var t in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
                t.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(ruta, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static GameObject Hijo(GameObject raiz, string ruta)
        {
            return raiz.transform.Find(ruta).gameObject;
        }

        static TextMeshProUGUI Texto(GameObject raiz, string ruta, string texto)
        {
            var t = Hijo(raiz, ruta).GetComponent<TextMeshProUGUI>();
            t.text = texto;
            return t;
        }

        static string Argumento(string nombre)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == nombre) return args[i + 1];
            return null;
        }
    }
}
