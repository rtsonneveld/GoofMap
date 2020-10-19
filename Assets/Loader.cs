using Assimp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using Material = UnityEngine.Material;

namespace GoofMap {
    public class Loader : MonoBehaviour {
        
        public string GameFolder;
        public Dictionary<string, Assimp.Scene> Meshes = new Dictionary<string, Assimp.Scene>();
        public Material DefaultMaterial;
        public Material TwoSidedMaterial;

        public Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
        public Dictionary<string, GameObject> LevelRoots = new Dictionary<string, GameObject>();
        public Dictionary<string, SpawnInfo> LevelSpawnInfo = new Dictionary<string, SpawnInfo>();

        public float ScaleFactor = 0.01f;

        public event Action OnLoad;

        public GameObject ErrorMessage;

        // Start is called before the first frame update
        public void Start()
        {
            Load();
        }

        public void Load()
        {
            string pkgFile = Path.Combine(GameFolder, "data0.pkg");

            if (!File.Exists(pkgFile)) {
                ErrorMessage.SetActive(true);
                return;
            } else {
                ErrorMessage.SetActive(false);
            }

            LoadPkg(pkgFile);

            OnLoad?.Invoke();

            SelectLevel(LevelRoots.Keys.First());
        }

        private void LoadPkg(string pkgFile)
        {
            ZipArchive archive = ZipFile.OpenRead(pkgFile);

            // Level Files
            var lvlEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".lvl"));

            // Meshes
            var badEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".bad"));
            var aseEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".ase"));
            
            // Material Definitions
            var madEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".mad"));

            // Textures
            var tgaEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".tga"));
            var bmpEntries = archive.Entries.Where(e => e.FullName.ToLower().EndsWith(".bmp"));

            AssimpContext importer = new AssimpContext();

            foreach (var l in tgaEntries) {

                using (var stream = l.Open()) {
                    Texture2D tex = TGALoader.LoadTGA(stream);
                    Textures[l.FullName.ToLower()] = tex;
                }
            }

            foreach (var l in bmpEntries) {

                using (var stream = l.Open()) {
                    Texture2D tex = BMPLoader.LoadBMP(stream);
                    Textures[l.FullName.ToLower()] = tex;
                }
            }

            foreach (var l in aseEntries) {

                using (var stream = l.Open()) {
                    Meshes.Add(l.FullName.ToLower(), importer.ImportFileFromStream(stream, PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices | PostProcessSteps.FlipWindingOrder));
                }
            }

            foreach (var l in badEntries) {

                using (var stream = l.Open()) {

                    string meshName = null;

                    using (var reader = new StreamReader(stream)) {

                        while (reader.Peek() >= 0) {
                            string line = reader.ReadLine();
                            if (line.StartsWith("mesh "))
                                meshName = line.Substring("mesh ".Length).ToLower();
                        }
                    }

                    if (meshName != null) {
                        if (Meshes.ContainsKey(meshName))
                            Meshes.Add(l.FullName.ToLower(), Meshes[meshName]);
                        else
                            Debug.LogError("Mesh " + meshName + " not found!");
                    }
                }
            }

            foreach (var l in lvlEntries) {

                using (var stream = l.Open()) {
                    using (var reader = new StreamReader(stream)) {
                        LvlFile level = new GameObject(l.Name.ToLower()).AddComponent<LvlFile>();
                        level.Read(reader, this, l.Name.ToLower());

                        LevelRoots.Add(l.Name.ToLower(), level.gameObject);
                    }
                }
            }
        }

        public void SelectLevel(string levelName)
        {
            foreach(GameObject g in LevelRoots.Values) {
                g.SetActive(false);
            }

            LevelRoots[levelName].SetActive(true);

            GameObject cameraGao = UnityEngine.Camera.main.gameObject;
            cameraGao.transform.position = LevelSpawnInfo[levelName].Position;
            cameraGao.transform.LookAt(LevelSpawnInfo[levelName].LookAt, Vector3.up);
        }

    }
}