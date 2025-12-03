using System.Drawing;
using System.Drawing.Drawing2D;

namespace YemenWhatsApp.Helpers
{
    public static class GraphicsExtensions
    {
        public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            return CreateRoundedRectangle(bounds, radius, radius, radius, radius);
        }

        public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int topLeft, int topRight, int bottomRight, int bottomLeft)
        {
            GraphicsPath path = new GraphicsPath();

            if (topLeft > 0)
                path.AddArc(bounds.X, bounds.Y, topLeft * 2, topLeft * 2, 180, 90);
            else
                path.AddLine(bounds.X, bounds.Y, bounds.X, bounds.Y);

            if (topRight > 0)
                path.AddArc(bounds.X + bounds.Width - topRight * 2, bounds.Y, topRight * 2, topRight * 2, 270, 90);
            else
                path.AddLine(bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y);

            if (bottomRight > 0)
                path.AddArc(bounds.X + bounds.Width - bottomRight * 2,
                           bounds.Y + bounds.Height - bottomRight * 2,
                           bottomRight * 2, bottomRight * 2, 0, 90);
            else
                path.AddLine(bounds.X + bounds.Width, bounds.Y + bounds.Height,
                           bounds.X + bounds.Width, bounds.Y + bounds.Height);

            if (bottomLeft > 0)
                path.AddArc(bounds.X, bounds.Y + bounds.Height - bottomLeft * 2,
                           bottomLeft * 2, bottomLeft * 2, 90, 90);
            else
                path.AddLine(bounds.X, bounds.Y + bounds.Height, bounds.X, bounds.Y + bounds.Height);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, float x, float y, float width, float height, float radius)
        {
            DrawRoundedRectangle(graphics, pen, new Rectangle((int)x, (int)y, (int)width, (int)height), (int)radius);
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
        {
            FillRoundedRectangle(graphics, brush, new Rectangle((int)x, (int)y, (int)width, (int)height), (int)radius);
        }

        public static void DrawCircle(this Graphics graphics, Pen pen, Point center, int radius)
        {
            graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);
        }

        public static void FillCircle(this Graphics graphics, Brush brush, Point center, int radius)
        {
            graphics.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
        }

        public static void DrawStringWithShadow(this Graphics graphics, string text, Font font, Brush textBrush, Brush shadowBrush, PointF location, int shadowOffset = 1)
        {
            // رسم الظل
            graphics.DrawString(text, font, shadowBrush,
                new PointF(location.X + shadowOffset, location.Y + shadowOffset));

            // رسم النص
            graphics.DrawString(text, font, textBrush, location);
        }

        public static void DrawStringWithShadow(this Graphics graphics, string text, Font font, Brush textBrush, Brush shadowBrush, RectangleF layoutRectangle, StringFormat format, int shadowOffset = 1)
        {
            // رسم الظل
            graphics.DrawString(text, font, shadowBrush,
                new RectangleF(layoutRectangle.X + shadowOffset, layoutRectangle.Y + shadowOffset,
                    layoutRectangle.Width, layoutRectangle.Height), format);

            // رسم النص
            graphics.DrawString(text, font, textBrush, layoutRectangle, format);
        }

        public static void DrawGradientRectangle(this Graphics graphics, Rectangle bounds, Color startColor, Color endColor, LinearGradientMode mode = LinearGradientMode.Vertical)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(bounds, startColor, endColor, mode))
            {
                graphics.FillRectangle(brush, bounds);
            }
        }

        public static void DrawGradientRoundedRectangle(this Graphics graphics, Rectangle bounds, Color startColor, Color endColor, int radius, LinearGradientMode mode = LinearGradientMode.Vertical)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            using (LinearGradientBrush brush = new LinearGradientBrush(bounds, startColor, endColor, mode))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static void DrawMessageBubble(this Graphics graphics, Rectangle bounds, Color bubbleColor, bool isMe = false, int radius = 15)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            using (SolidBrush brush = new SolidBrush(bubbleColor))
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);

                // إضافة ذيل للفقاعة
                if (isMe)
                {
                    // ذيل على اليمين
                    Point[] tail = new Point[]
                    {
                        new Point(bounds.Right - 5, bounds.Bottom - 10),
                        new Point(bounds.Right + 5, bounds.Bottom),
                        new Point(bounds.Right - 5, bounds.Bottom + 10)
                    };

                    graphics.FillPolygon(brush, tail);
                    graphics.DrawPolygon(pen, tail);
                }
                else
                {
                    // ذيل على اليسار
                    Point[] tail = new Point[]
                    {
                        new Point(bounds.Left + 5, bounds.Bottom - 10),
                        new Point(bounds.Left - 5, bounds.Bottom),
                        new Point(bounds.Left + 5, bounds.Bottom + 10)
                    };

                    graphics.FillPolygon(brush, tail);
                    graphics.DrawPolygon(pen, tail);
                }
            }
        }

        public static Bitmap CreateUserAvatar(string text, Color backgroundColor, Size size)
        {
            Bitmap bitmap = new Bitmap(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // رسم خلفية دائرية
                using (SolidBrush brush = new SolidBrush(backgroundColor))
                {
                    g.FillEllipse(brush, 0, 0, size.Width, size.Height);
                }

                // رسم النص
                if (!string.IsNullOrEmpty(text))
                {
                    using (Font font = new Font("Arial", size.Height / 3, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        string displayText = text.Length > 2 ? text.Substring(0, 2).ToUpper() : text.ToUpper();
                        g.DrawString(displayText, font, textBrush,
                            new RectangleF(0, 0, size.Width, size.Height), format);
                    }
                }
            }

            return bitmap;
        }
    }
}