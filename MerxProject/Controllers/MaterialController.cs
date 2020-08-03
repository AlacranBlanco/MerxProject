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
    public class MaterialController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Material> _Materiales;
        private PaginadorGenerico<Material> _PaginadorMateriales;

        [AllowAnonymous]
        [HttpGet]
        public ActionResult popUpMateriales(int? Id, string accion)
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
                    var material = DbModel.Materiales.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var material = DbModel.Materiales.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(material);
                }
            }
            return RedirectToAction("ListaMaterial");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> popUpMateriales(Material materiales, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usaurio quiere Editar
                if (materiales.Id > 0 && accion == "2")
                {
                    // Edición
                    var material = DbModel.Materiales.Find(materiales.Id);

                    if (material != null)
                    {
                        try
                        {
                            DbModel.Materiales.AddOrUpdate(materiales);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMaterial");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                    }

                }
                else if (materiales.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var material = DbModel.Materiales.Find(materiales.Id);

                    if (material != null)
                    {

                        try
                        {
                            var producto = DbModel.Productos.Where(x => x.CategoriaMaterial == material).Count();
                            if (producto < 1)
                            {
                                var procesosActuales = DbModel.Procesos.Where(x => x.Inventario.Producto.CategoriaMaterial.Id == material.Id).Count();
                                if (procesosActuales < 1)
                                {
                                    DbModel.Materiales.Remove(material);
                                    DbModel.SaveChanges();
                                    resultado = "Eliminación finalizada";
                                    Session["res"] = resultado;
                                    Session["tipo"] = "Exito";
                                    return RedirectToAction("ListaMaterial");
                                }
                                else
                                {
                                    resultado = "Un producto de este tipo está en proceso. No se puede eliminar";
                                    Session["res"] = resultado;
                                    return RedirectToAction("ListaMaterial");
                                }
                            }
                            else
                            {
                                resultado = "Aún hay productos con este material, no se puede eliminar";
                                Session["res"] = resultado;
                                return RedirectToAction("ListaMaterial");
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                    }

                }
                else
                {
                    string dir = Server.MapPath("~/Content/assets/img/Materiales");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var originalFile = Path.GetFileName(postedFile.FileName);
                    string fileId = Guid.NewGuid().ToString().Replace("-", "");
                    var path = Path.Combine(dir, fileId);
                    postedFile.SaveAs(path + postedFile.FileName);
                    materiales.Imagen = fileId + postedFile.FileName;


                    if (materiales != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Materiales.Add(materiales);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaMaterial");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaMaterial");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
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
        public ActionResult ListaMaterial(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Materiales.Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Materiales = DbModel.Materiales.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMateriales = new PaginadorGenerico<Material>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Materiales
                };


                return View(_PaginadorMateriales);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Materiales.Where(x => x.Nombre.Contains(parameter) ||
                                                          (x.Descripcion.Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Materiales = DbModel.Materiales.Where(x => x.Nombre.Contains(parameter) ||
                                                      (x.Descripcion.Contains(parameter))).OrderBy(x => x.Nombre)
                                                      .Skip((pagina - 1) * _RegistrosPorPagina)
                                                      .Take(_RegistrosPorPagina)
                                                      .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorMateriales = new PaginadorGenerico<Material>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Materiales
                };

                if (_TotalRegistros < 1)
                {
                    Session["res"] = ("No hay resultados");
                    return View("ListaMaterial");
                }
                else
                {
                    return View("ListaMaterial", _PaginadorMateriales);
                }
            }
        }
    }
}
