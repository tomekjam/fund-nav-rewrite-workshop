using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FundNav.Legacy
{
    // CLI entry point. Computes NAV per unit + daily management fee for every
    // (share class, valuation date) and writes two CSV files that the
    // characterization runner diffs against golden/.
    public static class Program
    {
        public static int Main(string[] args)
        {
            string outDir = args.Length > 0 ? args[0] : "out";
            Directory.CreateDirectory(outDir);

            string baseDir = AppContext.BaseDirectory;
            string configPath = Path.Combine(baseDir, "appsettings.json");
            string connectionString;
            string auditSalt;
            using (var doc = JsonDocument.Parse(File.ReadAllText(configPath)))
            {
                connectionString = doc.RootElement
                    .GetProperty("ConnectionStrings").GetProperty("FundNav").GetString();
                auditSalt = doc.RootElement.GetProperty("AuditSalt").GetString();
            }
            // Allow override for CI / containers.
            string envConn = Environment.GetEnvironmentVariable("FUNDNAV_CONNECTION");
            if (!string.IsNullOrEmpty(envConn))
            {
                connectionString = envConn;
            }

            var service = new NavService(connectionString, auditSalt);
            List<NavResult> results = service.ComputeAll();

            WriteNavCsv(Path.Combine(outDir, "nav.csv"), results);
            WriteFeesCsv(Path.Combine(outDir, "fees.csv"), results);

            Console.WriteLine("Computed " + results.Count + " rows.");
            Console.WriteLine("Wrote " + Path.Combine(outDir, "nav.csv"));
            Console.WriteLine("Wrote " + Path.Combine(outDir, "fees.csv"));
            return 0;
        }

        private static void WriteNavCsv(string path, List<NavResult> results)
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("ShareClassId,AsOfDate,UnitsInIssue,NetAssetValue,NavPerUnit\n");
            foreach (var x in results)
            {
                sb.Append(x.ShareClassId).Append(',')
                  .Append(x.AsOfDate.ToString("yyyy-MM-dd", ci)).Append(',')
                  .Append(x.UnitsInIssue.ToString("0", ci)).Append(',')
                  .Append(x.NetAssetValue.ToString("F2", ci)).Append(',')
                  .Append(x.NavPerUnit.ToString("F6", ci)).Append('\n');
            }
            File.WriteAllText(path, sb.ToString());
        }

        private static void WriteFeesCsv(string path, List<NavResult> results)
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("ShareClassId,AsOfDate,NetAssetValue,ManagementFeeBps,DailyManagementFee\n");
            foreach (var x in results)
            {
                sb.Append(x.ShareClassId).Append(',')
                  .Append(x.AsOfDate.ToString("yyyy-MM-dd", ci)).Append(',')
                  .Append(x.NetAssetValue.ToString("F2", ci)).Append(',')
                  .Append(x.ManagementFeeBps.ToString(ci)).Append(',')
                  .Append(x.DailyManagementFee.ToString("F2", ci)).Append('\n');
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}
