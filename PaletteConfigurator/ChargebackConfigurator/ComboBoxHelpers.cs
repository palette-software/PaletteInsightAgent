using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteConfigurator.ChargebackConfigurator
{
    class ComboBoxHelpers
    {
        /// <summary>
        /// Fills a combobox with values and selects the default one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="objects"></param>
        /// <param name="defaultValue"></param>
        public static void FillCombobox<T>(ComboBox comboBox, IEnumerable<T> objects,
             object defaultValue = null)
        {
            foreach (var tz in objects)
            {
                comboBox.Items.Add(tz);
            }

            // Set the default value if not present
            if (defaultValue != null)
            {
                selectObject(comboBox, defaultValue);
            }
        }

        /// <summary>
        ///  Selects an object in a combobox
        /// </summary>
        /// <param name="combobox"></param>
        /// <param name="item"></param>
        public static void selectObject<T>(ComboBox combobox, T item )
        {
            var items = combobox.Items;
            var itemCount = items.Count;
            var selectedIndex = 0;

            for (var i = 0; i < itemCount; ++i)
            {
                var currentItem = items[i];
                if (item.Equals(currentItem))
                {
                    selectedIndex = i;
                    break;
                }
            }

            combobox.SelectedIndex = selectedIndex;
        }

    }
}
