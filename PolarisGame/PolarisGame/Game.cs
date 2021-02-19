using Polaris;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PolarisGame
{
    public class Game : Application
    {
        static GameObject Drill = null;
        static Mesh DrillMesh = null;
        static Material DrillMat = null;
        static Texture DrillTex = null;

        public override void OnAttach()
        {
            Drill = new GameObject("Drill A")
            {
                Name = "Drill"
            };
            DrillMesh = Mesh.LoadFromModel(@"Assets/builtin/gl/tests/test1/test_assets/Drill_01.FBX");
            Drill.Mesh = DrillMesh;
            DrillMat = new Material(@"Assets\builtin\shaders\std");
            DrillTex = new Texture(@"Assets/builtin/gl/tests/test1/test_assets/Drill_01_8-bit_Diffuse.png");
            DrillMat.SetProperty("MainTexture", DrillTex);
            Drill.Material = DrillMat;
            Drill.Scale = new Vector3(0.06f, 0.06f, 0.06f);
            Drill.Position = new Vector3(0, 0, 0);
            Camera.current.Position = new Vector3(0, 0, -10);
            Console.WriteLine("Hello World!");
        }

        public override void OnUpdate()
        {
            //Console.WriteLine("Hello Update!");
        }

        public override void OnDetach()
        {
            Console.WriteLine("Goodbye World!");
        }

        public override string GetName()
        {
            return "Application";
        }
    }
}
