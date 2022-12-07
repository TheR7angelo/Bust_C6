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

        var loop = 0;
        await Parallel.ForEachAsync(apps, (app, _) =>
        {
            var c6 = new C6Reader(C6Path);
            var insee = app.Key;
            var city = Db.GetCityNameByInsee(insee);
            


            Interlocked.Increment(ref loop);
            
            return default;
        });
    }
}