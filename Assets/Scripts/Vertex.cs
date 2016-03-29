using UnityEngine;
using System.Collections.Generic;

public class Vertex
{
    public class VertexProperties
    {
        public Vector3 m_position { get; set; } // location of this point
        public Vector3 m_mass { get; set; } //mass of this vertex in the barycentric coordinate system
        public Color m_color { get; set; } //color at this vertex
        public Vector2 m_uv { get; set; } //uv coordinate at this vertex
        public Vector3 m_normal { get; set; } //normal at this vertex
        public Vector4 m_tangent { get; set; } //tangent at this vertex

        public VertexProperties(Vector3 position)
        {
            m_position = position;
            m_mass = Vector3.zero;
            m_color = Color.black;
            m_uv = Vector2.zero;
            m_normal = Vector3.zero;
            m_tangent = Vector4.zero;
        }

        public VertexProperties(Vector3 position, Color color, Vector2 uv, Vector3 normal, Vector4 tangent) : this(position)
        {
            m_color = color;
            m_uv = uv;
            m_normal = normal;
            m_tangent = tangent;
        }

        public VertexProperties(Vector3 position, Color color, Vector2 uv, Vector3 normal, Vector4 tangent, Vector3 mass) : this(position, color, uv, normal, tangent)
        {            
            m_mass = mass;
        }

        public VertexProperties(VertexProperties other) : this(other.m_position, other.m_color, other.m_uv, other.m_normal, other.m_tangent, other.m_mass)
        {

        }
    }

    private int m_iID; // place of vertex in original list
    public int ID
    {
        get
        {
            return m_iID;
        }
    }

    private VertexProperties m_properties;
    public VertexProperties Properties
    {
        get
        {
            return m_properties;
        }
    }

    private List<Vertex> m_neighbors; // adjacent vertices
    public List<Vertex> Neighbors
    {
        get
        {
            return m_neighbors;
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

    public Vertex(int id, VertexProperties properties)
    {
        m_iID = id;
        m_properties = properties;

        m_neighbors = new List<Vertex>(3);
        m_adjacentTriangles = new List<Triangle>(3);
    }

    public void AddAdjacentTriangle(Triangle triangle)
    {
        m_adjacentTriangles.Add(triangle);
    }

    public bool HasAdjacentTriangle(Triangle triangle)
    {
        for (int i = 0; i != m_adjacentTriangles.Count; i++)
        {
            if (triangle == m_adjacentTriangles[i])
                return true;
        }

        return false;
    }

    public void AddNeighbor(Vertex neighbor)
    {
        if (!HasNeighbor(neighbor))
            m_neighbors.Add(neighbor);
    }

    public bool HasNeighbor(Vertex neighbor)
    {
        for (int i = 0; i != m_neighbors.Count; i++)
        {
            if (neighbor == m_neighbors[i])
                return true;
        }

        return false;
    }
}