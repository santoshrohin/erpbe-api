namespace ErpBE.Infrastructure.Services;

/// <summary>
/// Converts numbers to words (for invoice amount in words)
/// </summary>
public static class NumberToWordsConverter
{
    private static readonly string[] Units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    private static readonly string[] Teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
    private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

    public static string ConvertToWords(double amount)
    {
        try
        {
            // Split into rupees and paise
            int rupees = (int)Math.Floor(amount);
            int paise = (int)Math.Round((amount - rupees) * 100);

            string result = "";

            if (rupees == 0 && paise == 0)
            {
                return "Zero Rupees Only";
            }

            if (rupees > 0)
            {
                result = ConvertRupeesToWords(rupees) + " Rupees";
            }

            if (paise > 0)
            {
                if (rupees > 0)
                    result += " and ";
                result += ConvertRupeesToWords(paise) + " Paise";
            }

            return result + " Only";
        }
        catch
        {
            return "Amount Conversion Error";
        }
    }

    private static string ConvertRupeesToWords(int number)
    {
        if (number == 0)
            return "";

        string result = "";

        // Crore (10,000,000)
        int crore = number / 10000000;
        if (crore > 0)
        {
            result += ConvertBelowThousand(crore) + " Crore ";
            number %= 10000000;
        }

        // Lakh (100,000)
        int lakh = number / 100000;
        if (lakh > 0)
        {
            result += ConvertBelowThousand(lakh) + " Lakh ";
            number %= 100000;
        }

        // Thousand (1,000)
        int thousand = number / 1000;
        if (thousand > 0)
        {
            result += ConvertBelowThousand(thousand) + " Thousand ";
            number %= 1000;
        }

        // Hundred (100)
        int hundred = number / 100;
        if (hundred > 0)
        {
            result += Units[hundred] + " Hundred ";
            number %= 100;
        }

        // Below hundred
        if (number > 0)
        {
            result += ConvertBelowHundred(number);
        }

        return result.Trim();
    }

    private static string ConvertBelowThousand(int number)
    {
        string result = "";

        int hundred = number / 100;
        if (hundred > 0)
        {
            result += Units[hundred] + " Hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            result += ConvertBelowHundred(number);
        }

        return result.Trim();
    }

    private static string ConvertBelowHundred(int number)
    {
        if (number < 10)
            return Units[number];
        else if (number < 20)
            return Teens[number - 10];
        else
        {
            int ten = number / 10;
            int unit = number % 10;
            return Tens[ten] + (unit > 0 ? " " + Units[unit] : "");
        }
    }
}

