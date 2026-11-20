using System.Diagnostics;

namespace Recursividad.Core;

public sealed record MuestraHanoi(int Discos, long Movimientos, long Llamadas, int Profundidad, double MedianaMs, long BytesAsignados);
public static class MedicionHanoi
{
    public static int[] ParsearProfundidades(string texto)
    {
        var valores = EntradaArreglo.Parsear(texto).Distinct().Order().ToArray();
        if (valores.Length == 0 || valores.Length > 10 || valores.Any(n => n < 0 || n > 20))
            throw new ArgumentException("Escribe de 1 a 10 cantidades distintas de discos, entre 0 y 20.");
        return valores;
    }
    public static IReadOnlyList<MuestraHanoi> Ejecutar(IEnumerable<int> discos, CancellationToken token = default)
    {
        var entradas = discos.Distinct().Order().ToArray();
        if (entradas.Length == 0 || entradas.Length > 10 || entradas.Any(n => n < 0 || n > 20))
            throw new ArgumentException("Se admiten de 1 a 10 tamaños entre 0 y 20.");
        var salida = new List<MuestraHanoi>();
        foreach (int n in entradas)
        {
            token.ThrowIfCancellationRequested();
            for (int j = 0; j < 3; j++) Hanoi.Recursivo(n, false, token);
            var tiempos = new double[7]; var bytes = new long[7]; var reloj = new Stopwatch();
            DatosHanoi resultado = null!;
            for (int j = 0; j < 7; j++)
            {
                long antes = GC.GetAllocatedBytesForCurrentThread();
                reloj.Restart(); resultado = Hanoi.Recursivo(n, false, token); reloj.Stop();
                bytes[j] = GC.GetAllocatedBytesForCurrentThread() - antes;
                tiempos[j] = reloj.Elapsed.TotalMilliseconds;
            }
            Array.Sort(tiempos); Array.Sort(bytes);
            salida.Add(new(n, resultado.Movimientos, resultado.Llamadas, resultado.Profundidad, tiempos[3], bytes[3]));
        }
        return salida.AsReadOnly();
    }
}
