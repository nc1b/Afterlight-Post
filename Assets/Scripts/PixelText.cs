using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Pixel Text")]
public class PixelText : MaskableGraphic
{
    const int CellW = 6, CellH = 9, GlyphW = 5, Columns = 16, Rows = 4;

    public Texture2D fontTexture;
    [TextArea(1, 4)] [SerializeField] string text = "";
    public int scale = 1;
    public TextAnchor anchor = TextAnchor.UpperLeft;
    public bool shadow = true;
    public Color shadowColour = new Color(0.03f, 0.05f, 0.1f, 0.9f);

    public override Texture mainTexture
    {
        get { return fontTexture != null ? fontTexture : s_WhiteTexture; }
    }

    public string Text
    {
        get { return text; }
        set
        {
            if (text == value) return;
            text = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (string.IsNullOrEmpty(text)) return;

        string[] lines = text.ToUpperInvariant().Split('\n');
        Rect rect = rectTransform.rect;
        float blockHeight = lines.Length * CellH * scale;

        float top;
        if (anchor <= TextAnchor.UpperRight) top = rect.yMax;
        else if (anchor <= TextAnchor.MiddleRight) top = rect.center.y + blockHeight / 2f;
        else top = rect.yMin + blockHeight;
        top = Mathf.Round(top);

        if (shadow)
        {
            Color sc = shadowColour;
            sc.a *= color.a;
            DrawLines(vh, lines, rect, top, scale, -scale, sc);
        }
        DrawLines(vh, lines, rect, top, 0, 0, color);
    }

    void DrawLines(VertexHelper vh, string[] lines, Rect rect, float top, float dx, float dy, Color32 tint)
    {
        int column = (int)anchor % 3;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            float lineWidth = (line.Length * CellW - 1) * scale;
            float x = rect.xMin;
            if (column == 1) x = rect.center.x - lineWidth / 2f;
            else if (column == 2) x = rect.xMax - lineWidth;
            x = Mathf.Round(x) + dx;
            float y = top - i * CellH * scale + dy;

            foreach (char ch in line)
            {
                int index = ch - 32;
                if (index < 0 || index >= Columns * Rows) index = 31;  
                if (index != 0) AddGlyph(vh, index, x, y, tint);
                x += CellW * scale;
            }
        }
    }

    void AddGlyph(VertexHelper vh, int index, float x, float y, Color32 tint)
    {
        int col = index % Columns, row = index / Columns;
        float w = Columns * CellW, h = Rows * CellH;
        float u0 = col * CellW / w, u1 = (col * CellW + GlyphW) / w;
        float v1 = 1f - row * CellH / h, v0 = 1f - (row + 1) * CellH / h;
        float x1 = x + GlyphW * scale, y0 = y - CellH * scale;

        int start = vh.currentVertCount;
        vh.AddVert(new Vector3(x, y0), tint, new Vector2(u0, v0));
        vh.AddVert(new Vector3(x, y), tint, new Vector2(u0, v1));
        vh.AddVert(new Vector3(x1, y), tint, new Vector2(u1, v1));
        vh.AddVert(new Vector3(x1, y0), tint, new Vector2(u1, v0));
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
