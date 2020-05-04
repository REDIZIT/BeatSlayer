#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using CoversManagement;
using InGame.Helpers;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class UrlsChecker
{
    static UrlsChecker()
    {
        bool isLocalhost = IsGameWorkingWithLocalhost();
        if (isLocalhost)
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildDropper);
        }
    }
    public static void BuildDropper(BuildPlayerOptions obj)
    {
        Debug.LogError("UNITY BUILD WAS CANCELLED BY URLS CHECKER");
        throw new BuildPlayerWindow.BuildMethodException();
    }
    
    
    
    
    public static bool IsGameWorkingWithLocalhost(bool throwException = false)
    {
        List<string> localhostUrls = new List<string>();
        
        Stopwatch w = new Stopwatch();
        w.Start();
        
        CheckClassForLocalhost(typeof(AccountManager), localhostUrls);
        CheckClassForLocalhost(typeof(DatabaseScript), localhostUrls);
        CheckClassForLocalhost(typeof(Helpers), localhostUrls);
        CheckClassForLocalhost(typeof(CoversManager), localhostUrls);
        
        w.Stop();

        if (localhostUrls.Count > 0)
        {
            string message = "GAME IS WORKING WITH LOCALHOST SERVER!\n\n" + string.Join("\n", localhostUrls) + "\n\n";
            Debug.LogError(message);

            return true;
        }

        return false;
    }
    static void CheckClassForLocalhost(Type cls, List<string> log)
    {
        var fields = cls.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        foreach (var field in fields)
        {
            if(field == null) continue;

            if (field.IsLiteral && !field.IsInitOnly)
            {
                string value = (field.GetValue(null) as string).ToLower();
                if (value.Contains("localhost") || value.Contains(":5001") || value.Contains("https"))
                {
                    log.Add(cls.Name + ": " +field.Name);
                }
            }
        }
    }
}

#endif