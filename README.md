# Unidad 2: Recursividad

**Alumno:** Axel Giovani Gonzalez Lopez · **Matrícula:** I24050379  
**Docente:** Ruben Riojas · **Lenguaje:** C# / .NET 10

Las dos aplicaciones principales son **Windows Forms**. Los proyectos de consola
se destinan exclusivamente a pruebas y mediciones reproducibles.

## Abrir la solución

1. Extraer el ZIP completo; no ejecutarlo desde el explorador del comprimido.
2. Abrir **Unidad2.sln** en Visual Studio con soporte para .NET 10 y desarrollo
   de escritorio de Windows.
3. Elegir **Catalogo.Forms** o **Hanoi.Forms** como proyecto de inicio y pulsar F5.
4. Para ver el diseño, abrir `MainForm.cs` y elegir **Ver diseñador**.

Cada formulario tiene `MainForm.Designer.cs` para controles y distribución y
`MainForm.cs` para sus eventos. El control `HanoiCanvas` dibuja el estado real
de las torres. La lógica y validación se encuentran en `Recursividad.Core`.

```powershell
dotnet build Unidad2.sln -c Release -m:1
dotnet run --project Catalogo.Forms -c Release
dotnet run --project Hanoi.Forms -c Release
```

La carpeta **Ejecutar** del paquete incluye binarios Release. Abrir el `.exe`
correspondiente conservando todos sus archivos vecinos. Requieren Windows y
el runtime **.NET Desktop 10** instalado; no son ejecutables autocontenidos.

## Catálogo de ejercicios

- Elegir factorial, Fibonacci, Hanoi, suma o búsqueda.
- Ingresar n para los tres primeros; ingresar un arreglo para suma o búsqueda.
- Los arreglos aceptan enteros separados por coma, punto y coma o espacio.
  Una entrada vacía significa un arreglo vacío. Los índices empiezan en cero.
- Pulsar **Resolver y comparar** para ejecutar ambas versiones. La tabla
  muestra el resultado real y los contadores recursivos.
- En factorial, Fibonacci, suma y búsqueda se muestran como máximo las primeras
  200 entradas de traza. Para Hanoi se muestran hasta 200 movimientos.
  Los contadores totales continúan aunque se recorte la vista.

El factorial admite 0..20 por el rango Int64. Fibonacci directo admite 0..30
para acotar su trabajo exponencial. Los arreglos admiten hasta 500 elementos
y el plan de Hanoi admite 0..12 discos. Son límites de esta práctica, no
umbrales universales de desbordamiento de pila.

La búsqueda devuelve la **primera** coincidencia o -1. El Fibonacci mostrado
es recursivo directo, sin memoización, para estudiar sus repeticiones.

## Simulador de Hanoi

- Elegir discos y pulsar **Calcular plan**.
- Usar Siguiente, Anterior, Reiniciar y Reproducir/Pausar.
- El intervalo en milisegundos controla la animación, no el cálculo algorítmico.
- Cambiar el número de discos requiere volver a calcular el plan.
- La lista muestra hasta 1000 movimientos; el plan completo permanece disponible.
- Con cero discos se obtiene un plan vacío y una llamada base de profundidad 1.

En **Medición de profundidad**, ingresar de uno a diez tamaños distintos entre
0 y 20. Pulsar **Medir**, **Cancelar** o **Exportar CSV**. Los tamaños se normalizan
eliminando duplicados y ordenándolos. La ventana permanece disponible mientras
se calcula en segundo plano. El CSV se guarda donde el usuario elija.

## Mediciones y memoria

Por tamaño: 3 calentamientos y 7 muestras; se reportan medianas independientes
de tiempo y asignaciones del hilo. Se recorre la solución sin guardar movimientos
ni dibujar. Se incluyen contadores y comprobaciones de cancelación.

- Movimientos: M(n)=2^n-1.
- Entradas al método recursivo, incluyendo casos vacíos: C(n)=2^(n+1)-1.
- Profundidad lógica máxima, contando la entrada inicial y la base: D(n)=n+1.

El espacio auxiliar del recorrido es O(n) de pila; almacenar el plan agrega
O(2^n). `GC.GetAllocatedBytesForCurrentThread` observa asignaciones de heap
administrado, no el tamaño de la pila ni la memoria residente. Los 48 bytes
observados por muestra en este equipo no significan que la pila sea constante.
Los tiempos cambian por JIT, arquitectura, actividad del equipo y orden de
ejecución; no se determina un umbral real de StackOverflowException.

## Pruebas reproducibles

```powershell
dotnet run --project Verificaciones -c Release -- evidencias
dotnet run --project PruebasInterfaz -c Release -- evidencias
```

`Verificaciones` aprobó 164 casos de algoritmos, dominios, métricas, estados y
cancelación. `PruebasInterfaz` aprobó 19 comprobaciones de botones, reproducción,
medición y disposición de controles. Una condición fallida termina con error.
Las pruebas no intentan provocar un desbordamiento real de pila.

Las imágenes PNG son **renders directos de WinForms con DrawToBitmap**, no
capturas del escritorio. Proceden de formularios ejecutados con entradas de
prueba. Los registros originales, el JSON y el CSV se entregan en `evidencias`.
Los tiempos visibles en el render de interfaz pertenecen a otra ejecución y
pueden diferir de los del reporte principal.

## Datos y constantes

Las ventanas reciben datos editables. Los algoritmos calculan cada respuesta;
no devuelven respuestas precargadas. Las constantes de límites y valores
iniciales no son resultados simulados. Los casos de prueba sí tienen valores
conocidos para comprobar expectativas. Los datos de las ventanas no se guardan
al cerrar; solo se persiste el CSV si el usuario lo exporta.

## Repositorio y videos

El código fuente de la Unidad 2 está publicado en el repositorio público:
[github.com/Giovani4l/unidad-2-recursividad](https://github.com/Giovani4l/unidad-2-recursividad).

Los enlaces a los videos se agregarán cuando estén grabados y publicados en el
canal del alumno. Cada video debe mostrar la pantalla y la cámara web en la
esquina inferior derecha.

## Referencias principales

Black, P. E., & Rodgers, P. (2013, 30 de septiembre). *Recursion*. En P. E. Black
(Ed.), Dictionary of Algorithms and Data Structures. NIST.
https://xlinux.nist.gov/dads/HTML/recursion.html

Black, P. E. (2022, 21 de abril). *Towers of Hanoi*. En P. E. Black (Ed.),
Dictionary of Algorithms and Data Structures. NIST.
https://xlinux.nist.gov/dads/HTML/towersOfHanoi.html

Cornell University. (2025). *6. Recursion*. CS 2110.
https://www.cs.cornell.edu/courses/cs2110/2025fa/lectures/lec06/

Microsoft. (s. f.). *GC.GetAllocatedBytesForCurrentThread Method*. Microsoft Learn.
https://learn.microsoft.com/en-us/dotnet/api/system.gc.getallocatedbytesforcurrentthread?view=net-10.0
