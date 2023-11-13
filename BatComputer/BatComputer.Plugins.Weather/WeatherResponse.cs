using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatComputer.Plugins.Weather;

public class WeatherResponse
{
    public CurrentWeather? Current { get; set; }
    public DailyWeather? Daily { get; set; }
}