using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class WattsUtils
    {
        public static Dictionary<WattType, long> WattTypeValue = new()
        {
            {WattType.Watt, 1 },
            {WattType.Kilowatt, 1_000 },
            {WattType.Megawatt, 1_000_000 },
            {WattType.Gigawatt, 1_000_000_000 }
        };

        [Serializable]
        public record Watt
        {
            public WattType WattType;
            public double Value;

            public Watt(WattType wattType, double value)
            {
                WattType = wattType;
                Value = value;
            }
        }

        public static Watt ConvertWatt(Watt input, WattType targetType)
        {
            if (input.WattType == targetType)
            {
                return input;
            }

            double inputValueInWatts = input.Value * WattTypeValue[input.WattType];
            double outputValue = inputValueInWatts / WattTypeValue[targetType];

            if (outputValue > double.MaxValue || outputValue < double.MinValue)
            {
                throw new OverflowException("Conversion result is outside the range of an int.");
            }

            return new Watt(targetType, outputValue);
        }

        public static int CompareWatts(Watt watt1, Watt watt2)
        {
            double watts1 = watt1.Value * WattTypeValue[watt1.WattType];
            double watts2 = watt2.Value * WattTypeValue[watt2.WattType];

            return watts1.CompareTo(watts2);
        }

        public static Watt SumWatts(IEnumerable<Watt> watts, WattType resultType = WattType.Watt)
        {
            double totalWatts = 0;

            foreach (var watt in watts)
            {
                totalWatts += (double)watt.Value * WattTypeValue[watt.WattType];
            }

            double resultValue = totalWatts / WattTypeValue[resultType];

            if (resultValue > double.MaxValue || resultValue < double.MinValue)
            {
                throw new OverflowException("Sum result is outside the range of an int.");
            }

            return new Watt(resultType, resultValue);
        }

        public static double ConvertToUnit(Watt watt, WattType targetUnit)
        {
            if (watt.WattType == targetUnit)
                return watt.Value;

            double conversionFactor = WattTypeValue[targetUnit] / WattTypeValue[watt.WattType];
            return watt.Value / conversionFactor;
        }

        public static WattType GetHighestUnit(WattType unit1, WattType unit2)
        {
            return (WattType)Math.Max((int)unit1, (int)unit2);
        }

        public static Watt SumWatts(IEnumerable<Watt> watts)
        {
            double totalWatts = 0;

            foreach (var watt in watts)
            {
                totalWatts += (double)watt.Value * WattTypeValue[watt.WattType];
            }

            WattType appropriateType = DetermineAppropriateWattType(totalWatts);
            double resultValue = totalWatts / WattTypeValue[appropriateType];

            if (resultValue > double.MaxValue || resultValue < double.MinValue)
            {
                throw new OverflowException("Sum result is outside the range of an int.");
            }

            return new Watt(appropriateType, resultValue);
        }

        private static WattType DetermineAppropriateWattType(double watts)
        {
            if (watts >= WattTypeValue[WattType.Gigawatt])
                return WattType.Gigawatt;
            if (watts >= WattTypeValue[WattType.Megawatt])
                return WattType.Megawatt;
            if (watts >= WattTypeValue[WattType.Kilowatt])
                return WattType.Kilowatt;
            return WattType.Watt;
        }

        public enum WattType
        {
            Watt = 0,
            Kilowatt = 1,
            Megawatt = 2,
            Gigawatt = 3
        }
    }
}
