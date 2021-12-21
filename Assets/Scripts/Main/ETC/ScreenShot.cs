using UnityEngine;
using System.Collections;

public class ScreenShot : MonoBehaviour
{
    public int resWidth = 1920;
    public int resHeight = 1080;

    private new Camera camera = null;
    public bool takeHiResShot = false;

    public static string ScreenShotName(int width, int height)
    {
        if (Application.isEditor)
        {
            return string.Format("{0}/Resources/screenshots/screen_{1}x{2}_{3}.jpg",
                             Application.dataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }
        else
        {
            return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.jpg",
                             Application.persistentDataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }
        
    }
    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    void LateUpdate()
    {
        takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToJPG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            takeHiResShot = false;
        }
    }
}