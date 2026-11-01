// Menús de apoyo para tomar las capturas del informe sin tocar la escena.
// Seleccionan un objeto de la escena para que el Inspector lo muestre.

using UnityEditor;
using UnityEngine;

namespace UDV
{
    public static class CapturasUDV
    {
        [MenuItem("UDV/Capturas/Seleccionar Juego")]
        public static void SeleccionarJuego() { Seleccionar("Juego"); }

        [MenuItem("UDV/Capturas/Seleccionar teclado")]
        public static void SeleccionarTeclado() { Seleccionar("teclado"); }

        [MenuItem("UDV/Capturas/Seleccionar computadora")]
        public static void SeleccionarComputadora() { Seleccionar("computadora"); }

        [MenuItem("UDV/Capturas/Seleccionar Canvas")]
        public static void SeleccionarCanvas() { Seleccionar("Canvas"); }

        [MenuItem("UDV/Capturas/Seleccionar ARCamera")]
        public static void SeleccionarARCamera() { Seleccionar("ARCamera"); }

        [MenuItem("UDV/Capturas/Mostrar Vuforia Configuration")]
        public static void MostrarVuforiaConfiguration()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/VuforiaConfiguration.asset");
            if (cfg == null) { Debug.LogWarning("[UDV] No existe VuforiaConfiguration.asset"); return; }
            Selection.activeObject = cfg;
        }

        // Parcial 2: alta fidelidad
        [MenuItem("UDV/Capturas/P2 Pivote del teclado (AnimacionModelo)")]
        public static void SeleccionarPivote() { SeleccionarRuta("teclado/Pivote"); }

        [MenuItem("UDV/Capturas/P2 Modelo del teclado (Animator)")]
        public static void SeleccionarModelo() { SeleccionarRuta("teclado/Pivote/Modelo"); }

        [MenuItem("UDV/Capturas/P2 Audio (GestorAudio)")]
        public static void SeleccionarAudio() { Seleccionar("Audio"); }

        [MenuItem("UDV/Capturas/P2 Boton Jugar (On Click)")]
        public static void SeleccionarBotonJugar() { SeleccionarRuta("Canvas/PanelMenu/BotonJugar"); }

        [MenuItem("UDV/Capturas/P2 Clip Flotar")]
        public static void SeleccionarClip() { SeleccionarAsset("Assets/Animaciones/Flotar.anim"); }

        [MenuItem("UDV/Capturas/P2 Animator Controller")]
        public static void SeleccionarControlador() { SeleccionarAsset("Assets/Animaciones/ModeloRA.controller"); }

        [MenuItem("UDV/Capturas/Deseleccionar")]
        public static void Deseleccionar() { Selection.activeObject = null; }

        // Ruta "padre/hijo/nieto" desde un objeto raíz de la escena (encuentra también objetos inactivos).
        static void SeleccionarRuta(string ruta)
        {
            var partes = ruta.Split(new[] { '/' }, 2);
            var raiz = GameObject.Find(partes[0]);
            var t = raiz == null ? null : (partes.Length > 1 ? raiz.transform.Find(partes[1]) : raiz.transform);
            if (t == null) { Debug.LogWarning("[UDV] No existe " + ruta); return; }
            Selection.activeGameObject = t.gameObject;
            EditorGUIUtility.PingObject(t.gameObject);
        }

        static void SeleccionarAsset(string ruta)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(ruta);
            if (asset == null) { Debug.LogWarning("[UDV] No existe " + ruta); return; }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        static void Seleccionar(string nombre)
        {
            var go = GameObject.Find(nombre);
            if (go == null) { Debug.LogWarning("[UDV] No existe " + nombre); return; }
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }
    }
}
