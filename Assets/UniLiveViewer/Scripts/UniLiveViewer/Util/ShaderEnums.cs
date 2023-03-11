namespace UniLiveViewer
{
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }
    public enum BlendMode_MToon
    {
        Opaque,
        Cutout,
        Transparent,
        TransparentWithZWrite
    }
    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }
    public enum RenderFace//この並びはURP
    {
        Both,
        Back,
        Front
    }
}