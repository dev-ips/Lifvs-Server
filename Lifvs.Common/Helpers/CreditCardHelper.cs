using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Lifvs.Common.Enums.Enums;

namespace Lifvs.Common.Helpers
{
    public class CreditCardHelper
    {
        string errorMessage = string.Empty;

        public static bool IsCardNumberValid(string cardNumber, out string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                string normalizeCardNumber = NormalizeCardNumber(cardNumber);
                int i, checkSum = 0;

                // Compute checksum of every other digit starting from right-most digit
                for (i = normalizeCardNumber.Length - 1; i >= 0; i -= 2)
                    checkSum += (normalizeCardNumber[i] - '0');

                // Now take digits not included in first checksum, multiple by two,
                // and compute checksum of resulting digits
                for (i = normalizeCardNumber.Length - 2; i >= 0; i -= 2)
                {
                    int val = ((normalizeCardNumber[i] - '0') * 2);
                    while (val > 0)
                    {
                        checkSum += (val % 10);
                        val /= 10;
                    }
                }

                // Number is valid if sum of both checksums MOD 10 equals 0
                if ((checkSum % 10) == 0)
                {
                    var cardType =  GetCardType(normalizeCardNumber);
                    errorMessage = string.Empty;
                    return true;
                }
                else
                {
                    GetCardType(normalizeCardNumber);

                    errorMessage = "Card number is not valid!";
                    return false;
                }
            }
            else
            {
                errorMessage = "Card number is not valid!";
                return false;
            }
        }

        public static string NormalizeCardNumber(string cardNumber)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in cardNumber)
            {
                if (Char.IsDigit(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }


        private class CardTypeInfo
        {
            public CardTypeInfo(string regEx, int length, CardType type)
            {
                RegEx = regEx;
                Length = length;
                Type = type;
            }

            public string RegEx { get; set; }
            public int Length { get; set; }
            public CardType Type { get; set; }
        }

        private static CardTypeInfo[] _cardTypeInfo =
        {
            new CardTypeInfo("^(22|23|24|25|26|27|51|52|53|54|55)", 16, CardType.MasterCard),
            new CardTypeInfo("^(5|6)", 13, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 14, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 15, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 16, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 17, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 18, CardType.Maestro),
            new CardTypeInfo("^(5|6)", 19, CardType.Maestro),
            new CardTypeInfo("^(4)", 13, CardType.VISA),
            new CardTypeInfo("^(4)", 14, CardType.VISA),
            new CardTypeInfo("^(4)", 15, CardType.VISA),
            new CardTypeInfo("^(4)", 16, CardType.VISA),
            new CardTypeInfo("^(4)", 17, CardType.VISA),
            new CardTypeInfo("^(4)", 18, CardType.VISA),
            new CardTypeInfo("^(4)", 19, CardType.VISA),
            new CardTypeInfo("^(34|37)", 15, CardType.Amex),
            new CardTypeInfo("^(6011|6440|6599)", 16, CardType.Discover),
            new CardTypeInfo("^(30|36|38)",14, CardType.DinersClub),
            new CardTypeInfo("^(3528|3589)", 16, CardType.JCB),
            new CardTypeInfo("^(3528|3589)", 19, CardType.JCB),
            new CardTypeInfo("^(2014|2149)", 15, CardType.enRoute),
        };

        public static CardType GetCardType(string cardNumber)
        {
            foreach (CardTypeInfo info in _cardTypeInfo)
            {
                if (cardNumber.Length == info.Length &&
                    Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type;
            }

            return CardType.Unknown;
        }
        public static string GetCardTokens(string cardType)
        {
            var token = string.Empty;
            switch (cardType.ToLower().Replace(" ",""))
            {
                case "visa":
                    token = "tok_visa";
                    break;
                case "visa(debit)":
                    token = "tok_visa_debit";
                    break;
                case "mastercard":
                    token = "tok_mastercard";
                    break;
                case "mastercard(2-series)":
                    token = "tok_mastercard";
                    break;
                case "mastercard(debit)":
                    token = "tok_mastercard_debit";
                    break;
                case "mastercard(prepaid)":
                    token = "tok_mastercard_prepaid";
                    break;
                case "americanexpress":
                    token = "tok_amex";
                    break;
                case "discover":
                    token = "tok_discover";
                    break;
                case "dinersclub":
                    token = "tok_diners";
                    break;
                case "jcb":
                    token = "tok_jcb";
                    break;
                default:
                    token = "";
                    break;
            }
            return token;
        }
    }
}
