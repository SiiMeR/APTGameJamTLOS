using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShroomEffect : MonoBehaviour
{
    public Shader shroomEffectShader;

    private Material _shroomEffectMaterial;
    
    public Material ShroomEffectMaterial
    {
        get
        {
            if (!_shroomEffectMaterial && shroomEffectShader)
            {
                _shroomEffectMaterial = new Material(shroomEffectShader) { hideFlags = HideFlags.HideAndDontSave };
            }

            return _shroomEffectMaterial;
        }
    }

    public static bool _shroomEffectActive;

    public static void ToggleShroomEffect()
    {
        _shroomEffectActive = !_shroomEffectActive;
    }
    
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        
        if (!ShroomEffectMaterial || !_shroomEffectActive)
        {
            Debug.LogWarning("Something wrong with shroom post process");
            Graphics.Blit(src,dest);
            return;
        }

        var renderTexture = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default);

        renderTexture.filterMode = FilterMode.Point;

        Graphics.Blit(src,renderTexture, _shroomEffectMaterial);

        Graphics.Blit(renderTexture, dest);
        
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
