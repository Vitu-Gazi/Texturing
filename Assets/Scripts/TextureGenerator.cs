using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    [SerializeField] private Material sourceMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;

    private Material material;
    private Texture2D texture;

    private List<WaterController> waters = new List<WaterController>();

    private void Start()
    {
        material = new Material(sourceMaterial);

        texture = new Texture2D(128, 128);
        material.mainTexture = texture;

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if (x > 20 && x < 70 && y > 20 && y < 70)
                {
                    texture.SetPixel(x, y, Color.blue);
                    waters.Add(new WaterController(x, y));
                }
                else
                {
                    texture.SetPixel(x, y, Color.red);
                }
            }
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        meshRenderer.material = material;

        StartCoroutine(PixelGravity());
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (meshCollider.Raycast(ray, out RaycastHit hit, 100))
            {
                int x = (int)(hit.textureCoord.x * texture.width);
                int y = (int)(hit.textureCoord.y * texture.height);

                texture.SetPixel(x, y, Color.blue);

                foreach (var water in waters)
                {
                    if (water.x == x && water.y == y)
                    {
                        texture.Apply();
                        return;
                    }
                }

                waters.Add(new WaterController(x, y));

                texture.Apply();
            }
        }
    }

    private IEnumerator PixelGravity ()
    {
        yield return new WaitForSeconds (0.02f);

        foreach (var water in waters)
        {
            if (texture.GetPixel(water.x, water.y + 1) == Color.red)
            {
                texture.SetPixel(water.x, water.y, Color.red);
                texture.SetPixel(water.x, water.y + 1, Color.blue);

                water.y++;
            }
            else if (CheckBottomLayer(water))
            {
                if (WaterLeftOffset(water))
                {
                    texture.SetPixel(water.x, water.y, Color.red);
                    texture.SetPixel(water.x + 1, water.y, Color.blue);

                    water.x++;
                }
                else if (WaterRightOffset(water))
                {
                    texture.SetPixel(water.x, water.y, Color.red);
                    texture.SetPixel(water.x - 1, water.y, Color.blue);

                    water.x--;
                }
            }
        }

        texture.Apply();

        StartCoroutine(PixelGravity());
    }

    private bool WaterLeftOffset (WaterController water)
    {
        if (texture.GetPixel(water.x + 1, water.y) != Color.blue)
        {
            return true;
        }

        return false;
    }
    private bool WaterRightOffset(WaterController water)
    {
        if (texture.GetPixel(water.x - 1, water.y) != Color.blue)
        {
            return true;
        }

        return false;
    }
    private bool CheckBottomLayer (WaterController water)
    {
        for (int i = 0; i < texture.width; i++)
        {
            if (texture.GetPixel(i, water.y - 1) == Color.blue && texture.GetPixel(i, water.y + 1) == Color.blue)
            {
                return true;
            }
        }

        return false;
    }
}


[Serializable]
public class WaterController
{
    public WaterController (int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x;
    public int y;
}
