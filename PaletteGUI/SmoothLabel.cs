using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteGUI
{
    public partial class SmoothLabel : Label
    {
        public float Kerning { get; set; }

        public SmoothLabel()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //Graphics g = e.Graphics;
            //Brush brush = new SolidBrush(Color.Black);

            //float pos = 0.0f;
            //for (int i = 0; i < Text.Length; ++i)
            //{
            //    string charToDraw = new string(Text[i], 1);
            //    g.DrawString(charToDraw, Font, brush, pos, 0.0f);
            //    SizeF sizeChar = g.MeasureString(charToDraw, Font);
            //    pos += sizeChar.Width * Kerning;
            //}

            base.OnPaint(e);
        }
    }
}
