using System;
using System.Collections.Generic;
using System.Text;

namespace Polaris
{
    public class Camera
    {
        public static Camera current = new Camera();
        public static Camera editor = new Camera { Position = new System.Numerics.Vector3(0, 0, 10) };
        public System.Numerics.Vector3 Position = System.Numerics.Vector3.Zero;
        public System.Numerics.Vector3 Rotation = new System.Numerics.Vector3(0, 180, 0);
        public float FOV = 45;
        public float Near = 0.01f;
        public float Far = 10000;
    }
}
