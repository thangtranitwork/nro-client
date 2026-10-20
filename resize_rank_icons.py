"""
Resize rank icons to match setting.png size
Run: python resize_rank_icons.py
"""

from PIL import Image
import os

# Paths
mainImage = r"g:\HUNR_Client\Assets\Resources\res\x4\mainImage"

# Rank icon files
rank_icons = [
    "dong.png",
    "sat.png", 
    "bac.png",
    "vang.png",
    "bach-kim.png",
    "luc-bao.png",
    "cao-thu.png",
    "dai-cao-thu.png"
]

# Get target size from setting.png
setting_path = os.path.join(mainImage, "setting.png")
with Image.open(setting_path) as setting_img:
    target_size = setting_img.size
    print(f"Setting icon size: {target_size}")

# Resize each rank icon
for icon_name in rank_icons:
    icon_path = os.path.join(mainImage, icon_name)
    if os.path.exists(icon_path):
        with Image.open(icon_path) as img:
            original_size = img.size
            # Resize to match setting icon
            resized = img.resize(target_size, Image.Resampling.LANCZOS)
            resized.save(icon_path)
            print(f"Resized {icon_name}: {original_size} -> {target_size}")
    else:
        print(f"Not found: {icon_name}")

print("\nDone! All rank icons resized.")
