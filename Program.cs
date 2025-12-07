using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YemenWhatsApp.Data;
using YemenWhatsApp.Services;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace YemenWhatsApp
{
    internal static class Program
    {
        private static Mutex _appMutex;
        private static IHost _host;

        [STAThread]
        static void Main()
        {
            // منع تشغيل أكثر من نسخة
            _appMutex = new Mutex(true, "YemenWhatsApp_2.0", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show(
                    "❗ التطبيق يعمل بالفعل!\nيمكنك فتح نافذة واحدة فقط من Yemen WhatsApp.",
                    "Yemen WhatsApp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            try
            {
                // إنشاء واستضافة التطبيق
                _host = CreateHostBuilder().Build();

                // تهيئة التطبيق
                InitializeApplication();

                // تشغيل نافذة تسجيل الدخول أولاً
                Application.Run(_host.Services.GetRequiredService<Form2>());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ خطأ فادح في بدء التطبيق:\n{ex.Message}",
                    "خطأ فادح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _appMutex?.ReleaseMutex();
                _appMutex?.Dispose();
                _host?.Dispose();
            }
        }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // تسجيل DbContext
                    services.AddDbContext<ChatDbContext>(options =>
                        options.UseSqlServer(GetConnectionString()));

                    // تسجيل الخدمات
              

                    // تسجيل النماذج
                    services.AddTransient<Form2>(); // نافذة تسجيل الدخول أولاً
                    services.AddTransient<Form1>(); // النافذة الرئيسية

                    // تهيئة الجلسة
                    services.AddTransient(provider =>
                    {
                        SessionManager.Initialize();
                        return SessionManager.Instance;
                    });
                });
        }

        static string GetConnectionString()
        {
            // خيارات متعددة للاتصال بقاعدة البيانات
            string[] connectionStrings =
            {
                @"Server=(localdb)\MSSQLLocalDB;Database=YemenChatDB;Trusted_Connection=True;TrustServerCertificate=True;",
                @"Server=.\SQLEXPRESS;Database=YemenChatDB;Trusted_Connection=True;TrustServerCertificate=True;",
                @"Server=localhost;Database=YemenChatDB;Trusted_Connection=True;TrustServerCertificate=True;",
                @"Server=DESKTOP-2U7RVGF;Database=YemenChatDB;Trusted_Connection=True;TrustServerCertificate=True;"
            };

            // محاولة الاتصال بكل خيار
            foreach (var connectionString in connectionStrings)
            {
                try
                {
                    using (var context = new ChatDbContext())
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);

                        var tempContext = new ChatDbContext(optionsBuilder.Options);
                        if (tempContext.Database.CanConnect())
                        {
                            return connectionString;
                        }
                    }
                }
                catch
                {
                    // الاستمرار في المحاولة مع الخيار التالي
                }
            }

            // استخدام LocalDB افتراضياً
            return connectionStrings[0];
        }

        static void InitializeApplication()
        {
            
                try
                {
                    // إنشاء المجلدات الأساسية فقط
                    Task.Run(() => CreateEssentialFolders());

                    // تهيئة الجلسة (فقط تحميل الإعدادات)
                    SessionManager.Initialize();

                    // تسجيل بدء التشغيل
                    ErrorHandler.LogInfo("🔥 التطبيق بدأ في الوضع السريع");

                    Console.WriteLine("✅ تم التهيئة السريعة");
                }
                catch
                {
                    // تجاهل الأخطاء للسرعة
                }
            }

            static void CreateEssentialFolders()
            {
                try
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string appFolder = Path.Combine(appDataPath, "YemenWhatsApp");

                    if (!Directory.Exists(appFolder))
                    {
                        Directory.CreateDirectory(appFolder);

                        // المجلدات الأساسية فقط
                        Directory.CreateDirectory(Path.Combine(appFolder, "Data"));
                        Directory.CreateDirectory(Path.Combine(appFolder, "Users"));
                    }
                }
                catch { }
            
            try
            {
                // إنشاء مجلدات التطبيق
                CreateApplicationFolders();

                // تهيئة قاعدة البيانات
                DatabaseHelper.InitializeDatabase();

                // تهيئة إعدادات التطبيق
                InitializeSettings();

                // تنظيف السجلات القديمة
                ErrorHandler.ClearLogs(7);

                // تنظيف الملفات القديمة
                LocalStorage.CleanupOldFiles(30);

                Console.WriteLine("✅ تم تهيئة التطبيق بنجاح");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("فشل تهيئة التطبيق", ex);
            }
        }

        static void CreateApplicationFolders()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "YemenWhatsApp");

                // المجلدات المطلوبة
                string[] folders =
                {
                    appFolder,
                    Path.Combine(appFolder, "Data"),
                    Path.Combine(appFolder, "Logs"),
                    Path.Combine(appFolder, "Backups"),
                    Path.Combine(appFolder, "Users"),
                    Path.Combine(appFolder, "Temp"),
                    Path.Combine(appFolder, "Media"),
                    Path.Combine(appFolder, "Profiles")
                };

                foreach (string folder in folders)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("فشل إنشاء مجلدات التطبيق", ex);
            }
        }

        static void InitializeSettings()
        {
            try
            {
                // إعدادات التطبيق الأساسية
                LocalStorage.SaveSetting("AppVersion", "2.0.0");
                LocalStorage.SaveSetting("FirstRun", DateTime.Now.ToString());
                LocalStorage.SaveSetting("Theme", "light");
                LocalStorage.SaveSetting("Language", "ar");
                LocalStorage.SaveSetting("Notifications", "true");
                LocalStorage.SaveSetting("Sounds", "true");
                LocalStorage.SaveSetting("AutoConnect", "false");
                LocalStorage.SaveSetting("RememberMe", "true");

                // إعدادات العرض
                LocalStorage.SaveSetting("WindowWidth", "450");
                LocalStorage.SaveSetting("WindowHeight", "550");
                LocalStorage.SaveSetting("WindowState", "Normal");

                // إعدادات الدردشة
                LocalStorage.SaveSetting("AutoDownload", "true");
                LocalStorage.SaveSetting("MaxFileSize", "10"); // MB

                Console.WriteLine("✅ تم تهيئة الإعدادات بنجاح");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("فشل تهيئة الإعدادات", ex);
            }
        }
    }
}