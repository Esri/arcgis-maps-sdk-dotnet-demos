using System;
using System.Globalization;
using System.Windows.Data;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.Calcite.WPF;

namespace RoutingSample.Desktop
{
    public class ManeuverToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DirectionManeuverType m)
            {
                switch(m)
                {
                    case DirectionManeuverType.Stop: return (char)(ushort)CalciteIcon.Stop;
                    case DirectionManeuverType.Straight: return (char)(ushort)CalciteIcon.Straight;
                    case DirectionManeuverType.BearLeft: return (char)(ushort)CalciteIcon.BearLeft;
                    case DirectionManeuverType.BearRight: return (char)(ushort)CalciteIcon.BearRight;
                    case DirectionManeuverType.TurnLeft: return (char)(ushort)CalciteIcon.Left;
                    case DirectionManeuverType.TurnRight: return (char)(ushort)CalciteIcon.Right;
                    case DirectionManeuverType.SharpLeft: return (char)(ushort)CalciteIcon.SharpLeft;
                    case DirectionManeuverType.SharpRight: return (char)(ushort)CalciteIcon.SharpRight;
                    case DirectionManeuverType.UTurn: return (char)(ushort)CalciteIcon.UTurn;
                    case DirectionManeuverType.Roundabout: return (char)(ushort)CalciteIcon.RoundAbout;
                    case DirectionManeuverType.HighwayMerge: return (char)(ushort)CalciteIcon.MergeOnHighway;
                    case DirectionManeuverType.HighwayExit: return (char)(ushort)CalciteIcon.ExitHighwayRight;
                    case DirectionManeuverType.HighwayChange: return (char)(ushort)CalciteIcon.HighwayChange;
                    case DirectionManeuverType.ForkCenter: return (char)(ushort)CalciteIcon.ForkMiddle;
                    case DirectionManeuverType.ForkLeft: return (char)(ushort)CalciteIcon.ForkLeft;
                    case DirectionManeuverType.ForkRight: return (char)(ushort)CalciteIcon.ForkRight;
                    case DirectionManeuverType.Depart: return (char)(ushort)CalciteIcon.ChevronStart;
                    case DirectionManeuverType.RampRight: return (char)(ushort)CalciteIcon.RampRight;
                    case DirectionManeuverType.RampLeft: return (char)(ushort)CalciteIcon.RampLeft;
                    case DirectionManeuverType.TurnLeftRight: return (char)(ushort)CalciteIcon.LeftRight;
                    case DirectionManeuverType.TurnRightLeft: return (char)(ushort)CalciteIcon.RightLeft;
                    case DirectionManeuverType.TurnRightRight: return (char)(ushort)CalciteIcon.RightRight;
                    case DirectionManeuverType.TurnLeftLeft: return (char)(ushort)CalciteIcon.LeftLeft;
                    case DirectionManeuverType.PedestrianRamp: return (char)(ushort)CalciteIcon.TakePedestrianRamp;
                    case DirectionManeuverType.Elevator: return (char)(ushort)CalciteIcon.Elevator;
                    case DirectionManeuverType.Escalator: return (char)(ushort)CalciteIcon.Escalator;
                    case DirectionManeuverType.Stairs: return (char)(ushort)CalciteIcon.Stairs;
                    case DirectionManeuverType.DoorPassage: return (char)(ushort)CalciteIcon.WalkThroughDoor;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
