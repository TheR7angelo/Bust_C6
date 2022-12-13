using System;

namespace Burst_C6.Function;

public static class AssemblyCl
{
    private static readonly System.Reflection.Assembly Assembly = System.Reflection.Assembly.GetEntryAssembly()!;
    private static readonly System.Reflection.Assembly ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
    public static Version GetVersionDeploy() => Assembly.GetName().Version!;
    public static string GetNameDeploy() => Assembly.GetName().Name!;

    public static string GetFolderPath()
    {
        return AppContext.BaseDirectory;
    }

}