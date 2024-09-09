using UnityEngine;

namespace MathBuddy.Colors
{
    /// <summary>
    /// Extension class for color related methods and helper tools.
    /// </summary>
    public static class ColorsExtensions
    {
        private const int RED = 0, GREEN = 1, BLUE = 2;

        /// <summary>
        /// Convert rgb value to hue saturation luminance value
        /// </summary>
        public static HSLAColor RGBToHSL(Color rgb)
        {
            HSLAColor hsl = HSLAColor.black;
            hsl.a = rgb.a;

            int min, max;
            //find index of lowest and biggest value
            min = (rgb.r < rgb.b) ? (rgb.r < rgb.g) ? RED : GREEN : (rgb.b < rgb.g) ? BLUE : GREEN;
            max = (rgb.r > rgb.b) ? (rgb.r > rgb.g) ? RED : GREEN : (rgb.b > rgb.g) ? BLUE : GREEN;

            //luminance
            hsl.l = (rgb[min] + rgb[max]) / 2.0f;
            //saturation
            if (rgb[min] == rgb[max]) {
                //greyscale value
                hsl.s = 0.0f;
            } else if (hsl.l < 0.5f) {
                hsl.s = (rgb[max] - rgb[min]) / (rgb[max] + rgb[min]);
            } else {
                hsl.s = (rgb[max] - rgb[min]) / (2.0f - rgb[max] - rgb[min]);
            }

            switch (max) {
                case RED:
                    hsl.h = (rgb[GREEN] - rgb[BLUE]) / (rgb[max] - rgb[min]);
                    break;
                case GREEN:
                    hsl.h = 2.0f + (rgb[BLUE] - rgb[RED]) / (rgb[max] - rgb[min]);
                    break;
                case BLUE:
                    hsl.h = 4.0f + (rgb[RED] - rgb[GREEN]) / (rgb[max] - rgb[min]);
                    break;
                default:
                    Debug.LogError($"Wrong maximum value in HSV conversion for ({rgb})");
                    return HSLAColor.error;
            }

            hsl.h *= 60;
            if (hsl.h < 0) {
                hsl.h += 360f;
            }

            return hsl;
        }
    }

    /// <summary>
    /// Hue, Saturation, Luminocity, Alpha, color.
    /// </summary>
    public struct HSLAColor
    {
        public static HSLAColor black = new HSLAColor(0f, 0f, 0f, 1f);
        public static HSLAColor error = new HSLAColor(-1f, -1f, -1f, -1f);

        public HSLAColor(float h, float s, float l, float a)
        {
            this.h = h;
            this.s = s;
            this.l = l;
            this.a = a;
        }

        //Checks whether the saturation is low (black-greyish color) and whether the hue is in the red/blue spectrum, dark hue color.
        public bool IsColorDark()
        {
            if (s < 0.5f) {
                return l < 0.4f;
            }
            else {
                if (l > 0.4f && l < 0.6f) {
                    return !(h > 20f && h < 210f);
                }
                else {
                    return l < 0.5f;
                }
            }
        }



        public float h, s, l, a;

        public override string ToString()
        {
            return $"HSLA({h},{s},{l},{a})";
        }
    }
}
