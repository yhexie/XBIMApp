using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    class AxIndoorIfcCreatorSlab
    {
        List<AxWallLine> m_WallPolylines;
        List<AxDoor> m_DoorPolylines;
        double door_Dist_Wall_Threshold=300;
        string m_wallFileName;
        string m_doorFileName;
        double m_MinX=0;
        double m_MinY=0;
        bool checkDoorCreate = false;
        public void setSlabFile(string wall_file)
        {
            m_wallFileName = wall_file;
        }
        
        //读取墙线文件
        private void ReadSlabPolygonSHP(string FILENAME)
        {
            IntPtr hShp = ShapeLib.SHPOpen(FILENAME, "rb+");
            IntPtr hDbf = ShapeLib.DBFOpen(FILENAME, "r+");
            if (hDbf.Equals(IntPtr.Zero))
            {
                Console.WriteLine("Error:  Unable to create {0}.dbf!", FILENAME);
                return;
            }
            m_WallPolylines = new List<AxWallLine>();
            // get shape info and verify shapes were created correctly
            double[] minB = new double[4];
            double[] maxB = new double[4];
            int nEntities = 0;
            ShapeLib.ShapeType shapeType = 0;
            ShapeLib.SHPGetInfo(hShp, ref nEntities, ref shapeType, minB, maxB);
            int iCeilingZ = ShapeLib.DBFGetFieldIndex(hDbf, "CeilingZ");
            int iFloorZ = ShapeLib.DBFGetFieldIndex(hDbf, "FloorZ");
            m_MinX = minB[0];
            m_MinY = minB[1];
            // test SHPReadObject on the first shape
            for (int i = 0; i < nEntities; i++)
            {
                int iShape = i;
               
                IntPtr pshpObj = ShapeLib.SHPReadObject(hShp, iShape);
                AxPolyline2d plline = new AxPolyline2d();
                ShapeLib.SHPObject shpObj = new ShapeLib.SHPObject();
                Marshal.PtrToStructure(pshpObj, shpObj);

                int parts = shpObj.nParts;
               
                if (parts > 0)
                {
                    int[] partStart = new int[parts];
                    Marshal.Copy(shpObj.paPartStart, partStart, 0, parts);
                   
                    int[] partType = new int[parts];
                    Marshal.Copy(shpObj.paPartType, partType, 0, parts);
                   
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
                        Vector2d pt = new Vector2d(x * 1000, y * 1000);
                        plline.polyline.Add(pt);//
                    }
                    double ceilingZ = 0;
                    double floorZ = 0;
                    if (iCeilingZ == -1 || iFloorZ == -1)
                    {
                        ceilingZ = 2.40;
                        floorZ = 0;
                    }
                    else
                    {
                        ceilingZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iCeilingZ);
                        floorZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iFloorZ);
                    }
                    AxWallLine wall = new AxWallLine();
                    wall.WallId = 1000 + i;
                    wall.m_Polyline = plline;
                    wall.m_MaxZ = ceilingZ * 1000;
                    wall.m_MinZ = floorZ * 1000;
                    wall.m_Thickness = 150;
                    m_WallPolylines.Add(wall);
                }
                ShapeLib.SHPDestroyObject(pshpObj);
            }
            ShapeLib.SHPClose(hShp);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadLine();
        }

       

        public void CreateBuilding(string fileName_)
        {
            ReadSlabPolygonSHP(m_wallFileName);
           
            using (var model = CreateandInitModel("HelloSlab"))
            {
                if (model != null)
                {
                    IfcBuilding building = CreateBuilding(model, "Default Building");
                    List<IfcSlabStandardCase> m_Walls = new List<IfcSlabStandardCase>();
                    if (m_WallPolylines.Count > 0)
                    {
                        for (int i = 0; i < m_WallPolylines.Count; i++)
                        {
                            AxPolyline2d x = m_WallPolylines[i].m_Polyline;
                            double thickness = m_WallPolylines[i].m_Thickness;
                            if (x.polyline.Count > 2)
                            {
                                double baseH = m_WallPolylines[i].m_MinZ ;
                                double heightWall = (m_WallPolylines[i].m_MaxZ - baseH) ;
                                IfcSlabStandardCase wall = CreateSlab(model, x, thickness, baseH, heightWall);
                                m_Walls.Add(wall);
                            }
                          
                        }
                    }
                    using (var txn = model.BeginTransaction("Add Wall"))
                    {
                        if (m_Walls.Count > 0)
                        {
                            for (int i = 0; i < m_Walls.Count; i++)
                            {
                                IfcSlabStandardCase wall = m_Walls[i];
                                building.AddElement(wall);
                            }
                        }
                        txn.Commit();
                    }

                    // if (wall != null)
                    {
                        try
                        {
                            Console.WriteLine("Standard Wall successfully created....");
                            //保存到文件
                            model.SaveAs(fileName_, IfcStorageType.Ifc);
                            Console.WriteLine("HelloWallIfc4.ifc has been successfully written");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to save HelloWall.ifc");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                //else
                //{
                //    Console.WriteLine("Failed to initialise the model");
                //}
            }
            Console.WriteLine("Press any key to exit to view the IFC file....");
            // Console.ReadKey();
        }
        public IfcBuilding CreateBuilding(IfcStore model, string name)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Building"))
            {
                var building = model.Instances.New<IfcBuilding>();
                building.Name = name;

                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                var localPlacement = model.Instances.New<IfcLocalPlacement>();
                building.ObjectPlacement = localPlacement;
                var placement = model.Instances.New<IfcAxis2Placement3D>();
                localPlacement.RelativePlacement = placement;
                placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
                //获取项目
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                project.AddBuilding(building);
                
                txn.Commit();
                return building;
            }
        }



        /// <summary>
        /// 设置模型的基本参数、单元、所有权等
        /// </summary>
        /// <param name="projectName">项目名称</param>
        /// <returns></returns>
        public IfcStore CreateandInitModel(string projectName)
        {
            //首先我们需要为模型创建凭证、这个在之前的文章都有解释
            var credentials = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "xBimTeam",
                ApplicationFullName = "Hello Wall Application",
                ApplicationIdentifier = "HelloWall.exe",
                ApplicationVersion = "1.0",
                EditorsFamilyName = "Team",
                EditorsGivenName = "xBIM",
                EditorsOrganisationName = "xBimTeam"
            };
            //那么先创建 IfcStore,IfcStore 是IFC4 格式存放在内存中而不是数据库
            //如果模型大于50MB的Ifc或者需要强大的事务处理，数据库在性能方面通常会更好

            var model = IfcStore.Create(credentials, IfcSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
            // 启动事务、将所有的模型更改为 ACID
            using (var txn = model.BeginTransaction("Initialise Model"))
            {

                //常见项目信息
                var project = model.Instances.New<IfcProject>();
                //设置单位   这里是英制 
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = projectName;
                var site = model.Instances.New<IfcSite>();
                project.AddSite(site);
                //提交修改
                txn.Commit();
            }
            return model;

        }


        public IfcSlabStandardCase CreateSlab(IfcStore model, AxPolyline2d curve_, double width, double baseHeight, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Curved Wall"))
            {
                var wall = model.Instances.New<IfcSlabStandardCase>();
                wall.Name = "A curved wall";

                IfcPolyline curve = model.Instances.New<IfcPolyline>();
                for (int i = 0; i < curve_.polyline.Count; i++)
                {
                    var pt0 = model.Instances.New<IfcCartesianPoint>();
                    double pt_X = curve_.polyline[i].X;
                    double pt_Y = curve_.polyline[i].Y;
                    pt0.SetXY(pt_X, pt_Y); //在任意位置插入
                    curve.Points.Add(pt0);
                }

                var rectProf = model.Instances.New<IfcArbitraryClosedProfileDef>();
                rectProf.ProfileType = IfcProfileTypeEnum.AREA;
                rectProf.OuterCurve = curve;

                //模型区域实心
                var body = model.Instances.New<IfcExtrudedAreaSolid>();
                body.Depth = height;
                body.SweptArea = rectProf;
                body.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body.ExtrudedDirection.SetXYZ(0, 0, 1);

                //在模型中插入几何参数
                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);
                body.Position = model.Instances.New<IfcAxis2Placement3D>();
                body.Position.Location = origin;

                //创建一个定义形状来保存几何
                var shape = model.Instances.New<IfcShapeRepresentation>();
                var modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape.ContextOfItems = modelContext;
                shape.RepresentationType = "SweptSolid";
                shape.RepresentationIdentifier = "Body";
                shape.Items.Add(body);

                //创建产品定义并将模型几何添加到墙上
                var rep = model.Instances.New<IfcProductDefinitionShape>();
                rep.Representations.Add(shape);
                wall.Representation = rep;

                //把墙放到模型中
                var lp = model.Instances.New<IfcLocalPlacement>();
                var ax3D = model.Instances.New<IfcAxis2Placement3D>();
                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(0, 0, baseHeight);
                ax3D.Location = origin2;
                ax3D.RefDirection = model.Instances.New<IfcDirection>();
                ax3D.RefDirection.SetXYZ(1, 0, 0);
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1);
                lp.RelativePlacement = ax3D;
                wall.ObjectPlacement = lp;

                //Where子句：IfcWallStandard依赖于提供一个IfcMaterialLayerSetUsage
                var ifcMaterialLayerSetUsage = model.Instances.New<IfcMaterialLayerSetUsage>();
                var ifcMaterialLayerSet = model.Instances.New<IfcMaterialLayerSet>();
                var ifcMaterialLayer = model.Instances.New<IfcMaterialLayer>();
                ifcMaterialLayer.LayerThickness = 10;
                ifcMaterialLayerSet.MaterialLayers.Add(ifcMaterialLayer);
                ifcMaterialLayerSetUsage.ForLayerSet = ifcMaterialLayerSet;
                ifcMaterialLayerSetUsage.LayerSetDirection = IfcLayerSetDirectionEnum.AXIS2;
                ifcMaterialLayerSetUsage.DirectionSense = IfcDirectionSenseEnum.NEGATIVE;
                ifcMaterialLayerSetUsage.OffsetFromReferenceLine = 150;

                //添加材料到墙上
                var material = model.Instances.New<IfcMaterial>();
                material.Name = "some material";
                var ifcRelAssociatesMaterial = model.Instances.New<IfcRelAssociatesMaterial>();
                ifcRelAssociatesMaterial.RelatingMaterial = material;
                ifcRelAssociatesMaterial.RelatedObjects.Add(wall);

                ifcRelAssociatesMaterial.RelatingMaterial = ifcMaterialLayerSetUsage;

                //IfcPresentationLayerAssignment对于IfcWall或IfcWallStandardCase中的CAD演示是必需的 
                var ifcPresentationLayerAssignment = model.Instances.New<IfcPresentationLayerAssignment>();
                ifcPresentationLayerAssignment.Name = "some ifcPresentationLayerAssignment";
                ifcPresentationLayerAssignment.AssignedItems.Add(shape);
                // 如果IfcPolyline具有两个点，则对于IfcWall是必需的
                var ifcPolyline = model.Instances.New<IfcPolyline>();
                var startPoint = model.Instances.New<IfcCartesianPoint>();
                startPoint.SetXY(0, 0);
                var endPoint = model.Instances.New<IfcCartesianPoint>();
                endPoint.SetXY(1000, 0);
                ifcPolyline.Points.Add(startPoint);
                ifcPolyline.Points.Add(endPoint);

                var shape2D = model.Instances.New<IfcShapeRepresentation>();
                shape2D.ContextOfItems = modelContext;
                shape2D.RepresentationIdentifier = "Axis";
                shape2D.RepresentationType = "Curve2D";
                shape2D.Items.Add(ifcPolyline);
                rep.Representations.Add(shape2D);
                txn.Commit();
                return wall;
            }

        }

    }
}
