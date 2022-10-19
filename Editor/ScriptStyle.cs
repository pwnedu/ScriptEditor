using UnityEngine;

namespace pwnedu.ScriptEditor
{
    [CreateAssetMenu(menuName = "Unity Tools/Script Editor/Script Editor Style", order = 1)]
    public class ScriptStyle : ScriptableObject
    {
        public bool lineDisplay;
        public bool countDisplay;

        public StyleData style;
    }

    [System.Serializable]
    public struct StyleData
    {
        [Header("Border Background Style")]
        [SerializeField] [Tooltip("Increase the alpha channel to see the background.")] private Color backgroundColor;
        [SerializeField] private bool randomBackgroundColor;
        public Color BackgroundColor => randomBackgroundColor ? Random.ColorHSV() : backgroundColor;

        [Header("Header Background Style")]
        [SerializeField][Tooltip("Increase the alpha channel to see the background.")] private Color headerColor;
        [SerializeField] private bool randomHeaderColor;
        public Color HeaderColor => randomHeaderColor ? Random.ColorHSV() : headerColor;

        [Header("Header Text Style")]
        [SerializeField] [Range(0, 18), Tooltip("Keep it 0 for default size. Max is 16 as per the range")] private int textSize;
        [SerializeField] [Tooltip("Increase the alpha channel to see the text.")] private Color32 textColor;
        [SerializeField] private TextAnchor textAlignment;
        [SerializeField] private FontStyle textStyle;
        public GUIStyle TextStyle => new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = textColor },
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(4, 0, 0, 0),
            alignment = textAlignment,
            clipping = TextClipping.Clip,
            fontStyle = textStyle,
            fixedHeight = 20,
            richText = true,
            fontSize = textSize
        };

        [Header("Line Number Text Style")]
        [SerializeField] [Range(0, 10), Tooltip("Keep it 0 for default size. Max is 16 as per the range")] private int numberSize;
        [SerializeField] [Tooltip("Increase the alpha channel to see the text.")] private Color32 numberColor;
        //[SerializeField] private TextAnchor numbertAlignment;
        [SerializeField] private FontStyle numberStyle;

        public GUIStyle NumberStyle => new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = numberColor },
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleRight,
            clipping = TextClipping.Clip,
            fontStyle = numberStyle,
            fixedHeight = 16,
            richText = true,
            fontSize = numberSize
        };

        [Header("Footer Text Style")]
        [SerializeField] [Range(0, 12), Tooltip("Keep it 0 for default size. Max is 16 as per the range")] private int footerSize;
        [SerializeField] [Tooltip("Increase the alpha channel to see the text.")] private Color32 footerColor;
        //[SerializeField] private TextAnchor footerAlignment;
        [SerializeField] private FontStyle footerStyle;

        public GUIStyle FooterStyle => new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = footerColor },
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 2),
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            fontStyle = footerStyle,
            fixedHeight = 15,
            richText = true,
            fontSize = footerSize
        };

        [Header("Horizontal Bar Style")]
        [Tooltip("Increase the alpha channel to see the text.")] public Color32 lineColor;
        //GUIStyle HorizontalLine = new GUIStyle()
        //{
        //    HorizontalLine = new GUIStyle();
        //    HorizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        //    HorizontalLine.margin = new RectOffset(0, 0, 4, 4);
        //    HorizontalLine.fixedHeight = 1;
        //};
    }
}