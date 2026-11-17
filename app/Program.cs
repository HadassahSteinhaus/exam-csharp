using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        string path = args.Length > 0 ? args[0] : FindSample();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        int reportDay = ReadReportDay(lines);
        var loans = new List<Loan>();
        var errors = new List<string>();

        for (int i = 2; i < lines.Length; i++)
        {
            string raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string[] fields = raw.Split(',');
            if (!TryReadLoan(raw, fields, out Loan loan))
            {
                errors.Add(raw);
                continue;
            }

            loan.Fine = Fine.Calculate(loan, reportDay);
            loans.Add(loan);
        }

        PrintReport(loans, errors, reportDay);
    }

    static int ReadReportDay(string[] lines)
    {
        return int.Parse(lines[0].Split(':')[1].Trim());
    }

    static bool TryReadLoan(string raw, string[] fields, out Loan loan)
    {
        loan = null;
        if (fields.Length != 6) return false;

        for (int i = 0; i < fields.Length; i++) fields[i] = fields[i].Trim();
        if (fields[0] == "" || fields[1] == "" || fields[2] == "" || fields[3] == "" || fields[4] == "") return false;
        if (fields[2] != "student" && fields[2] != "teacher" && fields[2] != "guest") return false;
        if (!int.TryParse(fields[4], out int loanDay)) return false;

        int? returnDay = null;
        if (fields[5] != "")
        {
            if (!int.TryParse(fields[5], out int parsedReturnDay)) return false;
            returnDay = parsedReturnDay;
            if (returnDay < loanDay) return false;
        }

        loan = new Loan
        {
            Raw = raw,
            ReaderId = fields[0],
            Name = fields[1],
            Type = fields[2],
            Book = fields[3],
            LoanDay = loanDay,
            ReturnDay = returnDay
        };
        return true;
    }

    static void PrintReport(List<Loan> loans, List<string> errors, int reportDay)
    {
        var readers = loans
            .GroupBy(loan => loan.ReaderId)
            .Select(group => new
            {
                ReaderId = group.Key,
                Name = group.First().Name,
                Fine = group.Sum(loan => loan.Fine)
            }).ToList();

        Console.WriteLine("FINES");
        foreach (var reader in readers)
            Console.WriteLine($"{reader.ReaderId},{reader.Name},{reader.Fine}");

        Console.WriteLine("FLAGGED");
        foreach (Loan loan in loans.Where(IsFlagged(reportDay)).GroupBy(x => x.ReaderId).Select(x => x.First()))
            Console.WriteLine($"{loan.ReaderId},{loan.Name}");

        Console.WriteLine("ERRORS");
        foreach (string error in errors) Console.WriteLine(error);

        Console.WriteLine($"TOTAL,{readers.Sum(reader => reader.Fine)}");

        Console.WriteLine("TOP_BOOKS");
        Console.WriteLine("book,count");
        foreach (var book in loans.GroupBy(loan => loan.Book)
                     .OrderByDescending(group => group.Count())
                     .ThenBy(group => group.Key, StringComparer.Ordinal)
                     .Take(3))
            Console.WriteLine($"{book.Key},{book.Count()}");

        Console.WriteLine("AT_RISK");
        foreach (var reader in loans.GroupBy(loan => loan.ReaderId)
                     .Where(group => group.Count(loan => loan.Fine > 0) > 2))
            Console.WriteLine($"{reader.Key},{reader.First().Name}");
    }

    static Func<Loan, bool> IsFlagged(int reportDay)
    {
        return loan => loan.Type == "teacher" && loan.ReturnDay == null && reportDay - loan.LoanDay > 60;
    }

    static string FindSample()
    {
        DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "data", "sample.txt");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return Path.Combine("data", "sample.txt");
    }
}
