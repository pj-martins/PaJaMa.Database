using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PaJaMa.Database.DataGenerate.Content;

namespace PaJaMa.Database.Studio.DataGenerate.Content
{
	public partial class frmContentBase : Form
	{
		public ContentBase Content { get; set; }

		public frmContentBase()
		{
			InitializeComponent();
		}

		protected virtual void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
		}
	}
}
