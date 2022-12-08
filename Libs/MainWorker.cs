using Libs.Xlsx.Readers;
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
        var path = Path.GetDirectoryName(C6Path);
        var name = Path.GetFileNameWithoutExtension(C6Path);
        var savePath = Path.Join(path,name);

        var c3A = new C3AReader(C3APath);

        var apps = (await c3A.GetAllAppsByInsee()).ToList();

        var loop = 0;
        var max = apps.Count;
        
        await Parallel.ForEachAsync(apps, async (app, token) =>
        {
            var c6 = new C6Reader(C6Path);
            var insee = app.Key;
            
            var allApp = app.Select(s => s.App).ToList();

            var clearExport = ClearExport(c6, insee, allApp);
            var clearPicture = c6.CleanPicture(allApp);

            await Task.WhenAll(clearExport, clearPicture);
            
            await c6.Book.SaveAsAsync($"{savePath}-{insee}.xlsx", token);
            
            var p = Interlocked.Increment(ref loop);
            var pro = (int)((double)p / max * 100);
            Progress?.Report(pro);
        });
    }

    private async Task ClearExport(C6Reader c6, int insee, List<string> allApp)
    {
        var city = Db.GetCityNameByInsee(insee);

        await c6.Writecartridge(insee, city);
        await c6.CleanFields(allApp);
        await c6.CleanBackgroud();
    }
}