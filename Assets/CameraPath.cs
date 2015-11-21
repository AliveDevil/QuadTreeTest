using UnityEngine;

public class CameraPath : MonoBehaviour
{
    public CameraPath Follow;
    public QuadTree QuadTree;
    public float Speed;
    public Vector2[] Spline;
    private float length;
    private float time;

    public static long GetBinCoeff(long N, long K)
    {
        // This function gets the total number of unique combinations based upon N and K.
        // N is the total number of items.
        // K is the size of the group.
        // Total number of unique combinations = N! / ( K! (N - K)! ).
        // This function is less efficient, but is more likely to not overflow when N and K are large.
        // Taken from:  http://blog.plover.com/math/choose.html
        //
        long r = 1;
        long d;
        if (K > N) return 0;
        for (d = 1; d <= K; d++)
        {
            r *= N--;
            r /= d;
        }
        return r;
    }

    public void OnDrawGizmos()
    {
        length = 0;
        for (int i = 0; i < Spline.Length - 1; i++)
            length += (Spline[i] - Spline[i + 1]).magnitude;
        float step = length / 20;
        Gizmos.color = Color.gray;
        for (int i = 0; i < 20; i++)
        {
            Gizmos.DrawLine(Sample(step * i).X0Z(), Sample(step * (i + 1)).X0Z());
        }
    }

    public void OnDrawGizmosSelected()
    {
        length = 0;
        for (int i = 0; i < Spline.Length - 1; i++)
            length += (Spline[i] - Spline[i + 1]).magnitude;
        float step = length / 20;
        Gizmos.color = Color.white;
        for (int i = 0; i < 20; i++)
        {
            Gizmos.DrawLine(Sample(step * i).X0Z(), Sample(step * (i + 1)).X0Z());
        }
    }

    private void Awake()
    {
        if (Follow) return;
        length = 0;
        for (int i = 0; i < Spline.Length - 1; i++)
            length += (Spline[i] - Spline[i + 1]).magnitude;
    }

    private Vector2 Sample(float distance)
    {
        float t = distance / length;

        Vector2 result = Vector2.zero;
        for (int i = 0; i < Spline.Length; i++)
        {
            long coeff = GetBinCoeff(Spline.Length - 1, i);
            result += coeff * Mathf.Pow(1 - t, Spline.Length - i - 1) * Mathf.Pow(t, i) * Spline[i];
        }
        return result;
    }

    private void Start()
    {
        if (!Follow) return;
        length = Follow.length;
    }

    private void Update()
    {
        if (Follow)
            time = Follow.time;
        else
        {
            time += Speed * Time.deltaTime;
            if (time > length)
                time = length;
        }
        Vector2 v = Sample(time);
        Vector3 lastPosition = transform.position;
        Vector3 nextPosition = v.XYZ(QuadTree.Sample(v) + 10);
        if (Follow)
            transform.position = nextPosition;
        else
            transform.Translate((nextPosition - lastPosition).X0Z() + Vector3.up * (nextPosition.y - lastPosition.y) * Time.deltaTime / 2, Space.World);
    }
}
