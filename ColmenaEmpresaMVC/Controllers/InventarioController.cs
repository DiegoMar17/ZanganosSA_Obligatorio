using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InventarioController : Controller
    {
        private static readonly List<ItemInventario> _items = new()
        {
            new() { Id=1, Nombre="Alzas de madera",   Unidad="u",  CantidadActual=80,  CantidadMaxima=100, CantidadMinima=20 },
            new() { Id=2, Nombre="Ácido oxálico",     Unidad="kg", CantidadActual=2,   CantidadMaxima=8,   CantidadMinima=2 },
            new() { Id=3, Nombre="Marcos de cera",    Unidad="u",  CantidadActual=120, CantidadMaxima=200, CantidadMinima=40 },
            new() { Id=4, Nombre="Jarabe azucarado",  Unidad="L",  CantidadActual=5,   CantidadMaxima=50,  CantidadMinima=20 },
            new() { Id=5, Nombre="Trajes apícolas",   Unidad="u",  CantidadActual=4,   CantidadMaxima=4,   CantidadMinima=2 },
        };

        public IActionResult Index()
        {
            ViewBag.BajoMinimo    = _items.Count(i => i.CantidadActual <= i.CantidadMinima);
            ViewBag.ItemsTotales  = _items.Count;
            return View(_items);
        }
    }
}
