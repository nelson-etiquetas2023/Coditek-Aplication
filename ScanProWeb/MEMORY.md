# MEMORY - ScanProWeb

## Layouts (preferencias permanentes)
- **Index de productos (`Pages/Productos.razor`, ruta `/productos`)**: siempre **30% grid / 70% tabpage**.
  - Izquierda 30%: QuickGrid de búsqueda (flex: 0 0 calc(30% - 8px)).
  - Derecha 70%: card/tabpage de detalles (flex: 0 0 calc(70% - 8px)).
  - Contenedor: `d-flex gap-3`. No volver a `col-md-4/col-md-8` (33/66).
  - Decisión del usuario: 2026-11-08 — mantener siempre ese layout en futuras ediciones del index.
