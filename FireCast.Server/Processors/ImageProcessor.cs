using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FireCast.Server.Processors
{
    public static class ImageProcessor
    {
        public const int MAximumUDP_PacketSize = 60000;
        public const int PackageHeaderLength = 3;
        public const int AdvancedPackageHeaderLength = 11;

        public static List<byte[]> GetMappedImage(Bitmap bitmap, Random _random, List<Rectangle> imageCrops)
        {
            var crops = GetCroppedImages(bitmap, imageCrops);
            var bytes = GetFramesBytes(crops, _random);
            return bytes;
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
        private static List<byte[]> GetCipheredCroppedPackages(byte[] bytes, int width, int height, Random _random)
        {
            List<byte[]> packages = new List<byte[]>();

            var payloadLength = MAximumUDP_PacketSize - AdvancedPackageHeaderLength;

            int packagesCount = bytes.Length / payloadLength;

            var remnant = bytes.Length % payloadLength;
            packagesCount += remnant == 0 ? 0 : 1;
            byte[] guidBytes = new byte[2];
            _random.NextBytes(guidBytes);
            byte[] headerPackage = new byte[AdvancedPackageHeaderLength];
            headerPackage[0] = (byte)packagesCount;
            Array.Copy(guidBytes, 0, headerPackage, 1, guidBytes.Length);
            var widthBytes = BitConverter.GetBytes(width);
            var heightBytes = BitConverter.GetBytes(height);

            Array.Copy(widthBytes, 0, headerPackage, 1 + guidBytes.Length, widthBytes.Length);
            Array.Copy(heightBytes, 0, headerPackage, 1 + guidBytes.Length + widthBytes.Length, heightBytes.Length);

            packages.Add(headerPackage);
            for (int x = 0; x < packagesCount; x++)
            {
                var bytesToCopy = payloadLength;
                if (remnant != 0 && x == packagesCount - 1)
                {
                    bytesToCopy = remnant;
                }
                byte[] package = new byte[bytesToCopy + AdvancedPackageHeaderLength];
                Array.Copy(headerPackage, 0, package, 0, AdvancedPackageHeaderLength);
                package[0] = (byte)x;
                Array.Copy(bytes, x * payloadLength, package, AdvancedPackageHeaderLength, bytesToCopy);
                packages.Add(package);
            }
            byte[] tailPackage = new byte[AdvancedPackageHeaderLength];
            Array.Copy(headerPackage, 0, tailPackage, 0, AdvancedPackageHeaderLength);
            tailPackage[0] = 0;
            packages.Add(tailPackage);
            return packages;
        }
        private static List<byte[]> GetFramesBytes(List<BitmapFrame> frames, Random _random)
        {
            List<byte[]> bytesList = new List<byte[]>();

            foreach (var frame in frames)
            {
                //byte[] frameBytes;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    frame.Bitmap.Save(ms, ImageFormat.Jpeg);
                //    frameBytes = ms.ToArray();
                //}
                var rawbytesCount = frame.Bitmap.Width * frame.Bitmap.Height * (Image.GetPixelFormatSize(frame.Bitmap.PixelFormat) / 8);
                byte[] frameBytes = new byte[rawbytesCount];
                BitmapData bitmapData = frame.Bitmap.LockBits(new Rectangle(0, 0, frame.Bitmap.Width, frame.Bitmap.Height), ImageLockMode.ReadOnly, frame.Bitmap.PixelFormat);
                Marshal.Copy(bitmapData.Scan0, frameBytes, 0, rawbytesCount);
                var bytes = GetCipheredCroppedPackages(frameBytes, frame.Width, frame.Height, _random);
                bytesList.AddRange(bytes);
            }
            return bytesList;
        }
        private static List<BitmapFrame> GetCroppedImages(Bitmap bitmap, IEnumerable<Rectangle> rectangles)
        {
            List<BitmapFrame> bitmaps = new List<BitmapFrame>();
            foreach(var rect in rectangles)
            {
                var bmp = bitmap.Clone(rect, bitmap.PixelFormat);
                bitmaps.Add(new BitmapFrame()
                {
                    Bitmap = bmp,
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height
                });
            }
            return bitmaps;
        }
        public static List<Rectangle> CropImage(int width, int height)
        {
            List<Rectangle> rectangles = new List<Rectangle>();

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
