using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Network
{
    public static class ImageProcessor
    {
        public const int MAximumUDP_PacketSize = 60000;
        public const int PackageHeaderLength = 3;

        public static List<byte[]> GetMappedImage(Bitmap bitmap)
        {
            var imageCrops = CropImage(bitmap);
            var crops = GetCroppedImages(bitmap, imageCrops);
            return null;
        }

        public static List<byte[]> GetCipheredPackages(byte[] bytes, Random _random)
        {
            var payloadLength = MAximumUDP_PacketSize - PackageHeaderLength;
            int packagesCount = bytes.Length / payloadLength;
            var remnant = bytes.Length % payloadLength;
            packagesCount += remnant == 0 ? 0 : 1;
            List<byte[]> packages = new List<byte[]>(packagesCount + 2);
            byte[] guidBytes = new byte[2];
            _random.NextBytes(guidBytes);
            byte[] headerPackage = new byte[PackageHeaderLength];
            headerPackage[0] = (byte)packagesCount;
            Array.Copy(guidBytes, 0, headerPackage, 1, guidBytes.Length);
            packages.Add(headerPackage);
            for (int x = 0; x < packagesCount; x++)
            {
                var bytesToCopy = payloadLength;
                if (remnant != 0 && x == packagesCount - 1)
                {
                    bytesToCopy = remnant;
                }
                byte[] package = new byte[bytesToCopy + PackageHeaderLength];
                Array.Copy(headerPackage, 0, package, 0, PackageHeaderLength);
                package[0] = (byte)x;
                Array.Copy(bytes, x * payloadLength, package, PackageHeaderLength, bytesToCopy);
                packages.Add(package);
            }
            byte[] tailPackage = new byte[PackageHeaderLength];
            Array.Copy(headerPackage, 0, tailPackage, 0, PackageHeaderLength);
            tailPackage[0] = 0;
            packages.Add(tailPackage);
            return packages;
        }
        private static List<Bitmap> GetCroppedImages(Bitmap bitmap, IEnumerable<Rectangle> rectangles)
        {
            List<Bitmap> bitmaps = new List<Bitmap>(rectangles.Count());
            foreach(var rect in rectangles)
            {
                var bmp = bitmap.Clone(rect, bitmap.PixelFormat);
                bitmaps.Add(bmp);
            }
            return bitmaps;
        }
        private static List<Rectangle> CropImage(Bitmap bitmap)
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            var height = bitmap.Height;
            var width = bitmap.Width;

            Rectangle singleRectangle;

            if(height % width == 0)
            {
                int dimension = width > height ? (width / height) : (height / width);
                singleRectangle = new Rectangle(0, 0, dimension, dimension);

                for (int x = 0; x < width; x += dimension)
                {
                    for (int y = 0; y < height; y += dimension)
                    {
                        rectangles.Add(new Rectangle(x, y, dimension, dimension));
                    }
                }
            }
            else
            {
                if (width % 10 != 0 || height % 10 != 0) throw new ArgumentException("These dimensions are not supported yet...");

                int rectWidth = width / 10;
                int rectHeight = height / 10;

                for(int x = 0; x <  width; x+= rectWidth)
                {
                    for(int y = 0; y < height; y += rectHeight)
                    {
                        rectangles.Add(new Rectangle(x, y, rectWidth, rectHeight));
                    }
                }
            }

            return rectangles;
        }
    }
}
