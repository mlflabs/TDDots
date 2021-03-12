using UnityEngine;

namespace Mlf.Utils
{
    public class UtilsText
    {
        public const int sortingOrderDefault = 5000;

        // Create Text in the World
        public static TextMesh CreateWorldText(string text,
                                                Transform parent = null,
                                                Vector3 localPosition = default(Vector3),
                                                int fontSize = 40,
                                                Color? color = null,
                                                TextAnchor textAnchor = TextAnchor.UpperLeft,
                                                TextAlignment textAlignment = TextAlignment.Left,
                int sortingOrder = sortingOrderDefault)
        {
            if (color == null) color = Color.white;
            //parent.localScale = new Vector3(1f, 1f, 1f);
            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
        }

        // Create Text in the World
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localScale = new Vector3(0.3f, 0.3f, 3f);
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        public static TextMesh CreateWorldText(string text,
                                                Vector3 localPosition,
                                                int fontSize,
                                                Color color,
                                                TextAnchor textAnchor = TextAnchor.UpperLeft,
                                                TextAlignment textAlignment = TextAlignment.Left)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.localPosition = localPosition;
            //transform.localScale  -= new Vector3(0.5f, 0.5f, 0f);
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.transform.localScale = new Vector3(0.3f, 0.3f, 3f);
            //textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }





    }

}
