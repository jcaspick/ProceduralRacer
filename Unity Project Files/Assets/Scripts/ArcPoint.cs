public struct ArcPoint {

    public float t, d;

    public ArcPoint(float t, float d)
    {
        this.t = t;
        this.d = d;
    }

    public void Set(float newt, float newd)
    {
        t = newt;
        d = newd;
    }
}