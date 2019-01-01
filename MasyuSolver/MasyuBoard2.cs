using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MasyuSolver
{
    public class MasyuBoard2
    {
        public static char[] CHARS = new char[] { '.', 'Q', 'O', '-', 'X', 'v', '^', '*' };
        static Tuple<string, string>[] PATTERN_STRING_PAIRS = new Tuple<string, string>[] {
            // completing lines through circles
            new Tuple<string, string>("....\nQ-..\n....", "..X.\nQ-.-\n..X."),
            new Tuple<string, string>("...\n-O.\n...", ".X.\n-O-\n.X."),
            // line continuation through Xs
            new Tuple<string, string>("...\nX*X\n.-.", ".-.\nX.X\n.-."),
            new Tuple<string, string>(".X.\nX*.\n.-.", ".X.\nX.-\n.-."),
            // X completion around lines
            new Tuple<string, string>(".-.\n-*.\n...", ".-.\n-.X\n.X."),
            new Tuple<string, string>(".-.\n.*.\n.-.", ".-.\nX.X\n.-."),
            new Tuple<string, string>(".X.\nX*X\n...", ".X.\nX.X\n.X."),
            // black circles blocked by features
            new Tuple<string, string>(".....\nXQ...\n.....", "...X.\nXQ-.-\n...X."),
            new Tuple<string, string>(".......\nX..Q...\n.......", ".....X.\nX.XQ-.-\n.....X."),
            new Tuple<string, string>(".........\n...Q.Q...\n.........", ".X.....X.\n-.-QXQ-.-\n.X.....X."),
            new Tuple<string, string>(".....-\n...Q..\n......", ".X...-\n-.-QX.\n.X...."),
            new Tuple<string, string>("..........\n...Q...O.O\n..........", ".X........\n-.-QX..O.O\n.X........"),
            new Tuple<string, string>("O...O\n.....\n..Q..\n.....\n.....\n.....", "O...O\n..X..\n..Q..\n..-..\n.X.X.\n..-.."),
            // white circle rule
            new Tuple<string, string>("-.-O...", "-.-O-.X"),
            // white circles blocked by features
            new Tuple<string, string>("...\n.O.\n.X.", ".X.\n-O-\n.X."),
            new Tuple<string, string>(".......\n.O.O.O.\n.......", ".-.-.-.\nXOXOXOX\n.-.-.-."),
            new Tuple<string, string>(".......\n-..O.O.\n.......", "...-.-.\n-.XOXOX\n...-.-."),
            new Tuple<string, string>(".......\n-..O..-\n.......", "...-...\n-.XOX.-\n...-..."),
            // in/out and walls
            new Tuple<string, string>("v-.", "v-^"),
            new Tuple<string, string>("^-.", "^-v"),
            new Tuple<string, string>("vX.", "vXv"),
            new Tuple<string, string>("^X.", "^X^"),
            new Tuple<string, string>("v.^", "v-^"),
            new Tuple<string, string>("^.v", "^-v"),
            new Tuple<string, string>("v.v", "vXv"),
            new Tuple<string, string>("^.^", "^X^"),
            // NOTE: the pattern of in/out following the outside of a corner doesn't seem to be necessary?
        };

        private byte[,] board;
        private List<MasyuPattern>[,,] patternLookupTable;

        public MasyuBoard2(int width, int height)
        {
            // Populate board, adding Xs and outs to perimeter.
            // TODO: Expand edge of board to allow more patterns to work at edge.
            int arrWidth = width * 2 + 1, arrHeight = height * 2 + 1;
            board = new byte[arrWidth, arrHeight];
            for (int x = 0; x < arrWidth; x++)
            {
                for (int y = 0; y < arrHeight; y++)
                {
                    if (x != 0 && x != arrWidth - 1 && y != 0 && y != arrHeight - 1)
                    {
                        continue;
                    }
                    board[x, y] = (x + y) % 2 == 0 ? (byte)6 : (byte)4;
                }
            }

            // TODO: Populate pattern lookup table with all rotations/reflections.
            // TODO: Sort contradiction patterns to the front.
            patternLookupTable = new List<MasyuPattern>[arrWidth, arrHeight, 7];
            for (int x = 0; x < patternLookupTable.GetLength(0); x++)
            {
                for (int y = 0; y < patternLookupTable.GetLength(1); y++)
                {
                    for (int z = 0; z < patternLookupTable.GetLength(2); z++)
                    {
                        patternLookupTable[x, y, z] = new List<MasyuPattern>();
                    }
                }
            }
            // TODO: Can't we avoid centering patterns on spaces where that type of piece cannot appear? Won't save much space, though.
            // TODO: Look into precomputing coordinates so we don't need to add offsets. Would multiply memory usage by the size of the board...
            foreach (Tuple<string, string> patternStringPair in PATTERN_STRING_PAIRS)
            {
                Debug.Assert(patternStringPair.Item1.Length == patternStringPair.Item2.Length);
                Tuple<int, int, byte>[] checkPattern = Util.ParsePatternString(patternStringPair.Item1);
                Tuple<int, int, byte>[] setPattern = Util.ParsePatternString(patternStringPair.Item2);
                List<Tuple<int, int, byte, bool>> combined = new List<Tuple<int, int, byte, bool>>(checkPattern.Select(t => new Tuple<int, int, byte, bool>(t.Item1, t.Item2, t.Item3, false)));
                combined.AddRange(setPattern.Except(checkPattern).Select(t => new Tuple<int, int, byte, bool>(t.Item1, t.Item2, t.Item3, true)));
                Tuple<int, int, byte, bool>[][] patternRotations = Util.AllPatternRotationsAndReflections(combined);
                foreach (Tuple<int, int, byte, bool>[] rotation in patternRotations)
                {
                    int maxX = rotation.Max(t => t.Item1);
                    int maxY = rotation.Max(t => t.Item2);
                    Tuple<int, int, byte>[] check = rotation.Where(t => !t.Item4).Select(t => new Tuple<int, int, byte>(t.Item1, t.Item2, t.Item3)).ToArray();
                    Tuple<int, int, byte>[] set = rotation.Where(t => t.Item4).Select(t => new Tuple<int, int, byte>(t.Item1, t.Item2, t.Item3)).ToArray();
                    // For each checked feature, create a pattern with it at its center.
                    foreach (Tuple<int, int, byte> checkFeature in check)
                    {
                        if (checkFeature.Item3 == 7) {
                            continue;
                        }
                        // Bit of a hack here: check for the special '*' character, which must be placed on a circle space.
                        Tuple<int, int, byte> starTuple = check.FirstOrDefault(t => t.Item3 == 7);
                        // Recenter the pattern.
                        Tuple<int, int, byte>[] offsetCheck = check.Where(t => t != checkFeature && t.Item3 != 7).Select(t => new Tuple<int, int, byte>(t.Item1 - checkFeature.Item1, t.Item2 - checkFeature.Item2, t.Item3)).ToArray();
                        Tuple<int, int, byte>[] offsetSet = set.Select(t => new Tuple<int, int, byte>(t.Item1 - checkFeature.Item1, t.Item2 - checkFeature.Item2, t.Item3)).ToArray();
                        MasyuPattern pattern = new MasyuPattern(offsetCheck, offsetSet);
                        // Store each recentered pattern wherever it will fit in the table.
                        int rightMargin = maxX - checkFeature.Item1;
                        int bottomMargin = maxY - checkFeature.Item2;
                        for (int x = checkFeature.Item1; x < patternLookupTable.GetLength(0) - rightMargin; x++)
                        {
                            for (int y = checkFeature.Item2; y < patternLookupTable.GetLength(1) - bottomMargin; y++)
                            {
                                // If we had a '*', don't add this pattern in a place that would make it fall not on a circle space.
                                if (starTuple != null) {
                                    int cx = x - checkFeature.Item1 + starTuple.Item1, cy = y - checkFeature.Item2 + starTuple.Item2;
                                    bool isCircleSpace = (cx % 2 == 1) && (cy % 2 == 1);
                                    if (!isCircleSpace) {
                                        continue;
                                    }
                                }
                                patternLookupTable[x, y, checkFeature.Item3].Add(pattern);
                            }
                        }
                    }
                }
            }
        }

        public void Solve()
        {
            Clear();
            PropagateConstraints(null);
        }

        private void Clear()
        {
            for (int y = 1; y < board.GetLength(1) - 1; y++)
            {
                for (int x = 1; x < board.GetLength(0) - 1; x++)
                {
                    if (board[x, y] > 2)
                    {
                        board[x, y] = 0;
                    }
                }
            }
        }
        private void PropagateConstraints(Tuple<int, int> placed)
        {
            Queue<Tuple<int, int>> newFeatureCoors = new Queue<Tuple<int, int>>();
            if (placed != null)
            {
                newFeatureCoors.Enqueue(placed);
            }
            // If no coordinate is provided, it's our first propagation. Start with all circles.
            else
            {
                for (int x = 1; x < board.GetLength(0) - 1; x += 2)
                {
                    for (int y = 1; y < board.GetLength(1) - 1; y += 2)
                    {
                        if (board[x, y] == 1 || board[x, y] == 2)
                        {
                            newFeatureCoors.Enqueue(new Tuple<int, int>(x, y));
                        }
                    }
                }
            }
            // Propagate.
            while (newFeatureCoors.Count > 0)
            {
                Tuple<int, int> coor = newFeatureCoors.Dequeue();
                byte featureType = board[coor.Item1, coor.Item2];
                foreach (MasyuPattern pattern in patternLookupTable[coor.Item1, coor.Item2, featureType])
                {
                    // Check if pattern applies to new feature.
                    bool applies = true;
                    for (int i = 0; i < pattern.check.Length; i++)
                    {
                        if (board[coor.Item1 + pattern.check[i].Item1, coor.Item2 + pattern.check[i].Item2] != pattern.check[i].Item3)
                        {
                            applies = false;
                            break;
                        }
                    }
                    if (!applies)
                    {
                        continue;
                    }
                    // Apply the pattern.
                    // TODO: Contradiction patterns.
                    for (int i = 0; i < pattern.set.Length; i++)
                    {
                        int x = coor.Item1 + pattern.set[i].Item1;
                        int y = coor.Item2 + pattern.set[i].Item2;
                        byte present = board[x, y];
                        if (present > 0 && present != pattern.set[i].Item3)
                        {
                            // TODO: Placement contradictions.
                        } else if (present == 0)
                        {
                            board[x, y] = pattern.set[i].Item3;
                            newFeatureCoors.Enqueue(new Tuple<int, int>(x, y));
                        }
                    }
                }
            }
        }

        public void SetCircle(int x, int y, MasyuCircle circle)
        {
            int bx = x * 2 + 1, by = y * 2 + 1;
            if (board[bx, by] != 0)
            {
                board[bx, by] = 0;
                return;
            }
            int index = Array.IndexOf(CHARS, (char)circle);
            board[bx, by] = (byte)index;
        }
        public MasyuCircle GetCircle(int x, int y)
        {
            int bx = x * 2 + 1, by = y * 2 + 1;
            byte val = board[bx, by];
            if (val == 1)
            {
                return MasyuCircle.BLACK;
            }
            if (val == 2)
            {
                return MasyuCircle.WHITE;
            }
            Debug.Assert(val == 0);
            return MasyuCircle.NONE;
        }
        // TODO: Consolidate these functions.
        public bool IsLine(int x, int y, bool horizontal)
        {
            int bx = horizontal ? x * 2 + 2 : x * 2 + 1;
            int by = horizontal ? y * 2 + 1 : y * 2 + 2;
            return board[bx, by] == 3;
        }
        public bool IsX(int x, int y, bool horizontal)
        {
            int bx = horizontal ? x * 2 + 2 : x * 2 + 1;
            int by = horizontal ? y * 2 + 1 : y * 2 + 2;
            return board[bx, by] == 4;
        }
        public MasyuInOut GetInOut(int x, int y) {
            int bx = x * 2 + 2, by = y * 2 + 2;
            byte val = board[bx, by];
            if (val == 5) {
                return MasyuInOut.IN;
            }
            if (val == 6) {
                return MasyuInOut.OUT;
            }
            Debug.Assert(val == 0);
            return MasyuInOut.UNKNOWN;
        }

        private StringBuilder sb;
        public override string ToString()
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    sb.Append(CHARS[board[x, y]]);
                }
                sb.AppendLine();
            }
            string output = sb.ToString();
            sb.Clear();
            return output;
        }
    }

    // TOOD: Check performance as struct.
    public class MasyuPattern
    {
        public Tuple<int, int, byte>[] check, set;

        public MasyuPattern(Tuple<int, int, byte>[] check, Tuple<int, int, byte>[] after)
        {
            this.check = check;
            this.set = after;
        }
    }

    public enum MasyuCircle
    {
        NONE = '.', BLACK = 'Q', WHITE = 'O'
    }
}