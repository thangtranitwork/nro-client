namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class GameUtils
    {
        private static GameUtils instance { get; set; }
        private long timeUse, timeChat, timeChatGlobal;
        public static GameUtils gI()
        {
            if (instance == null)
            {
                instance = new GameUtils();
            }
            return instance;
        }
        public void useItemWithTime(int id, long time)
        {
            foreach (var item in Char.myCharz().arrItemBag)
            {
                if (item != null && item.template.id == id)
                {
                    if (mSystem.currentTimeMillis() - timeUse >= time)
                    {
                        timeUse = mSystem.currentTimeMillis();
                        time = timeUse;
                        UseItem(id);
                        break;
                    }
                }
            }
        }
        public void UseItem(int id)
        {
            foreach(var item in Char.myCharz().arrItemBag)
            {
                if(item != null && item.template.id == id)
                {
                    Service.gI().useItem(0, 1, (sbyte)(item.indexUI), -1);
                    UnityEngine.Debug.Log("use");
                    break;
                }
            }
        }
        public void focusManual()
        {
            if (global::Char.myCharz().charFocus != null)
            {
                this.GotoXY(global::Char.myCharz().charFocus.cx, global::Char.myCharz().charFocus.cy);
                return;
            }
            if (global::Char.myCharz().itemFocus != null)
            {
                this.GotoXY(global::Char.myCharz().itemFocus.x, global::Char.myCharz().itemFocus.y);
                return;
            }
            if (global::Char.myCharz().mobFocus != null)
            {
                this.GotoXY(global::Char.myCharz().mobFocus.x, global::Char.myCharz().mobFocus.y);
                return;
            }
            if (global::Char.myCharz().npcFocus != null)
            {
                this.GotoXY(global::Char.myCharz().npcFocus.cx, global::Char.myCharz().npcFocus.cy - 3);
                return;
            }
            GameScr.info1.addInfo("Không Có Mục Tiêu!", 0);
        }
        public void ResetFocus()
        {
            Char.myCharz().itemFocus = null;
            Char.myCharz().mobFocus = null;
            Char.myCharz().charFocus = null;
            Char.myCharz().npcFocus = null;
        }
        public void saveBoolean(string data, bool boolean)
        {
            Rms.saveRMSInt(data, boolean ? 1 : 0);
        }
        public int GetYGround(int a)
        {
            int num = 50;
            int i = 0;
            while (i < 30)
            {
                i++;
                num += 24;
                if (TileMap.tileTypeAt(a, num, 2))
                {
                    if (num % 24 != 0)
                    {
                        num -= num % 24;
                    }
                    return num;
                }
            }
            return num;
        }
        public int MapToX(int xSd)
        {
            int num = 50;
            int i = 0;
            while (i < 30)
            {
                i++;
                num += 24;
                if (TileMap.tileTypeAt(xSd, num, 2))
                {
                    if (num % 24 != 0)
                    {
                        num -= num % 24;
                    }
                    return num;
                }
            }
            return num;
        }
        public void GotoNpc(int npcID)
        {
            for (int i = 0; i < GameScr.vNpc.size(); i++)
            {
                Npc npc = (Npc)GameScr.vNpc.elementAt(i);
                if (npc.template.npcTemplateId == npcID && global::Math.abs(npc.cx - global::Char.myCharz().cx) >= 50)
                {
                    GotoXY(npc.cx, npc.cy - 1);
                    global::Char.myCharz().focusManualTo(npc);
                    return;
                }

            }

        }
        public int FindIndexItem(int id)
        {
            for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
            {
                Item item = global::Char.myCharz().arrItemBag[i];
                if (item != null && item.template.id == id)
                {
                    return item.indexUI;
                }
            }
            return -1;
        }
        public void startChat(IChatable chatable, string name, string text, int typeInput)
        {
            ChatTextField chatTextField = ChatTextField.gI();
            chatTextField.strChat = name;
            chatTextField.tfChat.name = text;
            chatTextField.tfChat.setIputType(typeInput);
            chatTextField.left.caption = "OK";
            chatTextField.right.caption = "Đóng";
            chatTextField.startChat2(chatable, string.Empty);
        }
        public void GotoXY(int x, int y)
        {
            global::Char.myCharz().cx = x;
            global::Char.myCharz().cy = y;
            Service.gI().charMove();
        }
        public void chatWithTime(string chat, long time)
        {
            if (mSystem.currentTimeMillis() - timeChat >= time)
            {
                timeChat = mSystem.currentTimeMillis();
                time = timeChat;
                Service.gI().chat(chat);

            }
        }
        private ChatTextField tf => ChatTextField.gI();
        public void resetTF()
        {
            tf.strChat = "Chat";
            tf.tfChat.name = "chat";
            tf.to = "";
            tf.tfChat.setIputType(TField.INPUT_TYPE_ANY);
            tf.parentScreen = GameScr.gI();
            tf.isShow = false;
        }
        public void chatGlobalWithTime(string chat, long time)
        {
            if (mSystem.currentTimeMillis() - timeChatGlobal >= time)
            {
                timeChatGlobal = mSystem.currentTimeMillis();
                time = timeChatGlobal;
                Service.gI().chatGlobal(chat);

            }
        }
        public void FocusTo(int charId)
        {
            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                global::Char @char = (global::Char)GameScr.vCharInMap.elementAt(i);
                bool flag = !@char.isMiniPet && !@char.isPet && @char.charID == charId;
                if (flag)
                {
                    global::Char.myCharz().mobFocus = null;
                    global::Char.myCharz().npcFocus = null;
                    global::Char.myCharz().itemFocus = null;
                    global::Char.myCharz().charFocus = @char;
                    break;
                }
            }
        }
        public bool IsExistItem(int id)
        {
            foreach (var item in Char.myCharz().arrItemBag)
            {
                if (item != null && item.template.id == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
