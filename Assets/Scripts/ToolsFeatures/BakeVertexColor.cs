using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeVertexColor : MonoBehaviour
{
    [InstanceButton(typeof(BakeVertexColor), nameof(BakeColors), "Bake Vertex Colors", ButtonActivityType.OnEditMode)]
    public Texture2D mColorTexture;

    private Mesh newMesh;


    public void BakeColors()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        if(mColorTexture == null || mf == null)
        {
            Debug.LogError("Duh...");
            return;
        }
        else if (!mColorTexture.isReadable || !mf.sharedMesh.isReadable)
        {
            Debug.LogError("Set texture and mesh to read/write enabled!");
            return;
        }
        if (newMesh != null) DestroyImmediate(newMesh);
        newMesh = Instantiate(mf.sharedMesh);
        Color[] colorMesh = new Color[newMesh.vertices.Length];
        for(int i = 0; i < newMesh.vertices.Length; i++)
        {
            Vector2 pos = newMesh.uv[i];
            Color color = mColorTexture.GetPixelBilinear(pos.x, pos.y);
            colorMesh[i] = color;
        }

        newMesh.colors = colorMesh;
        mf.mesh = newMesh;
        Debug.Log("Successful baking!");
    }


}
