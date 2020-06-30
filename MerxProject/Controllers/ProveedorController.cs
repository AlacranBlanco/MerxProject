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
        [AllowAnonymous]
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
                    var material = DbModel.Proveedores.Find(Id);
                    ViewBag.title = "Editar";
                    ViewBag.Accion = "2";
                    ViewBag.img = true;
                    return View(material);
                }
                else if (accion == "3")
                {
                    var material = DbModel.Proveedores.Find(Id);
                    ViewBag.title = "Eliminar";
                    ViewBag.Accion = "3";
                    ViewBag.img = true;
                    return View(material);
                }
            }
            return RedirectToAction("ListaProveedor");
        }

        [AllowAnonymous]
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
                    var material = DbModel.Proveedores.Find(proveedores.Id);

                    if (material != null)
                    {
                        try
                        {
                            DbModel.Proveedores.AddOrUpdate(proveedores);
                            DbModel.SaveChanges();
                            resultado = "Actualización realizada";
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaProveedor");

                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
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
                            return RedirectToAction("ListaMaterial");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
                            return RedirectToAction("ListaMaterial");
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
                            return RedirectToAction("ListaProveedor");
                        }
                        catch (Exception ex)
                        {
                            resultado = ex.Message;
                            ViewBag.res = resultado;
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
        [AllowAnonymous]
        public ActionResult ListaProveedor()
        {
            using (ApplicationDbContext DbModel = new ApplicationDbContext())
            {
                var proveedores = DbModel.Proveedores.ToList();
                return View(proveedores);
            }
        }
    }
}