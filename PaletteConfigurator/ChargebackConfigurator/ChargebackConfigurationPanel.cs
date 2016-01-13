using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace PaletteConfigurator.ChargebackConfigurator
{

    public partial class ChargebackConfigurationPanel : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChargebackModel SelectedModel
        {
            get { return selectedModel; }
            set
            {
                selectedModel = value;
                modelBindingSource.DataSource = selectedModel;
            }
        }

        private ChargebackModel selectedModel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<ChargeBackKind> ChargebackKinds
        {
            get { return chargebackKinds; }
            set
            {
                chargebackKinds = value;
                kindDataSource.DataSource = value;
            }
        }
        private IList<ChargeBackKind> chargebackKinds;



        public ChargebackConfigurationPanel()
        {
            InitializeComponent();

            SetupControls();
            InitializeBindings();

            ChargebackKinds = new ChargeBackKind[] {

                new ChargeBackKind
                {
                    Name = "Peak",
                    Price = 200,
                    SymbolColor = ChargeBackKind.COLORS[0],
                },

                new ChargeBackKind
                {
                    Name = "Off Peak",
                    Price = 100,
                    SymbolColor = ChargeBackKind.COLORS[1],
                },

                new ChargeBackKind
                {
                    Name = "Holidays",
                    Price = 50,
                    SymbolColor = ChargeBackKind.COLORS[2],
                },
            };

            SelectedModel = new ChargebackModel
            {
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddMonths(3),
                UnitPriceCurrency = "USD"
            };

        }

        /// <summary>
        /// Helper to run a delegate for all day of week/ hour of day combo
        /// </summary>
        /// <param name="act">an action taking day_of_week,hour_of_day, index as parameters</param>
        private void ForAllHours(Action<int, int, int> act)
        {
            for (var dow = 0; dow < 7; ++dow)
                for (var hod = 0; hod < 24; ++hod)
                {
                    var idx = dow * 24 + hod;
                    act(dow, hod, idx);
                }
        }

        private void SetupControls()
        {
            // Setup the comboboxes
            ComboBoxHelpers.FillCombobox(
                currencySelector,
                CurrencyTools.NameToCodeMap.Select(x => CurrencyEntry.Make(x.Value)),
                CurrencyEntry.Make(RegionInfo.CurrentRegion)
                );

            ComboBoxHelpers.FillCombobox(
                timezoneSelector,
                TimeZoneInfo.GetSystemTimeZones(),
                TimeZoneInfo.Local);
        }

        private void InitializeBindings()
        {
            effectiveFromPicker.DataBindings.Add(new Binding("Value", modelBindingSource, "EffectiveFrom"));
            effectiveToPicker.DataBindings.Add(new Binding("Value", modelBindingSource, "EffectiveTo"));

            
            currencySelector.DataBindings.Add(new Binding("SelectedItem", modelBindingSource, "UnitPriceCurrency"));

            categoryIndexSource.DataSource = chargebackKindSelector;
            // bind the selected category in the category editor to the
            // index we'll update the clicked hours
            weeklyCategoriesControl.DataBindings.Add(new Binding("CategoryIndex", categoryIndexSource, "SelectedIndex"));

            // bind the currency selector to the category selector for currency display
            chargebackKindSelector.DataBindings.Add(new Binding("Currency", modelBindingSource, "UnitPriceCurrency"));
        }
    }
}
