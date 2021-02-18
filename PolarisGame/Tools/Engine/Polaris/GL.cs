using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Polaris
{
    public static class GL
    {
        internal static Silk.NET.OpenGL.GL OGL => Application.GL;
        internal static Matrix4x4 view = Matrix4x4.Identity;
        internal static Matrix4x4 proj = Matrix4x4.Identity;

        public unsafe static void DrawMesh(Mesh mesh, Camera cam, Matrix4x4 WorldTransform)
        {
            if (cam == null) cam = Camera.current;
            //-:cnd:noEmit
#if POLARIS_EDITOR
            cam = Camera.editor;
#endif
            //+:cnd:noEmit
            Matrix4x4 model = WorldTransform;
            new MaterialProperties().SetPropertyMatrix4("model", false, &model);
            view = Matrix4x4.CreateLookAt
                (cam.Position, cam.Position +
                Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll
                (
                    Vim.MathOps.ToRadians(cam.Rotation.Y),
                    Vim.MathOps.ToRadians(cam.Rotation.X),
                    Vim.MathOps.ToRadians(cam.Rotation.Z)
                )), Vector3.UnitY) * Matrix4x4.CreateScale(1.0f / EditorGUI.ViewportAspectRatio, 1.0f, 1.0f);
            fixed (Matrix4x4* my_view = &view)
            {
                fixed (Matrix4x4* my_proj = &proj)
                {
                    new MaterialProperties().SetPropertyMatrix4("view", false, my_view);
                    proj = Matrix4x4.CreatePerspectiveFieldOfView
                        (Vim.MathOps.ToRadians(cam.FOV),
                        (float)Application.ViewportMax / Application.ViewportMax, cam.Near, cam.Far);
                    new MaterialProperties().SetPropertyMatrix4("proj", false, my_proj);
                }
            }
            mesh.Bind();
            OGL.DrawElements
                (GLEnum.Triangles,
                (uint)mesh.Indices.Length, GLEnum.UnsignedInt, (void*)0);
            foreach (Mesh m in mesh.SubMeshes)
            {
                DrawMesh(m, cam, WorldTransform);
            }
        }
    }
}
