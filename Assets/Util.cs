
using Assets.Behaviour;
using Assimp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace GoofMap {
    public static class Util {
        public static List<string> InvisibleMaterials = new List<string>()
        {
            "lvl11_invis.tga",
            "invisible.tga",
            "jump_guide",
            "boundary_collide",
            "invisible_collide",
            "invisible",
            "air_barrier",
            "exit_00",
            "exit_01",
            "Inv_collide_dbsided"
        };

        public static Vector3 StringToVector3(string s, Loader loader, bool applyScale = true)
        {
            var split = s.Split(',');

            float x = float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat);
            float z = float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat);

            if (applyScale) {
                return new Vector3(x * loader.ScaleFactor, y * loader.ScaleFactor, z * loader.ScaleFactor);
            } else {
                return new Vector3(x, y, z);
            }
        }

        public static GameObject GameObjectFromScene(Scene scene, Loader loader)
        {
            GameObject sceneGao = new GameObject("Scene");
            List<UnityEngine.Material> materials = GenerateMaterialsFromScene(scene, loader);

            foreach (Assimp.Mesh m in scene.Meshes) {

                GameObject meshGao = new GameObject("Mesh");
                meshGao.transform.parent = sceneGao.transform;

                MeshFilter mf = meshGao.AddComponent<MeshFilter>();
                mf.mesh = GenerateMesh(m, loader);

                MeshRenderer mr = meshGao.AddComponent<MeshRenderer>();
                mr.material = materials[m.MaterialIndex];

                MeshCollider mc = meshGao.AddComponent<MeshCollider>();

                string materialName = mr.material.name;
                foreach (string t in materialName.Split(';')) {
                    if (InvisibleMaterials.Contains(t)) {
                        meshGao.layer = LayerMask.NameToLayer("InvisibleGeometry");
                    }
                }

                meshGao.name = meshGao.name+" (Material "+materialName+")";
            }

            return sceneGao;
        }

        private static UnityEngine.Mesh GenerateMesh(Assimp.Mesh m, Loader loader)
        {

                List<Vector3D> verts = m.Vertices;
                List<Vector3D> norms = (m.HasNormals) ? m.Normals : null;
                List<Vector3D> uvs = m.HasTextureCoords(0) ? m.TextureCoordinateChannels[0] : null;

                UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();
                unityMesh.vertices = verts.Select(v => new Vector3(v.X * loader.ScaleFactor, v.Y * loader.ScaleFactor, -v.Z * loader.ScaleFactor)).ToArray();
                unityMesh.normals = norms.Select(v => new Vector3(v.X, v.Y, -v.Z)).ToArray();
                unityMesh.uv = uvs != null ? uvs.Select(v => new Vector2(v.X, -v.Y)).ToArray() : null; // Flip uv's over y axis

                List<int> triangles = new List<int>();

                foreach (Face f in m.Faces) {
                    if (f.IndexCount == 3) {
                        triangles.AddRange(f.Indices);
                    } else {
                        Debug.LogError($"IndexCount {f.IndexCount} is not 3");
                    }
                }

                unityMesh.triangles = triangles.ToArray();
                return unityMesh;
        }

        private static List<UnityEngine.Material> GenerateMaterialsFromScene(Scene scene, Loader loader)
        {
            List<UnityEngine.Material> materials = new List<UnityEngine.Material>();

            foreach (Assimp.Material mat in scene.Materials) {

                UnityEngine.Material um;

                if (mat.HasTwoSided) {
                    um = new UnityEngine.Material(loader.TwoSidedMaterial);
                } else {
                    um = new UnityEngine.Material(loader.DefaultMaterial);
                }

                string path = mat.TextureDiffuse.FilePath;
                if (path != null && path.LastIndexOf("\\") != -1) {

                    string filename = path.Substring(path.LastIndexOf("\\") + 1).ToLower();

                    um.name = mat.Name+";"+filename+";";

                    if (loader.Textures.ContainsKey(filename)) {

                        um.mainTexture = loader.Textures[filename];
                    } else {
                        um.name += ";(texture not found);";
                    }

                } else {

                    um.name = $"No Texture;{mat.Name};";
                }

                // TODO: proper color/alpha from .mad files
                //um.color = AssimpToUnityColor(mat.ColorDiffuse);

                materials.Add(um);

            }

            return materials;
        }


        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform) {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static Color AssimpToUnityColor(Color4D col)
        {
            return new Color(col.R, col.G, col.B, col.A);
        }

        public static Bounds GetBoundsIncludingChildren(GameObject gao)
        {
            var bounds = new Bounds(gao.transform.position, Vector3.one);
            GoofObject[] goofObjects = gao.GetComponentsInChildren<GoofObject>();
            foreach (GoofObject obj in goofObjects) {
                bounds.Encapsulate(obj.bounds);
            }

            return bounds;
        }
    }
}
