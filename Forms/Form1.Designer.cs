namespace YemenWhatsApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            topPanel = new Panel();
            appTitleLabel = new Label();
            profilePictureBox = new PictureBox();
            statusLabel = new Label();
            splitContainer1 = new SplitContainer();
            leftPanel = new Panel();
            label1 = new Label();
            onlineCountLabel = new Label();
            usersListBox = new ListBox();
            btnViewStats = new Button();
            rightPanel = new Panel();
            chatHeaderPanel = new Panel();
            chatTitleLabel = new Label();
            chatStatusLabel = new Label();
            messagesFlowPanel = new FlowLayoutPanel();
            messagePanel = new Panel();
            messageTextBox = new TextBox();
            sendButton = new Button();
            attachButton = new Button();
            controlPanel = new Panel();
            label2 = new Label();
            usernameTextBox = new TextBox();
            connectButton = new Button();
            label3 = new Label();
            serverUrlTextBox = new TextBox();
            chatTypePanel = new Panel();
            publicRadioButton = new RadioButton();
            privateRadioButton = new RadioButton();
            targetUsersComboBox = new ComboBox();
            btnUpdateProfile = new Button();
            infoPanel = new Panel();
            infoLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)profilePictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            leftPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            chatHeaderPanel.SuspendLayout();
            messagePanel.SuspendLayout();
            controlPanel.SuspendLayout();
            chatTypePanel.SuspendLayout();
            infoPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.FromArgb(0, 150, 136);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(1200, 5);
            topPanel.TabIndex = 0;
            topPanel.Paint += topPanel_Paint;
            // 
            // appTitleLabel
            // 
            appTitleLabel.AutoSize = true;
            appTitleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            appTitleLabel.ForeColor = Color.FromArgb(0, 150, 136);
            appTitleLabel.Location = new Point(12, 15);
            appTitleLabel.Name = "appTitleLabel";
            appTitleLabel.Size = new Size(244, 32);
            appTitleLabel.TabIndex = 1;
            appTitleLabel.Text = "🇾🇪 Yemen WhatsApp";
            // 
            // profilePictureBox
            // 
            profilePictureBox.BackColor = Color.Transparent;
            profilePictureBox.Location = new Point(1120, 10);
            profilePictureBox.Name = "profilePictureBox";
            profilePictureBox.Size = new Size(70, 70);
            profilePictureBox.TabIndex = 2;
            profilePictureBox.TabStop = false;
            profilePictureBox.Click += ProfilePictureBox_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            statusLabel.ForeColor = Color.Gray;
            statusLabel.Location = new Point(1020, 30);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(82, 23);
            statusLabel.TabIndex = 3;
            statusLabel.Text = "غير متصل";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 5);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(leftPanel);
            splitContainer1.Panel1.RightToLeft = RightToLeft.Yes;
            splitContainer1.Panel1MinSize = 300;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(rightPanel);
            splitContainer1.Panel2.RightToLeft = RightToLeft.Yes;
            splitContainer1.Panel2MinSize = 600;
            splitContainer1.Size = new Size(1200, 695);
            splitContainer1.SplitterDistance = 300;
            splitContainer1.TabIndex = 4;
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.FromArgb(250, 250, 250);
            leftPanel.Controls.Add(label1);
            leftPanel.Controls.Add(onlineCountLabel);
            leftPanel.Controls.Add(usersListBox);
            leftPanel.Controls.Add(btnViewStats);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(10);
            leftPanel.Size = new Size(300, 695);
            leftPanel.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(10, 10);
            label1.Name = "label1";
            label1.Size = new Size(117, 28);
            label1.TabIndex = 0;
            label1.Text = "المستخدمون";
            // 
            // onlineCountLabel
            // 
            onlineCountLabel.AutoSize = true;
            onlineCountLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            onlineCountLabel.ForeColor = Color.Green;
            onlineCountLabel.Location = new Point(200, 12);
            onlineCountLabel.Name = "onlineCountLabel";
            onlineCountLabel.Size = new Size(68, 23);
            onlineCountLabel.TabIndex = 1;
            onlineCountLabel.Text = "0 متصل";
            // 
            // usersListBox
            // 
            usersListBox.BackColor = Color.White;
            usersListBox.BorderStyle = BorderStyle.None;
            usersListBox.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            usersListBox.FormattingEnabled = true;
            usersListBox.ItemHeight = 25;
            usersListBox.Location = new Point(10, 40);
            usersListBox.Name = "usersListBox";
            usersListBox.Size = new Size(280, 500);
            usersListBox.TabIndex = 2;
            usersListBox.SelectedIndexChanged += usersListBox_SelectedIndexChanged;
            // 
            // btnViewStats
            // 
            btnViewStats.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnViewStats.Location = new Point(10, 550);
            btnViewStats.Name = "btnViewStats";
            btnViewStats.Size = new Size(280, 35);
            btnViewStats.TabIndex = 3;
            btnViewStats.Text = "📊 الإحصائيات";
            btnViewStats.Click += btnViewStats_Click;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(chatHeaderPanel);
            rightPanel.Controls.Add(messagesFlowPanel);
            rightPanel.Controls.Add(messagePanel);
            rightPanel.Controls.Add(controlPanel);
            rightPanel.Controls.Add(infoPanel);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(896, 695);
            rightPanel.TabIndex = 0;
            // 
            // chatHeaderPanel
            // 
            chatHeaderPanel.BackColor = Color.FromArgb(0, 150, 136);
            chatHeaderPanel.Controls.Add(chatTitleLabel);
            chatHeaderPanel.Controls.Add(chatStatusLabel);
            chatHeaderPanel.Dock = DockStyle.Top;
            chatHeaderPanel.Location = new Point(0, 0);
            chatHeaderPanel.Name = "chatHeaderPanel";
            chatHeaderPanel.Size = new Size(896, 60);
            chatHeaderPanel.TabIndex = 0;
            chatHeaderPanel.Paint += chatHeaderPanel_Paint;
            // 
            // chatTitleLabel
            // 
            chatTitleLabel.AutoSize = true;
            chatTitleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            chatTitleLabel.ForeColor = Color.White;
            chatTitleLabel.Location = new Point(20, 15);
            chatTitleLabel.Name = "chatTitleLabel";
            chatTitleLabel.Size = new Size(227, 32);
            chatTitleLabel.TabIndex = 0;
            chatTitleLabel.Text = "Yemen Chat Group";
            // 
            // chatStatusLabel
            // 
            chatStatusLabel.AutoSize = true;
            chatStatusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            chatStatusLabel.ForeColor = Color.White;
            chatStatusLabel.Location = new Point(20, 40);
            chatStatusLabel.Name = "chatStatusLabel";
            chatStatusLabel.Size = new Size(94, 23);
            chatStatusLabel.TabIndex = 1;
            chatStatusLabel.Text = "اتصال فوري";
            // 
            // messagesFlowPanel
            // 
            messagesFlowPanel.AutoScroll = true;
            messagesFlowPanel.BackColor = Color.FromArgb(240, 245, 240);
            messagesFlowPanel.Dock = DockStyle.Fill;
            messagesFlowPanel.FlowDirection = FlowDirection.TopDown;
            messagesFlowPanel.Location = new Point(0, 0);
            messagesFlowPanel.Name = "messagesFlowPanel";
            messagesFlowPanel.Size = new Size(896, 455);
            messagesFlowPanel.TabIndex = 1;
            messagesFlowPanel.WrapContents = false;
            messagesFlowPanel.Paint += messagesFlowPanel_Paint;
            // 
            // messagePanel
            // 
            messagePanel.BackColor = Color.White;
            messagePanel.Controls.Add(messageTextBox);
            messagePanel.Controls.Add(sendButton);
            messagePanel.Controls.Add(attachButton);
            messagePanel.Dock = DockStyle.Bottom;
            messagePanel.Location = new Point(0, 455);
            messagePanel.Name = "messagePanel";
            messagePanel.Padding = new Padding(10);
            messagePanel.Size = new Size(896, 60);
            messagePanel.TabIndex = 2;
            // 
            // messageTextBox
            // 
            messageTextBox.BorderStyle = BorderStyle.FixedSingle;
            messageTextBox.Dock = DockStyle.Fill;
            messageTextBox.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            messageTextBox.Location = new Point(50, 10);
            messageTextBox.Multiline = true;
            messageTextBox.Name = "messageTextBox";
            messageTextBox.Size = new Size(736, 40);
            messageTextBox.TabIndex = 0;
            messageTextBox.KeyPress += messageTextBox_KeyPress;
            // 
            // sendButton
            // 
            sendButton.BackColor = Color.FromArgb(0, 150, 136);
            sendButton.Dock = DockStyle.Right;
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            sendButton.ForeColor = Color.White;
            sendButton.Location = new Point(786, 10);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(100, 40);
            sendButton.TabIndex = 1;
            sendButton.Text = "إرسال";
            sendButton.UseVisualStyleBackColor = false;
            sendButton.Click += sendButton_Click;
            // 
            // attachButton
            // 
            attachButton.BackColor = Color.FromArgb(200, 200, 200);
            attachButton.Dock = DockStyle.Left;
            attachButton.FlatStyle = FlatStyle.Flat;
            attachButton.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            attachButton.Location = new Point(10, 10);
            attachButton.Name = "attachButton";
            attachButton.Size = new Size(40, 40);
            attachButton.TabIndex = 2;
            attachButton.Text = "📎";
            attachButton.UseVisualStyleBackColor = false;
            attachButton.Click += attachButton_Click_1;
            // 
            // controlPanel
            // 
            controlPanel.BackColor = Color.FromArgb(245, 245, 245);
            controlPanel.Controls.Add(label2);
            controlPanel.Controls.Add(usernameTextBox);
            controlPanel.Controls.Add(connectButton);
            controlPanel.Controls.Add(label3);
            controlPanel.Controls.Add(serverUrlTextBox);
            controlPanel.Controls.Add(chatTypePanel);
            controlPanel.Controls.Add(btnUpdateProfile);
            controlPanel.Dock = DockStyle.Bottom;
            controlPanel.Location = new Point(0, 515);
            controlPanel.Name = "controlPanel";
            controlPanel.Padding = new Padding(20);
            controlPanel.Size = new Size(896, 100);
            controlPanel.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 15);
            label2.Name = "label2";
            label2.Size = new Size(104, 20);
            label2.TabIndex = 0;
            label2.Text = "اسم المستخدم:";
            // 
            // usernameTextBox
            // 
            usernameTextBox.Location = new Point(120, 12);
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.Size = new Size(150, 27);
            usernameTextBox.TabIndex = 1;
            usernameTextBox.TextChanged += usernameTextBox_TextChanged;
            // 
            // connectButton
            // 
            connectButton.BackColor = Color.FromArgb(0, 120, 215);
            connectButton.FlatStyle = FlatStyle.Flat;
            connectButton.ForeColor = Color.White;
            connectButton.Location = new Point(280, 10);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(100, 28);
            connectButton.TabIndex = 2;
            connectButton.Text = "الاتصال";
            connectButton.UseVisualStyleBackColor = false;
            connectButton.Click += connectButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(400, 15);
            label3.Name = "label3";
            label3.Size = new Size(91, 20);
            label3.TabIndex = 3;
            label3.Text = "عنوان الخادم:";
            // 
            // serverUrlTextBox
            // 
            serverUrlTextBox.Location = new Point(480, 12);
            serverUrlTextBox.Name = "serverUrlTextBox";
            serverUrlTextBox.Size = new Size(250, 27);
            serverUrlTextBox.TabIndex = 4;
            serverUrlTextBox.Text = "http://localhost:5000";
            serverUrlTextBox.TextChanged += serverUrlTextBox_TextChanged;
            // 
            // chatTypePanel
            // 
            chatTypePanel.Controls.Add(publicRadioButton);
            chatTypePanel.Controls.Add(privateRadioButton);
            chatTypePanel.Controls.Add(targetUsersComboBox);
            chatTypePanel.Location = new Point(20, 50);
            chatTypePanel.Name = "chatTypePanel";
            chatTypePanel.Size = new Size(300, 30);
            chatTypePanel.TabIndex = 5;
            // 
            // publicRadioButton
            // 
            publicRadioButton.AutoSize = true;
            publicRadioButton.Checked = true;
            publicRadioButton.Location = new Point(0, 5);
            publicRadioButton.Name = "publicRadioButton";
            publicRadioButton.Size = new Size(52, 24);
            publicRadioButton.TabIndex = 0;
            publicRadioButton.TabStop = true;
            publicRadioButton.Text = "عام";
            publicRadioButton.UseVisualStyleBackColor = true;
            // 
            // privateRadioButton
            // 
            privateRadioButton.AutoSize = true;
            privateRadioButton.Location = new Point(60, 5);
            privateRadioButton.Name = "privateRadioButton";
            privateRadioButton.Size = new Size(63, 24);
            privateRadioButton.TabIndex = 1;
            privateRadioButton.Text = "خاص";
            privateRadioButton.UseVisualStyleBackColor = true;
            privateRadioButton.CheckedChanged += privateRadioButton_CheckedChanged;
            // 
            // targetUsersComboBox
            // 
            targetUsersComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            targetUsersComboBox.Location = new Point(120, 3);
            targetUsersComboBox.Name = "targetUsersComboBox";
            targetUsersComboBox.Size = new Size(150, 28);
            targetUsersComboBox.TabIndex = 2;
            targetUsersComboBox.Visible = false;
            targetUsersComboBox.SelectedIndexChanged += targetUsersComboBox_SelectedIndexChanged;
            // 
            // btnUpdateProfile
            // 
            btnUpdateProfile.BackColor = Color.FromArgb(76, 175, 80);
            btnUpdateProfile.FlatStyle = FlatStyle.Flat;
            btnUpdateProfile.ForeColor = Color.White;
            btnUpdateProfile.Location = new Point(750, 10);
            btnUpdateProfile.Name = "btnUpdateProfile";
            btnUpdateProfile.Size = new Size(120, 28);
            btnUpdateProfile.TabIndex = 5;
            btnUpdateProfile.Text = "تحديث الملف الشخصي";
            btnUpdateProfile.UseVisualStyleBackColor = false;
            btnUpdateProfile.Click += btnUpdateProfile_Click;
            // 
            // infoPanel
            // 
            infoPanel.BackColor = Color.FromArgb(240, 240, 240);
            infoPanel.Controls.Add(infoLabel);
            infoPanel.Dock = DockStyle.Bottom;
            infoPanel.Location = new Point(0, 615);
            infoPanel.Name = "infoPanel";
            infoPanel.Padding = new Padding(10);
            infoPanel.Size = new Size(896, 80);
            infoPanel.TabIndex = 4;
            // 
            // infoLabel
            // 
            infoLabel.Dock = DockStyle.Fill;
            infoLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            infoLabel.ForeColor = Color.DimGray;
            infoLabel.Location = new Point(10, 10);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(876, 60);
            infoLabel.TabIndex = 0;
            infoLabel.Text = "Yemen WhatsApp Desktop";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1200, 700);
            Controls.Add(splitContainer1);
            Controls.Add(profilePictureBox);
            Controls.Add(statusLabel);
            Controls.Add(appTitleLabel);
            Controls.Add(topPanel);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(800, 600);
            Name = "Form1";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yemen WhatsApp Desktop";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)profilePictureBox).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            chatHeaderPanel.ResumeLayout(false);
            chatHeaderPanel.PerformLayout();
            messagePanel.ResumeLayout(false);
            messagePanel.PerformLayout();
            controlPanel.ResumeLayout(false);
            controlPanel.PerformLayout();
            chatTypePanel.ResumeLayout(false);
            chatTypePanel.PerformLayout();
            infoPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        // ========== إعلان العناصر ==========
        private Panel topPanel;
        private Label appTitleLabel;
        private PictureBox profilePictureBox;
        private Label statusLabel;
        private SplitContainer splitContainer1;
        private Panel leftPanel;
        private Label label1;
        private Label onlineCountLabel;
        private ListBox usersListBox;
        private Button btnViewStats;
        private Panel rightPanel;
        private Panel chatHeaderPanel;
        private Label chatTitleLabel;
        private Label chatStatusLabel;
        private FlowLayoutPanel messagesFlowPanel;
        private Panel messagePanel;
        private TextBox messageTextBox;
        private Button sendButton;
        private Button attachButton;
        private Panel controlPanel;
        private Label label2;
        private TextBox usernameTextBox;
        private Button connectButton;
        private Label label3;
        private TextBox serverUrlTextBox;
        private Panel chatTypePanel;
        private RadioButton publicRadioButton;
        private RadioButton privateRadioButton;
        private ComboBox targetUsersComboBox;
        private Panel infoPanel;
        private Label infoLabel;
        private Button btnUpdateProfile;
    }
}