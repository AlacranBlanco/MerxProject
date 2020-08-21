using MerxProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MerxProject.Controllers
{
    public class ComprasController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Compra> _Compras;
        private List<DetalleCompra> _DetalleCompras;
        private PaginadorGenerico<Compra> _PaginadorCompras;
        private PaginadorGenerico<DetalleCompra> _PaginadorDetalleCompra;

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpCompras(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                ViewBag.Proveedores = DbModel.Proveedores.Include("Persona").ToList();
                ViewBag.Empleados = DbModel.Empleados.Include("Personass").Include("Usuarioss").ToList();
                if (accion == "1")
                {
                    var correo = User.Identity.Name;
                    var compra = new Compra();
                    compra.FechaRegistro = DateTime.Now;
                    compra.Estatus = 0;
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    if (correo != "")
                    {
                        var Persona = DbModel.Personas.FirstOrDefault(x => x.Correo == correo);
                        compra.Empleado = DbModel.Empleados.
                            Include("Personass").
                            FirstOrDefault(x => x.Personass.idPersona == Persona.idPersona);
                    }
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    return View("popUpCompras", compra);
                }
                else if (accion == "2")
                {
                    var compra = DbModel.Compras.Find(Id);
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    return View(compra);
                }
                else if (accion == "3")
                {
                    var compra = DbModel.Compras.Find(Id);
                    ViewBag.title = "Cancelar";
                    ViewBag.Accion = "3";
                    return View(compra);
                }
                else if (accion == "5")
                {
                    var compra = DbModel.Compras.Find(Id);
                    ViewBag.title = "Marcar como Entregada";
                    ViewBag.Accion = "5";
                    return View(compra);
                }

            }
            return RedirectToAction("popUpCompras");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpCompras(Compra compras, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (compras.Id > 0 && accion == "2")
                {
                    // Edición
                    var compra = DbModel.Compras.Include("Proveedor").Include("Empleado").Where(e => e.Id == compras.Id).FirstOrDefault();

                    if (compra != null)
                    {
                        var proveedor = DbModel.Proveedores.Find(compras.Proveedor.Id);
                        var empleado = DbModel.Empleados.Find(compras.Empleado.Id);

                        if (proveedor != null && empleado != null)
                        {

                            compra.Proveedor = proveedor;
                            compra.Empleado = empleado;
                            compra.Folio = compra.Proveedor.RFC.Substring(0, 4).ToUpper() +
                                                compras.FechaRegistro.Year + compras.FechaRegistro.Month +
                                                compras.FechaRegistro.Day + compras.FechaRegistro.Hour +
                                                compras.FechaRegistro.Minute + compras.FechaRegistro.Second;
                            try
                            {
                                DbModel.Compras.AddOrUpdate(compra);
                                DbModel.SaveChanges();
                                resultado = "Actualización realizada";
                                ViewBag.res = resultado;
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaCompras");

                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;
                                Session["res"] = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                        }
                    }

                }
                else if (compras.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var compra = DbModel.Compras.Find(compras.Id);

                    if (compra != null)
                    {

                        try
                        {
                            compra.Estatus = 4;
                            DbModel.Compras.AddOrUpdate(compra);
                            DbModel.SaveChanges();
                            resultado = "Cancelación finalizada";
                            ViewBag.res = resultado;

                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaCompras");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;

                            Session["res"] = resultado;
                            return RedirectToAction("ListaCompras");
                        }
                    }
                }
                else if (compras.Id > 0 && accion == "5")
                {
                    // Entrega
                    //var compra = DbModel.Compras.Find(compras.Id);
                    var compra = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona").Where(e => e.Id == compras.Id).FirstOrDefault();

                    if (compra != null)
                    {
                        var proveedor = DbModel.Proveedores.Find(compra.Proveedor.Id);
                        var empleado = DbModel.Empleados.Find(compra.Empleado.Id);

                        if (proveedor != null && empleado != null)
                        {

                            compras.Proveedor = proveedor;
                            compras.Empleado = empleado;
                            try
                            {
                                compra.Estatus = 3;
                                DbModel.Compras.AddOrUpdate(compra);

                                var detalles = DbModel.DetalleCompra.Include("MateriaPrima").Include("Herramienta").Where(e => e.Compra.Id == compra.Id).ToList();
                                if (detalles.Count() > 0)
                                {
                                    foreach (DetalleCompra det in detalles)
                                    {
                                        if (det.Herramienta != null)
                                        {
                                            var herramienta = DbModel.Herramientas.Where(e => e.Id == det.Herramienta.Id).FirstOrDefault();
                                            herramienta.Cantidad += Convert.ToInt32(Math.Floor(det.Cantidad));
                                            DbModel.Herramientas.AddOrUpdate(herramienta);
                                        }
                                        else if (det.MateriaPrima != null)
                                        {
                                            var materiaPrima = DbModel.Materiales.Where(e => e.Id == det.MateriaPrima.Id).FirstOrDefault();
                                            materiaPrima.Cantidad += Convert.ToInt32(Math.Floor(det.Cantidad));
                                            DbModel.Materiales.AddOrUpdate(materiaPrima);
                                        }
                                    }
                                }
                                DbModel.SaveChanges();
                                resultado = "Entrega finalizada";
                                ViewBag.res = resultado;

                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaCompras");
                            }

                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;

                                Session["res"] = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                        }
                    }
                }
                else
                {
                    if (compras != null)
                    {
                        var proveedor = DbModel.Proveedores.Find(compras.Proveedor.Id);
                        var empleado = DbModel.Empleados.Find(compras.Empleado.Id);

                        if (proveedor != null && empleado != null)
                        {

                            compras.Proveedor = proveedor;
                            compras.Empleado = empleado;
                            // Aquí código para crear
                            try
                            {
                                compras.Folio = compras.Proveedor.RFC.Substring(0,4).ToUpper() + 
                                                compras.FechaRegistro.Year + compras.FechaRegistro.Month +
                                                compras.FechaRegistro.Day + compras.FechaRegistro.Hour +
                                                compras.FechaRegistro.Minute + compras.FechaRegistro.Second;
                                DbModel.Compras.Add(compras);
                                DbModel.SaveChanges();
                                resultado = "Inserción realizada";
                                ViewBag.res = resultado;

                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaDetalleCompra");
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                ViewBag.res = resultado;

                                Session["res"] = resultado;
                                return RedirectToAction("ListaCompras");
                            }
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaCompras");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaCompras(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona").Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Compras = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona").OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                foreach (Compra compra in _Compras)
                {
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                }
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorCompras = new PaginadorGenerico<Compra>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Compras
                };


                return View(_PaginadorCompras);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona")
                                                        .Where(x => x.Folio.Contains(parameter) ||
                                                        x.Empleado.Personass.Nombre.Contains(parameter) ||
                                                        x.Proveedor.Persona.Nombre.Contains(parameter) ||
                                                        x.FechaRegistro.ToString().Contains(parameter)).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Compras = DbModel.Compras.Include("Empleado.Personass").Include("Proveedor.Persona")
                                                        .Where(x => x.Folio.Contains(parameter) ||
                                                        x.Empleado.Personass.Nombre.Contains(parameter) ||
                                                        x.Proveedor.Persona.Nombre.Contains(parameter) ||
                                                        x.FechaRegistro.ToString().Contains(parameter))
                                                        .OrderBy(x => x.Folio)
                                                      .Skip((pagina - 1) * _RegistrosPorPagina)
                                                      .Take(_RegistrosPorPagina)
                                                      .ToList();
                foreach (Compra compra in _Compras)
                {
                    compra.DS_Estatus = ((EstatusC[])(Enum.GetValues(typeof(EstatusC))))[Convert.ToInt32(compra.Estatus)].ToString();
                }
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorCompras = new PaginadorGenerico<Compra>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Compras
                };

                if (_TotalRegistros < 1)
                {
                    Session["res"] = ("No hay resultados");
                    return RedirectToAction("ListaCompras");
                }
                else
                {
                    return View("ListaCompras", _PaginadorCompras);
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaDetalleCompra(int? idCompra, int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var compra = DbModel.Compras.Where(e => e.Id == idCompra).FirstOrDefault();

                // Número total de registros de la tabla DetalleCompra
                _TotalRegistros = DbModel.DetalleCompra
                        .Include("Herramienta").Include("MateriaPrima").Include("Compra")
                        .Where(e => e.Compra.Id == idCompra).Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _DetalleCompras = DbModel.DetalleCompra.Include("Herramienta").Include("MateriaPrima").Include("Compra")
                                                 .Where(e => e.Compra.Id == idCompra)
                                                 .OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorDetalleCompra = new PaginadorGenerico<DetalleCompra>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _DetalleCompras
                };
                ViewBag.title = "Detalle de Compra " + compra.Folio;
                ViewBag.Accion = "4";
                ViewBag.compra = compra;
                ViewBag.total = compra.MontoTotal;

                return View(_PaginadorDetalleCompra);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpDetalleCompra(int Id, string accion, string tipo, int? idCompra)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                ViewBag.tipo = tipo;
                if (accion == "1")
                {
                    var compra = DbModel.Compras.Include("Proveedor").Where(e => e.Id == Id).FirstOrDefault();
                    if (tipo != null)
                    {
                        if (tipo.Equals("H"))
                        {
                            ViewBag.Articulos = DbModel.Herramientas.Where(e => e.idProveedor == compra.Proveedor.Id).ToList();
                        }
                        else if (tipo.Equals("MP"))
                        {
                            ViewBag.Articulos = DbModel.Materiales.Where(e => e.idProveedor == compra.Proveedor.Id).ToList();
                        }
                    }
                    var detalleCompra = new DetalleCompra();

                    detalleCompra.Compra = compra;

                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    //return PartialView("popUpDetalleCompra", detalleCompra);
                    return View(detalleCompra);
                }
                
                else if (accion == "3")
                {
                    var detalleCompra = DbModel.DetalleCompra.Find(Id);
                    var compra = DbModel.Compras.Where(e => e.Id == idCompra).FirstOrDefault();
                    ViewBag.compra = compra;
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    return View(detalleCompra);
                    /*if (idCompra != null)
                    {
                        var detalleCompra = DbModel.DetalleCompra.Find(Id);
                        var compra = DbModel.Compras.Where(e => e.Id == idCompra).FirstOrDefault();
                        var detalles = DbModel.DetalleCompra.Include("MateriaPrima").Include("Herramienta").Where(e => e.Compra.Id == idCompra).ToList();
                        
                        if (detalleCompra != null)
                        {

                            try
                            {
                                compra.MontoTotal -= detalleCompra.PrecioTotal;
                                if (detalles.Count() == 1)
                                {
                                    compra.Estatus = 0;
                                }
                                DbModel.DetalleCompra.Remove(detalleCompra);
                                DbModel.Compras.AddOrUpdate(compra);
                                DbModel.SaveChanges();
                                ViewBag.title = "Eliminar";
                                ViewBag.Accion = "3";
                                ViewBag.compra = compra;
                                ViewBag.res = "Eliminación finalizada";
                                return RedirectToAction("ListaDetalleCompra", new { idCompra = compra.Id });
                            }
                            catch (Exception ex)
                            {
                                ViewBag.res = ex.Message;
                                return RedirectToAction("ListaDetalleCompra", new { idCompra = compra.Id });
                            }
                        }
                    }*/
                }
            }
            return RedirectToAction("ListaCompras");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpDetalleCompra(DetalleCompra detalleCompra, int? idCompra, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (detalleCompra.Id > 0 && accion == "3")
                {
                    // Eliminación
                    /*var detCompra = DbModel.DetalleCompra.Find(detalleCompra.Id);
                    var compra = DbModel.Compras.Include("Proveedor").Include("Empleado").Where(e => e.Id == idCompra).FirstOrDefault();

                    if (detCompra != null)
                    {

                        try
                        {
                            var detalles = DbModel.DetalleCompra.Include("MateriaPrima").Include("Herramienta").Where(e => e.Compra.Id == idCompra).ToList();
                            compra.MontoTotal -= detCompra.PrecioTotal;
                            if(detalles.Count() <= 1)
                            {
                                compra.Estatus = 0;
                            }
                            DbModel.DetalleCompra.Remove(detCompra);
                            DbModel.Compras.AddOrUpdate(compra);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            return View("ListaDetalleCompra");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaDetalleCompra");
                        }
                    }*/
                    if (idCompra != null)
                    {
                        var detCompra = DbModel.DetalleCompra.Find(detalleCompra.Id);
                        var compra = DbModel.Compras.Where(e => e.Id == idCompra).FirstOrDefault();
                        var detalles = DbModel.DetalleCompra.Include("MateriaPrima").Include("Herramienta").Where(e => e.Compra.Id == idCompra).ToList();

                        if (detCompra != null)
                        {

                            try
                            {
                                compra.MontoTotal -= detCompra.PrecioTotal;
                                if (detalles.Count() == 1)
                                {
                                    compra.Estatus = 0;
                                }
                                DbModel.DetalleCompra.Remove(detCompra);
                                DbModel.Compras.AddOrUpdate(compra);
                                DbModel.SaveChanges();
                                ViewBag.title = "Eliminar";
                                ViewBag.Accion = "3";
                                ViewBag.compra = compra;
                                resultado = "Eliminación finalizada";
                                ViewBag.res = resultado;
                                Session["res"] = resultado;
                                Session["tipo"] = "Exito";
                                return RedirectToAction("ListaDetalleCompra", new { idCompra = compra.Id });
                            }
                            catch (Exception ex)
                            {
                                resultado = ex.Message;
                                Session["res"] = resultado;
                                ViewBag.res = ex.Message;
                                return RedirectToAction("ListaDetalleCompra", new { idCompra = compra.Id });
                            }
                        }
                    }
                }
                
                else
                {
                    if (detalleCompra != null)
                    {
                        bool b = false;
                        // Aquí código para crear
                        try
                        {
                            var compra = DbModel.Compras.Include("Proveedor").Include("Empleado").Where(e => e.Id == idCompra).FirstOrDefault();
                            var detalles = DbModel.DetalleCompra.Include("MateriaPrima").Include("Herramienta").Where(e => e.Compra.Id == idCompra).ToList();

                            if (detalleCompra.Herramienta != null)
                            {
                                var herramienta = DbModel.Herramientas.Where(e => e.Id == detalleCompra.Herramienta.Id).FirstOrDefault();
                                detalleCompra.Herramienta = herramienta;
                                detalleCompra.PrecioUnitario = herramienta.Precio;
                                detalleCompra.PrecioTotal = detalleCompra.PrecioUnitario * detalleCompra.Cantidad;
                                if (detalles.Count() > 0)
                                {
                                    foreach (DetalleCompra det in detalles)
                                    {
                                        if (det.Herramienta != null)
                                        {
                                            if (det.Herramienta.Id == detalleCompra.Herramienta.Id)
                                            {
                                                det.Cantidad += detalleCompra.Cantidad;
                                                det.PrecioTotal += detalleCompra.PrecioTotal;
                                                DbModel.DetalleCompra.AddOrUpdate(det);
                                                b = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            else if (detalleCompra.MateriaPrima != null)
                            {
                                var material = DbModel.Materiales.Where(e => e.Id == detalleCompra.MateriaPrima.Id).FirstOrDefault();
                                detalleCompra.MateriaPrima = material;
                                detalleCompra.PrecioUnitario = material.Precio;
                                detalleCompra.PrecioTotal = detalleCompra.PrecioUnitario * detalleCompra.Cantidad;
                                if (detalles.Count() > 0)
                                {
                                    foreach (DetalleCompra det in detalles)
                                    {
                                        if (det.MateriaPrima != null)
                                        {
                                            if (det.MateriaPrima.Id == detalleCompra.MateriaPrima.Id)
                                            {
                                                det.Cantidad += detalleCompra.Cantidad;
                                                det.PrecioTotal += detalleCompra.PrecioTotal;
                                                DbModel.DetalleCompra.AddOrUpdate(det);
                                                b = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            compra.MontoTotal += detalleCompra.PrecioTotal;
                            compra.Estatus = 1;
                            detalleCompra.Compra = compra;

                            if (!b)
                            {
                                DbModel.DetalleCompra.Add(detalleCompra);
                            }
                            DbModel.Compras.AddOrUpdate(compra);

                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            ViewBag.compra = compra;

                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaDetalleCompra", new { idCompra = compra.Id });
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;

                            Session["res"] = resultado;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaDetalleCompra");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaDetalleCompra");
            }
        }

   
        public ActionResult DatosArticulo(int id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Articulo = DbModel.Herramientas.ToList();
                    //return Json(Articulos, JsonRequestBehavior.AllowGet);
                
                return PartialView("_ListaDetalleCompra");
            }
        }
    }
}