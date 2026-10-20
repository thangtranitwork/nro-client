using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NQMP
{
    public class ClickHandler
    {
        private ArrayList a;
        public static int MenuSelected;
        public static int cmdX;
        public static int cmdY;
        public int size()
        {
            if (a == null)
            {
                return 0;
            }
            return a.Count;
        }
        public static void paint(mGraphics g)
        {
            if (ClickController.isClick())
            {
                if (Char.myCharz().taskMaint.taskId < 2)
                { return; }
                if (!GameCanvas.panel.isShow && GameCanvas.currentDialog == null && ChatPopup.currChatPopup == null && ChatPopup.serverChatPopUp == null)
                {
                    int x = (GameCanvas.w / 2) - 120;
                    ClickController.paintClick(g, x, GameScr.yClick, "M");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 90;
                        Service.gI().openUIZone();
                        GameCanvas.clearAllPointerEvent();
                        GameCanvas.panel.setTypeZone();
                        GameCanvas.panel.show();
                    }
                    x += 30;



                    ClickController.paintClick(g, x, GameScr.yClick, "A");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 92;
                        NQMPMain.aaMod.isAk = !NQMPMain.aaMod.isAk;
                        GameScr.info1.addInfo("Đã " + (NQMPMain.aaMod.isAk ? "bật" : "tắt") + " tự động đánh.", 0);
                        GameCanvas.clearAllPointerEvent();
                    }
                    x += 30;

                    ClickController.paintClick(g, x, GameScr.yClick, "E");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 92;
                        NQMPMain.aaMod.isAutoHoiSinh = !NQMPMain.aaMod.isAutoHoiSinh;
                        GameScr.info1.addInfo("Đã " + (NQMPMain.aaMod.isAutoHoiSinh ? "bật" : "tắt") + " tự động hồi sinh.", 0);

                        GameCanvas.clearAllPointerEvent();
                    }

                    x += 30;



                    ClickController.paintClick(g, x, GameScr.yClick, "G");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 93;
                        bool flag10 = global::Char.myCharz().charFocus == null;
                        if (flag10)
                        {
                            GameScr.info1.addInfo("Vui Lòng Chọn Mục Tiêu!", 0);
                        }
                        else
                        {
                            Service.gI().giaodich(0, global::Char.myCharz().charFocus.charID, -1, -1);
                            GameScr.info1.addInfo("Đã Gửi Lời Mời Giao Dịch Đến: " + global::Char.myCharz().charFocus.subcName(), 0);
                        }
                        GameCanvas.clearAllPointerEvent();
                    }
                    x += 30;

                    ClickController.paintClick(g, x, GameScr.yClick, "S");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        NQMPMain.aaMod.isAutoFocus = !NQMPMain.aaMod.isAutoFocus;
                        GameScr.info1.addInfo("Đã " + (NQMPMain.aaMod.isAutoFocus ? "bật" : "tắt") + " tự động Focus vào Boss.", 0);
                        GameCanvas.clearAllPointerEvent();
                    }
                    x += 30;

                    ClickController.paintClick(g, x, GameScr.yClick, "X");
                    

                    x += 30;
                    ClickController.paintClick(g, x, GameScr.yClick, "T");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        AutoTrain.ShowMenu();
                        GameCanvas.clearAllPointerEvent();
                    }

                    x += 30;
                    ClickController.paintClick(g, x, GameScr.yClick, "N");
                    if (GameCanvas.isPointerHoldIn(x, GameScr.yClick, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        AutoNhat.ShowMenu();
                        GameCanvas.clearAllPointerEvent();
                    }


                    ClickController.paintClick(g, 10, GameCanvas.h - 125, "J");
                    if (GameCanvas.isPointerHoldIn(10, GameCanvas.h - 125, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 93;
                        LoadMap.LoadMapLeft();
                        GameCanvas.clearAllPointerEvent();
                    }

                    ClickController.paintClick(g, 40, GameCanvas.h - 125, "K");
                    if (GameCanvas.isPointerHoldIn(40, GameCanvas.h - 125, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 95;
                        LoadMap.LoadMapCenter();
                        GameCanvas.clearAllPointerEvent();
                    }

                    ClickController.paintClick(g, 70, GameCanvas.h - 125, "L");
                    if (GameCanvas.isPointerHoldIn(70, GameCanvas.h - 125, 18, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 94;
                        LoadMap.LoadMapRight();
                        GameCanvas.clearAllPointerEvent();
                    }
                    if (SmallImage.imgNew[1088] == null)
                    {
                        SmallImage.createImage(1088);
                        return;
                    }
                    int w = GameScr.imgNut.getWidth();
                    int h = GameScr.imgNut.getHeight();
                    g.drawImage(GameScr.imgNut, GameCanvas.w - 85, GameCanvas.h - 85);
                    SmallImage.drawSmallImage(g, 1088, GameCanvas.w - 85 + w / 2, GameCanvas.h - 85 + h / 2, 0, mGraphics.VCENTER | mGraphics.HCENTER);

                    if (GameCanvas.isPointerHoldIn(GameCanvas.w - 85, GameCanvas.h - 85, w, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 91;
                        NQMPMain.UseItem(193);
                        NQMPMain.UseItem(194);
                        GameCanvas.clearAllPointerEvent();
                    }
                    if (SmallImage.imgNew[3896] == null)
                    {
                        SmallImage.createImage(3896);
                        return;
                    }
                    g.drawImage(GameScr.imgNut, GameCanvas.w - 40, GameCanvas.h - 120);
                    SmallImage.drawSmallImage(g, 3896, GameCanvas.w - 40 + w / 2, GameCanvas.h - 120 + h / 2, 0, mGraphics.VCENTER | mGraphics.HCENTER);
                    if (GameCanvas.isPointerHoldIn(GameCanvas.w - 40, GameCanvas.h - 120, w, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        mScreen.keyTouch = 93;
                        NQMPMain.UseItem(454);
                        NQMPMain.UseItem(921);
                        GameCanvas.clearAllPointerEvent();
                    }

                }
            }
        }
    }
}
