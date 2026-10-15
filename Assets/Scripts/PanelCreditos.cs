using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pantalla de créditos: autores, herramientas y recursos externos con su licencia.
// El texto se desplaza solo hacia arriba
public class PanelCreditos : MonoBehaviour
{
    private const float Velocidad = 60f;

    private const string Texto =
        "<size=70><color=#FFC832>MISIÓN GUANAJUATO</color></size>\n<size=44>Rescate en 2D</size>\n\n" +
        "<color=#FFC832>DESARROLLO</color>\n" +
        "Mirza Natzielly Morales Lezama\n<size=32>Programación, integración y build</size>\n\n" +
        "Juan Gustavo Ángel Cruz Méndez\n<size=32>Historia, arte, niveles y audio</size>\n\n" +
        "<color=#FFC832>INSTITUCIÓN</color>\n" +
        "Universidad Tecnológica del Norte de Guanajuato\n" +
        "Ingeniería en Gestión y Desarrollo de Software\nCreación de Videojuegos · 2026\n\n" +
        "<color=#FFC832>HERRAMIENTAS</color>\n" +
        "Unity 6 (Universal Render Pipeline 2D)\nC#\nTextMesh Pro\nInput System\nGit, GitHub y GitHub Projects\n\n" +
        "<color=#FFC832>RECURSOS EXTERNOS Y LICENCIAS</color>\n" +
        "Fondos, sprites, interfaz, música y efectos de sonido:\ngenerados con Magnific, bajo la licencia\nde uso del plan de suscripción del equipo.\n\n" +
        "Fuente tipográfica: Liberation Sans\n(incluida con TextMesh Pro, licencia SIL Open Font License).\n\n" +
        "Fotografías de Irapuato usadas solo como referencia\npara dibujar los escenarios.\n\n" +
        "\"Cemento Fresazul\" es una marca ficticia.\n\n" +
        "<color=#FFC832>GRACIAS POR JUGAR</color>";

    private RectTransform contenido;
    private RectTransform marco;

    public static RectTransform Abrir(Transform padre, Action alCerrar)
    {
        RectTransform caja = InterfazUI.Ventana(padre, "CRÉDITOS", new Vector2(1300, 1000), InterfazUI.ColorTitulo);

        // Marco con máscara: el texto se desplaza dentro sin salirse de la ventana
        RectTransform marco = InterfazUI.Colocar(InterfazUI.Crear("Marco", caja), new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(1200, 660));
        marco.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI t = InterfazUI.Texto(marco, Texto, 40, InterfazUI.ColorTexto, TextAlignmentOptions.Top, "Texto");
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        // La altura se calcula con el ancho final del texto
        float alto = t.GetPreferredValues(Texto, 1150f, 0f).y;
        rt.sizeDelta = new Vector2(1150, alto + 40);
        rt.anchoredPosition = new Vector2(0, -660);

        PanelCreditos panel = caja.gameObject.AddComponent<PanelCreditos>();
        panel.contenido = rt;
        panel.marco = marco;

        Button volver = InterfazUI.Boton(caja, "Volver", () => alCerrar?.Invoke());
        InterfazUI.Colocar((RectTransform)volver.transform, new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(520, 110));
        InterfazUI.Seleccionar(volver);
        return caja;
    }

    void Update()
    {
        // Sube el texto; al terminar vuelve a empezar desde abajo
        Vector2 p = contenido.anchoredPosition;
        p.y += Velocidad * Time.unscaledDeltaTime;
        if (p.y > contenido.sizeDelta.y) p.y = -marco.sizeDelta.y;
        contenido.anchoredPosition = p;
    }
}
