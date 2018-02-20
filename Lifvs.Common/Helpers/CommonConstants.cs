using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Helpers
{
    public static class CommonConstants
    {
        public const string RoleUser = "USER";

        public const double UserCodeExpireTime = 10;
        public const double CartExpireTime = 5;
        public const int Vat1InPercentage = 25;
        public const int Vat2InPercentage = 12;



        public static string GenerateRandomCode()
        {
            string code = string.Empty;
            Random rand = new Random();

            for (int i = 0; i <= 4; i++)
            {
                code += Convert.ToString(rand.Next(0, 9));
            }
            return code;
        }
    }
}
