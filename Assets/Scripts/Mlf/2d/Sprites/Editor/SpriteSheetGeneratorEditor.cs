using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteSheetGenerator))]
public class SpriteSheetGeneratorEditor : Editor
{


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpriteSheetGenerator generator = (SpriteSheetGenerator)target;

        if (GUILayout.Button("GenerateSprite"))
        {
            generator.GenerateTexture();
        }

    }




}
