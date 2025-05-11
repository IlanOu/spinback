using UnityEngine;

public static class Anchors
{
    public enum AnchorType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public class Anchor
    {
        public Vector2 min;
        public Vector2 max;
        public Anchor(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public static readonly Anchor TopLeft = new Anchor(new Vector2(0, 1), new Vector2(0, 1));
    public static readonly Anchor TopRight = new Anchor(new Vector2(1, 1), new Vector2(1, 1));
    public static readonly Anchor BottomLeft = new Anchor(new Vector2(0, 0), new Vector2(0, 0));
    public static readonly Anchor BottomRight = new Anchor(new Vector2(1, 0), new Vector2(1, 0));
    public static readonly Vector2 defaultPivot = new Vector2(0.5f, 0.5f);

    public static Anchor GetAnchor(AnchorType type)
    {
        switch (type)
        {
            case AnchorType.TopLeft:
                return TopLeft;
            case AnchorType.TopRight:
                return TopRight;
            case AnchorType.BottomLeft:
                return BottomLeft;
            case AnchorType.BottomRight:
                return BottomRight;
            default:
                return TopLeft;
        }
    }
}