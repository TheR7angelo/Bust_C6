using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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

        await Parallel.ForEachAsync(FieldEntrys, (worksheet, token) =>
        {
            var rowMax = worksheet!.Dimension.End.Row;
            var max = rowMax;
            
            for (var row = rowMax - 1; row > 8; row--)
            {
                var name = worksheet.Cells[row, 1].Value;
                if (name is null) continue;
                var nameStr = name.ToString();

                var xname = string.Empty;
                if (nameStr[0].Equals('0')) xname = nameStr[1..];

                if (app.Contains(nameStr) || app.Contains(xname))
                {
                    max = row - 1;
                    continue;
                }

                var deleteRow = Math.Abs(max - row + 1);
                worksheet.DeleteRow(row, deleteRow);
                max = row - 1;
            }
            return default;
        }); 
        // foreach (var worksheet in FieldEntrys)
        // {
        //     var rowMax = worksheet!.Dimension.End.Row;
        //     var max = rowMax;
        //     
        //     for (var row = rowMax - 1; row > 8; row--)
        //     {
        //         var name = worksheet.Cells[row, 1].Value;
        //         if (name is null) continue;
        //         var nameStr = name.ToString();
        //
        //         var xname = string.Empty;
        //         if (nameStr[0].Equals('0')) xname = nameStr[1..];
        //
        //         if (app.Contains(nameStr) || app.Contains(xname))
        //         {
        //             max = row - 1;
        //             continue;
        //         }
        //
        //         var deleteRow = Math.Abs(max - row + 1);
        //         worksheet.DeleteRow(row, deleteRow);
        //         max = row - 1;
        //     }
        // }
    }

    public async Task CleanBackgroud()
    {
        if (FieldEntrys.Count.Equals(0)) return;

        await Parallel.ForEachAsync(Exports, (worksheet, _) =>
        {
            var beige = ColorTranslator.FromHtml("#FFCC99");
            var transparent = Color.Transparent;

            var current = Color.Crimson; 
            
            int? min = null;
            var rowMax = worksheet!.Dimension.End.Row;

            for (var row = 9; row < rowMax; row++)
            {
                var name = worksheet.Cells[row, 1].Value;
                if (name is null) continue;

                if (min is null) min = row;
                else
                {
                    current = current switch
                    {
                        { } c when c.Equals(beige) => transparent,
                        _ => beige
                    };
                    try
                    {
                        worksheet.Cells[(int)min, 1, row-1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    }
                    catch (Exception)
                    {
                        // pass
                    }
                    
                    worksheet.Cells[(int)min, 1, row-1, 1].Style.Fill.BackgroundColor.SetColor(current);
                    min = row;
                }
                
            }

            return default;
        });
    }

    public Task CleanPicture(List<string> app)
    {
        var rowMax = Picture!.Dimension.End.Row;
        var delete = false; 
        
        for (var row = rowMax - 1; row > 1; row--)
        {
            var name = Picture.Cells[row, 1].Value;
            if (name is null && !delete) continue;
            if (delete)
            {
                delete = false;
                for (var i = 1; i <= 4; i++) DeletePicture(row, i);
                
                Picture.DeleteRow(row, 2);
            }
            else
            {
                var nameStr = name!.ToString()!;
                nameStr = nameStr.Split('_')[0];
                var xname = string.Empty;
                if (nameStr[0].Equals('0')) xname = nameStr[1..];
                
                if (app.Contains(nameStr) || app.Contains(xname)) continue;

                delete = true;
            }
        }

        return Task.CompletedTask;
    }
    
    #endregion

    #region Function

    private void DeletePicture(int row, int col)
    {
        var pictures = Picture!.Drawings;
        var pics = pictures.Where(s => s.From.Row.Equals(row) && s.From.Column.Equals(col)).ToList();
        if (!pics.Any()) return;

        foreach (var pic in pics)
        {
            Picture.Drawings.Remove(pic);
        }
    }

    #endregion
    
    
    
    
    
    
    
    
    
    
    
    
    
}