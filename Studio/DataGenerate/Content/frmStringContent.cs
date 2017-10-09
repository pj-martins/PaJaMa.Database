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
	public partial class frmStringContent : frmContentBase
	{
		public frmStringContent()
		{
			InitializeComponent();
		}

		protected override void btnOK_Click(object sender, EventArgs e)
		{
			(Content as StringContent).ContentType = (StringContentType)cboContentType.SelectedItem;
			base.btnOK_Click(sender, e);
		}

		private void frmStringContent_Load(object sender, EventArgs e)
		{
			foreach (var contentType in Enum.GetValues(typeof(StringContentType)).OfType<StringContentType>())
			{
				cboContentType.Items.Add(contentType);
			}

			cboContentType.SelectedItem = (Content as StringContent).ContentType;
		}
	}
}
