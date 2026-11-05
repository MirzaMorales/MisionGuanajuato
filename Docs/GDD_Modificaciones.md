# Actualización del GDD — Modificaciones a las mecánicas y su justificación

Este documento complementa el GDD original de *Misión Guanajuato: Rescate en 2D*. Describe las mecánicas de la Unidad 1 tal como quedaron implementadas y justifica por escrito cada cambio respecto al diseño original. Debe incorporarse al GDD (Google Doc).

## 1. Mecánicas de la Unidad 1 implementadas
| Mecánica (GDD) | Implementación |
|---|---|
| Caminar y correr por el escenario | `PlayerController`: movimiento horizontal con Rigidbody2D, velocidad configurable. |
| Saltar sobre obstáculos y enemigos | Salto solo desde el suelo (sin doble salto), fuerza configurable. |
| Recolectar objetos y obtener puntos | Fresas (`Collectible`) y cajas de fresas: +10 puntos cada una. |
| Perder una vida al tocar un enemigo u obstáculo | `PlayerHealth`: 3 vidas, invulnerabilidad breve tras el golpe. |
| Regresar al último punto de control | `Checkpoint`: letreros "PUNTO FRESA". |
| Llegar al final del nivel para desbloquear la siguiente ciudad | `LevelGoal` + `Partida`: guarda el nivel completado y desbloquea el siguiente. |
| Caer fuera del escenario regresa al punto de control | Baches con zona de caída (`ZonaLetal`). |

## 2. Modificaciones y justificación

### 2.1 El Nivel 1 se divide en tres escenas
**Antes:** una sola escena de Irapuato.
**Ahora:** Estadio Sergio León Chávez, Centro Histórico y Central de Autobuses, conectadas con túneles y una transición.
**Justificación:** cada zona usa un fondo ilustrado distinto de un lugar real de Irapuato; dividirlo mantiene los fondos con buena resolución y permite subir la dificultad por zona. Las vidas, los puntos y el tamaño se conservan entre escenas, así que para el jugador sigue siendo un solo nivel.

### 2.2 Obstáculos propios de Irapuato
**Antes:** mochilas y cuadernos.
**Ahora:** botes de basura y baches (huecos de caída libre).
**Justificación:** los escenarios son calles reales de la ciudad; los obstáculos urbanos son coherentes con ellos y los baches agregan el reto de "caer fuera del escenario" que ya describía la lógica principal del GDD.

### 2.3 Fresas como coleccionable
**Antes:** monedas, libros y útiles.
**Ahora:** fresas.
**Justificación:** Irapuato es la capital de la fresa; el coleccionable refuerza la identidad del nivel. Libros y útiles pueden usarse en niveles posteriores.

### 2.4 La rata de alcantarilla como primer enemigo
**Antes:** tareas y exámenes animados.
**Ahora (Nivel 1):** rata de alcantarilla con camisa amarilla.
**Justificación:** encaja con el ambiente urbano. Su comportamiento (patrulla, da la vuelta al chocar, no cae a los huecos, se aplasta al caerle encima) es la IA básica que pide la evaluación. Los enemigos tipo tarea o examen se conservan para Dolores Hidalgo.

### 2.5 Cajas golpeables y tamaño grande como vida extra
**Nuevo:** cajas café (sueltan fresas) y doradas (sueltan un envase de mermelada que hace crecer al Estudiante). Estando grande, un golpe solo lo devuelve al tamaño normal.
**Justificación:** el GDD se inspira en Mario Bros.; esta mecánica da recompensa por explorar y un margen de error para jugadores de 8 a 15 años.

### 2.6 Vidas y progreso persistentes
**Nuevo:** las vidas, los puntos y el tamaño se conservan entre escenas; hay guardado automático, récord y nivel desbloqueado.
**Justificación:** sin esto, dividir el nivel en escenas reiniciaría el progreso; además lo pide la evaluación (sistema de guardado).

## 3. Condiciones de victoria y derrota
- **Victoria de nivel:** entrar por la puerta de la Central de Autobuses. Se muestra la ciudad superada, puntos, tiempo y récord.
- **Victoria del juego:** rescatar a la novia en la Catedral de San Miguel de Allende (Nivel 4, HU18).
- **Derrota:** quedarse sin vidas. Aparece Game Over con la opción de reintentar el nivel.

## 4. Dificultad progresiva (Nivel 1)
| Escena | Enemigos | Baches | Otros |
|---|---|---|---|
| Estadio | Ninguno (zona de práctica con tutorial) | 1 | 4 botes, cajas |
| Centro Histórico | 2 ratas lentas | 2 | escalera de jardineras |
| Central de Autobuses | 3 ratas más rápidas | 2 (uno más ancho) | cajas entre ratas |

## 5. Interfaz y sonido
- Pantalla de inicio, menú principal (Jugar, Continuar, Opciones, Créditos, Salir), opciones (volúmenes, calidad, pantalla completa, controles), pausa, HUD, Game Over y nivel superado.
- Música por nivel y efectos de salto, recolección, golpe, aplastar, caja, crecer, checkpoint, victoria y Game Over.
- La interfaz se escala a cualquier resolución (referencia 1920x1080).
- Recursos gráficos y de audio generados con Magnific, bajo la licencia del plan del equipo (ver créditos).

## 6. Tablero
https://github.com/users/MirzaMorales/projects/5
