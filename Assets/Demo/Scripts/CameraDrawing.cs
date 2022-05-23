using ModelDrawing.Drawing;
using UnityEngine;

namespace ModelDrawing.Demo.Scripts
{
    public class CameraDrawing : MonoBehaviour
    {
        [SerializeField] private int brushSize = 50;
        [SerializeField] private Color color;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update() {

            brushSize += (int)Input.mouseScrollDelta.y;

            if (Input.GetMouseButton(0)) 
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f)) 
                {
                    if (hit.collider.TryGetComponent(out DrawingCanvas canvas))
                    {
                        int rayX = (int)(hit.textureCoord.x * canvas.GetTextureSize());
                        int rayY = (int)(hit.textureCoord.y * canvas.GetTextureSize());
                        canvas.DrawCircleAsync(rayX, rayY, brushSize, color);
                    }
                }
            }
        }
    }
}
