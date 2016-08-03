using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRPhotoMosaic.Method
{
    public class ColorSpace
    {
        public static double ref_a = 500.0f * 25.0f / 29.0f;
        public static double ref_b = 200.0f * 25.0f / 29.0f;
        public struct AllColorSpace
        {
            public ColorSpace.RGB RGB;
            public ColorSpace.YUV YUV;
            public ColorSpace.HSL HSL;
            public ColorSpace.HSV HSV;
            public ColorSpace.XYZ XYZ;
            public ColorSpace.Lab Lab;
        }

        public struct YUV 
        {
            public double Y;
            public double U;
            public double V;
        }

        public struct RGB
        {
            public int R;
            public int G;
            public int B;
        }

        public struct HSL 
        {
            public double H;
            public double S;
            public double L;
        }

        public struct HSV
        {
            public double H;
            public double S;
            public double V;
        }

        public struct XYZ
        {
            public double X;
            public double Y;
            public double Z;
        }

        public struct Lab
        {
            public double L;
            public double a;
            public double b;
        }

        public YUV RGB2YUV(int R, int G, int B)
        {
            YUV CSYUV;

            CSYUV.Y = 0.299 * R + 0.587 * G + 0.114 * B;
            CSYUV.U = -0.169 * R - 0.331 * G + 0.5 * B + 128;
            CSYUV.V = 0.5 * R - 0.419 * G - 0.081 * B + 128;

            return CSYUV;
        }

        public RGB YUV2RGB(double Y, double U, double V)
        {
            RGB CSRGB;

            CSRGB.R = Convert.ToInt32(Y - 0.00093 * (U - 128) + 1.401687 * (V - 128));
            CSRGB.G = Convert.ToInt32(Y - 0.3437 * (U - 128) - 0.71417 * (V - 128));
            CSRGB.B = Convert.ToInt32(Y + 1.77216 * (U - 128) + 0.00099 * (V - 128));

            CSRGB = RGB_ArgumentException_Handle(CSRGB);
            return CSRGB;
        }

        public HSL RGB2HSL(int R, int G, int B)
        {
            HSL CSHSL;
            double dou_R = R / 255.0;
            double dou_G = G / 255.0;
            double dou_B = B / 255.0;   //HSL要先將RGB弄到∈[0,1]

            double max_RGB = Max<double>(dou_R, dou_G, dou_B); //MAX value of RGB
            double min_RGB = Min<double>(dou_R, dou_G, dou_B); //Min value of RGB
            double del_RGB = max_RGB-min_RGB; //Delta RGB value
            double del_R=0;
            double del_G=0;
            double del_B=0;
            CSHSL.H = 0;
            CSHSL.L = (max_RGB + min_RGB) / 2.0;

            if (max_RGB == 0)
            {
                CSHSL.H = 0;
                CSHSL.S = 0;
            }
            else
            {
                if (CSHSL.L < 0.5)
                    CSHSL.S = del_RGB / (max_RGB + min_RGB);
                else if ((2.0 - max_RGB - min_RGB) == 0)
                    CSHSL.S = del_RGB / 1;
                else
                    CSHSL.S = del_RGB / (2.0 - max_RGB - min_RGB);
                if (del_RGB == 0)
                {
                    del_R = (((max_RGB - dou_R) / 6.0) + (del_RGB / 2.0)) / 1;
                    del_G = (((max_RGB - dou_G) / 6.0) + (del_RGB / 2.0)) / 1;
                    del_B = (((max_RGB - dou_B) / 6.0) + (del_RGB / 2.0)) / 1;
                }
                else
                {
                    del_R = (((max_RGB - dou_R) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                    del_G = (((max_RGB - dou_G) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                    del_B = (((max_RGB - dou_B) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                }

                if (dou_R == max_RGB)
                    CSHSL.H = del_B - del_G;
                else if (dou_G == max_RGB)
                    CSHSL.H = (1.0 / 3.0) + del_R - del_B;
                else if (dou_B == max_RGB)
                    CSHSL.H = (2.0 / 3.0) + del_G - del_R;

                if (CSHSL.H < 0)
                    CSHSL.H += 1;
                if (CSHSL.H > 1)
                    CSHSL.H -= 1;
            }
    
            return CSHSL;
        }

        public RGB HSL2RGB(double H, double S, double L)
        {
            RGB CSRGB;
            double var1=0,var2=0;
            if (S == 0)
            {
                CSRGB.R = Convert.ToInt32(L * 255.0);
                CSRGB.G = Convert.ToInt32(L * 255.0);
                CSRGB.B = Convert.ToInt32(L * 255.0);
            }
            else
            {
                if (L < 0.5)
                    var2 = L * (1.0 + S);
                else
                    var2 = (L + S) - (S * L);

                var1 = 2.0 * L - var2;

                CSRGB.R = Convert.ToInt32(255.0 * Hue2RGB(var1, var2, H + (1.0 / 3.0)));
                CSRGB.G = Convert.ToInt32(255.0 * Hue2RGB(var1,var2,H));
                CSRGB.B = Convert.ToInt32(255.0 * Hue2RGB(var1, var2, H - (1.0 / 3.0)));
            }
            CSRGB = RGB_ArgumentException_Handle(CSRGB);
            return CSRGB;
        }

        private double Hue2RGB( double v1, double v2, double vH )
        {
            if ( vH < 0 ) 
                vH += 1;
            if ( vH > 1 )
                vH -= 1;
            if ( ( 6.0 * vH ) < 1 ) 
                return v1 + ( v2 - v1 ) * 6.0 * vH;
            if ( ( 2.0 * vH ) < 1 ) 
                return v2;
            if ((3.0 * vH) < 2)
                return v1 + (v2 - v1) * ((2.0 / 3.0) - vH) * 6.0;

            return v1; 
        }

        public HSV RGB2HSV(int R, int G, int B)
        { 
            HSV CSHSV;

            double dou_R = R / 255.0;
            double dou_G = G / 255.0;
            double dou_B = B / 255.0;

            double max_RGB = Max<double>(dou_R, dou_G, dou_B); //MAX value of RGB
            double min_RGB = Min<double>(dou_R, dou_G, dou_B); //Min value of RGB
            double del_RGB = max_RGB-min_RGB; //Delta RGB value
            double del_R = 0;
            double del_G = 0;
            double del_B = 0;
            CSHSV.H = 0;
            CSHSV.V = max_RGB;

            if (del_RGB == 0)                     //This is a gray, no chroma...
            {
               CSHSV.H = 0;                         //HSV results from 0 to 1
               CSHSV.S = 0;
            }
            else                                   //Chromatic data...
            {
                if (max_RGB == 0 )
                    CSHSV.S = del_RGB / 1;
                else
                    CSHSV.S = del_RGB / max_RGB;


                if (del_RGB == 0)
                {
                    del_R = (((max_RGB - dou_R) / 6.0) + (del_RGB / 2.0)) / 1;
                    del_G = (((max_RGB - dou_G) / 6.0) + (del_RGB / 2.0)) / 1;
                    del_B = (((max_RGB - dou_B) / 6.0) + (del_RGB / 2.0)) / 1;
                }
                else
                {
                    del_R = (((max_RGB - dou_R) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                    del_G = (((max_RGB - dou_G) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                    del_B = (((max_RGB - dou_B) / 6.0) + (del_RGB / 2.0)) / del_RGB;
                }

                if (dou_R == max_RGB) 
                    CSHSV.H = del_B - del_G;
                else if (dou_G == max_RGB)
                    CSHSV.H = ( 1.0 / 3.0 ) + del_R - del_B;
                else if (dou_B == max_RGB)
                    CSHSV.H = ( 2.0 / 3.0 ) + del_G - del_R;

                if (CSHSV.H < 0)
                    CSHSV.H += 1;
                if (CSHSV.H > 1)
                    CSHSV.H -= 1;
            }

            return CSHSV;
        }

        public RGB HSV2RGB(double H, double S, double V)
        {
            RGB CSRGB;
            double p,q,t,f, var_h ,dou_R,dou_G,dou_B;
            int var_i;

            if (S == 0)
            {
                CSRGB.R = Convert.ToInt32(V * 255.0);
                CSRGB.G = Convert.ToInt32(V * 255.0);
                CSRGB.B = Convert.ToInt32(V * 255.0);
            }
            else
            {
                var_h = (H * 6) % 6;
                var_i = (int)(var_h);
                f = var_h - var_i;
                p = V * (1 - S);
                q = V * (1 - S * f);
                t = V * (1 - S * (1 - f));

                if (var_i == 0)
                {
                     dou_R = V;
                     dou_G = t;
                     dou_B = p;
                }
                else if (var_i == 1)
                { 
                     dou_R = q;
                     dou_G = V;
                     dou_B = p;
                }
                else if (var_i == 2) 
                { 
                    dou_R = p;
                    dou_G = V;
                    dou_B = t;
                }
                else if (var_i == 3) 
                { 
                    dou_R = p;
                    dou_G = q;
                    dou_B = V;

                }
                else if (var_i == 4) 
                {     
                    dou_R = t;
                    dou_G = p;
                    dou_B = V;
                }
                else
                {
                    dou_R = V;
                    dou_G = p;
                    dou_B = q;
                }

                CSRGB.R = Convert.ToInt32(dou_R * 255.0);              //RGB results from 0 to 255
                CSRGB.G = Convert.ToInt32(dou_G * 255.0);
                CSRGB.B = Convert.ToInt32(dou_B * 255.0);
               
            }
            CSRGB = RGB_ArgumentException_Handle(CSRGB);
            return CSRGB;
        }

        private RGB RGB_ArgumentException_Handle(RGB CSRGB)
        { 
            if (CSRGB.R < 0)
                CSRGB.R = 0;
            else if (CSRGB.R > 255)
                CSRGB.R = 255;

            if (CSRGB.G < 0)
                CSRGB.G = 0;
            else if (CSRGB.G > 255)
                CSRGB.G = 255;

            if (CSRGB.B < 0)
                CSRGB.B = 0;
            else if (CSRGB.B > 255)
                CSRGB.B = 255;

            return CSRGB;
        }

        public T Min<T>(T R, T G, T B) 
        { 
           T[] array ={R,G,B};
           return array.Min<T>();
        }

        public T Max<T>(T R, T G, T B)
        {
            T[] array = { R, G, B };
            return array.Max<T>();
        }

        public XYZ RGB2XYZ (int R, int G, int B) 
        {
            XYZ CSXYZ;

            double dou_R = R / 255.0;
            double dou_G = G / 255.0;
            double dou_B = B / 255.0; 

            dou_R=PivotRgb(dou_R);
            dou_G=PivotRgb(dou_G);
            dou_B=PivotRgb(dou_B);
      
            //Observer. = 2°, Illuminant = D65
            dou_R = dou_R * 100;
            dou_G = dou_G * 100;
            dou_B = dou_B * 100;

            CSXYZ.X = dou_R * 0.412453 + dou_G * 0.357580 + dou_B * 0.180423;
            CSXYZ.Y = dou_R * 0.212671 + dou_G * 0.715160 + dou_B * 0.072169;
            CSXYZ.Z = dou_R * 0.019334 + dou_G * 0.119193 + dou_B * 0.950227;
                
            return CSXYZ;
        }

        public XYZ RGBtoXYZ(RGB RGB)
        {
            XYZ CSXYZ;

            double dou_R = RGB.R / 255.0;
            double dou_G = RGB.G / 255.0;
            double dou_B = RGB.B / 255.0;

            dou_R = PivotRgb(dou_R);
            dou_G = PivotRgb(dou_G);
            dou_B = PivotRgb(dou_B);

            //Observer. = 2°, Illuminant = D65
            dou_R = dou_R * 100;
            dou_G = dou_G * 100;
            dou_B = dou_B * 100;

            CSXYZ.X = dou_R * 0.412453 + dou_G * 0.357580 + dou_B * 0.180423;
            CSXYZ.Y = dou_R * 0.212671 + dou_G * 0.715160 + dou_B * 0.072169;
            CSXYZ.Z = dou_R * 0.019334 + dou_G * 0.119193 + dou_B * 0.950227;

            return CSXYZ;
        }

        private double PivotRgb(double n)
        {
            if (n > 0.04045)
                return Math.Pow((n + 0.055) / 1.055, 2.4);
            else
                return  n/12.92;
        }

        public RGB XYZ2RGB(double X, double Y, double Z) 
        {
            RGB CSRGB = new RGB();
            double dou_R,dou_G,dou_B;

            double dou_X = X / 100.0;        //X from 0 to  95.047      (Observer = 2°, Illuminant = D65)
            double dou_Y = Y / 100.0;        //Y from 0 to 100.000
            double dou_Z = Z / 100.0;        //Z from 0 to 108.883

            dou_R = dou_X *  3.240479 + dou_Y * -1.537150 + dou_Z * -0.498535;
            dou_G = dou_X * -0.969256 + dou_Y *  1.875992 + dou_Z *  0.041556;
            dou_B = dou_X *  0.055648 + dou_Y * -0.204043 + dou_Z *  1.057311;

            if ( dou_R > 0.0031308 ) 
                dou_R = 1.055 * Math.Pow(dou_R ,  1 / 2.4 ) - 0.055;
            else                     
                dou_R = 12.92 * dou_R;

            if ( dou_G > 0.0031308 ) 
                dou_G = 1.055 * Math.Pow(dou_G ,  1 / 2.4 ) - 0.055;
            else                     
                dou_G = 12.92 * dou_G;

            if ( dou_B > 0.0031308 ) 
                dou_B = 1.055 * Math.Pow(dou_B ,  1 / 2.4 ) - 0.055;
            else                     
                dou_B = 12.92 * dou_B;

            CSRGB.R = Convert.ToInt32(dou_R * 255);
            CSRGB.G = Convert.ToInt32(dou_G * 255);
            CSRGB.B = Convert.ToInt32(dou_B * 255);

            return CSRGB = RGB_ArgumentException_Handle(CSRGB);
        }

        public RGB XYZ2RGB(XYZ XYZ)
        {
            RGB CSRGB = new RGB();
            double dou_R, dou_G, dou_B;

            double dou_X = XYZ.X / 100.0;        //X from 0 to  95.047      (Observer = 2°, Illuminant = D65)
            double dou_Y = XYZ.Y / 100.0;        //Y from 0 to 100.000
            double dou_Z = XYZ.Z / 100.0;        //Z from 0 to 108.883

            dou_R = dou_X * 3.240479 + dou_Y * -1.537150 + dou_Z * -0.498535;
            dou_G = dou_X * -0.969256 + dou_Y * 1.875992 + dou_Z * 0.041556;
            dou_B = dou_X * 0.055648 + dou_Y * -0.204043 + dou_Z * 1.057311;

            if (dou_R > 0.0031308)
                dou_R = 1.055 * Math.Pow(dou_R, 1 / 2.4) - 0.055;
            else
                dou_R = 12.92 * dou_R;

            if (dou_G > 0.0031308)
                dou_G = 1.055 * Math.Pow(dou_G, 1 / 2.4) - 0.055;
            else
                dou_G = 12.92 * dou_G;

            if (dou_B > 0.0031308)
                dou_B = 1.055 * Math.Pow(dou_B, 1 / 2.4) - 0.055;
            else
                dou_B = 12.92 * dou_B;

            CSRGB.R = Convert.ToInt32(dou_R * 255);
            CSRGB.G = Convert.ToInt32(dou_G * 255);
            CSRGB.B = Convert.ToInt32(dou_B * 255);

            return CSRGB = RGB_ArgumentException_Handle(CSRGB);
        }

        public Lab XYZ2Lab(double X, double Y, double Z) 
        {
            //reference http://www.brucelindbloom.com/Eqn_XYZ_to_Lab.html
            //https://en.wikipedia.org/wiki/Lab_color_space
            Lab CSLab;
            double ref_X =  95.047,ref_Y = 100.000,ref_Z = 108.883;
            double dou_X = X / ref_X;          //ref_X =  95.047   Observer= 2°, Illuminant= D65  White
            double dou_Y = Y / ref_Y;          //ref_Y = 100.000
            double dou_Z = Z / ref_Z;          //ref_Z = 108.883

            if ( dou_X > 0.008856 ) //Epsilon = 0.008856
                dou_X = Math.Pow(dou_X , 1.0/3.0);
            else                    
                dou_X = ( 7.787 * dou_X ) + ( 16.0 / 116.0 );  //Kappa = 7.787

            if ( dou_Y > 0.008856 ) 
                dou_Y = Math.Pow(dou_Y , 1.0/3.0);
            else                    
                dou_Y = ( 7.787 * dou_Y ) + ( 16.0 / 116.0 );

            if ( dou_Z > 0.008856 ) 
                dou_Z = Math.Pow(dou_Z , 1.0/3.0);
            else                    
                dou_Z = ( 7.787 * dou_Z ) + ( 16.0 / 116.0 );

            CSLab.L = Math.Max(0,116 * dou_Y - 16);
            CSLab.a = 500 * ( dou_X - dou_Y );
            CSLab.b = 200 * ( dou_Y - dou_Z );

            return CSLab;
        }

        public Lab XYZ2Lab(XYZ XYZ)
        {
            //reference http://www.brucelindbloom.com/Eqn_XYZ_to_Lab.html
            //https://en.wikipedia.org/wiki/Lab_color_space
            Lab CSLab;
            double ref_X = 95.047, ref_Y = 100.000, ref_Z = 108.883;
            double dou_X = XYZ.X / ref_X;          //ref_X =  95.047   Observer= 2°, Illuminant= D65  White
            double dou_Y = XYZ.Y / ref_Y;          //ref_Y = 100.000
            double dou_Z = XYZ.Z / ref_Z;          //ref_Z = 108.883

            if (dou_X > 0.008856) //Epsilon = 0.008856
                dou_X = Math.Pow(dou_X, 1.0 / 3.0);
            else
                dou_X = (7.787 * dou_X) + (16.0 / 116.0);  //Kappa = 7.787

            if (dou_Y > 0.008856)
                dou_Y = Math.Pow(dou_Y, 1.0 / 3.0);
            else
                dou_Y = (7.787 * dou_Y) + (16.0 / 116.0);

            if (dou_Z > 0.008856)
                dou_Z = Math.Pow(dou_Z, 1.0 / 3.0);
            else
                dou_Z = (7.787 * dou_Z) + (16.0 / 116.0);

            CSLab.L = Math.Max(0, 116 * dou_Y - 16);
            CSLab.a = 500 * (dou_X - dou_Y);
            CSLab.b = 200 * (dou_Y - dou_Z);

            return CSLab;
        }

        public XYZ LabtoXYZ(double L, double a, double b) 
        {
            XYZ CSXYZ;
            double ref_X =  95.047,ref_Y = 100.000,ref_Z = 108.883;
            double dou_Y = ( L + 16 ) / 116.0;
            double dou_X = a / 500.0 + dou_Y;
            double dou_Z = dou_Y - b / 200.0;

            if ( Math.Pow(dou_Y,3) > 0.008856 ) 
                dou_Y = Math.Pow(dou_Y,3);
            else                      
                dou_Y = ( dou_Y - 16.0 / 116.0 ) / 7.787;

            if ( Math.Pow(dou_X,3) > 0.008856 ) 
                dou_X = Math.Pow(dou_X,3);
            else                      
                dou_X = ( dou_X - 16.0 / 116.0 ) / 7.787;

            if (Math.Pow(dou_Z, 3) > 0.008856)
                dou_Z = Math.Pow(dou_Z, 3);
            else
                dou_Z = (dou_Z - 16.0 / 116.0) / 7.787;

            CSXYZ.X = ref_X * dou_X;     //ref_X =  95.047     Observer= 2°, Illuminant= D65
            CSXYZ.Y = ref_Y * dou_Y;     //ref_Y = 100.000
            CSXYZ.Z = ref_Z * dou_Z;     //ref_Z = 108.883

            return CSXYZ;
        }

        public XYZ LabtoXYZ(Lab Lab)
        {
            XYZ CSXYZ;
            double ref_X = 95.047, ref_Y = 100.000, ref_Z = 108.883;
            double dou_Y = (Lab.L + 16) / 116.0;
            double dou_X = Lab.a / 500.0 + dou_Y;
            double dou_Z = dou_Y - Lab.b / 200.0;

            if (Math.Pow(dou_Y, 3) > 0.008856)
                dou_Y = Math.Pow(dou_Y, 3);
            else
                dou_Y = (dou_Y - 16.0 / 116.0) / 7.787;

            if (Math.Pow(dou_X, 3) > 0.008856)
                dou_X = Math.Pow(dou_X, 3);
            else
                dou_X = (dou_X - 16.0 / 116.0) / 7.787;

            if (Math.Pow(dou_Z, 3) > 0.008856)
                dou_Z = Math.Pow(dou_Z, 3);
            else
                dou_Z = (dou_Z - 16.0 / 116.0) / 7.787;

            CSXYZ.X = ref_X * dou_X;     //ref_X =  95.047     Observer= 2°, Illuminant= D65
            CSXYZ.Y = ref_Y * dou_Y;     //ref_Y = 100.000
            CSXYZ.Z = ref_Z * dou_Z;     //ref_Z = 108.883

            return CSXYZ;
        }

        public Lab RGB2Lab(int R, int G, int B)
        {
            return XYZ2Lab(RGB2XYZ(R, G, B));
        }

        public Lab RGB2Lab(RGB RGB)
        {
            return XYZ2Lab(RGBtoXYZ(RGB));
        }

        public RGB Lab2RGB(double L, double a, double b)
        {
            return XYZ2RGB(LabtoXYZ(L, a, b));
        }

        public RGB LabtoRGB(Lab Lab)
        {
            return XYZ2RGB(LabtoXYZ(Lab));
        }
    }
}
