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

        [MenuItem("UDV/Capturas/Deseleccionar")]
        public static void Deseleccionar() { Selection.activeObject = null; }

        static void Seleccionar(string nombre)
        {
            var go = GameObject.Find(nombre);
            if (go == null) { Debug.LogWarning("[UDV] No existe " + nombre); return; }
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }
    }
}
