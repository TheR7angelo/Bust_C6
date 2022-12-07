using OfficeOpenXml;

namespace Libs.Xlsx.Readers;

public class Reader
{
    protected internal ExcelPackage Book { get; }
    
    public Reader(string file)
    {
        Book = new ExcelPackage(file);
    }
}