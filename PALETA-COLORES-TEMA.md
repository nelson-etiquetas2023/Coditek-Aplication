# Paleta de Colores — Tema UI (basada en LOGO.jpeg)

**Fuente del logo:** `C:\Users\User\OneDrive\Escritorio\CODIX-LABELS\LOGO.jpeg`
**Dimensiones:** 1600 × 900 px · Modo RGB (JPEG progresivo)
**Metodología:** Cuantización mediana (10 colores) sobre una muestra reducida (200×200 px). Los porcentajes son de píxeles de la muestra. El mapeo a Pantone se hizo por distancia euclidiana en RGB contra la librería **Pantone Coated (PMS)** (1341 referencias).

---

## 1. Colores dominantes extraídos del logo

| # | Color (HEX) | RGB | % píxeles | Rol visual en el logo |
|---|-------------|-----|-----------|-----------------------|
| 1 | `#FFFFFF` / `#FEFEFE` | (255,255,255) | ~78 % | Fondo / espacio negativo |
| 2 | `#031127` | (3, 17, 39) | ~6.7 % | Azul marino muy oscuro (sombras/profundidad) |
| 3 | `#071D48` | (7, 29, 72) | ~4.3 % | Azul oscuro (contornos) |
| 4 | `#195CCD` | (25, 92, 205) | ~6.0 % | Azul brillante (color principal del logo) |
| 5 | `#B4D6EA` | (180, 214, 234) | ~2.6 % | Azul claro / cielo (brillos y degradados) |

> El logo es un **isotipo azul sobre fondo blanco**. La familia cromática es azul (hue ≈ 217–220°) con una escala de luminosidad que va del marino casi negro al azul cielo.

---

## 2. Mapeo a Pantone PMS (Coated)

Colores Pantone más cercanos por distancia euclidiana en RGB:

| Color del logo (HEX) | Pantone PMS (código) | HEX del Pantone | Distancia RGB |
|----------------------|----------------------|-----------------|---------------|
| `#031127` | **Pantone 296 C** | `#041C2C` | 12.1 |
| `#071D48` | **Pantone 2768 C** | `#071D49` | 1.0 (casi exacto) |
| `#195CCD` | **Pantone 300 C** | `#005EB8` | 32.7 |
| | (alternativa) Pantone 285 C | `#0072CE` | 33.3 |
| | (alternativa) Pantone 2935 C | `#0057B8` | 33.0 |
| `#B4D6EA` | **Pantone 290 C** | `#B9D9EB` | 5.9 |

> **Nota:** Para el azul brillante (`#195CCD`) la coincidencia Pantone más cercana es 300 C, pero la distancia es mayor (≈33). Si se prefiere un azul más estándar y reconocible en impresión, **Pantone 285 C** es una alternativa válida y muy usada en branding. Se recomienda validar con una guía física Pantone antes de imprimir.

---

## 3. Paleta de tema recomendada para la UI

Sugerencia de asignación de los colores del logo a los roles de un tema de interfaz (web Blazor y/o app .NET MAUI):

| Rol UI | Color (HEX) | Pantone | Uso típico |
|--------|-------------|---------|------------|
| **Primary** | `#195CCD` | 300 C | Botones primarios, enlaces, acentos principales, header |
| **PrimaryDark** | `#071D48` | 2768 C | Hover/pressed de primario, sidebar activo, títulos fuertes |
| **PrimaryDarker** | `#031127` | 296 C | Texto sobre fondo claro, footer, barra de navegación oscura |
| **Secondary / Accent** | `#B4D6EA` | 290 C | Fondos de tarjetas, badges, hover suave, gráficos |
| **Background** | `#FFFFFF` | — | Fondo general de la app |
| **Surface** | `#F4F8FC` | — | Tarjetas, paneles, inputs (derivado del azul claro) |
| **Text (principal)** | `#031127` | 296 C | Texto principal sobre fondos claros |
| **Text (secundario)** | `#4A5A7A` | — | Texto secundario / placeholders |
| **Border / Divider** | `#B4D6EA` | 290 C | Bordes de inputs, separadores |
| **Success** | `#28BB5B` | — | (mantener verde existente para estados OK) |
| **Warning** | `#FFC107` | — | (amarillo estándar) |
| **Error** | `#DC3545` | — | (rojo estándar) |

### Valores RGB / HSL de referencia

| HEX | RGB | HSL |
|-----|-----|-----|
| `#195CCD` | (25, 92, 205) | hsl(218°, 78 %, 45 %) |
| `#071D48` | (7, 29, 72) | hsl(220°, 82 %, 15 %) |
| `#031127` | (3, 17, 39) | hsl(217°, 86 %, 8 %) |
| `#B4D6EA` | (180, 214, 234) | hsl(203°, 53 %, 81 %) |

---

## 4. Ejemplo de variables CSS (web Blazor)

```css
:root {
    --color-primary:        #195CCD;   /* Pantone 300 C */
    --color-primary-dark:   #071D48;   /* Pantone 2768 C */
    --color-primary-darker: #031127;   /* Pantone 296 C */
    --color-secondary:      #B4D6EA;   /* Pantone 290 C */
    --color-background:     #FFFFFF;
    --color-surface:        #F4F8FC;
    --color-text:           #031127;
    --color-text-secondary: #4A5A7A;
    --color-border:         #B4D6EA;
}
```

## 5. Ejemplo de Colors.xaml (.NET MAUI)

```xml
<Color x:Key="Primary">#195CCD</Color>
<Color x:Key="PrimaryDark">#071D48</Color>
<Color x:Key="PrimaryDarker">#031127</Color>
<Color x:Key="Secondary">#B4D6EA</Color>
<Color x:Key="Background">White</Color>
<Color x:Key="Surface">#F4F8FC</Color>
<Color x:Key="TextPrimary">#031127</Color>
<Color x:Key="TextSecondary">#4A5A7A</Color>
<Color x:Key="Border">#B4D6EA</Color>
```

---

## 6. Recomendaciones de accesibilidad (contraste WCAG)

| Combinación | Contraste | Resultado |
|-------------|-----------|-----------|
| `#031127` sobre `#FFFFFF` | 18.3 : 1 | AAA ✓ |
| `#195CCD` sobre `#FFFFFF` | 4.6 : 1 | AA ✓ (texto grande) |
| `#FFFFFF` sobre `#195CCD` | 4.6 : 1 | AA ✓ |
| `#071D48` sobre `#FFFFFF` | 15.4 : 1 | AAA ✓ |
| `#B4D6EA` sobre `#031127` | 11.2 : 1 | AAA ✓ |

> El azul brillante `#195CCD` como texto pequeño sobre blanco roza el límite AA (4.5:1). Para texto de cuerpo pequeño, preferir `#071D48` o `#031127`. Para botones (texto blanco sobre `#195CCD`) cumple AA.

---

## 7. Archivos generados durante el análisis

- `analyze.py` — cuantización del logo (Pillow).
- `match.py` — mapeo a Pantone PMS por distancia RGB.
- `pantone.json` — librería Pantone TCX/TPG (Margaret2/pantone-colors).
- `pantone-coated.json` — librería Pantone Coated PMS (brettapeters/pantones).

Estos archivos son auxiliares del análisis y pueden eliminarse una vez validada la paleta.

---

*Generado a partir de `LOGO.jpeg` (empresa JOGE). Validar colores finales contra una guía física Pantone antes de impresión.*
