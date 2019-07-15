using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using MapTools;
using System.Runtime.InteropServices;

namespace XBIMApp
{
    public partial class axFormField : Form
    {
        string wallFileName = string.Empty;
        string doorFileName = string.Empty;
        public axFormField()
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
            AxIndoorIfcCreatorField creator = new AxIndoorIfcCreatorField();
            creator.setWallFile(wallFileName);
            creator.setDoorFile(doorFileName);
            creator.setcheckDoorCreate(checkBox1.Checked);
            creator.setDist_Wall_Threshold(door_Dist_Wall_Threshold * 1000);
            string filename = "IfcWallWithDoors_XXX.ifc";
            creator.CreateBuilding(filename);
        }
    }
}
