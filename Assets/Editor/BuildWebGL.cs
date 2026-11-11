using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

// Build WebGL para itch.io: -executeMethod BuildWebGL.Build (sale en BuildWeb/)
// - itch.io limita cada archivo a 200 MB. Las texturas están sin compresión y el .data pasaba de 250 MB,
//   así que solo durante este build se comprimen para WebGL (DXT) y al terminar se deja todo como estaba.
// - Gzip con decompression fallback: itch.io no manda los encabezados Content-Encoding,
//   el fallback hace que el navegador lo descomprima solo (Brotli daba errores).
public static class BuildWebGL
{
    private const string WebGL = "WebGL";

    public static void Build()
    {
        var formatoPrevio = PlayerSettings.WebGL.compressionFormat;
        var fallbackPrevio = PlayerSettings.WebGL.decompressionFallback;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        var previos = ComprimirTexturasWebGL();
        BuildReport reporte;
        try
        {
            var escenas = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            reporte = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = escenas,
                locationPathName = "BuildWeb",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
        }
        finally
        {
            RestaurarTexturas(previos);
            PlayerSettings.WebGL.compressionFormat = formatoPrevio;
            PlayerSettings.WebGL.decompressionFallback = fallbackPrevio;
            AssetDatabase.SaveAssets();
        }
        EditorApplication.Exit(reporte.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Pone override WebGL comprimido a las texturas que no lo tienen; regresa los ajustes originales
    private static Dictionary<string, TextureImporterPlatformSettings> ComprimirTexturasWebGL()
    {
        var previos = new Dictionary<string, TextureImporterPlatformSettings>();
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
            {
                var ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(ruta) is TextureImporter imp)) continue;
                var actual = imp.GetPlatformTextureSettings(WebGL);
                if (actual.overridden) continue;

                var copia = new TextureImporterPlatformSettings();
                actual.CopyTo(copia);
                previos[ruta] = copia;

                var nuevo = new TextureImporterPlatformSettings();
                actual.CopyTo(nuevo);
                nuevo.overridden = true;
                nuevo.format = imp.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.DXT5 : TextureImporterFormat.DXT1;
                nuevo.textureCompression = TextureImporterCompression.Compressed;
                imp.SetPlatformTextureSettings(nuevo);
                imp.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        return previos;
    }

    private static void RestaurarTexturas(Dictionary<string, TextureImporterPlatformSettings> previos)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var par in previos)
            {
                if (!(AssetImporter.GetAtPath(par.Key) is TextureImporter imp)) continue;
                imp.SetPlatformTextureSettings(par.Value);
                imp.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
}
