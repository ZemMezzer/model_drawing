using Plugins.ModelDrawing.Drawing;
using UnityEngine;

namespace Plugins.ModelDrawing.Demo.Scripts
{
    public class CameraDrawing : MonoBehaviour
    {
        [SerializeField] private int _brushSize = 50;
        [SerializeField] private Color color;

        private void Update() {

            _brushSize += (int)Input.mouseScrollDelta.y;

            if (Input.GetMouseButton(0)) 
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f)) 
                {
                    if (hit.collider.TryGetComponent(out DrawingCanvas canvas))
                    {
                        int rayX = (int)(hit.textureCoord.x * canvas.GetTextureSize());
                        int rayY = (int)(hit.textureCoord.y * canvas.GetTextureSize());
                        canvas.DrawCircleAsync(rayX, rayY, _brushSize, color);
                    }
                }
            }
        }
    }
}
