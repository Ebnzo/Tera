﻿namespace Tera.Game
{
    public struct Vector3f
    {
        public float X;
        public float Y;
        public float Z;

        public override string ToString()
        {
            return $"({X},{Y},{Z})";
        }
    }
}