using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Polaris
{
    public sealed class Texture : IDisposable
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;

        public readonly string Filename = "";
        public int NativeTexture = -1;
        public Bitmap Bitmap = null;

        public unsafe Texture(string Filename)
        {
            this.Filename = Filename;
            Bitmap = new Bitmap(Filename);
            OGL.Enable(GLEnum.Texture2D);

            NativeTexture = (int)OGL.GenTexture();
            OGL.BindTexture(GLEnum.Texture2D, (uint)NativeTexture);
            OGL.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Linear);
            OGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            BitmapData data = Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            OGL.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)data.Width, (uint)data.Height, 0,
                GLEnum.Bgra, GLEnum.UnsignedByte, (void*)data.Scan0);

            Bitmap.UnlockBits(data);
        }

        public void Bind()
        {
            OGL.BindTexture(GLEnum.Texture2D, (uint)NativeTexture);
        }

        public void Dispose()
        {
            OGL.DeleteTexture((uint)NativeTexture);
            NativeTexture = -1;
            Bitmap.Dispose();
            Bitmap = null;
        }
    }
}
