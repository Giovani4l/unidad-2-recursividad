namespace Recursividad.Core;

public readonly record struct Movimiento(int Disco, int Origen, int Destino)
{
    public override string ToString() => $"Disco {Disco}: {(char)('A' + Origen)} -> {(char)('A' + Destino)}";
}
public sealed record DatosHanoi(int Discos, long Movimientos, long Llamadas, int Profundidad, IReadOnlyList<Movimiento> Plan);

public static class Hanoi
{
    private static void Validar(int n, bool guardar)
    {
        if (n < 0 || n > (guardar ? 12 : 20))
            throw new ArgumentOutOfRangeException(nameof(n), guardar ? "La simulación admite de 0 a 12 discos." : "La medición admite de 0 a 20 discos.");
    }
    public static DatosHanoi Recursivo(int n, bool guardar = true, CancellationToken token = default)
    {
        Validar(n, guardar);
        var plan = guardar ? new List<Movimiento>((1 << n) - 1) : null;
        long movimientos = 0, llamadas = 0; int maxima = 0;
        void Mover(int cantidad, int origen, int auxiliar, int destino, int profundidad)
        {
            token.ThrowIfCancellationRequested();
            llamadas++; maxima = Math.Max(maxima, profundidad);
            if (cantidad == 0) return; // Caso base: ninguna pieza que mover.
            Mover(cantidad - 1, origen, destino, auxiliar, profundidad + 1);
            movimientos++;
            plan?.Add(new(cantidad, origen, destino));
            Mover(cantidad - 1, auxiliar, origen, destino, profundidad + 1);
        }
        Mover(n, 0, 1, 2, 1);
        return new(n, movimientos, llamadas, maxima, plan is null ? Array.Empty<Movimiento>() : plan.AsReadOnly());
    }
    public static IReadOnlyList<Movimiento> Iterativo(int n)
    {
        Validar(n, true);
        var plan = new List<Movimiento>((1 << n) - 1);
        var pendientes = new Stack<(int N, int Origen, int Auxiliar, int Destino, bool Emitir)>();
        pendientes.Push((n, 0, 1, 2, false));
        while (pendientes.Count > 0)
        {
            var paso = pendientes.Pop();
            if (paso.Emitir) { plan.Add(new(paso.N, paso.Origen, paso.Destino)); continue; }
            if (paso.N == 0) continue;
            // Apilar en orden inverso, porque la pila es LIFO.
            pendientes.Push((paso.N - 1, paso.Auxiliar, paso.Origen, paso.Destino, false));
            pendientes.Push((paso.N, paso.Origen, paso.Auxiliar, paso.Destino, true));
            pendientes.Push((paso.N - 1, paso.Origen, paso.Destino, paso.Auxiliar, false));
        }
        return plan.AsReadOnly();
    }
}

/// <summary>Estado de la simulación. Cada avance verifica las reglas de Hanoi.</summary>
public sealed class EstadoHanoi
{
    private readonly IReadOnlyList<Movimiento> plan;
    private Stack<int>[] torres = new Stack<int>[3];
    public int Discos { get; }
    public int Posicion { get; private set; }
    public int Total => plan.Count;
    public bool Terminado => Posicion == Total;
    public int[][] Torres => torres.Select(t => t.ToArray()).ToArray();
    public EstadoHanoi(int discos, IReadOnlyList<Movimiento> movimientos)
    {
        if (discos < 0 || discos > 12) throw new ArgumentOutOfRangeException(nameof(discos));
        ArgumentNullException.ThrowIfNull(movimientos);
        Discos = discos; plan = movimientos; Reiniciar();
    }
    public void Reiniciar()
    {
        torres = new[] { new Stack<int>(), new Stack<int>(), new Stack<int>() };
        for (int disco = Discos; disco >= 1; disco--) torres[0].Push(disco);
        Posicion = 0;
    }
    private void Aplicar(Movimiento m)
    {
        if (m.Origen < 0 || m.Origen > 2 || m.Destino < 0 || m.Destino > 2 || m.Origen == m.Destino)
            throw new InvalidOperationException("Movimiento con postes inválidos.");
        if (torres[m.Origen].Count == 0 || torres[m.Origen].Peek() != m.Disco
            || (torres[m.Destino].Count > 0 && torres[m.Destino].Peek() < m.Disco))
            throw new InvalidOperationException("Movimiento contrario a las reglas de Hanoi.");
        torres[m.Destino].Push(torres[m.Origen].Pop());
    }
    public void Avanzar()
    {
        if (Terminado) return;
        Aplicar(plan[Posicion]); Posicion++;
    }
    public void Retroceder()
    {
        if (Posicion == 0) return;
        var m = plan[Posicion - 1]; Aplicar(new(m.Disco, m.Destino, m.Origen)); Posicion--;
    }
}
