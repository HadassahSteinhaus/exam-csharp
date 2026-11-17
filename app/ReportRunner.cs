using System;
using System.Runtime.CompilerServices;
using System.Text;

public static class ReportRunner
{
    [ModuleInitializer]
    public static void Start()
    {
        Console.OutputEncoding = new UTF8Encoding(false);

        string[] arguments = Environment.GetCommandLineArgs();
        string filePath = arguments.Length > 1 ? arguments[1] : LoanReader.FindSample();

        ReportData data = LoanReader.Read(filePath);
        ReportPrinter.Print(data);
    }
}
