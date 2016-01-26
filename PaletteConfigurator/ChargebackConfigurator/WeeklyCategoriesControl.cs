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
    [DesignerCategory("Chargeback")]
    public partial class WeeklyCategoriesControl : UserControl
    {
        [DefaultValue(0)]
        public int CategoryIndex { get; set; }

        [DefaultValue(5)]
        public int BoxPadding { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int[] CategoryIndices
        {
            get { return categoryIndices; }
            set
            {
                categoryIndices = value;
            }
        }


        private int[] categoryIndices;

        private WeekTableDescription tableDescription;

        public WeeklyCategoriesControl()
        {
            InitializeComponent();
            var ci = new int[24 * 7];
            for (int i = 0; i < 24 * 7; ++i)
            {
                ci[i] = 0;
            }
            CategoryIndices = ci;
            Paint += WeeklyCategoriesControl_Paint;
            Click += WeeklyCategoriesControl_Click;
            MouseMove += WeeklyCategoriesControl_MouseMove;
        }

        /// <summary>
        /// Updates any boxes the user dragged the mouse over
        /// while holding the left button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WeeklyCategoriesControl_MouseMove(object sender, MouseEventArgs e)
        {
            // if the left mouse button is down
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                UpdateFromMouse(e);
            }
        }

        /// <summary>
        /// Updates the clicked box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WeeklyCategoriesControl_Click(object sender, EventArgs e)
        {
            UpdateFromMouse(e);
        }

        private void UpdateFromMouse(EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coords = me.Location;

            var idx = WeekTableDrawer.GetEntryIndex(tableDescription, coords);
            // dont do anything if clicked outside of the valid borders
            if (idx < 0 || idx >= categoryIndices.Length) return;
            // dont refresh if the category is already set to this
            if (categoryIndices[idx] == CategoryIndex) return;
            // update the category
            categoryIndices[idx] = CategoryIndex;
            // Invalidate the paint rectangle.
            Invalidate(WeekTableDrawer.GetEntryPosition(tableDescription, idx));
        }

        private void WeeklyCategoriesControl_Paint(object sender, PaintEventArgs e)
        {
            // TODO: paint only the desired parts inside e.ClipRectangle
            tableDescription = new WeekTableDescription
            {
                Bounds = Bounds,
                GlobalPadding = new Size(5, 5),

                LabelWidth = 70,
                LabelHeight = 20,
            };

            WeekTableDrawer.DrawWeekTable(tableDescription, e.Graphics, categoryIndices);
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
                    var idx = GetIdx(dow, hod);
                    act(dow, hod, idx);
                }
        }

        /// <summary>
        /// Get the index in the categoryIndices
        /// </summary>
        /// <param name="dow"></param>
        /// <param name="hod"></param>
        /// <returns></returns>
        private static int GetIdx(int dow, int hod)
        {
            return dow * 24 + hod;
        }

    }
}
