using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Classes
{
    public class MenuHelper
    {
        public static void CreateFileMenu(UserControl ctrl)
        {
            var mnuStrip = ctrl.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (mnuStrip == null)
            {
                mnuStrip = new MenuStrip();
                ctrl.Controls.Add(mnuStrip);
            }

            var fileMenu = mnuStrip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Text.Contains("File"));
            if (fileMenu == null)
            {
                fileMenu = new ToolStripMenuItem("&File");
                mnuStrip.Items.Insert(0, fileMenu);
            }

            var connMenu = new ToolStripMenuItem("Manage &Connections");
            connMenu.Click += (object sender, EventArgs e) => new frmConnections().ShowDialog();
            fileMenu.DropDownItems.Add(connMenu);
        }
    }
}
