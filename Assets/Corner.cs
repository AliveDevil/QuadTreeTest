using System;
using UnityEngine;

public class Corner
{
    public event EventHandler EnabledChanged;

    public event EventHandler HeightChanged;

    public event EventHandler ReferencesChanged;

    private bool enabled;
    private float? height;
    private int references;

    public bool Enabled
    {
        get
        {
            return enabled;
        }
        private set
        {
            if (enabled == value) return;
            enabled = value;
            if (EnabledChanged != null)
                EnabledChanged(this, EventArgs.Empty);
        }
    }

    public float Height
    {
        get
        {
            if (!height.HasValue)
            {
                height = QuadTree.Sample(Position);
                if (HeightChanged != null)
                    HeightChanged(this, EventArgs.Empty);
            }

            return height.Value;
        }
    }

    public Vector2 Position { get; set; }

    public int References
    {
        get
        {
            return references;
        }
        set
        {
            if (references == value) return;
            references = Mathf.Max(0, value);
            if (ReferencesChanged != null)
                ReferencesChanged(this, EventArgs.Empty);
            Enabled = References > 0;
        }
    }

    public QuadTree QuadTree { get; set; }

    public void Reset()
    {
        height = null;
        if (HeightChanged != null)
            HeightChanged(this, EventArgs.Empty);
    }
}
