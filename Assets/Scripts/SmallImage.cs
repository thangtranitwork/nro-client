using System;
using System.Text;
using Assets.src.e;
using UnityEngine;

public class SmallImage
{
    public static int[][] smallImg;

    public static SmallImage instance;

    public static Image[] imgbig;

    public static Small[] imgNew;

    public static MyVector vKeys = new MyVector();

    public static Image imgEmpty = null;

    public static sbyte[] newSmallVersion;

    public static int smallCount;

    public static short maxSmall;

    public SmallImage()
    {
        readImage();
    }

    public static void loadBigRMS()
    {
        if (imgbig == null)
        {
            imgbig = new Image[5]
            {
                GameCanvas.loadImageRMS("/img/Big0.png"),
                GameCanvas.loadImageRMS("/img/Big1.png"),
                GameCanvas.loadImageRMS("/img/Big2.png"),
                GameCanvas.loadImageRMS("/img/Big3.png"),
                GameCanvas.loadImageRMS("/img/Big4.png")
            };
        }
    }

    public static void freeBig()
    {
        imgbig = null;
        mSystem.gcc();
    }

    public static void loadBigImage()
    {
        imgEmpty = Image.createRGBImage(new int[1], 1, 1, bl: true);
    }

    public static void init()
    {
        instance = null;
        instance = new SmallImage();
    }

    public void readData(byte[] data)
    {
    }

    public void readImage()
    {
        int num = 0;
        try
        {
            DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NR_image"));
            short num2 = dataInputStream.readShort();
            smallImg = new int[num2][];
            for (int i = 0; i < smallImg.Length; i++)
            {
                smallImg[i] = new int[5];
            }
            for (int j = 0; j < num2; j++)
            {
                num++;
                smallImg[j][0] = dataInputStream.readUnsignedByte();
                smallImg[j][1] = dataInputStream.readShort();
                smallImg[j][2] = dataInputStream.readShort();
                smallImg[j][3] = dataInputStream.readShort();
                smallImg[j][4] = dataInputStream.readShort();
            }
        }
        catch (Exception ex)
        {
            Cout.LogError3("Loi readImage: " + ex.ToString() + "i= " + num);
        }
    }

    public static void clearHastable()
    {
    }

    public static string HexToString(string text)
    {
        try
        {
            byte[] array = new byte[text.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }
            return Encoding.UTF8.GetString(array);
        }
        catch(Exception e) { return ""; }
    }

    public static string StringToHex(string str)
    {
        StringBuilder stringBuilder = new StringBuilder();
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        foreach (byte b in bytes)
        {
            stringBuilder.Append(b.ToString("X2"));
        }
        return stringBuilder.ToString();
    }

    public static void checkImgNewSize(int id)
    {
        if (id < 0)
        {
            return;
        }
        if (imgNew == null)
        {
            imgNew = new Small[Math.Max(id + 1000, 35000)];
        }
        else if (id >= imgNew.Length)
        {
            Array.Resize(ref imgNew, Math.Max(id + 1000, imgNew.Length * 2));
        }
    }

    public static void createImage(int id)
    {
        checkImgNewSize(id);

        string appDir = "";
        try
        {
            if (!string.IsNullOrEmpty(Application.dataPath))
            {
                appDir = System.IO.Path.GetFullPath(Application.dataPath + "/..");
            }
        }
        catch { }

        string baseDir = "";
        try
        {
            baseDir = AppDomain.CurrentDomain.BaseDirectory;
        }
        catch { }

        string[] candidateDirs = new string[]
        {
            appDir,
            baseDir,
            ".",
            "/home/thang/coding/HUNR_Windows_Client",
            "/home/thang/coding/HUNR_Client",
            "Z:/home/thang/coding/HUNR_Windows_Client",
            "Z:/home/thang/coding/HUNR_Client"
        };

        for (int d = 0; d < candidateDirs.Length; d++)
        {
            string dir = candidateDirs[d];
            if (string.IsNullOrEmpty(dir)) continue;

            string[] testFiles = new string[]
            {
                System.IO.Path.Combine(dir, "resources/image/" + mGraphics.zoomLevel + "/small/Small" + id + ".png"),
                System.IO.Path.Combine(dir, "resources/image/4/small/Small" + id + ".png"),
                System.IO.Path.Combine(dir, "resources/image/2/small/Small" + id + ".png"),
                System.IO.Path.Combine(dir, "SmallImage/Small" + id + ".png"),
                System.IO.Path.Combine(dir, "Assets/Resources/res/x" + mGraphics.zoomLevel + "/SmallImage/Small" + id + ".png")
            };

            for (int f = 0; f < testFiles.Length; f++)
            {
                string path = testFiles[f];
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        byte[] fileData = System.IO.File.ReadAllBytes(path);
                        if (fileData != null && fileData.Length > 0)
                        {
                            Image diskImg = Image.createImage(fileData);
                            if (diskImg != null && diskImg.texture != null)
                            {
                                imgNew[id] = new Small(diskImg, id);
                                return;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        Res.outz("is request =" + id + " zoom=" + mGraphics.zoomLevel);
        Image image2 = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
        if (image2 != null)
        {
            imgNew[id] = new Small(image2, id);
            return;
        }
        bool flag = false;
        sbyte[] array = null;
        try
        {
            array = ArrayCast.cast(Convert.FromBase64String(HexToString(Rms.loadRMSString(mGraphics.zoomLevel + "Small" + id))));
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
        if (array != null)
        {
            if (newSmallVersion != null && array.Length % 127 != newSmallVersion[id])
            {
                flag = true;
            }
            if (!flag)
            {
                Image image3 = Image.createImage(array, 0, array.Length);
                if (image3 != null)
                {
                    imgNew[id] = new Small(image3, id);
                }
                else
                {
                    flag = true;
                }
            }
        }
        else
        {
            flag = true;
        }
        if (flag)
        {
            if (imgNew[id] == null)
            {
                imgNew[id] = new Small(imgEmpty, id);
                Service.gI().requestIcon(id);
            }
            else if (imgNew[id].img == null || imgNew[id].img.Equals(imgEmpty))
            {
                if (GameCanvas.gameTick % 120 == 0)
                {
                    Service.gI().requestIcon(id);
                }
            }
        }
    }

    public static void drawSmallImage(mGraphics g, int id, int x, int y, int transform, int anchor)
    {
        if (id < 0)
        {
            return;
        }
        checkImgNewSize(id);
        if (id >= imgNew.Length)
        {
            return;
        }
        Small small = imgNew[id];
        if (small == null || small.img == null || small.img.Equals(imgEmpty))
        {
            createImage(id);
            small = imgNew[id];
        }
        if (imgbig == null)
        {
            if (small != null && small.img != null && !small.img.Equals(imgEmpty))
            {
                g.drawRegion(small, 0, 0, mGraphics.getImageWidth(small.img), mGraphics.getImageHeight(small.img), transform, x, y, anchor);
            }
        }
        else if (smallImg != null)
        {
            if (id >= smallImg.Length || smallImg[id] == null || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
            {
                if (small != null && small.img != null && !small.img.Equals(imgEmpty))
                {
                    small.paint(g, transform, x, y, anchor);
                }
            }
            else if (imgbig[smallImg[id][0]] != null)
            {
                g.drawRegion(imgbig[smallImg[id][0]], smallImg[id][1], smallImg[id][2], smallImg[id][3], smallImg[id][4], transform, x, y, anchor);
            }
        }
        else if (GameCanvas.currentScreen != GameScr.gI())
        {
            if (small != null && small.img != null && !small.img.Equals(imgEmpty))
            {
                small.paint(g, transform, x, y, anchor);
            }
        }
    }

    public static void drawSmallImage(mGraphics g, int id, int f, int x, int y, int w, int h, int transform, int anchor)
    {
        if (id < 0)
        {
            return;
        }
        checkImgNewSize(id);
        if (id >= imgNew.Length)
        {
            return;
        }
        Small small = imgNew[id];
        if (small == null || small.img == null || small.img.Equals(imgEmpty))
        {
            createImage(id);
            small = imgNew[id];
        }
        if (imgbig == null)
        {
            if (small != null && small.img != null && !small.img.Equals(imgEmpty))
            {
                g.drawRegion(small.img, 0, f * w, w, h, transform, x, y, anchor);
            }
        }
        else if (smallImg != null)
        {
            if (id >= smallImg.Length || smallImg[id] == null || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
            {
                if (small != null && small.img != null && !small.img.Equals(imgEmpty))
                {
                    small.paint(g, transform, f, x, y, w, h, anchor);
                }
            }
            else if (smallImg[id][0] != 4 && imgbig[smallImg[id][0]] != null)
            {
                g.drawRegion(imgbig[smallImg[id][0]], 0, f * w, w, h, transform, x, y, anchor);
            }
            else
            {
                if (small != null && small.img != null && !small.img.Equals(imgEmpty))
                {
                    small.paint(g, transform, f, x, y, w, h, anchor);
                }
            }
        }
        else if (GameCanvas.currentScreen != GameScr.gI())
        {
            if (small != null && small.img != null && !small.img.Equals(imgEmpty))
            {
                small.paint(g, transform, f, x, y, w, h, anchor);
            }
        }
    }

    public static void update()
    {

    }
}
