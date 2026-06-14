using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace FundNav.Legacy
{
    // The legacy "service": business logic and data access are tangled together.
    // Queries are built by STRING CONCATENATION (classic SQL injection risk --
    // a security review must flag this; the rewrite must use parameters).
    public class NavService
    {
        private readonly string _connectionString;
        private readonly string _auditSalt;

        public NavService(string connectionString, string auditSalt)
        {
            _connectionString = connectionString;
            _auditSalt = auditSalt;
        }

        public List<ShareClassRow> GetShareClasses()
        {
            var list = new List<ShareClassRow>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Ordering matters for the characterization output.
                string sql =
                    "SELECT ShareClassId, FundId, Name, Currency, ManagementFeeBps " +
                    "FROM ShareClass ORDER BY ShareClassId";
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ShareClassRow
                        {
                            ShareClassId = r.GetString(0),
                            FundId = r.GetString(1),
                            Name = r.GetString(2),
                            Currency = r.GetString(3),
                            ManagementFeeBps = r.GetInt32(4),
                        });
                    }
                }
            }
            return list;
        }

        public List<ValuationRow> GetValuations(string shareClassId)
        {
            var list = new List<ValuationRow>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // VULNERABILITY: shareClassId concatenated straight into SQL.
                string sql =
                    "SELECT ShareClassId, AsOfDate, NetAssetValue FROM Valuation " +
                    "WHERE ShareClassId = '" + shareClassId + "' " +
                    "ORDER BY AsOfDate";
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ValuationRow
                        {
                            ShareClassId = r.GetString(0),
                            AsOfDate = r.GetDateTime(1),
                            NetAssetValue = r.GetDecimal(2),
                        });
                    }
                }
            }
            return list;
        }

        // Units in issue = SUM(Subscription.Units) - SUM(Redemption.Units) up to date.
        public decimal GetUnitsInIssue(string shareClassId, DateTime asOf)
        {
            string asOfText = asOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            decimal subs = ScalarSum(
                "SELECT SUM(Units) FROM Subscription " +
                "WHERE ShareClassId = '" + shareClassId + "' " +   // VULNERABILITY: concatenation
                "AND TradeDate <= '" + asOfText + "'");            // VULNERABILITY: concatenation
            decimal reds = ScalarSum(
                "SELECT SUM(Units) FROM Redemption " +
                "WHERE ShareClassId = '" + shareClassId + "' " +
                "AND TradeDate <= '" + asOfText + "'");
            return subs - reds;
        }

        private decimal ScalarSum(string sql)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    object o = cmd.ExecuteScalar();
                    if (o == null || o == DBNull.Value)
                    {
                        return 0m;
                    }
                    return Convert.ToDecimal(o, CultureInfo.InvariantCulture);
                }
            }
        }

        // Core computation: for every (share class, valuation date) produce the
        // NAV per unit and the daily management fee accrual.
        public List<NavResult> ComputeAll()
        {
            var results = new List<NavResult>();
            foreach (var sc in GetShareClasses())
            {
                foreach (var v in GetValuations(sc.ShareClassId))
                {
                    decimal units = GetUnitsInIssue(sc.ShareClassId, v.AsOfDate);
                    if (units <= 0m)
                    {
                        // Skip dates before the class had any units.
                        continue;
                    }

                    // An incidental audit line -- uses the weak MD5 hash.
                    string clientRef = LegacyHash.ClientRef(sc.ShareClassId, _auditSalt);

                    results.Add(new NavResult
                    {
                        ShareClassId = sc.ShareClassId,
                        AsOfDate = v.AsOfDate,
                        UnitsInIssue = units,
                        NetAssetValue = v.NetAssetValue,
                        NavPerUnit = FeeCalculator.NavPerUnit(v.NetAssetValue, units),
                        ManagementFeeBps = sc.ManagementFeeBps,
                        DailyManagementFee = FeeCalculator.DailyManagementFee(v.NetAssetValue, sc.ManagementFeeBps),
                    });
                }
            }
            return results;
        }
    }
}
