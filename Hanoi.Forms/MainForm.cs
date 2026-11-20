using Recursividad.Core;
using System.Globalization;
using System.Text;

namespace Hanoi.Forms;

public partial class MainForm : Form
{
    private EstadoHanoi? simulacion;
    private readonly System.Windows.Forms.Timer animador = new();
    private CancellationTokenSource? cancelacion;
    private IReadOnlyList<MuestraHanoi> ultimaMedicion = Array.Empty<MuestraHanoi>();
    public MainForm()
    {
        InitializeComponent();
        preparar.Click += (_, _) => Preparar();
        siguiente.Click += (_, _) => Avanzar();
        anterior.Click += (_, _) => { Detener(); simulacion?.Retroceder(); Actualizar(); };
        reiniciar.Click += (_, _) => { Detener(); simulacion?.Reiniciar(); Actualizar(); };
        reproducir.Click += (_, _) => {
            if (simulacion is null) Preparar();
            if (animador.Enabled) Detener();
            else if (simulacion is { Terminado: false }) { animador.Interval = (int)intervalo.Value; animador.Start(); reproducir.Text = "Pausar"; }
        };
        intervalo.ValueChanged += (_, _) => animador.Interval = (int)intervalo.Value;
        animador.Tick += (_, _) => Avanzar();
        medir.Click += async (_, _) => await MedirAsync();
        cancelar.Click += (_, _) => cancelacion?.Cancel();
        exportar.Click += (_, _) => Exportar();
        FormClosed += (_, _) => { animador.Dispose(); cancelacion?.Cancel(); };
        Actualizar();
    }
    private void Preparar()
    {
        Detener(); int n = (int)discos.Value;
        var solucion = Recursividad.Core.Hanoi.Recursivo(n);
        simulacion = new(n, solucion.Plan); movimientos.Rows.Clear();
        foreach (var par in solucion.Plan.Select((m, i) => (m, i)).Take(1000))
            movimientos.Rows.Add(par.i + 1, par.m.Disco, (char)('A' + par.m.Origen), (char)('A' + par.m.Destino));
        informacion.Text = $"Plan calculado: {solucion.Movimientos:N0} movimientos; {solucion.Llamadas:N0} llamadas; profundidad lógica {solucion.Profundidad}. Lista: primeros 1000 movimientos.";
        Actualizar();
    }
    private void Avanzar()
    {
        simulacion?.Avanzar();
        if (simulacion is null || simulacion.Terminado) Detener();
        Actualizar();
    }
    private void Detener() { animador.Stop(); reproducir.Text = "Reproducir"; }
    private void Actualizar()
    {
        tablero.Estado = simulacion; tablero.Invalidate();
        progreso.Text = simulacion is null ? "Elige la cantidad de discos y calcula el plan."
            : $"Discos del plan: {simulacion.Discos}. Movimiento {simulacion.Posicion:N0} de {simulacion.Total:N0}. {(simulacion.Terminado ? "Solución completada." : "Poste de destino: C.")}";
        siguiente.Enabled = simulacion is { Terminado: false };
        anterior.Enabled = simulacion is { Posicion: > 0 };
        if (simulacion is not null && simulacion.Posicion > 0 && simulacion.Posicion <= movimientos.Rows.Count)
        { movimientos.ClearSelection(); movimientos.Rows[simulacion.Posicion - 1].Selected = true; }
    }
    private async Task MedirAsync()
    {
        try
        {
            int[] tamanos = MedicionHanoi.ParsearProfundidades(profundidades.Text);
            cancelacion = new CancellationTokenSource(); medir.Enabled = false; cancelar.Enabled = true;
            exportar.Enabled = false; estadoMedicion.Text = "Midiendo en segundo plano, sin dibujar ni almacenar movimientos…";
            var salida = await Task.Run(() => MedicionHanoi.Ejecutar(tamanos, cancelacion.Token));
            if (IsDisposed) return;
            ultimaMedicion = salida; resultados.Rows.Clear();
            foreach (var r in salida) resultados.Rows.Add(r.Discos, r.Movimientos, r.Llamadas, r.Profundidad, r.MedianaMs.ToString("F4"), r.BytesAsignados);
            estadoMedicion.Text = "Medianas de 7 muestras tras 3 calentamientos. Bytes asignados al hilo: no incluyen el tamaño de la pila de llamadas.";
            exportar.Enabled = true;
        }
        catch (OperationCanceledException) { if (!IsDisposed) estadoMedicion.Text = "Medición cancelada; no se guardan resultados parciales."; }
        catch (ArgumentException ex) { MessageBox.Show(this, ex.Message, "Revisa los tamaños"); }
        finally
        {
            cancelacion?.Dispose(); cancelacion = null;
            if (!IsDisposed) { medir.Enabled = true; cancelar.Enabled = false; exportar.Enabled = ultimaMedicion.Count > 0; }
        }
    }
    private void Exportar()
    {
        if (ultimaMedicion.Count == 0) return;
        using var dialogo = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "mediciones_hanoi.csv" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var lineas = new List<string> { "discos,movimientos,llamadas,profundidad,mediana_ms,bytes_asignados_hilo" };
            lineas.AddRange(ultimaMedicion.Select(r => FormattableString.Invariant($"{r.Discos},{r.Movimientos},{r.Llamadas},{r.Profundidad},{r.MedianaMs:F6},{r.BytesAsignados}")));
            File.WriteAllLines(dialogo.FileName, lineas, new UTF8Encoding(true));
            estadoMedicion.Text = "Resultados exportados al archivo elegido.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { MessageBox.Show(this, ex.Message, "No se pudo guardar"); }
    }
}
