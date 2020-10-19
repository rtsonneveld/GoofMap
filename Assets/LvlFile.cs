using GoofMap.Behaviour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GoofMap {
    public class LvlFile : MonoBehaviour {

        private StreamReader reader;

        public string levelName;
        public string meshName;
        public string envName;
        public string materialName;
        public string musicName;

        public void Read(StreamReader reader, Loader loader, string levelName)
        {
            this.levelName = levelName;

            if (!loader.LevelSpawnInfo.ContainsKey(levelName)) {
                loader.LevelSpawnInfo.Add(levelName, new SpawnInfo());
            }

            while (reader.Peek() >= 0) {
                ReadLine(reader.ReadLine(), loader);
            }

            if (loader.Meshes.ContainsKey(meshName)) {

                GameObject levelMesh = Util.GameObjectFromScene(loader.Meshes[meshName], loader);
                levelMesh.name = "Level Mesh "+meshName;
                levelMesh.transform.parent = gameObject.transform;
            } else {
                Debug.LogError("No mesh found with name " + meshName);
            }
        }

        private void ReadLine(string line, Loader loader)
        {
            string l = line.ToLower().Trim();

            Dictionary<string, string> vars = new Dictionary<string, string>();

            foreach(string p in l.Split(' ')) {
                string[] split = p.Split('=');
                if (split.Length > 1) {

                    if (split[1].EndsWith(">"))
                        split[1] = split[1].Substring(0, split[1].Length - 1);

                    if (vars.ContainsKey(split[0])) {
                        vars.Remove(split[0]);
                    }

                    vars.Add(split[0], split[1].Replace("\"",""));
                }
            }

            if (l.StartsWith("<level ")) {
                ReadLevelInfo(vars, loader);
            } else if (l.StartsWith("<object ")) {
                ReadObjectInfo(vars, loader);
            } else if(l.StartsWith("<trigger ")) {
                ReadTriggerInfo(vars, loader);
            }
        }

        private void ReadTriggerInfo(Dictionary<string, string> vars, Loader loader)
        {
            GameObject triggerGao = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerGao.name = $"Trigger Type {vars["type"]}";
            triggerGao.layer = LayerMask.NameToLayer("Triggers");
            triggerGao.transform.parent = gameObject.transform;
            triggerGao.transform.position = Util.StringToVector3(vars["pos"], loader);
            triggerGao.GetComponent<MeshRenderer>().material = loader.DefaultMaterial;

            if (vars["type"]=="spawn" && vars["id"]=="0") {
                SpawnInfo info = loader.LevelSpawnInfo[levelName];
                info.Position = triggerGao.transform.position;
                loader.LevelSpawnInfo[levelName] = info;
            }

            if (vars["type"] == "spawnlookat" && vars["id"] == "0") {
                SpawnInfo info = loader.LevelSpawnInfo[levelName];
                info.LookAt = triggerGao.transform.position;
                loader.LevelSpawnInfo[levelName] = info;
            }

            triggerGao.transform.localScale = Util.StringToVector3(vars["size"], loader) * 0.5f;
        }

        private void ReadObjectInfo(Dictionary<string, string> vars, Loader loader)
        {
            string meshName = vars["mesh"];
            GameObject objectGao;

            if (loader.Meshes.ContainsKey(meshName)) {
                objectGao = Util.GameObjectFromScene(loader.Meshes[meshName], loader);
                objectGao.name = $"Object Mesh {meshName} Type {vars["type"]}";

                Util.SetLayerRecursively(objectGao, LayerMask.NameToLayer("Objects"));
            } else {
                objectGao = new GameObject(meshName+" (mesh not found)");
            }

            if (meshName=="coin.ase") {
                objectGao.AddComponent<CoinBehaviour>();
            }

            objectGao.transform.parent = gameObject.transform;
            objectGao.transform.position = Util.StringToVector3(vars["pos"], loader);

            float scale = float.Parse(vars["scale"]);
            objectGao.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void ReadLevelInfo(Dictionary<string, string> vars, Loader loader)
        {
            this.meshName = vars["mesh"];
            this.envName = vars["env"];
            this.materialName = vars["material"];
            
            if (vars.ContainsKey("music"))
            this.musicName = vars["music"];
        }
    }
}
