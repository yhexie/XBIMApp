using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapTools;
using System.Runtime.InteropServices;

namespace XBIMApp
{
    public partial class Ifc2x3frm : Form
    {
        string wallFileName = string.Empty;
        string doorFileName = string.Empty;
        public Ifc2x3frm()
        {
            InitializeComponent();
        }

        private void btnChooseWallLines_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "打开墙线文件";
            dlg.Filter = "(*.shp)|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK && dlg.FileName != String.Empty)
            {
                textBox1.Text = dlg.FileName;
                wallFileName = dlg.FileName;
               
            }
        }
      
        private void btnChooseDoor_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "打开墙线文件";
            dlg.Filter = "(*.shp)|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK && dlg.FileName != String.Empty)
            {
                textBox2.Text = dlg.FileName;
                doorFileName = dlg.FileName;         
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            double door_Dist_Wall_Threshold=(double)numericUpDown1.Value;
            AxIndoorIfcCreatorIfc2x3 creator = new AxIndoorIfcCreatorIfc2x3();
            creator.setWallFile(wallFileName);
            creator.setDoorFile(doorFileName);
            creator.setDist_Wall_Threshold(door_Dist_Wall_Threshold * 1000);
            string filename = "IfcWallWithDoors_XXX.ifc";
            creator.CreateBuilding(filename);
        }
    }
}
