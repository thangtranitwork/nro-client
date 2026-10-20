using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class AutoItem : IChatable, IActionListener
    {
        private static AutoItem instance { get; set; }
        public List<GroupItem> groupItems = new List<GroupItem>(), groupBuys = new List<GroupItem>();
        private GroupItem groupItemUse;
        private GroupItem groupItemBuy;
        private long timeUse, lastTime;
        private int UseQuantity;
        private string[] inputUse;
        private string[] inputBuy;
        public static AutoItem gI()
        {
            if (instance == null)
            {
                instance = new AutoItem();
            }
            return instance;
        }
        public AutoItem()
        {
            inputUse = new string[] { "Auto Sử Dụng - Thời Gian", "Nhập Thời Gian Sử Dụng (Giây)" };
            inputBuy = new string[] { "Auto Mua Đồ", "Nhập Số Lượng Cần Mua" };
        }
        private IEnumerator autoUse()
        {
            yield return new WaitForSecondsRealtime(0.3f);
            if (groupItems.Count == 0 || groupItems == null)
            {
                yield break;
            }
            foreach (var item in groupItems)
            {
                GameUtils.gI().useItemWithTime(item.id, timeUse * 1000L);
            }
        }
        private IEnumerator autoBuy()
        {
            yield return new WaitForSecondsRealtime(0.3f);
            if (groupBuys.Count == 0 || groupBuys == null)
            {
                yield break;
            }
            foreach (var item in groupBuys)
            {
                if (UseQuantity == 0)
                {
                    groupBuys.Remove(item);
                    GameScr.info1.addInfo("Xong!", 0);
                    break;
                }
                while (UseQuantity > 0 && mSystem.currentTimeMillis() - lastTime >= 200L)
                {
                    Service.gI().buyItem((sbyte)((item.buyGold) ? 0 : 1), item.id, 0);
                    UseQuantity--;
                }
            }
        }
        private void addGroupItemToList(GroupItem groupItem)
        {
            //Debug.LogError("Add Item");
            GameCanvas.panel.hide();
            startChat(this, inputUse[0], inputUse[1], TField.INPUT_TYPE_NUMERIC);
            this.groupItemUse = groupItem;
        }
        private void addGroupItemToListBuy(GroupItem groupItem)
        {
            //Debug.LogError("Add Item");
            GameCanvas.panel.hide();
            startChat(this, inputBuy[0], inputBuy[1], TField.INPUT_TYPE_NUMERIC);
            this.groupItemBuy = groupItem;
        }
        public bool checkContains(int id)
        {
            if (groupItems.Count == 0) return false;
            foreach (var item in groupItems)
            {
                if (item.id == id)
                {
                    return true;
                }
            }
            return false;
        }
        public void Update()
        {
            Main.main.StartCoroutine(autoUse());
            Main.main.StartCoroutine(autoBuy());
        }
        private void removeItem(int id)
        {
            for (int i = 0; i < groupItems.Count; i++)
            {
                if (groupItems[i].id == id)
                {
                    groupItems.RemoveAt(i);
                    break;
                }
            }
        }
        public void onCancelChat()
        {
            resetTF();
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
        public void onChatFromMe(string text, string to)
        {
            ChatTextField chatTextField = ChatTextField.gI();
            if (chatTextField.strChat.Equals(inputUse[0]))
            {
                int time = int.Parse(chatTextField.tfChat.getText());
                timeUse = time;
                GameScr.info1.addInfo($"Delay: {time} giây", 0);
                groupItems.Add(groupItemUse);
                resetTF();
                return;
            }
            else if (chatTextField.strChat.Equals(inputBuy[0]))
            {
                int num = int.Parse(chatTextField.tfChat.getText());
                UseQuantity = num;
                GameScr.info1.addInfo($"Số Lượng: {num}", 0);
                groupBuys.Add(groupItemBuy);
                resetTF();
                return;
            }
        }
        private ChatTextField tf => ChatTextField.gI();
        private void resetTF()
        {
            tf.strChat = "Chat";
            tf.tfChat.name = "chat";
            tf.to = "";
            tf.tfChat.setIputType(TField.INPUT_TYPE_ANY);
            tf.parentScreen = GameScr.gI();
            tf.isShow = false;
        }
        private string getNameFromItem(GroupItem group)
        {
            foreach (var item in Char.myCharz().arrItemBag)
            {
                if (item != null && item.template.id == group.id)
                {
                    return item.template.name;
                }
            }
            return "";
        }
        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1:
                    addGroupItemToList((GroupItem)p);
                    break;
                case 2:
                    GameScr.info1.addInfo("Đã xóa ra khỏi danh sách", 0);
                    short id = (short)p;
                    removeItem(id);
                    break;
                case 3:
                    addGroupItemToListBuy((GroupItem)p);
                    break;
            }
        }
    }
}

