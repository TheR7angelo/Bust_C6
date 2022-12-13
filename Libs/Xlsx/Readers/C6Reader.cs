using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace Libs.Xlsx.Readers;

// var id = Environment.CurrentManagedThreadId;
// var sheetName = worksheet!.Name;

public class C6Reader : Reader, IDisposable
{
    private const string FieldEntryName = "Saisies terrain";
    private const string BasesName = "Bases";
    private const string PictureName = "Photos";

    private ExcelWorksheet? FieldEntry { get; }
    private ExcelWorksheet? Picture { get; }
    private List<ExcelWorksheet?> Exports { get; }

    private List<ExcelWorksheet?> FieldEntrys { get; }

    private ExcelDrawings Drawings { get; }
    
    public C6Reader(string file) : base(file)
    {
        FieldEntry = Book.Workbook.Worksheets[FieldEntryName];
        Picture = Book.Workbook.Worksheets[PictureName];
        Exports = GetExports();

        FieldEntrys = new List<ExcelWorksheet?>(Exports);
        FieldEntrys.Insert(0, FieldEntry);

        Drawings = Picture.Drawings;
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
            var rowMax = worksheet!.Dimension.End.Row;
            var max = rowMax;
            
            for (var row = rowMax - 1; row > 8; row--)
            {
                var name = worksheet.Cells[row, 1].Value;
                if (name is null) continue;
                var nameStr = name.ToString();

                var xname = string.Empty;
                if (nameStr![0].Equals('0')) xname = nameStr[1..];

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

    public Task<IEnumerable<string>> CleanPicture(List<string> app, int insee)
    {
        var rowMax = Picture!.Dimension.End.Row;
        var uris = new List<string>();

        for (var row = rowMax - 1; row > 8; row--)
        {
            var name = Picture.Cells[row, 1].Value;
            if (name is null) continue;

            var nameStr = name.ToString()!.Trim();
            if (nameStr.Equals(string.Empty)) continue;

            nameStr = nameStr.Split('_')[0];

            var xname = nameStr;
            if (xname[0].Equals('0')) xname = xname[1..];

            if (app.Contains(nameStr) || app.Contains(xname)) continue;

            for (var col = 0; col < 4; col++)
            {
                var us = DeletePicture(row, col);
                if (us is not null) uris.AddRange(us);
            }
            Picture.DeleteRow(row-1, 2);
        }

        return Task.FromResult<IEnumerable<string>>(uris);
    }
    
    #endregion

    #region Function

    private IEnumerable<string>? DeletePicture(int row, int col)
    {
        var pics = GetPicture(row-1, col);
        if (!pics.Any()) pics = GetPicture(row-2, col);

        if (!pics.Any()) return null;
        
        var uris = new List<string>();
        foreach (var pic in pics)
        {
            var uri = GetPictureUri(pic as ExcelPicture);
            if (uri is not null) uris.Add(uri);
            
            Drawings.Remove(pic);
        }

        return uris;
    }

    private List<ExcelDrawing> GetPicture(int row, int col) 
        => Drawings.Where(s => s.From.Row.Equals(row-1) && s.From.Column.Equals(col)).ToList();

    private static string? GetPictureUri(ExcelPicture? picture)
    {
        var uri = picture?.GetType().GetInterface("IPictureContainer")?.GetProperty("UriPic")?.GetValue(picture, null) as Uri;
        return uri?.OriginalString;
    }

    #endregion


    public void Dispose()
    {
        Book.Dispose();
        GC.Collect();
    }
}