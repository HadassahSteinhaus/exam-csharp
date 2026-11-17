using System;
using System.Text;

public static class ReportRunner
{
    public static void Run(string filePath)
    {
        Console.OutputEncoding = new UTF8Encoding(false);

        ReportData data = LoanReader.Read(filePath);
        ReportPrinter.Print(data);
    }
}
