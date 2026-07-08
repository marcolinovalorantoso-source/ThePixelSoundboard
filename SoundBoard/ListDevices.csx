#r "nuget: NAudio, 2.2.1"

using NAudio.Wave;

Console.WriteLine("=== Device Audio Output Disponibili ===\n");
Console.WriteLine("Total devices: " + WaveOut.DeviceCount);
for (int i = 0; i < WaveOut.DeviceCount; i++)
{
    var caps = WaveOut.GetCapabilities(i);
    Console.WriteLine($"Index {i}: {caps.ProductName}");
}
Console.WriteLine("\n=== Copia gli indici da usare in SoundBoard ===");
