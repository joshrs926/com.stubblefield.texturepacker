using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Stubblefield.TexturePacker.Editor
{
    [CustomEditor(typeof(TexturePacker))]
    internal class TexturePackerEditor : UnityEditor.Editor
    {
        public VisualTreeAsset uxml;
        const string previewButtonClass = "cs-channel-preview-button";
        const string previewButtonSelectedClass = previewButtonClass + "--selected";

        public override VisualElement CreateInspectorGUI()
        {
            TexturePacker texturePacker = target as TexturePacker;
            VisualElement root = new VisualElement();

            if (uxml != null)
            {
                root.Add(uxml.CloneTree());
            }
            root.Bind(serializedObject);
            
            VisualElement preview = root.Q(name: "cs-preview");
            preview.schedule.Execute(() =>
            {
                Background rtBackground = new Background() { renderTexture = texturePacker.PreviewRT };
                preview.style.backgroundImage = new StyleBackground(rtBackground);
                preview.MarkDirtyRepaint();
            }).Every(100);

            Button createButton = root.Q<Button>(name: "cs-create-button");
            createButton.clicked += texturePacker.CreateImageAsset;

            Label label = root.Q<Label>(name: "cs-destination-path");
            label.schedule.Execute(() =>
            {
                label.text = texturePacker.DestinationPath;
            }).Every(100);

            SetupPreviewToggle("cs-button-rgb", TexturePacker.PreviewChannel.RGB);
            SetupPreviewToggle("cs-button-rgba", TexturePacker.PreviewChannel.RGBA);
            SetupPreviewToggle("cs-button-split", TexturePacker.PreviewChannel.Split);
            SetupPreviewToggle("cs-button-r", TexturePacker.PreviewChannel.R);
            SetupPreviewToggle("cs-button-g", TexturePacker.PreviewChannel.G);
            SetupPreviewToggle("cs-button-b", TexturePacker.PreviewChannel.B);
            SetupPreviewToggle("cs-button-a", TexturePacker.PreviewChannel.A);
            UpdateSelectedPreviewButtons();
            return root;
            
            void SetupPreviewToggle(string name, TexturePacker.PreviewChannel previewChannel)
            {
                Button button = root.Q<Button>(name: name);
                if (button != null)
                {
                    button.clicked += () =>
                    {
                        // texturePacker.previewChannel = previewChannel;
                        serializedObject.FindProperty(nameof(TexturePacker.previewChannel)).enumValueIndex = (int)previewChannel;
                        serializedObject.ApplyModifiedProperties();
                        UpdateSelectedPreviewButtons();
                    };
                }
            }

            void UpdateSelectedPreviewButtons()
            {
                foreach (Button button in root.Query<Button>(className: previewButtonClass).Build())
                {
                    button.RemoveFromClassList(previewButtonSelectedClass);
                }
                Button selectedButton = GetButton(texturePacker.previewChannel);
                selectedButton?.AddToClassList(previewButtonSelectedClass);
            }

            Button GetButton(TexturePacker.PreviewChannel previewChannel)
            {
                return previewChannel switch
                {
                    TexturePacker.PreviewChannel.R => root.Q<Button>(name: "cs-button-r"),
                    TexturePacker.PreviewChannel.G => root.Q<Button>(name: "cs-button-g"),
                    TexturePacker.PreviewChannel.B => root.Q<Button>(name: "cs-button-b"),
                    TexturePacker.PreviewChannel.A => root.Q<Button>(name: "cs-button-a"),
                    TexturePacker.PreviewChannel.RGB => root.Q<Button>(name: "cs-button-rgb"),
                    TexturePacker.PreviewChannel.RGBA => root.Q<Button>(name: "cs-button-rgba"),
                    TexturePacker.PreviewChannel.Split => root.Q<Button>(name: "cs-button-split"),
                    _ => null,
                };
            }
        }

    }
}