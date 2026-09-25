# TecnoExplorador AR

Proyecto del curso **Seminario Estado Actual de la Tecnología** (LIS-SEAT), Universidad Da Vinci de Guatemala.
Estudiante: Gustavo Adolfo Pérez Canú, carné 202104862.

Juego de realidad aumentada para que niños de 7 a 10 años aprendan a reconocer los dispositivos tecnológicos de su casa y escuela. La aplicación muestra una pregunta aleatoria y el niño responde enfocando con la cámara la tarjeta (marcador) del dispositivo correcto.

## Herramientas

- Unity 6.3 LTS (6000.3.23f1), plantilla 3D Built-in.
- Vuforia Engine 11.4.4 (image targets, licencia Basic de desarrollo).
- TextMeshPro para la interfaz.

## Cómo abrir el proyecto

1. Clonar el repositorio y abrirlo con Unity Hub (Unity 6000.3.x).
2. Descargar `com.ptc.vuforia.engine-11.4.4.tgz` desde https://developer.vuforia.com/downloads/sdk y copiarlo en `Packages/` (no se versiona por su tamaño).
3. Abrir `Window > Vuforia Configuration` y pegar la propia license key (el archivo `VuforiaConfiguration.asset` no se versiona porque contiene la llave).
4. Abrir `Assets/Scenes/SampleScene.unity` y presionar Play con la webcam.

## Historia

- Laboratorio de RA (tarea 03): escena base con AR Camera, dos image targets, cubo y esfera, clases `Tarjeta` y `PresentadorTexto`.
- Evaluación parcial 1: capa de juego con banco de preguntas, selección aleatoria, validación y retroalimentación.
