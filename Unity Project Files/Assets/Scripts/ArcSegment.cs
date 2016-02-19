using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArcSegment : MonoBehaviour
{

    public float radius;
    public float angle;
    public float width;
    public float startAngle;
    public float endAngle;
    public Vector3 endPos;

    public float totalDistance;
    public float distanceOffset;
    public float slope;

    public bool flipped = true;

    private Vector3 center;

    public int arcDivisions = 24;
    public int widthDivisions = 2;
    public float uvOffset = 0;
    public float textureScale = 4;

    public Mesh mesh;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    public ArcSegment nextArc;

    public void Initialize()
    {
        if (!flipped)
        {
            startAngle = NormalizeAngle(startAngle);
            endAngle = NormalizeAngle(startAngle + angle);
        } else
        {
            startAngle = NormalizeAngle(-startAngle);
            endAngle = NormalizeAngle(-startAngle - angle);
        }
        center = GetCenter();
        totalDistance = angle / 180 * Mathf.PI * radius;
        endPos = ArcToWorld(new ArcPoint(angle + startAngle, radius));
    }

    public void ResetArc()
    {
        mesh.Clear();
        distanceOffset = 0;
        transform.position = Vector3.zero;
        center = Vector3.zero;
        startAngle = 0;
        endPos = Vector3.zero;
        nextArc = null;
    }

    public void GenerateMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        vertices = new Vector3[(arcDivisions + 1) * (widthDivisions + 1)];
        uv = new Vector2[vertices.Length];

        float arcStep = angle / arcDivisions;
        float widthStep = width / widthDivisions;
        ArcPoint nextVertex = new ArcPoint(0, 0);
        Vector2 nextUV = Vector2.zero;

        if (!flipped)
        {
            for (int t = 0; t <= arcDivisions; t++)
            {
                for (int w = 0; w <= widthDivisions; w++)
                {
                    nextVertex.Set(t * arcStep + startAngle, (w * widthStep - width / 2) + radius);
                    vertices[t * (widthDivisions + 1) + w] = ArcToLocal(nextVertex);

                    nextUV.Set((float)w / widthDivisions / textureScale, (float)t / arcDivisions * totalDistance / textureScale + uvOffset);
                    uv[t * (widthDivisions + 1) + w] = nextUV;
                }
            }
        }
        else
        {
            for (int t = 0; t <= arcDivisions; t++)
            {
                for (int w = 0; w <= widthDivisions; w++)
                {
                    nextVertex.Set(t * arcStep + startAngle, (w * -widthStep + width / 2) + radius);
                    vertices[t * (widthDivisions + 1) + w] = ArcToLocal(nextVertex);

                    nextUV.Set((float)w / widthDivisions / textureScale, (float)t / arcDivisions * totalDistance / textureScale + uvOffset);
                    uv[t * (widthDivisions + 1) + w] = nextUV;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        triangles = new int[arcDivisions * widthDivisions * 6];

        for (int ti = 0, vi = 0, t = 0; t < arcDivisions; t++, vi++)
        {
            for (int w = 0; w < widthDivisions; w++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + widthDivisions + 1;
                triangles[ti + 5] = vi + widthDivisions + 2;

                mesh.triangles = triangles;
            }
        }

        mesh.RecalculateNormals();
    }

    public bool ContainsPoint(Vector3 point, float margin)
    {
        ArcPoint p = WorldToArc(point);
        float rp = NormalizeAngle(p.t - startAngle);
        if (rp >= 0 && rp <= angle && p.d <= radius + width / 2 + margin && p.d >= radius - width / 2 - margin)
        {
            return true;
        }
        return false;
    }

    public float GetAngle(Vector3 point)
    {
        float raw = Mathf.Atan2(point.z, point.x);
        if (flipped)
        {
            return -raw * Mathf.Rad2Deg + 180;
        }
        return raw * Mathf.Rad2Deg;
    }

    float NormalizeAngle(float angle)
    {
        // expresses negative angles and angles over 360 degrees as their equivalent angles from 0-360 degrees

        while (angle < 0)
        {
            angle += 360;
        }
        while (angle >= 360)
        {
            angle -= 360;
        }

        return angle;
    }

    public ArcPoint WorldToArc(Vector3 world)
    {
        //*
        float t = NormalizeAngle(GetAngle(world.getFlat() - center.getFlat()));
        float d = (world.getFlat() - center.getFlat()).magnitude;
        //*/
        /*
        float t = NormalizeAngle(GetAngle(world - center));
        float d = (world - center).magnitude;
        */

        return new ArcPoint(t, d);
    }

    public Vector3 ArcToLocal(ArcPoint arcPoint)
    {
        float tr = arcPoint.t * Mathf.Deg2Rad;
        float d = arcPoint.d;
        if (flipped)
        {
            tr = (-arcPoint.t + 180) * Mathf.Deg2Rad;
        }
        return new Vector3(d * Mathf.Cos(tr), GetLocalHeight(arcPoint.t), d * Mathf.Sin(tr)) + center - transform.position;
    }

    public Vector3 ArcToWorld(ArcPoint arcPoint)
    {
        float tr = arcPoint.t * Mathf.Deg2Rad;
        float d = arcPoint.d;
        if (flipped)
        {
            tr = (-arcPoint.t + 180) * Mathf.Deg2Rad;
        }
        return new Vector3(d * Mathf.Cos(tr), GetHeight(arcPoint.t), d * Mathf.Sin(tr)) + center;
    }

    public float GetDistance(float t)
    {
        float relative = NormalizeAngle(t - startAngle);
        return relative / angle * totalDistance + distanceOffset;
    }

    public float GetHeight(float t)
    {
        return GetDistance(t) * slope;
    }

    public float GetLocalHeight(float t)
    {
        return (GetDistance(t) - distanceOffset) * slope;
    }

    Vector3 GetCenter()
    {
        float tr = (startAngle + 180) * Mathf.Deg2Rad;
        if (flipped)
        {
            tr = -startAngle * Mathf.Deg2Rad;
        }
        return new Vector3(radius * Mathf.Cos(tr), 0f, radius * Mathf.Sin(tr)) + transform.position;
    }

    /*
    //DEBUGGING GIZMOS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (vertices == null)
        {
            return;
        }
        Gizmos.DrawSphere(center, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(endPos, 0.2f);
    }
    // */
}