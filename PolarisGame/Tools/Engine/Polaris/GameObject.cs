using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Polaris
{
    public class GameObject
    {
        public string Name = "";
        public GameObject Parent
        {
            get
            {
                return Data.Parent;
            }
            set
            {
                List<GameObject> Lst;
                if (Data.Parent != null)
                {
                    Lst = Data.Parent.Nodes.ToList();
                    Lst.RemoveAll(c => c.GUID == GUID);
                    Data.Parent.Nodes = Lst.ToArray();
                }
                Data.Parent = value;
                if (value != null)
                {
                    Lst = Data.Parent.Nodes.ToList();
                    Lst.Add(this);
                    Data.Parent.Nodes = Lst.ToArray();
                }
            }
        }
        public Layer Layer
        {
            get
            {
                return Data.Layer;
            }
            set
            {
                Data.Layer = value;
            }
        }
        public Mesh Mesh
        {
            get
            {
                return Data.Mesh;
            }
            set
            {
                Data.Mesh = value;
            }
        }
        public Material Material
        {
            get
            {
                return Data.Material;
            }
            set
            {
                Data.Material = value;
            }
        }
        public GameObject[] GetChildren()
        {
            return Nodes.Where(c => c.Name != EditorGUI.Node.Add).ToArray();
        }
        public void AddChild(GameObject go)
        {
            go.Parent = this;
        }
        public void RemoveChild(GameObject go)
        {
            go.Parent = null;
        }
        public Vector3 Position
        {
            get
            {
                return Mesh.Position;
            }
            set
            {
                Mesh.Position = value;
            }
        }
        public Vector3 Rotation
        {
            get
            {
                return Mesh.Rotation;
            }
            set
            {
                Mesh.Rotation = value;
            }
        }
        public Vector3 Scale
        {
            get
            {
                return Mesh.Scale;
            }
            set
            {
                Mesh.Scale = value;
            }
        }
        public GameObject(string RuntimeID, bool IsRuntimeObject = true)
        {
            Data.RuntimeID = RuntimeID;
            Data.IsRuntimeObject = IsRuntimeObject;
            Scene.LoadedScene.AllNodes.Add(this);
        }
        public void Destroy()
        {
            foreach (GameObject c in GetChildren())
            {
                c.Destroy();
            }
            while (Scene.LoadedScene.AllNodes.Any(n => n.Data.ID == Data.ID))
            {
                if (Scene.LoadedScene.AllNodes.First(n => n.Data.ID == Data.ID).Parent != null)
                {
                    List<GameObject> Lst = Scene.LoadedScene.AllNodes.First(n => n.Data.ID == Data.ID).Parent.Nodes.ToList();
                    Lst.RemoveAll(child => child.Data.ID == Data.ID);
                    Scene.LoadedScene.AllNodes.First(n => n.Data.ID == Data.ID).Parent.Nodes = Lst.ToArray();
                    Scene.LoadedScene.AllNodes.First(n => n.Data.ID == Data.ID).Parent = null;
                }
                else
                {
                    List<GameObject> Lst = Scene.LoadedScene.AllNodes;
                    Lst.RemoveAll(child => child.Data.ID == Data.ID);
                    Scene.LoadedScene.AllNodes = Lst;
                }
            }
        }

        internal Guid GUID = Guid.NewGuid();
        internal GameObject[] Nodes = new GameObject[] { };
        internal GameObjectData Data
        {
            get
            {
                if (!EditorGUI.Node.DataDict.ContainsKey(GUID))
                {
                    EditorGUI.Node.DataDict.Add(GUID, new GameObjectData());
                }
                return EditorGUI.Node.DataDict[GUID];
            }
        }
        internal Matrix4x4 Transform
        {
            get
            {
                Matrix4x4 model = Matrix4x4.Identity;
                model *= Matrix4x4.CreateScale(Scale);
                model *= Matrix4x4.CreateFromYawPitchRoll
                (
                    Vim.MathOps.ToRadians(Rotation.Y),
                    Vim.MathOps.ToRadians(Rotation.X),
                    Vim.MathOps.ToRadians(Rotation.Z)
                );
                model *= Matrix4x4.CreateTranslation(Position);
                if (Parent != null) return Parent.Transform * model;
                else return model;
            }
        }
        internal class GameObjectData
        {
            public GameObject Parent = null;
            public Layer Layer = Application.Get();
            public Mesh Mesh = Mesh.Empty;
            public Material Material = null;
            public bool IsRuntimeObject = true;
            public bool IsSelected = false;
            public bool IsEditing = false;
            public string RuntimeID = "";
            public static uint LastID = 0;
            public uint ID = 0;
            public unsafe IntPtr Handle => (IntPtr)ID;
            public List<EditorGUI.Node> Children = new List<EditorGUI.Node>();
            public GameObjectData()
            {
                ID = LastID + 1;
                LastID = ID;
            }
            public void Set(GameObjectData Data)
            {
                if (Data == null) Data = new GameObjectData();
                Parent = Data.Parent;
                Layer = Data.Layer;
                Mesh = Data.Mesh;
                Material = Data.Material;
                IsSelected = Data.IsSelected;
                IsEditing = Data.IsEditing;
                ID = Data.ID;
                Children = Data.Children;
            }
        }
    }
}
