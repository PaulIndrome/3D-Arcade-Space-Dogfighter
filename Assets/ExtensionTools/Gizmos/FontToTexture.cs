#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace ExtensionTools.Text
{
    public class FontToTexture
    {
        //private void Start()
        //{
        //    //string ascii = "";
        //    //for (byte i = 32; i < 255; i++)
        //    //{
        //    //    ascii += (char)i;
        //    //}

        //}

        public struct Quad
        {
            public Vector2 LeftBottom;
            public Vector2 LeftUpper;
            public Vector2 RightUpper;
            public Vector2 RightBottom;

            public Vector2 UVLB;
            public Vector2 UVLU;
            public Vector2 UVRB;
            public Vector2 UVRU;
        }
        public struct TextData
        {
            public List<Quad> Quads;
            public float MinX;
            public float MaxX;
            public float MinY;
            public float MaxY;
        }
        public static TextData GenerateFromText(string text, float fontsize)
        {
            

            TextData textData = new TextData();
            textData.Quads = new List<Quad>();

            if (text == null)
                return textData;
            if (text.Length == 0)
                return textData;

            float horizontalMultiplier = 0.5f;
            int rowCount = 16;


            //Split lines
            string[] lines = text.Split('\n');

            float YOffset = lines.Length * fontsize / 2f;
            float[] XOffsets = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            { 
                XOffsets[i]= -((float)lines[i].Length * fontsize * horizontalMultiplier) / 2.0f;
                if (XOffsets[i] < textData.MinX)
                {
                    textData.MinX = XOffsets[i]-fontsize;
                    textData.MaxX = -XOffsets[i]+fontsize;
                }
            }


            int CharIndex = 0;
            int LineIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c.Equals('\n'))
                {
                    LineIndex++;
                    CharIndex = 0;
                    continue;
                }


                /*RECT*/

                Vector2 VerticalLineOffset = new Vector2(0f, -LineIndex * fontsize+ YOffset);
                Vector2 LeftBottom, LeftUpper, RightUpper, RightBottom;

                float Left = CharIndex * fontsize * horizontalMultiplier + XOffsets[LineIndex];
                float Right = Left + fontsize;
                float Upper = fontsize / 2.0f;
                float Bottom = -fontsize / 2.0f;

                LeftBottom = new Vector2(Left, Bottom)+ VerticalLineOffset;
                LeftUpper = new Vector2(Left, Upper)+ VerticalLineOffset;
                RightBottom = new Vector2(Right, Bottom)+ VerticalLineOffset;
                RightUpper = new Vector2(Right, Upper)+ VerticalLineOffset;

                Quad quad = new Quad();
                quad.LeftBottom = LeftBottom;
                quad.LeftUpper = LeftUpper;
                quad.RightBottom = RightBottom;
                quad.RightUpper = RightUpper;


                /*UV*/
                int indexofchar = (char)c;
                indexofchar -= 32;

                float xIndex = indexofchar % (rowCount);
                float yIndex = indexofchar / (rowCount);

                Vector2 position = new Vector2(xIndex / (float)rowCount, 1.0f - (yIndex) / (float)rowCount);
                float CellSize = 1f / (float)rowCount;
                position.y -= CellSize;

                quad.UVLB = position;
                quad.UVLU = position + new Vector2(0, CellSize);
                quad.UVRB = position + new Vector2(CellSize, 0);
                quad.UVRU = position + new Vector2(CellSize, CellSize);

                textData.Quads.Add(quad);

                CharIndex++;
            }


            textData.MaxY = fontsize / 2f+ YOffset;
            textData.MinY = textData.Quads[textData.Quads.Count-1].LeftBottom.y;

            return textData;
        }

        static Dictionary<string, Texture> m_LoadedFonts = new Dictionary<string, Texture>();
        public static Texture GetFontAtlas(string font)
        {
            if (m_LoadedFonts.ContainsKey(font))
                return m_LoadedFonts[font];

            string Path = PathToAsset.GetPathToExtensionToolsAsset() + "/Textures/Fonts/" + font.ToString() + "TextureAtlas.png";
            var Texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(Path);

            m_LoadedFonts.Add(font, Texture);
            return Texture;
        }
    }
}
#endif