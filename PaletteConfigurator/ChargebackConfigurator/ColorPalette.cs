using System.Drawing;

namespace PaletteConfigurator.ChargebackConfigurator
{
    /// <summary>
    /// Holder class for all the colors used in the chargeback configuration
    /// </summary>
    class ColorPalette
    {
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

        /// <summary>
        /// Returns the foreground color for an index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static Color ColorForIndex(int idx)
        {
            return COLORS[idx % COLORS.Length];
        }

        /// <summary>
        /// Returns a good writing / contrast color for an index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static Color ContrastColorForIndex(int idx)
        {
            return CONTRAST_COLORS[idx % CONTRAST_COLORS.Length];
        }

    }
}
