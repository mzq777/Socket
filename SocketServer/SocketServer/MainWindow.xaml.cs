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

namespace SocketServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //用于通信的Socket
        Socket socketSend;

        //用于监听的SOCKET
        Socket socketWatch;

        //将远程连接的客户端的IP地址和Socket存入集合中
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();

        //创建监听连接的线程
        Thread AcceptSocketThread;

        //接收客户端发送消息的线程
        Thread threadReceive;
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            //当点击开始监听的时候 在服务器端创建一个负责监听IP地址和端口号的Socket
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取ip地址
            IPAddress ip = IPAddress.Parse(this.txtIP.Text.Trim());

            //创建端口号
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(this.txtPort.Text.Trim()));

            //绑定IP地址和端口号
            socketWatch.Bind(point);

            this.txtRec.AppendText("监听成功" + " \r \n");

            //开始监听:设置最大可以同时连接多少个请求
            socketWatch.Listen(10);

            //创建线程
            AcceptSocketThread = new Thread(new ParameterizedThreadStart(StartListen));
            AcceptSocketThread.IsBackground = true;
            AcceptSocketThread.Start(socketWatch);
        }
        /// <summary>
        /// 等待客户端的连接，并且创建与之通信用的Socket
        /// </summary>
        /// <param name="obj"></param>
        private void StartListen(object obj)
        {
            Socket socketWatch = obj as Socket;
            while (true)
            {
                //等待客户端的连接，并且创建一个用于通信的Socket
                socketSend = socketWatch.Accept();
                //获取远程主机的ip地址和端口号
                string strIp = socketSend.RemoteEndPoint.ToString();
                dicSocket.Add(strIp, socketSend);
                this.cbxSocket.Dispatcher.Invoke(()=> {
                    AddCmbItem(strIp);
                });
                string strMsg = "远程主机：" + socketSend.RemoteEndPoint + "连接成功";
                //使用回调
                this.txtRec.Dispatcher.Invoke(()=> {
                    ReceiveMsg(strMsg);
                });           
                //定义接收客户端消息的线程
                Thread threadReceive = new Thread(new ParameterizedThreadStart(Receive));
                threadReceive.IsBackground = true;
                threadReceive.Start(socketSend);
            }
        }
        /// <summary>
        /// 服务器端不停的接收客户端发送的消息
        /// </summary>
        /// <param name="obj"></param>
        private void Receive(object obj)
        {
            try
            {
                Socket socketSend = obj as Socket;
                while (true)
                {
                    //客户端连接成功后，服务器接收客户端发送的消息
                    byte[] buffer = new byte[2048];
                    //实际接收到的有效字节数
                    int count = socketSend.Receive(buffer);
                    if (count == 0)//count 表示客户端关闭，要退出循环
                    {
                        break;
                    }
                    else
                    {
                        string str = Encoding.Default.GetString(buffer, 0, count);
                        string strReceiveMsg = "接收：" + socketSend.RemoteEndPoint + "发送的消息:" + str;
                        this.txtRec.Dispatcher.Invoke(() =>
                        {
                            ReceiveMsg(strReceiveMsg);
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dicSocket = new Dictionary<string, Socket>();
                this.cbxSocket.Dispatcher.Invoke(()=> {
                    this.cbxSocket.Items.Clear();
                });                
                socketWatch?.Close();
                socketSend?.Close();
                //终止线程
                AcceptSocketThread?.Abort();
                threadReceive?.Abort();
            }
            catch(Exception ex)
            {
                MessageBox.Show("终止监听出错:" + ex.Message);
            }
            
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
                byte[] buffer = Encoding.Default.GetBytes(strMsg);
                //获得用户选择的IP地址
                string ip = this.cbxSocket.SelectedItem.ToString();
                dicSocket[ip].Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("给客户端发送消息出错:" + ex.Message);
            }
        }
        #region 回调委托需要执行的方法
        private void ReceiveMsg(string strMsg)
        {
            this.txtRec.AppendText(strMsg + " \r \n");
        }

        private void AddCmbItem(string strItem)
        {
            this.cbxSocket.Items.Add(strItem);
        }
        #endregion

    }
}
