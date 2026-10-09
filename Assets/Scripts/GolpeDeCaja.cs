using UnityEngine;

// Lógica compartida por las cajas golpeables (CajaMermelada y CajaFresas)
public static class GolpeDeCaja
{
    // ¿El jugador golpeó la caja desde abajo con la cabeza?
    public static bool EsGolpeDesdeAbajo(Collision2D col, Transform caja)
    {
        // Solo el jugador abre cajas (el Estudiante no tiene tag, se reconoce por su controlador)
        if (col.collider.GetComponent<PlayerController>() == null) return false;

        // Debe quedar más o menos debajo de la caja, con algún punto de contacto por debajo de su centro
        bool debajo = Mathf.Abs(col.transform.position.x - caja.position.x) < 1.0f;
        if (!debajo) return false;

        foreach (ContactPoint2D contacto in col.contacts)
        {
            if (contacto.point.y < caja.position.y - 0.1f) return true;
        }
        return false;
    }
}
