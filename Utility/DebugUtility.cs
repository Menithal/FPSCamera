
using System;
using System.IO;
using UnityEngine;


class DebugUtility
{
    String debugPath = "";

    float throttle = 0;
    long count = 0;

    public DebugUtility()
    {
        String drive = Environment.GetEnvironmentVariable("HOMEDRIVE");
        String homePath = Environment.GetEnvironmentVariable("HOMEPATH");
        debugPath = drive + homePath + "\\Documents\\LIV\\Output\\PluginLogs.txt";
    }
    public void Write(String type, String source, String message, bool throttleOverride = false)
    {
        throttle += Time.deltaTime;

        if (throttle > 0.25 || throttleOverride)
        {
            if (!throttleOverride)
            {
                throttle = 0;
            }
            using (StreamWriter swrite = new StreamWriter(@debugPath, true))
            {
                if (count > 0 && !throttleOverride)
                {
                    swrite.WriteLine(DateTime.Now + " - WARN - DebugUtility - Throttled " + count + " Logs");
                }

                swrite.WriteLine(DateTime.Now + " - " + type.ToUpper() + " - " + source + ": " + message);
            }
        }
        else
        {
            count++;
        }
    }
}

public static class PluginLog
{
    private static DebugUtility debugger = new DebugUtility();

    public static void Log(String source, String message)
    {
#if DEBUG
        debugger.Write("log", source, message, true);
#endif
    }
    public static void Debug(String source, String message)
    {
#if DEBUG
        debugger.Write("debug", source, message);
#endif
    }
    public static void Warn(String source, String message)
    {
#if DEBUG
        debugger.Write("warn", source, message);
#endif
    }
    public static void Error(String source, String message)
    {
#if DEBUG
        debugger.Write("warn", source, message);
#endif
    }
}
