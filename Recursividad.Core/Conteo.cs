namespace Recursividad.Core;

public sealed record EntradaTraza(long Llamada, int Profundidad, string Operacion);
public sealed record Resultado<T>(T Valor, long Llamadas, int Profundidad, IReadOnlyList<EntradaTraza> Traza);

/// <summary>Cuenta entradas al algoritmo, incluido el caso base; no mide bytes de la pila.</summary>
internal sealed class Conteo(int limiteTraza = 0)
{
    public long Llamadas { get; private set; }
    public int Profundidad { get; private set; }
    public List<EntradaTraza> Traza { get; } = new();
    public void Entrar(string nombre, int argumento, int profundidad)
    {
        Llamadas++;
        Profundidad = Math.Max(Profundidad, profundidad);
        if (Traza.Count < limiteTraza)
            Traza.Add(new(Llamadas, profundidad, $"{nombre}({argumento})"));
    }
    public Resultado<T> Finalizar<T>(T valor) => new(valor, Llamadas, Profundidad, Traza.AsReadOnly());
}
