using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WireframeMeshBuilder : MonoBehaviour
{
    private List<Vertex> m_vertices;
    private List<Triangle> m_triangles;

    //public void BuildSimple()
    //{
    //    Mesh originalMesh = this.GetComponent<MeshFilter>().sharedMesh;

    //    if (originalMesh == null)
    //        throw new System.NullReferenceException("A mesh has to be assigned to the MeshFilter component");

    //    Vector3[] vertices = originalMesh.vertices;
    //    int[] triangles = originalMesh.triangles;

    //    Vector3[] masses = new Vector3[3];
    //    masses[0] = new Vector3(1, 0, 0);
    //    masses[1] = new Vector3(0, 1, 0);
    //    masses[2] = new Vector3(0, 0, 1);

    //    Vector3[] newVertices = new Vector3[triangles.Length];
    //    Color[] colors = new Color[triangles.Length];
    //    for (int i = 0; i != triangles.Length; i++)
    //    {
    //        newVertices[i] = vertices[triangles[i]];
    //        triangles[i] = i;

    //        Vector3 mass = masses[i % 3];
    //        colors[i] = new Color(mass.x, mass.y, mass.z, 1);
    //    }

    //    Mesh newMesh = new Mesh();
    //    newMesh.name = originalMesh.name + "_WF";

    //    newMesh.vertices = newVertices;
    //    newMesh.triangles = triangles;
    //    newMesh.colors = colors;

    //    newMesh.RecalculateNormals();

    //    AssetDatabase.CreateAsset(newMesh, "Assets/" + newMesh.name + ".asset");
    //    AssetDatabase.SaveAssets();
    //}

    public void BuildAdvanced()
    {
        Mesh originalMesh = this.GetComponent<MeshFilter>().sharedMesh;
        if (originalMesh == null)
            throw new System.NullReferenceException("A mesh has to be assigned to the MeshFilter component");

        

        PrepareMeshData(originalMesh);
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
        Vector2[] uv = new Vector2[m_vertices.Count];
        for (int i = 0; i != m_vertices.Count; i++)
        {
            vertices[i] = m_vertices[i].m_position;
            Vector3 vertexMass = m_vertices[i].m_mass;
            Color mass = new Color(vertexMass.x, vertexMass.y, vertexMass.z, 1);
            colors[i] = mass;
            uv[i] = m_vertices[i].m_uv;
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
        newMesh.uv = uv;

        newMesh.RecalculateNormals();

        AssetDatabase.CreateAsset(newMesh, "Assets/" + newMesh.name + ".asset");
        AssetDatabase.SaveAssets();
    }

    /**
    * Transform the inital mesh data (verts and tris) to one which is more appropriate to our algorithm (with more info in it)
    * **/
    private void PrepareMeshData(Mesh originalMesh)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;
        Color[] originalColors = originalMesh.colors;
        Vector2[] originalUV = originalMesh.uv;

        m_vertices = new List<Vertex>(originalVertices.Length);
        for (int i = 0; i != originalVertices.Length; i++)
        {
            Vertex vertex = new Vertex(originalVertices[i], i);
            if (originalColors != null && originalColors.Length > 0)
                vertex.m_color = originalColors[i];
            if (originalUV != null && originalUV.Length > 0)
                vertex.m_uv = originalUV[i];
            m_vertices.Add(vertex);
        }

        m_triangles = new List<Triangle>(originalTriangles.Length);
        for (int i = 0; i != originalTriangles.Length; i += 3)
        {
            Vertex v0 = m_vertices[originalTriangles[i]];
            Vertex v1 = m_vertices[originalTriangles[i + 1]];
            Vertex v2 = m_vertices[originalTriangles[i + 2]];

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

