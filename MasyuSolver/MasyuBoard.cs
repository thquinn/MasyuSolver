using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasyuSolver {
    public class MasyuBoard {
        MasyuCircle[,] circles;
        MasyuEdge[,] horizontalEdges, verticalEdges;
        MasyuInOut[,] inOut;

        public MasyuBoard(int width, int height) {
            circles = new MasyuCircle[width, height];
            horizontalEdges = new MasyuEdge[width - 1, height];
            verticalEdges = new MasyuEdge[width, height - 1];
            inOut = new MasyuInOut[width - 1, height - 1];
        }

        public int GetWidth() {
            return circles.GetLength(0);
        }
        public int GetHeight() {
            return circles.GetLength(1);
        }
        public MasyuCircle GetCircle(int x, int y) {
            return circles[x, y];
        }
        public void SetCircle(int x, int y, MasyuCircle type) {
            if (circles[x, y] != MasyuCircle.NONE) {
                circles[x, y] = MasyuCircle.NONE;
            } else {
                circles[x, y] = type;
            }
        }
    }

    public enum MasyuEdge {
        UNKNOWN, EDGE, X
    }
    public enum MasyuInOut {
        UNKNOWN, IN, OUT
    }
}
