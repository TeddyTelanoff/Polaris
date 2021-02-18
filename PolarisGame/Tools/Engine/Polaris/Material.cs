using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Polaris
{
    public abstract class Material
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;

        public MaterialProperties Properties = new MaterialProperties();

        public abstract void AddShaders();
        public abstract void Bind();

        private int Program = -1;
        private bool HasShaders = false;
        private readonly List<int> Shaders = new List<int>();

        internal static int CurrentProgram = -1;

        public Material()
        {
            BasicBind();
            AddShaders();
        }

        public void AddShader(ShaderType ShaderType, string src, SourceType SourceType = SourceType.Filepath)
        {
            BasicBind();
            if (SourceType == SourceType.Filepath) AddShader(ShaderType, File.ReadAllText(src), SourceType.Filedata);
            else if (SourceType == SourceType.Filedata) AddShaderData(ShaderType, src);
        }

        private void AddShaderData(ShaderType ShaderType, string src)
        {
            int shader = CreateShader(ShaderType, src);
            CheckShader(shader);
            OGL.AttachShader((uint)Program, (uint)shader);
            Shaders.Add(shader);
            HasShaders = true;
        }

        private int CreateShader(ShaderType ShaderType, string src)
        {
            int shader = (int)OGL.CreateShader((Silk.NET.OpenGL.ShaderType)ShaderType);
            OGL.ShaderSource((uint)shader, src);
            OGL.CompileShader((uint)shader);
            return shader;
        }

        private void CheckShader(int shader)
        {
            string log = OGL.GetShaderInfoLog((uint)shader);
            if (!string.IsNullOrWhiteSpace(log)) throw new Exception(log);
        }

        public void Push()
        {
            BasicBind();
            if (HasShaders)
            {
                OGL.LinkProgram((uint)Program);
                CheckProgram(Program);
                DeleteShaderSources();
                HasShaders = false;
                Shaders.Clear();
            }
            BasicBind();
            CurrentProgram = Program;
        }

        private void CheckProgram(int Program)
        {
            OGL.ValidateProgram((uint)Program);
            string log = OGL.GetProgramInfoLog((uint)Program);
            if (!string.IsNullOrWhiteSpace(log)) throw new Exception(log);
        }

        private void DeleteShaderSources()
        {
            foreach (int shader in Shaders)
            {
                //-:cnd:noEmit
#if RELEASE
                    OGL.DetachShader((uint)Program, (uint)shader);
#endif
                //+:cnd:noEmit
                OGL.DeleteShader((uint)shader);
            }
        }

        public void Pop()
        {
            OGL.UseProgram(0);
        }

        private void BasicBind()
        {
            if (Program < 0) Program = (int)OGL.CreateProgram();
            OGL.UseProgram((uint)Program);
        }
    }
}
