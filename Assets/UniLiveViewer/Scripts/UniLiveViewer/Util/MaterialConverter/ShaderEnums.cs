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
    /// <summary>
    /// NOTE: URPだとEditorしかない
    /// </summary>
    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }
    //public enum RenderFace//この並びはURP
    //{
    //    Both,
    //    Back,
    //    Front
    //}
}