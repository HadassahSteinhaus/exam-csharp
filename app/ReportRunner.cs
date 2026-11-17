using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

public static class ReportRunner
{
    [ModuleInitializer]
    public static void Start()
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        string[] args = Environment.GetCommandLineArgs();
        string path = args.Length > 1 ? args[1] : FindSample();
        if (!File.Exists(path)) return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        int reportDay = int.Parse(lines[0].Split(':')[1].Trim());
        List<Loan> loans = new List<Loan>();
        List<string> errors = new List<string>();

        for (int i = 2; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "") continue;
            Loan loan;
            if (TryCreateLoan(lines[i], out loan))
            {
                loan.Fine = Fine.Calculate(loan, reportDay);
                loans.Add(loan);
            }
            else errors.Add(lines[i]);
        }

        PrintReport(loans, errors, reportDay);
    }

    static bool TryCreateLoan(string line, out Loan loan)
    {
        loan = null;
        string[] fields = line.Split(',');
        if (fields.Length != 6) return false;
        for (int i = 0; i < fields.Length; i++) fields[i] = fields[i].Trim();
        if (fields[0] == "" || fields[1] == "" || fields[2] == "" || fields[3] == "" || fields[4] == "") return false;
        if (fields[2] != "student" && fields[2] != "teacher" && fields[2] != "guest") return false;

        int loanDay;
        if (!int.TryParse(fields[4], out loanDay)) return false;
        int? returnDay = null;
        if (fields[5] != "")
        {
            int value;
            if (!int.TryParse(fields[5], out value) || value < loanDay) return false;
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

    static void PrintReport(List<Loan> loans, List<string> errors, int reportDay)
    {
        List<string> ids = new List<string>();
        Dictionary<string, string> names = new Dictionary<string, string>();
        Dictionary<string, int> fines = new Dictionary<string, int>();
        Dictionary<string, int> lateLoans = new Dictionary<string, int>();
        Dictionary<string, int> books = new Dictionary<string, int>();
        List<string> flagged = new List<string>();

        for (int i = 0; i < loans.Count; i++)
        {
            Loan loan = loans[i];
            if (!fines.ContainsKey(loan.ReaderId))
            {
                ids.Add(loan.ReaderId);
                names[loan.ReaderId] = loan.Name;
                fines[loan.ReaderId] = 0;
                lateLoans[loan.ReaderId] = 0;
            }
            fines[loan.ReaderId] += loan.Fine;
            if (loan.Fine > 0) lateLoans[loan.ReaderId]++;
            if (!books.ContainsKey(loan.Book)) books[loan.Book] = 0;
            books[loan.Book]++;
            if (loan.Type == "teacher" && loan.ReturnDay == null && reportDay - loan.LoanDay > 60 && !flagged.Contains(loan.ReaderId)) flagged.Add(loan.ReaderId);
        }

        Console.WriteLine("FINES");
        for (int i = 0; i < ids.Count; i++) Console.WriteLine(ids[i] + "," + names[ids[i]] + "," + fines[ids[i]]);
        Console.WriteLine("FLAGGED");
        for (int i = 0; i < flagged.Count; i++) Console.WriteLine(flagged[i] + "," + names[flagged[i]]);
        Console.WriteLine("ERRORS");
        for (int i = 0; i < errors.Count; i++) Console.WriteLine(errors[i]);

        int total = 0;
        for (int i = 0; i < ids.Count; i++) total += fines[ids[i]];
        Console.WriteLine("TOTAL," + total);
        PrintTopBooks(books);
        Console.WriteLine("AT_RISK");
        for (int i = 0; i < ids.Count; i++) if (lateLoans[ids[i]] > 2) Console.WriteLine(ids[i] + "," + names[ids[i]]);
    }

    static void PrintTopBooks(Dictionary<string, int> books)
    {
        Console.WriteLine("TOP_BOOKS");
        Console.WriteLine("book,count");
        List<string> names = new List<string>(books.Keys);
        for (int i = 0; i < names.Count; i++)
            for (int j = i + 1; j < names.Count; j++)
                if (books[names[j]] > books[names[i]] || (books[names[j]] == books[names[i]] && StringComparer.Ordinal.Compare(names[j], names[i]) < 0))
                {
                    string temp = names[i];
                    names[i] = names[j];
                    names[j] = temp;
                }
        int count = names.Count < 3 ? names.Count : 3;
        for (int i = 0; i < count; i++) Console.WriteLine(names[i] + "," + books[names[i]]);
    }

    static string FindSample()
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
}
