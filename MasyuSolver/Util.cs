﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MasyuSolver
{
    public class Util
    {
        public static Tuple<int, int, byte>[] ParsePatternString(string patternString)
        {
            if (patternString.Length == 0)
            {
                return new Tuple<int, int, byte>[0];
            }
            List<Tuple<int, int, byte>> output = new List<Tuple<int, int, byte>>();
            string[] lines = patternString.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                Debug.Assert(lines[i].Length == lines[0].Length);
            }
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    char c = lines[y][x];
                    if (c == '.')
                    {
                        continue;
                    }
                    int index = Array.IndexOf(MasyuBoard.CHARS, c);
                    Debug.Assert(index > 0);
                    output.Add(new Tuple<int, int, byte>(x, y, (byte)index));
                }
            }
            return output.ToArray();
        }
        public static Tuple<int, int, byte, bool>[][] AllPatternRotationsAndReflections(List<Tuple<int, int, byte, bool>> pattern)
        {
            List<Tuple<int, int, byte, bool>[]> output = new List<Tuple<int, int, byte, bool>[]>();
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(-t.Item1, -t.Item2, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(-t.Item1, t.Item2, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(t.Item1, -t.Item2, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(t.Item1, t.Item2, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(-t.Item2, -t.Item1, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(-t.Item2, t.Item1, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(t.Item2, -t.Item1, t.Item3, t.Item4)).ToArray());
            output.Add(pattern.Select(t => new Tuple<int, int, byte, bool>(t.Item2, t.Item1, t.Item3, t.Item4)).ToArray());
            foreach (var rotation in output)
            {
                int minX = rotation.Min(t => t.Item1);
                int minY = rotation.Min(t => t.Item2);
                for (int i = 0; i < rotation.Length; i++)
                {
                    rotation[i] = new Tuple<int, int, byte, bool>(rotation[i].Item1 - minX, rotation[i].Item2 - minY, rotation[i].Item3, rotation[i].Item4);
                }
                Array.Sort(rotation);
            }
            for (int i = output.Count - 1; i >= 1; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (output[i].SequenceEqual(output[j]))
                    {
                        output.RemoveAt(i);
                        break;
                    }
                }
            }
            return output.ToArray();
        }
        public static int[] GetPathNeighbors(int x, int y, int width, int height) {
            bool horizontal = y % 2 == 1;
            List<int> pathNeighbors = new List<int>();
            if (x >= 2 && y >= 2) {
                pathNeighbors.Add(XYToIndex(x - 1, y - 1, width));
            }
            if (x >= 2 && y <= height - 3) {
                pathNeighbors.Add(XYToIndex(x - 1, y + 1, width));
            }
            if (x <= width - 3 && y >= 2) {
                pathNeighbors.Add(XYToIndex(x + 1, y - 1, width));
            }
            if (x <= width - 3 && y <= height - 3) {
                pathNeighbors.Add(XYToIndex(x + 1, y + 1, width));
            }
            if (horizontal) {
                if (x >= 3) {
                    pathNeighbors.Add(XYToIndex(x - 2, y, width));
                }
                if (x <= width - 4) {
                    pathNeighbors.Add(XYToIndex(x + 2, y, width));
                }
            } else {
                if (y >= 3) {
                    pathNeighbors.Add(XYToIndex(x, y - 2, width));
                }
                if (y <= height - 4) {
                    pathNeighbors.Add(XYToIndex(x, y + 2, width));
                }
            }
            return pathNeighbors.ToArray();
        }
        private static int XYToIndex(int x, int y, int width) {
            return y * width + x;
        }
    }
}