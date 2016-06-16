﻿// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Extension method for the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {
        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath<float> Expand(this EdgePath<float> edgePath, DirectedDynamicGraph graph)
        {
            return edgePath.Expand(graph.GetEdgeEnumerator());
        }

        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath<float> Expand(this EdgePath<float> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            if (edgePath.From == null)
            {
                return edgePath;
            }

            // expand everything before.
            edgePath = new EdgePath<float>(edgePath.Vertex, edgePath.Weight, edgePath.Edge, edgePath.From.Expand(enumerator));

            // expand list.
            return edgePath.ExpandLast(enumerator);
        }

        /// <summary>
        /// Expands the last edge in the given edge path.
        /// </summary>
        private static EdgePath<float> ExpandLast(this EdgePath<float> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            bool? direction;
            if (edgePath.Edge != Constants.NO_EDGE &&
                edgePath.From != null &&
                edgePath.From.Vertex != Constants.NO_VERTEX)
            {
                enumerator.MoveToEdge(edgePath.Edge);
                var contractedId = enumerator.GetContracted();
                if (contractedId.HasValue)
                { // there is a contracted vertex here!
                    // get source/target sequences.
                    var sequence1 = enumerator.GetSequence1();
                    sequence1.Reverse();
                    var sequence2 = enumerator.GetSequence2();

                    // move to the first edge (contracted -> from vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.From.Vertex, sequence1);
                    float weight1;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0, out weight1, out direction);
                    var edge1 = enumerator.IdDirected();

                    // move to the second edge (contracted -> to vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.Vertex, sequence2);
                    float weight2;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0, out weight2, out direction);
                    var edge2 = enumerator.IdDirected();

                    if (edgePath.Edge > 0)
                    {
                        var contractedPath = new EdgePath<float>(contractedId.Value, edgePath.From.Weight + weight1, -edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator);
                        return (new EdgePath<float>(edgePath.Vertex, edgePath.Weight, edge2, contractedPath)).ExpandLast(enumerator);
                    }
                    else
                    {
                        var contractedPath = new EdgePath<float>(contractedId.Value, edgePath.From.Weight + weight1, edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator);
                        return (new EdgePath<float>(edgePath.Vertex, edgePath.Weight, -edge2, contractedPath)).ExpandLast(enumerator);
                    }
                }
            }
            return edgePath;
        }

        /// <summary>
        /// Gets sequence 1, the first vertices right after the start vertex.
        /// </summary>
        public static uint[] GetSequence1(this EdgePath<float> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            return path.GetSequence1(enumerator, int.MaxValue);
        }

        /// <summary>
        /// Gets sequence 1, the first vertices right after the start vertex with a maximum of n.
        /// </summary>
        public static uint[] GetSequence1(this EdgePath<float> path, DirectedDynamicGraph.EdgeEnumerator enumerator, int n)
        {
            if (path.From == null)
            {
                return Constants.EMPTY_SEQUENCE;
            }

            var s = new List<uint>();
            s.Add(path.Vertex);
            while (true)
            {
                if (path.IsOriginal(enumerator))
                { // current segment is original.
                    if (s == null)
                    {
                        s = new List<uint>();
                    }
                    if (path.From.From != null)
                    { // we need more vertices and there are some more available.
                        s.Add(path.From.Vertex);
                        path = path.From;
                    }
                    else
                    { // we have enough.
                        var result = s.ToArray();
                        if (n < result.Length)
                        { // TODO: this can be way more efficient by creating only one array.
                            result = result.SubArray(result.Length - n, n);
                        }
                        result.Reverse();
                        return result;
                    }
                }
                else
                { // not an original edge, just return the start sequence.
                    var sequence = enumerator.GetSequence1();
                    if (path.From.From == null)
                    {
                        if (sequence.Length > n)
                        {
                            sequence = sequence.SubArray(sequence.Length - n, n);
                        }
                        return sequence;
                    }
                    s.Clear();
                    sequence.Reverse();
                    s.AddRange(sequence);
                    s.Add(path.From.Vertex);
                    path = path.From;
                }
            }
        }

        /// <summary>
        /// Gets sequence 2, the last vertices right before the end vertex.
        /// </summary>
        public static uint[] GetSequence2(this EdgePath<float> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            return path.GetSequence2(enumerator, int.MaxValue);
        }

        /// <summary>
        /// Gets sequence 2, the last vertices right before the end vertex with a maximum of n.
        /// </summary>
        public static uint[] GetSequence2(this EdgePath<float> path, DirectedDynamicGraph.EdgeEnumerator enumerator, int n)
        {
            if (path.From == null)
            {
                return Constants.EMPTY_SEQUENCE;
            }

            List<uint> s = null;
            while (true)
            {
                if (path.IsOriginal(enumerator))
                { // current segment is original.
                    if (s == null)
                    {
                        s = new List<uint>();
                    }
                    s.Add(path.From.Vertex);
                    if (s.Count < n && path.From.From != null)
                    { // we need more vertices and there are some more available.
                        path = path.From;
                    }
                    else
                    { // we have enough.
                        var result = s.ToArray();
                        result.Reverse();
                        return result;
                    }
                }
                else
                { // not an original edge, just return the start sequence.
                    return enumerator.GetSequence2();
                }
            }
        }

        /// <summary>
        /// Returns true if the last edge in this path is an original edge.
        /// </summary>
        public static bool IsOriginal(this EdgePath<float> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            if (path.From == null)
            { // when there is no previous vertex this is not an edge.
                throw new ArgumentException("The path is not an edge, cannot decide about originality.");
            }
            if (path.Edge == Constants.NO_EDGE)
            { // when there is no edge info, edge has to be original otherwise the info can never be recovered.
                return true;
            }
            enumerator.MoveToEdge(path.Edge);
            if (enumerator.IsOriginal())
            { // ok, edge is original.
                return true;
            }
            return false;
        }
    }
}