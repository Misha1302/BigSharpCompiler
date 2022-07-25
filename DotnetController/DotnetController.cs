using System.Diagnostics;
using System.Text;

namespace DotnetController;

public class DotnetBat
{
    private const string DOTNET_NAME = "CompilerDotnet.bat";

    public static void CreateWriteAndRunDotnet(string path, string code)
    {
        DotnetNewConsole(path);
        WriteCodeToDotnetProject(path, code);
        DotnetRun(path);
    }

    public static void DotnetNewConsole(string path, bool waitForExited = true)
    {
        ClearFile();
        using (var batStream = new FileStream(DOTNET_NAME, FileMode.OpenOrCreate))
        {
            var batText = $"cd /d {path}\ndotnet new console --force";
            var buffer = Encoding.Default.GetBytes(batText);
            batStream.Write(buffer, 0, buffer.Length);
        }

        var batFile = Process.Start(DOTNET_NAME);

        if (!waitForExited) return;
        while (!batFile.HasExited)
            Thread.Sleep(25);
    }


    public static void DotnetRun(string path, bool waitForExited = true)
    {
        ClearFile();
        using (var batStream = new FileStream(DOTNET_NAME, FileMode.OpenOrCreate))
        {
            var buffer = Encoding.Default.GetBytes($"cd /d {path}\ndotnet run");
            batStream.Write(buffer, 0, buffer.Length);
        }

        var batFile = Process.Start(DOTNET_NAME);

        if (!waitForExited) return;
        while (!batFile.HasExited)
            Thread.Sleep(25);
    }

    public static void WriteCodeToDotnetProject(string path, string code)
    {
        if (!path.EndsWith('\\')) path += '\\';
        File.WriteAllText(path + "Program.cs", code);
    }

    private static void ClearFile(string name = "CompilerDotnet.bat")
    {
        File.WriteAllText(name, string.Empty);
    }
}