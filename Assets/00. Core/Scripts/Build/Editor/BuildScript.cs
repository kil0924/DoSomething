using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class BuildScript
{
    public static void Build() 
    {
        Console.WriteLine("빌드 시작");
        
        PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("KEYALIAS_PASS");

        var args = Environment.GetCommandLineArgs();
        var name = GetArg(args, "-buildName") ?? "DoSomething";
        var isDebug = (GetArg(args, "-isDebug")?.ToLower() ?? "false") == "true" ;
        if (int.TryParse(GetArg(args, "-buildNumber"), out var buildNumber) == false)
        {
            buildNumber = 0;
        }
        
        BuildAndroid(name, isDebug, buildNumber);
    }

    private static void BuildAndroid(string name, bool isDebug, int buildNumber)
    {
        var sb = new StringBuilder();
        sb.Append($"name : {name}, ");
        sb.Append($"buildNumber : {buildNumber}, ");
        sb.Append($"isDebug : {isDebug}, ");
        Console.WriteLine(sb.ToString());
        
        string buildPath = $"Home/Output/Android/{buildNumber:000}_{name}.apk";
        BuildPipeline.BuildPlayer(new[] { "Assets/99. ETC/Performance Test/PerformanceTest.unity" }, 
            buildPath, 
            BuildTarget.Android,
            isDebug ? BuildOptions.Development : BuildOptions.None);
        Console.WriteLine("빌드 끝");
    }
    
    
    static string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
            
        return null;
    }
}