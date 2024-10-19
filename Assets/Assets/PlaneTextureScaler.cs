using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTextureScaler : MonoBehaviour
{
    private Renderer rend;
    private MaterialPropertyBlock mpb;
    // Define the size of the texture in world units
    public float textureSize = 1f; // Adjust this value as needed for your texture

    void Start()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Get the scale of the object
        Vector3 scale = transform.localScale;
        // Calculate tiling based on the scale
        // Planes generally use the x and z axes for scaling, not the y axis
        Vector2 tiling = new Vector2(scale.x / textureSize, scale.z / textureSize);

        // Retrieve the Material Property Block
        rend.GetPropertyBlock(mpb);

        // Sync tiling across all relevant textures if they exist
        SetTilingForTexture("_BaseMap", tiling);     // For Base Color Map (albedo)
        SetTilingForTexture("_MainTex", tiling);     // For Albedo (legacy naming)
        SetTilingForTexture("_BumpMap", tiling);     // For Normal Map
        SetTilingForTexture("_ParallaxMap", tiling); // For Height Map

        // Apply the changes to the material block
        rend.SetPropertyBlock(mpb);
    }

    // Function to set the tiling for a given texture if it exists
    void SetTilingForTexture(string textureName, Vector2 tiling)
    {
        if (rend.sharedMaterial.HasProperty(textureName))
        {
            mpb.SetVector(textureName + "_ST", new Vector4(tiling.x, tiling.y, 0, 0)); // Tiling and Offset
        }
    }
}
