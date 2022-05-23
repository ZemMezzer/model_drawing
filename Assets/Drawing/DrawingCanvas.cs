using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Plugins.ModelDrawing.Drawing
{
    public class DrawingCanvas : MonoBehaviour
    {
        [Range(2, 1024)]
        [SerializeField] private int textureSize = 128;
        [SerializeField] private int applyCountDelay = 1;
        [SerializeField] private TextureWrapMode textureWrapMode;
        [SerializeField] private FilterMode filterMode;
        [SerializeField] private ComputeShader shader;
        [SerializeField] private string maskName;
        [SerializeField] private Texture2D maskTexture;

        private Material material;
        private Renderer renderer;

        private readonly float[] coords = new float[2];
        private Texture2D resultTexture;
        private RenderTexture outputTexture;
        private int kernelIndex;
        private int maskPropertyId;
        private Action<AsyncGPUReadbackRequest> readCallBack;
        private AsyncGPUReadbackRequest request;
        private int applyCounter;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            
            readCallBack = asyncAction =>
            {
                resultTexture.SetPixelData(asyncAction.GetData<byte>(), 0);
                resultTexture.Apply();
            };
            
            PrepareMaterial();
            PrepareTexture();
            PrepareShader();

            resultTexture.wrapMode = textureWrapMode;
            resultTexture.filterMode = filterMode;
        }

        private void PrepareTexture()
        {
            resultTexture = new Texture2D(textureSize, textureSize);

            if (maskTexture != null)
            {
                resultTexture.SetPixels(maskTexture.GetPixels());
                resultTexture.Apply();
            }
            
            material.SetTexture(maskPropertyId, resultTexture);
        }
        
        private void PrepareMaterial()
        {
            maskPropertyId = Shader.PropertyToID(maskName);
            renderer = GetComponent<Renderer>();
            material = renderer.material;
            material = Instantiate(material);
            renderer.material = material;
            material.EnableKeyword(maskName);
        }

        private void PrepareShader()
        {
            outputTexture = new RenderTexture(textureSize, textureSize, 0)
            {
                enableRandomWrite = true
            };
            outputTexture.Create();
            kernelIndex = shader.FindKernel("Draw");
        }

        public void DrawCircleAsync(int x, int y, float size, Color color)
        {
            DispatchKernel(x,y,size,color);
            
            applyCounter++;
            
            if (request.done && applyCounter % applyCountDelay == 0)
            {
                 request = AsyncGPUReadback.Request(outputTexture, 0, readCallBack);
            }
            
            if (applyCounter % applyCountDelay == 0)
            {
                resultTexture.Apply();
            }
        }

        public void DrawCircle(int x, int y, float size, Color color)
        {
            DispatchKernel(x,y,size,color);

            applyCounter++;

            if (!request.done)
            {
                request.WaitForCompletion();
            }
            
            if (applyCounter % applyCountDelay == 0)
            {
                resultTexture.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
            }
            

            if (applyCounter % applyCountDelay == 0)
            {
                resultTexture.Apply();
            }
        }

        private void DispatchKernel(int x, int y, float size, Color color)
        {
            coords[0] = x;
            coords[1] = y;
            
            shader.SetTexture(kernelIndex, "Result", outputTexture);
            shader.SetTexture(kernelIndex, "ImageInput", material.GetTexture(maskPropertyId));
            shader.SetFloats("DrawPoint", coords);
            shader.SetFloat("BrushSize", size);
            shader.SetVector("Color", color);

            shader.Dispatch(kernelIndex, textureSize/8 , textureSize / 8, 1);

            RenderTexture.active = outputTexture;
        }

        public int GetTextureSize()
        {
            return textureSize;
        }
    }
}
