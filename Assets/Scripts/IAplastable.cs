using UnityEngine;

// Enemigo al que se le puede caer encima para aplastarlo (rata, examen, reloj...). PlayerHealth lo consulta para no quitar
// una vida cuando el jugador lo pisa desde arriba
public interface IAplastable
{
    bool EstaAplastada { get; }

    // El jugador está encima del enemigo: sus pies están a la altura de la cabeza o más arriba
    bool EsPisadaPor(Collider2D jugadorCol);

    // Le llegó comida lanzada con el poder de la mochila: el enemigo cae como si lo hubieran aplastado.
    // Devuelve falso si no le hizo nada (escondida, ya caída, jefe...) y la comida sigue de largo
    bool RecibirProyectil();
}
