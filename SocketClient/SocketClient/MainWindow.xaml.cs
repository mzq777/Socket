using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace SocketClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //创建连接的Socket
        Socket socketSend;

        //创建接收客户端发送消息的线程
        Thread threadReceive;
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(this.txtIP.Text.Trim());
                socketSend.Connect(ip, Convert.ToInt32(this.txtPort.Text.Trim()));
                this.txtRec.Dispatcher.Invoke(()=> {
                    SetValue("连接成功");
                });

                //开启一个新的线程不停的接收服务器发送消息的线程
                threadReceive = new Thread(new ThreadStart(Receive));
                //设置为后台线程
                threadReceive.IsBackground = true;
                threadReceive.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接服务端出错:" + ex.ToString());
            }
        }
        /// <summary>
        /// 接口服务器发送的消息
        /// </summary>
        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    //实际接收到的字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    else
                    {
                        string str = Encoding.Default.GetString(buffer, 0, r);
                        this.txtRec.Dispatcher.Invoke(()=> {
                            SetValue("接收远程服务器:" + socketSend.RemoteEndPoint + "发送的消息:" + str);
                        });                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("接收服务端发送的消息出错:" + ex.ToString());
            }
        }
        private void SetValue(string strValue)
        {
            this.txtRec.AppendText(strValue + "\r \n");
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //关闭socket
            socketSend?.Close();
            //终止线程
            threadReceive?.Abort();
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strMsg = this.txtSend.Text.Trim();
                byte[] buffer = new byte[2048];
                buffer = Encoding.Default.GetBytes(strMsg);
                int receive = socketSend.Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送消息出错:" + ex.Message);
            }
        }
    }
}
