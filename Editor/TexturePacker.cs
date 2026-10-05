using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Stubblefield.TexturePacker.Editor
{
    internal class TexturePacker : ScriptableObject 
    {
        public Texture2D tex0;
        public Texture2D tex1;
        public Texture2D tex2;
        public Texture2D tex3;
        public SourceChannel channel0;
        public SourceChannel channel1;
        public SourceChannel channel2;
        public SourceChannel channel3;
        [Delayed] public int resolution = 1024;
        public bool isColorData;
        public PreviewChannel previewChannel = PreviewChannel.RGB;
        Shader mainShader;
        Material mainMaterial;
        Shader previewShader;
        Material previewMaterial;
        RenderTexture mainRt;
        RenderTexture previewRt;
        string destinationLocalPath;

        public RenderTexture PreviewRT => previewRt;
        public string DestinationPath => destinationLocalPath;

        const GraphicsFormat linearFormat = GraphicsFormat.R8G8B8A8_UNorm;
        const GraphicsFormat srgbFormat = GraphicsFormat.R8G8B8A8_SRGB;
        const string mainShaderName = "Custom/TexturePacker";
        const string previewShaderName = "Custom/TexturePackerPreview";
        
        void OnValidate()
        {
            Render();
            GetPaths(out destinationLocalPath, out string _);
        }

        public void CreateImageAsset()
        {
            GetPaths(out destinationLocalPath, out string absolutePath);
            Render();
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = mainRt;
            Texture2D tex = new(resolution, resolution, linearFormat, TextureCreationFlags.None);
            tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            tex.Apply();
            RenderTexture.active = active;
            byte[] bytes = tex.EncodeToPNG();
            DestroyImmediate(tex);
            File.WriteAllBytes(absolutePath, bytes);
            AssetDatabase.ImportAsset(destinationLocalPath);
            Debug.Log($"Created packed texture at '{destinationLocalPath}'");
            TextureImporter importer = AssetImporter.GetAtPath(destinationLocalPath) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = isColorData;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
            else
            {
                Debug.LogError("Failed to get TextureImporter.");
            }
        }
        
        void Render()
        {
            if (!mainShader || mainShader.name != mainShaderName)
            {
                mainShader = Shader.Find(mainShaderName);
            }
            if (!mainShader) return;
            if (!previewShader || previewShader.name != previewShaderName)
            {
                previewShader = Shader.Find(previewShaderName);
            }
            if (!previewShader) return;
            if (!mainMaterial || mainMaterial.shader != mainShader)
            {
                mainMaterial = new Material(mainShader);
            }
            if (!mainMaterial) return;
            if (!previewMaterial || previewMaterial.shader != previewShader)
            {
                previewMaterial = new Material(previewShader);
            }
            if (!previewMaterial) return;
            resolution = Math.Clamp(resolution, 1, 16384);
            if (mainRt && (mainRt.width != resolution || mainRt.graphicsFormat != linearFormat || mainRt.sRGB != isColorData))
            {
                mainRt.Release();
                DestroyImmediate(mainRt);
            }
            if (previewRt && (previewRt.width != resolution || previewRt.graphicsFormat != linearFormat || previewRt.sRGB != isColorData))
            {
                previewRt.Release();
                DestroyImmediate(previewRt);
            }
            RenderTextureDescriptor descriptor = new(resolution, resolution, linearFormat, 0);
            descriptor.sRGB = isColorData;
            if (!mainRt)
            {
                mainRt = new RenderTexture(descriptor);
                mainRt.Create();
            }
            if (!previewRt)
            {
                previewRt = new RenderTexture(descriptor);
                previewRt.Create();
            }
            
            mainMaterial.SetTexture("_Texture0", tex0 ? tex0 : Texture2D.blackTexture);
            mainMaterial.SetTexture("_Texture1", tex1 ? tex1 : Texture2D.blackTexture);
            mainMaterial.SetTexture("_Texture2", tex2 ? tex2 : Texture2D.blackTexture);
            mainMaterial.SetTexture("_Texture3", tex3 ? tex3 : Texture2D.whiteTexture);
            
            mainMaterial.SetFloat("_Channel0", (float)channel0);
            mainMaterial.SetFloat("_Channel1", (float)channel1);
            mainMaterial.SetFloat("_Channel2", (float)channel2);
            mainMaterial.SetFloat("_Channel3", (float)channel3);
            
            previewMaterial.SetInteger("_PreviewChannel", (int)previewChannel);
        
            Graphics.Blit(null, mainRt, mainMaterial);
            Graphics.Blit(mainRt, previewRt, previewMaterial);
        }

        void GetPaths(out string localPath, out string absolutePath)
        {
            string directory = "Assets";
            Object existingAsset = tex0 ? tex0 : tex1 ? tex1 : tex2 ? tex2 : tex3 ? tex3 : null;
            if (existingAsset)
            {
                string existingAssetPath = AssetDatabase.GetAssetPath(existingAsset);
                if (!string.IsNullOrEmpty(existingAssetPath))
                {
                    directory = Path.GetDirectoryName(existingAssetPath);
                }
            }
            localPath = Path.Combine(directory ?? "", "_New Packed Texture.png");
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            absolutePath = Path.Combine(Application.dataPath, localPath.Substring(7, localPath.Length - 7));
        }
        
        public enum SourceChannel
        {
            R = 0,
            G = 1,
            B = 2,
            A = 3,

            OneMinusR = 4,
            OneMinusG = 5,
            OneMinusB = 6,
            OneMinusA = 7,

            White = 8,
            Black = 9,
            Gray = 10
        }

        public enum PreviewChannel
        {
            R = 0, 
            G = 1, 
            B = 2, 
            A = 3,
            RGB = 4,
            RGBA = 5,
            Split = 6,
        }
        
        public enum ImageFileType
        {
            PNG,
            JPG,
            TGA,
            EXR,
        }
    }
}