using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameCaro
{
    public partial class frmCaroChess : Form
    {

        public static int cdStep = 100;
        public static int cdTime = 15000;
        public static int cdInterval = 100;
        private CaroChess caroChess;
        private Graphics grs;
        SocketManager socket;
        public frmCaroChess()
        {
            InitializeComponent();
            caroChess = new CaroChess();
            caroChess.CreateChessPieces();
            grs = pnlChessBoard.CreateGraphics();
            PvC.Click += PvC_Click;
            btnComputer.Click += PvC_Click;
            exitToolStripMenuItem.Click += btnExit_Click;
            socket = new SocketManager();
            prcbCoolDown.Step = cdStep;
            prcbCoolDown.Maximum = cdTime;
            prcbCoolDown.Value = 0;

            tmCoolDown.Interval = cdInterval;


        }

        private void PvC_Click(object sender, EventArgs e)
        {
            grs.Clear(pnlChessBoard.BackColor);
            caroChess.StartPvC(grs);
            prcbCoolDown.Value = 0;
            tmCoolDown.Start();
        }

        private void pnlGame_Paint(object sender, PaintEventArgs e)
        {
            caroChess.DrawChessBoard(grs);
            caroChess.RepaintChess(grs);
        }

        private void frmCaroChess_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(58, 58, 60);
            btnComputer.BackColor = this.BackColor;
            btnNewGame.BackColor = this.BackColor;
            btnLAN.BackColor = this.BackColor;
            txbIP.BackColor = this.BackColor;

            btn2Player.BackColor = this.BackColor;
            btnExit.BackColor = this.BackColor;
            pnlChessBoard.BackColor = Color.White;

        }

        private void pnlChessBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (!caroChess.Ready)
                return;
            if (caroChess.PlayChess(e.X, e.Y, grs))
            {
                if (caroChess.Mode == 1)
                {
                    if (caroChess.CheckWin())
                    {
                        tmCoolDown.Stop();
                        caroChess.EndGame();
                        return;
                    }
                }
                else if (caroChess.Mode == 2)
                {
                    caroChess.LaunchComputer(grs);
                    if (caroChess.CheckWin())
                    {
                        tmCoolDown.Stop();
                        caroChess.EndGame();
                        return;
                    }
                }
                else if (caroChess.Mode == 3)
                {
                    pnlChessBoard.Enabled = false;
                    
                    socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.Location));
                    
                    Listen();
                    if (caroChess.CheckWin())
                    {
                        tmCoolDown.Stop();
                        caroChess.EndGame();
                        return;
                    }
                }
            }
            tmCoolDown.Start();
            prcbCoolDown.Value = 0;
        }

        public void OtherPlayerMark(Point point)
        {
            if (!caroChess.Ready)
                return;
            if (caroChess.PlayChess(point.X, point.Y, grs))
            {
                pnlChessBoard.Enabled = true;
                if (caroChess.CheckWin())
                {
                    tmCoolDown.Stop();
                    caroChess.EndGame();
                }
            }
        }
        private void PvsP(object sender, EventArgs e)
        {
            grs.Clear(pnlChessBoard.BackColor);
            caroChess.StartPvP(grs);
            prcbCoolDown.Value = 0;
            tmCoolDown.Start();

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            caroChess.Undo(grs);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            caroChess.Redo(grs);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult dlr = MessageBox.Show("Do you want to exit!", "Exit", MessageBoxButtons.YesNo); ;
            if (caroChess.Mode != 3)
            {
                if (dlr == DialogResult.Yes)
                {
                   
                    Application.Exit();
                }
            }

            else
            {
                if (dlr == DialogResult.Yes)
                {
                    try
                    {
                        socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                    }
                    catch { }

                    Application.Exit();
                }
            }

        }
        private void NewGame()
        {
            grs.Clear(pnlChessBoard.BackColor);
            caroChess.StartLAN(grs);
            
            tmCoolDown.Start();
            prcbCoolDown.Value = 0;
        }
        private void btnNewGame_Click(object sender, EventArgs e)
        {

            if (caroChess.Mode == 0)
            {
                MessageBox.Show("Chưa chọn chế độ chơi!", "Thông báo");
            }
            else if (caroChess.Mode == 1)
            {
                grs.Clear(pnlChessBoard.BackColor);
                caroChess.StartPvP(grs);
            }
            else if(caroChess.Mode == 2)
            {
                grs.Clear(pnlChessBoard.BackColor);
                caroChess.StartPvC(grs);
            }
            else
            {   
                socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));
                grs.Clear(pnlChessBoard.BackColor);
                caroChess.StartLAN(grs);
                pnlChessBoard.Enabled = true;
            }
            tmCoolDown.Start();
            prcbCoolDown.Value = 0;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();
            frm.Show();
        }

        private void btnLAN_Click(object sender, EventArgs e)
        {
            grs.Clear(pnlChessBoard.BackColor);
            caroChess.StartLAN(grs);
            socket.IP = txbIP.Text;
            if (!socket.ConnectServer())
            {
                socket.isServer = true;
                pnlChessBoard.Enabled = true;
                socket.CreateServer();
            }
            else
            {             
                socket.isServer = false;
                pnlChessBoard.Enabled = false;
                Listen();
                MessageBox.Show("Kết nối thành công");
            }
            tmCoolDown.Stop();
            prcbCoolDown.Value = 0;
        }

        private void frmCaroChess_Shown(object sender, EventArgs e)
        {
            txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (string.IsNullOrEmpty(txbIP.Text))
            {
                txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
        }
        void Listen()
        {

            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();

                    ProcessData(data);
                }
                catch { }
            });
            listenThread.IsBackground = true;
            listenThread.Start();

        }

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;

                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlChessBoard.Enabled = false;
                    }));
                    break;

                case (int)SocketCommand.QUIT:
                    tmCoolDown.Stop();
                    MessageBox.Show("Người chơi đã thoát!");
                    caroChess.Ready = false;
                    break;

                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        prcbCoolDown.Value = 0;
                        tmCoolDown.Start();
                        OtherPlayerMark(data.Point);
                    }));

                    break;
                case (int)SocketCommand.END_GAME:
                    break;
                default:
                    break;
            }
            Listen();
        }

        private void tmCoolDown_Tick(object sender, EventArgs e)
        {
            prcbCoolDown.PerformStep();

            if (prcbCoolDown.Value >= prcbCoolDown.Maximum)
            {
                tmCoolDown.Stop();
                caroChess.EndGame();

            }
        }

    
        
    }
}
