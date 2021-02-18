using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Polaris
{
    public sealed class MaterialProperties
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;

        private Dictionary<string, int> Cache = new Dictionary<string, int>();

        private int GetLocation(string name)
        {
            if (!Cache.ContainsKey(name))
            { 
                int Program = (int)Material.CurrentProgram;
                Cache.Add(name, OGL.GetUniformLocation((uint)Program, name));
            }
            return Cache[name];
        }

        public void SetProperty1(string name, float v0)
        {
            OGL.Uniform1(GetLocation(name), v0);
        }

        public unsafe void SetProperty1(string name, uint count, ref float* value)
        {
            OGL.Uniform1(GetLocation(name), count, value);
        }

        public void SetProperty1(string name, int v0)
        {
            OGL.Uniform1(GetLocation(name), v0);
        }

        public unsafe void SetProperty1(string name, uint count, int* value)
        {
            OGL.Uniform1(GetLocation(name), count, value);
        }

        public void SetProperty2(string name, float v0, double v1)
        {
            OGL.Uniform2(GetLocation(name), v0, v1);
        }

        public unsafe void SetProperty2(string name, uint count, double* value)
        {
            OGL.Uniform2(GetLocation(name), count, value);
        }

        public void SetProperty2(string name, int v0, int v1)
        {
            OGL.Uniform2(GetLocation(name), v0, v1);
        }

        public void SetProperty2(string name, Vector2 value)
        {
            SetProperty2(name, value.X, value.Y);
        }

        public void SetProperty3(string name, float v0, float v1, float v2)
        {
            OGL.Uniform3(GetLocation(name), v0, v1, v2);
        }

        public void SetProperty3(string name, float v0, float v1, int v2)
        {
            OGL.Uniform3(GetLocation(name), v0, v1, v2);
        }

        public unsafe void SetProperty3(string name, uint count, float* value)
        {
            OGL.Uniform3(GetLocation(name), count, value);
        }

        public void SetProperty3(string name, int v0, int v1, int v2)
        {
            OGL.Uniform3(GetLocation(name), v0, v1, v2);
        }

        public unsafe void SetProperty3(string name, uint count, int* value)
        {
            OGL.Uniform3(GetLocation(name), count, value);
        }

        public void SetProperty3(string name, Vector3 value)
        {
            SetProperty3(name, value.X, value.Y, value.Z);
        }

        public void SetProperty4(string name, float v0, float v1, float v2, float v3)
        {
            OGL.Uniform4(GetLocation(name), v0, v1, v2, v3);
        }

        public void SetProperty4(string name, float v0, float v1, float v2, int v3)
        {
            OGL.Uniform4(GetLocation(name), v0, v1, v2, v3);
        }

        public unsafe void SetProperty4(string name, uint count, float* value)
        {
            OGL.Uniform4(GetLocation(name), count, value);
        }

        public void SetProperty4(string name, int v0, int v1, int v2, int v3)
        {
            OGL.Uniform4(GetLocation(name), v0, v1, v2, v3);
        }

        public void SetProperty4(string name, Vector4 value)
        {
            SetProperty4(name, value.X, value.Y, value.Z, value.W);
        }

        public unsafe void SetPropertyMatrix2(string name, uint count, bool transpose, float* value)
        {
            OGL.UniformMatrix2(GetLocation(name), count, transpose, value);
        }

        public unsafe void SetPropertyMatrix3(string name, uint count, bool transpose, float* value)
        {
            OGL.UniformMatrix3(GetLocation(name), count, transpose, value);
        }

        public unsafe void SetPropertyMatrix4(string name, uint count, bool transpose, float* value)
        {
            OGL.UniformMatrix4(GetLocation(name), count, transpose, value);
        }

        public unsafe void SetPropertyMatrix4(string name, bool transpose, Matrix4x4* value)
        {
            OGL.UniformMatrix4(GetLocation(name), 1, transpose, value->M11);
        }
    }
}
