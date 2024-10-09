using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct SpriteLoadingProcedure
{
    public SpriteAnimator SpriteAnimator;
    public string CommonName;
    public string RelativeFolderPath;
    public SpriteQuery[] Queries;
}

[System.Serializable]
public struct SpriteQuery
{
    public string Name;
    public bool CanBeInterupted;
}

[System.Serializable]
public struct SpriteAnimationReserve
{
    public string Name;
    public List<SpriteAnimation> Animations;
}

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Main;

    private void Awake()
    {
        if (Main == null)
            Main = this;
        else
            Destroy(Main);

        foreach (SpriteLoadingProcedure procedure in procedures)
            LoadAnimations(procedure);
    }

    [Header("Settings")]
    [SerializeField]
    private int pixelPerUnit = 16;
    [SerializeField]
    private SpriteLoadingProcedure[] procedures;

    [Header("Debugs")]
    [SerializeField]
    private List<SpriteAnimationReserve> reserves;

    private void LoadAnimations(SpriteLoadingProcedure procedure)
    {
        // Get the path to the executable directory
        string executableDirectory = GetExecutableDirectory();
        string graphicsFolderPath = Path.Combine(executableDirectory, procedure.RelativeFolderPath);

        if (!Directory.Exists(graphicsFolderPath))
        {
            Debug.LogError("Graphics folder not found at: " + graphicsFolderPath);
            return;
        }

        // Get all PNG files in the Graphics folder
        string[] files = Directory.GetFiles(graphicsFolderPath, "*.png");

        List<SpriteAnimation> animations = new List<SpriteAnimation>();

        foreach (var query in procedure.Queries)
        {
            bool foundFile = false;

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();

                // Build filters
                var filters = new List<string>();
                filters.AddRange(query.Name.Replace(" ", "").Split(','));
                filters.Add(procedure.CommonName);

                bool found = true;
                foreach (var filter in filters)
                {
                    if (!fileName.Contains(filter.ToLower()))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    foundFile = true;

                    // Load texture from file
                    Texture2D texture = LoadTextureFromFile(filePath);

                    if (texture != null)
                    {
                        // Slice the texture into sprites
                        Sprite[] sprites = SliceTexture(texture, 32, 32);

                        SpriteAnimation animation = new SpriteAnimation
                        {
                            Name = query.Name,
                            Sprites = sprites,
                            canBeInterupted = query.CanBeInterupted
                        };

                        animations.Add(animation);
                        Debug.Log($"Loaded {procedure.CommonName}_{query.Name}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load texture for {procedure.CommonName}_{query.Name}");
                    }

                    break; // Exit the file loop once a match is found
                }
            }

            if (!foundFile)
            {
                Debug.LogWarning($"Cannot find {procedure.CommonName}_{query.Name}");
            }
        }

        if (procedure.SpriteAnimator == null)
        {
            reserves.Add(new SpriteAnimationReserve { Name = procedure.CommonName, Animations = animations });
            Debug.Log($"Added {animations.Count} animations to {procedure.CommonName} reserve");
        }
        else
        {
            procedure.SpriteAnimator.LoadAnimation(animations);
            Debug.Log($"Added {animations.Count} animations to {procedure.SpriteAnimator.gameObject.name}");
        }
    }


    private Texture2D LoadTextureFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                return tex;
            }
            else
            {
                Debug.LogError("Failed to load texture from file: " + filePath);
                return null;
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }
    }

    private Sprite[] SliceTexture(Texture2D texture, int spriteWidth, int spriteHeight)
    {
        List<Sprite> sprites = new List<Sprite>();

        int columns = texture.width / spriteWidth;
        int rows = texture.height / spriteHeight;

        for (int y = 0; y < rows; y++) // Loop from bottom to top
        {
            for (int x = 0; x < columns; x++)
            {
                int pixelX = x * spriteWidth;
                int pixelY = y * spriteHeight;

                Rect rect = new Rect(pixelX, pixelY, spriteWidth, spriteHeight);

                // Get the pixel data for the current frame
                Color[] pixels = texture.GetPixels(pixelX, pixelY, spriteWidth, spriteHeight);

                if (!IsFrameEmpty(pixels))
                {
                    // Create the sprite only if the frame is not empty
                    Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelPerUnit);
                    sprite.texture.filterMode = FilterMode.Point;
                    sprites.Add(sprite);
                }
            }
        }

        return sprites.ToArray();
    }

    private bool IsFrameEmpty(Color[] pixels)
    {
        foreach (Color pixel in pixels)
        {
            if (pixel.a != 0)
            {
                // Pixel is not fully transparent
                return false;
            }
        }
        return true;
    }

    private string GetExecutableDirectory()
    {
        string path = Application.dataPath;
        if (Application.isEditor)
        {
            // In the editor, Application.dataPath is the Assets folder
            // Go up one directory to get the project folder
            path = Path.GetFullPath(Path.Combine(path, ".."));
        }
        else
        {
            // In a build, Application.dataPath is the _Data folder
            // Go up one directory to get the executable folder
            path = Path.GetFullPath(Path.Combine(path, ".."));
        }
        return path;
    }

    public List<SpriteAnimation> GetReserve(string name)
    {
        foreach (var r in reserves)
        {
            if (r.Name == name)
                return r.Animations;
        }

        Debug.LogError($"Reserve not found for {name}");
        return null;
    }
}
