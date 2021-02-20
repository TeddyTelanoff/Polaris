using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Ultz.SilkExtensions.ImGui;
using System;
using System.Drawing;
using System.Numerics;

namespace Polaris
{
    internal class SceneRenderer : Layer
    {
        public static Silk.NET.OpenGL.GL OGL => Application.GL;
        public IInputContext InputContext = null;

        public static int FinalNativeTexture = -1;
        public static int DepthTexture = -1;
        public static int DepthBuffer = -1;
        public static int FBOHandle = -1;

        internal static Mesh Grid = null;
        internal static Material GridMat = null;

        public override void OnAttach()
        {
            CreateFBO();
            OGL.Viewport(0, 0, (uint)Application.Get().MainWindow.Size.X, 
                (uint)Application.Get().MainWindow.Size.Y);
            OGL.BindTexture(GLEnum.Texture2D, 0);
            OGL.BindFramebuffer(Silk.NET.OpenGL.GLEnum.Framebuffer, (uint)FBOHandle);
            OGL.Viewport(0, 0, Application.ViewportMax, Application.ViewportMax);
            Grid = Mesh.CreateFromScratch();
            Grid.Vertices = new float[]
            {
                 0.0f, 0.0f,  0.5f,
                 0.0f, 0.0f, -0.5f,
                 0.0f, 0.0f,  0.5f,
            };
            Grid.UVs = new float[]
            { 
                0, 0, 
                0, 0,
                0, 0
            };
            Grid.Normals = new float[]
            {
                0, 1, 0,
                0, 1, 0,
                0, 1, 0,
                0, 1, 0
            };
            Grid.Indices = new uint[]
            {
                0, 1, 2
            };
            Grid.UpdateBuffers();
            GridMat = new Material(@"builtin/shaders/stdcolor");
        }

        public unsafe override void OnUpdate()
        {
            OGL.Viewport(0, 0, (uint)Application.Get().MainWindow.Size.X,
                (uint)Application.Get().MainWindow.Size.Y);

            OGL.BindTexture(GLEnum.Texture2D, 0);
            OGL.BindFramebuffer(GLEnum.Framebuffer, (uint)FBOHandle);
            OGL.Viewport(0, 0, Application.ViewportMax, Application.ViewportMax);

            OGL.Enable(EnableCap.DepthTest);
            OGL.Enable(EnableCap.CullFace);

            OGL.ClearColor(Color.Black);

            OGL.Clear((uint)(ClearBufferMask.ColorBufferBit |
                ClearBufferMask.DepthBufferBit |
                ClearBufferMask.StencilBufferBit));
            //-:cnd:noEmit
#if POLARIS_EDITOR
            EditorGUI.UpdateEditorCamera();
#endif
            //+:cnd:noEmit
            //for (int x = -500; x <= 500; x++)
            //{
            //    GridMat.Push();
            //    GridMat.Bind();
            //    GL.DrawMesh(Grid, Camera.editor, Matrix4x4.CreateScale(1, 1, 1000) * Matrix4x4.CreateTranslation(x, 0, 0), true);
            //    GridMat.Pop();
            //}
            //for (int z = -500; z <= 500; z++)
            //{
            //    GridMat.Push();
            //    GridMat.Bind();
            //    GL.DrawMesh(Grid, Camera.editor, Matrix4x4.CreateScale(1, 1, 1000) * Matrix4x4.CreateRotationY(Vim.MathOps.ToRadians(90)) * Matrix4x4.CreateTranslation(0, 0, z), true);
            //    GridMat.Pop();
            //}
            foreach (Layer layer in Application.LayerStack)
            {
                foreach (GameObject go in Scene.LoadedScene.FindAll())
                {
                    if (go.Layer == layer.GetName() && go.Mesh != null)
                    {
                        if (go.Material != null)
                        {
                            go.Material.Push();
                            go.Material.Bind();
                            GL.DrawMesh(go.Mesh, Camera.current, go.Transform, EditorGUI.EnableWireframe);
                            go.Material.Pop();
                        }
                    }
                }
            }

            OGL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            OGL.Viewport(0, 0, (uint)Application.Get().MainWindow.Size.X,
                (uint)Application.Get().MainWindow.Size.Y);
        }

        public override void OnDetach()
        {
            InputContext?.Dispose();
        }

        private void CreateFBO()
        {
            if (FBOHandle >= 0)
            {
                OGL.DeleteFramebuffer((uint)FBOHandle);
                OGL.DeleteTexture((uint)FinalNativeTexture);
                OGL.DeleteTexture((uint)DepthTexture);
                OGL.DeleteBuffer((uint)DepthBuffer);
            }
            CreateFBOHandle();
        }

        private unsafe void CreateFBOHandle()
        {
            FBOHandle = (int)OGL.GenFramebuffer();
            OGL.BindFramebuffer(GLEnum.Framebuffer, (uint)FBOHandle);
            OGL.DrawBuffer(GLEnum.ColorAttachment0);
            FinalNativeTexture = (int)OGL.GenTexture();
            OGL.BindTexture(GLEnum.Texture2D, (uint)FinalNativeTexture);
            OGL.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgb,
                Application.ViewportMax, Application.ViewportMax, 0,
                GLEnum.Rgb, GLEnum.UnsignedByte, null);
            OGL.TexParameterI(GLEnum.Texture2D,
                GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            OGL.TexParameterI(GLEnum.Texture2D,
                GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            OGL.FramebufferTexture(GLEnum.Framebuffer,
                GLEnum.ColorAttachment0, (uint)FinalNativeTexture, 0);
            CreateDepthTexture();
        }

        private unsafe void CreateDepthTexture()
        {
            DepthTexture = (int)OGL.GenTexture();
            OGL.BindTexture(GLEnum.Texture2D, (uint)DepthTexture);
            OGL.TexImage2D(GLEnum.Texture2D, 0,
                (int)GLEnum.DepthComponent32,
                Application.ViewportMax,
                Application.ViewportMax, 0,
                GLEnum.DepthComponent,
                GLEnum.UnsignedByte, null);
            OGL.TexParameterI(GLEnum.Texture2D,
                GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            OGL.TexParameterI(GLEnum.Texture2D,
                GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            OGL.FramebufferTexture(GLEnum.Framebuffer,
                GLEnum.DepthAttachment, (uint)DepthTexture, 0);
            CreateDepthBuffer();
        }

        private void CreateDepthBuffer()
        {
            DepthBuffer = (int)OGL.GenRenderbuffer();
            OGL.BindRenderbuffer(GLEnum.Renderbuffer, (uint)DepthBuffer);
            OGL.RenderbufferStorage(GLEnum.Renderbuffer,
                GLEnum.DepthComponent,
                Application.ViewportMax,
                Application.ViewportMax);
            OGL.FramebufferRenderbuffer(GLEnum.Framebuffer,
                GLEnum.DepthAttachment,
                GLEnum.Renderbuffer, (uint)DepthBuffer);
            OGL.BindTexture(GLEnum.Texture2D, 0);
            OGL.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        public override string GetName()
        {
            return "SceneRenderer";
        }
    }
}
