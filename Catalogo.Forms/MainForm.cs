using Recursividad.Core;
namespace Catalogo.Forms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        ejercicio.Items.AddRange(new object[] { "Factorial", "Fibonacci", "Torres de Hanoi", "Suma de arreglo", "Búsqueda lineal" });
        ejercicio.SelectedIndexChanged += (_, _) => CambiarEjercicio();
        ejercicio.SelectedIndex = 0;
        ejecutar.Click += async (_, _) => await EjecutarAsync();
    }
    private void CambiarEjercicio()
    {
        int opcion = ejercicio.SelectedIndex;
        numero.Enabled = opcion < 3; arreglo.Enabled = opcion >= 3; objetivo.Enabled = opcion == 4;
        descripcion.Text = opcion switch
        {
            0 => "n: 0 a 20. Base: n <= 1 devuelve 1. Paso: n × factorial(n-1).",
            1 => "n: 0 a 30. Bases: F(0)=0 y F(1)=1. Paso: F(n-1)+F(n-2); versión directa sin memoización.",
            2 => "n: 0 a 12. Base: 0 discos. Paso: mover n-1, mover el disco n y mover los n-1 restantes.",
            3 => "Hasta 500 enteros. Base: índice == longitud devuelve 0. Paso: valor actual + suma del sufijo.",
            _ => "Hasta 500 enteros. Bases: fin del arreglo o coincidencia. Devuelve la primera posición, o -1."
        };
        resultados.Rows.Clear(); traza.Rows.Clear(); estado.Text = "Ingresa los datos y pulsa Resolver y comparar.";
    }
    private async Task EjecutarAsync()
    {
        ejecutar.Enabled = false; ejercicio.Enabled = false;
        numero.Enabled = false; arreglo.Enabled = false; objetivo.Enabled = false;
        try
        {
            int opcion = ejercicio.SelectedIndex, n = (int)numero.Value, x = (int)objetivo.Value;
            int[] valores = opcion >= 3 ? EntradaArreglo.Parsear(arreglo.Text) : Array.Empty<int>();
            var salida = await Task.Run(() => Calcular(opcion, n, valores, x));
            resultados.Rows.Clear(); traza.Rows.Clear();
            resultados.Rows.Add("Iterativo", salida.Iterativo, "—", "—");
            resultados.Rows.Add("Recursivo", salida.Recursivo, salida.Llamadas, salida.Profundidad);
            foreach (var entrada in salida.Traza) traza.Rows.Add(entrada.Llamada, entrada.Profundidad == 0 ? "—" : entrada.Profundidad, entrada.Operacion);
            estado.Text = salida.Iterativo == salida.Recursivo
                ? $"Resultados coincidentes. Llamadas: {salida.Llamadas:N0}; profundidad lógica máxima: {salida.Profundidad}."
                : "Los resultados difieren: revisa la implementación.";
            detalle.Text = opcion == 2 ? "Se muestran hasta 200 movimientos. Cada uno se calcula; no se almacena una respuesta predeterminada."
                : "Se muestran las primeras 200 entradas como máximo. El conteo total incluye los casos base, aunque la traza se recorte.";
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException)
        { MessageBox.Show(this, ex.Message, "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        finally
        {
            ejecutar.Enabled = true; ejercicio.Enabled = true;
            numero.Enabled = ejercicio.SelectedIndex < 3; arreglo.Enabled = ejercicio.SelectedIndex >= 3;
            objetivo.Enabled = ejercicio.SelectedIndex == 4;
        }
    }
    private sealed record Salida(string Iterativo, string Recursivo, long Llamadas, int Profundidad, IReadOnlyList<EntradaTraza> Traza);
    private static Salida Calcular(int opcion, int n, int[] valores, int x)
    {
        if (opcion == 2)
        {
            var iterativo = Recursividad.Core.Hanoi.Iterativo(n);
            var recursivo = Recursividad.Core.Hanoi.Recursivo(n);
            if (!iterativo.SequenceEqual(recursivo.Plan)) throw new InvalidOperationException("Los planes de Hanoi no coinciden.");
            return new($"{iterativo.Count} movimientos", $"{recursivo.Movimientos} movimientos", recursivo.Llamadas,
                recursivo.Profundidad, recursivo.Plan.Take(200).Select((m, i) => new EntradaTraza(i + 1, 0, m.ToString())).ToArray());
        }
        if (opcion == 4)
        {
            int iterativo = BusquedaLineal.Iterativo(valores, x); var recursivo = BusquedaLineal.Recursivo(valores, x);
            return new(iterativo.ToString(), recursivo.Valor.ToString(), recursivo.Llamadas, recursivo.Profundidad, recursivo.Traza);
        }
        long valorIterativo; Resultado<long> valorRecursivo;
        if (opcion == 0) { valorIterativo = Factorial.Iterativo(n); valorRecursivo = Factorial.Recursivo(n); }
        else if (opcion == 1) { valorIterativo = Fibonacci.Iterativo(n); valorRecursivo = Fibonacci.Recursivo(n); }
        else { valorIterativo = SumaArreglo.Iterativo(valores); valorRecursivo = SumaArreglo.Recursivo(valores); }
        return new(valorIterativo.ToString(), valorRecursivo.Valor.ToString(), valorRecursivo.Llamadas, valorRecursivo.Profundidad, valorRecursivo.Traza);
    }
}
