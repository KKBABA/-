using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.frmson
{
	public partial class Form_ComName : Form
	{
		public Form_ComName()
		{
			InitializeComponent();
			string[] comname = SerialPort.GetPortNames();

			for(int i = 0; i < comname.Length; i++)
			{
				CheckBox check = new CheckBox();
				check.Text = comname[i];
				flowLayoutPanel1.Controls.Add(check);
			}

		}
		public Action<string[]> Openfunc;
		private void button2_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.No;
			
		}

		private void button1_Click(object sender, EventArgs e)
		{
			List<string> strings = new List<string>() ;
			foreach(var item  in flowLayoutPanel1.Controls)
			{
				if(item is CheckBox)
				{
					CheckBox checkBox = item as CheckBox;
					if (checkBox.Checked)
					{
						strings.Add(checkBox.Text);
					}
				}
			}
			Openfunc?.Invoke(strings.ToArray());
			this.DialogResult = DialogResult.OK;
		}
	}
}
