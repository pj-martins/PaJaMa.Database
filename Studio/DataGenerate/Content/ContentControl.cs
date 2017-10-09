using PaJaMa.Database.DataGenerate.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.DataGenerate.Content
{
	public class ContentControl
	{
		public static void ShowPropertiesControl(ContentBase content)
		{
			frmContentBase frm = null;
			if (content is StringContent)
				frm = new frmStringContent();
			else if (content is NumericContent)
				frm = new frmNumericContent();
			else if (content is DateTimeContent)
				frm = new frmDateTimeContent();

			else throw new NotImplementedException();

			frm.Content = content;
			frm.ShowDialog();
		}
	}
}
