using Polaris;

namespace Polaris
{
    public class StandardMaterial : Material
    {
        public Texture MainTex = null;

        public override void AddShaders()
        {
            AddShader(ShaderType.VertexShader, @"Assets\builtin\shaders\std\StandardShader.vert");
            AddShader(ShaderType.FragmentShader, @"Assets\builtin\shaders\std\StandardShader.frag");
        }

        public override void Bind()
        {
            MainTex.Bind();
        }
    }
}