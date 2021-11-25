using System.Collections.Generic;
using PathCreation;
using UnityEngine;

namespace PathCreation.Examples {
    public class RoadMeshCreator : PathSceneTool {
        [Header ("Road settings")]
        public float roadWidth = 20.0f;
        
        public float thickness = 5f;
        public bool flattenSurface;

        [Header ("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public float textureTiling = 1;

        [SerializeField, HideInInspector]
        GameObject meshHolder;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        Vector3 center;
        public Vector3[] CirclePoints;
        public Vector3[] RoadPoints;
        float radius;
        int numberOfPoints;
        private void Awake()
        {
             
             numberOfPoints = 20;
             RoadPoints = new Vector3[numberOfPoints];
             CirclePoints = new Vector3[numberOfPoints];
             center = new Vector3(0.0f, 0.0f, 0.0f);
             radius = 100;
             for (int i = 0; i < numberOfPoints; i++)
             {
                 float phi = i * 2 * Mathf.PI / numberOfPoints;
                 CirclePoints[i] = new Vector3(center.x + radius * Mathf.Cos(phi), 0.0f, center.z + radius * Mathf.Sin(phi));
                 
             }
            /*
           CirclePoints[0] = new Vector3(198.56f,0f,0f);
           CirclePoints[1] = new Vector3(193.57f,0f,61.47f);
           CirclePoints[2] = new Vector3(173.96f,0f,144.25f);
           CirclePoints[3] = new Vector3(133.04f,0f,226.77f);
           CirclePoints[4] = new Vector3(65.86f,0f,289.64f);
           CirclePoints[5] = new Vector3(-31.66f,0f,301.62f);
           CirclePoints[6] = new Vector3(-111.26f,0f,254.61f);
           CirclePoints[7] = new Vector3(-158f,0f,180.27f);
           CirclePoints[8] = new Vector3(-183.62f,0f,117.5f);
           CirclePoints[9] = new Vector3(-192.62f,0f,61.8f);
           CirclePoints[10] = new Vector3(-197.33f,0f,-0.87f);
           CirclePoints[11] = new Vector3(-195.23f,0f,-61.8f);
           CirclePoints[12] = new Vector3(-174.37f,0f,-142.6f);
           CirclePoints[13] = new Vector3(-149.55f, 0f, -199.24f);
           CirclePoints[14] = new Vector3(-84.08f,0f,-278.29f);
           CirclePoints[15] = new Vector3(-1.76f,0f,-302.72f);
           CirclePoints[16] = new Vector3(93.96f,0f,-270.86f);
           CirclePoints[17] = new Vector3(150.76f,0f,-193.48f);
           CirclePoints[18] = new Vector3(174.86f,0f,-133.76f);
           CirclePoints[19] = new Vector3(192.48f,0f,-73.5f);

           GeneratePath(CirclePoints, true); ;
           PathUpdated();
           */
            /*
            float t = 0.0f;
            float addition = 1.0f / numberOfPoints;
            
            for (int i = 0; i < numberOfPoints; i++)
            {
                CirclePoints[i] = CirclePoints[i] - pathCreator.path.GetNormal(t) * Random.Range(0.0f, Vector3.Distance(center, CirclePoints[i]) / 2);
                t += addition;
            }*/

            int layerMask = 1 << 7; 
            for(int i = 0; i < numberOfPoints; i++)
            {
                RaycastHit hit;
                //Debug.DrawRay(center, CirclePoints[i], Color.red,100);
                if (Physics.Raycast(center, CirclePoints[i], out hit, 1000.0f,layerMask))
                {
                    RoadPoints[i] =center + CirclePoints[i].normalized*Random.Range(hit.distance/2,hit.distance-30);
                } 
            }
            GeneratePath(RoadPoints, true);
            PathUpdated();
        }
        void Start()
        {

         
            GameObject car = GameObject.Find("Car02");
            car.transform.position = RoadPoints[0];
            car.transform.rotation = Quaternion.Euler(0,path.GetRotation(0.0f).y*180,0);
            
            GameObject startBoard = GameObject.Find("StartBoard");
            startBoard.transform.rotation = Quaternion.Euler(0,path.GetRotation(0.0f).y*180,0);
            Vector3 posStart= path.GetPointAtDistance(0.0f);
            posStart.y = -5;
            startBoard.transform.position = posStart;
            GameObject startLine = GameObject.Find("StartLine");
            startLine.transform.rotation = Quaternion.Euler(0, path.GetRotation(0.0f).y*180, 0);
            Vector3 pos = path.GetPointAtDistance(0.0f);
            pos.y += 0.1f;
            startLine.transform.position = pos;
        }

        VertexPath GeneratePath(Vector3[] points, bool closedPath)
        {
            //pathCreator = new PathCreator();
            // Create a closed, 2D bezier path from the supplied points array
            // These points are treated as anchors, which the path will pass through
            // The control points for the path will be generated automatically
            BezierPath bezierPath = new BezierPath(points, closedPath, PathSpace.xyz);
           
            pathCreator.GetComponent<PathCreator>().bezierPath = bezierPath;

            // Then create a vertex path from the bezier path, to be used for movement etc
            return new VertexPath(bezierPath, meshHolder.transform);


        }

        protected override void PathUpdated () {
            if (pathCreator != null) {
                AssignMeshComponents ();
                AssignMaterials ();
                

            }
        }

        Mesh CreateRoadMesh () {
            Vector3[] verts = new Vector3[path.NumPoints * 8];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
            int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };
            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);
            for (int i = 0; i < path.NumPoints; i++) {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross (path.GetTangent (i), path.GetNormal (i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal (i) : Vector3.Cross (localUp, path.GetTangent (i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint (i) - localRight * Mathf.Abs (roadWidth);
                Vector3 vertSideB = path.GetPoint (i) + localRight * Mathf.Abs (roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                uvs[vertIndex + 0] = new Vector2 (0, path.times[i]);
                uvs[vertIndex + 1] = new Vector2 (1, path.times[i]);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;
                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;
                // Sides of road normals
                normals[vertIndex + 4] = -localRight;
                normals[vertIndex + 5] = localRight;
                normals[vertIndex + 6] = -localRight;
                normals[vertIndex + 7] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop) {
                    for (int j = 0; j < triangleMap.Length; j++) {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++) {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                    }

                }

                vertIndex += 8;
                triIndex += 6;
            }
            
            mesh.Clear ();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.subMeshCount = 3;
            mesh.SetTriangles (roadTriangles, 0);
            mesh.SetTriangles (underRoadTriangles, 1);
            mesh.SetTriangles (sideOfRoadTriangles, 2);
            mesh.RecalculateBounds ();
            return mesh;
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        void AssignMeshComponents () {
            meshHolder = GameObject.Find("Road Mesh Holder");
            if (meshHolder != null)
            {
                DestroyImmediate(meshHolder);
            }
            if (meshHolder == null) {
                meshHolder = new GameObject ("Road Mesh Holder");
                meshHolder.layer = 6; //RoadMesh layer-hez tartozzon
            }

            meshHolder.transform.rotation = Quaternion.identity;
            meshHolder.transform.position = Vector3.zero;
            meshHolder.transform.localScale = Vector3.one;

            // Ensure mesh renderer and filter components are assigned
            if (!meshHolder.gameObject.GetComponent<MeshFilter> ()) {
                meshHolder.gameObject.AddComponent<MeshFilter> ();
            }
            if (!meshHolder.GetComponent<MeshRenderer> ()) {
                meshHolder.gameObject.AddComponent<MeshRenderer> ();
            }

            meshRenderer = meshHolder.GetComponent<MeshRenderer> ();
            meshFilter = meshHolder.GetComponent<MeshFilter> ();
            if (mesh == null) {
                mesh = new Mesh ();
            }
            meshFilter.sharedMesh = mesh;
            Rigidbody meshHolderRigidbody = meshHolder.AddComponent<Rigidbody>();
            meshHolderRigidbody.isKinematic = true;
            MeshCollider meshHolderCollider = meshHolder.AddComponent<MeshCollider>();
            meshHolderCollider.convex = false;
            meshHolderCollider.sharedMesh = CreateRoadMesh();
            meshHolderCollider.enabled = true;

        }

        void AssignMaterials () {
            if (roadMaterial != null && undersideMaterial != null) {
                meshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
                meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3 (1, textureTiling);
            }
        }

    }
}