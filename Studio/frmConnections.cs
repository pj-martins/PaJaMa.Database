using PaJaMa.Database.Library;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio
{
    public partial class frmConnections : Form
    {
        public static event EventHandler ConnectionsChanged;

        public frmConnections()
        {
            InitializeComponent();
        }

        private void frmConnections_Load(object sender, EventArgs e)
        {
            cboDataSource.Items.AddRange(DataSource.GetDataSourceTypes().Select(t => new TypeDisplay
            {
                Type = t,
                ShortName = t.FullName.Substring(t.FullName.LastIndexOf('.') + 1)
            }).ToArray());
            var connections = DatabaseConnection.GetConnections();
            gridMain.AutoGenerateColumns = false;
            gridMain.DataSource = new BindingList<DatabaseConnection>(connections.OrderBy(x => x.ConnectionName).ToList());
            enableDisableControls();
        }

        private void gridMain_SelectionChanged(object sender, EventArgs e)
        {
            var selection = gridMain.SelectedRows.Count > 0 ? gridMain.SelectedRows[0] : null;
            if (selection != null)
            {
                var conn = (DatabaseConnection)selection.DataBoundItem;
                txtConnectionName.Text = conn.ConnectionName;
                txtServer.Text = conn.Server;
                numPort.Value = conn.Port;
                txtDatabase.Text = conn.Database;
                txtUser.Text = conn.UserName;
                txtPassword.Text = conn.Password;
                txtAppend.Text = conn.Append;
                txtTunnel.Text = conn.Tunnel;
                numTunnelPort.Value = conn.TunnelPort;
                txtTunnelKey.Text = conn.TunnelKeyFile;
                chkIntegratedSecurity.Checked = conn.IntegratedSecurity;
                if (!string.IsNullOrEmpty(conn.DataSourceType))
                {
                    cboDataSource.SelectedItem = cboDataSource.Items.OfType<TypeDisplay>().First(x => x.Type.FullName == conn.DataSourceType);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this connection?", "Remove Connection", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                gridMain.Rows.Remove(gridMain.SelectedRows[0]);
                enableDisableControls();
                save();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            (gridMain.DataSource as BindingList<DatabaseConnection>).Add(new DatabaseConnection());
            gridMain.Rows[gridMain.Rows.Count - 1].Selected = true;
            enableDisableControls();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var selectedItem = gridMain.SelectedRows[0].DataBoundItem as DatabaseConnection;
            selectedItem.ConnectionName = txtConnectionName.Text;
            selectedItem.Server = txtServer.Text;
            selectedItem.Port = (int)numPort.Value;
            selectedItem.Database = txtDatabase.Text;
            selectedItem.UserName = txtUser.Text;
            selectedItem.Password = txtPassword.Text;
            selectedItem.Append = txtAppend.Text;
            selectedItem.IntegratedSecurity = chkIntegratedSecurity.Checked;
            selectedItem.Tunnel = txtTunnel.Text;
            selectedItem.TunnelPort = (int)numTunnelPort.Value;
            selectedItem.TunnelKeyFile = txtTunnelKey.Text;
            selectedItem.DataSourceType = (cboDataSource.SelectedItem as TypeDisplay).Type.FullName;
            save();
            gridMain.Invalidate();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var selectedItem = gridMain.SelectedRows[0].DataBoundItem as DatabaseConnection;
            var clone = new DatabaseConnection()
            {
                ConnectionName = selectedItem.ConnectionName + " - Copy",
                Server = selectedItem.Server,
                Port = selectedItem.Port,
                Database = selectedItem.Database,
                DataSourceType = selectedItem.DataSourceType,
                UserName = selectedItem.UserName,
                Password = selectedItem.Password,
                IntegratedSecurity = selectedItem.IntegratedSecurity,
                Append = selectedItem.Append,
                Tunnel = selectedItem.Tunnel,
                TunnelPort = selectedItem.TunnelPort,
                TunnelKeyFile = selectedItem.TunnelKeyFile
            };
            (gridMain.DataSource as BindingList<DatabaseConnection>).Add(clone);
            gridMain.Rows[gridMain.Rows.Count - 1].Selected = true;
        }

        private void save()
        {
            var newConnections = gridMain.Rows.OfType<DataGridViewRow>().Select(x => x.DataBoundItem as DatabaseConnection).ToList();
            DatabaseConnection.SetConnections(newConnections);
            ConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private void enableDisableControls()
        {
            bool enabled = gridMain.Rows.Count > 0;
            txtConnectionName.Enabled =
                cboDataSource.Enabled =
                txtServer.Enabled =
                numPort.Enabled =
                txtDatabase.Enabled =
                txtUser.Enabled =
                txtPassword.Enabled =
                txtAppend.Enabled =
                chkIntegratedSecurity.Enabled =
                btnCopy.Enabled =
                btnSave.Enabled =
                btnRemove.Enabled =
                txtTunnel.Enabled =
                txtTunnelKey.Enabled =
                    enabled;
        }

        private void txtServer_TextChanged(object sender, EventArgs e)
        {
            gridMain.Invalidate();
        }

        private void btnBrowseKey_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtTunnelKey.Text = dlg.FileName;
            }
        }
    }

    public class TypeDisplay
    {
        public string ShortName { get; set; }
        public Type Type { get; set; }
    }
}
