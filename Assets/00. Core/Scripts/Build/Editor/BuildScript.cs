using UnityEditor;
using System;

public class BuildScript
{
    public static void Build() 
    {
        string buildPath = "BuildOutput/RPG.apk";
        BuildPipeline.BuildPlayer(new[] { "Assets/03. Rpg/Rpg.unity" }, 
            buildPath, 
            BuildTarget.Android, 
            BuildOptions.None);
    }
}
