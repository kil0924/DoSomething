using UnityEditor;
using System;

public class BuildScript
{
    public static void Build() 
    {
        PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("KEYALIAS_PASS");

        string buildPath = "BuildOutput/RPG.apk";
        BuildPipeline.BuildPlayer(new[] { "Assets/03. Rpg/Rpg.unity" }, 
            buildPath, 
            BuildTarget.Android, 
            BuildOptions.None);
    }
}
