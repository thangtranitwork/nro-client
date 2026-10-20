using Assets.Scripts.Assembly_CSharp.HSNR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.HSNR
{
    class AutoChat : IActionListener, IChatable
    {
        static AutoChat instance;
        public static AutoChat gI()
        {
            instance ??= new AutoChat();
            return instance;
        }
        public static string chatMap = "";
        public static string chatGlobal = "";
        public static long timeChatMap = 5000;
        public static long timeChatGlobal = 60000;
        public static long lastChatMap = 0;
        public static long lastChatGlobal = 0;
        public static bool enableChatMap = false;
        public static bool enableChatGlobal = false;
        public static string[] inputChatMap = new string[4]
        {
            "Nhập nội dung chat Map","Nội dung","Thời gian auto chat map(ms)","Thời gian(ms)"
        };
        public static string[] inputChatGlobal = new string[4]
        {
            "Nhập nội dung chat thế giới","Nội dung","Thời gian auto chat thế giới(ms)","Thời gian(ms)"
        };
        public static void showMenuChatMap()
        {
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Trạng thái [" + (enableChatMap ? "ON" : "OFF") + "]", gI(), 5, null));
            myVector.addElement(new Command("Nôi dung chat", gI(), 1, null));
            myVector.addElement(new Command($"Thời gian chat[{timeChatMap}]ms", gI(), 2, null));
            GameCanvas.menu.startAt(myVector, 0);
        }
        public static void showMenuChatGlobal()
        {
            MyVector myVector = new MyVector();
            myVector.addElement(new Command("Trạng thái [" + (enableChatGlobal ? "ON" : "OFF") + "]", gI(), 6, null));
            myVector.addElement(new Command("Nôi dung chat", gI(), 3, null));
            myVector.addElement(new Command($"Thời gian chat[{timeChatGlobal}]ms", gI(), 4, null));
            GameCanvas.menu.startAt(myVector, 0);
        }
        public static void update()
        {
            if (enableChatMap && mSystem.currentTimeMillis() - lastChatMap >= timeChatMap)
            {
                lastChatMap = mSystem.currentTimeMillis();
                if (chatMap != string.Empty)
                {
                    Service.gI().chat(chatMap);
                }
            }
            if (enableChatGlobal && mSystem.currentTimeMillis() - lastChatGlobal >= timeChatGlobal)
            {
                lastChatGlobal = mSystem.currentTimeMillis();
                if (chatGlobal != string.Empty)
                {
                    Service.gI().chatGlobal(chatGlobal);
                }
            }
        }
        public static void resetTF()
        {
            GameUtils.gI().resetTF();
        }

        public void onChatFromMe(string text, string to)
        {
            try
            {
                ChatTextField chat = ChatTextField.gI();
                if (chat.strChat == null || chat.strChat == string.Empty)
                {
                    resetTF();
                    return;
                }
                if (chat.tfChat.getText() == null || chat.tfChat.getText() == string.Empty)
                {
                    resetTF();
                    return;
                }
                if (chat.strChat.Equals(inputChatMap[0]))
                {
                    chatMap = chat.tfChat.getText();
                    GameScr.info1.addInfo($"Nội dung autochat:{chatMap}", 0);
                    resetTF();
                    return;
                }
                if (chat.strChat.Equals(inputChatGlobal[0]))
                {
                    chatGlobal = chat.tfChat.getText();
                    GameScr.info1.addInfo($"Nội dung autochat:{chatGlobal}", 0);
                    resetTF();
                    return;
                }
                if (chat.strChat.Equals(inputChatMap[2]))
                {
                    int time = int.Parse(chat.tfChat.getText());
                    if (time > 1000)
                    {
                        timeChatMap = time;
                        GameScr.info1.addInfo($"Thời gian autochat: {time} ms", 0);
                        resetTF();
                    }
                    else
                    {
                        GameScr.info1.addInfo($"Nhỏ nhất là 1000ms", 0);
                    }
                    return;
                }
                if (chat.strChat.Equals(inputChatGlobal[2]))
                {
                    int time = int.Parse(chat.tfChat.getText());
                    if (time >= 60000)
                    {
                        timeChatGlobal = time;
                        GameScr.info1.addInfo($"Thời gian autochat: {time} ms", 0);
                        resetTF();
                    }
                    else
                    {
                        GameScr.info1.addInfo($"Nhỏ nhất là 60000ms", 0);
                    }
                    return;
                }
            }
            finally
            { resetTF(); }
        }

        public void onCancelChat()
        {
            resetTF();
        }

        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case 1: setChatMap(); break;
                case 2: setTimeChatMap(); break;
                case 3: setChatGlobal(); break;
                case 4: setTimeChatGlobal(); break;
                case 5: enableChatMap = !enableChatMap; break;
                case 6: enableChatGlobal = !enableChatGlobal; break;
            }
        }









        public static void setChatMap()
        {
            GameCanvas.panel.isShow = false;
            ChatTextField.gI().strChat = inputChatMap[0];
            ChatTextField.gI().tfChat.name = inputChatMap[1];
            ChatTextField.gI().startChat2(gI(), string.Empty);
            ChatTextField.gI().tfChat.setText(chatMap);
        }
        public static void setTimeChatMap()
        {
            GameCanvas.panel.isShow = false;
            ChatTextField.gI().strChat = inputChatMap[2];
            ChatTextField.gI().tfChat.name = inputChatMap[3];
            ChatTextField.gI().startChat2(gI(), string.Empty);
            ChatTextField.gI().tfChat.setText(timeChatMap.ToString());
        }

        public static void setChatGlobal()
        {
            GameCanvas.panel.isShow = false;
            ChatTextField.gI().strChat = inputChatGlobal[0];
            ChatTextField.gI().tfChat.name = inputChatGlobal[1];
            ChatTextField.gI().startChat2(gI(), string.Empty);
            ChatTextField.gI().tfChat.setText(chatGlobal);
        }
        public static void setTimeChatGlobal()
        {
            GameCanvas.panel.isShow = false;
            ChatTextField.gI().strChat = inputChatGlobal[2];
            ChatTextField.gI().tfChat.name = inputChatGlobal[3];
            ChatTextField.gI().startChat2(gI(), string.Empty);
            ChatTextField.gI().tfChat.setText(timeChatGlobal.ToString());
        }
    }
}
