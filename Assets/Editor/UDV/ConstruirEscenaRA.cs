// Script de editor que termina de armar la escena del laboratorio de RA (pasos 5 y 6 del enunciado).
// Menú: UDV > Construir escena RA. También se puede ejecutar en batch mode:
//   Unity -batchmode -projectPath <proyecto> -executeMethod UDV.ConstruirEscenaRA.Construir -quit
// Es idempotente: si un objeto ya existe lo reutiliza y solo ajusta lo que falte.

using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Vuforia;

namespace UDV
{
    public static class ConstruirEscenaRA
    {
        const string RutaEscena = "Assets/Scenes/SampleScene.unity";
        const string DataSet = "Vuforia/UDV_SEAT_2026.xml";

        [MenuItem("UDV/Construir escena RA")]
        public static void Construir()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

            // 1. Cámara convencional apagada, AR Camera presente.
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam != null) mainCam.SetActive(false);
            if (Object.FindFirstObjectByType<VuforiaBehaviour>(FindObjectsInactive.Include) == null)
                EditorApplication.ExecuteMenuItem("GameObject/Vuforia Engine/AR Camera");

            // 2. Image targets con su objeto 3D.
            var servidor = ObtenerTarget("servidor", new Vector3(0, 0, 0));
            var sw = ObtenerTarget("switch", new Vector3(2, 0, 0));
            ObtenerHijoPrimitivo(servidor, "Cube", PrimitiveType.Cube);
            ObtenerHijoPrimitivo(sw, "Sphere", PrimitiveType.Sphere);

            // 3. Interfaz: TextMeshPro esenciales, Canvas escalable y etiqueta abajo.
            if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
                TMP_PackageResourceImporter.ImportResources(true, false, false);
            var etiqueta = ObtenerEtiqueta();

            // 4. Componentes de POO y eventos en cada target.
            Enlazar(servidor, "Servidor SRV-01", etiqueta);
            Enlazar(sw, "Switch SW-CORE-01", etiqueta);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[UDV] Escena RA construida y guardada: " + RutaEscena);
        }

        internal static GameObject ObtenerTarget(string nombre, Vector3 posicion)
        {
            var existente = Object.FindObjectsByType<ImageTargetBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(t => t.gameObject.name == nombre);
            GameObject go;
            if (existente != null)
            {
                go = existente.gameObject;
            }
            else
            {
                // El menú crea el objeto como hijo de la selección actual: se limpia antes y se desanida después.
                Selection.activeGameObject = null;
                EditorApplication.ExecuteMenuItem("GameObject/Vuforia Engine/Image Target");
                go = Selection.activeGameObject;
                go.transform.SetParent(null, true);
                go.name = nombre;
                var so = new SerializedObject(go.GetComponent<ImageTargetBehaviour>());
                so.FindProperty("mImageTargetType").intValue = 0;      // From Database
                so.FindProperty("mDataSetPath").stringValue = DataSet;
                so.FindProperty("mTrackableName").stringValue = nombre;
                so.FindProperty("mInitializedInEditor").boolValue = true;
                so.FindProperty("mWidth").floatValue = 1f;
                so.FindProperty("mHeight").floatValue = 0.666667f;
                so.FindProperty("mAspectRatio").floatValue = 0.666667f;
                so.ApplyModifiedPropertiesWithoutUndo();
                // ImageTargetPreview es interna en Vuforia 11: se localiza por nombre de tipo.
                var prev = go.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().Name == "ImageTargetPreview");
                if (prev != null)
                {
                    var sp = new SerializedObject(prev);
                    sp.FindProperty("mTargetName").stringValue = nombre;
                    sp.FindProperty("mDatasetName").stringValue = DataSet;
                    sp.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            go.transform.position = posicion;
            var handler = go.GetComponent<DefaultObserverEventHandler>() ?? go.AddComponent<DefaultObserverEventHandler>();
            handler.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked;
            return go;
        }

        internal static GameObject ObtenerHijoPrimitivo(GameObject padre, string nombre, PrimitiveType tipo)
        {
            var t = padre.transform.Find(nombre);
            GameObject go = t != null ? t.gameObject : GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            go.transform.SetParent(padre.transform, false);
            go.transform.localPosition = new Vector3(0, 0.15f, 0);
            go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            return go;
        }

        static TMP_Text ObtenerEtiqueta()
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                // Módulo de entrada según el sistema activo del proyecto.
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
            }

            var textoT = canvasGo.transform.Find("Text (TMP)");
            TextMeshProUGUI tmp;
            if (textoT != null)
            {
                tmp = textoT.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                var textoGo = new GameObject("Text (TMP)", typeof(RectTransform));
                textoGo.transform.SetParent(canvasGo.transform, false);
                tmp = textoGo.AddComponent<TextMeshProUGUI>();
            }
            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 60);
            rt.sizeDelta = new Vector2(1200, 120);
            tmp.text = "...";
            tmp.fontSize = 64;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            // Si el texto se creó antes de importar los TMP Essentials, asignar la fuente por defecto.
            if (tmp.font == null && TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
                tmp.fontSharedMaterial = tmp.font.material;
            }
            return tmp;
        }

        static void Enlazar(GameObject target, string nombreTarjeta, TMP_Text etiqueta)
        {
            var tarjeta = target.GetComponent<Tarjeta>() ?? target.AddComponent<Tarjeta>();
            var st = new SerializedObject(tarjeta);
            st.FindProperty("nombre").stringValue = nombreTarjeta;
            st.ApplyModifiedPropertiesWithoutUndo();

            var presentador = target.GetComponent<PresentadorTexto>() ?? target.AddComponent<PresentadorTexto>();
            var sp = new SerializedObject(presentador);
            sp.FindProperty("TMPEtiqueta").objectReferenceValue = etiqueta;
            sp.ApplyModifiedPropertiesWithoutUndo();

            var handler = target.GetComponent<DefaultObserverEventHandler>();
            Reemplazar(handler.OnTargetFound, presentador, presentador.Saludar);
            Reemplazar(handler.OnTargetLost, presentador, presentador.LimpiarTexto);
        }

        // Deja exactamente un listener persistente (Runtime Only) apuntando al método indicado.
        internal static void Reemplazar(UnityEvent evento, Object objetivo, UnityAction accion)
        {
            for (int i = evento.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(evento, i);
            UnityEventTools.AddPersistentListener(evento, accion);
            evento.SetPersistentListenerState(0, UnityEventCallState.RuntimeOnly);
        }
    }
}
