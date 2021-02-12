using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// Creates a brush with a solid color
    /// </summary>
    public sealed class SolidColorBrush : Brush
    {
        #region Predefined Colors
        /// <summary>
        /// Brush for <see cref="Color.AliceBlue"/>
        /// </summary>
        public static Brush AliceBlue { get; } = new SolidColorBrush(Color.AliceBlue);

        /// <summary>
        /// Brush for <see cref="Color.AntiqueWhite"/>
        /// </summary>
        public static Brush AntiqueWhite { get; } = new SolidColorBrush(Color.AntiqueWhite);

        /// <summary>
        /// Brush for <see cref="Color.Aqua"/>
        /// </summary>
        public static Brush Aqua { get; } = new SolidColorBrush(Color.Aqua);

        /// <summary>
        /// Brush for <see cref="Color.Aquamarine"/>
        /// </summary>
        public static Brush Aquamarine { get; } = new SolidColorBrush(Color.Aquamarine);

        /// <summary>
        /// Brush for <see cref="Color.Azure"/>
        /// </summary>
        public static Brush Azure { get; } = new SolidColorBrush(Color.Azure);

        /// <summary>
        /// Brush for <see cref="Color.Beige"/>
        /// </summary>
        public static Brush Beige { get; } = new SolidColorBrush(Color.Beige);

        /// <summary>
        /// Brush for <see cref="Color.Bisque"/>
        /// </summary>
        public static Brush Bisque { get; } = new SolidColorBrush(Color.Bisque);

        /// <summary>
        /// Brush for <see cref="Color.Black"/>
        /// </summary>
        public static Brush Black { get; } = new SolidColorBrush(Color.Black);

        /// <summary>
        /// Brush for <see cref="Color.BlanchedAlmond"/>
        /// </summary>
        public static Brush BlanchedAlmond { get; } = new SolidColorBrush(Color.BlanchedAlmond);

        /// <summary>
        /// Brush for <see cref="Color.Blue"/>
        /// </summary>
        public static Brush Blue { get; } = new SolidColorBrush(Color.Blue);

        /// <summary>
        /// Brush for <see cref="Color.BlueViolet"/>
        /// </summary>
        public static Brush BlueViolet { get; } = new SolidColorBrush(Color.BlueViolet);

        /// <summary>
        /// Brush for <see cref="Color.Brown"/>
        /// </summary>
        public static Brush Brown { get; } = new SolidColorBrush(Color.Brown);

        /// <summary>
        /// Brush for <see cref="Color.BurlyWood"/>
        /// </summary>
        public static Brush BurlyWood { get; } = new SolidColorBrush(Color.BurlyWood);

        /// <summary>
        /// Brush for <see cref="Color.CadetBlue"/>
        /// </summary>
        public static Brush CadetBlue { get; } = new SolidColorBrush(Color.CadetBlue);

        /// <summary>
        /// Brush for <see cref="Color.Chartreuse"/>
        /// </summary>
        public static Brush Chartreuse { get; } = new SolidColorBrush(Color.Chartreuse);

        /// <summary>
        /// Brush for <see cref="Color.Chocolate"/>
        /// </summary>
        public static Brush Chocolate { get; } = new SolidColorBrush(Color.Chocolate);

        /// <summary>
        /// Brush for <see cref="Color.Coral"/>
        /// </summary>
        public static Brush Coral { get; } = new SolidColorBrush(Color.Coral);

        /// <summary>
        /// Brush for <see cref="Color.CornflowerBlue"/>
        /// </summary>
        public static Brush CornflowerBlue { get; } = new SolidColorBrush(Color.CornflowerBlue);

        /// <summary>
        /// Brush for <see cref="Color.Cornsilk"/>
        /// </summary>
        public static Brush Cornsilk { get; } = new SolidColorBrush(Color.Cornsilk);

        /// <summary>
        /// Brush for <see cref="Color.Crimson"/>
        /// </summary>
        public static Brush Crimson { get; } = new SolidColorBrush(Color.Crimson);

        /// <summary>
        /// Brush for <see cref="Color.Cyan"/>
        /// </summary>
        public static Brush Cyan { get; } = new SolidColorBrush(Color.Cyan);

        /// <summary>
        /// Brush for <see cref="Color.DarkBlue"/>
        /// </summary>
        public static Brush DarkBlue { get; } = new SolidColorBrush(Color.DarkBlue);

        /// <summary>
        /// Brush for <see cref="Color.DarkCyan"/>
        /// </summary>
        public static Brush DarkCyan { get; } = new SolidColorBrush(Color.DarkCyan);

        /// <summary>
        /// Brush for <see cref="Color.DarkGoldenrod"/>
        /// </summary>
        public static Brush DarkGoldenrod { get; } = new SolidColorBrush(Color.DarkGoldenrod);

        /// <summary>
        /// Brush for <see cref="Color.DarkGray"/>
        /// </summary>
        public static Brush DarkGray { get; } = new SolidColorBrush(Color.DarkGray);

        /// <summary>
        /// Brush for <see cref="Color.DarkGreen"/>
        /// </summary>
        public static Brush DarkGreen { get; } = new SolidColorBrush(Color.DarkGreen);

        /// <summary>
        /// Brush for <see cref="Color.DarkKhaki"/>
        /// </summary>
        public static Brush DarkKhaki { get; } = new SolidColorBrush(Color.DarkKhaki);

        /// <summary>
        /// Brush for <see cref="Color.DarkMagenta"/>
        /// </summary>
        public static Brush DarkMagenta { get; } = new SolidColorBrush(Color.DarkMagenta);

        /// <summary>
        /// Brush for <see cref="Color.DarkOliveGreen"/>
        /// </summary>
        public static Brush DarkOliveGreen { get; } = new SolidColorBrush(Color.DarkOliveGreen);

        /// <summary>
        /// Brush for <see cref="Color.DarkOrange"/>
        /// </summary>
        public static Brush DarkOrange { get; } = new SolidColorBrush(Color.DarkOrange);

        /// <summary>
        /// Brush for <see cref="Color.DarkOrchid"/>
        /// </summary>
        public static Brush DarkOrchid { get; } = new SolidColorBrush(Color.DarkOrchid);

        /// <summary>
        /// Brush for <see cref="Color.DarkRed"/>
        /// </summary>
        public static Brush DarkRed { get; } = new SolidColorBrush(Color.DarkRed);

        /// <summary>
        /// Brush for <see cref="Color.DarkSalmon"/>
        /// </summary>
        public static Brush DarkSalmon { get; } = new SolidColorBrush(Color.DarkSalmon);

        /// <summary>
        /// Brush for <see cref="Color.DarkSeaGreen"/>
        /// </summary>
        public static Brush DarkSeaGreen { get; } = new SolidColorBrush(Color.DarkSeaGreen);

        /// <summary>
        /// Brush for <see cref="Color.DarkSlateBlue"/>
        /// </summary>
        public static Brush DarkSlateBlue { get; } = new SolidColorBrush(Color.DarkSlateBlue);

        /// <summary>
        /// Brush for <see cref="Color.DarkSlateGray"/>
        /// </summary>
        public static Brush DarkSlateGray { get; } = new SolidColorBrush(Color.DarkSlateGray);

        /// <summary>
        /// Brush for <see cref="Color.DarkTurquoise"/>
        /// </summary>
        public static Brush DarkTurquoise { get; } = new SolidColorBrush(Color.DarkTurquoise);

        /// <summary>
        /// Brush for <see cref="Color.DarkViolet"/>
        /// </summary>
        public static Brush DarkViolet { get; } = new SolidColorBrush(Color.DarkViolet);

        /// <summary>
        /// Brush for <see cref="Color.DeepPink"/>
        /// </summary>
        public static Brush DeepPink { get; } = new SolidColorBrush(Color.DeepPink);

        /// <summary>
        /// Brush for <see cref="Color.DeepSkyBlue"/>
        /// </summary>
        public static Brush DeepSkyBlue { get; } = new SolidColorBrush(Color.DeepSkyBlue);

        /// <summary>
        /// Brush for <see cref="Color.DimGray"/>
        /// </summary>
        public static Brush DimGray { get; } = new SolidColorBrush(Color.DimGray);

        /// <summary>
        /// Brush for <see cref="Color.DodgerBlue"/>
        /// </summary>
        public static Brush DodgerBlue { get; } = new SolidColorBrush(Color.DodgerBlue);

        /// <summary>
        /// Brush for <see cref="Color.Firebrick"/>
        /// </summary>
        public static Brush Firebrick { get; } = new SolidColorBrush(Color.Firebrick);

        /// <summary>
        /// Brush for <see cref="Color.FloralWhite"/>
        /// </summary>
        public static Brush FloralWhite { get; } = new SolidColorBrush(Color.FloralWhite);

        /// <summary>
        /// Brush for <see cref="Color.ForestGreen"/>
        /// </summary>
        public static Brush ForestGreen { get; } = new SolidColorBrush(Color.ForestGreen);

        /// <summary>
        /// Brush for <see cref="Color.Fuchsia"/>
        /// </summary>
        public static Brush Fuchsia { get; } = new SolidColorBrush(Color.Fuchsia);

        /// <summary>
        /// Brush for <see cref="Color.Gainsboro"/>
        /// </summary>
        public static Brush Gainsboro { get; } = new SolidColorBrush(Color.Gainsboro);

        /// <summary>
        /// Brush for <see cref="Color.GhostWhite"/>
        /// </summary>
        public static Brush GhostWhite { get; } = new SolidColorBrush(Color.GhostWhite);

        /// <summary>
        /// Brush for <see cref="Color.Gold"/>
        /// </summary>
        public static Brush Gold { get; } = new SolidColorBrush(Color.Gold);

        /// <summary>
        /// Brush for <see cref="Color.Goldenrod"/>
        /// </summary>
        public static Brush Goldenrod { get; } = new SolidColorBrush(Color.Goldenrod);

        /// <summary>
        /// Brush for <see cref="Color.Gray"/>
        /// </summary>
        public static Brush Gray { get; } = new SolidColorBrush(Color.Gray);

        /// <summary>
        /// Brush for <see cref="Color.Green"/>
        /// </summary>
        public static Brush Green { get; } = new SolidColorBrush(Color.Green);

        /// <summary>
        /// Brush for <see cref="Color.GreenYellow"/>
        /// </summary>
        public static Brush GreenYellow { get; } = new SolidColorBrush(Color.GreenYellow);

        /// <summary>
        /// Brush for <see cref="Color.Honeydew"/>
        /// </summary>
        public static Brush Honeydew { get; } = new SolidColorBrush(Color.Honeydew);

        /// <summary>
        /// Brush for <see cref="Color.HotPink"/>
        /// </summary>
        public static Brush HotPink { get; } = new SolidColorBrush(Color.HotPink);

        /// <summary>
        /// Brush for <see cref="Color.IndianRed"/>
        /// </summary>
        public static Brush IndianRed { get; } = new SolidColorBrush(Color.IndianRed);

        /// <summary>
        /// Brush for <see cref="Color.Indigo"/>
        /// </summary>
        public static Brush Indigo { get; } = new SolidColorBrush(Color.Indigo);

        /// <summary>
        /// Brush for <see cref="Color.Ivory"/>
        /// </summary>
        public static Brush Ivory { get; } = new SolidColorBrush(Color.Ivory);

        /// <summary>
        /// Brush for <see cref="Color.Khaki"/>
        /// </summary>
        public static Brush Khaki { get; } = new SolidColorBrush(Color.Khaki);

        /// <summary>
        /// Brush for <see cref="Color.Lavender"/>
        /// </summary>
        public static Brush Lavender { get; } = new SolidColorBrush(Color.Lavender);

        /// <summary>
        /// Brush for <see cref="Color.LavenderBlush"/>
        /// </summary>
        public static Brush LavenderBlush { get; } = new SolidColorBrush(Color.LavenderBlush);

        /// <summary>
        /// Brush for <see cref="Color.LawnGreen"/>
        /// </summary>
        public static Brush LawnGreen { get; } = new SolidColorBrush(Color.LawnGreen);

        /// <summary>
        /// Brush for <see cref="Color.LemonChiffon"/>
        /// </summary>
        public static Brush LemonChiffon { get; } = new SolidColorBrush(Color.LemonChiffon);

        /// <summary>
        /// Brush for <see cref="Color.LightBlue"/>
        /// </summary>
        public static Brush LightBlue { get; } = new SolidColorBrush(Color.LightBlue);

        /// <summary>
        /// Brush for <see cref="Color.LightCoral"/>
        /// </summary>
        public static Brush LightCoral { get; } = new SolidColorBrush(Color.LightCoral);

        /// <summary>
        /// Brush for <see cref="Color.LightCyan"/>
        /// </summary>
        public static Brush LightCyan { get; } = new SolidColorBrush(Color.LightCyan);

        /// <summary>
        /// Brush for <see cref="Color.LightGoldenrodYellow"/>
        /// </summary>
        public static Brush LightGoldenrodYellow { get; } = new SolidColorBrush(Color.LightGoldenrodYellow);

        /// <summary>
        /// Brush for <see cref="Color.LightGray"/>
        /// </summary>
        public static Brush LightGray { get; } = new SolidColorBrush(Color.LightGray);

        /// <summary>
        /// Brush for <see cref="Color.LightGreen"/>
        /// </summary>
        public static Brush LightGreen { get; } = new SolidColorBrush(Color.LightGreen);

        /// <summary>
        /// Brush for <see cref="Color.LightPink"/>
        /// </summary>
        public static Brush LightPink { get; } = new SolidColorBrush(Color.LightPink);

        /// <summary>
        /// Brush for <see cref="Color.LightSalmon"/>
        /// </summary>
        public static Brush LightSalmon { get; } = new SolidColorBrush(Color.LightSalmon);

        /// <summary>
        /// Brush for <see cref="Color.LightSeaGreen"/>
        /// </summary>
        public static Brush LightSeaGreen { get; } = new SolidColorBrush(Color.LightSeaGreen);

        /// <summary>
        /// Brush for <see cref="Color.LightSkyBlue"/>
        /// </summary>
        public static Brush LightSkyBlue { get; } = new SolidColorBrush(Color.LightSkyBlue);

        /// <summary>
        /// Brush for <see cref="Color.LightSlateGray"/>
        /// </summary>
        public static Brush LightSlateGray { get; } = new SolidColorBrush(Color.LightSlateGray);

        /// <summary>
        /// Brush for <see cref="Color.LightSteelBlue"/>
        /// </summary>
        public static Brush LightSteelBlue { get; } = new SolidColorBrush(Color.LightSteelBlue);

        /// <summary>
        /// Brush for <see cref="Color.LightYellow"/>
        /// </summary>
        public static Brush LightYellow { get; } = new SolidColorBrush(Color.LightYellow);

        /// <summary>
        /// Brush for <see cref="Color.Lime"/>
        /// </summary>
        public static Brush Lime { get; } = new SolidColorBrush(Color.Lime);

        /// <summary>
        /// Brush for <see cref="Color.LimeGreen"/>
        /// </summary>
        public static Brush LimeGreen { get; } = new SolidColorBrush(Color.LimeGreen);

        /// <summary>
        /// Brush for <see cref="Color.Linen"/>
        /// </summary>
        public static Brush Linen { get; } = new SolidColorBrush(Color.Linen);

        /// <summary>
        /// Brush for <see cref="Color.Magenta"/>
        /// </summary>
        public static Brush Magenta { get; } = new SolidColorBrush(Color.Magenta);

        /// <summary>
        /// Brush for <see cref="Color.Maroon"/>
        /// </summary>
        public static Brush Maroon { get; } = new SolidColorBrush(Color.Maroon);

        /// <summary>
        /// Brush for <see cref="Color.MediumAquamarine"/>
        /// </summary>
        public static Brush MediumAquamarine { get; } = new SolidColorBrush(Color.MediumAquamarine);

        /// <summary>
        /// Brush for <see cref="Color.MediumBlue"/>
        /// </summary>
        public static Brush MediumBlue { get; } = new SolidColorBrush(Color.MediumBlue);

        /// <summary>
        /// Brush for <see cref="Color.MediumOrchid"/>
        /// </summary>
        public static Brush MediumOrchid { get; } = new SolidColorBrush(Color.MediumOrchid);

        /// <summary>
        /// Brush for <see cref="Color.MediumPurple"/>
        /// </summary>
        public static Brush MediumPurple { get; } = new SolidColorBrush(Color.MediumPurple);

        /// <summary>
        /// Brush for <see cref="Color.MediumSeaGreen"/>
        /// </summary>
        public static Brush MediumSeaGreen { get; } = new SolidColorBrush(Color.MediumSeaGreen);

        /// <summary>
        /// Brush for <see cref="Color.MediumSlateBlue"/>
        /// </summary>
        public static Brush MediumSlateBlue { get; } = new SolidColorBrush(Color.MediumSlateBlue);

        /// <summary>
        /// Brush for <see cref="Color.MediumSpringGreen"/>
        /// </summary>
        public static Brush MediumSpringGreen { get; } = new SolidColorBrush(Color.MediumSpringGreen);

        /// <summary>
        /// Brush for <see cref="Color.MediumTurquoise"/>
        /// </summary>
        public static Brush MediumTurquoise { get; } = new SolidColorBrush(Color.MediumTurquoise);

        /// <summary>
        /// Brush for <see cref="Color.MediumVioletRed"/>
        /// </summary>
        public static Brush MediumVioletRed { get; } = new SolidColorBrush(Color.MediumVioletRed);

        /// <summary>
        /// Brush for <see cref="Color.MidnightBlue"/>
        /// </summary>
        public static Brush MidnightBlue { get; } = new SolidColorBrush(Color.MidnightBlue);

        /// <summary>
        /// Brush for <see cref="Color.MintCream"/>
        /// </summary>
        public static Brush MintCream { get; } = new SolidColorBrush(Color.MintCream);

        /// <summary>
        /// Brush for <see cref="Color.MistyRose"/>
        /// </summary>
        public static Brush MistyRose { get; } = new SolidColorBrush(Color.MistyRose);

        /// <summary>
        /// Brush for <see cref="Color.Moccasin"/>
        /// </summary>
        public static Brush Moccasin { get; } = new SolidColorBrush(Color.Moccasin);

        /// <summary>
        /// Brush for <see cref="Color.NavajoWhite"/>
        /// </summary>
        public static Brush NavajoWhite { get; } = new SolidColorBrush(Color.NavajoWhite);

        /// <summary>
        /// Brush for <see cref="Color.Navy"/>
        /// </summary>
        public static Brush Navy { get; } = new SolidColorBrush(Color.Navy);

        /// <summary>
        /// Brush for <see cref="Color.OldLace"/>
        /// </summary>
        public static Brush OldLace { get; } = new SolidColorBrush(Color.OldLace);

        /// <summary>
        /// Brush for <see cref="Color.Olive"/>
        /// </summary>
        public static Brush Olive { get; } = new SolidColorBrush(Color.Olive);

        /// <summary>
        /// Brush for <see cref="Color.OliveDrab"/>
        /// </summary>
        public static Brush OliveDrab { get; } = new SolidColorBrush(Color.OliveDrab);

        /// <summary>
        /// Brush for <see cref="Color.Orange"/>
        /// </summary>
        public static Brush Orange { get; } = new SolidColorBrush(Color.Orange);

        /// <summary>
        /// Brush for <see cref="Color.OrangeRed"/>
        /// </summary>
        public static Brush OrangeRed { get; } = new SolidColorBrush(Color.OrangeRed);

        /// <summary>
        /// Brush for <see cref="Color.Orchid"/>
        /// </summary>
        public static Brush Orchid { get; } = new SolidColorBrush(Color.Orchid);

        /// <summary>
        /// Brush for <see cref="Color.PaleGoldenrod"/>
        /// </summary>
        public static Brush PaleGoldenrod { get; } = new SolidColorBrush(Color.PaleGoldenrod);

        /// <summary>
        /// Brush for <see cref="Color.PaleGreen"/>
        /// </summary>
        public static Brush PaleGreen { get; } = new SolidColorBrush(Color.PaleGreen);

        /// <summary>
        /// Brush for <see cref="Color.PaleTurquoise"/>
        /// </summary>
        public static Brush PaleTurquoise { get; } = new SolidColorBrush(Color.PaleTurquoise);

        /// <summary>
        /// Brush for <see cref="Color.PaleVioletRed"/>
        /// </summary>
        public static Brush PaleVioletRed { get; } = new SolidColorBrush(Color.PaleVioletRed);

        /// <summary>
        /// Brush for <see cref="Color.PapayaWhip"/>
        /// </summary>
        public static Brush PapayaWhip { get; } = new SolidColorBrush(Color.PapayaWhip);

        /// <summary>
        /// Brush for <see cref="Color.PeachPuff"/>
        /// </summary>
        public static Brush PeachPuff { get; } = new SolidColorBrush(Color.PeachPuff);

        /// <summary>
        /// Brush for <see cref="Color.Peru"/>
        /// </summary>
        public static Brush Peru { get; } = new SolidColorBrush(Color.Peru);

        /// <summary>
        /// Brush for <see cref="Color.Pink"/>
        /// </summary>
        public static Brush Pink { get; } = new SolidColorBrush(Color.Pink);

        /// <summary>
        /// Brush for <see cref="Color.Plum"/>
        /// </summary>
        public static Brush Plum { get; } = new SolidColorBrush(Color.Plum);

        /// <summary>
        /// Brush for <see cref="Color.PowderBlue"/>
        /// </summary>
        public static Brush PowderBlue { get; } = new SolidColorBrush(Color.PowderBlue);

        /// <summary>
        /// Brush for <see cref="Color.Purple"/>
        /// </summary>
        public static Brush Purple { get; } = new SolidColorBrush(Color.Purple);

        /// <summary>
        /// Brush for <see cref="Color.Red"/>
        /// </summary>
        public static Brush Red { get; } = new SolidColorBrush(Color.Red);

        /// <summary>
        /// Brush for <see cref="Color.RosyBrown"/>
        /// </summary>
        public static Brush RosyBrown { get; } = new SolidColorBrush(Color.RosyBrown);

        /// <summary>
        /// Brush for <see cref="Color.RoyalBlue"/>
        /// </summary>
        public static Brush RoyalBlue { get; } = new SolidColorBrush(Color.RoyalBlue);

        /// <summary>
        /// Brush for <see cref="Color.SaddleBrown"/>
        /// </summary>
        public static Brush SaddleBrown { get; } = new SolidColorBrush(Color.SaddleBrown);

        /// <summary>
        /// Brush for <see cref="Color.Salmon"/>
        /// </summary>
        public static Brush Salmon { get; } = new SolidColorBrush(Color.Salmon);

        /// <summary>
        /// Brush for <see cref="Color.SandyBrown"/>
        /// </summary>
        public static Brush SandyBrown { get; } = new SolidColorBrush(Color.SandyBrown);

        /// <summary>
        /// Brush for <see cref="Color.SeaGreen"/>
        /// </summary>
        public static Brush SeaGreen { get; } = new SolidColorBrush(Color.SeaGreen);

        /// <summary>
        /// Brush for <see cref="Color.SeaShell"/>
        /// </summary>
        public static Brush SeaShell { get; } = new SolidColorBrush(Color.SeaShell);

        /// <summary>
        /// Brush for <see cref="Color.Sienna"/>
        /// </summary>
        public static Brush Sienna { get; } = new SolidColorBrush(Color.Sienna);

        /// <summary>
        /// Brush for <see cref="Color.Silver"/>
        /// </summary>
        public static Brush Silver { get; } = new SolidColorBrush(Color.Silver);

        /// <summary>
        /// Brush for <see cref="Color.SkyBlue"/>
        /// </summary>
        public static Brush SkyBlue { get; } = new SolidColorBrush(Color.SkyBlue);

        /// <summary>
        /// Brush for <see cref="Color.SlateBlue"/>
        /// </summary>
        public static Brush SlateBlue { get; } = new SolidColorBrush(Color.SlateBlue);

        /// <summary>
        /// Brush for <see cref="Color.SlateGray"/>
        /// </summary>
        public static Brush SlateGray { get; } = new SolidColorBrush(Color.SlateGray);

        /// <summary>
        /// Brush for <see cref="Color.Snow"/>
        /// </summary>
        public static Brush Snow { get; } = new SolidColorBrush(Color.Snow);

        /// <summary>
        /// Brush for <see cref="Color.SpringGreen"/>
        /// </summary>
        public static Brush SpringGreen { get; } = new SolidColorBrush(Color.SpringGreen);

        /// <summary>
        /// Brush for <see cref="Color.SteelBlue"/>
        /// </summary>
        public static Brush SteelBlue { get; } = new SolidColorBrush(Color.SteelBlue);

        /// <summary>
        /// Brush for <see cref="Color.Tan"/>
        /// </summary>
        public static Brush Tan { get; } = new SolidColorBrush(Color.Tan);

        /// <summary>
        /// Brush for <see cref="Color.Teal"/>
        /// </summary>
        public static Brush Teal { get; } = new SolidColorBrush(Color.Teal);

        /// <summary>
        /// Brush for <see cref="Color.Thistle"/>
        /// </summary>
        public static Brush Thistle { get; } = new SolidColorBrush(Color.Thistle);

        /// <summary>
        /// Brush for <see cref="Color.Tomato"/>
        /// </summary>
        public static Brush Tomato { get; } = new SolidColorBrush(Color.Tomato);

        /// <summary>
        /// Brush for <see cref="Color.Transparent"/>
        /// </summary>
        public static Brush Transparent { get; } = new SolidColorBrush(Color.Transparent);

        /// <summary>
        /// Brush for <see cref="Color.Turquoise"/>
        /// </summary>
        public static Brush Turquoise { get; } = new SolidColorBrush(Color.Turquoise);

        /// <summary>
        /// Brush for <see cref="Color.Violet"/>
        /// </summary>
        public static Brush Violet { get; } = new SolidColorBrush(Color.Violet);

        /// <summary>
        /// Brush for <see cref="Color.Wheat"/>
        /// </summary>
        public static Brush Wheat { get; } = new SolidColorBrush(Color.Wheat);

        /// <summary>
        /// Brush for <see cref="Color.White"/>
        /// </summary>
        public static Brush White { get; } = new SolidColorBrush(Color.White);

        /// <summary>
        /// Brush for <see cref="Color.WhiteSmoke"/>
        /// </summary>
        public static Brush WhiteSmoke { get; } = new SolidColorBrush(Color.WhiteSmoke);

        /// <summary>
        /// Brush for <see cref="Color.Yellow"/>
        /// </summary>
        public static Brush Yellow { get; } = new SolidColorBrush(Color.Yellow);

        /// <summary>
        /// Brush for <see cref="Color.YellowGreen"/>
        /// </summary>
        public static Brush YellowGreen { get; } = new SolidColorBrush(Color.YellowGreen);
        #endregion

        private readonly Color _color;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolidColorBrush"/> class.
        /// </summary>
        /// <param name="color">The color to be used for this brush.</param>
        public SolidColorBrush(Color color)
        {
            _color = color;
        }

        /// <inheritdoc/>
        public override void Draw(SpriteBatch batch, Rectangle area, float alpha)
        {
            batch.Draw(Skin.Pix, area, _color);
        }
    }
}
