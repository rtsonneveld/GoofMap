using StbiSharp;
using System;
using System.IO;
using UnityEngine;

public static class BMPLoader {

    public static Texture2D LoadBMP(string fileName)
    {
        using (var imageFile = File.OpenRead(fileName)) {
            return LoadBMP(imageFile);
        }
    }

    public static Texture2D LoadBMP(Stream BPMStream)
    {
        try {
            using (var memoryStream = new MemoryStream()) {
                BPMStream.CopyTo(memoryStream);

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
}