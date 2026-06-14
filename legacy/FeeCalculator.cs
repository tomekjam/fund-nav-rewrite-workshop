using System;

namespace FundNav.Legacy
{
    // Pricing maths. Notice the magic numbers and the ad hoc rounding -- the
    // rewrite should name these intentions instead of hard-coding them.
    public static class FeeCalculator
    {
        // NAV per unit. NetAssetValue is the class NAV; units is units in issue.
        public static decimal NavPerUnit(decimal netAssetValue, decimal units)
        {
            if (units <= 0m)
            {
                // No units in issue -> no meaningful price.
                return 0m;
            }

            decimal raw = netAssetValue / units;

            // Ad hoc rounding to 6 dp. MidpointRounding.AwayFromZero is chosen
            // deliberately: the platform default (banker's rounding) would give
            // different prices at exact .5 midpoints. Behaviour to PRESERVE.
            return Math.Round(raw, 6, MidpointRounding.AwayFromZero);
        }

        // Daily management fee accrual.
        //
        // Intended rule (per the fund prospectus): actual/365, i.e.
        //     NetAssetValue * Bps / 10000 / 365
        //
        // QUIRK A (BUG -- to be FIXED in the rewrite):
        //     this code divides by 360, not 365. A leftover "bankers' year"
        //     day-count that slipped in years ago. It slightly overstates the
        //     daily fee. The new system must use 365.
        //
        // QUIRK B (REAL BUSINESS RULE -- to be PRESERVED):
        //     the accrual is rounded DOWN to 2 decimals (never round up).
        //     Downstream reconciliation and investor reporting depend on this
        //     floor; rounding half-up would break those processes.
        public static decimal DailyManagementFee(decimal netAssetValue, int bps)
        {
            decimal raw = netAssetValue * bps / 10000m / 360m;   // QUIRK A: /360 should be /365

            // QUIRK B: floor to 2 decimals (round down), not standard rounding.
            return Math.Floor(raw * 100m) / 100m;
        }
    }
}
