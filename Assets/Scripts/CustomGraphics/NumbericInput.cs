using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomGraphics
{
    class NumbericInput
    {
        public int x, y, w, h;
        public int value;
        public int minValue;
        public int maxValue;
        public Action<int> onValueChanged;
        public string caption;
        public bool isEditing;
        public string editText;
        public int cursorPosition;
        public int blinkTimer;
        
        public NumbericInput(int x, int y, int w, int h, int value, int minValue, int maxValue, Action<int> onValueChanged, string caption = "")
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.value = Math.Max(minValue, Math.Min(maxValue, value));
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.onValueChanged = onValueChanged;
            this.caption = caption;
            this.isEditing = false;
            this.editText = value.ToString();
            this.cursorPosition = editText.Length;
            this.blinkTimer = 0;
        }
        
        public void updateKey()
        {
            // Xử lý click vào input
            if (GameCanvas.isPointerHoldIn(x, y, w, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
            {
                GameCanvas.clearAllPointerEvent();
                try
                {
                    isEditing = true;
                    editText = value.ToString();
                    cursorPosition = editText.Length;
                    blinkTimer = 0;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            // Xử lý input khi đang edit
            if (isEditing)
            {
                // Xử lý phím FIRE (Enter) để xác nhận
                if (GameCanvas.keyPressed[KeyConstant.KEY_FIRE])
                {
                    confirmEdit();
                }
                
                // Xử lý phím BACK (Escape) để hủy
                if (GameCanvas.keyPressed[KeyConstant.KEY_BACK])
                {
                    cancelEdit();
                }
                
                // Xử lý phím CLEAR (Backspace)
                if (GameCanvas.keyPressed[KeyConstant.KEY_CLEAR])
                {
                    if (cursorPosition > 0)
                    {
                        editText = editText.Remove(cursorPosition - 1, 1);
                        cursorPosition--;
                    }
                }
                
                // Xử lý phím mũi tên trái/phải
                if (GameCanvas.keyPressed[KeyConstant.KEY_LEFT])
                {
                    cursorPosition = Math.Max(0, cursorPosition - 1);
                }
                if (GameCanvas.keyPressed[KeyConstant.KEY_RIGHT])
                {
                    cursorPosition = Math.Min(editText.Length, cursorPosition + 1);
                }
                
                // Xử lý input số
                for (int i = (int)KeyConstant.KEY_NUM0; i <= (int)KeyConstant.KEY_NUM9; i++)
                {
                    if (GameCanvas.keyPressed[i])
                    {
                        char digit = (char)('0' + (i - (int)KeyConstant.KEY_NUM0));
                        editText = editText.Insert(cursorPosition, digit.ToString());
                        cursorPosition++;
                    }
                }
                
                // Click bên ngoài để xác nhận
                if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                {
                    if (!GameCanvas.isPointerHoldIn(x, y, w, h))
                    {
                        confirmEdit();
                    }
                }
            }
            
            // Cập nhật blink timer
            blinkTimer = (blinkTimer + 1) % 60;
        }
        
        private void confirmEdit()
        {
            try
            {
                int newValue = int.Parse(editText);
                if (newValue >= minValue && newValue <= maxValue)
                {
                    value = newValue;
                    onValueChanged?.Invoke(value);
                }
                else
                {
                    editText = value.ToString();
                }
            }
            catch
            {
                editText = value.ToString();
            }
            isEditing = false;
        }
        
        private void cancelEdit()
        {
            editText = value.ToString();
            isEditing = false;
        }
        
        public void paint(mGraphics g)
        {
            // Vẽ background
            g.setColor(11837316);
            g.fillRect(x, y, w, h, 5);
            g.setColor(10587766);
            g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
            
            // Vẽ border khi đang edit
            if (isEditing)
            {
                g.setColor(7040779);
                g.drawRect(x, y, w, h);
            }
            
            // Vẽ text
            string displayText = isEditing ? editText : value.ToString();
            mFont.tahoma_7b_dark.drawString(g, displayText, x + 5, y + 5, mFont.LEFT);
            
            // Vẽ cursor khi đang edit
            if (isEditing && blinkTimer < 30)
            {
                int cursorX = x + 5 + mFont.tahoma_7b_dark.getWidth(displayText.Substring(0, cursorPosition));
                g.setColor(0);
                g.drawLine(cursorX, y + 3, cursorX, y + h - 3);
            }
            
            // Vẽ caption nếu có
            if (!string.IsNullOrEmpty(caption))
            {
                mFont.tahoma_7b_dark.drawString(g, caption, x, y - 15, mFont.LEFT);
            }
            
            // Vẽ min/max values
            string rangeText = $"[{minValue} - {maxValue}]";
            mFont.tahoma_7b_dark.drawString(g, rangeText, x + w + 5, y + 5, mFont.LEFT);
        }
        
        public void setValue(int newValue)
        {
            value = Math.Max(minValue, Math.Min(maxValue, newValue));
            editText = value.ToString();
            onValueChanged?.Invoke(value);
        }
        
        public int getValue()
        {
            return value;
        }
    }
} 