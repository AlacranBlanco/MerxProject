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
    public class MuebleController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Mueble> _Muebles;
        private PaginadorGenerico<Mueble> _PaginadorMuebles;

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpMuebles(int? Id, string accion)
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                if (accion == "1")
                {
                    ViewBag.title = "Nuevo";
                    ViewBag.Accion = "1";
                    ViewBag.img = false;
                    return View();
                }
                else if (accion == "2")
                {
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var material = DbModel.Muebles.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(material);
                }
            }
            return RedirectToAction("ListaMueble");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpMuebles(Mueble muebles, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (muebles.Id > 0 && accion == "2")
                {
                    // Edición
                    var mueble = DbModel.Muebles.Find(muebles.Id);

                    if (mueble != null)
                    {
                        try
                        {
                            DbModel.Muebles.AddOrUpdate(mueble);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMueble");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else if (muebles.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var mueble = DbModel.Muebles.Find(muebles.Id);

                    if (mueble != null)
                    {

                        try
                        {
                            var productos = DbModel.Productos.Where(x => x.CategoriaMueble == mueble).Count();
                            if (productos < 1)
                            {
                                var procesosActuales = DbModel.Procesos.Where(x => x.Inventario.Producto.CategoriaMueble.Id == mueble.Id).Count();
                                if (procesosActuales < 1)
                                {
                                    DbModel.Muebles.Remove(mueble);
                                    DbModel.SaveChanges();
                                    resultado = "Eliminación finalizada";
                                    Session["res"] = resultado;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaMueble");
                                }
                                else
                                {
                                    resultado = "Un producto de este tipo está en proceso. No se puede eliminar";
                                    Session["res"] = resultado;
                                    return RedirectToAction("ListaMueble");
                                }
                            }
                            else
                            {
                                resultado = "Aún hay productos con este tipo de mueble, primero se tienen que quitar";
                                Session["res"] = resultado;
                                return RedirectToAction("ListaMueble");
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Muebles");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    muebles.Imagen = fileId + postedFile.FileName;


                    if (muebles != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Muebles.Add(muebles);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMueble");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMueble");
                        }
                    }
                    resultado = "Error";
                    Session["res"] = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaMaterial");


            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ListaMueble(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Muebles.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Muebles = DbModel.Muebles.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMuebles = new PaginadorGenerico<Mueble>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Muebles
                };


                return View(_PaginadorMuebles);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Muebles.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Muebles = DbModel.Muebles.Where(x => x.Nombre.Contains(parameter) ||
                    (x.Descripcion.Contains(parameter))).OrderBy(x => x.Nombre)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMuebles = new PaginadorGenerico<Mueble>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Muebles
                };

                if (_TotalRegistros < 1)
                {
                    ViewBag.alert = ("No hay resultados");
                    return RedirectToAction("ListaMueble");
                }
                else
                {
                    return View("ListaMueble", _PaginadorMuebles);
                }
            }
        }
    }
}