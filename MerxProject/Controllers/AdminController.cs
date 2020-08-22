using MerxProject.Models;
using MerxProject.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Orders> _Ordenes;
        private PaginadorGenerico<Orders> _PaginadorOrdenes;

        // GET: Admin
        public ActionResult Dashboard()
        {
            var Procesos = new List<ProcesosActuales>();
            Procesos = ObtenerProcesos();
            if(Procesos.Count() > 0)
            {
                ViewBag.Procesos = Procesos;
            }
            var Productos = new List<ProductoMasVendido>();
            Productos = ProductoBarChart();
            if(Productos.Count() > 0)
            {
                ViewBag.Productos = ProductoBarChart();
            }
            
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var VentasDelMes = DbModel.Orders.Where(x => x.DiaOrden.Month == DateTime.Now.Month).ToList();
                if (VentasDelMes.Count > 0)
                {
                    ViewBag.nulo = false;
                    return View();
                }
                else
                {
                    ViewBag.nulo = true;
                    return View();
                }
            }
        }

        public ActionResult ListaVentas(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Ventas = DbModel.Orders.ToList();
                _TotalRegistros = DbModel.Orders.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Ordenes = DbModel.Orders.OrderBy(x => x.DiaOrden)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorOrdenes = new PaginadorGenerico<Orders>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Ordenes
                };

                return View(_PaginadorOrdenes);
            }
        }

        public ActionResult PopUpOrden(int Id)
        {
            var Detalles = new List<Venta>();
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Detalle = DbModel.OrdersDetails.Where(x => x.idOrder == Id).ToList();
                double total = 0;
                ViewBag.title = "Detalles";
                foreach (var item in Detalle)
                {
                    total += item.Precio * item.Cantidad;
                    var Venta = new Venta()
                    {
                        Producto = DbModel.Productos.Find(item.idProducto).Nombre,
                        Color = DbModel.Colores.Find(item.idColor).Nombre,
                        Costo = item.Precio.ToString(),
                        Cantidad = item.Cantidad.ToString()
                    };
                    Detalles.Add(Venta);
                }
                ViewBag.Total = total;
                ViewBag.Detalles = Detalles;
                return View();
            }
        }

        public List<ProcesosActuales> ObtenerProcesos()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Empleados = DbModel.Empleados.ToList();
                var Personas = DbModel.Personas.ToList();
                var Procesos = DbModel.Procesos.ToList();
                var ProcesosActuales = new List<ProcesosActuales>();
                TimeSpan TiempoPasado = new TimeSpan();
                DateTime Hoy = DateTime.Now;

                foreach (var item in Procesos)
                {
                    TiempoPasado = item.Tiempo.Subtract(Hoy);
                    var PA = new ProcesosActuales()
                    {
                        Id = item.Id,
                        Empleado = item.Empleado,
                        Horas = TiempoPasado.ToString(@"hh\:mm"),
                        ProcesoActual = item.Nombre,
                        Status = item.Estado
                    };
                    ProcesosActuales.Add(PA);
                }
                return ProcesosActuales;
            }
        }

        public ActionResult BarChart()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Detalles = new List<OrdersDetails>();
                var VentasDelMes = DbModel.Orders.Where(x => x.DiaOrden.Month == DateTime.Now.Month).ToList();
                int dia = 0;

                var GananciasDelMes = new List<VentasDelMes>();
                
                foreach (var item in VentasDelMes)
                {
                    if(item.DiaOrden.Day <= dia)
                    {
                        break;
                    }
                    var VentaPorDia = DbModel.Orders.Where(x => x.DiaOrden.Day == item.DiaOrden.Day).ToList();
                    dia = item.DiaOrden.Day;
                    double ganancia = 0;
                    foreach (var detail in VentaPorDia)
                    {
                        Detalles = DbModel.OrdersDetails.Where(x => x.idOrder == detail.IdOrder).ToList();
                        foreach(var det in Detalles)
                        {
                            ganancia += det.Precio;
                        }
                    }
                    var Ventas = new VentasDelMes()
                    {
                        dia = item.DiaOrden.Day,
                        Ganancia = ganancia
                    };
                    GananciasDelMes.Add(Ventas);
                }
                return Json(GananciasDelMes, JsonRequestBehavior.AllowGet);
            }
        }

        public List<ProductoMasVendido> ProductoBarChart()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Detalles = new List<OrdersDetails>();
                var Ids = new List<int>();
                var VentasDelMes = DbModel.Orders.Where(x => x.DiaOrden.Month == DateTime.Now.Month).ToList();
                
                var ProductoDelMes = new List<ProductoMasVendido>();

                if (VentasDelMes.Count() > 0)
                {

                    foreach (var item in VentasDelMes)
                    {
                        bool encontrado = false;
                        Detalles = DbModel.OrdersDetails.Where(x => x.idOrder == item.IdOrder).ToList();
                        string Nombre = null;
                        foreach (var Det in Detalles)
                        {

                            foreach (var lista in Ids)
                            {
                                if (Det.idProducto == lista)
                                {
                                    encontrado = true;
                                }
                            }
                            if (encontrado)
                            {
                                break;
                            }
                            Ids.Add(Det.idProducto);
                            int Count = 0;
                            var existe = DbModel.OrdersDetails.Where(x => x.idProducto == Det.idProducto).Count();
                            if (existe > 0)
                            {
                                var prod = DbModel.OrdersDetails.Where(x => x.idProducto == Det.idProducto).ToList();
                                foreach (var cant in prod)
                                {
                                    Count += cant.Cantidad;
                                }
                                Nombre = DbModel.Productos.Find(Det.idProducto).Nombre;
                                var PV = new ProductoMasVendido()
                                {
                                    Cantidad = Count,
                                    Producto = Nombre
                                };
                                ProductoDelMes.Add(PV);
                            }
                        }

                    }
                    return ProductoDelMes.OrderByDescending(x => x.Cantidad).Take(5).ToList();
                }
                else
                {
                    var PV = new ProductoMasVendido()
                    {
                        Cantidad = 0,
                        Producto = "No se tiene registro de ningún producto"
                    };
                    ProductoDelMes.Add(PV);
                    return ProductoDelMes;
                }
            }
        }

        private class VentasDelMes
        {
            public int dia { get; set; }
            public double Ganancia { get; set; }
        }

        public class ProductoMasVendido
        {
            public string Producto { get; set; }
            public int Cantidad { get; set; }
        }

        public class ProcesosActuales
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string ProcesoActual { get; set; }
            public string Horas { get; set; }
            public string Empleado { get; set; }
        }

        public class Venta
        {
            public string Producto { get; set; }
            public string Color { get; set; }
            public string Costo { get; set; }
            public string Cantidad { get; set; }
        }
    }
}