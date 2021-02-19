using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace Polaris
{
    internal class EditorGUI
    {
        internal static float ViewportAspectRatio = 1.0f;
        internal static bool ShowHidden = false;
        internal static bool ProjectDialog = false;
        internal static string NewGameName = "";
        internal static string MaterialName = "";
        internal static List<Material> Materials = new List<Material>();
        internal unsafe static void Render()
        {
            Vector2 WindowMinSize = ImGui.GetStyle().WindowMinSize;
            ImGui.GetStyle().WindowMinSize.X = 350;
            ImGui.DockSpaceOverViewport();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.Button("New Game (Using Current Template)"))
                    {
#if POLARIS_EDITOR
                        NewGameName = "";
                        ProjectDialog = true;
#endif
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
            ImGui.GetStyle().WindowMinSize = WindowMinSize;
            DrawHierarchy();
            Vector4 bg = new Vector4(0, 0, 0, 1);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, bg);
            Vector4 bg1 = new Vector4(0.03f, 0.03f, 0.03f, 1);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, bg1);
            ImGui.PushStyleColor(ImGuiCol.Button, bg1);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, bg1);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, bg1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (ImGui.Begin("Scene", ImGuiWindowFlags.NoScrollbar))
            {
                if (ProjectDialog)
                {
                    Vector4 bg2 = new Vector4(0.23f, 0.23f, 0.23f, 1);
                    ImGui.PushStyleColor(ImGuiCol.Text, bg2);
                    ImGui.PushFont(ImGuiRenderer.FontDefaultHuge);
                    Vector2 pos = ImGui.GetWindowSize() * 0.2f;
                    pos.X = ImGui.GetWindowSize().X * 0.5f - 465;
                    ImGui.SetCursorPos(pos);
                    ImGui.Text("\t\t[Polaris Project System]\n\n\t\t\tNew Game Project\n\t\t\t\t  Enter name");
                    ImGui.SetWindowFontScale(1);
                    pos = ImGui.GetWindowSize() * 0.75f;
                    pos.X = 0;
                    ImGui.SetCursorPos(pos);
                    ImGui.PopFont();
                    ImGui.PopStyleColor();
                    ImGui.InputText("##Name", ref NewGameName, 100);
                    ImGui.SameLine();
                    if (ImGui.Button("Create (or, press Esc to cancel)", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                    {
#if POLARIS_EDITOR
                        if (NewGameName.Length > 0)
                        {
                            Directory.CreateDirectory(@"C:\Users\Public\Documents\Polaris");
                            DirectoryCopy(VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName, @"C:\Users\Public\Documents\Polaris\" + NewGameName, true);
                            System.Threading.Thread.Sleep(500);
                            new Process
                            {
                                StartInfo = new ProcessStartInfo(@"C:\Users\Public\Documents\Polaris\" + NewGameName)
                                {
                                    UseShellExecute = true
                                }
                            }.Start();
                            Application.Get().MainWindow.Close();
                        }
#endif
                    }
                }
                else
                {
                    Vector2 ImgSize = ImGui.GetContentRegionAvail();
                    if (ImgSize.Y > ImgSize.X)
                    {
                        ImgSize.X = ImgSize.Y * ViewportAspectRatio;
                    }
                    else
                    {
                        ImgSize.Y = ImgSize.X * ViewportAspectRatio;
                    }
                    ImGui.SetCursorPos((ImGui.GetWindowSize() - ImgSize) * 0.5f);
                    ImGui.Image((IntPtr)SceneRenderer.FinalNativeTexture, ImgSize,
                        new Vector2(0, 1), new Vector2(1, 0));
                }
                ImGui.End();
            }
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            if (ImGui.Begin("Details"))
            {
                foreach (GameObject go in Scene.LoadedScene.AllNodes.Where(go => go.Data != null && go.Data.IsSelected))
                {
                    float buffer1 = 100;
                    float buffer2 = 100;
                    Vector3 Ps = go.Position;
                    if (DrawVector3("Position", buffer1, ref Ps, Vector3.Zero, buffer2, 0.02f))
                    {
                        go.Position = Ps;
                    }
                    Vector3 Rt = go.Rotation;
                    if (DrawVector3("Rotation", buffer1, ref Rt, Vector3.Zero, buffer2, 1))
                    {
                        go.Rotation = Rt;
                    }
                    Vector3 Sc = go.Scale;
                    if (DrawVector3("Scale", buffer1, ref Sc, Vector3.One, buffer2, 0.02f))
                    {
                        go.Scale = Sc;
                    }

                    ImGui.NewLine();

                    //ImGui.Text("Shader");
                    //int sh = 0;
                    //ImGui.Combo("##sh", ref sh, )
                    ImGui.TreePush();
                    if (ImGui.TreeNode("Materials"))
                    {
                        ImGui.Text("Material Properties");
                        if (go.Material != null && go.Material is Material)
                        {
                            if (ImGui.TreeNode(((Material)go.Material).MaterialName))
                            {
                                ImGui.Text("'" + ((Material)go.Material).MaterialName + "' Properties");
                                ImGui.Text("Shader: " + ((Material)go.Material).ShaderName);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            if (ImGui.TreeNode("None"))
                            {
                                ImGui.TreePop();
                            }
                        }
                        ImGui.NewLine();
                        ImGui.Text("Material Asset Explorer");
                        if (ImGui.TreeNode("Tools"))
                        {
                            ImGui.Text("New Material");
                            ImGui.InputText("##nmat", ref MaterialName, 100);
                            if (ImGui.Button("Create"))
                            {
                                Material mat = new Material(@"Assets\builtin\shaders\std");
                                mat.MaterialName = MaterialName;
                                go.Material = mat;
                            }
                            ImGui.NewLine();
                            ImGui.Text("New Name");
                            ImGui.InputText("##nmat1", ref MaterialName, 100);
                            if (ImGui.Button("Rename"))
                            {
                                if (go.Material != null && go.Material is Material) ((Material)go.Material).MaterialName = MaterialName;
                            }
                            ImGui.NewLine();
                            ImGui.Text("Use Material");
                            ImGui.InputText("##nmat2", ref MaterialName, 100);
                            if (ImGui.Button("Set"))
                            {
                                try
                                {
                                    go.Material = Materials.First(mat => mat.MaterialName.ToLower() == MaterialName.ToLower());
                                }
                                catch 
                                {
                                    MaterialName = "Error: That material doesn't exist!";
                                }
                            }
                            ImGui.NewLine();
                            if (ImGui.Button("Delete Material"))
                            {
                                if (go.Material != null && go.Material is Material) Materials.RemoveAll(m => m.MaterialName == ((Material)go.Material).MaterialName);
                                go.Material = null;
                            }
                            ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                    break;
                }
                ImGui.End();
            }
            if (ImGui.Begin("Debugger"))
            {
                if (ImGui.Button(!ShowHidden ? "Show Hidden" : "Hide Hidden"))
                {
                    ShowHidden ^= true;
                }
                ImGui.Text("Add items to Polaris.Debug.Items to print them here!");
                foreach (KeyValuePair<string, object> obj in Debug.Items)
                {
                    ImGui.Spacing();
                    ImGui.TreePush();
                    if (ImGui.TreeNodeEx(obj.Key, ImGuiTreeNodeFlags.SpanFullWidth))
                    {
                        Print(obj.Value);
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                }
                ImGui.End();
            }
        }
        static Vector4 Base = new Vector4(0.502f, 0.075f, 0.256f, 1.0f);
        static Vector4 bg = new Vector4(0.200f, 0.220f, 0.270f, 1.0f);
        static Vector4 text = new Vector4(0.860f, 0.930f, 0.890f, 1.0f);
        static float high_val = 0.8f;
        static float mid_val = 0.5f;
        static float low_val = 0.3f;
        static float window_offset = -0.2f;
        static Vector4 make_high(float alpha)
        {
            Vector4 res = new Vector4(0, 0, 0, alpha);
            ImGui.ColorConvertRGBtoHSV(Base.X, Base.Y, Base.Z, out res.X, out res.Y, out res.Z);
            res.Z = high_val;
            ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
            return res;
        }
        static Vector4 make_mid(float alpha)
        {
            Vector4 res = new Vector4(0, 0, 0, alpha);
            ImGui.ColorConvertRGBtoHSV(Base.X, Base.Y, Base.Z, out res.X, out res.Y, out res.Z);
            res.Z = mid_val;
            ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
            return res;
        }
        static Vector4 make_low(float alpha)
        {
            Vector4 res = new Vector4(0, 0, 0, alpha);
            ImGui.ColorConvertRGBtoHSV(Base.X, Base.Y, Base.Z, out res.X, out res.Y, out res.Z);
            res.Z = low_val;
            ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
            return res;
        }
        static Vector4 make_bg(float alpha, float offset = 0.0f)
        {
            Vector4 res = new Vector4(0, 0, 0, alpha);
            ImGui.ColorConvertRGBtoHSV(Base.X, Base.Y, Base.Z, out res.X, out res.Y, out res.Z);
            res.Z += offset;
            ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
            return res;
        }
        static Vector4 make_text(float alpha)
        {
            return new Vector4(text.X, text.Y, text.Z, alpha);
        }
        static void theme_generator()
        {
            ImGui.Begin("Theme generator");
            ImGui.ColorEdit4("base", ref Base, ImGuiColorEditFlags.PickerHueWheel);
            ImGui.ColorEdit4("bg", ref bg, ImGuiColorEditFlags.PickerHueWheel);
            ImGui.ColorEdit4("text", ref text, ImGuiColorEditFlags.PickerHueWheel);
            ImGui.SliderFloat("high", ref high_val, 0, 1);
            ImGui.SliderFloat("mid", ref mid_val, 0, 1);
            ImGui.SliderFloat("low", ref low_val, 0, 1);
            ImGui.SliderFloat("window", ref window_offset, -0.4f, 0.4f);

            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Text] = make_text(0.78f);
            style.Colors[(int)ImGuiCol.TextDisabled] = make_text(0.28f);
            style.Colors[(int)ImGuiCol.WindowBg] = make_bg(1.00f, window_offset);
            style.Colors[(int)ImGuiCol.ChildBg] = make_bg(0.58f);
            style.Colors[(int)ImGuiCol.PopupBg] = make_bg(0.9f);
            style.Colors[(int)ImGuiCol.Border] = make_bg(0.6f, -0.05f);
            style.Colors[(int)ImGuiCol.BorderShadow] = make_bg(0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = make_bg(1.00f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = make_mid(0.78f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.TitleBg] = make_low(1.00f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = make_high(1.00f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = make_bg(0.75f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = make_bg(0.47f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = make_bg(1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = make_low(1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = make_mid(0.78f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.CheckMark] = make_high(1.00f);
            style.Colors[(int)ImGuiCol.SliderGrab] = make_bg(1.0f, .1f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = make_high(1.0f);
            style.Colors[(int)ImGuiCol.Button] = make_bg(1.0f, .2f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.ButtonActive] = make_high(1.00f);
            style.Colors[(int)ImGuiCol.Header] = make_mid(0.76f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = make_mid(0.86f);
            style.Colors[(int)ImGuiCol.HeaderActive] = make_high(1.00f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.47f, 0.77f, 0.83f, 0.04f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = make_mid(0.78f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.PlotLines] = make_text(0.63f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = make_text(0.63f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = make_mid(0.43f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = make_bg(0.73f);
            style.Colors[(int)ImGuiCol.Tab] = make_bg(0.40f);
            style.Colors[(int)ImGuiCol.TabHovered] = make_high(1.00f);
            style.Colors[(int)ImGuiCol.TabActive] = make_mid(1.00f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = make_bg(0.40f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = make_bg(0.70f);
            style.Colors[(int)ImGuiCol.DockingPreview] = make_high(0.30f);

            if (ImGui.Button("Export"))
            {
                File.Delete(@"C:\Users\Public\Documents\theme.txt");
                StreamWriter sw = File.AppendText(@"C:\Users\Public\Documents\theme.txt");
                sw.WriteLine("ImGuiStylePtr style = ImGui.GetStyle();\n");
                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    Vector4 col = style.Colors[i];
                    string name = Enum.GetNames(typeof(ImGuiCol))[i];
                    sw.WriteLine("style.Colors[(int)ImGuiCol." + name + "] = new Vector4(" +
                        col.X.ToString() + "f, " + col.Y.ToString() + "f, " +
                        col.Z.ToString() + "f, " + col.W.ToString() + "f);\n");
                }
                sw.Close();
                new Process
                {
                    StartInfo = new ProcessStartInfo(@"C:\Users\Public\Documents\theme.txt")
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            ImGui.End();
        }
        // DrawVector3 - Written by me,
        // Inspired by the Hazel Game Engine, DrawVec3Control method @ https://github.com/TheCherno/Hazel/blob/master/Hazelnut/src/Panels/SceneHierarchyPanel.cpp
        #region DrawVector3
        internal static bool DrawVector3(string text, float text_width, ref Vector3 vector, Vector3 init, float buffer, float speed)
        {
            return DrawVector3(text, text_width, ref vector, init, Vector3.Zero, Vector3.Zero, buffer, speed);
        }
        internal static bool DrawVector3(string text, float text_width, ref Vector3 vector, Vector3 init, Vector3 min, Vector3 max, float buffer, float speed)
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            float size_x = ImGui.GetContentRegionAvail().X - buffer;

            if (min == Vector3.Zero && max == Vector3.Zero)
            {
                min = new Vector3(float.MinValue);
                max = new Vector3(float.MaxValue);
            }

            Vector3 new_vector = vector;
            bool IsUsed = false;

            ImGui.PushID(text);

            float NextCursorPosX = 0;

            // X
            NextCursorPosX = ImGui.GetCursorPosX() + text_width;

            ImGui.Text(text);
            ImGui.SameLine();
            ImGui.SetCursorPosX(NextCursorPosX);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, -1));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1, 0, 0, 1));
            if (ImGui.Button("X")) { new_vector.X = init.X; IsUsed = true; }
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.PushItemWidth(size_x / 3.0f - text_width / 3.0f + buffer / 6.0f);
            if (ImGui.DragFloat("##x", ref new_vector.X, speed, min.X, max.X)) IsUsed = true;
            ImGui.PopItemWidth();

            // Y
            NextCursorPosX = ImGui.GetCursorPosX() + text_width;
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, -1));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0.7f, 0, 1));
            if (ImGui.Button("Y")) { new_vector.Y = init.Y; IsUsed = true; }
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.PushItemWidth(size_x / 3.0f - text_width / 3.0f + buffer / 6.0f);
            if (ImGui.DragFloat("##y", ref new_vector.Y, speed, min.Y, max.Y)) IsUsed = true;
            ImGui.PopItemWidth();

            // Z
            NextCursorPosX = ImGui.GetCursorPosX() + text_width;
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, -1));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0.2f, 0.7f, 1));
            if (ImGui.Button("Z")) { new_vector.Z = init.Z; IsUsed = true; }
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.PushItemWidth(size_x / 3.0f - text_width / 3.0f + buffer / 6.0f);
            if (ImGui.DragFloat("##z", ref new_vector.Z, speed, min.Z, max.Z)) IsUsed = true;
            ImGui.PopItemWidth();

            ImGui.PopID();

            if (IsUsed) vector = new_vector;
            ImGui.PopStyleVar();
            return IsUsed;
        }
        #endregion
        internal static Vector2 CalcItemSize(Vector2 size, float default_x, float default_y)
        {
            Vector2 content_max = Vector2.Zero;
            if (size.X < 0.0f || size.Y < 0.0f)
                content_max = ImGui.GetWindowPos() + ImGui.GetContentRegionMax();
            if (size.X <= 0.0f)
                size.X = (size.X == 0.0f) ? default_x : Math.Max(content_max.X - ImGui.GetCursorPos().X, 4.0f) + size.X;
            if (size.Y <= 0.0f)
                size.Y = (size.Y == 0.0f) ? default_y : Math.Max(content_max.Y - ImGui.GetCursorPos().Y, 4.0f) + size.Y;
            return size;
        }
        internal static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (dir.Name == "bin" || dir.Name == "obj") return;

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        internal static class VisualStudioProvider
        {
            internal static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
            {
                var directory = new DirectoryInfo(
                    currentPath ?? Directory.GetCurrentDirectory());
                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }
                return directory;
            }
        }
        internal class Node
        {
            internal static Node MainHierarchyNode = null;
            internal static Node DraggedNode = null;
            internal static ImGuiPayload Dummy = new ImGuiPayload();
            internal const string Add = "+";
            internal const string Subtract = "-";
            internal bool IsEditing
            {
                get
                {
                    return Data.IsEditing;
                }
                set
                {
                    Data.IsEditing = value;
                }
            }
            internal IntPtr Handle => Data.Handle;
            internal uint ID => Data.ID;
            internal string Name = "";
            internal Guid GUID = Guid.Empty;
            internal Node Parent = null;
            internal List<Node> Children
            {
                get
                {
                    return Data.Children;
                }
                set
                {
                    Data.Children = value;
                }
            }
            internal bool IsSelected
            {
                get
                {
                    return Data.IsSelected;
                }
                set
                {
                    Data.IsSelected = value;
                }
            }
            internal Node(Node parent, GameObject.GameObjectData Data, GameObject go) : this()
            {
                this.Data.Set(Data);
                if (go != null) GUID = go.GUID;
                List<Node> Lst;
                if (Parent != null)
                {
                    Lst = Parent.Children;
                    Lst.RemoveAll(c => c.GUID == GUID);
                    Parent.Children = Lst;
                }
                Parent = parent;
                if (parent != null)
                {
                    Lst = Parent.Children;
                    Lst.Add(this);
                    Parent.Children = Lst;
                }
            }
            internal static Dictionary<Guid, GameObject.GameObjectData> DataDict =
                new Dictionary<Guid, GameObject.GameObjectData>();
            internal GameObject.GameObjectData Data
            {
                get
                {
                    if (!DataDict.ContainsKey(GUID))
                    {
                        DataDict.Add(GUID, new GameObject.GameObjectData());
                    }
                    return DataDict[GUID];
                }
            }
            static Node()
            {
                MainHierarchyNode = new Node
                {
                    Name = "Scene"
                };
                MainHierarchyNode.Data.Set(new GameObject.GameObjectData());
                MainHierarchyNode.GUID = Guid.NewGuid();
                MainHierarchyNode.Data.ID = MainHierarchyNode.ID;
                MainHierarchyNode.Data.IsRuntimeObject = false;
            }
            protected Node() { }
        }
        internal unsafe static void DrawHierarchy()
        {
            if (ImGui.Begin("Hierarchy"))
            {
                SceneToNodes(Scene.LoadedScene);
                ImGui.TreePush();
                DrawNode(Node.MainHierarchyNode);
                ImGui.TreePop();
                ImGui.End();
            }
        }
        internal unsafe static void SceneToNodes(Scene Scene)
        {
            foreach (Node n in Node.MainHierarchyNode.Children.ToList()) n.Parent = null;
            Node.MainHierarchyNode.Children.Clear();
            foreach (GameObject go in Scene.AllNodes.Where(go => go.Parent == null))
            {
                Node n = new Node(Node.MainHierarchyNode, go.Data, go)
                {
                    Name = go.Name
                };
                n.Children = SceneToNodes(go, n);
            }
        }
        internal unsafe static List<Node> SceneToNodes(GameObject Scene, Node node)
        {
            List<Node> all = new List<Node>();
            foreach (GameObject go in Scene.Nodes)
            {
                Node n = new Node(node, go.Data, go)
                {
                    Name = go.Name
                };
                n.Children = SceneToNodes(go, n);
                all.Add(n);
            }
            return all;
        }
        internal unsafe static void RegisterDragger(Node node, bool TargetOnly = false)
        {
            if (!TargetOnly)
            {
                if (ImGui.BeginDragDropSource())
                {
                    ImGui.Button(node.Name);
                    Node.DraggedNode = node;
                    fixed (ImGuiPayload* Dummy = &Node.Dummy)
                    {
                        ImGui.SetDragDropPayload("Hierarchy", (IntPtr)Dummy, sizeof(int));
                        Node.DraggedNode = node;
                    }
                    ImGui.EndDragDropSource();
                }
            }
            if (ImGui.BeginDragDropTarget())
            {
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    try
                    {
                        ImGui.AcceptDragDropPayload("Hierarchy");
                        if (node.Name != Node.Add)
                        {
                            if ((Scene.LoadedScene.AllNodes.Any(n => n.Data.ID == node.ID) &&
                                Scene.LoadedScene.AllNodes.Any(n => n.Data.ID == Node.DraggedNode.ID)) || node.Parent != null &&
                                Node.DraggedNode.Parent.ID == node.Parent.ID)
                            {
                                int me = -1;
                                int them = -1;
                                if (node.Parent != null && Node.DraggedNode.Parent != null)
                                {
                                    me = node.Parent.Children.FindIndex(n => n.ID == node.ID);
                                    them = Node.DraggedNode.Parent.Children.FindIndex(n => n.ID == Node.DraggedNode.ID);
                                }
                                else
                                {
                                    me = Scene.LoadedScene.AllNodes.FindIndex(n => n.Data.ID == node.ID);
                                    them = Scene.LoadedScene.AllNodes.FindIndex(n => n.Data.ID == Node.DraggedNode.ID);
                                }
                                GameObject Me = null;
                                try
                                {
                                    Me = Scene.LoadedScene.AllNodes.First(n => n.GUID == node.GUID);
                                }
                                catch { }
                                GameObject Them = null;
                                try
                                {
                                    Them = Scene.LoadedScene.AllNodes.First(n => n.GUID == Node.DraggedNode.GUID);
                                }
                                catch { }
                                if (me >= 0 && them >= 0 && Me != null && Them != null &&
                                    Me.Parent != null && Them.Parent != null)
                                {
                                    List<GameObject> Lst = Them.Parent.Nodes.ToList();
                                    Lst = Lst.Swap(me, them).ToList();
                                    Them.Parent.Nodes = Lst.ToArray();
                                }
                                else
                                {
                                    List<GameObject> Lst = Scene.LoadedScene.AllNodes;
                                    Lst = Lst.Swap(me, them).ToList();
                                    Scene.LoadedScene.AllNodes = Lst;
                                }
                            }
                        }
                        else
                        {
                            GameObject Me = null;
                            try
                            {
                                Me = Scene.LoadedScene.AllNodes.First(n => n.GUID == node.Parent.GUID);
                            }
                            catch { }
                            GameObject Them = null;
                            try
                            {
                                Them = Scene.LoadedScene.AllNodes.First(n => n.GUID == Node.DraggedNode.GUID);
                            }
                            catch { }
                            if (Me != null)
                            {
                                Me.AddChild(Them);
                            }
                            else
                            {
                                Them.Parent = null;
                            }
                        }
                    }
                    catch { }
                    ImGui.EndDragDropTarget();
                }
            }
        }
        internal unsafe static void DrawNode(Node node)
        {
            if (node.Name != Node.Add)
            {
                node.Children.RemoveAll(child => child.Name == Node.Add);
                Node add = new Node(node, null, null)
                {
                    Name = Node.Add
                };
                bool Selected = node.IsSelected;
                ImGui.Checkbox("##" + node.ID, ref Selected);
                node.IsSelected = Selected;
                ImGui.SameLine();
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanFullWidth;
                if (node.ID == Node.MainHierarchyNode.ID)
                {
                    flags |= ImGuiTreeNodeFlags.DefaultOpen;
                }
                if (ImGui.TreeNodeEx(node.Handle, flags, node.Name + (node.Data.IsRuntimeObject ? " (Runtime Object)" : "")))
                {
                    foreach (Node child in node.Children.ToList())
                    {
                        DrawNode(child);
                    }
                    ImGui.TreePop();
                }
                RegisterDragger(node);
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && node.Parent != null)
                {
                    node.IsEditing = true;
                }
                if (node.IsEditing)
                {
                    if (ImGui.BeginPopupContextWindow("Edit Node"))
                    {
                        ImGui.InputText("Name", ref Scene.LoadedScene.AllNodes.First(n => n.Data.ID == node.ID).Name, 100);
                        if (ImGui.Button("OK"))
                        {
                            node.IsEditing = false;
                        }
                        if (ImGui.Button("Delete"))
                        {
                            node.IsEditing = false;
                            Scene.LoadedScene.AllNodes.First(n => n.GUID == node.GUID).Destroy();
                        }
                        ImGui.EndPopup();
                    }
                }
            }
            else
            {
                DrawAdder(node);
            }
        }
        internal unsafe static void DrawAdder(Node node)
        {
            bool Btn = ImGui.Button(Node.Add);
            RegisterDragger(node, true);
            if (Btn)
            {
                GameObject add = new GameObject(Guid.NewGuid().ToString(), false)
                {
                    Name = "GameObject"
                };
                try
                {
                    add.Parent = Scene.LoadedScene.AllNodes.First(n => n.GUID == node.Parent.GUID);
                }
                catch
                {
                    add.Parent = null;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button(Node.Subtract))
            {
                try
                {
                    Node n = node.Parent;
                    if (n != null && n.Parent != null && n.Parent.Parent != null)
                    {
                        GameObject go = Scene.LoadedScene.AllNodes.First(i => i.Data.ID == n.ID);
                        go.Parent = go.Parent.Parent;
                    }
                }
                catch { }
            }
        }
        internal static void Print(object obj, bool push = true)
        {
            if (obj != null)
            {
                if (push) ImGui.TreePush();
                var Lst = obj.GetType().GetProperties
                    (BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select
                    (i => new CustomPropertyDescriptor(i));
                if (!ShowHidden)
                {
                    Lst = obj.GetType().GetProperties
                       (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).Select
                       (i => new CustomPropertyDescriptor(i));
                }
                try
                {
                    foreach (CustomPropertyDescriptor descriptor in Lst)
                    {
                        try
                        {
                            string name = descriptor.Name;
                            object value = descriptor.GetValue(obj);
                            if (ImGui.TreeNodeEx(name + " = " + (value != null ? value.ToString() : "NULL"), ImGuiTreeNodeFlags.SpanFullWidth))
                            {
                                Print(value, false);
                                ImGui.TreePop();
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                var Lst2 = obj.GetType().GetFields
                    (BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select
                    (i => new CustomFieldDescriptor(i));
                if (!ShowHidden)
                {
                    Lst2 = obj.GetType().GetFields
                       (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).Select
                       (i => new CustomFieldDescriptor(i));
                }
                try
                {
                    foreach (CustomFieldDescriptor descriptor in Lst2)
                    {
                        try
                        {
                            string name = descriptor.Name;
                            object value = descriptor.GetValue(obj);
                            if (ImGui.TreeNodeEx(name + " = " + (value != null ? value.ToString() : "NULL"), ImGuiTreeNodeFlags.SpanFullWidth))
                            {
                                Print(value, false);
                                ImGui.TreePop();
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                try
                {
                    int i = 0;
                    foreach (object o in (IEnumerable<object>)obj)
                    {
                        if (ImGui.TreeNodeEx("[" + i + "]", ImGuiTreeNodeFlags.SpanFullWidth))
                        {
                            Print(o, false);
                            ImGui.TreePop();
                        }
                        i++;
                    }
                }
                catch { }
                if (push) ImGui.TreePop();
            }
        }
        internal class CustomPropertyDescriptor : PropertyDescriptor
        {
            readonly PropertyInfo propertyInfo;
            internal CustomPropertyDescriptor(PropertyInfo propertyInfo)
                : base(propertyInfo.Name, Array.ConvertAll(propertyInfo.GetCustomAttributes(true), o => (Attribute)o))
            {
                this.propertyInfo = propertyInfo;
            }
            public override bool CanResetValue(object component)
            {
                return false;
            }
            public override Type ComponentType
            {
                get
                {
                    return this.propertyInfo.DeclaringType;
                }
            }
            public override object GetValue(object component)
            {
                return this.propertyInfo.GetValue(component, null);
            }
            public override bool IsReadOnly
            {
                get
                {
                    return !this.propertyInfo.CanWrite;
                }
            }
            public override Type PropertyType
            {
                get
                {
                    return this.propertyInfo.PropertyType;
                }
            }
            public override void ResetValue(object component)
            {
            }
            public override void SetValue(object component, object value)
            {
                this.propertyInfo.SetValue(component, value, null);
            }
            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
        internal class CustomFieldDescriptor : PropertyDescriptor
        {
            readonly FieldInfo FieldInfo;
            internal CustomFieldDescriptor(FieldInfo FieldInfo)
                : base(FieldInfo.Name, Array.ConvertAll(FieldInfo.GetCustomAttributes(true), o => (Attribute)o))
            {
                this.FieldInfo = FieldInfo;
            }
            public override bool CanResetValue(object component)
            {
                return false;
            }
            public override Type ComponentType
            {
                get
                {
                    return this.FieldInfo.DeclaringType;
                }
            }
            public override object GetValue(object component)
            {
                return this.FieldInfo.GetValue(component);
            }
            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            internal Type FieldType
            {
                get
                {
                    return this.FieldInfo.FieldType;
                }
            }
            public override Type PropertyType => FieldType;
            public override void ResetValue(object component)
            {
            }
            public override void SetValue(object component, object value)
            {
                this.FieldInfo.SetValue(component, value);
            }
            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
        internal static bool LastMousePositionSet = false;
        internal static Vector2 LastMousePosition = new Vector2(-1, -1);
        internal static float RotSpeed = 50;
        internal static bool ShouldUpdateEditorCamera = false;
        internal static void UpdateEditorCamera()
        {
            if (ShouldUpdateEditorCamera)
            {
                float DeltaX = 0;
                float DeltaY = 0;
                Application.Get().input.Mice[0].Cursor.CursorMode = CursorMode.Hidden;
                Application.Get().input.Mice[0].Cursor.IsConfined = true;
                if (LastMousePositionSet)
                {
                    DeltaX = Application.Get().input.Mice[0].Position.X - LastMousePosition.X;
                    DeltaY = Application.Get().input.Mice[0].Position.Y - LastMousePosition.Y;
                    Application.Get().input.Mice[0].Position -= new Vector2(DeltaX, DeltaY);
                }
                else
                {
                    Application.Get().input.Mice[0].Position = new Vector2
                        (Application.Get().MainWindow.Position.X +
                         Application.Get().MainWindow.Size.X / 2.0f,
                         Application.Get().MainWindow.Position.Y +
                         Application.Get().MainWindow.Size.Y / 2.0f);
                    Application.Get().input.Mice[0].Cursor.CursorMode = CursorMode.Hidden;
                    Application.Get().input.Mice[0].Cursor.IsConfined = true;
                }
                Vector3 PreRot = Camera.editor.Rotation;
                Camera.editor.Rotation += new Vector3
                (
                    DeltaY * RotSpeed * Time.DeltaTime,
                    DeltaX * RotSpeed * Time.DeltaTime * -1.0f,
                    0.0f
                );
                if (Math.Abs(Camera.editor.Rotation.X) >= 88)
                {
                    Camera.editor.Rotation.X = PreRot.X;
                }
                float Speed = Time.DeltaTime * 400.0f;
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.W))
                {
                    Camera.editor.Position +=
                        Vector3.Transform(new System.Numerics.Vector3(0, 0, Speed),
                        Matrix4x4.CreateFromYawPitchRoll
                        (
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Y), 0,
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Z)
                        ));
                }
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.S))
                {
                    Camera.editor.Position +=
                        Vector3.Transform(new System.Numerics.Vector3(0, 0, -Speed),
                        Matrix4x4.CreateFromYawPitchRoll
                        (
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Y), 0,
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Z)
                        ));
                }
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.A))
                {
                    Camera.editor.Position +=
                        Vector3.Transform(new System.Numerics.Vector3(Speed, 0, 0),
                        Matrix4x4.CreateFromYawPitchRoll
                        (
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Y), 0,
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Z)
                        ));
                }
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.D))
                {
                    Camera.editor.Position +=
                        Vector3.Transform(new System.Numerics.Vector3(-Speed, 0, 0),
                        Matrix4x4.CreateFromYawPitchRoll
                        (
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Y), 0,
                            Vim.MathOps.ToRadians(Camera.editor.Rotation.Z)
                        ));
                }
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.Space))
                {
                    Camera.editor.Position += new System.Numerics.Vector3(0, Speed, 0);
                }
                if (Application.Get().input.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
                {
                    Camera.editor.Position += new System.Numerics.Vector3(0, -Speed, 0);
                }
                if (RotSpeed < 50)
                {
                    RotSpeed += Math.Abs(10.0f + DeltaX + DeltaY) / 3.0f;
                }
                else
                {
                    RotSpeed = 50;
                }
                LastMousePosition = Application.Get().input.Mice[0].Position;
                LastMousePositionSet = true;
            }
            else
            {
                Application.Get().input.Mice[0].Cursor.CursorMode = CursorMode.Normal;
                Application.Get().input.Mice[0].Cursor.IsConfined = false;
            }
        } 
    }
    internal static class SwapExt
    {
        internal static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }
}
