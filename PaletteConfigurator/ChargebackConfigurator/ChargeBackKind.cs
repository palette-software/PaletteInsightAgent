using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaletteConfigurator.ChargebackConfigurator
{
    public class ChargeBackKind
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Color SymbolColor { get; set; }


        public static readonly Color[] COLORS = new Color[] {
            Color.Green,
            Color.Red,
            Color.Orange,
            Color.Blue,
            Color.Yellow,
            Color.SkyBlue,
            Color.YellowGreen
        };
    }
}
