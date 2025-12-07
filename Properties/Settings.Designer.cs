namespace YemenWhatsApp.Properties
{


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.8.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastUsername
        {
            get
            {
                return ((string)(this["LastUsername"]));
            }
            set
            {
                this["LastUsername"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ServerUrl
        {
            get
            {
                return ((string)(this["ServerUrl"]));
            }
            set
            {
                this["ServerUrl"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsOnlineMode
        {
            get
            {
                return ((bool)(this["IsOnlineMode"]));
            }
            set
            {
                this["IsOnlineMode"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Theme
        {
            get
            {
                return ((string)(this["Theme"]));
            }
            set
            {
                this["Theme"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NotificationsEnabled
        {
            get
            {
                return ((bool)(this["NotificationsEnabled"]));
            }
            set
            {
                this["NotificationsEnabled"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SoundsEnabled
        {
            get
            {
                return ((bool)(this["SoundsEnabled"]));
            }
            set
            {
                this["SoundsEnabled"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1200")]
        public int WindowWidth
        {
            get
            {
                return ((int)(this["WindowWidth"]));
            }
            set
            {
                this["WindowWidth"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("700")]
        public int WindowHeight
        {
            get
            {
                return ((int)(this["WindowHeight"]));
            }
            set
            {
                this["WindowHeight"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public string WindowState
        {
            get
            {
                return ((string)(this["WindowState"]));
            }
            set
            {
                this["WindowState"] = value;
            }
        }
    }
}