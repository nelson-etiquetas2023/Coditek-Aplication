# Etiquetas — Nueva etiqueta con campos en blanco + validación al guardar

Fecha: 2026-11-13 · Estado: Pendiente de aprobación

## Intención
Al pulsar **«Nueva etiqueta»**, el modal de alta debe abrir con **Descripción, Ancho, Largo y Código ZPL en blanco**, de modo que al guardar la validación obligue a que el usuario haya introducido los datos (nada precargado que pase por válido).

## Alcance
- `ScanProWeb/Pages/Etiquetas.razor` únicamente. Sin cambios en API, migraciones ni layout.

## Enfoque (único sensato)
1. `OpenNew()`: `editModel = new() { Dpmm = 8, CantidadPorRollo = 1000, Codigo = "", Descripcion = "", CodigoZpl = "", WidthMm = 0, LengthMm = 0 }`; `widthText = ""`; `lengthText = ""`. Se elimina la llamada a `InsertSampleZpl()` y el método `InsertSampleZpl()` (queda sin uso).
2. Validación al guardar (`SaveAsync`, patrón actual de toasts, sin cambios de UX):
   - Descripción no vacía → si falta, toast y no guarda.
   - Ancho/Largo: números en pulgadas, parseables y **> 0** (en blanco → `TryParseInches` falla → toast y no guarda).
   - ZPL no vacío → toast y no guarda.
3. `OpenEdit()` no cambia: carga los valores existentes para edición.
4. Ayuda visual del ZPL bajo el textarea se mantiene.

## Alternativas descartadas
- Bloquear el botón Guardar hasta tener form válido: más estado/código, sin ganancia real aquí.
- Validar ancho/largo con DataAnnotations sobre strings de pulgadas: la validación manual actual (`TryParseInches` + toasts) es más directa.

## Criterios de aceptación
1. «Nueva etiqueta» abre el modal con Descripción, Ancho, Largo y Código ZPL vacíos; ID Código en placeholder `ETI-0000`.
2. Guardar con campos vacíos muestra toast de error y no crea la etiqueta (probar al menos: todo vacío, falta descripción, falta ancho/largo, falta ZPL).
3. Ancho/Largo en blanco o con texto no numérico no se aceptan.
4. Guardar con datos válidos crea la etiqueta y el grid/detalle se refresca.
5. «Editar» sigue precargando los valores de la etiqueta existente.
