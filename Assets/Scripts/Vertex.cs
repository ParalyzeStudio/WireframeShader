using UnityEngine;
using System.Collections.Generic;

public class Vertex
{
    public Vector3 m_position { get; set; } // location of this point
    public Vector3 m_mass { get; set; } //mass of this vertex in the barycentric coordinate system

    private int m_iID; // place of vertex in original list
    public int ID
    {
        get
        {
            return m_iID;
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

    public Vertex(Vector3 v, int _id)
    {
        m_position = v;
        m_iID = _id;
        m_mass = Vector3.zero;

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