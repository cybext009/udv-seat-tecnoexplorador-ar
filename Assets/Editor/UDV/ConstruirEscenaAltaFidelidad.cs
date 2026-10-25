// Script de editor que convierte la escena del parcial 1 en la versión de alta fidelidad (Evaluación parcial 2):
// modelos 3D con animación, interfaz con paneles Menú / Juego / Victoria, audio y ciclo de racha.
// Menú: UDV > Construir escena Alta Fidelidad. Batch mode:
//   Unity -batchmode -projectPath <proyecto> -executeMethod UDV.ConstruirEscenaAltaFidelidad.Construir -quit
// Idempotente: borra y vuelve a crear lo que arma, así se puede correr cada vez que cambian los assets.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Vuforia;
using Image = UnityEngine.UI.Image;   // Vuforia también define una clase Image

namespace UDV
{
    public static class ConstruirEscenaAltaFidelidad
    {
        const string RutaEscena = "Assets/Scenes/SampleScene.unity";
        const string RutaClipFlotar = "Assets/Animaciones/Flotar.anim";
        const string RutaControlador = "Assets/Animaciones/ModeloRA.controller";
        const string RutaEstrella = "Assets/UI/estrella.png";
        const string RutaRedondeado = "Assets/UI/redondeado.png";

        // Ancho de los image targets en la escena (mWidth = 1).
        const float Tam = 1f;
        // Lado mayor del modelo respecto al ancho de la tarjeta.
        const float FraccionModelo = 0.7f;

        // Modelo 3D de la Asset Store por marcador: nombre del prefab del paquete "Computer devices" (Maximalist).
        // Se busca por nombre porque la carpeta del paquete lleva una "С" cirílica ("Сomputer devices").
        // Nombre vacío o prefab inexistente = se usa una primitiva de respaldo (como en el parcial 1).
        const string CarpetaModelos = "Assets/JustPlay";
        static readonly Dictionary<string, string> Modelos = new Dictionary<string, string>
        {
            { "computadora", "Monitor" },
            { "teclado", "Keyboard" },
            { "raton", "Mouse" },
            { "wifi", "Router" },
        };

        // Rotación local de cada modelo para que mire hacia el jugador (ajustada con la vista previa de los modelos).
        static readonly Dictionary<string, Vector3> Rotaciones = new Dictionary<string, Vector3>
        {
            { "computadora", new Vector3(0f, 180f, 0f) },   // el FBX viene mirando hacia atrás
            { "teclado", Vector3.zero },
            { "raton", Vector3.zero },
            { "wifi", new Vector3(0f, 180f, 0f) },          // igual: sin girar se ven los puertos
        };

        // Audio (Asset Store: "Free Casual Music Pack" y "FREE Casual Game SFX Pack"). Ruta vacía = sin ese sonido.
        const string RutaMusica = "Assets/FREE CASUAL MUSIC PACK/CASUAL THEME 2/CASUAL THEME 2 - LOOP 1.wav";
        const string RutaInicio = "Assets/FREE CASUAL MUSIC PACK/BONUS 2 - SFX/POWERUP 1.wav";
        const string RutaCorrecto = "Assets/FREE CASUAL MUSIC PACK/BONUS 2 - SFX/SPECIAL COLECT 1.wav";
        const string RutaIncorrecto = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-10.wav";
        const string RutaVictoria = "Assets/FREE CASUAL MUSIC PACK/BONUS 2 - SFX/FIREWORKS 3.wav";

        // Paleta: azul y dorado UDV, verde y rojo para la retroalimentación.
        static readonly Color Azul = new Color32(0x00, 0x19, 0x46, 0xFF);
        static readonly Color Dorado = new Color32(0xE8, 0xB9, 0x2E, 0xFF);
        static readonly Color Verde = new Color32(0x2E, 0x9E, 0x4F, 0xFF);
        static readonly Color Rojo = new Color32(0xC0, 0x39, 0x2B, 0xFF);
        static readonly Color Crema = new Color32(0xFF, 0xF3, 0xC4, 0xF2);
        static readonly Color BlancoSuave = new Color(1f, 1f, 1f, 0.92f);

        static Sprite spriteRedondeado;
        static Sprite spriteEstrella;

        [MenuItem("UDV/Construir escena Alta Fidelidad")]
        public static void Construir()
        {
            var escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            PrepararSprites();
            var controlador = CrearAnimacionFlotar();

            var juego = GameObject.Find("Juego");
            var gestor = juego.GetComponent<GestorPreguntas>();
            var sg = new SerializedObject(gestor);
            sg.FindProperty("meta").intValue = 3;
            sg.FindProperty("segundosEntrePreguntas").floatValue = 2.5f;
            sg.ApplyModifiedPropertiesWithoutUndo();

            // 1. Modelos 3D animados sobre cada marcador.
            foreach (var clave in Modelos.Keys)
                ConstruirModelo(GameObject.Find(clave), clave, controlador, gestor);

            // 2. Interfaz.
            ConstruirInterfaz(gestor);

            // 3. Audio.
            ConstruirAudio(gestor);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[UDV] Escena de alta fidelidad construida y guardada: " + RutaEscena);
        }

        // ------------------------------------------------------------------ Modelos
        // Jerarquía por marcador:  ImageTarget > Pivote (AnimacionModelo) > Modelo (Animator Flotar) > Malla
        static void ConstruirModelo(GameObject target, string clave, AnimatorController controlador, GestorPreguntas gestor)
        {
            Borrar(target.transform, "Pivote");
            Borrar(target.transform, "Figura");

            var pivote = new GameObject("Pivote");
            pivote.transform.SetParent(target.transform, false);
            var animacion = pivote.AddComponent<AnimacionModelo>();
            var sa = new SerializedObject(animacion);
            sa.FindProperty("alturaSalto").floatValue = 0.25f * Tam;
            sa.FindProperty("amplitudSacudida").floatValue = 0.12f * Tam;
            sa.ApplyModifiedPropertiesWithoutUndo();

            var modelo = new GameObject("Modelo");
            modelo.transform.SetParent(pivote.transform, false);
            var animator = modelo.AddComponent<Animator>();
            animator.runtimeAnimatorController = controlador;

            var malla = InstanciarMalla(clave);
            malla.name = "Malla";
            malla.transform.SetParent(modelo.transform, false);
            malla.transform.localRotation = Quaternion.Euler(Rotaciones[clave]);
            AjustarATarjeta(malla);

            var respuesta = target.GetComponent<RespuestaRA>();
            var sr = new SerializedObject(respuesta);
            sr.FindProperty("gestor").objectReferenceValue = gestor;
            sr.FindProperty("animacion").objectReferenceValue = animacion;
            sr.ApplyModifiedPropertiesWithoutUndo();

            var handler = target.GetComponent<DefaultObserverEventHandler>();
            ConstruirEscenaRA.Reemplazar(handler.OnTargetFound, respuesta, respuesta.AlEncontrar);
            ConstruirEscenaRA.Reemplazar(handler.OnTargetLost, respuesta, respuesta.AlPerder);
        }

        static GameObject InstanciarMalla(string clave)
        {
            var asset = BuscarPrefab(Modelos[clave]);
            if (asset != null)
                return (GameObject)PrefabUtility.InstantiatePrefab(asset);

            Debug.LogWarning("[UDV] Sin modelo 3D para '" + clave + "', se usa una primitiva de respaldo");
            switch (clave)
            {
                case "teclado":
                    var t = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    t.transform.localScale = new Vector3(1f, 0.12f, 0.4f);
                    return Envolver(t);
                case "raton": return GameObject.CreatePrimitive(PrimitiveType.Sphere);
                case "wifi":
                    var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    w.transform.localScale = new Vector3(1f, 0.25f, 1f);
                    return Envolver(w);
                default: return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
        }

        static GameObject BuscarPrefab(string nombre)
        {
            if (string.IsNullOrEmpty(nombre) || !AssetDatabase.IsValidFolder(CarpetaModelos)) return null;
            foreach (var guid in AssetDatabase.FindAssets(nombre + " t:Prefab", new[] { CarpetaModelos }))
            {
                var ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(ruta) == nombre)
                    return AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            }
            return null;
        }

        // Mete la primitiva en un objeto vacío para que su escala propia no se pierda al ajustar.
        static GameObject Envolver(GameObject hijo)
        {
            var raiz = new GameObject();
            hijo.transform.SetParent(raiz.transform, false);
            return raiz;
        }

        // Escala el modelo para que su lado mayor mida FraccionModelo de la tarjeta y lo apoya sobre ella, centrado.
        static void AjustarATarjeta(GameObject malla)
        {
            var renderers = malla.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            float lado = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (lado <= 0f) return;
            float factor = FraccionModelo * Tam / lado;
            malla.transform.localScale *= factor;

            b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            var padre = malla.transform.parent;
            var centroBase = padre.InverseTransformPoint(new Vector3(b.center.x, b.min.y, b.center.z));
            malla.transform.localPosition -= centroBase;
        }

        static AnimatorController CrearAnimacionFlotar()
        {
            System.IO.Directory.CreateDirectory("Assets/Animaciones");
            var clip = new AnimationClip { name = "Flotar" };
            // 8 s de clip: sube y baja 4 veces (cada 2 s) y da una vuelta completa.
            float altura = 0.08f * Tam;
            var subir = new AnimationCurve();
            for (int i = 0; i <= 8; i++)
                subir.AddKey(new Keyframe(i, i % 2 == 0 ? 0f : altura, 0f, 0f));
            clip.SetCurve("", typeof(Transform), "localPosition.y", subir);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, 8f, 360f));
            var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
            ajustes.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, ajustes);

            AssetDatabase.DeleteAsset(RutaClipFlotar);
            AssetDatabase.DeleteAsset(RutaControlador);
            AssetDatabase.CreateAsset(clip, RutaClipFlotar);
            return AnimatorController.CreateAnimatorControllerAtPathWithClip(RutaControlador, clip);
        }

        // ------------------------------------------------------------------ Interfaz
        static void ConstruirInterfaz(GestorPreguntas gestor)
        {
            var canvas = GameObject.Find("Canvas");
            foreach (var viejo in new[] { "Pregunta", "Mensaje", "Puntaje", "PanelJuego", "PanelVictoria", "PanelMenu" })
                Borrar(canvas.transform, viejo);

            var escalador = canvas.GetComponent<CanvasScaler>() ?? canvas.AddComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920, 1080);
            escalador.matchWidthOrHeight = 0.5f;

            // Pantalla de juego
            var panelJuego = Panel(canvas.transform, "PanelJuego", new Color(0, 0, 0, 0));
            var barra = Caja(panelJuego, "BarraPregunta", Crema, new Vector2(0.5f, 1f), new Vector2(-20, -30), new Vector2(1060, 150));
            var textoPregunta = Texto(barra, "TextoPregunta", "Pregunta", 52, Azul, FontStyles.Bold, Estirar());
            var botonMenuJuego = Boton(panelJuego, "BotonMenu", "Menú", Color.white, Azul, new Vector2(0f, 1f), new Vector2(30, -30), new Vector2(200, 110), 40);
            var cajaRacha = Caja(panelJuego, "PanelRacha", BlancoSuave, new Vector2(1f, 1f), new Vector2(-30, -30), new Vector2(380, 180));
            var textoRacha = Texto(cajaRacha, "TextoRacha", "Racha 0 / 3", 44, Azul, FontStyles.Bold, Franja(0.55f, 1f));
            var estrellas = new Image[3];
            for (int i = 0; i < 3; i++)
                estrellas[i] = Icono(cajaRacha, "Estrella" + (i + 1), spriteEstrella, new Vector2(0.5f, 0f), new Vector2((i - 1) * 100, 20), new Vector2(84, 84));
            var cajaMensaje = Caja(panelJuego, "PanelMensaje", BlancoSuave, new Vector2(0f, 0f), new Vector2(30, 30), new Vector2(1280, 130));
            var textoMensaje = Texto(cajaMensaje, "TextoMensaje", "", 46, Azul, FontStyles.Bold, Estirar());
            var cajaMarcador = Caja(panelJuego, "PanelMarcador", BlancoSuave, new Vector2(1f, 0f), new Vector2(-30, 30), new Vector2(540, 130));
            var textoMarcador = Texto(cajaMarcador, "TextoMarcador", "Marcador: ninguno", 40, Azul, FontStyles.Normal, Estirar());

            // Victoria (encima del juego)
            var panelVictoria = Panel(canvas.transform, "PanelVictoria", new Color(Azul.r, Azul.g, Azul.b, 0.6f));
            var tarjeta = Caja(panelVictoria, "TarjetaVictoria", new Color32(0xFF, 0xFD, 0xF2, 0xFF), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1000, 660));
            Texto(tarjeta, "TextoGanaste", "¡GANASTE!", 120, Azul, FontStyles.Bold, Franja(0.74f, 0.97f));
            for (int i = 0; i < 3; i++)
            {
                var e = Icono(tarjeta, "Estrella" + (i + 1), spriteEstrella, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 150, 110), new Vector2(130, 130));
                e.color = Dorado;
            }
            var textoResumen = Texto(tarjeta, "TextoResumen", "", 44, Azul, FontStyles.Normal, Franja(0.25f, 0.5f));
            var botonOtraVez = Boton(tarjeta, "BotonJugarDeNuevo", "Jugar de nuevo", Verde, Color.white, new Vector2(0.5f, 0f), new Vector2(-200, 50), new Vector2(440, 120), 46);
            var botonMenuVictoria = Boton(tarjeta, "BotonMenu", "Menú", new Color32(0xDD, 0xE1, 0xE8, 0xFF), Azul, new Vector2(0.5f, 0f), new Vector2(230, 50), new Vector2(320, 120), 46);

            // Menú principal (pantalla completa, encima de todo)
            var panelMenu = Panel(canvas.transform, "PanelMenu", new Color(Azul.r, Azul.g, Azul.b, 0.9f));
            Texto(panelMenu.transform, "TextoTitulo", "TecnoExplorador AR", 120, Color.white, FontStyles.Bold, Franja(0.70f, 0.88f));
            Texto(panelMenu.transform, "TextoLema", "¡Descubre los aparatos de tu casa jugando!", 50, Dorado, FontStyles.Normal, Franja(0.62f, 0.70f));
            var botonJugar = Boton(panelMenu.transform, "BotonJugar", "JUGAR", Verde, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(560, 140), 70);
            var botonSalir = Boton(panelMenu.transform, "BotonSalir", "SALIR", Rojo, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(420, 110), 52);
            Texto(panelMenu.transform, "TextoAutor", "Autor: Gustavo Adolfo Pérez Canú · Carné 202104862", 40, new Color(0.85f, 0.89f, 0.96f), FontStyles.Normal, Franja(0.06f, 0.12f));
            Texto(panelMenu.transform, "TextoCurso", "Universidad Da Vinci de Guatemala · Seminario Estado Actual de la Tecnología", 30, new Color(0.62f, 0.69f, 0.81f), FontStyles.Normal, Franja(0.02f, 0.06f));

            // Botones -> métodos del juego (listeners persistentes: se ven en el inspector, On Click ()).
            Conectar(botonJugar, gestor.IniciarJuego);
            Conectar(botonSalir, gestor.Salir);
            Conectar(botonMenuJuego, gestor.VolverAlMenu);
            Conectar(botonOtraVez, gestor.IniciarJuego);
            Conectar(botonMenuVictoria, gestor.VolverAlMenu);

            // Gestor de la interfaz
            var gi = canvas.GetComponent<GestorInterfaz>() ?? canvas.AddComponent<GestorInterfaz>();
            var s = new SerializedObject(gi);
            s.FindProperty("juego").objectReferenceValue = gestor;
            s.FindProperty("panelMenu").objectReferenceValue = panelMenu;
            s.FindProperty("panelJuego").objectReferenceValue = panelJuego;
            s.FindProperty("panelVictoria").objectReferenceValue = panelVictoria;
            s.FindProperty("textoPregunta").objectReferenceValue = textoPregunta;
            s.FindProperty("textoRacha").objectReferenceValue = textoRacha;
            s.FindProperty("fondoMensaje").objectReferenceValue = cajaMensaje.GetComponent<Image>();
            s.FindProperty("textoMensaje").objectReferenceValue = textoMensaje;
            s.FindProperty("textoMarcador").objectReferenceValue = textoMarcador;
            s.FindProperty("textoResumen").objectReferenceValue = textoResumen;
            var arr = s.FindProperty("estrellas");
            arr.arraySize = estrellas.Length;
            for (int i = 0; i < estrellas.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = estrellas[i];
            s.ApplyModifiedPropertiesWithoutUndo();

            // Todo lo creado va en la capa UI, como el Canvas.
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = canvas.layer;

            // Estado inicial en el editor: se ve el menú (en ejecución lo decide GestorInterfaz).
            panelJuego.SetActive(false);
            panelVictoria.SetActive(false);
            panelMenu.SetActive(true);
        }

        static void Conectar(Button boton, UnityAction accion)
        {
            UnityEventTools.AddPersistentListener(boton.onClick, accion);
        }

        static GameObject Panel(Transform padre, string nombre, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = color.a > 0.01f;   // los paneles opacos bloquean clics hacia atrás
            return go;
        }

        static Transform Caja(GameObject padre, string nombre, Color color, Vector2 ancla, Vector2 posicion, Vector2 tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre.transform, false);
            Colocar((RectTransform)go.transform, ancla, posicion, tamano);
            var img = go.GetComponent<Image>();
            img.sprite = spriteRedondeado;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            return go.transform;
        }

        static Transform Caja(Transform padre, string nombre, Color color, Vector2 ancla, Vector2 posicion, Vector2 tamano)
        {
            return Caja(padre.gameObject, nombre, color, ancla, posicion, tamano);
        }

        static Image Icono(Transform padre, string nombre, Sprite sprite, Vector2 ancla, Vector2 posicion, Vector2 tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            Colocar((RectTransform)go.transform, ancla, posicion, tamano);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = new Color(0.79f, 0.81f, 0.85f);
            return img;
        }

        static Button Boton(Transform padre, string nombre, string etiqueta, Color fondo, Color letra, Vector2 ancla, Vector2 posicion, Vector2 tamano, float fuente)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
            Colocar((RectTransform)go.transform, ancla, posicion, tamano);
            var img = go.GetComponent<Image>();
            img.sprite = spriteRedondeado;
            img.type = Image.Type.Sliced;
            img.color = fondo;
            var boton = go.GetComponent<Button>();
            var colores = boton.colors;
            colores.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colores.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            boton.colors = colores;
            Texto(go.transform, "Texto", etiqueta, fuente, letra, FontStyles.Bold, Estirar());
            return boton;
        }

        static Button Boton(GameObject padre, string nombre, string etiqueta, Color fondo, Color letra, Vector2 ancla, Vector2 posicion, Vector2 tamano, float fuente)
        {
            return Boton(padre.transform, nombre, etiqueta, fondo, letra, ancla, posicion, tamano, fuente);
        }

        // Rectángulo relativo al padre: (anclaMin, anclaMax) en fracciones.
        struct Area { public Vector2 min, max; }
        static Area Estirar() { return new Area { min = Vector2.zero, max = Vector2.one }; }
        static Area Franja(float y0, float y1) { return new Area { min = new Vector2(0f, y0), max = new Vector2(1f, y1) }; }

        static TextMeshProUGUI Texto(Transform padre, string nombre, string texto, float fuente, Color color, FontStyles estilo, Area area)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = area.min;
            rt.anchorMax = area.max;
            rt.offsetMin = new Vector2(24, 8);
            rt.offsetMax = new Vector2(-24, -8);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
                tmp.fontSharedMaterial = tmp.font.material;
            }
            tmp.text = texto;
            tmp.fontSize = fuente;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = fuente * 0.5f;
            tmp.fontSizeMax = fuente;
            tmp.fontStyle = estilo;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        static TextMeshProUGUI Texto(GameObject padre, string nombre, string texto, float fuente, Color color, FontStyles estilo, Area area)
        {
            return Texto(padre.transform, nombre, texto, fuente, color, estilo, area);
        }

        static void Colocar(RectTransform rt, Vector2 ancla, Vector2 posicion, Vector2 tamano)
        {
            rt.anchorMin = ancla;
            rt.anchorMax = ancla;
            rt.pivot = ancla;
            rt.anchoredPosition = posicion;
            rt.sizeDelta = tamano;
        }

        static void PrepararSprites()
        {
            spriteEstrella = ImportarSprite(RutaEstrella, Vector4.zero);
            spriteRedondeado = ImportarSprite(RutaRedondeado, new Vector4(34, 34, 34, 34));
        }

        static Sprite ImportarSprite(string ruta, Vector4 borde)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
            if (imp.textureType != TextureImporterType.Sprite || imp.spriteBorder != borde)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spriteBorder = borde;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }

        // ------------------------------------------------------------------ Audio
        static void ConstruirAudio(GestorPreguntas gestor)
        {
            var viejo = GameObject.Find("Audio");
            if (viejo != null) Object.DestroyImmediate(viejo);

            var go = new GameObject("Audio");
            var musica = go.AddComponent<AudioSource>();
            musica.playOnAwake = false;
            musica.loop = true;
            var efectos = go.AddComponent<AudioSource>();
            efectos.playOnAwake = false;

            var ga = go.AddComponent<GestorAudio>();
            var s = new SerializedObject(ga);
            s.FindProperty("juego").objectReferenceValue = gestor;
            s.FindProperty("fuenteMusica").objectReferenceValue = musica;
            s.FindProperty("fuenteEfectos").objectReferenceValue = efectos;
            s.FindProperty("musica").objectReferenceValue = Clip(RutaMusica);
            s.FindProperty("sonidoInicio").objectReferenceValue = Clip(RutaInicio);
            s.FindProperty("sonidoCorrecto").objectReferenceValue = Clip(RutaCorrecto);
            s.FindProperty("sonidoIncorrecto").objectReferenceValue = Clip(RutaIncorrecto);
            s.FindProperty("sonidoVictoria").objectReferenceValue = Clip(RutaVictoria);
            s.ApplyModifiedPropertiesWithoutUndo();
        }

        static AudioClip Clip(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return null;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
            if (clip == null) Debug.LogWarning("[UDV] No se encontró el audio " + ruta);
            return clip;
        }

        // ------------------------------------------------------------------ Utilidades
        static void Borrar(Transform padre, string nombre)
        {
            var t = padre.Find(nombre);
            while (t != null)
            {
                Object.DestroyImmediate(t.gameObject);
                t = padre.Find(nombre);
            }
        }
    }
}
