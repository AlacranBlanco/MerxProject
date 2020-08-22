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
    public class ProveedorController : Controller
    {
        private readonly int _RegistrosPorPagina = 10;

        private List<Proveedor> _Proveedores;
        private PaginadorGenerico<Proveedor> _PaginadorProveedores;


        [HttpGet]
        public ActionResult popUpProveedores(int? Id, string accion)
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
                    var proveedor = DbModel.Proveedores.Find(Id);
                    proveedor.Persona = (Persona)proveedor.Persona;
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(proveedor);
                }
                else if (accion == "3")
                {
                    var proveedor = DbModel.Proveedores.Find(Id);
                    proveedor.Persona = (Persona)proveedor.Persona;
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(proveedor);
                }
            }
            return RedirectToAction("ListaProveedor");
        }

        [HttpPost]
        public async Task<ActionResult> popUpProveedores(Proveedor proveedores, string accion, HttpPostedFileBase postedFile)
        {
            string resultado;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Si el applicationUser viene diferente de null, significa que el usuario quiere Editar
                if (proveedores.Id > 0 && accion == "2")
                {
                    // Edición
                    var proveedor = DbModel.Proveedores.Find(proveedores.Id);

                    if (proveedor != null)
                    {
                        try
                        {
                            DbModel.Proveedores.AddOrUpdate(proveedores);
                            DbModel.Personas.AddOrUpdate(proveedores.Persona);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProveedor");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProveedor");
                        }
                    }

                }
                else if (proveedores.Id > 0 && accion == "3")
                {
                    // Eliminación
                    var proveedor = DbModel.Proveedores.Find(proveedores.Id);

                    if (proveedor != null)
                    {

                        try
                        {
                            DbModel.Proveedores.Remove(proveedor);
                            DbModel.SaveChanges();
                            resultado = "Eliminación finalizada";
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProveedor");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProveedor");
                        }
                    }

                }
                else
                {
                    if (proveedores != null)
                    {
                        // Aquí código para crear
                        try
                        {

                            DbModel.Proveedores.Add(proveedores);
                            DbModel.SaveChanges();
                            resultado = "Inserción realizada";
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            Session["tipo"] = "Exito";
                            return RedirectToAction("ListaProveedor");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            Session["res"] = resultado;
                            return RedirectToAction("ListaProveedor");
                        }
                    }
                    resultado = "Error";
                    ViewBag.res = resultado;
                    /* En caso de que sea una creación directa, pues realizamos otro flujo, pero eso depende de la vista que se vaya a usar.
                    * Como en mi caso use algo que ya trae el proyecto por defecto pues use sus métodos, creo que si ya hacemos otros vistas,
                    * tocará relizar el context y todo eso. 
                    */
                }

                return RedirectToAction("ListaProveedor");


            }
        }


        [HttpGet]
        [Authorize(Roles = "Administrador, Empleado")]
        public ActionResult ListaProveedor(int pagina = 1)
        {
            int _TotalRegistros = 0;

            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos
                _TotalRegistros = DbModel.Proveedores.Include("Persona").Count();
                // Obtenemos la 'página de registros' de la tabla Productos
                _Proveedores = DbModel.Proveedores.Include("Persona").OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * _RegistrosPorPagina)
                                                 .Take(_RegistrosPorPagina)
                                                 .ToList();
                // Número total de páginas de la tabla Productos
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProveedores = new PaginadorGenerico<Proveedor>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Proveedores
                };


                return View(_PaginadorProveedores);
            }
        }

        public ActionResult BuscarLista(string parameter, int pagina = 1)
        {
            int _TotalRegistros = 0;
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                // Número total de registros de la tabla Productos donde sean parecidos al parámetro
                _TotalRegistros = DbModel.Proveedores.Include("Persona").Where(x => x.RazonSocial.Contains(parameter) ||
                                                        x.RFC.Contains(parameter) || x.Persona.Nombre.Contains(parameter) ||
                                                        x.Persona.Direccion.Contains(parameter) ||
                                                        x.Persona.Telefono.Contains(parameter) ||
                                                        x.Persona.Ciudad.Contains(parameter) ||
                                                        x.Persona.Estado.Contains(parameter) ||
                                                        x.Persona.CodigoPostal.Contains(parameter) ||
                                                      (x.Persona.Correo.Contains(parameter))).Count();
                // Obtenemos la 'página de registros' de la tabla Productos donde sean parecidos al parámetro
                _Proveedores = DbModel.Proveedores.Include("Persona").Where(x => x.RazonSocial.Contains(parameter) ||
                                                        x.RFC.Contains(parameter) || x.Persona.Nombre.Contains(parameter) ||
                                                        x.Persona.Direccion.Contains(parameter) || 
                                                        x.Persona.Telefono.Contains(parameter) ||
                                                        x.Persona.Ciudad.Contains(parameter) ||
                                                        x.Persona.Estado.Contains(parameter) ||
                                                        x.Persona.CodigoPostal.Contains(parameter) ||
                                                      (x.Persona.Correo.Contains(parameter))).OrderBy(x => x.Persona.Nombre)
                                                      .Skip((pagina - 1) * _RegistrosPorPagina)
                                                      .Take(_RegistrosPorPagina)
                                                      .ToList();
                // Número total de páginas de la tabla Productos donde sean parecidos al parámetro
                var _TotalPaginas = (int)Math.Ceiling((double)_TotalRegistros / _RegistrosPorPagina);
                // Instanciamos la 'Clase de paginación' y asignamos los nuevos valores
                _PaginadorProveedores = new PaginadorGenerico<Proveedor>()
                {
                    RegistrosPorPagina = _RegistrosPorPagina,
                    TotalRegistros = _TotalRegistros,
                    TotalPaginas = _TotalPaginas,
                    PaginaActual = pagina,
                    Resultado = _Proveedores
                };

                if (_TotalRegistros < 1)
                {
                    Session["res"] = ("No hay resultados");
                    return RedirectToAction("ListaMaterial");
                }
                else
                {
                    return View("ListaProveedor", _PaginadorProveedores);
                }
            }
        }
    }
}