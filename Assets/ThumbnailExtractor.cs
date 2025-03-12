using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

// Testing script in order to extract video frames that can potentially be loaded in later, in order to make the video skimming faster.
public class ThumbnailExtractor : MonoBehaviour
{
    private VideoPlayer vp;
    private string dirPath;
    private string videoURL;
    private List<Texture> frames = new List<Texture>();

    // Start is called before the first frame update
    void Start()
    {
        dirPath = Application.dataPath + "/../ThumbnailImages/";
        videoURL = Application.streamingAssetsPath + "/Catcafe.mp4";
        Directory.CreateDirectory(dirPath);
        vp = GetComponent<VideoPlayer>();
        vp.url = videoURL;
        ExtractFrames(vp);
        //if (!Directory.Exists(dirPath))
        //{
        //    //if it doesn't, create it
        //    Directory.CreateDirectory(dirPath);
        //    vp.url = videoURL;
        //    ExtractFrames(vp);
        //}
    }
    private void Update()
    {
        if (frames.Count >= (int)vp.frameCount)
        {
            for (int i = 0; i < frames.Count; i++)
            {
                Texture2D tex = GetTexture(frames[i]);
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(dirPath + i + ".png", bytes);
            }
        }
    }

    void ExtractFrames(VideoPlayer videoPlayer)
    {
        videoPlayer.Stop();
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.prepareCompleted += Prepared;
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += FrameReady;
        videoPlayer.Prepare();
    }

    void Prepared(VideoPlayer vp) => vp.Play();

    void FrameReady(VideoPlayer vp, long frameIdx)
    {
        vp.Pause();
        Debug.Log("FrameReady " + frameIdx);
        Texture textureToCopy = vp.texture;
        frames.Add(textureToCopy);
        vp.Play();
        vp.frame = frameIdx + 1;
    }

    private Texture2D GetTexture(Texture tex)
    {
        Texture2D thumbnail = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        RenderTexture cTexture = RenderTexture.active;
        RenderTexture rTexture = new RenderTexture(tex.width, tex.height, 32);
        UnityEngine.Graphics.Blit(tex, rTexture);

        RenderTexture.active = rTexture;
        thumbnail.ReadPixels(new Rect(0, 0, rTexture.width, rTexture.height), 0, 0);
        thumbnail.Apply();

        UnityEngine.Color[] pixels = thumbnail.GetPixels();

        RenderTexture.active = cTexture;

        rTexture.Release();

        return thumbnail;
    }
}
