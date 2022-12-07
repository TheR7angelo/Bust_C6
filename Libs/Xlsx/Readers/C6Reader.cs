using OfficeOpenXml;

namespace Libs.Xlsx.Readers;

public class C6Reader : Reader
{
    private const string FieldEntryName = "Saisies terrain";
    private const string BasesName = "Bases";
    private const string PictureName = "Photos";

    public ExcelWorksheet? FieldEntry { get; }
    public ExcelWorksheet? Picture { get; }
    public ExcelWorksheet? LastExport { get; }

    public C6Reader(string file) : base(file)
    {
        FieldEntry = Book.Workbook.Worksheets[FieldEntryName];
        Picture = Book.Workbook.Worksheets[PictureName];
        GetLastExport();
        // LastExport = GetLastExport();
    }

    private void GetLastExport()
    {
        var removes = new List<string> { FieldEntryName, BasesName, PictureName };
        var worksheets = Book.Workbook.Worksheets.Select(x => x.Name).Except(removes);
        

    }
    
    // public void WriteCiyName(string city)
    // {
    //     Book.
    // }
}