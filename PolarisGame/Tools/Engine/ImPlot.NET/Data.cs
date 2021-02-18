using System;
using System.Collections.Generic;
using System.Text;

namespace ImPlotNET
{
    public static class Data
    {
        public static void Init()
        {
            ImGuiNET.ImGui.SetCurrentContext(ImGuiNET.Data.Context);
        }
    }
}
