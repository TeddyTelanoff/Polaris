using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public static class Data
    {
        public static void Init()
        {
            ImGuiNET.ImGui.SetCurrentContext(ImGuiNET.Data.Context);
        }
    }
}
