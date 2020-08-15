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
    [Authorize(Roles = "Administrador, Empleado")]
    public class HerramientaController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Herramienta> _Herramientas;
        private PaginadorGenerico<Herramienta> _PaginadorHerramientas;

        // GET: Herramienta
        [Authorize (Roles = "Administrador, Empleado")]
        public ActionResult ListaHerramienta(int pagina = 1)
        {
            int _TotalRegistros = 0;

            if (User.IsInRole("Administrador"))
            {
                ViewBag.Admin = true;
            }

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                _TotalRegistros = DbModel.Herramientas.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Herramientas = DbModel.Herramientas.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorHerramientas = new PaginadorGenerico<Herramienta>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Herramientas
                };

                var Reportes = new List<Tuple<int, int>>();
                var ListaReportes = DbModel.HerramientaReportes.ToList();
                
                foreach (var item in _Herramientas)
                {
                    var romper = false;
                    foreach (var rep in ListaReportes)
                    {
                        if (romper) break;
                        if(item.Id == rep.herramienta.Id)
                        {
                            var count = DbModel.HerramientaReportes.Where(x => x.herramienta.Id == rep.herramienta.Id).Count();
                            Reportes.Add(new Tuple<int, int>(item.Id, count));
                            romper = true;
                        }
                        
                    }
                }
                ViewBag.reportes = Reportes;

                return View(_PaginadorHerramientas);
            }
        }

        public ActionResult BuscarHerramienta(string param, int pagina = 1)
        {
            int _TotalRegistros = 0;
            if (!string.IsNullOrWhiteSpace(param))
            {
                using (ApplicationDbContext DbModel = new ApplicationDbContext())
                {
                    _TotalRegistros = DbModel.Herramientas.Where(x => x.Nombre.Contains(param) ||
                                                                    (x.Descripcion.Contains(param)))
                                                                    .OrderBy(x => x.Nombre)
                                                                    .Count();
                    // Obtenemos la 'página de registros' de la tabla Productos
                    _Herramientas = DbModel.Herramientas.Where(x => x.Nombre.Contains(param) ||
                                                                (x.Descripcion.Contains(param)))
                                                                .OrderBy(x => x.Nombre)
                                                                .Skip((pagina - 1) * _RegistrosPorPagina)
                                                                .Take(_RegistrosPorPagina)
                                                                .ToList();
                    // Número total de páginas de la tabla Productos
                    var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                    // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                    _PaginadorHerramientas = new PaginadorGenerico<Herramienta>()
                    {
                        RegistrosPorPagina = _RegistrosPorPagina,
                        TotalRegistros = _TotalRegistros,
                        TotalPaginas = _TotalPaginas,
                        PaginaActual = pagina,
                        Resultado = _Herramientas
                    };
                    if(_TotalRegistros < 1)
                    {
                        Session["res"] = "No hay resultados";
                        return RedirectToAction("ListaHerramienta");
                    }
                    else
                    {
                        return View("ListaHerramienta", _PaginadorHerramientas);
                    }
                }
            }
            else
            {
                Session["res"] = "No hay resultados";
                return RedirectToAction("ListaHerramienta");
            }
        }

        public ActionResult popUpHerramienta(int? id, string accion)
        {
            string res;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    var Herramienta = new Herramienta();
                    ViewBag.Title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View(Herramienta);
                }
                else if (accion == "2")
                {
                    var herramienta = DbModel.Herramientas.Find(id);
                    ViewBag.Title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(herramienta);
                }
                else if (accion == "3")
                {
                    var herramienta = DbModel.Herramientas.Find(id);
                    ViewBag.Title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(herramienta);
                }
                else if (accion == "4" && id != null)
                {
                    var herramienta = DbModel.Herramientas.Find(id);
                    var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                    var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                    var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();

                    var Detalle = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id).ToList();

                    res = null;
                    bool Usando = false;

                    foreach (var item in Detalle)
                    {
                        if(item.Empleado == persona.Nombre)
                        {
                            res = "El usuario actual ya tiene en uso esta herramienta";
                            Usando = true;
                            break;
                        }
                    }
                    if (!Usando)
                    {
                        res = ServicioHerramienta(herramienta.Id, "4");
                        Session["tipo"] = "Exito";
                    }
                    
                    
                    Session["res"] = res;
                    
                    return RedirectToAction("ListaHerramienta");
                }
                else if(accion == "5")
                {
                    var herramienta = DbModel.Herramientas.Find(id);

                    var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                    var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                    var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();

                    var Detalle = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id).ToList();

                    res = null;

                    foreach (var item in Detalle)
                    {
                        if (item.Empleado == persona.Nombre)
                        {
                            res = ServicioHerramienta(herramienta.Id, "5");
                            Session["tipo"] = "Exito";
                            Session["res"] = res;
                            return RedirectToAction("ListaHerramienta");
                        }
                    }
                    Session["res"] = "El usuario actual no tiene esta herramienta en uso";
                    return RedirectToAction("ListaHerramienta");



                }
                return RedirectToAction("ListaHerramienta");
            }
        }

        [HttpPost]
        public async Task<ActionResult> popUpHerramientas(Herramienta herramientas, string accion, HttpPostedFileBase postedFile, int? id)
        {
            string resultado;
            
            using (var DbModel = new ApplicationDbContext())
            {
                if (id != null)
                {
                    if (id != 0)
                    {
                        var tool = DbModel.Herramientas.Find(id);
                        herramientas.Id = tool.Id;
                    }
                }
                if (herramientas.Id > 0 && accion == "2")
                {
                    // Edición
                    var herramienta = DbModel.Herramientas.Find(herramientas.Id);

                    if (postedFile != null)
                    {
                        string dir = Server.MapPath("~/Content/assets/img/Herramientas");
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var originalFile = Path.GetFileName(postedFile.FileName);
                        string fileId = Guid.NewGuid().ToString().Replace("-", "");
                        var path = Path.Combine(dir, fileId);
                        postedFile.SaveAs(path + postedFile.FileName);
                        herramientas.Imagen = fileId + postedFile.FileName;
                    }
                    else
                    {
                        herramientas.Imagen = herramienta.Imagen;
                    }

                    try
                    {

                        DbModel.Herramientas.AddOrUpdate(herramientas);
                        DbModel.SaveChanges();
                        resultado = "Actualización realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaHerramienta");

                    }
                    catch (Exception ex)
                    {
                        resultado = ex.Message;
                        Session["res"] = resultado;
                        return RedirectToAction("ListaHerramienta");
                    }
                }
                else if (herramientas.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var herramienta = DbModel.Herramientas.Find(herramientas.Id);
                    var Detalle = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id).Count();

                    if (herramienta != null && Detalle < 1)
                    {

                        try
                        {
                            DbModel.Herramientas.Remove(herramienta);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaHerramienta");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaHerramienta");
                        }
                    }
                    else
                    {
                        resultado = "La herramienta no fue encontrada o está siendo usada por alguien. No se puede eliminar";
                        Session["res"] = resultado;
                        return RedirectToAction("ListaHerramienta");
                    }

                }
                else
                {
                    if (postedFile != null)
                    {
                        string dir = Server.MapPath("~/Content/assets/img/Herramienta");
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var originalFile = Path.GetFileName(postedFile.FileName);
                        string fileId = Guid.NewGuid().ToString().Replace("-", "");
                        var path = Path.Combine(dir, fileId);
                        postedFile.SaveAs(path + postedFile.FileName);
                        herramientas.Imagen = fileId + postedFile.FileName;
                    }
                    // Aquí código para crear
                    try
                    {
                        DbModel.Herramientas.Add(herramientas);
                        DbModel.SaveChanges();
                        resultado = "Inserción realizada";
                        Session["res"] = resultado;
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaHerramienta");
                    }
                    catch (Exception ex)
                    {
                        resultado = ex.Message;
                        Session["res"] = resultado;
                        return RedirectToAction("ListaHerramienta");
                    }   
                }
            }
        }

        [HttpGet]
        public ActionResult VerHerramienta(int? id)
        {
            if(id!= null)
            {
                using (ApplicationDbContext DbModel = new ApplicationDbContext())
                {
                    var Herramientas = DbModel.Herramientas.ToList();
                    var Detalles = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == id).ToList();

                    if (Detalles.Count() > 0)
                    {
                        var TiempoActual = new List<Tuple<int, TimeSpan>>();
                        TimeSpan TiempoPasado = new TimeSpan();
                        DateTime Hoy = DateTime.Now;

                        foreach (var item in Detalles)
                        {
                            TiempoPasado = item.Time.Subtract(Hoy);
                            TiempoActual.Add(new Tuple<int, TimeSpan>(item.Id, TiempoPasado));
                        }
                        ViewBag.Tiempo = TiempoActual;                        
                        ViewBag.Herramienta = Detalles;

                        //foreach(var item in Detalles)
                        //{
                        //    detalle.Id = item.Id;
                        //    detalle.Empleado = item.Empleado;
                        //    detalle.Herramienta = item.Herramienta;
                        //    detalle.Time = item.Time;
                        //}

                        return View(Detalles);
                    }
                    else
                    {
                        string resultado = "Nadie está usando esta herramienta";
                        Session["res"] = resultado;
                        ViewBag.reload = true;
                        return RedirectToAction("ListaHerramienta");
                    }
                }
            }
            else
            {
                return RedirectToAction("ListaHerramienta");
            }
        }
        
        public string ServicioHerramienta(int Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();

                if (persona != null)
                {
                    try
                    {
                        if (accion == "4")
                        {
                            var herramienta = DbModel.Herramientas.Find(Id);
                            herramienta.EnUso += 1;

                            var detalleHerramienta = new DetalleHerramienta()
                            {
                                Empleado = persona.Nombre,
                                Herramienta = herramienta,
                                Time = DateTime.Now
                            };
                            DbModel.Herramientas.AddOrUpdate(herramienta);
                            DbModel.DetalleHerramientas.Add(detalleHerramienta);
                            DbModel.SaveChanges();
                            return "Actualización realizada";
                        }
                        else if(accion == "5")
                        {
                            var herramienta = DbModel.Herramientas.Find(Id);
                            herramienta.EnUso -= 1;
                            var detalleHerramienta = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id &&
                                                                                      (x.Empleado == persona.Nombre))
                                                                                      .First();
                            DbModel.Herramientas.AddOrUpdate(herramienta);
                            DbModel.DetalleHerramientas.Remove(detalleHerramienta);
                            DbModel.SaveChanges();
                            return "Herramienta devuelta";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                else
                {
                    return "Usuario no reconocido";
                }
                return "Error";
            }
        }

        public ActionResult reportarHerramienta(int Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();

                try
                {
                    var herramienta = DbModel.Herramientas.Find(Id);

                    if (accion == "6")
                    {
                        var reportes = new HerramientaReporte()
                        {
                            DateTime = DateTime.Now,
                            Empleado = persona.Nombre,
                            herramienta = herramienta
                        };

                        DbModel.HerramientaReportes.Add(reportes);
                        var i = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id && x.Empleado == persona.Nombre).Count();
                        if (i > 0)
                        {
                            if (!herramienta.Reportada)
                            {
                                herramienta.Reportada = true;
                                DbModel.Herramientas.AddOrUpdate(herramienta);
                            }
                            var Detalle = DbModel.DetalleHerramientas.Where(x => x.Herramienta.Id == herramienta.Id && x.Empleado == persona.Nombre).FirstOrDefault();
                            DbModel.DetalleHerramientas.Remove(Detalle);
                            DbModel.SaveChanges();
                            Session["res"] = "Reporte añadido";
                            Session["tipo"] = "Exito";
                            RedirectToAction("ListaHerramienta");
                        }
                        else
                        {
                            Session["res"] = "No se tiene una herramienta de este tipo en uso";
                            RedirectToAction("ListaHerramienta");
                        }
                    }
                    else if (accion == "7")
                    {
                        var Herramientas = DbModel.Herramientas.ToList();
                        var reportes = DbModel.HerramientaReportes.Where(x => x.herramienta.Id == Id).ToList();

                        if (reportes.Count() > 0)
                        {
                            ViewBag.Herramienta = reportes;
                            return View(reportes);
                        }
                        else
                        {
                            string resultado = "Nadie ha reportado esta herramienta";
                            Session["res"] = resultado;
                            RedirectToAction("ListaHerramienta");
                        }
                    }
                    else
                    {
                        Session["res"] = "Usuario no encontrado";
                        RedirectToAction("ListaHerramienta");
                    }
                }
                catch (Exception ex)
                {
                    Session["res"] = ex.Message;
                    return RedirectToAction("ListaHerramienta");
                }
                return RedirectToAction("ListaHerramienta");
            }
        }

        [HttpPost]
        public ActionResult reportarHerramienta(int reporte, string accion, int Id)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var usuario = DbModel.Usuarios.Where(x => x.User == User.Identity.Name).FirstOrDefault();
                var empleado = DbModel.Empleados.Where(x => x.Usuarioss.idUsuario == usuario.idUsuario).FirstOrDefault();
                var persona = DbModel.Personas.Where(x => x.idPersona == empleado.Personass.idPersona).FirstOrDefault();

                var herramientaReporte = DbModel.HerramientaReportes.Find(reporte);


                try
                {
                    var herramienta = DbModel.Herramientas.Find(Id);
                    var Reportes = DbModel.HerramientaReportes.Where(x => x.herramienta.Id == Id).Count();
                    if (Reportes > 1)
                    {
                        herramienta.Reportada = true;
                    }
                    else
                    {
                        herramienta.Reportada = false;
                    }

                    if (accion == "8")
                    {
                        herramienta.EnUso -= 1;
                        DbModel.Herramientas.AddOrUpdate(herramienta);
                        DbModel.HerramientaReportes.Attach(herramientaReporte);
                        DbModel.HerramientaReportes.Remove(herramientaReporte);
                        DbModel.SaveChanges();
                        Session["res"] = "Reporte eliminado";
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaHerramienta");
                    }
                    else if (accion == "9")
                    {
                        herramienta.EnUso -= 1;
                        herramienta.Cantidad -= 1;
                        DbModel.HerramientaReportes.Attach(herramientaReporte);
                        DbModel.HerramientaReportes.Remove(herramientaReporte);
                        DbModel.Herramientas.AddOrUpdate(herramienta);
                        DbModel.SaveChanges();
                        Session["res"] = "Reporte solucionado";
                        Session["tipo"] = "Exito";
                        return RedirectToAction("ListaHerramienta");
                    }
                    else
                    {
                        Session["res"] = "Error";
                        return RedirectToAction("ListaHerramienta");
                    }
                }
                catch (Exception ex)
                {
                    Session["res"] = ex.Message;
                    return RedirectToAction("ListaHerramienta");
                }
            }
        }
    }
}