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
                var inventarios = DbModel.Inventarios.Where(x => x.Color.Nombre != null).ToList();
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
        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult ListaProcesos (int pagina = 1)
        {
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Admin = true;
            }
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var Colores = DbModel.Colores.ToList();
                var Muebles = DbModel.Muebles.ToList();
                var Materiales = DbModel.Materiales.ToList();
                var Productos = DbModel.Productos.ToList();
                var Inventarios = DbModel.Inventarios.ToList();
                var Empleados = DbModel.Empleados.ToList();
                var Personas = DbModel.Personas.ToList();

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
                    TiempoPasado = item.Tiempo.Subtract(Hoy);
                    TiempoActual.Add(new Tuple<string, TimeSpan>(item.Id, TiempoPasado));
                }
                ViewBag.Tiempo = TiempoActual;
                return View(_PaginadorProcesos);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult popUpProcesos(string Id, string accion)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                try
                {
                    if (accion == "1" && string.IsNullOrEmpty(Id))
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
                        var CatMue = DbModel.Muebles.ToList();
                        var CatMat = DbModel.Materiales.ToList();
                        var prod = DbModel.Productos.ToList();
                        var color = DbModel.Colores.ToList();
                        var Inv = DbModel.Inventarios.ToList();
                        var procesos = DbModel.Procesos.Where(x=>x.Id==Id).First();

                        procesos.Tiempo = DateTime.Now;
                        procesos.Estado = "En espera";
                        switch (procesos.Nombre)
                        {
                            case "Corte":
                                var Piezas = DbModel.Piezas.ToList();
                                var Mueble = DbModel.DetalleMuebles.Where(x => x.Mueble.Id == procesos.Inventario.Producto.CategoriaMueble.Id).ToList();
                                var Material = DbModel.PiezaMateriales.Where(x => x.Material.Id == procesos.Inventario.Producto.CategoriaMaterial.Id).ToList();
                                bool FaltanPiezas = false;
                                
                                foreach (var item in Mueble)
                                {
                                    if (FaltanPiezas)
                                    {
                                        break;
                                    }
                                    foreach (var Pieza in Material)
                                    {
                                        if (item.Pieza.Id == Pieza.Pieza.Id)
                                        {
                                            if (item.Cantidad <= Pieza.Cantidad)
                                            {
                                                Pieza.Cantidad -= item.Cantidad;
                                                DbModel.PiezaMateriales.AddOrUpdate(Pieza);
                                                break;
                                            }
                                            else
                                            {
                                                int faltante = item.Cantidad - Pieza.Cantidad;
                                                Session["res"] = "Faltan " + faltante + " piezas de " + item.Pieza.Nombre;
                                                FaltanPiezas = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (FaltanPiezas)
                                {
                                    return RedirectToAction("ListaProcesos");
                                }
                                procesos.Nombre = "Construcción";
                                break;
                            case "Construcción":
                                procesos.Nombre = "Detallado"; 
                                break;
                            case "Detallado":
                                if(procesos.Inventario.Color.Nombre == "Natural")
                                {
                                    var mat = DbModel.Materiales.Where(x => x.Nombre == procesos.Inventario.Producto.CategoriaMaterial.Nombre).FirstOrDefault().Nombre;
                                    if (!mat.Contains("Bambú"))
                                    {
                                        var Barniz = DbModel.Materiales.Where(x => x.Nombre == "Barniz").FirstOrDefault();
                                        if (Barniz.Cantidad > procesos.Inventario.Producto.CategoriaMueble.GalonesUsados)
                                        {
                                            //1 galón de barniz por cada 9 metros cuadrados.
                                            Barniz.Cantidad -= procesos.Inventario.Producto.CategoriaMueble.GalonesUsados;
                                            DbModel.Materiales.AddOrUpdate(Barniz);
                                        }
                                        else
                                        {
                                            Session["res"] = "Ya no hay barniz suficiente para continuar";
                                            return RedirectToAction("ListaProceso");
                                        }
                                    }
                                    else
                                    {
                                        var BarnizEspecial = DbModel.Materiales.Where(x => x.Nombre == "Barniz Bambú").FirstOrDefault();
                                        if(BarnizEspecial.Cantidad > procesos.Inventario.Producto.CategoriaMueble.GalonesUsados)
                                        {
                                            BarnizEspecial.Cantidad -= procesos.Inventario.Producto.CategoriaMueble.GalonesUsados;
                                            DbModel.Materiales.AddOrUpdate(BarnizEspecial);
                                        }
                                        else
                                        {
                                            Session["res"] = "Ya no hay barniz especial para bambú suficiente para continuar";
                                            return RedirectToAction("ListaProceso");
                                        }
                                    }
                                }
                                else
                                {
                                    var Pintura = DbModel.Materiales.Where(x => x.Nombre == "Pintura " + procesos.Inventario.Color.Nombre).FirstOrDefault();
                                    if(Pintura.Cantidad > procesos.Inventario.Producto.CategoriaMueble.GalonesUsados)
                                    {
                                        Pintura.Cantidad -= procesos.Inventario.Producto.CategoriaMueble.GalonesUsados;
                                        DbModel.Materiales.AddOrUpdate(Pintura);
                                    }
                                    else
                                    {
                                        Session["res"] = "Ya no hay pintura "+ procesos.Inventario.Color.Nombre +" suficiente para continuar";
                                        return RedirectToAction("ListaProcesos");
                                    }
                                }
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
                        procesos.Empleado = null;
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
                    else if (accion == "4")
                    {
                        var procesos = DbModel.Procesos.Find(Id);
                        procesos.Estado = "En proceso";
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
            return RedirectToAction("ListaProcesos");
        }

        [Authorize(Roles = "Administrador, Empleado")]
        [HttpPost]
        public async Task<ActionResult> popUpProcesos(int Id)
        {
            string resultado;
            if (Id != 0)
            {
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
                    catch (Exception ex)
                    {
                        resultado = ex.Message;
                        Session["res"] = resultado;
                        return RedirectToAction("ListaProcesos");
                    }
                }
            }
            else
            {
                Session["res"] = "Tienes que seleccionar un color del producto";
                return RedirectToAction("ListaProcesos");
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

        public ActionResult TomarPedido(int? cliente, string Id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var CatMue = DbModel.Muebles.ToList();
                var CatMat = DbModel.Materiales.ToList();
                var prod = DbModel.Productos.ToList();
                var color = DbModel.Colores.ToList();
                var Inv = DbModel.Inventarios.ToList();
                var procesos = DbModel.Procesos.Where(x => x.Id == Id).First();

                var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();
                

                if (persona!=null && cliente == null)
                {
                    try
                    {
                        
                        if (procesos.Nombre == "Finalizado")
                        {
                            procesos.Estado = "Finalizado";
                            DbModel.Procesos.AddOrUpdate(procesos);
                            DbModel.SaveChanges();
                            Session["res"] = "Proceso ya finalizado, esperando confirmación del cliente";
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProcesos");
                        }
                        else
                        {
                            procesos.Estado = "En proceso";
                            procesos.Empleado = persona.Nombre;
                            procesos.Tiempo = DateTime.Now;
                            DbModel.Procesos.AddOrUpdate(procesos);
                            DbModel.SaveChanges();
                            Session["res"] = "Proceso iniciado";
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProcesos");
                        }
                    }
                    catch(Exception ex)
                    {
                        Session["res"] = ex.Message;
                        return RedirectToAction("ListaProcesos");
                    }
                }
                else if(cliente != null)
                {
                    
                    if (cliente == 1)
                    {
                        DbModel.Procesos.Remove(procesos);
                        DbModel.SaveChanges();
                        Session["res"] = "Gracias por su confianza";
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ConsultarPedido");
                    }
                    else
                    {   
                        procesos.Inventario.Cantidad += 1;
                        DbModel.Inventarios.AddOrUpdate(procesos.Inventario);
                        DbModel.Procesos.Remove(procesos);
                        DbModel.SaveChanges();
                        Session["res"] = "Proceso finalizado";
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaProcesos");
                    }
                }
                else
                {
                    Session["res"] = "Usuario no reconocido";
                    return RedirectToAction("ConsultarPedido");
                }
            }
        }

        [HttpGet]
        public ActionResult ConsultarPedido(string Id = null, string prueba = null)
        {
            if (Id != null)
            {
                ViewBag.IdPedido = Id;
            }
            
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
                        var color = DbModel.Colores.ToList();
                        var catMat = DbModel.Materiales.ToList();
                        var catMue = DbModel.Muebles.ToList();
                        var producto = DbModel.Productos.ToList();
                        var inventario = DbModel.Inventarios.ToList();
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