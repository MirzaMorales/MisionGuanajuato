using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Control de la partida. Se crea solo al iniciar el juego y sobrevive entre escenas.
// - Jugar, Continuar, Reintentar, Siguiente nivel y Menú principal.
// - Cronómetro del nivel (se muestra en el HUD).
// - Al entrar a cada escena jugable: guarda el progreso, pone la música, crea el HUD y el menú de pausa.
// - Derrota (sin vidas) -> pantalla de Game Over. Victoria (meta del nivel) -> pantalla de nivel superado.
public class Partida : MonoBehaviour
{
    public const string EscenaSplash = "Splash";
    public const string EscenaMenu = "MenuPrincipal";
    public const string PrimeraEscena = "Nivel1_Irapuato";

    // Datos de cada escena jugable: textos de la narrativa, objetivo y música
    public class InfoEscena
    {
        public string escena;
        public int numeroNivel;
        public string ciudad;
        public string lugar;
        public string narrativa;
        public string objetivo;
        public string musica;
        public string primeraEscenaDelNivel;
        public string siguienteNivel;
        public string mensajeVictoria;
        // Ícono del marcador de puntos (ruta en Resources)
        public string iconoColeccionable = "UI/fresa";
    }

    private static readonly InfoEscena[] Escenas =
    {
        new InfoEscena
        {
            escena = "Nivel1_Irapuato", numeroNivel = 1, ciudad = "Irapuato", lugar = "Estadio Sergio León Chávez",
            narrativa = "La invitación misteriosa decía: «Tu viaje empieza en el estadio».",
            objetivo = "Objetivo: llega al túnel al final del estadio",
            musica = "nivel1", primeraEscenaDelNivel = "Nivel1_Irapuato", siguienteNivel = "Nivel2_Guanajuato"
        },
        new InfoEscena
        {
            escena = "Nivel1B_CentroHistorico", numeroNivel = 1, ciudad = "Irapuato", lugar = "Centro Histórico",
            narrativa = "Alguien vio a tu novia cerca del Templo de San Francisco...",
            objetivo = "Objetivo: cruza el centro y entra al túnel. ¡Cuidado con las ratas!",
            musica = "nivel1", primeraEscenaDelNivel = "Nivel1_Irapuato", siguienteNivel = "Nivel2_Guanajuato"
        },
        new InfoEscena
        {
            escena = "Nivel1C_CentralAutobuses", numeroNivel = 1, ciudad = "Irapuato", lugar = "Central de Autobuses",
            narrativa = "Un boleto olvidado dice: «Destino: Guanajuato capital».",
            objetivo = "Objetivo: entra por la puerta de la terminal",
            musica = "nivel1", primeraEscenaDelNivel = "Nivel1_Irapuato", siguienteNivel = "Nivel2_Guanajuato",
            mensajeVictoria = "Encontraste el boleto de autobús a Guanajuato capital.\nLa pista de tu novia continúa..."
        },
        new InfoEscena
        {
            escena = "Nivel2_Guanajuato", numeroNivel = 2, ciudad = "Guanajuato", lugar = "Callejones",
            narrativa = "Dicen que vieron a tu novia subiendo por el Callejón del Beso...",
            objetivo = "Objetivo: cruza los callejones y entra al túnel. ¡Cuidado con las momias!",
            musica = "nivel2", primeraEscenaDelNivel = "Nivel2_Guanajuato", siguienteNivel = "Nivel3_DoloresHidalgo",
            iconoColeccionable = "UI/charamusca"
        },
        new InfoEscena
        {
            escena = "Nivel2B_Tuneles", numeroNivel = 2, ciudad = "Guanajuato", lugar = "Túneles subterráneos",
            narrativa = "Bajo la ciudad corren los túneles... alguien vio pasar a una señora con tu novia.",
            objetivo = "Objetivo: atraviesa los túneles. ¡Las momias aquí son más rápidas!",
            musica = "nivel2", primeraEscenaDelNivel = "Nivel2_Guanajuato", siguienteNivel = "Nivel3_DoloresHidalgo",
            iconoColeccionable = "UI/charamusca"
        },
        new InfoEscena
        {
            escena = "Nivel2C_Pipila", numeroNivel = 2, ciudad = "Guanajuato", lugar = "Mirador del Pípila",
            narrativa = "Desde el Pípila se ve toda la ciudad... y a lo lejos, a la señora con tu novia.",
            objetivo = "Objetivo: sube en el funicular y llega al monumento del Pípila",
            musica = "nivel2", primeraEscenaDelNivel = "Nivel2_Guanajuato", siguienteNivel = "Nivel3_DoloresHidalgo",
            iconoColeccionable = "UI/charamusca",
            mensajeVictoria = "Desde el Pípila viste salir un coche rumbo a Dolores Hidalgo.\nLa pista de tu novia continúa..."
        },
        new InfoEscena
        {
            escena = "Nivel3_DoloresHidalgo", numeroNivel = 3, ciudad = "Dolores Hidalgo", lugar = "Plaza Principal",
            narrativa = "Dolores Hidalgo, la cuna de la Independencia... pero hoy el tiempo corre en tu contra.",
            objetivo = "Objetivo: cruza la plaza hasta el túnel. ¡Los exámenes y los relojes quieren detenerte!",
            musica = "nivel3", primeraEscenaDelNivel = "Nivel3_DoloresHidalgo", siguienteNivel = "Nivel4_ParqueJuarez",
            iconoColeccionable = "UI/nieve"
        },
        new InfoEscena
        {
            escena = "Nivel3B_UTNG", numeroNivel = 3, ciudad = "Dolores Hidalgo", lugar = "Universidad Tecnológica del Norte de Guanajuato",
            narrativa = "En la universidad dicen que la señora pasó con tu novia rumbo al mercado.",
            objetivo = "Objetivo: cruza el campus de la UTNG. ¡Cuidado con los relojes despertadores!",
            musica = "nivel3", primeraEscenaDelNivel = "Nivel3_DoloresHidalgo", siguienteNivel = "Nivel4_ParqueJuarez",
            iconoColeccionable = "UI/nieve"
        },
        new InfoEscena
        {
            escena = "Nivel3C_Mercado", numeroNivel = 3, ciudad = "Dolores Hidalgo", lugar = "Mercado Independencia",
            narrativa = "En el mercado huele a nieve de mantecado... una vendedora señala la nevería del fondo.",
            objetivo = "Objetivo: atraviesa el mercado y entra a la nevería",
            musica = "nivel3", primeraEscenaDelNivel = "Nivel3_DoloresHidalgo", siguienteNivel = "Nivel4_ParqueJuarez",
            iconoColeccionable = "UI/nieve",
            mensajeVictoria = "En la nevería te dieron la pista: tu novia fue llevada a San Miguel de Allende.\nLa Catedral te espera..."
        },
        new InfoEscena
        {
            escena = "Nivel4_ParqueJuarez", numeroNivel = 4, ciudad = "San Miguel de Allende", lugar = "Parque Benito Juárez",
            narrativa = "Entre jacarandas y bancas de hierro, alguien vio pasar a la señora con tu novia.",
            objetivo = "Objetivo: cruza el parque. ¡Cuidado con los turistas, los burros y las mojigangas!",
            musica = "nivel4", primeraEscenaDelNivel = "Nivel4_ParqueJuarez", siguienteNivel = "",
            iconoColeccionable = "UI/corazon_alado"
        },
        new InfoEscena
        {
            escena = "Nivel4B_Chorro", numeroNivel = 4, ciudad = "San Miguel de Allende", lugar = "El Chorro y los Lavaderos",
            narrativa = "Junto al manantial donde lavaban las abuelas, las huellas siguen hacia el centro.",
            objetivo = "Objetivo: baja por El Chorro hasta el túnel",
            musica = "nivel4", primeraEscenaDelNivel = "Nivel4_ParqueJuarez", siguienteNivel = "",
            iconoColeccionable = "UI/corazon_alado"
        },
        new InfoEscena
        {
            escena = "Nivel4C_JardinAtrio", numeroNivel = 4, ciudad = "San Miguel de Allende", lugar = "Jardín Principal y atrio de la Parroquia",
            narrativa = "Las torres de la Parroquia se alzan al fondo... tu novia tiene que estar ahí dentro.",
            objetivo = "Objetivo: consigue la llave y abre la puerta de la Parroquia",
            musica = "nivel4", primeraEscenaDelNivel = "Nivel4_ParqueJuarez", siguienteNivel = "",
            iconoColeccionable = "UI/corazon_alado"
        },
        new InfoEscena
        {
            escena = "Nivel4D_Parroquia", numeroNivel = 4, ciudad = "San Miguel de Allende", lugar = "Interior de la Parroquia",
            narrativa = "Mr. Johnson y Doña María no quieren que llegues hasta ella...",
            objetivo = "Esquiva los billetes y lánzale comida con F",
            musica = "jefe", primeraEscenaDelNivel = "Nivel4_ParqueJuarez", siguienteNivel = "",
            iconoColeccionable = "UI/corazon_alado",
            mensajeVictoria = "Doña María entendió y tu novia corrió a tus brazos.\n¡Misión cumplida, Guanajuato!"
        },
    };

    private static Partida instancia;
    private bool terminada;

    public static float Tiempo { get; private set; }
    public static InfoEscena EscenaActual { get; private set; }
    public static bool Terminada => instancia != null && instancia.terminada;

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain"
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        if (instancia != null) return;

        Tiempo = 0f;
        GameObject go = new GameObject("Partida");
        DontDestroyOnLoad(go);
        instancia = go.AddComponent<Partida>();
        SceneManager.sceneLoaded += instancia.AlCargarEscena;

        // Calidad gráfica elegida en el menú de opciones
        int calidad = PlayerPrefs.GetInt(PanelOpciones.ClaveCalidad, QualitySettings.GetQualityLevel());
        if (calidad >= 0 && calidad < QualitySettings.names.Length) QualitySettings.SetQualityLevel(calidad, true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void Update()
    {
        if (EscenaActual != null && !terminada && Time.timeScale > 0f) Tiempo += Time.deltaTime;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f;
        terminada = false;
        EscenaActual = Buscar(escena.name);

        if (escena.name == EscenaSplash || escena.name == EscenaMenu)
        {
            Sonido.Musica("menu");
            return;
        }
        if (EscenaActual == null) return;

        Sonido.Musica(EscenaActual.musica);

        PlayerHealth jugador = FindFirstObjectByType<PlayerHealth>();
        if (jugador != null)
        {
            jugador.GameOver += AlPerder;
            // Respaldo: si por algo la animación de derrota no llega a avisar, la pantalla sale de todos modos
            jugador.VidasCambiadas += vidas =>
            {
                if (vidas <= 0) StartCoroutine(GameOverDeRespaldo());
            };
            // Autoguardado al inicio de cada escena: "Continuar" vuelve aquí
            Guardado.GuardarPartida(escena.name, jugador.VidasActuales, Puntuacion.Total, jugador.EstaGrande, Tiempo);
        }

        HUDJuego.Crear(EscenaActual);
        MenuPausa.Crear();
    }

    private static InfoEscena Buscar(string nombre)
    {
        foreach (InfoEscena e in Escenas)
        {
            if (e.escena == nombre) return e;
        }
        return null;
    }

    // ---------- Acciones de los menús ----------

    // Partida nueva: vidas completas, puntos en 0 y desde la primera escena
    public static void NuevaPartida()
    {
        PlayerHealth.ReiniciarVidas();
        Puntuacion.ReiniciarPuntos();
        Llave.Reiniciar();
        Tiempo = 0f;
        Guardado.BorrarPartida();
        Time.timeScale = 1f;
        TransicionEscena.Cargar(PrimeraEscena);
    }

    public static void Continuar()
    {
        if (!Guardado.HayPartida) return;

        PlayerHealth.EstablecerPartida(Guardado.VidasGuardadas, Guardado.GrandeGuardado);
        Puntuacion.Establecer(Guardado.PuntosGuardados);
        Tiempo = Guardado.TiempoGuardado;
        Time.timeScale = 1f;
        TransicionEscena.Cargar(Guardado.EscenaGuardada);
    }

    // Después de Game Over: el nivel empieza de nuevo con las vidas completas
    public static void Reintentar()
    {
        string inicio = EscenaActual != null ? EscenaActual.primeraEscenaDelNivel : PrimeraEscena;
        PlayerHealth.ReiniciarVidas();
        Puntuacion.ReiniciarPuntos();
        Llave.Reiniciar();
        Tiempo = 0f;
        Time.timeScale = 1f;
        TransicionEscena.Cargar(inicio);
    }

    public static bool HaySiguienteNivel =>
        EscenaActual != null && !string.IsNullOrEmpty(EscenaActual.siguienteNivel)
        && Application.CanStreamedLevelBeLoaded(EscenaActual.siguienteNivel);

    public static void SiguienteNivel()
    {
        if (!HaySiguienteNivel) return;
        Time.timeScale = 1f;
        TransicionEscena.Cargar(EscenaActual.siguienteNivel);
    }

    public static void IrAlMenu()
    {
        Time.timeScale = 1f;
        TransicionEscena.Cargar(EscenaMenu);
    }

    // ---------- Victoria y derrota ----------

    // Lo llama LevelGoal en la última escena de un nivel
    public static void NivelCompletado()
    {
        if (instancia == null || EscenaActual == null || instancia.terminada) return;
        instancia.terminada = true;

        bool record = Guardado.RegistrarPuntaje(Puntuacion.Total);
        Guardado.DesbloquearNivel(EscenaActual.numeroNivel + 1);

        // "Continuar" llevará al siguiente nivel en cuanto exista en Build Settings
        PlayerHealth jugador = FindFirstObjectByType<PlayerHealth>();
        if (jugador != null && !string.IsNullOrEmpty(EscenaActual.siguienteNivel))
        {
            Guardado.GuardarPartida(EscenaActual.siguienteNivel, jugador.VidasActuales, Puntuacion.Total, jugador.EstaGrande, 0f);
        }

        Sonido.DetenerMusica();
        Sonido.Efecto("victoria");
        PantallaFinNivel.Mostrar(EscenaActual, Puntuacion.Total, Tiempo, record);
        Time.timeScale = 0f;
    }

    // La animación de derrota dura ~2 s; si a los 3.5 s no avisó, se muestra el Game Over igual
    private IEnumerator GameOverDeRespaldo()
    {
        yield return new WaitForSecondsRealtime(3.5f);
        if (!terminada) Debug.LogWarning("Partida: la animación de derrota no avisó del Game Over; se muestra por respaldo");
        AlPerder();
    }

    private void AlPerder()
    {
        if (terminada) return;
        // Si ya se venció al jefe se ve el final feliz, nunca el Game Over
        if (BossController.JefeVencido) return;
        terminada = true;
        StartCoroutine(MostrarGameOver());
    }

    private IEnumerator MostrarGameOver()
    {
        bool record = Guardado.RegistrarPuntaje(Puntuacion.Total);
        Guardado.BorrarPartida();
        Sonido.DetenerMusica();
        // Si la escena tiene su propio final triste (ej. el del jefe en la Parroquia), se ve antes del Game Over
        FinalDerrota finalTriste = FindFirstObjectByType<FinalDerrota>();
        if (finalTriste != null) yield return finalTriste.Secuencia();
        Sonido.Efecto("gameover");

        // PlayerHealth avisa al terminar su animación de derrota; solo un respiro antes de la pantalla
        yield return new WaitForSecondsRealtime(0.3f);
        PantallaGameOver.Mostrar(Puntuacion.Total, Tiempo, record);
        Time.timeScale = 0f;
    }

    public static string FormatoTiempo(float segundos)
    {
        int s = Mathf.FloorToInt(segundos);
        return $"{s / 60:00}:{s % 60:00}";
    }
}
