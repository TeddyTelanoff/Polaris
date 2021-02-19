using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace Polaris
{
    public abstract partial class Application : Layer
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;

        internal SceneRenderer SceneRenderer = null;
        internal ImGuiRenderer ImGuiRenderer = null;
        internal IWindow MainWindow = null;
        internal IInputContext input = null;

        public const int ViewportMax = 2000;

        internal void CreateWindow()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1600, 900);
            options.WindowState = WindowState.Maximized;
            options.Title = "Polaris";
            MainWindow = Window.Create(options);

            MainWindow.Load += OnLoad;
            MainWindow.Render += OnRender;
            MainWindow.Closing += OnClose;

            MainWindow.Run();
        }

        internal unsafe void OnLoad()
        {
            MainWindow.WindowState = WindowState.Maximized;
            input = MainWindow.CreateInput();

            PushLayer(this);
            SceneRenderer = new SceneRenderer();
            PushLayer(SceneRenderer);
            ImGuiRenderer = new ImGuiRenderer();
            PushLayer(ImGuiRenderer);

            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += Application.Get().KeyDown;
            }
        }

        internal unsafe void OnRender(double delta)
        {
            Time.DeltaTime = (float)delta;

            foreach (Layer layer in LayerStack)
            {
                layer.OnUpdate();
            }


            OGL.BindFramebuffer(Silk.NET.OpenGL.GLEnum.Framebuffer, 0);
        }

        internal void OnClose()
        {
            foreach (Layer layer in LayerStack)
            {
                layer.OnDetach();
            }
        }

        internal void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                EditorGUI.NewGameName = "";
                EditorGUI.ProjectDialog = false;
            }
            if (arg2 == Key.Tab)
            {
                EditorGUI.ShouldUpdateEditorCamera ^= true;
            }
        }
    }
}
