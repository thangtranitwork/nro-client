using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomGraphics
{
    class NumbericSlider
    {
        public int x, y, w, h;
        public int value;
        public int minValue;
        public int maxValue;
        public Action<int> onValueChanged;
        public string caption;
        public bool isDragging;
        public int sliderWidth = 20;
        public int trackHeight = 8;
        
        public NumbericSlider(int x, int y, int w, int h, int value, int minValue, int maxValue, Action<int> onValueChanged, string caption = "")
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
            this.isDragging = false;
        }
        
        public void updateKey()
        {
            int trackY = y + h / 2 - trackHeight / 2;
            int sliderX = getSliderX();
           
            // Xử lý click vào track để nhảy đến vị trí đó
            if (GameCanvas.isPointerHoldIn(x, trackY, w, trackHeight))
            {
                if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
                {
                    GameCanvas.clearAllPointerEvent();
                    try
                    {
                        int clickX = GameCanvas.pxLast;
                        int newValue = getValueFromX(clickX);
                        setValue(newValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else if (GameCanvas.isPointerDown)
                {
                    try
                    {
                        int clickX = GameCanvas.px;
                        int newValue = getValueFromX(clickX);
                        setValue(newValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
        
        private int getSliderX()
        {
            float ratio = (float)(value - minValue) / (maxValue - minValue);
            return x + (int)(ratio * (w - sliderWidth));
        }
        
        private int getValueFromX(int xPos)
        {
            // Đảm bảo xPos nằm trong phạm vi hợp lệ
            xPos = Math.Max(x, Math.Min(x + w - sliderWidth, xPos));
            
            float ratio = (float)(xPos - x) / (w - sliderWidth);
            ratio = Math.Max(0, Math.Min(1, ratio));
            
            int newValue = minValue + (int)(ratio * (maxValue - minValue));
            
            // Đảm bảo giá trị nằm trong phạm vi
            return Math.Max(minValue, Math.Min(maxValue, newValue));
        }
        
        public void paint(mGraphics g)
        {
            int trackY = y + h / 2 - trackHeight / 2;
            int sliderX = getSliderX();
            
            // Vẽ track (đường trượt)
            g.setColor(11837316);
            g.fillRect(x, trackY, w, trackHeight, trackHeight / 2);
            g.setColor(10587766);
            g.fillRect(x + 1, trackY + 1, w - 2, trackHeight - 2, trackHeight / 2);
            
            // Vẽ phần đã chọn của track
            if (value > minValue)
            {
                int filledWidth = sliderX - x;
                g.setColor(7040779);
                g.fillRect(x, trackY, filledWidth, trackHeight, trackHeight / 2);
            }
            
            // Vẽ slider (nút trượt)
            g.setColor(isDragging ? 7040779 : 11837316);
            g.fillRect(sliderX, y, sliderWidth, h, 5);
            g.setColor(isDragging ? 9993045 : 10587766);
            g.fillRect(sliderX + 1, y + 1, sliderWidth - 2, h - 2, 5);
            
            // Vẽ giá trị hiện tại
            string valueText = value.ToString();
            mFont.tahoma_7b_dark.drawString(g, valueText, x + w / 2, y + h-3, mFont.CENTER);
            
            // Vẽ caption nếu có
            if (!string.IsNullOrEmpty(caption))
            {
                mFont.tahoma_7b_dark.drawString(g, caption, x, y - 5, mFont.LEFT);
            }
            
            // Vẽ min/max values
            mFont.tahoma_7b_dark.drawString(g, minValue.ToString(), x, y + h -3, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, maxValue.ToString(), x + w, y + h-3 , mFont.RIGHT);
        }
        
        public void setValue(int newValue)
        {
            int oldValue = value;
            value = Math.Max(minValue, Math.Min(maxValue, newValue));
            
            if (value != oldValue)
            {
                Debug.Log($"Slider value changed: {oldValue} -> {value}");
                onValueChanged?.Invoke(value);
            }
        }
        
        public int getValue()
        {
            return value;
        }
        
        public void setRange(int min, int max)
        {
            minValue = min;
            maxValue = max;
            value = Math.Max(minValue, Math.Min(maxValue, value));
        }
        
        public float getPercentage()
        {
            return (float)(value - minValue) / (maxValue - minValue);
        }
    }
} 