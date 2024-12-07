using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ColorStudio {

    public static class Voxelizer {

        struct Cuboid {
            public Vector3 min, max;
            public Color32 color;
            public bool deleted;
        }

        struct Face {
            public Vector3 center;
            public Vector3 size;
            public Vector3[] vertices;
            public Vector3[] normals;
            public Color32 color;

            public Face(Vector3 center, Vector3 size, Vector3[] vertices, Vector3[] normals, Color32 color) {
                this.center = center;
                this.size = size;
                this.vertices = vertices;
                this.normals = normals;
                this.color = color;
            }


            public static bool operator ==(Face f1, Face f2) {
                return f1.size == f2.size && f1.center == f2.center;
            }

            public static bool operator !=(Face f1, Face f2) {
                return f1.size != f2.size || f1.center != f2.center;
            }

            public override bool Equals(object obj) {
                if (obj == null || !(obj is Face))
                    return false;
                Face other = (Face)obj;
                return size == other.size && center == other.center;
            }

            public override int GetHashCode() {
                unchecked {
                    int hash = 23;
                    hash = hash * 31 + center.GetHashCode();
                    hash = hash * 31 + size.GetHashCode();
                    return hash;
                }
            }
        }


        static readonly List<Vector3> vertices = new List<Vector3>();
        static readonly List<int> indices = new List<int>();
        static readonly List<Vector3> uvs = new List<Vector3>();
        static readonly List<Vector3> normals = new List<Vector3>();
        static readonly List<Color32> meshColors = new List<Color32>();
        static Cuboid[] cuboids = new Cuboid[128];
        static readonly Vector3[] faceUVs =  {
            new Vector3 (0, 0, 0), new Vector3 (0, 1, 0), new Vector3 (1, 0, 0), new Vector3 (1, 1, 0)
        };

        static void Encapsulate(int k, Vector3 pointMin, Vector3 pointMax) {
            if (pointMin.x < cuboids[k].min.x) cuboids[k].min.x = pointMin.x;
            else if (pointMax.x > cuboids[k].max.x) cuboids[k].max.x = pointMax.x;
            if (pointMin.y < cuboids[k].min.y) cuboids[k].min.y = pointMin.y;
            else if (pointMax.y > cuboids[k].max.y) cuboids[k].max.y = pointMax.y;
            if (pointMin.z < cuboids[k].min.z) cuboids[k].min.z = pointMin.z;
            else if (pointMax.z > cuboids[k].max.z) cuboids[k].max.z = pointMax.z;
        }


        public static GameObject GeneratePrefab(string path, Color32[] colors, int sizeX, int sizeY) {

            // Generate a cuboid per visible voxel
            GameObject obj = GenerateVoxelObject(colors, sizeX, sizeY, sizeZ: 1, offset: Vector3.zero, scale: Vector3.one / 16f, skipTransparentColors: true, 128);
            if (obj == null) return null;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);

            // Store the mesh inside the prefab
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            AssetDatabase.AddObjectToAsset(mesh, prefab);
            prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
            Material mat = obj.GetComponent<MeshRenderer>().sharedMaterial;
            AssetDatabase.AddObjectToAsset(mat, prefab);
            prefab.GetComponent<MeshRenderer>().sharedMaterial = mat;
            AssetDatabase.SaveAssets();
            Object.DestroyImmediate(obj);

            return prefab;
        }



        public static GameObject GenerateVoxelObject(Color32[] colors, int sizeX, int sizeY, int sizeZ, Vector3 offset, Vector3 scale, bool skipTransparentColors = true, int alphaCutoutThreshold = 128) {

            int index;
            int ONE_Y_ROW = sizeZ * sizeX;
            int ONE_Z_ROW = sizeX;

            Cuboid cuboid = new Cuboid();
            int cuboidsCount = 0;
            for (int y = 0; y < sizeY; y++) {
                int posy = y * ONE_Y_ROW;
                for (int z = 0; z < sizeZ; z++) {
                    int posz = z * ONE_Z_ROW;
                    for (int x = 0; x < sizeX; x++) {
                        index = posy + posz + x;
                        Color32 color = colors[index];
                        if (!skipTransparentColors || color.a >= alphaCutoutThreshold) {
                            cuboid.min.x = x - sizeX / 2f;
                            cuboid.min.y = y - sizeY / 2f;
                            cuboid.min.z = z - sizeZ / 2f;
                            cuboid.max.x = cuboid.min.x + 1;
                            cuboid.max.y = cuboid.min.y + 1;
                            cuboid.max.z = cuboid.min.z + 1;
                            cuboid.color = color;
                            if (cuboidsCount >= cuboids.Length) {
                                Cuboid[] newCuboids = new Cuboid[cuboidsCount * 2];
                                System.Array.Copy(cuboids, newCuboids, cuboids.Length);
                                cuboids = newCuboids;
                            }
                            cuboids[cuboidsCount++] = cuboid;
                        }
                    }
                }
            }

            // Optimization 1: Fusion same color cuboids
            bool repeat = true;
            while (repeat) {
                repeat = false;
                for (int k = 0; k < cuboidsCount; k++) {
                    if (cuboids[k].deleted)
                        continue;
                    Vector3 f1min = cuboids[k].min;
                    Vector3 f1max = cuboids[k].max;
                    for (int j = k + 1; j < cuboidsCount; j++) {
                        if (cuboids[j].deleted)
                            continue;
                        if (cuboids[k].color.r == cuboids[j].color.r && cuboids[k].color.g == cuboids[j].color.g && cuboids[k].color.b == cuboids[j].color.b) {
                            bool touching = false;
                            Vector3 f2min = cuboids[j].min;
                            Vector3 f2max = cuboids[j].max;
                            // Touching back or forward faces?
                            if (f1min.x == f2min.x && f1max.x == f2max.x && f1min.y == f2min.y && f1max.y == f2max.y) {
                                touching = f1min.z == f2max.z || f1max.z == f2min.z;
                                // ... left or right faces?
                            } else if (f1min.z == f2min.z && f1max.z == f2max.z && f1min.y == f2min.y && f1max.y == f2max.y) {
                                touching = f1min.x == f2max.x || f1max.x == f2min.x;
                                // ... top or bottom faces?
                            } else if (f1min.x == f2min.x && f1max.x == f2max.x && f1min.z == f2min.z && f1max.z == f2max.z) {
                                touching = f1min.y == f2max.y || f1max.y == f2min.y;
                            }
                            if (touching) {
                                Encapsulate(k, cuboids[j].min, cuboids[j].max);
                                //cuboids [k].bounds.Encapsulate (f2);
                                f1min = cuboids[k].min;
                                f1max = cuboids[k].max;

                                cuboids[j].deleted = true;
                                repeat = true;
                            }
                        }
                    }
                }
            }

            // Optimization 2: Remove hidden cuboids
            for (int k = 0; k < cuboidsCount; k++) {
                if (cuboids[k].deleted)
                    continue;
                Vector3 f1min = cuboids[k].min;
                Vector3 f1max = cuboids[k].max;
                int occlusion = 0;
                for (int j = k + 1; j < cuboidsCount; j++) {
                    if (cuboids[j].deleted)
                        continue;
                    Vector3 f2min = cuboids[j].min;
                    Vector3 f2max = cuboids[j].max;
                    // Touching back or forward faces?
                    if (f1min.x >= f2min.x && f1max.x <= f2max.x && f1min.y >= f2min.y && f1max.y <= f2max.y) {
                        if (f1min.z == f2max.z)
                            occlusion++;
                        if (f1max.z == f2min.z)
                            occlusion++;
                        // ... left or right faces?
                    } else if (f1min.z >= f2min.z && f1max.z <= f2max.z && f1min.y >= f2min.y && f1max.y <= f2max.y) {
                        if (f1min.x == f2max.x)
                            occlusion++;
                        if (f1max.x == f2min.x)
                            occlusion++;
                        // ... top or bottom faces?
                    } else if (f1min.x >= f2min.x && f1max.x <= f2max.x && f1min.z >= f2min.z && f1max.z <= f2max.z) {
                        if (f1min.y == f2max.y)
                            occlusion++;
                        if (f1max.y == f2min.y)
                            occlusion++;
                    }
                    if (occlusion == 6) {
                        cuboids[k].deleted = true;
                        break;
                    }
                }
            }

            // Optimization 3: Fragment cuboids into faces and remove duplicates
            HashSet<Face> faces = new HashSet<Face>();
            for (int k = 0; k < cuboidsCount; k++) {
                if (cuboids[k].deleted)
                    continue;
                Vector3 min = cuboids[k].min;
                Vector3 max = cuboids[k].max;
                Vector3 size = max - min;
                Face top = new Face(new Vector3((min.x + max.x) * 0.5f, max.y, (min.z + max.z) * 0.5f), new Vector3(size.x, 0, size.z), Cube.faceVerticesTop, Cube.normalsUp, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, top);
                Face bottom = new Face(new Vector3((min.x + max.x) * 0.5f, min.y, (min.z + max.z) * 0.5f), new Vector3(size.x, 0, size.z), Cube.faceVerticesBottom, Cube.normalsDown, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, bottom);
                Face left = new Face(new Vector3(min.x, (min.y + max.y) * 0.5f, (min.z + max.z) * 0.5f), new Vector3(0, size.y, size.z), Cube.faceVerticesLeft, Cube.normalsLeft, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, left);
                Face right = new Face(new Vector3(max.x, (min.y + max.y) * 0.5f, (min.z + max.z) * 0.5f), new Vector3(0, size.y, size.z), Cube.faceVerticesRight, Cube.normalsRight, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, right);
                Face back = new Face(new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, min.z), new Vector3(size.x, size.y, 0), Cube.faceVerticesBack, Cube.normalsBack, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, back);
                Face forward = new Face(new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, max.z), new Vector3(size.x, size.y, 0), Cube.faceVerticesForward, Cube.normalsForward, cuboids[k].color);
                RemoveDuplicateOrAddFace(faces, forward);
            }

            // Create geometry & uv mapping
            vertices.Clear();
            uvs.Clear();
            indices.Clear();
            normals.Clear();
            meshColors.Clear();
            index = 0;
            foreach (Face face in faces) {
                Vector3 faceVertex;
                for (int j = 0; j < 4; j++) {
                    faceVertex.x = (face.center.x + face.vertices[j].x * face.size.x) * scale.x + offset.x;
                    faceVertex.y = (face.center.y + face.vertices[j].y * face.size.y) * scale.y + offset.y;
                    faceVertex.z = (face.center.z + face.vertices[j].z * face.size.z) * scale.z + offset.z;
                    vertices.Add(faceVertex);
                    meshColors.Add(face.color);
                    uvs.Add(faceUVs[j]);
                }
                normals.AddRange(face.normals);
                indices.Add(index);
                indices.Add(index + 1);
                indices.Add(index + 2);
                indices.Add(index + 3);
                indices.Add(index + 2);
                indices.Add(index + 1);

                index += 4;
            }

            if (vertices.Count == 0) return null;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "Object";

            Mesh mesh = new Mesh();
            if (!Application.isMobilePlatform) {
                // support for very big models on desktop only - mobile support is not guaranteed! To fully support mobile, including MALI-400 GPUs,
                // we should partition the mesh into several submeshes of up to 65535 vertices each
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);
            mesh.SetColors(meshColors);

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            mf.mesh = mesh;

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.sharedMaterial = Object.Instantiate(mr.sharedMaterial);
            mr.sharedMaterial.shader = Shader.Find("Color Studio/UnlitVertexColor");

            Collider collider = obj.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            EditorUtility.SetDirty(obj);

            return obj;
        }

        static void RemoveDuplicateOrAddFace(HashSet<Face> faces, Face face) {
            if (faces.Contains(face)) {
                faces.Remove(face);
            } else {
                faces.Add(face);
            }
        }
    }

}
