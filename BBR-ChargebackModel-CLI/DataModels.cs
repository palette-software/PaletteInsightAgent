using System;
using FileHelpers;
using System.Linq;

namespace BBR_ChargebackModel_CLI
{


    /// <summary>
    /// The entity class for reading/writing ChargebackModels
    /// </summary>
    [DelimitedRecord(",")]
    public class ChargebackModel
    {
        #region Data
        [FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss")]
        public DateTime EffectiveFrom;

        [FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss")]
        public DateTime EffectiveTo;

        public string TimezoneForChargeback;
        public string UnitPriceCurrency;

        #endregion

        public override string ToString()
        {
            return String.Format("[ChargebackModel effectiveFrom={0} effectiveTo={1} timezonForChargeback={2} unitPriceCurrency={3}]",
                EffectiveFrom.ToString(), EffectiveTo.ToString(), TimezoneForChargeback, UnitPriceCurrency);
        }

        /// <summary>
        /// Load a model from a CSV file.
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns></returns>
        public static ChargebackModel FromFile(string filePath)
        {
            var engine = new FileHelperEngine<ChargebackModel>();
            return engine.ReadFile(filePath).First();
        }
    }

    public enum ChargebackUsageType
    {
        INTERACTOR = 1, EXTRACTOR = 0
    }

    [DelimitedRecord(",")]
    public class ChargebackValue
    {
        public ChargebackUsageType UsageType;
        public Int32 DayOfWeek;
        public Int32 HourOfDay;

        public decimal UnitPrice;
        public string RateCategory;

        /// <summary>
        /// Loads a list of chargeback values from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static ChargebackValue[] FromFile(string filePath)
        {
            var engine = new FileHelperEngine<ChargebackValue>();
            return engine.ReadFile(filePath);
        }
    }



}
