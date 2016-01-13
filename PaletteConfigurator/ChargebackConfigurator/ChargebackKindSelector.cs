using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteConfigurator.ChargebackConfigurator
{
    public partial class ChargebackKindSelector : UserControl
    {
        /// <summary>
        /// The currency to use
        /// </summary>
        [DefaultValue("USD")]
        public string Currency
        {
            get { return currency; }
            set
            {
                currency = value;
                // handle nulls in designer view
                if (categories == null) return;
                // update the currency in the categories on change
                foreach (var category in categories)
                {
                    category.Currency = value;
                }
                categoriesBindingSource.ResetBindings(false);
            }
        }

        private string currency;

        /// <summary>
        /// The currently selected category index.
        /// </summary>
        public int SelectedIndex { get; set; }


        /// <summary>
        /// All categories
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<ChargebackCategory> Categories
        {
            get { return categories; }
            set
            {
                // convert it to a BindingList, so DataBinding
                // does work on it
                var bl = new BindingList<ChargebackCategory>(value);
                categories = bl;
                categoriesBindingSource.DataSource = categories;
            }
        }

        private IList<ChargebackCategory> categories;


        public ChargebackKindSelector()
        {
            InitializeComponent();
            SetupCategorySelector();

            Categories = new List<ChargebackCategory> {
                new ChargebackCategory { Name="Peak", Price = 100, Currency = Currency },
                new ChargebackCategory { Name="Off-Peak", Price = 50, Currency = Currency }
            };

            selectionBindingSource.DataSource = this;

        }

        /// <summary>
        /// Initialize the category selector listbox
        /// </summary>
        private void SetupCategorySelector()
        {
            categoryListBox.DisplayMember = "DisplayString";
            categoryListBox.DrawItem += CategoryListBox_DrawItem;
            categoryListBox.DrawMode = DrawMode.OwnerDrawFixed;
            categoryListBox.DataBindings.Add(new Binding("SelectedIndex", selectionBindingSource, "SelectedIndex"));

            categoryListBox.MouseDoubleClick += CategoryListBox_MouseDoubleClick;
        }

        /// <summary>
        /// Pops up the category editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowCategoryEditorDialog((ChargebackCategory)categoryListBox.SelectedItem);
        }

        /// <summary>
        /// Displays a category editor dialog with the currently selected category
        /// </summary>
        private void ShowCategoryEditorDialog(ChargebackCategory cat)
        {
            var form = new ChargebackCategoryEditor();
            form.Category = cat; 
            form.ShowDialog();
        }

        /// <summary>
        /// Draws the color swatch in front of the listbox entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CategoryListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var listBox = categoryListBox;
            // skip work if the index is outside of the collection
            if (e.Index < 0 || e.Index >= listBox.Items.Count) return;

            e.DrawBackground();

            var name = listBox.Items[e.Index].ToString();
            var selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

            var categoryColor = ColorPalette.ColorForIndex(e.Index);

            var fillRectSide = e.Bounds.Height - 5;
            var fillRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, fillRectSide, fillRectSide);


            using (var b = new SolidBrush(ColorPalette.ColorForIndex(e.Index)))
            using (var contrastBrush = new SolidBrush(ColorPalette.ContrastColorForIndex(e.Index)))
            {

                e.Graphics.FillRectangle(b, fillRect);
                e.Graphics.DrawRectangle(Pens.Black, fillRect);
            }

            var textBounds = e.Bounds;
            var textBrush = selected ? SystemBrushes.HighlightText : SystemBrushes.ControlText;
            textBounds.Offset(fillRectSide + 5, 0);
            e.Graphics.DrawString(name, Font, textBrush, textBounds);
        }

        /// <summary>
        /// Adds a new category and shows the editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addNewButton_Click(object sender, EventArgs e)
        {
            var c = new ChargebackCategory
            {
                Name = "Unnamed Category " + categoryListBox.Items.Count,
                Price = 100,
                Currency = Currency
            };
            categories.Add(c);
            ShowCategoryEditorDialog(c);
        }

        /// <summary>
        /// Selects the last item in the combobox
        /// </summary>
        private void SelectLastCategory()
        {
            categoryListBox.SelectedIndex = categoryListBox.Items.Count - 1;
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            var selectedItems = categoryListBox.SelectedItems;
            if (selectedItems.Count == 0) return;
            // We need to copy the list so changing it by removing stuff does not
            // matter
            var selectedArray = new ChargebackCategory[selectedItems.Count];
            selectedItems.CopyTo(selectedArray, 0);

            foreach (var cat in selectedArray)
                categories.Remove((ChargebackCategory)cat);
        }
    }

    /// <summary>
    /// Data model for the chargeback category as used by the GUI
    /// </summary>
    public class ChargebackCategory
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public string Currency { get; set; }

        override public string ToString()
        {
            return String.Format("{0} - {1} {2}", Name, Price, Currency);
        }
    }

}
