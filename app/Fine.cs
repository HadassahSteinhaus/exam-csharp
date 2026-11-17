using System;

public class Loan
{
    public string Raw { get; set; }
    public string ReaderId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Book { get; set; }
    public int LoanDay { get; set; }
    public int? ReturnDay { get; set; }
    public int Fine { get; set; }
}

public static class Fine
{
    public static int Calculate(Loan loan, int reportDay)
    {
        int endDay = loan.ReturnDay ?? reportDay;
        int days = endDay - loan.LoanDay;

        if (loan.Type == "student") return StudentFine(days);
        if (loan.Type == "guest") return GuestFine(days);
        if (loan.Type == "alumna") return AlumnaFine(days);
        return 0;
    }

    static int StudentFine(int days)
    {
        if (days <= 15) return 0;
        int fine = days - 14;
        if (fine > 40) fine = 40;
        return fine;
    }

    static int GuestFine(int days)
    {
        if (days <= 7) return 0;
        return (int)Math.Ceiling((days - 7) * 2.5);
    }

    static int AlumnaFine(int days)
    {
        if (days <= 10) return 0;
        int fine = (int)Math.Ceiling((days - 10) * 1.5);
        if (fine > 25) fine = 25;
        return fine;
    }
}
