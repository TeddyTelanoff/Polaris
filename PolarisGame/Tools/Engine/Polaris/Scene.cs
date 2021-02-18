using System;
using System.Collections.Generic;
using System.Text;

namespace Polaris
{
    public class Scene
    {
        public static Scene LoadedScene = new Scene();

        internal List<GameObject> AllNodes = new List<GameObject>();

        public IEnumerable<GameObject> FindAll()
        {
            foreach (GameObject go in AllNodes)
            {
                if (go.Name != EditorGUI.Node.Add)
                {
                    yield return go;
                }
            }
        }
    }
}
