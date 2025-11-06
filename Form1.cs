using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using RorzeComm.Log;
using RorzeUnit.Class;
using RorzeUnit.Class.Aligner;
using RorzeUnit.Interface;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using RorzeComm.Threading;
using AAComm;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace PanelAlignerTest4
{
    public partial class Form1 : Form
    {
        private bool _isTestMode = false;

        I_Aligner m_aligner;
        private SPollingThread _exePolling;
        private SLogger m_Logger = SLogger.GetLogger("CommunicationLog");
        private TcpListener _listener;
        private const int PORT = 8888;
        private volatile bool _stopCycle = false;

        private const string STATUS_BUSY = "10010/0000";
        private const string STATUS_READY = "11000/0000";

        public Form1()
        {
            InitializeComponent();
            m_aligner = new SSAlignerPanelXYR(1, false, false);

            _exePolling = new SPollingThread(10);
            _exePolling.DoPolling += _exe_DoPolling;

            this.Text += _isTestMode ? " --- [TEST MODE]" : " --- [REAL MODE]";
        }

        private void WriteLog(string strContent, [CallerMemberName] string meberName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string strMsg = string.Format("[ALN{0}] : {1}  at line {2} ({3})", 1, strContent, lineNumber, meberName);
            m_Logger.WriteLog(strMsg);
        }

        private void _exe_DoPolling()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => _exe_DoPolling_UI()));
            }
            else
            {
                _exe_DoPolling_UI();
            }
        }

        private void _exe_DoPolling_UI()
        {
            if (_isTestMode) return;

            try
            {
                int r_mabs = int.Parse(tB_MABS_R.Text);
                int x_mabs = int.Parse(tB_MABS_X.Text);
                int y_mabs = int.Parse(tB_MABS_Y.Text);
                Execute_MABS(r_mabs, x_mabs, y_mabs);
                if (m_aligner.IsError) return;

                int r_mrel = int.Parse(tB_MREL_R.Text);
                int x_mrel = int.Parse(tB_MREL_X.Text);
                int y_mrel = int.Parse(tB_MREL_Y.Text);
                Execute_MRAL(r_mrel, x_mrel, y_mrel);
                if (m_aligner.IsError) return;
            }
            catch (Exception ex)
            {
                WriteLog($"_exe_DoPolling Error: {ex.Message}");
            }
        }

        private void UpdateButtonState(Button btn, bool enabled)
        {
            if (btn == null) return;

            if (btn.InvokeRequired)
            {
                btn.Invoke(new Action(() => btn.Enabled = enabled));
            }
            else
            {
                btn.Enabled = enabled;
            }
        }

        #region Refactored Execute Logic

        private string Execute_STAT()
        {
            try
            {
                return STATUS_READY;
            }
            catch (Exception ex)
            {
                WriteLog($"Execute_STAT Error: {ex.Message}");
                return "Error/Unknown";
            }
        }

        private string Execute_GPOS(string axis)
        {
            try
            {
                if (_isTestMode)
                {
                    return "12345";
                }

                WriteLog($"Execute_GPOS: Axis '{axis}' is not implemented yet. Returning 0.");
                return "0";
            }
            catch (Exception ex)
            {
                WriteLog($"Execute_GPOS Error: {ex.Message}");
                return "-1";
            }
        }

        // (所有 Execute_ 函式都返回 bool)
        private bool Execute_ALGN()
        {
            try
            {
                UpdateButtonState(bt_ALGN, false);
                m_aligner.ResetInPos();
                m_aligner.ALGN(1);
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_ALGN, true);
            }
        }

        private bool Execute_ORGN()
        {
            try
            {
                UpdateButtonState(bt_ORGN, false);
                m_aligner.ResetInPos();
                m_aligner.ORGN();
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_ORGN, true);
            }
        }

        private bool Execute_R_EXTD(int distance)
        {
            try
            {
                UpdateButtonState(bt_R_EXTD, false);
                m_aligner.ResetInPos();
                m_aligner.Rot1EXTD(distance);
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_R_EXTD, true);
            }
        }

        private bool Execute_R_STEP(int distance)
        {
            try
            {
                UpdateButtonState(bt_R_STEP, false);
                m_aligner.ResetInPos();
                m_aligner.Rot1STEP(distance);
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_R_STEP, true);
            }
        }

        private bool Execute_INIT()
        {
            try
            {
                UpdateButtonState(bt_INIT, false);
                m_aligner.ResetInPos();
                m_aligner.INIT();
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_INIT, true);
            }
        }

        private bool Execute_RSTA()
        {
            try
            {
                UpdateButtonState(bt_RSTA, false);
                m_aligner.ResetInPos();
                m_aligner.RSTA(1);
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_RSTA, true);
            }
        }

        private bool Execute_MRAL(int r, int x, int y)
        {
            try
            {
                WriteLog($"Execute_MRAL:Start (R:{r}, X:{x}, Y:{y})");
                UpdateButtonState(bt_MRAL, false);
                m_aligner.AlignerMoveTest_MREL(r, x, y);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_MRAL, true);
            }
        }

        private bool Execute_MABS(int r, int x, int y)
        {
            try
            {
                WriteLog($"Execute_MABS:Start (R:{r}, X:{x}, Y:{y})");
                UpdateButtonState(bt_MABS, false);
                m_aligner.ResetInPos();
                m_aligner.AlignerMoveTest_MABS(r, x, y);
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_MABS, true);
            }
        }

        private bool Execute_STOP()
        {
            try
            {
                UpdateButtonState(bt_STOP, false);
                m_aligner.ResetInPos();
                m_aligner.STOP();
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_STOP, true);
                _stopCycle = true;
            }
        }

        private bool Execute_CLMP()
        {
            try
            {
                UpdateButtonState(bt_CLMP, false);
                m_aligner.ResetInPos();
                m_aligner.CLMP();
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_CLMP, true);
            }
        }

        private bool Execute_UCLM()
        {
            try
            {
                UpdateButtonState(bt_UCLM, false);
                m_aligner.ResetInPos();
                m_aligner.UCLM();
                m_aligner.WaitInPos(30000);
                return true;
            }
            catch (SException ex) { WriteLog("<<SException>>" + ex); return false; }
            catch (Exception ex) { WriteLog("<<Exception>>" + ex); return false; }
            finally
            {
                UpdateButtonState(bt_UCLM, true);
            }
        }

        #endregion

        #region Button Click Events (已修正：移除 Task.Run)

        private void bt_ALGN_Click(object sender, EventArgs e) { Execute_ALGN(); }
        private void bt_ORGN_Click(object sender, EventArgs e) { Execute_ORGN(); }
        private void bt_INIT_Click(object sender, EventArgs e) { Execute_INIT(); }
        private void bt_RSTA_Click(object sender, EventArgs e) { Execute_RSTA(); }
        private void bt_STOP_Click(object sender, EventArgs e) { Execute_STOP(); }
        private void bt_CLMP_Click(object sender, EventArgs e) { Execute_CLMP(); }
        private void bt_UCLM_Click(object sender, EventArgs e) { Execute_UCLM(); }
        private void bt_Cycle_Click(object sender, EventArgs e) { _exePolling.Set(); }

        private void bt_R_EXTD_Click(object sender, EventArgs e)
        {
            try
            {
                int distance = int.Parse(tB_EXTD_Distance.Text);
                Execute_R_EXTD(distance);
            }
            catch (Exception ex) { WriteLog($"Invalid R_EXTD input: {ex.Message}"); }
        }

        private void bt_R_STEP_Click(object sender, EventArgs e)
        {
            try
            {
                int distance = int.Parse(tB_EXTD_Distance.Text);
                Execute_R_STEP(distance);
            }
            catch (Exception ex) { WriteLog($"Invalid R_STEP input: {ex.Message}"); }
        }

        private void bt_MRAL_Click(object sender, EventArgs e)
        {
            try
            {
                int r = int.Parse(tB_MREL_R.Text);
                int x = int.Parse(tB_MREL_X.Text);
                int y = int.Parse(tB_MREL_Y.Text);
                Execute_MRAL(r, x, y);
            }
            catch (Exception ex) { WriteLog($"Invalid MRAL input: {ex.Message}"); }
        }

        private void bt_MABS_Click(object sender, EventArgs e)
        {
            try
            {
                int r = int.Parse(tB_MABS_R.Text);
                int x = int.Parse(tB_MABS_X.Text);
                int y = int.Parse(tB_MABS_Y.Text);
                Execute_MABS(r, x, y);
            }
            catch (Exception ex) { WriteLog($"Invalid MABS input: {ex.Message}"); }
        }

        private void bt_Cycle_Click_1(object sender, EventArgs e)
        {
            _stopCycle = false;
            Task.Run(() =>
            {
                WriteLog("Cycle Test Started...");
                try
                {
                    int r_mrel = 0, x_mrel = 0, y_mrel = 0;
                    int r_mabs = 0, x_mabs = 0, y_mabs = 0;
                    this.Invoke(new Action(() => {
                        r_mrel = int.Parse(tB_MREL_R.Text);
                        x_mrel = int.Parse(tB_MREL_X.Text);
                        y_mrel = int.Parse(tB_MREL_Y.Text);
                        r_mabs = int.Parse(tB_MABS_R.Text);
                        x_mabs = int.Parse(tB_MABS_X.Text);
                        y_mabs = int.Parse(tB_MABS_Y.Text);
                    }));

                    while (true)
                    {
                        if (_stopCycle || m_aligner.IsError) { break; }

                        if (_isTestMode)
                        {
                            WriteLog("Cycle Test run skipped in Test Mode.");
                            break;
                        }

                        this.Invoke(new Func<bool>(Execute_ORGN));
                        Thread.Sleep(300);
                        if (_stopCycle || m_aligner.IsError) break;

                        this.Invoke(new Func<bool>(Execute_UCLM));
                        Thread.Sleep(300);
                        if (_stopCycle || m_aligner.IsError) break;

                        this.Invoke(new Func<bool>(Execute_CLMP));
                        Thread.Sleep(300);
                        if (_stopCycle || m_aligner.IsError) break;

                        this.Invoke(new Func<bool>(() => Execute_MRAL(r_mrel, x_mrel, y_mrel)));
                        if (_stopCycle || m_aligner.IsError) break;

                        this.Invoke(new Func<bool>(() => Execute_MABS(r_mabs, x_mabs, y_mabs)));
                        Thread.Sleep(300);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Cycle Test Error (Invalid UI Values?): {ex.Message}");
                }
                WriteLog("Cycle Test Stopped. StopFlag=" + _stopCycle + ", IsError=" + m_aligner.IsError);
                _stopCycle = false;
            });
        }

        #endregion

        #region Form Load/Closing Events

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(() => StartSocketServer());
            WriteLog($"Socket server started on port {PORT}");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _listener?.Stop();
            _stopCycle = true;
            CommAPI m_MCtrlComm = new CommAPI();
            m_MCtrlComm.CloseAACommServer(true);
        }

        #endregion

        #region Async Task Completion Wrapper

        private void RunTaskWithCompletion(NetworkStream stream, string alignerName, string cmdName, string axis, Func<bool> taskToRun)
        {
            string baseCmd = $"{(string.IsNullOrEmpty(axis) ? "" : axis + ".")}{cmdName}";

            this.BeginInvoke(new Action(async () =>
            {
                string reply;
                try
                {
                    bool success = taskToRun();

                    if (success)
                    {
                        reply = $"a{alignerName}.STAT:{STATUS_READY}\r";
                    }
                    else
                    {
                        //reply = $"E{alignerName}.{baseCmd}:3\r";
                        reply = $"a{alignerName}.STAT:{STATUS_READY}\r";
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"RunTaskWithCompletion/UI Invoke failed: {ex.Message}");
                    reply = $"E{alignerName}.{baseCmd}:3\r";
                }

                try
                {
                    // [NEW LOG]
                    WriteLog($"Sending Task Completion Reply: {reply.TrimEnd('\r')}");
                    byte[] replyBytes = Encoding.ASCII.GetBytes(reply);
                    await stream.WriteAsync(replyBytes, 0, replyBytes.Length);
                }
                catch (IOException) { WriteLog("Client disconnected before .END command could be sent."); }
                catch (Exception ex) { WriteLog($"Failed to send .END command: {ex.Message}"); }
            }));
        }

        #endregion


        #region Socket Server Logic

        private async void StartSocketServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PORT);
                _listener.Start();

                while (true)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    WriteLog($"Client connected: {client.Client.RemoteEndPoint}");
                    Task.Run(() => HandleClient(client)).ConfigureAwait(false);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                WriteLog("Socket server stopped.");
            }
            catch (Exception ex)
            {
                WriteLog($"Socket server error: {ex.Message}");
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            StringBuilder sb = new StringBuilder();
            var stream = client.GetStream();
            var buffer = new byte[1024];

            try
            {
                string welcome = "oALGN.CNCT:\r";
                WriteLog($"Sending Welcome: {welcome.TrimEnd('\r')}");
                byte[] welcomeMsg = Encoding.ASCII.GetBytes(welcome);
                await stream.WriteAsync(welcomeMsg, 0, welcomeMsg.Length);

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    sb.Append(receivedData);
                    string content = sb.ToString();

                    if (content.Contains("\\r"))
                    {
                        content = content.Replace("\\r", "\r");
                    }

                    int index;
                    while ((index = content.IndexOf('\r')) != -1)
                    {
                        string command = content.Substring(0, index);
                        content = content.Substring(index + 1);

                        command = command.Trim();
                        if (!string.IsNullOrEmpty(command))
                        {
                            WriteLog($"Received command: {command}");

                            if (command.ToUpper() == "EXIT")
                            {
                                client.Close();
                                return;
                            }

                            List<string> immediateReplies = ProcessSocketCommand_Adapter(command, stream);

                            foreach (var reply in immediateReplies)
                            {
                                WriteLog($"Sending Immediate Reply: {reply}");
                                byte[] replyBytes = Encoding.ASCII.GetBytes(reply + "\r");
                                await stream.WriteAsync(replyBytes, 0, replyBytes.Length);

                                await Task.Delay(600); 
                            }
                        }
                    }
                    sb.Clear();
                    sb.Append(content);
                }
            }
            catch (IOException)
            {
                WriteLog($"Client disconnected (IOException): {client.Client.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                WriteLog($"Error handling client {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                client.Close();
                WriteLog($"Client connection closed: {client.Client.RemoteEndPoint}");
            }
        }

        private int ParseParameter(string command)
        {
            try
            {
                int pOpen = command.IndexOf('(');
                if (pOpen == -1) return int.MinValue;
                int pClose = command.IndexOf(')');
                if (pClose == -1) return int.MinValue;
                string allParams = command.Substring(pOpen + 1, pClose - pOpen - 1);
                if (string.IsNullOrEmpty(allParams)) return int.MinValue;
                string firstParam = allParams.Split(',')[0].Trim();
                if (int.TryParse(firstParam, out int result))
                {
                    return result;
                }
                return int.MinValue;
            }
            catch (Exception ex)
            {
                WriteLog($"ParseParameter Error: {ex.Message} (Command: {command})");
                return int.MinValue;
            }
        }


        private List<string> ProcessSocketCommand_Adapter(string command, NetworkStream stream) // 回傳 List<string>
        {
            string cleanCommand = command.Trim().TrimEnd(':');
            string alignerName = "UNKNOWN";
            string commandName = "UNKNOWN";
            string axis = "";

            try
            {
                string[] parts = cleanCommand.Split('.');
                if (parts.Length < 2)
                {
                    WriteLog($"Invalid command structure: {command}");
                    return new List<string> { "ERROR: Invalid command format" };
                }
                if (parts[0].Length > 1)
                    alignerName = parts[0].Substring(1).ToUpper();
                if (parts.Length == 2)
                {
                    commandName = parts[1].Split('(')[0].ToUpper();
                }
                else if (parts.Length == 3)
                {
                    axis = parts[1].ToUpper();
                    commandName = parts[2].Split('(')[0].ToUpper();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Command initial parse error: {ex.Message} (Command: {command})");
                return new List<string> { "ERROR: Command parse error" };
            }

            try
            {
                string busyReply = $"a{alignerName}.STAT:{STATUS_BUSY}";

                switch (commandName)
                {
                    case "STAT":
                        // [NEW LOG]
                        WriteLog("Executing STAT query...");
                        string statusPayload = (string)this.Invoke(new Func<string>(Execute_STAT));
                        return new List<string> { $"a{alignerName}.STAT:{statusPayload}" };

                    case "GPOS":
                        // [NEW LOG]
                        WriteLog($"Executing GPOS query for axis {axis}...");
                        string gposPayload = (string)this.Invoke(new Func<string>(() => Execute_GPOS(axis)));
                        return new List<string> { $"a{alignerName}.{axis}.GPOS:{gposPayload}" };

                    case "STEP":
                        string stepAckReply;
                        string stepAxisForReply;

                        int value = ParseParameter(cleanCommand);
                        if (value == int.MinValue) return new List<string> { $"E{alignerName}.{axis}.STEP:1" };

                        int r = 0, x = 0, y = 0;

                        // 模擬rot指令
                        if (axis == "ROT1")
                        {
                            r = value;
                            stepAckReply = $"a{alignerName}.ROT.STEP:";
                            stepAxisForReply = "ROT";
                            WriteLog($"Executing Robot simulator");
                            return new List<string> {stepAckReply};
                        }
                        else if(axis == "XAX1" || axis == "YAX1")
                        {
                            stepAckReply = $"a{alignerName}.ROT.STEP:";
                            stepAxisForReply = "";
                            WriteLog($"Executing X,Y simulator");
                            return new List<string> { stepAckReply };
                        }
                        else if (axis == "X")
                        {
                            x = value;
                            stepAckReply = $"a{alignerName}.X.STEP:";
                            stepAxisForReply = "X";
                        }
                        else if (axis == "Y")
                        {
                            y = value;
                            stepAckReply = $"a{alignerName}.Y.STEP:";
                            stepAxisForReply = "Y";
                        }
                        else if (axis == "R")
                        {
                            r = value;
                            stepAckReply = $"a{alignerName}.R.STEP:";
                            stepAxisForReply = "R";
                        }
                        else
                        {
                            WriteLog($"Unknown axis in STEP command: {axis}");
                            return new List<string> { $"E{alignerName}.{axis}.STEP:1" };
                        }

                        // [NEW LOG]
                        WriteLog($"Executing STEP (MRAL): R={r}, X={x}, Y={y}");
                        RunTaskWithCompletion(stream, alignerName, commandName, stepAxisForReply, () => Execute_MRAL(r, x, y));
                        return new List<string> { stepAckReply, busyReply };

                    case "EXTD":
                        WriteLog($"Unsupported command: {command}. EXTD commands are not handled.");
                        return new List<string> { $"E{alignerName}.{axis}.EXTD:1g" };

                    case "ORGN":
                        string orgnAck = $"a{alignerName}.ORGN:";
                        if (_isTestMode)
                        {
                            WriteLog("Simulating ORGN (Test Mode)...");
                            Func<bool> fakeOrgnTask = () => { Thread.Sleep(1000); return true; };
                            RunTaskWithCompletion(stream, alignerName, commandName, "", fakeOrgnTask);
                        }
                        else
                        {
                            // [NEW LOG]
                            WriteLog("Executing ORGN (Real Mode)...");
                            RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_ORGN());
                        }
                        return new List<string> { orgnAck, busyReply };

                    case "ALGN":
                        string algnAck = $"a{alignerName}.ALGN:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing ALGN (Test Mode): Skipping.");
                            return new List<string> { algnAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing ALGN (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_ALGN());
                        return new List<string> { algnAck, busyReply };

                    case "CLMP":
                        string clmpAck = $"a{alignerName}.CLMP:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing CLMP (Test Mode): Skipping.");
                            return new List<string> { clmpAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing CLMP (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_CLMP());
                        return new List<string> { clmpAck, busyReply };

                    case "UCLM":
                        string uclmAck = $"a{alignerName}.UCLM:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing UCLM (Test Mode): Skipping.");
                            return new List<string> { uclmAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing UCLM (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_UCLM());
                        return new List<string> { uclmAck, busyReply };

                    case "STOP":
                        string stopAck = $"a{alignerName}.STOP:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing STOP (Test Mode): Skipping.");
                            return new List<string> { stopAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing STOP (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_STOP());
                        return new List<string> { stopAck, busyReply };

                    case "INIT":
                        string initAck = $"a{alignerName}.INIT:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing INIT (Test Mode): Skipping.");
                            return new List<string> { initAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing INIT (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_INIT());
                        return new List<string> { initAck, busyReply };

                    case "RSTA":
                        string rstaAck = $"a{alignerName}.RSTA:";
                        if (_isTestMode)
                        {
                            WriteLog("Executing RSTA (Test Mode): Skipping.");
                            return new List<string> { rstaAck };
                        }
                        // [NEW LOG]
                        WriteLog("Executing RSTA (Real Mode)...");
                        RunTaskWithCompletion(stream, alignerName, commandName, "", () => Execute_RSTA());
                        return new List<string> { rstaAck, busyReply };

                    default:
                        WriteLog($"Unknown command: {command}");
                        return new List<string> { $"E{alignerName}.{commandName}:1" };
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Command Processing Error: {ex.Message} (Command: {command})");
                return new List<string> { "ERROR: Internal server error." };
            }
        }


        #endregion
    }
}