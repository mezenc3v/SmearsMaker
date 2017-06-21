using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using SmearTracer.Core.Abstract;
using SmearTracer.Core.Models;

namespace SmearTracer.Core
{
    public class BsmPair:ISequenceOfPartMaker
    {
        private readonly Segment _segment;
        private readonly double _maxDistance;

        public BsmPair(Segment segment, double maxDistance)
        {
            _segment = segment;
            _maxDistance = maxDistance;
        }

        private List<SequenceOfParts> Combining(List<SequenceOfParts> sequences)
        {
            bool distanceCheck;

            do
            {
                distanceCheck = false;
                if (sequences.Count > 1)
                {
                    for (int i = 0; i < sequences.Count && sequences.Count > 1; i++)
                    {
                        var index = NearestPart(sequences, sequences[i]);

                        if (Distance(sequences[i], sequences[index]) < _maxDistance)
                        {
                            distanceCheck = true;
                            var combinedSequence = Combine(sequences[i], sequences[index]);
                            var segmentToBeDeleted = sequences[index];
                            sequences.RemoveAt(i);
                            sequences.Remove(segmentToBeDeleted);
                            sequences.Add(combinedSequence);
                        }
                    }
                }
            } while (distanceCheck);

            return sequences;
        }

        private static SequenceOfParts Combine(SequenceOfParts first, SequenceOfParts second)
        {
            SequenceOfParts newSequence = new BrushStroke();

            var a = Distance(first.Points.First().Position, second.Points.First().Position);
            var b = Distance(first.Points.Last().Position, second.Points.Last().Position);
            var c = Distance(first.Points.First().Position, second.Points.Last().Position);
            var d = Distance(first.Points.Last().Position, second.Points.First().Position);

            if (a < b)
            {
                if (a < c)
                {
                    if (a < d)
                    {
                        first.Points.Reverse();
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                    else
                    {
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                }
                else
                {
                    if (c < d)
                    {
                        first.Points.Reverse();
                        newSequence.Points.AddRange(second.Points);
                        newSequence.Points.AddRange(first.Points);
                    }
                    else
                    {
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                }
            }
            else
            {
                if (b < c)
                {
                    if (b < d)
                    {
                        second.Points.Reverse();
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                    else
                    {
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                }
                else
                {
                    if (c < d)
                    {
                        first.Points.Reverse();
                        newSequence.Points.AddRange(second.Points);
                        newSequence.Points.AddRange(first.Points);
                    }
                    else
                    {
                        newSequence.Points.AddRange(first.Points);
                        newSequence.Points.AddRange(second.Points);
                    }
                }
            }

            return newSequence;
        }

        private static int NearestPart(IList<SequenceOfParts> sequences, SequenceOfParts seq)
        {
            var headPosition = seq.Points.First().Position;
            var tailPosition = seq.Points.Last().Position;
            
            double minDistance;
            int index;

            if (sequences.First() != seq)
            {
                minDistance = Distance(sequences[0].Points.First().Position, headPosition);
                index = 0;
            }
            else
            {
                minDistance = Distance(sequences[1].Points.First().Position, headPosition);
                index = 1;
            }

            foreach (var sequence in sequences.Where(s=>s != seq))
            {
                var hh = Distance(sequence.Points.First().Position, headPosition);
                var ht = Distance(sequence.Points.First().Position, tailPosition);
                var tt = Distance(sequence.Points.Last().Position, tailPosition);
                var th = Distance(sequence.Points.Last().Position, headPosition);

                if (hh > 0 && minDistance > hh)
                {
                    index = sequences.IndexOf(sequence);
                    minDistance = hh;
                }

                if (ht > 0 && minDistance > ht)
                {
                    index = sequences.IndexOf(sequence);
                    minDistance = ht;
                }

                if (tt > 0 && minDistance > tt)
                {
                    index = sequences.IndexOf(sequence);
                    minDistance = tt;
                }

                if (th > 0 && minDistance > th)
                {
                    index = sequences.IndexOf(sequence);
                    minDistance = th;
                }
            }

            return index;
        }

        private static int FindNearestSequence(IReadOnlyList<SequenceOfParts> sequences, IUnit point)
        {
            var index = 0;
            var minDistance = Distance(sequences.First().Points.First().Position, point.Position);

            for (int i = 0; i < sequences.Count; i++)
            {
                var distanceHead = Distance(sequences[i].Points.First().Position, point.Position);
                var distanceTail = Distance(sequences[i].Points.Last().Position, point.Position);

                if (minDistance > distanceHead)
                {
                    minDistance = distanceHead;
                    index = i;
                }

                if (minDistance > distanceTail)
                {
                    minDistance = distanceTail;
                    index = i;
                }
            }

            return index;
        }

        private static SequenceOfParts Combine(SequenceOfParts first, IUnit second)
        {
            SequenceOfParts newSequence = new BrushStroke();

            var distanceHead = Distance(first.Points.First().Position, second.Position);
            var distanceTail = Distance(first.Points.Last().Position, second.Position);

            if (distanceHead > distanceTail)
            {
                newSequence.Points.AddRange(first.Points);
                newSequence.Points.Add(second);
            }
            else
            {
                newSequence.Points.Add(second);
                newSequence.Points.AddRange(first.Points);
            }

            return newSequence;
        }

        private List<SequenceOfParts> Pairing()
        {
            var brushStrokes = new List<SequenceOfParts>();
            if (_segment.GraphicUnits.Count > 1)
            {
                var points = _segment.GraphicUnits.Select(p => p.Center).ToList();
                var startPoint = points.OrderBy(p=>Distance(_segment.Center.Position, p.Position)).First();

                while (points.Count > 0)
                {
                    if (points.Count > 1)
                    {
                        var list = new List<IUnit>();
                        var main = points.OrderBy(p => Distance(startPoint.Position, p.Position)).Last();

                        list.Add(main);
                        points.Remove(main);

                        var next = points.OrderBy(p => Distance(main.Position, p.Position)).First();

                        if (Distance(next.Position, main.Position) < _maxDistance)
                        {
                            list.Add(next);
                            points.Remove(next);
                            brushStrokes.Add(new BrushStroke { Points = list });
                        }
                        else
                        {
                            var index = FindNearestSequence(brushStrokes, list.First());
                            var newSequence = Combine(brushStrokes[index], list.First());

                            brushStrokes.RemoveAt(index);
                            brushStrokes.Add(newSequence);
                        }
                    }
                    else
                    {
                        var index = FindNearestSequence(brushStrokes, points.First());
                        var newSequence = Combine(brushStrokes[index], points.First());

                        brushStrokes.RemoveAt(index);
                        brushStrokes.Add(newSequence);
                        points = new List<Pixel>();
                    }
                }
            }
            else
            {
                var brushStroke = new BrushStroke();
                brushStroke.Points.Add(_segment.GraphicUnits[0].Center);
                brushStrokes.Add(brushStroke);
            }

            return brushStrokes;
        }

        private static double Distance(Point first, Point second)
        {
            var sum = Math.Pow(first.X - second.X, 2);
            sum += Math.Pow(first.Y - second.Y, 2);
            return Math.Sqrt(sum);
        }

        private static double Distance(SequenceOfParts first, SequenceOfParts second)
        {
            var hh = Distance(first.Points.First().Position, second.Points.First().Position);
            var ht = Distance(first.Points.First().Position, second.Points.Last().Position);
            var tt = Distance(first.Points.Last().Position, second.Points.Last().Position);
            var th = Distance(first.Points.Last().Position, second.Points.First().Position);
            var minDistance = hh;

            if (ht<minDistance)
            {
                minDistance = ht;
            }

            if (tt < minDistance)
            {
                minDistance = tt;
            }

            if (th < minDistance)
            {
                minDistance = th;
            }

            return minDistance;
        }

        public List<SequenceOfParts> Execute()
        {
            var pairs = Pairing();
            var brushStrokes = Combining(pairs);
            return brushStrokes;
        }
    }
}
