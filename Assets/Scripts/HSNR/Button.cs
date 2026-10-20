namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class Button
    {
        public string caption;
        public IActionListener ActionListener;
        public int idAction;
        public int x, y;
        private bool isFocus;
        public Button(string caption, IActionListener actionListener, int idAction)
        {
            this.caption = caption;
            ActionListener = actionListener;
            this.idAction = idAction;
        }
        public void performAction()
        {
            GameCanvas.clearAllPointerEvent();
            if (idAction > 0)
            {
                if (ActionListener != null)
                {
                    ActionListener.perform(idAction, null);
                }
                else
                {
                    GameScr.gI().actionPerform(idAction, null);
                }
            }
        }
        public bool isPointerPressInside()
        {
            isFocus = false;
            if (GameCanvas.isPointerHoldIn(x, y, 18, 18))
            {
                if (GameCanvas.isPointerDown)
                {
                    isFocus = true;
                }
                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointerClick)
                {
                    return true;
                }
            }
            return false;
        }
        public void Paint(mGraphics g)
        {
            int b = isFocus ? 14 : 18;
            int s = isFocus ? 12 : 16;
            int xx = isFocus ? x + 2 : x;
            int yy = isFocus ? y + 2 : y;
            g.setColor(0, 0.5f);
            g.fillRect(xx, yy, b, b, 100);
            g.setColor(16384512);
            g.fillRect(xx + 1, yy + 1, s, s, 100);
            if (!string.IsNullOrEmpty(caption))
            {
                mFont.tahoma_7b_yellow.drawString(g, caption, x + 10, y + 8 - mFont.tahoma_7b_yellow.getHeight() / 2, mGraphics.VCENTER | mGraphics.HCENTER);
            }
        }
    }
}
