using Libs.Xlsx.Readers;
using Libs.Xlsx.Types;
using OfficeOpenXml;

namespace Libs;

public class MainWorker
{
    private string C3APath { get; }
    private string C6Path { get; }
    private Sqlite.Sqlite Db { get; } 
    
    private IProgress<int>? Progress { get; }

    public MainWorker(string c3APath, string c6Path, IProgress<int>? progress = null)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        Db = new Sqlite.Sqlite();
        C3APath = c3APath;
        C6Path = c6Path;
        Progress = progress;
        
        
    }

    public async Task Start()
    {
        var c3A = new C3AReader(C3APath);
        // var c6 = new C6Reader(C6Path);

        var apps = await c3A.GetAllAppsByInsee();
        var listC6S = apps.Select(s => s.Key).Select(insee => new SInseeC6 { Insee = insee, C6Reader = new C6Reader(C6Path) }).ToList();

        await Parallel.ForEachAsync(listC6S, (c6S, _) =>
        {
            var c6 = c6S.C6Reader;
            var insee = c6S.Insee;
            var city = Db.GetCityNameByInsee(insee);
            
            Console.WriteLine($"{insee} => {city}");
            
            return default;
        });
    }
}