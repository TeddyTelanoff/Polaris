using System;
using System.Collections.Generic;
using System.Text;

namespace Polaris
{
    public abstract class Layer
    {
        public abstract string GetName();
        public abstract void OnAttach();
        public abstract void OnUpdate();
        public abstract void OnDetach();
    }
}
