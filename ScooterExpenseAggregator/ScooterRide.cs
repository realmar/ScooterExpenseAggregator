using System.Collections.Specialized;
using Microsoft.VisualBasic.CompilerServices;

namespace Realmar.ScooterExpenseAggregator
{
    public struct ScooterRide
    {
        public double Distance;
        public double Price;
        public double Time;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Price: {Price} Distance: {Distance} Time: {Time}";
        }

        public static ScooterRide operator +(ScooterRide a) => a;

        public static ScooterRide operator -(ScooterRide a)
            => new ScooterRide
            {
                Distance = -a.Distance,
                Price = -a.Price,
                Time = -a.Time
            };

        public static ScooterRide operator +(ScooterRide a, ScooterRide b)
            => new ScooterRide
            {
                Distance = a.Distance + b.Distance,
                Price = a.Price + b.Price,
                Time = a.Time + b.Time
            };

        public static ScooterRide operator -(ScooterRide a, ScooterRide b) => -a + b;
    }
}