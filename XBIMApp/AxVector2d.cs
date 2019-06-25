using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBIMApp
{
    /// <summary>
    /// 3D向量类
    /// </summary>
    public class Vector2d
    {
        public double[] vector;
        private const double E = 0.0000001f;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// 
        public Vector2d()
        {
            vector = new double[2];
        }
        public Vector2d(double x, double y)
        {
            vector = new double[2] { x, y};
        }
        public Vector2d(Vector2d vct)
        {
            vector = new double[2];
            vector[0] = vct.X;
            vector[1] = vct.Y;
        }

        public Vector3d ToVector3d()
        {
            Vector3d vec3 = new Vector3d();
            vec3.X = this.X;
            vec3.Y = this.Y;
            vec3.Z = 0;
            return vec3;
        }

        #region 属性
        /// <summary>
        /// X向量
        /// </summary>
        public double X
        {
            get { return vector[0]; }
            set { vector[0] = value; }
        }
        /// <summary>
        /// Y向量
        /// </summary>
        public double Y
        {
            get { return vector[1]; }
            set { vector[1] = value; }
        }
       
        #endregion

        #region 向量操作
        /// <summary>
        /// /// <summary>
        /// 向量加法+
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2d operator +(Vector2d lhs, Vector2d rhs)//向量加
        {
            Vector2d result = new Vector2d(lhs);
            result.X += rhs.X;
            result.Y += rhs.Y;
            return result;
        }
        /// <summary>
        /// 向量减-
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2d operator -(Vector2d lhs, Vector2d rhs)//向量减法
        {
            Vector2d result = new Vector2d(lhs);
            result.X -= rhs.X;
            result.Y -= rhs.Y;
            return result;
        }
        /// <summary>
        /// 向量除
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2d operator /(Vector2d lhs, double rhs)//向量除以数量
        {
            if (rhs != 0)
                return new Vector2d(lhs.X / rhs, lhs.Y / rhs);
            else
                return new Vector2d(0, 0);
        }
        /// <summary>
        /// 向量数乘*
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2d operator *(double lhs, Vector2d rhs)//左乘数量
        {
            return new Vector2d(lhs * rhs.X, lhs * rhs.Y);
        }
        /// <summary>
        /// 向量数乘
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector2d operator *(Vector2d lhs, double rhs)//右乘数量
        {
            return new Vector2d(lhs.X * rhs, lhs.Y * rhs);
        }

        /// <summary>
        /// 判断量向量是否相等
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>True 或False</returns>
        public static bool operator ==(Vector2d lhs, Vector2d rhs)
        {
            if (Math.Abs(lhs.X - rhs.X) < E && Math.Abs(lhs.Y - rhs.Y) < E)
                return true;
            else
                return false;
        }
        public static bool operator !=(Vector2d lhs, Vector2d rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
        /// <summary>
        /// 向量叉积，求与两向量垂直的向量
        /// </summary>
        public static Vector3d Cross(Vector2d v1, Vector2d v2)
        {
            Vector3d r = new Vector3d(0, 0, 0);
            r.X = 0;
            r.Y = 0;
            r.Z = (v1.X * v2.Y) - (v1.Y * v2.X);
            return r;
        }
        /// <summary>
        /// 向量数量积
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static double operator *(Vector2d lhs, Vector2d rhs)//
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y ;
        }
        /// <summary>
        /// 内积
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double InnerMultiply(Vector2d v1, Vector2d v2)
        {
            double inner = 0.0;
            inner = v1.X * v2.X + v1.Y * v2.Y ;
            return inner;
        }
        /// <summary>
        /// 求向量长度，向量的模
        /// </summary>
        public static double Magnitude(Vector2d v1)
        {
            return (double)Math.Sqrt((v1.X * v1.X) + (v1.Y * v1.Y) );
        }
        /// <summary>
        /// 单位化向量
        /// </summary>
        public static Vector2d Normalize(Vector2d v1)
        {
            double magnitude = Magnitude(v1);
            v1 = v1 / magnitude;
            return v1;
        }
        #endregion
    }

}
