namespace Assets.Scripts.Assembly_CSharp.HSNR
{
    public struct GroupItem
    {
        public int id;
        public int indexUI;
        public bool buyGold;
        public bool buyCoin;
        public long lastUse;
        public GroupItem(int id, int indexUI, bool buyGold, bool buyCoin)
        {
            this.id = id;
            this.indexUI = indexUI;
            this.buyGold = buyGold;
            this.buyCoin = buyCoin;
            lastUse = 0;
        }
    }
}
