using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Polaris
{
    public class Material : RawMaterial
    {
        private string _MaterialName = "";
        public string MaterialName
        {
            get
            {
                MaterialName = _MaterialName;
                return _MaterialName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _MaterialName = "New Material";
                }
                else
                {
                    _MaterialName = value;
                }
            }
        }
        public string ShaderName = "Error/Untitled";
        public string ShaderPath = "";
        private static Dictionary<string, Texture> TextureCache = new Dictionary<string, Texture>();

        private string FilePath
        {
            get
            {
                return Path.Combine(ShaderPath, "shader");
            }
        }
        private string _Src = null;
        private bool _SrcSet = false;
        private string Src
        {
            get
            {
                if (!_SrcSet)
                {
                    try
                    {
                        _Src = File.ReadAllText(FilePath);
                    }
                    catch
                    {
                        return null;
                    }
                    _SrcSet = true;
                }
                return _Src;
            }
        }

        protected override bool CustomInit()
        {
            return true;
        }

        public Material(string ShaderPath)
        {
            if (!Directory.Exists(ShaderPath)) throw new Exception("Shader directory/path doesn't exist!");
            this.ShaderPath = ShaderPath;
            Init();
            EditorGUI.Materials.Add(this);
        }

        public override void AddShaders()
        {
            if (Src == null)
            {
                ShaderPath = @"Assets\builtin\shaders\std";
            }
            foreach (string tmp_ln in Src.Split('>'))
            {
                string ln = tmp_ln + ">";
                ln = ln.Replace("\n", "").Replace("\r", "");
                if (ln.StartsWith("v_"))
                {
                    string v = ln.Split(new string[] { "v_" }, StringSplitOptions.None)[1].Split('<')[0];
                    if (v == "ShaderName")
                    {
                        string n = ln.Split('<')[1].Split('>')[0];
                        ShaderName = n;
                    }
                }
                if (ln.StartsWith("s_"))
                {
                    string sh = ln.Split(new string[] { "s_" }, StringSplitOptions.None)[1].Split('<')[0];
                    ShaderType st = (ShaderType)Enum.GetValues
                        (typeof(ShaderType)).GetValue
                        (Enum.GetNames(typeof
                        (ShaderType)).ToList().IndexOf(sh));
                    string src = ln.Split('<')[1].Split('>')[0];
                    if (File.Exists(Path.Combine(ShaderPath, src))) src = Path.Combine(ShaderPath, src);
                    if (!File.Exists(src)) throw new Exception("Shader doesn't exist: " + Enum.GetName(typeof(ShaderType), st) + "@" + src);
                    AddShader(st, src);
                }
            }
        }

        public class Property
        {
            public readonly string Name = "";
            public readonly string Value = "";
            public readonly Type Type = null;

            public Property(string Name, string Value, Type Type)
            {
                this.Name = Name;
                this.Value = Value;
                this.Type = Type;

                this.Value = this.Value.Replace("|NO-TEX|", @"Assets\builtin\textures\prototyping\Purple\texture_08.png");
            }

            public object Get(bool CreateTexture = false)
            {
                if (CreateTexture && Type == typeof(Texture)) return new Texture(Value);
                if (Type == typeof(float)) return float.Parse(Value);
                if (Type == typeof(Vector2)) return new Vector2
                        (float.Parse(Regex.Replace(Value.Split(',')[0], @"\s+", "")),
                        float.Parse(Regex.Replace(Value.Split(',')[1], @"\s+", "")));
                if (Type == typeof(Vector3)) return new Vector3
                        (float.Parse(Regex.Replace(Value.Split(',')[0], @"\s+", "")),
                        float.Parse(Regex.Replace(Value.Split(',')[1], @"\s+", "")),
                        float.Parse(Regex.Replace(Value.Split(',')[2], @"\s+", "")));
                return Value;
            }

            public T Get<T>(bool CreateTexture = false)
            {
                return (T)Get(CreateTexture);
            }
        }

        public List<Property> GetProperties()
        {
            List<Property> Lst = new List<Property>();
            foreach (string tmp_ln in Src.Split('>'))
            {
                string ln = tmp_ln + ">";
                ln = ln.Replace("\n", "").Replace("\r", "");
                ln = new Property(null, ln, null).Value;
                if (ln.StartsWith("t_"))
                {
                    string t_name = ln.Split(new string[] { "t_" }, StringSplitOptions.None)[1].Split('<')[0];
                    string src = ln.Split('<')[1].Split('>')[0];
                    if (File.Exists(Path.Combine(ShaderPath, src))) src = Path.Combine(ShaderPath, src);
                    Lst.Add(new Property(t_name, src, typeof(Texture)));
                }
                if (ln.StartsWith("f_"))
                {
                    string f_name = ln.Split(new string[] { "f_" }, StringSplitOptions.None)[1].Split('<')[0];
                    string src = ln.Split('<')[1].Split('>')[0];
                    int dim = src.ToCharArray().Count(c => c == ',') + 1;
                    if (dim == 1) Lst.Add(new Property(f_name, src, typeof(float)));
                    if (dim == 2) Lst.Add(new Property(f_name, src, typeof(Vector2)));
                    if (dim == 3) Lst.Add(new Property(f_name, src, typeof(Vector3)));
                }
            }
            return Lst;
        }

        public void SetProperty(string Name, string value)
        {
            string src = Src;
            foreach (string tmp_ln in Src.Split('>'))
            {
                string ln = tmp_ln + ">";
                try
                {
                    ln = ln.Replace("\n", "").Replace("\r", "");
                    if (ln.Split('_')[1].Split('<')[0] == Name)
                    {
                        ln = ln.Replace("<" + ln.Split('<')[1].Split('>')[0] + ">", "<" + value + ">");
                    }
                }
                catch { }
                src += ln + Environment.NewLine;
            }
            _Src = src;
        }

        public void SetProperty(string Name, Texture value)
        {
            SetProperty(Name, value.Filename);
        }

        public void SetProperty(string Name, float value)
        {
            SetProperty(Name, value.ToString());
        }

        public void SetProperty(string Name, Vector2 value)
        {
            SetProperty(Name, value.X.ToString() + ", " + value.Y.ToString());
        }

        public void SetProperty(string Name, Vector3 value)
        {
            SetProperty(Name, value.X.ToString() + ", " + value.Y.ToString() + ", " + value.Z.ToString());
        }

        public override void Bind()
        {
            int TexIdx = 0;
            foreach (Property property in GetProperties())
            {
                if (property.Type == typeof(Texture))
                {
                    string path = property.Get<string>();
                    if (!File.Exists(path)) throw new Exception("Texture doesn't exist: " + property.Name + "@" + path);
                    if (!TextureCache.ContainsKey(path)) TextureCache.Add(path, property.Get<Texture>(true));
                    TextureUnit TexUnit = (Silk.NET.OpenGL.TextureUnit)Enum.GetValues(typeof(Silk.NET.OpenGL.TextureUnit)).GetValue
                        (Enum.GetNames(typeof(Silk.NET.OpenGL.TextureUnit)).ToList().IndexOf("Texture" + TexIdx));
                    OGL.ActiveTexture(TexUnit);
                    TextureCache[path].Bind();
                    Properties.SetProperty1(property.Name, TexIdx);
                    TexIdx++;
                }
                else if (property.Type == typeof(float))
                {
                    Properties.SetProperty1(property.Name, property.Get<float>());
                }
                else if (property.Type == typeof(Vector2))
                {
                    Properties.SetProperty2(property.Name, property.Get<Vector2>());
                }
                else if (property.Type == typeof(Vector3))
                {
                    Properties.SetProperty3(property.Name, property.Get<Vector3>());
                }
            }
        }
    }
}
