namespace UniLiveViewer
{
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }
    public enum BlendMode_MToon
    {
        Opaque = 0,
        Cutout,
        Transparent,
        TransparentWithZWrite
    }
    /// <summary>
    /// NOTE: URPだとEditorしかない
    /// </summary>
    public enum BlendMode
    {
        Alpha = 0,
        Premultiply,
        Additive,
        Multiply
    }
}