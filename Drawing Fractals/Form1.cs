using System;
using System.Collections.Generic;
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

        private int colorIndex;

        // Cache font instead of recreating font objects each time we paint.
        private Font fnt = new Font("Arial", 10);

        private int garbageCollectionSize = 8;
        private List<ChainedPoint> listAllPoints = new List<ChainedPoint>();
        private Panel panel1 = new Panel();
        private PictureBox pictureBox1 = new PictureBox();
        private Timer timer = new Timer();

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

        private void DrawFractal()
        {
            int intWindowHeight = Convert.ToInt32(ConfigurationManager.AppSettings["window-height"]);
            const double goldenRatio = 1.61803398874989484820458683436;

            int intWindowWidth = (int)Math.Round(intWindowHeight / (1 / goldenRatio));
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = panel1.CreateGraphics();

            List<Color> listMaterialColors = new List<Color>();

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

            if (countLayers == 0)
            {
                ChainedPoint centerPoint = new ChainedPoint(intWindowWidth / 2, intWindowHeight / 2, (int)Directions.Center);
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

                        if (newPoint.X <= (length * 3) || newPoint.X > (intWindowWidth - (length * 3) - 1) || (newPoint.Y <= (length * 3) || newPoint.Y > (intWindowHeight - (length * 3) - 1)))
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
                }

                rainbowPen.Dispose();

                objChainedPoint.Alive = false;
            }

            if (layerLimit > 0 && layerLimit == countLayers + 1)
            {
                timer.Stop();
            }

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

            panel1.Dock = DockStyle.Fill;
            panel1.BackColor = ColorTranslator.FromHtml("#F5F5F6");
            panel1.Click += Panel1_Click;

            // Add the PictureBox control to the Form.
            //this.Controls.Add(pictureBox1);
            Controls.Add(panel1);

            timer.Interval = layerGenerationRate;
            timer.Tick += Timer_Tick;

            timer.Enabled = true;

            timer.Start();
        }

        private bool InterpretPathing(string pathing, int i)
        {
            List<char> listPathOrder = new List<char>();
            listPathOrder.AddRange(pathing);

            int index = i % listPathOrder.Count;

            return listPathOrder[index] == '1';
        }

        private void Panel1_Click(object sender, EventArgs e)
        {
            DrawFractal();

            countLayers++;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DrawFractal();

            countLayers++;
        }
    }
}