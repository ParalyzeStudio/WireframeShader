using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WireframeMeshBuilder))]
public class WireframeMeshBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WireframeMeshBuilder meshBuilder = (WireframeMeshBuilder)target;
        if (GUILayout.Button("Transform Mesh SIMPLE"))
        {
            meshBuilder.BuildSimple();
        }
        if (GUILayout.Button("Transform Mesh"))
        {
            meshBuilder.BuildAdvanced();
        }
    }
}
