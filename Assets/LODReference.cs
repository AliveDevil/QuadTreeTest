using UnityEngine;

public class LODReference : MonoBehaviour
{
    public Vector2 Position;
    public AnimationCurve LoDCurve;

    public int LoD(AxisAlignedRectangle bounds)
    {
        if (bounds.Contains(Position)) return Helper.LevelsOfDetail;
        float distance = bounds.Distance(Position);
        return (int)(LoDCurve.Evaluate(distance / Helper.ViewDistance) * Helper.LevelsOfDetail);
        //return Mathf.FloorToInt(Mathf.Clamp01(1 - (distance / Helper.ViewDistance) + 0.01f) * 15);
    }

    private void Update()
    {
        Position.x = transform.position.x;
        Position.y = transform.position.z;
    }
}
