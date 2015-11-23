using UnityEngine;

public struct AxisAlignedRectangle
{
    private Vector2 position;
    private Vector2 size;

    public Vector2 Position
    {
        get { return position; }
    }

    public Vector2 Size
    {
        get { return size; }
    }

    public AxisAlignedRectangle(Vector2 position, float size)
    {
        this.position = position;
        this.size = new Vector2(size, size);
    }

    public AxisAlignedRectangle(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
    }

    public bool Inner(Vector2 v)
    {
        return
            Mathf.Abs(v.x - Position.x) < Size.x / 2 &&
            Mathf.Abs(v.y - Position.y) < Size.y / 2;
    }

    public bool Contains(Vector2 v)
    {
        return
            Mathf.Abs(v.x - Position.x) <= Size.x / 2 &&
            Mathf.Abs(v.y - Position.y) <= Size.y / 2;
    }

    public float Distance(Vector2 v)
    {
        v -= Position;
        float dx = Mathf.Max(Mathf.Abs(v.x) - Size.x / 2, 0);
        float dy = Mathf.Max(Mathf.Abs(v.y) - Size.y / 2, 0);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public AxisAlignedRectangle Split(Quadrant quadrant)
    {
        Vector2 quarterSize = Size / 4;
        Vector2 halfSize = Size / 2;
        Vector2 center = Position;
        switch (quadrant)
        {
            case Quadrant.BottomLeft:
                center += new Vector2(-quarterSize.x, -quarterSize.y);
                break;

            case Quadrant.TopLeft:
                center += new Vector2(-quarterSize.x, quarterSize.y);
                break;

            case Quadrant.BottomRight:
                center += new Vector2(quarterSize.x, -quarterSize.y);
                break;

            case Quadrant.TopRight:
                center += new Vector2(quarterSize.x, quarterSize.y);
                break;
        }
        return new AxisAlignedRectangle(center, halfSize);
    }
}

public enum Quadrant
{
    BottomLeft = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 3
}
