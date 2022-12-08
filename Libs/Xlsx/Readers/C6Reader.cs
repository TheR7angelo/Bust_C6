using OfficeOpenXml;

namespace Libs.Xlsx.Readers;

// var id = Environment.CurrentManagedThreadId;
// var sheetName = worksheet!.Name;

public class C6Reader : Reader
{
    private const string FieldEntryName = "Saisies terrain";
    private const string BasesName = "Bases";
    private const string PictureName = "Photos";

    public ExcelWorksheet? FieldEntry { get; }
    public ExcelWorksheet? Picture { get; }
    public List<ExcelWorksheet?> Exports { get; }
    
    public List<ExcelWorksheet?> FieldEntrys { get; }

    public C6Reader(string file) : base(file)
    {
        FieldEntry = Book.Workbook.Worksheets[FieldEntryName];
        Picture = Book.Workbook.Worksheets[PictureName];
        Exports = GetExports();

        FieldEntrys = new List<ExcelWorksheet?>(Exports);
        FieldEntrys.Insert(0, FieldEntry);
    }

    #region Actions

    private List<ExcelWorksheet?> GetExports()
    {
        var removes = new List<string> { FieldEntryName, BasesName, PictureName };
        var worksheets = Book.Workbook.Worksheets.Select(x => x.Name.Trim()).Except(removes);
        worksheets = GetExportsNames(worksheets);

        return worksheets.Select(worksheet => Book.Workbook.Worksheets[worksheet]).ToList()!;
    }

    private static IEnumerable<string> GetExportsNames(IEnumerable<string> worksheetsNames) 
        => worksheetsNames.Where(worksheet => worksheet.ToLower().Contains("export") && worksheet.Any(char.IsDigit)).ToList();

    #endregion

    #region Task

    public async Task Writecartridge(int insee, string? city)
    {
        await Parallel.ForEachAsync(Exports, (worksheet, _) =>
        {
            worksheet!.Cells["C3"].Value = city;
            worksheet.Cells["G3"].Value = insee;

            return default;
        });
    }

    public async Task CleanFields(List<string> app, int insee)
    {
        if (FieldEntrys.Count.Equals(0)) return;

        await Parallel.ForEachAsync(FieldEntrys, (worksheet, _) =>
        {
            int? max = null;
            var rowMax = worksheet.Dimension.End.Row;

            for (var row = rowMax - 1; row > 8; row--)
            {
                var name = worksheet.Cells[row, 1].Value;
                if (name is null) continue;

                var nameStr = name.ToString()!;
                var xname = string.Empty;
                if (nameStr[0].Equals('0')) xname = nameStr[1..];

                if (app.Contains(nameStr) || app.Contains(xname)) continue;

                if (max is null) max = row;
                else
                {
                    var nbrDelteRow = (int)max - row;
                    worksheet.DeleteRow(row, nbrDelteRow);
                    max = null;
                }
            }

            return default;
        });
    }

    #endregion
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}