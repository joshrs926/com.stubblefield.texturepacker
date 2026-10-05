using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Stubblefield.TexturePacker.Editor
{
    internal class TexturePackerWindow : EditorWindow
    {
        [MenuItem("Tools/Texture Packer")]
        static void ShowWindow()
        {
            TexturePackerWindow window = GetWindow<TexturePackerWindow>();
            window.titleContent = new GUIContent("Texture Packer");
        }

        void CreateGUI()
        {
            TexturePacker texturePacker = GetOrCreateTexturePacker(this);
            SerializedObject so = new(texturePacker);
            rootVisualElement.Add(new InspectorElement(so));
        }

        static TexturePacker GetOrCreateTexturePacker(TexturePackerWindow window)
        {
            MonoScript ms = MonoScript.FromScriptableObject(window);
            string scriptPath = AssetDatabase.GetAssetPath(ms);
            string scriptFolder = Path.GetDirectoryName(scriptPath);
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(TexturePacker)}", new[] { scriptFolder });
            TexturePacker texturePacker = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                texturePacker = AssetDatabase.LoadAssetAtPath<TexturePacker>(assetPath);
            }
            if (!texturePacker)
            {
                texturePacker = ScriptableObject.CreateInstance<TexturePacker>();
                string path = Path.Combine(scriptFolder, "Default Texture Packer.asset");
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(texturePacker, path);
            }
            return texturePacker;
        }
    }
}