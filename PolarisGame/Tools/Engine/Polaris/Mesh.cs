using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;
using Silk.NET.OpenGL;

namespace Polaris
{
    [Serializable]
    public sealed class Mesh
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;

        public string Name = "";
        public float[] Buffer
        {
            get
            {
                int i = 0;
                int j = 0;
                int k = 0;
                List<float> lst = new List<float>();
                while (true)
                {
                    try
                    {
                        float x = Vertices[i + 0];
                        float y = Vertices[i + 1];
                        float z = Vertices[i + 2];
                        lst.Add(x);
                        lst.Add(y);
                        lst.Add(z);
                        i += 3;
                        float x1 = UVs[j + 0];
                        float y1 = UVs[j + 1];
                        lst.Add(x1);
                        lst.Add(y1);
                        j += 2;
                        float x2 = Normals[k + 0];
                        float y2 = Normals[k + 1];
                        float z2 = Normals[k + 2];
                        lst.Add(x2);
                        lst.Add(y2);
                        lst.Add(z2);
                        k += 3;
                    }
                    catch 
                    {
                        break;
                    }
                }
                return lst.ToArray();
            }
        }
        public float[] Vertices = new float[] { };
        public float[] UVs = new float[] { };
        public float[] Normals = new float[] { };
        public uint[] Indices = new uint[] { };
        public bool IsStatic = true;
        public List<Mesh> SubMeshes = new List<Mesh>();

        public System.Numerics.Vector3 Position = System.Numerics.Vector3.Zero;
        public System.Numerics.Vector3 Rotation = System.Numerics.Vector3.Zero;
        public System.Numerics.Vector3 Scale = System.Numerics.Vector3.One;

        private int VAO = -1;
        private int VBO = -1;
        private int IBO = -1;
        internal System.Numerics.Matrix4x4 Transform
        {
            get
            {
                System.Numerics.Matrix4x4 model = System.Numerics.Matrix4x4.Identity;
                model *= System.Numerics.Matrix4x4.CreateScale(Scale);
                model *= System.Numerics.Matrix4x4.CreateFromYawPitchRoll
                (
                    Vim.MathOps.ToRadians(Rotation.Y),
                    Vim.MathOps.ToRadians(Rotation.X),
                    Vim.MathOps.ToRadians(Rotation.Z)
                );
                model *= System.Numerics.Matrix4x4.CreateTranslation(Position);
                return model * NodeTransform;
            }
        }

        public static Mesh Empty = CreateFromScratch();

        private System.Numerics.Matrix4x4 NodeTransform = System.Numerics.Matrix4x4.Identity;

        private static readonly AssimpContext Context = new AssimpContext();
        private static readonly PostProcessSteps PostProcess = new PostProcessSteps();

        public void UpdateBuffers()
        {
            CleanBuffers();
            GenerateEmptyBuffers();
            ApplyBufferData();
            Bind();
        }

        public void CleanBuffers()
        {
            if (VAO >= 0) OGL.DeleteVertexArray((uint)VAO);
            if (VBO >= 0) OGL.DeleteBuffer((uint)VBO);
            if (IBO >= 0) OGL.DeleteBuffer((uint)IBO);
            VAO = -1;
            VBO = -1;
            IBO = -1;
        }

        private void GenerateEmptyBuffers()
        {
            VAO = (int)OGL.GenVertexArray();
            VBO = (int)OGL.GenBuffer();
            IBO = (int)OGL.GenBuffer();
        }

        private void ApplyBufferData()
        {
            ApplyVBOBufferData();
            ApplyIBOBufferData();
        }

        private unsafe void ApplyVBOBufferData()
        {
            OGL.BindBuffer(GLEnum.ArrayBuffer, (uint)VBO);
            fixed (void* arr = Buffer)
            {
                OGL.BufferData(GLEnum.ArrayBuffer, (UIntPtr)(sizeof(float) * Buffer.Length), arr, IsStatic ?
                    GLEnum.StaticDraw : GLEnum.DynamicDraw);
            }
        }

        private unsafe void ApplyIBOBufferData()
        {
            OGL.BindBuffer(GLEnum.ElementArrayBuffer, (uint)IBO);
            fixed (void* arr = Indices)
            {
                OGL.BufferData(GLEnum.ElementArrayBuffer, (UIntPtr)(sizeof(uint) * Indices.Length), arr, IsStatic ?
                    GLEnum.StaticDraw : GLEnum.DynamicDraw);
            }
        }

        public void Bind()
        {
            if (VAO < 0) UpdateBuffers();
            OGL.BindVertexArray((uint)VAO);
            OGL.BindBuffer(GLEnum.ArrayBuffer, (uint)VBO);
            ApplyAttributes();
            OGL.BindBuffer(GLEnum.ElementArrayBuffer, (uint)IBO);
        }

        private unsafe void ApplyAttributes()
        {
            int stride = sizeof(float) * (3 + 2 + 3);
            OGL.EnableVertexAttribArray(0);
            OGL.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)stride, (void*)0);
            OGL.EnableVertexAttribArray(1);
            OGL.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)stride, (void*)(sizeof(float) * 3));
            OGL.EnableVertexAttribArray(2);
            OGL.VertexAttribPointer(2, 3, GLEnum.Float, false, (uint)stride, (void*)(sizeof(float) * (3 + 2)));
        }

        static Mesh()
        {
            PostProcess |= PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType;
        }

        private Mesh() { }

        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        public static Mesh LoadFromModel(string Filename)
        {
            if (Cache.ContainsKey(Filename)) return Cache[Filename];
            Assimp.Scene scene = Context.ImportFile(Filename, PostProcess);
            Cache.Add(Filename, GetMesh(scene));
            return Cache[Filename];
        }

        public static Mesh CreateFromScratch()
        {
            Mesh result = new Mesh();
            return result;
        }

        private static Mesh GetMesh(Assimp.Scene scene, Node node = null, Assimp.Mesh mesh = null, bool Root = true)
        {
            Mesh result = InitMesh(ref scene, ref node, ref Root);
            ProcessMesh(ref mesh, ref result);
            ProcessNode(ref scene, ref node, ref mesh, ref result);
            result.UpdateBuffers();
            return result;
        }

        private static Mesh InitMesh(ref Assimp.Scene scene, ref Node node, ref bool Root)
        {
            Mesh result = new Mesh();
            if (node == null && Root) node = scene.RootNode;
            if (node != null)
            {
                result.Name = node.Name;
                node.Transform.Decompose(out Vector3D A, out Quaternion B, out Vector3D C);
                System.Numerics.Matrix4x4 D = System.Numerics.Matrix4x4.Identity *
                    System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(C.X, C.Y, C.Z)) *
                    System.Numerics.Matrix4x4.CreateFromQuaternion(new System.Numerics.Quaternion(B.X, B.Y, B.Z, B.W)) *
                    System.Numerics.Matrix4x4.CreateScale(new System.Numerics.Vector3(A.X, A.Y, A.Z));
                result.NodeTransform = D;
            }
            return result;
        }

        private static void ProcessMesh(ref Assimp.Mesh mesh, ref Mesh result)
        {
            if (mesh != null)
            {
                if (!string.IsNullOrWhiteSpace(mesh.Name)) result.Name = mesh.Name;
                List<float> vertices = new List<float>();
                foreach (Vector3D pos in mesh.Vertices)
                {
                    vertices.Add(pos.X);
                    vertices.Add(pos.Y);
                    vertices.Add(pos.Z);
                }
                result.Vertices = vertices.ToArray();
                List<float> uvs = new List<float>();
                foreach (Vector3D uv in mesh.TextureCoordinateChannels[0])
                {
                    uvs.Add(uv.X);
                    uvs.Add(1.0f - uv.Y);
                }
                result.UVs = uvs.ToArray();
                List<float> normals = new List<float>();
                foreach (Vector3D normal in mesh.Normals)
                {
                    normals.Add(normal.X);
                    normals.Add(normal.Y);
                    normals.Add(normal.Z);
                }
                result.Normals = normals.ToArray();
                result.Indices = mesh.GetIndices().Select(i => (uint)i).ToArray();
            }
        }

        private static void ProcessNode(ref Assimp.Scene scene, ref Node node, ref Assimp.Mesh mesh, ref Mesh result)
        {
            if (node != null && mesh == null)
            {
                List<Assimp.Mesh> Meshes = new List<Assimp.Mesh>();
                foreach (int i in node.MeshIndices) Meshes.Add(scene.Meshes[i]);
                foreach (Assimp.Mesh childMesh in Meshes) result.SubMeshes.Add(GetMesh(scene, node, childMesh, false));
                foreach (Node childNode in node.Children) result.SubMeshes.Add(GetMesh(scene, childNode, null, false));
            }
        }
    }
}
