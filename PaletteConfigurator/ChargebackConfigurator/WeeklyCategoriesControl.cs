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
                UpdateIndices();
            }
        }


        private int[] categoryIndices;


        private Size cellSize;

        /// <summary>
        /// The colors used
        /// </summary>
        public static readonly Color[] COLORS = new Color[] {
            Color.Green,
            Color.Red,
            Color.Orange,
            Color.Blue,
            Color.Yellow,
            Color.SkyBlue,
            Color.YellowGreen
        };

        /// <summary>
        /// Colors for writing text on the previous color list
        /// </summary>
        public static readonly Color[] CONTRAST_COLORS = new Color[]
        {
            Color.Black,
            Color.White,
            Color.Black,
            Color.White,
            Color.Black,
            Color.Black,
            Color.White
        };


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
        }

        private void WeeklyCategoriesControl_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coords = me.Location;

            var dow = coords.X / cellSize.Width;
            var hod = coords.Y / cellSize.Height;

            //categoryIndices[GetIdx(dow, hod)] += 1;
            categoryIndices[GetIdx(dow, hod)] = CategoryIndex;
            // Invalidate the paint rectangle.
            Invalidate(
                new Rectangle(
                    dow * (cellSize.Width + BoxPadding),
                    hod * (cellSize.Height + BoxPadding),
                    cellSize.Width,
                    cellSize.Height));
        }

        private void WeeklyCategoriesControl_Paint(object sender, PaintEventArgs e)
        {
            // TODO: paint only the desired parts inside e.ClipRectangle
            var innerPadding = 2;
            var halfPadding = BoxPadding / 2;
            var cellWidth = (Size.Width / 7) - BoxPadding;
            var cellHeight = (Size.Height / 24) - BoxPadding;
            var cellSize = new Size(cellWidth, cellHeight);
            // store the cellsize for processing
            this.cellSize = cellSize;
            var textPadding = new Size(innerPadding, innerPadding);

            var g = e.Graphics;
            var f = SystemFonts.DefaultFont;


            ForAllHours((dow, hod, idx) =>
            {
                var x = dow * (cellWidth + BoxPadding);
                var y = hod * (cellHeight + BoxPadding);

                var colorIndex = categoryIndices[idx] % COLORS.Length;
                using (var brush = new SolidBrush(COLORS[colorIndex]))
                using (var contrastBrush = new SolidBrush(CONTRAST_COLORS[colorIndex]))
                {
                    var rectOrigin = new Point(x, y);
                    var origin = new Point(x + halfPadding, y + halfPadding);

                    g.FillRectangle(brush, new Rectangle(rectOrigin, cellSize));
                    g.DrawRectangle(Pens.White, new Rectangle(rectOrigin, cellSize));

                    g.DrawString(
                        string.Format("{0}-{1}h", hod, hod + 1),
                        f,
                        contrastBrush,
                        Point.Add(origin, textPadding));
                }

            });

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

        private void UpdateIndices()
        {
            //           
        }
    }
}
