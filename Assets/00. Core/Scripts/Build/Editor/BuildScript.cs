using UnityEditor;
using System;
using System.IO;
using UnityEngine;

public class BuildScript
{
    public static void Build() 
    {
        Console.WriteLine("빌드 시작");
        
        PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("KEYALIAS_PASS");

        string buildPath = "BuildOutput/RPG.apk";
        BuildPipeline.BuildPlayer(new[] { "Assets/03. Rpg/Rpg.unity" }, 
            buildPath, 
            BuildTarget.Android, 
            BuildOptions.None);
        Console.WriteLine("빌드 끝");
    }
}