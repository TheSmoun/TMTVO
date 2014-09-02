using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class SessionsModule : Module
    {
        public Track Track { get; private set; }
        public Weather Weather
        {
            get
            {
                return Track.Weather;
            }
        }

        private WeatherWidget weatherWidget;

        public SessionsModule(WeatherWidget weatherWidget) : base("Sessions")
        {
            this.weatherWidget = weatherWidget;
            this.weatherWidget.Module = this;

            Track = new Track();
            Track.Weather = new Weather();
            Track.Sectors = new List<float>();
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            ConfigurationSection weekendInfo = rootNode.GetConfigurationSection("WeekendInfo");
            ConfigurationSection weekendOptions = weekendInfo.GetConfigurationSection("WeekendOptions");

            Track.Name = weekendInfo.GetString("TrackName");
            Track.Id = int.Parse(weekendInfo.GetString("TrackID"));
            string length = weekendInfo.GetString("TrackLength");
            Track.Length = float.Parse(length.Substring(0, length.Length - 3).Replace('.', ',')) * 1000;
            Track.DisplayName = weekendInfo.GetString("TrackDisplayName");
            Track.DisplayShortName = weekendInfo.GetString("TrackDisplayShortName");
            Track.City = weekendInfo.GetString("TrackCity");
            Track.Country = weekendInfo.GetString("TrackCountry");
            string alt = weekendInfo.GetString("TrackAltitude");
            Track.Altitude = float.Parse(alt.Substring(0, alt.Length - 2).Replace('.', ','));
            string lat = weekendInfo.GetString("TrackLatitude");
            Track.Latitude = float.Parse(lat.Substring(0, lat.Length - 2).Replace('.', ','));
            string lon = weekendInfo.GetString("TrackLongitude");
            Track.Longitude = float.Parse(lon.Substring(0, lon.Length - 2).Replace('.', ','));
            Track.NumberOfTurns = int.Parse(weekendInfo.GetString("TrackNumTurns"));
            string pl = weekendInfo.GetString("TrackPitSpeedLimit");
            Track.PitSpeedLimit = float.Parse(pl.Substring(0, pl.Length - 4).Replace('.', ','));
            string trackType = weekendInfo.GetString("TrackType");
            if (trackType.StartsWith("road"))
                Track.TrackType = TrackType.Road;
            else if (trackType.StartsWith("oval"))
                Track.TrackType = TrackType.Oval;
            else
                Track.TrackType = TrackType.None;

            Weather.Skies = (Skies)Enum.Parse(typeof(Skies), weekendInfo.GetString("TrackSkies").Replace(" ", ""));
            string airTemp = weekendInfo.GetString("TrackAirTemp");
            Weather.AirTemp = float.Parse(airTemp.Substring(0, airTemp.Length - 2).Replace('.', ','));
            string trackTemp = weekendInfo.GetString("TrackSurfaceTemp");
            Weather.TrackTemp = float.Parse(trackTemp.Substring(0, trackTemp.Length - 2).Replace('.', ','));
            string windSpeed = weekendInfo.GetString("TrackWindVel");
            Weather.WindSpeed = float.Parse(windSpeed.Substring(0, windSpeed.Length - 4).Replace('.', ','));
            string humidity = weekendOptions.GetString("RelativeHumidity");
            Weather.Humidity = int.Parse(humidity.Substring(0, humidity.Length - 2));

            Track.Sectors.Clear();
            ConfigurationSection splitTime = rootNode.GetConfigurationSection("SplitTimeInfo");
            foreach (Dictionary<string, object> dict in splitTime.GetMapList("Sectors"))
                Track.Sectors.Add(float.Parse(dict.GetDictValue("SectorStartPct").Replace('.', ',')));
        }

        public override void Reset()
        {
            Track = null;
            Track = new Track();
            Track.Weather = new Weather();
            Track.Sectors = new List<float>();
        }
    }
}
