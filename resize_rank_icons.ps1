# PowerShell script to resize rank icons to match setting.png size
# Run: .\resize_rank_icons.ps1

Add-Type -AssemblyName System.Drawing

$mainImagePath = "g:\HUNR_Client\Assets\Resources\res\x4\mainImage"

# Rank icon files
$rankIcons = @(
    "dong.png",
    "sat.png", 
    "bac.png",
    "vang.png",
    "bach-kim.png",
    "luc-bao.png",
    "cao-thu.png",
    "dai-cao-thu.png"
)

# Get target size from setting.png
$settingPath = Join-Path $mainImagePath "setting.png"
$settingImg = [System.Drawing.Image]::FromFile($settingPath)
$targetWidth = $settingImg.Width
$targetHeight = $settingImg.Height
Write-Host "Setting icon size: $targetWidth x $targetHeight"
$settingImg.Dispose()

# Resize each rank icon
foreach ($iconName in $rankIcons) {
    $iconPath = Join-Path $mainImagePath $iconName
    
    if (Test-Path $iconPath) {
        # Load original image
        $img = [System.Drawing.Image]::FromFile($iconPath)
        $originalWidth = $img.Width
        $originalHeight = $img.Height
        
        # Create resized bitmap
        $resized = New-Object System.Drawing.Bitmap($targetWidth, $targetHeight)
        $graphics = [System.Drawing.Graphics]::FromImage($resized)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.DrawImage($img, 0, 0, $targetWidth, $targetHeight)
        
        # Dispose original to release file lock
        $img.Dispose()
        $graphics.Dispose()
        
        # Save resized image
        $resized.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $resized.Dispose()
        
        Write-Host "Resized $iconName : $originalWidth x $originalHeight -> $targetWidth x $targetHeight"
    } else {
        Write-Host "Not found: $iconName"
    }
}

Write-Host "`nDone! All rank icons resized to match setting.png"
