using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using YemenWhatsApp.Data;
using YemenWhatsApp.Services;

namespace YemenWhatsApp
{
    public partial class Form2 : Form
    {
        // الخدمات
        private ApiService apiService;
        private bool isLoading = false;
        private bool quickModeEnabled = true; // تمكين الوضع السريع

        public Form2()
        {
            InitializeComponent();
            InitializeServices();
            InitializeForm();
            LoadSavedCredentials();
            SetupEvents();
            CheckQuickLoginAvailability();
        }

        private void InitializeServices()
        {
            apiService = new ApiService();
            SessionManager.Initialize();
            ErrorHandler.LogInfo("تم تشغيل نافذة تسجيل الدخول");
        }

        private void InitializeForm()
        {
            // إعداد الشعار
            picLogo.Image = CreateAppLogo();

            // إعداد الأزرار
            SetupButtons();

            // إعداد الحقول
            SetupTextFields();

            // التركيز على حقل اسم المستخدم
            txtUsername.Focus();

            // إخفاء شريط التقدم
            progressBar.Visible = false;

            // تحديث حالة الاتصال في الخلفية
            _ = UpdateConnectionStatus();
        }

        private Image CreateAppLogo()
        {
            Bitmap bmp = new Bitmap(150, 150);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                // خلفية دائرية متدرجة
                using (System.Drawing.Drawing2D.LinearGradientBrush brush =
                    new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Point(0, 0), new Point(150, 150),
                        Color.FromArgb(0, 150, 136), Color.FromArgb(0, 120, 215)))
                {
                    g.FillEllipse(brush, 0, 0, 150, 150);
                }

                // أيقونة الدردشة
                using (Font font = new Font("Segoe UI Emoji", 60))
                {
                    g.DrawString("💬", font, Brushes.White, 35, 30);
                }

                // العلم اليمني
                using (Pen pen = new Pen(Color.Red, 4))
                {
                    g.DrawLine(pen, 110, 20, 140, 20);
                    g.DrawLine(pen, 125, 10, 125, 30);
                }

                // حد دائري
                using (Pen pen = new Pen(Color.White, 4))
                {
                    g.DrawEllipse(pen, 2, 2, 146, 146);
                }
            }
            return bmp;
        }

        private void SetupButtons()
        {
            // زر تسجيل الدخول
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 195);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 175);

            // زر الدخول كضيف
            btnGuest.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnGuest.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 235, 235);

            // زر إنشاء حساب جديد
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatAppearance.MouseOverBackColor = Color.FromArgb(66, 165, 70);

            // زر الإعدادات
            btnSettings.FlatAppearance.BorderSize = 0;

            // زر الخروج
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);

            // زر الدخول السريع
            btnQuickLogin.FlatAppearance.BorderSize = 0;
            btnQuickLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 213, 79);
        }

        private void SetupTextFields()
        {
            // حقل اسم المستخدم
            txtUsername.Enter += (s, e) => txtUsername.BackColor = Color.White;
            txtUsername.Leave += (s, e) => txtUsername.BackColor = Color.FromArgb(248, 248, 248);
            txtUsername.TextChanged += TxtUsername_TextChanged;

            // حقل كلمة المرور
            txtPassword.Enter += (s, e) => txtPassword.BackColor = Color.White;
            txtPassword.Leave += (s, e) => txtPassword.BackColor = Color.FromArgb(248, 248, 248);
        }

        private void SetupEvents()
        {
            // أحداث الأزرار
            btnLogin.Click += async (s, e) => await NormalLogin();
            btnGuest.Click += BtnGuest_Click;
            btnRegister.Click += BtnRegister_Click;
            btnSettings.Click += BtnSettings_Click;
            btnExit.Click += (s, e) => Application.Exit();
            btnQuickLogin.Click += BtnQuickLogin_Click;

            // أحداث الروابط
            linkForgotPassword.LinkClicked += LinkForgotPassword_LinkClicked;
            linkHelp.LinkClicked += LinkHelp_LinkClicked;

            // أحداث القائمة المنسدلة
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;

            // أحداث لوحة المفاتيح
            txtUsername.KeyPress += TxtUsername_KeyPress;
            txtPassword.KeyPress += TxtPassword_KeyPress;

            // أحداث التركيز
            txtUsername.Enter += (s, e) => ShowHint("أدخل اسم المستخدم (3 أحرف على الأقل)");
            txtPassword.Enter += (s, e) => ShowHint("كلمة المرور اختيارية في هذا الإصدار");
        }

        private void LoadSavedCredentials()
        {
            try
            {
                // تحميل اسم المستخدم المحفوظ
                string lastUsername = LocalStorage.GetSetting("LastUsername", "");
                if (!string.IsNullOrEmpty(lastUsername))
                {
                    txtUsername.Text = lastUsername;
                    txtUsername.SelectAll();
                }

                // تحميل خانة تذكرني
                chkRememberMe.Checked = LocalStorage.GetSettingBool("RememberMe", true);

                // تحميل اللغة
                string language = LocalStorage.GetSetting("Language", "ar");
                cmbLanguage.SelectedIndex = language == "en" ? 1 : 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تحميل بيانات الدخول المحفوظة", ex);
            }
        }

        private void CheckQuickLoginAvailability()
        {
            // إذا كان هناك اسم مستخدم محفوظ، عرض خيار الدخول السريع
            if (!string.IsNullOrEmpty(txtUsername.Text) && txtUsername.Text.Length >= 3)
            {
                btnQuickLogin.Visible = true;
                lblQuickMode.Visible = true;
                lblQuickMode.Text = $"لتسريع الدخول: أدخل اسمك واضغط Enter";
            }
        }

        private async Task UpdateConnectionStatus()
        {
            try
            {
                lblStatus.Visible = true;
                lblStatus.Text = "جاري فحص الاتصال...";
                lblStatus.ForeColor = Color.Blue;

                // فحص سريع للاتصال (مع مهلة قصيرة)
                bool isConnected = await Task.Run(async () =>
                {
                    try
                    {
                        return await apiService.TestConnectionAsync();
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (isConnected)
                {
                    lblStatus.Text = "✓ متصل بالخادم";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblStatus.Text = "⚡ الوضع المحلي (سريع)";
                    lblStatus.ForeColor = Color.Orange;

                    // إذا لم يكن هناك اتصال، عرض خيار الدخول السريع
                    btnQuickLogin.Visible = true;
                    lblQuickMode.Visible = true;
                    lblQuickMode.Text = "الوضع المحلي مفعل - اضغط ⚡ للدخول الفوري";
                }

                LocalStorage.SaveSetting("LastConnectionStatus", isConnected.ToString());
            }
            catch (Exception ex)
            {
                lblStatus.Text = "✗ اتصال محدود";
                lblStatus.ForeColor = Color.Red;
                ErrorHandler.LogError("خطأ في فحص الاتصال", ex);
            }
        }

        // ========== تسجيل الدخول العادي ==========

        private async Task NormalLogin()
        {
            if (isLoading) return;

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // التحقق من صحة المدخلات
            if (!ValidateInput(username, password))
                return;

            try
            {
                SetLoadingState(true);

                // حفظ بيانات الدخول إذا طُلب ذلك
                if (chkRememberMe.Checked)
                {
                    LocalStorage.SaveSetting("LastUsername", username);
                    LocalStorage.SaveSetting("RememberMe", "true");
                }

                // تحديث حالة الاتصال
                lblStatus.Text = "جاري تسجيل الدخول...";
                lblStatus.ForeColor = Color.Blue;

                // محاولة تسجيل الدخول عبر الخادم (مع مهلة قصيرة)
                var connectionTask = apiService.TestConnectionAsync();
                if (await Task.WhenAny(connectionTask, Task.Delay(2000)) == connectionTask)
                {
                    bool isOnline = await connectionTask;
                    SessionManager.IsOnlineMode = isOnline;

                    if (isOnline)
                    {
                        // تسجيل الدخول عبر API
                        var result = await apiService.AuthenticateAsync(username, password);
                        if (result.Success)
                        {
                            ErrorHandler.LogInfo($"تم تسجيل دخول المستخدم: {username}");
                            OpenMainForm(username);
                            return;
                        }
                    }
                }

                // الوضع غير متصل - تسجيل دخول محلي سريع
                await QuickLocalLogin(username);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تسجيل الدخول", ex);
                // حتى في حالة الخطأ، حاول الدخول المحلي السريع
                await QuickLocalLogin(username);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // ========== تسجيل الدخول السريع ==========

        private async void BtnQuickLogin_Click(object sender, EventArgs e)
        {
            await QuickLogin();
        }

        private async Task QuickLogin()
        {
            if (isLoading) return;

            string username = txtUsername.Text.Trim();

            // التحقق السريع
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                MessageBox.Show("اسم المستخدم يجب أن يكون 3 أحرف على الأقل",
                    "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                txtUsername.SelectAll();
                return;
            }

            // حفظ إذا طُلب
            if (chkRememberMe.Checked)
            {
                LocalStorage.SaveSetting("LastUsername", username);
                LocalStorage.SaveSetting("RememberMe", "true");
            }

            // إظهار حالة التحميل السريعة
            SetQuickLoading(true);

            try
            {
                // تسجيل الدخول المحلي السريع
                await QuickLocalLogin(username);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في الدخول السريع", ex);
                // حتى في حالة الخطأ، افتح النافذة
                OpenMainFormImmediately(username);
            }
            finally
            {
                SetQuickLoading(false);
            }
        }

        private async Task QuickLocalLogin(string username)
        {
            try
            {
                SessionManager.CurrentUsername = username;
                SessionManager.IsOnlineMode = false; // دائمًا محلي في الوضع السريع
                SessionManager.Login(username, "local");

                // حفظ سريع في قاعدة البيانات المحلية (في الخلفية)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var db = new ChatDbContext())
                        {
                            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
                            if (user == null)
                            {
                                user = new Models.User
                                {
                                    Username = username,
                                    Status = "متصل",
                                    IsOnline = true,
                                    Color = GetRandomColor(),
                                    Avatar = "👤",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                await db.Users.AddAsync(user);
                            }
                            else
                            {
                                user.IsOnline = true;
                                user.LastSeen = DateTime.Now;
                                user.LoginCount++;
                            }
                            await db.SaveChangesAsync();
                        }
                    }
                    catch { }
                });

                ErrorHandler.LogInfo($"تم تسجيل دخول المستخدم محلياً: {username}");
                OpenMainFormImmediately(username);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في الدخول المحلي السريع", ex);
                OpenMainFormImmediately(username);
            }
        }

        // ========== دوال مساعدة ==========

        private bool ValidateInput(string username, string password)
        {
            // التحقق من اسم المستخدم
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("الرجاء إدخال اسم المستخدم");
                txtUsername.Focus();
                return false;
            }

            if (username.Length < 3)
            {
                ShowError("اسم المستخدم يجب أن يكون 3 أحرف على الأقل");
                txtUsername.Focus();
                txtUsername.SelectAll();
                return false;
            }

            if (username.Length > 50)
            {
                ShowError("اسم المستخدم لا يمكن أن يتجاوز 50 حرفاً");
                txtUsername.Focus();
                txtUsername.SelectAll();
                return false;
            }

            return true;
        }

        private void SetLoadingState(bool loading)
        {
            isLoading = loading;

            this.Invoke(new Action(() =>
            {
                progressBar.Visible = loading;
                btnLogin.Enabled = !loading;
                btnGuest.Enabled = !loading;
                btnRegister.Enabled = !loading;
                btnQuickLogin.Enabled = !loading;
                txtUsername.Enabled = !loading;
                txtPassword.Enabled = !loading;

                if (loading)
                {
                    btnLogin.Text = "جاري التسجيل...";
                    Cursor = Cursors.WaitCursor;
                }
                else
                {
                    btnLogin.Text = "تسجيل الدخول";
                    Cursor = Cursors.Default;
                }
            }));
        }

        private void SetQuickLoading(bool loading)
        {
            isLoading = loading;

            this.Invoke(new Action(() =>
            {
                progressBar.Visible = loading;
                btnLogin.Enabled = !loading;
                btnGuest.Enabled = !loading;
                btnRegister.Enabled = !loading;
                btnQuickLogin.Enabled = !loading;
                txtUsername.Enabled = !loading;
                txtPassword.Enabled = !loading;

                if (loading)
                {
                    btnQuickLogin.Text = "⚡ جاري...";
                    lblStatus.Text = "⚡ جاري الدخول السريع...";
                    lblStatus.ForeColor = Color.Orange;
                    Cursor = Cursors.WaitCursor;
                }
                else
                {
                    btnQuickLogin.Text = "⚡ دخول سريع";
                    lblStatus.Text = "⚡ جاهز";
                    lblStatus.ForeColor = Color.Green;
                    Cursor = Cursors.Default;
                }
            }));
        }

        private string GetRandomColor()
        {
            string[] colors = { "#0078D7", "#107C10", "#5C2D91", "#D83B01", "#F2C811", "#008272" };
            Random random = new Random();
            return colors[random.Next(colors.Length)];
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowHint(string message)
        {
            // يمكن إضافة تلميح أدوات (tooltip) هنا
        }

        private void OpenMainForm(string username)
        {
            this.Invoke(new Action(() =>
            {
                // فتح النموذج الرئيسي
                Form1 mainForm = new Form1();
                mainForm.Show();

                // إخفاء نافذة التسجيل
                this.Hide();

                // عند إغلاق النموذج الرئيسي، إغلاق التطبيق
                mainForm.FormClosed += (s, e) => this.Close();
            }));
        }

        private void OpenMainFormImmediately(string username)
        {
            this.Invoke(new Action(() =>
            {
                // إخفاء نافذة التسجيل أولاً
                this.Hide();

                // فتح النافذة الرئيسية
                Form1 mainForm = new Form1();
                mainForm.Show();

                // عند إغلاق النافذة الرئيسية، إغلاق التطبيق
                mainForm.FormClosed += (s, e) => this.Close();

                // إغلاق نافذة التسجيل بعد فتح الرئيسية
                this.Close();
            }));
        }

        // ========== معالجات الأحداث ==========

        private void BtnGuest_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            string guestName = SessionManager.GenerateGuestUsername();

            // حفظ كضيف
            LocalStorage.SaveSetting("IsGuest", "true");
            LocalStorage.SaveSetting("GuestUsername", guestName);

            SessionManager.CurrentUsername = guestName;
            SessionManager.IsOnlineMode = false;
            SessionManager.Login(guestName, "guest", true);

            ErrorHandler.LogInfo($"تم الدخول كضيف: {guestName}");
            OpenMainFormImmediately(guestName);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            ShowRegistrationDialog();
        }

        private void ShowRegistrationDialog()
        {
            using (Form registerForm = new Form())
            {
                registerForm.Text = "إنشاء حساب جديد";
                registerForm.Size = new Size(400, 500);
                registerForm.StartPosition = FormStartPosition.CenterParent;
                registerForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                registerForm.MaximizeBox = false;
                registerForm.MinimizeBox = false;
                registerForm.BackColor = Color.White;

                // عناصر التسجيل
                Label lblTitle = new Label
                {
                    Text = "إنشاء حساب جديد",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 215),
                    Location = new Point(100, 20),
                    Size = new Size(200, 40),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                TextBox txtNewUsername = new TextBox
                {
                    Location = new Point(50, 80),
                    Size = new Size(300, 30),
                    Font = new Font("Segoe UI", 11),
                    PlaceholderText = "اسم المستخدم (3-50 حرف)"
                };

                TextBox txtNewPassword = new TextBox
                {
                    Location = new Point(50, 130),
                    Size = new Size(300, 30),
                    Font = new Font("Segoe UI", 11),
                    PasswordChar = '•',
                    PlaceholderText = "كلمة المرور"
                };

                TextBox txtConfirmPassword = new TextBox
                {
                    Location = new Point(50, 180),
                    Size = new Size(300, 30),
                    Font = new Font("Segoe UI", 11),
                    PasswordChar = '•',
                    PlaceholderText = "تأكيد كلمة المرور"
                };

                Button btnCreateAccount = new Button
                {
                    Text = "إنشاء الحساب",
                    BackColor = Color.FromArgb(76, 175, 80),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(50, 230),
                    Size = new Size(300, 40),
                    FlatStyle = FlatStyle.Flat
                };

                btnCreateAccount.Click += async (s, e) =>
                {
                    string newUsername = txtNewUsername.Text.Trim();
                    string newPassword = txtNewPassword.Text;
                    string confirmPassword = txtConfirmPassword.Text;

                    // التحقق من صحة المدخلات
                    if (string.IsNullOrWhiteSpace(newUsername))
                    {
                        MessageBox.Show("الرجاء إدخال اسم المستخدم", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (newUsername.Length < 3 || newUsername.Length > 50)
                    {
                        MessageBox.Show("اسم المستخدم يجب أن يكون بين 3 و 50 حرفاً", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length < 6)
                    {
                        MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (newPassword != confirmPassword)
                    {
                        MessageBox.Show("كلمات المرور غير متطابقة", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // إنشاء الحساب محلياً
                    try
                    {
                        using (var db = new ChatDbContext())
                        {
                            // التحقق من عدم وجود اسم مستخدم مكرر
                            var existingUser = await db.Users
                                .FirstOrDefaultAsync(u => u.Username == newUsername);

                            if (existingUser != null)
                            {
                                MessageBox.Show("اسم المستخدم موجود مسبقاً", "خطأ",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // إنشاء المستخدم الجديد
                            var newUser = new Models.User
                            {
                                Username = newUsername,
                                Status = "متصل",
                                IsOnline = true,
                                Color = GetRandomColor(),
                                Avatar = "👤",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            await db.Users.AddAsync(newUser);
                            await db.SaveChangesAsync();

                            MessageBox.Show("تم إنشاء الحساب بنجاح! يمكنك الآن تسجيل الدخول", "نجاح",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            registerForm.DialogResult = DialogResult.OK;
                            registerForm.Close();

                            // تعبئة الحقول في نافذة التسجيل
                            txtUsername.Text = newUsername;
                            txtPassword.Text = newPassword;
                            txtPassword.Focus();

                            // عرض خيار الدخول السريع
                            btnQuickLogin.Visible = true;
                            lblQuickMode.Visible = true;
                            lblQuickMode.Text = "حسابك جاهز! اضغط ⚡ للدخول السريع";
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError("خطأ في إنشاء الحساب", ex);
                        MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                Button btnCancel = new Button
                {
                    Text = "إلغاء",
                    BackColor = Color.FromArgb(200, 200, 200),
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(50, 280),
                    Size = new Size(300, 35),
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.Click += (s, e) => registerForm.Close();

                registerForm.Controls.AddRange(new Control[]
                {
                    lblTitle, txtNewUsername, txtNewPassword, txtConfirmPassword,
                    btnCreateAccount, btnCancel
                });

                registerForm.ShowDialog();
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            ShowSettingsDialog();
        }

        private void ShowSettingsDialog()
        {
            using (Form settingsForm = new Form())
            {
                settingsForm.Text = "إعدادات التطبيق";
                settingsForm.Size = new Size(500, 600);
                settingsForm.StartPosition = FormStartPosition.CenterParent;
                settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                settingsForm.MaximizeBox = false;
                settingsForm.MinimizeBox = false;
                settingsForm.BackColor = Color.White;

                TabControl tabControl = new TabControl
                {
                    Location = new Point(10, 10),
                    Size = new Size(470, 500)
                };

                // تبويب الاتصال
                TabPage tabConnection = CreateConnectionTab();

                // تبويب الدردشة
                TabPage tabChat = CreateChatTab();

                // تبويب المظهر
                TabPage tabAppearance = CreateAppearanceTab();

                // تبويب حول
                TabPage tabAbout = CreateAboutTab();

                tabControl.TabPages.AddRange(new TabPage[]
                {
                    tabConnection,
                    tabChat,
                    tabAppearance,
                    tabAbout
                });

                Button btnSave = new Button
                {
                    Text = "حفظ",
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(300, 520),
                    Size = new Size(90, 35),
                    FlatStyle = FlatStyle.Flat
                };

                Button btnCancel = new Button
                {
                    Text = "إلغاء",
                    BackColor = Color.FromArgb(200, 200, 200),
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(400, 520),
                    Size = new Size(90, 35),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };

                btnSave.Click += (s, e) =>
                {
                    SaveSettingsFromDialog(tabControl);
                    settingsForm.DialogResult = DialogResult.OK;
                };

                settingsForm.Controls.Add(tabControl);
                settingsForm.Controls.Add(btnSave);
                settingsForm.Controls.Add(btnCancel);

                settingsForm.ShowDialog();
            }
        }

        private TabPage CreateConnectionTab()
        {
            TabPage tab = new TabPage("الاتصال");
            tab.BackColor = Color.White;

            Label lblServer = new Label
            {
                Text = "عنوان الخادم:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 20),
                Size = new Size(100, 25)
            };

            TextBox txtServer = new TextBox
            {
                Text = LocalStorage.GetSetting("ServerUrl", "http://localhost:5000"),
                Location = new Point(130, 20),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10)
            };

            CheckBox chkAutoConnect = new CheckBox
            {
                Text = "الاتصال التلقائي عند التشغيل",
                Checked = LocalStorage.GetSettingBool("AutoConnect", false),
                Location = new Point(20, 60),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10)
            };

            CheckBox chkRemember = new CheckBox
            {
                Text = "تذكر بيانات الدخول",
                Checked = LocalStorage.GetSettingBool("RememberMe", true),
                Location = new Point(20, 90),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10)
            };

            CheckBox chkQuickMode = new CheckBox
            {
                Text = "تفعيل الوضع السريع (الدخول الفوري)",
                Checked = LocalStorage.GetSettingBool("QuickMode", true),
                Location = new Point(20, 120),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10)
            };

            Button btnTestConnection = new Button
            {
                Text = "فحص الاتصال",
                Location = new Point(20, 160),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 10)
            };

            btnTestConnection.Click += async (s, e) =>
            {
                btnTestConnection.Enabled = false;
                btnTestConnection.Text = "جاري الفحص...";

                try
                {
                    bool isConnected = await apiService.TestConnectionAsync();

                    if (isConnected)
                    {
                        MessageBox.Show("✓ الاتصال بالخادم ناجح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("✗ لا يمكن الاتصال بالخادم\nسيتم استخدام الوضع المحلي", "معلومات",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("خطأ في فحص الاتصال", ex);
                    MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnTestConnection.Enabled = true;
                    btnTestConnection.Text = "فحص الاتصال";
                }
            };

            tab.Controls.AddRange(new Control[]
            {
                lblServer, txtServer,
                chkAutoConnect, chkRemember, chkQuickMode,
                btnTestConnection
            });

            return tab;
        }

        private TabPage CreateChatTab()
        {
            TabPage tab = new TabPage("الدردشة");
            tab.BackColor = Color.White;

            CheckBox chkNotifications = new CheckBox
            {
                Text = "تفعيل الإشعارات",
                Checked = LocalStorage.GetSettingBool("Notifications", true),
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10)
            };

            CheckBox chkSounds = new CheckBox
            {
                Text = "تفعيل الأصوات",
                Checked = LocalStorage.GetSettingBool("Sounds", true),
                Location = new Point(20, 50),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10)
            };

            CheckBox chkAutoDownload = new CheckBox
            {
                Text = "التنزيل التلقائي للملفات",
                Checked = LocalStorage.GetSettingBool("AutoDownload", true),
                Location = new Point(20, 80),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10)
            };

            Label lblMaxSize = new Label
            {
                Text = "الحجم الأقصى للملفات (MB):",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 120),
                Size = new Size(180, 25)
            };

            NumericUpDown numMaxSize = new NumericUpDown
            {
                Value = LocalStorage.GetSettingInt("MaxFileSize", 10),
                Minimum = 1,
                Maximum = 100,
                Location = new Point(210, 120),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10)
            };

            tab.Controls.AddRange(new Control[]
            {
                chkNotifications, chkSounds, chkAutoDownload,
                lblMaxSize, numMaxSize
            });

            return tab;
        }

        private TabPage CreateAppearanceTab()
        {
            TabPage tab = new TabPage("المظهر");
            tab.BackColor = Color.White;

            Label lblTheme = new Label
            {
                Text = "السمة:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 20),
                Size = new Size(100, 25)
            };

            ComboBox cmbTheme = new ComboBox
            {
                Location = new Point(130, 20),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbTheme.Items.AddRange(new string[] { "فاتح", "غامق", "أزرق", "أخضر" });
            cmbTheme.SelectedIndex = 0;

            Label lblFontSize = new Label
            {
                Text = "حجم الخط:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                Size = new Size(100, 25)
            };

            ComboBox cmbFontSize = new ComboBox
            {
                Location = new Point(130, 60),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbFontSize.Items.AddRange(new string[] { "صغير", "متوسط", "كبير" });
            cmbFontSize.SelectedIndex = 1;

            CheckBox chkRTL = new CheckBox
            {
                Text = "اتجاه النص من اليمين لليسار",
                Checked = true,
                Enabled = false,
                Location = new Point(20, 100),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10)
            };

            tab.Controls.AddRange(new Control[]
            {
                lblTheme, cmbTheme,
                lblFontSize, cmbFontSize,
                chkRTL
            });

            return tab;
        }

        private TabPage CreateAboutTab()
        {
            TabPage tab = new TabPage("حول");
            tab.BackColor = Color.White;

            PictureBox picAboutLogo = new PictureBox
            {
                Image = CreateAppLogo(),
                Location = new Point(150, 20),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Label lblAppName = new Label
            {
                Text = "Yemen WhatsApp Desktop",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                Location = new Point(50, 130),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblVersion = new Label
            {
                Text = "الإصدار: 2.0.0",
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 180),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblDeveloper = new Label
            {
                Text = "المطور: إيمن عبدالوهاب الصالحي",
                Font = new Font("Segoe UI", 11),
                Location = new Point(50, 210),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblCopyright = new Label
            {
                Text = "© 2025 Yemen Software",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(50, 240),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            LinkLabel linkWebsite = new LinkLabel
            {
                Text = "الموقع الإلكتروني",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 280),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                LinkColor = Color.FromArgb(0, 120, 215)
            };
            linkWebsite.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.yemenwhatsapp.com",
                    UseShellExecute = true
                });
            };

            LinkLabel linkContact = new LinkLabel
            {
                Text = "اتصل بنا",
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, 310),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                LinkColor = Color.FromArgb(0, 120, 215)
            };
            linkContact.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mailto:support@yemenwhatsapp.com",
                    UseShellExecute = true
                });
            };

            Button btnCheckUpdates = new Button
            {
                Text = "فحص التحديثات",
                Location = new Point(150, 350),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10)
            };
            btnCheckUpdates.Click += (s, e) =>
            {
                MessageBox.Show("أنت تستخدم أحدث إصدار من التطبيق", "التحديثات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            tab.Controls.AddRange(new Control[]
            {
                picAboutLogo, lblAppName, lblVersion, lblDeveloper,
                lblCopyright, linkWebsite, linkContact, btnCheckUpdates
            });

            return tab;
        }

        private void SaveSettingsFromDialog(TabControl tabControl)
        {
            try
            {
                // حفظ إعدادات الاتصال
                foreach (Control control in tabControl.TabPages[0].Controls)
                {
                    if (control is TextBox txt && txt.Name.Contains("Server"))
                    {
                        LocalStorage.SaveSetting("ServerUrl", txt.Text);
                        apiService.Initialize(txt.Text);
                    }
                    else if (control is CheckBox chk)
                    {
                        if (chk.Text.Contains("الاتصال التلقائي"))
                            LocalStorage.SaveSetting("AutoConnect", chk.Checked.ToString());
                        else if (chk.Text.Contains("تذكر"))
                            LocalStorage.SaveSetting("RememberMe", chk.Checked.ToString());
                        else if (chk.Text.Contains("الوضع السريع"))
                        {
                            LocalStorage.SaveSetting("QuickMode", chk.Checked.ToString());
                            quickModeEnabled = chk.Checked;
                            btnQuickLogin.Visible = chk.Checked;
                            lblQuickMode.Visible = chk.Checked;
                        }
                    }
                }

                // حفظ إعدادات الدردشة
                foreach (Control control in tabControl.TabPages[1].Controls)
                {
                    if (control is CheckBox chk)
                    {
                        if (chk.Text.Contains("الإشعارات"))
                            LocalStorage.SaveSetting("Notifications", chk.Checked.ToString());
                        else if (chk.Text.Contains("الأصوات"))
                            LocalStorage.SaveSetting("Sounds", chk.Checked.ToString());
                        else if (chk.Text.Contains("التنزيل التلقائي"))
                            LocalStorage.SaveSetting("AutoDownload", chk.Checked.ToString());
                    }
                    else if (control is NumericUpDown num)
                    {
                        LocalStorage.SaveSetting("MaxFileSize", num.Value.ToString());
                    }
                }

                MessageBox.Show("تم حفظ الإعدادات بنجاح", "معلومات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في حفظ الإعدادات", ex);
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("في هذا الإصدار، يمكنك الدخول بأي اسم مستخدم.\n" +
                          "كلمة المرور اختيارية.\n" +
                          "في الإصدارات القادمة سيتم إضافة نظام استعادة كلمة المرور.",
                          "نسيت كلمة المرور",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LinkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string helpText = @"🎯 Yemen WhatsApp - دليل الاستخدام

خيارات الدخول:
1. تسجيل الدخول العادي:
   - أدخل اسم المستخدم
   - أدخل كلمة المرور (اختياري)
   - اضغط 'تسجيل الدخول'

2. الدخول السريع (⚡):
   - أدخل اسم المستخدم فقط
   - اضغط زر ⚡ أو اضغط Enter
   - الدخول الفوري بدون انتظار

3. الدخول كضيف:
   - سيتم إنشاء اسم مؤقت
   - مثالي للاستخدام السريع

4. إنشاء حساب جديد:
   - حفظ المستخدمين محلياً
   - إمكانية إضافة كلمة مرور

ملاحظات:
• الوضع السريع يعمل بدون إنترنت
• جميع البيانات تحفظ على جهازك
• يمكنك التبديل بين الوضعين

للأسئلة: aiababsa123@gmail.com";

            MessageBox.Show(helpText, "المساعدة",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string language = cmbLanguage.SelectedIndex == 0 ? "ar" : "en";
            LocalStorage.SaveSetting("Language", language);

            MessageBox.Show("سيتم تطبيق تغيير اللغة بعد إعادة تشغيل التطبيق", "تغيير اللغة",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ========== أحداث لوحة المفاتيح ==========

        private void TxtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !isLoading)
            {
                if (quickModeEnabled && !string.IsNullOrEmpty(txtUsername.Text.Trim()))
                {
                    // الدخول السريع عند الضغط على Enter
                    QuickLogin();
                }
                else
                {
                    txtPassword.Focus();
                }
                e.Handled = true;
            }
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !isLoading)
            {
                btnLogin.PerformClick();
                e.Handled = true;
            }
        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            // تغيير لون الحقل بناءً على الصحة
            if (txtUsername.Text.Length >= 3 && txtUsername.Text.Length <= 50)
            {
                txtUsername.BackColor = Color.FromArgb(220, 255, 220);

                // إذا كان الوضع السريع مفعلاً، عرض زر الدخول السريع
                if (quickModeEnabled)
                {
                    btnQuickLogin.Visible = true;
                    lblQuickMode.Visible = true;
                }
            }
            else
            {
                txtUsername.BackColor = Color.FromArgb(255, 220, 220);
                btnQuickLogin.Visible = false;
                lblQuickMode.Visible = false;
            }
        }

        // ========== جعل النافذة قابلة للسحب ==========

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOffset = new Point(-e.X, -e.Y);
                isMouseDown = true;
            }
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(
                    "هل تريد حقاً الخروج من التطبيق؟",
                    "تأكيد الخروج",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    ErrorHandler.LogInfo("تم إغلاق نافذة تسجيل الدخول");
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // تحميل إعداد الوضع السريع
            quickModeEnabled = LocalStorage.GetSettingBool("QuickMode", true);
            btnQuickLogin.Visible = quickModeEnabled;
            lblQuickMode.Visible = quickModeEnabled;

            // إذا كان هناك اسم مستخدم محفوظ، اقتراح الدخول السريع
            if (!string.IsNullOrEmpty(txtUsername.Text) && txtUsername.Text.Length >= 3)
            {
                lblQuickMode.Text = "لتسريع الدخول: اضغط ⚡ أو Enter";
            }
        }
    }
}
