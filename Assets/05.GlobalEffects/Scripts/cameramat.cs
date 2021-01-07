using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameramat : MonoBehaviour
{
	public Material material;
     
	private void OnRenderImage(RenderTexture source, RenderTexture target) {
     
		Graphics.Blit(source, target, material);
   
     
    }
    
}
