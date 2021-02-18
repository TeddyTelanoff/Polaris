using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Polaris
{
    public abstract partial class Application : Layer
    {
        internal static Silk.NET.OpenGL.GL GL
        {
            get
            {
                return Silk.NET.OpenGL.GL.GetApi
                    (Get().MainWindow);
            }
        }

        public Application()
        {
            InternalAppData.Instance = this;
        }

        public static Application Get()
        {
            return InternalAppData.Instance;
        }

        public static ImmutableList<Layer> LayerStack 
        { private set; get; } = null;

        public void Run()
        {
            LayerStack = ImmutableList.Create<Layer>();
            CreateWindow();
            GL?.Dispose();
        }

        public static void PushLayer(Layer layer)
        {
            LayerStack = LayerStack.Add(layer);
            layer.OnAttach();
        }

        public static Layer PopLayer()
        {
            Layer layer = LayerStack.ToImmutableArray()[^1];
            LayerStack = LayerStack.RemoveAt
                (LayerStack.ToImmutableArray().Length - 1);
            layer.OnDetach();
            return layer;
        }

        public static Layer PopLayer(Layer layer)
        {
            LayerStack = LayerStack.Remove(layer);
            layer.OnDetach();
            return layer;
        }
    }

    internal static class InternalAppData
    {
        public static Application Instance = null;
    }
}
