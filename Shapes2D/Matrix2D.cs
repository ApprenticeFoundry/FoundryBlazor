

using FoundryBlazor.Extensions;
using System.Drawing;
/**
* Represents an affine transformation matrix, and provides tools for constructing and concatenating matrices.
*
* This matrix can be visualized as:
*
* 	[ a  c  tx
* 	  b  d  ty
* 	  0  0  1  ]
*
* Note the locations of b and c.
*
* @class Matrix2D
* @param {Number} [a=1] Specifies the a property for the new matrix.
* @param {Number} [b=0] Specifies the b property for the new matrix.
* @param {Number} [c=0] Specifies the c property for the new matrix.
* @param {Number} [d=1] Specifies the d property for the new matrix.
* @param {Number} [tx=0] Specifies the tx property for the new matrix.
* @param {Number} [ty=0] Specifies the ty property for the new matrix.
* @constructor
**/
namespace FoundryBlazor.Shape;

 public class Matrix2D 
 {
    public static readonly double DEG_TO_RAD = Math.PI / 180;
    public static readonly double RAD_TO_DEG = 180 / Math.PI;
    public static readonly double TWO_PI = 2.0 * Math.PI;
    private string svg = "";

    public double a = 1; //Position (0, 0) in a 3x3 affine transformation matrix.
    public double b = 0; //Position (0, 1) in a 3x3 affine transformation matrix.
    public double c = 0; //Position (1, 0) in a 3x3 affine transformation matrix.
    public double d = 1; //Position (1, 1) in a 3x3 affine transformation matrix.
    public double tx = 0; //Position (2, 0) in a 3x3 affine transformation matrix.
    public double ty = 0; //Position (2, 1) in a 3x3 affine transformation matrix.

    private static readonly Queue<Matrix2D> cashe = new Queue<Matrix2D>();
    public static Matrix2D NewMatrix()
    {
        if (cashe.Count == 0)
            return new Matrix2D();

        //$"Recycle Matrix2D {cashe.Count}".WriteInfo();
        var result = cashe.Dequeue();
        return result;
    }

    public static Matrix2D? SmashMatrix(Matrix2D? source)
    {
        if (source == null) return null;

        source.Zero();
        cashe.Enqueue(source);
       // $"Smash Matrix2D {cashe.Count}".WriteNote();
        return null;
    }

    public Matrix2D() 
    {  
    }
    public Matrix2D(Matrix2D matrix) 
    {
        Copy(matrix);
    }

    public bool IsSVGRefreshed()
    {
        if (!string.IsNullOrEmpty(svg))
            return true;
        return false;
    }

    public string SVGMatrix()
    {
        if (IsSVGRefreshed())
            return svg;

        svg = $"matrix({a}, {b}, {c}, {d}, {tx}, {ty})";
        return svg;
    }

    public Matrix2D Copy(Matrix2D matrix) 
    {
        a = matrix.a;
        b = matrix.b;
        c = matrix.c;
        d = matrix.d;
        tx = matrix.tx;
        ty = matrix.ty;
        return this;
    }

    public bool Equals(Matrix2D matrix ) {
        return tx == matrix.tx && ty == matrix.ty && a == matrix.a && b == matrix.b && c == matrix.c && d == matrix.d;
    }

    public Matrix2D Append(double a, double b, double c, double d, double tx, double ty) 
    {
        var a1 = this.a;
        var b1 = this.b;
        var c1 = this.c;
        var d1 = this.d;
        if (a != 1 || b != 0 || c != 0 || d != 1) {
            this.a = a1 * a + c1 * b;
            this.b = b1 * a + d1 * b;
            this.c = a1 * c + c1 * d;
            this.d = b1 * c + d1 * d;
        }
        this.tx = a1 * tx + c1 * ty + this.tx;
        this.ty = b1 * tx + d1 * ty + this.ty;
        return this;
    }  

    public Matrix2D Prepend(double a, double b, double c, double d, double tx, double ty) 
    {
        var a1 = this.a;
        var c1 = this.c;
        var tx1 = this.tx;

        this.a = a * a1 + c * this.b;
        this.b = b * a1 + d * this.b;
        this.c = a * c1 + c * this.d;
        this.d = b * c1 + d * this.d;
        this.tx = a * tx1 + c * this.ty + tx;
        this.ty = b * tx1 + d * this.ty + ty;
        return this;
    }
    public Matrix2D Set(double a, double b, double c, double d, double tx, double ty) 
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.tx = tx;
        this.ty = ty;
        return this;
    }

    public Matrix2D Zero()
    {
        this.a = 1;
        this.b = 0;
        this.c = 0;
        this.d = 1;
        this.tx = 0;
        this.ty = 0;
        this.svg = "";
        return this;
    }

    public Matrix2D AppendMatrix(Matrix2D matrix ) {
        return Append(matrix.a, matrix.b, matrix.c, matrix.d, matrix.tx, matrix.ty);
    }

    public Matrix2D PrependMatrix(Matrix2D matrix) {
        return Prepend(matrix.a, matrix.b, matrix.c, matrix.d, matrix.tx, matrix.ty);
    }

    public Matrix2D AppendTransform(double x, double y, double scaleX, double scaleY, double rotation, double regX, double regY) {

        var r = rotation * Matrix2D.DEG_TO_RAD;
        var cos = Math.Cos(r);
        var sin = Math.Sin(r);

        Append(cos * scaleX, sin * scaleX, -sin * scaleY, cos * scaleY, x, y);
        
        // append the registration offset:
        tx -= regX * a + regY * c;
        ty -= regX * b + regY * d;
        
        return this;
    }  

    public Matrix2D AppendTransformWithSkey(double x, double y, double scaleX, double scaleY, double rotation, double skewX, double skewY, double regX, double regY) {

        var r = rotation * Matrix2D.DEG_TO_RAD;
        var cos = Math.Cos(r);
        var sin = Math.Sin(r);


        if (skewX != 0.0 || skewY != 0.0) {
            // TODO: can this be combined into a single append operation?
            skewX *= Matrix2D.DEG_TO_RAD;
            skewY *= Matrix2D.DEG_TO_RAD;
            Append(Math.Cos(skewY), Math.Sin(skewY), -Math.Sin(skewX), Math.Cos(skewX), x, y);
            Append(cos * scaleX, sin * scaleX, -sin * scaleY, cos * scaleY, 0, 0);
        } else {
            Append(cos * scaleX, sin * scaleX, -sin * scaleY, cos * scaleY, x, y);
        }


        // append the registration offset:
        tx -= regX * a + regY * c;
        ty -= regX * b + regY * d;
        
        return this;
    }   

    public Matrix2D PrependTransform(double x, double y, double scaleX, double scaleY, double rotation, double skewX, double skewY, double regX, double regY) {

        var r = rotation * Matrix2D.DEG_TO_RAD;
        var cos = Math.Cos(r);
        var sin = Math.Sin(r);
   

        // prepend the registration offset:
        tx -= regX;
        ty -= regY;
        
        if (skewX != 0.0 || skewY != 0.0) {
            // TODO: can this be combined into a single prepend operation?
            skewX *= Matrix2D.DEG_TO_RAD;
            skewY *= Matrix2D.DEG_TO_RAD;
            Prepend(cos * scaleX, sin * scaleX, -sin * scaleY, cos * scaleY, 0, 0);
            Prepend(Math.Cos(skewY), Math.Sin(skewY), -Math.Sin(skewX), Math.Cos(skewX), x, y);
        } else {
            Prepend(cos * scaleX, sin * scaleX, -sin * scaleY, cos * scaleY, x, y);
        }
        return this;
    } 

    public Matrix2D Rotate(double angle) 
    {
        var ang = angle * Matrix2D.DEG_TO_RAD;
        var cos = Math.Cos(ang);
        var sin = Math.Sin(ang);

        var a1 = a;
        var b1 = b;

        a = a1 * cos + c * sin;
        b = b1 * cos + d * sin;
        c = -a1 * sin + c * cos;
        d = -b1 * sin + d * cos;
        return this;
    }   

    public Matrix2D Skew(double skewX, double skewY) {
        var skewx = skewX * Matrix2D.DEG_TO_RAD;
        var skewy = skewY * Matrix2D.DEG_TO_RAD;
        Append(Math.Cos(skewy), Math.Sin(skewy), -Math.Sin(skewx), Math.Cos(skewx), 0, 0);
        return this;
    }

    public Matrix2D Scale(double x, double y) {
        a *= x;
        b *= x;
        c *= y;
        d *= y;
        return this;
    }

     public Matrix2D  Translate(double x, double y) {
        tx += a * x + c * y;
        ty += b * x + d * y;
        return this;
    }

    public Matrix2D Identity() {
        a = d = 1;
        b = c = tx = ty = 0;
        return this;
    }   

    public Matrix2D Invert() {
        var a1 = a;
        var b1 = b;
        var c1 = c;
        var d1 = d;
        var tx1 = tx;
        //var ty1 = ty;
        var n = a1 * d1 - b1 * c1;

        a = d1 / n;
        b = -b1 / n;
        c = -c1 / n;
        d = a1 / n;
        tx = (c1 * ty - d1 * tx1) / n;
        ty = -(a1 * ty - b1 * tx1) / n;
        return this;
    }    

     public Matrix2D InvertCopy() {
        var result = new Matrix2D(this);
        return result.Invert();
    }

    public bool IsIdentity() {
        return tx == 0 && ty == 0 && a == 1 && b == 0 && c == 0 && d == 1;
    }  



    public Matrix2D Clone() 
    {
        var matrix = new Matrix2D(this);
        return matrix;
    }

    public static Point NoTransformPoint(int x, int y) 
    {
        var pt = new Point(x, y);
        return pt;
    }
    public static Point NoTransformPoint(Point pt) 
    {
        return pt;
    }

    public (int, int) TransformPoint(int x, int y) 
    {
        var X = x * this.a + y * this.c + this.tx;
        var Y = x * this.b + y * this.d + this.ty;
        //var pt = new Point((int)X, (int)Y);
        return ((int)X, (int)Y);
    }
    public Point TransformToPoint(int x, int y) 
    {
        var (X,Y) = TransformPoint(x, y);
        var pt = new Point(X, Y);
        return pt;
    }
    public Rectangle TransformRectangle(int x, int y, int width, int height, Rectangle rect) 
    {
        // rect.X = (int)(x * this.a + y * this.c + this.tx);
        // rect.Y = (int)(x * this.b + y * this.d + this.ty);
        // rect.Width = width;
        // rect.Height = height;

        double X = this.a * x + this.c * y + this.tx;
        double Y = this.b * x + this.d * y + this.ty;
        
        double x2 = this.a * (x + width) + this.c * (y + height) + this.tx;
        double y2 = this.b * (x + width) + this.d * (y + height) + this.ty;


        double Width = Math.Abs(x2 - X); 
        double Height = Math.Abs(y2 - Y);

        rect.X = (int)x;
        rect.Y = (int)y;
        rect.Width = (int)Width;
        rect.Height =(int)Height;
        return rect;
    }
    // public Point TransformPoint(Point pt) 
    // {
    //     return TransformPoint(pt.X,pt.Y);
    // }
    


 }