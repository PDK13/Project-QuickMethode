using System.IO;
using UnityEngine;

public class QSprite
{
    #region ==================================== File

    public static Sprite GetScreenShot(string Path)
    {
        Texture2D TextureScreen = null;
        byte[] ByteEncode;

        ByteEncode = File.ReadAllBytes(Path);
        TextureScreen = new Texture2D(100, 100, TextureFormat.RGBA32, false);
        TextureScreen.LoadImage(ByteEncode);
        //
        if (TextureScreen == null)
            return null;
        //
        return Sprite.Create(TextureScreen, new Rect(0, 0, TextureScreen.width, TextureScreen.height), new Vector2(0.5f, 0.5f));
    }

    #endregion

    #region ==================================== Sprite

    public static Vector2 GetSpriteSizePixel(Sprite From)
    {
        return GetSpriteSizeUnit(From) * GetSpritePixelPerUnit(From) * 1.0f;
    }

    public static Vector2 GetSpriteSizeUnit(Sprite From)
    {
        return From.bounds.size * 1.0f;
    }

    public static float GetSpritePixelPerUnit(Sprite From)
    {
        return From.pixelsPerUnit * 1.0f;
    }

    #endregion

    #region ==================================== Sprite Renderer

    //...

    #endregion

    #region ==================================== Border

    public static Vector2 GetBorderPos(SpriteRenderer From, params Direction[] Bound)
    {
        //Primary Value: Collider
        Vector2 Pos = From.bounds.center;
        Vector2 Size = From.bounds.size;

        //Caculate Position from Value Data
        foreach (Direction DirBound in Bound)
        {
            switch (DirBound)
            {
                case Direction.Up:
                    Pos.y += Size.y / 2;
                    break;
                case Direction.Down:
                    Pos.y -= Size.y / 2;
                    break;
                case Direction.Left:
                    Pos.x -= Size.x / 2;
                    break;
                case Direction.Right:
                    Pos.x += Size.x / 2;
                    break;
            }
        }
        return Pos;
    }

    public static Vector2 GetBorderPos(SpriteRenderer From, params DirectionX[] Bound)
    {
        //Primary Value: Collider
        Vector2 Pos = From.bounds.center;
        Vector2 Size = From.bounds.size;

        //Caculate Position from Value Data
        foreach (DirectionX DirBound in Bound)
        {
            switch (DirBound)
            {
                case DirectionX.Left:
                    Pos.x -= Size.x / 2;
                    break;
                case DirectionX.Right:
                    Pos.x += Size.x / 2;
                    break;
            }
        }
        return Pos;
    }

    public static Vector2 GetBorderPos(SpriteRenderer From, params DirectionY[] Bound)
    {
        //Primary Value: Collider
        Vector2 Pos = From.bounds.center;
        Vector2 Size = From.bounds.size;

        //Caculate Position from Value Data
        foreach (DirectionY DirBound in Bound)
        {
            switch (DirBound)
            {
                case DirectionY.Up:
                    Pos.y += Size.y / 2;
                    break;
                case DirectionY.Down:
                    Pos.y -= Size.y / 2;
                    break;
            }
        }
        return Pos;
    }

    #endregion

    #region ==================================== Texture

    //Texture can be used for Window Editor (Button)

    //Ex:
    //Window Editor:
    //Texture2D Texture = QSprite.GetTextureConvert(Sprite);
    //GUIContent Content = new GUIContent("", (Texture)Texture);
    //GUILayout.Button(Content());

    public static Texture2D GetTextureConvert(Sprite From)
    {
        if (From.texture.isReadable == false)
        {
            return null;
        }

        Texture2D Texture = new Texture2D((int)From.rect.width, (int)From.rect.height);

        Color[] ColorPixel = From.texture.GetPixels(
            (int)From.textureRect.x,
            (int)From.textureRect.y,
            (int)From.textureRect.width,
            (int)From.textureRect.height);
        Texture.SetPixels(ColorPixel);
        Texture.Apply();
        return Texture;
    }

    public static string GetTextureConvertName(Sprite From)
    {
        return GetTextureConvert(From).name;
    }

    #endregion
}