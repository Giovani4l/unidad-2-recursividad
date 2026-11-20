using System.Reflection;
using System.Drawing.Imaging;

// Integración en el propio proceso. Los renders no son capturas del escritorio.
internal static class Program
{
    private static int aprobadas;
    private static T Campo<T>(Form form, string nombre) where T : class =>
        (T)(form.GetType().GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!);
    private static void Check(bool ok, string nombre)
    {
        if (!ok) throw new InvalidOperationException("FALLO: " + nombre);
        Console.WriteLine("PASS " + nombre); aprobadas++;
    }
    private static void Click(Form form, string nombre)
    {
        typeof(Button).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(Campo<Button>(form, nombre), new object[] { EventArgs.Empty });
        Application.DoEvents();
    }
    private static void Esperar(Func<bool> terminado)
    {
        var limite = DateTime.UtcNow.AddSeconds(20);
        while (!terminado() && DateTime.UtcNow < limite) { Application.DoEvents(); Thread.Sleep(10); }
        if (!terminado()) throw new TimeoutException("No terminó la operación de la interfaz.");
    }
    private static IEnumerable<Control> Descendientes(Control raiz)
    {
        foreach (Control control in raiz.Controls)
        { yield return control; foreach (var hijo in Descendientes(control)) yield return hijo; }
    }
    private static void Render(Form form, string archivo)
    {
        form.Show(); Application.DoEvents(); form.PerformLayout();
        foreach (var panel in Descendientes(form).OfType<FlowLayoutPanel>().Where(p => p.Visible))
            Check(panel.Controls.Cast<Control>().All(c => c.Right <= panel.ClientSize.Width && c.Bottom <= panel.ClientSize.Height), "controles sin recorte en " + panel.Name);
        using var imagen = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(imagen, new Rectangle(Point.Empty, imagen.Size));
        imagen.Save(archivo, ImageFormat.Png);
    }
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        string destino = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Environment.CurrentDirectory, "evidencias");
        Directory.CreateDirectory(destino);
        using var catalogo = new Catalogo.Forms.MainForm();
        var ejercicio = Campo<ComboBox>(catalogo, "ejercicio");
        var n = Campo<NumericUpDown>(catalogo, "numero");
        var tabla = Campo<DataGridView>(catalogo, "resultados");
        void Resolver(int opcion, string esperado)
        {
            ejercicio.SelectedIndex = opcion; Click(catalogo, "ejecutar");
            Esperar(() => Campo<Button>(catalogo, "ejecutar").Enabled);
            Check(tabla.Rows.Count == 2 && tabla.Rows[0].Cells[1].Value?.ToString() == esperado
                && tabla.Rows[1].Cells[1].Value?.ToString() == esperado, "catálogo: " + ejercicio.Text);
        }
        n.Value = 7; Resolver(0, "5040");
        Check(tabla.Rows[1].Cells[2].Value?.ToString() == "7", "contador real de factorial en pantalla");
        n.Value = 10; Resolver(1, "55");
        n.Value = 4; Resolver(2, "15 movimientos");
        Campo<TextBox>(catalogo, "arreglo").Text = "3,-1,8"; Resolver(3, "10");
        Campo<TextBox>(catalogo, "arreglo").Text = "4,9,9,1";
        Campo<NumericUpDown>(catalogo, "objetivo").Value = 9; Resolver(4, "1");
        Campo<NumericUpDown>(catalogo, "objetivo").Value = 8; Resolver(4, "-1");
        n.Value = 7; Resolver(0, "5040");
        Render(catalogo, Path.Combine(destino, "Catalogo_formulario.png")); catalogo.Hide();

        using var hanoi = new Hanoi.Forms.MainForm();
        Campo<NumericUpDown>(hanoi, "discos").Value = 3; Click(hanoi, "preparar");
        Check(Campo<DataGridView>(hanoi, "movimientos").Rows.Count == 7, "plan de tres discos genera siete movimientos");
        Click(hanoi, "siguiente");
        Check(Campo<Label>(hanoi, "progreso").Text.Contains("Movimiento 1 de 7"), "avance del simulador");
        Click(hanoi, "anterior");
        Check(Campo<Label>(hanoi, "progreso").Text.Contains("Movimiento 0 de 7"), "retroceso del simulador");
        for (int i = 0; i < 7; i++) Click(hanoi, "siguiente");
        Check(Campo<Label>(hanoi, "progreso").Text.Contains("Solución completada"), "simulador alcanza destino");
        Click(hanoi, "reiniciar");
        Click(hanoi, "siguiente");Click(hanoi, "siguiente");Click(hanoi, "siguiente");
        Render(hanoi, Path.Combine(destino, "Hanoi_simulacion.png"));
        Campo<NumericUpDown>(hanoi, "intervalo").Value = 50;
        Click(hanoi, "reproducir");
        Esperar(() => !Campo<Label>(hanoi, "progreso").Text.Contains("Movimiento 3 de 7"));
        Check(true, "temporizador ejecuta la reproducción");
        if (Campo<Button>(hanoi, "reproducir").Text == "Pausar") Click(hanoi, "reproducir");
        Campo<TabControl>(hanoi, "pestanas").SelectedIndex = 1;
        Campo<TextBox>(hanoi, "profundidades").Text = "0,4,8";
        Click(hanoi, "medir"); Esperar(() => Campo<Button>(hanoi, "medir").Enabled);
        var medidas = Campo<DataGridView>(hanoi, "resultados");
        Check(medidas.Rows.Count == 3 && medidas.Rows[2].Cells[2].Value?.ToString() == "511"
            && medidas.Rows[2].Cells[3].Value?.ToString() == "9", "medición desde la interfaz con tamaños elegidos");
        Check(Campo<Button>(hanoi, "exportar").Enabled, "exportación disponible tras medir");
        Render(hanoi, Path.Combine(destino, "Hanoi_medicion.png"));
        Console.WriteLine($"RESULTADO: {aprobadas} comprobaciones de interfaz aprobadas");
        Console.WriteLine("Imágenes: renders directos de WinForms con DrawToBitmap; no son capturas del escritorio.");
    }
}
