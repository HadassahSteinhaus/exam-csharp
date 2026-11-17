using System;
using System.Collections.Generic;
using System.Linq;

public static class ReportPrinter
{
    public static void Print(ReportData data)
    {
        CalculateFines(data.Loans, data.ReportDay);

        List<ReaderSummary> readers = GetReaderSummaries(data.Loans);
        List<ReaderSummary> flaggedReaders = GetFlaggedReaders(data.Loans, data.ReportDay);

        PrintFines(readers);
        PrintFlagged(flaggedReaders);
        PrintErrors(data.Errors);
        Console.WriteLine("TOTAL," + readers.Sum(reader => reader.Fine));
        PrintTopBooks(data.Loans);
        PrintAtRisk(readers);
    }

    static void CalculateFines(List<Loan> loans, int reportDay)
    {
        foreach (Loan loan in loans)
            loan.Fine = Fine.Calculate(loan, reportDay);
    }

    static List<ReaderSummary> GetReaderSummaries(List<Loan> loans)
    {
        return loans
            .GroupBy(loan => loan.ReaderId)
            .Select(group => new ReaderSummary
            {
                ReaderId = group.Key,
                Name = group.First().Name,
                Fine = group.Sum(loan => loan.Fine),
                LateLoans = group.Count(loan => loan.Fine > 0)
            })
            .ToList();
    }

    static List<ReaderSummary> GetFlaggedReaders(List<Loan> loans, int reportDay)
    {
        return loans
            .Where(loan => loan.Type == "teacher" && loan.ReturnDay == null && reportDay - loan.LoanDay > 60)
            .GroupBy(loan => loan.ReaderId)
            .Select(group => new ReaderSummary
            {
                ReaderId = group.Key,
                Name = group.First().Name
            })
            .ToList();
    }

    static void PrintFines(List<ReaderSummary> readers)
    {
        Console.WriteLine("FINES");
        foreach (ReaderSummary reader in readers)
            Console.WriteLine(reader.ReaderId + "," + reader.Name + "," + reader.Fine);
    }

    static void PrintFlagged(List<ReaderSummary> readers)
    {
        Console.WriteLine("FLAGGED");
        foreach (ReaderSummary reader in readers)
            Console.WriteLine(reader.ReaderId + "," + reader.Name);
    }

    static void PrintErrors(List<string> errors)
    {
        Console.WriteLine("ERRORS");
        foreach (string error in errors) Console.WriteLine(error);
    }

    static void PrintTopBooks(List<Loan> loans)
    {
        List<IGrouping<string, Loan>> books = loans
            .GroupBy(loan => loan.Book)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Take(3)
            .ToList();

        Console.WriteLine("TOP_BOOKS");
        Console.WriteLine("book,count");
        foreach (IGrouping<string, Loan> book in books)
            Console.WriteLine(book.Key + "," + book.Count());
    }

    static void PrintAtRisk(List<ReaderSummary> readers)
    {
        Console.WriteLine("AT_RISK");
        foreach (ReaderSummary reader in readers.Where(reader => reader.LateLoans > 2))
            Console.WriteLine(reader.ReaderId + "," + reader.Name);
    }
}

public class ReaderSummary
{
    public string ReaderId { get; set; }
    public string Name { get; set; }
    public int Fine { get; set; }
    public int LateLoans { get; set; }
}
