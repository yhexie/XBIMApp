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
    class AxIndoorIfcCreatorField
    {
        List<AxWallLine> m_WallPolylines;
        List<AxDoor> m_DoorPolylines;
        double door_Dist_Wall_Threshold=300;
        string m_wallFileName;
        string m_doorFileName;
        double m_MinX=0;
        double m_MinY=0;
        bool checkDoorCreate = false;
        public void setWallFile(string wall_file)
        {
            m_wallFileName = wall_file;
        }
        public void setDoorFile(string door_file)
        {
            m_doorFileName = door_file;
        }
        public void setcheckDoorCreate(bool ischk)
        {
            checkDoorCreate = ischk;
        }
        public void setDist_Wall_Threshold(double threshold_)
        {
            door_Dist_Wall_Threshold = threshold_;
        }
        //读取墙线文件
        private void ReadWallLinesSHP(string FILENAME)
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
                    double ceilingZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iCeilingZ);
                    double floorZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iFloorZ);
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

        //读取门窗文件
        private void ReadDoorLinesSHP(string FILENAME)
        {
            IntPtr hShp = ShapeLib.SHPOpen(FILENAME, "rb+");
            IntPtr hDbf = ShapeLib.DBFOpen(FILENAME,"r+");
            if (hDbf.Equals(IntPtr.Zero))
            {
                Console.WriteLine("Error:  Unable to create {0}.dbf!", FILENAME);
                return;
            }
            int iIsDoor = ShapeLib.DBFGetFieldIndex(hDbf, "IsDoor");
            int iCeilingZ = ShapeLib.DBFGetFieldIndex(hDbf, "MaxZ");
            int iFloorZ = ShapeLib.DBFGetFieldIndex(hDbf, "MinZ");
            m_DoorPolylines = new List<AxDoor>();
            // get shape info and verify shapes were created correctly
            double[] minB = new double[4];
            double[] maxB = new double[4];
            int nEntities = 0;
            ShapeLib.ShapeType shapeType = 0;
            ShapeLib.SHPGetInfo(hShp, ref nEntities, ref shapeType, minB, maxB);
           
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
                        x = x - m_MinX;
                        y = y - m_MinY;
                        Vector2d pt = new Vector2d(x * 1000, y * 1000);
                        plline.polyline.Add(pt);//
                    }
                    int isDoor = 1;
                    isDoor = ShapeLib.DBFReadIntegerAttribute(hDbf, iShape, iIsDoor);
                    double  ceilingZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iCeilingZ);
                    double floorZ = ShapeLib.DBFReadDoubleAttribute(hDbf, iShape, iFloorZ);
                    AxDoor door = new AxDoor();
                    door.Id = 2000 + i;
                    door.m_Polyline = plline;
                    door.m_MaxZ = ceilingZ * 1000;
                    door.m_MinZ = floorZ * 1000;
                    door.m_Thickness = 100;
                    door.IsDoor = isDoor;
                    m_DoorPolylines.Add(door);
                }
                ShapeLib.SHPDestroyObject(pshpObj);
            }
            ShapeLib.SHPClose(hShp);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadLine();
        }

        public void CreateBuilding(string fileName_)
        {
            ReadWallLinesSHP(m_wallFileName);
            ReadDoorLinesSHP(m_doorFileName);
            if (checkDoorCreate== false)
            {
                m_DoorPolylines.Clear();
            }
            //判断门在墙上
            List<AxDoor> m_Unique_Doors = new List<AxDoor>();
            if (m_DoorPolylines.Count > 0)
            {
                m_Unique_Doors.Add(m_DoorPolylines[0]);
                for (int i = 1; i < m_DoorPolylines.Count; i++)
                {
                    int k = 0;
                    AxDoor door_i = m_DoorPolylines[i];
                    for (int j = 0; j < m_Unique_Doors.Count; j++)
                    {
                        AxDoor door_j = m_Unique_Doors[j];
                        //如果两个门平行且距离<0.3m
                        Vector2d source_ = door_i.m_Polyline.polyline[0];
                        Vector2d target_ = door_i.m_Polyline.polyline[door_i.m_Polyline.polyline.Count - 1];
                        Vector2d dir_i = source_ - target_;
                        Vector2d mid_i = (door_i.m_Polyline.polyline[0] + door_i.m_Polyline.polyline[door_i.m_Polyline.polyline.Count - 1]) / 2;

                        Vector2d source_j = door_j.m_Polyline.polyline[0];
                        Vector2d target_j = door_j.m_Polyline.polyline[door_i.m_Polyline.polyline.Count - 1];
                        Vector2d dir_j = target_j - source_j;

                        double ang1 = AxMath.GetQuadrantAngle(dir_i.X, dir_i.Y);
                        double ang2 = AxMath.GetQuadrantAngle(dir_j.X, dir_j.Y);
                        bool isParallel = AxMath.parallel_seg_seg(door_i.getLineSegment(), door_j.getLineSegment());
                        double dist = AxMath.dist_Point_to_Segment(mid_i.ToVector3d(), source_j.ToVector3d(), target_j.ToVector3d());
                        if (isParallel && dist < door_Dist_Wall_Threshold)
                        {

                        }
                        else
                        {
                            k++;
                        }
                    }
                    if (k == m_Unique_Doors.Count)
                    {
                        m_Unique_Doors.Add(m_DoorPolylines[i]);
                    }
                }
            }
            /*---------------------------------------------
            *遍历所有唯一的门,判断每个门所在的墙
            *-----------------------------------------------*/
            for (int i = 0; i < m_Unique_Doors.Count; i++)
            {
                //判断每个门所在的墙
                AxDoor door = m_Unique_Doors[i];
                AxPolyline2d doorTmep = door.m_Polyline;
                AxSegment2 door_seg = new AxSegment2(doorTmep.polyline[0], doorTmep.polyline[doorTmep.polyline.Count - 1]);
                Vector2d mid = (doorTmep.polyline[0] + doorTmep.polyline[doorTmep.polyline.Count - 1]) / 2;
                for (int k = 0; k < m_WallPolylines.Count; k++)
                {
                    AxWallLine wallTmp = m_WallPolylines[k];
                    List<Vector2d> wallPts=wallTmp.m_Polyline.polyline;
                    //门和墙平行，门投影在墙上，门的端点和中点投影落在墙上
                    AxSegment2 wallSeg = new AxSegment2(wallPts[0], wallPts[wallPts.Count-1]);
                    double min_wall_x = Math.Min(wallSeg.source.X, wallSeg.target.X);
                    double max_wall_x = Math.Max(wallSeg.source.X, wallSeg.target.X);
                    double min_wall_y = Math.Min(wallSeg.source.Y, wallSeg.target.Y);
                    double max_wall_y = Math.Max(wallSeg.source.Y, wallSeg.target.Y);

                    Vector2d dir_i = door_seg.ToVector();
                    Vector2d dir_j = wallSeg.ToVector();
                    double ang1 = AxMath.GetQuadrantAngle(dir_i.X, dir_i.Y);
                    double ang2 = AxMath.GetQuadrantAngle(dir_j.X, dir_j.Y);

                    double dist = AxMath.dist_Point_to_Segment(mid.ToVector3d(), wallSeg.source.ToVector3d(), wallSeg.target.ToVector3d());
                    bool isParallel = AxMath.parallel_seg_seg(door_seg, wallSeg);
                    if (isParallel && dist < door_Dist_Wall_Threshold)
                    {
                        if (door.WallId_0 == -1)//如果第一个没有赋值，赋第一个
                        {
                            door.WallId_0 = wallTmp.WallId;
                            m_WallPolylines[k].AddDoor(door.Id);
                        }
                        else
                        {

                        }
                    }
                }
            }

            using (var model = CreateandInitModel("HelloWall"))
            {
                if (model != null)
                {
                    IfcBuilding building = CreateBuilding(model, "Default Building");
                    List<IfcWallStandardCase> m_Walls = new List<IfcWallStandardCase>();
                    List<IfcDoor> m_IfcDoors = new List<IfcDoor>();
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
                                IfcWallStandardCase wall = CreateCurvedWall(model, x, thickness, baseH, heightWall);
                                if (m_WallPolylines[i].m_Doors != null)
                                {
                                    List<int> doors = m_WallPolylines[i].m_Doors;
                                    for (int i0 = 0; i0 < doors.Count; i0++)
                                    {
                                        int doorId=doors[i0];
                                        for (int i1 = 0; i1 < m_Unique_Doors.Count; i1++)
                                        {
                                            AxDoor door=m_Unique_Doors[i1];
                                            if (door.Id == doorId)
                                            {
                                                AxSegment2 seg=door.getLineSegment();
                                                if (door.IsDoor == 1)
                                                {
                                                    double height = door.m_MaxZ - door.m_MinZ;
                                                    IfcDoor ifc_door = CreateDoor(model, wall, seg.source.X, seg.source.Y, seg.target.X, seg.target.Y, 300, door.m_MinZ, height);
                                                    m_IfcDoors.Add(ifc_door);
                                                }
                                                else
                                                {
                                                    double height = door.m_MaxZ - door.m_MinZ;
                                                    IfcWindow ifc_door = CreateWindow(model, wall, seg.source.X, seg.source.Y, seg.target.X, seg.target.Y, 300, door.m_MinZ , height);
                                                }
                                                break;
                                            }
                                        }
                                    }

                                }
                                m_Walls.Add(wall);
                            }
                            else if (x.polyline.Count == 2)
                            {
                                double fromX = x.polyline[0].X;
                                double fromY = x.polyline[0].Y;
                                double toX = x.polyline[1].X;
                                double toY = x.polyline[1].Y;
                                double baseH = m_WallPolylines[i].m_MinZ ;
                                double heightWall = (m_WallPolylines[i].m_MaxZ - baseH);
                                IfcWallStandardCase wall = CreateWall(model, fromX, fromY, toX, toY, thickness,baseH, heightWall );
                                if (m_WallPolylines[i].m_Doors != null)
                                {
                                    List<int> doors = m_WallPolylines[i].m_Doors;
                                    for (int i0 = 0; i0 < doors.Count; i0++)
                                    {
                                        int doorId = doors[i0];
                                        for (int i1 = 0; i1 < m_Unique_Doors.Count; i1++)
                                        {
                                            AxDoor door = m_Unique_Doors[i1];
                                            if (door.Id == doorId)
                                            {
                                                AxSegment2 seg = door.getLineSegment();
                                                if (door.IsDoor==1)
                                                {
                                                    double height = door.m_MaxZ - door.m_MinZ;
                                                    IfcDoor ifc_door = CreateDoor(model, wall, seg.source.X, seg.source.Y, seg.target.X, seg.target.Y, 300, door.m_MinZ , height);
                                                    m_IfcDoors.Add(ifc_door);
                                                }
                                                else
                                                {
                                                    double height = door.m_MaxZ - door.m_MinZ;
                                                    IfcWindow ifc_door = CreateWindow(model, wall, seg.source.X, seg.source.Y, seg.target.X, seg.target.Y, 300, door.m_MinZ, height);
                                                }
                                               
                                                break;
                                            }
                                        }
                                    }
                                }
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
                                IfcWallStandardCase wall = m_Walls[i];
                                building.AddElement(wall);
                            }
                        }
                        if (m_IfcDoors.Count > 0)
                        {
                            for (int i = 0; i < m_IfcDoors.Count; i++)
                            {
                                IfcDoor door = m_IfcDoors[i];
                                building.AddElement(door);
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

        /// <summary>
        /// 创建墙
        /// </summary>
        /// <param name="model"></param>
        /// <param name="length">矩形的长度 </param>
        /// <param name="width">矩形占地面积的宽度（墙的宽度）</param>
        /// <param name="height">墙高度</param>
        /// <returns></returns>
        public IfcWallStandardCase CreateWall(IfcStore model, double fromX, double fromY, double toX, double toY, double width,double baseHeight, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Wall"))
            {
                var wall = model.Instances.New<IfcWallStandardCase>();
                wall.Name = "A Standard rectangular wall";

                double dx = toX - fromX;
                double dy = toY - fromY;
                double mid_x = (fromX + toX) / 2;
                double mid_y = (fromY + toY) / 2;
                double length = Math.Sqrt((toX - fromX) * (toX - fromX) + (toY - fromY) * (toY - fromY));

                // 墙的矩形剖面
                var rectProf = model.Instances.New<IfcRectangleProfileDef>();
                rectProf.ProfileType = IfcProfileTypeEnum.AREA;
                rectProf.XDim = width;
                rectProf.YDim = length;

                var position = model.Instances.New<IfcCartesianPoint>();
                position.SetXY(0, 0); //在任意位置插入
                rectProf.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectProf.Position.Location = position;


                //模型区域实心,拉伸
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
                body.Position.RefDirection = model.Instances.New<IfcDirection>();
                body.Position.RefDirection.SetXYZ(-dy, dx, 0);
                body.Position.Axis = model.Instances.New<IfcDirection>();
                body.Position.Axis.SetXYZ(0, 0, 1);

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

                ax3D.RefDirection = model.Instances.New<IfcDirection>();
                ax3D.RefDirection.SetXYZ(1, 0, 0);
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1);
                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(mid_x, mid_y, baseHeight);//墙插入世界中
                ax3D.Location = origin2;
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

        public IfcWallStandardCase CreateCurvedWall(IfcStore model, AxPolyline2d curve_,  double width,double baseHeight, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Curved Wall"))
            {
                var wall = model.Instances.New<IfcWallStandardCase>();
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

                var rectProf = model.Instances.New<IfcCenterLineProfileDef>();
                rectProf.ProfileType = IfcProfileTypeEnum.AREA;
                rectProf.Thickness = width;
                rectProf.Curve = curve;

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

        public IfcDoor CreateDoor(IfcStore model, IfcWallStandardCase wall, double fromX, double fromY, double toX, double toY, double width,double baseHeight,double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Door"))
            {
                double length = Math.Sqrt((toX - fromX) * (toX - fromX) + (toY - fromY) * (toY - fromY));
                double dx = toX - fromX;
                double dy = toY - fromY;
                double mid_x = (fromX + toX) / 2;
                double mid_y = (fromY + toY) / 2;
                var rectDoor = model.Instances.New<IfcRectangleProfileDef>();
                rectDoor.ProfileType = IfcProfileTypeEnum.AREA;
                rectDoor.XDim = width - 100;
                rectDoor.YDim = length;

                var insertPoint = model.Instances.New<IfcCartesianPoint>();
                insertPoint.SetXY(0, 0); //在任意位置插入
                rectDoor.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectDoor.Position.Location = insertPoint;

                //门模型区域实心
                var body_door = model.Instances.New<IfcExtrudedAreaSolid>();
                body_door.Depth = height;
                body_door.SweptArea = rectDoor;
                body_door.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_door.ExtrudedDirection.SetXYZ(0, 0, 1);

                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);
                //拉伸对象，同时旋转
                body_door.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_door.Position.Location = origin;
                body_door.Position.RefDirection = model.Instances.New<IfcDirection>();
                body_door.Position.RefDirection.SetXYZ(-dy, dx, 0);
                body_door.Position.Axis = model.Instances.New<IfcDirection>();
                body_door.Position.Axis.SetXYZ(0, 0, 1);

                //创建一个定义形状来保存几何
                var shape_door = model.Instances.New<IfcShapeRepresentation>();
                var modelContext_door = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape_door.ContextOfItems = modelContext_door;
                shape_door.RepresentationType = "SweptSolid";
                shape_door.RepresentationIdentifier = "Body";
                shape_door.Items.Add(body_door);

                //创建产品定义并将模型几何添加到墙上
                var rep_door = model.Instances.New<IfcProductDefinitionShape>();
                rep_door.Representations.Add(shape_door);

                var door = model.Instances.New<IfcDoor>();
                door.Name = "A Door";
                door.PredefinedType = IfcDoorTypeEnum.GATE;
                door.OperationType = IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT;
                door.OverallHeight = 400;
                door.OverallWidth = 400;

                var lp_door = model.Instances.New<IfcLocalPlacement>();
                var wallplace = wall.ObjectPlacement;
                

                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(mid_x, mid_y, baseHeight);//插入世界的位置
                var ax3D_door = model.Instances.New<IfcAxis2Placement3D>();
                ax3D_door.RefDirection = model.Instances.New<IfcDirection>();
                ax3D_door.RefDirection.SetXYZ(1, 0, 0);//x轴
                ax3D_door.Axis = model.Instances.New<IfcDirection>();
                ax3D_door.Axis.SetXYZ(0, 0, 1);//Z轴
                ax3D_door.Location = origin2;
                lp_door.RelativePlacement = ax3D_door;
                //lp_door.PlacementRelTo = wallplace;//此处删除了
                door.ObjectPlacement = lp_door;
                door.Representation = rep_door;

                ////////////////////////////////////////////////////////////////////
                var m_OpeningEle = model.Instances.New<IfcOpeningElement>();
                m_OpeningEle.Name = "My Openings";
                m_OpeningEle.PredefinedType = IfcOpeningElementTypeEnum.OPENING;

                var rectOpening = model.Instances.New<IfcRectangleProfileDef>();
                rectOpening.ProfileType = IfcProfileTypeEnum.AREA;
                rectOpening.XDim = width;
                rectOpening.YDim = length;
                rectOpening.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectOpening.Position.Location = insertPoint;

                //Openging模型拉伸
                var body_Opeinging = model.Instances.New<IfcExtrudedAreaSolid>();
                body_Opeinging.Depth = height;
                body_Opeinging.SweptArea = rectOpening;
                body_Opeinging.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_Opeinging.ExtrudedDirection.SetXYZ(0, 0, 1);
                //Openging旋转
                body_Opeinging.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_Opeinging.Position.Location = origin;
                body_Opeinging.Position.RefDirection = model.Instances.New<IfcDirection>();
                body_Opeinging.Position.RefDirection.SetXYZ(-dy, dx, 0);
                body_Opeinging.Position.Axis = model.Instances.New<IfcDirection>();
                body_Opeinging.Position.Axis.SetXYZ(0, 0, 1);

                var shape__Opeinging = model.Instances.New<IfcShapeRepresentation>();
                var modelContext__Opeinging = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape__Opeinging.ContextOfItems = modelContext__Opeinging;
                shape__Opeinging.RepresentationType = "SweptSolid";
                shape__Opeinging.RepresentationIdentifier = "Body";
                shape__Opeinging.Items.Add(body_Opeinging);

                //创建产品定义并将模型几何添加到墙上
                var rep_Opening = model.Instances.New<IfcProductDefinitionShape>();
                rep_Opening.Representations.Add(shape__Opeinging);
                m_OpeningEle.ObjectPlacement = lp_door;//位置相同
                m_OpeningEle.Representation = rep_Opening;

                var m_RelFills = model.Instances.New<IfcRelFillsElement>();
                m_RelFills.RelatingOpeningElement = m_OpeningEle;
                m_RelFills.RelatedBuildingElement = door;
                var voidRel = model.Instances.New<IfcRelVoidsElement>();
                voidRel.RelatedOpeningElement = m_OpeningEle;
                voidRel.RelatingBuildingElement = wall;

                var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "Reference";
                    psv.Description = "Reference";
                    psv.NominalValue = new IfcTimeMeasure(150.0);
                    psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                    {
                        siu.UnitType = IfcUnitEnum.TIMEUNIT;
                        siu.Name = IfcSIUnitName.SECOND;
                    });
                });
                //设置模型元素数量
                var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
                {
                    ps.Name = "Pset_DoorCommon";
                    ps.Description = "Property Set";
                    ps.HasProperties.Add(ifcPropertySingleValue);

                });
                //需建立关系
                model.Instances.New<IfcRelDefinesByProperties>(
                    rdbp =>
                    {
                        rdbp.Name = "Property Association";
                        rdbp.Description = "IfcPropertySet associated to wall";
                        rdbp.RelatedObjects.Add(door);
                        rdbp.RelatingPropertyDefinition = ifcPropertySet;
                    });

                txn.Commit();
                return door;
            }

        }

        public IfcWindow CreateWindow(IfcStore model, IfcWallStandardCase wall, double fromX, double fromY, double toX, double toY, double width, double baseHeight, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Window"))
            {
                double length = Math.Sqrt((toX - fromX) * (toX - fromX) + (toY - fromY) * (toY - fromY));
                double dx = toX - fromX;
                double dy = toY - fromY;
                double mid_x = (fromX + toX) / 2;
                double mid_y = (fromY + toY) / 2;
                var rectDoor = model.Instances.New<IfcRectangleProfileDef>();
                rectDoor.ProfileType = IfcProfileTypeEnum.AREA;
                rectDoor.XDim = width - 100;
                rectDoor.YDim = length;

                var insertPoint = model.Instances.New<IfcCartesianPoint>();
                insertPoint.SetXY(0, 0); //在任意位置插入
                rectDoor.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectDoor.Position.Location = insertPoint;

                //门模型区域实心
                var body_door = model.Instances.New<IfcExtrudedAreaSolid>();
                body_door.Depth = height;
                body_door.SweptArea = rectDoor;
                body_door.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_door.ExtrudedDirection.SetXYZ(0, 0, 1);

                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);
                //拉伸对象，同时旋转
                body_door.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_door.Position.Location = origin;
                body_door.Position.RefDirection = model.Instances.New<IfcDirection>();
                body_door.Position.RefDirection.SetXYZ(-dy, dx, 0);
                body_door.Position.Axis = model.Instances.New<IfcDirection>();
                body_door.Position.Axis.SetXYZ(0, 0, 1);

                //创建一个定义形状来保存几何
                var shape_door = model.Instances.New<IfcShapeRepresentation>();
                var modelContext_door = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape_door.ContextOfItems = modelContext_door;
                shape_door.RepresentationType = "SweptSolid";
                shape_door.RepresentationIdentifier = "Body";
                shape_door.Items.Add(body_door);

                //创建产品定义并将模型几何添加到墙上
                var rep_door = model.Instances.New<IfcProductDefinitionShape>();
                rep_door.Representations.Add(shape_door);

                var door = model.Instances.New<IfcWindow>();
                door.Name = "A Door";
                door.PredefinedType = IfcWindowTypeEnum.WINDOW;
               
                door.OverallHeight = 400;
                door.OverallWidth = 400;

                var lp_door = model.Instances.New<IfcLocalPlacement>();
                var wallplace = wall.ObjectPlacement;


                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(mid_x, mid_y,  baseHeight);
                var ax3D_door = model.Instances.New<IfcAxis2Placement3D>();
                ax3D_door.RefDirection = model.Instances.New<IfcDirection>();
                ax3D_door.RefDirection.SetXYZ(1, 0, 0);//x轴
                ax3D_door.Axis = model.Instances.New<IfcDirection>();
                ax3D_door.Axis.SetXYZ(0, 0, 1);//Z轴
                ax3D_door.Location = origin2;
                lp_door.RelativePlacement = ax3D_door;
                //lp_door.PlacementRelTo = wallplace;//此处删除了
                door.ObjectPlacement = lp_door;
                door.Representation = rep_door;

                ////////////////////////////////////////////////////////////////////
                var m_OpeningEle = model.Instances.New<IfcOpeningElement>();
                m_OpeningEle.Name = "My Openings";
                m_OpeningEle.PredefinedType = IfcOpeningElementTypeEnum.OPENING;

                var rectOpening = model.Instances.New<IfcRectangleProfileDef>();
                rectOpening.ProfileType = IfcProfileTypeEnum.AREA;
                rectOpening.XDim = width;
                rectOpening.YDim = length;
                rectOpening.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectOpening.Position.Location = insertPoint;

                //Openging模型拉伸
                var body_Opeinging = model.Instances.New<IfcExtrudedAreaSolid>();
                body_Opeinging.Depth = height;
                body_Opeinging.SweptArea = rectOpening;
                body_Opeinging.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_Opeinging.ExtrudedDirection.SetXYZ(0, 0, 1);
                //Openging旋转
                body_Opeinging.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_Opeinging.Position.Location = origin;
                body_Opeinging.Position.RefDirection = model.Instances.New<IfcDirection>();
                body_Opeinging.Position.RefDirection.SetXYZ(-dy, dx, 0);
                body_Opeinging.Position.Axis = model.Instances.New<IfcDirection>();
                body_Opeinging.Position.Axis.SetXYZ(0, 0, 1);

                var shape__Opeinging = model.Instances.New<IfcShapeRepresentation>();
                var modelContext__Opeinging = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape__Opeinging.ContextOfItems = modelContext__Opeinging;
                shape__Opeinging.RepresentationType = "SweptSolid";
                shape__Opeinging.RepresentationIdentifier = "Body";
                shape__Opeinging.Items.Add(body_Opeinging);

                //创建产品定义并将模型几何添加到墙上
                var rep_Opening = model.Instances.New<IfcProductDefinitionShape>();
                rep_Opening.Representations.Add(shape__Opeinging);
                m_OpeningEle.ObjectPlacement = lp_door;//位置相同
                m_OpeningEle.Representation = rep_Opening;

                var m_RelFills = model.Instances.New<IfcRelFillsElement>();
                m_RelFills.RelatingOpeningElement = m_OpeningEle;
                m_RelFills.RelatedBuildingElement = door;
                var voidRel = model.Instances.New<IfcRelVoidsElement>();
                voidRel.RelatedOpeningElement = m_OpeningEle;
                voidRel.RelatingBuildingElement = wall;

                var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "Reference";
                    psv.Description = "Reference";
                    psv.NominalValue = new IfcTimeMeasure(150.0);
                    psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                    {
                        siu.UnitType = IfcUnitEnum.TIMEUNIT;
                        siu.Name = IfcSIUnitName.SECOND;
                    });
                });
                //设置模型元素数量
                var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
                {
                    ps.Name = "Pset_DoorCommon";
                    ps.Description = "Property Set";
                    ps.HasProperties.Add(ifcPropertySingleValue);

                });
                //需建立关系
                model.Instances.New<IfcRelDefinesByProperties>(
                    rdbp =>
                    {
                        rdbp.Name = "Property Association";
                        rdbp.Description = "IfcPropertySet associated to wall";
                        rdbp.RelatedObjects.Add(door);
                        rdbp.RelatingPropertyDefinition = ifcPropertySet;
                    });

                txn.Commit();
                return door;
            }

        }
        public IfcWindow CreateWindow(IfcStore model, IfcWallStandardCase wall, double length, double width, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Window"))
            {
                var rectDoor = model.Instances.New<IfcRectangleProfileDef>();
                rectDoor.ProfileType = IfcProfileTypeEnum.CURVE;
                rectDoor.XDim = width - 100;
                rectDoor.YDim = length;

                var insertPoint = model.Instances.New<IfcCartesianPoint>();
                insertPoint.SetXY(0, 0); //在任意位置插入
                rectDoor.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectDoor.Position.Location = insertPoint;

                //模型区域实心
                var body_door = model.Instances.New<IfcExtrudedAreaSolid>();
                body_door.Depth = height;
                body_door.SweptArea = rectDoor;
                body_door.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_door.ExtrudedDirection.SetXYZ(0, 0, 1);

                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);
                //在模型中插入几何参数
                body_door.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_door.Position.Location = origin;

                //创建一个定义形状来保存几何
                var shape_door = model.Instances.New<IfcShapeRepresentation>();
                var modelContext_door = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape_door.ContextOfItems = modelContext_door;
                shape_door.RepresentationType = "SweptSolid";
                shape_door.RepresentationIdentifier = "Body";
                shape_door.Items.Add(body_door);

                //创建产品定义并将模型几何添加到墙上
                var rep_door = model.Instances.New<IfcProductDefinitionShape>();
                rep_door.Representations.Add(shape_door);

                // var door = model.Instances.New<IfcDoorStandardCase>();
                //door.Name = "A Door";
                //door.PredefinedType = IfcDoorTypeEnum.GATE;
                //door.OperationType = IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT;
                //door.OverallHeight = 400;
                //door.OverallWidth = 400;
                var door = model.Instances.New<IfcWindow>();
                door.Name = "A Door";
                door.PredefinedType = IfcWindowTypeEnum.WINDOW;
                // door.OperationType = IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT;
                door.OverallHeight = 400;
                door.OverallWidth = 400;
                door.PartitioningType = IfcWindowTypePartitioningEnum.SINGLE_PANEL;

                var windowType = model.Instances.New<IfcWindowType>();
                windowType.Name = "Window";
                windowType.Description = "ddddd";
                windowType.PartitioningType = IfcWindowTypePartitioningEnum.SINGLE_PANEL;
                var windowType_Rel = model.Instances.New<IfcRelDefinesByType>();
                windowType_Rel.RelatedObjects.Add(door);
                windowType_Rel.RelatingType = windowType;
                var lp_door = model.Instances.New<IfcLocalPlacement>();
                var wallplace = wall.ObjectPlacement;
                var ax3D_door = model.Instances.New<IfcAxis2Placement3D>();
                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(0, 3000, 1000);

                ax3D_door.RefDirection = model.Instances.New<IfcDirection>();
                ax3D_door.RefDirection.SetXYZ(1, 0, 0);//x轴
                ax3D_door.Axis = model.Instances.New<IfcDirection>();
                ax3D_door.Axis.SetXYZ(0, 0, 1);//Z轴
                ax3D_door.Location = origin2;
                //lp_door.RelativePlacement = wallplace.RelativePlacement;
                lp_door.RelativePlacement = ax3D_door;
                lp_door.PlacementRelTo = wallplace;
                door.ObjectPlacement = lp_door;
                door.Representation = rep_door;
                // var m_door_style = model.Instances.New<IfcSurfaceStyle>();
                ////////////////////////////////////////////////////////////////////
                var m_OpeningEle = model.Instances.New<IfcOpeningElement>();
                m_OpeningEle.Name = "My Openings";
                m_OpeningEle.PredefinedType = IfcOpeningElementTypeEnum.OPENING;

                var rectOpening = model.Instances.New<IfcRectangleProfileDef>();
                rectOpening.ProfileType = IfcProfileTypeEnum.AREA;
                rectOpening.XDim = width;
                rectOpening.YDim = length;
                rectOpening.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectOpening.Position.Location = insertPoint;

                //模型区域实心
                var body_Opeinging = model.Instances.New<IfcExtrudedAreaSolid>();
                body_Opeinging.Depth = height;
                body_Opeinging.SweptArea = rectOpening;
                body_Opeinging.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body_Opeinging.ExtrudedDirection.SetXYZ(0, 0, 1);
                body_Opeinging.Position = model.Instances.New<IfcAxis2Placement3D>();
                body_Opeinging.Position.Location = origin;
                var shape__Opeinging = model.Instances.New<IfcShapeRepresentation>();
                var modelContext__Opeinging = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape__Opeinging.ContextOfItems = modelContext__Opeinging;
                shape__Opeinging.RepresentationType = "SweptSolid";
                shape__Opeinging.RepresentationIdentifier = "Body";
                shape__Opeinging.Items.Add(body_Opeinging);

                //创建产品定义并将模型几何添加到墙上
                var rep_Opening = model.Instances.New<IfcProductDefinitionShape>();
                rep_Opening.Representations.Add(shape__Opeinging);
                m_OpeningEle.ObjectPlacement = lp_door;
                m_OpeningEle.Representation = rep_Opening;

                var m_RelFills = model.Instances.New<IfcRelFillsElement>();
                m_RelFills.RelatingOpeningElement = m_OpeningEle;
                m_RelFills.RelatedBuildingElement = door;
                var voidRel = model.Instances.New<IfcRelVoidsElement>();
                voidRel.RelatedOpeningElement = m_OpeningEle;
                voidRel.RelatingBuildingElement = wall;

                var material98 = model.Instances.New<IfcMaterial>();
                material98.Name = "Glass";
                var material100 = model.Instances.New<IfcMaterial>();
                material100.Name = "Wood";

                var n_ifcMaterialConstituentSet = model.Instances.New<IfcMaterialConstituentSet>();
                var n_ifcMaterialConstituent = model.Instances.New<IfcMaterialConstituent>();
                n_ifcMaterialConstituent.Category = "Framing";
                n_ifcMaterialConstituent.Material = material98;
                var n_ifcMaterialConstituent100 = model.Instances.New<IfcMaterialConstituent>();
                n_ifcMaterialConstituent100.Category = "Framing";
                n_ifcMaterialConstituent100.Material = material100;
                //n_ifcMaterialConstituent.Model = door;
                n_ifcMaterialConstituentSet.MaterialConstituents.Add(n_ifcMaterialConstituent);
                n_ifcMaterialConstituentSet.MaterialConstituents.Add(n_ifcMaterialConstituent100);
                var ifcRelAssociatesMaterial = model.Instances.New<IfcRelAssociatesMaterial>();
                ifcRelAssociatesMaterial.RelatingMaterial = n_ifcMaterialConstituentSet;
                ifcRelAssociatesMaterial.RelatedObjects.Add(door);

                var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "Reference";
                    psv.Description = "Reference";
                    psv.NominalValue = new IfcTimeMeasure(150.0);
                    psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                    {
                        siu.UnitType = IfcUnitEnum.TIMEUNIT;
                        siu.Name = IfcSIUnitName.SECOND;
                    });
                });
                var ifcPropertySingleValue2 = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "FireRating";
                    psv.Description = "";

                });
                var ifcPropertySingleValue3 = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "AcousticRating";
                    psv.Description = "AcousticRating";

                });
                var ifcPropertySingleValue4 = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "IsExternal";
                    psv.Description = "IsExternal";
                    psv.NominalValue = new IfcBoolean(true);

                });
                var ifcPropertySingleValue5 = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "Infiltration";
                    psv.Description = "Infiltration";
                    psv.NominalValue = new IfcReal(0.3);

                });
                var ifcPropertySingleValue6 = model.Instances.New<IfcPropertySingleValue>(psv =>
                {
                    psv.Name = "ThermalTransmittance";
                    psv.Description = "ThermalTransmittance";
                    psv.NominalValue = new IfcReal(0.24);

                });
                //设置模型元素数量
                var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
                {
                    ps.Name = "Pset_WindowCommon";
                    ps.Description = "Property Set";
                    ps.HasProperties.Add(ifcPropertySingleValue);
                    ps.HasProperties.Add(ifcPropertySingleValue2);
                    ps.HasProperties.Add(ifcPropertySingleValue3);
                    ps.HasProperties.Add(ifcPropertySingleValue4);
                    ps.HasProperties.Add(ifcPropertySingleValue5);
                    ps.HasProperties.Add(ifcPropertySingleValue6);
                });
                //需建立关系
                model.Instances.New<IfcRelDefinesByProperties>(
                    rdbp =>
                    {
                        rdbp.Name = "Property Association";
                        rdbp.Description = "IfcPropertySet associated to wall";
                        rdbp.RelatedObjects.Add(door);
                        rdbp.RelatingPropertyDefinition = ifcPropertySet;
                    });

                txn.Commit();
                return door;
            }

        }
    }
}
