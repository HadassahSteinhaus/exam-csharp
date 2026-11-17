using System;
using System.Collections.Generic;

public static class ReportPrinter
{
    public static void Print(ReportData data)
    {
        List<string> readerIds = new List<string>();
        Dictionary<string, string> readerNames = new Dictionary<string, string>();
        Dictionary<string, int> readerFines = new Dictionary<string, int>();
        Dictionary<string, int> lateLoans = new Dictionary<string, int>();
        Dictionary<string, int> bookCounts = new Dictionary<string, int>();
        List<string> flaggedReaders = new List<string>();

        CalculateData(data, readerIds, readerNames, readerFines, lateLoans, bookCounts, flaggedReaders);

        PrintFines(readerIds, readerNames, readerFines);
        PrintFlagged(flaggedReaders, readerNames);
        PrintErrors(data.Errors);
        PrintTotal(readerIds, readerFines);
        PrintTopBooks(bookCounts);
        PrintAtRisk(readerIds, readerNames, lateLoans);
    }

    static void CalculateData(
        ReportData data,
        List<string> readerIds,
        Dictionary<string, string> readerNames,
        Dictionary<string, int> readerFines,
        Dictionary<string, int> lateLoans,
        Dictionary<string, int> bookCounts,
        List<string> flaggedReaders)
    {
        for (int i = 0; i < data.Loans.Count; i++)
        {
            Loan loan = data.Loans[i];
            AddReader(loan, readerIds, readerNames, readerFines, lateLoans);

            loan.Fine = Fine.Calculate(loan, data.ReportDay);
            readerFines[loan.ReaderId] += loan.Fine;
            if (loan.Fine > 0) lateLoans[loan.ReaderId]++;

            AddBook(loan.Book, bookCounts);
            AddFlaggedReader(loan, data.ReportDay, flaggedReaders);
        }
    }

    static void AddReader(
        Loan loan,
        List<string> readerIds,
        Dictionary<string, string> readerNames,
        Dictionary<string, int> readerFines,
        Dictionary<string, int> lateLoans)
    {
        if (readerFines.ContainsKey(loan.ReaderId)) return;

        readerIds.Add(loan.ReaderId);
        readerNames[loan.ReaderId] = loan.Name;
        readerFines[loan.ReaderId] = 0;
        lateLoans[loan.ReaderId] = 0;
    }

    static void AddBook(string book, Dictionary<string, int> bookCounts)
    {
        if (!bookCounts.ContainsKey(book)) bookCounts[book] = 0;
        bookCounts[book]++;
    }

    static void AddFlaggedReader(Loan loan, int reportDay, List<string> flaggedReaders)
    {
        bool shouldFlag = loan.Type == "teacher" && loan.ReturnDay == null && reportDay - loan.LoanDay > 60;
        if (shouldFlag && !flaggedReaders.Contains(loan.ReaderId)) flaggedReaders.Add(loan.ReaderId);
    }

    static void PrintFines(List<string> ids, Dictionary<string, string> names, Dictionary<string, int> fines)
    {
        Console.WriteLine("FINES");
        for (int i = 0; i < ids.Count; i++)
            Console.WriteLine(ids[i] + "," + names[ids[i]] + "," + fines[ids[i]]);
    }

    static void PrintFlagged(List<string> ids, Dictionary<string, string> names)
    {
        Console.WriteLine("FLAGGED");
        for (int i = 0; i < ids.Count; i++) Console.WriteLine(ids[i] + "," + names[ids[i]]);
    }

    static void PrintErrors(List<string> errors)
    {
        Console.WriteLine("ERRORS");
        for (int i = 0; i < errors.Count; i++) Console.WriteLine(errors[i]);
    }

    static void PrintTotal(List<string> ids, Dictionary<string, int> fines)
    {
        int total = 0;
        for (int i = 0; i < ids.Count; i++) total += fines[ids[i]];
        Console.WriteLine("TOTAL," + total);
    }

    static void PrintTopBooks(Dictionary<string, int> bookCounts)
    {
        List<string> books = new List<string>(bookCounts.Keys);
        SortBooks(books, bookCounts);

        Console.WriteLine("TOP_BOOKS");
        Console.WriteLine("book,count");
        int amount = books.Count < 3 ? books.Count : 3;
        for (int i = 0; i < amount; i++) Console.WriteLine(books[i] + "," + bookCounts[books[i]]);
    }

    static void SortBooks(List<string> books, Dictionary<string, int> bookCounts)
    {
        for (int i = 0; i < books.Count; i++)
        {
            for (int j = i + 1; j < books.Count; j++)
            {
                bool moreLoans = bookCounts[books[j]] > bookCounts[books[i]];
                bool sameLoans = bookCounts[books[j]] == bookCounts[books[i]];
                bool comesFirst = StringComparer.Ordinal.Compare(books[j], books[i]) < 0;

                if (moreLoans || (sameLoans && comesFirst))
                {
                    string temp = books[i];
                    books[i] = books[j];
                    books[j] = temp;
                }
            }
        }
    }

    static void PrintAtRisk(List<string> ids, Dictionary<string, string> names, Dictionary<string, int> lateLoans)
    {
        Console.WriteLine("AT_RISK");
        for (int i = 0; i < ids.Count; i++)
            if (lateLoans[ids[i]] > 2) Console.WriteLine(ids[i] + "," + names[ids[i]]);
    }
}
