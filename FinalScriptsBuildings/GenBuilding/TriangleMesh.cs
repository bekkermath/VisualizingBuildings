using UnityEngine;

public class TriangleMesh : MonoBehaviour
{
    public Transform[] vertTransforms;
    public Color color;
    public Chamber chamber;

    public void Create(Transform[] vertTransforms)
    {
        this.vertTransforms = new Transform[vertTransforms.Length];
        this.vertTransforms = vertTransforms;

        Vector3[] vertices;
        vertices = new Vector3[vertTransforms.Length];
        for(int i = 0; i < vertTransforms.Length; i++)
        {
            vertices[i] = this.vertTransforms[i].localPosition;
        }
        int[] triangles;
        triangles = new int[] { 0, 1, 2, 2, 1, 0 };

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        MeshRenderer triangleRenderer = GetComponent<MeshRenderer>();
        triangleRenderer.material.SetColor("_Color", this.color);

        chamber = this.GetComponent<Chamber>();
    }

    public void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices;
        vertices = new Vector3[this.vertTransforms.Length];
        for (int i = 0; i < this.vertTransforms.Length; i++)
        {
            vertices[i] = this.vertTransforms[i].localPosition;
        }

        int[] triangles;
        triangles = new int[] { 0, 1, 2, 2, 1, 0 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}