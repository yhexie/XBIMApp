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
namespace XBIMApp
{
    class AxIndoorIFCCreator
    {
        public List<AxPolyline> m_Polylines;
        public void setPolylines(List<AxPolyline> Polylines_)
        {
            m_Polylines=Polylines_;
        }
        public void CreateBuilding(string fileName_)
        {  
            using (var model = CreateandInitModel("HelloWall"))
            {
                if (model != null)
                {
                    IfcBuilding building = CreateBuilding(model, "Default Building");
                    List<IfcWallStandardCase> m_Walls=new List<IfcWallStandardCase>();
                    if (m_Polylines.Count>0)
                    {
                        for (int i = 0; i < m_Polylines.Count; i++)
                        {
                            List<double> x = m_Polylines[i].polylineX;
                            List<double> y = m_Polylines[i].polylineY;
                          
                            if (x.Count > 2)
                            {
                                IfcWallStandardCase wall = CreateCurvedWall(model, x, y, 300, 2400);
                                m_Walls.Add(wall);
                            }
                            else if (x.Count == 2)
                            {
                                double fromX = x[0];
                                double fromY = y[0];
                                double toX = x[1];
                                double toY = y[1];
                                IfcWallStandardCase wall = CreateWall(model, fromX, fromY, toX, toY, 300, 2400);
                                m_Walls.Add(wall);
                            }
                        }         
                    }
                    
                    //IfcWallStandardCase wall2 = CreateWall(model, 0, 1000, 0, 2000, 300, 2500);
                    //List<double> x = new List<double>();
     
                    //x.Add(0);
                    //x.Add(0);
                    //x.Add(1000);
                    //List<double> y = new List<double>();
                  
                    //y.Add(2000);
                    //y.Add(3000);
                    //y.Add(5000);
                    //IfcWallStandardCase wall3= CreateCurvedWall(model,x, y, 300, 2400);
                    //IfcDoor door = CreateDoor(model, wall3, 0, 3000,1000,5000, 300, 2200);
                   // IfcWindow widnow = CreateWindow(model, wall3, 1000, 300, 1000);
                   // if (wall != null) AddPropertiesToWall(model, wall);
                    using (var txn = model.BeginTransaction("Add Wall"))
                    {
                        if (m_Walls.Count>0)
                        {
                            for (int i = 0; i < m_Walls.Count; i++)
                            {
                                IfcWallStandardCase wall = m_Walls[i];
                                  building.AddElement(wall);
                            }  
                        }
                        //building.AddElement(wall);
                        //building.AddElement(wall3);
                        //building.AddElement(door);
                        //building.AddElement(widnow);
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
        public IfcWallStandardCase CreateWall(IfcStore model, double fromX,double fromY, double toX,double toY, double width, double height)
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
                double length = Math.Sqrt((toX - fromX) * (toX - fromX)+(toY - fromY) * (toY - fromY));
                
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
                ax3D.RefDirection.SetXYZ(1,0, 0);
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1);
                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(mid_x, mid_y, 0);
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

        public IfcWallStandardCase CreateCurvedWall(IfcStore model, List<double> curve_X, List<double> curve_Y, double width, double height)
        {
            //启动事务
            using (var txn = model.BeginTransaction("Create Curved Wall"))
            {
                var wall = model.Instances.New<IfcWallStandardCase>();
                wall.Name = "A curved wall";

                IfcPolyline curve = model.Instances.New<IfcPolyline>();
                for (int i = 0; i < curve_X.Count; i++)
                {
                    var pt0 = model.Instances.New<IfcCartesianPoint>();
                    double pt_X = curve_X[i];
                    double pt_Y = curve_Y[i];
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
                ax3D.Location = origin;
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

        public IfcDoor CreateDoor(IfcStore model, IfcWallStandardCase wall, double fromX,double fromY, double toX,double toY, double width, double height)
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

                var door = model.Instances.New<IfcDoor>();
                door.Name = "A Door";
                door.PredefinedType = IfcDoorTypeEnum.GATE;
                door.OperationType = IfcDoorTypeOperationEnum.DOUBLE_SWING_LEFT;
                door.OverallHeight = 400;
                door.OverallWidth = 400;


                var lp_door = model.Instances.New<IfcLocalPlacement>();
                var wallplace = wall.ObjectPlacement;
                var ax3D_door = model.Instances.New<IfcAxis2Placement3D>();
                var origin2 = model.Instances.New<IfcCartesianPoint>();
                origin2.SetXYZ(mid_x, mid_y, 10);

                ax3D_door.RefDirection = model.Instances.New<IfcDirection>();
                ax3D_door.RefDirection.SetXYZ(-dy, dx, 0);//x轴
                ax3D_door.Axis = model.Instances.New<IfcDirection>();
                ax3D_door.Axis.SetXYZ(0, 0, 1);//Z轴
                ax3D_door.Location = origin2;
                //lp_door.RelativePlacement = wallplace.RelativePlacement;
                lp_door.RelativePlacement = ax3D_door;
                lp_door.PlacementRelTo = wallplace;
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
