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
    public partial class ChargebackHourControl : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int KindIndex
        {
            get { return kindIndex; }
            set {
                kindIndex = value;
            }
        }
        private int kindIndex = 0;

        public int DayOfWeek { get; set; }
        public int HourOfDay { get; set; }

        public Color CellColor { get; set; }


        public ChargebackHourControl()
        {
            InitializeComponent();
            dayLabel.DataBindings.Add(new Binding("Text", bindingSource, "DayOfWeek"));
            hourLabel.DataBindings.Add(new Binding("Text", bindingSource, "HourOfDay"));

            this.Paint += ChargebackHourControl_Paint;
            bindingSource.DataSource = this;
        }

        private void ChargebackHourControl_Paint(object sender, PaintEventArgs e)
        {
            var cellColor = CellColor == null ? Color.DarkRed : CellColor;
            var cellBrush = new SolidBrush(cellColor);

            e.Graphics.FillRectangle(cellBrush, e.ClipRectangle);
        }
    }
}
