using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Flann;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.GPU;

namespace QRPhotoMosaic.Method
{
    public class IndecesMapping
    {
        public int IndexStart { get; set; }
        public int IndexEnd { get; set; }
        public int Similarity { get; set; }
        public string fileName { get; set; }
    }

    public static class FLANN
    {

        public static Matrix<float> ComputeSingleDescriptors(Bitmap block)
        {
            Matrix<float> descs;
            SURFDetector detector = new SURFDetector(500, true);

            using (Image<Gray, Byte> img = new Image<Gray, byte>(block))
            {
                VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(img, null);
                descs = detector.ComputeDescriptorsRaw(img, null, keyPoints);
            }

            return descs;
        }

        public static Matrix<float> ComputeSingleDescriptors(string fileName)
        {
            Matrix<float> descs;
            SURFDetector detector = new SURFDetector(500, true);

            using (Image<Gray, Byte> img = new Image<Gray, byte>(fileName))
            {
                VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(img, null);
                descs = detector.ComputeDescriptorsRaw(img, null, keyPoints);
            }

            
            return descs;
        }

        public static IList<Matrix<float>> ComputeMultipleDescriptors(List<string> fileNames, out IList<IndecesMapping> imap)
        {
            imap = new List<IndecesMapping>();

            IList<Matrix<float>> descs = new List<Matrix<float>>();

            int r = 0;

            for (int i = 0; i < fileNames.Count; i++)
            {
                var desc = ComputeSingleDescriptors(fileNames[i]);
                if (desc != null)
                {
                    descs.Add(desc);

                    imap.Add(new IndecesMapping()
                    {
                        fileName = fileNames[i],
                        IndexStart = r,
                        IndexEnd = r + desc.Rows - 1
                    });r += desc.Rows;
                }
            }

            return descs;
        }

        public static Matrix<float> ConcatDescriptors(IList<Matrix<float>> descriptors)
        {
            int cols = descriptors[0].Cols;
            int rows = descriptors.Sum(a => a.Rows);

            float[,] concatedDescs = new float[rows, cols];

            int offset = 0;

            foreach (var descriptor in descriptors)
            {
                // append new descriptors
                Buffer.BlockCopy(descriptor.ManagedArray, 0, concatedDescs, offset, sizeof(float) * descriptor.ManagedArray.Length);
                offset += sizeof(float) * descriptor.ManagedArray.Length;
            }

            return new Matrix<float>(concatedDescs);
        }

        public static void FindMatches(Matrix<float> dbDescriptors, Matrix<float> queryDescriptors, ref IList<IndecesMapping> imap)
        {
            var indices = new Matrix<int>(queryDescriptors.Rows, 2); // matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(queryDescriptors.Rows, 2); // matrix that will contain distances to the 2-nearest neighbors found

            // create FLANN index with 4 kd-trees and perform KNN search over it look for 2 nearest neighbours
            var flannIndex = new Index(dbDescriptors, 4);
            flannIndex.KnnSearch(queryDescriptors, indices, dists, 2, 24);

            for (int i = 0; i < indices.Rows; i++)
            {
                // filter out all inadequate pairs based on distance between pairs
                if (dists.Data[i, 0] < (0.6 * dists.Data[i, 1]))
                {
                    // find image from the db to which current descriptor range belongs and increment similarity value.
                    // in the actual implementation this should be done differently as it's not very efficient for large image collections.
                    foreach (var img in imap)
                    {
                        if (img.IndexStart <= i && img.IndexEnd >= i)
                        {
                            img.Similarity++;
                            break;
                        }
                    }
                }
            }
        }

        public static IList<IndecesMapping> Match(List<Tile> tiles, Bitmap block)
        {
            //string[] dbImages = { "1.jpg", "2.jpg", "3.jpg" };
            List<string> dbImages = new List<string>();
            for (int i = 0; i < tiles.Count; ++i)
            {
                dbImages.Add(tiles[i].Name);
            }
            //string queryImage = "query.jpg";

            IList<IndecesMapping> imap;

            // compute descriptors for each image
            var dbDescsList = ComputeMultipleDescriptors(dbImages, out imap);

            // concatenate all DB images descriptors into single Matrix
            Matrix<float> dbDescs = ConcatDescriptors(dbDescsList);

            // compute descriptors for the query image
            //Matrix<float> queryDescriptors = ComputeSingleDescriptors(queryImage);
            Matrix<float> queryDescriptors = ComputeSingleDescriptors(block);

            FindMatches(dbDescs, queryDescriptors, ref imap);

            return imap;
        }

    }
}
