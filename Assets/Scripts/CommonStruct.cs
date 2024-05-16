

public struct PairedAction
{
    public string key;
    public float pressTime;
    public float releaseTime;

    public PairedAction(string k, float p, float r)
    {
        key = k;
        pressTime = p;
        releaseTime = r;
    }
}

public struct SingleAction
{
    public string key;
    public float pressTime;

    public SingleAction(string k, float p)
    {
        key = k;
        pressTime = p;
    }
}