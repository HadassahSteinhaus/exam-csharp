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

        if (loan.Type == "student")
        {
            if (days <= 15) return 0;
            return Math.Min(30, days - 14);
        }

        if (loan.Type == "guest")
        {
            if (days <= 7) return 0;
            return (int)Math.Ceiling((days - 7) * 2.5);
        }

        return 0;
    }
}
