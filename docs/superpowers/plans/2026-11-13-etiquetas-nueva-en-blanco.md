# Etiquetas — Nueva etiqueta en blanco: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Al pulsar «Nueva etiqueta», el modal abre con Descripción, Ancho, Largo y Código ZPL en blanco; al guardar, la validación exige que el usuario haya introducido los datos.

**Architecture:** Cambio aislado en el componente Blazor `Pages/Etiquetas.razor`. `OpenNew()` deja de precargar valores y de insertar un ZPL de ejemplo; la validación existente en `SaveAsync()` (toasts) pasa a ser la que obliga a introducir los datos. `OpenEdit()` no cambia.

**Tech Stack:** Blazor WebAssembly (.NET 10), BlazorBootstrap (`Modal`, `InputText`, `EditForm`), ToastService.

**Spec:** `docs/superpowers/specs/2026-11-13-etiquetas-nueva-en-blanco-design.md`

## Global Constraints

- No tocar API, migraciones, `AppDbContext`, ni el layout 30/70 (`.etiquetas-grid` / `.etiquetas-detail`).
- Ancho/Largo siguen siendo texto libre en **pulgadas** con `@bind:event="oninput"` y se convierten a mm en `SaveAsync` (`× 25.4m`, redondeo a 2 decimales).
- Validación al guardar se mantiene como toasts de `ToastService` (patrón actual), no `ValidationMessage` inline para ancho/largo.
- ID Código (`editModel.Codigo`) sigue en placeholder `ETI-0000`, deshabilitado, asignado por el servidor.
- Placeholders y textos de UI en español.

## Review Focus

1. **Abrir «Editar» y pulsar Cancelar y luego «Nueva etiqueta»** — deben aparecer los campos en blanco, no los valores del último editado ni del último guardado. → cubierto por Task 1 (estado nuevo en `OpenNew`) y verificado en Task 1 Step 3.
2. **Ancho o Largo con coma decimal (`1,5`) o en blanco** — `TryParseInches` normaliza coma→punto; en blanco debe fallar la validación con toast. → cubierto por Task 1 Step 3 (casos manuales).
3. **ZPL vacío pero descripción y dimensiones válidas** — no debe crear; toast «El código ZPL es obligatorio.» → cubierto por Task 1 Step 3.
4. **`InsertSampleZpl` eliminado pero referenciado en otro sitio** — `grep` debe devolver 0 referencias tras Task 1. → cubierto por Task 1 Step 2.
5. **Dpmm/CantidadPorRollo en la etiqueta nueva** — deben conservar defaults `8` y `1000` (los usa la preview y el guardado). → cubierto por Task 1 Interfaces.

---

### Task 1: Blanquear «Nueva etiqueta» y verificar validación

**Files:**
- Modify: `ScanProWeb/Pages/Etiquetas.razor:300-308` (`OpenNew`)
- Modify: `ScanProWeb/Pages/Etiquetas.razor:399-412` (eliminar `InsertSampleZpl`)

**Interfaces:**
- Consumes: `ApiLabelService.LabelTemplateDto` con propiedades `Id, Codigo, Descripcion, WidthMm, LengthMm, CantidadPorRollo, CodigoZpl, Dpmm`.
- Produces: `OpenNew()` — sin retorno; deja `editingId = 0`, `editModel = new LabelTemplateDto { Dpmm = 8, CantidadPorRollo = 1000, Codigo = "", Descripcion = "", CodigoZpl = "", WidthMm = 0, LengthMm = 0 }`, `widthText = ""`, `lengthText = ""`, y muestra el modal. `SaveAsync()` no cambia de firma ni de lógica: descripción obligatoria, `TryParseInches(widthText/lengthText)` con valor > 0, `CodigoZpl` obligatoria.

- [ ] **Step 1: Modificar `OpenNew()`**

Reemplazar el cuerpo actual (líneas 300-308) por:

```csharp
private async Task OpenNew()
{
    editingId = 0;
    editModel = new ApiLabelService.LabelTemplateDto { Dpmm = 8, WidthMm = 0m, LengthMm = 0m, CantidadPorRollo = 1000, Codigo = "", Descripcion = "", CodigoZpl = "" };
    widthText = "";
    lengthText = "";
    if (editModal is not null) await editModal.ShowAsync();
}
```

Sin llamada a `InsertSampleZpl()`.

- [ ] **Step 2: Eliminar `InsertSampleZpl()` completo**

Borrar el método entero (líneas 399-412). Verificar que no queda ninguna referencia:

Run: `grep`/`Select-String -Path ScanProWeb\Pages\Etiquetas.razor -Pattern "InsertSampleZpl"`
Expected: sin resultados.

- [ ] **Step 3: Build y verificación manual**

Run: `dotnet build ScanProWeb\CODIX-LABELS.csproj`
Expected: `Build succeeded`, 0 errores.

Con la app corriendo (API en `http://localhost:5220/` + `dotnet watch` o F5 en ScanProWeb):

1. Pulsar **«Nueva etiqueta»** → Descripción, Ancho, Largo y Código ZPL en blanco; ID Código en placeholder `ETI-0000`.
2. Pulsar **Guardar** con todo vacío → toast rojo «La descripción de la etiqueta es obligatoria.»; no se crea.
3. Rellenar solo Descripción → Guardar → toast de ancho/largo inválido; no se crea.
4. Descripción + Ancho `1.2` → Guardar → toast de largo inválido; no se crea.
5. Descripción + Ancho `1.2` + Largo `0.7` → Guardar → toast «El código ZPL es obligatorio.»; no se crea.
6. Descripción + Ancho `1,5` (coma) + Largo `0.7` + ZPL `^XA^XZ` → Guardar → **sí** crea; el grid se refresca y la etiqueta queda seleccionada.
7. Pulsar **Editar** en cualquier etiqueta → el modal precarga sus valores (sin regresión).
8. (Review Focus #1) Con el modal de **Editar** abierto pulsar Cancelar, luego **«Nueva etiqueta»** → campos en blanco.

- [ ] **Step 4: Commit**

```bash
git add ScanProWeb/Pages/Etiquetas.razor docs/superpowers/specs/2026-11-13-etiquetas-nueva-en-blanco-design.md docs/superpowers/plans/2026-11-13-etiquetas-nueva-en-blanco.md
git commit -m "feat(etiquetas): abrir 'Nueva etiqueta' con campos en blanco y validar al guardar"
```
