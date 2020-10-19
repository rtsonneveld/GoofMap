// This was made by aaro4130 on the Unity forums.  Thanks boss!
// It's been optimized and slimmed down for the purpose of loading Quake 3 TGA textures from memory streams.

using StbiSharp;
using System;
using System.IO;
using UnityEngine;

public static class TGALoader {

    public static Texture2D LoadTGA(string fileName)
    {
        using (var imageFile = File.OpenRead(fileName)) {
            return LoadTGA(imageFile);
        }
    }

    public static Texture2D LoadTGA(Stream TGAStream)
    {

        try {
            using (var memoryStream = new MemoryStream()) {
                TGAStream.CopyTo(memoryStream);

                var image = Stbi.LoadFromMemory(memoryStream, 4);

                MemoryStream pngStream = new MemoryStream();

                byte[] array = image.Data.ToArray();

                Texture2D tex = new Texture2D(image.Width, image.Height, TextureFormat.RGBA32, false);
                tex.LoadRawTextureData(array);
                tex.Apply();
                tex.EncodeToPNG();

                return tex;
            }
        } catch (Exception e) {
            Debug.LogError(e);
            return new Texture2D(8, 8);
        }

    }

    /*
     * public static Texture2D LoadTGA(Stream TGAStream)
    {
        var image = TGASharpLib.TGA.FromStream(TGAStream);

        Bitmap bmp = image.ToBitmap();

        MemoryStream ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        var buffer = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(buffer, 0, buffer.Length);
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(buffer);
        
        return tex;
    }*/
}