using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Drawing_Fractals
{
    public partial class Form1 : Form
    {
        public int countLayers;

        private readonly bool centerIsX = Convert.ToBoolean(ConfigurationManager.AppSettings["center-as-x"]);
        private readonly int centerMultiplier = Convert.ToInt32(ConfigurationManager.AppSettings["center-multiplier"]);
        private readonly int colorGroupingAmount = Convert.ToInt32(ConfigurationManager.AppSettings["color-grouping-amount"]);
        private readonly bool killOnEdge = Convert.ToBoolean(ConfigurationManager.AppSettings["kill-on-edge"]);
        private readonly int layerLimit = Convert.ToInt32(ConfigurationManager.AppSettings["layer-limit"]);
        private readonly int length = Convert.ToInt32(ConfigurationManager.AppSettings["line-length"]);
        private readonly string pathing = ConfigurationManager.AppSettings["pathing"];
        private readonly float width = Convert.ToSingle(ConfigurationManager.AppSettings["line-width"]);
        private readonly int layerGenerationRate = Convert.ToInt32(ConfigurationManager.AppSettings["layer-generation-rate"]);
        private readonly int straightMultiplier = Convert.ToInt32(ConfigurationManager.AppSettings["straight-multiplier"]);
        private readonly int diagonalMultiplier = Convert.ToInt32(ConfigurationManager.AppSettings["diagonal-multiplier"]);
        private readonly string color = ConfigurationManager.AppSettings["color"];
        private readonly bool generateInBackground = Convert.ToBoolean(ConfigurationManager.AppSettings["generate-in-background"]);

        private System.Drawing.Bitmap TheBitmap;

        private int colorIndex;

        // Cache font instead of recreating font objects each time we paint.
        private Font fnt = new Font("Arial", 10);

        private int garbageCollectionSize = 8;
        private List<ChainedPoint> listAllPoints = new List<ChainedPoint>();
        private DoubleBufferPanel panel1 = new DoubleBufferPanel();
        private PictureBox pictureBox1 = new PictureBox();
        //private Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        public enum Directions { North, Northeast, East, Southeast, South, Southwest, West, Northwest, Center }

        private ChainedPoint CheckForDiagonalIntersection(ChainedPoint objPoint, List<ChainedPoint> listDiagonalPoints, int length)
        {
            //limit list to only closish points
            //listDiagonalPoints = listDiagonalPoints.Where(p => Math.Sqrt(Math.Pow(p.X - objPoint.X, 2) + Math.Pow(p.Y - objPoint.Y, 2)) < 2 * length).ToList();
            //foreach (ChainedPoint objDiagonalPoint in listDiagonalPoints)
            for (int i = listDiagonalPoints.Count - 1; i >= 0; i--)
            {
                ChainedPoint objDiagonalPoint = listDiagonalPoints[i];
                if (objPoint.FromDirection == (int)Directions.Northeast)
                {
                    if ((objDiagonalPoint.FromDirection == (int)Directions.Southeast && (objDiagonalPoint.Y > objPoint.Y) && Math.Abs(Math.Abs(objPoint.Y - objDiagonalPoint.Y) - length) <= 1 && Math.Abs(objPoint.X - objDiagonalPoint.X) <= 1)
                        || (objDiagonalPoint.FromDirection == (int)Directions.Northwest && (objDiagonalPoint.X < objPoint.X) && Math.Abs(Math.Abs(objPoint.X - objDiagonalPoint.X) - length) <= 1 && Math.Abs(objPoint.Y - objDiagonalPoint.Y) <= 1))
                    {
                        objPoint.X -= ((length * diagonalMultiplier) / 2);
                        objPoint.Y += ((length * diagonalMultiplier) / 2);
                        objPoint.Alive = false;
                        break;
                    }
                }
                if (objPoint.FromDirection == (int)Directions.Southeast)
                {
                    if ((objDiagonalPoint.FromDirection == (int)Directions.Northeast && (objDiagonalPoint.Y < objPoint.Y) && Math.Abs(Math.Abs(objPoint.Y - objDiagonalPoint.Y) - length) <= 1 && Math.Abs(objPoint.X - objDiagonalPoint.X) <= 1)
                        || (objDiagonalPoint.FromDirection == (int)Directions.Southwest && (objDiagonalPoint.X < objPoint.X) && Math.Abs(Math.Abs(objPoint.X - objDiagonalPoint.X) - length) <= 1 && Math.Abs(objPoint.Y - objDiagonalPoint.Y) <= 1))
                    {
                        objPoint.X -= ((length * diagonalMultiplier) / 2);
                        objPoint.Y -= ((length * diagonalMultiplier) / 2);
                        objPoint.Alive = false;
                        break;
                    }
                }
                if (objPoint.FromDirection == (int)Directions.Southwest)
                {
                    if ((objDiagonalPoint.FromDirection == (int)Directions.Northwest && (objDiagonalPoint.Y < objPoint.Y) && Math.Abs(Math.Abs(objPoint.Y - objDiagonalPoint.Y) - length) <= 1 && Math.Abs(objPoint.X - objDiagonalPoint.X) <= 1)
                        || (objDiagonalPoint.FromDirection == (int)Directions.Southeast && (objDiagonalPoint.X > objPoint.X) && Math.Abs(Math.Abs(objPoint.X - objDiagonalPoint.X) - length) <= 1 && Math.Abs(objPoint.Y - objDiagonalPoint.Y) <= 1))
                    {
                        objPoint.X += ((length * diagonalMultiplier) / 2);
                        objPoint.Y -= ((length * diagonalMultiplier) / 2);
                        objPoint.Alive = false;
                        break;
                    }
                }
                if (objPoint.FromDirection == (int)Directions.Northwest)
                {
                    if ((objDiagonalPoint.FromDirection == (int)Directions.Southwest && (objDiagonalPoint.Y > objPoint.Y) && Math.Abs(Math.Abs(objPoint.Y - objDiagonalPoint.Y) - length) <= 1 && Math.Abs(objPoint.X - objDiagonalPoint.X) <= 1)
                        || (objDiagonalPoint.FromDirection == (int)Directions.Northeast && (objDiagonalPoint.X > objPoint.X) && Math.Abs(Math.Abs(objPoint.X - objDiagonalPoint.X) - length) <= 1 && Math.Abs(objPoint.Y - objDiagonalPoint.Y) <= 1))
                    {
                        objPoint.X += ((length * diagonalMultiplier) / 2);
                        objPoint.Y += ((length * diagonalMultiplier) / 2);
                        objPoint.Alive = false;
                        break;
                    }
                }
            }

            return objPoint;
        }

        private void DrawFractal(Graphics g, BackgroundWorker worker, DoWorkEventArgs e)
        {
            // Create a local version of the graphics object for the PictureBox.
            //Graphics g = Graphics.FromImage(TheBitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            List<Color> listMaterialColors = new List<Color>();

            listMaterialColors = GetColorList();

            if (countLayers == 0)
            {
                ChainedPoint centerPoint = new ChainedPoint(panel1.Width / 2, panel1.Height / 2, (int)Directions.Center);
                //ChainedPoint centerPoint = new ChainedPoint(length, length, (int)Directions.Southeast);

                Pen originalPen = new Pen(listMaterialColors[0])
                {
                    Width = width
                };

                originalPen.Dispose();

                listAllPoints.Add(centerPoint);
            }

            if (countLayers > garbageCollectionSize * 2)
            {
                //listAllPoints.RemoveAll(p => p.LayerIndexAded < garbageCollectionSize);
                garbageCollectionSize *= 2;
            }

            List<ChainedPoint> listAlivePoints = listAllPoints.Where(p => p.Alive).ToList();

            //if (!listAlivePoints.Any(p => (p.X > 0 && p.X <= 1028 * 1.5) && (p.Y > 0 && p.Y <= 1028 * 1.5)))
            //{
            //    timer.Stop();
            //}
            Color selectedColor = listMaterialColors[colorIndex];

            if (countLayers % colorGroupingAmount == colorGroupingAmount - 1)
            {
                colorIndex++;
                if (colorIndex == listMaterialColors.Count)
                {
                    colorIndex = 0;
                }
            }
            int j = 0;
            foreach (ChainedPoint objChainedPoint in listAlivePoints)
            {
                ChainedPoint newPoint1 = null;
                ChainedPoint newPoint2 = null;
                ChainedPoint newPoint3 = null;
                ChainedPoint newPoint4 = null;
                bool boolTLayer = InterpretPathing(pathing, countLayers);
                switch (objChainedPoint.FromDirection)
                {
                    case (int)Directions.North:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * straightMultiplier), objChainedPoint.Y, (int)Directions.West);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * straightMultiplier), objChainedPoint.Y, (int)Directions.East);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northwest);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northeast);
                            }
                            break;
                        }
                    case (int)Directions.Northeast:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northwest);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southeast);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y - (length * straightMultiplier), (int)Directions.North);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * straightMultiplier), objChainedPoint.Y, (int)Directions.East);
                            }
                            break;
                        }
                    case (int)Directions.East:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y - (length * straightMultiplier), (int)Directions.North);
                                newPoint2 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y + (length * straightMultiplier), (int)Directions.South);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northeast);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southeast);
                            }
                            break;
                        }
                    case (int)Directions.Southeast:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northeast);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southwest);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * straightMultiplier), objChainedPoint.Y, (int)Directions.East);
                                newPoint2 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y + (length * straightMultiplier), (int)Directions.South);
                            }
                            break;
                        }
                    case (int)Directions.South:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * straightMultiplier), objChainedPoint.Y, (int)Directions.East);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * straightMultiplier), objChainedPoint.Y, (int)Directions.West);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southeast);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southwest);
                            }
                            break;
                        }
                    case (int)Directions.Southwest:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southeast);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northwest);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y + (length * straightMultiplier), (int)Directions.South);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * straightMultiplier), objChainedPoint.Y, (int)Directions.West);
                            }
                            break;
                        }
                    case (int)Directions.West:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y + (length * straightMultiplier), (int)Directions.South);
                                newPoint2 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y - (length * straightMultiplier), (int)Directions.North);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southwest);
                                newPoint2 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northwest);
                            }
                            break;
                        }
                    case (int)Directions.Northwest:
                        {
                            if (boolTLayer)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * diagonalMultiplier), objChainedPoint.Y + (length * diagonalMultiplier), (int)Directions.Southwest);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * diagonalMultiplier), objChainedPoint.Y - (length * diagonalMultiplier), (int)Directions.Northeast);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X - (length * straightMultiplier), objChainedPoint.Y, (int)Directions.West);
                                newPoint2 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y - (length * straightMultiplier), (int)Directions.North);
                            }
                            break;
                        }
                    case (int)Directions.Center:
                        {
                            if (centerIsX)
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X + (length * centerMultiplier), objChainedPoint.Y - (length * centerMultiplier), (int)Directions.Northeast);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * centerMultiplier), objChainedPoint.Y + (length * centerMultiplier), (int)Directions.Southeast);
                                newPoint3 = new ChainedPoint(objChainedPoint.X - (length * centerMultiplier), objChainedPoint.Y + (length * centerMultiplier), (int)Directions.Southwest);
                                newPoint4 = new ChainedPoint(objChainedPoint.X - (length * centerMultiplier), objChainedPoint.Y - (length * centerMultiplier), (int)Directions.Northwest);
                            }
                            else
                            {
                                newPoint1 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y - (length * centerMultiplier), (int)Directions.North);
                                newPoint2 = new ChainedPoint(objChainedPoint.X + (length * centerMultiplier), objChainedPoint.Y, (int)Directions.East);
                                newPoint3 = new ChainedPoint(objChainedPoint.X, objChainedPoint.Y + (length * centerMultiplier), (int)Directions.South);
                                newPoint4 = new ChainedPoint(objChainedPoint.X - (length * centerMultiplier), objChainedPoint.Y, (int)Directions.West);
                            }
                            break;
                        }
                    default:
                        break;
                }

                Pen rainbowPen = new Pen(selectedColor)
                {
                    Width = width
                };

                List<ChainedPoint> listNewPoints = new List<ChainedPoint>
                {
                    newPoint1,
                    newPoint2 };

                if (newPoint3 != null)
                {
                    listNewPoints.Add(newPoint3);
                }
                if (newPoint4 != null)
                {
                    listNewPoints.Add(newPoint4);
                }
                for (int i = 0; i < listNewPoints.Count; i++)
                {
                    ChainedPoint newPoint = listNewPoints[i];
                    //kill on edge
                    if (killOnEdge)
                    {
                        if (newPoint.X <= (length * 3) || newPoint.X > (pictureBox1.Width - (length * 3) - 1) || (newPoint.Y <= (length * 3) || newPoint.Y > (pictureBox1.Height - (length * 3) - 1)))
                        {
                            continue;
                        }
                    }
                    if (newPoint.IsDiagonal())
                    {
                        newPoint = CheckForDiagonalIntersection(newPoint, listAllPoints.Where(p => p.IsDiagonal()).ToList(), length);
                    }
                    g.DrawLine(rainbowPen, objChainedPoint.ToPoint(), newPoint.ToPoint());

                    if (newPoint.Alive && listAllPoints.Contains(newPoint))
                    {
                        newPoint.Alive = false;
                        foreach (ChainedPoint objPointToKill in listAllPoints.Where(p => p.Equals(newPoint) && p.Alive).ToList())
                        {
                            objPointToKill.Alive = false;
                        }
                    }

                    newPoint.LayerIndexAded = countLayers;

                    listAllPoints.Add(newPoint);

                    if (j % 64 == 0)
                    {
                        worker.ReportProgress(0, "right");
                    }

                    j++;
                }

                rainbowPen.Dispose();

                objChainedPoint.Alive = false;
            }

            //if (layerLimit > 0 && layerLimit == countLayers + 1)
            //{
            //    //timer.Stop();
            //}
        }

        private List<Color> GetColorList()
        {
            List<Color> listMaterialColors;
            switch (color)
            {
                case "rainbow":
                    {
                        listMaterialColors = new List<Color>
                        {
                            //rainbow
                            ColorTranslator.FromHtml("#F44336"),
                            ColorTranslator.FromHtml("#FF5722"),
                            ColorTranslator.FromHtml("#FF9800"),
                            ColorTranslator.FromHtml("#FFC107"),
                            ColorTranslator.FromHtml("#FFEB3B"),
                            ColorTranslator.FromHtml("#CDDC39"),
                            ColorTranslator.FromHtml("#8BC34A"),
                            ColorTranslator.FromHtml("#4CAF50"),
                            ColorTranslator.FromHtml("#009688"),
                            ColorTranslator.FromHtml("#00BCD4"),
                            ColorTranslator.FromHtml("#03A9F4"),
                            ColorTranslator.FromHtml("#2196F3"),
                            ColorTranslator.FromHtml("#3F51B5"),
                            ColorTranslator.FromHtml("#673AB7"),
                            ColorTranslator.FromHtml("#9C27B0"),
                            ColorTranslator.FromHtml("#E91E63"),
                        };
                        break;
                    }
                case "alt-rainbow":
                    {
                        listMaterialColors = new List<Color>
                        {
                            //rainbow
                            ColorTranslator.FromHtml("#F44336"), //1
                            ColorTranslator.FromHtml("#009688"), //9
                            ColorTranslator.FromHtml("#FF5722"), //2
                            ColorTranslator.FromHtml("#00BCD4"), //10
                            ColorTranslator.FromHtml("#FF9800"), //3
                            ColorTranslator.FromHtml("#03A9F4"), //11
                            ColorTranslator.FromHtml("#FFC107"), //4
                            ColorTranslator.FromHtml("#2196F3"), //12
                            ColorTranslator.FromHtml("#FFEB3B"), //5
                            ColorTranslator.FromHtml("#3F51B5"), //13
                            ColorTranslator.FromHtml("#CDDC39"), //6
                            ColorTranslator.FromHtml("#673AB7"), //14
                            ColorTranslator.FromHtml("#8BC34A"), //7
                            ColorTranslator.FromHtml("#9C27B0"), //15
                            ColorTranslator.FromHtml("#4CAF50"), //8
                            ColorTranslator.FromHtml("#E91E63"), //16
                        };
                        break;
                    }
                case "dark-rainbow":
                    {
                        listMaterialColors = new List<Color>
                        {
                            //rainbow
                            ColorTranslator.FromHtml("#b71c1c"),
                            ColorTranslator.FromHtml("#BF360C"),
                            ColorTranslator.FromHtml("#E65100"),
                            ColorTranslator.FromHtml("#FF6F00"),
                            ColorTranslator.FromHtml("#F57F17"),
                            ColorTranslator.FromHtml("#827717"),
                            ColorTranslator.FromHtml("#33691E"),
                            ColorTranslator.FromHtml("#1B5E20"),
                            ColorTranslator.FromHtml("#004D40"),
                            ColorTranslator.FromHtml("#006064"),
                            ColorTranslator.FromHtml("#01579B"),
                            ColorTranslator.FromHtml("#0D47A1"),
                            ColorTranslator.FromHtml("#1A237E"),
                            ColorTranslator.FromHtml("#311B92"),
                            ColorTranslator.FromHtml("#4A148C"),
                            ColorTranslator.FromHtml("#880E4F")
                        };
                        break;
                    }
                case "brightness-rainbow":
                    {
                        listMaterialColors = new List<Color>
                        {
                            ColorTranslator.FromHtml("#ffcdd2"), //Red 100
                            ColorTranslator.FromHtml("#FFCCBC"), //Deep Orange 100
                            ColorTranslator.FromHtml("#FFE0B2"), //Orange 100
                            ColorTranslator.FromHtml("#FFECB3"), //Amber 100
                            ColorTranslator.FromHtml("#FFF9C4"), //Yellow 100
                            ColorTranslator.FromHtml("#F0F4C3"), //Lime 100
                            ColorTranslator.FromHtml("#DCEDC8"), //Light Green 100
                            ColorTranslator.FromHtml("#C8E6C9"), //Green 100
                            ColorTranslator.FromHtml("#B2DFDB"), //Teal 100
                            ColorTranslator.FromHtml("#B2EBF2"), //Cyan 100
                            ColorTranslator.FromHtml("#B3E5FC"), //Light Blue 100
                            ColorTranslator.FromHtml("#BBDEFB"), //Blue 100
                            ColorTranslator.FromHtml("#C5CAE9"), //Indigo 100
                            ColorTranslator.FromHtml("#D1C4E9"), //Deep Purple 100
                            ColorTranslator.FromHtml("#E1BEE7"), //Purple 100
                            ColorTranslator.FromHtml("#F8BBD0"), //Pink 100

                            ColorTranslator.FromHtml("#ef9a9a"), //Red 200
                            ColorTranslator.FromHtml("#FFAB91"), //Deep Orange 200
                            ColorTranslator.FromHtml("#FFCC80"), //Orange 200
                            ColorTranslator.FromHtml("#FFE082"), //Amber 200
                            ColorTranslator.FromHtml("#FFF59D"), //Yellow 200
                            ColorTranslator.FromHtml("#E6EE9C"), //Lime 200
                            ColorTranslator.FromHtml("#C5E1A5"), //Light Green 200
                            ColorTranslator.FromHtml("#A5D6A7"), //Green 200
                            ColorTranslator.FromHtml("#80CBC4"), //Teal 200
                            ColorTranslator.FromHtml("#80DEEA"), //Cyan 200
                            ColorTranslator.FromHtml("#81D4FA"), //Light Blue 200
                            ColorTranslator.FromHtml("#90CAF9"), //Blue 200
                            ColorTranslator.FromHtml("#9FA8DA"), //Indigo 200
                            ColorTranslator.FromHtml("#B39DDB"), //Deep Purple 200
                            ColorTranslator.FromHtml("#CE93D8"), //Purple 200
                            ColorTranslator.FromHtml("#F48FB1"), //Pink 200

                            ColorTranslator.FromHtml("#e57373"), //Red 300
                            ColorTranslator.FromHtml("#FF8A65"), //Deep Orange 300
                            ColorTranslator.FromHtml("#FFB74D"), //Orange 300
                            ColorTranslator.FromHtml("#FFD54F"), //Amber 300
                            ColorTranslator.FromHtml("#FFF176"), //Yellow 300
                            ColorTranslator.FromHtml("#DCE775"), //Lime 300
                            ColorTranslator.FromHtml("#AED581"), //Light Green 300
                            ColorTranslator.FromHtml("#81C784"), //Green 300
                            ColorTranslator.FromHtml("#4DB6AC"), //Teal 300
                            ColorTranslator.FromHtml("#4DD0E1"), //Cyan 300
                            ColorTranslator.FromHtml("#4FC3F7"), //Light Blue 300
                            ColorTranslator.FromHtml("#64B5F6"), //Blue 300
                            ColorTranslator.FromHtml("#7986CB"), //Indigo 300
                            ColorTranslator.FromHtml("#9575CD"), //Deep Purple 300
                            ColorTranslator.FromHtml("#BA68C8"), //Purple 300
                            ColorTranslator.FromHtml("#F06292"), //Pink 300

                            ColorTranslator.FromHtml("#ef5350"), //Red 400
                            ColorTranslator.FromHtml("#FF7043"), //Deep Orange 400
                            ColorTranslator.FromHtml("#FFA726"), //Orange 400
                            ColorTranslator.FromHtml("#FFCA28"), //Amber 400
                            ColorTranslator.FromHtml("#FFEE58"), //Yellow 400
                            ColorTranslator.FromHtml("#D4E157"), //Lime 400
                            ColorTranslator.FromHtml("#9CCC65"), //Light Green 400
                            ColorTranslator.FromHtml("#66BB6A"), //Green 400
                            ColorTranslator.FromHtml("#26A69A"), //Teal 400
                            ColorTranslator.FromHtml("#26C6DA"), //Cyan 400
                            ColorTranslator.FromHtml("#29B6F6"), //Light Blue 400
                            ColorTranslator.FromHtml("#42A5F5"), //Blue 400
                            ColorTranslator.FromHtml("#5C6BC0"), //Indigo 400
                            ColorTranslator.FromHtml("#7E57C2"), //Deep Purple 400
                            ColorTranslator.FromHtml("#AB47BC"), //Purple 400
                            ColorTranslator.FromHtml("#EC407A"), //Pink 400

                            ColorTranslator.FromHtml("#F44336"), //Red 500
                            ColorTranslator.FromHtml("#FF5722"), //Deep Orange 500
                            ColorTranslator.FromHtml("#FF9800"), //Orange 500
                            ColorTranslator.FromHtml("#FFC107"), //Amber 500
                            ColorTranslator.FromHtml("#FFEB3B"), //Yellow 500
                            ColorTranslator.FromHtml("#CDDC39"), //Lime 500
                            ColorTranslator.FromHtml("#8BC34A"), //Light Green 500
                            ColorTranslator.FromHtml("#4CAF50"), //Green 500
                            ColorTranslator.FromHtml("#009688"), //Teal 500
                            ColorTranslator.FromHtml("#00BCD4"), //Cyan 500
                            ColorTranslator.FromHtml("#03A9F4"), //Light Blue 500
                            ColorTranslator.FromHtml("#2196F3"), //Blue 500
                            ColorTranslator.FromHtml("#3F51B5"), //Indigo 500
                            ColorTranslator.FromHtml("#673AB7"), //Deep Purple 500
                            ColorTranslator.FromHtml("#9C27B0"), //Purple 500
                            ColorTranslator.FromHtml("#E91E63"), //Pink 500

                            ColorTranslator.FromHtml("#e53935"), //Red 600
                            ColorTranslator.FromHtml("#F4511E"), //Deep Orange 600
                            ColorTranslator.FromHtml("#FB8C00"), //Orange 600
                            ColorTranslator.FromHtml("#FFB300"), //Amber 600
                            ColorTranslator.FromHtml("#FDD835"), //Yellow 600
                            ColorTranslator.FromHtml("#C0CA33"), //Lime 600
                            ColorTranslator.FromHtml("#7CB342"), //Light Green 600
                            ColorTranslator.FromHtml("#43A047"), //Green 600
                            ColorTranslator.FromHtml("#00897B"), //Teal 600
                            ColorTranslator.FromHtml("#00ACC1"), //Cyan 600
                            ColorTranslator.FromHtml("#039BE5"), //Light Blue 600
                            ColorTranslator.FromHtml("#1E88E5"), //Blue 600
                            ColorTranslator.FromHtml("#3949AB"), //Indigo 600
                            ColorTranslator.FromHtml("#5E35B1"), //Deep Purple 600
                            ColorTranslator.FromHtml("#8E24AA"), //Purple 600
                            ColorTranslator.FromHtml("#D81B60"), //Pink 600

                            ColorTranslator.FromHtml("#d32f2f"), //Red 700
                            ColorTranslator.FromHtml("#E64A19"), //Deep Orange 700
                            ColorTranslator.FromHtml("#F57C00"), //Orange 700
                            ColorTranslator.FromHtml("#FFA000"), //Amber 700
                            ColorTranslator.FromHtml("#FBC02D"), //Yellow 700
                            ColorTranslator.FromHtml("#AFB42B"), //Lime 700
                            ColorTranslator.FromHtml("#689F38"), //Light Green 700
                            ColorTranslator.FromHtml("#388E3C"), //Green 700
                            ColorTranslator.FromHtml("#00796B"), //Teal 700
                            ColorTranslator.FromHtml("#0097A7"), //Cyan 700
                            ColorTranslator.FromHtml("#0288D1"), //Light Blue 700
                            ColorTranslator.FromHtml("#1976D2"), //Blue 700
                            ColorTranslator.FromHtml("#303F9F"), //Indigo 700
                            ColorTranslator.FromHtml("#512DA8"), //Deep Purple 700
                            ColorTranslator.FromHtml("#7B1FA2"), //Purple 700
                            ColorTranslator.FromHtml("#C2185B"), //Pink 700

                            ColorTranslator.FromHtml("#c62828"), //Red 800
                            ColorTranslator.FromHtml("#D84315"), //Deep Orange 800
                            ColorTranslator.FromHtml("#EF6C00"), //Orange 800
                            ColorTranslator.FromHtml("#FF8F00"), //Amber 800
                            ColorTranslator.FromHtml("#F9A825"), //Yellow 800
                            ColorTranslator.FromHtml("#9E9D24"), //Lime 800
                            ColorTranslator.FromHtml("#558B2F"), //Light Green 800
                            ColorTranslator.FromHtml("#2E7D32"), //Green 800
                            ColorTranslator.FromHtml("#00695C"), //Teal 800
                            ColorTranslator.FromHtml("#00838F"), //Cyan 800
                            ColorTranslator.FromHtml("#0277BD"), //Light Blue 800
                            ColorTranslator.FromHtml("#1565C0"), //Blue 800
                            ColorTranslator.FromHtml("#283593"), //Indigo 800
                            ColorTranslator.FromHtml("#4527A0"), //Deep Purple 800
                            ColorTranslator.FromHtml("#6A1B9A"), //Purple 800
                            ColorTranslator.FromHtml("#AD1457"), //Pink 800

                            ColorTranslator.FromHtml("#b71c1c"), //Red 900
                            ColorTranslator.FromHtml("#BF360C"), //Deep Orange 900
                            ColorTranslator.FromHtml("#E65100"), //Orange 900
                            ColorTranslator.FromHtml("#FF6F00"), //Amber 900
                            ColorTranslator.FromHtml("#F57F17"), //Yellow 900
                            ColorTranslator.FromHtml("#827717"), //Lime 900
                            ColorTranslator.FromHtml("#33691E"), //Light Green 900
                            ColorTranslator.FromHtml("#1B5E20"), //Green 900
                            ColorTranslator.FromHtml("#004D40"), //Teal 900
                            ColorTranslator.FromHtml("#006064"), //Cyan 900
                            ColorTranslator.FromHtml("#01579B"), //Light Blue 900
                            ColorTranslator.FromHtml("#0D47A1"), //Blue 900
                            ColorTranslator.FromHtml("#1A237E"), //Indigo 900
                            ColorTranslator.FromHtml("#311B92"), //Deep Purple 900
                            ColorTranslator.FromHtml("#4A148C"), //Purple 900
                            ColorTranslator.FromHtml("#880E4F"), //Pink 900

                            ColorTranslator.FromHtml("#c62828"), //Red 800
                            ColorTranslator.FromHtml("#D84315"), //Deep Orange 800
                            ColorTranslator.FromHtml("#EF6C00"), //Orange 800
                            ColorTranslator.FromHtml("#FF8F00"), //Amber 800
                            ColorTranslator.FromHtml("#F9A825"), //Yellow 800
                            ColorTranslator.FromHtml("#9E9D24"), //Lime 800
                            ColorTranslator.FromHtml("#558B2F"), //Light Green 800
                            ColorTranslator.FromHtml("#2E7D32"), //Green 800
                            ColorTranslator.FromHtml("#00695C"), //Teal 800
                            ColorTranslator.FromHtml("#00838F"), //Cyan 800
                            ColorTranslator.FromHtml("#0277BD"), //Light Blue 800
                            ColorTranslator.FromHtml("#1565C0"), //Blue 800
                            ColorTranslator.FromHtml("#283593"), //Indigo 800
                            ColorTranslator.FromHtml("#4527A0"), //Deep Purple 800
                            ColorTranslator.FromHtml("#6A1B9A"), //Purple 800
                            ColorTranslator.FromHtml("#AD1457"), //Pink 800

                            ColorTranslator.FromHtml("#d32f2f"), //Red 700
                            ColorTranslator.FromHtml("#E64A19"), //Deep Orange 700
                            ColorTranslator.FromHtml("#F57C00"), //Orange 700
                            ColorTranslator.FromHtml("#FFA000"), //Amber 700
                            ColorTranslator.FromHtml("#FBC02D"), //Yellow 700
                            ColorTranslator.FromHtml("#AFB42B"), //Lime 700
                            ColorTranslator.FromHtml("#689F38"), //Light Green 700
                            ColorTranslator.FromHtml("#388E3C"), //Green 700
                            ColorTranslator.FromHtml("#00796B"), //Teal 700
                            ColorTranslator.FromHtml("#0097A7"), //Cyan 700
                            ColorTranslator.FromHtml("#0288D1"), //Light Blue 700
                            ColorTranslator.FromHtml("#1976D2"), //Blue 700
                            ColorTranslator.FromHtml("#303F9F"), //Indigo 700
                            ColorTranslator.FromHtml("#512DA8"), //Deep Purple 700
                            ColorTranslator.FromHtml("#7B1FA2"), //Purple 700
                            ColorTranslator.FromHtml("#C2185B"), //Pink 700

                            ColorTranslator.FromHtml("#e53935"), //Red 600
                            ColorTranslator.FromHtml("#F4511E"), //Deep Orange 600
                            ColorTranslator.FromHtml("#FB8C00"), //Orange 600
                            ColorTranslator.FromHtml("#FFB300"), //Amber 600
                            ColorTranslator.FromHtml("#FDD835"), //Yellow 600
                            ColorTranslator.FromHtml("#C0CA33"), //Lime 600
                            ColorTranslator.FromHtml("#7CB342"), //Light Green 600
                            ColorTranslator.FromHtml("#43A047"), //Green 600
                            ColorTranslator.FromHtml("#00897B"), //Teal 600
                            ColorTranslator.FromHtml("#00ACC1"), //Cyan 600
                            ColorTranslator.FromHtml("#039BE5"), //Light Blue 600
                            ColorTranslator.FromHtml("#1E88E5"), //Blue 600
                            ColorTranslator.FromHtml("#3949AB"), //Indigo 600
                            ColorTranslator.FromHtml("#5E35B1"), //Deep Purple 600
                            ColorTranslator.FromHtml("#8E24AA"), //Purple 600
                            ColorTranslator.FromHtml("#D81B60"), //Pink 600

                            ColorTranslator.FromHtml("#F44336"), //Red 500
                            ColorTranslator.FromHtml("#FF5722"), //Deep Orange 500
                            ColorTranslator.FromHtml("#FF9800"), //Orange 500
                            ColorTranslator.FromHtml("#FFC107"), //Amber 500
                            ColorTranslator.FromHtml("#FFEB3B"), //Yellow 500
                            ColorTranslator.FromHtml("#CDDC39"), //Lime 500
                            ColorTranslator.FromHtml("#8BC34A"), //Light Green 500
                            ColorTranslator.FromHtml("#4CAF50"), //Green 500
                            ColorTranslator.FromHtml("#009688"), //Teal 500
                            ColorTranslator.FromHtml("#00BCD4"), //Cyan 500
                            ColorTranslator.FromHtml("#03A9F4"), //Light Blue 500
                            ColorTranslator.FromHtml("#2196F3"), //Blue 500
                            ColorTranslator.FromHtml("#3F51B5"), //Indigo 500
                            ColorTranslator.FromHtml("#673AB7"), //Deep Purple 500
                            ColorTranslator.FromHtml("#9C27B0"), //Purple 500
                            ColorTranslator.FromHtml("#E91E63"), //Pink 500

                            ColorTranslator.FromHtml("#ef5350"), //Red 400
                            ColorTranslator.FromHtml("#FF7043"), //Deep Orange 400
                            ColorTranslator.FromHtml("#FFA726"), //Orange 400
                            ColorTranslator.FromHtml("#FFCA28"), //Amber 400
                            ColorTranslator.FromHtml("#FFEE58"), //Yellow 400
                            ColorTranslator.FromHtml("#D4E157"), //Lime 400
                            ColorTranslator.FromHtml("#9CCC65"), //Light Green 400
                            ColorTranslator.FromHtml("#66BB6A"), //Green 400
                            ColorTranslator.FromHtml("#26A69A"), //Teal 400
                            ColorTranslator.FromHtml("#26C6DA"), //Cyan 400
                            ColorTranslator.FromHtml("#29B6F6"), //Light Blue 400
                            ColorTranslator.FromHtml("#42A5F5"), //Blue 400
                            ColorTranslator.FromHtml("#5C6BC0"), //Indigo 400
                            ColorTranslator.FromHtml("#7E57C2"), //Deep Purple 400
                            ColorTranslator.FromHtml("#AB47BC"), //Purple 400
                            ColorTranslator.FromHtml("#EC407A"), //Pink 400

                            ColorTranslator.FromHtml("#e57373"), //Red 300
                            ColorTranslator.FromHtml("#FF8A65"), //Deep Orange 300
                            ColorTranslator.FromHtml("#FFB74D"), //Orange 300
                            ColorTranslator.FromHtml("#FFD54F"), //Amber 300
                            ColorTranslator.FromHtml("#FFF176"), //Yellow 300
                            ColorTranslator.FromHtml("#DCE775"), //Lime 300
                            ColorTranslator.FromHtml("#AED581"), //Light Green 300
                            ColorTranslator.FromHtml("#81C784"), //Green 300
                            ColorTranslator.FromHtml("#4DB6AC"), //Teal 300
                            ColorTranslator.FromHtml("#4DD0E1"), //Cyan 300
                            ColorTranslator.FromHtml("#4FC3F7"), //Light Blue 300
                            ColorTranslator.FromHtml("#64B5F6"), //Blue 300
                            ColorTranslator.FromHtml("#7986CB"), //Indigo 300
                            ColorTranslator.FromHtml("#9575CD"), //Deep Purple 300
                            ColorTranslator.FromHtml("#BA68C8"), //Purple 300
                            ColorTranslator.FromHtml("#F06292"), //Pink 300

                            ColorTranslator.FromHtml("#ef9a9a"), //Red 200
                            ColorTranslator.FromHtml("#FFAB91"), //Deep Orange 200
                            ColorTranslator.FromHtml("#FFCC80"), //Orange 200
                            ColorTranslator.FromHtml("#FFE082"), //Amber 200
                            ColorTranslator.FromHtml("#FFF59D"), //Yellow 200
                            ColorTranslator.FromHtml("#E6EE9C"), //Lime 200
                            ColorTranslator.FromHtml("#C5E1A5"), //Light Green 200
                            ColorTranslator.FromHtml("#A5D6A7"), //Green 200
                            ColorTranslator.FromHtml("#80CBC4"), //Teal 200
                            ColorTranslator.FromHtml("#80DEEA"), //Cyan 200
                            ColorTranslator.FromHtml("#81D4FA"), //Light Blue 200
                            ColorTranslator.FromHtml("#90CAF9"), //Blue 200
                            ColorTranslator.FromHtml("#9FA8DA"), //Indigo 200
                            ColorTranslator.FromHtml("#B39DDB"), //Deep Purple 200
                            ColorTranslator.FromHtml("#CE93D8"), //Purple 200
                            ColorTranslator.FromHtml("#F48FB1"), //Pink 200
                        };
                        break;
                    }
                case "reds":
                    {
                        listMaterialColors = new List<Color>
                        {
                            ColorTranslator.FromHtml("#b71c1c"),
                            ColorTranslator.FromHtml("#c62828"),
                            ColorTranslator.FromHtml("#d32f2f"),
                            ColorTranslator.FromHtml("#e53935"),
                            ColorTranslator.FromHtml("#f44336"),
                            ColorTranslator.FromHtml("#ef5350"),
                            ColorTranslator.FromHtml("#e57373"),
                            ColorTranslator.FromHtml("#ef9a9a"),
                            ColorTranslator.FromHtml("#ffcdd2"),
                            ColorTranslator.FromHtml("#ffebee"),
                            ColorTranslator.FromHtml("#ffcdd2"),
                            ColorTranslator.FromHtml("#ef9a9a"),
                            ColorTranslator.FromHtml("#e57373"),
                            ColorTranslator.FromHtml("#ef5350"),
                            ColorTranslator.FromHtml("#f44336"),
                            ColorTranslator.FromHtml("#e53935"),
                            ColorTranslator.FromHtml("#d32f2f"),
                            ColorTranslator.FromHtml("#c62828")
                        };
                        break;
                    }
                case "google":
                    {
                        listMaterialColors = new List<Color>
                        {
                            ColorTranslator.FromHtml("#db3236"),
                            ColorTranslator.FromHtml("#f4c20d"),
                            ColorTranslator.FromHtml("#3cba54"),
                            ColorTranslator.FromHtml("#4885ed")
                        };
                        break;
                    }

                case "black":
                    {
                        listMaterialColors = new List<Color>
                        {
                            ColorTranslator.FromHtml("#000000")
                        };
                        break;
                    }
                default:
                    {
                        listMaterialColors = new List<Color>
                        {
                            //Black
                            ColorTranslator.FromHtml("#000000")
                        };
                        break;
                    }
            }

            return listMaterialColors;
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Dock the PictureBox to the form and set its background to white.
            //pictureBox1.Dock = DockStyle.Fill;
            //pictureBox1.BackColor = Color.Gray;
            // Connect the Paint event of the PictureBox to the event handler method.
            //pictureBox1.Paint += this.pictureBox1_Paint;

            //FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);

            panel1.BackColor = ColorTranslator.FromHtml("#000000");

            if (!generateInBackground)
            {
                pictureBox1.Paint += PictureBox1_Paint;
            }

            int intWindowHeight = Convert.ToInt32(ConfigurationManager.AppSettings["image-height"]);
            const double goldenRatio = 1.61803398874989484820458683436;

            int intWindowWidth = (int)Math.Round(intWindowHeight / (1 / goldenRatio));

            TheBitmap = new System.Drawing.Bitmap(intWindowHeight, intWindowHeight);
            panel1.Height = intWindowHeight;
            panel1.Width = intWindowHeight;
            pictureBox1.Height = intWindowHeight;
            pictureBox1.Width = intWindowHeight;

            //panel1.AutoScroll = true;

            // Add the PictureBox control to the Form.
            //this.Controls.Add(pictureBox1);
            Controls.Add(panel1);

            panel1.Controls.Add(pictureBox1);

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            pictureBox1.Click += PictureBox1_Click;

            //timer.Interval = layerGenerationRate;
            //timer.Tick += Timer_Tick;

            //timer.Enabled = true;

            //timer.Start();

            System.ComponentModel.BackgroundWorker backgroundWorker = new System.ComponentModel.BackgroundWorker();

            backgroundWorker.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;

            Graphics g = Graphics.FromImage(TheBitmap);

            backgroundWorker.RunWorkerAsync(g);

            //Task.Factory.StartNew(() =>
            //{
            //    using (Graphics g = Graphics.FromImage(TheBitmap))
            //    {
            //        while (layerLimit == 0 || layerLimit > countLayers)
            //        {
            //            DrawFractal(g);
            //            countLayers++;
            //        }
            //            //do your drawing routines here
            //        }
            //        //invoke an action against the main thread to draw the buffer to the background image of the main form.
            //        this.Invoke(new Action(pictureBox1.Refresh));
            //});
            //DrawFractal();

            //pictureBox1.Refresh();
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            switch (me.Button)
            {
                case MouseButtons.Left:
                    // Left click
                    break;

                case MouseButtons.Right:
                    // Right click
                    TheBitmap.Save(@"C:\Fractals\" + DateTime.Now.ToString("MM-dd-yyyy--hh-mm-ss") + ".bmp");
                    break;
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!generateInBackground)
            {
                pictureBox1.Refresh();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // pictureBox1.Refresh();
            //countLayers++;
        }

        private void BackgroundWorker1_DoWork(object sender,
            DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the
            // RunWorkerCompleted eventhandler.
            Graphics g = (Graphics)e.Argument;
            g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#F5F5F6")), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            while (layerLimit == 0 || layerLimit > countLayers)
            {
                DrawFractal(g, worker, e);
                countLayers++;
                worker.ReportProgress(0, "right");
            }

            if (generateInBackground)
            {
                TheBitmap.Save(@"C:\Fractals\" + DateTime.Now.ToString("MM-dd-yyyy--hh-mm-ss") + ".bmp");
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(TheBitmap, 0, 0);
        }

        private bool InterpretPathing(string pathing, int i)
        {
            List<char> listPathOrder = new List<char>();
            listPathOrder.AddRange(pathing);

            int index = i % listPathOrder.Count;

            return listPathOrder[index] == '1';
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //DrawFractal();

            countLayers++;
        }

    }
}