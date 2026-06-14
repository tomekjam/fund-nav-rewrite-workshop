using System;
using System.Security.Cryptography;
using System.Text;

namespace FundNav.Legacy
{
    // Produces a short "client reference" used only in audit log lines.
    //
    // SMELL (security review): MD5 is a broken hash. It must NOT be carried
    // over to the new system. Because this value never feeds the NAV / fee
    // numbers, replacing it (e.g. with SHA-256) does not change any
    // characterization output -- a clean "do not port the weak pattern" case.
    public static class LegacyHash
    {
        public static string ClientRef(string email, string salt)
        {
            using (var md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(salt + email));
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString().Substring(0, 12);
            }
        }
    }
}
