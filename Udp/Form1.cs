using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Threading;

namespace Udp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// 
        /// 用于UDP发送的网络服务类
        /// 
        private UdpClient udpcSend;
        private string LocalIP, RemoteIP;
        private int LocalPort, RemotePort;
        /// 
        /// 用于UDP接收的网络服务类
        /// 
        private UdpClient udpcRecv;

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "192.168.1.3";//127.0.0.1也可以指的本机 192.168.1.3 是本地电脑ip
            textBox2.Text = "192.168.1.3";
            //textBox3.Text = "8847";
            // textBox4.Text = "8848";
            CheckForIllegalCrossThreadCalls = false;

            LocalIP = textBox1.Text;
            RemoteIP = textBox2.Text;
           // LocalPort = (Int32)textBox3.Text;
           // RemotePort = textBox4.Text;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("请先输入待发送内容");
                return;
            }

            // 匿名发送
            //udpcSend = new UdpClient(0);             // 自动分配本地IPv4地址
            // 实名发送
             IPEndPoint localIpep = new IPEndPoint(IPAddress.Parse(LocalIP), 9001); // 本机IP，指定的端口号
             udpcSend = new UdpClient(localIpep);
           // udpcSend = new UdpClient(0);
            Thread thrSend = new Thread(SendMessage);
            thrSend.Start(richTextBox1.Text);
        }
        private void SendMessage(object obj)
        {
            string message = (string)obj;
            byte[] sendbytes = Encoding.Unicode.GetBytes(message);
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(RemoteIP), 8008); // 发送到的IP地址和端口号
            udpcSend.Send(sendbytes, sendbytes.Length, remoteIpep);
            udpcSend.Close();
            ResetTextBox(richTextBox1);
        }

        /// <summary>
        /// 开关：在监听UDP报文阶段为true，否则为false
        /// </summary>
        bool IsUdpcRecvStart = false;
        /// <summary>
        /// 线程：不断监听UDP报文
        /// </summary>
        Thread thrRecv;


        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsUdpcRecvStart) // 未监听的情况，开始监听
            {
                IPEndPoint localIpep = new IPEndPoint(IPAddress.Parse(RemoteIP), 8008); // 本机IP和监听端口号
                udpcRecv = new UdpClient(localIpep);
                thrRecv = new Thread(ReceiveMessage);
                thrRecv.Start();
                IsUdpcRecvStart = true;
               // MessageBox.Show("1123");
                ShowMessage(richTextBox2, "UDP监听器已成功启动");
            }
            else // 正在监听的情况，终止监听
            {
                thrRecv.Abort(); // 必须先关闭这个线程，否则会异常
                udpcRecv.Close();
                IsUdpcRecvStart = false;
                ShowMessage(richTextBox2, "UDP监听器已成功关闭");
            }

        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="obj"></param>
        private void ReceiveMessage(object obj)
        {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            //实例化一个远程端点，ip和端口可以随意指定，等调用client.Receive时会将该端点改成真正发送端端口

            while (true)
            {
                try
                {

                    byte[] bytRecv = udpcRecv.Receive(ref remoteIpep);
                    string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                    ShowMessage(richTextBox2, string.Format("{0}[{1}]", remoteIpep, message));
                }
                catch (Exception ex)
                {
                    ShowMessage(richTextBox2, ex.Message);
                    break;
                }
            }
        }
        // 向RichTextBox中添加文本
        delegate void ShowMessageDelegate(RichTextBox txtbox, string message);
        private void ShowMessage(RichTextBox txtbox, string message)
        {
            if (txtbox.InvokeRequired)
            {
                ShowMessageDelegate showMessageDelegate = ShowMessage;
                txtbox.Invoke(showMessageDelegate, new object[] { txtbox, message });
            }
            else
            {
                txtbox.Text += message + "\r\n";
            }
        }

        // 清空指定RichTextBox中的文本
        delegate void ResetTextBoxDelegate(RichTextBox txtbox);
        private void ResetTextBox(RichTextBox txtbox)
        {
            if (txtbox.InvokeRequired)
            {
                ResetTextBoxDelegate resetTextBoxDelegate = ResetTextBox;
                txtbox.Invoke(resetTextBoxDelegate, new object[] { txtbox });
            }
            else
            {
                txtbox.Text = "";
            }
        }

        /// <summary>
        /// 关闭程序，强制退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

       
    }



    
}
