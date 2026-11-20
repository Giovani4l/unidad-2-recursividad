using Recursividad.Core;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
string destino = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Environment.CurrentDirectory, "evidencias");
Directory.CreateDirectory(destino);
int aprobadas = 0;
void Check(bool ok, string caso)
{
    if (!ok) throw new InvalidOperationException("FALLO: " + caso);
    Console.WriteLine("PASS " + caso); aprobadas++;
}
void Rechaza(Action accion, string caso)
{
    try { accion(); }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OperationCanceledException)
    { Check(true, caso); return; }
    throw new InvalidOperationException("No se rechazó: " + caso);
}
foreach (int n in Enumerable.Range(0, 21))
{
    var r = Factorial.Recursivo(n);
    Check(r.Valor == Factorial.Iterativo(n) && r.Llamadas == Math.Max(1, n) && r.Profundidad == Math.Max(1, n), $"factorial {n}, resultado y métricas");
}
Check(Factorial.Recursivo(0).Valor == 1 && Factorial.Recursivo(5).Valor == 120
    && Factorial.Recursivo(20).Valor == 2432902008176640000L, "factoriales conocidos independientes");
foreach (int n in Enumerable.Range(0, 16))
{
    var r = Fibonacci.Recursivo(n);
    Check(r.Valor == Fibonacci.Iterativo(n) && r.Llamadas == 2 * Fibonacci.Iterativo(n + 1) - 1
        && r.Profundidad == Math.Max(1, n), $"Fibonacci {n}, resultado, llamadas y profundidad");
}
Check(Fibonacci.Recursivo(30, 0).Valor == 832040, "Fibonacci 30 conocido");
var random = new Random(24050379);
for (int longitud = 0; longitud <= 24; longitud++)
{
    int[] a = Enumerable.Range(0, longitud).Select(_ => random.Next(-20, 21)).ToArray();
    var suma = SumaArreglo.Recursivo(a);
    Check(suma.Valor == a.Sum(x => (long)x) && suma.Valor == SumaArreglo.Iterativo(a)
        && suma.Profundidad == longitud + 1 && suma.Llamadas == longitud + 1, $"suma de {longitud} elementos");
    int x = longitud > 0 ? a[longitud / 2] : 100;
    var busqueda = BusquedaLineal.Recursivo(a, x);
    Check(busqueda.Valor == Array.IndexOf(a, x) && busqueda.Valor == BusquedaLineal.Iterativo(a, x), $"búsqueda en longitud {longitud}");
    var ausente = BusquedaLineal.Recursivo(a, 100);
    Check(ausente.Valor == -1 && ausente.Llamadas == longitud + 1, $"ausencia en longitud {longitud}");
}
Check(BusquedaLineal.Recursivo(new[] { 4, 9, 9, 1 }, 9).Valor == 1, "primera coincidencia entre duplicados");
Check(SumaArreglo.Recursivo(Enumerable.Repeat(int.MaxValue, 500).ToArray(), 0).Valor == 1073741823500L, "suma de 500 enteros sin desbordar Int32");
Check(EntradaArreglo.Parsear("3, -2; 7\n9").SequenceEqual(new[] { 3, -2, 7, 9 }), "entrada con separadores y negativos");
Check(EntradaArreglo.Parsear("").Length == 0, "arreglo vacío válido");
foreach (int n in Enumerable.Range(0, 11))
{
    var r = Hanoi.Recursivo(n);
    Check(r.Plan.SequenceEqual(Hanoi.Iterativo(n)) && r.Movimientos == (1L << n) - 1
        && r.Llamadas == (1L << (n + 1)) - 1 && r.Profundidad == n + 1, $"Hanoi {n}: secuencia y métricas");
    var estado = new EstadoHanoi(n, r.Plan);
    while (!estado.Terminado) estado.Avanzar();
    Check(estado.Torres[2].SequenceEqual(Enumerable.Range(1, n)) && estado.Torres[0].Length == 0, $"Hanoi {n}: destino correcto y movimientos legales");
    while (estado.Posicion > 0) estado.Retroceder();
    Check(estado.Torres[0].SequenceEqual(Enumerable.Range(1, n)), $"Hanoi {n}: deshacer todos los movimientos");
}
Rechaza(() => Factorial.Recursivo(-1), "factorial negativo");
Rechaza(() => Factorial.Iterativo(21), "factorial fuera de Int64");
Rechaza(() => Fibonacci.Recursivo(31), "límite de Fibonacci directo");
Rechaza(() => EntradaArreglo.Parsear("1, texto"), "token inválido en arreglo");
Rechaza(() => SumaArreglo.Recursivo(new int[501]), "profundidad de arreglo acotada");
Rechaza(() => Hanoi.Recursivo(13, true), "plan demasiado grande para simulación");
Rechaza(() => Hanoi.Recursivo(21, false), "medición fuera de límite");
Rechaza(() => Hanoi.Recursivo(3, false, new CancellationToken(true)), "cancelación antes de recorrer Hanoi");
Rechaza(() => MedicionHanoi.ParsearProfundidades(""), "medición sin tamaños");
Rechaza(() => MedicionHanoi.ParsearProfundidades("2,21"), "tamaño inválido de medición");
Check(MedicionHanoi.ParsearProfundidades("8,4,4,0").SequenceEqual(new[] { 0, 4, 8 }), "normalización de tamaños de medición");
var ilegal = new EstadoHanoi(2, new[] { new Movimiento(2, 0, 2) });
Rechaza(ilegal.Avanzar, "simulador rechaza mover disco que no está en el tope");
var muestras = MedicionHanoi.Ejecutar(new[] { 0, 1, 4, 8, 12, 16, 20 });
Check(muestras.All(m => m.Movimientos == (1L << m.Discos) - 1 && m.Llamadas == (1L << (m.Discos + 1)) - 1
    && m.Profundidad == m.Discos + 1 && m.MedianaMs >= 0 && m.BytesAsignados > 0), "mediciones reales y contadores coherentes");
string LeerSdk()
{
    try
    {
        using var proceso = Process.Start(new ProcessStartInfo("dotnet", "--version")
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true });
        string version = proceso?.StandardOutput.ReadToEnd().Trim() ?? "no disponible";
        proceso?.WaitForExit();
        return version;
    }
    catch (System.ComponentModel.Win32Exception) { return "no disponible"; }
}
var resumen = new { Aprobadas = aprobadas, Runtime = RuntimeInformation.FrameworkDescription,
    SO = RuntimeInformation.OSDescription, Arquitectura = RuntimeInformation.ProcessArchitecture.ToString(),
    SDK = LeerSdk(), Metodo = "Release; 3 calentamientos; mediana de 7 muestras; sin guardar movimientos; incluye instrumentación y control de cancelación", Muestras = muestras };
File.WriteAllText(Path.Combine(destino, "resultados.json"), JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true }));
var csv = new List<string> { "discos,movimientos,llamadas,profundidad,mediana_ms,bytes_asignados_hilo" };
csv.AddRange(muestras.Select(m => FormattableString.Invariant($"{m.Discos},{m.Movimientos},{m.Llamadas},{m.Profundidad},{m.MedianaMs:F6},{m.BytesAsignados}")));
File.WriteAllLines(Path.Combine(destino, "mediciones_hanoi.csv"), csv);
Console.WriteLine($"RESULTADO: {aprobadas} verificaciones aprobadas");
