
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NQMP
{
    public class AutoItem : IActionListener, IChatable
    {
        public static AutoItem gI()
        {
            if (AutoItem.Instance == null)
            {
                AutoItem.Instance = new AutoItem();
            }
            return AutoItem.Instance;
        }

        public static void update()
        {
            if (AutoItem.listItemUse.Count == 00)
            {
                return;
            }
            for (int i = 0; i < AutoItem.listItemUse.Count; i++)
            {

                if (mSystem.currentTimeMillis() - AutoItem.listItemUse[i].lastTimeUseItem > (long)(AutoItem.listItemUse[i].timeDelay * 1000))
                {
                    if (!GameScr.gI().isBagFull())
                    {
                        if (FindItem(listItemUse[i].idItems) != -1)
                        {
                            AutoItem.listItemUse[i].lastTimeUseItem = mSystem.currentTimeMillis();
                            Service.gI().useItem(0, 1, FindItem(listItemUse[i].idItems), -1);
                        }
                        else { listItemUse.Remove(listItemUse[i]); break; }
                    }
                    else
                    {
                        listItemUse.Remove(listItemUse[i]); break;
                    }
                }
            }
        }
        public static sbyte FindItem(int item)
        {
            for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
            {
                Item it = Char.myCharz().arrItemBag[i];
                if (it != null && it.template.id == item)
                {
                    return (sbyte)i;
                }
            }
            return -1;
        }

        public void onChatFromMe(string text, string to)
        {
            if (ChatTextField.gI().tfChat.getText() == null || ChatTextField.gI().tfChat.getText().Equals(string.Empty) || text.Equals(string.Empty) || text == null)
            {
                resetTFChat();
                return;
            }
            if (ChatTextField.gI().strChat.Equals(AutoItem.inputTimeDelay[0]))
            {
                try
                {
                    int time = int.Parse(ChatTextField.gI().tfChat.getText());
                    GameScr.info1.addInfo(string.Concat(new string[]
                    {
                    "Auto: ",
                    ItemTemplates.get((short)AutoItem.idItemObj).name,
                    " [",
                    time.ToString(),
                    " giây]"
                    }), 0);
                    AutoItem.listItemUse.Add(new AutoItem(AutoItem.idItemObj, time));
                    UnityEngine.Debug.Log("ID Auto 2 :" + (int)idItemObj);
                }
                catch
                {
                    GameScr.info1.addInfo("Số Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
                    ShowMenu();
                }
                resetTFChat();
                return;
            }
        }
        void resetTFChat()
        {
            ChatTextField.gI().isShow = false;
            ChatTextField.gI().strChat = "Chat";
            ChatTextField.gI().tfChat.name = "chat";
            ChatTextField.gI().parentScreen = GameScr.gI();
        }
        public void onCancelChat()
        {
            resetTFChat();
        }

        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1:

                    AutoItem.idItemObj = (int)p;
                    ChatTextField.gI().strChat = AutoItem.inputTimeDelay[0];
                    ChatTextField.gI().tfChat.name = AutoItem.inputTimeDelay[1];
                    GameCanvas.panel.isShow = false;
                    ChatTextField.gI().startChat2(AutoItem.gI(), string.Empty);
                    return;
                case 2:
                    for (int i = 0; i < AutoItem.listItemUse.Count; i++)
                    {
                        if (AutoItem.listItemUse[i].idItems == (int)p)
                        {
                            AutoItem.listItemUse.RemoveAt(i);
                            GameScr.info1.addInfo("Dừng auto: " + ItemTemplates.get((short)((int)p)).name, 0);
                        }
                    }
                    return;
                case 3:
                    return;
                case 10:
                    ShowMenu();
                    break;
                case 11:
                    Item item = (Item)p;
                    if (item != null)
                    {
                        int quatity = item.quantity / 7;
                        ChatTextField.gI().strChat = "Nhập Số Lượng Muốn Ghép";
                        ChatTextField.gI().tfChat.name = "Số lượng có thể ghép :" + quatity + " - Sẽ cần " + (quatity * 7) + " viên";
                        GameCanvas.panel.isShow = false;
                        tempIDNro = item.template.id;
                        ChatTextField.gI().startChat2(AutoItem.gI(), string.Empty);
                    }
                    break;
                default:
                    return;
            }
        }
        public static int tempIDNro = 0;
        public static void ShowMenu()
        {
            MyVector myVector = new MyVector();
            List<int> list = new List<int>();
            list.Clear();
            for (int i = global::Char.myCharz().arrItemBag.Length - 1; i >= 0; i--)
            {
                Item item = global::Char.myCharz().arrItemBag[i];
                if (item != null && !list.Contains((int)item.template.id) && AutoItem.ItemCanUse(item))
                {
                    list.Add((int)item.template.id);
                }
            }
            for (int j = 0; j < list.Count; j++)
            {
                if (!AutoItem.checkList(list[j]))
                {
                    myVector.addElement(new Command(string.Concat(new string[]
                    {
                    ItemTemplates.get((short)list[j]).name,
                    "\n[TẮT]"
                    }), AutoItem.gI(), 1, list[j]));
                }
                else
                {
                    for (int k = 0; k < AutoItem.listItemUse.Count; k++)
                    {
                        if (AutoItem.listItemUse[k].idItems == list[j])
                        {
                            myVector.addElement(new Command(string.Concat(new string[]
                            {
                            ItemTemplates.get((short)list[j]).name,
                            "\n[",
                            AutoItem.listItemUse[k].timeDelay.ToString(),
                            " giây]"
                            }), AutoItem.gI(), 2, list[j]));
                        }
                    }
                }
            }
            GameCanvas.menu.startAt(myVector, 3);
        }
        public static bool ItemCanUse(Item item)
        {
            return item.template.type == 29 || item.template.type == 27 || item.template.type == 6 || item.template.type == 31;
        }
        public AutoItem(int ID, int time)
        {
            this.idItems = ID;
            this.timeDelay = time;
        }
        public AutoItem()
        {
        }

        public static bool checkList(int Id)
        {
            for (int i = 0; i < AutoItem.listItemUse.Count; i++)
            {
                if (AutoItem.listItemUse[i].idItems == Id)
                {
                    return true;
                }
            }
            return false;
        }

        private static AutoItem Instance;

        public long lastTimeUseItem;

        public static int TimeUseItem;

        public static List<AutoItem> listItemUse = new List<AutoItem>();

        public int idItems;

        public int timeDelay;

        private static string[] inputTimeDelay = new string[]
        {
        "Nhập thời gian sử dụng item <Giây>",
        "Time Item"
        };

        private static int idItemObj;
    }
}
