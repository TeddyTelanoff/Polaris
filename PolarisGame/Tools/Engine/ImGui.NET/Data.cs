using System;
using System.Collections.Generic;
using System.Text;

namespace ImGuiNET
{
    public static class Data
    {
        public static IntPtr _Context = IntPtr.Zero;
        public static IntPtr Context
        {
            get
            {
                if (_Context == null)
                {
                    throw new Exception("Context == null");
                }
                return _Context;
            }
            set
            {
                _Context = value;
            }
        }
        public static void Init()
        {
            ImGuiNET.ImGui.SetCurrentContext(ImGuiNET.Data.Context);
        }
    }
}
