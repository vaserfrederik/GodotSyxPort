using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

class _FBODebug : CORE_RESOURCE
{
    private int width;
    private int height;
    private _FBOBlitter blitter;

    private readonly int ID;
    private int textureID;
    private readonly int stencilID;

    _FBODebug(SETTINGS sett)
    {
        blitter = new _FBOBlitter(sett);
        this.width = sett.getNativeWidth();
        this.height = sett.getNativeHeight();

        ID = GenFramebuffers(1)[0];
        stencilID = GenRenderbuffers(1)[0];
        GenerateTextures();
    }

    private void GenerateTextures()
    {
        GlHelper.checkErrors();
        BindFramebuffer(FramebufferTarget.Framebuffer, ID);

        textureID = GlHelper.getFBTexture(width, height);
        FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureID, 0);

        BindRenderbuffer(RenderbufferTarget.Renderbuffer, stencilID);
        RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, stencilID);

        DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

        if (CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new System.RuntimeException("Could not create fbo");

        BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GlHelper.checkErrors();
    }

    private void DeleteTextures()
    {
        DeleteTextures(1, new int[] { textureID });
        DeleteRenderbuffers(1, new int[] { stencilID });
    }

    public override void dis()
    {
        GlHelper.checkErrors();
        BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        DeleteTextures();
        DeleteFramebuffers(1, new int[] { ID });
        blitter.dis();
        GlHelper.checkErrors();
    }

    public void applySettings(SETTINGS sett)
    {
        if (this.width != sett.getNativeWidth() || this.height != sett.getNativeHeight())
        {
            this.width = sett.getNativeWidth();
            this.height = sett.getNativeHeight();
            DeleteTextures();
            GenerateTextures();
        }
    }

    public void bindAndClear()
    {
        BindFramebuffer(FramebufferTarget.DrawFramebuffer, ID);
        DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });
        GlHelper.ViewPort.set(width, height);
        Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void blitTexture()
    {
        blitter.blit(ID);
    }
}