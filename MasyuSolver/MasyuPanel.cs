using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasyuSolver {
    public partial class MasyuPanel : UserControl {
        static float BORDER_PERCENT = .1f;

        MasyuBoard board;
        int boardWidth, boardHeight;
        float cellSize, xBorder, yBorder;

        public MasyuPanel() {
            ResizeRedraw = true;
            InitializeComponent();

            boardWidth = 20;
            boardHeight = 10;
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
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            // Calculate layout.
            base.OnPaint(e);
            float horizonalCellSize = (float)Width / boardWidth;
            float verticalCellSize = (float)Height / boardHeight;
            cellSize = Math.Min(horizonalCellSize, verticalCellSize) * (1 - BORDER_PERCENT);
            xBorder = Width / 2 - boardWidth / 2 * cellSize;
            yBorder = Height / 2 - boardHeight / 2 * cellSize;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid.
            Pen grayPen = new Pen(Color.LightGray);
            for (int x = 1; x < boardWidth; x++) {
                e.Graphics.DrawLine(grayPen, xBorder + x * cellSize, yBorder, xBorder + x * cellSize, Height - yBorder);
            }
            for (int y = 1; y < boardHeight; y++) {
                e.Graphics.DrawLine(grayPen, xBorder, yBorder + y * cellSize, Width - xBorder, yBorder + y * cellSize);
            }
            Pen blackPen = new Pen(Color.Black, Math.Max(1, cellSize / 8));
            e.Graphics.DrawRectangle(blackPen, xBorder - cellSize / 16, yBorder - cellSize / 16, Width - xBorder * 2 + cellSize / 8, Height - yBorder * 2 + cellSize / 8);

            // Draw circles.
            float circleRadius = cellSize * .33f;
            Brush blackCircleBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
            float blackCircleRadius = circleRadius + cellSize / 20;
            Pen whiteCirclePen = new Pen(Color.FromArgb(20, 20, 20), cellSize / 10);
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
        }
    }
}
