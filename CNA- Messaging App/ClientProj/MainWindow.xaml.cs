using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Packets;

namespace CNA__Tut5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientProj.Client m_client;
        public MainWindow(ClientProj.Client client)
        {
            InitializeComponent();
            m_client = client;
            chatBox.IsReadOnly = true;
        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (messageText.Text == "")
            {
                MessageBox.Show("No message in text box!", "Warning!");
            }
            else
            {
                NamePacket name = new NamePacket(localName.Text);
                ChatMessagePacket message = new ChatMessagePacket(messageText.Text);
                //string message = messageText.Text;
                //messageText.Text = "";
                if (localName.Text == "")
                {
                    MessageBox.Show("Please enter a name in Local Name Textbox!", "Warning!");
                }
                else
                {
                    //Packet message = messageText.Text;
                    //chatBox.Text += name + " says: " + message + "\n";
                    //m_client.SendMessage(localName.Text, messageText.Text);
                    m_client.TCPSendMessage(name);
                    m_client.TCPSendMessage(message);
                    localName.IsReadOnly = true;
                }
            }
        }

        public void UpdateChatBox(string message)
        {
            chatBox.Dispatcher.Invoke(() =>
            {
                chatBox.Text += message + Environment.NewLine;
                chatBox.ScrollToEnd();
            });
        }

        public void UpdateNameChatBox(string message)
        {
            chatBox.Dispatcher.Invoke(() =>
            {
                chatBox.Text += message;
            });
        }

        public void UpdateUserList(string message)
        {
            userList.Dispatcher.Invoke(() =>
            {
                userList.Items.Add(message);
            });
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpPacket help = new HelpPacket();
            m_client.TCPSendMessage(help);
        }


    }
}
