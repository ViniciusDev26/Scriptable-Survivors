using UnityEngine;
using UnityEngine.UI;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Monta elementos de interface por código, como o resto da cena.
    /// Image sem sprite desenha um retângulo com a cor dada — é o bastante
    /// para cartas e painéis, e não exige nenhum asset de imagem.
    /// </summary>
    internal static class UiBuilder
    {
        public static RectTransform Panel(string name, Transform parent, Color color)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            host.transform.SetParent(parent, worldPositionStays: false);
            host.GetComponent<Image>().color = color;
            return host.GetComponent<RectTransform>();
        }

        public static Text Label(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            TextAnchor anchor,
            Color color)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            host.transform.SetParent(parent, worldPositionStays: false);

            var label = host.GetComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = anchor;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }
    }
}
