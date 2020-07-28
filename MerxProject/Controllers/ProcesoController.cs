using MerxProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class ProcesoController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Proceso> _Procesos;
        private PaginadorGenerico<Proceso> _PaginadorProcesos;

        // GET: Proceso
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult InventarioPorId(int Id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Colores = DbModel.Colores.ToList();
                var producto = DbModel.Productos.ToList();
                var inventarios = DbModel.Inventarios.ToList();
                var invId = new List<Inventario>();
                invId = inventarios.Where(x => x.Producto.Id == Id).ToList();
                var colores = new List<Tuple<int, string>>();
                foreach (var item in invId)
                {
                    colores.Add(new Tuple<int, string>(item.Id, item.Color.Nombre));
                }
                SelectList obgInv = new SelectList(colores, "item1", "item2", 0);
                return Json(obgInv);
            }
        }

        public ActionResult ProductoPorParametro(string Parametro)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (!string.IsNullOrEmpty(Parametro))
                {
                    var muebles = DbModel.Muebles.ToList();
                    var producto = DbModel.Productos.ToList();
                    var pd = new List<Producto>();
                    pd = producto.Where(x => x.Nombre.Contains(Parametro) ||
                                       (x.Descripcion.Contains(Parametro)) ||
                                       (x.CategoriaMueble.Nombre.Contains(Parametro)))
                                       .ToList();
                    var productos = new List<Tuple<int, string>>();
                    foreach (var item in pd)
                    {
                        productos.Add(new Tuple<int, string>(item.Id, item.Nombre));
                    }
                    SelectList objProd = new SelectList(productos, "item1", "item2", 0);
                    return Json(objProd);
                }
                else
                {
                    var producto = DbModel.Productos.ToList();
                    var pd = new List<Producto>();
                    pd = producto.ToList();
                    var productos = new List<Tuple<int, string>>();
                    foreach (var item in pd)
                    {
                        productos.Add(new Tuple<int, string>(item.Id, item.Nombre));
                    }
                    SelectList objProd = new SelectList(productos, "item1", "item2", 0);
                    return Json(objProd);
                }
            }
        }
        

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaProcesos (int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Procesos.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Procesos = DbModel.Procesos.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProcesos = new PaginadorGenerico<Proceso>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Procesos
                };

                var TiempoActual = new List<Tuple<string, TimeSpan>>();
                TimeSpan TiempoPasado = new TimeSpan();
                DateTime Hoy = DateTime.Now;

                foreach (var item in _Procesos)
                {
                    TiempoPasado = Hoy - item.Tiempo;
                    TiempoActual.Add(new Tuple<string, TimeSpan>(item.Id, TiempoPasado));
                }

                ViewBag.Tiempo = TiempoActual;
                return View(_PaginadorProcesos);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult popUpProcesos(int? Id, string accion)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                try
                {
                    if (accion == "1")
                    {
                        ViewBag.title = "Nuevo";
                        ViewBag.Accion = "1";
                        var productos = new List<Producto>();
                        productos = DbModel.Productos.ToList();
                        ViewBag.Productos = productos;

                        return View();
                    }
                    else if (accion == "2")
                    {
                     
                        var procesos = DbModel.Procesos.Find(Id);
                        procesos.Tiempo = DateTime.Now;
                        procesos.Estado = "En espera";
                        switch (procesos.Nombre)
                        {
                            case "Corte":
                                procesos.Nombre = "Construcción";
                                break;
                            case "Construcción":
                                procesos.Nombre = "Detallado";
                                break;
                            case "Detallado":
                                procesos.Nombre = "Pintado";
                                break;
                            case "Pintado":
                                procesos.Nombre = "Finalizado";
                                break;
                            case "Finalizado":
                                resultado = "Proceso terminado, esperando a que cliente confirma";
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaProcesos");
                        }
                        DbModel.Procesos.AddOrUpdate(procesos);
                        DbModel.SaveChanges();
                        resultado = "Actualización realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProcesos");
                    }
                    else if (accion == "3")
                    {
                        var procesos = DbModel.Procesos.Find(Id);
                        procesos.Estado = "Detenido";
                        DbModel.Procesos.AddOrUpdate(procesos);
                        DbModel.SaveChanges();
                        return RedirectToAction("ListaProcesos");
                    }
                }
                catch (Exception ex)
                {
                    resultado = ex.Message;
                    Session["res"] = resultado;
                    return RedirectToAction("ListaProcesos");
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpProcesos(int Id)
        {
            string resultado;
            
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                try
                {
                    string res;
                    res = GenerarPedido(Id);
                    resultado = "Proceso iniciado, " + res + " es el Id";
                    Session["res"] = resultado;
                    Session["tipo"] = "Exito";
                    return RedirectToAction("ListaProcesos");
                }
                catch(Exception ex)
                {
                    resultado = ex.Message;
                    Session["res"] = resultado;
                    return RedirectToAction("ListaProcesos");
                }
            }
        }

        public string GenerarPedido(int Id)
        {
            if (!string.IsNullOrWhiteSpace(Id.ToString()))
            {
                using (ApplicationDbContext DbModel = new ApplicationDbContext())
                {
                    var muebles = DbModel.Muebles.ToList();
                    var productos = DbModel.Productos.ToList();
                    var inventario = DbModel.Inventarios.Find(Id);

                    var guid = Guid.NewGuid();
                    string id = guid.ToString();
                    var proceso = new Proceso
                    {
                        Id = id,
                        Inventario = inventario,
                        Estado = "En espera",
                        Tiempo = DateTime.Now,
                        Registro = DateTime.Now,
                        Nombre = "Corte"
                        
                    };

                    DbModel.Procesos.Add(proceso);
                    DbModel.SaveChanges();

                    return id;
                }
            }
            else
            {
                return "Error, producto no encontrado";
            }
        }

        public string TomarPedido(int Id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var personas = DbModel.Personas.ToList();
                var usuarios = DbModel.Usuarios.ToList();
                var usuario = usuarios.Where(x => x.User == User.Identity.Name).ToList();

                string nombre = null;
                foreach (var item in personas)
                {
                    foreach (var per in usuario)
                    {
                        if (item.idUsuario == per.idUsuario)
                        {
                            nombre = item.Nombre;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    try
                    {
                        var inventario = DbModel.Inventarios.ToList();
                        var proceso = DbModel.Procesos.Find(Id);
                        proceso.Estado = "En proceso";
                        proceso.Empleado = nombre;
                        proceso.Tiempo = DateTime.Now;
                        DbModel.Procesos.AddOrUpdate(proceso);
                        DbModel.SaveChanges();
                        return "Exito";
                    }
                    catch(Exception ex)
                    {
                        return ex.Message;
                    }
                }
                else
                {
                    return "Usuario no reconocido";
                }
            }
        }

        [HttpGet]
        public ActionResult ConsultarPedido()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConsultarPedido(string Id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                try
                {
                    var proceso = DbModel.Procesos.Find(Id);
                    if (proceso != null)
                    {
                        Session["res"] = "Pedido encontrado";
                        Session["tipo"] = "Exito";
                        ViewBag.proceso = proceso;
                        return View();
                    }
                    else
                    {
                        Session["res"] = "Pedido no encontrado";
                        return RedirectToAction("ConsultarPedido");
                    }
                }
                catch (Exception ex)
                {
                    Session["res"] = ex.Message;
                    return RedirectToAction("ConsultarPedido");
                }
            }
        }
    }
}