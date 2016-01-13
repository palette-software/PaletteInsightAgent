using System;
using System.Drawing;

namespace PaletteConfigurator.ChargebackConfigurator
{
    public struct WeekTableDescription
    {
        public Rectangle Bounds;
        public Size LocalPadding;
        public Size GlobalPadding;

        public int LabelWidth;
        public int LabelHeight;

        /// <summary>
        /// Returns the table body rectangle
        /// </summary>
        /// <returns></returns>
        public Rectangle BodyRectangle
        {
            get
            {
            return new Rectangle(
                GlobalPadding.Width + LabelWidth,
                GlobalPadding.Height + LabelHeight,
                Bounds.Width - (2* GlobalPadding.Width) - LabelWidth,     
                Bounds.Height - (2* GlobalPadding.Height) - LabelHeight
                );

            }
        }
    }

    public struct CellSizes
    {
        public Size raw;
        public Size withPadding;
    }


    /// <summary>
    /// A function namespace for the weekly chargeback table rendering
    /// </summary>
    class WeekTableDrawer
    {
        public const int DAYS = 7;
        public const int HOURS = 24;

        private static readonly string[] DAY_NAMES = new string[] {
            "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"
        };

        public static void DrawWeekTable(WeekTableDescription desc, Graphics g, int[] categoryIndices)
        {
            // dont work on bad input
            if (categoryIndices.Length < DAYS * HOURS) return;
            var cellSize = GetCellSize(desc);
            DrawTableBody(g, categoryIndices, cellSize, desc.BodyRectangle);
            DrawHourLabels(desc, g, cellSize);
            DrawDayLabels(desc, g, cellSize);
        }

        /// <summary>
        /// Draws the day labels above the table
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="g"></param>
        /// <param name="cellSize"></param>
        private static void DrawDayLabels(WeekTableDescription desc, Graphics g, CellSizes cellSize)
        {
            var labelFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };


            var labelSize = new Size(cellSize.withPadding.Width, desc.LabelHeight - (desc.GlobalPadding.Height * 2));
            g.TranslateTransform(desc.GlobalPadding.Width + desc.LabelWidth, desc.GlobalPadding.Height);
            for (var d = 0; d < DAYS; ++d)
            {
                g.DrawString(
                    DAY_NAMES[d],
                    SystemFonts.DefaultFont,
                    SystemBrushes.ControlText,
                    new Rectangle(
                        new Point((d * cellSize.withPadding.Width), 0),
                        labelSize
                        ),
                    labelFormat
                    );
            }
            g.ResetTransform();
        }

        /// <summary>
        /// Draws the hour labels on the left of the table
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="g"></param>
        /// <param name="cellSize"></param>
        private static void DrawHourLabels(WeekTableDescription desc, Graphics g, CellSizes cellSize)
        {
            var labelFormat = new StringFormat {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };

            var labelSize = new Size(
                desc.LabelWidth - 2 * desc.GlobalPadding.Width, 
                cellSize.withPadding.Height
                );

            g.TranslateTransform(desc.GlobalPadding.Width, desc.LabelHeight + desc.GlobalPadding.Height);
            for (var h = 0; h < HOURS; ++h)
            {
                g.DrawString(
                    String.Format("{0}-{1}h", h, h + 1),
                    SystemFonts.DefaultFont,
                    SystemBrushes.ControlText,
                    new Rectangle( new Point( 0, (h * cellSize.withPadding.Height)), labelSize),
                    labelFormat
                    );
            }
            g.ResetTransform();
        }


        /// <summary>
        /// Draws the table body itself
        /// </summary>
        /// <param name="g"></param>
        /// <param name="categoryIndices"></param>
        /// <param name="cellSize"></param>
        private static void DrawTableBody(Graphics g, int[] categoryIndices, CellSizes cellSize, Rectangle bodyRect)
        {

            g.TranslateTransform(bodyRect.X, bodyRect.Y);
            // paint the 
            ForAllHours((dow, hod, idx) =>
            {
                var origin = new Point(dow * cellSize.withPadding.Width, hod * cellSize.withPadding.Height);
                var categoryIdx = categoryIndices[idx];

                using (var colorBrush = new SolidBrush(ColorPalette.ColorForIndex(categoryIdx)))
                using (var contrastBrush = new SolidBrush(ColorPalette.ContrastColorForIndex(categoryIdx)))
                using (var contrastPen = new Pen(contrastBrush))
                {
                    var cellRect = new Rectangle(origin, cellSize.raw);
                    g.FillRectangle(colorBrush, cellRect);
                    g.DrawRectangle(contrastPen, cellRect);
                }
            });

            g.ResetTransform();
        }


        /// <summary>
        /// Returns the cell size from the description
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        private static CellSizes GetCellSize(WeekTableDescription desc)
        {
            var bounds = desc.BodyRectangle;
            var padding = desc.GlobalPadding;
            // do some global calculations
            var cellW = (bounds.Width / DAYS) - padding.Width;
            var cellH = (bounds.Height / HOURS) - padding.Height;

            return new CellSizes
            {
                withPadding = new Size(cellW + padding.Width, cellH + padding.Height),
                raw = new Size(cellW, cellH)
            };

        }

        /// <summary>
        /// Helper to run a delegate for all day of week/ hour of day combo
        /// </summary>
        /// <param name="act">an action taking day_of_week,hour_of_day, index as parameters</param>
        private static void ForAllHours(Action<int, int, int> act)
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


        public static int GetEntryIndex(WeekTableDescription desc, Point mouse)
        {
            var c = mouse;
            // return if the mouse is outside the body rectangle
            if (!desc.BodyRectangle.Contains(mouse)) return -1;

            var cellSize = GetCellSize(desc);
            // offset for the labels
            c.Offset(
                -(desc.GlobalPadding.Width * 2 + desc.LabelWidth),
                -(desc.GlobalPadding.Height * 2 + desc.LabelHeight));

            if (c.Y > HOURS * cellSize.withPadding.Height) return -1;

            return GetIdx(c.X / cellSize.withPadding.Width, c.Y / cellSize.withPadding.Height);
        }


        public static Rectangle GetEntryPosition(WeekTableDescription desc, int idx)
        {
            // calc the cell size
            var cellSize = GetCellSize(desc);
            // store some things
            var dow = idx / HOURS;
            var hod = idx % HOURS;
            var gpw = desc.GlobalPadding.Width;
            var gph = desc.GlobalPadding.Height;
            var csw = cellSize.withPadding.Width;
            var csh = cellSize.withPadding.Height;
            

            return new Rectangle(
                (gpw) + desc.LabelWidth  + dow * csw,
                (gph) + desc.LabelHeight + hod * csh,
                csw,
                csh
                );

        }

    }
}
