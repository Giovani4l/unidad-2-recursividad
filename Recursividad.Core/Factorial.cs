namespace Recursividad.Core;

public static class Factorial
{
    private static void Validar(int n)
    {
        // 20! cabe en Int64; 21! no. También se evita recursión sin límite.
        if (n < 0 || n > 20) throw new ArgumentOutOfRangeException(nameof(n), "Factorial admite n de 0 a 20.");
    }
    public static long Iterativo(int n)
    {
        Validar(n);
        long acumulado = 1;
        for (int i = 2; i <= n; i++) acumulado = checked(acumulado * i);
        return acumulado;
    }
    public static Resultado<long> Recursivo(int n, int limiteTraza = 200)
    {
        Validar(n);
        var conteo = new Conteo(limiteTraza);
        return conteo.Finalizar(Visitar(n, 1, conteo));
    }
    private static long Visitar(int n, int profundidad, Conteo conteo)
    {
        conteo.Entrar("Factorial", n, profundidad);
        if (n <= 1) return 1; // Caso base: 0! = 1! = 1.
        return checked(n * Visitar(n - 1, profundidad + 1, conteo));
    }
}
