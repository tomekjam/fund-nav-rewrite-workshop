using System;

namespace FundNav.Legacy
{
    // Anemic data carriers. The legacy module keeps logic OUT of these and
    // mixes it into NavService -- one of the things the rewrite untangles.

    public class ShareClassRow
    {
        public string ShareClassId;
        public string FundId;
        public string Name;
        public string Currency;
        public int ManagementFeeBps;
    }

    public class ValuationRow
    {
        public string ShareClassId;
        public DateTime AsOfDate;
        public decimal NetAssetValue;
    }

    // Output record: NAV per unit + the daily management fee accrual.
    public class NavResult
    {
        public string ShareClassId;
        public DateTime AsOfDate;
        public decimal UnitsInIssue;
        public decimal NetAssetValue;
        public decimal NavPerUnit;
        public int ManagementFeeBps;
        public decimal DailyManagementFee;
    }
}
