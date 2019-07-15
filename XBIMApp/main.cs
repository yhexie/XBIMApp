using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xbim.Common.Step21;
using Xbim.Ifc;

namespace XBIMApp
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        private void TspCreateIFC_Click(object sender, EventArgs e)
        {
            axForm frm = new axForm();
            frm.ShowDialog();
        }

        private void TspMergeIFC_Click(object sender, EventArgs e)
        {
            dlgMergeTwoIfc dlg = new dlgMergeTwoIfc();
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                String fileName1 = dlg.ifcFileName1;
                String fileName2 = dlg.ifcFileName2;
                var editor = new XbimEditorCredentials
                {
                    ApplicationDevelopersName = "Yhexie",
                    ApplicationFullName = "Your app",
                    ApplicationIdentifier = "Your app ID",
                    ApplicationVersion = "4.0",
                    //your user
                    EditorsFamilyName = "WHU",
                    EditorsGivenName = "WHU",
                    EditorsOrganisationName = "Wuhan University"
                };

                using (var federation = IfcStore.Create(editor, IfcSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
                {
                    federation.AddModelReference(fileName1, "Bob The Builder", "Original Constructor"); //IFC4 文件
                    federation.AddModelReference(fileName2, "Tyna", "Extensions Builder"); //IFC2x3  文件

                    Console.WriteLine("Model is federation: {federation.IsFederation}");
                    Console.WriteLine("Number of overall entities: {federation.FederatedInstances.Count}");
                    Console.WriteLine("Number of walls: {federation.FederatedInstances.CountOf<IIfcWall>()}");
                    foreach (var refModel in federation.ReferencedModels)
                    {
                        Console.WriteLine();
                        Console.WriteLine("    Referenced model: {refModel.Name}");
                        Console.WriteLine("    Referenced model organization: {refModel.OwningOrganisation}");
                        Console.WriteLine("    Number of walls: {refModel.Model.Instances.CountOf<IIfcWall>()}");
                    }
                    //保存IFC文件
                    federation.SaveAs("federation.ifc");
                }
            }
            
        }

        private void TspCreateIFC2_Click(object sender, EventArgs e)
        {
            axFormField frm = new axFormField();
            frm.ShowDialog();
        }

        private void tspCreateSlabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "打开墙线文件";
            dlg.Filter = "(*.shp)|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK && dlg.FileName != String.Empty)
            {
                //带字段CeilingZ和FloorZ，特殊之处在于FloorZ大于CeilingZ
                String wallFileName = dlg.FileName;
                AxIndoorIfcCreatorSlab slab = new AxIndoorIfcCreatorSlab();
                slab.setSlabFile(wallFileName);
                slab.CreateBuilding("slab.ifc");
            }
        }
    }
}
