INFORME TÉCNICO DE INTEGRACIÓN
Sistema de Proctoring
Módulo Frontend — Flutter Web
v1.1 — Despliegue Web


Dirigido a
Equipo de Desarrollo Frontend (Flutter Web)
Emitido por
Equipo Backend — Python
Versión
v1.1
Plataforma
Flutter Web — despliegue en navegador
Estado
En desarrollo — entorno local disponible


1. Contexto y responsabilidades
   La plataforma de pruebas técnicas corre en el navegador (Flutter Web). Durante la sesión, un módulo de monitoreo pasivo opera en segundo plano capturando frames de la cámara frontal y enviándolos al backend Python para su análisis mediante visión computacional (YOLO + MediaPipe).

El candidato es informado previamente de que será monitoreado, pero no verá ninguna interfaz relacionada con el monitoreo durante la prueba. Todo el procesamiento ocurre en el servidor.

ℹ  El equipo frontend NO procesa video ni corre detección. Su única responsabilidad es capturar frames desde la cámara del navegador y enviarlos al backend mediante HTTP.


⚠  IMPORTANTE: Como la plataforma corre en Flutter Web, el paquete camera de Flutter NO funciona. Se debe usar dart:html con getUserMedia — nativo del navegador. Ver sección 3.


FRONTEND — Flutter Web
BACKEND — Python (FastAPI)
Solicitar acceso a la cámara via getUserMedia
Recibir y decodificar el frame JPEG
Capturar frame cada 500ms con Canvas
Correr detección (YOLO + MediaPipe)
Codificar frame a base64 y enviar al backend
Acumular eventos de la sesión
Llamar al endpoint de cierre al terminar
Generar y guardar reporte en base de datos


2. Endpoints de la API
   La API corre sobre FastAPI (Python). Todos los endpoints reciben y retornan JSON. Deben consumirse en este orden estricto:

2.1  Iniciar sesión
Método
POST
URL
/api/session/start
Cuándo llamar
Cuando el candidato presiona el botón Comenzar prueba


Body (JSON):
{
"usuario_id": "string   // ID único del candidato en la plataforma"
}


Respuesta exitosa (200):
{
"session_id": "550e8400-e29b-41d4-a716-446655440000"
}


⚠  Guardar el session_id en memoria. Es obligatorio en cada llamada posterior. Sin él, el backend rechaza los frames.


2.2  Enviar frame
Método
POST
URL
/api/frame
Cuándo llamar
Cada 500ms mientras la prueba esté activa
Formato
JPEG codificado en Base64


Body (JSON):
{
"session_id": "550e8400-e29b-41d4-a716-446655440000",
"frame_b64":  "string   // Imagen JPEG codificada en Base64"
}


Respuesta (200) — solo para monitoreo interno, no mostrar al candidato:
{
"mirando":         true,
"hay_intruso":     false,
"hay_dispositivo": false,
"eventos":         []
}


2.3  Terminar sesión
Método
POST
URL
/api/session/end
Cuándo llamar
Al presionar Terminar prueba o al expirar el tiempo


Body (JSON):
{
"session_id": "550e8400-e29b-41d4-a716-446655440000"
}


Respuesta (200):
{
"ok": true,
"reporte": {
"session_id":              "550e8400-...",
"usuario_id":              "user-123",
"inicio":                  "2024-06-01T10:00:00Z",
"fin":                     "2024-06-01T11:00:00Z",
"total_frames_procesados": 7200,
"eventos": [
{ "tipo": "dispositivo_prohibido", "detalle": "cell phone", "timestamp": "..." },
{ "tipo": "distraccion",           "detalle": "+10s fuera", "timestamp": "..." }
]
}
}


✔  El reporte ya fue guardado en base de datos por el backend. El frontend puede ignorarlo o pasarlo al administrador según el flujo de la plataforma.


3. Implementación en Flutter Web
   3.1  Por qué NO usar el paquete camera
   El paquete camera de Flutter usa APIs nativas del SO (Android/iOS). En Flutter Web esas APIs no existen. El navegador expone su propio sistema de acceso a la cámara a través de la API getUserMedia de JavaScript, accesible desde Dart mediante dart:html.



camera (paquete Flutter)
dart:html getUserMedia
Flutter Mobile
✔ Funciona
✘ No aplica
Flutter Web
✘ No funciona
✔ Solución correcta


3.2  Dependencias requeridas
Agregar al archivo pubspec.yaml. No se necesita ningún paquete especial para la cámara en web — dart:html viene incluido en Flutter Web por defecto:

dependencies:
http: ^1.0.0   # Llamadas HTTP a la API

# dart:html NO se agrega en pubspec.yaml — viene incluido en Flutter Web


3.3  Servicio de proctoring para Flutter Web
Crear el archivo proctoring_service.dart. Todo ocurre en memoria — ningún elemento de video o canvas se inserta al DOM, por lo que el candidato no ve nada relacionado con el monitoreo:

import 'dart:async';
import 'dart:convert';
import 'dart:html' as html;
import 'package:http/http.dart' as http;

class ProctoringService {
final String apiBase;
final String usuarioId;

html.VideoElement?  _video;
html.CanvasElement? _canvas;
String?             _sessionId;
Timer?              _timer;

ProctoringService({required this.apiBase, required this.usuarioId});

// ── Llamar al presionar 'Comenzar prueba' ──────────────────────
Future<void> iniciar() async {
// 1. Pedir acceso a la cámara frontal via navegador
final stream = await html.window.navigator.mediaDevices!
.getUserMedia({'video': {'facingMode': 'user'}, 'audio': false});

    // 2. VideoElement en memoria — NUNCA se inserta al DOM
    _video = html.VideoElement()
      ..srcObject = stream
      ..autoplay  = true;
 
    // 3. Canvas en memoria para capturar frames
    _canvas = html.CanvasElement(width: 640, height: 480);
 
    // 4. Crear sesión en el backend
    final res = await http.post(
      Uri.parse('$apiBase/api/session/start'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'usuario_id': usuarioId}),
    );
    _sessionId = jsonDecode(res.body)['session_id'];
 
    // 5. Esperar a que el video esté listo
    await _video!.onCanPlay.first;
 
    // 6. Enviar frames cada 500ms — silencioso
    _timer = Timer.periodic(
      const Duration(milliseconds: 500),
      (_) => _enviarFrame(),
    );
}

Future<void> _enviarFrame() async {
if (_video == null || _canvas == null || _sessionId == null) return;
try {
// Dibujar frame actual del video en el canvas
_canvas!.context2D.drawImageScaled(_video!, 0, 0, 640, 480);

      // Obtener base64 JPEG (calidad 0.7 para reducir tamaño)
      final dataUrl = _canvas!.toDataUrl('image/jpeg', 0.7);
      final b64     = dataUrl.split(',')[1];
 
      await http.post(
        Uri.parse('$apiBase/api/frame'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'session_id': _sessionId, 'frame_b64': b64}),
      );
      // Silencioso: no mostramos nada al candidato
    } catch (_) { /* silencioso ante errores de red */ }
}

// ── Llamar al presionar 'Terminar prueba' ──────────────────────
Future<Map<String, dynamic>> terminar() async {
_timer?.cancel();

    // Detener todas las pistas de la cámara
    _video?.srcObject?.getTracks().forEach((t) => t.stop());
    _video  = null;
    _canvas = null;
 
    final res = await http.post(
      Uri.parse('$apiBase/api/session/end'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'session_id': _sessionId}),
    );
    return jsonDecode(res.body)['reporte'];
}
}


3.4  Uso desde cualquier pantalla de la prueba
final proctoring = ProctoringService(
apiBase:   'https://api.tudominio.com',  // Ver sección 4
usuarioId: currentUser.id,
);

// Al presionar 'Comenzar prueba':
await proctoring.iniciar();

// Al presionar 'Terminar prueba' o al expirar el tiempo:
final reporte = await proctoring.terminar();
// reporte es un Map<String, dynamic> con el resumen completo de la sesión


4. CORS — Consideración crítica para Web
   Dado que Flutter Web corre en el navegador, todas las peticiones HTTP a la API van a activar la política CORS del navegador. El backend ya está configurado para esto, pero el equipo frontend debe tener en cuenta lo siguiente:

Entorno
Configuración CORS
Entorno local
El backend debe correr con CORS abierto (allow_origins=["*"])
Producción
El backend restringirá CORS al dominio real de la plataforma
Qué significa
Si el dominio de despliegue cambia, avisar al equipo backend para actualizar la configuración


⚠  Si al hacer peticiones desde el navegador aparece un error 'CORS policy' o 'Access-Control-Allow-Origin', no es un problema del frontend — se debe notificar al equipo backend para que actualice la configuración del servidor.


5. URLs de la API
   Entorno
   URL base
   Local (desarrollo)
   http://localhost:8000
   Red local (dispositivos)
   http://192.168.X.X:8000  (reemplazar con la IP del servidor)
   Producción
   Por definir — se comunicará cuando el deploy esté listo


ℹ  En local, si Flutter Web y el backend corren en la misma máquina, usar http://localhost:8000. Si corren en máquinas distintas de la misma red, usar la IP local del servidor.


6. Reglas de comportamiento esperado
   La cámara se activa en segundo plano. No mostrar ningún preview, indicador de grabación ni alerta relacionada con la cámara en la UI del candidato.
   El monitoreo inicia únicamente cuando el candidato confirma que comenzará la prueba — nunca antes.
   El navegador mostrará automáticamente un indicador de cámara activa (punto rojo o ícono en la pestaña). Esto es forzado por el SO y el navegador, no se puede suprimir y no debe preocupar — el candidato fue informado previamente.
   Si el envío de un frame falla (timeout, error de red), se omite ese frame y se continúa. No interrumpir la prueba por errores de envío.
   Si /api/session/start falla, no permitir que la prueba comience. Mostrar un mensaje de error al candidato.
   Al terminar la prueba (botón o tiempo agotado), siempre llamar a /api/session/end antes de navegar a otra pantalla. Si no se llama, el reporte no se guarda en la base de datos.
   La respuesta de /api/frame es para uso interno únicamente. No exponer su contenido al candidato bajo ninguna circunstancia.

7. Contacto y coordinación
   Cualquier duda sobre el comportamiento de la API, cambios en endpoints, errores CORS o gestión de errores debe coordinarse directamente con el equipo backend antes de implementar soluciones alternativas en el cliente.
