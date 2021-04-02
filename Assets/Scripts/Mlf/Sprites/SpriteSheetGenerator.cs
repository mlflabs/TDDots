using UnityEngine;

public class SpriteSheetGenerator : MonoBehaviour
{

    //[SerializeField] private Texture2D headTexture;
    //[SerializeField] private Texture2D bodyTexture;
    //[SerializeField] private Texture2D legTexture;
    //[SerializeField] private Texture2D leftHandTexture;
    //[SerializeField] private Texture2D rightHandTexture;

    [SerializeField] private Texture2D source;
    [SerializeField] public Color shaderColor = new Color(0, 0, 0);
    public int textureWidth = 512;
    public int textureHeight = 512;
    public int spriteHeight = 32;
    public int spriteWidth = 32;
    [Range(0, 4)]
    public int headIndex = 1;


    [SerializeField] private MeshRenderer display;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void GenerateTexture()
    {
        Texture2D texture = new Texture2D(
            textureWidth, textureHeight, TextureFormat.RGBA32, true);

        texture.filterMode = FilterMode.Point;

        //Head
        // Random.Range(0, 4);
        int h = source.height;


        Color[] colors;

        ////Body
        colors = source.GetPixels(spriteWidth * headIndex,
            h - (spriteHeight * 2), spriteWidth, spriteHeight);


        ////Head
        colors = MergeColors(colors,
                             source.GetPixels(
                                 spriteWidth * headIndex,
                                 h - spriteHeight, spriteWidth, spriteHeight));


        ////Legs
        colors = MergeColors(colors,
                             source.GetPixels(
                                 spriteWidth * headIndex,
                                 h - (spriteHeight * 3), spriteWidth, spriteHeight));





        texture.SetPixels(0, 0, spriteWidth, spriteHeight, colors);
        texture.Apply();

        display.material.SetTexture("_MainTex", texture);
        //display.material.shader.s
        //display.material.mainTexture = texture;
    }



    private Color[] MergeColors(Color[] colors1, Color[] colors2, Color shadeColor = default(Color))
    {
        Color clearColor = new Color(0, 0, 0);
        clearColor.a = 0;

        for (int i = 0; i < colors1.Length; i++)
        {

            if (colors2[i].a != 0)
            {
                colors1[i] = colors2[i];
                //colors1[i] += shadeColor;
            }
        }
        return colors1;
    }

}
