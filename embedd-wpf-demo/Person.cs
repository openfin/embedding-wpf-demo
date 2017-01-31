using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace embedd_wpf_demo
{
    class Person
    {
        private const int Precision = 2;
        private double _income;
        private double _travel;

        public string First { get; set; }
        public string Last { get; set; }
        public int Pets { get; set; }
        public string BirthDate { get; set; }
        public string ResidenceState { get; set; }
        public string BirthState { get; set; }
        public bool Employed { get; set; }
        public double Income
        {
            get { return Math.Round(_income, PRECISION); }
            set { _income = value; }
        }
        public double Travel
        {
            get { return Math.Round(_travel, PRECISION); }
            set { _travel = value; }
        }
    }
}
