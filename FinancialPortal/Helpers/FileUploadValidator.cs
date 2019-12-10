using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace FinancialPortal.Helpers
{
    public class FileUploadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return false;
            }
            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
            {
                return false;
            }
            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                           ImageFormat.Png.Equals(img.RawFormat) ||
                           ImageFormat.Tiff.Equals(img.RawFormat) ||
                           ImageFormat.Bmp.Equals(img.RawFormat) ||
                           ImageFormat.Gif.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool IsWebFriendlyFile(HttpPostedFileBase file)
        {
            var maxSize = WebConfigurationManager.AppSettings["MaxFileSize"];
            var minSize = WebConfigurationManager.AppSettings["MinFileSize"];

            if (file == null)
            {
                return false;
            }
            if (file.ContentLength > Convert.ToInt32(maxSize) || file.ContentLength < Convert.ToInt32(minSize))
            {
                return false;
            }
            try
            {
                var allowedExt = WebConfigurationManager.AppSettings["AllowedExt"];
                var fileExt = Path.GetExtension(file.FileName);

                return allowedExt.Contains(fileExt);
            }
            catch
            {
                return false;
            }
        }

        public static string GetIcon(string file)
        {
            var low = file.ToLower();
            var keyValue = WebConfigurationManager.AppSettings[Path.GetExtension(file)];
            var defaultValue = WebConfigurationManager.AppSettings["DefaultIcon"];
            if (Path.GetExtension(low) == ".jpg" || Path.GetExtension(low) == ".png" || Path.GetExtension(low) == ".tiff" || Path.GetExtension(low) == ".bmp" || Path.GetExtension(low) == ".gif")
            {
                return file;
            }
            else
            {
                return string.IsNullOrEmpty(keyValue) ? defaultValue : keyValue;
            }
        }

    }
}