using System.Collections.Generic;

namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public class ChatInfo
    {
        public string name;
        public int charID;
        public bool isNewMessage;
        public List<InfoItem> chats = new List<InfoItem>();
        public ChatInfo(string name, int charId)
        {
            this.name = name;
            this.charID = charId;
            this.isNewMessage = true;
        }
    }
}
