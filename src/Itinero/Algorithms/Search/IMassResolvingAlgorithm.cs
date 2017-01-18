﻿// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
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

using System.Collections.Generic;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// Abstract representation of a mass resolving algorithm.
    /// </summary>
    public interface IMassResolvingAlgorithm : IAlgorithm
    {
        /// <summary>
        /// Returns the errors indexed per location idx.
        /// </summary>
        Dictionary<int, LocationError> Errors { get; }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        List<RouterPoint> RouterPoints { get; }
        
        /// <summary>
        /// Returns the index of the location in the resolved points list.
        /// </summary>
        /// <returns></returns>
        int IndexOf(int locationIdx);

        /// <summary>
        /// Returns the index of the router point in the original locations array.
        /// </summary>
        /// <returns></returns>
        int LocationIndexOf(int routerPointIdx);
    }
}