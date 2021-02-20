using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Ultz.SilkExtensions.ImGui;
using Silk.NET.Input.Extensions;
using ImGuiNET;
using System.Numerics;
using System.Text;
using System.IO;
using System;

namespace Polaris
{
	internal class ImGuiRenderer : Layer
    {
		public ImGuiController ImGuiController = null;
		public static ImFontPtr FontDefault = null;
		public static ImFontPtr FontDefaultHuge = null;

		public unsafe override void OnAttach()
        {
            ImGuiController = new ImGuiController(
                Application.Get().MainWindow.CreateOpenGL(),
                Application.Get().MainWindow,
                Application.Get().MainWindow.CreateInput()
            );

			ImGui.GetIO().NativePtr->IniFilename = null;
			if (!File.Exists(new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName + @"/imgui.ini"))
			{
				ImGui.SaveIniSettingsToDisk(new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName + @"/imgui.ini");
			}
			ImGui.LoadIniSettingsFromDisk(new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName + @"/imgui.ini");

			ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
			ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

			FontDefault = ImGui.GetIO().Fonts.AddFontFromFileTTF
				(@"builtin\fonts\OpenSans\OpenSans-Regular.ttf", 20);
			ImGuiController.RecreateFontDeviceTexture();

			FontDefaultHuge = ImGui.GetIO().Fonts.AddFontFromFileTTF
				(@"builtin\fonts\OpenSans\OpenSans-Regular.ttf", FontDefault.FontSize * 4);
			ImGuiController.RecreateFontDeviceTexture();

			ImGuiStylePtr style = ImGui.GetStyle();

			style.Alpha = 1.0f;
			style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
			style.WindowMinSize = new Vector2(200, 200);
			style.FramePadding = new Vector2(4, 2);
			style.ItemSpacing = new Vector2(6, 3);
			style.ItemInnerSpacing = new Vector2(6, 4);
			style.WindowRounding = 0.0f;
			style.FrameRounding = 0.0f;
			style.ColumnsMinSpacing = 50.0f;
			style.GrabMinSize = 14.0f;
			style.GrabRounding = 6.0f;
			style.ScrollbarSize = 12.0f;
			style.ScrollbarRounding = 30.0f;
			style.AntiAliasedLines = true;
			style.AntiAliasedFill = true;

			style.Colors[(int)ImGuiCol.Text] = new Vector4(0.9999f, 1f, 0.99994284f, 0.78f);

			style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.9999f, 1f, 0.99994284f, 0.28f);

			style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(-0.14471881f, -0.16687626f, -0.1732549f, 1f);

			style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.58f);

			style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.9f);

			style.Colors[(int)ImGuiCol.Border] = new Vector4(0.010645908f, 0.012275871f, 0.012745101f, 0.6f);

			style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0f);

			style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 1f);

			style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.78f);

			style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.099400006f, 0.11461884f, 0.119f, 1f);

			style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.75f);

			style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.47f);

			style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 1f);

			style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.099400006f, 0.11461884f, 0.119f, 1f);

			style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.78f);

			style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.13594003f, 0.15675339f, 0.1627451f, 1f);

			style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.Button] = new Vector4(0.21946944f, 0.25307176f, 0.2627451f, 1f);

			style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.Header] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.76f);

			style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.86f);

			style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.43f, 0.43f, 0.5f, 0.5f);

			style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1f, 0.4f, 0.75f, 0.78f);

			style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1f, 0.4f, 0.75f, 1f);

			style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.47f, 0.77f, 0.83f, 0.04f);

			style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.78f);

			style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.4f);

			style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 1f);

			style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.4f);

			style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.7f);

			style.Colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.18292941f, 0.21093717f, 0.219f, 0.3f);

			style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.2f, 0.2f, 0.2f, 1f);

			style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.9999f, 1f, 0.99994284f, 0.63f);

			style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.9999f, 1f, 0.99994284f, 0.63f);

			style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 1f);

			style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.1269647f, 0.14640388f, 0.152f, 0.43f);

			style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1f, 1f, 0f, 0.9f);

			style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1f);

			style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1f, 1f, 1f, 0.7f);

			style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.8f, 0.8f, 0.8f, 0.2f);

			style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.052410614f, 0.060435046f, 0.0627451f, 0.73f);

		}

		public override void OnUpdate()
		{
			ImGuiController.Update(Time.DeltaTime);
			ImGui.PushFont(FontDefault);
			EditorGUI.Render();
			ImGui.PopFont();
			ImGuiController.Render();
        }

        public override void OnDetach()
		{
			ImGui.SaveIniSettingsToDisk(new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName + @"/imgui.ini");
			ImGuiController?.Dispose();
        }

        public override string GetName()
        {
			return "ImGuiRenderer";

		}
    }
}
