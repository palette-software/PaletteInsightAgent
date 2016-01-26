using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.ChargebackConfigurator
{
    /// <summary>
    /// The entity class for reading/writing ChargebackModels
    /// </summary>
    public class ChargebackModel
    {
        #region Data
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public string TimezoneForChargeback { get; set; }
        public string UnitPriceCurrency { get; set; }

        /// <summary>
        /// The cost of a single unit of storage.
        /// </summary>
        public decimal StorageUnitPrice { get; set; }

        /// <summary>
        /// The amount of bytes in a single storage unit for chargeback.
        /// </summary>
        public Int64 StoregeBytesPerUnit { get; set; }

        #endregion

        public override string ToString()
        {
            return String.Format("[ChargebackModel effectiveFrom={0} effectiveTo={1} timezonForChargeback={2} unitPriceCurrency={3}]",
                EffectiveFrom.ToString(), EffectiveTo.ToString(), TimezoneForChargeback, UnitPriceCurrency);
        }

    }

    public enum ChargebackUsageType
    {
        INTERACTOR = 1, EXTRACTOR = 0
    }

    public class ChargebackValue
    {
        public ChargebackUsageType UsageType { get; set; }
        public Int32 DayOfWeek { get; set; }
        public Int32 HourOfDay { get; set; }

        public decimal UnitPrice { get; set; }
        public string RateCategory { get; set; }

    }

}
