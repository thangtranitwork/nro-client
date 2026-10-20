using System;
using System.Collections.Generic;
using System.Threading;

namespace NQMP
{
	// Token: 0x020000C0 RID: 192
	public class AutoBuyFunctions : IActionListener, IChatable
	{
		// Token: 0x06000A21 RID: 2593 RVA: 0x000A6154 File Offset: 0x000A4354
		public static AutoBuyFunctions getInstance()
		{
			bool flag = AutoBuyFunctions._Instance == null;
			if (flag)
			{
				AutoBuyFunctions._Instance = new AutoBuyFunctions();
			}
			return AutoBuyFunctions._Instance;
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x000A6184 File Offset: 0x000A4384
		public static void Update()
		{
			bool flag = AutoBuyFunctions.listItemAuto.Count > 0;
			if (flag)
			{
				for (int i = 0; i < AutoBuyFunctions.listItemAuto.Count; i++)
				{
					AutoBuyFunctions.Item item = AutoBuyFunctions.listItemAuto[i];
					bool flag2 = mSystem.currentTimeMillis() - item.LastTimeUse > (long)(item.Delay * 1000);
					if (flag2)
					{
						item.LastTimeUse = mSystem.currentTimeMillis();
						Service.gI().useItem(0, 1, -1, (short)item.Id);
						break;
					}
				}
			}
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x000A6210 File Offset: 0x000A4410
		public void onChatFromMe(string text, string to)
		{
			bool flag = ChatTextField.gI().tfChat.getText() != null && !ChatTextField.gI().tfChat.getText().Equals(string.Empty) && !text.Equals(string.Empty) && text != null;
			if (flag)
			{
				bool flag2 = ChatTextField.gI().strChat.Equals(AutoBuyFunctions.inputDelay[0]);
				if (flag2)
				{
					try
					{
						int delay = int.Parse(ChatTextField.gI().tfChat.getText());
						AutoBuyFunctions.itemToAuto.Delay = delay;
						GameScr.info1.addInfo(string.Concat(new string[]
						{
							"Auto ",
							AutoBuyFunctions.itemToAuto.Name,
							": ",
							delay.ToString(),
							" giây"
						}), 0);
						AutoBuyFunctions.listItemAuto.Add(AutoBuyFunctions.itemToAuto);
					}
					catch
					{
						GameScr.info1.addInfo("Delay Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
					}
					AutoBuyFunctions.ResetChatTextField();
				}
				else
				{
					bool flag3 = ChatTextField.gI().strChat.Equals(AutoBuyFunctions.inputBuyQuantity[0]);
					if (flag3)
					{
						try
						{
							int quantity = int.Parse(ChatTextField.gI().tfChat.getText());
							AutoBuyFunctions.itemToAuto.Quantity = quantity;
							new Thread(delegate ()
							{
								this.AutoBuy(AutoBuyFunctions.itemToAuto);
							}).Start();
						}
						catch
						{
							GameScr.info1.addInfo("Số Lượng Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
						}
						AutoBuyFunctions.ResetChatTextField();
					}
					else
					{
						bool flag4 = ChatTextField.gI().strChat.Equals(AutoBuyFunctions.inputSellQuantity[0]);
						if (flag4)
						{
							try
							{
								int quantity2 = int.Parse(ChatTextField.gI().tfChat.getText());
								AutoBuyFunctions.itemToAuto.Quantity = quantity2;
								new Thread(delegate ()
								{
									AutoBuyFunctions.AutoSell(AutoBuyFunctions.itemToAuto);
								}).Start();
							}
							catch
							{
								GameScr.info1.addInfo("Số Lượng Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
							}
							AutoBuyFunctions.ResetChatTextField();
						}
					}
				}
			}
			else
			{
				ChatTextField.gI().isShow = false;
			}
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00003FF5 File Offset: 0x000021F5
		public void onCancelChat()
		{
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x000A645C File Offset: 0x000A465C
		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
				case 1:
					AutoBuyFunctions.OpenTFAutoUseItem((AutoBuyFunctions.Item)p);
					break;
				case 2:
					AutoBuyFunctions.RemoveItemAuto((int)p);
					break;
				case 3:
					AutoBuyFunctions.OpenTFAutoTradeItem((AutoBuyFunctions.Item)p);
					break;
				case 4:
					AutoBuyFunctions.set1.Add(((global::Item)p).getFullName());
					break;
				case 5:
					AutoBuyFunctions.set2.Add(((global::Item)p).getFullName());
					break;
				case 6:
					AutoBuyFunctions.set1.Remove(((global::Item)p).getFullName());
					break;
				case 7:
					AutoBuyFunctions.set2.Remove(((global::Item)p).getFullName());
					break;
			}
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x00006812 File Offset: 0x00004A12
		private static void ResetChatTextField()
		{
			ChatTextField.gI().strChat = "Chat";
			ChatTextField.gI().tfChat.name = "chat";
			ChatTextField.gI().isShow = false;
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x000A6524 File Offset: 0x000A4724
		public static bool isAutoUse(int templateId)
		{
			for (int i = 0; i < AutoBuyFunctions.listItemAuto.Count; i++)
			{
				bool flag = AutoBuyFunctions.listItemAuto[i].Id == templateId;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x000A6570 File Offset: 0x000A4770
		private static void RemoveItemAuto(int templateId)
		{
			for (int i = 0; i < AutoBuyFunctions.listItemAuto.Count; i++)
			{
				bool flag = AutoBuyFunctions.listItemAuto[i].Id == templateId;
				if (flag)
				{
					AutoBuyFunctions.listItemAuto.RemoveAt(i);
					break;
				}
			}
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x000A65C0 File Offset: 0x000A47C0
		private static void OpenTFAutoUseItem(AutoBuyFunctions.Item item)
		{
			AutoBuyFunctions.itemToAuto = item;
			ChatTextField.gI().strChat = AutoBuyFunctions.inputDelay[0];
			ChatTextField.gI().tfChat.name = AutoBuyFunctions.inputDelay[1];
			GameCanvas.panel.isShow = false;
			ChatTextField.gI().startChat2(AutoBuyFunctions.getInstance(), string.Empty);
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x000A661C File Offset: 0x000A481C
		private static void OpenTFAutoTradeItem(AutoBuyFunctions.Item item)
		{
			AutoBuyFunctions.itemToAuto = item;
			GameCanvas.panel.isShow = false;
			bool isSell = item.IsSell;
			if (isSell)
			{
				ChatTextField.gI().strChat = AutoBuyFunctions.inputSellQuantity[0];
				ChatTextField.gI().tfChat.name = AutoBuyFunctions.inputSellQuantity[1];
			}
			else
			{
				ChatTextField.gI().strChat = AutoBuyFunctions.inputBuyQuantity[0];
				ChatTextField.gI().tfChat.name = AutoBuyFunctions.inputBuyQuantity[1];
			}
			ChatTextField.gI().startChat2(AutoBuyFunctions.getInstance(), string.Empty);
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x000A66B0 File Offset: 0x000A48B0
		private static void AutoSell(AutoBuyFunctions.Item item)
		{
			Thread.Sleep(100);
			short index = item.Index;
			while (item.Quantity > 0)
			{
				bool flag = global::Char.myCharz().arrItemBag[(int)index] == null || (global::Char.myCharz().arrItemBag[(int)index] != null && global::Char.myCharz().arrItemBag[(int)index].template.id != (short)item.Id);
				if (flag)
				{
					GameScr.info1.addInfo("Không Tìm Thấy Item!", 0);
				}
				else
				{
					Service.gI().saleItem(0, 1, (short)(index + 3));
					Thread.Sleep(100);
					Service.gI().saleItem(1, 1, index);
					Thread.Sleep(1000);
					item.Quantity--;
					bool flag2 = global::Char.myCharz().xu > 1963100000L;
					if (!flag2)
					{
						continue;
					}
					GameScr.info1.addInfo("Xong!", 0);
				}
				return;
			}
			GameScr.info1.addInfo("Xong!", 0);
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x000A67C0 File Offset: 0x000A49C0
		private void AutoBuy(AutoBuyFunctions.Item item)
		{
			while (item.Quantity > 0 && !GameScr.gI().isBagFull())
			{
				Service.gI().buyItem((sbyte)((!item.IsGold) ? 1 : 0), item.Id, 0);
				item.Quantity--;
				Thread.Sleep(500);
			}
			GameScr.info1.addInfo("Xong!", 0);
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x000A6838 File Offset: 0x000A4A38
		public static void useSet(int type)
		{
			bool flag = AutoBuyFunctions.isChangingSet;
			if (flag)
			{
				GameScr.info1.addInfo("Đang Mặc Đồ!", 0);
			}
			else
			{
				new Thread(delegate ()
				{
					bool flag2 = type == 0;
					if (flag2)
					{
						AutoBuyFunctions.ChangeSet(AutoBuyFunctions.set1);
					}
					bool flag3 = type == 1;
					if (flag3)
					{
						AutoBuyFunctions.ChangeSet(AutoBuyFunctions.set2);
					}
				}).Start();
			}
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x000A6888 File Offset: 0x000A4A88
		private static void ChangeSet(List<string> set)
		{
			bool flag = AutoBuyFunctions.isChangingSet;
			if (flag)
			{
				GameScr.info1.addInfo("Đang Mặc Đồ!", 0);
			}
			else
			{
				AutoBuyFunctions.isChangingSet = true;
				for (int i = global::Char.myCharz().arrItemBag.Length - 1; i >= 0; i--)
				{
					global::Item item = global::Char.myCharz().arrItemBag[i];
					bool flag2 = item != null && set.Contains(item.getFullName());
					if (flag2)
					{
						Service.gI().getItem(4, (sbyte)i);
						Thread.Sleep(100);
					}
				}
				AutoBuyFunctions.isChangingSet = false;
			}
		}

		// Token: 0x04001318 RID: 4888
		private static AutoBuyFunctions _Instance;

		// Token: 0x04001319 RID: 4889
		private static List<AutoBuyFunctions.Item> listItemAuto = new List<AutoBuyFunctions.Item>();

		// Token: 0x0400131A RID: 4890
		private static AutoBuyFunctions.Item itemToAuto;

		// Token: 0x0400131B RID: 4891
		public static List<string> set1 = new List<string>();

		// Token: 0x0400131C RID: 4892
		public static List<string> set2 = new List<string>();

		// Token: 0x0400131D RID: 4893
		private static bool isChangingSet;

		// Token: 0x0400131E RID: 4894
		private static string[] inputDelay = new string[]
		{
			"Nhập delay",
			"giây"
		};

		// Token: 0x0400131F RID: 4895
		private static string[] inputSellQuantity = new string[]
		{
			"Nhập số lượng bán",
			"số lượng"
		};

		// Token: 0x04001320 RID: 4896
		private static string[] inputBuyQuantity = new string[]
		{
			"Nhập số lượng mua",
			"số lượng"
		};

		// Token: 0x020000C1 RID: 193
		public class Item
		{
			// Token: 0x06000A32 RID: 2610 RVA: 0x00006852 File Offset: 0x00004A52
			public Item(int int_1, string string_0)
			{
				this.Id = int_1;
				this.Name = string_0;
			}

			// Token: 0x06000A33 RID: 2611 RVA: 0x0000686A File Offset: 0x00004A6A
			public Item(int int_1, short short_0, bool bool_0, bool bool_1)
			{
				this.Id = int_1;
				this.IsGold = bool_0;
				this.Index = short_0;
				this.IsSell = bool_1;
			}

			// Token: 0x06000A34 RID: 2612 RVA: 0x000A699C File Offset: 0x000A4B9C
			private void AutoBuy(AutoBuyFunctions.Item item)
			{
				while (item.Quantity > 0 && !GameScr.gI().isBagFull())
				{
					Service.gI().buyItem((sbyte)((!item.IsGold) ? 1 : 0), item.Id, 0);
					item.Quantity--;
					Thread.Sleep(100);
				}
				GameScr.info1.addInfo("Xong!", 0);
			}

			// Token: 0x04001321 RID: 4897
			public int Id;

			// Token: 0x04001322 RID: 4898
			public string Name;

			// Token: 0x04001323 RID: 4899
			public int Quantity;

			// Token: 0x04001324 RID: 4900
			public short Index;

			// Token: 0x04001325 RID: 4901
			public bool IsGold;

			// Token: 0x04001326 RID: 4902
			public bool IsSell;

			// Token: 0x04001327 RID: 4903
			public int Delay;

			// Token: 0x04001328 RID: 4904
			public long LastTimeUse;
		}
	}
}
