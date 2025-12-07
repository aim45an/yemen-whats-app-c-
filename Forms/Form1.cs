using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;
using YemenWhatsApp.Data;
using YemenWhatsApp.Services;
using Microsoft.EntityFrameworkCore;

namespace YemenWhatsApp
{
    public partial class Form1 : Form
    {
        private List<string> onlineUsers = new List<string> { "أحمد", "محمد", "فاطمة", "خالد" };
        private string currentUser = "";
        private bool isConnected = false;
        private ApiService apiService = new ApiService();
        private ChatDbContext dbContext;
        private string selectedFilePath;
        private Image profileImage;
        private string loginMethod;
        private bool _isLocalMode = false;

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // الحصول على طريقة تسجيل الدخول
            loginMethod = SessionManager.LoginMethod;
            currentUser = SessionManager.CurrentUsername;

            // تهيئة قاعدة البيانات
            InitializeDatabase();

            // عرض رسالة ترحيبية
            if (!string.IsNullOrEmpty(currentUser))
            {
                if (SessionManager.IsGuest)
                {
                    AddSystemMessage($"🟡 مرحباً بك كضيف: {currentUser}");
                    AddSystemMessage("⚠ ملاحظة: في الوضع المحلي - لن يتم حفظ بياناتك بشكل دائم");
                }
                else
                {
                    AddSystemMessage($"👋 مرحباً بك {currentUser}");
                }

                // تمكين الأزرار
                sendButton.Enabled = true;
                messageTextBox.Enabled = true;
                attachButton.Enabled = true;
                btnUpdateProfile.Enabled = true;
                isConnected = true;
                connectButton.Text = "❌ قطع الاتصال";
                statusLabel.Text = " متصل (محلي)";
            }

            // إعداد الحالة الأولية
            chatStatusLabel.Text = " اتصال فوري";
            appTitleLabel.Text = "🇾🇪 Yemen WhatsApp";
            chatTitleLabel.Text = "Yemen Chat Group";
            onlineCountLabel.Text = "0 متصل";

            // إعداد ألوان الحقول
            usernameTextBox.BackColor = Color.FromArgb(220, 255, 220);
            messageTextBox.BackColor = Color.White;
            serverUrlTextBox.BackColor = Color.White;

            // إعداد القوائم المنسدلة
            InitializeComboBoxes();

            // تحميل المستخدمين التجريبيين
            LoadSampleUsers();

            // معلومات التطبيق
            infoLabel.Text = @"Yemen WhatsApp Desktop
الإصدار: 2.0.0
مميزات التطبيق:
• دردشة فورية عامة وخاصة
• واجهة مشابهة للواتساب
• قاعدة بيانات SQL Server
• متعدد المستخدمين
• دعم الملفات والصور
• يعمل بدون إنترنت
• برمجة: ايمن عبدالوهاب الصالحي";

            // تهيئة ApiService
            apiService.Initialize(serverUrlTextBox.Text);

            // إعداد صورة الملف الشخصي
            SetupProfilePicture();

            // تحديث قائمة المستخدمين
            UpdateUsersList();
        }

        private async void InitializeDatabase()
        {
            try
            {
                dbContext = new ChatDbContext();
                await Task.Run(() => DatabaseHelper.InitializeDatabase());
                Console.WriteLine("✅ تم تهيئة قاعدة البيانات المحلية");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("فشل تهيئة قاعدة البيانات", ex);
            }
        }

        private void SetupProfilePicture()
        {
            // جعل صورة الملف الشخصي دائرية
            profilePictureBox.Paint += ProfilePictureBox_Paint;

            // حدث النقر لفتح إعدادات الملف الشخصي
            profilePictureBox.Click += ProfilePictureBox_Click;

            // حدث النقر بزر الماوس الأيمن
            profilePictureBox.MouseClick += ProfilePictureBox_MouseClick;

            // تحميل صورة افتراضية
            LoadDefaultProfileImage();
        }

        private void LoadDefaultProfileImage()
        {
            // صورة افتراضية إذا لم توجد صورة
            profileImage = CreateDefaultProfileImage();
            profilePictureBox.Invalidate(); // إعادة رسم
        }

        private Image CreateDefaultProfileImage()
        {
            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // خلفية دائرية
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    g.FillEllipse(brush, 0, 0, 100, 100);
                }

                // أيقونة المستخدم
                using (Font font = new Font("Segoe UI", 40))
                {
                    g.DrawString("👤", font, Brushes.White, 20, 15);
                }

                // حد دائري
                using (Pen pen = new Pen(Color.White, 3))
                {
                    g.DrawEllipse(pen, 1, 1, 98, 98);
                }
            }
            return bmp;
        }

        private void ProfilePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (profileImage != null)
            {
                // رسم صورة دائرية
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, profilePictureBox.Width - 1, profilePictureBox.Height - 1);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(profileImage, 0, 0, profilePictureBox.Width, profilePictureBox.Height);
                }

                // حد دائري
                using (Pen pen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawEllipse(pen, 0, 0, profilePictureBox.Width - 1, profilePictureBox.Height - 1);
                }
            }
        }

        private async void ProfilePictureBox_Click(object sender, EventArgs e)
        {
            if (isConnected && !string.IsNullOrEmpty(currentUser))
            {
                await ShowProfileDialog();
            }
        }

        private void ProfilePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowProfileContextMenu(e.Location);
            }
        }

        private async Task ShowProfileDialog()
        {
            try
            {
                using (var db = new ChatDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == currentUser);

                    if (user != null)
                    {
                        string message = $"👤 الملف الشخصي\n\n" +
                                        $"الاسم: {currentUser}\n" +
                                        $"الحالة: {(user.IsOnline ? "🟢 متصل" : "غير متصل")}\n" +
                                        $"آخر ظهور: {user.LastSeen:yyyy-MM-dd HH:mm}\n" +
                                        $"عدد الرسائل: {user.TotalMessagesSent}\n" +
                                        $"تاريخ التسجيل: {user.CreatedAt:yyyy-MM-dd}\n";

                        if (!string.IsNullOrEmpty(user.Bio))
                        {
                            message += $"\nنبذة: {user.Bio}";
                        }

                        MessageBox.Show(message, "الملف الشخصي",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في عرض الملف الشخصي", ex);
            }
        }

        private void ShowProfileContextMenu(Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem changePhotoItem = new ToolStripMenuItem("تغيير الصورة");
            changePhotoItem.Click += async (s, e) => await ChangeProfilePhoto();

            ToolStripMenuItem viewProfileItem = new ToolStripMenuItem("عرض الملف الشخصي");
            viewProfileItem.Click += async (s, e) => await ShowProfileDialog();

            ToolStripMenuItem updateStatusItem = new ToolStripMenuItem("تغيير الحالة");
            updateStatusItem.Click += (s, e) => UpdateUserStatus();

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("الإعدادات");
            settingsItem.Click += (s, e) => ShowSettingsDialog();

            menu.Items.AddRange(new ToolStripItem[] {
                changePhotoItem,
                viewProfileItem,
                updateStatusItem,
                new ToolStripSeparator(),
                settingsItem
            });

            menu.Show(profilePictureBox, location);
        }

        private async Task ChangeProfilePhoto()
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Title = "اختر صورة للملف الشخصي";
                    dialog.Filter = "الصور (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                    dialog.Multiselect = false;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string imagePath = dialog.FileName;

                        // التحقق من حجم الصورة
                        FileInfo fileInfo = new FileInfo(imagePath);
                        if (fileInfo.Length > 5 * 1024 * 1024) // 5 MB
                        {
                            MessageBox.Show("الصورة كبيرة جداً! الحد الأقصى 5MB", "تحذير",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // نسخ الصورة لمجلد المستخدم
                        string userFolder = LocalStorage.GetUserDataPath(currentUser);
                        string profileFolder = Path.Combine(userFolder, "Profile");

                        if (!Directory.Exists(profileFolder))
                            Directory.CreateDirectory(profileFolder);

                        string destPath = Path.Combine(profileFolder, $"profile_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(imagePath)}");
                        File.Copy(imagePath, destPath, true);

                        // تحديث الصورة
                        var result = await apiService.UpdateProfileImageAsync(currentUser, destPath);

                        if (result.Success)
                        {
                            // تحميل الصورة الجديدة
                            LoadProfileImage(destPath);

                            MessageBox.Show("تم تحديث صورة الملف الشخصي", "نجاح",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تغيير صورة الملف الشخصي", ex);
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProfileImage(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        profileImage = Image.FromStream(fs);
                        profilePictureBox.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تحميل صورة الملف الشخصي", ex);
                LoadDefaultProfileImage();
            }
        }

        private void UpdateUserStatus()
        {
            if (!isConnected) return;

            string currentStatus = SessionManager.CurrentUser?.Status ?? "متصل";
            string newStatus = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل حالتك الجديدة:", "تغيير الحالة", currentStatus);

            if (!string.IsNullOrEmpty(newStatus))
            {
                try
                {
                    var update = new UserProfileUpdate
                    {
                        Status = newStatus,
                        DisplayName = SessionManager.CurrentUser?.Name,
                        Bio = SessionManager.CurrentUser?.Bio,
                        Color = SessionManager.CurrentUser?.Color,
                        Avatar = SessionManager.CurrentUser?.Avatar
                    };

                    apiService.UpdateUserProfileAsync(currentUser, update);
                    statusLabel.Text = " " + newStatus;

                    MessageBox.Show("تم تحديث حالتك", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("خطأ في تحديث الحالة", ex);
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowSettingsDialog()
        {
            MessageBox.Show("إعدادات التطبيق:\n\n" +
                          "• إشعارات: مفعلة\n" +
                          "• الأصوات: مفعلة\n" +
                          "• الوضع: فاتح\n" +
                          "• جودة الصور: عالية\n" +
                          "• الوضع الحالي: " + (_isLocalMode ? "محلي" : "متصل بالخادم") + "\n\n" +
                          "سيتم إضافة المزيد من الإعدادات قريباً.",
                          "الإعدادات", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComboBoxes()
        {
            targetUsersComboBox.Items.Clear();
            foreach (var user in onlineUsers)
            {
                targetUsersComboBox.Items.Add(user);
            }
            if (targetUsersComboBox.Items.Count > 0)
                targetUsersComboBox.SelectedIndex = 0;
        }

        private async void LoadSampleUsers()
        {
            try
            {
                var response = await apiService.GetOnlineUsersAsync();
                if (response.Success && response.Data != null)
                {
                    usersListBox.Items.Clear();

                    foreach (var user in response.Data.Users)
                    {
                        string statusIcon = user.IsOnline ? "🟢 " : "⚫ ";
                        string itemText = $"{statusIcon}{user.Username} ({user.Status})";

                        if (user.Username == currentUser)
                            itemText += " (أنت)";

                        usersListBox.Items.Add(itemText);
                    }

                    onlineCountLabel.Text = $"{response.Data.OnlineCount} متصل";
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تحميل المستخدمين", ex);
                LoadDefaultUsers();
            }
        }

        private void LoadDefaultUsers()
        {
            usersListBox.Items.Clear();
            usersListBox.Items.Add("🟢 أحمد (متصل)");
            usersListBox.Items.Add("🟢 محمد (متصل)");
            usersListBox.Items.Add("⚫ فاطمة (غير متصل)");
            usersListBox.Items.Add("🟢 خالد (متصل)");
            usersListBox.Items.Add("⚫ سارة (غير متصل)");
            onlineCountLabel.Text = "3 متصل";
        }

        private async void UpdateUsersList()
        {
            try
            {
                var response = await apiService.GetOnlineUsersAsync();
                if (response.Success && response.Data != null)
                {
                    usersListBox.Items.Clear();

                    // إضافة المستخدم الحالي أولاً
                    if (!string.IsNullOrEmpty(currentUser))
                    {
                        usersListBox.Items.Add($"🟢 {currentUser} (أنت) (متصل)");
                    }

                    foreach (var user in response.Data.Users)
                    {
                        if (user.Username != currentUser) // تجنب تكرار المستخدم الحالي
                        {
                            string statusIcon = user.IsOnline ? "🟢 " : "⚫ ";
                            string itemText = $"{statusIcon}{user.Username} ({user.Status})";
                            usersListBox.Items.Add(itemText);
                        }
                    }

                    onlineCountLabel.Text = $"{response.Data.OnlineCount} متصل";

                    // تحديث القائمة المنسدلة للمستخدمين
                    targetUsersComboBox.Items.Clear();
                    foreach (var user in response.Data.Users)
                    {
                        if (user.Username != currentUser)
                        {
                            targetUsersComboBox.Items.Add(user.Username);
                        }
                    }
                    if (targetUsersComboBox.Items.Count > 0)
                        targetUsersComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تحديث قائمة المستخدمين", ex);
            }
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                // قطع الاتصال
                try
                {
                    await apiService.LogoutAsync();
                }
                catch { }

                isConnected = false;
                currentUser = "";
                ResetConnectionState();

                AddSystemMessage("تم قطع الاتصال");

                // إزالة المستخدم من القائمة
                for (int i = 0; i < usersListBox.Items.Count; i++)
                {
                    if (usersListBox.Items[i].ToString().Contains("(أنت)"))
                    {
                        usersListBox.Items.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                // عملية الاتصال
                currentUser = usernameTextBox.Text.Trim();

                if (string.IsNullOrEmpty(currentUser) || currentUser.Length < 3)
                {
                    MessageBox.Show("الرجاء إدخال اسم مستخدم صالح (3 أحرف على الأقل)",
                        "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // اختبار اتصال الخادم أولاً
                    apiService.Initialize(serverUrlTextBox.Text);
                    _isLocalMode = !apiService.IsServerAvailable();

                    // تسجيل الدخول
                    var authResult = await apiService.AuthenticateAsync(currentUser);

                    if (authResult.Success)
                    {
                        isConnected = true;
                        statusLabel.Text = " متصل" + (_isLocalMode ? " (محلي)" : "");
                        chatStatusLabel.Text = _isLocalMode ? " الوضع المحلي" : " متصل بالخادم";
                        connectButton.Text = "❌ قطع الاتصال";
                        sendButton.Enabled = true;
                        messageTextBox.Enabled = true;
                        attachButton.Enabled = true;
                        btnUpdateProfile.Enabled = true;
                        usernameTextBox.Enabled = false;

                        // رسالة ترحيبية
                        AddSystemMessage($" تم الاتصال بنجاح كـ {currentUser}");
                        AddSystemMessage($" الوضع: {(_isLocalMode ? "محلي (بدون خادم)" : "متصل بالخادم")}");
                        AddSystemMessage(" يمكنك الآن البدء في الدردشة!");

                        // تحديث قائمة المستخدمين
                        UpdateUsersList();

                        // تحميل الرسائل السابقة
                        await LoadMessages();

                        // تحميل صورة الملف الشخصي
                        LoadUserProfileImage();
                    }
                    else
                    {
                        MessageBox.Show(authResult.Message, "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("خطأ في الاتصال", ex);
                    MessageBox.Show($"خطأ في الاتصال: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResetConnectionState()
        {
            statusLabel.Text = " غير متصل";
            chatStatusLabel.Text = " غير متصل";
            connectButton.Text = " الاتصال";
            sendButton.Enabled = false;
            messageTextBox.Enabled = false;
            attachButton.Enabled = false;
            btnUpdateProfile.Enabled = false;
            usernameTextBox.Enabled = true;
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async Task SendMessage()
        {
            if (!isConnected || string.IsNullOrWhiteSpace(messageTextBox.Text))
                return;

            string message = messageTextBox.Text.Trim();
            bool isPrivate = privateRadioButton.Checked;
            string targetUser = isPrivate ?
                targetUsersComboBox.SelectedItem?.ToString() : "الجميع";

            if (isPrivate && string.IsNullOrEmpty(targetUser))
            {
                MessageBox.Show("الرجاء اختيار مستخدم للدردشة الخاصة",
                    "دردشة خاصة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // عرض الرسالة فوراً
            AddChatMessage(currentUser, message, DateTime.Now, true, isPrivate, targetUser);

            // إرسال الرسالة
            var result = await apiService.SendMessageAsync(message,
                isPrivate ? "private" : "public", targetUser, selectedFilePath);

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // محاكاة رد من مستخدم آخر للدردشة العامة
                if (!isPrivate && !_isLocalMode)
                {
                    Timer responseTimer = new Timer { Interval = 2000 };
                    responseTimer.Tick += (s, ev) =>
                    {
                        string[] responses = { "مرحباً!", "كيف الحال؟", "أهلاً وسهلاً", "شكراً لك" };
                        string randomResponse = responses[new Random().Next(responses.Length)];

                        this.Invoke(new Action(() =>
                        {
                            AddChatMessage("أحمد", randomResponse, DateTime.Now, false, false, "الجميع");
                        }));

                        ((Timer)s).Stop();
                        ((Timer)s).Dispose();
                    };
                    responseTimer.Start();
                }
            }

            // مسح حقل النص
            messageTextBox.Clear();
            messageTextBox.Focus();
            selectedFilePath = null;
        }

        private void AddChatMessage(string sender, string message, DateTime time, bool isMe, bool isPrivate, string targetUser = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, string, DateTime, bool, bool, string>(
                    AddChatMessage), sender, message, time, isMe, isPrivate, targetUser);
                return;
            }

            // إنشاء حاوية الرسالة
            Panel container = new Panel
            {
                Margin = new Padding(10, 8, 10, 8),
                Width = messagesFlowPanel.ClientSize.Width - 30,
                BackColor = Color.Transparent
            };

            // فقاعة الرسالة
            Panel bubble = new Panel
            {
                MaximumSize = new Size(500, 0),
                AutoSize = true,
                Padding = new Padding(15, 12, 15, 35)
            };

            // تحديد لون الفقاعة
            if (isMe)
            {
                bubble.BackColor = Color.FromArgb(220, 248, 198); // لون فاتح أخضر
            }
            else if (isPrivate)
            {
                bubble.BackColor = Color.FromArgb(255, 245, 220); // لون بيج فاتح
            }
            else
            {
                bubble.BackColor = Color.White;
            }

            // نص الرسالة
            Label lblText = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                AutoSize = true,
                MaximumSize = new Size(450, 0)
            };

            // وقت الرسالة
            Label lblTime = new Label
            {
                Text = time.ToString("hh:mm tt"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            // حساب المواقع
            bubble.Controls.Add(lblText);
            lblTime.Location = new Point(15, lblText.Height + 5);
            bubble.Controls.Add(lblTime);
            bubble.Height = lblText.Height + 40;
            bubble.Width = Math.Min(lblText.Width + 30, 500);

            // تحديد موقع الفقاعة
            if (isMe)
            {
                container.Dock = DockStyle.Right;
                bubble.Location = new Point(container.Width - bubble.Width - 10, 0);

                // إضافة علامة تسليم صغيرة
                Label lblDelivered = new Label
                {
                    Text = "✓✓",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.Gray,
                    AutoSize = true
                };
                lblDelivered.Location = new Point(bubble.Width - 25, lblText.Height + 7);
                bubble.Controls.Add(lblDelivered);
            }
            else
            {
                container.Dock = DockStyle.Left;
                bubble.Location = new Point(10, 0);

                // إضافة اسم المرسل للرسائل الخاصة
                if (isPrivate)
                {
                    Label lblSender = new Label
                    {
                        Text = $"{sender} ↙",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 120, 215),
                        AutoSize = true
                    };
                    lblSender.Location = new Point(15, -18);
                    container.Controls.Add(lblSender);
                    container.Height = bubble.Height + 20;
                }
                else
                {
                    // إضافة اسم المرسل للدردشة العامة
                    Label lblSender = new Label
                    {
                        Text = sender,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 120, 215),
                        AutoSize = true
                    };
                    lblSender.Location = new Point(15, -18);
                    container.Controls.Add(lblSender);
                    container.Height = bubble.Height + 20;
                }
            }

            // إضافة حواف مستديرة
            bubble.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawRoundedRectangle(e.Graphics,
                    new Rectangle(0, 0, bubble.Width - 1, bubble.Height - 1),
                    15, bubble.BackColor);
            };

            container.Controls.Add(bubble);
            if (!isPrivate && !isMe) container.Height = bubble.Height + 20;
            messagesFlowPanel.Controls.Add(container);
            ScrollToBottom();
        }

        private async void LoadUserProfileImage()
        {
            if (!string.IsNullOrEmpty(currentUser) && isConnected)
            {
                try
                {
                    string imagePath = await apiService.GetProfileImageAsync(currentUser);

                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        LoadProfileImage(imagePath);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("خطأ في تحميل صورة المستخدم", ex);
                }
            }
        }

        private async Task LoadMessages()
        {
            try
            {
                messagesFlowPanel.Controls.Clear();

                bool isPrivate = privateRadioButton.Checked;
                string targetUser = isPrivate ? targetUsersComboBox.SelectedItem?.ToString() : null;

                var response = await apiService.GetMessagesAsync(
                    isPrivate ? "private" : "public",
                    targetUser);

                if (response.Success && response.Data != null)
                {
                    foreach (var message in response.Data.Messages.OrderBy(m => m.Timestamp))
                    {
                        bool isMe = message.Sender == currentUser;
                        bool isPrivateMsg = message.IsPrivate;

                        AddChatMessage(
                            message.Sender,
                            message.Content,
                            message.Timestamp,
                            isMe,
                            isPrivateMsg,
                            message.Receiver);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في جلب الرسائل", ex);
            }
        }

        private void AddSystemMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AddSystemMessage), message);
                return;
            }

            Label lbl = new Label
            {
                Text = $"• {message} •",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(245, 245, 245),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 10, 0, 10),
                Dock = DockStyle.Top
            };
            messagesFlowPanel.Controls.Add(lbl);
            ScrollToBottom();
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle bounds, int radius, Color fillColor)
        {
            using (SolidBrush brush = new SolidBrush(fillColor))
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
                path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }

        private void ScrollToBottom()
        {
            if (messagesFlowPanel.Controls.Count > 0)
            {
                messagesFlowPanel.ScrollControlIntoView(
                    messagesFlowPanel.Controls[messagesFlowPanel.Controls.Count - 1]);
            }
        }

        // ========== باقي الدوال (بدون تغيير) ==========

        private void usernameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (usernameTextBox.Text.Length >= 3)
            {
                usernameTextBox.BackColor = Color.FromArgb(220, 255, 220);
            }
            else
            {
                usernameTextBox.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        private void messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter &&
                !string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                if (Control.ModifierKeys != Keys.Shift)
                {
                    SendMessage();
                    e.Handled = true;
                }
            }
        }

        private void privateRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            targetUsersComboBox.Visible = privateRadioButton.Checked;

            if (privateRadioButton.Checked && targetUsersComboBox.SelectedItem != null)
            {
                chatTitleLabel.Text = $"الدردشة مع {targetUsersComboBox.SelectedItem}";
                LoadMessages();
            }
            else
            {
                chatTitleLabel.Text = "Yemen Chat Group";
                LoadMessages();
            }
        }

        private void targetUsersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (privateRadioButton.Checked && targetUsersComboBox.SelectedItem != null)
            {
                chatTitleLabel.Text = $"الدردشة مع {targetUsersComboBox.SelectedItem}";
                LoadMessages();
            }
        }

        private void usersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (usersListBox.SelectedItem != null && isConnected)
            {
                string selectedUser = usersListBox.SelectedItem.ToString()
                    .Replace("🟢 ", "")
                    .Replace("⚫ ", "")
                    .Replace(" (أنت)", "")
                    .Split('(')[0].Trim();

                if (selectedUser != currentUser && !selectedUser.Contains("أنت"))
                {
                    privateRadioButton.Checked = true;
                    if (targetUsersComboBox.Items.Contains(selectedUser))
                    {
                        targetUsersComboBox.SelectedItem = selectedUser;
                    }
                }
            }
        }

        private void messagesFlowPanel_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 245, 240)))
            {
                e.Graphics.FillRectangle(brush, messagesFlowPanel.ClientRectangle);
            }
        }

        private void chatHeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 150, 136)))
            {
                e.Graphics.FillRectangle(brush, chatHeaderPanel.ClientRectangle);
            }
        }

        private void topPanel_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 150, 136)))
            {
                e.Graphics.FillRectangle(brush, topPanel.ClientRectangle);
            }
        }

        private void serverUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            if (IsValidUrl(serverUrlTextBox.Text))
            {
                serverUrlTextBox.BackColor = Color.FromArgb(220, 255, 220);
            }
            else
            {
                serverUrlTextBox.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void attachButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Title = "اختر ملف للمرفق";
                    dialog.Filter = GetFileFilters();
                    dialog.Multiselect = false;
                    dialog.InitialDirectory =
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        ProcessSelectedFile(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetFileFilters()
        {
            return "الصور (.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                   "مستندات PDF (.pdf)|*.pdf|" +
                   "مستندات Word (.doc, *.docx)|*.doc;*.docx|" +
                   "ملفات Excel (.xls, *.xlsx)|*.xls;*.xlsx|" +
                   "كل الملفات (.*)|*.*";
        }

        private void ProcessSelectedFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLower();
            long fileSize = new FileInfo(filePath).Length;

            // التحقق من حجم الملف (مثال: 10MB كحد أقصى)
            if (fileSize > 10 * 1024 * 1024)
            {
                MessageBox.Show("الملف كبير جدًا! الحد الأقصى 10MB",
                    "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // إضافة رسالة بنجاح الاختيار
            AddSystemMessage($"✅ تم اختيار الملف: {fileName} ({FormatFileSize(fileSize)})");

            // حفظ مسار الملف
            selectedFilePath = filePath;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "بايت", "كيلوبايت", "ميجابايت", "جيجابايت" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private async void btnUpdateProfile_Click(object sender, EventArgs e)
        {
            if (!isConnected || string.IsNullOrEmpty(currentUser))
            {
                MessageBox.Show("يجب الاتصال أولاً لتحديث الملف الشخصي",
                    "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ShowProfileUpdateDialog();
        }

        private async Task ShowProfileUpdateDialog()
        {
            try
            {
                using (var db = new ChatDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == currentUser);

                    if (user != null)
                    {
                        using (Form dialog = new Form())
                        {
                            dialog.Text = "تحديث الملف الشخصي";
                            dialog.Size = new Size(400, 500);
                            dialog.StartPosition = FormStartPosition.CenterParent;
                            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                            dialog.MaximizeBox = false;
                            dialog.MinimizeBox = false;

                            // التحكمات
                            TextBox txtDisplayName = new TextBox
                            {
                                Text = user.Name ?? user.Username,
                                Location = new Point(20, 50),
                                Size = new Size(340, 25)
                            };

                            TextBox txtBio = new TextBox
                            {
                                Text = user.Bio ?? "",
                                Location = new Point(20, 100),
                                Size = new Size(340, 80),
                                Multiline = true
                            };

                            TextBox txtStatus = new TextBox
                            {
                                Text = user.Status,
                                Location = new Point(20, 200),
                                Size = new Size(340, 25)
                            };

                            Button btnChangePhoto = new Button
                            {
                                Text = "تغيير الصورة",
                                Location = new Point(20, 250),
                                Size = new Size(150, 30)
                            };

                            Button btnSave = new Button
                            {
                                Text = "حفظ",
                                Location = new Point(200, 400),
                                Size = new Size(80, 30),
                                DialogResult = DialogResult.OK
                            };

                            Button btnCancel = new Button
                            {
                                Text = "إلغاء",
                                Location = new Point(290, 400),
                                Size = new Size(80, 30),
                                DialogResult = DialogResult.Cancel
                            };

                            // الأحداث
                            btnChangePhoto.Click += async (s, e) =>
                            {
                                await ChangeProfilePhoto();
                                dialog.Close();
                            };

                            // إضافة التحكمات
                            dialog.Controls.Add(new Label { Text = "الاسم:", Location = new Point(20, 30) });
                            dialog.Controls.Add(txtDisplayName);
                            dialog.Controls.Add(new Label { Text = "نبذة:", Location = new Point(20, 80) });
                            dialog.Controls.Add(txtBio);
                            dialog.Controls.Add(new Label { Text = "الحالة:", Location = new Point(20, 180) });
                            dialog.Controls.Add(txtStatus);
                            dialog.Controls.Add(btnChangePhoto);
                            dialog.Controls.Add(btnSave);
                            dialog.Controls.Add(btnCancel);

                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                var update = new UserProfileUpdate
                                {
                                    DisplayName = txtDisplayName.Text,
                                    Bio = txtBio.Text,
                                    Status = txtStatus.Text,
                                    Color = user.Color
                                };

                                var result = await apiService.UpdateUserProfileAsync(currentUser, update);

                                if (result.Success)
                                {
                                    MessageBox.Show("تم تحديث الملف الشخصي", "نجاح",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show(result.Message, "خطأ",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تحديث الملف الشخصي", ex);
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnViewStats_Click(object sender, EventArgs e)
        {
            if (!isConnected || string.IsNullOrEmpty(currentUser))
            {
                MessageBox.Show("يجب الاتصال أولاً لعرض الإحصائيات",
                    "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ShowUserStatistics();
        }

        private async Task ShowUserStatistics()
        {
            try
            {
                var result = await apiService.GetUserStatisticsAsync(currentUser);

                if (result.Success && result.Data != null)
                {
                    string stats = $"📊 إحصائيات المستخدم\n\n" +
                                  $"👤 الاسم: {result.Data.Username}\n" +
                                  $"📨 الرسائل المرسلة: {result.Data.TotalMessagesSent}\n" +
                                  $"📩 الرسائل المستلمة: {result.Data.TotalMessagesReceived}\n" +
                                  $"📎 الملفات المرسلة: {result.Data.TotalFilesSent}\n" +
                                  $"🔢 عدد مرات الدخول: {result.Data.LoginCount}\n" +
                                  $"🕒 آخر دخول: {result.Data.LastLogin:yyyy-MM-dd HH:mm}\n" +
                                  $"📅 تاريخ التسجيل: {result.Data.CreatedAt:yyyy-MM-dd}\n" +
                                  $"🟢 الحالة: {(result.Data.IsOnline ? "متصل" : "غير متصل")}\n" +
                                  $"👁️ آخر ظهور: {result.Data.LastSeen:HH:mm}";

                    MessageBox.Show(stats, "إحصائيات المستخدم",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في عرض الإحصائيات", ex);
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.splitContainer1.SplitterDistance = 300;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // تسجيل الخروج عند إغلاق النافذة
                if (isConnected)
                {
                    apiService.LogoutAsync();
                }
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}