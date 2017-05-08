using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PassKeep.Views.Converters {
    class BitmapConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(!(value is Bitmap)) {
                throw new ArgumentException();
            }

            Bitmap canvas = new Bitmap(23, 23);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            //補間方法として高品質双三次補間を指定する
            g.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            //画像を縮小して描画する
            g.DrawImage(value as Bitmap, 0, 0, canvas.Width, canvas.Height);

            return CreateBitmapSourceFromBitmap(canvas);
        }

        private BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap) {
            using (var memoryStream = new MemoryStream()) {
                // You need to specify the image format to fill the stream. 
                // I'm assuming it is PNG
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var bitmapDecoder = BitmapDecoder.Create(
                memoryStream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

                // This will disconnect the stream from the image completely...
                var writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
                writable.Freeze();

                return writable;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
