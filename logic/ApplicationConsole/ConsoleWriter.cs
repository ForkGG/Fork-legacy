using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Fork.Logic.ApplicationConsole;

public class ConsoleWriter : TextWriter
{
    private bool appStarted;
    private FileInfo logFile;
    private readonly List<string> preCachedList = new();

    public override Encoding Encoding => Encoding.ASCII;

    public override void WriteLine(string value)
    {
        if (appStarted)
        {
            WriteLine(value, 0);
        }
        else
        {
            preCachedList.Add(value);
        }
    }

    private void WriteLine(string value, int retries)
    {
        try
        {
            using StreamWriter sw = logFile.AppendText();
            DateTime now = DateTime.Now;
            string toWrite = "[" + now + "] " + value;
            sw.WriteLine(toWrite);
        }
        catch (IOException)
        {
            retries++;
            if (retries < 200)
            {
                Thread thread = new(() =>
                {
                    Thread.Sleep(500);
                    WriteLine(value, retries);
                });
                thread.IsBackground = true;
                thread.Start();
            }
            // No more retries? Give up!
        }
        catch (Exception)
        {
            // Should not fail the app
        }
    }

    public void AppStarted()
    {
        logFile = new FileInfo(Path.Combine(App.ApplicationPath, "logs", "consoleLog.txt"));
        if (!logFile.Exists)
        {
            if (!new DirectoryInfo(Path.Combine(App.ApplicationPath, "logs")).Exists)
            {
                Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "logs"));
            }

            FileStream r = logFile.Create();
            r.Close();
        }

        appStarted = true;
        string toAdd = "";
        foreach (string s in preCachedList) toAdd += $"\n{s}";
        WriteLine(toAdd);
    }
}