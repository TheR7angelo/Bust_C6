using OfficeOpenXml;

namespace Libs.Xlsx.Reader;

public class Reader
{
    protected ExcelPackage Book { get; }
    
    public Reader(string file)
    {
        Book = new ExcelPackage(file);
    }
}