using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode;
using ZxingForQRcodeHiding.QrCode.Internal;
using System.Windows.Forms;

namespace QRPhotoMosaic.Method
{
    class QRCodeInfo
    {
        public int QRSize { get; set; }
        public BitMatrix QRmatrix { get; set; }
        public int QRModule_Num { get; set; }
        public int moduleLength { get; set; }
        public int QRVersion { get; set; }
        public int AlignmentPatternLocation_X { get; set; }
        public int AlignmentPatternLocation_Y { get; set; }
        int[] AlignmentPattern_num = { 0, 1, 6, 13, 22, 33, 46 };

        public struct QRcodeCoordinate
        {
            public int Finder_X_Begin;
            public int Finder_X_End;
            public int Finder_Y_Begin;
            public int FinderLength;
            public int ModuleLength;

            public QRcodeCoordinate(int a)
            {
                Finder_X_Begin = 0;
                Finder_X_End = 0;
                Finder_Y_Begin = 0;
                FinderLength = 0;
                ModuleLength = 0;
            }
        }

        private QRcodeCoordinate CalculateModuleSize(BitMatrix QRmatrix) //計算每個module由多少pixel組成
        {
            int width = QRmatrix.Width;
            int height = QRmatrix.Height;
            QRcodeCoordinate QRCoor = new QRcodeCoordinate(0);
            Boolean flagBlack = true, flagWhite = true;

            for (int y = 0; y < height; y++)  // find module size
            {
                for (int x = 0; x < width; x++)
                {
                    if (QRmatrix[x, y] && flagBlack)
                    {
                        QRCoor.Finder_X_Begin = x;
                        QRCoor.Finder_Y_Begin = y;
                        flagBlack = false;
                    }
                    else if (!QRmatrix[x, y] && !flagBlack && flagWhite)
                    {
                        QRCoor.Finder_X_End = x;
                        flagWhite = false;
                    }
                    else if (!flagWhite && !flagBlack)
                        break;
                }//for
                if (!flagWhite && !flagBlack)
                    break;
            }//for

            QRCoor.FinderLength = QRCoor.Finder_X_End - QRCoor.Finder_X_Begin;//計算左上角finderpattern有多長
            QRCoor.ModuleLength = QRCoor.FinderLength / 7; //finderpattern由7個module組成

            return QRCoor;
        }

        private int CalculateQRcodeModule(int QRVersion) //計算QRcode有多少module組成
        {
            return 17 + QRVersion * 4;
        }

        private BitMatrix cutQRcode(BitMatrix matrix, int Finder_X_Begin, int Finder_Y_Begin, int QRModule_Num, int moduleLength)
        {
            int cutQRcodeSize = moduleLength * QRModule_Num;
            BitMatrix cutQRCode = new BitMatrix(cutQRcodeSize, cutQRcodeSize);
            for (int DrawMap_Y_Begin = Finder_Y_Begin, y = 0; DrawMap_Y_Begin < Finder_Y_Begin + moduleLength * QRModule_Num; DrawMap_Y_Begin++, y++)
            {
                for (int DrawMap_X_Begin = Finder_X_Begin, x = 0; DrawMap_X_Begin < Finder_X_Begin + moduleLength * QRModule_Num; DrawMap_X_Begin++, x++)
                {
                    cutQRCode[x, y] = matrix[DrawMap_X_Begin, DrawMap_Y_Begin];
                }
            }
            return cutQRCode;
        }

        private BitMatrix ScaleModuleLengthTo1(BitMatrix matrix, int moduleLength, int QRModule_Num)
        {
            BitMatrix ScaleQRCode = new BitMatrix(QRModule_Num, QRModule_Num);
            for (int y = 0; y < matrix.Height; y++)
            {
                for (int x = 0; x < matrix.Width; x++)
                {
                    if (x % moduleLength == 0 && y % moduleLength == 0)
                    {
                        if (matrix[x, y]) //黑為1 白為0
                        {
                            ScaleQRCode[x / moduleLength, y / moduleLength] = true;
                        }
                        else
                        {
                            ScaleQRCode[x / moduleLength, y / moduleLength] = false;
                        }
                    }
                }//for
            }//for
            return ScaleQRCode;
        }

        private void findAlignmentPatternLocations(int QRVersion, int moduleLength)
        {

            if (QRVersion == 2)
            {
                AlignmentPatternLocation_X = 16 * moduleLength;
                AlignmentPatternLocation_Y = 16 * moduleLength;
            }
            else if (QRVersion == 3)
            {
                AlignmentPatternLocation_X = 20 * moduleLength;
                AlignmentPatternLocation_Y = 20 * moduleLength;
            }
            else if (QRVersion == 4)
            {
                AlignmentPatternLocation_X = 24 * moduleLength;
                AlignmentPatternLocation_Y = 24 * moduleLength;
            }
            else if (QRVersion == 5)
            {
                AlignmentPatternLocation_X = 28 * moduleLength;
                AlignmentPatternLocation_Y = 28 * moduleLength;
            }
            else if (QRVersion == 6)
            {
                AlignmentPatternLocation_X = 32 * moduleLength;
                AlignmentPatternLocation_Y = 32 * moduleLength;
            }
            else if (QRVersion == 7)
            {
                AlignmentPatternLocation_X = 32 * moduleLength;
                AlignmentPatternLocation_Y = 32 * moduleLength;
            }
        }

        public void GetQRCodeInfo(BitMatrix QRmatrix, int QRVersion)
        {
            this.QRVersion = QRVersion;
            QRcodeCoordinate QRCoor = new QRcodeCoordinate(0);
            QRModule_Num = CalculateQRcodeModule(QRVersion);
            QRCoor = CalculateModuleSize(QRmatrix);
            moduleLength = QRCoor.ModuleLength;
            QRmatrix = cutQRcode(QRmatrix, QRCoor.Finder_X_Begin, QRCoor.Finder_Y_Begin, QRModule_Num, moduleLength);
            this.QRmatrix = ScaleModuleLengthTo1(QRmatrix, moduleLength, QRModule_Num);
            QRSize = 1 * QRModule_Num;
            moduleLength = 1;
            findAlignmentPatternLocations(QRVersion, moduleLength);
        }



    }
}
