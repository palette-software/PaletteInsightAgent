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
        /// <summary>
        /// The binding target for the model setup
        /// </summary>
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



        public ChargebackConfigurationPanel()
        {
            InitializeComponent();

            SetupControls();
            InitializeBindings();
            SelectedModel = new ChargebackModel
            {
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddMonths(3),
                UnitPriceCurrency = selectedModel.UnitPriceCurrency
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

        /// <summary>
        /// Initialize the comboboxes with content
        /// </summary>
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

        /// <summary>
        /// Set up the bindings
        /// </summary>
        private void InitializeBindings()
        {
            effectiveFromPicker.DataBindings.Add(new Binding("Value", modelBindingSource, "EffectiveFrom", true, DataSourceUpdateMode.OnPropertyChanged));
            effectiveToPicker.DataBindings.Add(new Binding("Value", modelBindingSource, "EffectiveTo", true, DataSourceUpdateMode.OnPropertyChanged));

            
            currencySelector.DataBindings.Add(new Binding("SelectedItem", modelBindingSource, "UnitPriceCurrency", true, DataSourceUpdateMode.OnPropertyChanged));

            categoryIndexSource.DataSource = chargebackKindSelector;
            // bind the selected category in the category editor to the
            // index we'll update the clicked hours
            weeklyCategoriesControl.DataBindings.Add(new Binding("CategoryIndex", categoryIndexSource, "SelectedIndex", false, DataSourceUpdateMode.OnPropertyChanged));

            // bind the currency selector to the category selector for currency display
            chargebackKindSelector.DataBindings.Add(new Binding("Currency", modelBindingSource, "UnitPriceCurrency", false, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
