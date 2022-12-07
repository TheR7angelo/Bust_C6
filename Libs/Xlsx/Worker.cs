using OfficeOpenXml;

namespace Libs.Xlsx;

public class Worker
{
    public Worker()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }
}