using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;

namespace AreaMapper
{
    class Program
    {

        abstract class Shape
        {
            public abstract void DrawShapeOnCanvas(Graphics g);
            public abstract void Scale(int x, int y);
           
        }

        class Box : Shape
        {
            public int X1 { get; set; }
            public int X2 { get; set; }
            public int Y1 { get; set; }
            public int Y2 { get; set; }
            public override void DrawShapeOnCanvas(Graphics g)
            {
                Pen p = new Pen(Color.Red);
                g.DrawRectangle(p, new Rectangle(X1, Y2, X2 - X1, Y1 - Y2));
            }

            public override void Scale(int x, int y)
            {
                //Remove the negatives
                X1 = X1 + x;
                X2 = X2 + x;
                Y1 = Y1 + y;                
                Y2 = Y2 + y;              
            }
        }

        class Sphere : Shape
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Radius { get; set; }

            public override void DrawShapeOnCanvas(Graphics g)
            {
                Pen p = new Pen(Color.Blue);
                p.Width = 2;
                g.DrawEllipse(p, X, Y, Radius, Radius);
            }

            public override void Scale(int x, int y)
            {
                X = X + x;
                Y = Y + y;                
            }
        }

        class Dome : Sphere
        {
            public override void DrawShapeOnCanvas(Graphics g)
            {
                Pen p = new Pen(Color.Green);
                p.Width = 2;
                g.DrawEllipse(p, X, Y, 1, 1);
            }
           
        }

        static void Main(string[] args)
        {

            string Filename = args[0];


            int largestX = int.MinValue;
            int largestY = int.MinValue;
            List<Shape> shapes = new List<Shape>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using(TextReader reader = new StreamReader(Filename))
            {

                XmlReader xmlreader = XmlReader.Create(reader, settings);

                while (xmlreader.Read())
                {
                    if(xmlreader.Name == "AreaDefinition" && xmlreader.NodeType != XmlNodeType.EndElement)
                    {
                        string shape = xmlreader.GetAttribute("shape");
                        if (shape == "sphere")
                        {
                            Sphere sphere = new Sphere()
                            {
                                X = (int)float.Parse(xmlreader.GetAttribute("z1")),
                                Y = -(int)float.Parse(xmlreader.GetAttribute("x1")),
                                Radius = (int)float.Parse(xmlreader.GetAttribute("radius")),
                            };
                            shapes.Add(sphere);
                            if (largestX < sphere.X + sphere.Radius) largestX = sphere.X + sphere.Radius;
                            if (largestY < Math.Abs(sphere.Y) + sphere.Radius) largestY = Math.Abs(sphere.Y) + sphere.Radius;
                        }
                        else if (shape == "box")
                        {
                            Box b = new Box()
                            {
                                X1 = (int)float.Parse(xmlreader.GetAttribute("z1")),
                                Y1 = -(int)float.Parse(xmlreader.GetAttribute("x1")),
                                X2 = (int)float.Parse(xmlreader.GetAttribute("z2")),
                                Y2 = -(int)float.Parse(xmlreader.GetAttribute("x2")),
                            };
                            shapes.Add(b);
                            if (largestX < b.X2) largestX = b.X2;
                            if (largestY < Math.Abs(b.Y2)) largestY = Math.Abs(b.Y2);
                        }
                        else if (shape == "dome")
                        {
                            Dome sphere = new Dome()
                            {
                                X = (int)float.Parse(xmlreader.GetAttribute("z1")),
                                Y = -(int)float.Parse(xmlreader.GetAttribute("x1")),                                
                            };
                            shapes.Add(sphere);
                            if (largestX < sphere.X + 1) largestX = sphere.X + 1;
                            if (largestY < Math.Abs(sphere.Y) + 1) largestY = Math.Abs(sphere.Y) + 1;
                        }
                        else
                        {
                            throw new Exception("No idea what " + shape + " Is");
                        }

                        
                    }
                }

                
            }


            largestX = Math.Abs(largestX);
            largestY = Math.Abs(largestY);

            Bitmap image = new Bitmap(largestX * 2, largestY * 2);
            Graphics g = Graphics.FromImage(image);
            SolidBrush brush = new SolidBrush(Color.Black);
            g.FillRectangle(brush, new Rectangle(0, 0, largestX * 2,  largestY * 2));

            foreach (Shape s in shapes)
            {
                s.Scale(largestX, largestY);
                s.DrawShapeOnCanvas(g);
            }

            string outputname = Filename.Replace(".xml", "") + "-Map.png";

            image.Save(outputname, System.Drawing.Imaging.ImageFormat.Png);
            
        }
    }
}
