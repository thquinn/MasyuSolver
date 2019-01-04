using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace MasyuSolver {
    public partial class MasyuPanel : UserControl {
        static float BORDER_PERCENT = .125f;
        static float WHITE_CIRCLE_OUTLINE_PERCENT = .066f;

        MasyuBoard board;
        int boardWidth, boardHeight;
        float cellSize, xBorder, yBorder;

        public TextBox textBox;

        public MasyuPanel() {
            ResizeRedraw = true;
            InitializeComponent();

            boardWidth = 12;
            boardHeight = 12;
            board = new MasyuBoard(boardWidth, boardHeight);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseClick(e);
            int x = (int)Math.Floor((e.X - xBorder) / cellSize);
            int y = (int)Math.Floor((e.Y - yBorder) / cellSize);
            if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight) {
                return;
            }
            board.SetCircle(x, y, e.Button == MouseButtons.Right || ModifierKeys == Keys.Shift ? MasyuCircle.WHITE : MasyuCircle.BLACK);
            textBox.Clear();
            board.PropagateConstaints(Log);
            Invalidate();
        }
        public void Log(string s) {
            if (textBox.TextLength > 0) {
                textBox.Text += "\r\n";
            }
            textBox.Text += s;
        }

        // Menu items.
        public void New(int boardWidth, int boardHeight) {
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            board = new MasyuBoard(boardWidth, boardHeight);
            Invalidate();
        }
        public void Solve() {
            textBox.Clear();
            board.Solve(Log);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            // Calculate layout.
            base.OnPaint(e);
            float horizonalCellSize = (float)Width / boardWidth;
            float verticalCellSize = (float)Height / boardHeight;
            cellSize = Math.Min(horizonalCellSize, verticalCellSize) * (1 - BORDER_PERCENT);
            xBorder = Width / 2 - boardWidth / 2f * cellSize;
            yBorder = Height / 2 - boardHeight / 2f * cellSize;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw grid.
            Pen grayPen = new Pen(Color.LightGray);
            for (int x = 1; x < boardWidth; x++) {
                e.Graphics.DrawLine(grayPen, xBorder + x * cellSize, yBorder, xBorder + x * cellSize, Height - yBorder);
            }
            for (int y = 1; y < boardHeight; y++) {
                e.Graphics.DrawLine(grayPen, xBorder, yBorder + y * cellSize, Width - xBorder, yBorder + y * cellSize);
            }
            Pen blackPen = new Pen(Color.Black, Math.Max(1, cellSize / 6));
            blackPen.EndCap = LineCap.Round;
            blackPen.StartCap = LineCap.Round;
            e.Graphics.DrawRectangle(blackPen, xBorder - cellSize / 16, yBorder - cellSize / 16, Width - xBorder * 2 + cellSize / 8, Height - yBorder * 2 + cellSize / 8);

            // Draw circles.
            float circleRadius = cellSize * .33f;
            Brush blackCircleBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
            float blackCircleRadius = circleRadius + cellSize / 20;
            Pen whiteCirclePen = new Pen(Color.FromArgb(20, 20, 20), cellSize * WHITE_CIRCLE_OUTLINE_PERCENT);
            for (int x = 0; x < boardWidth; x++) {
                for (int y = 0; y < boardHeight; y++) {
                    MasyuCircle circle = board.GetCircle(x, y);
                    if (circle == MasyuCircle.NONE) {
                        continue;
                    }
                    if (circle == MasyuCircle.BLACK) {
                        e.Graphics.FillEllipse(blackCircleBrush, xBorder + (x + .5f) * cellSize - blackCircleRadius, yBorder + (y + .5f) * cellSize - blackCircleRadius, 2 * blackCircleRadius, 2 * blackCircleRadius);
                    } else if (circle == MasyuCircle.WHITE) {
                        e.Graphics.DrawEllipse(whiteCirclePen, xBorder + (x + .5f) * cellSize - circleRadius, yBorder + (y + .5f) * cellSize - circleRadius, 2 * circleRadius, 2 * circleRadius);
                    }
                }
            }

            // Draw horizontal lines and Xs.
            Pen xPen = new Pen(Color.FromArgb(125, 125, 125), Math.Max(1, cellSize / 20));
            for (int x = 0; x < boardWidth - 1; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board.IsLine(x, y, true))
                    {
                        e.Graphics.DrawLine(blackPen, xBorder + (x + .5f) * cellSize, yBorder + (y + .5f) * cellSize, xBorder + (x + 1.5f) * cellSize, yBorder + (y + .5f) * cellSize);
                    } else if (board.IsX(x, y, true))
                    {
                        float px = xBorder + (x + 1) * cellSize;
                        float py = yBorder + (y + .5f) * cellSize;
                        float offset = cellSize * .066f;
                        e.Graphics.DrawLine(xPen, px - offset, py - offset, px + offset, py + offset);
                        e.Graphics.DrawLine(xPen, px + offset, py - offset, px - offset, py + offset);
                    }
                }
            }
            // Draw vertical lines.
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight - 1; y++)
                {
                    if (board.IsLine(x, y, false))
                    {
                        e.Graphics.DrawLine(blackPen, xBorder + (x + .5f) * cellSize, yBorder + (y + .5f) * cellSize, xBorder + (x + .5f) * cellSize, yBorder + (y + 1.5f) * cellSize);
                    }
                    else if (board.IsX(x, y, false))
                    {
                        float px = xBorder + (x + .5f) * cellSize;
                        float py = yBorder + (y + 1) * cellSize;
                        float offset = cellSize * .066f;
                        e.Graphics.DrawLine(xPen, px - offset, py - offset, px + offset, py + offset);
                        e.Graphics.DrawLine(xPen, px + offset, py - offset, px - offset, py + offset);
                    }
                }
            }

            // Draw in/out.
            Brush inBrush = new SolidBrush(Color.FromArgb(164, 222, 255));
            Brush outBrush = new SolidBrush(Color.FromArgb(221, 243, 255));
            for (int x = 0; x < boardWidth - 1; x++) {
                for (int y = 0; y < boardHeight - 1; y++) {
                    MasyuInOut inOut = board.GetInOut(x, y);
                    if (inOut == MasyuInOut.UNKNOWN) {
                        continue;
                    }
                    float px = xBorder + (x + 1) * cellSize;
                    float py = yBorder + (y + 1) * cellSize;
                    float offset = cellSize * .2f;
                    PointF diamond1 = new PointF(px - offset, py);
                    PointF diamond2 = new PointF(px, py - offset);
                    PointF diamond3 = new PointF(px + offset, py);
                    PointF diamond4 = new PointF(px, py + offset);
                    e.Graphics.FillPolygon(inOut == MasyuInOut.IN ? inBrush : outBrush, new PointF[] { diamond1, diamond2, diamond3, diamond4 });
                }
            }
        }
    }
}
