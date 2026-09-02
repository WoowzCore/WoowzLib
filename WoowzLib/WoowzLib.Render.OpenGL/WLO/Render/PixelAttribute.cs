using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;
using WLO.Math;

namespace WLO.Render;

public struct PixelAttribute : IEquatable<PixelAttribute>{
    public string                Name;
    public FramebufferAttachment Attachment;
    public int                   Count;
    public InternalFormat        Format;
    public bool                  IsTexture;
    public Color4B?              Default;
    
    public PixelAttribute(string Name, int Count, FramebufferAttachment Attachment, InternalFormat Format = InternalFormat.Rgba8, bool IsTexture = false, Color4B? Default = null){
        this.Name       = Name;
        this.Count      = Count;
        this.Attachment = Attachment;
        this.Format     = Format;
        this.IsTexture  = IsTexture;
        this.Default    = Default;
    }

    public static PixelAttribute Color(string Name, int Count, int Index = 0, Color4B? Default = null, InternalFormat Format = InternalFormat.Rgba8, bool IsTexture = true) => new PixelAttribute(Name, Count, FramebufferAttachment.ColorAttachment0 + Index, Format, IsTexture, Default);
    public static PixelAttribute Depth(bool IsTexture = false, InternalFormat Format = InternalFormat.DepthComponent24) => new PixelAttribute("gl_FragDepth", 1, FramebufferAttachment.DepthAttachment, Format, IsTexture);
    public static PixelAttribute Stencil(bool IsTexture = false, InternalFormat Format = InternalFormat.StencilIndex8) => new PixelAttribute("gl_Stencil", 1, FramebufferAttachment.StencilAttachment, Format, IsTexture);
    
    // ----------------------------------------------------------------------

    public bool Equals(PixelAttribute Other) => Name == Other.Name && Count == Other.Count && Format == Other.Format && IsTexture == Other.IsTexture && Default == Other.Default;

    public override bool Equals(object? Object) => Object is PixelAttribute Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(Name, Attachment, Count, Format, IsTexture, Default);

    public static bool operator ==(PixelAttribute Left, PixelAttribute Right) =>  Left.Equals(Right);
    public static bool operator !=(PixelAttribute Left, PixelAttribute Right) => !Left.Equals(Right);
}