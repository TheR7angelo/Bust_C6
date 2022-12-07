using Libs.Xlsx.Readers;
using Libs.Xlsx.Types;
using OfficeOpenXml;

namespace Libs;

public class MainWorker
{
    private string C3APath { get; }
    private string C6Path { get; }

    private static Random Random = new();
    
    private IProgress<int>? Progress { get; }

    public MainWorker(string c3APath, string c6Path, IProgress<int>? progress = null)
    {
        C3APath = c3APath;
        C6Path = c6Path;
        Progress = progress;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task Start()
    {
        var c3A = new C3AReader(C3APath);
        // var c6 = new C6Reader(C6Path);

        var apps = await c3A.GetAllAppsByInsee();
        var c6S = apps.Select(s => s.Key).Select(insee => new SInseeC6 { Insee = insee, C6Reader = new C6Reader(C6Path) }).ToList();
        
        
    }
}