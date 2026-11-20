using Recursividad.Core;
using System.ComponentModel;

namespace Hanoi.Forms;

public sealed class HanoiCanvas : Control
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EstadoHanoi? Estado { get; set; }
    public HanoiCanvas() { DoubleBuffered = true; BackColor = Color.White; ResizeRedraw = true; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int margen = 25, suelo = Height - 42; float anchoColumna = (Width - 2f * margen) / 3;
        using var poste = new SolidBrush(Color.FromArgb(90, 95, 102));
        using var letra = new SolidBrush(Color.FromArgb(30, 40, 52));
        var torres = Estado?.Torres ?? new[] { Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>() };
        int n = Math.Max(1, Estado?.Discos ?? 1); float altoDisco = Math.Clamp((Height - 90f) / n, 12, 23);
        Color[] colores = { Color.SteelBlue, Color.DarkSlateBlue, Color.DarkCyan, Color.Peru };
        for (int t = 0; t < 3; t++)
        {
            float centro = margen + anchoColumna * (t + .5f);
            g.FillRectangle(poste, centro - 3, 30, 6, Math.Max(1, suelo - 30));
            g.FillRectangle(poste, margen + t * anchoColumna + 5, suelo, anchoColumna - 10, 5);
            g.DrawString(((char)('A' + t)).ToString(), Font, letra, centro - 7, suelo + 10);
            var discos = torres[t].Reverse().ToArray();
            for (int i = 0; i < discos.Length; i++)
            {
                int disco = discos[i]; float ancho = 24 + (anchoColumna - 45) * disco / n;
                float y = suelo - (i + 1) * altoDisco;
                using var relleno = new SolidBrush(colores[(disco - 1) % colores.Length]);
                g.FillRectangle(relleno, centro - ancho / 2, y, ancho, altoDisco - 2);
                var texto = disco.ToString(); var medida = g.MeasureString(texto, Font);
                g.DrawString(texto, Font, Brushes.White, centro - medida.Width / 2, y + (altoDisco - medida.Height) / 2);
            }
        }
        if (Estado is null) g.DrawString("Calcula un plan para iniciar la simulación.", Font, letra, 18, 8);
    }
}
