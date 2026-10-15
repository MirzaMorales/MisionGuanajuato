using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Introducción de la historia antes de empezar una partida nueva: el texto aparece letra por letra.
// Enter, Espacio, clic o el botón A del control adelantan el texto y luego comienzan el juego
public class PantallaHistoria : MonoBehaviour
{
    private const float LetrasPorSegundo = 45f;

    private static readonly string[] Parrafos =
    {
        "Un día, tu novia recibió una misteriosa invitación para visitar distintos lugares de Guanajuato... y desapareció.",
        "Descubres que está atrapada en la Catedral de San Miguel de Allende.",
        "Para llegar hasta ella tendrás que recorrer cuatro ciudades, juntar fresas y esquivar los peligros del camino.",
        "Tu viaje comienza en Irapuato, la capital de la fresa."
    };

    private TextMeshProUGUI texto;
    private TextMeshProUGUI aviso;
    private Action alTerminar;
    private bool escribiendo;
    private bool terminado;
    private float inicio;

    void Awake()
    {
        inicio = Time.unscaledTime;
    }

    public static void Mostrar(Transform padre, Action alTerminar)
    {
        Image velo = InterfazUI.Panel(padre, new Color(0.04f, 0.03f, 0.07f, 0.96f), "Historia");
        velo.raycastTarget = true;
        PantallaHistoria h = velo.gameObject.AddComponent<PantallaHistoria>();
        h.alTerminar = alTerminar;

        h.texto = InterfazUI.Texto(velo.transform, "", 50, InterfazUI.ColorTexto);
        InterfazUI.Colocar(h.texto.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1500, 700));

        h.aviso = InterfazUI.Texto(velo.transform, "Enter / Espacio / A para continuar", 32, InterfazUI.ColorTitulo);
        InterfazUI.Colocar(h.aviso.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 60), new Vector2(1200, 60));

        Button saltar = InterfazUI.Boton(velo.transform, "Saltar historia", h.Terminar, 420, 90);
        InterfazUI.Colocar((RectTransform)saltar.transform, new Vector2(1f, 0f), new Vector2(-40, 40), new Vector2(420, 90));

        h.StartCoroutine(h.Escribir());
    }

    private IEnumerator Escribir()
    {
        escribiendo = true;
        texto.text = string.Join("\n\n", Parrafos);
        texto.maxVisibleCharacters = 0;
        texto.ForceMeshUpdate();
        int total = texto.textInfo.characterCount;

        for (float visibles = 0; visibles < total && escribiendo; visibles += LetrasPorSegundo * Time.unscaledDeltaTime)
        {
            texto.maxVisibleCharacters = (int)visibles;
            yield return null;
        }
        texto.maxVisibleCharacters = total;
        escribiendo = false;
    }

    void Update()
    {
        // La misma tecla que abrió la historia no debe saltarla
        if (Time.unscaledTime - inicio < 0.3f) return;

        bool avanzar = (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                       || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                       || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        if (!avanzar) return;

        // Primer toque: muestra todo el texto; segundo toque: empieza el juego
        if (escribiendo) escribiendo = false;
        else Terminar();
    }

    private void Terminar()
    {
        if (terminado) return;
        terminado = true;
        alTerminar?.Invoke();
    }
}
