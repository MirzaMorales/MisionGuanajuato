using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Tipografías del juego, ambas pixel art y con licencia SIL Open Font License:
// - Press Start 2P para títulos (estilo arcade).
// - Pixelify Sans para botones y textos (pixel legible).
// Se crean como fuentes dinámicas de TextMesh Pro a partir de los TTF de Resources/Fuentes,
// con Liberation Sans de respaldo por si falta algún carácter
public static class Tipografia
{
    private static TMP_FontAsset titulo;
    private static TMP_FontAsset texto;

    public static TMP_FontAsset Titulo => titulo != null ? titulo : (titulo = Crear("Fuentes/PressStart2P"));
    public static TMP_FontAsset Texto => texto != null ? texto : (texto = Crear("Fuentes/PixelifySans"));

    private static TMP_FontAsset Crear(string ruta)
    {
        Font fuente = Resources.Load<Font>(ruta);
        if (fuente == null)
        {
            Debug.LogWarning("Tipografia: no existe la fuente Resources/" + ruta);
            return TMP_Settings.defaultFontAsset;
        }

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(fuente, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        if (asset == null) return TMP_Settings.defaultFontAsset;

        asset.name = fuente.name;
        if (TMP_Settings.defaultFontAsset != null)
        {
            asset.fallbackFontAssetTable = new List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };
        }
        return asset;
    }
}
