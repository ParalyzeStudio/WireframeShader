using UnityEngine;
using System.Collections.Generic;


public class Triangle
{
    private Vertex[] m_vertices;// the 3 points that make this tri
    public Vertex[] Vertices
    {
        get
        {
            return m_vertices;
        }
    }

    private List<Triangle> m_adjacentTriangles; // adjacent triangles
    public List<Triangle> AdjacentTriangles
    {
        get
        {
            return m_adjacentTriangles;
        }
    }

    public bool m_massesAssigned { get; set; }

    public Triangle(Vertex v0, Vertex v1, Vertex v2)
    {
        m_vertices = new Vertex[3];
        m_vertices[0] = v0;
        m_vertices[1] = v1;
        m_vertices[2] = v2;

        m_adjacentTriangles = new List<Triangle>();
        m_massesAssigned = false;
    }   

    public bool HasVertex(Vertex v)
    {
        return v == m_vertices[0] || v == m_vertices[1] || v == m_vertices[2];
    }

    /**
     * Find adjacent triangles by cross-checking triangles vertices neighbors
     * **/
    public void FindAdjacentTriangles()
    {
        Dictionary<Triangle, int> adjacentTrianglesToVertices = new Dictionary<Triangle, int>();

        for (int i = 0; i != 3; i++)
        {
            Vertex vertex = m_vertices[i];
            for (int j = 0; j != vertex.AdjacentTriangles.Count; j++)
            {
                Triangle vertexAdjacentTriangle = vertex.AdjacentTriangles[j];
                if (m_adjacentTriangles.Contains(vertexAdjacentTriangle))
                    continue;

                int count = 0;
                if (adjacentTrianglesToVertices.TryGetValue(vertexAdjacentTriangle, out count))
                {
                    count++;
                    adjacentTrianglesToVertices[vertexAdjacentTriangle] = count;
                }
                else
                {
                    adjacentTrianglesToVertices.Add(vertexAdjacentTriangle, 1);
                }
            }
        }

        //extract triangles that have a count of 2 i.e 2 vertices (an edge) share it
        foreach (KeyValuePair<Triangle, int> kvp in adjacentTrianglesToVertices)
        {
            if (kvp.Value == 2)
            {
                m_adjacentTriangles.Add(kvp.Key);
                kvp.Key.m_adjacentTriangles.Add(this);
            }
        }
    }

    /**
     * Assign a mass to each vertex if it has not been assigned yet
     * **/
    public void AssignMasses()
    {
        //sum the masses for each vertex
        Vector3 sum = Vector3.zero;
        for (int i = 0; i != 3; i++)
        {
            sum += m_vertices[i].Properties.m_mass;
        }

        for (int i = 0; i != 3; i++)
        {
            Vertex vertex = m_vertices[i];
            if (vertex.Properties.m_mass == Vector3.zero)
            {
                //Find the first available mass and assign it to this vertex
                if (sum.x == 0)
                {
                    vertex.Properties.m_mass = new Vector3(1, 0, 0);
                    sum.x = 1;
                }
                else if (sum.y == 0)
                {
                    vertex.Properties.m_mass = new Vector3(0, 1, 0);
                    sum.y = 1;
                }
                else //there cannot be any other case than (sum.z == 0)
                {
                    vertex.Properties.m_mass = new Vector3(0, 0, 1);
                    sum.z = 1;
                }
            }
        }

        m_massesAssigned = true;
    }

    /**
     * Tell if two vertices have the same non-zero mass
     * **/
    public bool HasDuplicateNonZeroMasses()
    {
        for (int i = 0; i != 3; i++)
        {
            if (m_vertices[i].Properties.m_mass == Vector3.zero)
                return false;
        }

        if (m_vertices[0].Properties.m_mass == m_vertices[1].Properties.m_mass ||
            m_vertices[0].Properties.m_mass == m_vertices[2].Properties.m_mass ||
            m_vertices[1].Properties.m_mass == m_vertices[2].Properties.m_mass)
            return true;

        return false;
    }

    /**
     * Create new vertices for this triangle and assign a mass for each
     * **/
    public void RebuildAtIndex(int index)
    {
        Vertex.VertexProperties vertex1Properties = new Vertex.VertexProperties(m_vertices[0].Properties);
        Vertex.VertexProperties vertex2Properties = new Vertex.VertexProperties(m_vertices[1].Properties);
        Vertex.VertexProperties vertex3Properties = new Vertex.VertexProperties(m_vertices[2].Properties);
        vertex1Properties.m_mass = new Vector3(1, 0, 0);
        vertex2Properties.m_mass = new Vector3(0, 1, 0);
        vertex3Properties.m_mass = new Vector3(0, 0, 1);

        m_vertices[0] = new Vertex(index, vertex1Properties);
        m_vertices[1] = new Vertex(index + 1, vertex2Properties);
        m_vertices[2] = new Vertex(index + 2, vertex3Properties);

        //m_vertices[0] = new Vertex(m_vertices[0].m_position, index, m_vertices[0].m_color, m_vertices[0].m_uv);
        //m_vertices[1] = new Vertex(m_vertices[1].m_position, index + 1, m_vertices[1].m_color, m_vertices[1].m_uv);
        //m_vertices[2] = new Vertex(m_vertices[2].m_position, index + 2, m_vertices[2].m_color, m_vertices[2].m_uv);

        //m_vertices[0].m_mass = new Vector3(1, 0, 0);
        //m_vertices[1].m_mass = new Vector3(0, 1, 0);
        //m_vertices[2].m_mass = new Vector3(0, 0, 1);
    }
}
