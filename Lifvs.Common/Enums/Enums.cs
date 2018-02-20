namespace Lifvs.Common.Enums
{
    public static class Enums
    {
        public enum Roles
        {
            SuperAdmin = 1,
            Admin = 2,
            Employee = 3
        };

        public enum CardType
        {
            Unknown = 0,
            MasterCard = 1,
            VISA = 2,
            Amex = 3,
            Discover = 4,
            DinersClub = 5,
            JCB = 6,
            enRoute = 7,
            Maestro = 8
        }
        public static string MaskCardDigits(string cardNumber)
        {
            var maskedNumber = string.Empty;

            if (cardNumber.Contains(" "))
            {
                maskedNumber = "XXXX XXXX XXXX " + cardNumber.Remove(0, 15);
            }
            else if (cardNumber.Contains("-"))
            {
                maskedNumber = "XXXX-XXXX-XXXX-" + cardNumber.Remove(0, 15);
            }
            else
            {
                maskedNumber = "XXXXXXXXXXXX" + cardNumber.Remove(0, 12);
            }
            return maskedNumber;
        }
    }
}
