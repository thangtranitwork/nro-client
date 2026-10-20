using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomGraphics
{
    class ComboBox
    {
        public int x, y, w, h;
        public bool isOpen;
        public int selectedIndex;
        public List<string> items;
        public Action<int> onSelectionChanged;
        public string caption;
        public int maxVisibleItems = 5;
        public int itemHeight = 20;
        
        public ComboBox(int x, int y, int w, int h, List<string> items, Action<int> onSelectionChanged, string caption = "")
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.items = items ?? new List<string>();
            this.onSelectionChanged = onSelectionChanged;
            this.caption = caption;
            this.selectedIndex = 0;
            this.isOpen = false;
        }
        
        public void updateKey()
        {
            // Xử lý click vào combobox chính
            if (GameCanvas.isPointerHoldIn(x, y, w, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
            {
                GameCanvas.clearAllPointerEvent();
                try
                {
                    isOpen = !isOpen;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            // Xử lý click vào các item trong dropdown
            if (isOpen)
            {
                int dropdownHeight = Math.Min(items.Count, maxVisibleItems) * itemHeight;
                int dropdownY = y + h;
                
                for (int i = 0; i < Math.Min(items.Count, maxVisibleItems); i++)
                {
                    int itemY = dropdownY + (i * itemHeight);
                    
                    if (GameCanvas.isPointerHoldIn(x, itemY, w, itemHeight) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                    {
                        GameCanvas.clearAllPointerEvent();
                        try
                        {
                            selectedIndex = i;
                            isOpen = false;
                            onSelectionChanged?.Invoke(selectedIndex);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        break;
                    }
                }
                
                // Click bên ngoài để đóng dropdown
                if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                {
                    if (!GameCanvas.isPointerHoldIn(x, y, w, h + dropdownHeight))
                    {
                        isOpen = false;
                    }
                }
            }
        }
        
        public void paint(mGraphics g)
        {
            // Vẽ combobox chính
            g.setColor(11837316);
            g.fillRect(x, y, w, h, 5);
            g.setColor(10587766);
            g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
            
            // Vẽ text đã chọn
            string selectedText = items.Count > 0 && selectedIndex < items.Count ? items[selectedIndex] : "";
            mFont.tahoma_7b_dark.drawString(g, selectedText, x + 5, y + 5, mFont.LEFT);
            
            // Vẽ mũi tên dropdown
            string arrow = isOpen ? "▲" : "▼";
            mFont.tahoma_7b_dark.drawString(g, arrow, x + w - 15, y + 5, mFont.CENTER);
            
            // Vẽ caption nếu có
            if (!string.IsNullOrEmpty(caption))
            {
                mFont.tahoma_7b_dark.drawString(g, caption, x, y - 15, mFont.LEFT);
            }
            
            // Vẽ dropdown menu nếu đang mở
            if (isOpen)
            {
                int dropdownHeight = Math.Min(items.Count, maxVisibleItems) * itemHeight;
                int dropdownY = y + h;
                
                // Vẽ background của dropdown
                g.setColor(11837316);
                g.fillRect(x, dropdownY, w, dropdownHeight, 5);
                g.setColor(10587766);
                g.fillRect(x + 1, dropdownY + 1, w - 2, dropdownHeight - 2, 5);
                
                // Vẽ các item
                for (int i = 0; i < Math.Min(items.Count, maxVisibleItems); i++)
                {
                    int itemY = dropdownY + (i * itemHeight);
                    
                    // Highlight item được chọn
                    if (i == selectedIndex)
                    {
                        g.setColor(7040779);
                        g.fillRect(x + 2, itemY + 2, w - 4, itemHeight - 4, 3);
                    }
                    
                    // Vẽ text của item
                    mFont.tahoma_7b_dark.drawString(g, items[i], x + 5, itemY + 5, mFont.LEFT);
                }
            }
        }
        
        public void setSelectedIndex(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                selectedIndex = index;
                onSelectionChanged?.Invoke(selectedIndex);
            }
        }
        
        public string getSelectedItem()
        {
            return items.Count > 0 && selectedIndex < items.Count ? items[selectedIndex] : "";
        }
        
        public void addItem(string item)
        {
            items.Add(item);
        }
        
        public void removeItem(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
                if (selectedIndex >= items.Count)
                {
                    selectedIndex = Math.Max(0, items.Count - 1);
                }
            }
        }
        
        public void clearItems()
        {
            items.Clear();
            selectedIndex = 0;
        }
    }
} 