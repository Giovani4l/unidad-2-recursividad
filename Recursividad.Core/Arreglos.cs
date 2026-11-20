using System.Globalization;

namespace Recursividad.Core;

public static class EntradaArreglo
{
    public static int[] Parsear(string texto)
    {
        var partes = texto.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length > 500) throw new ArgumentException("Se admiten como máximo 500 enteros.");
        var resultado = new int[partes.Length];
        for (int i = 0; i < partes.Length; i++)
            if (!int.TryParse(partes[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out resultado[i]))
                throw new ArgumentException($"El valor '{partes[i]}' no es un entero Int32 válido.");
        return resultado;
    }
    internal static void Validar(int[] valores)
    {
        ArgumentNullException.ThrowIfNull(valores);
        if (valores.Length > 500) throw new ArgumentException("La práctica limita los arreglos a 500 elementos.");
    }
}
public static class SumaArreglo
{
    public static long Iterativo(int[] valores)
    {
        EntradaArreglo.Validar(valores);
        long suma = 0;
        foreach (int valor in valores) suma = checked(suma + valor);
        return suma;
    }
    public static Resultado<long> Recursivo(int[] valores, int limiteTraza = 200)
    {
        EntradaArreglo.Validar(valores);
        var conteo = new Conteo(limiteTraza);
        return conteo.Finalizar(Visitar(valores, 0, 1, conteo));
    }
    private static long Visitar(int[] valores, int indice, int profundidad, Conteo conteo)
    {
        conteo.Entrar("Suma desde índice", indice, profundidad);
        if (indice == valores.Length) return 0; // Caso base: sufijo vacío.
        return checked(valores[indice] + Visitar(valores, indice + 1, profundidad + 1, conteo));
    }
}
public static class BusquedaLineal
{
    public static int Iterativo(int[] valores, int buscado)
    {
        EntradaArreglo.Validar(valores);
        for (int i = 0; i < valores.Length; i++) if (valores[i] == buscado) return i;
        return -1;
    }
    public static Resultado<int> Recursivo(int[] valores, int buscado, int limiteTraza = 200)
    {
        EntradaArreglo.Validar(valores);
        var conteo = new Conteo(limiteTraza);
        return conteo.Finalizar(Visitar(valores, buscado, 0, 1, conteo));
    }
    private static int Visitar(int[] valores, int buscado, int indice, int profundidad, Conteo conteo)
    {
        conteo.Entrar("Buscar desde índice", indice, profundidad);
        if (indice == valores.Length) return -1; // Caso base: no queda ningún candidato.
        if (valores[indice] == buscado) return indice; // Caso base: primera coincidencia.
        return Visitar(valores, buscado, indice + 1, profundidad + 1, conteo);
    }
}
