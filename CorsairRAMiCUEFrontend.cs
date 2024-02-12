using RGB.NET.Core;
using RGB.NET.Devices.Corsair;

class CorsairRAMiCUEFrontend : IFrontend
{
    class Screen(Led[][] leds) : IScreen
    {
        public int Width => leds.Length;
        public int Height => leds[0].Length;

        public void SetColor((int X, int Y) position, (float R, float G, float B) color)
        {
            leds[position.X][position.Y].Color = new(color.R, color.G, color.B);
        }

        public void Flush() { }
    }

    public string Name => "Corsair iCUE RAM";

    private readonly RGBSurface _surface;

    public CorsairRAMiCUEFrontend()
    {
        _surface = new RGBSurface();
    }

    public IScreen GetScreen()
    {
        Console.WriteLine(" [*] iCUE Initialization...");
        _surface.Load(CorsairDeviceProvider.Instance);
        _surface.RegisterUpdateTrigger(new TimerUpdateTrigger());
        _surface.AlignDevices();

        Console.WriteLine($" [*] Found {_surface.Leds.Count()} LEDs");
        foreach (var led in _surface.Leds)
        {
#if DEBUG
            Console.WriteLine($"[debug] Found LED {led.Device.DeviceInfo.Manufacturer} {led.Device.DeviceInfo.Model}:");
            Console.WriteLine($"[debug]   location: {led.Location}, size: {led.Size}, type: {led.Device.DeviceInfo.DeviceType}");
#endif
            led.Color = new Color(0.0f, 1.0f, 0.0f);
        }

        var ramLeds = _surface.Leds.Where(led => led.Device.DeviceInfo.DeviceType == RGBDeviceType.DRAM);
        var groups = ramLeds.GroupBy(led => led.Device).Select(group => group.ToArray()).ToArray();

        // Sanity checks
        if (groups.Length == 0)
        {
            throw new Exception("Can't find any Corsair iCUE LEDs!");
        }
        if (ramLeds.DistinctBy(led => led.Size).Count() > 1)
        {
            throw new Exception("LED area mismatch!");
        }
        if (groups.Length < 2)
        {
            throw new Exception($"Not enough RAM sticks ({groups.Length} < 2) or RAM stick detection failed");
        }
        if (groups.DistinctBy(group => group.Length).Count() > 1)
        {
            throw new Exception("LED count mismatch!");
        }
        if (groups[0].Length < 4)
        {
            throw new Exception($"Not enough LEDs per RAM stick ({groups.Length} < 4)!");
        }
        //

        Console.WriteLine($" [*] Found {groups.Length} RAM sticks with {groups[0].Length} LEDs per stick");

        return new Screen(groups);
    }

    public void Dispose()
    {
        _surface.Dispose();
    }
}
