using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WireframeMeshBuilder : MonoBehaviour
{
    private List<Vertex> m_vertices;
    private List<Triangle> m_triangles;

    public void BuildSimple()
    {
        Mesh originalMesh = this.GetComponent<MeshFilter>().sharedMesh;

        if (originalMesh == null)
            throw new System.NullReferenceException("A mesh has to be assigned to the MeshFilter component");

        Vector3[] vertices = originalMesh.vertices;
        int vertexCount = originalMesh.vertexCount;
        int[] triangles = originalMesh.triangles;

        Vector3[] masses = new Vector3[3];
        masses[0] = new Vector3(1, 0, 0);
        masses[1] = new Vector3(0, 1, 0);
        masses[2] = new Vector3(0, 0, 1);

        Vector3[] newVertices = new Vector3[triangles.Length];
        Color[] colors = new Color[triangles.Length];
        for (int i = 0; i != triangles.Length; i++)
        {
            newVertices[i] = vertices[triangles[i]];
            triangles[i] = i;

            Vector3 mass = masses[i % 3];
            colors[i] = new Color(mass.x, mass.y, mass.z, 1);
        }

        Mesh newMesh = new Mesh();
        newMesh.name = originalMesh.name + "_WF";

        newMesh.vertices = newVertices;
        newMesh.triangles = triangles;
        newMesh.colors = colors;

        AssetDatabase.CreateAsset(newMesh, "Assets/" + newMesh.name + ".asset");
        AssetDatabase.SaveAssets();
    }

    public void BuildAdvanced()
    {
        //Mesh originalMesh = new Mesh();
        //originalMesh.name = "DummyMesh";
        //Vector3[] originalVertices = new Vector3[11];
        //originalVertices[0] = new Vector3(0, 0, 0);
        //originalVertices[1] = new Vector3(4, 1.5f, 0);
        //originalVertices[2] = new Vector3(1, 3, 0);
        //originalVertices[3] = new Vector3(0, 5, 0);
        //originalVertices[4] = new Vector3(3, 4, 0);
        //originalVertices[5] = new Vector3(2, 6, 0);
        //originalVertices[6] = new Vector3(4, 7, 0);
        //originalVertices[7] = new Vector3(1.5f, 8, 0);
        //originalVertices[8] = new Vector3(5, -1, 0);
        //originalVertices[9] = new Vector3(7, 3, 0);
        //originalVertices[10] = new Vector3(6, 5, 0);



        //int[] originalTriangles = new int[33];
        //originalTriangles[0] = 0;
        //originalTriangles[1] = 1;
        //originalTriangles[2] = 2;
        //originalTriangles[3] = 0;
        //originalTriangles[4] = 8;
        //originalTriangles[5] = 1;
        //originalTriangles[6] = 1;
        //originalTriangles[7] = 8;
        //originalTriangles[8] = 10;
        //originalTriangles[9] = 8;
        //originalTriangles[10] = 9;
        //originalTriangles[11] = 10;
        //originalTriangles[12] = 1;
        //originalTriangles[13] = 10;
        //originalTriangles[14] = 4;
        //originalTriangles[15] = 2;
        //originalTriangles[16] = 1;
        //originalTriangles[17] = 4;
        //originalTriangles[18] = 3;
        //originalTriangles[19] = 2;
        //originalTriangles[20] = 4;
        //originalTriangles[21] = 3;
        //originalTriangles[22] = 4;
        //originalTriangles[23] = 5;
        //originalTriangles[24] = 5;
        //originalTriangles[25] = 4;
        //originalTriangles[26] = 6;
        //originalTriangles[27] = 3;
        //originalTriangles[28] = 5;
        //originalTriangles[29] = 7;
        //originalTriangles[30] = 5;
        //originalTriangles[31] = 6;
        //originalTriangles[32] = 7;


        Mesh originalMesh = this.GetComponent<MeshFilter>().sharedMesh;
        if (originalMesh == null)
            throw new System.NullReferenceException("A mesh has to be assigned to the MeshFilter component");

        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        PrepareMeshData(new List<Vector3>(originalVertices), new List<int>(originalTriangles));
        AssignMassesToMesh();

        //Extract triangles that have duplicate masses on their vertices
        for (int i = 0; i != m_triangles.Count; i++)
        {
            Triangle triangle = m_triangles[i];
            if (triangle.HasDuplicateNonZeroMasses())
            {
                //Add 3 vertices to the vertex list and modify the indices for this triangle accordingly
                triangle.RebuildAtIndex(m_vertices.Count);
                m_vertices.AddRange(new List<Vertex>(triangle.Vertices));
            }
        }

        //rebuild the mesh vertices and triangles arrays and put the barycentric masses inside the colors array        
        Vector3[] vertices = new Vector3[m_vertices.Count];
        Color[] colors = new Color[m_vertices.Count];
        for (int i = 0; i != m_vertices.Count; i++)
        {
            vertices[i] = m_vertices[i].m_position;
            Vector3 vertexMass = m_vertices[i].m_mass;
            Color mass = new Color(vertexMass.x, vertexMass.y, vertexMass.z, 1);
            colors[i] = mass;
        }

        int[] triangles = new int[3 * m_triangles.Count];
        for (int i = 0; i != m_triangles.Count; i++)
        {
            Triangle triangle = m_triangles[i];
            triangles[3 * i] = triangle.Vertices[0].ID;
            triangles[3 * i + 1] = triangle.Vertices[1].ID;
            triangles[3 * i + 2] = triangle.Vertices[2].ID;
        }

        Mesh newMesh = new Mesh();
        newMesh.name = originalMesh.name + "_WF";

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.colors = colors;

        AssetDatabase.CreateAsset(newMesh, "Assets/" + newMesh.name + ".asset");
        AssetDatabase.SaveAssets();
    }

    /**
    * Transform the inital mesh data (verts and tris) to one which is more appropriate to our algorithm (with more info in it)
    * **/
    private void PrepareMeshData(List<Vector3> verts, List<int> tris)
    {
        m_vertices = new List<Vertex>(verts.Count);
        for (int i = 0; i != verts.Count; i++)
        {
            m_vertices.Add(new Vertex(verts[i], i));
        }

        m_triangles = new List<Triangle>(tris.Count);
        for (int i = 0; i != tris.Count; i += 3)
        {
            Vertex v0 = m_vertices[tris[i]];
            Vertex v1 = m_vertices[tris[i + 1]];
            Vertex v2 = m_vertices[tris[i + 2]];

            Triangle triangle = new Triangle(v0, v1, v2);
            m_triangles.Add(triangle);

            //Set this triangle as an adjacent triangle for every point
            v0.AddAdjacentTriangle(triangle);
            v1.AddAdjacentTriangle(triangle);
            v2.AddAdjacentTriangle(triangle);

            //for each triangle vertex, set the 2 opposite points as neighbors
            v0.AddNeighbor(v1);
            v0.AddNeighbor(v2);
            v1.AddNeighbor(v0);
            v1.AddNeighbor(v2);
            v2.AddNeighbor(v0);
            v2.AddNeighbor(v1);
        }

        //now populate the adjacent triangles list for every triangle
        for (int i = 0; i != m_triangles.Count; i++)
        {
            m_triangles[i].FindAdjacentTriangles();
        }
    }

    /**
     * Assign to each vertex a Vector3 that can take the value of (1,0,0) or (0,1,0) or (0,0,1).
     * Those values correspond to the triangle vertices masses inside a barycentric coordinate system.
     * Order does not matter as long as the triangle does not contain any duplicate masses.
     * **/
    public void AssignMassesToMesh()
    {
        if (m_vertices.Count == 0)
            return;

        //Assign masses to every triangle vertices
        Triangle triangle = FindTriangleWithNoMassAssigned();
        while (triangle != null)
        {
            AssignMassesToTriangle(triangle);
            triangle = FindTriangleWithNoMassAssigned(); //try to find another triangle where masses have not been assigned
        }        
    }

    private Triangle FindTriangleWithNoMassAssigned()
    {
        for (int i = 0; i != m_triangles.Count; i++)
        {
            if (!m_triangles[i].m_massesAssigned)
                return m_triangles[i];
        }

        return null;
    }

    private void AssignMassesToTriangle(Triangle triangle)
    {
        triangle.AssignMasses();

        //recursively call this method on neighbouring triangles
        for (int i = 0; i != triangle.AdjacentTriangles.Count; i++)
        {
            Triangle adjacentTriangle = triangle.AdjacentTriangles[i];
            if (!adjacentTriangle.m_massesAssigned)
                AssignMassesToTriangle(triangle.AdjacentTriangles[i]);
        }
    }
}

