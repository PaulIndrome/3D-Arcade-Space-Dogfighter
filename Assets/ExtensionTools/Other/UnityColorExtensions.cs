using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Animations;

namespace ExtensionTools
{
    public static class UnityColorExtensions
    {
        public static void Copy(this ref Color color, Color other)
        {
            color.r = other.r;
            color.g = other.g;
            color.b = other.b;
            color.a = other.a;
        }
        public static void ChangeSaturation(this ref Color color, float saturation)
        {
            float H,S,V;
            Color.RGBToHSV(color, out H, out S, out V);

            S += saturation;
            S = Mathf.Clamp01(S);

            color.Copy(Color.HSVToRGB(H, S, V));
        }

        public static void ChangeBrightness(this ref Color color, float brightness)
        {
            float H, S, V;
            Color.RGBToHSV(color, out H, out S, out V);

            V += brightness;
            V = Mathf.Clamp01(V);

            color.Copy(Color.HSVToRGB(H, S, V));
        }

        public static void ShiftHue(this ref Color color, float hue)
        {
            float H, S, V;
            Color.RGBToHSV(color, out H, out S, out V);

            H += hue;

            if (H < 0f)
                H = 1.0f + H;
            H %= 1.0f;

            color.Copy(Color.HSVToRGB(H, S, V));
        }

        public static float GetLuminance(this Color color)
        {
            return (0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b) * (color.a);
        }
    }
}
