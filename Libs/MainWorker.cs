using System.IO.Compression;
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

        using var c3A = new C3AReader(C3APath);

        var apps = await c3A.GetAllAppsByInsee();
        
        var loop = 0;
        var insees = apps.Select(s => s.Insee).DistinctBy(s => s).ToList();
        var max = insees.Count;

        await Parallel.ForEachAsync(insees, async (insee, token) =>
        {
            using var c6 = new C6Reader(C6Path);

            var allApp = apps.Where(s => s.Insee.Equals(insee)).Select(s => s.App).ToList();

            var clearExport = ClearExport(c6, insee, allApp);
            var clearPicture = c6.CleanPicture(allApp);

            await Task.WhenAll(clearExport, clearPicture);

            var filePath = $"{savePath}-{insee}.xlsx";
            await c6.Book.SaveAsAsync(filePath, token);

            await ClearZip(filePath, clearPicture.Result);
            
            var p = Interlocked.Increment(ref loop);
            var pro = (int)((double)p / max * 100);
            Progress?.Report(pro);
        });
        
        Console.WriteLine("end");
    }

    private async Task ClearExport(C6Reader c6, int insee, List<string> allApp)
    {
        var city = Db.GetCityNameByInsee(insee);

        await c6.Writecartridge(insee, city);
        await c6.CleanFields(allApp, insee);
        await c6.CleanBackgroud();
    }

    private Task ClearZip(string filePath, IEnumerable<string> pictures)
    {
        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Update);

        foreach (var picture in pictures)
        {
            var entry = archive.Entries.FirstOrDefault(s => s.FullName.Equals(picture));
            entry?.Delete();
        }

        return Task.CompletedTask;
    }
}