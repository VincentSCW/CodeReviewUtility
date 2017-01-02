using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Utility.Library.Misc
{
    public class PasswordProtector
    {
        // Create byte array for additional entropy when using Protect method. 
        static byte[] s_aditionalEntropy = { 9, 8, 7, 6, 5 };

        public static string Protect(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetString(Protect(encoding.GetBytes(data)));
        }

        public static string Unprotect(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetString(Unprotect(encoding.GetBytes(data)));
        }

        public static byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted 
                //  only by the same current user. 
                return ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                System.Diagnostics.Debug.WriteLine("Data was not encrypted. An error occurred.");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
        }

        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser. 
                return ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                System.Diagnostics.Debug.WriteLine("Data was not decrypted. An error occurred.");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
        }

    }
}
