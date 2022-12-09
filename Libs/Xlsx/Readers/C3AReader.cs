using Libs.Xlsx.Types;

namespace Libs.Xlsx.Readers;

public class C3AReader : Reader, IDisposable
{
    public C3AReader(string file) : base(file)
    {
    }

    public async Task<List<SInseeApp>> GetAllAppsByInsee()
    {
        var tenant = GetAllApp(2, 15);
        var abouti = GetAllApp(4, 15);

        await Task.WhenAll(tenant, abouti);

        var apps = tenant.Result.Concat(abouti.Result).Distinct();

        var results = new List<SInseeApp>();
        foreach (var appInsee in apps)
        {
            if (appInsee is null) continue;
            var app = appInsee.Split('/');
            var insee = Convert.ToInt32(app[0]);

            results.Add(new SInseeApp
            {
                Insee = insee,
                App = app[1]
            });
        }
        
        return results;
    }

    private Task<List<string>> GetAllApp(int col, int minRow)
    {
        var results = new List<string>();
        var sheet = Book.Workbook.Worksheets["Commandes Fermes"];
        
        Parallel.For(minRow, sheet.Dimension.End.Row, row =>
        {
            var colType = sheet.Cells[row, col].Text;
            var colName = sheet.Cells[row, col + 1].Text;

            if (colType is null || colName is null) return;
            if (!colType.Equals("A")) return;
            
            results.Add(colName);
        });
        
        return Task.FromResult(results.Distinct().ToList());
    }

    public void Dispose()
    {
        Book.Dispose();
        GC.Collect();
    }
}