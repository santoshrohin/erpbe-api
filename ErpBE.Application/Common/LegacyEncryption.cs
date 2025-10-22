using Microsoft.VisualBasic;

namespace ErpBE.Application.Common
{
    public static class LegacyEncryption
    {
        public static string Encrypt(string pwd)
        {
            int I, Pos = 0;
            int Len = pwd.Length;
            string STR = "";

            for (I = 0; I < Len; I++)
            {
                char ChrSt = pwd[I];
                int encript = Strings.Asc(ChrSt);
                encript = encript * 20;
                encript = encript / 2;
                encript = encript - 100;

                if (Pos == 0)
                {
                    STR = STR + encript.ToString();
                    Pos++;
                }
                else
                {
                    STR = STR + "-" + encript.ToString();
                }
            }
            return STR;
        }
    }
}
