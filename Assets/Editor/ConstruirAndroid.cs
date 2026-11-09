using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Genera el APK para instalar en un celular Android y probar los controles táctiles.
// Menú: Misión Guanajuato > Construir APK (Android), o por consola con -executeMethod ConstruirAndroid.Construir
// El APK sale en Builds/MisionGuanajuato.apk, firmado con la llave de depuración de Unity (sirve para instalarlo a mano)
public static class ConstruirAndroid
{
    private const string Ruta = "Builds/MisionGuanajuato.apk";

    [MenuItem("Misión Guanajuato/Construir APK (Android)")]
    public static void Construir()
    {
        string[] escenas = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (escenas.Length == 0)
        {
            Debug.LogError("No hay escenas activas en Build Profiles.");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "mx.utng.misionguanajuato");
        PlayerSettings.productName = "Misión Guanajuato";
        // ARM64 necesita IL2CPP; los celulares actuales son de 64 bits
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        EditorUserBuildSettings.buildAppBundle = false;

        Directory.CreateDirectory("Builds");
        BuildReport informe = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = escenas,
            locationPathName = Ruta,
            target = BuildTarget.Android,
            options = BuildOptions.None
        });

        bool bien = informe.summary.result == BuildResult.Succeeded;
        Debug.Log(bien ? $"APK listo: {Ruta} ({informe.summary.totalSize / (1024 * 1024)} MB)" : "Falló la construcción del APK.");
        if (Application.isBatchMode) EditorApplication.Exit(bien ? 0 : 1);
    }
}
