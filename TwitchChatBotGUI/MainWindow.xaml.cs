﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchChatBotGUI.MenuItems;
using TwitchChatBot;

namespace TwitchChatBotGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            bot = new TwitchBot();

            bot.Destination = new Endpoint();
            bot.Destination.EndpointAddress = "irc.twitch.tv";
            bot.Destination.EndpointPort = 6667;

            MainWindowName.DataContext = bot;
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            bot.Connect();

            //https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=amoyxo9a7agc0e1gjpcawa1rqb2ciy4&redirect_uri=http://localhost:6555

            bot.SendMessage("PASS oauth:" + bot.Auth.AuthKey + "\r\n");
            bot.SendMessage("NICK " + bot.Auth.AuthName + "\r\n");

            //bot.JoinTwitchChannel("sovietmade");

            //bot.SendMessage("JOIN #" + bot.Auth.AuthName + "\r\n");
            //bot.SendMessage("PRIVMSG #sovietmade :test\r\n");
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageBox.Text;
            //IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#"+bot.TwitchChannel, false), new IrcCommandParameter(messageToSend, true));
            //bot.SendMessage(ic.ToString() + "\r\n");
            bot.SendMessageToCurrentChannel(messageToSend);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer--;
        }
        async Task CountDown(CancellationToken ct)
        {
            while (timer >= 0)
            {
                TimerLabel.Content = String.Format("Quiz starting in {0}",timer);
                await Task.Delay(100);
            }
        }

        async Task ShowTimer(CancellationToken ct) 
        {
            t = new System.Timers.Timer(1000);
            t.Elapsed += timer1_Tick;
            t.Start();

            await CountDown(ct);
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StartQuizCT != null) {
                if (t != null)
                {
                    t.Stop();
                }
                StartQuizCT.Cancel();
            }
            StartQuizCT = new CancellationTokenSource();
            bot.SendMessageToCurrentChannel("Quiz is starting!");
            //IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#" + bot.TwitchChannel, false), new IrcCommandParameter("Starting Quiz in 60 seconds!", true));
            //bot.SendMessage(ic.ToString() + "\r\n");
            bot.StartQuiz();
            TimerLabel.Content = 60;
            timer = 60;
            await ShowTimer(StartQuizCT.Token);
            TimerLabel.Content = "Quiz is running...";
        }

        async Task GetAuth(CancellationToken ct)
        {
            bot.TwitchAuthorize();
            while(bot.Auth.AuthName == ""){
                await Task.Delay(100);
            }
        }

        async private void Authorize_Click(object sender, RoutedEventArgs e)
        {               
            StartAuthorize = new CancellationTokenSource();
            await GetAuth(StartAuthorize.Token); 
        }

        CancellationTokenSource StartAuthorize;
        CancellationTokenSource StartQuizCT;
        int timer;
        System.Timers.Timer t;
        TwitchBot bot;




        #region functionality for menu-driven UI

        private void OpenNetworkSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new NetworkSettings(bot, Pop));
        }

        private void OpenQuizSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new QuizSettings(bot, Pop));
        }

        private void OpenChannelSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new ChannelSettings(bot, Pop));
        }

        private void ChannelButtonClick(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new ChannelSettings(bot, Pop));
        }

        private void ShowPopupWithUserControl(ITwitchMenuItem inControl)
        {
            inControl.CurrentPopup.PlacementTarget = this;
            inControl.CurrentPopup.Placement = PlacementMode.Center;
            inControl.CurrentPopup.Child = (inControl as UserControl);
            inControl.CurrentPopup.IsOpen = true;
            inControl.CurrentPopup.StaysOpen = false;
        }

        private void LogOutButtonClick(object sender, RoutedEventArgs e)
        {
            bot.TwitchLogOut();
        }

        #endregion






    }
}
