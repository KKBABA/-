using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.frmson;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		public List<SerialPort> serialPorts = new List<SerialPort>();
		public Form1()
		{
			InitializeComponent();

			Cmb_com.Items.Clear();
			Cmb_com.Items.AddRange(SerialPort.GetPortNames());
			Cmb_com.SelectedIndex=0;


			Cmb_btl.Items.Clear();
			Cmb_btl.Items.AddRange(new string[] { "9600", "38400" });
			Cmb_btl.SelectedIndex = 0;

			Cmb_jyw.Items.Clear();
			Cmb_jyw.Items.AddRange(new string[] { "无校验", "奇校验","偶校验" });
			Cmb_jyw.SelectedIndex = 0;

			Cmb_sjw.Items.Clear();
			Cmb_sjw.Items.AddRange(new string[] { "8", "7", "6", "5" });
			Cmb_sjw.SelectedIndex = 0;

			Cmb_tzw.Items.Clear();
			Cmb_tzw.Items.AddRange(new string[] { "1", "2", "0" });
			Cmb_tzw.SelectedIndex = 0;

			Cmb_gs.Items.Clear();
			Cmb_gs.Items.AddRange(new string[] { "字符串", "16进制" });
			Cmb_gs.SelectedIndex = 0;

			checkBox4.Checked = true;
			//byte b = 0x0F;
			//string ss = "0x0f";

			//byte.Parse(ss);
			//MessageBox.Show(Convert.ToByte(ss,16).ToString("X2"));

			
			DTTesterCode  = new DataTable();
			DTTesterCode.Columns.Add("品牌");
			DTTesterCode.Columns.Add("型号");
			DTTesterCode.Columns.Add("命令解释");
			DTTesterCode.Columns.Add("命令类型");
			DTTesterCode.Columns.Add("命令代码");

			DTTesterCode.Rows.Add("锐捷", "RJ6901A", "16#", "停止命令", "0x7B, 0x00, 0x08, 0x02, 0x0F, 0x00, 0x19, 0x7D");
			DTTesterCode.Rows.Add("锐捷", "RJ6901A", "16#", "启动命令", "0x7B, 0x00, 0x08, 0x02, 0x0F, 0xFF, 0x18, 0x7D");
			DTTesterCode.Rows.Add("锐捷", "RJ6901A", "16#", "读取命令", "0x7B, 0x00, 0x08, 0x02, 0xF0, 0x11, 0x0B, 0x7D");
			DTTesterCode.Rows.Add("锐捷", "RJ6901A", "16#", "设置命令", "0x7B, 0x00, 0x16, 0x02, 0x5A, 0x11, 0x00, 0x10, 0x00, 0xC8, 0x00, 0x0A, 0x00, 0x64, 0x00, 0x00, 0x00, 0xC8, 0x05, 0xDC, 0x72, 0x7D");

			DTTesterCode.Rows.Add("日置", "ST5520-01", "Sting", "启动命令", ":START");
			DTTesterCode.Rows.Add("日置", "ST5520-01", "Sting", "设置测试电压100V", ":VOLTage 100");
			DTTesterCode.Rows.Add("日置", "ST5520-01", "Sting", "设置测试时间1秒", ":TIMer 1");

			dataGridView1.DataSource = DTTesterCode;

		}

		public DataTable DTTesterCode=null;

		private void But_open_Click(object sender, EventArgs e)
		{
			if (checkBox1.Checked)
			{
				MessageBox.Show("请关闭打开所有端口号！！");
				return;
			}
			Button but = sender as Button;
			try
			{
				if (but.Text == "打开")
				{
					Parity jyw = Parity.None;
					switch (Cmb_jyw.Text)
					{
						case "无校验":
							jyw = Parity.None;
							break;
						case "奇校验":
							jyw = Parity.Odd;
							break;
						case "偶校验":
							jyw = Parity.Even;
							break;
					}
					StopBits stop = StopBits.One;
					switch (Cmb_tzw.Text)
					{
						case "0":
							stop = StopBits.None;
							break;
						case "1":
							stop = StopBits.One;
							break;
						case "2":
							stop = StopBits.OnePointFive;
							break;
					}
					SerialPort serialPort = new SerialPort(Cmb_com.Text, int.Parse(Cmb_btl.Text), jyw, int.Parse(Cmb_sjw.Text), stop);
					serialPort.DataReceived += SerialPort_DataReceived;

					if (serialPort.IsOpen == false)
					{
						serialPort.Open();
					}
					serialPorts.Add(serialPort);
					but.Text = "关闭";
					Lab_ts.Text = $"{serialPort.PortName} {serialPort.BaudRate} {serialPort.Parity} {serialPort.DataBits} {serialPort.StopBits}";
				}
				else
				{

					if (serialPorts.Count > 0 && serialPorts.First().IsOpen)
					{
						serialPorts.First().Close();
						serialPorts.First().Dispose();
						serialPorts.Clear();
					}
					but.Text = "打开";
					Lab_ts.Text = "";
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				but.Text = "打开";
				Lab_ts.Text = "";
			}
			

		}
		bool IsString = true;
		bool IsReadLine = true;
		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort port=sender as SerialPort;
			if (IsString)
			{
				string buffstring = port.PortName + "接收: ";
				buffstring += port.ReadExisting();
				PrintRead(buffstring, IsReadLine);
			}
			else
			{
				byte[] buff = new byte[port.BytesToRead];
				port.Read(buff, 0, buff.Length);
				string buffstring = port.PortName+"接收: ";
				for(int i = 0; i < buff.Length; i++)
				{
					buffstring += buff[i].ToString("X2")+" ";
				}
				PrintRead(buffstring, IsReadLine);
			}
			
		}

		public void PrintRead(string txt,bool isline)
		{
			if (Txb_read.InvokeRequired)
			{
				Txb_read.Invoke(new Action(() =>
				{
					if (isline)
					{
						Txb_read.Text += txt + "\r\n";
					}
					else
					{
						Txb_read.Text += txt;
					}
				}));
			}
			else
			{
				if (isline)
				{
					Txb_read.Text += txt + "\r\n";
				}
				else
				{
					Txb_read.Text += txt;
				}
			}
			
			

		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (checkBox1.Checked)
				{
					DisposSerialPortsList();
					Lab_ts.Text = "";
					string[] comarr=SerialPort.GetPortNames();
					for (int i = 0; i < comarr.Length; i++)
					{
						Parity jyw = Parity.None;
						switch (Cmb_jyw.Text)
						{
							case "无校验":
								jyw = Parity.None;
								break;
							case "奇校验":
								jyw = Parity.Odd;
								break;
							case "偶校验":
								jyw = Parity.Even;
								break;
						}
						StopBits stop = StopBits.One;
						switch (Cmb_tzw.Text)
						{
							case "0":
								stop = StopBits.None;
								break;
							case "1":
								stop = StopBits.One;
								break;
							case "2":
								stop = StopBits.OnePointFive;
								break;
						}
						SerialPort serialPort = new SerialPort(comarr[i], int.Parse(Cmb_btl.Text), jyw, int.Parse(Cmb_sjw.Text), stop);
						serialPort.DataReceived += SerialPort_DataReceived;
						try
						{
							if (serialPort.IsOpen == false)
							{

								serialPort.Open();
							}
							serialPorts.Add(serialPort);
							Lab_ts.Text += serialPort.PortName + " ";
						}catch(Exception ex)
						{
							MessageBox.Show( serialPort.PortName + "打开失败"+ex.Message);
						}
					}


				}
				else
				{
					DisposSerialPortsList();
					Lab_ts.Text = "";
				}
			}catch(Exception ex)
			{
				MessageBox.Show(ex.Message + "打开失败");
				Lab_ts.Text = "";
			}
		}

		public void InitSerial(string[] com)
		{
			try
			{
				
				DisposSerialPortsList();
				Lab_ts.Text = "";
				string[] comarr = com;
				for (int i = 0; i < comarr.Length; i++)
				{
					Parity jyw = Parity.None;
					switch (Cmb_jyw.Text)
					{
						case "无校验":
							jyw = Parity.None;
							break;
						case "奇校验":
							jyw = Parity.Odd;
							break;
						case "偶校验":
							jyw = Parity.Even;
							break;
					}
					StopBits stop = StopBits.One;
					switch (Cmb_tzw.Text)
					{
						case "0":
							stop = StopBits.None;
							break;
						case "1":
							stop = StopBits.One;
							break;
						case "2":
							stop = StopBits.OnePointFive;
							break;
					}
					SerialPort serialPort = new SerialPort(comarr[i], int.Parse(Cmb_btl.Text), jyw, int.Parse(Cmb_sjw.Text), stop);
					serialPort.DataReceived += SerialPort_DataReceived;
					try
					{
						if (serialPort.IsOpen == false)
						{

							serialPort.Open();
						}
						serialPorts.Add(serialPort);
						Lab_ts.Text += serialPort.PortName + " ";
					}
					catch (Exception ex)
					{
						MessageBox.Show(serialPort.PortName + "打开失败" + ex.Message);
					}
				}


				
				
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "打开失败");
				Lab_ts.Text = "";
			}
		}


		public bool DisposSerialPortsList()
		{
			if (serialPorts.Count > 0)
			{
				for(int i = 0; i < serialPorts.Count; i++)
				{
					if (serialPorts[i].IsOpen)
					{
						serialPorts[i].Close();
					}
					serialPorts[i].Dispose();
				}
				serialPorts.Clear();
			}
			return true;



		}

		private void Cmb_gs_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Cmb_gs.Text == "字符串")
			{
				IsString=true;
			}
			else
			{
				IsString = false;
			}
		}

		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox=sender as CheckBox;
			if (checkBox.Checked)
			{
				IsReadLine=true;
			}
			else
			{
				IsReadLine = false;
			}
		}


		public bool IsWirte16=false;
		bool IsWriteLine = false;
		private void button3_Click(object sender, EventArgs e)
		{
			try
			{
				if (serialPorts.Count > 0)
				{
					if (IsWirte16)
					{
						string write = Txb_write.Text.Replace("0x","").Replace("0X","").Trim();

						string[] warrstr = write.Split(' ');
						byte[] warr = new byte[warrstr.Length];
						byte by = 0;
						for (int i = 0; i < warrstr.Length; i++)
						{
							by = Convert.ToByte(warrstr[i],16);
							warr[i] = by;
						}
						for (int i = 0; i < serialPorts.Count; i++)
						{
							if (serialPorts[i].IsOpen)
							{
								serialPorts[i].Write(warr, 0, warr.Length);
								string wr = serialPorts[i].PortName+"发送：";
								for(int j=0;j< warr.Length; j++)
								{
									wr+=warr[j].ToString("X2")+" ";
								}
								PrintRead(wr, IsReadLine);
							}
						}
					}
					else
					{
						for (int i = 0; i < serialPorts.Count; i++)
						{
							if (serialPorts[i].IsOpen)
							{
								if (IsWriteLine)
								{
									serialPorts[i].Write(Txb_write.Text.Trim()+"\r\n");
								}
								else
								{
									serialPorts[i].Write(Txb_write.Text.Trim());
								}
								string wr = serialPorts[i].PortName + "发送："+ Txb_write.Text.Trim();
								PrintRead(wr, IsReadLine);
							}
						}
					}
				}
				else
				{
					MessageBox.Show("请先打开串口！");
				}
			}catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = sender as CheckBox;
			if (checkBox.Checked)
			{
				IsWriteLine = true;
			}
			else
			{
				IsWriteLine = false;
			}
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = sender as CheckBox;
			if (checkBox.Checked)
			{
				IsWirte16 = true;
			}
			else
			{
				IsWirte16 = false;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.Txb_read.Text = "";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.Txb_write.Text = "";
		}

		private void But_close_Click(object sender, EventArgs e)
		{
			Form_ComName form_ComName = new Form_ComName();

			if (checkBox1.Checked==false)
			{
				form_ComName.Openfunc += InitSerial;
				form_ComName.ShowDialog();
			}
			else
			{
				MessageBox.Show("请关闭打开所有串口");
			}
		}
	}




}
