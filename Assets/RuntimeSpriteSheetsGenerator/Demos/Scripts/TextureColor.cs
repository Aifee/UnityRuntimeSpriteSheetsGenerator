using DaVikingCode.RectanglePacking;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TextureColor : MonoBehaviour
{
    private const int RECTANGLE_COUNT = 500;
    private const float SIZE_MULTIPLIER = 2;
    private int MainTex_ID = Shader.PropertyToID("_MainTex");
    private const string ShaderName_UIDefault = "UI/Default";
    private const string ShaderName_BlitCopy = "UI/CopyStd";
    private const int TEXTURE_SIZE = 512;

    public RawImage image;
    private Material material;
    private Material sampling;
    private RenderTexture mainTexture;
    public Slider sliderWidth;
    public Slider sliderHeight;
    public Text packingTimeText;

    private RectanglePacker mPacker;
    private List<Rect> mRectangles = new List<Rect>();
    private List<Texture2D> textures = new List<Texture2D>();

    // Start is called before the first frame update
    private void Start()
    {
        sampling = new Material(Shader.Find(ShaderName_BlitCopy));
        mainTexture = RenderTexture.GetTemporary(TEXTURE_SIZE, TEXTURE_SIZE, 0, RenderTextureFormat.ARGB32);
        image.material = material;
        createRectangles();
        updateRectangles();
        sliderWidth.onValueChanged.AddListener(updatePackingBox);
        sliderHeight.onValueChanged.AddListener(updatePackingBox);
    }

    private void updatePackingBox(float value)
    {
        updateRectangles();
    }

    private void createRectangles()
    {
        int index = 0;
        int width;
        int height;
        for (int i = 0; i < 10; i++)
        {
            width = (int)(20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
            height = (int)(20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
            Texture2D texture = Resources.Load<Texture2D>(string.Format("AutoGeneratePNG/auto_{0}", index));
            textures.Add(texture);
            mRectangles.Add(new Rect(0, 0, width, height));
            index++;
        }

        for (int i = 10; i < RECTANGLE_COUNT; i++)
        {
            Texture2D texture = Resources.Load<Texture2D>(string.Format("AutoGeneratePNG/auto_{0}", index));
            textures.Add(texture);
            mRectangles.Add(new Rect(0, 0, texture.width, texture.height));
            index++;
        }
    }

    private void updateRectangles()
    {
        DateTime start = DateTime.Now;
        const int padding = 1;

        if (mPacker == null)
        {
            mPacker = new RectanglePacker((int)sliderWidth.value, (int)sliderHeight.value, padding);
        }
        else
        {
            mPacker.reset((int)sliderWidth.value, (int)sliderHeight.value, padding);
        }

        for (int i = 0; i < RECTANGLE_COUNT; i++)
        {
            mPacker.insertRectangle((int)mRectangles[i].width, (int)mRectangles[i].height, i);
        }

        mPacker.packRectangles();
        Clear();
        DateTime end = DateTime.Now;
        if (mPacker.rectangleCount > 0)
        {
            packingTimeText.text = mPacker.rectangleCount + " rectangles packed in " + (end - start).Milliseconds + "ms";
            IntegerRectangle rect = new IntegerRectangle();
            for (int j = 0; j < mPacker.rectangleCount; j++)
            {
                rect = mPacker.getRectangle(j, rect);
                int index = mPacker.getRectangleId(j);
                Texture2D texture = textures[index];
                SetTexture(texture, IntegerRectangle2Rect(rect));
            }
        }
        
    }

    private void Clear()
    {
        Graphics.SetRenderTarget(mainTexture);
        GL.PushMatrix();
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        GL.PopMatrix();
    }
    
    public void SetTexture(Texture2D texture, Rect rect)
    {
        if (material == null)
        {
            material = new Material(Shader.Find(ShaderName_UIDefault));
        }
        sampling.SetTexture(MainTex_ID, texture);
        if (sampling.SetPass(0))
        {
            Graphics.SetRenderTarget(mainTexture);

            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            {
                Vector3 vertex1 = new Vector3(rect.x, rect.y, 0);
                Vector3 vertex2 = new Vector3(rect.x, rect.y + rect.height, 0);
                Vector3 vertex3 = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
                Vector3 vertex4 = new Vector3(rect.x + rect.width, rect.y, 0);

                GL.TexCoord2(0, 0);
                GL.Vertex(vertex1);

                GL.TexCoord2(0, 1);
                GL.Vertex(vertex2);

                GL.TexCoord2(1, 1);
                GL.Vertex(vertex3);

                GL.TexCoord2(1, 0);
                GL.Vertex(vertex4);
            }
            GL.End();
            GL.PopMatrix();
        }
        material.SetTexture(MainTex_ID, mainTexture);
        image.material = material;
    }

    private Rect IntegerRectangle2Rect(IntegerRectangle integer)
    {
        Rect rect = new Rect();
        rect.x = (float)integer.x / (float)TEXTURE_SIZE;
        rect.y = (float)integer.y / (float)TEXTURE_SIZE;
        rect.width = (float)integer.width / (float)TEXTURE_SIZE;
        rect.height = (float)integer.height / (float)TEXTURE_SIZE;
        return rect;
    }
    private Color32 convertHexToRGBA(uint color)
    {
        return new Color32(
            (byte)((color >> 16) & 0xFF),
            (byte)((color >> 8) & 0xFF),
            (byte)((color) & 0xFF),
            (byte)((color >> 24) & 0xFF)
            );
    }

    [ContextMenu("Auto Generate PNG")]
    private void CreateTextures()
    {
        string root = string.Format("{0}/RuntimeSpriteSheetsGenerator/Demos/Resources/AutoGeneratePNG/", Application.dataPath);
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }
        Directory.CreateDirectory(root);
        int index = 0;
        int width;
        int height;
        for (int i = 0; i < 10; i++)
        {
            width = (int)(20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
            height = (int)(20 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER * SIZE_MULTIPLIER);
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color color = convertHexToRGBA((uint)(0xFF171703 + (((18 * ((i + 4) % 13)) << 16) + ((31 * ((i * 3) % 8)) << 8) + 63 * (((i + 1) * 3) % 5))));

            int size = width * height;
            Color32[] tmpColor = new Color32[size];
            for (int j = 0; j < tmpColor.Length; j++)
            {
                tmpColor[j] = color;
            }
            for (int j = 0; j < width; j++)
            {
                int top = j;
                tmpColor[top] = Color.black;
                int bottom = (height - 1) * width + j;
                tmpColor[bottom] = Color.black;
            }
            for (int j = 1; j < height; j++)
            {
                int left = (j - 1) * width;
                tmpColor[left] = Color.black;

                int right = (j * width) - 1;
                tmpColor[right] = Color.black;
            }
            texture.SetPixels32(0, 0, width, height, tmpColor);
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            string filename = string.Format("{0}auto_{1}.png", root, index);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            System.IO.File.WriteAllBytes(filename, bytes);
            index++;
        }

        for (int i = 10; i < RECTANGLE_COUNT; i++)
        {
            width = (int)(3 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER);
            height = (int)(3 * SIZE_MULTIPLIER + Mathf.Floor(UnityEngine.Random.value * 8) * SIZE_MULTIPLIER);
            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color color = convertHexToRGBA((uint)(0xFF171703 + (((18 * ((i + 4) % 13)) << 16) + ((31 * ((i * 3) % 8)) << 8) + 63 * (((i + 1) * 3) % 5))));

            int size = width * height;
            Color32[] tmpColor = new Color32[size];
            for (int j = 0; j < tmpColor.Length; j++)
            {
                tmpColor[j] = color;
            }
            for (int j = 0; j < width; j++)
            {
                int top = j;
                tmpColor[top] = Color.black;
                int bottom = (height - 1) * width + j;
                tmpColor[bottom] = Color.black;
            }
            for (int j = 1; j < height; j++)
            {
                int left = (j - 1) * width;
                tmpColor[left] = Color.black;

                int right = (j * width) - 1;
                tmpColor[right] = Color.black;
            }
            texture.SetPixels32(0, 0, width, height, tmpColor);
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            string filename = string.Format("{0}auto_{1}.png", root, index);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            System.IO.File.WriteAllBytes(filename, bytes);
            index++;
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}