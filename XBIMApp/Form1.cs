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
    public partial class Form1 : Form
    {
       List< AxPolyline> m_Polylines;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //first create and initialise a model called Hello Wall
           
           // LaunchNotepad("HelloWallIfc4.ifc");
        }
        // private static void LaunchNotepad(string fileName)
        //{
        //    Process p;
        //    try
        //    {

        //        p = new Process {StartInfo = {FileName = fileName, CreateNoWindow = false}};
        //        p.Start();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception Occurred :{0},{1}",
        //                  ex.Message, ex.StackTrace);
        //    }
        //}

      
        /// <summary>
        /// 给墙添加属性
        /// </summary>
        /// <param name="model">XbimModel</param>
        /// <param name="wall"></param>
        static private void AddPropertiesToWall(IfcStore model, IfcWallStandardCase wall)
        {
            using (var txn = model.BeginTransaction("Create Wall"))
            {
                CreateElementQuantity(model, wall);
                CreateSimpleProperty(model, wall); 
                txn.Commit(); 
            }
        }

        private static void CreateSimpleProperty(IfcStore model, IfcWallStandardCase wall)
        {
            var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
            {
                psv.Name = "IfcPropertySingleValue:Time";
                psv.Description = "";
                psv.NominalValue = new IfcTimeMeasure(150.0);
                psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.TIMEUNIT;
                    siu.Name = IfcSIUnitName.SECOND;
                });
            });
            var ifcPropertyEnumeratedValue = model.Instances.New<IfcPropertyEnumeratedValue>(pev =>
            {
                pev.Name = "IfcPropertyEnumeratedValue:Music";
                pev.EnumerationReference = model.Instances.New<IfcPropertyEnumeration>(pe =>
                    {
                        pe.Name = "Notes";
                        pe.EnumerationValues.Add(new IfcLabel("Do"));
                        pe.EnumerationValues.Add(new IfcLabel("Re"));
                        pe.EnumerationValues.Add(new IfcLabel("Mi"));
                        pe.EnumerationValues.Add(new IfcLabel("Fa"));
                        pe.EnumerationValues.Add(new IfcLabel("So"));
                        pe.EnumerationValues.Add(new IfcLabel("La"));
                        pe.EnumerationValues.Add(new IfcLabel("Ti"));
                    });
                pev.EnumerationValues.Add(new IfcLabel("Do"));
                pev.EnumerationValues.Add(new IfcLabel("Re"));
                pev.EnumerationValues.Add(new IfcLabel("Mi"));

            });
            var ifcPropertyBoundedValue = model.Instances.New<IfcPropertyBoundedValue>(pbv => 
            {
                pbv.Name = "IfcPropertyBoundedValue:Mass";
                pbv.Description = "";
                pbv.UpperBoundValue = new IfcMassMeasure(5000.0);
                pbv.LowerBoundValue = new IfcMassMeasure(1000.0);
                pbv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.MASSUNIT;
                    siu.Name = IfcSIUnitName.GRAM;
                    siu.Prefix = IfcSIPrefix.KILO;                  
                });
            });

            var definingValues = new List<IfcReal> { new IfcReal(100.0), new IfcReal(200.0), new IfcReal(400.0), new IfcReal(800.0), new IfcReal(1600.0), new IfcReal(3200.0), };
            var definedValues = new List<IfcReal> { new IfcReal(20.0), new IfcReal(42.0), new IfcReal(46.0), new IfcReal(56.0), new IfcReal(60.0), new IfcReal(65.0), };
            var ifcPropertyTableValue = model.Instances.New<IfcPropertyTableValue>(ptv =>
            {
                ptv.Name = "IfcPropertyTableValue:Sound";
                foreach (var item in definingValues)
                {
                    ptv.DefiningValues.Add(item);
                }
                foreach (var item in definedValues)
                {
                    ptv.DefinedValues.Add(item);
                }
                ptv.DefinedUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
                {
                    cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                    {
                        de.LengthExponent = 0;
                        de.MassExponent = 0;
                        de.TimeExponent = 0;
                        de.ElectricCurrentExponent = 0;
                        de.ThermodynamicTemperatureExponent = 0;
                        de.AmountOfSubstanceExponent = 0;
                        de.LuminousIntensityExponent = 0;
                    });
                    cd.UnitType = IfcUnitEnum.FREQUENCYUNIT;
                    cd.Name = "dB";
                });


            });

            var listValues = new List<IfcLabel> { new IfcLabel("Red"), new IfcLabel("Green"), new IfcLabel("Blue"), new IfcLabel("Pink"), new IfcLabel("White"), new IfcLabel("Black"), };
            var ifcPropertyListValue = model.Instances.New<IfcPropertyListValue>(plv =>
            {
                plv.Name = "IfcPropertyListValue:Colours";
                foreach (var item in listValues)
                {
                    plv.ListValues.Add(item);
                }
            });

            var ifcMaterial = model.Instances.New<IfcMaterial>(m =>
            {
                m.Name = "Brick";
            });
            var ifcPrValueMaterial = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:Material";
                prv.PropertyReference = ifcMaterial;
            });


            var ifcMaterialList = model.Instances.New<IfcMaterialList>(ml =>
                {
                    ml.Materials.Add(ifcMaterial);
                    ml.Materials.Add(model.Instances.New<IfcMaterial>(m =>{m.Name = "Cavity";}));
                    ml.Materials.Add(model.Instances.New<IfcMaterial>(m => { m.Name = "Block"; }));
                });


            var ifcMaterialLayer = model.Instances.New<IfcMaterialLayer>(ml =>
            {
                ml.Material = ifcMaterial;
                ml.LayerThickness = 100.0;
            });
            var ifcPrValueMatLayer = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:MaterialLayer";
                prv.PropertyReference = ifcMaterialLayer;
            });

            var ifcDocumentReference = model.Instances.New<IfcDocumentReference>(dr =>
            {
                dr.Name = "Document";
                dr.Location = "c://Documents//TheDoc.Txt";
            });
            var ifcPrValueRef = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:Document";
                prv.PropertyReference = ifcDocumentReference;
            });

            var ifcTimeSeries = model.Instances.New<IfcRegularTimeSeries>(ts =>
            {
                ts.Name = "Regular Time Series";
                ts.Description = "Time series of events";
                ts.StartTime = new IfcDateTime("2015-02-14T12:01:01");
                ts.EndTime = new IfcDateTime("2015-05-15T12:01:01");
                ts.TimeSeriesDataType = IfcTimeSeriesDataTypeEnum.CONTINUOUS;
                ts.DataOrigin = IfcDataOriginEnum.MEASURED;
                ts.TimeStep = 604800; //7 days in secs
            });

            var ifcPrValueTimeSeries = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:TimeSeries";
                prv.PropertyReference = ifcTimeSeries;
            });

            var ifcAddress = model.Instances.New<IfcPostalAddress>(a =>
            {
                a.InternalLocation = "Room 101";
                a.AddressLines.AddRange(new[] { new IfcLabel("12 New road"), new IfcLabel("DoxField" ) });
                a.Town = "Sunderland";
                a.PostalCode = "DL01 6SX";
            });
            var ifcPrValueAddress = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:Address";
                prv.PropertyReference = ifcAddress;
            });
            var ifcTelecomAddress = model.Instances.New<IfcTelecomAddress>(a =>
            {
                a.TelephoneNumbers.Add(new IfcLabel("01325 6589965"));
                a.ElectronicMailAddresses.Add(new IfcLabel("bob@bobsworks.com"));
            });
            var ifcPrValueTelecom = model.Instances.New<IfcPropertyReferenceValue>(prv =>
            {
                prv.Name = "IfcPropertyReferenceValue:Telecom";
                prv.PropertyReference = ifcTelecomAddress;
            });



            //设置模型元素数量
            var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
            {              
                ps.Name = "Test:IfcPropertySet";
                ps.Description = "Property Set";
                ps.HasProperties.Add(ifcPropertySingleValue);
                ps.HasProperties.Add(ifcPropertyEnumeratedValue);
                ps.HasProperties.Add(ifcPropertyBoundedValue);
                ps.HasProperties.Add(ifcPropertyTableValue);
                ps.HasProperties.Add(ifcPropertyListValue);
                ps.HasProperties.Add(ifcPrValueMaterial);
                ps.HasProperties.Add(ifcPrValueMatLayer);
                ps.HasProperties.Add(ifcPrValueRef);
                ps.HasProperties.Add(ifcPrValueTimeSeries);
                ps.HasProperties.Add(ifcPrValueAddress);
                ps.HasProperties.Add(ifcPrValueTelecom);             
            });

            //需建立关系
            model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
            {                
                rdbp.Name = "Property Association";
                rdbp.Description = "IfcPropertySet associated to wall";
                rdbp.RelatedObjects.Add(wall);
                rdbp.RelatingPropertyDefinition = ifcPropertySet;
            });
        }

        private static void CreateElementQuantity(IfcStore model, IfcWallStandardCase wall)
        {
            //创建模型元素数量
            //首先我们需模型简单物理量，首先将使用模型量长度
            var ifcQuantityArea = model.Instances.New<IfcQuantityLength>(qa =>
            {
                qa.Name = "IfcQuantityArea:Area";
                qa.Description = "";
                qa.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                    siu.Prefix = IfcSIPrefix.MILLI;
                    siu.Name = IfcSIUnitName.METRE;
                });
                qa.LengthValue = 100.0;

            });
            //上下文相关单元的数量计数
            var ifcContextDependentUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
                {
                    cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                        {
                            de.LengthExponent = 1;
                            de.MassExponent = 0;
                            de.TimeExponent = 0;
                            de.ElectricCurrentExponent = 0;
                            de.ThermodynamicTemperatureExponent = 0;
                            de.AmountOfSubstanceExponent = 0;
                            de.LuminousIntensityExponent = 0;
                        });
                    cd.UnitType = IfcUnitEnum.LENGTHUNIT;
                    cd.Name = "Elephants";
                });
                var ifcQuantityCount = model.Instances.New<IfcQuantityCount>(qc =>
                {
                    qc.Name = "IfcQuantityCount:Elephant";
                    qc.CountValue = 12;
                    qc.Unit = ifcContextDependentUnit;
                });


             //使用转换单位
            var ifcConversionBasedUnit = model.Instances.New<IfcConversionBasedUnit>(cbu =>
            {
                cbu.ConversionFactor = model.Instances.New<IfcMeasureWithUnit>(mu =>
                {
                    mu.ValueComponent = new IfcRatioMeasure(25.4);
                    mu.UnitComponent = model.Instances.New<IfcSIUnit>(siu =>
                    {
                        siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                        siu.Prefix = IfcSIPrefix.MILLI;
                        siu.Name = IfcSIUnitName.METRE;
                    });

                });
                cbu.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                {
                    de.LengthExponent = 1;
                    de.MassExponent = 0;
                    de.TimeExponent = 0;
                    de.ElectricCurrentExponent = 0;
                    de.ThermodynamicTemperatureExponent = 0;
                    de.AmountOfSubstanceExponent = 0;
                    de.LuminousIntensityExponent = 0;
                });
                cbu.UnitType = IfcUnitEnum.LENGTHUNIT;
                cbu.Name = "Inch";
            });
            var ifcQuantityLength = model.Instances.New<IfcQuantityLength>(qa =>
            {
                qa.Name = "IfcQuantityLength:Length";
                qa.Description = "";
                qa.Unit = ifcConversionBasedUnit;
                qa.LengthValue = 24.0;
            });

            //lets create the IfcElementQuantity
            var ifcElementQuantity = model.Instances.New<IfcElementQuantity>(eq =>
            {               
                eq.Name = "Test:IfcElementQuantity";
                eq.Description = "Measurement quantity";
                eq.Quantities.Add(ifcQuantityArea);
                eq.Quantities.Add(ifcQuantityCount);
                eq.Quantities.Add(ifcQuantityLength);
            });

            //下步 建议关系
            model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
            {              
                rdbp.Name = "Area Association";
                rdbp.Description = "IfcElementQuantity associated to wall";
                rdbp.RelatedObjects.Add(wall);
                rdbp.RelatingPropertyDefinition = ifcElementQuantity;
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "打开线文件";
            dlg.Filter = "(*.shp)|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK && dlg.FileName != String.Empty)
            {
                textBox1.Text = dlg.FileName;
                string fileName = dlg.FileName;
                ReadSHP(fileName);
            }
        }
        private void ReadSHP(string FILENAME)
        {
            IntPtr hShp;
            hShp = ShapeLib.SHPOpen(FILENAME, "rb+");
            m_Polylines = new List<AxPolyline>();
            // get shape info and verify shapes were created correctly
            double[] minB = new double[4];
            double[] maxB = new double[4];
            int nEntities = 0;
            ShapeLib.ShapeType shapeType = 0;
            ShapeLib.SHPGetInfo(hShp, ref nEntities, ref shapeType, minB, maxB);
            listBox1.Items.Add(string.Format("Number Entries: {0}", nEntities));
            listBox1.Items.Add(string.Format("ShapeType: {0}", shapeType));
            listBox1.Items.Add(string.Format("Min XY: {0}, {1}", minB[0], minB[1]));
            listBox1.Items.Add(string.Format("Max XY: {0}, {1}", maxB[0], maxB[1]));
            
            // test SHPReadObject on the first shape
            for (int i = 0; i < nEntities; i++)
            {
                int iShape = i;
                listBox1.Items.Add(string.Format("Shape({0}): ", iShape));
                IntPtr pshpObj = ShapeLib.SHPReadObject(hShp, iShape);
                AxPolyline plline = new AxPolyline();
                // Get the SHPObject associated with our IntPtr pshpObj
                // We create a new SHPObject in managed code, then use Marshal.PtrToStructure
                // to copy the unmanaged memory pointed to by pshpObj into our managed copy.
                ShapeLib.SHPObject shpObj = new ShapeLib.SHPObject();
                Marshal.PtrToStructure(pshpObj, shpObj);

                listBox1.Items.Add(string.Format("Min XY of shape({0}): ({1}, {2})", iShape, shpObj.dfXMin, shpObj.dfYMin));
                listBox1.Items.Add(string.Format("Max XY of shape({0}): ({1}, {2})", iShape, shpObj.dfXMax, shpObj.dfYMax));
                listBox1.Items.Add(string.Format("Points of shape({0}): ({1})", iShape, shpObj.nVertices));
                int parts = shpObj.nParts;
                listBox1.Items.Add(string.Format("Parts of shape({0}): ({1})", iShape, parts));
                if (parts > 0)
                {
                    int[] partStart = new int[parts];
                    Marshal.Copy(shpObj.paPartStart, partStart, 0, parts);
                    for (int j = 0; j < partStart.Length; j++)
                    {
                        listBox1.Items.Add(string.Format("FirstPart of shape({0}): ({1})", iShape, partStart[j]));
                    }
                    int[] partType = new int[parts];
                    Marshal.Copy(shpObj.paPartType, partType, 0, parts);
                    for (int j = 0; j < partType.Length; j++)
                    {
                        listBox1.Items.Add(string.Format("FirstPartType of shape({0}): ({1})", iShape, (MapTools.ShapeLib.PartType)partType[j]));
                    }
                    int number = shpObj.nVertices;
                    double[] m_padfX = new double[number];
                    Marshal.Copy(shpObj.padfX, m_padfX, 0, number);
                    double[] m_padfY = new double[number];
                    Marshal.Copy(shpObj.padfY, m_padfY, 0, number);
                    for (int iv = 0; iv < number; iv++)
                    {
                        double x = m_padfX[iv];
                        double y = m_padfY[iv];
                        x = x - minB[0];
                        y = y - minB[1];
                        plline.polylineX.Add(x*1000);//
                        plline.polylineY.Add(y*1000);//
                    }
                    m_Polylines.Add(plline);
                }         
                ShapeLib.SHPDestroyObject(pshpObj);
            }
            ShapeLib.SHPClose(hShp);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadLine();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Reading the shp file....");
            if (m_Polylines==null)
            {
                return;
            }
            Console.WriteLine("Initialising the IFC Project....");
            string filename = "IfcWalls_XXX.ifc";
            AxIndoorIFCCreator creator = new AxIndoorIFCCreator();
            creator.setPolylines(m_Polylines);
            creator.CreateBuilding(filename);
        }
    
    }
}
