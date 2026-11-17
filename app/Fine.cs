using System;

public class Fine
{

    public static int Print(List<string[]> rows, reportDay)
    {
        console.WriteLine("Fines:")
        Fines(rows, reportDay);

    }


    public static void Fines(List<string[]> rows, reportDay)
    {
        //foundReaderId=rows.Find(a=>(a.rea))

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null)
            {
                switch (rows[i].type)
                {
                    case 'teacher': return teacherFine(rows[i], reportDay);
                    case 'student': return gestFine(rows[i], reportDay);
                    case 'gest': return studentFine(rows[i], reportDay);
                }
            }

        }
    }
    public static int studentFine(row, reportDay)
    {
        if (row.return_day == 0)
        {
            row.return_day = reportDay;
        }
        int days = row.return_day - row.loan_day;
        int sum = 0
        if (days > 14)
        {
            sum = days - 14;
            sum = sum > 30 ? 30 : sum;
            console.WriteLine(row.reader_id + " " + row.name + "" + sum)
              return sum;

        }

        return 0;
    }


    public static int teacherFine(row, reportDay)
    {
        if (row.return_day == 0 && reportDay - row.loan_day > 60)
        {
            console.WriteLine(row.reader_id + " " + row.name + "" + 0)
            return 0;
        }

    }
    public static int gestFine(row, reportDay)
    {
        if (row.return_day == 0)
        {
            row.return_day = reportDay;
        }
        int days = row.return_day - row.loan_day;
        int sum = 0
        if (days > 7)
        {
            sum = (days - 7) * 2.5;
            console.WriteLine(row.reader_id + " " + row.name + "" + sum)
            return sum;
        }

        return 0;

    }

}
