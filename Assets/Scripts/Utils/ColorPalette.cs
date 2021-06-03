using UnityEngine;

namespace Fluid
{
    public static class ColorPalette
    {
        public static Color BLACK = new Color(0,0,0);
        public static Color BLUE = new Color(45 / 255f, 145 / 255f, 220 / 255f);
        public static Color GREEN = new Color(45 / 255f, 110 / 255f, 50 / 255f);
        public static Color CYAN = new Color(120 / 255f, 140 / 255f, 150 / 255f);
        public static Color RED = new Color(100 / 255f, 5 / 255f, 5 / 255f);
        public static Color MAGENTA = new Color(100 / 255f, 35 / 255f, 105 / 255f);
        public static Color BROWN = new Color(95 / 255f, 70 / 255f, 40 / 255f);
        public static Color LGRAY = new Color(160 / 255f, 160 / 255f, 160 / 255f);
        public static Color DGRAY = new Color(72 / 255f, 69 / 255f, 60 / 255f);
        public static Color LBLUE = new Color(135 / 255f, 150 / 255f, 185 / 255f);
        public static Color LGREEN = new Color(130 / 255f, 165 / 255f, 105 / 255f);
        public static Color LCYAN = new Color(170 / 255f, 192 / 255f, 205 / 255f);
        public static Color LRED = new Color(145 / 255f, 30 / 255f, 35 / 255f);
        public static Color LMAGENTA = new Color(140 / 255f, 20 / 255f, 160 / 255f);
        public static Color YELLOW = new Color(180 / 255f, 180 / 255f, 115 / 255f);
        public static Color WHITE = new Color(1, 1, 1);

        public static Color[] Colors = new Color[]
        {
            BLACK,
            BLUE,
            GREEN,
            CYAN,
            RED,
            MAGENTA,
            BROWN,
            LGRAY,
            DGRAY,
            LBLUE,
            LGREEN,
            LCYAN,
            LRED,
            LMAGENTA,
            YELLOW,
            WHITE
        };

        public static Color GetColor(ColorIndex index)
        {
            return Colors[(int) index];
        }
    }

    public enum ColorIndex
    {
        BLACK,
        BLUE,
        GREEN,
        CYAN,
        RED,
        MAGENTA,
        BROWN,
        LGRAY,
        DGRAY,
        LBLUE,
        LGREEN,
        LCYAN,
        LRED,
        LMAGENTA,
        YELLOW,
        WHITE
    }
}