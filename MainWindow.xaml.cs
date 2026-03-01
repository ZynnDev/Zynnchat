using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZynnChat
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<string>> privateChats = new();
        private Dictionary<string, List<string>> serverChats = new();

        private string currentChat = "";
        private bool isServer = false;

        private const string SettingsFile = "userSettings.json";
        private UserSettings settings = new();

        public MainWindow()
        {
            InitializeComponent();

            AddFriend("Anders");
            AddFriend("Mikkel");
            AddFriend("Sara");

            LoadSettings();
            ApplySettings();
        }

        // =========================
        // FRIENDS
        // =========================
        private void AddFriend(string name)
        {
            if (!privateChats.ContainsKey(name))
            {
                privateChats[name] = new List<string>();
                FriendsList.Items.Add(name);
            }
        }

        private void FriendsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FriendsList.SelectedItem is string friendName)
            {
                OpenPrivateChat(friendName);
            }
        }

        private void OpenPrivateChat(string friendName)
        {
            isServer = false;
            currentChat = friendName;

            if (!privateChats.ContainsKey(friendName))
                privateChats[friendName] = new List<string>();

            RefreshChat(privateChats[friendName]);
        }

        // =========================
        // SERVERS (100% STABIL)
        // =========================
        private void AddServer(string name)
        {
            if (!serverChats.ContainsKey(name))
            {
                serverChats[name] = new List<string>();
                ServerList.Items.Add(name);
            }
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Indtast server navn:",
                "Opret Server",
                "Ny Server");

            if (string.IsNullOrWhiteSpace(name)) return;

            AddServer(name);

            // Tving selection refresh
            ServerList.SelectedItem = null;
            ServerList.SelectedItem = name;

            OpenServerChat(name);
        }

        private void ServerList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ServerList.SelectedItem is string serverName)
            {
                OpenServerChat(serverName);
            }
        }

        private void OpenServerChat(string serverName)
        {
            isServer = true;
            currentChat = serverName;

            if (!serverChats.ContainsKey(serverName))
                serverChats[serverName] = new List<string>();

            RefreshChat(serverChats[serverName]);
        }

        // =========================
        // CHAT SYSTEM
        // =========================
        private void RefreshChat(List<string> messages)
        {
            ChatList.Items.Clear();

            foreach (var msg in messages)
                ChatList.Items.Add(msg);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(ChatInput.Text)) return;
            if (string.IsNullOrEmpty(currentChat)) return;

            string message = $"{settings.Username}: {ChatInput.Text}";

            if (isServer)
                serverChats[currentChat].Add(message);
            else
                privateChats[currentChat].Add(message);

            ChatInput.Clear();

            if (isServer)
                RefreshChat(serverChats[currentChat]);
            else
                RefreshChat(privateChats[currentChat]);
        }

        // =========================
        // TABS
        // =========================
        private void ShowFriends_Click(object sender, RoutedEventArgs e)
        {
            FriendsPanel.Visibility = Visibility.Visible;
            ChannelsPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowChannels_Click(object sender, RoutedEventArgs e)
        {
            FriendsPanel.Visibility = Visibility.Collapsed;
            ChannelsPanel.Visibility = Visibility.Visible;
        }

        // =========================
        // SETTINGS
        // =========================
        private void OpenSettings_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Visible;
            SettingsUsernameBox.Text = settings.Username;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            settings.Username = SettingsUsernameBox.Text;
            SaveSettings();
            ApplySettings();
            SettingsOverlay.Visibility = Visibility.Collapsed;
        }

        private void UploadProfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                settings.ProfileImagePath = dialog.FileName;
                SaveSettings();
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            BottomUsername.Text = settings.Username;

            if (!string.IsNullOrEmpty(settings.ProfileImagePath) &&
                File.Exists(settings.ProfileImagePath))
            {
                ProfileEllipse.Fill = new ImageBrush(
                    new BitmapImage(new Uri(settings.ProfileImagePath)));
            }
            else
            {
                ProfileEllipse.Fill = new ImageBrush(
                    new BitmapImage(new Uri("https://i.imgur.com/6VBx3io.png")));
            }
        }

        private void SaveSettings()
        {
            File.WriteAllText(SettingsFile,
                JsonSerializer.Serialize(settings));
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                settings = JsonSerializer.Deserialize<UserSettings>(
                    File.ReadAllText(SettingsFile)) ?? new();
            }
        }
    }

    public class UserSettings
    {
        public string Username { get; set; } = "User";
        public string ProfileImagePath { get; set; } = "";
    }
}
