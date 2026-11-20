namespace Recursividad.Core;

public static class Fibonacci
{
    private static void Validar(int n)
    {
        // Se conserva la versión recursiva directa para analizar sus repeticiones.
        if (n < 0 || n > 30) throw new ArgumentOutOfRangeException(nameof(n), "Fibonacci directo admite n de 0 a 30.");
    }
    public static long Iterativo(int n)
    {
        Validar(n);
        long anterior = 0, actual = 1;
        for (int i = 0; i < n; i++) (anterior, actual) = (actual, checked(anterior + actual));
        return anterior;
    }
    public static Resultado<long> Recursivo(int n, int limiteTraza = 200)
    {
        Validar(n);
        var conteo = new Conteo(limiteTraza);
        return conteo.Finalizar(Visitar(n, 1, conteo));
    }
    private static long Visitar(int n, int profundidad, Conteo conteo)
    {
        conteo.Entrar("Fibonacci", n, profundidad);
        if (n <= 1) return n; // Dos casos base: F(0)=0 y F(1)=1.
        return checked(Visitar(n - 1, profundidad + 1, conteo)
            + Visitar(n - 2, profundidad + 1, conteo));
    }
}
