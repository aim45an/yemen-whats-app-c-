@echo off
echo بناء Yemen WhatsApp Desktop...

REM تنظيف الحلقة السابقة
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM البناء
dotnet restore
dotnet build -c Release

if %errorlevel% neq 0 (
    echo ❌ فشل البناء
    pause
    exit /b 1
)

REM نسخ الملفات
echo 📦 نسخ ملفات التطبيق...
if not exist "Dist" mkdir "Dist"
xcopy "bin\Release\net6.0-windows\*.*" "Dist\" /E /Y

REM إنشاء ملف التثبيت
echo 🚀 إنشاء حزمة التثبيت...
powershell Compress-Archive -Path "Dist\*" -DestinationPath "YemenWhatsApp_v2.0.0.zip" -Force

echo ✅ تم البناء بنجاح!
echo 📁 الملف الناتج: YemenWhatsApp_v2.0.0.zip
pause