using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
    namespace PathFinding
    {
        //public class LocationData<T>
        //{
        //    public T Location { get; set; }
        //    public bool IsWalkable { get; set; }
        //    public float Cost { get; set; }
        //    public LocationData()
        //    { }
        //}
        /// <summary>
        /// A class that represents the AI map for path finding.
        /// This map is different from what you see visually.
        /// This map stores the map information required for path finding
        /// </summary>
        public interface IMap<T>
        {
            //public Map()
            //{ }

            // A map should just comprise a set of locations
            //protected Dictionary<T, LocationData<T>> mLocations = new Dictionary<T, LocationData<T>>();

            List<T> GetNeighbours(T loc);
            //public LocationData<T> GetLocationData(T key)
            //{
            //    return mLocations[key];
            //}
        }
    }
}