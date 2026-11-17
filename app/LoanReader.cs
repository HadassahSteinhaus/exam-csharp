using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ReportData
{
    public int ReportDay { get; set; }
    public List<Loan> Loans { get; set; }
    public List<string> Errors { get; set; }
}

public static class LoanReader
{
    public static ReportData Read(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        ReportData data = new ReportData
        {
            ReportDay = ReadReportDay(lines[0]),
            Loans = new List<Loan>(),
            Errors = new List<string>()
        };

        for (int i = 2; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "") continue;

            Loan loan;
            if (TryCreateLoan(lines[i], out loan)) data.Loans.Add(loan);
            else data.Errors.Add(lines[i]);
        }

        return data;
    }

    public static string FindSample()
    {
        DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            string path = Path.Combine(directory.FullName, "data", "sample.txt");
            if (File.Exists(path)) return path;
            directory = directory.Parent;
        }
        return Path.Combine("data", "sample.txt");
    }

    static int ReadReportDay(string line)
    {
        return int.Parse(line.Split(':')[1].Trim());
    }

    static bool TryCreateLoan(string line, out Loan loan)
    {
        loan = null;
        string[] fields = line.Split(',');
        if (fields.Length != 6) return false;

        for (int i = 0; i < fields.Length; i++) fields[i] = fields[i].Trim();
        if (IsMissingRequiredField(fields)) return false;
        if (!IsKnownType(fields[2])) return false;

        int loanDay;
        if (!int.TryParse(fields[4], out loanDay)) return false;

        int? returnDay = null;
        if (fields[5] != "")
        {
            int value;
            if (!int.TryParse(fields[5], out value)) return false;
            if (value < loanDay) return false;
            returnDay = value;
        }

        loan = new Loan
        {
            Raw = line,
            ReaderId = fields[0],
            Name = fields[1],
            Type = fields[2],
            Book = fields[3],
            LoanDay = loanDay,
            ReturnDay = returnDay
        };
        return true;
    }

    static bool IsMissingRequiredField(string[] fields)
    {
        for (int i = 0; i < 5; i++)
            if (fields[i] == "") return true;
        return false;
    }

    static bool IsKnownType(string type)
    {
        return type == "student" || type == "teacher" || type == "guest" || type == "alumna";
    }
}
