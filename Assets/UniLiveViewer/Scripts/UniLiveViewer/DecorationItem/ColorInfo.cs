using UnityEngine;

namespace UniLiveViewer
{
    public enum ColorInfo
    {
        White,
        Yellow,
        YellowGreen,
        Green,
        SkyBlue,
        Blue,
        Purple,
        Pink,
        Red,
        Orange
    }

    public static class ColorInfoExtension
    {
        public static ColorInfo ToColorInfo(this string colorName)
        {
            return colorName switch
            {
                "w" => ColorInfo.White,
                "y" => ColorInfo.Yellow,
                "g" => ColorInfo.Green,
                "sk" => ColorInfo.SkyBlue,
                "b" => ColorInfo.Blue,
                "pu" => ColorInfo.Purple,
                "pk" => ColorInfo.Pink,
                "r" => ColorInfo.Red,
                "o" => ColorInfo.Orange,
                _ => ColorInfo.White,
            };
        }

        public static Color ToColor(this ColorInfo colorInfo)
        {
            return colorInfo switch
            {
                ColorInfo.White => Color.white,
                ColorInfo.Yellow => Color.yellow,
                ColorInfo.Green => Color.green,
                ColorInfo.SkyBlue => Color.cyan,
                ColorInfo.Blue => Color.blue,
                ColorInfo.Purple => new Color(0.5f, 0.0f, 1.0f),
                ColorInfo.Pink => new Color(1.0f, 0.0f, 0.8f),
                ColorInfo.Red => Color.red,
                ColorInfo.Orange => new Color(1.0f, 0.5f, 0.0f),
                _ => Color.white,
            };
        }
    }
}